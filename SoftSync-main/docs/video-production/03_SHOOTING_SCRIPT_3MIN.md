# SoftSync Space — Kịch bản quay chính xác 03:00

**Mở file này và quay theo từng bước.** Ngày kiểm tra: 07/11/2026. Đây là bản quay những trải nghiệm công khai đã xác minh trên **http://localhost:5203**, gồm tài nguyên, case study, Mentor và hướng dẫn kỹ năng. Không cần đăng nhập để quay bản chính.

Chưa được cấp tài khoản test; CV/assessment/roadmap chưa kiểm tra end-to-end. Ba báo cáo EXE201 chưa tìm thấy. URL `https://softsync.id.vn/` báo lỗi kết nối trong phiên khảo sát. Không dựng các phần chưa xác minh thành demo đã hoạt động. Nguồn: `E:/fpt_edu/EXE/SoftSync-ui-publish-20261107/SoftSync-main`; mã R/S tham chiếu file 01 cùng thư mục. Chưa xác minh binary local trùng tuyệt đối commit source.

## Chuẩn bị trong 10 phút

1. Dùng trình duyệt ở **1920×1080**, zoom 100%, giao diện sáng và tiếng Việt như bản đã quan sát. Giữ cố định kích thước trong mọi take. Nếu màn hình không đủ, dùng canvas/capture 1080p và kiểm tra chữ trước; không kéo giãn video sai tỉ lệ. Browser khảo sát chưa được đặt 1080p nên vị trí/fold có thể khác khi quay.
2. OBS hoặc tương đương: canvas/output 1920×1080, 30 FPS, quay MKV rồi remux MP4. Thu VO riêng. Kiểm tra 10 giây hình/âm thanh; không cần cài package vào dự án.
3. Ẩn bookmark, DevTools, tài khoản, thông báo và tab không liên quan. Chuẩn bị các URL dưới đây trước khi quay. Không để thanh địa chỉ/localhost trong vùng thành phẩm.
4. Không chạy lại backend/database hoặc sửa cấu hình. Cổng 5203 đã có ứng dụng lúc khảo sát. Nếu hiện không chạy, dừng quay và yêu cầu môi trường demo an toàn từ người phụ trách; không tự chạy startup có migration/seed.
5. Kiểm tra nhanh: `/resources` có 5 mục; Giao tiếp còn 2; I-Statement còn 1. `/case-studies` có case và phản hồi. `/mentors` có hồ sơ, lọc Giao tiếp có kết quả. Nếu khác, dùng phương án dưới từng cảnh, không bịa dữ liệu.
6. Không nhập tài khoản hoặc thông tin cá nhân. Không bấm gửi yêu cầu, gửi tin nhắn, đăng ký, lưu kết quả AI hay nộp bài trên production. Được quay tìm/lọc và lựa chọn case đã xác minh chỉ đổi state của component.

| Tab | URL chuẩn bị | Điều kiện đầu |
|---|---|---|
| A | http://localhost:5203/ | Đầu hero, không ở neo |
| B | http://localhost:5203/resources | Tất cả, ô tìm trống |
| C | http://localhost:5203/case-studies | Chưa chọn đáp án |
| D | http://localhost:5203/mentors | Tất cả, ô tìm trống |
| E | http://localhost:5203/community | Đầu trang |

Quay theo cụm, không cần theo thứ tự dựng: landing → tài nguyên → case → Mentor → hướng dẫn; dựng card mở/đóng riêng. Quay dư 3–5 giây trước/sau thao tác. Raw dài hơn video thành phẩm, xem file 06.

## Timeline khóa

