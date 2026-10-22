using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;

namespace SoftSync.Presentation.Services;

public sealed class HuggingFaceRoleplayAiService(HuggingFaceJsonClient ai) : IRoleplayAiService
{
    public async Task<string?> ReplyAsync(
        RoleplayScenarioDto scenario,
        IReadOnlyList<RoleplayTurnDto> turns,
        bool vietnamese)
    {
        var result = await ai.AskAsync<RoleplayReply>("roleplay-session-turn", """
            Act only as the supplied role-play persona. Continue the realistic conversation in one or two concise sentences.
            Never write the learner's response, never grade the learner, and never leave the scenario.
            Use the requested language. Return JSON only: {"content":"..."}.
            """, new
        {
            language = vietnamese ? "vi" : "en",
            scenario = new
            {
                scenario.TitleEn,
                scenario.TitleVi,
                scenario.ContextEn,
                scenario.ContextVi,
                scenario.PersonaEn,
                scenario.PersonaVi,
                scenario.ObjectiveEn,
                scenario.ObjectiveVi
            },
            transcript = turns.Select(turn => new { speaker = turn.Speaker.ToString(), turn.Content })
        });

        return string.IsNullOrWhiteSpace(result?.Content) ? null : result.Content.Trim();
    }

    public Task<RoleplayAiEvaluationDto?> EvaluateAsync(
        RoleplayScenarioDto scenario,
        IReadOnlyList<RoleplayTurnDto> turns,
        bool vietnamese) =>
        ai.AskAsync<RoleplayAiEvaluationDto>("roleplay-session-evaluation", """
            Evaluate only the learner messages in the supplied transcript against the supplied rubric.
            Each score must be numeric, use the exact rubric key, and stay between zero and that criterion's maxScore.
            Do not invent actions, quotations, or evidence. Narrative feedback must be concise and use the requested language.
            Return JSON only matching:
            {"scores":{"criterion-key":0},"strengths":["..."],"weaknesses":["..."],"suggestedImprovement":"...","recommendedNextPractice":"..."}.
            """, new
        {
            language = vietnamese ? "vi" : "en",
            scenario = new
            {
                scenario.TitleEn,
                scenario.TitleVi,
                scenario.ContextEn,
                scenario.ContextVi,
                scenario.ObjectiveEn,
                scenario.ObjectiveVi
            },
            scenario.Rubric,
            transcript = turns.Select(turn => new { speaker = turn.Speaker.ToString(), turn.Content })
        });

    private sealed class RoleplayReply
    {
        public string Content { get; set; } = string.Empty;
    }
}
