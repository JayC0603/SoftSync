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

namespace SoftSync.Presentation.Configuration;

public static class ApplicationServices
{
    public static void AddSoftSyncUi(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(options =>
            {
                options.DetailedErrors = builder.Environment.IsDevelopment();
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
            })
            .AddHubOptions(options =>
            {
                // Allow brief mobile network stalls without discarding a healthy circuit.
                // Keep these intervals aligned with blazor-start.js.
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.HandshakeTimeout = TimeSpan.FromSeconds(30);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });

        // Needed so App.razor and scoped UI services can read browser preferences during
        // SSR / enhanced navigation and render the first frame without flashing.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            // Compress static text assets; leave authenticated HTML outside compression.
            options.MimeTypes = ["text/css", "text/javascript", "application/javascript", "image/svg+xml"];
        });

        var frontendOrigins = (builder.Configuration["FRONTEND_ORIGINS"] ?? "http://localhost:3000")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy.WithOrigins(frontendOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        // UI localization (EN/VI). Seed each circuit from the cookie so SSR and the
        // interactive render always use the same language.
        builder.Services.AddScoped<ViewportState>(services => new(
            services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request.Cookies["ss-viewport"] == "desktop"));
        builder.Services.AddScoped<LocalizationService>(services =>
        {
            var request = services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
            return new LocalizationService(LocalizationService.Parse(request?.Cookies["ss-lang"]));
        });
        builder.Services.AddScoped<UserProfileState>();
        builder.Services.AddSingleton<NotificationChanges>();
        builder.Services.AddScoped<AnnouncementService>();
        builder.Services.AddScoped<NotificationInbox>();
        builder.Services.AddScoped<MentorRequestWorkflow>();
        builder.Services.AddScoped<MentorRequests>();
        builder.Services.AddScoped<CourseUploadService>();
        builder.Services.AddHttpClient("CourseBlob", client => client.Timeout = TimeSpan.FromMinutes(10));
        builder.Services.Configure<CvIntegrationOptions>(builder.Configuration.GetSection("CvIntegration"));
        builder.Services.AddHttpClient<CvIntegrationService>(client => client.Timeout = TimeSpan.FromSeconds(20));
    }

    public static void AddSoftSyncApplicationServices(this WebApplicationBuilder builder)
    {
        // 2. Register Repositories (DAL)
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISkillRepository, SkillRepository>();
        builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        builder.Services.AddScoped<IRoadmapRepository, RoadmapRepository>();
        builder.Services.AddScoped<IRoleplayRepository, RoleplayRepository>();
        builder.Services.AddScoped<ICourseRepository, CourseRepository>();
        builder.Services.AddScoped<ICaseStudyRepository, CaseStudyRepository>();
        builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
        builder.Services.AddScoped<IChatRepository, ChatRepository>();
        builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        builder.Services.AddScoped<IMentorRepository, MentorRepository>();
        builder.Services.AddScoped<IMentorSupportRequestRepository, MentorSupportRequestRepository>();

        // 3. Register AI Services (BLL - Mocked)
        builder.Services.AddScoped<IAiAssessmentService, HuggingFaceAiAssessmentService>();
        builder.Services.AddSingleton(_ => AssistantKnowledgeBase.Load(
            Path.Combine(builder.Environment.ContentRootPath, "Assets", "AiKnowledge", "softsync-assistant.vi-en.json")));
        builder.Services.AddScoped<KnowledgeBasedAiAssistantService>();
        builder.Services.AddScoped<IAiAssistantService, LlmAiAssistantService>();
        builder.Services.AddScoped<IAiTutorService, AiTutorService>();
        builder.Services.AddSingleton<PdfDocumentKnowledge>();
        builder.Services.AddScoped<HuggingFaceJsonClient>();
        builder.Services.AddScoped<SoftSync.Presentation.Services.CvReviewService>();
        builder.Services.AddScoped<CvWorkspaceService>();
        builder.Services.AddScoped<IAiSecretProtector, AiSecretProtector>();
        builder.Services.AddScoped<IAiAdminAccess, IdentityAiAdminAccess>();
        builder.Services.AddScoped<IAiProviderConfigurationResolver, AiProviderConfigurationResolver>();
        builder.Services.AddScoped<IAiProviderConnectionTester, AiProviderConnectionTester>();
        builder.Services.AddScoped<IAiProviderManagementService, AiProviderManagementService>();
        builder.Services.AddScoped<AiProviderService>();
        builder.Services.AddSingleton<SystemTelemetry>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<SystemTelemetry>());
        builder.Services.AddScoped<AdminAnalyticsService>();
        builder.Services.AddScoped<LearningActivityService>();
        builder.Services.AddScoped<IAiLearningEvaluationService, AiLearningEvaluationService>();
        builder.Services.AddScoped<IAiQuizService, AiQuizService>();
        builder.Services.AddScoped<IRoleplayAiService, HuggingFaceRoleplayAiService>();
        builder.Services.AddScoped<IAiRoadmapService, FakeAiRoadmapService>();

        // 4. Register Business Services (BLL)
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ISkillService, SkillService>();
        builder.Services.AddScoped<IAssessmentService, AssessmentService>();
        builder.Services.AddScoped<IRoadmapService, RoadmapService>();
        builder.Services.AddScoped<IRoleplayService, RoleplayService>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<IChallengeService, ChallengeService>();
        builder.Services.AddScoped<IProgressService, ProgressService>();
        builder.Services.AddScoped<IAccessibilityService, AccessibilityService>();
        builder.Services.AddScoped<ILearningJourneyService, LearningJourneyService>();
        builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
        builder.Services.AddScoped<ICaseStudyService, CaseStudyService>();
        builder.Services.AddScoped<IMentorService, MentorService>();
        builder.Services.AddScoped<IGameBankService, GameBankService>();
        builder.Services.AddScoped<IdentityRoleManagementService>();
        builder.Services.AddScoped<LearnerAccessState>();

        // 5. HttpClient for future AI integration
        // Credentials are per-request headers, never defaults/URL. Reject redirects to other hosts.
        builder.Services.AddHttpClient("AiRuntime", client => client.Timeout = Timeout.InfiniteTimeSpan)
            .RemoveAllLoggers()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        builder.Services.AddHttpClient("AiApi", client =>
        {
            AiApiConfiguration.Configure(client, builder.Configuration);
        });
        builder.Services.AddHttpClient("Gemini", client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(45);
        });

        // CV/JD matching runs as an internal Python service. Authentication remains in
        // SoftSync; the AI service receives only the files needed for one inference.
        builder.Services.AddHttpClient<RecruitmentAiClient>(client =>
        {
            var baseUrl = builder.Configuration["RecruitmentAi:BaseUrl"];
            client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? "http://127.0.0.1:8001/" : baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("RecruitmentAi:TimeoutSeconds", 600));
        });
    }
}
