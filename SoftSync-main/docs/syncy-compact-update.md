# Banner Hỏi SYNCY gọn với mascot chính thức

- Dùng /images/syncy-robot.png, cùng asset chính thức ở Assistant/About/Home; không tạo hoặc sửa ảnh.
- Hiển thị đầy đủ mascot bằng object-fit:contain, vùng 72x72px.
- Rút còn tiêu đề Hỏi SYNCY và mô tả Giải đáp kỹ năng, gợi ý lộ trình; bỏ subtitle lặp. Banner đo được cao 90px ở 320/375/390/430px, cả Light/Dark.
- Giữ vị trí banner và link /assistant, không thay chức năng hoặc API.
- Sửa StrictMobileHome.razor và wwwroot/css/strict-mobile-home.css trong SoftSync.Presentation.
- Build .NET/Vite thành công: 0 lỗi, 5 cảnh báo .NET có sẵn.
- Edge/Playwright: 8 tổ hợp kích thước/chế độ màu đạt; ảnh không thiếu, không cắt, không đè chữ, không tràn ngang; click banner tới /assistant. Request ngoài localhost bị chặn trong bài kiểm tra. Chưa kiểm tra thiết bị vật lý/production.
- Báo cáo: syncy-compact-qa.json; screenshot thực: screenshots/syncy-compact/.
- Thay đổi ở local, chưa push.