# SoftSync Space — Recording Checklist

Ngày khảo sát 07/11/2026. Bản chính là public showcase trên localhost, không yêu cầu tài khoản/CV. Mọi nhãn A/B/C chỉ đúng phạm vi file 01. Mở file 03 khi quay; file 06 là thứ tự thu raw, file 05 để thu VO.

## Thiết bị và cấu hình

- [ ] OBS hoặc công cụ tương đương đã có; không cài package vào dự án.
- [ ] Desktop/capture 1920×1080, tỉ lệ 16:9; OBS canvas/output 1920×1080, 30 FPS. Không crop mất thao tác hoặc kéo giãn khung khác tỉ lệ.
- [ ] Quay MKV, remux MP4 bằng tính năng có sẵn; H.264 chất lượng cao, giữ chữ nhỏ đọc được. Gợi ý CQP 18–22 tùy máy; xem thử để chọn, không coi đây là thông số bắt buộc cho mọi thiết bị.
- [ ] Thu 10 giây thử, xem chuyển động chuột/chữ và kiểm tra không dropped frame. Chỉ quay dài sau khi hình/âm thanh đạt.
- [ ] Micro gần và ổn định, thu riêng 48 kHz nếu có; tránh clipping. Lưu bản VO gốc. Nhạc thấp hơn lời, không lấn phản hồi trên màn hình. Chỉ dùng nhạc có quyền sử dụng.
- [ ] Browser zoom 100%, tiếng Việt, giao diện sáng như bản quan sát; ẩn bookmark/DevTools/tab riêng tư. Ẩn thanh địa chỉ khi dựng. Không thay browser zoom giữa take.
- [ ] Tắt thông báo bằng thao tác người quay; đóng cửa sổ chứa thông tin tài khoản. Không để email/điện thoại/người dùng thật xuất hiện.

## Môi trường và dữ liệu

- [ ] Kiểm tra `http://localhost:5203/` đang chạy. Nếu không, xin môi trường demo an toàn; không tự chạy initializer/migration hoặc sửa .env.
- [ ] Không dùng mật khẩu tìm thấy trong source hoặc tự tạo account production. Không gọi API ghi chỉ để có cảnh đẹp.
- [ ] Bản chính: phiên khách, từ khóa `I-Statement`, case có sẵn, hồ sơ công khai Mentor, hướng dẫn kỹ năng. Không cần CV hoặc lịch sử học.
- [ ] `/resources`: Tất cả = 5, Giao tiếp = 2, Giao tiếp + I-Statement = 1 trong bản đã kiểm tra. Nếu dữ liệu đổi, quay kết quả thật và cập nhật kịch bản, không ép số cũ.
- [ ] Case Group Communication còn có thể quay cùng phản hồi thật; case random, không giả định lần tải nào cũng giống nhau.
- [ ] `/mentors` có danh sách; bộ lọc Giao tiếp và link hồ sơ hoạt động. Tên hồ sơ runtime có dấu khác landing; giữ chữ đang hiển thị.
- [ ] Không gọi UI community là feed/chat. Trang skill 1 hiện chưa có Mentor được gắn kỹ năng; không che lỗi để tuyên bố có kết nối.
- [ ] Landing có nhãn minh họa: giữ nhãn khi thẻ xuất hiện, không dùng 38%/13 trên 34/55% như thành tích cá nhân.

## URL cần mở sẵn

| Thứ tự tab | URL | Reset trước take |
|---|---|---|
| A | http://localhost:5203/ | Đầu hero, không có neo |
| B | http://localhost:5203/resources | Tất cả, ô tìm trống |
| C | http://localhost:5203/case-studies | Chưa chọn đáp án; case đúng nội dung nếu quay main |
| D | http://localhost:5203/mentors | Tất cả, ô tìm trống |
| E | http://localhost:5203/community | Đầu trang |
| Qua click | http://localhost:5203/mentors/2 | Chỉ mở từ link hồ sơ thật trong take |
| Qua click | http://localhost:5203/community/skills/1 | Chỉ mở từ thẻ Giao tiếp trong take |

`https://softsync.id.vn/` báo ERR_CONNECTION_CLOSED lúc khảo sát, chưa phù hợp làm đích CTA/QR. Trước phát hành có thể kiểm tra lại, nhưng không tự chuyển sang production để thao tác ghi dữ liệu.

## Thứ tự quay và kiểm từng cảnh

1. Landing: hero → bấm Xem lộ trình → neo bốn bước. Lấy thêm logo/shot tĩnh cho card. Kiểm nhãn minh họa.
2. Tài nguyên: quay liên tục Tất cả → Giao tiếp → I-Statement → hold. Không bấm Mở/Lưu, không hứa tải tài liệu.
3. Case: quay câu hỏi + hai đáp án → chọn trao đổi riêng → phản hồi. Raw cùng một lượt; không ghép kết quả khác.
4. Mentor: danh sách → lọc Giao tiếp → Xem hồ sơ → đọc chuyên môn → cuộn quy trình. Không gửi yêu cầu.
5. Community: trang tổng quan → Giao tiếp → ba bước hướng dẫn. Không bấm link ngoài/Mở lộ trình.
6. Thu VO và dựng card cảnh 01, phần đầu 02, cuối 06, cảnh 07. Đối chiếu chữ VO trong file 05.

