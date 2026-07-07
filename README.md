# SoftSync

Nền tảng học kỹ năng mềm ứng dụng AI, xây dựng bằng **Blazor Server (.NET 8)** theo kiến trúc 3 lớp. Giao diện dùng **Bootstrap** kết hợp một hệ **liquid glass** (kính mờ) tự dựng, biên dịch qua **Vite + Tailwind CSS v4**.

## Tính năng

- Đánh giá kỹ năng mềm bằng AI (assessment)
- Lộ trình học cá nhân hóa (roadmap)
- Trợ lý AI hỏi đáp (chat assistant)
- Theo dõi tiến độ, case study, kết nối mentor
- Giao diện liquid glass: kính mờ, ánh sáng bám con trỏ, nền aurora động

> Các service AI hiện dùng bản giả lập (`Fake*Service`). Xem [API_INTEGRATION_NOTES.md](API_INTEGRATION_NOTES.md) để tích hợp AI thật.

## Kiến trúc

| Project                   | Vai trò                                                             |
| ------------------------- | -------------------------------------------------------------------- |
| `SoftSync.Common`       | DTOs, Enums dùng chung                                              |
| `SoftSync.DAL`          | Data Access Layer — EF Core,`DbContext`, Repositories, Migrations |
| `SoftSync.BLL`          | Business Logic Layer — Services + Interfaces (gồm interface AI)    |
| `SoftSync.Presentation` | Blazor Server UI + toolchain front-end (Vite/Tailwind)               |

## Yêu cầu môi trường

- **.NET SDK 10.0** (dự án target `net10.0`)
- **Node.js 18+** và **npm** (để biên dịch CSS/JS front-end)
- **SQL Server** (LocalDB / Express / full / named instance) — sửa connection string cho khớp instance của bạn

## Cài đặt & chạy

### 1. Cấu hình database

Connection string nằm ở [SoftSync.Presentation/appsettings.json](SoftSync.Presentation/appsettings.json):

```json
"ConnectionStrings": {
  "SoftSyncDb": "Server=localhost\\JAYC;Database=SoftSyncDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Sửa `Server=` cho khớp SQL Server của bạn:

- Instance mặc định: `Server=localhost`
- Named instance (ví dụ máy này): `Server=localhost\\JAYC`
- SQL LocalDB: `Server=(localdb)\\MSSQLLocalDB`

Xem tên instance đang có bằng: `sc query state= all | findstr MSSQL` (PowerShell). Instance `MSSQL$JAYC` tương ứng chuỗi `localhost\JAYC`.

### 2. Áp migration để tạo database

```bash
# Cài công cụ EF nếu chưa có
dotnet tool install --global dotnet-ef

# Từ thư mục gốc solution, tạo DB từ migration Initial sẵn có
dotnet ef database update --project SoftSync.DAL --startup-project SoftSync.Presentation
```

Nếu `dotnet` không nằm trong PATH, `dotnet-ef` sẽ báo "cannot find dotnet". Thêm tạm vào PATH trước khi chạy:

```powershell
$env:Path = "C:\Program Files\dotnet;$env:Path"
dotnet ef database update --project SoftSync.DAL --startup-project SoftSync.Presentation
```

### 3. Chạy ứng dụng

```bash
dotnet run --project SoftSync.Presentation
```

Bước `dotnet run`/`dotnet build` **tự động** chạy `npm install` + `npm run build` (cấu hình trong [.csproj](SoftSync.Presentation/SoftSync.Presentation.csproj)) để sinh `wwwroot/dist/app.css` và `app.js`. Nếu không có Node, build .NET vẫn chạy nhưng bỏ qua phần front-end (sẽ có cảnh báo).

Mặc định app chạy tại:

- https://localhost:7212
- http://localhost:5104

## Toolchain front-end (Vite + Tailwind)

Nguồn nằm trong `SoftSync.Presentation/`:

| Đường dẫn       | Nội dung                                                                    |
| ------------------- | ---------------------------------------------------------------------------- |
| `Styles/main.css` | Tailwind v4 (tắt preflight để không đụng Bootstrap) + hệ liquid glass |
| `Scripts/main.js` | Entry bundle, xử lý ánh sáng bám con trỏ (`.ss-glass-interactive`)   |
| `vite.config.js`  | Cấu hình Vite → xuất`wwwroot/dist/`                                    |
| `package.json`    | Dependencies + scripts                                                       |

Lệnh npm (chạy trong `SoftSync.Presentation/`):

```bash
npm install       # cài dependencies
npm run build     # build 1 lần ra wwwroot/dist
npm run dev       # watch, rebuild khi sửa CSS/JS
```

Khi phát triển giao diện, chạy song song để có hot-reload:

```bash
# Terminal 1 — watch front-end
cd SoftSync.Presentation && npm run dev

