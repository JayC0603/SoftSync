# Google: dùng tài khoản SoftSync hiện có

Khi chọn Google ở Đăng nhập hoặc Đăng ký, nếu email đã có tài khoản SoftSync và
Google xác nhận quyền sở hữu Gmail/Workspace, callback tự liên kết Google và đăng
nhập vào đúng user hiện có. Không hiện bước nhập lại mật khẩu SoftSync, không tạo
user mới, không chuyển người dùng cũ sang profile setup. User ID, hồ sơ, mật khẩu,
vai trò và dữ liệu học tập được giữ nguyên.

`GoogleAccountLinkService` nhận thông tin từ `GetExternalLoginInfoAsync`, tức cookie
external được máy chủ bảo vệ sau OAuth. Kiểm tra provider, subject, trạng thái xác
thực, email đã xác minh và email khớp theo Identity normalizer. Bổ sung mapping
`hd` từ Google userinfo cho Workspace.

Gmail đã xác minh, hoặc Workspace có `email_verified=true` và `hd`, đáp ứng điều
kiện Google quản lý email. Email bên thứ ba không có `hd` vẫn giữ bước xác nhận quyền sở hữu
tài khoản cũ theo [hướng dẫn Google](https://developers.google.com/identity/sign-in/web/backend-auth).
Không liên kết chỉ vì hai chuỗi email trùng nhau.

Tài khoản bị khóa, bị hạn chế đăng nhập hoặc bật xác thực hai bước không được bỏ
qua kiểm tra. Không ghi đè Google subject đã liên kết; không nhận provider key
đang thuộc user khác. Đăng nhập dùng `ExternalLoginSignInAsync` với
`bypassTwoFactor: false`. Xung đột unique key của login trả về lỗi đăng nhập thay
vì cấp cookie từ lần ghi thất bại.

Kiểm tra: build .NET/Vite và 48 test đạt (25 test Google mới, 23 test đăng ký/bản
dịch hiện có). Bao gồm callback Razor thật đọc cookie external được ký, application
cookie được Identity cấp và xác thực lại, cả intent login/register, tài khoản cũ
có/không có mật khẩu, dữ liệu user/role, khóa/2FA/email chưa xác nhận, provider
ownership, cookie giả và xung đột insert mô phỏng. Tài khoản Google mới vẫn cần
intent đăng ký; đăng nhập bằng email chưa đăng ký không tự tạo user.

Các test dùng database trong bộ nhớ và Google principal mô phỏng. Chưa thực hiện
OAuth với tài khoản Google thật hoặc kiểm tra sau deploy. Không đổi database
Production, không gọi chatbot/CV/OCR.
