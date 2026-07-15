using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Repositories;

namespace SoftSync.Presentation.Services;

public sealed class HuggingFaceJsonClient(
    IHttpClientFactory clients,
    IConfiguration configuration,
    ILogger<HuggingFaceJsonClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<T?> AskAsync<T>(string task, string systemPrompt, object input)
    {
        var enabled = configuration.GetValue("AiApi:Enabled", false);
        var apiKey = configuration["AiApi:ApiKey"];
        if (!enabled || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("AI evaluation {Task} is using fallback. Enabled={Enabled}, ApiKeyConfigured={Configured}", task, enabled, !string.IsNullOrWhiteSpace(apiKey));
            return default;
        }

        try
        {
            var model = configuration["AiApi:Model"] ?? "Qwen/Qwen3-4B-Instruct-2507:cheapest";
            var request = new
            {
                model,
                temperature = 0.15,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = JsonSerializer.Serialize(input, JsonOptions) }
                }
            };
            var client = clients.CreateClient("AiApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            using var response = await client.PostAsJsonAsync("v1/chat/completions", request);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("AI evaluation {Task} failed with HTTP {StatusCode}", task, (int)response.StatusCode);
                return default;
            }
            using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var content = payload.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            var result = JsonSerializer.Deserialize<T>(content ?? "", JsonOptions);
            logger.LogInformation("AI evaluation {Task} completed with model {Model}", task, model);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI evaluation {Task} failed; using deterministic fallback", task);
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
        var result = await ai.AskAsync<AssessmentGrade>("entry-assessment", """
            Evaluate one soft-skill assessment from the selected behavioral options. Preserve the validated 1-4 option weights: return a raw score from 8 to 32 and do not deviate more than 2 points from fixedScore. Return JSON only: {"score":0}. Do not reward ideal-sounding behavior beyond the supplied weights.
            """, new { skillId, fixedScore, responses = options.Select(x => new { x.QuestionId, x.OptionText, x.OptionTextVi, x.ScoreValue }) });
        var score = Math.Clamp(result?.Score ?? fixedScore, Math.Max(8, fixedScore - 2), Math.Min(32, fixedScore + 2));
        return new AssessmentResultDto { SkillId = skillId, Score = score, Level = SoftSync.Common.AssessmentScoring.BandFor(score), CreatedAt = DateTime.UtcNow };
    }
    private sealed class AssessmentGrade { public int Score { get; set; } }
}
