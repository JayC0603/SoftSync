# SoftSync Next

Vercel migration target for the existing ASP.NET Core application.

This is the first migration slice: a deployable Next.js shell backed by the
ASP.NET Core API on Render. Existing .NET pages, Identity, and domain services
remain in the original project until each slice is migrated and verified.

## Local

```powershell
npm install
npm run dev
```

## Vercel

Import this directory as the project root and set:

```text
BACKEND_URL=https://your-render-service.onrender.com
```

The frontend must not receive `DATABASE_URL` or connect to PostgreSQL directly.
After changing an environment variable, redeploy the Vercel project.
