# Giao diện người học SoftSync

Áp dụng skill `clear-ui-design` và `frontend-design` (đã cài tại `C:/Users/phing/.codex/skills/frontend-design`). Người dùng giao quyền quyết định thiết kế; ưu tiên nội dung học tập và nền riêng cho từng mục.

## Phạm vi hiện tại

Dashboard, lộ trình, tiến độ, trợ lý SYNCY và Smart CV. Lớp giao diện nằm trong `SoftSync.Presentation/wwwroot/css/learner-studio.css`, được nạp sau `desktop-workspace.css`.

- Chỉ áp dụng từ 901px; cả stylesheet và thẻ link đều có media guard.
- Bài học và AI: tím SoftSync. Lộ trình và tiến độ: xanh ngọc. CV và tài nguyên: xanh lam. Mục tiêu và thành tích: vàng nhạt.
- Giữ mascot, hình minh họa sẵn có, dữ liệu thật, luồng thao tác và song ngữ.
- Nền trang có màu nhẹ; biểu đồ, nội dung và form giữ bề mặt dễ đọc. Dark mode có bảng màu nền riêng.
- Không thêm thư viện giao diện hay font ngoài.

## Kiểm tra ngày 22/10/2026

- Build Presentation thành công, 0 lỗi; còn 14 cảnh báo từ dependency và mã hiện có.
- Đăng nhập tài khoản demo người học và mở dashboard, roadmap, progress, assistant, cv trên Chromium 1366px: không tràn ngang.
- Chụp và xem dashboard chế độ tối.
- 390px: không tràn ngang; computed style phần chào mừng không đổi khi gỡ stylesheet mới.
- Ảnh kiểm tra nằm trong `docs/screenshots/learner-studio/`.

Đây là lớp hoàn thiện giao diện desktop trên các trang đang có, không thay đổi cách tính tiến độ hay chức năng AI/CV.
