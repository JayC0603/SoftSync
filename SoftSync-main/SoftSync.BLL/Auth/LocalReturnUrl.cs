namespace SoftSync.BLL.Auth;

public static class LocalReturnUrl
{
    public static string Normalize(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return "/dashboard";
        var value = returnUrl.Trim();
        var decoded = Uri.UnescapeDataString(value);
        if (Unsafe(value) || Unsafe(decoded) || value == "/") return "/dashboard";
        return value.StartsWith('/') ? value : "/" + value;
    }

    private static bool Unsafe(string value) => value.StartsWith("//") || value.Contains('\\')
        || value.Any(char.IsControl) || Uri.IsWellFormedUriString(value, UriKind.Absolute);
}
