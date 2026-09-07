# Quinntyne Brown Studio

[![Verify and package](https://github.com/QuinntyneBrown/quinntyne-brown-studio/actions/workflows/verify.yml/badge.svg)](https://github.com/QuinntyneBrown/quinntyne-brown-studio/actions/workflows/verify.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A photography studio management platform for weddings, events, headshots, and family portraits. Built with .NET, Angular, and SQL Server Express LocalDB, it brings public galleries and quotations, studio administration, and private client delivery into one workspace.

[Get started](#run-locally) · [Documentation](#documentation) · [Contribute](CONTRIBUTING.md) · [Get help](SUPPORT.md) · [Report a vulnerability](SECURITY.md)

## Features

| Area                  | Capabilities                                                                                                                  |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| Marketing             | Published galleries, live itemized quotes, availability, print prices, packages, and promotions                               |
| Studio administration | Sessions, resumable photo uploads, preview and AI-assisted review, schedules, equipment, studios, rates, vendors, and content |
| Client delivery       | Invitation-based access to assigned galleries, ordered albums, and print requests with saved price snapshots                  |
| Background processing | Photo previews, advisory AI analysis, email, retention notices, and confirmed deletion through a durable outbox               |
| Design system         | Independent component catalog, shared design tokens, responsive screen patterns, and browser acceptance tests                 |

The [implementation report](docs/implementation/platform-completion.md) records local feature completion and acceptance evidence. Production qualification remains open for camera RAW fixtures, upload capacity, AI usefulness, and Windows backup/restore and external services; see the [evidence register](docs/specs/decisions.md#evidence-register). Payments, shipping, automated print fulfillment, and booking confirmation are outside the current baseline.

## Explore the interface

Three narrated recordings in [docs/demo](docs/demo/README.md) show the packaged marketing, administration, and client applications driving themselves against the real API and LocalDB. After cloning the repository, open `docs/mocks/index.html` from your local filesystem in a browser to explore the marketing, admin, and client prototypes without installing dependencies. These interactive HTML mocks use fictional data and simulated services. The [prototype guide](docs/mocks/README.md) explains the workflows and their differences from the application.

For the standalone component catalog, run the following from the repository root:

```powershell
Push-Location design-system
npm ci
npm start
```

Open [the local design system](http://127.0.0.1:5183). Stop it with Ctrl+C, then run `Pop-Location` to return to the repository root. It requires Node.js and npm and runs independently of the studio API and database. See the [design-system guide](design-system/README.md) for its build and tests.

## Run locally

### Prerequisites

| Tool                     | Requirement                                                                                                               |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------- |
| Operating system         | Windows for the API, worker, and LocalDB persistence tests                                                                |
| Database                 | SQL Server Express LocalDB with the `MSSQLLocalDB` instance available to your Windows account                             |
| .NET SDK                 | 10.0.303, with patch roll-forward as defined in [global.json](global.json)                                                |
| Node.js and npm          | Node.js 24.18 or a compatible later 24.x patch; npm 11.16.0 is declared in [frontend/package.json](frontend/package.json) |
| Shell and source control | PowerShell 7 and Git                                                                                                      |
| Documentation checks     | Python 3.11 or later; CI uses 3.11                                                                                        |
| RAW photo processing     | LibRaw's `dcraw_emu` on PATH, or the `Raw__Executable` setting; unnecessary for the JPEG walkthrough                      |

Normal development and production persist records, accounts, and the outbox in LocalDB. API and worker use the same Windows account on one host. The quick start defaults to database `QbsDevelopment` and applies migrations before startup. Restarting preserves data. SQL Server service instances, Azure SQL, and Linux backend containers are unsupported; [OD-10](docs/specs/decisions.md#od-10--localdb-persistence-and-windows-hosting) defines the supported runtime.

Development uses explicit external-service adapters: photos are written under `.artifacts/photos`, routing and AI return illustrative responses, and email is captured locally. These adapters do not change database persistence. Database fakes are supplied only by acceptance tests. Production requires an explicit `ConnectionStrings__Studio`; see the [Windows operating runbook](deploy/README.md).

### Build and start

Clone the repository, or start in your existing checkout:

```powershell
git clone https://github.com/QuinntyneBrown/quinntyne-brown-studio.git
Set-Location quinntyne-brown-studio
```

Run these commands from the repository root:

```powershell
dotnet restore backend/QuinntyneBrownStudio.slnx --locked-mode
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

Open [marketing](https://localhost:7443), [administration](https://localhost:7443/admin/), or the [client application](https://localhost:7443/client/). The startup script exports a development certificate; if it is not trusted locally, run `dotnet dev-certs https --trust` as a separate local setup step. Stop the foreground script with Ctrl+C to stop its API process. After stopping, remove the bootstrap values from the shell with `Remove-Item Env:Bootstrap__Email, Env:Bootstrap__Password -ErrorAction SilentlyContinue`; subsequent starts can reuse the provisioned account.

### Try a studio workflow

Sign in as the local administrator, configure rates and a resolved base studio, then add photographers, working windows, and sessions. Upload a JPEG from a session workspace to exercise actual block transfer, digest validation, preview conversion, and worker processing. RAW conversion additionally requires LibRaw's `dcraw_emu` on PATH, or `Raw__Executable` pointing to it. Camera compatibility remains subject to the fixture gate below.

Invite a client from administration and assign a session. In controlled development, open `/api/admin/development-mail` while signed in as an administrator to retrieve captured invitation links. This endpoint is absent in production. Use a separate browser context for the invited client. Empty production catalogs contain no prototype prices, galleries, or credentials.

## Build and verify

From the repository root, after completing the dependency installation above:

```powershell
dotnet build backend/QuinntyneBrownStudio.slnx -c Release --no-restore
dotnet test backend/QuinntyneBrownStudio.slnx -c Release --no-build --logger trx --results-directory .artifacts/acceptance
Push-Location frontend
npm run build:libs
npm run build:apps
Pop-Location
Push-Location e2e
npm ci
npx playwright install
npm run typecheck
npm test
Pop-Location
Push-Location design-system
npm ci
npx playwright install
npm test
npm run build
npm run check:artifact
Pop-Location
python scripts/verify-architecture.py
python docs/detailed-designs/verify.py
dotnet publish backend/src/QuinntyneBrownStudio.Api -c Release -p:UseAppHost=false -o .artifacts/windows/api
dotnet publish backend/src/QuinntyneBrownStudio.Worker -c Release -p:UseAppHost=false -o .artifacts/windows/worker
dotnet publish backend/src/QuinntyneBrownStudio.Qualification -c Release -p:UseAppHost=false -o .artifacts/windows/qualification
```

Backend acceptance tests explicitly supply a transaction-capable fake and controlled dependencies. Separate persistence tests use disposable LocalDB databases on Windows to verify runtime registrations, restart behavior, migrations, and identity. The older SQL boundary tests retain the optional test-only `QBS_SQL` override; it is not runtime configuration.

The [application acceptance suite](e2e/README.md) uses page objects and controlled service bindings across Chromium, Firefox, and WebKit at three viewport sizes. The design system validates its manifest and fixtures, exercises the same browser and viewport matrix with product API requests blocked, and checks the built static artifact.

Run `./scripts/smoke-platform.ps1` for the packaged HTTPS/LocalDB workflow with a uniquely named database and captured development email. `-KeepRunning` leaves that isolated preview available; generated credentials are stored with Windows user encryption under ignored `.artifacts/platform/live`. The [completion report](docs/implementation/platform-completion.md) records acceptance evidence, and [qualification commands](docs/implementation/qualification.md) collect the remaining external evidence. See [CONTRIBUTING.md](CONTRIBUTING.md) for change-specific validation and the pull request workflow.

## Repository map

| Path                              | Responsibility                                                                                                                                 |
| --------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| `backend/src`                     | Domain policies; Application use cases and MediatR handlers; Infrastructure persistence and adapters; API, worker, and qualification CLI hosts |
| `backend/tests`                   | API integration and disposable LocalDB acceptance tests                                                                                        |
| `frontend/projects`               | Three Angular applications and four reusable libraries                                                                                         |
| `e2e`                             | Playwright acceptance suite: page objects and specs for the three applications                                                                 |
| `design-system`                   | Standalone design system: tokens, component catalog, screen patterns, static deployment                                                        |
| `frontend/component-catalog.json` | Inventory and contracts for every application component                                                                                        |
| `deploy`                          | Supported Windows/LocalDB runbook and superseded deployment archive                                                                            |
| `docs/specs`                      | L1 and L2 requirements with acceptance criteria, and the decision baseline                                                                     |
| `docs/detailed-designs`           | Feature designs, shared architecture and contracts, rendered diagrams, acceptance register                                                     |
| `docs/mocks`                      | The approved HTML prototype and its browser checks                                                                                             |
| `docs/implementation`             | Verification results, acceptance gaps and implementation decisions                                                                             |
| `scripts`                         | Architecture and diagram checks, the development gateway, the packaged smoke run, and the demonstration recording                              |
| `.github`                         | Verification and deployment workflows, issue templates, and pull request guidance                                                              |

The backend follows Clean Architecture with vertical feature slices and inward dependencies. MediatR is pinned to **12.5.0**. Angular separates the `components`, `api`, `domain`, and `application` libraries; the three product applications own their bootstraps. Service consumers use interfaces and injection tokens. [AGENTS.md](AGENTS.md) defines the repository conventions for all contributions.

## Documentation

| Start here                                                                                                       | Purpose                                                                     |
| ---------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| [Requirements](docs/specs/L1.md) and [decisions](docs/specs/decisions.md)                                        | Product scope, acceptance criteria, and approved policies                   |
| [Detailed designs](docs/detailed-designs/README.md)                                                              | Architecture, feature designs, contracts, diagrams, and acceptance register |
| [Frontend](frontend/README.md) and [components](docs/components.md)                                              | Angular workspace and presentation contracts                                |
| [Design system](design-system/README.md) and [HTML mocks](docs/mocks/README.md)                                  | Visual reference, interactive prototypes, and independent catalog           |
| [Acceptance tests](e2e/README.md)                                                                                | Browser setup, page objects, and packaged integration                       |
| [Demonstration](docs/demo/README.md)                                                                             | Three narrated recordings of the packaged applications driving themselves   |
| [Implementation status](docs/implementation/README.md) and [qualification](docs/implementation/qualification.md) | Recorded verification, remaining gaps, and external evidence commands       |
| [Windows deployment](deploy/README.md) and [domain strategy](deploy/domain-strategy.md)                          | LocalDB configuration, migrations, provisioning, backups, operations, and the `quinntynebrown.studio` layout |

## Deployment and release readiness

Windows deployment instructions and LocalDB configuration are in [deploy/README.md](deploy/README.md). The old Azure SQL and Linux backend deployment is archived under `deploy/legacy/` and is unsupported. Local verification and packaging do not provision Azure resources; the design system has a [separate static-site deployment workflow](.github/workflows/deploy-design-system.yml). Release still requires camera fixtures, measured upload capacity, approved AI evaluation, Windows backup/restore evidence, and external-service qualification recorded in the [evidence register](docs/specs/decisions.md#evidence-register). See [implementation status](docs/implementation/README.md) for the precise limits of current acceptance evidence.

## Contributing

Contributions to code, documentation, accessibility, design, and reproducible bug reports are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) for setup, acceptance-driven development, coding conventions, and review expectations. Start with an [existing issue](https://github.com/QuinntyneBrown/quinntyne-brown-studio/issues) or describe the problem and proposed acceptance criteria in a new one.

Participants follow the [Code of Conduct](CODE_OF_CONDUCT.md). [CONTRIBUTORS.md](CONTRIBUTORS.md) recognizes the people behind the project and explains how contributions are credited.

## Support and security

Use [SUPPORT.md](SUPPORT.md) to find troubleshooting guidance and ask a question. Report suspected vulnerabilities privately using [SECURITY.md](SECURITY.md), which describes the reporting channel, useful evidence, and supported scope.

## License

Copyright (c) 2026 Quinntyne Brown and contributors.

Project code and original documentation are available under the [MIT License](LICENSE). Dependencies and bundled sample photography retain their own licenses; see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) and the [photo attribution](docs/mocks/assets/photos/ATTRIBUTION.md).
