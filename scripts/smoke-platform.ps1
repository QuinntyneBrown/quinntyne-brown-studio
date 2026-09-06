param(
    [string]$Dotnet = 'dotnet',
    [int]$GatewayPort = 7453,
    [int]$ApiPort = 7454,
    [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$studioRoot = Split-Path -Parent $PSScriptRoot
if ($GatewayPort -lt 1024 -or $GatewayPort -gt 65535 -or $ApiPort -lt 1024 -or $ApiPort -gt 65535 -or $GatewayPort -eq $ApiPort) { throw 'Supply two distinct TCP ports between 1024 and 65535.' }
foreach ($port in @($GatewayPort, $ApiPort)) {
    if (Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue) { throw "Port $port is already in use. Choose unused ports before starting an isolated run." }
}
$runDirectory = Join-Path $studioRoot '.artifacts/platform/live'
New-Item -ItemType Directory -Force $runDirectory | Out-Null
$databaseName = 'QbsPlatformAcceptance_' + [Guid]::NewGuid().ToString('N')
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
$env:Development__Controlled = 'true'
$env:ConnectionStrings__Studio = "Server=(localdb)\MSSQLLocalDB;Database=$databaseName;Integrated Security=true;Encrypt=true;TrustServerCertificate=true"
$env:ASPNETCORE_URLS = "http://127.0.0.1:$ApiPort"
$env:PublicOrigin = "https://localhost:$GatewayPort"
$env:Development__PhotoDirectory = Join-Path $runDirectory $databaseName
$env:QBS_SMOKE_ORIGIN = $env:PublicOrigin
$env:QBS_API_PORT = [string]$ApiPort
$env:QBS_GATEWAY_PORT = [string]$GatewayPort
$env:Bootstrap__Email = 'platform-admin@example.test'
$env:Bootstrap__Password = 'Qbs!' + [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
$protectedPassword = ConvertTo-SecureString $env:Bootstrap__Password -AsPlainText -Force | ConvertFrom-SecureString
[ordered]@{ email = $env:Bootstrap__Email; protectedPassword = $protectedPassword; database = $databaseName; origin = $env:PublicOrigin } | ConvertTo-Json | Set-Content (Join-Path $runDirectory 'operator.json') -Encoding utf8
$apiDirectory = Join-Path $runDirectory 'api'
& $Dotnet publish (Join-Path $studioRoot 'backend/src/Qbs.Api') -c Release -p:UseAppHost=false -o $apiDirectory *> (Join-Path $runDirectory 'publish.log')
if ($LASTEXITCODE -ne 0) { throw 'API publishing failed. See the isolated run log.' }
$apiDll = Join-Path $apiDirectory 'Qbs.Api.dll'
& $Dotnet $apiDll --migrate *> (Join-Path $runDirectory 'migrate.log')
if ($LASTEXITCODE -ne 0) { throw 'Isolated LocalDB migration failed.' }
& $Dotnet $apiDll --provision-admin *> (Join-Path $runDirectory 'provision.log')
if ($LASTEXITCODE -ne 0) { throw 'Isolated administrator provisioning failed.' }
$env:QBS_TLS_CERT = Join-Path $runDirectory 'localhost.pem'
$env:QBS_TLS_KEY = Join-Path $runDirectory 'localhost.key'
& $Dotnet dev-certs https --export-path $env:QBS_TLS_CERT --format PEM --no-password *> (Join-Path $runDirectory 'certificate.log')
if ($LASTEXITCODE -ne 0) { throw 'Development certificate export failed.' }
$apiProcess = Start-Process $Dotnet -ArgumentList @($apiDll) -WorkingDirectory $studioRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDirectory 'api.log') -RedirectStandardError (Join-Path $runDirectory 'api-error.log')
$gatewayProcess = Start-Process (Get-Command node).Source -ArgumentList @((Join-Path $studioRoot 'scripts/dev-gateway.mjs')) -WorkingDirectory $studioRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDirectory 'gateway.log') -RedirectStandardError (Join-Path $runDirectory 'gateway-error.log')
try {
    [ordered]@{ apiProcessId = $apiProcess.Id; gatewayProcessId = $gatewayProcess.Id; database = $databaseName; origin = $env:PublicOrigin } | ConvertTo-Json | Set-Content (Join-Path $runDirectory 'processes.json') -Encoding utf8
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        try { $response = Invoke-WebRequest "$env:PublicOrigin/api/health" -SkipCertificateCheck; if ($response.StatusCode -eq 200) { $ready = $true; break } } catch { }
        Start-Sleep -Milliseconds 500
    }
    if (-not $ready) { throw 'The isolated gateway/API did not become ready.' }
    Push-Location (Join-Path $studioRoot 'e2e')
    try { npx playwright test --config fullstack.playwright.config.ts; if ($LASTEXITCODE -ne 0) { throw 'The LocalDB browser workflow failed.' } }
    finally { Pop-Location }
    Write-Output "LocalDB browser workflow passed at $env:PublicOrigin. Database: $databaseName"
} finally {
    Remove-Item Env:Bootstrap__Email, Env:Bootstrap__Password -ErrorAction SilentlyContinue
    if (-not $KeepRunning) {
        if (-not $gatewayProcess.HasExited) { Stop-Process -Id $gatewayProcess.Id }
        if (-not $apiProcess.HasExited) { Stop-Process -Id $apiProcess.Id }
    }
}