# Terminal 2 — Blazor hot reload
dotnet watch --project SoftSync.Presentation
```

### Quy ước `tw:` prefix (quan trọng)

Dự án chạy **hybrid Bootstrap + Tailwind**: Bootstrap CSS vẫn được nạp cho các component (button, badge, alert, progress, spinner, form) và màu sắc, còn layout (grid/flex/spacing/typography) dùng Tailwind.

Vì Bootstrap và Tailwind trùng tên nhiều utility (`p-4`, `gap-3`, `mb-3`...) nhưng **khác giá trị**, Tailwind được cấu hình bật **prefix `tw:`** (trong [Styles/main.css](SoftSync.Presentation/Styles/main.css)). Mọi utility Tailwind phải viết kèm tiền tố:

```razor
<!-- Tailwind: LUÔN có tiền tố tw: -->
<div class="tw:grid tw:grid-cols-1 tw:md:grid-cols-3 tw:gap-6 tw:p-6 tw:flex tw:items-center">

<!-- Bootstrap: giữ nguyên, KHÔNG prefix -->
<button class="btn btn-primary">...</button>
<span class="badge bg-success">...</span>
<i class="bi bi-check-circle-fill text-success"></i>
```

Khi viết markup mới:

- **Layout/spacing/typography** → dùng Tailwind có prefix: `tw:flex`, `tw:grid`, `tw:p-6`, `tw:mb-4`, `tw:text-xl`, `tw:font-bold`, `tw:md:grid-cols-2`...
- **Component + màu** → giữ Bootstrap không prefix: `btn`, `btn-primary`, `badge`, `bg-*`, `text-muted`, `progress`, `form-control`...
- **Icon** → luôn giữ `bi bi-*` (Bootstrap Icons, dependency riêng).

Lưu ý thang đo khác nhau: Bootstrap `p-4` = 1.5rem, còn Tailwind `tw:p-4` = 1rem. Khi chuyển từ Bootstrap sang Tailwind cần đổi bậc (`p-4` → `tw:p-6`, `p-5` → `tw:p-12`).

## Hệ liquid glass

Các class dùng lại được (định nghĩa trong `Styles/main.css`):

- `.ss-glass` — bề mặt kính mờ: frost blur, specular highlight, sheen chéo trôi
- `.ss-glass-interactive` — thêm vệt sáng bám theo con trỏ chuột
- `.ss-aurora` — nền màu blur trôi (đặt sẵn trong `MainLayout.razor`)
- `.ss-card` đã được nâng cấp sẵn thành kính, nên mọi card kế thừa tự động

Toàn bộ hiệu ứng tôn trọng `prefers-reduced-motion`.

## Tích hợp AI thật

Ba interface AI (`IAiAssessmentService`, `IAiAssistantService`, `IAiRoadmapService`) đang dùng bản giả lập. Xem hướng dẫn thay thế bằng service thật trong [API_INTEGRATION_NOTES.md](API_INTEGRATION_NOTES.md).
