# Typography và khoảng cách cuối Homepage

Giữ bố cục theo ảnh: huy hiệu trái, tiêu đề và metadata ở giữa, trạng thái hoàn
thành góc phải, lời chúc bên dưới; quote trái và cảnh núi phải. Giữ dữ liệu thật,
chức năng xem thêm/thu gọn và năm tab điều hướng.

Nguyên nhân: mobile-home.css đặt trạng thái hoàn thành ở hàng riêng khi dưới
540px; quote có chiều cao tối thiểu 215px và vùng chữ rộng 88%. Bottom navigation
có bottom bằng 8px cộng safe-area, trong khi workspace dự phòng 92px cộng safe-area.
Các lớp CSS dùng nhiều giá trị khác nhau nên tạo cảm giác thanh nổi cao và cuối
trang thừa khoảng cách.

Đã sửa:

- `SoftSync.Presentation/wwwroot/css/mobile-home.css`: giữ trạng thái ở góc phải,
  dành chỗ cho tiêu đề để không đè nhau, thu gọn quote; tên bài/kỹ năng/cố vấn
  tối thiểu 14px, metadata tối thiểu 12px; bỏ cắt mô tả hero. Thanh dưới bottom 0,
  safe-area chỉ trong padding. Khoảng dự phòng 68px cộng safe-area tại workspace.
- `SoftSync.Presentation/wwwroot/css/mobile-layout.css`: cùng quy tắc bottom 0,
  padding safe-area và chiều cao dự phòng cho layout mobile chung. Giữ kính mờ
  và xử lý bàn phím đã có.

Kiểm tra thực tế bằng Playwright/Edge: 320, 360, 375, 390, 414, 430, 768 px ở
light/dark; không tràn ngang; bottom navigation nằm đúng cạnh dưới ở đầu/giữa/cuối.
Thẻ thành tích với dữ liệu thử ở 320/390/768/1180 px, hai theme: không chồng tiêu
đề và nhãn trạng thái. Build .NET thành công, 0 lỗi, 6 cảnh báo có sẵn.

Ảnh đầu/cuối 375/390/430 px được lưu tại `.ui-qa/screenshots/reference-ending-*`.
Chưa kiểm tra trên thiết bị iOS Safari/Android Chrome thật hoặc thanh địa chỉ
trình duyệt di động thật; thay đổi viewport trong automation không thay thế được
kiểm tra đó. Không tạo artwork mới; hình cảnh núi dùng bộ asset được cung cấp.
