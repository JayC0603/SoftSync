# Vercel Services

For GitHub deployments, set **Root Directory** to the repository root (leave it
empty) and **Framework Preset** to **Services**. The root `vercel.json` declares
the `softsync` container, whose source is in `SoftSync-main/`.

If Root Directory is explicitly `SoftSync-main`, that directory also contains
a `vercel.json` with the service root set to `.`. Do not select
`SoftSync-main/SoftSync.Presentation` or `SoftSync-main/SoftSync.Next` as the root.

`MISSING_SERVICES` occurs when the selected root does not contain a deployed
`vercel.json` declaring at least one service. Both configuration files and
`SoftSync-main/Dockerfile.vercel` must be included in the Git commit used by
Vercel. Files that exist only on the developer's machine do not reach GitHub
deployments.

After pushing these files, redeploy the latest commit. Preserve the existing
environment variables and domain configuration. No database or storage store
needs to be recreated for this configuration fix.

Reference: https://vercel.com/kb/guide/vercel-services

## Blazor connections

`softsync-tech.vercel.app` is an alias of the `softsync` Services project.
The separate `softsync-next` project has its own deployment status and is not
the Blazor application. Check the deployment linked to the domain before
changing hosting settings.

Blazor starts through `wwwroot/js/blazor-start.js` with a direct WebSocket to
`/_blazor`. Skipping the separate negotiation request prevents a connection ID
created on one Vercel instance from being used on another. Client and server
use a 60-second timeout and a 15-second keep-alive interval. Short disconnects
are retried with bounded delays; inactive circuits remain available for up to
five minutes on their original instance. Startup failures expose the existing
reload action rather than silently disabling interactive controls.

Vercel closes WebSockets when a Function reaches its maximum duration. A new
connection can reach a different instance; in-memory Blazor circuits cannot
survive instance replacement or redeployment. These settings reduce negotiation
failures and premature timeouts, but cannot remove that platform limit. A
discarded circuit requires a fresh page. The reconnect handler first tries to
resume the old circuit. When a healthy server rejects it, read-only pages reload
automatically once, with a one-minute guard against reload loops. Pages with
edited fields or autofilled credentials keep their input and show a native
reload button instead. Offline retries stop after three attempts, so the user
is never trapped behind a permanent reconnect spinner. Returning to a visible
tab or restoring the network retries a previously failed recovery.

Validate the startup configuration with `node --test tests/blazor-start.test.mjs`.
On a deployed page, the browser Network panel should show a WebSocket request
to `/_blazor` with status `101`, without a `/_blazor/negotiate` request.

References: [Vercel WebSockets](https://vercel.com/docs/functions/websockets),
[Blazor SignalR configuration](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-10.0).
