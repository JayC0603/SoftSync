# CLAUDE.md — Hướng dẫn context cho SoftSync

> File này giúp Claude nắm nhanh dự án mà không phải đọc lại toàn bộ code. Cập nhật khi kiến trúc thay đổi.

## Tổng quan
SoftSync — nền tảng học kỹ năng mềm bằng AI. **Blazor Server (.NET 10, `net10.0`)**, kiến trúc 3 lớp. UI song ngữ EN/VI. Bootstrap + Tailwind v4 (prefix `tw:`) + hệ liquid glass tự dựng.

## Kiến trúc (4 project)
| Project | Vai trò | File chính |
| --- | --- | --- |
| `SoftSync.Common` | DTO + Enum dùng chung | `Dtos/SoftSyncDtos.cs`, `Enums/CommonEnums.cs` |
| `SoftSync.DAL` | EF Core, DbContext, Entities, Repository, Migrations | `Data/SoftSyncDbContext.cs`, `Entities/Entities.cs`, `Repositories/Repositories.cs` |
| `SoftSync.BLL` | Service + Interface (gồm AI + Auth) | `Interfaces/IServices.cs`, `Services/BusinessServices.cs`, `Services/Fake/FakeServices.cs`, `Auth/*` |
| `SoftSync.Presentation` | Blazor Server UI + Vite/Tailwind | `Program.cs`, `Components/**`, `Services/LocalizationService.cs` + `Translations.cs` |

## Quy ước quan trọng
- **Tailwind luôn có prefix `tw:`** (vì hybrid với Bootstrap). Layout/spacing/typography → Tailwind (`tw:flex`, `tw:p-6`). Component + màu → Bootstrap (`btn`, `badge`, `bg-*`). Icon → `bi bi-*`.
- Thang đo khác nhau: Bootstrap `p-4`=1.5rem, Tailwind `tw:p-4`=1rem. Đổi bậc khi chuyển.
- **Localization**: mọi chuỗi UI qua `L["key"]`; định nghĩa cặp (En, Vi) trong `Services/Translations.cs`. Trang kế thừa `LocalizedComponentBase` để tự re-render khi đổi ngôn ngữ.
- AI services đang là **Fake*** (`Services/Fake/FakeServices.cs`).
- DB: SQL Server `localhost\JAYC`, connection string trong `appsettings.json` (`SoftSyncDb`).

## Lệnh hay dùng
- `dotnet` không có sẵn trong PATH của bash → prefix: `export PATH="/c/Program Files/dotnet:$PATH"`.
- Build: `dotnet build SoftSync.sln`
- Chạy: `dotnet run --project SoftSync.Presentation` (tự chạy `npm install` + `npm run build`). URL: https://localhost:7212 , http://localhost:5104
- Migration: `dotnet ef migrations add <Name> -p SoftSync.DAL -s SoftSync.Presentation` rồi `dotnet ef database update ...`

## Navbar + Avatar + Level (verify OK)
- Thanh nav tách ra component riêng [Components/Layout/NavBar.razor](SoftSync.Presentation/Components/Layout/NavBar.razor) **có `@rendermode InteractiveServer`** — vì `MainLayout` render static SSR nên toggle ngôn ngữ (`@onclick` + JS `ssLang`) phải nằm trong component interactive này. Nav: pill item + icon, search, chuông có badge, user chip (avatar + Level N + tên) có dropdown menu.
- `ApplicationUser` có `AvatarUrl` + `ExperiencePoints`. Level tính từ XP qua [SoftSync.Common/LevelSystem.cs](SoftSync.Common/LevelSystem.cs) (ngưỡng lũy tiến 100·N; `UserDto.Level`/`LevelProgressPercent` là computed, không lưu DB).
- Cộng XP: hoàn thành assessment +50 (`AssessmentService.SubmitAssessmentAsync`), hoàn thành roadmap item +30 (`RoadmapService.MarkCompleteAsync`, chỉ cộng 1 lần khi chuyển sang completed).
- Upload avatar ở [ProfileSetup.razor](SoftSync.Presentation/Components/Pages/ProfileSetup.razor): `InputFile` → lưu `wwwroot/uploads/avatars/`, max 3MB, chỉ nhận image; `IUserService.SetAvatarAsync`.
- `IUserService` thêm: `SetAvatarAsync`, `AddExperienceAsync`. Demo user seed XP=320 (~Level 3); `DbInitializer` backfill XP nếu user demo cũ đang =0.
- Trang hồ sơ [Components/Pages/Profile.razor](SoftSync.Presentation/Components/Pages/Profile.razor) (`/profile`, `[Authorize]`): avatar + đổi ảnh, level badge, **thanh XP** (`ss-xp-bar`/`ss-xp-fill` theo `LevelProgressPercent`), 3 ô thống kê (số bài đánh giá, kỹ năng theo dõi, cấp hiện tại), thông tin tài khoản + nút sang `/profile-setup` để sửa. NavBar dropdown có thanh XP mini + link `/profile` (Hồ sơ) và `/profile-setup` (Chỉnh sửa).

