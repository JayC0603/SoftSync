using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Repositories;

namespace SoftSync.Presentation.Services;

// Historical type name retained for existing consumers; transport now uses Gemini configuration.
public sealed class HuggingFaceJsonClient(
    IHttpClientFactory clients,
    IConfiguration configuration,
    ILogger<HuggingFaceJsonClient> logger,
    IAiProviderConfigurationResolver? resolver = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public Task<ResolvedAiProviderConfiguration?> ResolveAsync(string? task = null, CancellationToken cancellationToken = default)
        => resolver is null ? Task.FromResult(EnvironmentAiConfiguration.Resolve(configuration, task)) : resolver.ResolveAsync(task, cancellationToken);

    public async Task<T?> AskAsync<T>(string task, string systemPrompt, object input, CancellationToken cancellationToken = default,
        ResolvedAiProviderConfiguration? resolved = null)
    {
        try
        {
            var settings = resolved ?? await ResolveAsync(task, cancellationToken);
            if (settings is null) return default;
            var model = settings.Model;
            var request = new
            {
                model,
                temperature = task == "assistant" ? 0.25 : 0.15,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = JsonSerializer.Serialize(input, JsonOptions) }
                }
            };
            var client = clients.CreateClient("AiRuntime");
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));
            using var message = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(settings.Endpoint), AiApiConfiguration.ChatEndpoint));
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
            message.Content = JsonContent.Create(request);
            using var response = await client.SendAsync(message, deadline.Token);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("AI evaluation {Task} failed with HTTP {StatusCode}", task, (int)response.StatusCode);
                return default;
            }
            await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token);
            using var payload = await JsonDocument.ParseAsync(stream, cancellationToken: deadline.Token);
            var content = payload.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            if (task == "cv-review" && payload.RootElement.GetProperty("choices")[0].TryGetProperty("finish_reason", out var reason)
                && reason.GetString() is "length" or "content_filter") return default;
            var result = StructuredAiJson.Parse<T>(content);
            logger.LogInformation("AI evaluation {Task} completed with model {Model}", task, model);
            return result;
        }
        catch (Exception ex)
        {
            if (task == "cv-review" && cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);
            logger.LogWarning("AI evaluation {Task} failed ({ErrorType}); using feature fallback", task, ex.GetType().Name);
            return default;
        }
    }
}

public interface IAiLearningEvaluationService
{
    Task<string> ContinueRoleplayAsync(RoleplayScenario scenario, IReadOnlyList<string> userMessages, IReadOnlyList<string> aiMessages, bool vi);
    Task<RoadmapRoleplayAttemptDto> GradeRoleplayAsync(RoleplayScenario scenario, List<string> userMessages, List<string> aiMessages, bool vi);
    Task<string> EvaluateQuizAsync(string skill, IReadOnlyList<AiQuizAnswer> answers, int fixedScore, bool vi);
    Task<string> EvaluateReflectionAsync(string lessonTitle, string reflectionJson, double quizScore, double roleplayScore, bool vi);
    Task<AiVoiceEvaluation?> EvaluateVoiceAsync(AiVoiceInput input, bool vi);
}

public sealed record AiQuizAnswer(string Question, string SelectedAnswer, string BestAnswer, int Points);
public sealed record AiVoiceInput(string Scenario, string Transcript, double Duration, double Confidence, double WordsPerMinute, double WordAccuracy, int FillerCount, double LongestSilence, double PitchVariation, double VolumeVariation);
public sealed class AiVoiceEvaluation
{
    public double PacingScore { get; set; }
    public double ArticulationScore { get; set; }
    public double FluencyScore { get; set; }
    public double ToneScore { get; set; }
    public string FeedbackVi { get; set; } = string.Empty;
    public string FeedbackEn { get; set; } = string.Empty;
}

