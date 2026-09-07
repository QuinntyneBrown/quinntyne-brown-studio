# Acceptance suite

Playwright acceptance tests for the marketing, administration, and client applications. The suite is its own project so the tests, their page objects, and their runner stay independent of the Angular workspace they exercise.

```sh
npm ci
npx playwright install
npm test
```

The suite drives the Angular workspace, so it needs the same Node 24 runtime the workspace requires. `npm run test:chromium` narrows a local run to one browser when the other engines are not installed.

The configuration starts `ng serve` for each application from [`../frontend`](../frontend/README.md) on ports 4320, 4321, and 4322, chosen away from the Angular default so a neighbouring project cannot serve this suite by accident, and runs every spec across Chromium, Firefox, and WebKit at 390, 768, and 1440 CSS-pixel widths.

## Layout

- `page-objects/` — one page object per screen. It owns the selectors and the interactions.
- `specs/` — tests state intent and name the acceptance criteria they cover. No selector belongs in a spec.

API responses are mocked inside the page object, so a run never reaches a real service implementation. The design system has its own suite in [`../design-system`](../design-system/README.md); the HTML prototype has its own checks in [`../docs/mocks`](../docs/mocks/README.md).

The marketing server uses Angular's `acceptance` configuration. Its file replacement binds `QUOTE_SERVICE` to `MockQuoteService`; the quote page object supplies typed operations through a browser fixture. Production builds bind `QuoteService` and contain no runtime switch to activate the fixture. Other existing scenarios retain their HTTP mocks.

The live quote scenarios reference the Given–When–Then criteria in [the slice brief](../docs/implementation/live-quote-slice.md). Runner timeouts allow browser startup and teardown on the Windows browser matrix; they are not application latency requirements.


The normal suite starts all products in acceptance configuration on ports 4420–4422. Feature tokens bind to explicit controlled services supplied by page objects; unexpected product API traffic is blocked. Set `QBS_E2E_SITES` only when running a targeted subset (for example `admin,client`).

`demo/demo.spec.ts` is a demonstration, not a test. With `demo.playwright.config.ts` it records the three narrated walkthroughs in [`../docs/demo`](../docs/demo/README.md) against the packaged applications, a published API and a disposable LocalDB database, using the same page objects as the suites. From the repository root, `scripts/record-demo.ps1` prepares that environment and runs it. It lives outside `specs/` and gates nothing.

`integration/localdb-platform.spec.ts` uses its separate `fullstack.playwright.config.ts` and deliberately exercises production HTTP adapters through packaged applications. From the repository root, `scripts/smoke-platform.ps1` creates the isolated LocalDB database, provisions generated credentials, starts the HTTPS gateway and runs this workflow. It does not change the normal development database or deploy cloud resources.
