using SoftSync.Common;
using SoftSync.Common.Dtos;

namespace SoftSync.Presentation.Components.Journey;

// A view projection of saved roadmap data, never an enrollment or completion store.
public sealed record VietnamJourneyStop(string Id, int ItemId, int SkillId, string CourseKey,
    string LandmarkId, int Step, int Order, string Title, JourneyState State, bool Boss,
    int Completed, int Total);

public static class VietnamJourney
{
    public static IReadOnlyList<RoadmapItemDto> Order(IEnumerable<RoadmapItemDto> items) =>
        items.OrderBy(x => x.PriorityOrder).ThenBy(x => x.WeekNumber).ThenBy(x => x.Id).ToList();

    public static IReadOnlyList<VietnamJourneyStop> Stops(IReadOnlyList<RoadmapItemDto> items, string key)
    {
        var active = JourneyProgress.ActiveIndex(items);
        var selected = items.Select((item, index) => (item, index))
            .Where(x => SkillTheme.KeyFor(x.item) == key).ToList();
        var stops = new List<VietnamJourneyStop>();
        for (var i = 0; i < selected.Count; i++)
        {
            var (item, index) = selected[i];
            // Multiple lessons can share a city; the renderer clusters them instead of inventing coordinates.
            var landmark = i == 0 || i < selected.Count / 2 ? "hanoi" : "danang";
            var state = JourneyProgress.State(item, index, active, RoadmapReviewMode.Enabled);
            stops.Add(new($"skill:{item.SkillId}:item:{item.Id}", item.Id, item.SkillId, key,
                landmark, JourneyProgress.ResumeStep(item), i + 1, item.Title, state, false,
                item.CompletedActivityCount, item.TotalActivityCount));
        }
        if (selected.Count > 0)
        {
            var (last, index) = selected[^1];
            // The final milestone is the actual reflection of the last saved lesson, not a fabricated level.
            var state = JourneyProgress.Completed(last, 6) ? JourneyState.Completed
                : JourneyProgress.State(last, index, active, RoadmapReviewMode.Enabled) != JourneyState.Locked && JourneyProgress.CanOpen(last, 6)
                    ? JourneyState.Current : JourneyState.Locked;
            stops.Add(new($"skill:{last.SkillId}:item:{last.Id}:reflection", last.Id, last.SkillId, key,
                "hcm", 6, stops.Count + 1, "Reflection & AI feedback", state, true,
                last.CompletedActivityCount, last.TotalActivityCount));
        }
        return stops;
    }

    public static bool CanOpen(IReadOnlyList<RoadmapItemDto> items, VietnamJourneyStop stop)
    {
        var item = items.FirstOrDefault(x => x.Id == stop.ItemId);
        if (item is null || SkillTheme.KeyFor(item) != stop.CourseKey) return false;
        var index = items.ToList().IndexOf(item);
        return JourneyProgress.State(item, index, JourneyProgress.ActiveIndex(items), RoadmapReviewMode.Enabled) != JourneyState.Locked
            && JourneyProgress.CanOpen(item, stop.Step);
    }
}
