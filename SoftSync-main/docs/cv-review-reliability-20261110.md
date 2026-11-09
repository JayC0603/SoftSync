# Nhận xét CV + JD: độ ổn định và thời gian chờ

Người dùng báo `ai_invalid_response` sau lần sửa tự động và phân tích CV kèm JD
chậm. Chưa có log validation hoặc CV/JD gây lỗi trên Production; không quy một
nguyên nhân duy nhất cho mọi tài liệu.

## Kết quả kiểm tra

- Lượt gọi thứ nhất bằng key XAH lưu cục bộ: `dungcsnd113/gemini-flash-3.8`
  trả 404 (`ai_model_unavailable`) sau khoảng 1,4 giây. Tên này không có trong
  danh mục `/v1/models` hiện tại. Cấu hình fallback Production có thể khác, nên
  đây chưa phải bằng chứng về model cuối cùng đã tạo phản hồi lỗi trên tên miền.
- Giữ Gemini 3.8 làm primary cho CV, dùng route đang niêm yết
  `cuzem3/gemini-3.8-flash-low`. Ánh xạ cấu hình legacy chính xác sang route mới
  để xử lý cả biến môi trường cũ; các model tùy chỉnh khác được giữ nguyên.
- Lượt gọi thứ hai, cùng CV/JD giả, tiếng Việt: phản hồi hợp lệ ngay lần đầu,
  9.227ms, bốn tiêu chí và chín yêu cầu JD. TypeScript được đánh dấu
  `NotDemonstrated`, evidence rỗng, không gán kỹ năng không có trong CV.
- Phát lại kết quả vào service không gọi API: tính điểm văn bản 72, điểm đối
  chiếu theo bằng chứng 84; lần sau dùng cache. Đây là một mẫu đo, chưa phải
  benchmark thống kê hoặc thời gian cam kết cho CV thật.
- Tổng đúng hai lượt tạo nội dung đã được người dùng cho phép. Đọc model catalog
  là yêu cầu metadata, không tạo nội dung. Không gửi CV/JD thật hoặc thay dữ liệu
  Production. Artefact giả lập trong `.tools/cv-diagnostic/` được Git ignore.

## Sửa xử lý phản hồi và độ trễ

- Trước sửa, mỗi lần AI có deadline 110 giây; service có thể tạo rồi sửa, UI
  chờ tối đa 240 giây. Cấu hình mới giới hạn tổng AI ở 90 giây, dùng chung cho
  chờ trong service, tạo, fallback và sửa; không cấp thêm toàn bộ deadline cho
  lần sửa. Thời gian đọc file, tìm/lưu kết quả có giới hạn riêng.
- Chuẩn hóa enum đã biết, chữ số JSON dạng chuỗi và các khác biệt typography
  Unicode thường có trong PDF. Không dùng fuzzy matching; không chấp nhận bằng
  chứng/yêu cầu bịa, điểm ngoài phạm vi, phản hồi thiếu, JSON bị cắt hoặc lọc.
- Báo lỗi JD tới từng dòng và trường. Lượt sửa nhận đúng section cũ để hiểu chỉ
  số dòng, giữ nguyên những section đã hợp lệ; thông báo log không chứa CV/JD.
- Yêu cầu trích dẫn ngắn, liên tục và nguyên văn để giảm lặp nội dung khi sinh
  bảng JD. CV/JD đầu vào vẫn được gửi đầy đủ; không cắt mất yêu cầu cuối tài liệu.
- CV gốc, quyền sở hữu, dữ liệu đã lưu, rubric và cách tính điểm không đổi.

Build .NET/Vite đạt. Bộ kiểm tra liên quan đạt 93 test, một real-AI opt-in test
được skip; hai lượt gọi thật được chạy riêng có giới hạn. Bổ sung bảy test cho
deadline chung, sửa từng field, Unicode/enum, numeric strings và migration model.
Chưa xác minh trên Vercel hoặc bằng chính tài liệu người dùng báo lỗi.
