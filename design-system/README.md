# Quinntyne Brown Studio Design System

The standalone, first-class reference for the studio's foundations, 23 components, 23 responsive screen-pattern states, and 4 dialog scenarios. It has no runtime, validation, test, or build dependency on another folder in this repository, and it can be copied, versioned, and deployed on its own.

## Run locally

```sh
npm ci
npm start
```

Open `http://127.0.0.1:5183/`. Component, pattern, and dialog URLs support direct navigation and browser refresh. The ports are 5183 for the catalog and 4181 for the test server, kept away from the common Vite and Angular defaults so a neighbouring project cannot serve this suite by accident.

## Validate and test

```sh
npm run validate
npm test
```

`npm run validate` checks the manifest against the stylesheet and the rendered fixtures, the required package scripts, and the absence of references outside this folder. `npm test` adds the Playwright suite, which exercises the 390, 768, and 1440 CSS-pixel viewports with the studio API blocked. Install the browser once with `npx playwright install chromium` if needed.

## Build and deploy

```sh
npm run build
```

`npm run check:artifact` then serves that artifact under the checked-in navigation configuration and exercises the deep links, the published manifest, the isolated preview, and the missing-asset response, so a routing mistake fails before deployment.

The artifact is `dist/`. Deploy it to Azure Static Web Apps; the build includes `staticwebapp.config.json`, `component-manifest.json`, the isolated preview page, and the navigation fallback that keeps deep links working. [`.github/workflows/deploy-design-system.yml`](../.github/workflows/deploy-design-system.yml) validates, tests, builds, and uploads it. No studio backend, database, or credential is required to browse the result.

## Ownership

- `assets/tokens.css` is authoritative for the studio's colour, type, spacing, and shape tokens.
- `assets/components.css` is authoritative for the component classes and their responsive behaviour.
- `component-manifest.json` is the versioned public inventory: components, the classes and states each owns, and its rendered examples.
- `assets/catalog-content.js` owns the standalone screen-pattern and dialog fixtures. Its content is illustrative; it holds no studio data or credentials.
- The Angular `components` library and the application screens in `frontend/` re-implement these classes for the product. Products may duplicate these values; they must not import this folder at runtime.
- Layout classes that belong to one feature and appear on one screen — the quote summary, the session panels, the settings editors — stay with that feature and are shown through the screen patterns rather than as separate components.

## Adding a component

1. Add the styles to `assets/components.css`.
2. Add the entry to `component-manifest.json` with its classes, states, and at least one example.
3. Run `npm run validate`, then `npm test`.
4. Only then let an application consume it.
