using SoftSync.Presentation.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using SoftSync.BLL.Auth;
using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Services;
using SoftSync.BLL.Services.Fake;
using SoftSync.Presentation.Components.Account;
using SoftSync.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
        options.DetailedErrors = builder.Environment.IsDevelopment());

// UI localization (EN/VI). Scoped per circuit so each user has their own language.
builder.Services.AddScoped<LocalizationService>();

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("SoftSyncDb");
builder.Services.AddDbContext<SoftSyncDbContext>(options =>
    options.UseSqlServer(connectionString));

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
builder.Services.AddScoped<IAiAssessmentService, FakeAiAssessmentService>();
builder.Services.AddScoped<IAiAssistantService, FakeAiAssistantService>();
builder.Services.AddScoped<IAiRoadmapService, FakeAiRoadmapService>();

// 4. Register Business Services (BLL)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<ICaseStudyService, CaseStudyService>();
builder.Services.AddScoped<IMentorService, MentorService>();

// 5. HttpClient for future AI integration
builder.Services.AddHttpClient("AiApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AiApi:BaseUrl"] ?? "http://localhost:5000");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
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
