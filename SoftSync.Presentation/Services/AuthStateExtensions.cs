using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SoftSync.Presentation.Services;

/// <summary>Helpers for reading the current user's id from the auth state in
/// interactive components (which can't touch HttpContext).</summary>
public static class AuthStateExtensions
{
    /// <summary>Returns the authenticated user's integer id, or 0 if not signed in.</summary>
    public static async Task<int> GetUserIdAsync(this AuthenticationStateProvider provider)
    {
        var state = await provider.GetAuthenticationStateAsync();
        return GetUserId(state.User);
    }

    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }
}
