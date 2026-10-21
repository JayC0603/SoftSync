# SoftSync — đối chiếu ảnh đã duyệt, 21/10/2026

Nguồn duy nhất: `design-input/strict-mobile-20261021/SoftSync_TrangChu_Mobile_Approved_Kit_20261021/reference/00_TRANG_CHU_DA_CHOT.png` trong workspace. Đã đọc toàn bộ PROMPT trước khi sửa. Người dùng xác nhận tiếp tục triển khai theo ảnh và giữ vị trí.

## Thay đổi của lượt này
- `SoftSync.Presentation/Components/Shared/StrictMobileHome.razor`: dùng hình người nữ SYNCY từ ảnh duyệt thay mascot.
- `SoftSync.Presentation/wwwroot/css/strict-mobile-home.css`: hình SYNCY bên trái; bottom navigation có lề và bo góc theo ảnh; chừa khoảng cách footer/nav.
- `SoftSync.Presentation/wwwroot/images/home-approved/`: bổ sung welcome_vietnam_mountain, quick_progress, quick_resources, quick_test, quick_community, syncy_support (WebP). Cắt trực tiếp từ ảnh duyệt, chỉ bỏ mép UI; không AI, không vẽ lại. Đây là crop có nền, không phải ảnh gốc tách lớp.
- Giữ nguyên các thay đổi có sẵn trong workspace, API, dữ liệu, logo, icon điều hướng, authentication và trang khác.

## Đối chiếu từng phần
| Phần | Kết quả / sai khác còn lại |
| --- | --- |
| Header | Giữ logo và thứ tự điều khiển có sẵn. Logo/avatar thật khác dữ liệu minh họa. |
| Search | Đúng vị trí; input và bộ lọc hoạt động. |
| Welcome | Chữ trái, phong cảnh phải; tên lấy account thật. Crop phong cảnh giới hạn độ phân giải. |
| Mục tiêu | Đúng vị trí, không icon trang trí; nút chi tiết bên phải. |
| Bài hiện tại | Chữ trái, học viên/laptop phải, CTA xanh phía dưới; tên bài thực khác mẫu. |
| Thống kê | 4 ô cùng hàng, màu theo mẫu; số liệu thật. |
| Hành trình | Landscape trái và tiến độ phải, không icon đầu tiêu đề. Demo có 3 nhóm nên cao hơn ảnh 1 nhóm; giữ các nhóm hiện có. |
| Truy cập nhanh | 4 card cùng hàng tại 375/390/430, đúng ảnh tương ứng. Chữ xuống dòng theo viewport. |
| SYNCY | Hình người nữ trái, chữ/mũi tên phải; nằm giữa Quick access và Gợi ý. |
| Gợi ý | Đúng vị trí; dữ liệu hiện không có đề xuất nên hiển thị empty state. |
| Mentor | 2 card ngang; tên/ảnh thực từ service, không giả danh tính theo ảnh mẫu. |
| Thành tích | Đúng vị trí; demo chưa có thành tích nên hiển thị empty state. Chưa kiểm tra live mở rộng khi có nhiều thành tích. |
| Quote | Banner thấp, chữ trái và phong cảnh phải. |
| Footer | Đúng vị trí trên nav; giữ link xác thực. Help/privacy/terms/contact chưa có URL nên chưa liên kết; chưa có URL YouTube/email để thêm. |
| Bottom Navigation | Đúng 5 mục, thứ tự giữ nguyên; fixed và có lề, không đổi icon hiện có. |

## Kiểm thử
- Build .NET/Vite thành công, 0 lỗi; 5 warning .NET hiện có và cảnh báo Vite về ảnh ngoài phạm vi/bundle.
- 107/107 bài kiểm thử hiện có đạt.
- Playwright/Edge headless với mobile/touch: 320, 360, 375, 390, 414, 430px, Light/Dark; không tràn ngang, tiêu đề không đè CTA.
- Screenshot thực full-page: `screenshots/locked-home/home-{light,dark}-{375,390,430}.png`.
- `*-scroll.png`: viewport cao 844px với nav fixed tại vị trí của viewport đầu. Ảnh không hậu tố scroll dùng viewport cao bằng nội dung để xem toàn trang, không thay CSS sản phẩm.
- Đã kiểm tra thứ tự section, ảnh không bị thiếu, tìm kiếm/no-match/bộ lọc/reset, học tiếp, 4 shortcut, SYNCY, Mentor, About, hành trình tới Roadmap và bản đồ tương tác vẫn hoạt động.
- 768/1024px không tràn ngang, desktop 1024px vẫn dùng layout cũ.
- Báo cáo máy: `locked-home-qa.json`. Không kiểm tra điện thoại vật lý hoặc backend production.

Chưa tuyên bố giống 100%: dữ liệu, logo/avatar/Mentor thật, số nhóm kỹ năng, trạng thái rỗng và Footer thiếu URL vẫn khác hình minh họa. Không hardcode dữ liệu để làm ảnh giống mẫu.