public sealed class AiLearningEvaluationService(HuggingFaceJsonClient ai) : IAiLearningEvaluationService
{
    public async Task<string> ContinueRoleplayAsync(RoleplayScenario scenario, IReadOnlyList<string> userMessages, IReadOnlyList<string> aiMessages, bool vi)
    {
        var result = await ai.AskAsync<BilingualText>("roleplay-turn", RoleplayPrompt + " Return JSON: {\"vi\":\"...\",\"en\":\"...\"}.", new { scenario, userMessages, aiMessages });
        return vi ? result?.Vi ?? CommunicationRoleplayEngine.Reply(userMessages.Last(), userMessages.Count, true)
                  : result?.En ?? CommunicationRoleplayEngine.Reply(userMessages.Last(), userMessages.Count, false);
    }

    public async Task<RoadmapRoleplayAttemptDto> GradeRoleplayAsync(RoleplayScenario scenario, List<string> userMessages, List<string> aiMessages, bool vi)
    {
        var result = await ai.AskAsync<RoleplayGrade>("roleplay-grade", """
            Grade the learner's role-play using this exact rubric: emotional intelligence 0-3, active listening 0-3, I-statement 0-2, concrete time-bound solution 0-2. Judge meaning, not keywords. Return JSON only: {"emotionalIntelligence":0,"activeListening":0,"iStatement":0,"solution":0,"feedbackVi":"...","feedbackEn":"..."}. Keep feedback specific and constructive. Read Vietnamese diacritics exactly.
            """, new { scenario, userMessages, aiMessages });
        if (result is null) return CommunicationRoleplayEngine.Grade(scenario.Id, userMessages, aiMessages, vi);
        var eq = Math.Clamp(result.EmotionalIntelligence, 0, 3);
        var listening = Math.Clamp(result.ActiveListening, 0, 3);
        var statement = Math.Clamp(result.IStatement, 0, 2);
        var solution = Math.Clamp(result.Solution, 0, 2);
        return new RoadmapRoleplayAttemptDto { ScenarioId = scenario.Id, UserMessages = userMessages, AiMessages = aiMessages, EmotionalIntelligenceScore = eq, ActiveListeningScore = listening, IStatementScore = statement, SolutionScore = solution, TotalScore = Math.Round(eq + listening + statement + solution, 1), Feedback = vi ? result.FeedbackVi : result.FeedbackEn };
    }

    public async Task<string> EvaluateQuizAsync(string skill, IReadOnlyList<AiQuizAnswer> answers, int fixedScore, bool vi)
    {
        var result = await ai.AskAsync<BilingualText>("quiz-feedback", """
            Analyze a completed soft-skills quiz. The fixedScore is authoritative and must never be changed. Explain the strongest pattern, the main misconception, and one concrete practice step. Return JSON only: {"vi":"...","en":"..."}. Read Vietnamese exactly.
            """, new { skill, fixedScore, answers });
        return vi ? result?.Vi ?? "Hãy xem lại các câu chưa tối ưu và áp dụng một hành vi cụ thể trong tuần này."
                  : result?.En ?? "Review the less effective choices and practice one concrete behavior this week.";
    }

    public async Task<string> EvaluateReflectionAsync(string lessonTitle, string reflectionJson, double quizScore, double roleplayScore, bool vi)
    {
        var result = await ai.AskAsync<BilingualText>("reflection-feedback", """
            Coach the learner from their reflection and actual quiz/role-play scores. Identify one demonstrated strength, one gap, and improve their action commitment so it is specific and measurable. Do not invent activity. Return JSON only: {"vi":"...","en":"..."}.
            """, new { lessonTitle, reflectionJson, quizScore, roleplayScore });
        return vi ? result?.Vi ?? "Hãy biến cam kết của bạn thành một hành động cụ thể, có thời hạn và cách tự kiểm tra."
                  : result?.En ?? "Turn your commitment into a specific, time-bound action with a way to check progress.";
    }

