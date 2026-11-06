# Đồng bộ VI/EN

## Cơ chế

- Giữ `LocalizationService` scoped theo circuit và `Translations` hiện có.
- `UiTextCatalog` bổ sung cặp EN/VI cho nhãn, trạng thái, lỗi Course/Quiz/Teacher/Admin/CV,
  accessibility text và validation. Chỉ dùng `Ui.Text()` với chuỗi giao diện hệ thống.
- Các trang được bổ sung `LocalizedComponentBase`; đăng ký `OnChanged` và hủy khi dispose.
- Navigation và Settings dùng cùng service. `ssLang.set` lưu cookie/localStorage và cập nhật
  `html.lang`; SSR lấy cookie `ss-lang`. Không thay `CurrentCulture` toàn process.
- Chrome tương tác riêng để skip-link và error notice cũng cập nhật khi đổi ngôn ngữ,
  không làm MainLayout nhận một RenderFragment xuyên render-mode boundary.
- Form tài khoản giữ DataAnnotations/Identity rules; component validation chỉ dịch đầu ra,
  không đổi validation hoặc policy. Identity errors dùng code, không đưa chi tiết tài khoản vào UI.

## AI và dữ liệu

- AI Quiz draft/explanation/feedback nhận `language=vi|en` từ service hiện tại.
- Quiz review xóa cache phản hồi tạm khi đổi ngôn ngữ; response cũ đến muộn không ghi đè UI mới.
  Không tự gọi lại AI và không sửa điểm/đáp án chuẩn.
- CV upload/reanalysis nhận ngôn ngữ từ UI. `Language` là snapshot do server đặt trong
  `ResultJson`; kết quả cũ mặc định `vi`, không cần migration.
- CV history/reload giữ ngôn ngữ gốc và hiển thị nhãn. Chọn “Phân tích lại” để tạo kết quả mới
  theo ngôn ngữ đang chọn; mô tả tuyển dụng và CV gốc không bị dịch hay sửa.
- Tutor request có `Language`; prompt và fallback nhất quán. Assistant/assessment dùng cơ chế
  bilingual hiện có; roleplay giữ tham số ngôn ngữ hiện có.
- Không tự dịch nội dung Teacher/user nhập, tiêu đề khóa học, captions/transcript, CV/JD,
  chat history hay kết quả AI đã lưu. Đó là dữ liệu gốc, không phải lỗi catalog.
- Không gọi Gemini thật hoặc dịch API tự động. Provider vẫn cần tuân thủ prompt;
  tests mock không chứng minh mọi response thật luôn đúng ngôn ngữ.

## Kiểm tra

Chạy `scripts/Test-CvAcceptance.ps1`: restore/build/test bằng .NET 10, PostgreSQL runtime cô lập,
actual Identity/HTTP, EF model check, frontend build và `git diff --check`.
Tests gồm catalog hai chiều, user isolation, component re-render/disposal, validation state,
AI payload language, Tutor fallback, CV snapshot/reanalysis và HTTP cookie/reload EN → VI.
Browser tương tác và AI thật cần kiểm tra riêng; không tự chạy API có phí.

Thêm chuỗi mới: dùng key trong `Translations` hoặc cặp đầy đủ trong `UiTextCatalog`,
không hardcode label trong Razor và không dùng DOM text replacement để dịch toàn trang.
