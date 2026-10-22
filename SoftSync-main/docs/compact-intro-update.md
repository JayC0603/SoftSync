# Thu gọn đầu Trang chủ và sửa Mentor autoplay

Phạm vi: người dùng cho phép thiết kế lại phần đầu mobile vì nhiều chữ, quá lớn. Giữ vị trí các section, logo/icon, hình gốc, API, dữ liệu và chức năng điều hướng.

## Thay đổi
- Lời chào rút thành “Chào [tên]!” và một dòng mô tả; phong cảnh nhỏ, nhẹ bên phải.
- Mục tiêu hôm nay chỉ giữ hành động cần làm, bỏ tên bài bị lặp với hero và mô tả dài. CTA chi tiết giữ nguyên đích.
- Hero: tiêu đề 18px, bỏ dòng tên kỹ năng bị lặp, tiến độ ngắn hơn, chiều cao linh hoạt tối thiểu 184px, giữ hình và CTA.
- Header/padding/gap và bốn ô thống kê gọn hơn. Dữ liệu không đổi.
- Mentor: bỏ điều kiện phải nhìn thấy trọn cả dãy và hover tự dừng riêng trên Trang chủ mobile. Trượt khi vùng Mentor hiện trên màn hình; chạm/bấm, cuộn thủ công hoặc focus mới tạm dừng. Tôn trọng Reduce Motion.
- Danh sách đúng 2 Mentor trước đây không có overflow nên không thể lướt. Thêm một lượt bản sao của chính các card thật để cuộn vòng, ẩn bản sao khỏi cây accessibility và tab order, không tạo Mentor giả hoặc gọi thêm API. Kích thước card giữ nguyên. Không đổi behavior của carousel khác.

## File sản phẩm
- SoftSync.Presentation/Components/Shared/StrictMobileHome.razor
- SoftSync.Presentation/wwwroot/css/strict-mobile-home.css
- SoftSync.Presentation/wwwroot/js/design-system.js

## Kiểm tra
- .NET/Vite build thành công: 0 lỗi; 5 cảnh báo .NET hiện có.
- Playwright/Edge: 375/390/430, Light/Dark; không tràn ngang, title không đè CTA, mục tiêu khoảng 63px. 390/430 hero dữ liệu demo cao 184px, 375px khoảng 202px; số liệu nằm trong vùng đầu viewport 844px.
- Kiểm tra riêng đúng 2 Mentor bằng DOM trình duyệt: vẫn tự lướt khi mới hiển thị một phần và con trỏ hover; touch dừng; tới cuối vòng vẫn tiếp tục. Không ghi thay đổi vào database/service.
- Screenshot thực: screenshots/compact-intro/top-{light,dark}-{375,390,430}.png và full-{light,dark}-{375,390,430}.png. Ảnh full dùng viewport 844px nên nav fixed xuất hiện tại vị trí viewport đầu.
- Báo cáo máy: compact-intro-qa.json. Các ảnh top-long-content là fixture chữ dài thay trong DOM trình duyệt, không phải dữ liệu account thật.
- Chưa kiểm thử trên điện thoại vật lý hoặc xác nhận deploy production; thay đổi hiện ở workspace local.