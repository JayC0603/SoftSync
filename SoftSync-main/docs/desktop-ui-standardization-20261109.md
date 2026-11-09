Desktop UI standardization — 2026-11-09
=====================================

Các trang chức năng dùng chung thang chữ desktop: tiêu đề 24px, section 18px,
card 16px, body 14px, nội dung đọc 16px / line-height 1.7, caption tối thiểu 12px.
Các giá trị là `rem`, kế thừa font và bộ token mật độ hiện có trong
`compact-layout.css`. Không đổi root font-size hoặc dùng zoom/scale để thu nhỏ UI.
Token mới chỉ có hiệu lực từ breakpoint desktop hiện tại: 901px.

Thay đổi
--------

- Chuẩn hóa tiêu đề dashboard, học tập, khóa học, CV, tiến độ, hồ sơ, lịch,
  cài đặt, danh sách game, lịch sử làm bài và menu Thêm. Thay giá trị trong
  các quy tắc hiện có bằng token; không thêm stylesheet override hoặc thêm
  `!important` mới.
- Giảm khoảng đệm workspace về 24px, header tối thiểu 60px và cho phép tăng
  chiều cao theo nội dung; card chính dùng padding 16px, radius 12px.
- Trang học: section 23px → 18px, title 34px → 24px, giảm search/control,
  khoảng cách và chiều cao bản đồ minh họa. Giữ nguyên route/component dùng chung.
- Hồ sơ: title 38px → 24px; CV: 36px → 24px; tiến độ: 30px → 24px;
  lịch: 56px → 24px; cài đặt: 34.4px → 24px; lịch sử làm bài: 36px → 24px.
- Tiến độ và các nhãn desktop trong chatbot/cài đặt/bài học: nâng chữ 8–11px
  lên caption 12px. Giữ nguyên giá trị dự phòng trên mobile.
- Chatbot: chỉ chỉnh CSS của phần SYNCY đang có trong workspace; giữ nguyên
  markup, logic, lịch sử và các thay đổi người dùng đã thực hiện trước đó.
  Cỡ chữ tin nhắn là 16px trên desktop; phần nhập và cuộc hội thoại vẫn cuộn được.
- CV Review: thay `<main class="ss-workspace">` lồng trong workspace bằng
  `<section class="ss-cv-review">`, tránh lặp landmark main và cộng thêm lề sidebar.
  Vùng đọc kết quả có vai trò typography riêng. Không sửa logic upload/phân tích.
- Lịch: hero desktop 310px → 200px, toolbar xếp trên cùng một hàng khi đủ chỗ,
  giữ modal có cuộn và giới hạn theo viewport.
- Admin: giảm padding header/panel/bảng và KPI về tối đa 28px; giữ bảng cuộn
  trong vùng riêng. Không sửa dữ liệu, quyền hoặc các thao tác quản trị.
- Landing page giữ hero trong khoảng 42–56px trên desktop và thang typography riêng. Các chữ nhỏ bên trong
  hình mô phỏng sản phẩm không bị ép theo typography của form/dashboard.
- Không thêm nhãn sản phẩm mới; giữ nguyên cơ chế VI/EN trong LOCALIZATION_NOTES.md.

Kiểm chứng
----------

Chạy bằng Chrome headless trên hai bản ứng dụng demo localhost, database trong
bộ nhớ. AI bị tắt trong các tiến trình này; không dùng dữ liệu Production.

- Build Presentation và frontend Vite thành công, 0 lỗi. Còn 4 cảnh báo C# có sẵn
  cùng cảnh báo asset/chunk của Vite; không coi đây là lỗi đã sửa trong thay đổi UI.
- 32 test sẵn có về localization, settings persistence, schedule và CV history:
  đạt, không bỏ qua test nào trong nhóm này.
- Kiểm tra trang chính: 12 route × 8 viewport = 96 lượt, không phát hiện tràn ngang
  toàn trang hoặc pageerror JavaScript.
- Kiểm tra thêm 9 trang phụ và 7 trang quản trị × 8 viewport = 128 lượt, không có
  chuyển hướng ngoài dự kiến, tràn ngang toàn trang hoặc pageerror JavaScript.
  Tổng hai nhóm là 224 lượt kiểm tra trên 28 route.
