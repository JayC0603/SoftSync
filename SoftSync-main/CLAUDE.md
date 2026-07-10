# CLAUDE.md — SoftSync context

> Giúp Claude nắm nhanh dự án. Cập nhật khi kiến trúc/quy ước thay đổi.

## Tổng quan
SoftSync — nền tảng học kỹ năng mềm bằng AI. **Blazor Server (.NET 10 `net10.0`)**, 3 lớp, UI song ngữ EN/VI. Bootstrap + Tailwind v4 (prefix `tw:`) + liquid glass tự dựng.

## Kiến trúc
| Project | Vai trò | File chính |
| --- | --- | --- |
| `SoftSync.Common` | DTO + Enum + `LevelSystem.cs` | `Dtos/SoftSyncDtos.cs`, `Enums/CommonEnums.cs` |
| `SoftSync.DAL` | EF Core, DbContext, Entities, Repo, Migrations, seed | `Data/*`, `Entities/Entities.cs`, `Repositories/Repositories.cs` |
| `SoftSync.BLL` | Service + Interface (+ AI fake + Auth) | `Interfaces/IServices.cs`, `Services/BusinessServices.cs`, `Services/Fake/*`, `Auth/*` |
| `SoftSync.Presentation` | Blazor Server UI + Vite/Tailwind | `Program.cs`, `Components/**`, `Services/LocalizationService.cs`+`Translations.cs`, `Styles/main.css`, `Scripts/main.js` |

## Quy ước quan trọng
- **Tailwind luôn prefix `tw:`** (hybrid Bootstrap). Layout/spacing/typography → Tailwind; component + màu → Bootstrap (`btn`, `badge`, `bg-*`); icon → `bi bi-*`. Thang đo khác nhau: Bootstrap `p-4`=1.5rem, Tailwind `tw:p-4`=1rem.
- **Localization**: mọi chuỗi UI qua `L["key"]`; cặp (En, Vi) trong `Services/Translations.cs`. Trang kế thừa `LocalizedComponentBase` để tự re-render khi đổi ngôn ngữ. `Translations.cs` là dict → **key trùng = crash lúc startup**. Check: `grep -oE '\["[^"]+"\]' Translations.cs | sort | uniq -d` trước khi build.
- **DbContext scoped theo circuit**: component layout (NavBar…) và trang KHÔNG được cùng query DbContext đồng thời lúc init (crash "A second operation was started on this context"). Fix: layout load DB trong **scope DI riêng** (`IServiceScopeFactory.CreateAsyncScope`) tại `OnAfterRenderAsync`.
- **Auth = "Blazor Web App Individual Accounts"**: Blazor Server không set cookie qua SignalR circuit → trang `Components/Account/**` render **static SSR** (không `@rendermode`); trang cũ giữ interactive bằng `@rendermode InteractiveServer` từng trang (bỏ global ở `App.razor`). Identity: `ApplicationUser : IdentityUser<int>`.
- **Theme/dark mode**: `Scripts/main.js` → `window.ssTheme` (set `data-theme`/`data-reduce-motion` trên `<html>`, persist localStorage, auto-init). Ngôn ngữ persist qua `ssLang`. main.js đi qua Vite → build sinh `wwwroot/dist/app.js`.
- **AI services** đang là **Fake*** (`Services/Fake/FakeServices.cs`).
- **Chỉ 3 kỹ năng active** (`QuizSeedData.ActiveSkillIds = {1,3,4}`); kỹ năng chưa có bộ đề bị ẩn. Quiz: 8 câu/kỹ năng, song ngữ (`*Text`/`*TextVi`). Chấm điểm thật theo % từng kỹ năng.
- **XP/Level**: `LevelSystem.cs` (ngưỡng 100·N; `UserDto.Level`/`LevelProgressPercent` computed, không lưu DB). Cộng XP: assessment +50, roadmap item +30 (1 lần). **Chưa có** hệ Day Streak / Badges (nếu UI hiện các số này là proxy/placeholder).

