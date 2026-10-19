# Phần cuối Homepage theo ảnh mới

Đã sửa `MobileHome.razor`, `AppFooter.razor`, `mobile-home.css` và `footer.css`.

- Thành tích: huy hiệu khiên, tên bài, điểm và ngày riêng, nhãn hoàn thành,
  chevron và dòng chúc mừng. Lấy đúng dữ liệu quiz đã qua; không thêm thành tích
  cho tài khoản chưa đạt. Giữ trạng thái rỗng có thật.
- Trích dẫn: chữ bên trái, nền trời và đường lên núi bên phải bằng asset sẵn có.
- Footer dashboard: logo và chữ SoftSync, ba liên kết, Facebook, YouTube và website.
  Người dùng xác nhận chưa có kênh YouTube: biểu tượng có mô tả, không gắn URL giả.
  Các phần bổ sung được ẩn trên trang khác để giữ giao diện footer hiện có.

Build thành công (0 lỗi, 6 cảnh báo có sẵn). Kiểm tra Playwright tại 320, 375,
390, 430, 768, 1180 và 1440 px ở cả light/dark: không tràn ngang.

Kiểm tra HtmlRenderer với dữ liệu thử riêng cho 0/1/2/4 kỹ năng: qua; điểm/ngày
quiz giữ đúng dữ liệu, quiz trượt không thành achievement. Kiểm tra thẻ đã hoàn
thành ở 320/390/768/1180 px, cả hai theme: không đè nhãn hoặc tràn ngang.
Các fixture này không ghi vào dữ liệu ứng dụng.

- [Giao diện thật của tài khoản demo, chưa có thành tích](screenshots/homepage-bottom-light-390.png)
- [Thẻ đã hoàn thành với dữ liệu kiểm thử](screenshots/homepage-bottom-achievement-fixture.png)

Hình núi dùng asset người dùng đã cung cấp, có chi tiết khác artwork trong ảnh
mới; không tạo ảnh thay thế. Chưa thử trên điện thoại vật lý.
