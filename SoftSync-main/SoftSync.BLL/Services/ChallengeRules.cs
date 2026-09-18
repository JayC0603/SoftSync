using SoftSync.DAL.Entities;
using SoftSync.Common.Dtos;

namespace SoftSync.BLL.Services;

public static class ChallengeRules
{
    public const int RequiredQuestionCount = 10;
    public const int PassingScore = 5;

    public static bool IsValidQuestion(ChallengeQuestion question) =>
        !string.IsNullOrWhiteSpace(question.QuestionText)
        && question.Options.Count == 4
        && question.Options.Count(option => option.IsCorrect) == 1
        && question.Options.All(option => !string.IsNullOrWhiteSpace(option.Text));

    public static bool CanPublish(IReadOnlyCollection<ChallengeQuestion> questions) =>
        questions.Count == RequiredQuestionCount && questions.All(IsValidQuestion);

    public static bool IsValidAiDraft(AiQuizQuestionDraftDto? draft) =>
        draft is not null && !string.IsNullOrWhiteSpace(draft.QuestionText) && !string.IsNullOrWhiteSpace(draft.Explanation)
        && draft.Options.Count == 4 && draft.Options.All(x => !string.IsNullOrWhiteSpace(x))
        && draft.Options.Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 4
        && draft.SuggestedCorrectOptionIndex is >= 0 and < 4;

    public static int Grade(IReadOnlyCollection<ChallengeQuestion> questions, IReadOnlyDictionary<int, int> selectedOptions)
    {
        if (!CanPublish(questions) || selectedOptions.Count != RequiredQuestionCount)
            throw new ArgumentException("A submission must answer all ten valid questions.", nameof(selectedOptions));

        var score = 0;
        foreach (var question in questions)
        {
            if (!selectedOptions.TryGetValue(question.Id, out var optionId)
                || !question.Options.Any(option => option.Id == optionId))
                throw new ArgumentException("A selected option does not belong to its question.", nameof(selectedOptions));

            if (question.Options.Any(option => option.Id == optionId && option.IsCorrect)) score++;
        }

        return score;
    }
}
