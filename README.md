# Quinntyne Brown Studio

[![Verify and package](https://github.com/QuinntyneBrown/quinntyne-brown-studio/actions/workflows/verify.yml/badge.svg)](https://github.com/QuinntyneBrown/quinntyne-brown-studio/actions/workflows/verify.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Quinntyne Brown Studio is an open-source photography studio management platform
for weddings, events, headshots, and family portraits. It brings public
galleries and quotations, studio administration, and private client delivery
into one workspace.

The project is being developed with .NET, Angular, and SQL Server. It includes:

- a public site for galleries, availability, quotes, prices, and promotions;
- administration tools for sessions, photos, schedules, rates, vendors, and
  content; and
- client galleries, albums, and print requests.

## Getting started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0), using the
  version specified in [`global.json`](global.json);
- [Node.js](https://nodejs.org/) and npm; and
- SQL Server Express LocalDB for the full local application.

The complete local development workflow, including platform-specific
requirements, is documented in [`CONTRIBUTING.md`](CONTRIBUTING.md).

### Build and run

From the repository root:

```powershell
dotnet restore backend/QuinntyneBrownStudio.slnx --locked-mode

Push-Location frontend
npm ci
npm run build:libs
npm run build:apps
Pop-Location
```

To start the local application, configure a local administrator account and run
the development gateway:

```powershell
$env:Bootstrap__Email = "admin@example.test"
$env:Bootstrap__Password = "use-a-local-password"
./scripts/start-dev.ps1
```

Then open the marketing site at <https://localhost:7443>. Do not commit local
credentials or generated files. See [`CONTRIBUTING.md`](CONTRIBUTING.md) for
validation commands and development guidance.

## Repository structure

| Directory | Purpose |
| --- | --- |
| [`backend/src`](backend/src) | .NET domain, application, infrastructure, API, and worker projects |
| [`backend/tests`](backend/tests) | Backend integration and acceptance tests |
| [`frontend/projects`](frontend/projects) | Angular applications and shared libraries |
| [`e2e`](e2e) | Playwright application acceptance tests |
| [`design-system`](design-system) | Standalone component catalog and visual reference |
| [`docs`](docs) | Requirements, design documentation, and implementation notes |

## Documentation

- [Requirements and product scope](docs/specs/L1.md)
- [Detailed designs and acceptance criteria](docs/detailed-designs/README.md)
- [Frontend development guide](frontend/README.md)
- [Design-system guide](design-system/README.md)
- [Application acceptance tests](e2e/README.md)
- [Support](SUPPORT.md)

## Contributing

Contributions to code, documentation, accessibility, design, and reproducible
bug reports are welcome. Please read
[`CONTRIBUTING.md`](CONTRIBUTING.md), follow the
[Code of Conduct](CODE_OF_CONDUCT.md), and search
[existing issues](https://github.com/QuinntyneBrown/quinntyne-brown-studio/issues)
before opening a new one.

Please report suspected vulnerabilities privately using
[SECURITY.md](SECURITY.md).

## License

Project code and original documentation are available under the
[MIT License](LICENSE). Dependencies and bundled sample photography retain
their own licenses; see
[`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).
