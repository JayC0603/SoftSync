# SoftSync — UI/UX learning platform

## Audit dựa trên source

- Course discovery trước đây nhúng tất cả video/transcript của mọi course; không có route lesson riêng.
- Teacher Studio là một form dài, yêu cầu nhập Skill ID và không có thao tác chọn sửa lesson/question dù service hỗ trợ.
- Navbar có nút search/bell không có handler và số thông báo 3 cố định.
- Dashboard hiển thị streak 4 ngày và tuần học cố định, không lấy từ dữ liệu. Progress lấy max skill không tương ứng nội dung đang học.
- `liquid-glass.css` được nạp sau CSS Vite; selector global có `!important`, ảnh hưởng bề mặt editor và account.
- Có nhiều page-local CSS. Không thể kết luận chất lượng layout/contrast thực tế chỉ bằng source.

## Hướng thiết kế và phạm vi đã triển khai

Canvas trung tính, surface đặc cho học tập/soạn bài, primary tím, border nhẹ, radius 8–12px, heading và khoảng cách dùng chung. Giữ glass ở các phần khác, Bootstrap và quy ước Tailwind `tw:`. Không thêm framework, package hay schema.

Shared components: `PageHeader`, `EmptyState`, `LoadingState`, `LessonMedia`.

- Learner: `/courses` là discovery; `/courses/{id}` là overview; `/courses/{id}/lessons/{lessonId}` chỉ nhúng video bài đang xem. Outline phân biệt hoàn thành/đang xem/chưa bắt đầu bằng chữ và icon. Transcript dùng native details. Previous/Next và link final quiz dùng route hiện có.
- Dashboard: continue course có tiến độ, next lesson, fallback roadmap/course; chỉ hiển thị course thật. Bỏ streak, weekly checks và thời lượng giả. Điểm XP và roadmap count không còn gọi là số liệu trong tuần.
- Teacher: ba phần Details/Lessons/Final quiz; course search, skill dropdown, chọn sửa lesson/question, preview lesson, AI draft tùy chọn, published quiz read-only. Không có delete/duplicate/drag-and-drop giả. Xác nhận bỏ nội dung chưa lưu khi đổi course/editor/phần hoặc rời trang. Save rõ ràng, không có autosave.
- Admin/CV/Quiz/History/Review/Analytics: sử dụng shared workspace surfaces, form/table/focus styles; giữ các handler và quyền hiện tại.
- Navbar: links chính và menu Explore, Teacher/Admin hiện theo role; bỏ các nút không hoạt động. Account dùng calmer surface; Login có label `for`/input `id`.
- Quiz progress phản ánh số câu đã trả lời, không nhầm vị trí hiện tại thành mức hoàn thành.

## Business/security không thay đổi

Presentation vẫn gọi BLL. Không thêm DAL/DbContext vào UI. Các route mới chỉ chọn course/lesson từ `GetPublishedAsync(authenticatedUserId)`. Writes vẫn dùng authenticated identity và service ownership hiện có. Student DTO không thêm đáp án đúng. Teacher phải xác nhận lại AI draft; grading/persistence không sửa. Không tạo migration, không thay provider/secret, không tác động database người dùng.

## Kiểm thử và giới hạn

`LearningUiTests`: semantic heading, escaped content, empty-state navigation, native video controls/caption/transcript, HTTP Identity learner routes, không nhúng video ở discovery, không hiển thị draft/lesson sai course, User không vào Teacher, Teacher không vào Admin. Regression suite cũ vẫn chạy; assertion câu mô tả Course cũ được thay bằng các assertion discovery phù hợp và chặt hơn.

Browser skill đã được đọc và thử bootstrap: không có browser khả dụng. Learner, Teacher course/lesson/quiz, Admin và mobile browser acceptance đều **NOT TESTED**. Không có screenshot, không xác nhận WCAG AA hoặc responsive bằng kiểm thử browser. HTTP SSR PASS không thay thế thao tác tương tác thực tế.

## Phần còn lại

- Đây là đợt nền tảng + luồng trọng tâm, chưa phải redesign hoàn chỉnh toàn bộ Presentation.
- Assessment, roadmap/AI lecturer, Assistant, Settings/Profile và Community còn page-local styles/interaction cần đợt kiểm tra tiếp; không tự thay luồng backend để phục vụ thiết kế.
- Cần browser QA ở 390px, 768px và 1440px: overflow, focus order, native confirmation, upload, save/edit, AI draft, video và submit quiz.
- Outline mobile hiện nằm trước lesson và dùng details do user tự đóng; chưa tự collapse theo viewport.
- Teacher chưa có inline validation đầy đủ từng field; các lỗi business vẫn trả qua thông báo. Không bịa persistence cho section/rich content/visibility chưa có trong model.
- Caption language vẫn `vi` theo convention cũ; model chưa có metadata ngôn ngữ caption.
- Vite còn cảnh báo asset `/images/background-softsync.png`; không liên quan grading/ownership, cần xác minh asset deployment.
- SDK Docker không có npm, nên MSBuild bỏ qua frontend với warning; gate chạy frontend riêng bằng Node 20. Warnings Identity form binding/ForwardedHeaders có từ trước.

Không commit/push. Không rebuild container web của người dùng; để thấy source mới cần build/deploy ứng dụng theo workflow hiện tại.
