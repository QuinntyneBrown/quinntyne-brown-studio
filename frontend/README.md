# Studio frontend

Angular 22 workspace for marketing, administration and client access. Shared `domain`, `api`, `components` and `application` libraries keep contracts, transport, presentation and workflows separate. The design system is a separate product at [`design-system/`](../design-system/README.md) with its own package, build, tests and deployment.

Run `npm ci`, `npm run build:libs` and `npm run build:apps` from this directory. `npm run format` formats frontend sources. The acceptance suite is its own project at [`e2e/`](../e2e/README.md); it starts these applications with mocked APIs and checks the browser and viewport matrix.

For real authenticated local operation, use the HTTPS gateway described in the [root README](../README.md). Direct `ng serve` hosts are intended for isolated mocked checks; production secure cookies require the same-origin HTTPS setup.

`component-catalog.json` inventories every application component, its contract, and the [design-system](../design-system/README.md) entry and URL that show it; `python scripts/verify-architecture.py` fails when a component has no catalogued example. Applications reuse the BEM class names owned by the design system and consume every service through an injection token, so a Playwright run never reaches a real implementation.