| ID | In | Out exclusive | Duration | Nội dung |
|---|---|---|---:|---|
| 01 | 00:00 | 00:15 | 15 giây | Một khó khăn quen thuộc |
| 02 | 00:15 | 00:35 | 20 giây | Khám phá điểm bắt đầu |
| 03 | 00:35 | 01:05 | 30 giây | Tìm tài nguyên theo nhu cầu |
| 04 | 01:05 | 01:40 | 35 giây | Case study và phản hồi |
| 05 | 01:40 | 02:20 | 40 giây | Lọc Mentor và xem hồ sơ |
| 06 | 02:20 | 02:45 | 25 giây | Chuẩn bị câu hỏi cụ thể |
| 07 | 02:45 | 03:00 | 15 giây | Một bước nhỏ hôm nay |

**Tổng 180 giây = 5.400 frame ở 30 FPS.** Giao diện thật 140 giây = 77,8%. Đồ họa 40 giây. Transition nằm trong mỗi cảnh; không cộng thời gian dissolve. Lời đọc tham khảo 120–140 đơn vị tách bằng khoảng trắng/phút; tiếng Việt có từ nhiều âm tiết nên bắt buộc nghe thử bằng giọng thật. File 05 có đếm lời, thời gian đọc và khoảng nghỉ tính tại 130/phút. Khoảng nghỉ dành cho thao tác/đọc UI; không tăng VO để lấp kín.

## SCENE 01 — MỘT KHÓ KHĂN QUEN THUỘC

**Nhịp VO:** khoảng 12 giây đọc ở 130 đơn vị/phút; 3 giây nghỉ/thao tác trong cảnh. Thu thử để căn nhịp thật; không kéo dài cảnh.

**Timeline:** 00:00–00:15 · **15 giây**. **Mục tiêu:** đưa nhu cầu sinh viên lên trước sản phẩm.

**Route/component:** không điều hướng trong thành phẩm; logo SoftSync lấy từ trang chủ đã quan sát. Title card dựng hậu kỳ, không giả làm trang website. **Điều kiện/dữ liệu:** sinh viên giả định muốn giao tiếp tốt hơn trong nhóm; không tên/ảnh thật, không số liệu khảo sát.

| Mốc dựng | Hành động/hình ảnh |
|---|---|
| 00:00–00:05 | Nền sáng, câu hỏi “Muốn tiến bộ, bắt đầu từ đâu?” |
| 00:05–00:11 | Chuyển chữ: “Ngại lên tiếng khi làm việc nhóm?” |
| 00:11–00:15 | Logo và tên SoftSync, chuẩn bị sang website |

**Chuột/nhập/cuộn:** không có. **Zoom:** chữ giữa màn hình, tối đa hai dòng. **VO:**

> Ngại lên tiếng khi làm việc nhóm? Muốn cải thiện kỹ năng mềm, đôi khi điều khó nhất là biết nên bắt đầu từ đâu.

**Overlay:** hai câu hỏi ở bảng; subtitle theo đúng VO. **Transition:** fade 8 frame trong cuối cảnh, không kéo Out quá 00:15. **Trạng thái/bằng chứng:** nội dung kể chuyện, không phải khảo sát người dùng; logo R01/S01. **Lưu ý:** không dùng thống kê thiếu nguồn. **Thay thế:** chữ/logo tĩnh nếu không dựng motion.

## SCENE 02 — KHÁM PHÁ MỘT ĐIỂM BẮT ĐẦU

**Nhịp VO:** khoảng 13 giây đọc ở 130 đơn vị/phút; 7 giây nghỉ/thao tác trong cảnh. Thu thử để căn nhịp thật; không kéo dài cảnh.

**Timeline:** 00:15–00:35 · **20 giây**. **Mục tiêu:** nối nhu cầu với định hướng sản phẩm.

**Route:** `http://localhost:5203/` → `http://localhost:5203/#learning-journey`. **Component:** Home.razor, hero, “Xem lộ trình”, learning-journey. **Điều kiện/dữ liệu:** phiên khách; đầu trang hero. 38%, 13/34, 55% chỉ là minh họa sẵn có.

