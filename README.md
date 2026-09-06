# Quinntyne Brown Studio

A photography studio platform with public galleries and quotations, studio administration, private client galleries, albums, and print requests. The implementation follows the [requirements](docs/specs/L1.md), [detailed designs](docs/detailed-designs/README.md), and [approved policies](docs/specs/decisions.md).

## Run locally

Prerequisites: Windows, SQL Server Express LocalDB, .NET SDK 10.0.303, Node.js 24.18 or a compatible later 24.x patch, npm, and PowerShell 7. Normal development and production persist records, accounts, and the outbox in LocalDB. API and worker use the same Windows account on one host. The quick start defaults to database `QbsDevelopment` and applies migrations before startup. Restarting preserves data.

Development uses explicit external-service adapters: photos are written under `.artifacts/photos`, routing and AI return illustrative responses, and email is captured locally. These adapters do not change database persistence. Database fakes are supplied only by acceptance tests. Production requires an explicit `ConnectionStrings__Studio`; see the [Windows operating runbook](deploy/README.md).

```powershell
dotnet restore backend/Qbs.slnx --locked-mode
Push-Location frontend
npm ci
npm run build:libs
npm run build:apps
Pop-Location

# First run: supply your own local credentials; do not commit them.
# Later runs may omit both Bootstrap variables and reuse existing accounts.
$env:Bootstrap__Email = Read-Host 'Local administrator email'
$credential = Get-Credential -UserName $env:Bootstrap__Email -Message 'Local administrator password'
$env:Bootstrap__Password = $credential.GetNetworkCredential().Password
./scripts/start-dev.ps1
```

Open `https://localhost:7443`, administration at `/admin/`, and the client application at `/client/`. The design system runs on its own: `npm ci` then `npm start` in [`design-system/`](design-system/README.md) serves the catalog at `http://127.0.0.1:5183` with no studio backend, database, or product cookie. The startup script exports a development certificate; if it is not trusted locally, run `dotnet dev-certs https --trust` as a separate local setup step. Stop the foreground script with Ctrl+C to stop its API process.

Sign in as the local administrator, configure rates and a resolved base studio, then add photographers, working windows, and sessions. Upload a JPEG from a session workspace to exercise actual block transfer, digest validation, preview conversion, and worker processing. RAW conversion additionally requires LibRaw's `dcraw_emu` on PATH, or `Raw__Executable` pointing to it. Camera compatibility remains subject to the fixture gate below.

Invite a client from administration and assign a session. In controlled development, open `/api/admin/development-mail` while signed in as an administrator to retrieve captured invitation links. This endpoint is absent in production. Use a separate browser context for the invited client. Empty production catalogs contain no prototype prices, galleries, or credentials.

## Build and verify

```powershell
dotnet build backend/Qbs.slnx -c Release
dotnet test backend/Qbs.slnx --logger trx --results-directory .artifacts/acceptance
Push-Location frontend
npm run build:libs
npm run build:apps
Pop-Location
Push-Location e2e
npm ci
npx playwright install
npm test
Pop-Location
Push-Location design-system
npm ci
npx playwright install chromium
npm test
npm run build
Pop-Location
python scripts/verify-architecture.py
python docs/detailed-designs/verify.py
dotnet publish backend/src/Qbs.Api -c Release -p:UseAppHost=false -o .artifacts/windows/api
dotnet publish backend/src/Qbs.Worker -c Release -p:UseAppHost=false -o .artifacts/windows/worker
```

The design system validates its manifest, styles, and fixtures, then exercises its catalog in the browser at the three viewport widths with every product API request blocked. Backend acceptance tests explicitly supply a transaction-capable fake and controlled dependencies. Separate persistence tests use disposable LocalDB databases on Windows and verify the normal runtime registrations, restart behavior, migrations, and identity. Cleanup checks the fixture's unique `QbsTest_`, `QbsQuoteTest_`, or `QbsPersistenceTest_` prefix before deleting its database. The older SQL boundary tests retain the optional test-only `QBS_SQL` override; it is not runtime configuration. The acceptance suite in [`e2e/`](e2e/README.md) uses page objects and mocked responses across three browsers and three viewport sizes. A real HTTPS smoke script is also available at [scripts/smoke-local.mjs](scripts/smoke-local.mjs).

## Repository map

| Path | Responsibility |
| --- | --- |
| `backend/src/Qbs.Domain` | Records, value contracts, pricing and time policies |
| `backend/src/Qbs.Application` | Use cases, MediatR handlers, service and persistence ports |
| `backend/src/Qbs.Infrastructure` | EF Core/Identity, SQL transactions and outbox, Azure adapters, image processing |
| `backend/src/Qbs.Api` | Controller endpoints, authorization, antiforgery, provisioning and migrations |
| `backend/src/Qbs.Worker` | Durable preview, AI, email, retention and deletion processing |
| `frontend/projects` | Three Angular applications and four reusable libraries |
| `e2e` | Playwright acceptance suite: page objects and specs for the three applications |
| `design-system` | Standalone design system: tokens, component catalog, screen patterns, static deployment |
| `frontend/component-catalog.json` | Inventory and contracts for every application component |
| `deploy` | Container images, gateway, Azure Bicep and database setup |
| `docs/specs` | L1 and L2 requirements with acceptance criteria, and the decision baseline |
| `docs/detailed-designs` | Feature designs, shared architecture and contracts, rendered diagrams, acceptance register |
| `docs/mocks` | The approved HTML prototype and its browser checks |
| `docs/implementation` | Verification results, acceptance gaps and implementation decisions |
| `scripts` | Architecture and diagram checks, the development gateway, and the packaged smoke run |

Windows deployment instructions and LocalDB configuration are in [deploy/README.md](deploy/README.md). The old Azure SQL and Linux backend deployment is archived under `deploy/legacy/` and is unsupported. No Azure resources are deployed by local verification or CI. Release still requires camera fixtures, measured upload capacity, approved AI evaluation, Windows backup/restore evidence, and external-service qualification recorded in the [evidence register](docs/specs/decisions.md#evidence-register). See [implementation status](docs/implementation/README.md) for the precise limits of current acceptance evidence.
