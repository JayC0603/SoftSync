using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components.Journey;
using Xunit;

namespace SoftSync.Tests;

public class VietnamJourneyTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void CourseSwitchingOnlyProjectsTheRequestedSavedCourse(int count)
    {
        var items = Enumerable.Range(1, count).Select(i => new RoadmapItemDto
        { Id = i * 10, SkillId = i, SkillName = $"Custom skill {i}", Title = $"Lesson {i}", PriorityOrder = i }).ToList();
        Assert.Empty(VietnamJourney.Stops(items, "not-enrolled"));
        foreach (var item in items)
        {
            var key = SkillTheme.KeyFor(item);
            var stops = VietnamJourney.Stops(items, key);
            Assert.Equal(2, stops.Count); // Saved lesson + its real final reflection.
            Assert.All(stops, x => { Assert.Equal(item.Id, x.ItemId); Assert.Equal(key, x.CourseKey); });
            Assert.Single(stops, x => x.Boss);
            Assert.Equal(items[0] == item, VietnamJourney.CanOpen(items, stops[0]));
            Assert.False(VietnamJourney.CanOpen(items, stops[^1]));
        }
    }

    [Fact]
    public void RecommendationsNeverUnlockFutureCoursesOrChangeSavedProgress()
    {
        var items = new[] {
            new RoadmapItemDto { Id = 1, SkillId = 1, SkillName = "Communication", IsNextRecommended = false },
            new RoadmapItemDto { Id = 2, SkillId = 3, SkillName = "Time Management", IsNextRecommended = true }
        };
        var future = VietnamJourney.Stops(items, "time");
        Assert.All(future, stop => Assert.Equal(JourneyState.Locked, stop.State));
        Assert.All(future, stop => Assert.False(VietnamJourney.CanOpen(items, stop)));
        Assert.False(items[1].IsCompleted);
        Assert.Equal(0, items[1].CompletedActivityCount);
    }

    [Fact]
    public void BossUsesActualReflectionPrerequisitesAndStableActivityIdentity()
    {
        var item = new RoadmapItemDto { Id = 42, SkillId = 3, SkillName = "Time Management" };
        var boss = VietnamJourney.Stops([item], "time")[^1];
        Assert.Equal("skill:3:item:42:reflection", boss.Id);
        Assert.Equal(6, boss.Step);
        Assert.Equal(JourneyState.Locked, boss.State);
        item.IsScenarioCompleted = true;
        var unlocked = VietnamJourney.Stops([item], "time")[^1];
        Assert.Equal(boss.Id, unlocked.Id);
        Assert.True(VietnamJourney.CanOpen([item], unlocked));
        item.IsReflectionCompleted = true;
        Assert.Equal(JourneyState.Completed, VietnamJourney.Stops([item], "time")[^1].State);
    }

    [Fact]
    public void CompletedCheckpointsSurviveOrderingAndSelectionChanges()
    {
        var first = new RoadmapItemDto { Id = 11, SkillId = 1, SkillName = "Communication", IsCompleted = true };
        var second = new RoadmapItemDto { Id = 12, SkillId = 1, SkillName = "Communication" };
        var original = VietnamJourney.Stops([first, second], "communication");
        var changed = VietnamJourney.Stops([second, first], "communication");
        Assert.Equal(original.First().Id, changed.Single(x => x.ItemId == first.Id && !x.Boss).Id);
        Assert.Equal(JourneyState.Completed, changed.Single(x => x.ItemId == first.Id && !x.Boss).State);
        Assert.All(changed, stop => Assert.Contains(stop.LandmarkId, new[] { "hanoi", "danang", "hcm" }));
    }
}
