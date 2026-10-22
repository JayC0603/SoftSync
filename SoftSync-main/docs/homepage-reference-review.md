# Trang chủ — chỉnh theo bố cục ảnh tham chiếu

Bản này thay thế các lựa chọn bố cục khác ảnh trong lần triển khai V3 trước.
Giữ thứ tự các khối, hero hai bên, hàng shortcut bốn ô, gợi ý và mentor cuộn
ngang, hàng thành tích và banner phong cảnh cuối trang. Không đổi sang grid 2×2
trên điện thoại hoặc đưa CTA hero xuống một hàng dài dưới mascot.

## Tệp thay đổi và component tái sử dụng

- `Components/Shared/MobileHome.razor`: thêm bộ lọc tìm kiếm có hoạt động thật,
  ô cấp độ từ `UserDto.Level`, trạng thái trống cho thành tích để giữ vị trí khối.
- `wwwroot/css/mobile-home.css`: bố cục và tỷ lệ theo ảnh, header có lề và bo
  góc, progress/CTA bên trái hero, các hàng ngang, mentor nhỏ, phong cảnh welcome,
  nền planner ở hero, phong cảnh roadmap và quote. Chế độ tối giữ theme riêng.
- `wwwroot/js/design-system.js`: kéo bằng chuột được khi bắt đầu trên link mentor;
  chặn click nhầm nếu người dùng đang kéo.
- Các screenshot review và tài liệu này. Tiếp tục dùng MainLayout, HeaderActions,
  NotificationBell, NavBar, AppFooter, localization và các dịch vụ đang có.

## Dữ liệu và chức năng

Không sửa backend/database/authentication/API contract. Tên và XP từ
`IUserService`; bài học, tiến độ và lịch sử từ `IRoadmapService`; avatar, tên,
chuyên môn và liên kết mentor từ `IMentorService`. Khóa bài/tiếp tục hoạt động
vẫn qua `JourneyProgress` và chính sách hiện tại.

Filter tìm kiếm có các lựa chọn tất cả, có thể học, đã hoàn thành. Chỉ lọc danh
sách roadmap của người dùng, không sinh kết quả giả. Header, đổi ngôn ngữ,
theme, bottom navigation và tích hợp SYNCY giữ nguyên event handler và route.

## Asset

Dùng bộ `Downloads/softsync_home_assets` đã tối ưu ở `wwwroot/images/home-v3`:
SYNCY studying, SYNCY assistant, welcome landscape, study planner, roadmap
landscape, quote landscape, bốn icon shortcut và thumbnail phù hợp khi có dữ liệu.
Giữ logo gốc và ảnh mentor thật. Không dùng ảnh chụp UI làm giao diện hoặc cắt
nút/chữ từ screenshot.

## Kiểm tra

- .NET build: thành công, 0 lỗi; 6 cảnh báo cũ trong các tệp ngoài thay đổi này.
- `node --check` cho JavaScript: qua.
- Edge headless, 10 chiều rộng từ 320 tới 1440 px, cả light/dark: qua;
  kiểm tra document overflow và bounds của từng section.
- Đo vị trí thực tế tại 320, 390, 430, 768 px: bốn thống kê cùng hàng, bốn shortcut
  cùng hàng, CTA bên trái mascot, mentor cùng hàng và có chiều rộng nhỏ hơn card hero.
- Filter có thể học/đã hoàn thành, trạng thái không có kết quả: qua.
- Kéo trên link mentor không mở nhầm trang, link detail bằng bàn phím: qua.
- Tìm kiếm/xóa từ khóa, tiếp tục bài học, mentor details, sticky header và bottom
  navigation: qua. Giữ vùng chạm header 44×44 px.
- Carousel vẫn tự chuyển; nút điều khiển được giữ cho bàn phím và hiện khi nhận
  focus. `prefers-reduced-motion` tắt tự chuyển.
- Đã render và xem ảnh mobile qua nhiều lượt, tinh chỉnh chiều cao hero, thống
  kê và kích thước mentor. Không tuyên bố pixel-perfect.

## Ảnh review

- [Mobile light 390 px](screenshots/homepage-reference-mobile-light.png)
- [Mobile dark 390 px](screenshots/homepage-reference-mobile-dark.png)
- [Desktop 1440 px](screenshots/homepage-reference-desktop.png)

Ảnh dùng tài khoản demo có sẵn trong ứng dụng, không sửa số liệu để giống mẫu.
Script kiểm tra và số đo ở `.ui-qa/home-reference-*.cjs` và
`.ui-qa/home-reference-layout.json` (local, repo đã bỏ qua thư mục này).

## Khác biệt do dữ liệu thực tế

- Ô thứ tư là **cấp độ hiện tại** tính từ XP thật, không gán chuỗi học bảy ngày
  vì hệ thống chưa có dữ liệu streak đáng tin cậy.
- Số kỹ năng, số gợi ý, thành tích, tên và ảnh mentor theo tài khoản thực tế.
  Không thêm kỹ năng, rating, thời lượng, lượt đánh giá hoặc thành tích giả.
- Nếu chưa có thành tích, vẫn giữ khối với lời nhắc bắt đầu; không hiển thị các
  huy hiệu/điểm thưởng trong hình mẫu như thể đã đạt được.
- Mascot dùng đúng ảnh độc lập trong package; hình đó có tư thế khác mascot
  được vẽ trong mockup. Chưa kiểm tra trên điện thoại iOS/Android vật lý.
