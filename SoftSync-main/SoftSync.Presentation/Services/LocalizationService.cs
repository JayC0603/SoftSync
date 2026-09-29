namespace SoftSync.Presentation.Services;

/// <summary>Supported UI languages.</summary>
public enum AppLanguage
{
    En,
    Vi
}

/// <summary>
/// Holds the current UI language and resolves translation keys. Scoped per
/// Blazor circuit (per user). Components subscribe to <see cref="OnChanged"/>
/// so a language switch re-renders the UI without a full page reload.
/// </summary>
public class LocalizationService
{
    public LocalizationService(AppLanguage initialLanguage = AppLanguage.Vi)
    {
        Current = initialLanguage;
    }

    public AppLanguage Current { get; private set; }

    /// <summary>Raised whenever the language changes.</summary>
    public event Action? OnChanged;

    /// <summary>Look up a translation key for the current language.</summary>
    /// <remarks>Falls back to the English value, then to the raw key if missing.</remarks>
    public string this[string key] => Translations.Get(key, Current);

    /// <summary>Convenience method identical to the indexer.</summary>
    public string T(string key) => Translations.Get(key, Current);
    public string Text(string? literal) => UiTextCatalog.Get(literal, Current);
    public System.Globalization.CultureInfo Culture => System.Globalization.CultureInfo.GetCultureInfo(Current == AppLanguage.Vi ? "vi-VN" : "en-US");
    public string IdentityError(string code) => Current == AppLanguage.En ? code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "An account with these details already exists.",
        "PasswordTooShort" => "The password is too short.",
        "PasswordRequiresNonAlphanumeric" => "The password requires a symbol.",
        "PasswordRequiresDigit" => "The password requires a digit.",
        "PasswordRequiresLower" => "The password requires a lowercase letter.",
        "PasswordRequiresUpper" => "The password requires an uppercase letter.",
        "PasswordRequiresUniqueChars" => "The password needs more distinct characters.",
        "PasswordMismatch" => "The password is incorrect.",
        "InvalidToken" => "The verification code is invalid or expired.",
        "InvalidEmail" => "Invalid email address.",
        _ => "The account operation could not be completed. Check your information and try again."
    } : code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Tài khoản với thông tin này đã tồn tại.",
        "PasswordTooShort" => "Mật khẩu quá ngắn.",
        "PasswordRequiresNonAlphanumeric" => "Mật khẩu cần có ký tự đặc biệt.",
        "PasswordRequiresDigit" => "Mật khẩu cần có chữ số.",
        "PasswordRequiresLower" => "Mật khẩu cần có chữ thường.",
        "PasswordRequiresUpper" => "Mật khẩu cần có chữ hoa.",
        "PasswordRequiresUniqueChars" => "Mật khẩu cần nhiều ký tự khác nhau hơn.",
        "PasswordMismatch" => "Mật khẩu không đúng.",
        "InvalidToken" => "Mã xác minh không hợp lệ hoặc đã hết hạn.",
        "InvalidEmail" => "Email không hợp lệ.",
        _ => "Không thể hoàn tất thao tác tài khoản. Kiểm tra thông tin và thử lại."
    };

    public void SetLanguage(AppLanguage language)
    {
        if (Current == language) return;
        Current = language;
        OnChanged?.Invoke();
    }

    public void Toggle() => SetLanguage(Current == AppLanguage.En ? AppLanguage.Vi : AppLanguage.En);

    /// <summary>Parse a stored culture string (e.g. from localStorage) into a language.</summary>
    public static AppLanguage Parse(string? value) =>
        string.Equals(value, "en", StringComparison.OrdinalIgnoreCase) ? AppLanguage.En : AppLanguage.Vi;

    /// <summary>The short code persisted to localStorage.</summary>
    public string Code => Current == AppLanguage.Vi ? "vi" : "en";
}
