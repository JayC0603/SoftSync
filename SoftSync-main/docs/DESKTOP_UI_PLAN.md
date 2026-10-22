# SoftSync — Kế hoạch giao diện desktop

Ngày: 22/10/2026. Trạng thái: đề xuất để chủ dự án duyệt và giao Luna triển khai; chưa thay đổi giao diện.

## Mục tiêu và phạm vi

Thiết kế trải nghiệm laptop/máy tính cho SoftSync, tận dụng diện tích ngang, giúp người học thấy bài đang học, tiến độ và thao tác tiếp theo rõ ràng. Giữ nhận diện SoftSync, mascot SYNCY, tiếng Việt/English, light/dark và toàn bộ quy tắc học tập hiện có.

- Bảo toàn giao diện và tương tác hiện tại ở viewport ≤900 CSS px, gồm tablet thuộc vùng này.
- Desktop bắt đầu từ 901px, theo ranh giới hiện có của Learn. Không đổi breakpoint mobile để tiện làm desktop.
- Làm trên `SoftSync.Presentation` (Blazor Server .NET 10). Không chuyển sang Next.js, không thêm framework, thư viện UI hoặc API.
- Phạm vi chính: không gian người học. Các trang admin/teacher chỉ kiểm tra tương thích với shell dùng chung, chưa thiết kế lại nghiệp vụ của chúng.
- Đây là kế hoạch thiết kế và bàn giao; Luna chỉ bắt đầu code sau khi chủ dự án duyệt phạm vi.

## Kết quả khảo sát mã nguồn

1. `Components/Layout/MainLayout.razor` đã có `NavBar`, topbar, `HeaderActions`, vùng nội dung và footer. Có thể nâng cấp khung hiện có.
2. `Components/Pages/Dashboard.razor` tải user/roadmap rồi render `MobileHome` ở mọi kích thước.
3. `Components/Shared/MobileHome.razor` có cây `.mobile-home.mh-v3` và render thêm `StrictMobileHome`. `strict-mobile-home.css` bật `.sh-home` và ẩn `.mh-v3` ở ≤900px. Vì vậy tên MobileHome không có nghĩa toàn bộ component chỉ dùng trên điện thoại.
4. `RoadmapOverview.razor` đã có `.learn-desktop` và nhánh `.learn-v3` cho mobile; `learning-journey.css` được tải với media từ 901px. Tái sử dụng nhánh desktop này.
5. `App.razor` tải nhiều lớp CSS, gồm `design-system`, `dashboard-reference`, `compact-layout`, `mobile-home`, `strict-mobile-home`, `navigation-glass`. Nhiều selector có specificity cao và `!important`; thay CSS global dễ ảnh hưởng mobile.
6. Homepage công khai dùng `PublicLayout`, tách với workspace. Không nhầm homepage `/` với dashboard `/dashboard`.

Khảo sát này dựa trên mã nguồn. Cần chụp baseline mới ở thời điểm Luna bắt đầu; không dùng ảnh tài liệu cũ thay bằng chứng hiện trạng.

## Các hướng và lựa chọn đề xuất

| Hướng | Ưu điểm | Đánh đổi |
|---|---|---|
| A. Mở rộng nhánh desktop hiện có bằng CSS được giới hạn viewport — đề xuất | Giữ data/state/route hiện có; ít rủi ro mobile; giao theo từng màn hình | Cần kiểm tra cascade kỹ |
| B. Tạo bộ component desktop riêng cho mọi trang | Tự do bố cục | Hai cây UI có thể query hai lần, trùng ID, lệch state và tăng chi phí bảo trì |
| C. Xây lại desktop bằng Next.js | Tách giao diện mạnh | Tăng phạm vi auth/API/migration, không phù hợp nhiệm vụ này |

Chọn A. Chỉ tách một component trình bày desktop khi DOM hiện có thực sự không thể đáp ứng. Không thêm cây UI thứ hai chỉ để thay spacing. Không refactor logic của mobile trong cùng đợt.

## Ngôn ngữ thiết kế

