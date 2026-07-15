using System.Globalization;
using System.Text;
using System.Text.Json;
using SoftSync.BLL.Interfaces;

namespace SoftSync.BLL.AI;

public sealed class AssistantKnowledgeBase
{
    public IReadOnlyList<AssistantKnowledgeEntry> Entries { get; }

    private AssistantKnowledgeBase(IReadOnlyList<AssistantKnowledgeEntry> entries) => Entries = entries;

    public static AssistantKnowledgeBase Load(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("SoftSync AI knowledge base was not found.", path);
        var json = File.ReadAllText(path);
        var entries = JsonSerializer.Deserialize<List<AssistantKnowledgeEntry>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];
        if (entries.Count == 0) throw new InvalidDataException("SoftSync AI knowledge base is empty.");
        return new AssistantKnowledgeBase(entries);
    }
}

public sealed class AssistantKnowledgeEntry
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleVi { get; set; } = string.Empty;
    public string AnswerEn { get; set; } = string.Empty;
    public string AnswerVi { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}

public sealed class KnowledgeBasedAiAssistantService : IAiAssistantService
{
    private readonly AssistantKnowledgeBase knowledge;
    private readonly IProgressService progressService;

    public KnowledgeBasedAiAssistantService(AssistantKnowledgeBase knowledge, IProgressService progressService)
    {
        this.knowledge = knowledge;
        this.progressService = progressService;
    }

    public async Task<string> GetReplyAsync(string userMessage, int userId)
    {
        var english = userMessage.StartsWith("[en]", StringComparison.OrdinalIgnoreCase);
        if (userMessage.StartsWith("[en]", StringComparison.OrdinalIgnoreCase) || userMessage.StartsWith("[vi]", StringComparison.OrdinalIgnoreCase))
            userMessage = userMessage[4..];

        var query = Normalize(userMessage);
        var ranked = knowledge.Entries
            .Select(entry => new { Entry = entry, Score = Score(entry, query) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Entry.Id)
            .Take(2)
            .ToList();

        if (ranked.Count == 0)
            ranked.Add(new { Entry = knowledge.Entries.First(x => x.Id == "softsync-overview"), Score = 1 });

        var answer = string.Join("\n\n", ranked.Select(x => english ? x.Entry.AnswerEn : x.Entry.AnswerVi));
        var primaryRoute = ranked.Select(x => x.Entry.Route).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        if (primaryRoute == "/assessment" && userId > 0) primaryRoute = $"/assessment/{userId}";

        if (IsRecommendationQuery(query) && userId > 0)
        {
            var progress = (await progressService.GetUserProgressAsync(userId)).OrderBy(x => x.PercentComplete).FirstOrDefault();
            if (progress is not null)
            {
                answer += english
                    ? $"\n\nBased on your progress, prioritize {LocalizeSkill(progress.SkillName, true)} ({progress.PercentComplete}% complete)."
                    : $"\n\nDựa trên tiến độ hiện tại, bạn nên ưu tiên {LocalizeSkill(progress.SkillName, false)} (đã hoàn thành {progress.PercentComplete}%).";
                primaryRoute = "/roadmap";
            }
        }

        if (!string.IsNullOrWhiteSpace(primaryRoute))
            answer += english ? $"\n\nOpen in SoftSync: {primaryRoute}" : $"\n\nMở trong SoftSync: {primaryRoute}";
        return answer;
    }

    private static int Score(AssistantKnowledgeEntry entry, string query)
    {
        var score = 0;
        var title = Normalize($"{entry.TitleEn} {entry.TitleVi}");
        if (query.Length > 2 && title.Contains(query, StringComparison.Ordinal)) score += 10;
        foreach (var keyword in entry.Keywords.Select(Normalize).Where(x => x.Length > 1))
        {
            if (query.Contains(keyword, StringComparison.Ordinal)) score += keyword.Contains(' ') ? 6 : 3;
        }
        foreach (var tag in entry.Tags.Select(Normalize))
            if (query.Contains(tag, StringComparison.Ordinal)) score += 2;
        return score;
    }

    private static bool IsRecommendationQuery(string query) => new[] { "nen hoc", "goi y", "bat dau", "recommend", "what should", "improve", "cai thien" }.Any(query.Contains);

    private static string Normalize(string value)
    {
        var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.IsLetterOrDigit(c) ? c : ' ');
        return string.Join(' ', builder.ToString().Replace('đ', 'd').Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string LocalizeSkill(string value, bool english)
    {
        var normalized = Normalize(value);
        if (normalized.Contains("time management") || normalized.Contains("quan ly thoi gian")) return english ? "Time Management" : "Quản lý thời gian";
        if (normalized.Contains("critical") || normalized.Contains("tu duy phan bien")) return english ? "Critical Thinking" : "Tư duy phản biện";
        if (normalized.Contains("communication") || normalized.Contains("giao tiep")) return english ? "Communication" : "Giao tiếp";
        return value;
    }
}
