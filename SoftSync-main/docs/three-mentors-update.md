# Mentor: đúng 3 card, kích thước lớn hơn

- Bỏ toàn bộ card lặp dùng cho vòng cuộn; chỉ render tối đa 3 Mentor theo ID riêng biệt từ danh sách service hiện có. Không tạo thêm người/ảnh, không sửa dữ liệu API. Nếu nguồn trả ít hơn 3 người thì giữ số người thật.
- Card rộng 56% vùng cuộn thay vì khoảng 49%, tăng khoảng 15%; tối thiểu cao 116px, ảnh 46x54 thay vì 40x48, tên 13px thay vì 12px. Dưới 360px giữ 1 card đầy chiều rộng.
- Autoplay vẫn chạy khi nhìn thấy vùng Mentor, tạm dừng khi tương tác; hết dãy quay về đầu bằng reset tức thời thay vì nhân đôi card. Tôn trọng Reduce Motion.
- File sửa: Components/Shared/StrictMobileHome.razor; wwwroot/css/strict-mobile-home.css; wwwroot/js/design-system.js trong SoftSync.Presentation.
- Build .NET/Vite thành công, 0 lỗi, 5 cảnh báo .NET có sẵn.
- Playwright/Edge trên ứng dụng demo: kiểm tra đúng 3 ảnh, 3 link Mentor có ID riêng, không card lặp; responsive 320/375/390/430px Light/Dark; autoplay, quay về đầu, touch tạm dừng. Kết quả: three-mentors-qa.json, ảnh: screenshots/three-mentors/.
- Chưa kiểm tra điện thoại vật lý hoặc production. Thay đổi local, chưa push.