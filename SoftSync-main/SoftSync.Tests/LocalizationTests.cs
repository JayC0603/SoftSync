using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Services;
using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components;
using SoftSync.Presentation.Components.Shared;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class LocalizationTests
{
    [Fact]
    public void UI_catalog_is_complete_in_both_directions_and_preserves_unknown_user_content()
    {
        foreach (var pair in UiTextCatalog.Pairs)
        {
            Assert.False(string.IsNullOrWhiteSpace(pair.En));
            Assert.False(string.IsNullOrWhiteSpace(pair.Vi));
            Assert.Equal(pair.En, UiTextCatalog.Get(pair.En, AppLanguage.En));
            Assert.Equal(pair.Vi, UiTextCatalog.Get(pair.En, AppLanguage.Vi));
            Assert.Equal(pair.En, UiTextCatalog.Get(pair.Vi, AppLanguage.En));
        }
        const string userContent = "Tuyển developer C# <script>example</script>";
        Assert.Equal(userContent, UiTextCatalog.Get(userContent, AppLanguage.En));
    }

    [Theory]
    [InlineData("en", AppLanguage.En)]
    [InlineData("EN", AppLanguage.En)]
    [InlineData("vi", AppLanguage.Vi)]
    [InlineData(null, AppLanguage.Vi)]
    [InlineData("invalid", AppLanguage.Vi)]
    public void Stored_language_is_normalized_and_users_are_isolated(string? value, AppLanguage expected)
    {
        var userA = new LocalizationService(LocalizationService.Parse(value));
        var userB = new LocalizationService();
        Assert.Equal(expected, userA.Current);
        userA.SetLanguage(AppLanguage.En);
        Assert.Equal(AppLanguage.Vi, userB.Current);
        Assert.Equal("Choose CV", userA.Text("Chọn CV"));
    }

    [Fact]
    public async Task Component_re_renders_on_language_change_and_unsubscribes_on_disposal()
    {
        var language = new LocalizationService();
        var services = new ServiceCollection().AddLogging().AddSingleton(language).BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<Probe>();
            Assert.Contains("Chọn CV", WebUtility.HtmlDecode(output.ToHtmlString()));
            language.SetLanguage(AppLanguage.En);
            Assert.Contains("Choose CV", output.ToHtmlString());
            Assert.DoesNotContain("Chọn CV", WebUtility.HtmlDecode(output.ToHtmlString()));
        });
        await renderer.DisposeAsync();
        // A stale event subscription would invoke the disposed renderer here.
        language.SetLanguage(AppLanguage.Vi);
        await services.DisposeAsync();
    }

    [Theory]
    [InlineData("en")]
    [InlineData("vi")]
    public async Task Quiz_AI_receives_selected_language(string code)
    {
        var handler = new QuizHandler(code);
        using var http = new HttpClient(handler) { BaseAddress = new(AiApiConfiguration.DefaultBaseUrl) };
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["AiApi:Enabled"] = "true", ["AiApi:ApiKey"] = "synthetic-key" }).Build();
        var client = new HuggingFaceJsonClient(new Factory(http), config, NullLogger<HuggingFaceJsonClient>.Instance);
        var quiz = new AiQuizService(client, new(LocalizationService.Parse(code)));
        Assert.Equal("Example", await quiz.ExplainAnswerAsync(new() { QuestionText = "Example question", CorrectAnswer = "A", SelectedAnswer = "B" }));
        Assert.NotNull(await quiz.GenerateFeedbackAsync(new()));
        Assert.Equal(2, handler.Calls);
    }

    [Theory]
    [InlineData("en", "not available yet")]
    [InlineData("vi", "Chưa thể tạo")]
    public async Task Tutor_fallback_matches_requested_language(string code, string expected)
    {
        var tutor = new AiTutorService(new FailingAssistant());
        var result = await tutor.ExplainLessonAsync(new() { Language = code, LessonTitle = "Example", LessonContent = "Context" }, 1);
        Assert.True(result.IsFallback);
        Assert.Contains(expected, result.Text);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("vi")]
    public async Task CV_adapter_receives_language_and_records_it_without_mutating_input(string code)
    {
        const string jd = "Cần C#";
        var result = await CvRuntimeReadinessTests.Adapter("comparison", request =>
        {
            using var input = JsonDocument.Parse(request.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!);
            Assert.Equal(code, input.RootElement.GetProperty("language").GetString());
            Assert.Equal(jd, input.RootElement.GetProperty("jobDescription").GetString());
        }).AnalyzeAsync(CvAcceptanceFixtures.English, jobDescription: jd, language: code);
        Assert.Equal(code, result.Result.Language);
        Assert.Equal(jd, result.Result.JobDescription);
    }

    public sealed class Probe : LocalizedComponentBase
    { protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, L.Text("Chọn CV")); }
    [Fact]
    public async Task Validation_messages_follow_language_and_validation_state()
    {
        var language = new LocalizationService();
        await using var services = new ServiceCollection().AddLogging().AddSingleton(language).BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var context = new EditContext(new object());
        var messages = new ValidationMessageStore(context);
        var field = new FieldIdentifier(context.Model, "Password");
        messages.Add(field, "Vui lòng nhập mật khẩu.");
        RenderFragment child = builder => { builder.OpenComponent<LocalizedValidationSummary>(0); builder.CloseComponent(); };
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<CascadingValue<EditContext>>(ParameterView.FromDictionary(new Dictionary<string, object?>
            { ["Value"] = context, ["ChildContent"] = child }));
            Assert.Contains("Vui lòng nhập mật khẩu.", WebUtility.HtmlDecode(output.ToHtmlString()));
            language.SetLanguage(AppLanguage.En);
            Assert.Contains("Please enter your password.", output.ToHtmlString());
            Assert.DoesNotContain("Vui lòng nhập", WebUtility.HtmlDecode(output.ToHtmlString()));
            messages.Clear(); messages.Add(field, "Mật khẩu nhập lại không khớp.");
            context.NotifyValidationStateChanged();
            Assert.Contains("Passwords do not match.", output.ToHtmlString());
        });
    }
    private sealed class Factory(HttpClient http) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => http; }
    private sealed class QuizHandler(string expected) : HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            var messages = body.RootElement.GetProperty("messages");
            using var input = JsonDocument.Parse(messages[1].GetProperty("content").GetString()!);
            Assert.Equal(expected, input.RootElement.GetProperty("language").GetString());
            var json = JsonSerializer.Serialize(new { text = "Example", summary = "Example", strengths = new[] { "Example" }, weakAreas = Array.Empty<string>(), suggestedNextPractice = new[] { "Practice" } });
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = json } } } })) };
        }
    }
    private sealed class FailingAssistant : IAiAssistantService
    { public Task<string> GetReplyAsync(string message, int userId) => throw new InvalidOperationException(); }
}
