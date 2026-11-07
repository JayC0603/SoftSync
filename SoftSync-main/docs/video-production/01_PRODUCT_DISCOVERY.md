# SoftSync Space — Product Discovery

Ngày khảo sát: **07/11/2026, Asia/Saigon**. Workspace: `E:/fpt_edu/EXE`. Source tham chiếu: `SoftSync-ui-publish-20261107/SoftSync-main`, commit worktree `00296aa`. Bản gốc được tạo trong `E:/fpt_edu/EXE/docs/video-production/` vì workspace cha chứa nhiều bản dự án. Bản đưa lên Git nằm tại `SoftSync-main/docs/video-production/`; liên kết source đã chuyển sang đường dẫn tương đối để đọc trên GitHub.

## Phạm vi và độ tin cậy

Workspace có các bản SoftSync, SoftSync-main, SoftSync-master, SoftSync-review-softsync-merged-20261029 và SoftSync-ui-publish-20261107. Chọn bản ui-publish theo tên/ngày mới nhất, không trộn code các bản. Git status bản này sạch lúc bắt đầu. Không có AGENTS.md được tìm thấy trong workspace khi tìm cả file ẩn, loại thư mục dependency/cache.

Website **http://localhost:5203** đã chạy bằng tiến trình .NET; không khởi động thêm. UI được đối chiếu với source tham chiếu, nhưng chưa xác nhận thư mục làm việc/build hash của binary đang chạy. Vì vậy không khẳng định binary trùng tuyệt đối commit hoặc production giống local.

Chỉ xem trang, tìm/lọc và chọn case có state trong component. Không cài package, build, chạy test ứng dụng, sửa source/config, đọc .env, dùng mật khẩu seed, đăng ký tài khoản, upload CV, gửi tin nhắn, nộp assessment hoặc gọi API ghi dữ liệu. Website có analytics riêng; lượt xem có thể tự tạo telemetry phía ứng dụng, không thể cam kết backend không ghi bất kỳ dữ liệu nào. Không chủ động ghi dữ liệu nghiệp vụ.

Không tự chạy ứng dụng: `Program.cs` gọi initializer/seed khi startup; `CvBackend/app/main.py` gọi `Base.metadata.create_all`. Chế độ demo in-memory có trong code, nhưng chưa xác minh instance 5203 đang dùng chế độ đó hoặc được phép ghi dữ liệu test.

## Ba báo cáo

| Tài liệu | Kết quả | Cách xử lý |
|---|---|---|
| EXE201_G04_Group 5_FA26.docx | Không tìm thấy trong workspace | Chưa đọc Chapter 1; không suy diễn nội dung |
| EXE201_G04_Group 5_FA26 (1).docx | Không tìm thấy | Chưa đọc Chapter 2; không xác nhận khác biệt sản phẩm từ báo cáo |
| EXE201_G04_Group 5_FA26 (2).docx | Không tìm thấy | Chưa đọc Chapter 3; không xác nhận kết quả trải nghiệm, phản hồi, doanh thu |

Đã tìm `.docx` trong toàn workspace, gồm file ẩn/bị ignore, loại node_modules/.git/bin/obj/cache. Không tìm ngoài workspace, không tự tải tài liệu. Thông tin trong brief là định hướng của người dùng, không được trình bày như trích dẫn báo cáo đã đọc. Khi có tài liệu, đối chiếu mục tiêu → code → runtime, ưu tiên Chapter 3 nếu khác kế hoạch; hiện chưa thể hoàn thành đối chiếu nội dung ba chương.

## Kiến trúc và routing

- Ứng dụng đầy đủ: **ASP.NET Core net10.0, Blazor InteractiveServer, Razor Components, Identity**. Frontend Vite 6/Tailwind 4, Bootstrap trong UI. Không phải toàn bộ website là React.
- `SoftSync.Presentation`: pages, layouts, shared components, JS/CSS/assets, services và minimal API. Router dùng `AuthorizeRouteView` và `LearnerRouteGate`; một số luồng yêu cầu hoàn thành hồ sơ/assessment ngoài đăng nhập. Có viewport variants.
- `SoftSync.BLL`: nghiệp vụ, interfaces, AI contracts, services và rules. `SoftSync.DAL`: EF Core, PostgreSQL/Npgsql, entities, repositories và migrations. `SoftSync.Common`: DTO, enum, scoring.
- `SoftSync.Next`: Next 15, React 19, TypeScript, App Router; migration shell và CV dashboard/proxy. Chưa thay thế toàn bộ Blazor theo README và cấu trúc code.
- `CvBackend`: FastAPI, auth/storage/database. `CvAi`: FastAPI model service/checkpoints. `CvOcr`: OCR Python. Có source không chứng minh các service đang chạy hoặc model sẵn sàng.
- `SoftSync.Tests`, `docs`, `database`, `scripts`, Docker/deployment files hiện diện. Không sử dụng con số “54 tests passed” trong README như kết quả kiểm thử của lượt khảo sát này.
- VERCEL.md ngoài root và README trong Next có chỉ dẫn triển khai khác nhau; không xử lý/deploy trong nhiệm vụ này. URL public được README dẫn là https://softsync.id.vn/.