| Mốc dựng | Thao tác/hình ảnh |
|---|---|
| 00:15–00:20 | Card thương hiệu và thông điệp; 5 giây đồ họa |
| 00:20–00:25 | Cắt sang hero trang chủ, giữ logo/tiêu đề |
| 00:25–00:29 | Di chuột tới “Xem lộ trình” trong hero, bấm một lần, chờ cuộn neo |
| 00:29–00:35 | Giữ bốn bước “Hành trình phát triển cùng SoftSync” |

**Nhập liệu:** không. **Cuộn:** dùng link neo; không cuộn tay khi hiệu ứng chưa dừng. **Zoom:** 100%; nếu crop, giữ tiêu đề/bốn bước và nhãn minh họa của thẻ. **VO:**

> SoftSync Space hướng đến việc giúp sinh viên rèn luyện kỹ năng mềm. Hãy bắt đầu từ những công cụ có thể khám phá trực tiếp trên website.

**Overlay:** “Bắt đầu từ một hành động nhỏ”. **Transition:** hard cut tại 00:35. **Trạng thái:** A trang/neo, C thẻ Dashboard/roadmap minh họa. **Bằng chứng:** R01–R02; S01; screenshot đã quan sát section bốn bước và nhãn minh họa. **Lưu ý:** 15 giây website; không gọi section này là kết quả học tập cá nhân. Không bấm thẻ minh họa như thể đó là Dashboard thật. **Thay thế:** nếu neo không cuộn, quay cuộn tay chậm tới section; giữ đúng 15 giây website.

## SCENE 03 — TÌM ĐÚNG NỘI DUNG ĐỂ BẮT ĐẦU

**Nhịp VO:** khoảng 19 giây đọc ở 130 đơn vị/phút; 11 giây nghỉ/thao tác trong cảnh. Thu thử để căn nhịp thật; không kéo dài cảnh.

**Timeline:** 00:35–01:05 · **30 giây**. **Mục tiêu:** cho thấy danh sách thu gọn theo nhu cầu giao tiếp.

**Route:** `http://localhost:5203/resources`. **Component:** Resources.razor; resource-query, resource-filters, resource-list. **Điều kiện:** Tất cả, tìm kiếm trống. **Dữ liệu nhập:** `I-Statement`.

| Mốc dựng | Chuột/nhập liệu và kết quả cần thấy |
|---|---|
| 00:35–00:40 | Mở tab B, giữ tiêu đề và “5 tài nguyên” |
| 00:40–00:46 | Bấm “Giao tiếp”; dừng để thấy “2 tài nguyên” |
| 00:46–00:52 | Bấm ô “Tìm tài nguyên”, nhập chính xác `I-Statement` |
| 00:52–01:00 | Giữ “1 tài nguyên” và “Mẫu hội thoại I-Statement” |
| 01:00–01:05 | Hold kết quả, đưa chuột ra khoảng trắng; không bấm Mở/Lưu |

**Cuộn:** nếu hàng kết quả ngoài fold, cuộn khoảng 150–250 px trong 1–2 giây; giữ ô tìm/kết quả cùng khung nếu có thể. **Zoom:** crop hậu kỳ 110–115% vào từ khóa, số kết quả và thẻ. Không đổi browser zoom giữa take. **VO:**

> Với mục tiêu giao tiếp tốt hơn, bạn có thể lọc tài nguyên theo kỹ năng, rồi tìm một mẫu hội thoại cụ thể. Danh sách được thu gọn ngay trên màn hình, giúp việc chọn nội dung bớt lan man.

**Overlay:** “Chọn kỹ năng → Tìm nội dung phù hợp”. **Transition:** hard cut sang case tại 01:05. **Trạng thái/bằng chứng:** A tìm/lọc, R03–R05/S02. Danh mục tĩnh và các nút Mở điều hướng module khác, không chứng minh tài liệu riêng đã tải được. **Lưu ý:** không gọi bộ lọc là AI; không hứa bookmark qua nhiều phiên. **Thay thế:** nếu tìm kiếm không phản hồi nhưng bộ lọc hoạt động, chỉ giữ 5 → 2, kéo dài hold; VO đổi thành: “Chọn kỹ năng giao tiếp để thu gọn danh sách. Bạn có thể xem tên và loại tài nguyên trước khi quyết định mình muốn tìm hiểu nội dung nào. Một mục tiêu rõ giúp việc khám phá có trọng tâm hơn.”

