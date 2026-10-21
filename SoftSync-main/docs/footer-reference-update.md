# Footer theo mẫu mới

Người dùng yêu cầu bố cục theo ảnh và xác nhận để sẵn YouTube/email, sẽ bổ sung URL sau.

- Giữ logo chính thức, mô tả, Facebook thật và các route hiện có.
- Góc phải có Facebook / YouTube / email. YouTube và email là biểu tượng chưa có liên kết, aria-disabled và title giải thích; không bịa URL hoặc địa chỉ.
- Bản quyền và phiên bản lấy năm hiện tại theo giờ Việt Nam, Assembly Version (hiện 1.0.0), không hardcode phiên bản trong nội dung.
- Từ 640px: logo/mô tả bên trái, social bên phải; 2 hàng liên kết bên trái, copyright/version bên phải.
- Mobile 375/390/430px: tự xuống dòng mô tả và phần copyright để giữ chữ tối thiểu 12px. Nhóm pháp lý chia 2 cột, tránh Liên hệ rớt dòng lẻ. Giữ khoảng cách an toàn với nav.
- Chỉ sửa StrictMobileHome.razor và wwwroot/css/strict-mobile-home.css trong SoftSync.Presentation; không thay API hoặc chức năng khác.
- Build .NET/Vite thành công: 0 lỗi, 5 cảnh báo .NET có sẵn.
- Kiểm tra Edge/Playwright ở 320/375/390/430/640/783px, Light/Dark: footer-reference-qa.json. Screenshot thực ở screenshots/footer-reference/. Các request ngoài localhost được chặn trong bài kiểm tra để tránh tài nguyên bên thứ ba làm treo quá trình tải; ảnh chụp dùng asset ứng dụng thực tế và font sẵn có. Chưa kiểm tra điện thoại vật lý/production.
- Thay đổi local, chưa push.