using SoftSync.Common.Dtos;

namespace SoftSync.Presentation.Components.Journey;

public sealed record RoadmapActivityDefinition(int Step, string En, string Vi, string TypeEn, string TypeVi, string Icon)
{
    public int Id => Step;
}

// Shared with the lecture screen. These are its existing six persisted steps,
// not separate lessons or a second curriculum authored by the overview.
public static class RoadmapLearningContent
{
    public static IReadOnlyList<RoadmapActivityDefinition> Activities { get; } = Array.AsReadOnly<RoadmapActivityDefinition>(
    [
        new(1, "Video", "Video", "Video", "Video", "bi bi-play-circle"),
        new(2, "Script", "Script", "Theory", "Lý thuyết", "bi bi-file-text"),
        new(3, "Summary", "Tóm tắt", "Summary", "Tóm tắt", "bi bi-lightbulb"),
        new(4, "Quiz", "Quiz", "Quiz", "Quiz", "bi bi-ui-checks"),
        new(5, "Practice", "Thực hành", "Practice", "Thực hành", "bi bi-person-workspace"),
        new(6, "Reflection", "Phản tư", "Reflection", "Phản tư", "bi bi-stars")
    ]);

    public static IReadOnlyList<RoadmapItemDto> Levels(RoadmapItemDto item, IEnumerable<RoadmapItemDto> items) =>
        items.Where(x => SkillTheme.KeyFor(x) == SkillTheme.KeyFor(item))
            .OrderBy(x => x.WeekNumber).ThenBy(x => x.Id).ToList();

    public static int Ordinal(RoadmapItemDto item, IEnumerable<RoadmapItemDto> items) =>
        Levels(item, items).ToList().FindIndex(x => x.Id == item.Id) + 1;

    public static bool HasContent(RoadmapItemDto item, IEnumerable<RoadmapItemDto> items) =>
        SkillTheme.KeyFor(item) is "communication" or "time" && Ordinal(item, items) is 1 or 2;

    public static IReadOnlyList<RoadmapActivityDefinition> ActivitiesFor(RoadmapItemDto item, IEnumerable<RoadmapItemDto> items) =>
        HasContent(item, items) ? Activities.Where(x => x.Step <= item.TotalActivityCount).ToList() : [];

    public static string LessonTitle(RoadmapItemDto item, IEnumerable<RoadmapItemDto> items, bool vietnamese) =>
        (SkillTheme.KeyFor(item), Ordinal(item, items)) switch
        {
            ("communication", 1) => vietnamese ? "Giới thiệu về kỹ năng giao tiếp" : "Introduction to Communication Skills",
            ("communication", 2) => vietnamese ? "Kỹ năng lắng nghe - nền tảng của giao tiếp" : "Active Listening: A Foundation of Communication",
            ("time", 1) => vietnamese ? "Giới thiệu về kỹ năng quản lý thời gian" : "Introduction to Time Management Skills",
            ("time", 2) => vietnamese ? "Sắp xếp và ưu tiên những công việc quan trọng" : "Organizing and Prioritizing Important Work",
            _ => item.Title
        };
}
