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

For this repository layout, set **Root Directory** to
`SoftSync-main/SoftSync.Next` in Vercel Settings > Build and Deployment.
Do not select `SoftSync.Presentation` (Blazor Server) or the repository root.
If you import the inner `SoftSync-main` directory as its own repository instead,
the Root Directory is `SoftSync.Next`.

Keep **Framework Preset: Next.js**, leave **Output Directory** at its default,
and use Node.js **22.x**. Do not set the output directory to `wwwroot`, `dist`,
or `.next/standalone`; Vercel packages Next.js itself. `vercel.json` pins
`npm ci` and `npm run build` for this frontend.

Vercel detects Next.js and
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

The Docker Compose stack is for local verification. This deployment setup uses
Vercel for Next.js and Render (Docker) for ASP.NET Core; it does not deploy
`docker-compose.yml` to Vercel. The Next.js shell does not replace the full
Blazor Server interface: learning, accounts and the integrated CV workspace
still need the running ASP.NET host.

### Verify locally with Docker

From the parent `SoftSync-main` directory:

```powershell
Copy-Item .env.example .env
docker compose config --quiet
docker compose up --build
```

Open `http://localhost:3000` for Next.js, `http://localhost:8081` for the full
ASP.NET application, and `http://localhost:3000/api/health` to check the backend
connection. Compose uses `http://web:8080` for server requests and
`SOFTSYNC_APP_URL=http://localhost:8081` for browser links. A CV backend must
also be configured through `CV_API_URL` to use CV processing.

### Deploy order and troubleshooting

1. Deploy the ASP.NET Docker service using the repository's `render.yaml`.
   Check that `https://<backend>/health` returns HTTP 200.
2. In Vercel, set `BACKEND_URL` and `SOFTSYNC_APP_URL` to the public HTTPS
   ASP.NET URL. Configure `CV_API_URL` if using CV processing.
3. On the ASP.NET host, set `FRONTEND_ORIGINS` and
   `CvIntegration__FrontendUrl` to the public Vercel URL. Configure the CV
   shared secret and API on that host as described in the integration guide.
4. Redeploy both services after changing their environment variables.
   Verify `/`, `/courses`, `/cv`, and `/api/health` on the deployed frontend.

If Vercel reports a missing `package.json` or cannot detect Next.js, check the
Root Directory. If the homepage loads but `/api/health` returns 503, check
`BACKEND_URL` and the backend logs. Never use `localhost`, `web:8080`, a database
URL, or the Vercel frontend URL as the production `BACKEND_URL`.

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
