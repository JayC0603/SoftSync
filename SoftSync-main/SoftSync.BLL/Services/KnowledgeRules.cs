using SoftSync.Common.Enums;

namespace SoftSync.BLL.Services;

public static class KnowledgeRules
{
    public static bool IsUsable(KnowledgeSourceStatus status, string? content, int skillId, string? title) =>
        status == KnowledgeSourceStatus.Approved && !string.IsNullOrWhiteSpace(content) && content.Trim().Length <= 20000 && skillId > 0 && !string.IsNullOrWhiteSpace(title);

    public static bool CanEdit(int authenticatedUserId, int creatorUserId, bool isAdmin) =>
        authenticatedUserId > 0 && (isAdmin || authenticatedUserId == creatorUserId);

    public static string BuildContext(IEnumerable<(string Title, string Content)> approvedSources, int maxCharacters = 8000)
    {
        var result = new List<string>();
        var remaining = Math.Max(0, maxCharacters);
        foreach (var source in approvedSources)
        {
            if (remaining <= 0 || string.IsNullOrWhiteSpace(source.Content)) break;
            var content = source.Content.Trim();
            if (content.Length > remaining) content = content[..remaining];
            result.Add($"[{source.Title.Trim()}]\n{content}");
            remaining -= content.Length;
        }
        return string.Join("\n\n", result);
    }
}
