Mobile responsiveness — 2026-11-09
================================

Nút Quay lại
------------

- Đưa `MobileMoreBackLink` ra khỏi thanh trên, đặt ở đầu nội dung trang trong
  `.ss-page-back`. Giữ điều hướng về Thêm/trang cha và các nhãn VI/EN.
- Không thêm nút vào thanh điều hướng dưới. Thanh trên không còn chứa nút Quay lại.
- Ở màn hình tối đa 360px, dùng logo có accessible name SoftSync, ẩn phần chữ
  thương hiệu để không đè lên các nút thông báo/ngôn ngữ/tài khoản.

Điểm nghẽn đo được
-----------------

Chrome mô phỏng mobile 390×844. Chạy cả kết nối bình thường và mạng giới hạn:
latency 150ms, tải xuống 200.000 byte/giây (1,6 Mbps), tải lên 100.000 byte/giây,
CPU throttling 4×. Chạy với bộ nhớ đệm trình duyệt mới cho mỗi cấu hình.

Trang Mentor trên tên miền đang dùng ba PNG 2048×2048, tổng 8.416.078 byte để
hiển thị các ảnh đại diện 88–112px. Lần đo mạng giới hạn trên tên miền mất
47,7 giây để tải hết tài nguyên, tổng transfer khoảng 9,03 MB. TTFB khoảng
150ms trong lần đo này: tải ảnh lớn là một điểm nghẽn riêng, không thể quy toàn bộ
thời gian đó cho database hoặc độ trễ xử lý nút.

Một lần thử kết nối bình thường trên tên miền không thấy bộ lọc phản hồi trong
15 giây sau lần bấm đầu. Các lần sau và kiểm tra cục bộ có phản hồi. Vì chưa
triển khai bản mới, chưa kết luận đã xử lý toàn bộ vấn đề kết nối/hydration trên
Production. Blazor Server vẫn phụ thuộc vào kết nối mạng khi xử lý sự kiện.

Phần `LearnerAccessState` đã có cache trong phiên và chỉ nạp lại sau invalidation;
giữ nguyên logic này, không thêm cache hoặc thay đổi chính sách truy cập.

Chuyển tab và mở bản đồ
----------------------

Người dùng xác nhận chậm nhất khi chuyển tab/mở trang. Kiểm tra thêm năm tab chính
với tài khoản demo cục bộ, cả mạng bình thường và cấu hình giới hạn ở trên:

- Chuyển tab vẫn dùng cùng kết nối Blazor; không phát sinh document request hay
  WebSocket mới. Không có bằng chứng các tab đang reload toàn bộ trang.
- Lần đầu vào Học trước sửa phải nạp module bản đồ khoảng 1.066 KB. Một số yêu cầu
  metadata/ảnh bản đồ còn xuất hiện khi đã chuyển sang Tiến độ/Thêm.
- Tách `vietnam-map.js` thành entry 3,82 KB và engine tải động 1.066,31 KB. Entry
  giữ stylesheet và thao tác sheet; engine chỉ được yêu cầu sau khi canvas vào
  viewport, chờ 200ms và một lượt idle. Mở bản đồ rõ ràng sẽ bỏ qua lượt chờ này.
- Hủy lịch khi canvas ra khỏi viewport; khi DOM bị gỡ thì hủy lịch, observer và
  map. Import đã bắt đầu không thể hủy, nhưng kết quả trả về muộn sẽ không khởi
  tạo WebGL hoặc fetch địa danh cho trang đã rời đi.
- Sửa trạng thái khi component được dùng lại từ preview sang `map=full`: đọc lại
  chế độ khi `Compact` thay đổi và đồng bộ tương tác với trạng thái mở rộng.

Đo từ lúc bấm đến khi nội dung route xuất hiện, mỗi lượt đi qua chín lần chuyển
tab: Home, Học, Tiến độ, CV, Thêm rồi lặp lại một số tab. Lần đầu mở Học trên mạng
bình thường từ 608ms xuống 109ms; tab Tiến độ ngay sau đó từ 188ms xuống 84ms.
Các tab sau sửa ở mạng giới hạn nằm trong 133–312ms. Riêng Học ở mạng giới hạn
không cải thiện ổn định (trước 203–204ms, sau 211–239ms). Đây là các mẫu đo nhỏ,
không phải benchmark thống kê hoặc cam kết tốc độ Production; dữ liệu demo dùng
database trong bộ nhớ nên chưa phản ánh thời gian truy vấn Neon thực tế.

