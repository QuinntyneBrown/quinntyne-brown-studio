# Studio frontend

Angular 22 workspace for marketing, administration and client access. Shared `domain`, `api`, `components` and `application` libraries keep contracts, transport, presentation and workflows separate. The design system is a separate product at [`design-system/`](../design-system/README.md) with its own package, build, tests and deployment.

Run `npm ci`, `npm run build:libs` and `npm run build:apps` from this directory. `npm run format` formats frontend sources. The acceptance suite is its own project at [`e2e/`](../e2e/README.md); it starts these applications with mocked APIs and checks the browser and viewport matrix.

For real authenticated local operation, use the HTTPS gateway described in the [root README](../README.md). Direct `ng serve` hosts are intended for isolated mocked checks; production secure cookies require the same-origin HTTPS setup.

`component-catalog.json` inventories every application component, its contract, and the [design-system](../design-system/README.md) entry and URL that show it; `python scripts/verify-architecture.py` fails when a component has no catalogued example. Applications reuse the BEM class names owned by the design system and consume services through injection tokens. Controlled and real-API acceptance use separate runner configurations.

The public calculator now consumes the packed `reckoner/angular/quote` presentation.
Supply its API URL and `pk_` key through
`projects/marketing/src/environments/reckoner.ts` before building; empty values
show the unavailable state. Add the marketing origin to that Reckoner customer's
allowlist. The Studio backend separately needs `Reckoner__ApiBaseUrl` and
`Reckoner__SecretKey`; it mints temporary admin tokens only for authenticated
administrators. No secret key belongs in a frontend environment file. The eight
administrative widgets consume `reckoner/admin` from the same installed tarball.

The separate `e2e/reckoner.playwright.config.ts` runs the packed marketing consumer
against the real Reckoner API and an isolated SQL database, with controlled Maps
HTTP. Build the neighboring Reckoner browser host first; set `RECKONER_DOTNET`
to the x64 .NET executable on ARM64 Windows with LocalDB. Then run
`npx playwright test --config reckoner.playwright.config.ts` from `e2e/`.
The normal controlled suite remains available for the other application workflows.