## SYNCY onboarding wizard + Gender + dropdown — XONG (verify OK)
- Common: enum `Gender {Unspecified, Male, Female}`. `ApplicationUser.Gender` + `UserDto.Gender` + map trong `UserService`. Migration `AddGender` đã áp DB.
- [Services/Mascot.cs](SoftSync.Presentation/Services/Mascot.cs): `Mascot.ImageFor(Gender)` → `/images/syncy-male.png` | `/images/syncy-female.png` (Unspecified→male); `Mascot.Emoji`="🐟" fallback (img `onerror` hiện emoji).
- **ProfileSetup.razor** = wizard 5 bước (`[Authorize]`, `@rendermode InteractiveServer`): greeting SYNCY → About you (nhóm tuổi + chọn giới tính → set mascot) → Goal → Skills → "Start My Journey" → `/assessment/{userId}`. Lưu profile qua `UpdateProfileAsync` + skills qua `AddSkillSelectionsAsync`. (Upload avatar giờ chỉ ở trang `/profile`, không còn trong wizard.)
- **NavBar.razor** dropdown: My Profile(`/profile`) / My Roadmap(`/roadmap`) / Progress(`/progress`) / Settings(`/profile-setup`) / divider / Log out.
- Roadmap.razor thêm route rỗng `/roadmap` + fallback lấy id từ auth state (giống Progress/Assistant).
- Localization: `wizard.*`, `nav.myProfile/myRoadmap/settings` EN/VI.
- **BÀI HỌC QUAN TRỌNG**: `SoftSyncDbContext` scoped theo circuit → NavBar (layout) và trang KHÔNG được cùng query DbContext đồng thời trong lúc init (crash "A second operation was started on this context"). Fix: NavBar load profile trong **scope DI riêng** (`IServiceScopeFactory.CreateAsyncScope`) tại `OnAfterRenderAsync`. Áp dụng pattern này cho mọi component layout cần đọc DB.
- **NGƯỜI DÙNG cần lưu 2 ảnh** `syncy-male.png` + `syncy-female.png` vào `wwwroot/images/` (chưa có, code fallback emoji 🐟).
## Logout modal + Welcome animation — XONG (verify OK)
- `Scripts/main.js`: `window.ssWelcome.get()/clear()` (đọc/xóa cookie `ss-welcome`), `window.ssSubmitForm(id)` (submit form logout). Nhớ: main.js đi qua Vite bundle → build sinh `wwwroot/dist/app.js`.
- **NavBar.razor**: modal xác nhận logout. Nút Logout `type=button` → `OnLogoutClicked`: nếu URL ở `assessment`/`select-skills`/`profile-setup` (SensitivePaths) thì mở modal "Leave your learning journey?", không thì submit ngay form `#ss-logout-form` (qua `ssSubmitForm`). CSS `ss-modal-backdrop`/`ss-modal` + keyframes.
- [Components/Account/WelcomeCookie.cs](SoftSync.Presentation/Components/Account/WelcomeCookie.cs): `WelcomeCookie.Set(HttpContext, tên)` — cookie `ss-welcome` MaxAge 15s, HttpOnly=false (JS đọc được). Gọi sau sign-in ở Login/LoginWithOtp/Register/ExternalLogin (2 chỗ).
- [Components/Layout/WelcomeToast.razor](SoftSync.Presentation/Components/Layout/WelcomeToast.razor) (interactive, trong MainLayout): `OnAfterRenderAsync` đọc `ssWelcome.get()` → hiện toast SYNCY chào mừng (animation bounce + slide) → `ssWelcome.clear()` → tự ẩn ~4s. Tôn trọng `prefers-reduced-motion`.
- Localization: `logout.confirm.*`, `welcome.*`.

