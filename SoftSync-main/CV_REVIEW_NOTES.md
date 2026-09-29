# CV Review

Route `/cv-review` yêu cầu đăng nhập. Không có endpoint download hoặc lưu file gốc.

Luồng: Razor → `ICvReviewService` → validation/extraction → `ICvAnalysisService`
→ `HuggingFaceJsonClient` hiện có → schema validation/normalization → EF Core → PostgreSQL.

## Configuration

- `CvReview:MaxUploadBytes` (environment: `CvReview__MaxUploadBytes`): mặc định 5 MB; giới hạn cấu hình 1 KB–20 MB.
- Reuse `AiApi:Enabled`, `AiApi:ApiKey`, `AiApi:BaseUrl`, `AiApi:Model`, `AiApi:TimeoutSeconds`.
- API key đặt trong environment/user secrets, không commit vào source.
- Không có key, timeout, HTTP lỗi, JSON thiếu/null/sai hoặc domain validation fail: báo lỗi có kiểm soát; không lưu successful analysis.

## Database & privacy

Migration `20260928060751_AddCvReview` chỉ thêm `CvDocument`, `CvAnalysis`, FK và indexes.
Startup hiện có của repository apply migrations; cần review migration và backup theo quy trình deployment hiện tại trước deploy.
Không reset DB. Không thay migration cũ. Ownership của analysis được lấy qua document, không duplicate UserId.

Mỗi thao tác lấy authenticated user trên server; query read/re-analysis/delete luôn có ownership predicate.
Context được tạo riêng cho từng operation, không dùng circuit-scoped context để giữ CV.
Original file chỉ đọc trong memory. Text và kết quả lưu trong database riêng tư; không nằm trong `wwwroot`.
Xóa document xóa mọi analyses; backup và retention phía AI provider cần policy riêng khi vận hành production.
UI báo rõ việc gửi văn bản đến provider. Không log text hoặc parser/provider exception detail của CV.

PDF dùng PdfPig đã có trong repo. DOCX dùng ZipArchive + XmlReader, DTD bị cấm,
giới hạn file/XML/text/page để giảm rủi ro tài nguyên. Không OCR; PDF scan cần chuyển sang PDF có text/DOCX.
PDF parsing đồng bộ có cancellation giữa các trang, không thể interrupt từng bước nội bộ parser.

AI feedback là untrusted advisory content, render bằng Razor escaped text, không raw HTML/Markdown.
Prompt tách CV khỏi system instructions; không thể đảm bảo LLM luôn đúng hoặc chống mọi prompt injection.
Không ATS score/ranking/auto-rewrite/job matching.

## Verification

`CvReviewTests` dùng fake HTTP response và EF InMemory, không gọi paid provider.
Kiểm tra extraction PDF/DOCX, schema, failure, cancellation, ownership, reload, re-analysis và delete.
Production AI output quality và browser keyboard/screen-reader vẫn cần acceptance riêng.

## Chạy acceptance trước khi có API key

