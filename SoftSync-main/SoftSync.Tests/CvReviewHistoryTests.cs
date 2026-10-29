using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using SoftSync.Presentation.Components.Shared;
using SoftSync.Presentation.Services;
using Xunit;
using Review = SoftSync.Presentation.Services.CvReviewService;

namespace SoftSync.Tests;

public sealed class CvReviewHistoryTests
{
    private const string Cv = "Backend developer building accessible websites and REST API integrations with React and TypeScript.";
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [Fact]
    public async Task Fresh_workspace_reuses_saved_review_without_an_AI_call_or_duplicate_history_and_language_changes_invalidate_it()
    {
        await using var provider = Services();
        var document = await AddDocument(provider);
        var ai = new StubModel();
        using (var review = new Review(ai))
            await Invoke(Component(provider, review, document), "Analyze");
        Assert.Equal(1, ai.Calls);
        using var freshReview = new Review(ai);
        var reopened = Component(provider, freshReview, document);
        await Invoke(reopened, "Analyze");
        Assert.Equal(1, ai.Calls);
        Assert.True(Field<bool>(reopened, "saved"));
        Assert.NotNull(Field<string?>(reopened, "notice"));
        Assert.Equal("result", Field<string>(reopened, "screen"));
        await using var scope = provider.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<CvWorkspaceService>();
        Assert.Single(await store.ListReviewsAsync(101));
        Set(reopened, "L", new LocalizationService(AppLanguage.En));
        await Invoke(reopened, "Analyze");
        Assert.Equal(2, ai.Calls);
        Assert.Equal(2, (await store.ListReviewsAsync(101)).Count);
        Assert.Equal("en", Field<CvReviewResult>(reopened, "result").OutputLanguage);
    }

    [Fact]
    public async Task Opening_CV_and_review_screen_does_not_query_the_document_or_history_store()
    {
        await using var unavailable = new ServiceCollection().BuildServiceProvider();
        using var review = new Review(new StubModel());
        var component = Component(unavailable, review, new(Guid.NewGuid(), "cv.txt", "txt", 1, DateTime.UtcNow, Cv));
        Set(component, "Authentication", new SignedInUser());
        await Invoke(component, "OnInitializedAsync");
        Assert.False(Field<bool>(component, "busy"));
        Assert.False(Field<bool>(component, "documentsLoaded"));
        Assert.False(Field<bool>(component, "historyLoaded"));
        Assert.Null(Field<string?>(component, "error"));
        await Invoke(component, "Go", "review");
        Assert.Equal("review", Field<string>(component, "screen"));
        Assert.Null(Field<string?>(component, "error"));
    }

    [Fact]
    public async Task Library_loads_metadata_once_and_fetches_only_the_selected_document()
    {
        await using var provider = Services();
        var doc = await AddDocument(provider);
        using var review = new Review(new StubModel());
        var component = Component(provider, review, doc);
        Set(component, "selected", null!);
        await Invoke(component, "Go", "library");
        Assert.True(Field<bool>(component, "documentsLoaded"));
        Assert.False(Field<bool>(component, "historyLoaded"));
        Assert.False(Field<bool>(component, "busy"));
        var metadata = Assert.Single(Field<List<CvCareerWorkspace.CvDocument>>(component, "Documents"));
        Assert.Empty(metadata.Text);
        await Invoke(component, "Select", metadata);
        Assert.Equal(Cv, Field<CvCareerWorkspace.CvDocument>(component, "selected").Text);
        Assert.Equal("review", Field<string>(component, "screen"));
        await using var unavailable = new ServiceCollection().BuildServiceProvider();
        Set(component, "Scopes", unavailable.GetRequiredService<IServiceScopeFactory>());
        await Invoke(component, "Go", "library");
        await Invoke(component, "Select", Assert.Single(Field<List<CvCareerWorkspace.CvDocument>>(component, "Documents")));
        Assert.Null(Field<string?>(component, "error"));
        Assert.Equal("review", Field<string>(component, "screen"));
    }

    [Fact]
    public async Task Failed_library_load_does_not_lock_navigation_and_can_be_retried()
    {
        await using var unavailable = new ServiceCollection().BuildServiceProvider();
        await using var provider = Services();
        var doc = await AddDocument(provider);
        using var review = new Review(new StubModel());
        var component = Component(unavailable, review, doc);
        await Invoke(component, "Go", "library");
        Assert.NotNull(Field<string?>(component, "error"));
        Assert.False(Field<bool>(component, "busy"));
        Assert.False(Field<bool>(component, "loadingDocuments"));
        Assert.False(Field<bool>(component, "documentsLoaded"));
        Set(component, "Scopes", provider.GetRequiredService<IServiceScopeFactory>());
        await Invoke(component, "LoadDocuments");
        Assert.True(Field<bool>(component, "documentsLoaded"));
        Assert.Null(Field<string?>(component, "error"));
        await Invoke(component, "Go", "home");
        Assert.Equal("home", Field<string>(component, "screen"));
        Set(component, "Scopes", provider.GetRequiredService<IServiceScopeFactory>());
        await Invoke(component, "Go", "library");
        Assert.True(Field<bool>(component, "documentsLoaded"));
        Assert.Single(Field<List<CvCareerWorkspace.CvDocument>>(component, "Documents"));
        Assert.Null(Field<string?>(component, "error"));
    }

