param(
    [string]$Dotnet = 'dotnet',
    [int]$GatewayPort = 7463,
    [int]$ApiPort = 7464,
    [switch]$KeepRunning
)
# Records the demonstration videos in docs/demo against the packaged applications, a freshly
# published API and a disposable LocalDB database. It mirrors smoke-platform.ps1 deliberately:
# same adapters, same gateway, a different pair of ports and its own database, so a recording
# never touches the development database or an isolated smoke run.
$ErrorActionPreference = 'Stop'
$studioRoot = Split-Path -Parent $PSScriptRoot
if ($GatewayPort -lt 1024 -or $GatewayPort -gt 65535 -or $ApiPort -lt 1024 -or $ApiPort -gt 65535 -or $GatewayPort -eq $ApiPort) { throw 'Supply two distinct TCP ports between 1024 and 65535.' }
foreach ($port in @($GatewayPort, $ApiPort)) {
    if (Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue) { throw "Port $port is already in use. Choose unused ports before recording." }
}
foreach ($site in @('marketing', 'admin', 'client')) {
    if (-not (Test-Path (Join-Path $studioRoot "frontend/dist/$site/browser/index.html"))) { throw "frontend/dist/$site is missing. Run 'npm run build:libs' and 'npm run build:apps' in frontend first." }
}
if (-not (Test-Path (Join-Path $studioRoot 'e2e/node_modules/@playwright/test'))) { throw "e2e/node_modules is missing. Run 'npm ci' and 'npx playwright install chromium' in e2e first." }
$runDirectory = Join-Path $studioRoot '.artifacts/demo'
$outputDirectory = Join-Path $studioRoot 'docs/demo'
New-Item -ItemType Directory -Force $runDirectory, $outputDirectory | Out-Null
$databaseName = 'QbsDemo_' + (Get-Date -Format 'yyyyMMddHHmmss')
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
$env:Development__Controlled = 'true'
$env:ConnectionStrings__Studio = "Server=(localdb)\MSSQLLocalDB;Database=$databaseName;Integrated Security=true;Encrypt=true;TrustServerCertificate=true"
$env:ASPNETCORE_URLS = "http://127.0.0.1:$ApiPort"
$env:PublicOrigin = "https://localhost:$GatewayPort"
$env:Development__PhotoDirectory = Join-Path $runDirectory "photos/$databaseName"
$env:QBS_DEMO_ORIGIN = $env:PublicOrigin
$env:QBS_API_PORT = [string]$ApiPort
$env:QBS_GATEWAY_PORT = [string]$GatewayPort
# The address is typed on screen; the password is masked and never leaves this process.
$env:Bootstrap__Email = 'studio@quinntynebrown.example'
$env:Bootstrap__Password = 'Qbs!' + [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(12))
$apiDirectory = Join-Path $runDirectory 'api'
Write-Output "Publishing the API to $apiDirectory"
& $Dotnet publish (Join-Path $studioRoot 'backend/src/QuinntyneBrownStudio.Api') -c Release -p:UseAppHost=false -o $apiDirectory *> (Join-Path $runDirectory 'publish.log')
if ($LASTEXITCODE -ne 0) { throw 'API publishing failed. See .artifacts/demo/publish.log.' }
$apiDll = Join-Path $apiDirectory 'QuinntyneBrownStudio.Api.dll'
Write-Output "Migrating $databaseName"
& $Dotnet $apiDll --migrate *> (Join-Path $runDirectory 'migrate.log')
if ($LASTEXITCODE -ne 0) { throw 'Demo LocalDB migration failed. See .artifacts/demo/migrate.log.' }
& $Dotnet $apiDll --provision-admin *> (Join-Path $runDirectory 'provision.log')
if ($LASTEXITCODE -ne 0) { throw 'Demo administrator provisioning failed. See .artifacts/demo/provision.log.' }
$env:QBS_TLS_CERT = Join-Path $runDirectory 'localhost.pem'
$env:QBS_TLS_KEY = Join-Path $runDirectory 'localhost.key'
& $Dotnet dev-certs https --export-path $env:QBS_TLS_CERT --format PEM --no-password *> (Join-Path $runDirectory 'certificate.log')
if ($LASTEXITCODE -ne 0) { throw 'Development certificate export failed. See .artifacts/demo/certificate.log.' }
$apiProcess = Start-Process $Dotnet -ArgumentList @($apiDll) -WorkingDirectory $studioRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDirectory 'api.log') -RedirectStandardError (Join-Path $runDirectory 'api-error.log')
$gatewayProcess = Start-Process (Get-Command node).Source -ArgumentList @((Join-Path $studioRoot 'scripts/dev-gateway.mjs')) -WorkingDirectory $studioRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDirectory 'gateway.log') -RedirectStandardError (Join-Path $runDirectory 'gateway-error.log')
try {
    [ordered]@{ apiProcessId = $apiProcess.Id; gatewayProcessId = $gatewayProcess.Id; database = $databaseName; origin = $env:PublicOrigin } | ConvertTo-Json | Set-Content (Join-Path $runDirectory 'processes.json') -Encoding utf8
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        try { $response = Invoke-WebRequest "$env:PublicOrigin/api/health" -SkipCertificateCheck; if ($response.StatusCode -eq 200) { $ready = $true; break } } catch { }
        Start-Sleep -Milliseconds 500
    }
    if (-not $ready) { throw 'The demo gateway/API did not become ready. See .artifacts/demo/api-error.log.' }
    Write-Output "Recording against $env:PublicOrigin"
    Push-Location (Join-Path $studioRoot 'e2e')
    try { npx playwright test --config demo.playwright.config.ts; if ($LASTEXITCODE -ne 0) { throw 'The demonstration did not complete. See .artifacts/demo/recordings for the trace.' } }
    finally { Pop-Location }
    Write-Output 'Recordings:'
    Get-ChildItem $outputDirectory -Filter '*.webm' | ForEach-Object { Write-Output ("  {0}  {1:N1} MB" -f $_.Name, ($_.Length / 1MB)) }
    Write-Output "Chapter timings: $(Join-Path $runDirectory 'chapters')"
} finally {
    Remove-Item Env:Bootstrap__Email, Env:Bootstrap__Password -ErrorAction SilentlyContinue
    if ($KeepRunning) {
        Write-Output "Left running at $env:PublicOrigin with database $databaseName. Stop the processes in .artifacts/demo/processes.json when finished."
    } else {
        if (-not $gatewayProcess.HasExited) { Stop-Process -Id $gatewayProcess.Id }
        if (-not $apiProcess.HasExited) { Stop-Process -Id $apiProcess.Id }
        $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
        if ($sqlcmd) {
            & $sqlcmd.Source -S '(localdb)\MSSQLLocalDB' -Q "IF DB_ID(N'$databaseName') IS NOT NULL BEGIN ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$databaseName]; END" *> (Join-Path $runDirectory 'drop.log')
            if ($LASTEXITCODE -ne 0) { Write-Warning "Could not drop $databaseName. See .artifacts/demo/drop.log." }
        } else {
            Write-Warning "sqlcmd is not on PATH; the disposable database $databaseName was left in LocalDB."
        }
    }
}