## Feature Inventory

A = Verified Working trong phạm vi đã kiểm tra; B = Implemented chưa xác minh runtime; C = Partial/Mock; D = Planned/chưa tìm thấy triển khai UI phù hợp; E = Broken/Blocked. Một module có thể có trạng thái khác nhau ở từng phần. Redirect đăng nhập đúng thiết kế không đồng nghĩa tính năng bị hỏng.

| Feature | Route | Component | API | Status | Demo Readiness | Evidence |
|---|---|---|---|---|---|---|
| Landing, neo hành trình | `/`, `/#learning-journey` | Home | Không cần API nghiệp vụ cho neo | A trang/neo; C thẻ minh họa | Quay giới thiệu định hướng | R01–R02; S01 |
| Tìm/lọc tài nguyên | `/resources` | Resources | Lọc Items trong component | A tìm/lọc; C danh mục tĩnh | Chọn chính; không hứa mở/tải tài liệu | R03–R05; S02 |
| Bookmark tài nguyên | `/resources` | Resources | HashSet saved trong component | B thao tác chưa thử; không persistent | Không chọn | S02: ToggleSaved |
| Case study tương tác | `/case-studies` | CaseStudies | ICaseStudyService → repository; SelectOption cục bộ | A tải/chọn/phản hồi | Chọn chính, phản hồi biên soạn sẵn | R06–R07; S03 |
| Mentor: danh sách/lọc/hồ sơ | `/mentors`, `/mentors/{MentorId:int}` | Mentors, MentorProfile | IMentorService | A xem/lọc | Chọn chính; không gửi yêu cầu | R08–R10; S04 |
| Yêu cầu Mentor/thông báo | `/mentor-requests`, `/notifications` | MentorRequestsPage, Notifications | MentorRequestWorkflow, NotificationInbox | B; auth | Chưa chọn | Khai báo route; ApplicationServices |
| Community hướng dẫn | `/community`, `/community/skills/{SkillId:int}` | MobileCommunity, CommunitySkill | ISkillService | A xem hướng dẫn; C xã hội | Đoạn nối phụ | R11–R12; S05 |
| Bài đăng, bình luận, messaging giữa người dùng | Không tìm thấy route độc lập phù hợp | Không xác nhận UI tương ứng | Chat history của AI không phải chat giữa người dùng | D theo UI khảo sát | Loại | UI community ghi rõ chưa có nhóm/bài đăng/bình luận |
| Dashboard/profile/goals | `/dashboard`, `/profile`, `/profile-setup`, `/select-skills/{UserId:int}` | Dashboard, Profile, ProfileSetup, SelectSkills | IUserService, IProgressService, ISkillService | B; Dashboard bị auth | Chưa chọn | R15; S09 |
| Assessment/kết quả | `/assessment/{UserId:int}`, `/assessment-complete/{UserId:int}`, `/assessment-result/{UserId:int}` | Assessment, AssessmentCompleted, AssessmentResult | IAssessmentService, HuggingFaceAiAssessmentService | B; điểm rubric, AI diễn giải có fallback | Ưu tiên kiểm tra khi có test account | S08; BusinessServices |
| Roadmap | `/roadmap`, `/roadmap/{UserId:int}`, `/roadmap-legacy` và biến thể user | RoadmapOverview, Roadmap | IRoadmapService → IAiRoadmapService | C generator cơ học; B runtime | Chưa chọn; không gọi LLM đã hoạt động | R14; S07 |
| Bài học/practice/game theo roadmap | `/roadmap/lecture/...`, `/practice/...`, `/game/...`, `/voice-practice/...`, `/inbox-rush/...`, `/adventure` | RoadmapLecture, RoadmapPractice, RoadmapGame, CommunicationVoicePractice, InboxRush, RoadmapAdventure | LearningJourneyService, learning sessions API | B; auth | Chưa chọn | Khai báo route; Program |
| Course/quiz/mini game | `/courses`, `/courses/{CourseId:int}`, lesson route, `/game`, `/game/history`, `/game/result/{AttemptId:int}`, `/game/review/{AttemptId:int}` | Courses, MobileGame, MiniGameDirectory, Quiz* | ICourseService; course/game-history APIs | B; Game có guest directory riêng | Chưa chọn | Game.razor; route inventory |
| Smart CV chính | `/cv` | CvWorkspace → CvCareerWorkspace | CvWorkspaceService; extraction; Presentation CvReviewService | B; auth | Ưu tiên khảo sát nhưng chưa đủ điều kiện vào main | R13; S06 |
| CV review khác | `/cv-review`, `/cv-review/{AnalysisId:int}` | CvReview | ICvReviewService/BLL → AI + EF | B; auth | Không trộn với /cv | S06b |
| CV dashboard mở rộng | Next `/cv`, iframe có điều kiện trong Blazor | IntegratedCv, cv-app/App | Next /api/cv/[...path] → CvBackend/CvAi | B, backend/model chưa xác minh | Chưa chọn | Next source; CvIntegrationService |
| Trợ lý học tập | `/assistant`, `/assistant/{UserId:int}` | Assistant | LlmAiAssistantService, knowledge/PDF fallback, history | A UI; B phản hồi model | Chưa chọn hội thoại | R17; S10 |
| Progress/schedule/settings | `/progress` và biến thể user; `/schedule`, `/settings`, `/more` | Progress, Schedule, Settings, More | IProgressService, IScheduleService, AccessibilityService | B; auth | Chưa chọn | Route/source services |
| Accessibility/song ngữ | Nhiều trang, `/settings` | PublicLayout, localization/CSS | LocalizationService, AccessibilityService | A thấy nhãn/skip link; B kiểm thử hành vi | Không tuyên bố đạt chuẩn Universal Design | S11; chưa audit bàn phím/contrast/screen reader |
| SoftMap/workshop map | Không thấy route SoftMap trong Razor/Next khảo sát | Có workshop rules/maplibre dependency, chưa chứng minh UI map | Chưa xác nhận API map | D ở phạm vi demo | Loại | Không suy diễn tính năng từ package |
| Teacher/Admin | `/teacher/*`, `/admin/*` | TeacherCourses/Analytics/QuizResults, Admin* | Policies/services | B; chưa dùng quyền quản trị | Không quay | Route và authorization attributes |

