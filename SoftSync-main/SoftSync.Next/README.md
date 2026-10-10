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

The `/cv` route contains CV Storage, CV-JD Matching, CV Ranking, CV
Comparison, and CV Searching. Browser requests are sent to the same-origin
`/api/cv/*` proxy, so the FastAPI service URL remains server-side.

Set these variables in Vercel:

```text
CV_API_URL=https://your-cv-api-service.example.com
NEXT_PUBLIC_GOOGLE_CLIENT_ID=your-google-oauth-client-id
```

`CV_API_URL` must point to the FastAPI backend root without `/api/v1`. The
FastAPI backend must be able to reach its AI service and document storage.