Từ solution root `SoftSync-main`, Docker Desktop phải đang chạy:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-CvAcceptance.ps1
```

Script tạo PostgreSQL 17 **tạm, cô lập**, không expose cổng PostgreSQL, apply migrations thực,
build, chạy toàn bộ tests với PostgreSQL/HTTP runtime acceptance, kiểm tra model consistency,
build frontend và `git diff --check`. Chỉ dừng/xóa hai container có tên random do script tạo.
Không sử dụng database/volumes dữ liệu hiện có. NuGet cache được giữ để lần sau nhanh hơn.
Hai real-provider tests luôn skip nếu chưa opt-in; không có fake AI service đăng ký vào app.

Fixtures synthetic được tạo tại `SoftSync.Tests/bin/Debug/net10.0/Fixtures`:
`english.pdf`, `vietnamese.docx`, `long.pdf`, `image-only.pdf`, `near-limit.docx`, `above-limit.docx`.
PDF English dùng một cột; assertions kiểm tra từng heading, nội dung và thứ tự, không chỉ length.
Vietnamese DOCX kiểm tra dấu đầy đủ qua extraction và PostgreSQL. PDF nhiều cột có thể có thứ tự đọc khác;
chưa chứng minh hỗ trợ mọi layout/font PDF tiếng Việt. Image-only PDF không có text layer:
**OCR required for scanned CV support**, hiện trả lỗi có kiểm soát, không gọi AI.

Tests runtime HTTP đăng ký/login hai user thật qua Identity, kiểm tra `/cv-review`, history,
result detail/reload và route đổi ID của user khác. Upload/re-analysis/delete được acceptance ở
application service + PostgreSQL thật, dùng response giả lập tại HTTP boundary.
Đây **không phải Browser E2E**: chọn file, loading/click/re-analysis/delete trong browser cần chạy checklist bên dưới.

## API contract đã kiểm tra

### Đối chiếu CV với mô tả tuyển dụng

Ô mô tả tuyển dụng trên `/cv-review` không bắt buộc, tối đa 8.000 ký tự (kiểm tra cả server).
AI nhận CV và JD dưới dạng user data không tin cậy; kết quả phải có bảng đối chiếu
`RequirementMatches`: requirement, `Met | Partial | NotDemonstrated`, evidence, recommendation.
Không kết luận tuyển dụng hay suy luận thuộc tính nhạy cảm. Thiếu bảng/invalid status bị từ chối,
không lưu thành công giả. Server lưu JD gốc cùng bảng trong `ResultJson` hiện có, không migration.
History/reload giữ nguyên JD và kết quả; “Phân tích lại” dùng JD từ analysis mới nhất của CV thuộc owner.
Muốn đối chiếu với JD khác, nhập JD mới và upload lại. CV cũ không có JD vẫn đọc được.

CV Review đánh giá nội dung, không chỉ scan/extract: đánh giá tổng quan có lý do,
điểm mạnh/yếu gắn bằng chứng, feedback từng phần và cách sửa theo ưu tiên.
Có thể gợi ý viết lại bullet bằng dữ kiện hiện có; số liệu thiếu phải là placeholder,
không bịa thành tích. Không suy luận thiết kế trực quan từ extracted text, không chấm ATS.
Kết quả cũ giữ nguyên; dùng “Phân tích lại” để nhận đánh giá theo prompt mới.

- Provider: Gemini API (OpenAI-compatible), base URL mặc định `https://generativelanguage.googleapis.com/v1beta/openai/`.
- POST `chat/completions` tương đối với base URL trên; authentication `Authorization: Bearer <API key>` chỉ ở server. Không thêm `/v1` vào endpoint.
- Model mặc định `gemini-3.8-flash`; có thể override bằng `AiApi__Model` theo model được cấp cho project Google AI Studio.
- Riêng CV Review dùng model nhẹ `gemini-3.1-flash-lite`, override bằng `AiApi__CvModel` (`AiApi:CvModel` trong user secrets). Model gửi lên API và metadata lưu trong analysis dùng chung resolver; không đổi model của Assistant/Assessment. Không tự retry sang model đắt hơn.
- Request: `model`, `temperature: 0.15`, `response_format: { type: "json_object" }`, system/user messages tách biệt.
- Response: `choices[0].message.content` chứa JSON; `finish_reason=length/content_filter` bị từ chối cho CV.
- Timeout lấy `AiApi:TimeoutSeconds`, mặc định 45 giây. Không infinite retries; HTTP 429/5xx/timeout báo lỗi an toàn.
- JSON có fences/whitespace được xử lý. Thiếu optional lists dùng `[]`; explicit null bị từ chối.
  Summary, Suggestions và Sections vẫn bắt buộc, domain validation trước persistence.
- Readiness chỉ thông báo configured/not configured, **không chứng minh token hợp lệ hay model online**.

