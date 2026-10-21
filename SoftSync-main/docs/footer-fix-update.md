# Sửa Footer mobile

- Footer giữ logo, nội dung và các liên kết hiện có.
- Nhóm thông tin chính căn đều; bỏ dấu | gây rớt dòng.
- Nhóm hỗ trợ/pháp lý xếp cố định 2 cột, 2 hàng thay vì flex-wrap để Liên hệ không đứng lẻ dưới một hàng dài.
- Đồng bộ gap và line-height; copyright vẫn bên phải. Giữ khoảng trống an toàn phía trên bottom nav; bổ sung padding cho màn hình dưới 360px vì nav cao hơn tại kích thước này.
- File sản phẩm: SoftSync.Presentation/wwwroot/css/strict-mobile-home.css. Chỉ CSS dashboard mobile; không thay API/route hay thêm link chưa có URL.
- Kiểm tra bằng app local thực tế/Edge, 320/360/375/390/430px, Light/Dark; chi tiết footer-fix-qa.json, ảnh trong screenshots/footer-fix/.
- Không thay code .NET; không chạy lại build/test .NET cho thay đổi CSS này. Chưa kiểm tra production hoặc thiết bị vật lý.