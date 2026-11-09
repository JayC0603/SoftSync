# SoftSync Space — Shot List và thứ tự quay

Main: 180s, 1920×1080, 30 FPS. Source/runtime/giới hạn xem file 01; thao tác đầy đủ ở file 03. Không cần auth/CV. Thứ tự thu raw theo cụm trang để giảm điều hướng; thứ tự dựng theo timeline dưới đây. Raw duration có phần dư, không cộng vào 180s thành phẩm.

## Thu raw theo cụm

| Thứ tự quay | Raw/take đề xuất | URL và thao tác | Raw tối thiểu đề xuất | Shot thành phẩm | Có thể liên tục? |
|---:|---|---|---:|---|---|
| 1 | RAW_LANDING_01 | `/`: hero → Xem lộ trình → neo bốn bước | 35–45s | L01–L02 | Có, cùng take; bỏ đoạn chờ dư |
| 2 | RAW_RESOURCES_01 | `/resources`: 5 → Giao tiếp 2 → I-Statement 1 → hold | 45–55s | R01–R04 | Có; ưu tiên giữ trước/sau cùng take |
| 3 | RAW_CASE_01 | `/case-studies`: câu hỏi → đáp án trao đổi riêng → phản hồi | 50–60s | C01–C03 | Có; bắt buộc cùng case/phản hồi |
| 4 | RAW_MENTOR_01 | `/mentors`: 3 → Giao tiếp 1 → hồ sơ → cuộn quy trình | 55–70s | M01–M04 | Có; cần chờ điều hướng ổn định |
| 5 | RAW_GUIDE_01 | `/community` → Giao tiếp → ba bước hướng dẫn | 35–45s | G01–G02 | Có; không mở link ngoài |
| 6 | GRAPHICS_01 | Card mở, thương hiệu, câu hỏi, end card | Dựng trực tiếp | T01–T04 | Không phải raw website |
| 7 | VO_TAKE_01 | Thu theo file 05, mỗi cảnh một take + bản liền | Khoảng 4–6 phút gồm nghỉ/lặp | Audio 01–07 | Thu riêng để dễ thay lỗi |

Khoảng 220–275s raw website cho 140s sử dụng; quay dư thêm take nếu thao tác sai. Các tên trên là đề xuất đặt tên khi quay, không phải tệp đã được tạo. Không ghi footage vào repository trong nhiệm vụ chỉ tạo Markdown.

## Mapping shot với timeline dựng

| Shot | In | Out | Giây | Hình/thao tác | Zoom/chuyển cảnh |
|---|---|---|---:|---|---|
| T01 | 00:00 | 00:15 | 15 | Câu hỏi nhu cầu, logo | Fade 8 frame cuối, đồ họa |
| T02 | 00:15 | 00:20 | 5 | Card thương hiệu | Cắt sang UI |
| L01 | 00:20 | 00:25 | 5 | Hero/tiêu đề/logo | 100%, tĩnh |
| L02 | 00:25 | 00:35 | 10 | Bấm Xem lộ trình, neo và hold | Cuộn qua link, giữ nhãn minh họa |
| R01 | 00:35 | 00:40 | 5 | Toàn cảnh 5 tài nguyên | 100% |
| R02 | 00:40 | 00:46 | 6 | Lọc Giao tiếp, 2 mục | 110% crop nếu cần |
| R03 | 00:46 | 00:52 | 6 | Gõ I-Statement | Giữ cả ô nhập và số lượng |
| R04 | 00:52 | 01:05 | 13 | 1 mục, giữ kết quả | 110–115%, không bấm Mở/Lưu |
| C01 | 01:05 | 01:18 | 13 | Đọc câu hỏi/đáp án, hover lựa chọn | 100%; dịch subtitle |
| C02 | 01:18 | 01:22 | 4 | Bấm trao đổi riêng, phản hồi xuất hiện | Không jump tới kết quả khác |
| C03 | 01:22 | 01:40 | 18 | Đọc phản hồi, overlay dịch | 110–120% cuối shot |
| M01 | 01:40 | 01:47 | 7 | Danh sách 3 Mentor | 100% |
| M02 | 01:47 | 02:00 | 13 | Lọc Giao tiếp, bấm Xem hồ sơ | Không che click/điều hướng |
| M03 | 02:00 | 02:11 | 11 | Chuyên môn/phạm vi hỗ trợ | 110% |
| M04 | 02:11 | 02:20 | 9 | Cuộn quy trình, hold mục tiêu | Dissolve 8 frame trong cuối shot |
| G01 | 02:20 | 02:30 | 10 | Community mô tả phạm vi, bấm Giao tiếp | 100%, không hứa feed/chat |
| G02 | 02:30 | 02:40 | 10 | Ba dòng hướng dẫn kỹ năng | 110% |
| T03 | 02:40 | 02:45 | 5 | Card câu hỏi bước tiếp theo | Fade 8 frame trong shot |
| T04 | 02:45 | 03:00 | 15 | Tổng hợp, logo/tagline, hold | Fade nhạc 2s cuối; dừng đúng Out |

Website shots L/R/C/M/G = 140s. Graphics T = 40s. Tổng 180s. Đặt In/Out theo cột, không cộng transition như một shot riêng. Chỉ dùng crop hậu kỳ; browser zoom giữ 100% toàn buổi.

## Cảnh cần quay riêng và điểm dễ lỗi

Case random: kiểm nội dung trước take. Không thể lấy câu hỏi take A và phản hồi take B. Nếu case main không còn, quay phương án 35s trong file 03 và thay VO/overlay, không dùng phản hồi bịa.

Mentor: danh sách và hồ sơ phải cùng người. Không gửi yêu cầu hỗ trợ thật. Không kéo dài đoạn rỗng “chưa có Mentor gắn kỹ năng” để ám chỉ đã có kết nối.

Logo/card thu từ nhận diện có sẵn, không dựng mockup Dashboard thành kết quả người học. Không lấy 38%/13 trên 34 làm chứng cứ tiến bộ.

CV/assessment O1/O2 cần buổi riêng sau xác nhận môi trường/account test, không thuộc shot đã verified. CV 40s thay **M01–M04**, assessment/roadmap 35s thay **C01–C03**. Giữ raw kết quả thật, update file 01/03/05 và bảng shot trước dựng. Nếu điều kiện thiếu, giữ bản chính.

## Handoff cho người dựng

Nhận raw có tên rõ, bản VO cảnh và bản liền; note lỗi/giới hạn của mỗi take. Dựng hình trước theo mốc khóa, rồi ghép VO vào vùng đọc và giữ khoảng lặng quan sát. Không tăng tốc thao tác tới mức không đọc được hoặc time-stretch lời để ép vừa. Tất cả kết quả hiển thị phải có raw liên tục để đối chiếu. Kiểm bản xuất 5.400 frame, 30 FPS, 180s.
