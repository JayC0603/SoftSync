using System.Text.Json;

namespace SoftSync.Presentation.Services;

// Send both languages with the static login form so switching language never
// needs another request or replaces inputs (including password-manager values).
public static class LoginLanguageContent
{
    private static readonly string[] Keys =
    [
        "auth.login.title", "auth.login.subtitle", "auth.identifier", "auth.password",
        "auth.rememberMe", "auth.forgot", "auth.noAccount", "auth.or", "auth.backHome",
        "auth.googleNotConfigured", "auth.googleCancelled", "auth.error.invalidLogin",
        "auth.error.lockedOut", "settings.suspend.done", "settings.delete.done",
        "redesign.passwordToggle", "layout.languageMenu"
    ];

    // Literal labels used by the login layout and its shared footer.
    private static readonly string[][] Phrases =
    [
        ["you@example.com hoặc +84", "you@example.com or +84"],
        ["Nhập mật khẩu của bạn", "Enter your password"],
        ["Tiếp tục với Google", "Continue with Google"],
        ["Đăng ký", "Sign up"],
        ["Đổi giao diện sáng / tối", "Toggle light / dark theme"],
        ["SoftSync — Trang chủ", "SoftSync — Home"],
        ["Logo nhóm SoftSync", "SoftSync team logo"],
        ["Vui lòng nhập email hoặc số điện thoại.", "Please enter your email or phone number."],
        ["Vui lòng nhập mật khẩu.", "Please enter your password."],
        ["Học kỹ năng mềm cùng AI.", "Learn soft skills with AI."],
        ["Kết nối SoftSync", "Connect with SoftSync"],
        ["YouTube — sẽ bổ sung liên kết", "YouTube — link coming soon"],
        ["Email — sẽ bổ sung địa chỉ", "Email — address coming soon"],
        ["Thông tin SoftSync", "About SoftSync"],
        ["Về chúng tôi", "About us"],
        ["Cộng đồng", "Community"],
        ["Tài nguyên", "Resources"],
        ["Hỗ trợ và pháp lý", "Support and legal"],
        ["Trợ giúp", "Help"],
        ["Chính sách bảo mật", "Privacy"],
        ["Điều khoản sử dụng", "Terms"],
        ["Liên hệ", "Contact"],
        ["Phiên bản", "Version"]
    ];

    // Default JSON escaping keeps the embedded non-executable script safe.
    internal static IEnumerable<string[]> Pairs =>
        Keys.Select(key => new[] { Translations.Get(key, AppLanguage.Vi), Translations.Get(key, AppLanguage.En) })
            .Concat(Phrases);
    public static string Json { get; } = JsonSerializer.Serialize(Pairs);
}
