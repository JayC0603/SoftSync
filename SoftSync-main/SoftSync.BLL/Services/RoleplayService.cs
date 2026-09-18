using System.Text.Json;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;

namespace SoftSync.BLL.Services;

public sealed class RoleplayService(
    IRoleplayRepository repository,
    IRoadmapRepository roadmapRepository,
    IRoadmapService roadmapService,
    IRoleplayAiService ai) : IRoleplayService
{
    public IReadOnlyList<RoleplayScenarioDto> GetScenarios() => RoleplayCatalog.Scenarios;

    public async Task<RoleplaySessionDto?> StartAsync(int authenticatedUserId, int roadmapItemId, int scenarioId, bool vietnamese)
    {
        var item = await roadmapRepository.GetByIdAsync(roadmapItemId);
        var scenario = RoleplayCatalog.Find(scenarioId);
        if (authenticatedUserId <= 0 || item is null || item.UserId != authenticatedUserId
            || !item.PracticeCompletedAtUtc.HasValue || scenario is null)
            return null;

        var session = new RoleplaySession
        {
            UserId = authenticatedUserId,
            RoadmapItemId = roadmapItemId,
            ScenarioId = scenarioId,
            Status = RoleplaySessionStatus.InProgress
        };
        session.Turns.Add(new RoleplayTurn
        {
            Sequence = 1,
            Speaker = RoleplaySpeaker.Persona,
            Content = vietnamese ? scenario.OpeningVi : scenario.OpeningEn
        });
        await repository.AddSessionAsync(session);
        await repository.SaveChangesAsync();
        return Map(session, scenario);
    }

    public async Task<RoleplaySessionDto?> GetAsync(int authenticatedUserId, int sessionId)
    {
        var session = await repository.GetOwnedAsync(sessionId, authenticatedUserId);
        return session is null ? null : Map(session, RoleplayCatalog.Find(session.ScenarioId));
    }

    public async Task<IReadOnlyList<RoleplaySessionDto>> GetHistoryAsync(int authenticatedUserId, int roadmapItemId)
    {
        if (authenticatedUserId <= 0) return [];
        var sessions = await repository.GetHistoryAsync(authenticatedUserId, roadmapItemId);
        return sessions.Select(session => Map(session, RoleplayCatalog.Find(session.ScenarioId))).ToList();
    }

    public async Task<RoleplaySessionDto?> SendAsync(int authenticatedUserId, int sessionId, string message, bool vietnamese)
    {
        var value = message?.Trim() ?? string.Empty;
        var session = await repository.GetOwnedAsync(sessionId, authenticatedUserId);
        var scenario = session is null ? null : RoleplayCatalog.Find(session.ScenarioId);
        if (session is null || scenario is null || session.Status != RoleplaySessionStatus.InProgress
            || value.Length is 0 or > 2000 || session.Turns.Count(turn => turn.Speaker == RoleplaySpeaker.Learner) >= scenario.RequiredLearnerTurns
            || session.Turns.OrderBy(turn => turn.Sequence).LastOrDefault()?.Speaker != RoleplaySpeaker.Persona)
            return null;

        var learnerTurn = new RoleplayTurn
        {
            RoleplaySessionId = session.Id,
            Sequence = session.Turns.Count + 1,
            Speaker = RoleplaySpeaker.Learner,
            Content = value
        };
        await repository.AddTurnAsync(learnerTurn);
        session.Turns.Add(learnerTurn);
        session.ErrorMessage = string.Empty;
        await repository.SaveChangesAsync(); // Preserve valid input before any provider call.

        var turns = MapTurns(session.Turns);
        var learnerCount = turns.Count(turn => turn.Speaker == RoleplaySpeaker.Learner);
        if (learnerCount < scenario.RequiredLearnerTurns)
        {
            string? reply = null;
            try { reply = await ai.ReplyAsync(scenario, turns, vietnamese); }
            catch { session.ErrorMessage = "AI provider unavailable; deterministic fallback was used."; }
            if (string.IsNullOrWhiteSpace(reply))
            {
                session.ErrorMessage = "AI provider unavailable; deterministic fallback was used.";
                reply = RoleplayCatalog.FallbackReply(turns, vietnamese);
            }
            var personaTurn = new RoleplayTurn { RoleplaySessionId = session.Id, Sequence = session.Turns.Count + 1, Speaker = RoleplaySpeaker.Persona, Content = reply.Trim() };
            await repository.AddTurnAsync(personaTurn);
            session.Turns.Add(personaTurn);
            await repository.SaveChangesAsync();
            return Map(session, scenario);
        }

        RoleplayAiEvaluationDto? proposed = null;
        try { proposed = await ai.EvaluateAsync(scenario, turns, vietnamese); }
        catch { session.ErrorMessage = "AI evaluation unavailable; deterministic rubric fallback was used."; }
        if (proposed is null)
        {
            session.ErrorMessage = "AI evaluation unavailable; deterministic rubric fallback was used.";
            proposed = RoleplayCatalog.FallbackEvaluation(turns, vietnamese);
        }

        var evaluation = NormalizeEvaluation(scenario, turns, proposed, vietnamese);
        if (!await roadmapService.SaveRoleplayAttemptAsync(session.RoadmapItemId, authenticatedUserId, evaluation))
        {
            session.ErrorMessage = "The roadmap activity could not be completed. Your conversation remains saved.";
            await repository.SaveChangesAsync();
            return Map(session, scenario);
        }

        session.Status = RoleplaySessionStatus.Completed;
        session.CompletedAtUtc = DateTime.UtcNow;
        session.EvaluationJson = JsonSerializer.Serialize(evaluation);
        await repository.SaveChangesAsync();
        return Map(session, scenario);
    }

    private static RoadmapRoleplayAttemptDto NormalizeEvaluation(RoleplayScenarioDto scenario, IReadOnlyList<RoleplayTurnDto> turns, RoleplayAiEvaluationDto proposed, bool vietnamese)
    {
        var scores = scenario.Rubric.ToDictionary(
            criterion => criterion.Key,
            criterion => Math.Round(Math.Clamp(proposed.Scores.GetValueOrDefault(criterion.Key), 0, criterion.MaxScore), 1),
            StringComparer.OrdinalIgnoreCase);
        var learnerMessages = turns.Where(turn => turn.Speaker == RoleplaySpeaker.Learner).Select(turn => turn.Content).ToList();
        var personaMessages = turns.Where(turn => turn.Speaker == RoleplaySpeaker.Persona).Select(turn => turn.Content).ToList();
        var evidence = learnerMessages.Take(3).Select(message => $"“{(message.Length <= 180 ? message : message[..180] + "…") }”").ToList();
        var total = Math.Round(scores.Values.Sum(), 1);
        var strengths = CleanList(proposed.Strengths, vietnamese ? "Bạn đã hoàn thành đủ ba lượt phản hồi." : "You completed all three response turns.");
        var weaknesses = CleanList(proposed.Weaknesses, vietnamese ? "Hãy làm phản hồi cụ thể và có cấu trúc hơn." : "Make the response more specific and structured.");
        var improvement = Clean(proposed.SuggestedImprovement, vietnamese ? "Nêu rõ điều bạn đã nghe, nhu cầu của bạn và một bước tiếp theo có thời hạn." : "State what you heard, your need, and one time-bound next step.");
        var next = Clean(proposed.RecommendedNextPractice, vietnamese ? "Luyện lại cùng tình huống và dùng ít nhất một câu hỏi làm rõ." : "Retry the scenario and use at least one clarifying question.");
        return new RoadmapRoleplayAttemptDto
        {
            ScenarioId = scenario.Id,
            UserMessages = learnerMessages,
            AiMessages = personaMessages,
            RubricScores = scores,
            EmotionalIntelligenceScore = scores.GetValueOrDefault("empathy"),
            ActiveListeningScore = scores.GetValueOrDefault("clarity"),
            IStatementScore = scores.GetValueOrDefault("structure"),
            SolutionScore = scores.GetValueOrDefault("solution"),
            TotalScore = total,
            Strengths = strengths,
            Weaknesses = weaknesses,
            EvidenceFromConversation = evidence,
            SuggestedImprovement = improvement,
            RecommendedNextPractice = next,
            Feedback = string.Join(" ", strengths) + " " + improvement
        };
    }

    private static List<string> CleanList(IEnumerable<string>? values, string fallback)
    {
        var result = values?.Select(value => Clean(value, string.Empty)).Where(value => value.Length > 0).Distinct().Take(3).ToList() ?? [];
        return result.Count == 0 ? [fallback] : result;
    }

    private static string Clean(string? value, string fallback)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length == 0) return fallback;
        return text.Length <= 500 ? text : text[..500];
    }

    private static RoleplaySessionDto Map(RoleplaySession session, RoleplayScenarioDto? scenario)
    {
        RoadmapRoleplayAttemptDto? evaluation = null;
        if (!string.IsNullOrWhiteSpace(session.EvaluationJson))
        {
            try { evaluation = JsonSerializer.Deserialize<RoadmapRoleplayAttemptDto>(session.EvaluationJson); }
            catch (JsonException) { }
        }
        return new RoleplaySessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            RoadmapItemId = session.RoadmapItemId,
            ScenarioId = session.ScenarioId,
            Scenario = scenario ?? new(),
            Status = session.Status,
            StartedAtUtc = session.StartedAtUtc,
            CompletedAtUtc = session.CompletedAtUtc,
            Turns = MapTurns(session.Turns),
            Evaluation = evaluation,
            ErrorMessage = session.ErrorMessage
        };
    }

    private static List<RoleplayTurnDto> MapTurns(IEnumerable<RoleplayTurn> turns) => turns.OrderBy(turn => turn.Sequence).Select(turn => new RoleplayTurnDto
    {
        Id = turn.Id, Sequence = turn.Sequence, Speaker = turn.Speaker, Content = turn.Content, CreatedAtUtc = turn.CreatedAtUtc
    }).ToList();
}

