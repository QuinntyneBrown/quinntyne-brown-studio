# Windows and LocalDB operation

The supported backend target is one Windows host. The API, worker, migration command, and provisioning command run under the **same Windows account**, which owns the LocalDB instance. This implements [OD-10](../docs/specs/decisions.md#od-10--localdb-persistence-and-windows-hosting). No Windows services or cloud resources are installed by this runbook. The [legacy archive](legacy/README.md) is superseded.

## Prerequisites and packaging

Install .NET 10 (SDK 10.0.303 to build), SQL Server Express LocalDB, PowerShell 7, and Node.js 24.18 or a compatible later 24.x patch. Install the matching Windows LibRaw `dcraw_emu` executable for RAW processing and set `Raw__Executable` if it is not on PATH. Camera qualification remains subject to its existing evidence gate.

Run `SqlLocalDB info` and `SqlLocalDB start MSSQLLocalDB` under the account that will run the application. If needed, locate the utility under `C:\Program Files\Microsoft SQL Server\<version>\Tools\Binn\SqlLocalDB.exe`. Do not substitute another Windows account, LocalSystem, a SQL Server service instance, or a remote server. [Microsoft's LocalDB documentation](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) describes instance ownership and file access.

From the repository root:

```powershell
dotnet restore backend/QuinntyneBrownStudio.slnx --locked-mode
dotnet publish backend/src/QuinntyneBrownStudio.Api -c Release --no-restore -p:UseAppHost=false -o .artifacts/windows/api
dotnet publish backend/src/QuinntyneBrownStudio.Worker -c Release --no-restore -p:UseAppHost=false -o .artifacts/windows/worker
dotnet publish backend/src/QuinntyneBrownStudio.Qualification -c Release --no-restore -p:UseAppHost=false -o .artifacts/windows/qualification
npm ci --prefix frontend
npm run build:libs --prefix frontend
npm run build:apps --prefix frontend
```

The published DLLs use the installed .NET runtime, which must support the installed LocalDB driver. CI publishes these backend directories on Windows. Angular and independent design-system artifacts remain separate.

## Configure, migrate, and provision

Set these variables in each process's environment. Deployment settings belong outside source control:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:DOTNET_ENVIRONMENT = 'Production'
$env:ConnectionStrings__Studio = 'Server=(localdb)\MSSQLLocalDB;Database=QbsProduction;Integrated Security=true;Encrypt=true;TrustServerCertificate=true'
$env:PublicOrigin = 'https://studio.example.com'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:7444'
dotnet .artifacts/windows/api/QuinntyneBrownStudio.Api.dll --migrate
if ($LASTEXITCODE -ne 0) { throw 'Migration failed; do not start the application.' }
```

Production requires an explicit connection. Its database differs from development's `QbsDevelopment`; staging uses its own target. Integrated authentication needs no SQL password. SQL credentials, attached filenames, system databases, and non-LocalDB connections are rejected. `TrustServerCertificate` applies to this local instance example only.

`--migrate` applies the existing EF migrations and is repeatable. Back up existing data before upgrading. Normal startup never uses `EnsureCreated`, creates a replacement database, or applies migrations. A database created outside migrations requires operator reconciliation; do not delete it to resolve a schema error. EF tooling uses the same environment variables and validation; set the environment explicitly before `dotnet ef` commands. The former design-time `QBS_SQL` override is replaced by `ConnectionStrings__Studio`.

Provision the first administrator with credentials supplied only to that process:

```powershell
$env:Bootstrap__Email = Read-Host 'Administrator email'
$studioCredential = Get-Credential -UserName $env:Bootstrap__Email -Message 'Initial administrator credentials'
$env:Bootstrap__Password = $studioCredential.GetNetworkCredential().Password
try {
    dotnet .artifacts/windows/api/QuinntyneBrownStudio.Api.dll --provision-admin
    if ($LASTEXITCODE -ne 0) { throw 'Administrator provisioning failed.' }
} finally {
    Remove-Item Env:Bootstrap__Email, Env:Bootstrap__Password -ErrorAction SilentlyContinue
}
```

Provisioning is repeatable for an existing administrator and does not reset its password.

## Start and supervise

Run `dotnet .artifacts/windows/api/QuinntyneBrownStudio.Api.dll` and `dotnet .artifacts/windows/worker/QuinntyneBrownStudio.Worker.dll` in separate terminals under the owning account, with the same database and data-protection settings. Supervise processes through the host's operating procedures; service installation and automatic restart configuration are outside this change. Stop with Ctrl+C.

Both processes verify database access and migrations before serving or processing work. `/api/health` remains process liveness; use an authenticated read to verify continued database access. Capture process output and worker failures in the host's logging system.

Serve the built marketing, admin, and client files through the host's HTTPS reverse proxy at `/`, `/admin/`, and `/client/`, forwarding `/api/` to the loopback API. Preserve the original HTTPS scheme and host-only cookies, apply each app's SPA fallback, and keep the API bound to loopback. Supply the host's approved TLS certificate and DNS settings. The design system remains a separate static site; its Azure Static Web Apps deployment is unchanged.

For repository development, `scripts/start-dev.ps1` migrates, optionally provisions credentials supplied in the environment, and starts the existing local HTTPS gateway. Rates, studios, accounts, and outbox records persist after restart. Controlled Maps/AI/email/storage/queue adapters remain development boundaries. The integrated development processor handles the persistent outbox, so that quick start does not need a separate worker. Captured email and queue buffers remain ephemeral.

## External services

Production retains its Azure adapters. Configure credentials for the Windows processes through the Azure SDK's supported credential chain. Managed identity is available only on an appropriately configured Azure host; do not assume the archived Container Apps identities exist. Required resource roles and release qualification remain unchanged.

| Setting | Use |
| --- | --- |
| `Azure__BlobEndpoint` | Private originals and derivatives |
| `Azure__QueueEndpoint` | Processing queue URI |
| `Azure__MapsClientId` | Maps account client identifier |
| `Azure__EmailEndpoint`, `Azure__EmailSender` | Invitation and recovery delivery |
| `Azure__AiEndpoint`, `Azure__AiDeployment`, `Azure__AiModelVersion` | Qualified vision deployment |
| `Retention__AdministratorEmail` | Retention notices |
| `DataProtection__Directory` | Stable shared Windows key directory, protected by Windows DPAPI |
| `DataProtection__BlobUri`, `DataProtection__KeyUri` | Optional existing external key storage/wrapping |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Optional API telemetry |
| `Raw__Executable` | Windows LibRaw converter path |

Keep data-protection keys outside temporary build output. Credential, TLS, camera, AI, capacity, and external-resource qualification remain release evidence; this change does not deploy or certify them.

## Backup, restore, and troubleshooting

Take a SQL full backup using `BACKUP DATABASE [QbsProduction] TO DISK = N'C:\StudioBackups\QbsProduction.bak' WITH COPY_ONLY, CHECKSUM;` through SSMS or `sqlcmd` connected to `(localdb)\MSSQLLocalDB` with Windows authentication. Use a unique filename for each backup and a directory writable by the owning account. Retain backups separately from database files and preserve data-protection keys needed for encrypted job payloads. Copying live MDF/LDF files is not a backup.

Verify with `RESTORE VERIFYONLY ... WITH CHECKSUM`, then exercise restoration to a **new database name** and separate files using `RESTORE FILELISTONLY` and `RESTORE DATABASE ... WITH MOVE`. Verify schema, account login, saved studio configuration, and outbox records. For actual recovery, stop API and worker, restore to a new target, and change both connection strings together. Never overwrite the original database by default. Retain the previous publish directory for rollback compatible with the restored schema. Photo deletion has no application rollback.

For an inaccessible database, check installation, instance state, Windows identity, file access, and the database name. Run migration only when intentionally creating or upgrading that target. For pending migrations, back up and run `--migrate` before restarting both processes. For rejected configuration, select a named LocalDB instance with integrated authentication and a non-system database. Failures never select memory storage.

The active Compose file contains only storage emulation. Removing its SQL service does not delete any existing Docker volume. [Acceptance evidence](../docs/implementation/localdb-persistence.md) records runtime verification; production restoration and environment qualification remain explicit operational work.
