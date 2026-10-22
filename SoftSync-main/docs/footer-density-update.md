# Thu gọn Footer mobile

Người dùng xác nhận thu gọn Footer theo mẫu trước, gom liên kết và đưa copyright/version sát hơn.

- Giảm padding/gap; mô tả thương hiệu nằm cùng dòng khi đủ chỗ, tự xuống dòng khi hẹp.
- Thu khoảng cách nhóm social; Facebook vẫn có vùng bấm 44x44, YouTube/email chưa có URL giữ trạng thái chờ như đã thống nhất.
- Nhóm hỗ trợ/pháp lý giữ 2 cột để không bị liên kết đứng lẻ, giảm chiều cao hàng còn 22px. Nhóm link thật giữ nguyên.
- Copyright và phiên bản chung một hàng trên mobile; màn hình từ 640px vẫn giữ bố cục bên phải như mẫu.
- Chỉ sửa SoftSync.Presentation/wwwroot/css/strict-mobile-home.css; không đổi nội dung, URL hoặc API.
- So sánh trước/sau bằng CSS HEAD và CSS mới trong trình duyệt, cùng ứng dụng/demo: Footer 375/390/430px từ 199px còn 150px (giảm 49px, khoảng 25%); 320px còn 151px, 783px từ 124px còn 106px.
- 10 tổ hợp kích thước/theme đạt: 320/375/390/430/783px Light/Dark, không tràn ngang, chữ tối thiểu 12px, giữ đủ link/icon, footer cách nav khoảng 10–12px.
- Báo cáo máy: footer-density-qa.json; ảnh chụp thật: screenshots/footer-density/. Kiểm thử chặn request ngoài localhost. Chưa kiểm tra thiết bị vật lý hoặc production.
- Không chạy lại build/test .NET cho thay đổi CSS này. Chưa push.