Ba kịch bản trình duyệt đều đạt: rời tab trước lượt tải engine; rời tab trong khi
engine đang tải rồi quay lại; mở đầy đủ/phóng to/xem mốc/Escape/khôi phục focus/vào
bài học. 18 lần chuyển tab sau sửa không có document request, WebSocket mới hoặc
pageerror; không thấy yêu cầu metadata/ảnh từ map khởi tạo muộn sau khi rời tab.

Thay đổi ảnh hiển thị
--------------------

- Tạo bản WebP giữ nguyên nội dung và transparency: Mentor/SYNCY tối đa 512px,
  logo tối đa 128px; quality 0.86. Giữ nguyên các PNG gốc.
- `DisplayImages.Url` chỉ ánh xạ những ảnh hệ thống đã biết sang bản nhẹ.
  Không thay URL lưu trong database, API, ảnh upload hoặc hồ sơ Mentor.
- Dùng bản hiển thị tại các layout, danh sách/hồ sơ Mentor, trang chủ, chatbot
  và các component đang dùng những ảnh này. Metadata Open Graph/JSON-LD và ảnh
  gốc được giữ nguyên.
- Các file WebP có trong static web asset manifest của bản build.

| Ảnh | PNG gốc | WebP dùng hiển thị |
| --- | ---: | ---: |
| Nguyễn Hoàng Vân Anh | 2.749.719 byte | 27.912 byte |
| Nguyễn Thuỵ Tố Trâm | 2.932.833 byte | 29.864 byte |
| Nguyễn Xuân | 2.733.526 byte | 23.860 byte |
| SYNCY | 927.528 byte | 34.476 byte |
| Logo | 89.713 byte | 3.506 byte |

Ba ảnh Mentor giảm khoảng 99%; tổng năm ảnh từ 9,43 MB xuống 0,12 MB.

Đo trước/sau cục bộ, cùng điều kiện mạng giới hạn
----------------------------------------------

| Trang | Tải hết tài nguyên trước | Sau | Transfer trước | Sau |
| --- | ---: | ---: | ---: | ---: |
| Mentor | 46,9s | 4,74s | 9,03 MB | 0,608 MB |
| Giới thiệu | 5,30s | 0,62s | 0,931 MB | 0,035 MB |

Trang Giới thiệu được mở sau Mentor trong cùng browser context, nên có dùng lại
cache CSS/JS. Thứ tự giống nhau ở hai lượt đo. Đây là thời gian `load`, không
phải LCP/INP hay cam kết thời gian phản hồi của mọi nút. Số đo mô phỏng có nhiễu;
chưa đo bản tối ưu trên điện thoại vật lý hoặc sau deploy lên tên miền.

Kiểm chứng
----------

- Build .NET/Vite thành công; 70 test điều hướng, bản dịch, Mentor và hành trình
  học đạt trên bản cuối. Bốn cảnh báo C# hiện có vẫn còn.
- 170 lượt kiểm tra đi/về các mục Thêm, trang con, reload và VI/EN ở 320, 390,
  900, 901, 1366, 1440px. Nút nằm trong nội dung, không ở thanh trên; không tràn
  ngang trang/header hoặc chồng chữ logo lên control.
- 26 lượt kiểm tra danh sách/hồ sơ Mentor, nơi công tác, tìm kiếm và VI/EN sau
  đổi ảnh. Không gửi yêu cầu hỗ trợ cho Mentor.
- Không phát hiện pageerror JavaScript ở các lượt kiểm tra trên.
- `git diff --check` đạt. Không đổi API, dữ liệu hoặc cấu hình deployment.
- Không gọi chatbot, CV AI hoặc OCR trong các lần đo này.

Artefact cục bộ (Git ignore) tại `.tools/mobile-performance-20261109/`:
`before-live.json`, `before-local.json`, `after-local.json`, `image-sizes.json`,
`before-navigation.json`, `after-navigation.json`, `map-lifecycle.json`.
Các ảnh kiểm tra nút và kết quả tại `.tools/page-back-20261109/`;
kiểm tra hồ sơ Mentor tại `.tools/mentor-biographies-20261109/`.