public static class RoleplayCatalog
{
    private static readonly List<RoleplayRubricCriterionDto> CommunicationRubric =
    [
        new() { Key = "empathy", LabelEn = "Empathy", LabelVi = "Thấu cảm", MaxScore = 3 },
        new() { Key = "clarity", LabelEn = "Clarity", LabelVi = "Rõ ràng", MaxScore = 3 },
        new() { Key = "structure", LabelEn = "Structure", LabelVi = "Cấu trúc", MaxScore = 2 },
        new() { Key = "solution", LabelEn = "Solution", LabelVi = "Giải pháp", MaxScore = 2 }
    ];

    public static readonly IReadOnlyList<RoleplayScenarioDto> Scenarios =
    [
        Scenario(101, "Job Interview", "Phỏng vấn việc làm", "HR interviewer", "Nhà tuyển dụng", "Explain a difficult project and what you learned.", "Trình bày một dự án khó và điều bạn đã học được.", "Tell me about a difficult team project and your contribution.", "Hãy kể về một dự án nhóm khó và đóng góp của bạn."),
        Scenario(102, "Team Conflict", "Xung đột nhóm", "Teammate", "Đồng đội", "Resolve a missed deadline without blame.", "Giải quyết việc trễ hạn mà không đổ lỗi.", "I missed my part because of work. Why are you making this such a big issue?", "Mình trễ phần việc vì phải đi làm. Sao bạn lại làm mọi chuyện căng vậy?"),
        Scenario(103, "Giving Feedback", "Đưa phản hồi", "Teammate", "Đồng đội", "Give specific, respectful and actionable feedback.", "Đưa phản hồi cụ thể, tôn trọng và có thể hành động.", "You said my presentation was weak. What exactly should I change?", "Bạn nói bài thuyết trình của mình yếu. Cụ thể mình cần sửa gì?")
    ];

