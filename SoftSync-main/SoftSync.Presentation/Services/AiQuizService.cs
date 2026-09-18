using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.BLL.Services;

namespace SoftSync.Presentation.Services;

public sealed class AiQuizService(HuggingFaceJsonClient ai) : IAiQuizService
{
    public async Task<AiQuizQuestionDraftDto?> GenerateQuestionDraftAsync(AiQuizDraftRequestDto input, CancellationToken cancellationToken = default)
    {
        var draft = await ai.AskAsync<AiQuizQuestionDraftDto>("quiz-question-draft", """
            Create one draft soft-skills quiz question using only the supplied learning context. Return JSON only:
            {"questionText":"...","scenario":"...","options":["...","...","...","..."],"suggestedCorrectOptionIndex":0,"explanation":"...","suggestedAltText":"..."}.
            Produce exactly four distinct, non-empty options. The correct index must be 0-3. Include a concise explanation.
            This is an editable teacher draft, not published content. Do not include private or sensitive data.
            """, new
        {
            input.LessonTitle,
            lessonContent = Limit(input.LessonContent, 6000),
            input.TargetSkill,
            difficulty = input.Difficulty.ToString(),
            questionType = input.QuestionType.ToString()
        }, cancellationToken);

        return ChallengeRules.IsValidAiDraft(draft) ? Normalize(draft!) : null;
    }

    public async Task<string?> ExplainAnswerAsync(QuizAnswerReviewDto answer, CancellationToken cancellationToken = default)
    {
        var result = await ai.AskAsync<TextResponse>("quiz-answer-explanation", """
            Explain a completed quiz answer in clear Vietnamese. Use only the supplied question, selected answer,
            authoritative correct answer, and teacher explanation. Do not change which answer is correct and do not diagnose the learner.
            Return JSON only: {"text":"..."}.
            """, new
        {
            answer.QuestionText,
            answer.Scenario,
            answer.SelectedAnswer,
            answer.CorrectAnswer,
            teacherExplanation = answer.Explanation,
            answer.IsCorrect
        }, cancellationToken);
        return Clean(result?.Text, 2000);
    }

    public async Task<AiQuizFeedbackDto?> GenerateFeedbackAsync(QuizAttemptReviewDto review, CancellationToken cancellationToken = default)
    {
        var result = await ai.AskAsync<AiQuizFeedbackDto>("quiz-personalized-feedback", """
            Summarize this completed quiz in cautious Vietnamese. The persisted score and result are authoritative and must not be changed.
            Identify evidence-based strengths, weak areas, and concrete next practice. Never make psychological diagnoses or absolute claims.
            Prefer phrases such as 'Trong bài kiểm tra này', 'Bạn có xu hướng', and 'Bạn có thể luyện thêm'.
            Return JSON only: {"summary":"...","strengths":["..."],"weakAreas":["..."],"suggestedNextPractice":["..."]}.
            """, new
        {
            score = new { review.Result.CorrectAnswers, review.Result.TotalQuestions, result = review.Result.Result.ToString() },
            answers = review.Answers.Select(x => new { x.QuestionText, x.SelectedAnswer, x.CorrectAnswer, x.IsCorrect, teacherExplanation = x.Explanation })
        }, cancellationToken);
        if (result is null) return null;
        result.Summary = Clean(result.Summary, 2000) ?? string.Empty;
        result.Strengths = CleanList(result.Strengths);
        result.WeakAreas = CleanList(result.WeakAreas);
        result.SuggestedNextPractice = CleanList(result.SuggestedNextPractice);
        return string.IsNullOrWhiteSpace(result.Summary) || result.SuggestedNextPractice.Count == 0 ? null : result;
    }

    private static AiQuizQuestionDraftDto Normalize(AiQuizQuestionDraftDto draft) => new()
    {
        QuestionText = Clean(draft.QuestionText, 2000)!,
        Scenario = Clean(draft.Scenario, 3000) ?? string.Empty,
        Options = draft.Options.Select(x => Clean(x, 1000)!).ToList(),
        SuggestedCorrectOptionIndex = draft.SuggestedCorrectOptionIndex,
        Explanation = Clean(draft.Explanation, 3000)!,
        SuggestedAltText = Clean(draft.SuggestedAltText, 500) ?? string.Empty
    };

    private static List<string> CleanList(IEnumerable<string>? values) =>
        (values ?? []).Select(x => Clean(x, 1000)).Where(x => x is not null).Cast<string>().Distinct(StringComparer.OrdinalIgnoreCase).Take(5).ToList();

    private static string? Clean(string? value, int maxLength)
    {
        var clean = value?.Trim();
        return string.IsNullOrWhiteSpace(clean) ? null : clean[..Math.Min(clean.Length, maxLength)];
    }

    private static string Limit(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
    private sealed class TextResponse { public string Text { get; set; } = string.Empty; }
}