## SCENE 04 — THỬ MỘT CÁCH PHẢN HỒI KHÁC

**Nhịp VO:** khoảng 26 giây đọc ở 130 đơn vị/phút; 9 giây nghỉ/thao tác trong cảnh. Thu thử để căn nhịp thật; không kéo dài cảnh.

**Timeline:** 01:05–01:40 · **35 giây**. **Mục tiêu:** người xem thấy lựa chọn → phản hồi cụ thể.

**Route:** `http://localhost:5203/case-studies`. **Component:** CaseStudies.razor; case-card, case-options, case-feedback. **Điều kiện:** case chưa chọn. **Dữ liệu đã thấy:** Group Communication; “Your team member is not contributing. What do you do?”. Lựa chọn cần bấm: **“Talk to them privately to understand their situation.”**

| Mốc dựng | Thao tác/hình ảnh |
|---|---|
| 01:05–01:12 | Giữ câu hỏi và cả hai lựa chọn |
| 01:12–01:18 | Di chuột tới lựa chọn trao đổi riêng; dừng một nhịp |
| 01:18–01:22 | Bấm lựa chọn đó một lần; để phản hồi xuất hiện |
| 01:22–01:34 | Giữ “Lựa chọn tuyệt vời!” và “Direct, empathetic communication is key.” |
| 01:34–01:40 | Zoom nhẹ phần phản hồi, overlay dịch; không bấm Thử tình huống khác |

**Nhập liệu:** không. **Cuộn:** đủ thấy đáp án/phản hồi cùng ngữ cảnh, không lướt liên tục. **Zoom:** 110–120% sau khi người xem đã đọc câu hỏi. **VO:**

> Khi một thành viên chưa đóng góp, bạn sẽ làm thay hay trao đổi để hiểu nguyên nhân? Trong tình huống này, tôi chọn nói chuyện riêng với bạn ấy. Phản hồi hiện ra ngay, nhấn mạnh giao tiếp trực tiếp và thấu cảm. Đây là lời nhắc để suy nghĩ trước khi phản ứng.

**Overlay/subtitle:** đầu cảnh “Thành viên chưa đóng góp: bạn phản hồi thế nào?”; cuối cảnh “Giao tiếp trực tiếp và thấu cảm”. Chữ dịch chỉ là overlay hậu kỳ, không thay chữ UI. **Transition:** hard cut 01:40. **Trạng thái/bằng chứng:** A chọn/phản hồi R06–R07/S03; SelectOption chỉ gán state, phản hồi có sẵn, chưa xác minh lưu tiến độ. **Lưu ý:** UI tiếng Việt nhưng dữ liệu case tiếng Anh; không gọi phản hồi là AI.

**Thay thế:** LoadNext chọn case ngẫu nhiên. Nếu gặp case khác, không ghép câu hỏi cũ với đáp án mới. Chỉ dùng raw Group Communication đã quay thật. Nếu không có, thay trọn 35 giây bằng tài nguyên: 7 giây danh sách Tất cả → 7 giây lọc Quản lý thời gian → 10 giây đọc Bảng Ma trận Eisenhower → 11 giây giữ thẻ. VO: “Không chỉ giao tiếp, bạn còn có thể khám phá tài nguyên theo nhu cầu quản lý thời gian. Chọn kỹ năng, đọc tên và thời lượng, rồi quyết định nội dung muốn tìm hiểu tiếp. Video này chỉ minh họa cách duyệt danh mục; phần nội dung học cần kiểm tra riêng.” Đổi overlay thành “Khám phá tài nguyên theo mục tiêu”; chưa được gọi cảnh thay là practice đã làm xong.

