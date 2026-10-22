# SoftSync frontend audit and redesign

## Architecture

The deployed learning application is ASP.NET Core / Blazor Web App on .NET 10.
Interactive pages use InteractiveServer; account forms use static SSR so Identity
can issue cookies. Routes are declared in Razor pages and authorized by
AuthorizeRouteView. Vite 6 builds Tailwind 4 assets with the `tw:` prefix alongside
Bootstrap and Bootstrap Icons. A separate Next.js 15 / React 19 frontend exists;
it is not the UI serving the current /Account pages. This redesign targets Blazor.

## Route inventory / information architecture

| Area | Existing pages |
| --- | --- |
| Public discovery | /, /about, /mentors, /mentors/{id}, /community, /community/skills/{id}, /case-studies, /assistant |
| Account (static SSR) | Login, Register, ExternalLogin, LoginWithOtp, ForgotPassword, ResetPassword, ChangePassword, AccessDenied |
| Student workspace | /dashboard, /profile, /profile-setup, /settings, /select-skills/{id}, /resources |
| Learning | /courses, /roadmap, /roadmap-legacy, /roadmap/lecture, /roadmap/practice, /roadmap/game, /roadmap/voice-practice, /roadmap/inbox-rush |
| Assessment / progress | /assessment/{id}, /assessment-complete/{id}, /assessment-result/{id}, /progress, /progress/{id} |
| Quiz | /game, /game/history, /game/result/{id}, /game/review/{id} |
| Staff | /admin/users, /teacher, /teacher/courses, /teacher/courses/{id}/analytics, /teacher/quizzes/{id}/analytics, /teacher/quizzes/{id}/results |
| Other | /recruitment (authenticated), /counter (development example), /Error |

No new backend routes are required. Discovery uses horizontal public navigation;
authenticated learning retains the collapsible sidebar. Staff links retain
existing role checks (User, Teacher, Admin).

## Working features / dependencies

Identity password, Google OAuth and OTP flows; onboarding, user settings, assessments,
roadmaps, quizzes and progress services; mentor/community/resource pages; assistant
and chat history services; course management and analytics. Data is delivered by
existing BLL services and repositories, not invented browser APIs. AI services can
use configured providers or existing fake implementations; provider quality and
production configuration are outside this UI audit. SMTP/Twilio-backed flows need
their existing configuration. Existing error, loading and empty branches are retained.

## Findings

- Global, liquid-glass, journey and page-local styles compete; dark colors leak into light UI.
- Public discovery currently inherits the workspace sidebar.
- Large decorative hero and mentor cards precede a clear product explanation.
- Landing page claims 10K+ users and 24/7 support without backing data.
- Dashboard shows a hardcoded 4-day streak and hardcoded learning-stage statuses.
- Notification/search icons have no supported action; they must not imply working tools.
- Google configuration errors are implementation detail, not useful account-page content.
- Legacy hidden dashboard/progress markup includes sample figures and duplicate structure.
- Many controls have labels visually but lack programmatic label association.

## Reuse and redesign

Reuse logo, SYNCY mascot, translations, quiz/assessment controls, progress DTOs,
existing forms, permission-aware links, error branches and data services. Redesign
the public shell, product hero/journey, workspace overview, account composition,
navigation controls, assistant surfaces and shared semantic component tokens.

## Risks / boundaries

Preserve SSR forms, antiforgery, OAuth state, callbacks, roles, schemas and API
contracts. Existing service availability and claims cannot be inferred from a UI.
Do not present illustrative AI content as live evaluation. Removing unsupported
statistics removes presentation only, not stored data or working features.
Browser screenshots and end-to-end OAuth cannot be verified without a browser
and provider session; build/HTTP checks are not substitutes for those tests.