## Nhật ký browser

Công cụ có sẵn: browser automation CUA, AX/DOM, thao tác Playwright, screenshot quan sát landing. Không cài browser/package. Base local: `http://localhost:5203`. Nhật ký này là ghi chép quan sát, không phải bộ ảnh/network trace đã lưu.

| Mã | Trang/thao tác | Kết quả thực tế | Kết luận |
|---|---|---|---|
| R01 | Mở `/` | Hero, CTA, navigation, sections, footer. 38%, 13/34, 55% có nhãn minh họa | A trang; không phải progress người dùng thật |
| R02 | Bấm Xem lộ trình | URL `/#learning-journey`; screenshot thấy bốn bước và thẻ có nhãn minh họa | A điều hướng neo; không phải roadmap cá nhân |
| R03 | Mở `/resources` | 5 tài nguyên, bộ lọc và ô tìm kiếm | A UI |
| R04 | Bấm Giao tiếp | Còn 2: I-Statement và Phản hồi mang tính xây dựng | A lọc |
| R05 | Nhập I-Statement khi lọc Giao tiếp | Còn 1: Mẫu hội thoại I-Statement | A tìm kiếm |
| R06 | Mở `/case-studies` | Group Communication; câu hỏi thành viên không đóng góp; hai lựa chọn tiếng Anh | A tải case |
| R07 | Chọn Talk to them privately to understand their situation. | Lựa chọn tuyệt vời! / Direct, empathetic communication is key.; đáp án bị disable | A phản hồi có sẵn; không xác minh lưu tiến độ |
| R08 | Mở `/mentors` | 3 hồ sơ | A danh sách |
| R09 | Bấm Giao tiếp | Còn 1 hồ sơ Tố Trâm | A lọc |
| R10 | Bấm Xem hồ sơ | `/mentors/2`; tên, chuyên môn, phạm vi hỗ trợ và ba bước chuẩn bị | A xem hồ sơ; chưa gửi yêu cầu |
| R11 | Mở `/community` | Ba link kỹ năng; thông báo chưa có nhóm, bài đăng, bình luận | A hướng dẫn; C xã hội |
| R12 | Bấm Giao tiếp | `/community/skills/1`; ba bước hướng dẫn; chưa có Mentor gắn kỹ năng này | A xem hướng dẫn |
| R13 | Mở `/cv` | `/Account/Login?ReturnUrl=%2Fcv` | Auth chặn; CV end-to-end B |
| R14 | Mở `/roadmap` | `/Account/Login?ReturnUrl=%2Froadmap` | Auth chặn; chưa test roadmap |
| R15 | Mở `/dashboard` | `/Account/Login?ReturnUrl=%2Fdashboard` | Auth chặn; chưa test Dashboard |
| R16 | Xem `/Account/Register` và Login | Google disable, thông báo chưa cấu hình; Register có email thu gọn | A render; E Google local; chưa thử đăng ký/login |
| R17 | Mở `/assistant`, không gửi | Lời chào mặc định, ô nhập, gợi ý | A UI; chưa có trả lời model |
| R18 | Mở `https://softsync.id.vn/` | Browser báo `net::ERR_CONNECTION_CLOSED` | E truy cập từ phiên này; không kết luận outage toàn cầu |