    public static RoleplayScenarioDto? Find(int id) => Scenarios.FirstOrDefault(scenario => scenario.Id == id);

    public static string FallbackReply(IReadOnlyList<RoleplayTurnDto> turns, bool vi)
    {
        var learnerCount = turns.Count(turn => turn.Speaker == RoleplaySpeaker.Learner);
        return learnerCount == 1
            ? vi ? "Tôi hiểu ý bạn, nhưng bạn có thể giải thích cụ thể hơn bằng một ví dụ không?" : "I understand your point, but can you make it more specific with an example?"
            : vi ? "Bạn đề xuất bước tiếp theo cụ thể nào và khi nào chúng ta nên thực hiện?" : "What concrete next step do you propose, and when should we do it?";
    }

    public static RoleplayAiEvaluationDto FallbackEvaluation(IReadOnlyList<RoleplayTurnDto> turns, bool vi)
    {
        var text = string.Join(' ', turns.Where(turn => turn.Speaker == RoleplaySpeaker.Learner).Select(turn => turn.Content)).ToLowerInvariant();
        var hasQuestion = text.Contains('?');
        var hasExample = text.Any(char.IsDigit) || text.Contains("example") || text.Contains("ví dụ");
        var hasEmpathy = new[] { "understand", "sorry", "hiểu", "xin lỗi" }.Any(text.Contains);
        var hasSolution = new[] { "next", "could", "đề xuất", "có thể", "tomorrow", "ngày mai" }.Any(text.Contains);
        return new RoleplayAiEvaluationDto
        {
            Scores = new() { ["empathy"] = hasEmpathy ? 3 : 1, ["clarity"] = hasQuestion ? 3 : 1.5, ["structure"] = hasExample ? 2 : 1, ["solution"] = hasSolution ? 2 : .5 },
            Strengths = [vi ? "Bạn đã duy trì đủ ba lượt hội thoại." : "You sustained all three conversation turns."],
            Weaknesses = [vi ? "Phản hồi có thể cụ thể hơn." : "The response could be more specific."],
            SuggestedImprovement = vi ? "Thêm một ví dụ và đề xuất bước tiếp theo có thời hạn." : "Add one example and a time-bound next step.",
            RecommendedNextPractice = vi ? "Luyện lại và đặt ít nhất một câu hỏi làm rõ." : "Retry and ask at least one clarifying question."
        };
    }

    private static RoleplayScenarioDto Scenario(int id, string en, string vi, string personaEn, string personaVi, string objectiveEn, string objectiveVi, string openingEn, string openingVi) => new()
    {
        Id = id, TitleEn = en, TitleVi = vi, TargetSkillId = 1, PersonaEn = personaEn, PersonaVi = personaVi,
        ObjectiveEn = objectiveEn, ObjectiveVi = objectiveVi, ContextEn = objectiveEn, ContextVi = objectiveVi,
        OpeningEn = openingEn, OpeningVi = openingVi, Difficulty = "Intermediate", RequiredLearnerTurns = 3,
        Rubric = CommunicationRubric.Select(item => new RoleplayRubricCriterionDto { Key = item.Key, LabelEn = item.LabelEn, LabelVi = item.LabelVi, MaxScore = item.MaxScore }).ToList()
    };
}