- Các viewport: 1366×768, 1440×900, 1920×1080, 2560×1440, 900×900, 901×900,
  375×812 và 390×844. Nhóm trang phụ/admin kiểm tra cùng các chiều rộng; chiều
  cao 900px trên desktop và 844px trên hai viewport điện thoại.
- So sánh số đo heading/caption/header của nhóm trang chính trước/sau trên mobile
  và tại 900px: không thay đổi. Đây là kiểm tra số đo, không phải chứng nhận mọi
  trạng thái mobile giống từng pixel. Riêng sửa landmark CV là thay đổi cấu trúc có chủ đích.
- Thử mở/thu sidebar (mở rộng 224px), chuyển đủ 7 tab cài đặt, nhập JD tiếng Việt dài,
  mở/đóng modal lịch, truy cập bài học `/roadmap/lecture/1/1`.
- Thử VI → EN và theme tối trên dashboard, học tập, CV, cài đặt, lịch, chatbot.
- Zoom Chrome thật 200% trong profile riêng: cửa sổ 1440×900 cho viewport CSS
  720×450, DPR=2. Cả 12 route chính không tràn ngang; modal lịch nằm trong viewport,
  cuộn được và đóng bằng bàn phím. Không dùng CSS zoom để giả lập.
- Kiểm tra thêm cả 7 route quản trị ở zoom Chrome 200%: không tràn ngang toàn trang
  hoặc pageerror JavaScript.
- `git diff --check` đạt.

Ảnh và số đo
------------

Artefact kiểm tra được lưu cục bộ trong `.tools/desktop-system-20261109/` ở root
workspace (thư mục bị Git ignore). Các liên kết dưới đây dùng trong workspace này.

- [Mở so sánh ảnh trước/sau](../../.tools/desktop-system-20261109/comparison.html).
- [Số đo trước](../../.tools/desktop-system-20261109/before-results.json),
  [số đo sau](../../.tools/desktop-system-20261109/after-results.json).
- [Kiểm tra trang phụ/admin](../../.tools/desktop-system-20261109/extra-after.json).
- [Tương tác](../../.tools/desktop-system-20261109/interactions.json),
  [zoom Chrome 200%](../../.tools/desktop-system-20261109/zoom200.json).
- [Zoom 200% của admin](../../.tools/desktop-system-20261109/admin-zoom200.json).

| Trang, viewport 1440×900 | Trước | Sau |
| --- | --- | --- |
| Học tập | [Ảnh](../../.tools/desktop-system-20261109/before-1440-roadmap.png) | [Ảnh](../../.tools/desktop-system-20261109/after-1440-roadmap.png) |
| Cài đặt | [Ảnh](../../.tools/desktop-system-20261109/before-1440-settings.png) | [Ảnh](../../.tools/desktop-system-20261109/after-1440-settings.png) |
| Chatbot | [Ảnh](../../.tools/desktop-system-20261109/before-1440-assistant.png) | [Ảnh](../../.tools/desktop-system-20261109/after-1440-assistant.png) |
| Lịch | [Ảnh](../../.tools/desktop-system-20261109/before-1440-schedule.png) | [Ảnh](../../.tools/desktop-system-20261109/after-1440-schedule.png) |
| CV Review | [Ảnh](../../.tools/desktop-system-20261109/before-1440-cv-review.png) | [Ảnh](../../.tools/desktop-system-20261109/after-1440-cv-review.png) |

Giới hạn
--------

- Chưa kiểm tra màn hình kết quả CV có dữ liệu AI thật trong lần thay đổi UI này;
  các lần thử form không gửi yêu cầu phân tích. Chưa kiểm tra đầy đủ mọi nhánh
  loading/error và mọi dữ liệu dài có thể có trên Production.
- Trang học đã mở được với dữ liệu demo; không coi ảnh player đang chờ media là
  bằng chứng phát video, hoàn thành bài học hoặc mở khóa các bước tiếp theo.
- Đã kiểm tra trang quản trị bằng tài khoản demo riêng, không thao tác cấp quyền,
  gửi thông báo hoặc thay API key. Chưa kiểm tra dashboard giảng viên với khóa học
  đang xuất bản và tất cả loại màn hình game/quiz ở trạng thái đang chơi.
- Kết quả là kiểm tra cục bộ; chưa triển khai hoặc xác nhận thay đổi UI trên tên miền.
