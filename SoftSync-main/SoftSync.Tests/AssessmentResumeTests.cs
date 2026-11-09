using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Presentation.Components.Pages;
using Xunit;

namespace SoftSync.Tests;

public sealed class AssessmentResumeTests
{
    [Theory]
    [InlineData(false, "/assessment/42")]
    [InlineData(true, "/dashboard")]
    public async Task Returning_to_setup_after_refresh_resumes_quiz_until_it_is_submitted(bool submitted, string expected)
    {
        var page = new SetupProbe();
        var navigation = new TestNavigation();
        Inject(page, "AuthProvider", new SignedInUser());
        Inject(page, "Nav", navigation);
        Inject(page, "UserService", Stub<IUserService>.For(_ => Task.FromResult<UserDto?>(
            new() { Id = 42, FullName = "Test Learner", Age = 21, Goal = "communication" })));
        Inject(page, "AssessmentService", Stub<IAssessmentService>.For(_ =>
            Task.FromResult<IEnumerable<AssessmentResultDto>>(submitted ? [new()] : [])));

        await page.Initialize();

        Assert.Equal(expected, new Uri(navigation.Uri).AbsolutePath);
    }

    [Fact]
    public void Quiz_draft_restores_question_option_order_current_position_and_selected_answer()
    {
        var page = new Assessment();
        var type = typeof(Assessment);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        type.GetField("questions", flags)!.SetValue(page, new List<AssessmentQuestionDto>
        {
            new() { Id = 10, Options = [new() { Id = 101 }, new() { Id = 102 }] },
            new() { Id = 20, Options = [new() { Id = 201 }, new() { Id = 202 }] }
        });
        var draftType = type.GetNestedType("AssessmentDraft", BindingFlags.NonPublic)!;
        var draft = Activator.CreateInstance(draftType, nonPublic: true)!;
        draftType.GetProperty("QuestionIds")!.SetValue(draft, new List<int> { 20, 10 });
        draftType.GetProperty("OptionIds")!.SetValue(draft, new Dictionary<int, List<int>> { [20] = [202, 201], [10] = [102, 101] });
        draftType.GetProperty("Answers")!.SetValue(draft, new Dictionary<int, int> { [20] = 202, [10] = 101 });
        draftType.GetProperty("CurrentQuestionIndex")!.SetValue(draft, 1);

        Assert.True((bool)type.GetMethod("TryRestoreDraft", flags)!.Invoke(page, [draft])!);
        var questions = (List<AssessmentQuestionDto>)type.GetField("questions", flags)!.GetValue(page)!;
        Assert.Equal(new[] { 20, 10 }, questions.Select(q => q.Id));
        Assert.Equal(new[] { 102, 101 }, questions[1].Options.Select(o => o.Id));
        Assert.Equal(1, type.GetField("currentQuestionIndex", flags)!.GetValue(page));
        Assert.Equal(101, type.GetField("selectedOptionId", flags)!.GetValue(page));
    }

    private static void Inject(ProfileSetup page, string name, object value) =>
        typeof(ProfileSetup).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(page, value);

    private sealed class SetupProbe : ProfileSetup
    {
        public Task Initialize() => base.OnInitializedAsync();
    }

    private sealed class SignedInUser : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "42")], "test"))));
    }

    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() => Initialize("https://localhost/", "https://localhost/profile-setup");
        protected override void NavigateToCore(string uri, bool forceLoad) => Uri = ToAbsoluteUri(uri).ToString();
        protected override void NavigateToCore(string uri, NavigationOptions options) => Uri = ToAbsoluteUri(uri).ToString();
    }

    public class Stub<T> : DispatchProxy where T : class
    {
        public Func<string, object?> Respond = null!;
        public static T For(Func<string, object?> respond)
        {
            var instance = Create<T, Stub<T>>();
            ((Stub<T>)(object)instance).Respond = respond;
            return instance;
        }
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Respond(method!.Name);
    }
}
