# CV as a SoftSync feature

The canonical entry point is the authenticated ASP.NET `/cv` page. It uses the
normal SoftSync navigation and account menu and hosts the existing React CV
tools. Next.js `/cv` redirects to the canonical site for direct visits. The
embedded view accepts session messages only from the configured SoftSync origin
and parent window. Users sign in to SoftSync once; there is no additional CV
registration or password prompt.

The ASP.NET server exchanges a signed, 60-second, single-use assertion with the
CV API. The shared secret remains on the two servers. The CV service maps the
immutable SoftSync issuer/account ID to a CV user, without merging local or
Google accounts that happen to share an email. The React view keeps its session
in memory and renews it through the current SoftSync session before expiry. It
does not persist integration tokens in browser storage. The session endpoint is
authenticated, checks the current Identity security stamp/lockout, validates an
antiforgery token, and disables caching. Logging out uses the SoftSync account
menu; the CV frame has no separate logout or account registration flow.

Google sign-in is handled exclusively by the existing SoftSync
`/Account/PerformExternalLogin` and `/signin-google` flow. Returning Google users
receive the same Identity session as password users and open CV without another
provider button. Both relative `cv` and absolute-local `/cv` return paths are
normalized to `/cv` and retained through protected OAuth state. Configure
`Authentication__Google__ClientId` and `Authentication__Google__ClientSecret` on
ASP.NET (or the existing `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` aliases).
The authorized redirect URI in Google must be the public SoftSync origin followed
by `/signin-google`; the CV frontend does not need separate Google credentials.

The parent sends the active semantic colors and light/dark/reduced-motion
preferences to the CV view over the same origin-checked message channel. A theme
switch updates both surfaces immediately, including form fields, cards, dialogs,
and tables. The CV action buttons use the SoftSync violet.

The existing CV library and AI workflows retain their current business behavior.
The CV library is the existing shared recruitment workspace; this change does
not migrate old files or automatically merge users' older CV accounts. Enabling
the integration requires authenticated access to CV API operations. Existing
static/Cloudinary document links retain their existing sharing behavior.

## Configuration

ASP.NET / Render:

```text
CvIntegration__FrontendUrl=https://your-next-frontend.vercel.app
CvIntegration__ApiBaseUrl=https://your-cv-api.example.com
CvIntegration__SharedSecret=<random-secret-at-least-32-UTF8-bytes>
CvIntegration__Issuer=softsync
```

Next.js / Vercel (server-side variables):

```text
SOFTSYNC_APP_URL=https://softsync.id.vn
CV_API_URL=https://your-cv-api.example.com
```

FastAPI CV backend:

```text
SOFTSYNC_SHARED_SECRET=<same-secret-as-ASP.NET>
SOFTSYNC_ISSUER=softsync
```

Generate a random secret locally; do not paste it into chat or commit it. Use the
exact public SoftSync origin in `SOFTSYNC_APP_URL`, including its scheme. Do not
put a trailing path in any origin. When deployed on different domains, allow the
SoftSync origin in any frontend `frame-ancestors`/hosting policy. Parent and
embedded application must both use HTTPS in production.

The backend implementation is in the existing `Capstone Project/be` service:
`app/api/v1/endpoints/softsync.py`, `app/api/softsync_access.py`,
`app/models/softsync_identity.py`, the additions to configuration/auth/main, and
the `20261011_softsync_identity` migration. Deploy those changes with the frontend
and ASP.NET changes. A portable copy is included in
`docs/cv-backend-integration.patch`; apply it from the root of an unchanged CV
backend checkout using `git apply <path-to-patch>`. The working backend in this
workspace already contains those changes. For databases managed with Alembic, run `alembic upgrade
head` in that backend before enabling the integration. Startup `create_all`
also discovers the new identity table in deployments that use that mechanism.

## Validation

- Both ASP.NET and Next.js production builds.
- Backend assertion/profile/authentication tests in
  `Capstone Project/be/tests/test_softsync_integration.py` using isolated SQLite.
- Browser integration check: anonymous redirect, one SoftSync demo sign-in,
  automatic CV session, all five tools, authorization forwarding, desktop/mobile
  layout, rejected message origin, denied CSRF-less session creation, account
  logout and return to learning. AI responses can be mocked for this check;
  actual extraction/matching still requires the deployed AI and storage services.