## SCENE 05 — TÌM NGƯỜI ĐỒNG HÀNH PHÙ HỢP

**Nhịp VO:** khoảng 29 giây đọc ở 130 đơn vị/phút; 11 giây nghỉ/thao tác trong cảnh. Thu thử để căn nhịp thật; không kéo dài cảnh.

**Timeline:** 01:40–02:20 · **40 giây**. **Mục tiêu:** tìm theo kỹ năng, đọc chuyên môn/phạm vi hỗ trợ.

**Route:** `http://localhost:5203/mentors` → `http://localhost:5203/mentors/2`. **Component:** Mentors.razor; mentor-filters/list; MentorProfile.razor. **Điều kiện:** Tất cả, tìm kiếm trống. **Dữ liệu:** bộ lọc Giao tiếp; hồ sơ Tố Trâm. Runtime tên hiển thị **Th.S. Nguyễn Thuỵ Tố Trâm**, khác dấu ở landing; dùng đúng chữ đang hiện, không sửa website.

| Mốc dựng | Thao tác/hình ảnh |
|---|---|
| 01:40–01:47 | Mở tab D; giữ danh sách ba hồ sơ |
| 01:47–01:53 | Bấm “Giao tiếp”; giữ hồ sơ còn lại |
| 01:53–02:00 | Bấm “Xem hồ sơ” của Mentor Tố Trâm; chờ tiêu đề hiện |
| 02:00–02:11 | Giữ chuyên môn và “Mentor có thể hỗ trợ gì?” |
| 02:11–02:17 | Cuộn chậm xuống “Quy trình hỗ trợ”, thấy ba bước |
| 02:17–02:20 | Giữ bước “Mô tả mục tiêu học tập”; không gửi yêu cầu |

**Nhập liệu:** không nhập tên/email/câu hỏi. **Chuột:** bấm lọc rồi link; sau điều hướng đưa ra lề. **Cuộn:** khoảng 250–350 px khi cần, không làm mất tiêu đề ngay khi vừa mở. **Zoom:** danh sách 100%, hồ sơ crop 110%, giữ tên/chuyên môn làm ngữ cảnh. **VO:**

> Nếu cần thêm góc nhìn, bạn có thể tìm Mentor theo kỹ năng muốn phát triển. Tôi chọn giao tiếp và mở hồ sơ để xem chuyên môn, cùng cách chuẩn bị câu hỏi trước khi nhờ hỗ trợ. Biết rõ mình cần hỏi gì giúp bước tiếp theo cụ thể hơn. Video này chỉ trình diễn việc xem hồ sơ.

**Overlay:** “Tìm Mentor theo kỹ năng · Xem chuyên môn”. **Transition:** dissolve 8 frame nằm trong cuối cảnh, không tăng tổng. **Trạng thái/bằng chứng:** A lọc/xem R08–R10/S04; không xác minh yêu cầu, thông báo hay phản hồi thật. **Lưu ý:** hồ sơ công khai local không phải lời chứng thực hoặc buổi tư vấn đã diễn ra. Không hứa phản hồi ngay. **Thay thế:** nếu dữ liệu đổi, dùng raw đã quay đúng; hoặc chọn hồ sơ thực đang có, sửa tên/VO phù hợp. Không khẳng định một Mentor luôn sẵn sàng. Nếu không có hồ sơ tải được, dùng cảnh tài nguyên 40 giây với VO tương ứng và bỏ lời “tìm người đồng hành” ở closing.

## SCENE 06 — MANG ĐIỀU HỌC ĐƯỢC VÀO HÀNH ĐỘNG

**Nhịp VO:** khoảng 20 giây đọc ở 130 đơn vị/phút; 5 giây nghỉ/thao tác trong cảnh. Thu thử để căn nhịp thật; không kéo dài cảnh.

**Timeline:** 02:20–02:45 · **25 giây**. **Mục tiêu:** kết thúc trải nghiệm bằng việc chuẩn bị một tình huống/câu hỏi.