## Quiz assessment — 8 câu/kỹ năng, SONG NGỮ (verify OK)
- Bộ câu hỏi thật ở [SoftSync.DAL/Data/QuizSeedData.cs](SoftSync.DAL/Data/QuizSeedData.cs): **8 câu/kỹ năng × 7 kỹ năng = 56 câu**, mỗi câu 4 lựa chọn có `ScoreValue` 1–4 (1 kém … 4 lý tưởng). ID suy diễn ổn định: `questionId = skillId*100 + idx`, `optionId = questionId*10 + idx` → `HasData` không đổi giữa các lần chạy. `QuestionsPerSkill=8`, `MaxOptionScore=4`.
- **Mỗi câu & lựa chọn đều song ngữ**: record `Q`/`Opt` mang `(Text, TextVi, Score)`; helper `MC/Sc/O` nhận cả 2 ngôn ngữ. Entity `AssessmentQuestion.QuestionTextVi` + `AssessmentOption.OptionTextVi` (rỗng → fallback tiếng Anh).
- **Bộ câu tiếng Việt thật** (do người dùng cung cấp, gradient A→D = score 1→4) map: PHẦN 1 Quản lý thời gian & trì hoãn (câu 1–8) → skill 3; PHẦN 2 Giao tiếp & lắng nghe (câu 9–16) → skill 1; PHẦN 3 Tư duy phản biện & giải quyết vấn đề (câu 17–24) → **toàn bộ skill 4** (theo quyết định người dùng). Skill 4 tiếng Anh là bản dịch từ VI. Skill 2/5/6/7 giữ tiếng Anh gốc + thêm bản dịch VI.
- `SoftSyncDbContext.SeedData`: gọi `QuizSeedData.Questions()`/`.Options()`. Migration mới nhất `AddBilingualQuiz` (thêm 2 cột `*TextVi` + refresh HasData) đã áp DB — verify: 56 câu, 224 option, 0 dòng Vi rỗng, nvarchar (DATALENGTH=2×LEN).
- **Chọn ngôn ngữ ở tầng UI**: `AssessmentQuestionDto`/`AssessmentOptionDto` (BLL) mang cả `*Text` + `*TextVi` + `SkillNameVi`; `AssessmentService` điền cả 2, tên kỹ năng VI qua map `SkillNameVi(skillId)` (Skill entity chưa có cột VI). [Assessment.razor](SoftSync.Presentation/Components/Pages/Assessment.razor) có helper `QuestionLabel/OptionLabel/SkillLabel` chọn theo `L.Current` → **đổi ngôn ngữ giữa chừng quiz re-render tức thì** (kế thừa `LocalizedComponentBase`), không cần reload/round-trip DB.
- **Chấm điểm thật** (bỏ FakeAI): `AssessmentService.GetAssessmentQuestionsAsync` chỉ lấy câu hỏi của **kỹ năng người dùng đã chọn** ở wizard (`GetSelectedSkillIdsAsync`; nếu chưa chọn → fallback cả 7). `SubmitAssessmentAsync` nạp option đã chọn (`GetAnsweredOptionsAsync`, kèm `Question.SkillId`), tính **% theo từng kỹ năng** = tổng điểm / (số câu × 4), map Level (<50 Weak, <80 Average, else Good), lưu 1 `AssessmentResult`/kỹ năng qua `SaveResultsAsync`. XP +50 giữ nguyên.
- Repo mới: `GetSelectedSkillIdsAsync`, `GetAnsweredOptionsAsync`, `SaveResultsAsync`. DTO `AssessmentQuestionDto.SkillName`. [Assessment.razor](SoftSync.Presentation/Components/Pages/Assessment.razor): badge hiện `q.SkillName` (bỏ hardcode Communication) + thanh progress theo số câu.

## Xác thực (Auth) — ĐÃ TRIỂN KHAI XONG (verify OK)
Thêm login/logout + Google OAuth + login SĐT (mật khẩu & OTP) + quên mật khẩu (mã qua SMS/email). Dùng **ASP.NET Core Identity**. Dịch vụ thật: Google, Twilio (SMS), MailKit (SMTP) — khóa để trống trong user-secrets, thiếu khóa thì báo lỗi rõ, app vẫn build.

