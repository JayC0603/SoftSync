using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SoftSync.Common;
using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components.Journey;

namespace SoftSync.Presentation.Components.Shared;

public partial class ProgressDashboard
{
    [Parameter] public IReadOnlyList<RoadmapItemDto> Items { get; set; } = [];
    [Parameter] public int UserId { get; set; }
    private bool detail, dateOpen, reportOpen, allMilestones, scrollToTop;
    private string detailTab = "overview", skillFilter = "all";
    private int days = 7, compareDays = 7, activityDays = 7;
    private DateTime start = Today.AddDays(-6), end = Today;
    private DateTime draftStart = Today.AddDays(-6), draftEnd = Today;
    private static DateTime Today => DateTime.UtcNow.AddHours(7).Date;

    private static readonly int[] Minutes = [9, 9, 5, 4, 10, 5];
    private static readonly string[] StepNames = ["Video", "Nội dung", "Tóm tắt", "Quiz", "Thực hành", "Phản tư"];
    private static readonly Skill[] Skills =
    [
        new("communication", "Kỹ năng giao tiếp", "bi-chat-dots-fill", "blue", ["communication", "giao tiếp"]),
        new("time", "Quản lý thời gian", "bi-clock", "purple", ["time management", "thời gian"]),
        new("critical", "Tư duy phản biện", "bi-lightbulb", "orange", ["critical", "phản biện"]),
        new("problem", "Giải quyết vấn đề", "bi-gear-fill", "green", ["problem", "vấn đề"])
    ];
    private static readonly ActivityKind[] Kinds =
    [new("Video", "bi-collection-play-fill", "blue", [1]), new("Quiz", "bi-pie-chart-fill", "purple", [4]), new("Thực hành", "bi-geo-alt-fill", "green", [5]), new("Đọc tài liệu", "bi-file-earmark-text-fill", "orange", [2, 3, 6])];
    private IEnumerable<StudyEvent> Events => Items.SelectMany(item => Stamps(item).Select(x => new StudyEvent(item, x.Step, x.At.AddHours(7))));
    private List<StudyEvent> RangeEvents => Events.Where(x => Between(x.At, start, end)).ToList();
    private int Total => Items.Sum(x => x.TotalActivityCount);
    private int Completed => Items.Sum(x => x.CompletedActivityCount);
    private int Overall => RoadmapProgressCalculator.Percent(Completed, Total);
    private int RangeMinutes => RangeEvents.Sum(x => Minutes[x.Step - 1]);
    private string Hours => (RangeMinutes / 60d).ToString("0.#", ProgressCulture);
    private int CourseCount => Items.Select(SkillTheme.KeyFor).Distinct().Count();
    private int ActiveCourses => Items.GroupBy(SkillTheme.KeyFor).Count(g => g.Any(x => x.CompletedActivityCount > 0) && g.Any(x => !x.IsCompleted));
    private int Streak
    {
        get
        {
            var dates = Events.Select(x => x.At.Date).ToHashSet();
            var cursor = dates.Contains(Today) ? Today : Today.AddDays(-1);
            var count = 0;
            while (dates.Contains(cursor)) { count++; cursor = cursor.AddDays(-1); }
            return count;
        }
    }
    private IEnumerable<Skill> VisibleSkills => Skills.Where(s => skillFilter == "all" || s.Key == skillFilter);
    private List<RoadmapItemDto> SkillItems(Skill skill) => Items.Where(x => skill.Keys.Any(k => $"{x.SkillName} {x.Title}".Contains(k, StringComparison.OrdinalIgnoreCase))).ToList();
    private int SkillDone(Skill skill) => SkillItems(skill).Sum(x => x.CompletedActivityCount);
    private int SkillTotal(Skill skill) => SkillItems(skill).Sum(x => x.TotalActivityCount);
    private int SkillPercent(Skill skill) => RoadmapProgressCalculator.Percent(SkillDone(skill), SkillTotal(skill));
    private int KindCount(ActivityKind kind, bool filtered = false) => (filtered ? Events.Where(x => Between(x.At, end.AddDays(1 - activityDays), end)) : RangeEvents).Count(x => kind.Steps.Contains(x.Step));
    private int KindPercent(ActivityKind kind, bool filtered = false)
    {
        var total = Kinds.Sum(k => KindCount(k, filtered));
        if (total == 0) return 0;
        var shares = Kinds.Select(k => (Kind: k, Exact: KindCount(k, filtered) * 100d / total)).ToList();
        var remaining = 100 - shares.Sum(x => (int)x.Exact);
        var roundedUp = shares.OrderByDescending(x => x.Exact - (int)x.Exact).Take(remaining).Select(x => x.Kind).ToHashSet();
        return (int)shares.First(x => x.Kind == kind).Exact + (roundedUp.Contains(kind) ? 1 : 0);
    }
    private string Distribution
    {
        get
        {
            var count = RangeEvents.Count;
            if (count == 0) return "conic-gradient(var(--pg-track) 0% 100%)";
            var position = 0d;
            var parts = Kinds.Select(k => { var from = position; position += KindCount(k) * 100d / count; return $"var(--pg-{k.Color}) {Css(from)}% {Css(position)}%"; });
            return $"conic-gradient({string.Join(",", parts)})";
        }
    }
    private IReadOnlyList<ProgressTrendChart.Point> ChartPoints
    {
        get
        {
            var span = Math.Max(1, (end - start).Days + 1);
            var bucketSize = (int)Math.Ceiling(span / 7d);
            var events = RangeEvents;
            var points = new List<ProgressTrendChart.Point>();
            for (var offset = 0; offset < span; offset += bucketSize)
            {
                var from = start.AddDays(offset);
                var to = from.AddDays(Math.Min(bucketSize - 1, (end - from).Days));
                var bucket = events.Where(e => Between(e.At, from, to)).ToList();
                points.Add(new(from.ToString("dd/MM"), bucket.Sum(x => Minutes[x.Step - 1]), bucket.Count));
            }
            return points;
        }
    }
    private IEnumerable<StudyEvent> Milestones => RangeEvents.OrderByDescending(x => x.At).Take(allMilestones ? 50 : 4);
    private static bool Between(DateTime at, DateTime from, DateTime to) => at.Date >= from.Date && at.Date <= to.Date;
    private static IEnumerable<(int Step, DateTime At)> Stamps(RoadmapItemDto item)
    {
        DateTime?[] stamps = [item.VideoCompletedAtUtc, item.ScriptCompletedAtUtc, item.SummaryCompletedAtUtc, item.PracticeCompletedAtUtc, item.ScenarioCompletedAtUtc, item.ReflectionCompletedAtUtc];
        for (var i = 0; i < stamps.Length; i++) if (stamps[i] is { } at) yield return (i + 1, at);
    }
    private void SetDays(int value) { days = value; start = Today.AddDays(1 - value); end = Today; }
    private void ApplyDates() { start = draftStart.Date; end = draftEnd.Date; if (start > end) (start, end) = (end, start); days = (end - start).Days + 1; dateOpen = false; }
    private void OpenDetail(string tab = "overview") { detail = true; detailTab = tab; scrollToTop = true; }
    private void BackToOverview() { detail = false; dateOpen = false; scrollToTop = true; }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!scrollToTop) return;
        scrollToTop = false;
        await JS.InvokeVoidAsync("window.scrollTo", new { top = 0, left = 0, behavior = "instant" });
    }
    private void OpenDates() { draftStart = start; draftEnd = end; dateOpen = !dateOpen; }
    private string LessonUrl(StudyEvent e) => $"/roadmap/lecture/{UserId}/{e.Item.Id}?step={e.Step}";
    private string SkillUrl(Skill s) => $"/roadmap?skill={Uri.EscapeDataString(SkillItems(s).FirstOrDefault() is { } item ? SkillTheme.KeyFor(item) : s.Key)}";
    private (int Current, int Previous) Compare(bool time)
    {
        var from = end.AddDays(1 - compareDays);
        var previousStart = from.AddDays(-compareDays);
        var current = Events.Where(x => Between(x.At, from, end));
        var previous = Events.Where(x => Between(x.At, previousStart, from.AddDays(-1)));
        return time ? (current.Sum(x => Minutes[x.Step - 1]), previous.Sum(x => Minutes[x.Step - 1])) : (current.Count(), previous.Count());
    }
    private string Change(bool time)
    {
        var (current, previous) = Compare(time);
        return previous == 0 ? current == 0 ? "0%" : T("Mới") : $"{(current >= previous ? "+" : "")}{Math.Round((current - previous) * 100d / previous)}%";
    }
    private int ComparePercent(bool time) { var (a, b) = Compare(time); return Math.Max(a, b) == 0 ? 0 : (int)(a * 100d / Math.Max(a, b)); }
    private string CompareValue(bool time) => time ? TextFormat("{0} giờ", (Compare(true).Current / 60d).ToString("0.#", ProgressCulture)) : TextFormat("{0} hoạt động", Compare(false).Current);
    private static string Css(double number) => number.ToString("0.##", CultureInfo.InvariantCulture);
    private sealed record StudyEvent(RoadmapItemDto Item, int Step, DateTime At);
    private sealed record Skill(string Key, string Name, string Icon, string Color, string[] Keys);
    private sealed record ActivityKind(string Name, string Icon, string Color, int[] Steps);
}