**Route:** `http://localhost:5203/community` → `http://localhost:5203/community/skills/1`. **Component:** MobileCommunity.razor, CommunitySkill.razor. **Điều kiện/dữ liệu:** phiên khách, link Giao tiếp có sẵn.

| Mốc dựng | Thao tác/hình ảnh |
|---|---|
| 02:20–02:25 | Giữ tiêu đề “Hướng dẫn học và kết nối Mentor”, mô tả phạm vi |
| 02:25–02:30 | Bấm thẻ “Giao tiếp” |
| 02:30–02:40 | Giữ ba dòng “Cách sử dụng nhóm này”: bài học, câu hỏi cụ thể, riêng tư |
| 02:40–02:45 | Cắt sang card “Trong buổi họp tới, bạn sẽ thay đổi điều gì?” |

**Nhập liệu:** không; không đăng bài hoặc gửi câu hỏi. **Cuộn:** 100–200 px nếu cần thấy ba dòng. **Chuột:** sau khi bấm thẻ, đưa ra lề. **Zoom:** 110% vào ba dòng, vẫn thấy ngữ cảnh kỹ năng. **VO:**

> Trước khi xin góp ý, hãy mang theo một tình huống và một câu hỏi cụ thể. Khu hướng dẫn kỹ năng nhắc bạn chuẩn bị bối cảnh và bảo vệ thông tin riêng tư. Bước tiếp theo của bạn sẽ là gì?

**Overlay:** “Một tình huống · Một câu hỏi · Một bước tiếp theo”. **Transition:** fade 8 frame trong card cuối. **Trạng thái/bằng chứng:** A xem hướng dẫn R11–R12/S05; community C tính năng xã hội. **Lưu ý:** 20 giây website + 5 giây card. Giữ mô tả chưa có bài đăng/bình luận nếu xuất hiện. Skill 1 hiện chưa có Mentor gắn kỹ năng; không dựng thành có người đang trao đổi. **Thay thế:** nếu link skill không tải, giữ phần chuẩn bị câu hỏi trên `/community` đủ 20 giây rồi card 5 giây; VO vẫn phù hợp.

## SCENE 07 — MỘT BƯỚC NHỎ HÔM NAY

**Nhịp VO:** khoảng 13 giây đọc ở 130 đơn vị/phút; 2 giây nghỉ/thao tác trong cảnh. Thu thử để căn nhịp thật; không kéo dài cảnh.

**Timeline:** 02:45–03:00 · **15 giây**. **Mục tiêu:** tổng hợp ba việc đã thấy và khép đúng thời lượng.

**Route/component:** end card hậu kỳ, logo lấy từ trang chủ; không điều hướng. **Dữ liệu:** logo, tagline, không số liệu doanh thu/người dùng/việc làm. **Điều kiện:** các cảnh 03–05 còn nguyên; nếu đã thay cảnh, sửa closing tương ứng.

| Mốc dựng | Hình ảnh |
|---|---|
| 02:45–02:51 | “Tìm nội dung · Thử phản hồi · Tìm người đồng hành” |
| 02:51–02:57 | Logo và “Bắt đầu từ một bước nhỏ” |
| 02:57–03:00 | Hold end card, hạ nhạc, kết thúc frame chính xác |

**Chuột/nhập/cuộn:** không có. **Zoom:** logo tĩnh, chữ lớn. **VO:**

> Tìm nội dung, thử cách phản hồi, rồi chọn người đồng hành. SoftSync Space — bắt đầu từ một bước nhỏ để phát triển kỹ năng mỗi ngày.

**Overlay:** “SoftSync Space · Bắt đầu từ một bước nhỏ”. **Transition:** fade âm thanh trong 2 giây cuối; không thêm đuôi video. **Trạng thái/bằng chứng:** thông điệp tổng hợp thao tác A, R03–R10; không tuyên bố hiệu quả học tập đo được. **Lưu ý:** không gắn localhost thành URL công khai hoặc QR tới domain đang lỗi. **Thay thế:** end card chỉ logo/tagline, không URL. Nếu case/Mentor bị thay, closing dùng “Chọn một mục tiêu, tìm nội dung phù hợp và chuẩn bị cho bước tiếp theo. SoftSync Space — bắt đầu từ một bước nhỏ mỗi ngày.”

