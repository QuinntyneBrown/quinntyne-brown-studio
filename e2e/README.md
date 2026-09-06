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