- Giữ tím SoftSync làm accent, nền sáng hơi xanh/tím, mặt nội dung rõ và ít nhiễu. Dark mode lấy màu từ hệ token theme hiện có.
- Giữ logo, font và asset hiện có; SYNCY xuất hiện ở bài đang học/hỗ trợ, không lặp thành minh họa lớn ở mọi card.
- Khoảng cách theo nhịp 8px; panel padding 24px, gap 24px ở màn hình lớn và 16px ở laptop nhỏ. Bo góc panel 16–20px, control 10–12px.
- Body 16px, nội dung phụ 14px, tiêu đề trang 28–32px. Nội dung bài đọc giới hạn khoảng 65–75 ký tự/dòng.
- Một CTA chính trong từng khu vực: Tiếp tục học, Bắt đầu đánh giá hoặc Xem lại tùy dữ liệu thực tế. Không tạo thêm CTA cùng độ nổi bật cạnh nhau.
- Hover nhẹ, focus rõ, control cao tối thiểu 40px; dùng được bằng bàn phím. Không dùng hover làm cách duy nhất mở thông tin.
- Không thêm streak, badge, lịch học, phần trăm kỹ năng hoặc thống kê nếu DTO hiện tại không cung cấp dữ liệu đáng tin cậy.

## Khung desktop

| Viewport CSS px | Sidebar | Nội dung |
|---|---|---|
| ≤900 | Giữ nguyên | Giữ nguyên mobile hiện tại |
| 901–1199 | Thanh icon 80px, nhãn truy cập được bằng focus/tooltip | Một cột chính; panel phụ xuống dưới; padding 20px |
| 1200–1439 | Sidebar 224px có icon và nhãn | Hai cột khi đủ chỗ, cột phụ 280px; padding 24px |
| ≥1440 | Sidebar 240px | Hai cột chính/phụ 320px; padding 32px |

Vùng nội dung tối đa 1440px, căn giữa trong phần còn lại sau sidebar; main dùng `min-width:0`. Topbar sticky cao 64px; chỉ một scrollbar chính ở trang thông thường. Chat và bài học có thể có vùng cuộn riêng khi kiểm tra được bàn phím/focus.

Sidebar tái sử dụng điều hướng và phân quyền từ `NavBar`: nhóm Học tập, Công cụ, Kết nối; active state rõ. Không hiện link chỉ dành cho admin/teacher cho học viên. Topbar dùng `WorkspaceTitle` và `HeaderActions` hiện có, giữ chuông, ngôn ngữ, theme và menu tài khoản. Tìm kiếm học tập giữ ở dashboard với phạm vi dữ liệu hiện có; chưa đặt ô tìm kiếm toàn hệ thống khi chưa có backend tương ứng.

## Dashboard mục tiêu

Wireframe desktop ≥1200px, không phải ảnh giao diện đã triển khai:

```text
┌──────────────┬─────────────────────────────────────────────────┐
│ Logo         │ Tên trang            Chuông · VI/EN · User      │
│              ├─────────────────────────────────────────────────┤
│ Tổng quan    │ Chào [tên]             Tìm trong bài học         │
│ Học tập      ├───────────────────────────────┬─────────────────┤
│ Tiến độ      │ BÀI HỌC HIỆN TẠI              │ Mục tiêu hôm nay│
│              │ Tên · mô tả · tiến độ         │ + Hỏi SYNCY     │
│ Trợ lý AI    │ [Tiếp tục học]       SYNCY    │                 │
│ CV           ├───────────────────────────────┴─────────────────┤
│ Tài nguyên   │ Bài hoàn thành · Hoạt động · XP · Cấp độ         │
│              ├───────────────────────────────┬─────────────────┤
│ Mentor       │ Hành trình phát triển kỹ năng │ Gợi ý bài học   │
│ Cộng đồng    │ Truy cập nhanh                │ Thành quả gần đây│
│              ├───────────────────────────────┴─────────────────┤
│              │ Đội ngũ cố vấn · Footer gọn                      │
└──────────────┴─────────────────────────────────────────────────┘
```

Ánh xạ vào `.mh-v3`: `.mh-welcome`, `.mh-search`, `.mh-hero`, `.mh-goal`, `.mh-syncy`, `.mh-stats`, `.mh-roadmap`, `.mh-quick`, `.mh-recommendations`, `.mh-achievements`, `.mh-mentors`. Luna xác nhận selector tồn tại trước khi viết CSS; không tạo khối dữ liệu chỉ để lấp wireframe.

