param([switch]$SkipFrontend)
$ErrorActionPreference = 'Stop'
$cvRepo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$cvSuffix = [Guid]::NewGuid().ToString('N').Substring(0, 12)
$cvDb = "softsync-cv-acceptance-db-$cvSuffix"
$cvApp = "softsync-cv-acceptance-app-$cvSuffix"
function Invoke-CvDocker([string[]]$DockerArgs) {
    & docker @DockerArgs
    if ($LASTEXITCODE -ne 0) { throw "Docker command failed (exit $LASTEXITCODE)." }
}
try {
    Invoke-CvDocker @('run','--rm','-d','--name',$cvDb,'-e','POSTGRES_HOST_AUTH_METHOD=trust','postgres:17')
    $cvReady = $false
    for ($cvTry = 0; $cvTry -lt 30; $cvTry++) {
        & docker exec $cvDb pg_isready -q -U postgres > $null
        if ($LASTEXITCODE -eq 0) { $cvReady = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $cvReady) { throw 'Temporary PostgreSQL did not become ready.' }
    $cvSdk = @('run','--rm','-v',"${cvRepo}:/src",'-v','softsync-cv-nuget:/root/.nuget/packages','--network',"container:$cvDb",'-w','/src','mcr.microsoft.com/dotnet/sdk:10.0')
    Invoke-CvDocker ($cvSdk + @('sh','-c','dotnet restore && dotnet build --no-restore'))
    Invoke-CvDocker @('run','--rm','-d','--name',$cvApp,'--network',"container:$cvDb",'-v',"${cvRepo}:/src",'-w','/src/SoftSync.Presentation','-e','ASPNETCORE_URLS=http://0.0.0.0:8080','-e','ConnectionStrings__SoftSyncDb=Host=localhost;Database=postgres;Username=postgres','-e','AiApi__ApiKey=','-e','AiApi__Enabled=true','-e','Logging__LogLevel__Microsoft.EntityFrameworkCore=Warning','mcr.microsoft.com/dotnet/aspnet:10.0','dotnet','bin/Debug/net10.0/SoftSync.Presentation.dll')
    $cvReady = $false
    for ($cvTry = 0; $cvTry -lt 30; $cvTry++) {
        & docker exec $cvDb bash -c 'exec 2>/dev/null; echo > /dev/tcp/127.0.0.1/8080' > $null
        if ($LASTEXITCODE -eq 0) { $cvReady = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $cvReady) { throw 'Acceptance web app did not become ready.' }
    Invoke-CvDocker @('run','--rm','-v',"${cvRepo}:/src",'-v','softsync-cv-nuget:/root/.nuget/packages','--network',"container:$cvDb",'-e','SOFTSYNC_CV_ACCEPTANCE_DB=Host=localhost;Database=postgres;Username=postgres','-e','SOFTSYNC_CV_ACCEPTANCE_URL=http://localhost:8080','-w','/src','mcr.microsoft.com/dotnet/sdk:10.0','dotnet','test','--no-build')
    Invoke-CvDocker ($cvSdk + @('sh','-c','dotnet tool install dotnet-ef --version 10.0.0 --tool-path /tmp/ef && /tmp/ef/dotnet-ef migrations has-pending-model-changes --project SoftSync.DAL --startup-project SoftSync.Presentation --no-build'))
    if (-not $SkipFrontend) {
        Invoke-CvDocker @('run','--rm','-v',"${cvRepo}:/src",'-v','/src/SoftSync.Presentation/node_modules','-w','/src/SoftSync.Presentation','node:20','sh','-c','npm ci && npm run build && node Scripts/check-ui-css.mjs')
    }
    $cvPreviousErrorPolicy = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & git -C $cvRepo diff --check 2>$null
    $cvGitExit = $LASTEXITCODE
    $ErrorActionPreference = $cvPreviousErrorPolicy
    if ($cvGitExit -ne 0) { throw 'git diff --check failed.' }
    Write-Host 'CV acceptance PASS; real AI tests are deliberately skipped. Browser E2E is separate.'
}
finally {
    # Only the two randomly named containers created by this script are stopped.
    foreach ($cvContainer in @($cvApp, $cvDb)) {
        $cvRunning = & docker ps -q --filter "name=^${cvContainer}$"
        if ($cvRunning) { & docker stop $cvContainer | Out-Null }
    }
}
