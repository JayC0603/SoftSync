using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;

namespace SoftSync.Common;

/// <summary>Single business definition for roadmap completion and next action.</summary>
public static class RoadmapProgressCalculator
{
    public const int RequiredActivityCount = 6;

    public static int CompletedCount(RoadmapItemDto item) => CompletedCount(
        item.IsCompleted,
        item.VideoCompletedAtUtc.HasValue || item.IsVideoCompleted,
        item.ScriptCompletedAtUtc.HasValue,
        item.SummaryCompletedAtUtc.HasValue,
        item.PracticeCompletedAtUtc.HasValue || item.IsPracticeCompleted,
        item.ScenarioCompletedAtUtc.HasValue || item.IsScenarioCompleted,
        item.ReflectionCompletedAtUtc.HasValue || item.IsReflectionCompleted);

    public static int CompletedCount(bool legacyComplete, params bool[] activities)
        => legacyComplete ? RequiredActivityCount : activities.Count(done => done);

    public static int Percent(int completed, int total = RequiredActivityCount)
        => total <= 0 ? 0 : Math.Clamp(completed * 100 / total, 0, 100);

    public static RoadmapActivityType? NextActivity(RoadmapItemDto item)
    {
        if (item.IsCompleted) return null;
        if (!item.IsVideoCompleted && !item.VideoCompletedAtUtc.HasValue) return RoadmapActivityType.VideoLesson;
        if (!item.ScriptCompletedAtUtc.HasValue) return RoadmapActivityType.LessonScript;
        if (!item.SummaryCompletedAtUtc.HasValue) return RoadmapActivityType.LessonSummary;
        if (!item.IsPracticeCompleted && !item.PracticeCompletedAtUtc.HasValue) return RoadmapActivityType.Quiz;
        if (!item.IsScenarioCompleted && !item.ScenarioCompletedAtUtc.HasValue) return RoadmapActivityType.Roleplay;
        if (!item.IsReflectionCompleted && !item.ReflectionCompletedAtUtc.HasValue) return RoadmapActivityType.Reflection;
        return null;
    }
}
