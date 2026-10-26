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

public static class IdentityConfiguration
{
    public static bool AddSoftSyncIdentity(this WebApplicationBuilder builder)
    {
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
        var googleClientId = !string.IsNullOrWhiteSpace(googleSection["ClientId"])
            ? googleSection["ClientId"]
            : builder.Configuration["GOOGLE_CLIENT_ID"];
        var googleClientSecret = !string.IsNullOrWhiteSpace(googleSection["ClientSecret"])
            ? googleSection["ClientSecret"]
            : builder.Configuration["GOOGLE_CLIENT_SECRET"];
        var googleAuthConfigured = !string.IsNullOrWhiteSpace(googleClientId)
            && !string.IsNullOrWhiteSpace(googleClientSecret);
        if (googleAuthConfigured)
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                // Keep the callback explicit so local, Docker and production Google
                // Cloud registrations all use the same predictable endpoint.
                options.CallbackPath = "/signin-google";
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture");
                options.ClaimActions.MapJsonKey("urn:google:email_verified", "email_verified");
                options.Events.OnRemoteFailure = context =>
                {
                    // Cancellation and provider failures should return to a usable login page.
                    context.HandleResponse();
                    var savedReturnUrl = context.Properties is { } properties && properties.Items.TryGetValue("SoftSync.ReturnUrl", out var target) ? target : null;
                    var returnTo = LocalReturnUrl.Normalize(savedReturnUrl);
                    context.Response.Redirect("/Account/Login?googleError=1&ReturnUrl=" + Uri.EscapeDataString(returnTo));
                    return Task.CompletedTask;
                };
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

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(CourseAuthorization.UserPolicy, policy =>
                policy.RequireAuthenticatedUser())
            .AddPolicy(CourseAuthorization.TeacherPolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole(
                    CourseAuthorization.TeacherRole,
                    CourseAuthorization.AdminRole))
            .AddPolicy(CourseAuthorization.AdminPolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole(CourseAuthorization.AdminRole));

        // A role change updates the user's security stamp, invalidating old role claims.
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.FromMinutes(1));

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "SoftSync.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        // Bind sender config + register senders and the OTP service.
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
        builder.Services.Configure<TwilioOptions>(builder.Configuration.GetSection("Twilio"));
        builder.Services.AddScoped<IAppEmailSender, MailKitEmailSender>();
        builder.Services.AddScoped<IAppSmsSender, TwilioSmsSender>();
        builder.Services.AddScoped<VerificationCodeService>();


        return googleAuthConfigured;
    }
}
