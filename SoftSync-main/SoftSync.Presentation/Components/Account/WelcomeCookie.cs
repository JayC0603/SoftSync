namespace SoftSync.Presentation.Components.Account;

/// <summary>
/// Writes a short-lived cookie right after a successful sign-in. The interactive
/// WelcomeToast component reads it once (via JS) to play a welcome animation,
/// then clears it — so the greeting shows exactly once per login.
/// </summary>
public static class WelcomeCookie
{
    public const string Name = "ss-welcome";

    public static void Set(HttpContext context, string? displayName, bool isFirstLogin = false)
    {
        var name = string.IsNullOrWhiteSpace(displayName) ? " " : displayName;
        var value = $"{(isFirstLogin ? "first" : "returning")}|{name}";
        context.Response.Cookies.Append(Name, value, new CookieOptions
        {
            MaxAge = TimeSpan.FromSeconds(15),
            HttpOnly = false, // read by client-side JS on the next page
            IsEssential = true,
            Path = "/"
        });
    }
}