## Lệnh
- `dotnet` không có trong PATH bash → `export PATH="/c/Program Files/dotnet:$PATH:$HOME/.dotnet/tools"`.
- Build: `dotnet build SoftSync.sln` · Chạy: `dotnet run --project SoftSync.Presentation` (auto `npm install`+`npm run build`); URL https://localhost:7212 , http://localhost:5104
- Migration: `dotnet ef migrations add <Name> -p SoftSync.DAL -s SoftSync.Presentation` (khi tạo migration, phải **dừng app đang chạy** vì nó khóa DLL). `DbInitializer.SeedAsync` tự `MigrateAsync()` lúc boot → không cần `database update` tay.
- **DB: PostgreSQL** (Npgsql). Dev local qua Docker: `docker compose up -d db` (Postgres 16, db/user/pass = `softsync`). Conn string dev trong `appsettings.Development.json`; prod đọc env `DATABASE_URL` (URI `postgres://…` → `Program.cs` convert sang keyword string, bật SSL) hoặc `ConnectionStrings__SoftSyncDb`.

## Deploy (Render.com + Docker)
- Blazor Server **không chạy trên Vercel** (serverless, không .NET). Deploy lên host có .NET runtime.
- `SoftSync.Presentation/Dockerfile` multi-stage (SDK 10 + Node 20 build → aspnet 10 runtime), bind `$PORT` (fallback 8080). `.dockerignore` + `docker-compose.yml` ở gốc repo.
- `Program.cs`: `UseForwardedHeaders` (proxy TLS) chạy trước Auth; `UseHttpsRedirection` **chỉ ở Development**. `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` để ghi `DateTime` non-UTC không lỗi.
- Render Web Service (Docker, Dockerfile Path `SoftSync.Presentation/Dockerfile`, context = gốc repo) + Postgres free; env `DATABASE_URL` + `ASPNETCORE_ENVIRONMENT=Production`. Google OAuth redirect: `https://<app>.onrender.com/signin-google`. Free tier ngủ sau ~15' idle.

## Tài khoản demo
Seed runtime trong `DbInitializer`: `demo@softsync.local` / `Demo@12345`.

## Auth keys (user-secrets, project Presentation) — để trống → báo lỗi rõ, app vẫn build
`Authentication:Google:ClientId`/`ClientSecret` (redirect `https://localhost:7212/signin-google`), `Twilio:*`, `Smtp:*`.

## Trang chính đã làm (verify OK)
- **NavBar** (`Components/Layout/NavBar.razor`, `InteractiveServer`): nav pill + search + chuông badge + user chip (avatar + Level + tên) + dropdown (My Profile `/profile`, My Roadmap `/roadmap`, Progress, Settings `/settings`, Logout modal). Logout: nếu ở path nhạy cảm → modal, không thì submit form `#ss-logout-form` (`ssSubmitForm`).
- **Profile** `/profile`: avatar upload, level badge, thanh XP, 3 ô thống kê, chi tiết + nút `/profile-setup`.
- **ProfileSetup** `/profile-setup`: wizard 5 bước SYNCY (greeting → about+giới tính → goal → skills → start → `/assessment/{userId}`).
- **Settings** `/settings` (`InteractiveServer`, `[Authorize]`): tab sidebar. `ApplicationUser` có DisplayName, CurrentLevel, DailyStudyMinutes, StudyDaysPerWeek, PreferredStudyTime, PreferredLanguage, Theme, ReduceMotion. Save qua `UpdateSettingsAsync`+`AddSkillSelectionsAsync`. Account actions = endpoint HTTP thật (`Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs`): `POST /Account/Suspend`, `/Account/DeleteAccount` (modal gõ xác nhận). Đổi mật khẩu: `Components/Account/Pages/ChangePassword.razor`.
- **WelcomeToast** (MainLayout): đọc cookie `ss-welcome` → toast SYNCY chào mừng.
- Mascot: `Services/Mascot.cs` → `syncy-male.png`/`syncy-female.png` (`wwwroot/images/`).
