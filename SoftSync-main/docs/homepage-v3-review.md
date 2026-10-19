# SoftSync Homepage v3 — báo cáo triển khai

Trang `/dashboard` dùng một giao diện thống nhất trên điện thoại, tablet và
desktop. Giữ kiến trúc Blazor Interactive Server, session đăng nhập, dịch vụ dữ
liệu và điều hướng hiện tại. Không thêm dependency runtime, không sửa database,
API contract hoặc cơ chế xác thực.

## Source đã thay đổi

| Tệp | Thay đổi |
| --- | --- |
| `SoftSync.Presentation/Components/Pages/Dashboard.razor` | Giữ tải user, roadmap, kiểm tra onboarding và quyền Admin; dùng một component Trang chủ; bỏ markup desktop cũ bị trùng và danh sách mentor hardcode. |
| `SoftSync.Presentation/Components/Shared/MobileHome.razor` | Dùng chung cho mọi viewport; search, welcome, mục tiêu, hero, thống kê, tổng quan kỹ năng, shortcut, SYNCY, gợi ý, mentor thật, thành tích và động lực. |
| `SoftSync.Presentation/wwwroot/css/mobile-home.css` | Thay các lớp override cũ bằng CSS v3 có scope; màu xanh/trắng, dark tokens, breakpoint, sticky header, touch target, safe area và carousel. |
| `SoftSync.Presentation/Components/Pages/RoadmapOverview.razor` | Nhận query `skill` để từ tổng quan mở đúng kỹ năng; giữ quy tắc khóa bài học; sửa giá trị chuỗi `aria-pressed` cho nút chọn kỹ năng. |
| `SoftSync.Presentation/wwwroot/images/home-v3/` | 13 asset WebP tối ưu và README mô tả nguồn, kích thước, vị trí sử dụng. |

Tái sử dụng `MainLayout`, `HeaderActions`, `NotificationBell`, `NavBar`,
`AppFooter`, `UiState`, `LocalizedComponentBase`, `SkillTheme`, `JourneyProgress`
và carousel JavaScript hiện có. Header và bottom navigation không bị tạo bản sao.

## Dữ liệu thực tế

| Nội dung | Nguồn |
| --- | --- |
| Tên công khai, XP | `IUserService.GetUserByIdAsync`, `UserDto.PublicName`, `ExperiencePoints` |
| Bài học, hoạt động, tiến độ | `IRoadmapService.GetUserRoadmapAsync`, `RoadmapItemDto` |
| Tổng quan kỹ năng | Gom roadmap hiện có theo kỹ năng, tối đa bốn nhóm; không dùng bốn kỹ năng mẫu cố định. |
| Khóa bài và tiếp tục học | `JourneyProgress.State`, `RoadmapReviewMode.Enabled`, `JourneyProgress.ResumeStep`; cùng thứ tự với trang học tập. |
| Gợi ý | Tối đa hai bài chưa hoàn thành và có thể mở trong roadmap người dùng. |
| Mentor, avatar, chuyên môn, mô tả | `IMentorService.GetAllAsync` trong DI scope riêng; tối đa ba mentor thật. |
| Thành tích | Bài đã hoàn thành có dấu thời gian hoạt động, quiz đã vượt qua từ `QuizHistory`; sắp xếp theo thời điểm và lấy tối đa ba. |
| Onboarding và quyền | `AuthenticationStateProvider`, `IAssessmentService`; giữ logic hiện tại. |

Search tìm trong bài học/kỹ năng người dùng và dẫn đến thư viện tài nguyên khi
từ khóa phù hợp. Không có bộ lọc giả. SYNCY mở `/assistant` hiện có. Không sinh
phản hồi AI hoặc ghi dữ liệu trong tác vụ chỉnh giao diện.

## Asset

Đã kiểm tra 13 ảnh trong thư mục người dùng cung cấp, bao gồm kích thước và kênh
alpha. Tổng dung lượng output 143.590 byte, khoảng 140 KiB. Dùng hai SYNCY,
welcome/quote landscape, bốn icon shortcut và thumbnail time-management/teamwork
khi khớp bài học. Ba phong cảnh/planner còn lại được giữ sẵn, không đưa thêm vào
mọi section. Logo và ảnh mentor giữ nguồn thật trong ứng dụng.

