using SoftSync.Common.Dtos;

namespace SoftSync.Presentation.Services;

/// <summary>
/// Connects the directory to the biographies already published on DesktopHome.
/// Credentials come from the existing VI/EN catalog, never from guessed IDs.
/// </summary>
public sealed record PublishedMentorProfile(string RoleKey, string PositionKey, string SummaryKey,
    IReadOnlyList<string> HighlightKeys, IReadOnlyList<string> BiographyKeys);

public static class PublishedMentorProfiles
{
    private static readonly PublishedMentorProfile VanAnh = new("mentor.advisor.role",
        "mentor.advisor.position", "mentor.advisor.summary",
        Enumerable.Range(1, 6).Select(i => $"mentor.advisor.highlight{i}").ToArray(),
        ["mentor.advisor.bio1", "mentor.advisor.bio2"]);

    private static readonly PublishedMentorProfile ToTram = new("mentor.main.role",
        "mentor.main.position", "mentor.main.summary",
        Enumerable.Range(1, 5).Select(i => $"mentor.main.highlight{i}").ToArray(),
        ["mentor.main.bio1", "mentor.main.bio2", "mentor.main.bio3"]);

    private static readonly PublishedMentorProfile Xuan = new("mentor.advisor.role",
        "mentor.advisor.xuan.position", "mentor.advisor.xuan.summary",
        Enumerable.Range(1, 5).Select(i => $"mentor.advisor.xuan.highlight{i}").ToArray(),
        ["mentor.advisor.xuan.bio"]);

    public static PublishedMentorProfile? For(MentorDto mentor) => mentor.AvatarUrl switch
    {
        "/images/mentor-nguyen-hoang-van-anh.png" => VanAnh,
        "/images/mentor-nguyen-thuy-to-tram.png" => ToTram,
        "/images/mentor-nguyen-xuan.png" => Xuan,
        _ => null
    };
}