- Dùng grid cho từng nhóm nội dung; thứ tự đọc/focus phải hợp lý. Không đặt vị trí bằng `absolute` cho card nội dung.
- Hero giới hạn chiều cao theo nội dung, minh họa khoảng 25–30% diện tích; không phóng ảnh điện thoại ra toàn màn hình.
- Bốn số liệu lấy trực tiếp từ user/roadmap. Rỗng thì hiện trạng thái bắt đầu đánh giá; không dùng dữ liệu mẫu.
- Đường dẫn tiếp tục giữ `JourneyProgress.ResumeStep`, quyền truy cập và trạng thái khóa. Học xong chuyển về hành động xem thành quả/xem lại.
- Mentor giữ nguồn dữ liệu và tương tác carousel hiện có ở vòng đầu; tối ưu kích thước card và điều khiển desktop, không thêm auto-play mới.

## Thiết kế các màn hình tiếp theo

| Trang | Bố cục desktop | Luồng phải giữ |
|---|---|---|
| `/roadmap` | Chọn kỹ năng phía trên; bản đồ lớn ở cột chính; bài/hoạt động được chọn ở cột phụ | Khóa/mở bài, chọn kỹ năng, resume step, dữ liệu progress |
| `/roadmap/lecture/{UserId}/{ItemId}` | Nội dung/video chính và mục lục hoạt động phụ; vùng bài đọc có chiều rộng tối đa | Video/quiz/roleplay/reflection, lưu tiến độ, điều kiện hoàn thành |
| `/progress` | Hàng số liệu, biểu đồ rộng, lịch sử bên dưới; panel phụ chỉ khi có dữ liệu | Cách tính điểm/tiến độ hiện tại, dữ liệu lịch sử thật |
| `/assistant` | Lịch sử 220–240px bên trong workspace; hội thoại rộng vừa đọc; input ở đáy vùng chat | Tạo/mở phiên, gửi tin, pending/error/retry, không nhân đôi request AI |
| `/cv` | Điều khiển và kết quả thành hai khu vực khi đủ rộng; tài liệu/kết quả không bị co quá nhỏ | Upload/download, CSRF, auth và dịch vụ CV hiện có |
| `/resources`, `/mentors`, `/community`, `/courses` | Toolbar rõ; danh sách 2–3 cột tùy chiều rộng; card có cấu trúc đồng nhất | Filter/search/đường dẫn/quyền đang có; không thêm bộ lọc giả |
| `/profile`, `/settings` | Menu mục bên trái, form bên phải, chiều rộng form vừa đọc | Validation, save, đổi mật khẩu và xác nhận thao tác tài khoản |
| Homepage `/`, Account | Hero desktop hai cột; form đăng nhập chiều rộng 420–480px | Static SSR của Account, OAuth, nội dung/SEO và mobile giữ nguyên |

Các trang còn lại như game, assessment, case studies kiểm tra tương thích shell trước; chỉ bổ sung layout desktop trong vòng riêng nếu cần. Không gộp toàn bộ chúng vào lần code đầu.

## Rào chắn kỹ thuật cho Luna

1. Thêm `wwwroot/css/desktop-workspace.css`; toàn bộ rule, biến và pseudo-element mới nằm trong `@media (min-width:901px)`. Các breakpoint 1200/1440 lồng trong vùng desktop hoặc có điều kiện desktop tương đương.
2. Tải stylesheet qua `App.razor` sau các CSS workspace hiện tại, dùng `VersionedAsset` và `media="(min-width: 901px)"`. Bảo vệ cả bằng media của link lẫn media trong file.
3. Scope shell vào `.page`/`.ss-workspace`; scope từng trang vào root cụ thể (`.mh-v3`, `.learn-desktop`, `.pg-page`, `.assistant-layout`). Không viết rule mới chỉ có `body`, `h1`, `.container`, `.btn` hoặc `*` rồi áp dụng cho toàn site.
4. Không chỉnh `mobile-*.css`, `strict-mobile-home.css`, `learn-v3.css`, `StrictMobileHome.razor` hay component `Mobile*` của Journey để đạt mục tiêu desktop.
5. Không sửa hoặc xóa CSS desktop cũ trong vòng đầu. Ghi lại rule đang thắng cascade và ghi đè có giới hạn. Không tăng chuỗi `!important` vô hạn.
6. Vòng đầu ưu tiên CSS trên DOM hiện có; không đổi C# loader, repository, DI, API, route hay DTO. Nếu cần hook markup thì chỉ thêm class mang tên rõ và phải chứng minh không đổi mobile.
7. Không thêm query trong layout. Scoped DbContext theo circuit có thể lỗi khi layout và trang query đồng thời; nếu thật sự cần nguồn dữ liệu bổ sung, tách scope và duyệt riêng thay đổi đó.
8. Không render thêm bản desktop và mobile có cùng `id`, form, iframe hoặc control đang chạy song song. Nếu phải tách trình bày thì dùng state/data chung và kế hoạch lifecycle rõ trước khi code.
9. Giữ localization hiện có. Chuỗi mới dùng key EN/VI duy nhất trong `Translations.cs`; không chuyển đổi toàn bộ chuỗi cũ trong đợt này.
10. Giữ Account static SSR, auth/role check và cơ chế antiforgery. Không sửa backend để phục vụ yêu cầu bố cục.
11. Không đổi cơ chế nhận diện mobile, không sniff user-agent để chọn giao diện, không tạo URL `/desktop` riêng.

