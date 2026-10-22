using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components.Shared;
using SoftSync.Presentation.Services;

// Render the production components and change the same scoped language service
// used by the header, without recreating the page or resetting its filters.
var language = new LocalizationService(AppLanguage.Vi);
using var services = new ServiceCollection().AddLogging()
    .AddSingleton(language).AddSingleton<IJSRuntime, NoBrowserJs>().BuildServiceProvider();
await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
var passed = 0;
void Check(bool value, string description)
{
    if (!value) throw new InvalidOperationException(description);
    passed++;
    Console.WriteLine("PASS: " + description);
}
await renderer.Dispatcher.InvokeAsync(async () =>
{
    var root = await renderer.RenderComponentAsync<Fixture>();
    var dashboard = Fixture.Dashboard;
    var flags = BindingFlags.NonPublic | BindingFlags.Instance;
    string Html() => WebUtility.HtmlDecode(root.ToHtmlString());
    void Set(string name, object value) => typeof(ProgressDashboard).GetField(name, flags)!.SetValue(dashboard, value);
    object Get(string name) => typeof(ProgressDashboard).GetField(name, flags)!.GetValue(dashboard)!;
    string Value(string name) => (string)typeof(ProgressDashboard).GetProperty(name, flags)!.GetValue(dashboard)!;
    void Render() => typeof(ComponentBase).GetMethod("StateHasChanged", flags)!.Invoke(dashboard, null);
    void CheckEnglish(string state)
    {
        var html = Html();
        Check(!Regex.IsMatch(html, "[À-ỹĐđ]"), state + ": no Vietnamese UI, tooltip or accessibility label in English");
    }

    Check(Html().Contains("Tổng quan tiến độ"), "Overview starts in Vietnamese");
    Check(Value("Hours").Contains(','), "Vietnamese decimal separator");
    Set("days", 30);
    language.SetLanguage(AppLanguage.En);
    Check(Html().Contains("Progress overview") && Html().Contains("30 days"), "Overview updates live to English");
    CheckEnglish("Overview");
    Check(Value("Hours").Contains('.'), "English decimal separator");
    Check((int)Get("days") == 30, "Changing language preserves selected period");

    Set("detail", true);
    Set("dateOpen", true);
    Set("reportOpen", true);
    Render();
    Check(Html().Contains("Progress details") && Html().Contains("Learning trends"), "Details and chart are translated");
    Check(Html().Contains("Activity count") && Html().Contains("Study time (min)"), "Chart legend and units are translated");
    Check(Html().Contains("Save PDF / Print") && Html().Contains("estimated study hours"), "Report and summary are translated");
    Check(Html().Contains("Last 30 days") && Html().Contains("All skills"), "Select options are translated");
    CheckEnglish("Details, date picker and report");
    Set("detailTab", "skills");
    Set("skillFilter", "time");
    Render();
    Check(Html().Contains("Time management"), "Dynamic skill names are translated");
    language.SetLanguage(AppLanguage.Vi);
    Check(Html().Contains("Quản lý thời gian") && Html().Contains("Lưu PDF / In"), "Details return live to Vietnamese");
    Check((string)Get("skillFilter") == "time" && (bool)Get("reportOpen"), "Switching back preserves skill filter and report state");

    language.SetLanguage(AppLanguage.En);
    Set("detailTab", "overview");
    typeof(Fixture).GetField(nameof(Fixture.Empty))!.SetValue(null, true);
    Fixture.Refresh();
    Check(Html().Contains("No activity in this period yet."), "Empty account is translated");
    CheckEnglish("Empty account");
    language.SetLanguage(AppLanguage.Vi);
    Check(Html().Contains("Chưa có hoạt động trong khoảng thời gian này."), "Empty account returns to Vietnamese");
});
Console.WriteLine($"{passed} localization checks passed.");

public sealed class Fixture : ComponentBase
{
    public static ProgressDashboard Dashboard = null!;
    public static bool Empty;
    private static Fixture instance = null!;
    private static readonly RoadmapItemDto[] Items =
    [new() { Id = 1, SkillName = "Time Management", Title = "Time management lesson", CompletedActivityCount = 2, TotalActivityCount = 6, VideoCompletedAtUtc = DateTime.UtcNow, ScriptCompletedAtUtc = DateTime.UtcNow }];
    public static void Refresh() => instance.StateHasChanged();
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        instance = this;
        builder.OpenComponent<ProgressDashboard>(0);
        builder.AddAttribute(1, "Items", Empty ? Array.Empty<RoadmapItemDto>() : Items);
        builder.AddAttribute(2, "UserId", 1);
        builder.AddComponentReferenceCapture(3, component => Dashboard = (ProgressDashboard)component);
        builder.CloseComponent();
    }
}
public sealed class NoBrowserJs : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult(default(TValue)!);
}