    public Task<AiVoiceEvaluation?> EvaluateVoiceAsync(AiVoiceInput input, bool vi) => ai.AskAsync<AiVoiceEvaluation>("voice-feedback", """
        Evaluate a speaking transcript plus measured audio signals. Scores must be: pacing 0-3, articulation 0-3, fluency 0-2, tone 0-2. Use the numeric evidence; do not claim to hear audio. Return JSON only: {"pacingScore":0,"articulationScore":0,"fluencyScore":0,"toneScore":0,"feedbackVi":"...","feedbackEn":"..."}.
        """, input);

    private const string RoleplayPrompt = "You are the other person in a realistic soft-skills role-play. Respond naturally in one or two sentences, stay in character, and make the learner clarify or improve their proposal. Do not grade yet.";
    private sealed class BilingualText { public string Vi { get; set; } = string.Empty; public string En { get; set; } = string.Empty; }
    private sealed class RoleplayGrade { public double EmotionalIntelligence { get; set; } public double ActiveListening { get; set; } public double IStatement { get; set; } public double Solution { get; set; } public string FeedbackVi { get; set; } = string.Empty; public string FeedbackEn { get; set; } = string.Empty; }
}

public sealed class HuggingFaceAiAssessmentService(HuggingFaceJsonClient ai, IAssessmentRepository repository) : IAiAssessmentService
{
    public async Task<AssessmentResultDto> EvaluateAsync(List<UserAnswerDto> answers)
    {
        var optionIds = answers.Select(x => x.OptionId).Distinct().ToList();
        var options = (await repository.GetAnsweredOptionsAsync(optionIds)).ToList();
        var skillId = options.FirstOrDefault()?.Question?.SkillId ?? 1;
        var fixedScore = options.Sum(x => x.ScoreValue);
        var diagnosis = await ai.AskAsync<AssessmentDiagnosisDto>("entry-assessment", """
            Interpret one soft-skill assessment using only the supplied behavioral responses and fixedScore. fixedScore is authoritative: never recalculate or replace it. Do not invent behavior that is not supported by the responses. If evidence is limited, use cautious wording. Return bilingual JSON matching: {"english":{"strengths":["..."],"weaknesses":["..."],"evidence":["..."],"feedback":"...","recommendedActions":["..."]},"vietnamese":{"strengths":["..."],"weaknesses":["..."],"evidence":["..."],"feedback":"...","recommendedActions":["..."]}}. Each list must contain 1-3 concise items. Recommended actions must be observable and practical.
            """, new
            {
                skillId,
                fixedScore,
                maxScore = SoftSync.Common.AssessmentScoring.MaxScore,
                responses = options.Select(x => new { x.QuestionId, question = x.Question?.QuestionText, questionVi = x.Question?.QuestionTextVi, selectedAnswer = x.OptionText, selectedAnswerVi = x.OptionTextVi, x.ScoreValue })
            });
        return new AssessmentResultDto
        {
            SkillId = skillId,
            Score = fixedScore,
            Level = SoftSync.Common.AssessmentScoring.BandFor(fixedScore),
            Diagnosis = diagnosis ?? new(),
            CreatedAt = DateTime.UtcNow
        };
    }
}

internal static class StructuredAiJson
{
    public static T? Parse<T>(string? content)
    {
        var json = content?.Trim() ?? "";
        if (json.Length > 512 * 1024) throw new JsonException("Structured response exceeds the allowed size.");
        if (json.StartsWith("```", StringComparison.Ordinal))
        {
            var newline = json.IndexOf('\n');
            if (newline < 0 || !json.EndsWith("```", StringComparison.Ordinal)) throw new JsonException("Invalid JSON fence.");
            var language = json[3..newline].Trim();
            if (language.Length > 0 && !language.Equals("json", StringComparison.OrdinalIgnoreCase)) throw new JsonException("Invalid JSON fence.");
            json = json[(newline + 1)..^3].Trim();
        }
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
