# Quinntyne Brown Studio

A photography studio platform with public galleries and quotations, studio administration, private client galleries, albums, and print requests. The implementation follows the [requirements](docs/specs/L1.md), [detailed designs](docs/detailed-designs/README.md), and [approved policies](docs/specs/decisions.md).

## Run locally

Prerequisites: .NET SDK 10.0.303, Node.js 24.18 or a compatible later 24.x patch, npm, and PowerShell 7. The quick start uses explicit development adapters: records and accounts are held in memory, photos are written under `.artifacts/photos`, routing and AI return illustrative responses, and email is captured locally. Restarting clears records and accounts. Production does not fall back to these adapters.

```powershell
dotnet restore backend/Qbs.slnx --locked-mode
Push-Location frontend
npm ci
npm run build:apps
Pop-Location

# Supply your own local credentials; do not commit them.
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
az bicep build --file deploy/main.bicep --outfile .artifacts/azure-template.json
```

The design system validates its manifest, styles, and fixtures, then exercises its catalog in the browser at the three viewport widths with every product API request blocked. Backend acceptance tests use a transaction-capable fake and controlled dependencies. Separate tests use a real disposable SQL LocalDB database on Windows; elsewhere set `QBS_SQL` to a SQL Server connection with permission to create test databases. Only databases named `QbsTest_<guid>` are deleted by the SQL fixture. The acceptance suite in [`e2e/`](e2e/README.md) uses page objects and mocked HTTP responses across three browsers and three viewport sizes. A real HTTPS smoke script is also available at [scripts/smoke-local.mjs](scripts/smoke-local.mjs).

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

Deployment instructions and environment configuration are in [deploy/README.md](deploy/README.md). No Azure resources are deployed by local verification or CI. Release requires the camera fixtures, measured upload-capacity run, approved AI evaluation, and configured Azure environment recorded in the [evidence register](docs/specs/decisions.md#evidence-register). See [implementation status](docs/implementation/README.md) for the precise limits of current acceptance evidence.
