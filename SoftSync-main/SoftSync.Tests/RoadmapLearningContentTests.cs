using SoftSync.Common;
using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components.Journey;
using Xunit;

namespace SoftSync.Tests;

public sealed class RoadmapLearningContentTests
{
    [Fact]
    public void LevelsUseStoredTitlesAndLectureOrderInsteadOfPriorityOrTemplateTitles()
    {
        RoadmapItemDto[] items =
        [
            new() { Id = 20, SkillName = "Quản lý thời gian", Title = "Tên bài do dữ liệu cung cấp", WeekNumber = 9, PriorityOrder = 1 },
            new() { Id = 5, SkillName = "Giao tiếp", Title = "Khóa học khác", WeekNumber = 1 },
            new() { Id = 10, SkillName = "Quản lý thời gian", Title = "Tuần 2 - Nội dung đã lưu", WeekNumber = 2, PriorityOrder = 5 }
        ];
        var levels = RoadmapLearningContent.Levels(items[0], items);
        Assert.Equal(new[] { 10, 20 }, levels.Select(x => x.Id));
        Assert.Equal(new[] { "Tuần 2 - Nội dung đã lưu", "Tên bài do dữ liệu cung cấp" }, levels.Select(x => x.Title));
        Assert.Equal(2, RoadmapLearningContent.Ordinal(items[0], items));
        Assert.Equal("Sắp xếp và ưu tiên những công việc quan trọng", RoadmapLearningContent.LessonTitle(items[0], items, true));
    }

    [Fact]
    public void ActivitiesMatchTheLectureWorkflowAndPersistedTotal()
    {
        var item = new RoadmapItemDto { Id = 1, SkillName = "Giao tiếp", TotalActivityCount = 6 };
        Assert.Equal(RoadmapProgressCalculator.RequiredActivityCount, RoadmapLearningContent.Activities.Count);
        Assert.Equal(new[] { "Video", "Script", "Tóm tắt", "Quiz", "Thực hành", "Phản tư" },
            RoadmapLearningContent.ActivitiesFor(item, [item]).Select(x => x.Vi));
        item.TotalActivityCount = 3;
        Assert.Equal(new[] { 1, 2, 3 }, RoadmapLearningContent.ActivitiesFor(item, [item]).Select(x => x.Step));
    }

    [Fact]
    public void MissingContentDoesNotProduceInventedActivities()
    {
        RoadmapItemDto[] items =
        [
            new() { Id = 1, SkillName = "Giao tiếp", WeekNumber = 1 },
            new() { Id = 2, SkillName = "Giao tiếp", WeekNumber = 2 },
            new() { Id = 3, SkillName = "Giao tiếp", WeekNumber = 3, Title = "Nội dung chờ bổ sung" },
            new() { Id = 4, SkillId = 8, SkillName = "Kỹ năng khác", Title = "Tên được lưu" }
        ];
        Assert.Empty(RoadmapLearningContent.ActivitiesFor(items[2], items));
        Assert.Empty(RoadmapLearningContent.ActivitiesFor(items[3], items));
        Assert.Equal(items[2].Title, RoadmapLearningContent.LessonTitle(items[2], items, true));
        Assert.False(RoadmapLearningContent.HasContent(new() { Id = 99, SkillName = "Giao tiếp" }, items));
    }
}
