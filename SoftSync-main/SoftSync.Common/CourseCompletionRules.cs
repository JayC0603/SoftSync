namespace SoftSync.Common;

using SoftSync.Common.Enums;

public static class CourseCompletionRules
{
    public static bool IsEligibleFinalQuiz(CourseStatus status) => status == CourseStatus.Published;

    public static bool CanComplete(int totalLessons, int completedLessons, bool hasPublishedFinalQuiz, bool hasPassingFinalAttempt) =>
        totalLessons > 0 && completedLessons >= totalLessons && (!hasPublishedFinalQuiz || hasPassingFinalAttempt);
}
