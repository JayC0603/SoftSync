using System.Text.Json;

namespace SoftSync.Presentation.Services;

// Static SSR still handles account creation; language changes only update labels.
public static class RegisterLanguageContent
{
    private static readonly string[] Keys =
    [
        "auth.register.title", "auth.register.submit", "auth.haveAccount",
        "profile.fullName", "auth.email"
    ];
    private static readonly string[][] Phrases =
    [
        ["Tạo tài khoản để bắt đầu hành trình cùng SoftSync.", "Create an account to start your journey with SoftSync."],
        ["Nhập họ và tên", "Enter your full name"],
        ["Tạo mật khẩu", "Create a password"],
        ["Xác nhận mật khẩu", "Confirm password"],
        ["Nhập lại mật khẩu", "Confirm your password"],
        ["Tôi đồng ý với", "I agree to the"],
        ["Điều khoản", "Terms"],
        ["Chính sách bảo mật", "Privacy policy"],
        ["và", "and"],
        ["Điều khoản và Chính sách bảo mật", "Terms and Privacy policy"],
        ["Nội dung chính thức đang được cập nhật.", "The official content is being updated."],
        ["Đóng", "Close"],
        ["Vui lòng nhập họ tên.", "Please enter your full name."],
        ["Vui lòng nhập email.", "Please enter your email."],
        ["Email không hợp lệ.", "Invalid email address."],
        ["Số điện thoại không hợp lệ.", "Invalid phone number."],
        ["Mật khẩu phải từ 6 đến 100 ký tự.", "Password must be between 6 and 100 characters."],
        ["Mật khẩu nhập lại không khớp.", "The passwords do not match."],
        ["Vui lòng đồng ý với Điều khoản và Chính sách bảo mật / Please accept the Terms and Privacy policy.", "Please accept the Terms and Privacy policy."]
    ];
    public static string Json { get; } = JsonSerializer.Serialize(LoginLanguageContent.Pairs
        .Concat(Keys.Select(key => new[] { Translations.Get(key, AppLanguage.Vi), Translations.Get(key, AppLanguage.En) }))
        .Concat(Phrases));
}
