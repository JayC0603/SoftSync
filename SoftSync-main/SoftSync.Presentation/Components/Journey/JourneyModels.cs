using System.Globalization;
using SoftSync.Common.Dtos;

namespace SoftSync.Presentation.Components.Journey;

public enum JourneyState { Completed, Current, Available, Locked }

public sealed record SkillTheme(string Key, string Accent, string Light, string Secondary, string Icon)
{
    public string Css => $"--journey-accent:{Accent};--journey-light:{Light};--journey-secondary:{Secondary}";
    public static SkillTheme For(string key) => key switch
    {
        "critical" => new(key, "#38bdf8", "#a5f3fc", "#2563eb", "bi-puzzle"),
        "time" => new(key, "#9385ff", "#e7d3ff", "#f4c467", "bi-clock"),
        "teamwork" => new(key, "#2dd4bf", "#a7f3d0", "#a78bfa", "bi-people"),
        _ => new(key, "#8574ff", "#c5baff", "#38bdf8", "bi-chat-dots")
    };

    public static string KeyFor(RoadmapItemDto item)
    {
        var name = item.SkillName.ToLowerInvariant();
        if (name.Contains("giao tiếp") || name.Contains("communication")) return "communication";
        if (name.Contains("quản lý thời gian") || name.Contains("time management")) return "time";
        if (name.Contains("tư duy phản biện") || name.Contains("critical")) return "critical";
        if (name.Contains("làm việc nhóm") || name.Contains("teamwork")) return "teamwork";
        // Preserve distinct unsupported categories instead of merging their courses.
        return $"skill-{item.SkillId}-{name}";
    }
}

public static class JourneyProgress
{
    public static IReadOnlyList<JourneyCheckpoint> LevelCheckpoints(
        IReadOnlyList<RoadmapItemDto> items, string skillKey,
        Func<RoadmapItemDto, int, string> title, bool review = false)
    {
        var active = ActiveIndex(items);
        var result = new List<JourneyCheckpoint>();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (SkillTheme.KeyFor(item) != skillKey) continue;
            var ordinal = result.Count + 1;
            result.Add(new(item.Id, ResumeStep(item), title(item, ordinal), ordinal,
                State(item, i, active, review)));
        }
        // Reflection is an activity inside a real level, not another level.
        return result;
    }

    public static int ActiveIndex(IReadOnlyList<RoadmapItemDto> items)
    {
        for (var i = 0; i < items.Count; i++) if (!items[i].IsCompleted) return i;
        return Math.Max(0, items.Count - 1);
    }

    public static JourneyState State(RoadmapItemDto item, int index, int activeIndex, bool review)
        => item.IsCompleted ? JourneyState.Completed : index == activeIndex ? JourneyState.Current
            : review || index < activeIndex ? JourneyState.Available : JourneyState.Locked;

    // Match the lecture page's persisted evidence and prerequisite rules.
    public static bool Completed(RoadmapItemDto item, int step) => item.IsCompleted || step switch
    {
        1 => item.IsVideoCompleted,
        2 => item.ScriptCompletedAtUtc.HasValue,
        3 => item.SummaryCompletedAtUtc.HasValue,
        4 => item.IsPracticeCompleted,
        5 => item.IsScenarioCompleted,
        6 => item.IsReflectionCompleted,
        _ => false
    };

    public static bool CanOpen(RoadmapItemDto item, int step) => step is >= 1 and <= 6 &&
        (item.IsCompleted || step == 1 || Completed(item, step - 1));

    public static int ResumeStep(RoadmapItemDto item)
    {
        var saved = item.LastLearningStep switch
        { "script" => 2, "summary" => 3, "quiz" => 4, "practice" => 5, "reflection" => 6, _ => 1 };
        if (!Completed(item, saved) && CanOpen(item, saved)) return saved;
        return Enumerable.Range(1, 6).FirstOrDefault(s => !Completed(item, s) && CanOpen(item, s), 1);
    }
}

public sealed record JourneyCheckpoint(int ItemId, int Step, string Title, int Ordinal, JourneyState State, bool Boss = false);
public sealed record JourneyLesson(int Step, string Title, string Type, string Icon, bool Completed, bool Available);
public sealed record MapPoint(double X, double Y)
{
    public string Svg => $"{F(X)} {F(Y)}";
    public string Css => $"left:{F(X / RoadmapGeometry.Width * 100)}%;top:{F(Y / RoadmapGeometry.Height(Count) * 100)}%";
    public int Count { get; init; }
    public static string F(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

public static class RoadmapGeometry
{
    public const double Width = 420;
    public static double Height(int count) => Math.Max(480, 125 + Math.Max(1, count) * 110);
    public static MapPoint Point(int index, int count) => new(
        index == count - 1 ? 230 : index % 2 == 0 ? 90 : 140,
        Height(count) - 66 - index * ((Height(count) - 146) / Math.Max(1, count - 1))) { Count = count };
    public static string Segment(MapPoint from, MapPoint to)
    {
        var middle = (from.Y + to.Y) / 2;
        var bend = from.X < 100 ? 260 : 35;
        return $"M {from.Svg} C {MapPoint.F(bend)} {MapPoint.F(middle + 30)}, {MapPoint.F(bend)} {MapPoint.F(middle - 30)}, {to.Svg}";
    }
}
