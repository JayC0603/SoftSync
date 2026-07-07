using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace SoftSync.Presentation.Components.Account;

/// <summary>
/// Helper for redirecting from static SSR Account pages. Throws
/// <see cref="NavigationException"/> internally (via NavigateTo forceLoad) so
/// callers don't accidentally continue rendering after a redirect. Adapted from
/// the .NET Blazor Web App Individual Accounts template.
/// </summary>
public sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    public const string StatusCookieName = "Identity.StatusMessage";

    [DoesNotReturn]
    public void RedirectTo(string? uri)
    {
        uri ??= "";

        // Prevent open redirects by keeping only the relative path.
        if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        {
            uri = navigationManager.ToBaseRelativePath(uri);
        }

        navigationManager.NavigateTo(uri);
        throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
    }

    [DoesNotReturn]
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
        var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
        RedirectTo(newUri);
    }

    [DoesNotReturn]
    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookieName, message, new CookieOptions
        {
            MaxAge = TimeSpan.FromSeconds(5),
            HttpOnly = true,
            IsEssential = true
        });
        RedirectTo(uri);
    }

    private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

    [DoesNotReturn]
    public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

    [DoesNotReturn]
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
        => RedirectToWithStatus(CurrentPath, message, context);
}