Ảnh WebP phong cảnh/thumbnail gốc nhỏ, chỉ dùng ở kích thước nhỏ; không kéo làm
nền toàn trang. Bài không có thumbnail phù hợp dùng icon kỹ năng hiện có.

## Xác minh

- Build `.NET 10` thành công: 0 lỗi, 6 cảnh báo có sẵn ở `Program.cs`,
  `Progress.razor`, `Login.razor`, `Register.razor`.
- `node --check` cho JavaScript carousel hiện có: qua. Không có thay đổi TypeScript.
- Chạy bản local riêng ở cổng 5213 bằng chế độ demo in-memory có sẵn, không
  kết nối database production.
- Playwright Core + Edge headless: 320, 360, 375, 390, 414, 430, 480, 768,
  1024, 1440 px; cả light và dark: 20 tổ hợp. Không tràn ngang ngoài chủ đích.
- Kiểm tra bounds của từng section để không dựa riêng vào việc ẩn overflow.
- Header controls 44 × 44 px trên mobile; sticky header và bottom navigation
  không bị mất khi cuộn; có khoảng trống cuối trang.
- Search có kết quả/không có kết quả/xóa; CTA tiếp tục tới đúng route và bước
  học; link mentor mở trang chi tiết.
- Kiểm tra deep link kỹ năng và trạng thái khóa, carousel bằng bàn phím,
  nút chuyển thẻ, tạm dừng, tự chuyển và `prefers-reduced-motion`.
- Đổi tiếng Anh, dark mode sau reload, profile dropdown, menu Thêm và các
  shortcut: qua. Không có JavaScript page error trong các kịch bản này.
- Đã render, xem ảnh và tinh chỉnh qua nhiều lượt: header sticky, footer,
  khoảng cách, kích thước mentor, welcome illustration và màu tương phản.

Script và kết quả QA ở `.ui-qa/home-v3-*.cjs` và `.ui-qa/home-v3-layout.json`
(thư mục QA local đã được repo bỏ qua). Ảnh review:

- [Mobile light, chiều rộng 390 px](screenshots/homepage-v3-mobile-light.png)
- [Mobile dark, chiều rộng 390 px](screenshots/homepage-v3-mobile-dark.png)
- [Desktop, chiều rộng 1440 px](screenshots/homepage-v3-desktop.png)

Ảnh review mobile dùng viewport cao bằng tài liệu để thanh điều hướng cố định
nằm cuối ảnh xuất. Kiểm tra tương tác dùng viewport điện thoại bình thường
844/900 px chiều cao. Ảnh trong báo cáo dùng tài khoản demo thực tế từ ứng dụng,
không sửa số liệu để khớp mockup.

## Khác biệt còn lại với hình tham chiếu

- Không có dữ liệu streak, rating mentor hoặc số giờ học đáng tin cậy: không
  hiển thị các số liệu mẫu đó.
- Chỉ hiển thị số kỹ năng/gợi ý/thành tích thật có trong tài khoản; tài khoản
  demo hiện không có thành tích đủ dấu thời gian nên section thành tích ẩn.
- Avatar mentor, tên, tiêu đề và tiến độ theo dữ liệu ứng dụng, khác mockup.
- Mobile dùng shortcut 2 × 2 và timeline cuộn ngang khi cần để giữ chữ dễ đọc
  và vùng chạm đủ lớn; chiều cao trang vì vậy khác ảnh mẫu.
- Mascot là asset độc lập trong package, không phải hình cắt từ UI mockup.
- Không khẳng định pixel-perfect. Chưa kiểm tra trên thiết bị iOS/Android vật lý
  hoặc dữ liệu/tài khoản production. Nhánh lỗi mạng của mentor có retry nhưng
  chưa được fault-inject trên trình duyệt; UI thành tích có dữ liệu chưa được
  chụp với tài khoản production.
