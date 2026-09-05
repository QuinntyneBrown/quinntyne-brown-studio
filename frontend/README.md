# Studio frontend

Angular 22 workspace for marketing, administration, client access and the independent design system. Shared `domain`, `api`, `components` and `application` libraries keep contracts, transport, presentation and workflows separate.

Run `npm ci`, `npm run build:libs` and `npm run build:apps` from this directory. `npx playwright test` starts all four development hosts with mocked APIs and checks the browser/viewport matrix. `npm run format` formats frontend sources.

For real authenticated local operation, use the HTTPS gateway described in the [root README](../README.md). Direct `ng serve` hosts are intended for isolated mocked checks; production secure cookies require the same-origin HTTPS setup.

`component-catalog.json` inventories every component. Build scripts run `scripts/sync-catalog.mjs` to generate the contract data displayed by the independent catalog. Production apps use injected HTTP providers; the catalog uses controlled providers and can operate with every product API request blocked.
