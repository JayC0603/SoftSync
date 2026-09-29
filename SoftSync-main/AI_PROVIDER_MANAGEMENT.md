# Quản lý AI provider

## Sử dụng

Đăng nhập Admin → **Cấu hình AI** (`/admin/ai-configuration`). Thêm provider, tên,
model, endpoint chính thức và key. **Test kết nối** không lưu dữ liệu; có thể phát sinh
phí rất nhỏ. **Lưu cấu hình**, rồi **Kích hoạt**. Để trống key khi sửa sẽ giữ key đã lưu.
Đổi loại provider phải nhập key mới. Có thể đặt model nhẹ riêng cho CV; để trống dùng model chính.
Không tự publish nội dung, không thay chấm điểm hoặc ownership.

Triển khai lần đầu tính năng cần build/deploy để chạy migration additive
`20260929015853_AddAiProviderConfigurations`. Sau đó các thay đổi qua Admin không cần
restart/rebuild. `DbInitializer` tiếp tục áp dụng migration theo convention hiện tại;
không reset DB. Backup database trước triển khai như thông thường.

## Luồng runtime

```
AI feature → HuggingFaceJsonClient → IAiProviderConfigurationResolver
    → enabled default PostgreSQL row → decrypt on server → request snapshot
    → official OpenAI-compatible chat transport → existing parse/validation/fallback
```

Gemini là provider hiện tại; Hugging Face và OpenAI dùng transport compatible tại endpoint
chính thức. Khả năng JSON/temperature tùy model; Test kết nối chỉ kiểm tra minimal chat,
không chứng minh model hỗ trợ tất cả prompt/structured output. Không đổi prompt nghiệp vụ.
Roadmap vẫn dùng implementation local hiện có, không bị biến thành AI remote.

Resolver đọc một row không tracking **mỗi logical AI call**, không đọc từng token/stream,
không giữ cache config/key xuyên request. Vì không cache, save/enable/disable/set active
có hiệu lực trên request tiếp theo ở mọi app instance, không có stale cache cần invalidate.
Request đang chạy dùng snapshot cũ. CV lưu đúng model của snapshot đó.
Assistant bỏ response cache theo circuit để key/model mới không bị bỏ qua.

Ưu tiên: enabled default DB → options/environment khi chưa có enabled default.
Environment không seed/ghi đè DB. Lỗi giải mã, 401, 429 hay timeout của active DB config
không chuyển provider/key tự động: dùng controlled error/local fallback của từng feature.
DB outage không được ngầm đổi credentials. Disable active bị chặn; kích hoạt provider khác trước.

## Security

- Admin page policy + service `IAiAdminAccess` đọc principal và kiểm tra role trong DB
  mỗi operation; không nhận userId/isAdmin do browser khai báo.
- Key bảo vệ bằng Data Protection purpose `SoftSync.AiProvider.ApiKey.v1`.
  `AiProviderDto` chỉ chứa `HasApiKey`, không chứa ciphertext/key giải mã.
- Input `NewApiKey` là write-only; chỉ lưu protected value. Test draft không lưu key.
  Form bỏ key sau Save/clear/dispose. Browser chỉ biết key nó vừa nhập, không lấy lại key gốc.
- Endpoint UI chỉ chấp nhận official base URL theo enum; chặn private/local hosts,
  query/fragment/userinfo/custom proxy. Redirect HTTP bị tắt để tránh gửi key sai host.
  Environment fallback vẫn là trusted deployment configuration.
- Key chỉ nằm trong Authorization header. Named client tắt HTTP logging mặc định;
  không log exception/provider body. Kết quả Test là status an toàn, không trả stack trace.
- GUID revision chống stale edit; transaction advisory lock PostgreSQL serialize Set Active;
  partial unique index đảm bảo một default, CHECK đảm bảo default phải enabled.
- CreatedAt/UpdatedAt/UpdatedByUserId theo server; không ghi secret vào audit.

## Data Protection và deployment

**Đã có sẵn:** `EfDataProtectionKeyRepository` lưu key ring vào bảng `DataProtectionKeys`
trong PostgreSQL, application name `SoftSync`. Không phụ thuộc filesystem/container layer.
API config và key ring sống cùng persistent database; restart/redeploy với cùng DB đọc lại
key được. Docker Compose phải giữ volume PostgreSQL; không dùng `down -v`, không xóa key ring.
Backup/restore **cả** config và DataProtectionKeys. Không thay application name/purpose.

Key ring hiện có có thể chưa được mã hóa riêng at rest: kẻ truy cập toàn DB có thể giải mã
secret. Production cần TLS, quyền DB tối thiểu, encrypted backups/storage và bảo vệ key ring.
Đã thêm tùy chọn `DataProtection__CertificatePath` + `DataProtection__CertificatePassword`:
mount certificate PFX bên ngoài repo và cung cấp password bằng secret manager. Giữ certificate
(private key) qua restart/redeploy; backup an toàn. Cert bảo vệ keys mới được sinh;
keys cũ có thể còn plaintext, cần kế hoạch rotation/retention có kiểm thử, không tự xóa keys.
Không commit PFX/master keys. Mất DB key ring/certificate có thể mất khả năng giải mã.

## Verification

- `scripts/Test-CvAcceptance.ps1`: restore/build/test, startup, PostgreSQL migrations,
  HTTP Identity/CV regression, model consistency, npm build và diff check.
- `scripts/Test-AiProviderRestart.ps1`: hai process/container độc lập write → read,
  cùng DB/key ring, xác minh decrypt và outbound request giả lập sau restart.
- Tests dùng synthetic keys, isolated PostgreSQL schemas và mocked provider transport.
  Không gửi key thật/CV thật ra mạng.
- Browser acceptance và live provider acceptance phải báo riêng, không suy từ unit tests.
