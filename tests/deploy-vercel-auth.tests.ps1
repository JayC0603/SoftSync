$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path $PSScriptRoot -Parent
$parseErrors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile(
    (Join-Path $scriptRoot 'deploy-vercel.ps1'), [ref]$null, [ref]$parseErrors)
if ($parseErrors.Count) { throw 'Deployment script has syntax errors.' }
$functionAst = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Get-BlobUploadToken' }, $true)
. ([scriptblock]::Create($functionAst.Extent.Text))

# The extracted function runs here with a temporary, fake credential file.
New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot '.vercel') -Force | Out-Null
$script:lastCredentialPath = $null
$script:fakeValue = '"vercel_blob_rw_test_only"'
function Read-VercelOutput {
    param([string[]]$CliArguments)
    if ($CliArguments[4] -ne 'production') { throw 'Incorrect credential environment.' }
    $script:lastCredentialPath = $CliArguments[2]
    [System.IO.File]::WriteAllText($CliArguments[2], "BLOB_READ_WRITE_TOKEN=$script:fakeValue`n")
    return @{ ExitCode = 0; Text = '' }
}

foreach ($encoding in @('"vercel_blob_rw_test_only"', "'vercel_blob_rw_test_only'", 'vercel_blob_rw_test_only')) {
    $script:fakeValue = $encoding
    $value = Get-BlobUploadToken -TargetEnvironment production -ProjectRoot $PSScriptRoot
    if ($value -ne 'vercel_blob_rw_test_only') { throw 'Token decoding failed.' }
    if (Test-Path -LiteralPath $script:lastCredentialPath) { throw 'Temporary credentials were not removed.' }
}
$script:fakeValue = '""'
$rejected = $false
try { Get-BlobUploadToken -TargetEnvironment production -ProjectRoot $PSScriptRoot | Out-Null } catch { $rejected = $true }
if (-not $rejected) { throw 'Empty token was accepted.' }
if (Test-Path -LiteralPath $script:lastCredentialPath) { throw 'Temporary credentials leaked after failure.' }
Write-Output 'Passed: token decoding, environment selection, rejection and credential cleanup.'
