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
