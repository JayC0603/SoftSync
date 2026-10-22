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

    $deployArguments = @('deploy')
    if (-not $Preview) { $deployArguments += '--prod' }
    Invoke-Vercel -CliArguments $deployArguments
}
finally {
    Pop-Location
}
