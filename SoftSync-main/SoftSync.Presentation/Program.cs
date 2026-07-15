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

// Postgres maps DateTime to `timestamptz`. Some entities store non-UTC DateTime
// (e.g. CreatedAt); this switch keeps the pre-6.0 behavior so those writes don't
// throw. Must be set before the first Npgsql connection is opened.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
        options.DetailedErrors = builder.Environment.IsDevelopment());

// Needed so App.razor and scoped UI services can read browser preferences during
// SSR / enhanced navigation and render the first frame without flashing.
builder.Services.AddHttpContextAccessor();

// UI localization (EN/VI). Seed each circuit from the cookie so SSR and the
// interactive render always use the same language.
builder.Services.AddScoped<LocalizationService>(services =>
{
    var request = services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
    return new LocalizationService(LocalizationService.Parse(request?.Cookies["ss-lang"]));
});
builder.Services.AddScoped<UserProfileState>();

// 1. Database Configuration (PostgreSQL).
// Prefer Render's DATABASE_URL so a stale ConnectionStrings__SoftSyncDb value
// cannot override the managed database's current internal connection URL.
// Local/other environments can still use ConnectionStrings:SoftSyncDb.
// IConfiguration includes environment variables and Development User Secrets,
// allowing the same DATABASE_URL key to work on Render and on a local machine.
var databaseUrl = builder.Configuration["DATABASE_URL"];
var connectionString = !string.IsNullOrWhiteSpace(databaseUrl)
    ? BuildNpgsqlConnectionString(databaseUrl)
    : builder.Configuration.GetConnectionString("SoftSyncDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQL is not configured. Set DATABASE_URL on Render or " +
        "ConnectionStrings__SoftSyncDb for another environment.");
}
builder.Services.AddDbContext<SoftSyncDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<EfDataProtectionKeyRepository>();
builder.Services
    .AddDataProtection()
    .SetApplicationName("SoftSync");
builder.Services.AddOptions<KeyManagementOptions>()
    .Configure<EfDataProtectionKeyRepository>((options, repository) =>
    {
        options.XmlRepository = repository;
    });

// 1b. Authentication & Identity (ASP.NET Core Identity + cookie auth)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

// Only register Google when configured — AddGoogle throws at startup on empty keys.
var googleSection = builder.Configuration.GetSection("Authentication:Google");
var googleClientId = googleSection["ClientId"];
var googleClientSecret = googleSection["ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}
authBuilder.AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // dev: no email confirmation gate
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<SoftSyncDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "SoftSync.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Bind sender config + register senders and the OTP service.
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<TwilioOptions>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddScoped<IAppEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<IAppSmsSender, TwilioSmsSender>();
builder.Services.AddScoped<VerificationCodeService>();

// 2. Register Repositories (DAL)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IRoadmapRepository, RoadmapRepository>();
builder.Services.AddScoped<ICaseStudyRepository, CaseStudyRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IMentorRepository, MentorRepository>();

// 3. Register AI Services (BLL - Mocked)
builder.Services.AddScoped<IAiAssessmentService, HuggingFaceAiAssessmentService>();
builder.Services.AddSingleton(_ => AssistantKnowledgeBase.Load(
    Path.Combine(builder.Environment.ContentRootPath, "Assets", "AiKnowledge", "softsync-assistant.vi-en.json")));
builder.Services.AddScoped<KnowledgeBasedAiAssistantService>();
builder.Services.AddScoped<IAiAssistantService, LlmAiAssistantService>();
builder.Services.AddSingleton<PdfDocumentKnowledge>();
builder.Services.AddScoped<HuggingFaceJsonClient>();
builder.Services.AddScoped<IAiLearningEvaluationService, AiLearningEvaluationService>();
builder.Services.AddScoped<IAiRoadmapService, FakeAiRoadmapService>();

// 4. Register Business Services (BLL)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<ICaseStudyService, CaseStudyService>();
builder.Services.AddScoped<IMentorService, MentorService>();
builder.Services.AddScoped<IGameBankService, GameBankService>();

// 5. HttpClient for future AI integration
builder.Services.AddHttpClient("AiApi", client =>
{
    var baseUrl = builder.Configuration["AiApi:BaseUrl"];
    client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? "https://router.huggingface.co/" : baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("AiApi:TimeoutSeconds", 45));
});

var app = builder.Build();

// Behind Render's reverse proxy TLS terminates at the edge, so trust the
// X-Forwarded-Proto/For headers — otherwise the app thinks requests are HTTP,
// which breaks Secure cookies and the Blazor Server WebSocket. Must run before
// authentication. KnownNetworks/Proxies are cleared because the proxy hop isn't
// on a known private subnet in Render's network.
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
else
{
    // In the Render container TLS is handled by the proxy; redirecting inside the
    // container just breaks. Only redirect to HTTPS during local development.
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout + external-login endpoints backing the static Account UI.
app.MapAdditionalIdentityEndpoints();

// Apply migrations and seed the demo account at startup.
await DbInitializer.SeedAsync(app.Services);

app.Run();

// Converts a postgres://user:password@host:port/database URI (the format Render
// exposes via DATABASE_URL) into an Npgsql keyword connection string, enabling
// SSL which managed Postgres requires.
static string BuildNpgsqlConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var database = uri.AbsolutePath.TrimStart('/');
    var port = uri.Port > 0 ? uri.Port : 5432;

    return $"Host={uri.Host};Port={port};Database={database};Username={username};" +
           $"Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
}
