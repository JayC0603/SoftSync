using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.Presentation.Components.Account;
using SoftSync.Presentation.Components.Account.Pages;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class GoogleAccountLinkTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Verified_Gmail_uses_existing_user_and_real_Identity_cookie_without_password(bool hasPassword)
    {
        await using var app = new App();
        var original = await app.CreateUser("owner@gmail.com", hasPassword);
        var passwordHash = original.PasswordHash;
        await using var request = app.Request();
        var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByIdAsync(original.Id.ToString()))!;
        var result = await request.Services.GetRequiredService<GoogleAccountLinkService>().LinkAndSignInAsync(user, Info());
        Assert.True(result.Succeeded);
        Assert.Single(await users.GetLoginsAsync(user));
        var saved = Assert.Single(await request.Services.GetRequiredService<SoftSyncDbContext>().Users.ToListAsync());
        Assert.Equal(original.Id, saved.Id);
        Assert.Equal("Existing learner", saved.FullName);
        Assert.Equal("/existing-avatar.webp", saved.AvatarUrl);
        Assert.Equal(passwordHash, saved.PasswordHash);
        Assert.Equal(new[] { "User" }, await users.GetRolesAsync(user));
        await app.AssertSignedIn(request.Context, original.Id);
    }

    [Theory]
    [InlineData("unverified")]
    [InlineData("missing-verification")]
    [InlineData("other-provider")]
    [InlineData("unauthenticated")]
    [InlineData("wrong-subject")]
    [InlineData("different-email")]
    [InlineData("third-party-mailbox")]
    [InlineData("lookalike-gmail")]
    public async Task Untrusted_or_mismatched_identity_does_not_link_or_issue_cookie(string scenario)
    {
        await using var app = new App();
        var email = scenario == "third-party-mailbox" ? "owner@example.com" : scenario == "lookalike-gmail" ? "owner@evilgmail.com" : "owner@gmail.com";
        var original = await app.CreateUser(email);
        var info = Info(email: scenario == "different-email" ? "someone-else@gmail.com" : email,
            verified: scenario == "unverified" ? "false" : scenario == "missing-verification" ? null : "true",
            provider: scenario == "other-provider" ? "Other" : "Google", authenticated: scenario != "unauthenticated");
        if (scenario == "wrong-subject") info.ProviderKey = "not-the-claimed-subject";
        await using var request = app.Request();
        var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByIdAsync(original.Id.ToString()))!;
        var result = await request.Services.GetRequiredService<GoogleAccountLinkService>().LinkAndSignInAsync(user, info);
        Assert.False(result.Succeeded);
        Assert.Empty(await users.GetLoginsAsync(user));
        Assert.False(request.Context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task Workspace_authority_claim_allows_existing_email_to_sign_in()
    {
        await using var app = new App();
        var original = await app.CreateUser("owner@school.example");
        await using var request = app.Request();
        var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByIdAsync(original.Id.ToString()))!;
        var result = await request.Services.GetRequiredService<GoogleAccountLinkService>()
            .LinkAndSignInAsync(user, Info(email: user.Email!, hostedDomain: "school.example"));
        Assert.True(result.Succeeded);
        await app.AssertSignedIn(request.Context, original.Id);
    }

    [Theory]
    [InlineData("locked")]
    [InlineData("two-factor")]
    [InlineData("unconfirmed")]
    public async Task Local_account_restrictions_are_checked_before_linking(string restriction)
    {
        await using var app = new App(requireConfirmedEmail: restriction == "unconfirmed");
        var original = await app.CreateUser("owner@gmail.com");
        await using var request = app.Request();
        var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByIdAsync(original.Id.ToString()))!;
        if (restriction == "locked") Assert.True((await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(1))).Succeeded);
        if (restriction == "two-factor") Assert.True((await users.SetTwoFactorEnabledAsync(user, true)).Succeeded);
        var result = await request.Services.GetRequiredService<GoogleAccountLinkService>().LinkAndSignInAsync(user, Info());
        Assert.True(restriction == "locked" ? result.IsLockedOut : restriction == "two-factor" ? result.RequiresTwoFactor : result.IsNotAllowed);
        Assert.Empty(await users.GetLoginsAsync(user));
        Assert.False(request.Context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Existing_provider_ownership_is_preserved(bool belongsToAnotherUser)
    {
        await using var app = new App();
        var original = await app.CreateUser("owner@gmail.com");
        var other = belongsToAnotherUser ? await app.CreateUser("other@gmail.com") : original;
        await using var request = app.Request();
        var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var owner = (await users.FindByIdAsync(other.Id.ToString()))!;
        Assert.True((await users.AddLoginAsync(owner, Info(key: belongsToAnotherUser ? "google-subject" : "older-google-subject"))).Succeeded);
        var user = (await users.FindByIdAsync(original.Id.ToString()))!;
        var result = await request.Services.GetRequiredService<GoogleAccountLinkService>().LinkAndSignInAsync(user, Info());
        Assert.False(result.Succeeded);
        Assert.Single(await request.Services.GetRequiredService<SoftSyncDbContext>().UserLogins.ToListAsync());
        Assert.False(request.Context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task Returning_same_Google_subject_is_idempotent()
    {
        await using var app = new App();
        var original = await app.CreateUser("owner@gmail.com");
        for (var visit = 0; visit < 2; visit++)
        {
            await using var request = app.Request();
            var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByIdAsync(original.Id.ToString()))!;
            Assert.True((await request.Services.GetRequiredService<GoogleAccountLinkService>().LinkAndSignInAsync(user, Info())).Succeeded);
            Assert.Single(await users.GetLoginsAsync(user));
            await app.AssertSignedIn(request.Context, original.Id);
        }
    }

    [Theory]
    [InlineData("login", true)]
    [InlineData("register", true)]
    [InlineData("login", false)]
    [InlineData("register", false)]
    public async Task Actual_callback_reads_protected_external_cookie_and_redirects_without_confirmation(string intent, bool hasPassword)
    {
        await using var app = new App();
        var original = await app.CreateUser("owner@gmail.com", hasPassword);
        string cookie;
        await using (var challenge = app.Request())
        {
            var properties = new AuthenticationProperties();
            properties.Items["LoginProvider"] = "Google";
            properties.Items["SoftSync.AuthIntent"] = intent;
            await challenge.Context.SignInAsync(IdentityConstants.ExternalScheme, Info().Principal, properties);
            cookie = Cookies(challenge.Context);
        }
        await using var callback = app.Request(cookie);
        await using var renderer = new HtmlRenderer(callback.Services, callback.Services.GetRequiredService<ILoggerFactory>());
        await Assert.ThrowsAsync<NavigationException>(() => renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.RenderComponentAsync<CascadingValue<HttpContext>>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["Value"] = callback.Context,
                ["ChildContent"] = (RenderFragment)(builder => { builder.OpenComponent<ExternalLogin>(0); builder.CloseComponent(); })
            }));
        }));
        Assert.Equal("https://softsync.test/dashboard", app.Navigation.Destination);
        Assert.Single(await callback.Services.GetRequiredService<SoftSyncDbContext>().Users.ToListAsync());
        await app.AssertSignedIn(callback.Context, original.Id);
    }

    [Fact]
    public async Task Forged_external_cookie_cannot_link_or_sign_in()
    {
        await using var app = new App();
        await app.CreateUser("owner@gmail.com");
        await using var callback = app.Request(".AspNetCore.Identity.External=forged-cookie");
        await using var renderer = new HtmlRenderer(callback.Services, callback.Services.GetRequiredService<ILoggerFactory>());
        await Assert.ThrowsAsync<NavigationException>(() => RenderCallback(renderer, callback.Context));
        Assert.Equal("https://softsync.test/Account/Login", app.Navigation.Destination);
        Assert.Empty(await callback.Services.GetRequiredService<SoftSyncDbContext>().UserLogins.ToListAsync());
        Assert.DoesNotContain(".AspNetCore.Identity.Application=", Cookies(callback.Context));
    }

    [Theory]
    [InlineData("login")]
    [InlineData("register")]
    public async Task Unknown_email_still_requires_registration_intent(string intent)
    {
        await using var app = new App();
        await app.CreateUser("seed@gmail.com"); // Includes the existing User role.
        string cookie;
        await using (var challenge = app.Request())
        {
            var properties = new AuthenticationProperties();
            properties.Items["LoginProvider"] = "Google";
            properties.Items["SoftSync.AuthIntent"] = intent;
            await challenge.Context.SignInAsync(IdentityConstants.ExternalScheme, Info().Principal, properties);
            cookie = Cookies(challenge.Context);
        }
        await using var callback = app.Request(cookie);
        await using var renderer = new HtmlRenderer(callback.Services, callback.Services.GetRequiredService<ILoggerFactory>());
        if (intent == "login")
        {
            var html = await RenderCallback(renderer, callback.Context);
            Assert.Contains("registration-welcome", html);
            Assert.Null(app.Navigation.Destination);
            Assert.Single(await callback.Services.GetRequiredService<SoftSyncDbContext>().Users.ToListAsync());
            Assert.Empty(await callback.Services.GetRequiredService<SoftSyncDbContext>().UserLogins.ToListAsync());
            Assert.DoesNotContain(".AspNetCore.Identity.Application=", Cookies(callback.Context));
        }
        else
        {
            await Assert.ThrowsAsync<NavigationException>(() => RenderCallback(renderer, callback.Context));
            Assert.Equal("https://softsync.test/profile-setup", app.Navigation.Destination);
            var users = callback.Services.GetRequiredService<UserManager<ApplicationUser>>();
            var created = (await users.FindByEmailAsync("owner@gmail.com"))!;
            Assert.NotNull(created);
            Assert.Single(await users.GetLoginsAsync(created));
            Assert.False(await users.HasPasswordAsync(created));
            Assert.Equal(2, await callback.Services.GetRequiredService<SoftSyncDbContext>().Users.CountAsync());
            await app.AssertSignedIn(callback.Context, created.Id);
        }
    }

    [Fact]
    public async Task Concurrent_provider_claim_returns_failure_without_issuing_auth_cookie()
    {
        await using var app = new App(interceptor: new LoginConflict());
        var original = await app.CreateUser("owner@gmail.com");
        await using var request = app.Request();
        var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByIdAsync(original.Id.ToString()))!;
        var result = await request.Services.GetRequiredService<GoogleAccountLinkService>().LinkAndSignInAsync(user, Info());
        Assert.False(result.Succeeded);
        Assert.False(request.Context.Response.Headers.ContainsKey("Set-Cookie"));
        Assert.Empty(await request.Services.GetRequiredService<SoftSyncDbContext>().UserLogins.ToListAsync());
    }

    private static Task<string> RenderCallback(HtmlRenderer renderer, HttpContext context) => renderer.Dispatcher.InvokeAsync(async () =>
    {
        var output = await renderer.RenderComponentAsync<CascadingValue<HttpContext>>(ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            ["Value"] = context,
            ["ChildContent"] = (RenderFragment)(builder => { builder.OpenComponent<ExternalLogin>(0); builder.CloseComponent(); })
        }));
        return output.ToHtmlString();
    });

    private static ExternalLoginInfo Info(string email = "owner@gmail.com", string? verified = "true", string provider = "Google", bool authenticated = true,
        string key = "google-subject", string? hostedDomain = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, key), new(ClaimTypes.Email, email), new(ClaimTypes.Name, "Google display name") };
        if (verified is not null) claims.Add(new("urn:google:email_verified", verified));
        if (hostedDomain is not null) claims.Add(new("urn:google:hosted_domain", hostedDomain));
        return new(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? provider : null)), provider, key, provider);
    }

    private static string Cookies(HttpContext context) => string.Join("; ", context.Response.Headers.SetCookie
        .Select(value => value!.Split(';')[0]).Where(value => !value.EndsWith('=')));

    private sealed class App : IAsyncDisposable
    {
        private readonly ServiceProvider provider;
        public TestNavigation Navigation { get; } = new();
        public App(bool requireConfirmedEmail = false, SaveChangesInterceptor? interceptor = null)
        {
            var database = Guid.NewGuid().ToString();
            var services = new ServiceCollection().AddLogging().AddHttpContextAccessor();
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
            services.AddDbContext<SoftSyncDbContext>(options =>
            {
                options.UseInMemoryDatabase(database);
                if (interceptor is not null) options.AddInterceptors(interceptor);
            });
            services.AddAuthentication(options => { options.DefaultScheme = IdentityConstants.ApplicationScheme; options.DefaultSignInScheme = IdentityConstants.ExternalScheme; })
                .AddIdentityCookies();
            services.AddIdentityCore<ApplicationUser>(options => { options.User.RequireUniqueEmail = true; options.SignIn.RequireConfirmedEmail = requireConfirmedEmail; })
                .AddRoles<IdentityRole<int>>().AddEntityFrameworkStores<SoftSyncDbContext>().AddSignInManager().AddDefaultTokenProviders();
            services.AddScoped<GoogleAccountLinkService>();
            services.AddScoped<LocalizationService>();
            services.AddSingleton<NavigationManager>(Navigation);
            services.AddScoped<IdentityRedirectManager>();
            provider = services.BuildServiceProvider();
        }
        public RequestScope Request(string? cookie = null) => new(provider.CreateAsyncScope(), cookie);
        public async Task<ApplicationUser> CreateUser(string email, bool hasPassword = true)
        {
            await using var request = Request();
            var users = request.Services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { Email = email, UserName = email, FullName = "Existing learner", AvatarUrl = "/existing-avatar.webp" };
            Assert.True((hasPassword ? await users.CreateAsync(user, "Existing@123") : await users.CreateAsync(user)).Succeeded);
            var roles = request.Services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            if (!await roles.RoleExistsAsync("User")) Assert.True((await roles.CreateAsync(new("User"))).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, "User")).Succeeded);
            return user;
        }
        public async Task AssertSignedIn(HttpContext response, int expectedId)
        {
            await using var authenticated = Request(Cookies(response));
            var ticket = await authenticated.Context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            Assert.True(ticket.Succeeded);
            Assert.Equal(expectedId.ToString(), ticket.Principal!.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.True(ticket.Principal.IsInRole("User"));
            Assert.True(ticket.Properties!.IsPersistent);
        }
        public ValueTask DisposeAsync() => provider.DisposeAsync();
    }

    private sealed class RequestScope : IAsyncDisposable
    {
        private readonly AsyncServiceScope scope;
        public IServiceProvider Services => scope.ServiceProvider;
        public DefaultHttpContext Context { get; }
        public RequestScope(AsyncServiceScope scope, string? cookie)
        {
            this.scope = scope;
            Context = new() { RequestServices = Services };
            Context.Request.Scheme = "https";
            Context.Request.Host = new("softsync.test");
            if (cookie is not null) Context.Request.Headers.Cookie = cookie;
            Services.GetRequiredService<IHttpContextAccessor>().HttpContext = Context;
        }
        public ValueTask DisposeAsync() => scope.DisposeAsync();
    }

    private sealed class TestNavigation : NavigationManager
    {
        public string? Destination { get; private set; }
        public TestNavigation() => Initialize("https://softsync.test/", "https://softsync.test/Account/ExternalLogin");
        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Destination = ToAbsoluteUri(uri).AbsoluteUri;
            throw new NavigationException(Destination);
        }
    }

    private sealed class LoginConflict : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<IdentityUserLogin<int>>().Any(entry => entry.State == EntityState.Added))
                throw new DbUpdateException("Simulated competing callback", new PostgresException("duplicate login", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation,
                    tableName: "AspNetUserLogins", constraintName: "PK_AspNetUserLogins"));
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
