using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Components.Account;

/// <summary>
/// Minimal API endpoints that back the static Account UI: logout and the
/// external (Google) login challenge. These must be plain HTTP endpoints, not
/// interactive components, so the auth cookie can be written. Adapted from the
/// Blazor Web App Individual Accounts template.
/// </summary>
public static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/Account");

        group.MapPost("/PerformExternalLogin", (
            HttpContext context,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromForm] string provider,
            [FromForm] string returnUrl) =>
        {
            IEnumerable<KeyValuePair<string, StringValues>> query =
            [
                new("ReturnUrl", returnUrl),
                new("Action", "ExternalLoginCallback")
            ];

            var redirectUrl = UriHelper.BuildRelative(
                context.Request.PathBase,
                "/Account/ExternalLogin",
                QueryString.Create(query));

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return TypedResults.Challenge(properties, [provider]);
        });

        group.MapPost("/Logout", async (
            ClaimsPrincipal user,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromForm] string returnUrl) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect($"~/{returnUrl}");
        });

        // Temporarily suspend the current account: lock it far into the future and
        // sign out. The user can be restored later by clearing LockoutEnd. Requires
        // an authenticated user; anonymous callers just bounce to login.
        group.MapPost("/Suspend", async (
            ClaimsPrincipal principal,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
                return (IResult)TypedResults.LocalRedirect("~/Account/Login");

            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect("~/Account/Login?suspended=1");
        });

        // Permanently delete the current account and sign out. Irreversible; the
        // Settings UI gates this behind a typed-confirmation modal.
        group.MapPost("/DeleteAccount", async (
            ClaimsPrincipal principal,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
                return (IResult)TypedResults.LocalRedirect("~/Account/Login");

            await signInManager.SignOutAsync();
            await userManager.DeleteAsync(user);
            return TypedResults.LocalRedirect("~/Account/Login?deleted=1");
        });

        return group;
    }
}