| Cảnh | Điều kiện đạt |
|---|---|
| 01 | Nhu cầu gần gũi, không thống kê thiếu nguồn |
| 02 | Link neo hoạt động, không nhầm minh họa với roadmap cá nhân |
| 03 | Thấy trước/sau tìm-lọc, đọc rõ từ khóa và thẻ |
| 04 | Câu hỏi/đáp án/phản hồi cùng take; không gọi AI |
| 05 | Hồ sơ đúng người vừa bấm, thấy chuyên môn; không hứa có tư vấn thật |
| 06 | Hướng dẫn kỹ năng đúng phạm vi; 20s UI + 5s card |
| 07 | Tagline khớp các cảnh thực đã dùng, không URL chưa kiểm tra |

## Nếu quay bản có CV/assessment

- [ ] Chỉ thực hiện O1/O2 trong file 03 sau khi môi trường/tài khoản test và quyền ghi dữ liệu test được xác nhận. Không coi hết thời gian chờ là đã được phép.
- [ ] CV giả không PII; `/cv` PDF/TXT khác `/cv-review` PDF/DOCX. Dùng TXT/PDF text để giảm rủi ro OCR. Giới hạn source không chứng minh runtime đang dùng đúng cấu hình.
- [ ] Test một lượt đầy đủ trước ngày quay; ghi route, thao tác, loại file, kết quả, thời gian xử lý, có fallback hay không, raw filename trong file 01. Không đặt sẵn điểm hoặc tạo kết quả.
- [ ] Upload/submit/save là ghi dữ liệu: không làm trên production trong phạm vi nhiệm vụ này. Nếu thiếu quyền hoặc AI lỗi, giữ bản chính.
- [ ] Khi thay cảnh, cập nhật VO/shot/closing đồng bộ. Không thêm O1/O2 lên timeline cũ làm vượt 180s.

## Quy trình tránh lỗi khi quay

Quay dư 3–5 giây hai đầu, dừng con trỏ ở lề sau click, cuộn chậm 1–2 giây rồi hold. Không điều hướng tiếp trước khi nội dung ổn định. Nếu loading lâu, giữ raw; chỉ cắt chờ sau khi đã quan sát kết quả thật. Nếu lỗi, ghi nhận và chuyển phương án thay đã mô tả, không sửa ứng dụng.

Quay lại take bằng reset component/trang chỉ khi không có form thật đang nhập. Không refresh tài khoản production hay reset DB để lấy màn hình sạch. Giữ thư mục raw riêng do người quay quản lý; các file Markdown này không phải footage.

## Kiểm thành phẩm

- [ ] Xem một lượt không âm thanh: hiểu được trước/sau thao tác, chữ rõ, không có jump sang kết quả khác.
- [ ] Nghe một lượt không nhìn: câu chuyện liền mạch, VO tự nhiên, không giọng đọc quá nhanh.
- [ ] Xem cả hình/âm: “chọn”, “phản hồi”, “mở hồ sơ” trùng hành động thực tế.
- [ ] Có phụ đề tiếng Việt; dịch case tiếng Anh như lớp overlay, không sửa UI.
- [ ] 30 FPS cố định, **5.400 frame**, **180,000 giây**; frame cuối 00:02:59:29, điểm out 03:00:00:00. Không black tail, không gap/overlap.
- [ ] Mốc 00:15/00:35/01:05/01:40/02:20/02:45 đúng timeline. Transition trừ vào clip, không cộng ngoài.
- [ ] Main có **140 giây giao diện thật (77,8%)**. Không tăng card tới mức mất tỷ lệ này.
- [ ] Export xong xem lại file xuất, không chỉ timeline editor. Kiểm thuộc tính FPS/thời lượng và đếm frame nếu công cụ có; nếu audio padding/container lệch, chỉnh export để video stream đúng 5.400 frame.
- [ ] Không số liệu báo cáo chưa đọc, lời chứng thực bịa, hứa độ chính xác AI, dữ liệu cá nhân/token hoặc QR đang lỗi.

## Review tài liệu tại ngày bàn giao

- [x] Đã khảo sát source và route, phân loại A/B/C/D/E theo phạm vi.
- [x] Đã cố gắng browser và có các tương tác public được xác minh.
- [x] Đã ghi ba báo cáo chưa tìm thấy và URL public không truy cập được.
- [x] Kịch bản chính dựa trên thao tác đã quan sát; có phương án thay cảnh.
- [x] Không sửa ứng dụng/database/config, không cài package hoặc deploy.
- [ ] Chưa xác minh private/AI end-to-end vì thiếu tài khoản test và môi trường ghi an toàn.
- [ ] Chưa thu âm, quay footage hoặc export video. Thời lượng 180s là cấu trúc dựng đã kiểm tra; bản phim chỉ xác nhận sau khi người quay xuất file.

## Kết quả kiểm tra nhất quán tài liệu

Đã kiểm tra bằng script đọc Markdown: đủ 7 file; 7 cảnh liên tục tổng 180s; 19 shot liên tục tổng 180s; 140s giao diện; 5.400 frame ở 30 FPS; bản phụ 75s; 7 đoạn VO khớp nguyên văn giữa file 03 và 05; 83 liên kết source trỏ tới file tồn tại và số dòng hợp lệ. Git status của source tham chiếu vẫn sạch sau khảo sát. Đây là kiểm tra tài liệu, không thay thế kiểm thử chức năng private hoặc đo video/giọng thu thực tế.
