# SoftSync Next

Vercel migration target for the existing ASP.NET Core application.

This is the first migration slice: a deployable Next.js shell. Existing .NET pages, Identity, domain services, and PostgreSQL schema remain in the original project until each slice is migrated and verified.

## Local

```powershell
npm install
npm run dev
```

## Vercel

Import this directory as the project root. Add `DATABASE_URL` only after the Prisma data layer is added; never commit credentials.
