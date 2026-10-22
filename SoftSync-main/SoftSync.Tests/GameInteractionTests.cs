using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components;
using SoftSync.Presentation.Components.Pages;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public class GameInteractionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Question_has_separate_legend_clickable_options_and_retains_selection(bool submitting)
    {
        var html = await RenderQuestion(new() { [nameof(QuestionHarness.Submitting)] = submitting });
        Assert.Contains("<fieldset class=\"ss-quiz-question\">", html);
        Assert.Contains("class=\"ss-quiz-question-title\"", html);
        Assert.Contains("</legend>", html);
        Assert.True(html.IndexOf("</legend>", StringComparison.Ordinal) < html.IndexOf("<label class=\"ss-quiz-option\"", StringComparison.Ordinal));
        for(var i = 1; i <= 4; i++)
        {
            Assert.Contains($"for=\"quiz-1-option-{i}\"", html);
            Assert.Contains($"id=\"quiz-1-option-{i}\"", html);
        }
        Assert.Contains("type=\"radio\" name=\"question-1\"", html);
        Assert.Contains("checked", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("IsCorrect", html);
        Assert.DoesNotContain("class=\"form-check ", html);
        if(submitting) Assert.Contains("disabled checked", html);
        else Assert.DoesNotContain("disabled checked", html);
    }

    [Theory]
    [InlineData(19, false)]
    [InlineData(19, true)]
    [InlineData(20, true)]
    [InlineData(30, true)]
    public async Task Variable_length_quiz_progress_and_submit_use_actual_question_count(int count, bool complete)
    {
        var html = await RenderQuestion(new() { [nameof(QuestionHarness.QuestionCount)] = count, [nameof(QuestionHarness.AnswerAll)] = complete, [nameof(QuestionHarness.LastQuestion)] = true });
        Assert.Contains($"Question {count} of {count}", html);
        Assert.Contains($"Answered {(complete ? count : 1)} of {count}", html);
        Assert.Contains($"aria-valuemax=\"{count}\"", html);
        var submitButton = System.Text.RegularExpressions.Regex.Match(html, "<button[^>]*class=\"btn btn-success\"[^>]*>").Value;
        Assert.NotEmpty(submitButton);
        if(complete) Assert.DoesNotContain("disabled", submitButton);
        else Assert.Contains("disabled", submitButton);
        if(complete) Assert.Contains("width: 100%", html);
    }

    private static async Task<string> RenderQuestion(Dictionary<string, object?> parameters)
    {
        await using var services = new ServiceCollection().AddLogging()
            .AddSingleton(new LocalizationService(AppLanguage.En))
            .AddSingleton<AuthenticationStateProvider, TestAuthentication>()
            .AddSingleton<NavigationManager, TestNavigation>()
            .BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<QuestionHarness>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    // Static markup snapshot of Game's real render tree, without mounting its
    // InteractiveServer circuit in HtmlRenderer. Event behavior is tested above/below.
    public class QuestionHarness : ComponentBase
    {
        [Parameter] public bool Submitting { get; set; }
        [Parameter] public int QuestionCount { get; set; } = 10;
        [Parameter] public bool AnswerAll { get; set; }
        [Parameter] public bool LastQuestion { get; set; }
        [Inject] public LocalizationService Localization { get; set; } = null!;
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var game = new Game();
            typeof(LocalizedComponentBase).GetProperty("L", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, Localization);
            typeof(Game).GetProperty("Ui", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, Localization);
            SetField(game, "loading", false);
            SetField(game, "submitting", Submitting);
            SetField(game, "quiz", new StudentQuizDto { Title = "Communication", Questions = Enumerable.Range(1, QuestionCount).Select(id => new StudentQuizQuestionDto {
                Id = id, QuestionText = "A long question title that must remain above the first answer",
                Options = Enumerable.Range(1, 4).Select(option => new StudentQuizOptionDto { Id = option, Text = $"Answer {option} <script>" }).ToList()
            }).ToList() });
            Field<Dictionary<int, int>>(game, "answers")[1] = 2;
            if(AnswerAll) for(var id = 1; id <= QuestionCount; id++) Field<Dictionary<int, int>>(game, "answers")[id] = 2;
            if(LastQuestion) SetField(game, "currentIndex", QuestionCount - 1);
            typeof(Game).GetMethod("BuildRenderTree", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, [builder]);
        }
    }

    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() { Initialize("http://localhost/", "http://localhost/game"); }
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Repeated_start_uses_one_operation_and_disposes_its_scope()
    {
        var completion = new TaskCompletionSource<StudentQuizSessionDto?>();
        var scopes = new List<ScopeProbe>();
        await using var services = Services(scopes, () => completion.Task);
        var game = CreateGame(services);
        var first = Invoke(game, "StartAsync", 7);
        await Invoke(game, "StartAsync", 7);
        Assert.Single(scopes);
        Assert.False(scopes[0].Disposed);
        completion.SetResult(null);
        await first;
        Assert.True(scopes[0].Disposed);
        Assert.False(Field<bool>(game, "starting"));
    }

    [Fact]
    public async Task Failed_start_is_recoverable_and_retry_uses_a_fresh_scope()
    {
        var scopes = new List<ScopeProbe>();
        await using var services = Services(scopes, () => Task.FromException<StudentQuizSessionDto?>(new InvalidOperationException("private database details")));
        var game = CreateGame(services);
        await Invoke(game, "StartAsync", 7);
        await Invoke(game, "StartAsync", 7);
        Assert.Equal(2, scopes.Count);
        Assert.NotSame(scopes[0], scopes[1]);
        Assert.All(scopes, scope => Assert.True(scope.Disposed));
        Assert.False(Field<bool>(game, "starting"));
        Assert.DoesNotContain("private database", Field<string>(game, "errorMessage"));
        Assert.NotEmpty(Field<string>(game, "errorMessage"));
    }

    [Fact]
    public async Task Failed_submit_retains_answers_and_unlocks_retry()
    {
        var scopes = new List<ScopeProbe>();
        await using var services = Services(scopes, () => Task.FromResult<StudentQuizSessionDto?>(null));
        var game = CreateGame(services);
        SetField(game, "quiz", new StudentQuizDto { Questions = Enumerable.Range(1, 10).Select(id => new StudentQuizQuestionDto { Id = id }).ToList() });
        var answers = Field<Dictionary<int, int>>(game, "answers");
        for(var i = 1; i <= 10; i++) answers[i] = i * 4;
        await Invoke(game, "SubmitAsync");
        Assert.Equal(10, answers.Count);
        Assert.False(Field<bool>(game, "submitting"));
        Assert.NotEmpty(Field<string>(game, "errorMessage"));
        Assert.All(scopes, scope => Assert.True(scope.Disposed));
    }

    private static ServiceProvider Services(List<ScopeProbe> scopes, Func<Task<StudentQuizSessionDto?>> start)
    {
        return new ServiceCollection()
            .AddScoped(_ => { var probe = new ScopeProbe(); scopes.Add(probe); return probe; })
            .AddScoped<IChallengeService>(provider => {
                var proxy = DispatchProxy.Create<IChallengeService, ChallengeProxy>();
                var implementation = (ChallengeProxy)(object)proxy;
                implementation.Probe = provider.GetRequiredService<ScopeProbe>();
                implementation.Start = start;
                return proxy;
            }).BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private static Game CreateGame(ServiceProvider services)
    {
        var game = new Game();
        typeof(Game).GetProperty("ScopeFactory", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, services.GetRequiredService<IServiceScopeFactory>());
        typeof(Game).GetProperty("AuthenticationStateProvider", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, new TestAuthentication());
        typeof(LocalizedComponentBase).GetProperty("L", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, new LocalizationService(AppLanguage.En));
        return game;
    }
    private static Task Invoke(Game game, string method, params object[] arguments) => (Task)typeof(Game).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, arguments)!;
    private static T Field<T>(Game game, string name) => (T)typeof(Game).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
    private static void SetField(Game game, string name, object value) => typeof(Game).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, value);

    public class ChallengeProxy : DispatchProxy
    {
        public ScopeProbe Probe = null!;
        public Func<Task<StudentQuizSessionDto?>> Start = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Assert.False(Probe.Disposed);
            Assert.Equal(23, (int)args![1]!); // Identity, not a user-supplied route value.
            return method!.Name switch {
                "StartAttemptAsync" => Start(),
                "SubmitAsync" => Task.FromException<QuizAttemptResultDto?>(new InvalidOperationException("database failure")),
                _ => throw new NotSupportedException(method.Name)
            };
        }
    }
    public class ScopeProbe : IAsyncDisposable
    {
        public bool Disposed;
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
    }
    private sealed class TestAuthentication : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "23")], "test"))));
    }
}