## Phương án thay cảnh sau khi có điều kiện kiểm tra — KHÔNG phải bản đã xác minh

Giữ bản chính nếu chưa thỏa toàn bộ điều kiện: được cấp tài khoản test hợp lệ; môi trường test/demo được người phụ trách xác nhận tách production; được phép ghi dữ liệu test; UI và kết quả thực sự đã chạy; chỉ dữ liệu giả; ghi lại route/nhãn/kết quả thực tế và raw. Không dùng mật khẩu seed tự tìm, không tạo tài khoản production, không sửa hệ thống để quay. Bảng dưới là kế hoạch kiểm tra source-based **B/C**, phải cập nhật sau runtime; không đọc như bằng chứng thành công.

### O1 — Smart CV, thay trọn cảnh 05: 01:40–02:20, 40 giây

- Route đúng: **`/cv`**, CvWorkspace → CvCareerWorkspace, không trộn `/cv-review`. Chuẩn bị tệp TXT UTF-8 hoặc PDF có text, dưới 10 MiB theo source; tránh OCR để giảm rủi ro. Không dùng DOCX ở luồng này.
- Nội dung CV giả có thể chuẩn bị thành tệp bởi người quay: “Ứng viên Demo. Mục tiêu: thực tập sinh hỗ trợ dự án. Học vấn: sinh viên ngành kinh doanh. Dự án môn học: phối hợp nhóm bốn người, tổng hợp kế hoạch và trình bày kết quả. Kỹ năng: giao tiếp, làm việc nhóm, lập kế hoạch. Chưa có kinh nghiệm toàn thời gian.” Không số điện thoại, địa chỉ, email hoặc thông tin người thật. Đây là dữ liệu kiểm tra, không phải hồ sơ ứng viên thật.
- JD giả để dán: “Vị trí thực tập hỗ trợ dự án. Công việc gồm tổng hợp thông tin, phối hợp lịch nhóm và trình bày tiến độ. Yêu cầu giao tiếp rõ ràng, làm việc nhóm và lập kế hoạch. Chấp nhận sinh viên chưa có kinh nghiệm toàn thời gian.”
- Sau khi đăng nhập hợp lệ, mở `/cv`; từ màn hình chức năng chọn mục đánh giá/kiểm tra CV thực có. Nhãn menu đầu chưa được runtime quan sát, nên phải ghi lại nhãn thực trước khi quay. Mở kho bằng “Thay đổi” trong bước Chọn CV nếu màn hình đó xuất hiện; bấm **Tải lên CV**, chọn tệp giả. Upload có lưu dữ liệu nên chỉ thực hiện trong test đã được phép.
- Chọn CV từ kho, quay lại bước review; chọn **Dán văn bản**, dán JD vào **Mô tả công việc**; bấm **Phân tích ngay**. Quay raw liên tục cả trạng thái chờ và kết quả. Nếu lỗi/thiếu AI, không sửa code hoặc tạo kết quả giả; giữ cảnh Mentor chính.
- Dựng: 01:40–01:48 chọn CV (8s); 01:48–01:56 dán JD (8s); 01:56–02:02 bấm phân tích/trạng thái chờ (6s); 02:02–02:16 kết quả thật (14s); 02:16–02:20 hold đề xuất (4s). Cắt thời gian chờ dư, không cắt ghép kết quả của CV khác.
- Zoom kết quả có thật: Chất lượng CV/Phù hợp JD/đề xuất theo UI thực. Không đặt sẵn điểm. Không bấm Lưu nếu không cần. Chuột ở lề sau thao tác; cuộn chậm một lần; hard cut đúng 02:20.
- VO dự kiến: “Với một CV mẫu và mô tả công việc, bạn có thể xem những điểm cần chú ý trước khi hoàn thiện hồ sơ. Kết quả này đưa ra nhận xét và gợi ý để bạn cân nhắc, thay vì thay bạn quyết định. Hãy đọc từng đề xuất trong bối cảnh trải nghiệm thật của mình.” Chỉ dùng nếu kết quả đúng có nhận xét/gợi ý; sửa theo kết quả quan sát. Overlay “Gợi ý để hoàn thiện hồ sơ · Kết quả tham khảo”.
- Evidence source: S06; chưa có evidence runtime. Sau khi kiểm tra phải thêm ngày/môi trường/route/input/kết quả, trạng thái model và raw filename vào log file 01. Sửa closing thành “Tìm nội dung, thử phản hồi, rồi chuẩn bị hồ sơ cho bước tiếp theo.” Sửa file 05/06/07 đồng bộ. Thời lượng video vẫn 180s.