**Ràng buộc cốt lõi**: Blazor Server không set cookie qua SignalR circuit → dùng mẫu "Blazor Web App Individual Accounts": trang Account render **static SSR** (không `@rendermode`), các trang cũ giữ interactive bằng `@rendermode InteractiveServer` từng trang (bỏ global ở `App.razor`).

Đã làm:
- DAL: `User` → `ApplicationUser : IdentityUser<int>` (giữ khóa int cho mọi FK `UserId`); thêm entity `VerificationCode`; `SoftSyncDbContext` → `IdentityDbContext<ApplicationUser, IdentityRole<int>, int>`; xóa seed User Id=1.
- Common: enum `VerificationPurpose`.
- BLL `Auth/`: `SmtpOptions`/`TwilioOptions` (có `IsConfigured`), `IAppEmailSender`/`IAppSmsSender` + `MailKitEmailSender`/`TwilioSmsSender` (thiếu key → `InvalidOperationException`), `VerificationCodeService` (mã 6 số hash SHA-256, hết hạn 10', khóa sau 5 lần).
- Presentation `Components/Account/`: support classes (`IdentityRedirectManager`, `IdentityUserAccessor`, `IdentityRevalidatingAuthenticationStateProvider`, `IdentityComponentsEndpointRouteBuilderExtensions`), `Shared/` (`AccountLayout`, `StatusMessage`, `ExternalLoginPicker`), `Pages/` (`Login`, `Register`, `LoginWithOtp`, `ForgotPassword`, `ResetPassword`, `ExternalLogin`).

- `Program.cs`: wiring Identity + AddGoogle (gate theo config) + senders + options + `MapAdditionalIdentityEndpoints` + `DbInitializer.SeedAsync` + `UseAuthentication/UseAuthorization`.
- `App.razor`: bỏ `@rendermode` global; thêm `@rendermode InteractiveServer` vào từng trang cũ (Home, Assistant, CaseStudies, Progress, Community, Assessment, AssessmentResult, Roadmap, SelectSkills, ProfileSetup, Counter).
- `Routes.razor`: `RouteView` → `AuthorizeRouteView` + `<NotAuthorized>` → `RedirectToLogin` (`/Account/Login?ReturnUrl=`).
- `ProfileSetup.razor`: `[Authorize]`, cập nhật user đang đăng nhập (`UpdateProfileAsync`) thay vì tạo mới; Assistant/Progress bỏ hardcode `UserId = 1`, lấy từ auth state qua `AuthStateExtensions.GetUserIdAsync`.
- `Translations.cs`: thêm chuỗi `auth.*` (EN/VI). `MainLayout.razor`: `<AuthorizeView>` cho nav login/logout.
- Config placeholders trong `appsettings.json` (Authentication:Google, Twilio, Smtp).

### Migration & demo account
- Migration hiện tại: `20260707042431_InitialIdentity` (đã reset, xóa `Initial` cũ). Nếu đổi entity → tạo migration mới.
- Demo account (seed runtime trong `DbInitializer`): email `demo@softsync.local`, mật khẩu `Demo@12345`.
- dotnet-ef đã cài global (v10). PATH bash cần `export PATH="/c/Program Files/dotnet:$PATH:$HOME/.dotnet/tools"`.

### Điền khóa thật sau (user-secrets, trên project Presentation)
```
dotnet user-secrets set "Authentication:Google:ClientId" "..."
dotnet user-secrets set "Authentication:Google:ClientSecret" "..."
dotnet user-secrets set "Twilio:AccountSid" "..."   # + AuthToken, FromNumber
dotnet user-secrets set "Smtp:Host" "..."           # + User, Password, FromEmail
```
Google OAuth redirect URI: `https://localhost:7212/signin-google`.

### Đã verify (2026-07-07)
Build OK; DB tạo với bảng `AspNet*` + `VerificationCodes`; login demo email+password → 302 + nav hiện Logout; trang `[Authorize]` redirect về Login; forgot-password khi SMTP trống → báo lỗi rõ (không crash); Google chưa cấu hình → nút hiện thông báo "chưa cấu hình".

Kế hoạch chi tiết: `C:\Users\Admin\.claude\plans\prancy-skipping-cocke.md`.
