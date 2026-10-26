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

public static class DatabaseConfiguration
{
    public static bool AddSoftSyncDatabase(this WebApplicationBuilder builder)
    {
        // 1. Database Configuration. Demo mode uses an ephemeral in-memory database so
        // reviewers can exercise the product locally without PostgreSQL credentials.
        var demoMode = builder.Configuration.GetValue<bool>("SoftSync:DemoMode");

        if (demoMode)
        {
            // Windows Event Log requires elevated privileges on some machines. Console
            // logging keeps the zero-setup demo profile usable for normal accounts.
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
        }

        // Prefer Vercel's DATABASE_URL so a stale ConnectionStrings__SoftSyncDb value
        // cannot override the managed database's current internal connection URL.
        // Local/other environments can still use ConnectionStrings:SoftSyncDb.
        // IConfiguration includes environment variables and Development User Secrets,
        // allowing the same DATABASE_URL key to work on Vercel and on a local machine.
        if (demoMode)
        {
            builder.Services.AddDbContext<SoftSyncDbContext>(options =>
                options.UseInMemoryDatabase("SoftSyncDemo"));
        }
        else
        {
            var connectionString = PostgresConnection.Resolve(builder.Configuration);
            builder.Services.AddDbContext<SoftSyncDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        // CV operations own short-lived contexts; Identity's existing scoped context is unchanged.
        builder.Services.AddScoped<IDbContextFactory<SoftSyncDbContext>, CvDbContextFactory>();
        var cvUploadOptions = new CvUploadOptions
        {
            MaxBytes = Math.Clamp(builder.Configuration.GetValue<long?>("CvReview:MaxUploadBytes") ?? 5 * 1024 * 1024, 1024, 20 * 1024 * 1024)
        };
        builder.Services.AddSingleton(cvUploadOptions);
        builder.Services.AddScoped<ICvTextExtractor, CvTextExtractor>();
        builder.Services.AddScoped<ICvAnalysisService, AiCvAnalysisService>();
        builder.Services.AddScoped<ICvAiReadiness, CvAiReadiness>();
        builder.Services.AddScoped<ICvReviewService>(services => new SoftSync.BLL.Services.CvReviewService(
            services.GetRequiredService<IDbContextFactory<SoftSyncDbContext>>(),
            services.GetRequiredService<ICvTextExtractor>(), services.GetRequiredService<ICvAnalysisService>(), cvUploadOptions.MaxBytes));
        builder.Services.AddScoped<ICurrentScheduleUser, CurrentScheduleUser>();
        builder.Services.AddScoped<IScheduleService, ScheduleService>();

        builder.Services.AddSingleton<EfDataProtectionKeyRepository>();
        var dataProtection = builder.Services
            .AddDataProtection()
            .SetApplicationName("SoftSync");
        // Optional external certificate protects the persisted key ring at rest as well.
        // Mount it outside source control; keep the same certificate across redeployments.
        var protectionCertificatePath = builder.Configuration["DataProtection:CertificatePath"];
        if (!string.IsNullOrWhiteSpace(protectionCertificatePath))
        {
            dataProtection.ProtectKeysWithCertificate(System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(
                protectionCertificatePath, builder.Configuration["DataProtection:CertificatePassword"]));
        }
        builder.Services.AddOptions<KeyManagementOptions>()
            .Configure<EfDataProtectionKeyRepository>((options, repository) =>
            {
                options.XmlRepository = repository;
            });


        return demoMode;
    }
}
