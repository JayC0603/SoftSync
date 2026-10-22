# Desktop branch integration

Base: `b997c94` on `master`.

The integration includes the remote `Update-Home`, `dev`,
`feature/ai-cv-review-ui-flexible-quiz`,
`feature/personal-schedule-roadmap-dark`,
`fix/full-width-learning-workspace`, and `update-cv` branches. It also includes
the local `Update-Home` commit `fa2d5b4`.

## Resolution choices

- Desktop means a viewport of at least 901 CSS pixels, matching the existing
  mobile navigation breakpoint. Imported visual styles are restricted by media
  queries. New navigation links are hidden below this breakpoint.
- Home, Dashboard, Community, Courses, Game, and Teacher Courses mount one
  presentation variant at a time. Existing mobile markup is retained rather
  than rendering two interactive forms or running two sets of page queries.
  The public Home route retains `PublicLayout` and its mobile SEO content.
- The mobile Game layout is retained. Its submission limit and progress labels
  now use the actual number of questions so the shared flexible-quiz backend
  remains usable on mobile.
- AI provider management, AI CV review, learning preferences, personal schedule,
  and the separate Python CV services are integrated as shared functionality.
  The existing persistent CV workspace remains available at `/cv`; the incoming
  AI review page is available at `/cv-review`.
- The EF snapshot contains both existing notification/CV workspace entities and
  incoming AI provider/CV review/schedule entities. Existing migrations are
  retained. No database was migrated as part of this integration.
- The legacy `dev` branch has an unrelated history and a different repository
  root. Its game bank content, randomized assessment options, active skill
  selection and PostgreSQL behavior already exist in newer implementations.
  The merge retains those implementations and excludes duplicate project trees.
- Outdated IDE caches, build outputs, runtime configuration and lockfiles from
  the historical Home branch are excluded. Out-of-project duplicate page/image
  files from the schedule branch are also excluded.

## Verification

- .NET build and tests: 263 passed, 11 environment-dependent acceptance tests
  skipped. Game rendering tests cover 19-, 20- and 30-question quizzes on both
  desktop and mobile, including incomplete submissions.
- Python CV authentication/integration tests: 10 passed with isolated SQLite.
- EF `migrations has-pending-model-changes`: no pending model changes.
- Browser smoke test with an isolated demo account: Dashboard at 390, 900, 901
  and 1440px; Settings, Courses, Game and Community at 390 and 1440px; creating
  a schedule task; opening the CV review upload page. No JavaScript exceptions
  or horizontal page overflow were observed.
- Source comparison against the base confirms the mobile Home, Dashboard,
  Community, Courses and Teacher Courses views are preserved. The shared mobile
  dashboard component and the mobile/home/learning/options CSS files are unchanged.

Live provider requests, CV AI model inference, PostgreSQL acceptance tests and
production deployment require their separately configured services. Existing
dependency vulnerability and framework warnings remain outside this merge.
