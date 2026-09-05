$ErrorActionPreference = 'Stop'
$studioRoot = Split-Path -Parent $PSScriptRoot
if (-not $env:Bootstrap__Email -or -not $env:Bootstrap__Password) {
    throw 'Set Bootstrap__Email and Bootstrap__Password for your local administrator before starting.'
}
$certificateDirectory = Join-Path $studioRoot '.artifacts/certificates'
New-Item -ItemType Directory -Force $certificateDirectory | Out-Null
$certificate = Join-Path $certificateDirectory 'localhost.pem'
if (-not (Test-Path $certificate)) {
    dotnet dev-certs https --export-path $certificate --format PEM --no-password
    if ($LASTEXITCODE -ne 0) { throw 'Development certificate export failed.' }
}
$env:QBS_TLS_CERT = $certificate
$env:QBS_TLS_KEY = Join-Path $certificateDirectory 'localhost.key'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Development__Controlled = 'true'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:7444'
$env:PublicOrigin = 'https://localhost:7443'
$env:Development__PhotoDirectory = Join-Path $studioRoot '.artifacts/photos'
$apiLog = Join-Path $studioRoot '.artifacts/api.log'
$apiError = Join-Path $studioRoot '.artifacts/api-error.log'
dotnet build (Join-Path $studioRoot 'backend/Qbs.Api')
if ($LASTEXITCODE -ne 0) { throw 'API build failed.' }
$apiDll = Join-Path $studioRoot 'backend/Qbs.Api/bin/Debug/net10.0/Qbs.Api.dll'
$api = Start-Process dotnet -ArgumentList @($apiDll) -WorkingDirectory $studioRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $apiLog -RedirectStandardError $apiError
try { node (Join-Path $PSScriptRoot 'dev-gateway.mjs') }
finally { if (-not $api.HasExited) { Stop-Process -Id $api.Id } }
