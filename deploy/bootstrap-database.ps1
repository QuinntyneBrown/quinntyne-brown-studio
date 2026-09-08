param([Parameter(Mandatory)][string]$Outputs, [Parameter(Mandatory)][string]$ResourceGroup)
$ErrorActionPreference = 'Stop'
$qbsHost = (Get-Content -LiteralPath $Outputs -Raw | ConvertFrom-Json).host.value
if ($qbsHost.sqlServer -notmatch '^qbs-[a-z0-9]+$') { throw 'Invalid SQL server name.' }
# SQL contained service-principal SIDs use the application/client ID, not the RBAC object ID.
$qbsPrincipal = [Guid]::Parse($qbsHost.clientId)
$qbsSid = '0x' + ([BitConverter]::ToString($qbsPrincipal.ToByteArray())).Replace('-', '')
$qbsAddress = (Invoke-RestMethod 'https://api.ipify.org').Trim()
if (-not [System.Net.IPAddress]::TryParse($qbsAddress, [ref]([System.Net.IPAddress]$null))) { throw 'Cannot resolve runner IP.' }
$qbsRule = 'bootstrap-' + [Guid]::NewGuid().ToString('N')
try {
    az sql server firewall-rule create -g $ResourceGroup -s $qbsHost.sqlServer -n $qbsRule --start-ip-address $qbsAddress --end-ip-address $qbsAddress --only-show-errors -o none
    if ($LASTEXITCODE) { throw 'Cannot allow bootstrap runner.' }
    if (-not (Get-Module -ListAvailable SqlServer)) { Install-Module SqlServer -Scope CurrentUser -Force -AllowClobber }
    Import-Module SqlServer
    $qbsToken = az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv
    if ($LASTEXITCODE) { throw 'Cannot obtain SQL administrator token.' }
    $qbsQuery = @"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'id-qbs-prod')
    CREATE USER [id-qbs-prod] WITH SID = $qbsSid, TYPE = E;
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'id-qbs-prod' AND sid <> $qbsSid)
    THROW 51000, 'Existing runtime principal has a different identity; reconcile before deploying.', 1;
IF IS_ROLEMEMBER('db_ddladmin', 'id-qbs-prod') <> 1 ALTER ROLE db_ddladmin ADD MEMBER [id-qbs-prod];
IF IS_ROLEMEMBER('db_datareader', 'id-qbs-prod') <> 1 ALTER ROLE db_datareader ADD MEMBER [id-qbs-prod];
IF IS_ROLEMEMBER('db_datawriter', 'id-qbs-prod') <> 1 ALTER ROLE db_datawriter ADD MEMBER [id-qbs-prod];
"@
    for ($qbsAttempt = 0; $qbsAttempt -lt 12; $qbsAttempt++) {
        try {
            Invoke-Sqlcmd -ServerInstance "$($qbsHost.sqlServer).database.windows.net" -Database studio -AccessToken $qbsToken -Query $qbsQuery -ConnectionTimeout 30 -QueryTimeout 120 -ErrorAction Stop
            break
        } catch {
            if ($qbsAttempt -eq 11) { throw }
            Start-Sleep -Seconds 10
        }
    }
} finally {
    $qbsToken = $null
    az sql server firewall-rule delete -g $ResourceGroup -s $qbsHost.sqlServer -n $qbsRule --only-show-errors -o none
    if ($LASTEXITCODE) { throw "Remove temporary SQL firewall rule $qbsRule before continuing." }
}
