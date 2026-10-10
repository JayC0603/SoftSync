# SoftSync UI redesign delivery

## Implemented

- Separate horizontal public navigation from the learning workspace sidebar.
- Landing page with a product introduction, learning journey, labeled illustrative
  AI conversation, existing mentor content and a registration CTA.
- Shared navy/indigo/violet semantic palette, light default, preserved dark mode,
  Vietnamese default, consistent typography, controls, cards and responsive grids.
- Branded split-screen account pages; associated input labels, password visibility
  controls and Google buttons only when the existing provider is configured.
- Dashboard overview based on existing roadmap activity counts, lesson progress
  and user XP. Removed hardcoded streaks and unsupported public user statistics.
- Existing learning, assessment, progress, profile, settings and staff pages inherit
  the shared design system. Preserved route aliases and authorization policies.
- Reusable page header and loading/empty-state components; contextual workspace
  titles; persisted sidebar/theme preferences; public language switching on mobile.
- Existing mentor dialogs support focus containment and Escape via their existing
  close actions. Page content uses sections beneath a single layout main landmark.
- Bootstrap Icons 1.11.3 served locally with its MIT license and font assets.
- Removed nonfunctional notification/search presentation and sample resource
  progress; kept working resource filters, session bookmarks and navigation.

## Validation

- .NET Release build: successful, zero errors. Existing warnings remain; no claim
  is made that this frontend pass resolves backend/compiler warning debt.
- Existing backend test suite: 54 passed.
- JavaScript syntax and Git whitespace checks: passed.
- Headless Edge: 141 route/viewport checks across 360, 390, 768, 1024, 1366, 1440
  and 1920px. No page JavaScript errors. One resource-page overflow at 768px was
  found and fixed; targeted rechecks passed at all seven widths.
- Additional checks covered lesson/practice route rendering, local icon loading,
  demo password login, password visibility, sidebar/theme actions, public language
  switching, mobile navigation by keyboard, and mentor dialog focus/Tab/Escape.
- A student account remained unable to reach Teacher/Admin routes.
- Screenshots and local browser harness are in ignored `.ui-qa/`, not runtime
  application dependencies or committed user data.

## Limits

Google OAuth, SMTP/Twilio delivery, production AI replies and voice hardware were
not exercised against live external providers. Privileged staff populated states
need review with appropriate accounts. This is not a WCAG certification: manual
screen-reader, contrast and real-device reviews remain appropriate before launch.
Shared styling covers the existing route inventory; detailed layout changes focus
on public discovery, account pages, dashboard, assistant, progress and resources.
Existing lesson/game interactions and specialized layouts retain their behavior.

The implementation targets the deployed Blazor application, not the separate
Next.js CV frontend. No database schemas, API contracts, authentication handlers,
deployment environment values or role policies were changed for this redesign.

## Source map

- `Components/Layout/PublicLayout.razor`, `PublicNavigation.razor`: public shell.
- `Components/Layout/MainLayout.razor`, `NavBar.razor`, `WorkspaceTitle.razor`: app shell.
- `Components/Shared/UiPageHeader.razor`, `UiState.razor`: reusable presentation.
- `wwwroot/css/design-system.css`, `wwwroot/js/design-system.js`: shared theme/actions.
- `Components/Pages/Home.razor`, `Dashboard.razor`, `Progress.razor`, `Resources.razor`:
  content structure and truthful learning presentation.
- `Components/Account/Pages/Login.razor`, `Register.razor`,
  `Components/Account/Shared/ExternalLoginPicker.razor`: account presentation.
- `Components/App.razor`, `_Imports.razor`, `Services/Translations.cs`: integration.
- Other page edits replace nested main tags with sections or apply the public layout;
  profile/settings gain common headers and staff screens gain clearer headings.
- `wwwroot/bootstrap-icons/`: vendored existing icon set and license.
- `UI_REDESIGN_AUDIT.md`: route inventory, implementation audit and risks.
