# SoftSync Next (Vercel frontend)

Vercel frontend for the existing ASP.NET Core application.

This is the first migration slice: a deployable Next.js shell backed by the
ASP.NET Core API. Existing .NET pages, Identity, and domain services
remain in the original project until each slice is migrated and verified.

## Local

```powershell
npm install
npm run dev
```

## Vercel

Import `SoftSync.Next` as the Vercel project root. Vercel detects Next.js and
runs `npm run build` automatically. Set this server-side environment variable:

```text
BACKEND_URL=https://your-backend-service.onrender.com
```

`BACKEND_URL` is the public ASP.NET Core API URL, not the frontend's
`*.vercel.app` URL. Do not expose `DATABASE_URL` to the frontend or connect to
PostgreSQL directly.

On the ASP.NET Core host, allow the deployed frontend origin:

```text
FRONTEND_ORIGINS=https://your-project.vercel.app
```

Use a comma-separated list if both the production domain and another frontend
origin are required. After changing an environment variable, redeploy the
corresponding project.

## Complete CV dashboard

The `/cv` route opens CV Storage, CV-JD Matching, CV Ranking, CV Comparison,
and CV Searching inside the authenticated SoftSync workspace. Direct visits
redirect to the canonical SoftSync `/cv` page. Users sign in with their SoftSync
account once. Browser requests use the same-origin `/api/cv/*` proxy, so the
FastAPI service URL remains server-side. See [CV integration](../docs/CV_INTEGRATION.md)
for the server connection, backend migration, and deployment settings.

Set these variables in Vercel:

```text
CV_API_URL=https://your-cv-api-service.example.com
SOFTSYNC_APP_URL=https://softsync.id.vn
```

`CV_API_URL` must point to the FastAPI backend root without `/api/v1`. The
FastAPI backend must be able to reach its AI service and document storage.

## Homepage and course navigation

The homepage uses a responsive purple design with a product preview, an
asymmetric feature grid, mobile navigation, and links to the existing learning
and CV areas. The dashboard shown in the hero is explicitly labeled as sample
data; it does not represent the signed-in user's progress.

`/courses` reads published courses from the ASP.NET API. `/courses/[id]` reads
the public course details and provides a link to the original ASP.NET `/courses`
area for enrollment, lessons, and progress. Learning authentication stays in
that application and also opens the integrated CV workspace. API requests have a 10-second
timeout, and course pages provide unavailable, empty, and not-found states.

The existing CV dashboard and API proxy remain the entry points for CV storage,
matching, ranking, comparison, and searching. These operations require a working
CV backend and its AI/document services.
