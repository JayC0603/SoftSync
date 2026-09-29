using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Services;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class AiPostgresFact : FactAttribute
{
    public AiPostgresFact()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_DB")))
            Skip = "Requires isolated PostgreSQL acceptance database.";
    }
}
public sealed class AiRestartFact : FactAttribute
{
    public AiRestartFact()
    {
        if (Environment.GetEnvironmentVariable("AI_PROVIDER_RESTART_PHASE") is not ("write" or "read"))
            Skip = "Run the two-process restart acceptance script.";
    }
}

public sealed class AiProviderTests
{
    [Theory]
    [InlineData("http://127.0.0.1/")]
    [InlineData("https://generativelanguage.googleapis.com.evil.invalid/v1beta/openai/")]
    [InlineData("https://generativelanguage.googleapis.com/v1beta/openai/?key=secret")]
    [InlineData("https://user:secret@generativelanguage.googleapis.com/v1beta/openai/")]
    [InlineData("https://api.openai.com/v1/")]
    public void Untrusted_or_wrong_provider_endpoints_are_rejected(string endpoint)
    {
        var input = Draft(); input.Endpoint = endpoint;
        Assert.Throws<AiConfigurationException>(() => AiProviderRules.Validate(input));
    }

    [Theory]
    [InlineData(401, AiConnectionStatus.AuthenticationFailed)]
    [InlineData(403, AiConnectionStatus.AuthenticationFailed)]
    [InlineData(429, AiConnectionStatus.RateLimited)]
    [InlineData(400, AiConnectionStatus.ModelUnavailable)]
    [InlineData(404, AiConnectionStatus.ModelUnavailable)]
    [InlineData(500, AiConnectionStatus.Unavailable)]
    [InlineData(502, AiConnectionStatus.Unavailable)]
    [InlineData(503, AiConnectionStatus.Unavailable)]
    public async Task Test_connection_maps_errors_without_returning_provider_body(int status, AiConnectionStatus expected)
    {
        var handler = new RecordingHandler { Status = (HttpStatusCode)status };
        var tester = new AiProviderConnectionTester(new HttpFactory(handler));
        var actual = await tester.TestAsync(Runtime("secret-never-in-result", "test-model"));
        Assert.Equal(expected, actual);
        Assert.DoesNotContain("secret-never-in-result", handler.Body);
        Assert.DoesNotContain("secret-never-in-result", handler.Url);
    }
    [Fact]
    public async Task Test_connection_timeout_and_malformed_response_are_safe()
    {
        var handler = new RecordingHandler { Timeout = true };
        Assert.Equal(AiConnectionStatus.Timeout, await new AiProviderConnectionTester(new HttpFactory(handler)).TestAsync(Runtime("synthetic", "model")));
        handler.Timeout = false; handler.Malformed = true;
        Assert.Equal(AiConnectionStatus.InvalidResponse, await new AiProviderConnectionTester(new HttpFactory(handler)).TestAsync(Runtime("synthetic", "model")));
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new AiProviderConnectionTester(new HttpFactory(handler)).TestAsync(Runtime("synthetic", "model"), canceled.Token));
    }
    [Fact]
    public async Task All_management_operations_reject_untrusted_callers_before_accessing_DB_or_secret()
    {
        var service = new AiProviderManagementService(null!, new DeniedAccess(), null!, null!);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ListAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveAsync(Draft()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SetActiveAsync(Guid.NewGuid(), Guid.NewGuid()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SetEnabledAsync(Guid.NewGuid(), Guid.NewGuid(), true));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.TestAsync(Draft()));
    }

    [Fact]
    public async Task Provider_failure_logs_never_include_key_response_or_authorization()
    {
        var logger = new SafeLogger();
        var handler = new RecordingHandler { Status = HttpStatusCode.Unauthorized };
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["AiApi:Enabled"] = "true", ["AiApi:ApiKey"] = "synthetic-secret-leak-test" }).Build();
        var client = new HuggingFaceJsonClient(new HttpFactory(handler), settings, logger);
        Assert.Null(await client.AskAsync<CvAnalysisResult>("cv-review", "test", new { test = true }));
        Assert.NotEmpty(logger.Messages);
        Assert.DoesNotContain("synthetic-secret-leak-test", string.Join(' ', logger.Messages));
        Assert.DoesNotContain("Authorization", string.Join(' ', logger.Messages));
        Assert.DoesNotContain("Do not expose", string.Join(' ', logger.Messages));
    }

    [AiPostgresFact]
    public async Task PostgreSQL_protected_secret_hot_replacement_CV_ownership_and_single_default()
    {
        var factory = await Database("ai_" + Guid.NewGuid().ToString("N"));
        using var protectionServices = Protection(factory);
        var protector = new AiSecretProtector(protectionServices.GetRequiredService<IDataProtectionProvider>());
        var handler = new RecordingHandler(); var clients = new HttpFactory(handler);
        var management = new AiProviderManagementService(factory, new AllowedAccess(), protector, new AiProviderConnectionTester(clients));
        var options = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["AiApi:Enabled"] = "true", ["AiApi:ApiKey"] = "synthetic-env", ["AiApi:Model"] = "env-model" }).Build();
        var resolver = new AiProviderConfigurationResolver(factory, protector, options, NullLogger<AiProviderConfigurationResolver>.Instance);
        Assert.Equal("synthetic-env", (await resolver.ResolveAsync())!.ApiKey);
        var input = Draft(); input.NewApiKey = "synthetic-key-A"; input.CvModel = "cv-A";
        Assert.Equal(AiConnectionStatus.Success, await management.TestAsync(input));
        Assert.Empty(await management.ListAsync()); // Unsaved test must not persist.
        var a = await management.SaveAsync(input);
        Assert.True(a.HasApiKey); Assert.False(a.IsDefault);
        await management.SetActiveAsync(a.Id, a.Revision);
        a = Assert.Single(await management.ListAsync());
        var emptyOptions = new ConfigurationBuilder().Build();
        var databaseOnly = new AiProviderConfigurationResolver(factory, protector, emptyOptions, NullLogger<AiProviderConfigurationResolver>.Instance);
        Assert.True(await new CvAiReadiness(emptyOptions, databaseOnly).IsConfiguredAsync());
        Assert.DoesNotContain("synthetic-key-A", JsonSerializer.Serialize(a));
        await using (var db = factory.CreateDbContext())
        {
            var row = Assert.Single(await db.AiProviderConfigurations.ToListAsync());
            Assert.NotEqual("synthetic-key-A", row.EncryptedApiKey);
            Assert.DoesNotContain("synthetic-key-A", row.EncryptedApiKey);
            Assert.Equal("synthetic-key-A", protector.Unprotect(row.EncryptedApiKey));
            Assert.NotEmpty(await db.DataProtectionKeys.ToListAsync());
            db.Users.Add(new ApplicationUser { Id = 901, UserName = "synthetic-owner", NormalizedUserName = "SYNTHETIC-OWNER" });
            await db.SaveChangesAsync();
        }
        var transport = new HuggingFaceJsonClient(clients, options, NullLogger<HuggingFaceJsonClient>.Instance, resolver);
        var cv = new AiCvAnalysisService(transport, options);
        var reviews = new CvReviewService(factory, new CvTextExtractor(new()), cv, 5 * 1024 * 1024);
        var bytes = CvAcceptanceFixtures.Docx(CvAcceptanceFixtures.English);
        var review = await reviews.UploadAsync(901, new MemoryStream(bytes), "synthetic.docx", CvRuntimeReadinessTests.DocxMime, bytes.Length);
        Assert.Equal("Bearer synthetic-key-A", handler.Authorization); Assert.Equal("cv-A", handler.Model);
        await using (var stored = factory.CreateDbContext()) Assert.Equal("cv-A", (await stored.Set<CvAnalysis>().SingleAsync(x => x.Id == review.AnalysisId)).Model);
        Assert.Null(await reviews.GetAsync(902, review.AnalysisId));
        Assert.Null(await reviews.ReanalyzeAsync(902, review.DocumentId));
        var edit = Edit(a); edit.Model = "main-B"; edit.CvModel = "cv-B";
        var b = await management.SaveAsync(edit); // Blank key retains original.
        Assert.Equal("synthetic-key-A", (await resolver.ResolveAsync())!.ApiKey);
        edit = Edit(b); edit.NewApiKey = "synthetic-key-B";
        Assert.Equal(AiConnectionStatus.Success, await management.TestAsync(edit));
        Assert.Equal("synthetic-key-A", (await resolver.ResolveAsync())!.ApiKey); // Test didn't replace key.
        b = await management.SaveAsync(edit);
        var again = await reviews.ReanalyzeAsync(901, review.DocumentId);
        Assert.Equal("Bearer synthetic-key-B", handler.Authorization); Assert.Equal("cv-B", handler.Model);
        await using (var stored = factory.CreateDbContext())
        {
            Assert.Equal("cv-B", (await stored.Set<CvAnalysis>().SingleAsync(x => x.Id == again!.AnalysisId)).Model);
            Assert.Equal("cv-A", (await stored.Set<CvAnalysis>().SingleAsync(x => x.Id == review.AnalysisId)).Model);
        }
        await Assert.ThrowsAsync<AiConfigurationException>(() => management.SaveAsync(edit)); // Stale revision.
        await Assert.ThrowsAsync<AiConfigurationException>(() => management.SetEnabledAsync(b.Id, b.Revision, false));
        var disabled = Draft(); disabled.IsEnabled = false; disabled.NewApiKey = null; disabled.DisplayName = "Disabled";
        var d = await management.SaveAsync(disabled);
        await Assert.ThrowsAsync<AiConfigurationException>(() => management.SetActiveAsync(d.Id, d.Revision));
        var other = Draft(); other.DisplayName = "Other"; other.NewApiKey = "synthetic-key-C";
        var c = await management.SaveAsync(other);
        async Task<bool> Switch(AiProviderDto row)
        {
            try { await management.SetActiveAsync(row.Id, row.Revision); return true; }
            catch (AiConfigurationException) { return false; } // A competing save may correctly reject a stale revision.
        }
        var switches = await Task.WhenAll(Switch(b), Switch(c));
        Assert.Contains(true, switches);
        Assert.Single(await management.ListAsync(), x => x.IsDefault);
        await using var verify = factory.CreateDbContext();
        verify.Add(new AiProviderConfiguration { Id = Guid.NewGuid(), IsDefault = true, IsEnabled = true, Revision = Guid.NewGuid() });
        await Assert.ThrowsAsync<DbUpdateException>(() => verify.SaveChangesAsync());
    }

    [AiPostgresFact]
    public async Task Persisted_identity_role_required_even_with_stale_Admin_claim()
    {
        var factory = await Database("auth_" + Guid.NewGuid().ToString("N"));
        await using var db = factory.CreateDbContext();
        db.Users.Add(new ApplicationUser { Id = 801, UserName = "auth-test" });
        db.Roles.Add(new IdentityRole<int> { Id = 801, Name = "Admin", NormalizedName = "ADMIN" });
        db.UserRoles.Add(new IdentityUserRole<int> { UserId = 801, RoleId = 801 }); await db.SaveChangesAsync();
        foreach (var role in new[] { "User", "Teacher", "" })
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new IdentityAiAdminAccess(new Auth(role), factory).RequireAdminAsync());
        var access = new IdentityAiAdminAccess(new Auth("Admin"), factory);
        Assert.Equal(801, await access.RequireAdminAsync());
        db.UserRoles.Remove(await db.UserRoles.SingleAsync()); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => access.RequireAdminAsync());
    }

    // Run in two separate dotnet test containers/processes with the SAME PostgreSQL schema/key ring.
    [AiRestartFact]
    public async Task Secret_survives_process_restart()
    {
        var phase = Environment.GetEnvironmentVariable("AI_PROVIDER_RESTART_PHASE");
        var factory = await Database("ai_provider_restart", migrate: phase == "write");
        using var provider = Protection(factory);
        var protector = new AiSecretProtector(provider.GetRequiredService<IDataProtectionProvider>());
        if (phase == "write")
        {
            var management = new AiProviderManagementService(factory, new AllowedAccess(), protector, null!);
            var draft = Draft(); draft.NewApiKey = "synthetic-restart-key";
            var row = await management.SaveAsync(draft); await management.SetActiveAsync(row.Id, row.Revision);
            await using var db = factory.CreateDbContext();
            Assert.DoesNotContain("synthetic-restart-key", (await db.AiProviderConfigurations.SingleAsync()).EncryptedApiKey);
            Assert.NotEmpty(await db.DataProtectionKeys.ToListAsync());
        }
        else
        {
            var options = new ConfigurationBuilder().Build();
            var resolver = new AiProviderConfigurationResolver(factory, protector, options, NullLogger<AiProviderConfigurationResolver>.Instance);
            Assert.Equal("synthetic-restart-key", (await resolver.ResolveAsync())!.ApiKey);
            var handler = new RecordingHandler();
            var client = new HuggingFaceJsonClient(new HttpFactory(handler), options, NullLogger<HuggingFaceJsonClient>.Instance, resolver);
            Assert.NotNull(await client.AskAsync<CvAnalysisResult>("cv-review", "Synthetic restart test", new { test = true }));
            Assert.Equal("Bearer synthetic-restart-key", handler.Authorization);
        }
    }

    internal static async Task<CvDbContextFactory> Database(string schema, bool migrate = true)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        if (schema.Length > 80 || schema.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '_')))
            throw new ArgumentException("Test schema must be a bounded safe SQL identifier.");
        var raw = Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_DB")!;
        if (migrate)
        {
            await using var setup = new SoftSyncDbContext(new DbContextOptionsBuilder<SoftSyncDbContext>().UseNpgsql(raw).Options);
            var createSchemaSql = $"CREATE SCHEMA IF NOT EXISTS \"{schema}\"";
            await setup.Database.ExecuteSqlRawAsync(createSchemaSql);
        }
        var factory = new CvDbContextFactory(new DbContextOptionsBuilder<SoftSyncDbContext>().UseNpgsql(raw + ";SearchPath=" + schema).Options);
        if (migrate) { await using var db = factory.CreateDbContext(); await db.Database.MigrateAsync(); }
        return factory;
    }
    internal static ServiceProvider Protection(CvDbContextFactory factory)
    {
        var services = new ServiceCollection(); services.AddLogging();
        services.AddScoped(_ => factory.CreateDbContext());
        services.AddSingleton<EfDataProtectionKeyRepository>();
        services.AddDataProtection().SetApplicationName("SoftSync");
        services.AddOptions<KeyManagementOptions>().Configure<EfDataProtectionKeyRepository>((options, repo) => options.XmlRepository = repo);
        return services.BuildServiceProvider();
    }
    private static AiProviderEditDto Draft() => new() { DisplayName = "Synthetic provider", Model = "model-A", Endpoint = AiProviderRules.Endpoint(AiProviderType.Gemini) };
    private static AiProviderEditDto Edit(AiProviderDto row) => new() { Id = row.Id, Revision = row.Revision, Provider = row.Provider, DisplayName = row.DisplayName, Model = row.Model, CvModel = row.CvModel, Endpoint = row.Endpoint, IsEnabled = row.IsEnabled };
    private static ResolvedAiProviderConfiguration Runtime(string key, string model) => new() { Endpoint = AiProviderRules.Endpoint(AiProviderType.Gemini), ApiKey = key, Model = model, TimeoutSeconds = 1 };
    private sealed class AllowedAccess : IAiAdminAccess { public Task<int> RequireAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(801); }
    private sealed class DeniedAccess : IAiAdminAccess { public Task<int> RequireAdminAsync(CancellationToken cancellationToken = default) => throw new UnauthorizedAccessException(); }
    private sealed class SafeLogger : ILogger<HuggingFaceJsonClient>
    {
        public List<string> Messages = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }
    private sealed class Auth(string role) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(
            role.Length == 0 ? new ClaimsPrincipal(new ClaimsIdentity()) : new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, "801"), new Claim(ClaimTypes.Role, role) }, "Synthetic"))));
    }
    private sealed class HttpFactory(RecordingHandler handler) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => new(handler, disposeHandler: false); }
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpStatusCode Status = HttpStatusCode.OK;
        public bool Timeout, Malformed;
        public string Body = "", Url = "", Authorization = "", Model = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Timeout) { await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken); }
            Url = request.RequestUri!.AbsoluteUri; Authorization = request.Headers.Authorization?.ToString() ?? "";
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var input = JsonDocument.Parse(Body); Model = input.RootElement.GetProperty("model").GetString()!;
            var result = new CvAnalysisResult { Summary = "Evidence-based synthetic review", Strengths = ["Project evidence"], Weaknesses = ["Missing metrics"],
                SkillsDetected = ["C#"], MissingInformation = ["Outcomes"], Suggestions = ["Add truthful outcomes"],
                Sections = [new() { Section = "Projects", Feedback = "Project described", Suggestions = ["Add outcomes"] }] };
            var payload = JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = JsonSerializer.Serialize(result) } } } });
            return new(Status) { Content = new StringContent(Malformed ? "bad json" : Status == HttpStatusCode.OK ? payload : "Do not expose synthetic provider error or credentials.") };
        }
    }
}
