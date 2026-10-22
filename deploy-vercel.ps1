[CmdletBinding()]
param(
    [switch]$ProvisionDatabase,
    [switch]$ProvisionBlob,
    [switch]$Preview
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-Vercel {
    param([string[]]$CliArguments)
    & npx.cmd --yes vercel@65.0.0 @CliArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Vercel command failed. Resolve the reported error before retrying."
    }
}

function Read-VercelOutput {
    param([string[]]$CliArguments)
    # Windows PowerShell treats redirected native stderr as error records.
    # CLI progress on stderr must not turn a successful command into a failure.
    $ErrorActionPreference = 'Continue'
    $commandOutput = & npx.cmd --yes vercel@65.0.0 @CliArguments 2>&1
    return @{ ExitCode = $LASTEXITCODE; Text = ($commandOutput -join "`n") }
}

Push-Location $PSScriptRoot
try {
    $identity = Read-VercelOutput -CliArguments @('whoami')
    if ($identity.ExitCode -ne 0 -or $identity.Text -match 'Logged out|No existing credentials|not authenticated') {
        throw 'Sign in first with: npx --yes vercel@65.0.0 login'
    }
    if (-not (Test-Path -LiteralPath '.vercel/project.json')) {
        Invoke-Vercel -CliArguments @('link')
    }

    # Explicit switches avoid provisioning duplicate stores on ordinary deploys.
    # Review the integration's displayed plan before accepting its setup prompts.
    if ($ProvisionDatabase) {
        Invoke-Vercel -CliArguments @('integration', 'add', 'neon', '--name', 'softsync-db', '--environment', 'production', '--no-env-pull')
    }
    if ($ProvisionBlob) {
        Invoke-Vercel -CliArguments @('blob', 'create-store', 'softsync-courses', '--access', 'public', '--region', 'sin1', '--environment', 'production')
    }

    $target = if ($Preview) { 'preview' } else { 'production' }
    $environmentListing = Read-VercelOutput -CliArguments @('env', 'ls', '--environment', $target, '--no-color')
    if ($environmentListing.ExitCode -ne 0) { throw 'Unable to read project environment configuration.' }
    $environmentNames = $environmentListing.Text
    if ($environmentNames -notmatch '\b(DATABASE_URL|POSTGRES_URL)\b') {
        throw "Connect a Neon database to the $target environment before deploying. Use -ProvisionDatabase for the first production setup."
    }
    if ($environmentNames -notmatch '\bBLOB_READ_WRITE_TOKEN\b') {
        throw "Connect a public Blob store exposing BLOB_READ_WRITE_TOKEN to the $target environment before deploying."
    }

    # Large lesson videos use Blob's multipart upload instead of the deployment
    # upload API. Keep a local hash cache so retrying a deploy can resume.
    $videoDirectory = Join-Path $PSScriptRoot 'SoftSync-main/SoftSync.Presentation/wwwroot/videos'
    $manifestPath = Join-Path $PSScriptRoot 'SoftSync-main/SoftSync.Presentation/wwwroot/video-manifest.json'
    $cachePath = Join-Path $PSScriptRoot ".vercel/video-cache-$target.json"
    $videoCache = @{}
    if (Test-Path -LiteralPath $cachePath) {
        $cached = Get-Content -LiteralPath $cachePath -Raw | ConvertFrom-Json
        foreach ($entry in $cached.PSObject.Properties) { $videoCache[$entry.Name] = $entry.Value }
    }
    $videoManifest = @{}
    if (Test-Path -LiteralPath $videoDirectory) {
        foreach ($video in Get-ChildItem -LiteralPath $videoDirectory -File -Filter '*.mp4') {
            $hash = (Get-FileHash -LiteralPath $video.FullName -Algorithm SHA256).Hash
            $entry = $videoCache[$video.Name]
            if ($null -eq $entry -or $entry.Hash -ne $hash) {
                Write-Host "Uploading lesson video to Blob: $($video.Name)"
                # Hash-based names allow safe retries without overwriting media.
                $pathname = "videos/$($hash.ToLowerInvariant())/$($video.Name)"
                $upload = Read-VercelOutput -CliArguments @('blob', 'put', $video.FullName, '--pathname', $pathname, '--content-type', 'video/mp4', '--access', 'public', '--allow-overwrite', 'true', '--multipart', 'true', '--no-color')
                if ($upload.ExitCode -ne 0) { throw "Blob upload failed for $($video.Name): $($upload.Text)" }
                $url = [regex]::Match($upload.Text, 'https://[a-zA-Z0-9-]+\.public\.blob\.vercel-storage\.com/[^\s"<>]+').Value
                if ([string]::IsNullOrEmpty($url)) { throw "No public Blob URL returned for $($video.Name)." }
                $entry = @{ Hash = $hash; Url = $url }
                $videoCache[$video.Name] = $entry
                [System.IO.File]::WriteAllText($cachePath, ($videoCache | ConvertTo-Json -Depth 4))
            }
            $videoManifest[$video.Name] = $entry.Url
        }
    }
    if ($videoManifest.Count -eq 0) { throw 'No lesson videos found. Restore the video files with git lfs pull before deploying.' }
    [System.IO.File]::WriteAllText($manifestPath, ($videoManifest | ConvertTo-Json))

    $deployArguments = @('deploy')
    if (-not $Preview) { $deployArguments += '--prod' }
    Invoke-Vercel -CliArguments $deployArguments
}
finally {
    Pop-Location
}
