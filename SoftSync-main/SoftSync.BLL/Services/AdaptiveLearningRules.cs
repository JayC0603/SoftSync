using SoftSync.Common.Enums;

namespace SoftSync.BLL.Services;

public static class AdaptiveLearningRules
{
    public static string? NextAction(QuizAttemptResult? latestQuizResult, bool hasUnfinishedLesson, bool hasCompletedCourse) =>
        latestQuizResult == QuizAttemptResult.NotPass ? "Review the related lesson before retrying the quiz."
        : hasUnfinishedLesson ? "Continue the next unfinished lesson."
        : hasCompletedCourse ? "Explore an advanced course for the same skill."
        : null;
}
