# Persistent Smart CV workspace

The `/cv` workspace stores PDF/TXT originals, extracted text, and saved bilingual
reviews in PostgreSQL, scoped to the signed-in account. CV uploads are committed
immediately. Press **Save** on a completed analysis to retain its scores, feedback,
and the JD. Signing out, closing the browser, or restarting the application does
not remove these records when PostgreSQL is configured.

The original file can be downloaded from its library card. The download endpoint
validates the login security stamp and ownership; another account receives no file.
Deleting a CV removes its original file and extracted text but preserves saved
reviews. Reviews retain the CV name and JD after their source document is deleted.

## Deployment

- Use the existing persistent PostgreSQL `DATABASE_URL` or
  `ConnectionStrings__SoftSyncDb` configuration.
- Keep `SoftSync__DemoMode=false` (or unset) on the deployed application. Demo mode
  intentionally uses an in-memory database, which is cleared on process restart.
- The existing startup initializer applies the `AddPersistentCvWorkspace` migration
  automatically. It adds `CvDocuments` and `CvReviews`; it does not replace existing
  tables or data.
- AI review still requires `AiApi__Enabled=true` and a working `AiApi__ApiKey`,
  `AiApi__BaseUrl`, and `AiApi__Model` in the server configuration.
- Previous browser-session-only records are not imported; upload those original
  files again. No private CVs or API keys are included in source control.

## Verification

`CvWorkspacePersistenceTests` covers reopening with a fresh database context,
original-file integrity, account isolation, preserving reviews after deleting a
CV, idempotent review saves, and invalid uploads. The tests use EF InMemory;
PostgreSQL migration execution must also succeed on the deployment database.
