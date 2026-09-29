namespace SoftSync.BLL.Services;

public sealed record LearningOverview(int CourseCount, int CompletedCourses, decimal AverageQuizScore, int EvidenceCount);

public static class LearningAnalyticsRules
{
    public static decimal AverageQuizScore(IEnumerable<(int Correct, int Total)> attempts)
    {
        var valid = attempts.Where(x => x.Total > 0).Select(x => (decimal)x.Correct / x.Total * 10m).ToList();
        return valid.Count == 0 ? 0 : Math.Round(valid.Average(), 1);
    }

    public static string? Recommendation(decimal latestQuizScore, bool hasUnfinishedLesson, bool hasCompletedCourse) =>
        latestQuizScore < 5 ? "Review the related lesson before retrying the quiz."
        : hasUnfinishedLesson ? "Continue the next unfinished lesson."
        : hasCompletedCourse ? "Explore an advanced course for the same skill."
        : null;

    public static bool CanViewStudentAnalytics(int authenticatedUserId, int ownerUserId) =>
        authenticatedUserId > 0 && authenticatedUserId == ownerUserId;
}