## Kế hoạch triển khai theo từng gói

### Gói 0 — Baseline và bản thiết kế để duyệt

- [ ] Chụp hiện trạng cùng tài khoản/dữ liệu/theme/ngôn ngữ tại 375, 390, 430, 768, 900, 901, 1024, 1366, 1440, 1920px.
- [ ] Ghi rõ commit hoặc bản sao nguồn baseline, viewport/DPR, browser, zoom; dùng dữ liệu fixture cố định khi so sánh ảnh, che thời gian/nội dung động nếu cần.
- [ ] Kiểm tra CSS computed của sidebar, topbar, main, dashboard và hai nhánh Learn; lập danh sách selector bị ghi đè.
- [ ] Dựng bản nháp desktop dashboard và Learn ở 1366/1440 để chủ dự án duyệt trước khi nhân sang các màn hình.
- [ ] Ghi nhận trạng thái demo và phần dịch vụ AI/CV nào chưa cấu hình; không coi lỗi dịch vụ là lỗi bố cục.

### Gói 1 — Shell và dashboard, phạm vi lần code đầu được đề xuất

Files: tạo `wwwroot/css/desktop-workspace.css`; sửa duy nhất link stylesheet trong `Components/App.razor`. Chỉ thêm class ở `MainLayout.razor`/`MobileHome.razor` nếu cần hook và đã được rà soát.

- [ ] Hoàn thiện sidebar 80/224/240px, topbar 64px và content width theo bảng breakpoint.
- [ ] Dàn `.mh-v3` theo wireframe, dùng dữ liệu và handler hiện có.
- [ ] Kiểm tra loading/error/empty, học dở, hoàn thành toàn bộ, tên dài, EN/VI và dark/light.
- [ ] Xác nhận menu tài khoản, dropdown ngôn ngữ, chuông không bị cắt bởi `overflow` và keyboard focus không rơi vào nhánh ẩn.
- [ ] So sánh ảnh mobile với baseline, build/test, bàn giao screenshot trước/sau và danh sách file sửa.

Điểm duyệt: chỉ làm tiếp khi shell/dashboard desktop được duyệt và mobile không thay đổi ngoài sai khác động đã ghi nhận.

### Gói 2 — Learn và bài học

Files: thêm `wwwroot/css/desktop-learning.css`; đăng ký qua `App.razor` với media desktop. Đối chiếu `Pages/RoadmapOverview.razor`, `Pages/RoadmapLecture.razor`, `Components/Journey/*` để đặt selector; chỉ sửa nhánh desktop nếu cần.

- [ ] Hoàn thiện `.learn-desktop`, panel chọn bài và nội dung bài học; không chạm `.learn-v3`.
- [ ] Kiểm tra ba kỹ năng active, checkpoint khóa, completed/review, bài tiếp theo và resume step.
- [ ] Kiểm tra video, quiz và phản hồi dài không tràn cột; nút lưu/tiếp tục luôn truy cập được.
- [ ] Đổi viewport qua 900/901 khi đang học không reset bước, tạo request trùng hay làm mất dữ liệu nhập.

### Gói 3 — Progress, Assistant và CV

