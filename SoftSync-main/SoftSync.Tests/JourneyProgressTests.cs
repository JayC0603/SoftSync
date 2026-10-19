using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components.Journey;
using Xunit;

namespace SoftSync.Tests;

public sealed class JourneyProgressTests
{
    [Fact]
    public void MobileMapUsesOnlyPersistedLevelsAndSavedActivityState()
    {
        RoadmapItemDto[] items = [
            new() { Id = 31, SkillName = "Communication", Title = "Saved first", IsCompleted = true },
            new() { Id = 62, SkillName = "Communication", Title = "Saved second", IsVideoCompleted = true,
                ScriptCompletedAtUtc = DateTime.UtcNow, LastLearningStep = "summary" },
            new() { Id = 94, SkillName = "Time Management", Title = "Actual time lesson" }
        ];
        var points = JourneyProgress.LevelCheckpoints(items, "communication", (item, _) => item.Title);
        Assert.Equal(new[] { 31, 62 }, points.Select(x => x.ItemId));
        Assert.Equal(new[] { "Saved first", "Saved second" }, points.Select(x => x.Title));
        Assert.Equal(JourneyState.Completed, points[0].State);
        Assert.Equal(JourneyState.Current, points[1].State);
        Assert.Equal(3, points[1].Step);
        Assert.DoesNotContain(points, x => x.Boss);
        var future = Assert.Single(JourneyProgress.LevelCheckpoints(items, "time", (item, _) => item.Title));
        Assert.Equal(JourneyState.Locked, future.State);
        Assert.Empty(JourneyProgress.LevelCheckpoints(items, "missing", (item, _) => item.Title));
    }

    [Fact]
    public void ResumeUsesSavedStepOnlyWhenItsPrerequisiteIsComplete()
    {
        var item = new RoadmapItemDto { LastLearningStep = "reflection", CompletedActivityCount = 5 };
        Assert.Equal(1, JourneyProgress.ResumeStep(item));
        Assert.False(JourneyProgress.CanOpen(item, 6));
        item.IsVideoCompleted = true;
        Assert.Equal(2, JourneyProgress.ResumeStep(item));
        item.IsScenarioCompleted = true;
        Assert.Equal(6, JourneyProgress.ResumeStep(item));
    }

    [Fact]
    public void CompletionCountDoesNotUnlockActivitiesWithoutEvidence()
    {
        var item = new RoadmapItemDto { CompletedActivityCount = 6, ProgressPercent = 100 };
        Assert.False(JourneyProgress.Completed(item, 4));
        Assert.False(JourneyProgress.CanOpen(item, 4));
        item.SummaryCompletedAtUtc = DateTime.UtcNow;
        Assert.True(JourneyProgress.CanOpen(item, 4));
        Assert.False(JourneyProgress.Completed(item, 4));
    }

    [Fact]
    public void CompletedLevelsRemainRevisitableAndFutureLevelsRemainLocked()
    {
        RoadmapItemDto[] items = [new() { IsCompleted = true }, new(), new()];
        var active = JourneyProgress.ActiveIndex(items);
        Assert.Equal(1, active);
        Assert.Equal(JourneyState.Completed, JourneyProgress.State(items[0], 0, active, false));
        Assert.Equal(JourneyState.Current, JourneyProgress.State(items[1], 1, active, false));
        Assert.Equal(JourneyState.Locked, JourneyProgress.State(items[2], 2, active, false));
        Assert.Equal(JourneyState.Available, JourneyProgress.State(items[2], 2, active, true));
        Assert.True(JourneyProgress.CanOpen(items[0], 6));
    }

    [Fact]
    public void UnknownSkillsAreKeptSeparateAndMapCoordinatesScaleWithLevelCount()
    {
        Assert.NotEqual(SkillTheme.KeyFor(new() { SkillId = 10, SkillName = "Leadership" }),
            SkillTheme.KeyFor(new() { SkillId = 11, SkillName = "Creativity" }));
        foreach (var count in new[] { 2, 3, 5, 12 })
        {
            var points = Enumerable.Range(0, count).Select(i => RoadmapGeometry.Point(i, count)).ToList();
            Assert.All(points, p => { Assert.InRange(p.X, 0, RoadmapGeometry.Width); Assert.InRange(p.Y, 0, RoadmapGeometry.Height(count)); });
            Assert.True(points.Zip(points.Skip(1)).All(pair => pair.First.Y > pair.Second.Y));
        }
    }
}