    [Fact]
    public async Task Completed_analysis_is_saved_without_clicking_save_and_survives_reopening()
    {
        await using var provider = Services();
        var doc = await AddDocument(provider);
        using var review = new Review(new StubModel());
        var component = Component(provider, review, doc);

        await Invoke(component, "Analyze");

        Assert.True(Field<bool>(component, "saved"));
        Assert.Equal("result", Field<string>(component, "screen"));
        Assert.Null(Field<string?>(component, "error"));
        Assert.Single(Field<List<CvCareerWorkspace.ReviewEntry>>(component, "History"));
        await Invoke(component, "Save");
        await using var fresh = provider.CreateAsyncScope();
        var store = fresh.ServiceProvider.GetRequiredService<CvWorkspaceService>();
        var saved = Assert.Single(await store.ListReviewsAsync(101));
        Assert.Equal(doc.Id, saved.DocumentId);
        Assert.Equal(doc.Name, saved.Name);
        Assert.Equal(70, JsonSerializer.Deserialize<CvReviewResult>(saved.ResultJson)!.QualityScore);
        Assert.Empty(await store.ListReviewsAsync(102));
    }

    [Fact]
    public async Task Failed_auto_save_preserves_the_result_and_manual_retry_saves_once()
    {
        await using var provider = Services();
        var doc = await AddDocument(provider);
        using var review = new Review(new StubModel());
        // A missing persistence service simulates a store failure after AI completion.
        await using var unavailable = new ServiceCollection().BuildServiceProvider();
        var component = Component(unavailable, review, doc);

        await Invoke(component, "Analyze");

        Assert.False(Field<bool>(component, "saved"));
        Assert.NotNull(Field<CvReviewResult?>(component, "result"));
        Assert.Equal("result", Field<string>(component, "screen"));
        Assert.NotNull(Field<string?>(component, "error"));
        Assert.Empty(Field<List<CvCareerWorkspace.ReviewEntry>>(component, "History"));
        Set(component, "Scopes", provider.GetRequiredService<IServiceScopeFactory>());
        await Invoke(component, "Save");
        await Invoke(component, "Save");

        Assert.True(Field<bool>(component, "saved"));
        Assert.Null(Field<string?>(component, "error"));
        Assert.Single(Field<List<CvCareerWorkspace.ReviewEntry>>(component, "History"));
        await using var fresh = provider.CreateAsyncScope();
        Assert.Single(await fresh.ServiceProvider.GetRequiredService<CvWorkspaceService>().ListReviewsAsync(101));
    }

    private static ServiceProvider Services()
    {
        var name = Guid.NewGuid().ToString();
        return new ServiceCollection().AddDbContext<SoftSyncDbContext>(o => o.UseInMemoryDatabase(name))
            .AddScoped<CvWorkspaceService>().BuildServiceProvider();
    }

    private static async Task<SavedCv> AddDocument(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CvWorkspaceService>()
            .AddDocumentAsync(101, "cv.txt", "txt", Cv, [1]);
    }

    private static CvCareerWorkspace Component(ServiceProvider provider, Review review, SavedCv doc)
    {
        var component = new CvCareerWorkspace();
        Set(component, "Scopes", provider.GetRequiredService<IServiceScopeFactory>());
        Set(component, "Review", review);
        Set(component, "Logger", NullLogger<CvCareerWorkspace>.Instance);
        Set(component, "L", new SoftSync.Presentation.Services.LocalizationService());
        Set(component, "userId", 101);
        Set(component, "selected", new CvCareerWorkspace.CvDocument(doc.Id, doc.Name, doc.Extension, doc.Size, doc.CreatedAtUtc, doc.Text));
        return component;
    }

    private static void Set(object target, string name, object value)
    {
        var type = target.GetType();
        while (type is not null)
        {
            if (type.GetProperty(name, Private) is { } property) { property.SetValue(target, value); return; }
            if (type.GetField(name, Private) is { } field) { field.SetValue(target, value); return; }
            type = type.BaseType;
        }
        throw new InvalidOperationException(name);
    }

    private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private)!.GetValue(target)!;
    private static Task Invoke(object target, string name, params object[] args) => (Task)target.GetType().GetMethod(name, Private)!.Invoke(target, args)!;

    private sealed class SignedInUser : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        public override Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity([new(System.Security.Claims.ClaimTypes.NameIdentifier, "101")], "Test"))));
    }

    private sealed class StubModel : ICvReviewLanguageModel
    {
        public int Calls { get; private set; }
        public string Model => "test-model";
        private static CvReviewText Text() => new() { Vi = "Nhận xét cụ thể", En = "Specific feedback" };
        public Task<CvReviewAiResponse> AnalyzeAsync(string prompt, object input, CancellationToken token)
        {
            Calls++;
            return Task.FromResult(new CvReviewAiResponse
            {
                Summary = Text(), Strengths = [Text()], Improvements = [Text()],
                Criteria = new[] { "content", "clarity", "achievements", "structure" }
                    .Select(id => new CvReviewCriterion { Id = id, Score = 70, Feedback = Text() }).ToList(),
                Actions = [new() { Priority = "High", Section = Text(), Evidence = "REST API integrations", Issue = Text(), Action = Text() }]
            });
        }
    }
}
