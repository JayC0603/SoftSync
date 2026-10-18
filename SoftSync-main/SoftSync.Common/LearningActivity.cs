using SoftSync.Common.Dtos;

namespace SoftSync.Common;

public static class LearningActivity
{
    // UTC is explicit so dates do not vary with the hosting machine's time zone.
    public static HashSet<DateOnly> Days(IEnumerable<RoadmapItemDto> items) => items
        .SelectMany(item => new DateTime?[] { item.VideoCompletedAtUtc, item.ScriptCompletedAtUtc,
            item.SummaryCompletedAtUtc, item.PracticeCompletedAtUtc, item.ScenarioCompletedAtUtc,
            item.ReflectionCompletedAtUtc })
        .Where(value => value.HasValue)
        .Select(value => DateOnly.FromDateTime(value!.Value)).ToHashSet();

    public static int Streak(IReadOnlySet<DateOnly> days, DateOnly today)
    {
        var cursor = days.Contains(today) ? today : today.AddDays(-1);
        var count = 0;
        while (days.Contains(cursor)) { count++; cursor = cursor.AddDays(-1); }
        return count;
    }
}
