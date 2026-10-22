# Admin analytics and Gemini implementation

This change adds the first integrated implementation of the design in `ADMIN_ANALYTICS_GEMINI_PLAN.md`.

## Implemented

- Admin Gemini settings at `/admin/ai/settings`: up to three encrypted, masked API keys; model fields populated from the Google models API; priority ordering; per-key quota-group labels; connection check and cooldown/status display.
- A Gemini JSON gateway with a 90-second operation deadline, at most three provider attempts, `Retry-After` cooldown handling, same-quota-group skipping after rate limits, key invalidation on 401, and operation/token/latency records.
- SYNCY assistant and JSON-based CV, quiz, roleplay, and learning-evaluation calls use Gemini when it is enabled in the Admin settings. Existing Hugging Face configuration remains the fallback for those JSON evaluations and SYNCY when Gemini is disabled.
- `/admin/dashboard` shows live process CPU/memory, request/error counts, measured active learners, assessments, CV reviews, and AI operations. `/admin/learners` shows measured learning time, sessions, quiz averages, assessment averages, and CV-review counts per learner.
- Learning session endpoints and browser heartbeat tracking credit bounded, server-timed intervals on learning routes. Duplicate heartbeat sequence numbers are idempotent.
- Resource aggregates are pruned after 30 days and raw heartbeat events after 90 days; measured session totals remain available for learner summaries.
- PostgreSQL migration `20261022100000_AddAdminAnalyticsGemini` adds configuration, credential, AI-operation, resource-metric, and learning-session tables.
- Admin sign-in now lands on `/admin/dashboard`; desktop Admin navigation links to overview, learner analytics, and Gemini settings. No global learner/mobile stylesheet was changed.

## Deployment and limits

- Apply the new database migration before using the feature with PostgreSQL: `dotnet ef database update --project SoftSync.DAL --startup-project SoftSync.Presentation`.
- The current `DemoMode` uses an in-memory database. Admin keys/settings and historical analytics disappear when that demo process restarts; use PostgreSQL for persistence.
- API keys are protected through ASP.NET Core Data Protection using the application's configured key repository. Production deployment must keep that key repository durable and access-controlled; the existing repository is database-backed, so this does not meet the plan's stronger requirement for a master key stored separately from the database.
- A quota group is admin-entered metadata. Keys marked with the same group are skipped together after 429 because Google applies rate limits at project scope. Different projects should use different group labels.
- Learning activity tracking is limited to routes matched in `wwwroot/js/learning-activity.js`; add routes there when new learning screens are introduced.
- The current Admin metrics page reports process-level CPU/memory and HTTP request counts. Container quota, storage, database latency, and Blazor circuit counts remain future work.
- This environment has the .NET runtime but no .NET SDK, so the C# and Razor project could not be built here. The learning-activity JavaScript passed `node --check`; no test suite was run.