Không mở assessment bằng user ID đoán. Chưa mở trực tiếp profile/progress/course lesson và chưa nộp quiz. Chưa được cấp test account. Không gửi câu hỏi AI vì code có lưu lịch sử và chưa xác minh môi trường disposable. Không kết luận dịch vụ AI hỏng chỉ từ việc chưa thể kiểm tra.

## AI/CV: điều phải mô tả đúng

1. Assessment tính điểm theo rubric lựa chọn; AI hỗ trợ diễn giải, có fallback deterministic trong BusinessServices. Không gọi điểm số là AI tự đánh giá toàn bộ.
2. DI đăng ký `IAiRoadmapService → FakeAiRoadmapService`; sinh khung theo kỹ năng yếu, chỉ publish curriculum phù hợp. Không khẳng định LLM cá nhân hóa đã chạy.
3. `/cv`: InputFile nhận **PDF/TXT**, reader giới hạn **10 MiB** trong source; CV text tối đa 40.000, JD tối đa 20.000 ký tự. Có UI chất lượng CV, phù hợp JD, nhận xét/đề xuất, kho và lịch sử; tất cả B vì chưa đi qua auth.
4. `/cv-review`: luồng khác, **PDF/DOCX**, MaxUploadBytes mặc định 5 MiB, có thể cấu hình. Dùng BLL CvReviewService, khác Presentation CvReviewService. Không trộn kết quả/định dạng của hai luồng hoặc iframe Next.
5. API Presentation review nhận JSON, không mặc định upload multipart. Kho CV còn gọi service trực tiếp qua Blazor.
6. Assistant có LLM/knowledge/PDF/fallback; lời chào tĩnh không chứng minh model đã trả lời. Case study là phản hồi biên soạn sẵn, không phải AI chấm đáp án.
7. Không có bằng chứng cải thiện kỹ năng, độ chính xác AI, tỷ lệ việc làm, doanh thu hoặc phản hồi người dùng từ các báo cáo chưa đọc.

## API đã xác định trong source

| API | Vai trò | Kiểm thử trong phiên này |
|---|---|---|
| GET `/health` | Liveness, trạng thái cấu hình Google | Chưa gọi; không đại diện DB/AI health |
| GET `/api/courses/`, `/{id:int}` | Public published courses | Chưa gọi trực tiếp |
| POST `/api/learning/sessions/`, `/{sessionId:guid}/heartbeat`, `/end` | Ghi phiên học, auth | Không gọi |
| GET `/api/cv/review/csrf`, POST `/api/cv/review/` | CSRF, CV text review, auth | Không gọi |
| GET `/api/cv/documents/{id:guid}/download` | Tải tệp thuộc user, auth | Không gọi; không đoán ID |
| GET `/api/cv-integration/csrf`, POST `/api/cv-integration/session` | Bridge session CV | Không gọi |
| GET `/api/game-history/csrf`, `/{gameKey}`; POST `/{gameKey}/sync` | Lịch sử game | Không gọi |
| POST `/api/visits/`, `/{id:guid}` | Website telemetry | Không gọi thủ công |
| Next `/api/cv/[...path]`, `/api/health`, `/api/visits/[[...path]]` | Backend proxy | Backend đích chưa xác minh |

## Bằng chứng source

Các đường dẫn dưới đây tương đối với source tham chiếu. Bảng link và route có số dòng được bổ sung tự động từ file hiện tại ở cuối tài liệu.

- **S01:** `SoftSync.Presentation/Components/Pages/Home.razor` và `Components/Layout/PublicLayout.razor`.
- **S02:** `Components/Pages/Resources.razor`: `Filtered`, `Items`, `ToggleSaved`, `HashSet saved`; danh mục tĩnh, mở mục điều hướng sang các module khác.
- **S03:** `Components/Pages/CaseStudies.razor`: `SelectOption` chỉ gán `feedback/isCorrect`; `LoadNext` chọn ngẫu nhiên từ skill 1.
- **S04:** `Components/Pages/Mentors.razor`: `MatchesSkill`, `Filtered`; `MentorProfile.razor`.
- **S05:** `Components/Variants/MobileCommunity.razor`, `Components/Pages/CommunitySkill.razor`.
- **S06:** `Components/Shared/CvCareerWorkspace.razor`, `Services/CvDocumentExtractionService.cs`, `CvReviewDocumentReader.cs`, `CvReviewService.cs`, `CvReviewEndpoints.cs`.
- **S06b:** `Components/Pages/CvReview.razor`, `SoftSync.BLL/Services/CvReviewService.cs`.
- **S07:** `Configuration/ApplicationServices.cs`, `SoftSync.BLL/Services/Fake/FakeServices.cs`.
- **S08:** `Services/AiLearningEvaluationService.cs`, `SoftSync.BLL/Services/BusinessServices.cs`.
- **S09:** `Components/Routes.razor`, `Components/Shared/LearnerRouteGate.razor`.
- **S10:** `Components/Pages/Assistant.razor`, `Services/LlmAiAssistantService.cs`; có logic `TrySaveHistoryAsync`.
- **S11:** `Components/Layout/PublicLayout.razor` có skip link/main-content; CSS có reduced-motion, chưa kiểm thử accessibility toàn diện.