Tài liệu đối chiếu: [Gemini OpenAI compatibility](https://ai.google.dev/gemini-api/docs/openai).
Tên class `HuggingFaceJsonClient` được giữ để không thay đổi wiring/consumers hiện có; transport thực tế dùng cấu hình Gemini chung. Không thay schema, scoring, persistence hoặc ownership.
`OpenReadStream(UploadOptions.MaxBytes)` truyền giới hạn explicit, không dùng default 500 KB.
Không tăng SignalR message size lên 5 MB: stream được truyền theo chunk; browser transport near-limit chưa được E2E xác minh.
Tham khảo [Blazor file uploads](https://learn.microsoft.com/en-us/aspnet/core/blazor/file-uploads?view=aspnetcore-10.0).

## Cấu hình key an toàn sau này

Configuration key chính xác: **`AiApi:ApiKey`**; environment variable **`AiApi__ApiKey`**.
App đọc environment và Development user secrets hiện có. Compose đã forward `AiApi__ApiKey`
vào web container; user secrets trên máy host không tự xuất hiện trong Docker container.
Không gửi key vào chat hoặc commit `.env`/appsettings.

Local .NET SDK 10 (Development):

```powershell
dotnet user-secrets set "AiApi:ApiKey" "<API_KEY>" --project SoftSync.Presentation
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project SoftSync.Presentation
```

Lệnh chứa key literal có thể vào shell history. Để tránh gõ key vào command history khi dùng environment:

```powershell
$cvSecureKey = Read-Host "Gemini API key (Google AI Studio)" -AsSecureString
$env:AiApi__ApiKey = [System.Net.NetworkCredential]::new("", $cvSecureKey).Password
$env:AiApi__BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/"
$env:AiApi__Model = "gemini-3.8-flash"
$env:AiApi__CvModel = "gemini-3.1-flash-lite"
$env:AiApi__Enabled = "true"
# Không echo biến này. Secret chỉ truyền vào app/container phía server.
docker compose up -d --build web
```

Compose vẫn cần các secrets PostgreSQL/pgAdmin/bootstrap theo cấu hình hiện có; không reset DB.
Lấy key tại https://aistudio.google.com/api-keys. Nếu `.env`/user secrets còn BaseUrl hoặc Model Hugging Face, cập nhật các giá trị non-secret đó; environment override appsettings. Không dùng token Hugging Face cho Gemini.
`AiApi__Enabled=true` mặc định; nếu trước đó đặt false, cần bật lại. Các giá trị non-secret có thể override
qua `AiApi__BaseUrl`, `AiApi__Model`, `AiApi__TimeoutSeconds`.

## Gate AI thật (không chạy mặc định)

Trên máy có SDK10 và key đã đặt ở environment hoặc user secrets:

```powershell
$env:SOFTSYNC_CV_REAL_AI_ACCEPTANCE = "1"
dotnet test SoftSync.Tests --filter "Category=RealAi"
Remove-Item Env:SOFTSYNC_CV_REAL_AI_ACCEPTANCE
```

Hai test English/Vietnamese dùng **HttpClient thật**, không mock. Chỉ synthetic CV được gửi.
Test này có thể phát sinh phí; không tự chạy khi chỉ thêm key nếu chưa bật opt-in.
Không được kết luận production-ready chỉ từ parser mocks hoặc readiness=true.

## Browser checklist khi có browser

Login A → `/cv-review` → chọn các fixtures → loading → kết quả/lỗi có kiểm soát → reload URL
`/cv-review/{AnalysisId}` → history → mở kết quả → phân tích lại → xác nhận xóa.
Login B → đổi URL sang AnalysisId của A phải không xem được kết quả.
Thiếu key: trang vẫn mở, extraction hợp lệ dẫn tới thông báo AI chưa cấu hình, không lưu success.
Với key thật: lặp lại English + Vietnamese, xác minh persisted results trong UI, không chỉ API contract.
Browser E2E chưa được chạy nếu không có browser runtime kết nối.
