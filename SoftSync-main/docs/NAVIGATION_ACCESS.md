# Shared navigation and learner access

SoftSync uses ASP.NET Identity authentication state and existing saved assessment results. No database schema, API contract, assessment scoring or personalization algorithm changes are required.

## Shared components and state

- `LearnerAccessState` derives completion through `IAssessmentService.GetLatestResultsAsync` for the authenticated owner. Unknown, loading, incomplete, complete and failed lookup states remain distinct. Concurrent consumers share one lookup; authentication changes invalidate results and late responses cannot restore a previous identity.
- Database reads use a private dependency-injection scope so layout and page queries do not share a busy DbContext.
- `NavigationAccessRules` owns route eligibility and the More route group. `NavBar` retains its current icons, geometry and role-specific menus. A server-verified, identity-bound completion snapshot avoids a dock flash during hydration; failed lookups never use it.
- `LearnerRouteGate` runs before the page is mounted. Guests use the existing login redirect and safe ReturnUrl handling. Unassessed learners go to `/profile-setup` if required profile fields are missing, otherwise `/assessment/{id}`. Failed lookups show a reload/retry action instead of redirecting to an assessment.
- `MainLayout` and `PublicLayout` select the authenticated or public shell from Identity. Public pages reuse `PublicNavigation`; authenticated pages reuse `HeaderActions`, `NavBar` and the existing footer.
- The guest header has a single flag dropdown, existing theme toggle, registration CTA and a native modal drawer with keyboard focus containment, Escape and backdrop dismissal. Language selection is absent from the drawer.
- `Assessment` invalidates cached access only after successful server submission. Its draft restoration also waits for questions to exist, including initialization that finishes after the first render.
- `RoadmapService` rejects student roadmap reads/creation when no active assessment result exists, before invoking personalization or persisting a new roadmap. Stored Admin/Teacher roles retain their existing behavior.

## Route behavior

| Routes | Guest | Signed in without assessment | Assessed learner |
| --- | --- | --- | --- |
| `/`, `/about`, `/mentors`, `/community`, `/resources`, `/game` | Public header, no dock | Authenticated header, no dock | Authenticated header, eligible dock |
| `/dashboard`, `/roadmap/**`, `/roadmap-legacy/**`, `/progress/**` | Login with ReturnUrl | Complete profile or assessment | Existing personalized page |
| `/profile`, `/settings`, mentor support and other account features | Existing login protection | Remain accessible where existing policy permits | Existing access |
| `/Account/**` | Existing static authentication layout | Existing account layout | No dock |
| `/assessment/**`, `/assessment-result/**`, `/assessment-complete/**`, `/profile-setup`, `/select-skills/**` | Existing login protection | No dock | No dock, including explicit retakes |
| Admin/Teacher routes | Existing authorization | Existing role-specific navigation, no learner assessment requirement | Existing role-specific navigation |

`/game` exposes only the directory to guests. Its existing user-policy variants, attempt history, results and history API remain protected. `/resources` is the existing static resource catalog; opening protected learning destinations still goes through their access checks. CV and published courses retain their existing policies because they are not themselves personalized roadmap routes.

The profile prerequisite matches the existing wizard: name, age and goal. Gender is optional, and an empty skill selection retains the existing assessment fallback. Re-taking an assessment does not revoke a previously saved completion.

## Validation

- Full .NET suite: 303 passed, 11 skipped, 0 failed. Existing environment-dependent and explicitly skipped tests remain skipped.
- New coverage includes role/route dock rules, persisted results rather than XP, failed lookup and retry, completion invalidation, refresh, authentication changes, shared concurrent lookup, late lookup after logout and backend rejection before creating an unassessed roadmap.
- Existing learning fixtures now explicitly represent assessed learners, preserving their progress/reload assertions.
- Guest HTTP checks: landing, About, Mentor, Resources, Community, Mini Game, login and protected roadmap redirect.
- Browser suite passed in the existing ephemeral DemoMode: guest headers/drawer at 320/390/430px, language/theme, unassessed sign-in, public/account pages without a dock, protected learning redirects, real assessment answers and draft restoration after refresh, completion and dock on Dashboard/Learn/Progress/Mentor/Resources/Community/Game, More active state, refresh, new registration/profile prerequisite, logout and re-login without reassessment. No browser runtime exceptions. Results: `navigation-access-browser-results.json`. Reproducible script: `../../tests/navigation-access.browser.mjs` using APP_URL/CDP_URL/CHECK_OUTPUT_DIR.

## Existing TBDs

Help, Contact and Privacy destination URLs are already TBD in the shared footer. The drawer preserves disabled entries until real destination content/URLs are supplied; it does not add placeholder routes. Expiry invalidation follows the existing Identity security-stamp revalidation provider; its polling interval is unchanged.

## Changed files

- `SoftSync.BLL/Services/BusinessServices.cs`: enforce assessment prerequisite before roadmap generation.
- `SoftSync.Presentation/Services/LearnerAccessState.cs`: shared lookup/cache, identity invalidation and route rules.
- `SoftSync.Presentation/Components/Shared/LearnerRouteGate.razor` and `Components/Routes.razor`: shared page access guard and retry state.
- `SoftSync.Presentation/Components/Layout/{MainLayout,PublicLayout,PublicNavigation,NavBar}.razor`: shared shell selection, public controls and dock eligibility.
- `SoftSync.Presentation/Components/Pages/{Assessment,Game,Resources}.razor`: completion invalidation/draft restoration and public catalog access.
- `SoftSync.Presentation/Components/App.razor`, `Program.cs`, `wwwroot/css/navigation-access.css`, `wwwroot/js/page-navigation.js`: asset registration, DI, drawer styles and grouped active links.
- `SoftSync.Tests/NavigationAccessTests.cs`, `LearningWorkflowFixTests.cs`, `CoreWorkflowTests.cs`: state/routing/backend tests and assessed learning fixtures.
- `tests/navigation-access.browser.mjs`: local end-to-end verification.