## Chọn/loại

Main chọn ba module: **tìm/lọc tài nguyên, case study chọn/phản hồi, Mentor lọc/xem hồ sơ**. Community hướng dẫn làm đoạn nối; landing làm ngữ cảnh. Loại khỏi main: CV, assessment, roadmap, progress, hội thoại AI, messaging, SoftMap và vận hành admin vì chưa đủ runtime evidence hoặc chưa có UI tương ứng. File 03 có phương án thay cảnh sau khi test an toàn; chúng vẫn B/C ở ngày khảo sát.

## Link source và số dòng

- S01: [SoftSync.Presentation/Components/Pages/Home.razor:1](../../SoftSync.Presentation/Components/Pages/Home.razor#L1)
- S02: [SoftSync.Presentation/Components/Pages/Resources.razor:75](../../SoftSync.Presentation/Components/Pages/Resources.razor#L75)
- S03: [SoftSync.Presentation/Components/Pages/CaseStudies.razor:59](../../SoftSync.Presentation/Components/Pages/CaseStudies.razor#L59)
- S04: [SoftSync.Presentation/Components/Pages/Mentors.razor:31](../../SoftSync.Presentation/Components/Pages/Mentors.razor#L31)
- S04: [SoftSync.Presentation/Components/Pages/MentorProfile.razor:1](../../SoftSync.Presentation/Components/Pages/MentorProfile.razor#L1)
- S05: [SoftSync.Presentation/Components/Variants/MobileCommunity.razor:8](../../SoftSync.Presentation/Components/Variants/MobileCommunity.razor#L8)
- S05: [SoftSync.Presentation/Components/Pages/CommunitySkill.razor:1](../../SoftSync.Presentation/Components/Pages/CommunitySkill.razor#L1)
- S06: [SoftSync.Presentation/Components/Shared/CvCareerWorkspace.razor:53](../../SoftSync.Presentation/Components/Shared/CvCareerWorkspace.razor#L53)
- S06: [SoftSync.Presentation/Services/CvReviewDocumentReader.cs:8](../../SoftSync.Presentation/Services/CvReviewDocumentReader.cs#L8)
- S06b: [SoftSync.Presentation/Components/Pages/CvReview.razor:31](../../SoftSync.Presentation/Components/Pages/CvReview.razor#L31)
- S07: [SoftSync.Presentation/Configuration/ApplicationServices.cs:137](../../SoftSync.Presentation/Configuration/ApplicationServices.cs#L137)
- S07: [SoftSync.BLL/Services/Fake/FakeServices.cs:68](../../SoftSync.BLL/Services/Fake/FakeServices.cs#L68)
- S08: [SoftSync.Presentation/Services/AiLearningEvaluationService.cs:155](../../SoftSync.Presentation/Services/AiLearningEvaluationService.cs#L155)
- S09: [SoftSync.Presentation/Components/Shared/LearnerRouteGate.razor:34](../../SoftSync.Presentation/Components/Shared/LearnerRouteGate.razor#L34)
- S10: [SoftSync.Presentation/Components/Pages/Assistant.razor:153](../../SoftSync.Presentation/Components/Pages/Assistant.razor#L153)
- Startup: [SoftSync.Presentation/Program.cs:180](../../SoftSync.Presentation/Program.cs#L180)

## Route Razor đầy đủ từ source

Không phải mọi route dưới đây đã được browser-test. Attribute auth chỉ là một lớp kiểm tra; route không có attribute vẫn có thể dùng LearnerRouteGate/AuthorizeView. Component Variants không có @page không phải route độc lập.

| Route | File | Access tĩnh |
|---|---|---|
| `/Account/AccessDenied` | [SoftSync.Presentation/Components/Account/Pages/AccessDenied.razor](../../SoftSync.Presentation/Components/Account/Pages/AccessDenied.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/Account/ChangePassword` | [SoftSync.Presentation/Components/Account/Pages/ChangePassword.razor](../../SoftSync.Presentation/Components/Account/Pages/ChangePassword.razor#L1) | Auth/policy trong component |
| `/Account/ExternalLogin` | [SoftSync.Presentation/Components/Account/Pages/ExternalLogin.razor](../../SoftSync.Presentation/Components/Account/Pages/ExternalLogin.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/Account/ForgotPassword` | [SoftSync.Presentation/Components/Account/Pages/ForgotPassword.razor](../../SoftSync.Presentation/Components/Account/Pages/ForgotPassword.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/Account/Login` | [SoftSync.Presentation/Components/Account/Pages/Login.razor](../../SoftSync.Presentation/Components/Account/Pages/Login.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/Account/LoginWithOtp` | [SoftSync.Presentation/Components/Account/Pages/LoginWithOtp.razor](../../SoftSync.Presentation/Components/Account/Pages/LoginWithOtp.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/Account/Register` | [SoftSync.Presentation/Components/Account/Pages/Register.razor](../../SoftSync.Presentation/Components/Account/Pages/Register.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/Account/ResetPassword` | [SoftSync.Presentation/Components/Account/Pages/ResetPassword.razor](../../SoftSync.Presentation/Components/Account/Pages/ResetPassword.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/about` | [SoftSync.Presentation/Components/Pages/About.razor](../../SoftSync.Presentation/Components/Pages/About.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/admin/ai-configuration` | [SoftSync.Presentation/Components/Pages/AdminAiConfiguration.razor](../../SoftSync.Presentation/Components/Pages/AdminAiConfiguration.razor#L1) | Auth/policy trong component |
| `/admin/ai/settings` | [SoftSync.Presentation/Components/Pages/AdminAiSettings.razor](../../SoftSync.Presentation/Components/Pages/AdminAiSettings.razor#L1) | Auth/policy trong component |
| `/admin/notifications` | [SoftSync.Presentation/Components/Pages/AdminAnnouncements.razor](../../SoftSync.Presentation/Components/Pages/AdminAnnouncements.razor#L1) | Auth/policy trong component |
| `/admin/dashboard` | [SoftSync.Presentation/Components/Pages/AdminDashboard.razor](../../SoftSync.Presentation/Components/Pages/AdminDashboard.razor#L1) | Auth/policy trong component |
| `/admin/learners` | [SoftSync.Presentation/Components/Pages/AdminLearners.razor](../../SoftSync.Presentation/Components/Pages/AdminLearners.razor#L1) | Auth/policy trong component |
| `/admin/users` | [SoftSync.Presentation/Components/Pages/AdminUsers.razor](../../SoftSync.Presentation/Components/Pages/AdminUsers.razor#L1) | Auth/policy trong component |
| `/assessment/{UserId:int}` | [SoftSync.Presentation/Components/Pages/Assessment.razor](../../SoftSync.Presentation/Components/Pages/Assessment.razor#L2) | Auth/policy trong component |
| `/assessment-complete/{UserId:int}` | [SoftSync.Presentation/Components/Pages/AssessmentCompleted.razor](../../SoftSync.Presentation/Components/Pages/AssessmentCompleted.razor#L1) | Auth/policy trong component |
| `/assessment-result/{UserId:int}` | [SoftSync.Presentation/Components/Pages/AssessmentResult.razor](../../SoftSync.Presentation/Components/Pages/AssessmentResult.razor#L1) | Auth/policy trong component |
| `/assistant` | [SoftSync.Presentation/Components/Pages/Assistant.razor](../../SoftSync.Presentation/Components/Pages/Assistant.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/assistant/{UserId:int}` | [SoftSync.Presentation/Components/Pages/Assistant.razor](../../SoftSync.Presentation/Components/Pages/Assistant.razor#L2) | Không có attribute; xem gate/AuthorizeView |
| `/case-studies` | [SoftSync.Presentation/Components/Pages/CaseStudies.razor](../../SoftSync.Presentation/Components/Pages/CaseStudies.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/roadmap/voice-practice/{UserId:int}/{ItemId:int}` | [SoftSync.Presentation/Components/Pages/CommunicationVoicePractice.razor](../../SoftSync.Presentation/Components/Pages/CommunicationVoicePractice.razor#L1) | Auth/policy trong component |
| `/community` | [SoftSync.Presentation/Components/Pages/Community.razor](../../SoftSync.Presentation/Components/Pages/Community.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/community/skills/{SkillId:int}` | [SoftSync.Presentation/Components/Pages/CommunitySkill.razor](../../SoftSync.Presentation/Components/Pages/CommunitySkill.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/counter` | [SoftSync.Presentation/Components/Pages/Counter.razor](../../SoftSync.Presentation/Components/Pages/Counter.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/courses` | [SoftSync.Presentation/Components/Pages/Courses.razor](../../SoftSync.Presentation/Components/Pages/Courses.razor#L1) | Auth/policy trong component |
| `/courses/{CourseId:int}` | [SoftSync.Presentation/Components/Pages/Courses.razor](../../SoftSync.Presentation/Components/Pages/Courses.razor#L2) | Auth/policy trong component |
| `/courses/{CourseId:int}/lessons/{LessonId:int}` | [SoftSync.Presentation/Components/Pages/Courses.razor](../../SoftSync.Presentation/Components/Pages/Courses.razor#L3) | Auth/policy trong component |
| `/cv-review` | [SoftSync.Presentation/Components/Pages/CvReview.razor](../../SoftSync.Presentation/Components/Pages/CvReview.razor#L2) | Auth/policy trong component |
| `/cv-review/{AnalysisId:int}` | [SoftSync.Presentation/Components/Pages/CvReview.razor](../../SoftSync.Presentation/Components/Pages/CvReview.razor#L3) | Auth/policy trong component |
| `/cv` | [SoftSync.Presentation/Components/Pages/CvWorkspace.razor](../../SoftSync.Presentation/Components/Pages/CvWorkspace.razor#L1) | Auth/policy trong component |
| `/dashboard` | [SoftSync.Presentation/Components/Pages/Dashboard.razor](../../SoftSync.Presentation/Components/Pages/Dashboard.razor#L1) | Auth/policy trong component |
| `/Error` | [SoftSync.Presentation/Components/Pages/Error.razor](../../SoftSync.Presentation/Components/Pages/Error.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/game` | [SoftSync.Presentation/Components/Pages/Game.razor](../../SoftSync.Presentation/Components/Pages/Game.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/` | [SoftSync.Presentation/Components/Pages/Home.razor](../../SoftSync.Presentation/Components/Pages/Home.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/roadmap/inbox-rush/{UserId:int}/{ItemId:int}` | [SoftSync.Presentation/Components/Pages/InboxRush.razor](../../SoftSync.Presentation/Components/Pages/InboxRush.razor#L1) | Auth/policy trong component |
| `/mentors/{MentorId:int}` | [SoftSync.Presentation/Components/Pages/MentorProfile.razor](../../SoftSync.Presentation/Components/Pages/MentorProfile.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/mentor-requests` | [SoftSync.Presentation/Components/Pages/MentorRequestsPage.razor](../../SoftSync.Presentation/Components/Pages/MentorRequestsPage.razor#L1) | Auth/policy trong component |
| `/admin/mentor-requests` | [SoftSync.Presentation/Components/Pages/MentorRequestsPage.razor](../../SoftSync.Presentation/Components/Pages/MentorRequestsPage.razor#L2) | Auth/policy trong component |
| `/mentors` | [SoftSync.Presentation/Components/Pages/Mentors.razor](../../SoftSync.Presentation/Components/Pages/Mentors.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/more` | [SoftSync.Presentation/Components/Pages/More.razor](../../SoftSync.Presentation/Components/Pages/More.razor#L1) | Auth/policy trong component |
| `/notifications` | [SoftSync.Presentation/Components/Pages/Notifications.razor](../../SoftSync.Presentation/Components/Pages/Notifications.razor#L1) | Auth/policy trong component |
| `/profile` | [SoftSync.Presentation/Components/Pages/Profile.razor](../../SoftSync.Presentation/Components/Pages/Profile.razor#L1) | Auth/policy trong component |
| `/profile-setup` | [SoftSync.Presentation/Components/Pages/ProfileSetup.razor](../../SoftSync.Presentation/Components/Pages/ProfileSetup.razor#L1) | Auth/policy trong component |
| `/progress` | [SoftSync.Presentation/Components/Pages/Progress.razor](../../SoftSync.Presentation/Components/Pages/Progress.razor#L2) | Auth/policy trong component |
| `/progress/{UserId:int}` | [SoftSync.Presentation/Components/Pages/Progress.razor](../../SoftSync.Presentation/Components/Pages/Progress.razor#L3) | Auth/policy trong component |
| `/game/history` | [SoftSync.Presentation/Components/Pages/QuizHistory.razor](../../SoftSync.Presentation/Components/Pages/QuizHistory.razor#L1) | Auth/policy trong component |
| `/game/result/{AttemptId:int}` | [SoftSync.Presentation/Components/Pages/QuizResult.razor](../../SoftSync.Presentation/Components/Pages/QuizResult.razor#L1) | Auth/policy trong component |
| `/game/review/{AttemptId:int}` | [SoftSync.Presentation/Components/Pages/QuizReview.razor](../../SoftSync.Presentation/Components/Pages/QuizReview.razor#L1) | Auth/policy trong component |
| `/recruitment` | [SoftSync.Presentation/Components/Pages/Recruitment.razor](../../SoftSync.Presentation/Components/Pages/Recruitment.razor#L1) | Auth/policy trong component |
| `/resources` | [SoftSync.Presentation/Components/Pages/Resources.razor](../../SoftSync.Presentation/Components/Pages/Resources.razor#L1) | Không có attribute; xem gate/AuthorizeView |
| `/roadmap-legacy` | [SoftSync.Presentation/Components/Pages/Roadmap.razor](../../SoftSync.Presentation/Components/Pages/Roadmap.razor#L1) | Auth/policy trong component |
| `/roadmap-legacy/{UserId:int}` | [SoftSync.Presentation/Components/Pages/Roadmap.razor](../../SoftSync.Presentation/Components/Pages/Roadmap.razor#L2) | Auth/policy trong component |
| `/roadmap/adventure` | [SoftSync.Presentation/Components/Pages/RoadmapAdventure.razor](../../SoftSync.Presentation/Components/Pages/RoadmapAdventure.razor#L1) | Auth/policy trong component |
| `/roadmap/game/{UserId:int}/{ItemId:int}` | [SoftSync.Presentation/Components/Pages/RoadmapGame.razor](../../SoftSync.Presentation/Components/Pages/RoadmapGame.razor#L1) | Auth/policy trong component |
| `/roadmap/lecture/{UserId:int}/{ItemId:int}` | [SoftSync.Presentation/Components/Pages/RoadmapLecture.razor](../../SoftSync.Presentation/Components/Pages/RoadmapLecture.razor#L1) | Auth/policy trong component |
| `/roadmap` | [SoftSync.Presentation/Components/Pages/RoadmapOverview.razor](../../SoftSync.Presentation/Components/Pages/RoadmapOverview.razor#L1) | Auth/policy trong component |
| `/roadmap/{UserId:int}` | [SoftSync.Presentation/Components/Pages/RoadmapOverview.razor](../../SoftSync.Presentation/Components/Pages/RoadmapOverview.razor#L2) | Auth/policy trong component |
| `/roadmap/practice/{UserId:int}/{ItemId:int}` | [SoftSync.Presentation/Components/Pages/RoadmapPractice.razor](../../SoftSync.Presentation/Components/Pages/RoadmapPractice.razor#L1) | Auth/policy trong component |
| `/schedule` | [SoftSync.Presentation/Components/Pages/Schedule.razor](../../SoftSync.Presentation/Components/Pages/Schedule.razor#L1) | Auth/policy trong component |
| `/select-skills/{UserId:int}` | [SoftSync.Presentation/Components/Pages/SelectSkills.razor](../../SoftSync.Presentation/Components/Pages/SelectSkills.razor#L1) | Auth/policy trong component |
| `/settings` | [SoftSync.Presentation/Components/Pages/Settings.razor](../../SoftSync.Presentation/Components/Pages/Settings.razor#L1) | Auth/policy trong component |
| `/teacher/courses/{CourseId:int}/analytics` | [SoftSync.Presentation/Components/Pages/TeacherAnalytics.razor](../../SoftSync.Presentation/Components/Pages/TeacherAnalytics.razor#L1) | Auth/policy trong component |
| `/teacher/quizzes/{QuizId:int}/analytics` | [SoftSync.Presentation/Components/Pages/TeacherAnalytics.razor](../../SoftSync.Presentation/Components/Pages/TeacherAnalytics.razor#L2) | Auth/policy trong component |
| `/teacher` | [SoftSync.Presentation/Components/Pages/TeacherCourses.razor](../../SoftSync.Presentation/Components/Pages/TeacherCourses.razor#L1) | Auth/policy trong component |
| `/teacher/courses` | [SoftSync.Presentation/Components/Pages/TeacherCourses.razor](../../SoftSync.Presentation/Components/Pages/TeacherCourses.razor#L2) | Auth/policy trong component |
| `/teacher/quizzes/{QuizId:int}/results` | [SoftSync.Presentation/Components/Pages/TeacherQuizResults.razor](../../SoftSync.Presentation/Components/Pages/TeacherQuizResults.razor#L1) | Auth/policy trong component |

## Next App Router

| Route | File |
|---|---|
| `/api/cv/[...path]` | `SoftSync.Next/app/api/cv/[...path]/route.ts` |
| `/api/health` | `SoftSync.Next/app/api/health/route.ts` |
| `/api/visits/[[...path]]` | `SoftSync.Next/app/api/visits/[[...path]]/route.ts` |
| `/courses/[id]` | `SoftSync.Next/app/courses/[id]/page.tsx` |
| `/courses` | `SoftSync.Next/app/courses/page.tsx` |
| `/cv` | `SoftSync.Next/app/cv/page.tsx` |
| `/` | `SoftSync.Next/app/page.tsx` |
