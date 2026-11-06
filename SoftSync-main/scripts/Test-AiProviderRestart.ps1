$ErrorActionPreference = 'Stop'
$aiRepo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$aiDb = 'softsync-ai-restart-' + [Guid]::NewGuid().ToString('N').Substring(0, 12)
try {
    & docker run --rm -d --name $aiDb -e POSTGRES_HOST_AUTH_METHOD=trust postgres:17
    if ($LASTEXITCODE -ne 0) { throw 'Test PostgreSQL startup failed.' }
    $aiReady = $false
    for ($aiTry = 0; $aiTry -lt 30; $aiTry++) {
        & docker exec $aiDb pg_isready -q -U postgres > $null
        if ($LASTEXITCODE -eq 0) { $aiReady = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $aiReady) { throw 'Test PostgreSQL unavailable.' }
    foreach ($aiPhase in @('write','read')) {
        # Separate containers/processes; only PostgreSQL persists the app configuration and key ring.
        & docker run --rm -v "${aiRepo}:/src" -v softsync-cv-nuget:/root/.nuget/packages --network "container:$aiDb" -e 'SOFTSYNC_CV_ACCEPTANCE_DB=Host=localhost;Database=postgres;Username=postgres' -e "AI_PROVIDER_RESTART_PHASE=$aiPhase" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test --no-build --filter FullyQualifiedName~Secret_survives_process_restart
        if ($LASTEXITCODE -ne 0) { throw "Restart acceptance phase $aiPhase failed." }
    }
    Write-Host 'AI secret restart persistence PASS (two independent processes; synthetic provider transport).'
}
finally {
    $aiRunning = & docker ps -q --filter "name=^${aiDb}$"
    if ($aiRunning) { & docker stop $aiDb | Out-Null }
}
