# Homepage — kiểm tra theo ảnh và asset người dùng

## Phạm vi

Giao diện học tập ở `/dashboard`, dùng Blazor Interactive Server (.NET 10).
Giữ bố cục ảnh tham chiếu: hero hai cột, bốn ô thống kê, bốn lối tắt,
banner SYNCY, danh sách gợi ý và cố vấn nằm ngang, năm tab điều hướng.
Không sửa trang landing `/`, API, đăng nhập hoặc dữ liệu người dùng.

## Chẩn đoán và thay đổi

- `SoftSync.Presentation/Components/Shared/MobileHome.razor`: thêm trạng thái
  roadmap một kỹ năng gọn, gồm tên, trạng thái khóa, số hoạt động, phần trăm
  và thanh tiến độ thật; trạng thái chưa có kỹ năng thêm liên kết bắt đầu.
- `SoftSync.Presentation/wwwroot/css/mobile-home.css`: hợp nhất selector trùng
  và hai khối breakpoint mobile, giữ một tập quy tắc theo từng breakpoint.
  Giữ nền xanh nhạt và màu chữ theo ảnh, giữ token riêng cho chế độ tối.
  Cân lại ảnh welcome và mascot; bỏ `overflow-x: clip` ở cấp trang để kiểm tra
  tràn ngang thực tế, giữ cuộn ngang trong các danh sách có chủ đích.
- File báo cáo này và ảnh `screenshots/homepage-assets-*.png` được thêm để review.

Bộ `softsync_home_assets.zip` đã được kiểm tra. Dự án đã có 13 bản WebP
được tối ưu từ bộ này tại `wwwroot/images/home-v3`; tiếp tục dùng chúng.
Logo và ảnh cố vấn giữ nguồn hiện có. Không tạo ảnh hay nhân vật thay thế.

## Dữ liệu

- `Dashboard.razor` tải hồ sơ qua `IUserService`, lộ trình qua `IRoadmapService`.
- Bài hiện tại, mục tiêu, thống kê hoàn thành, gợi ý và thành tích lấy từ
  `RoadmapItemDto`; XP/cấp độ lấy từ `UserDto`.
- `JourneyProgress` quyết định khóa bài và hoạt động cần tiếp tục.
- `IMentorService` cung cấp tên, chuyên môn, tiểu sử và ảnh cố vấn.
- Giữ `LocalizedComponentBase`, `L.Ui`, lựa chọn ngôn ngữ và theme hiện có.

## Kiểm tra đã chạy

- `dotnet build SoftSync.Presentation/SoftSync.Presentation.csproj --no-restore`
  với thư mục output riêng: thành công, 0 lỗi; 6 cảnh báo có sẵn về nullable,
  KnownNetworks, form và trường Progress chưa gán.
- `dotnet test SoftSync.Tests/SoftSync.Tests.csproj --no-restore -v:q`:
  97 qua, 0 lỗi, 0 bỏ qua.
- Playwright/Edge trên app chạy thật bằng chế độ demo có sẵn, không sửa fixture
  để khớp số liệu trong ảnh. Kiểm tra 320, 360, 375, 390, 414, 430, 480,
  768, 1024, 1440 px ở cả light/dark: không tràn ngang ngoài trang.
- Tìm kiếm có kết quả/rỗng, xóa tìm kiếm, bộ lọc; hero mở đúng bài tiếp tục;
  liên kết kỹ năng, trạng thái khóa; kéo và bàn phím trên carousel; chi tiết
  mentor; sticky header; vùng chạm header; bottom navigation; shortcut;
  ngôn ngữ tiếng Anh, theme persistence, reduced motion: qua.
- Đã xem ảnh render và tinh chỉnh qua ba lượt: giao diện ban đầu, CSS đã
  hợp nhất, rồi cân lại kích thước mascot. Không có lệnh lint riêng trong
  `SoftSync.Presentation/package.json`.

## Ảnh và giới hạn

- [Mobile 390 light](screenshots/homepage-assets-light-390.png)
- [Mobile 390 dark](screenshots/homepage-assets-dark-390.png)
- [Desktop 1440](screenshots/homepage-assets-light-1440.png)

Ảnh full-page có thanh điều hướng cố định tại đáy viewport đầu tiên;
đây là hành vi screenshot của phần tử fixed.

Tên người học, số bài, XP, cấp độ, kỹ năng và thành tích theo dữ liệu thực tế.
Không gán streak bảy ngày hoặc rating thiếu dữ liệu để giống ảnh mẫu.
Mascot trong asset có tư thế khác mascot trong mockup nên không thể khẳng định
trùng từng pixel. Chưa kiểm tra trên điện thoại vật lý. Chạy trình duyệt trên
tài khoản demo đã xác nhận trạng thái roadmap hai kỹ năng; các nhánh 0/1/3+
chưa được xác nhận bằng tài khoản thật tương ứng.
