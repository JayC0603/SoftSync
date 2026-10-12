# Run the local preview with Google credentials from the ignored Docker .env.
# Import only OAuth settings; keep the demo database profile for local previews.
$ErrorActionPreference = 'Stop'
$previewEnvPath = Join-Path $PSScriptRoot '.env'
if (Test-Path -LiteralPath $previewEnvPath) {
    foreach ($previewLine in (Get-Content -LiteralPath $previewEnvPath)) {
        if ($previewLine -match '^\s*(GOOGLE_CLIENT_ID|GOOGLE_CLIENT_SECRET)\s*=\s*(.*)$') {
            $previewKey = $Matches[1]
            $previewValue = $Matches[2].Trim()
            if ($previewValue.Length -ge 2 -and
                (($previewValue.StartsWith('"') -and $previewValue.EndsWith('"')) -or
                 ($previewValue.StartsWith("'") -and $previewValue.EndsWith("'")))) {
                $previewValue = $previewValue.Substring(1, $previewValue.Length - 2)
            }
            if (-not [string]::IsNullOrWhiteSpace($previewValue) -and
                [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($previewKey))) {
                [Environment]::SetEnvironmentVariable($previewKey, $previewValue, 'Process')
            }
        }
    }
}
dotnet run --project (Join-Path $PSScriptRoot 'SoftSync.Presentation') --no-build --launch-profile demo
exit $LASTEXITCODE
