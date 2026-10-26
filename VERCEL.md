# Deploy SoftSync to Vercel

Import this repository with Root Directory set to the directory containing this
file. `vercel.json` builds the complete ASP.NET Core / Blazor application using
the Vercel Container Runtime. All routes, account pages, APIs, and static assets
use the same origin. Do not select the `SoftSync.Next` shell as the project root.

Required runtime environment variables in Vercel Project Settings:

```dotenv
DATABASE_URL=postgresql://USER:PASSWORD@HOST:5432/DATABASE?sslmode=require
BLOB_READ_WRITE_TOKEN=
BootstrapAdmin__Email=
BootstrapAdmin__Password=
```

Create a Neon PostgreSQL database through Vercel Marketplace and a **public**
Vercel Blob store, then connect both to the project. Database creation, project
connection, and billing are managed from Vercel; no previous hosting service is
required. PostgreSQL runs on Neon, not inside the application container.
Use separate databases and Blob stores for production and preview environments.
Existing production data must be migrated into that database before switching
the domain. Database migrations run when the app starts. Identity encryption
keys already live in PostgreSQL, so cookies survive container replacement.
The application accepts `DATABASE_URL` or `POSTGRES_URL`. Migrations prefer
`DATABASE_URL_UNPOOLED` or `POSTGRES_URL_NON_POOLING` for a direct connection.
Course images and videos upload to Blob; existing local uploads need copying
and their saved URLs updating before removing the previous host.

Optional runtime variables:

```dotenv
Authentication__Google__ClientId=
Authentication__Google__ClientSecret=
FRONTEND_ORIGINS=https://YOUR-DOMAIN
CvIntegration__FrontendUrl=
CvIntegration__ApiBaseUrl=
CvIntegration__SharedSecret=
RecruitmentAi__BaseUrl=
```

Register `https://YOUR-DOMAIN/signin-google` with Google OAuth. Set email/SMS
and AI credentials with the same ASP.NET `Section__Key` names used by the app.
External CV/AI services must have reachable URLs; they are not included in this
repository. Do not use localhost URLs for those services in production.

Blazor uses a direct WebSocket connection to avoid negotiation reaching a
different instance. Verify login, interactive navigation, uploads, and
reconnection on a preview deployment before switching the production domain.
Circuit state is in memory and can reset when a container is replaced.

First-time setup from this directory, after installing Node.js:

```powershell
npx --yes vercel@65.0.0 login
.\deploy-vercel.ps1 -ProvisionDatabase -ProvisionBlob
```

The script links the project, provisions and connects the database and Blob
store to production, checks the required environment variable names, and
deploys production. Select the desired plan in the integration setup prompts.
Before deployment, it uploads the bundled lesson videos to public Blob storage
using multipart uploads. Large MP4 files are excluded from both the deployment
upload and container image. A generated video manifest preserves `/videos/*`
URLs through redirects. Upload progress is cached under `.vercel/`; retrying the
script reuses completed uploads. This first upload transfers about 1.9 GB, so
check the selected Blob plan's storage allowance.
Provisioning switches create new resources; omit them for subsequent deploys:

```powershell
.\deploy-vercel.ps1
```

For preview deployments, connect a separate database and Blob store to the
preview environment first, then run `.\deploy-vercel.ps1 -Preview`.

The checked-in [database/schema.sql](SoftSync-main/database/schema.sql)
contains idempotent SQL generated from all EF migrations, including schema and
model seed data. The startup initializer also seeds application roles and an
optional administrator. It does not create a production demo account. To run
initialization explicitly with database environment variables already set:

```powershell
dotnet run --project SoftSync-main/SoftSync.Presentation -c Release -- --initialize-database
```

To regenerate SQL without database access:

```powershell
dotnet run --project SoftSync-main/SoftSync.Presentation -c Release -- --export-database-schema=SoftSync-main/database/schema.sql
```

Inspect `/health` and complete an account/database write after deployment.
Docker and a running Docker daemon are required for local `vercel dev` or
container build validation.

References: [Container runtime](https://vercel.com/docs/functions/runtimes/container),
[Services](https://vercel.com/kb/guide/vercel-services),
[Blob](https://vercel.com/docs/vercel-blob).