Files: thêm `wwwroot/css/desktop-tools.css`; đối chiếu `Pages/Progress.razor`, `Shared/ProgressDashboard.razor`, `Pages/Assistant.razor`, `Pages/CvWorkspace.razor`, `Shared/CvCareerWorkspace.razor`.

- [ ] Tối ưu chart/history cho desktop, giữ phép tính và trạng thái rỗng hiện tại.
- [ ] Tối ưu chat history, hội thoại và input; kiểm tra gửi một lần, mở phiên và lỗi AI.
- [ ] Dàn khu vực CV từ DOM hiện tại, kiểm tra tên file dài, bảng kết quả và dịch vụ unavailable.
- [ ] Chạy regression mobile và bàn giao riêng từng nhóm màn hình.

### Gói 4 — Trang phụ và public

- [ ] Hoàn thiện resources/mentor/community/courses theo grid desktop; profile/settings theo menu và form.
- [ ] Thiết kế homepage và Account desktop trong stylesheet public riêng với cùng media guard; không để selector workspace tác động public layout.
- [ ] Rà soát game/assessment/case studies và admin/teacher: sửa lỗi bố cục do shell nếu có, không mở rộng nghiệp vụ.
- [ ] Chạy đầy đủ ma trận nghiệm thu và cập nhật tài liệu file/ảnh.

## Tiêu chí nghiệm thu bắt buộc

- Mobile 375/390/430px, tablet 768px và ranh 900px: bố cục, kích thước, thứ tự, dock/footer/menu và luồng thao tác giữ nguyên so với baseline cố định.
- Desktop 901/1024/1366/1440/1920px: không có cuộn ngang toàn trang, chồng lấn, nội dung bị cắt hay khoảng trống lớn do grid thiếu section điều kiện.
- Trạng thái chưa đánh giá, chưa có roadmap, học dở, khóa bài, hoàn thành, mentor tải lỗi và AI/CV unavailable đều có cách tiếp tục rõ ràng.
- Light/dark, VI/EN, tên tiếng Việt dài, tên file dài, thông báo lỗi nhiều dòng, zoom 200%, Tab/Shift+Tab/Enter/Escape và reduced motion được kiểm tra.
- Không có hai request cho một thao tác, hai h1 hiển thị, ID trùng mới hay focus vào nhánh đang ẩn.
- Không đổi luồng đăng nhập, role, khóa/mở bài, lưu tiến độ hoặc đường dẫn resume.
- `dotnet build SoftSync.sln` thành công; `dotnet test SoftSync.Tests/SoftSync.Tests.csproj` không có test thất bại. Chạy build Vite và đọc kết quả vì target hiện dùng `ContinueOnError`; .NET build thành công chưa chứng minh CSS bundle đã build thành công.
- Lệnh frontend từ thư mục `SoftSync.Presentation`: `npm run build`.
- Không thêm unit test chỉ để xác nhận class CSS tồn tại; ưu tiên kiểm tra trình duyệt/screenshot và test luồng hiện có. Nếu thay logic thì bổ sung test hành vi tương ứng.
- Giao danh sách file thay đổi, ảnh before/after, ma trận viewport, kết quả lệnh và các kiểm tra chưa thực hiện. Không khẳng định “mobile không đổi” chỉ từ build/test C#.

## Prompt bàn giao Luna — lần 1

> Đọc CLAUDE.md và docs/DESKTOP_UI_PLAN.md. Chỉ thực hiện Gói 0 và Gói 1 sau khi tôi duyệt hướng thiết kế. Dự án chính là SoftSync.Presentation Blazor Server, không làm trên SoftSync.Next. Mục tiêu là shell và dashboard desktop theo kế hoạch. Mobile/tablet ≤900px phải giữ nguyên; mọi CSS mới chỉ hoạt động từ 901px. Tái sử dụng DOM desktop, dữ liệu, route và handler hiện có, không sửa backend hoặc component/CSS mobile. Tạo baseline trước, đưa bản nháp desktop dashboard để duyệt, rồi mới code. Sau triển khai, kiểm tra light/dark, VI/EN, trạng thái dữ liệu và screenshot regression mobile; chạy .NET build/test cùng Vite build. Báo cáo bằng chứng, file thay đổi và phần chưa kiểm tra. Dừng ở Gói 1 để tôi xem trước, chưa triển khai các gói sau.
