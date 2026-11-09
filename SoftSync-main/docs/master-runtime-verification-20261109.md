# Master runtime verification, 2026-11-09

The expanded acceptance run uses a published Release application in Production
mode and a disposable PostgreSQL 16 database. It does not use DemoMode or the
production database.

The run found two route regressions: `/courses` rendered the older component
with every lesson video inline, bypassing the searchable catalog, and
`/teacher/courses` rendered the older editor without `managedQuizId` support.
Both routes now use the existing components that implement those workflows.

Acceptance setup now submits the registration form's required AcceptTerms field.
The CV foreign-key assertion is scoped to public so the AI tests' independent
schemas cannot inflate its count. The English history assertion uses the
current Quiz History title and still rejects the Vietnamese title.

Validation after the fixes:

- Release publish completed and the published app started in Production mode.
- PostgreSQL migrations, Identity registration/login, learner/teacher/admin
  authorization, private CV data, schedule persistence and 19-question quizzes
  passed: 415 tests passed, 6 skipped, 0 failed.
- The skipped secret restart test passed separately in write and read processes.
  The other five skipped tests need live AI providers or recorded samples.
- All 14 JavaScript motion and Blazor reconnect tests passed.
- Deployment token handling checks and Docker Compose validation passed.
- Health, home, login, public courses API and built CSS/JS responded with HTTP 200.
- Dockerfile.vercel built successfully on Linux. Its Production container started
  against the disposable PostgreSQL database via DATABASE_URL; all three HTTP
  acceptance workflows and the health/static-asset checks passed in the container.
  The local database uses sslmode=disable; hosted PostgreSQL should require TLS.

Google OAuth, live AI/OCR, Blob uploads and the hosting provider's production
environment were not exercised by this isolated run.
