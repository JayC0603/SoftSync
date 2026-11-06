using SoftSync.Presentation.Configuration;
using SoftSync.Presentation.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using SoftSync.BLL.Auth;
using SoftSync.BLL.AI;
using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Services;
using SoftSync.BLL.Services.Fake;
using SoftSync.Presentation.Components.Account;
using SoftSync.Presentation.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;

// Postgres maps DateTime to `timestamptz`. Some entities store non-UTC DateTime
// (e.g. CreatedAt); this switch keeps the pre-6.0 behavior so those writes don't
// throw. Must be set before the first Npgsql connection is opened.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<WebsiteAnalyticsService>();

// Generate an idempotent schema from the checked-in EF migrations without
// connecting to a database or starting the web server.
var schemaArgument = args.FirstOrDefault(argument => argument.StartsWith("--export-database-schema=", StringComparison.Ordinal));
if (schemaArgument is not null)
{
    var outputPath = Path.GetFullPath(schemaArgument.Split('=', 2)[1]);
    await using var schemaDb = new SoftSyncDbContext(new DbContextOptionsBuilder<SoftSyncDbContext>()
        .UseNpgsql("Host=localhost;Database=softsync;Username=softsync").Options);
    var migrator = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>(schemaDb);
    var sql = migrator.GenerateScript(options: Microsoft.EntityFrameworkCore.Migrations.MigrationsSqlGenerationOptions.Idempotent);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    await File.WriteAllTextAsync(outputPath, sql);
    Console.WriteLine($"Database schema written to {outputPath}");
    return;
}

builder.AddSoftSyncUi();
var demoMode = builder.AddSoftSyncDatabase();
var googleAuthConfigured = builder.AddSoftSyncIdentity();
builder.AddSoftSyncApplicationServices();

var app = builder.Build();

// Behind Vercel's reverse proxy TLS terminates at the edge, so trust the
// X-Forwarded-Proto/For headers — otherwise the app thinks requests are HTTP,
// which breaks Secure cookies and the Blazor Server WebSocket. Must run before
// authentication. KnownNetworks/Proxies are cleared because the proxy hop isn't
// on a known private subnet in Vercel's network.
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else if (!demoMode)
{
    // In the Vercel container TLS is handled by the proxy; redirecting inside the
    // container just breaks. Only redirect to HTTPS during local development.
    app.UseHttpsRedirection();
}

app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // App.razor fingerprints these files by content. A normal refresh can
        // reuse them without revalidating dozens of stylesheets and scripts.
        if (context.Context.Request.Query.TryGetValue("v", out var version)
            && version.Count == 1 && version[0] is { Length: 16 } hash
            && hash.All(Uri.IsHexDigit))
            context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
    }
});
app.UseMiddleware<SystemMetricsMiddleware>();


// Deployment uploads exclude large lesson media. Preserve the existing lesson
// URLs by redirecting them to the verified public media source in the manifest.
var videoManifestPath = Path.Combine(app.Environment.WebRootPath, "video-manifest.json");
var lessonVideos = File.Exists(videoManifestPath)
    ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(videoManifestPath)) ?? new()
    : new Dictionary<string, string>();
app.MapMethods("/videos/{fileName}", ["GET", "HEAD"], (string fileName) =>
{
    if (lessonVideos.TryGetValue(fileName, out var url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == "https"
        && (uri.Host.EndsWith(".public.blob.vercel-storage.com", StringComparison.OrdinalIgnoreCase)
            || (uri.Host.Equals("softsync.id.vn", StringComparison.OrdinalIgnoreCase)
                && uri.AbsolutePath.Equals($"/videos/{fileName}", StringComparison.Ordinal))))
        return Results.Redirect(url);
    return Results.NotFound();
});
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapCvIntegrationEndpoints();
app.MapCvReviewEndpoints();
app.MapGameHistoryEndpoints();
app.MapWebsiteAnalytics();

var learningApi = app.MapGroup("/api/learning/sessions").RequireAuthorization();
learningApi.MapPost("/", async (StartLearningActivityRequest request, System.Security.Claims.ClaimsPrincipal user,
    LearningActivityService activities, CancellationToken ct) =>
{
    if (!LearningActivityService.TryGetUserId(user, out var userId)) return Results.Unauthorized();
    try { return Results.Ok(await activities.StartAsync(userId, request, ct)); }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
});
learningApi.MapPost("/{sessionId:guid}/heartbeat", async (Guid sessionId, LearningHeartbeatRequest request,
    System.Security.Claims.ClaimsPrincipal user, LearningActivityService activities, CancellationToken ct) =>
{
    if (!LearningActivityService.TryGetUserId(user, out var userId)) return Results.Unauthorized();
    if (request.SessionId != sessionId) return Results.BadRequest();
    return await activities.HeartbeatAsync(userId, request, ct) ? Results.Ok() : Results.NotFound();
});
learningApi.MapPost("/{sessionId:guid}/end", async (Guid sessionId, System.Security.Claims.ClaimsPrincipal user,
    LearningActivityService activities, CancellationToken ct) =>
{
    if (!LearningActivityService.TryGetUserId(user, out var userId)) return Results.Unauthorized();
    await activities.EndAsync(userId, sessionId, ct);
    return Results.NoContent();
});

if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/mobile", () => Results.File(
        Path.Combine(app.Environment.ContentRootPath, "Assets", "Dev", "mobile-preview.html"),
        "text/html; charset=utf-8"));
}

// Deployment monitoring uses this lightweight endpoint to check that the web process is
// accepting requests. Keep it independent from page rendering and user state.
app.MapGet("/health", () => Results.Ok(new
    {
        status = "ok",
        authentication = new { google = googleAuthConfigured ? "configured" : "not_configured" }
    }))
    .AllowAnonymous();

var coursesApi = app.MapGroup("/api/courses")
    .AllowAnonymous();

coursesApi.MapGet("/", async (ICourseService courses) =>
    Results.Ok(await courses.GetPublicPublishedAsync()));

coursesApi.MapGet("/{id:int}", async (int id, ICourseService courses) =>
{
    var course = await courses.GetPublicPublishedByIdAsync(id);
    return course is null ? Results.NotFound() : Results.Ok(course);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout + external-login endpoints backing the static Account UI.
app.MapAdditionalIdentityEndpoints();

// Apply migrations at startup; never provision the known demo account outside Development.
await DbInitializer.SeedAsync(app.Services, seedDemoAccount: app.Environment.IsDevelopment() || demoMode);

if (args.Contains("--initialize-database"))
{
    Console.WriteLine("Database migrations and application seed completed.");
    await app.DisposeAsync();
    return;
}

app.Run();
