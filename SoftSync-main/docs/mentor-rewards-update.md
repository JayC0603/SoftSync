# Mentor và thành tích mobile

- Giữ bề rộng card Mentor bằng một nửa vùng hiển thị (trừ gap 8px), vẫn 2 card ở 375/390/430px.
- Hiển thị toàn bộ Mentor đã được service hiện có trả về, dùng carousel sẵn có; sau khoảng 5,5 giây card trượt sang trái, hết danh sách quay lại đầu. Không thêm dữ liệu hoặc đổi API.
- Kéo ngang/chạm và phím mũi tên vẫn dùng được; autoplay tạm dừng khi hover/focus/tương tác, và tắt với Reduce Motion.
- Thành tích: tiêu đề 16px/700, mô tả và ngày 14px. Xem thêm/Thu gọn 13px/600; giữ nguyên logic mở rộng.
- Build .NET/Vite: thành công, 0 lỗi, 5 cảnh báo .NET hiện có.
- Kiểm tra trình duyệt và screenshot: `mentor-rewards-qa.json`, `screenshots/mentor-rewards/`.
- Screenshot thành tích sử dụng fixture lịch sử đã render bởi component thật, chỉ thay DOM trong trang kiểm thử vì demo không có thành tích; không ghi dữ liệu vào ứng dụng. Mentor lấy từ ứng dụng demo đang chạy.