### O2 — Assessment/Roadmap, thay cảnh 04: 01:05–01:40, 35 giây

- B/C; có auth và learner gate. Dùng ID tài khoản test từ điều hướng thật, không đoán ID. Chuẩn bị hồ sơ/mục tiêu trong test được phép, hoàn thành bài assessment thật ngoài thời lượng dựng, không nộp bừa để tạo “điểm đẹp”.
- Mở `/assessment/{UserId:int}` qua luồng app; đọc câu hỏi/chọn đáp án theo hồ sơ demo nhất quán; nộp chỉ khi được phép trong test. Quan sát trang kết quả mà app điều hướng đến, sau đó mở `/roadmap`. Kiểm tra kết quả được lưu và lộ trình tải thật. Không quay `/#learning-journey` thay cho roadmap cá nhân.
- Dựng 01:05–01:13 một câu hỏi (8s); 01:13–01:20 thao tác chọn (7s); 01:20–01:31 kết quả thật cùng bài (11s); 01:31–01:40 roadmap thật (9s). Các bước dài quay riêng, giữ nguồn cùng tài khoản/lần làm bài. Nếu chưa có kết quả thật, giữ case study chính.
- VO dự kiến: “Từ câu trả lời của mình, bạn có thể xem kết quả đánh giá và những nội dung nên chú ý. Lộ trình giúp hình dung bước học tiếp theo. Hãy xem đây là thông tin hỗ trợ tự nhìn lại, rồi lựa chọn cách thực hành phù hợp.” Chỉ dùng nếu nội dung hiển thị đúng. Overlay “Đánh giá để chọn bước tiếp theo”. Zoom vào kết quả, không phóng đại AI.
- S07–S09: generator roadmap hiện là FakeAiRoadmapService theo quy tắc; assessment có scoring rubric và AI diễn giải/fallback. Không gọi toàn bộ lộ trình là LLM tạo. Nếu không xác minh được nguồn diễn giải, chỉ nói “gợi ý của hệ thống”. Cập nhật file 01/05/06 trước khi dựng.

## Kiểm tra trước xuất

- [ ] Mỗi lời dẫn trùng thao tác/kết quả đang xuất hiện; không trộn bản chính và O1/O2.
- [ ] Raw case giữ câu hỏi và phản hồi cùng take; không giả điểm AI/progress.
- [ ] Có 140 giây website trong bản chính; chữ dịch không che phản hồi.
- [ ] VO nghe tự nhiên; giữ khoảng nghỉ đúng file 05, không time-stretch giọng quá mức.
- [ ] Timeline không gap/overlap, 30 FPS cố định, 5.400 frame, tổng 180,000 giây.
- [ ] Frame cuối 00:02:59:29; không outro/black tail ngoài 03:00.
- [ ] Không lộ thông tin thật, token, DevTools, tài khoản, tab riêng tư.
- [ ] Không thêm URL/QR chưa kiểm tra. Không nêu báo cáo chưa đọc như bằng chứng.
