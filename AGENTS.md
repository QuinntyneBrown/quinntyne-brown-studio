# Quinntyne Brown Studio

## Project

Quinntyne Brown Studio is a photography studio management platform for weddings, events, headshots, and family portraits: a public marketing site with a live quote calculator, print prices, and promotions; an admin app for session photo upload and review, schedules, equipment, rates, vendors, and content; and a client site for galleries, albums, and print requests.

## Speed Is Not the Goal

Quality and completeness beat finishing fast. Finish the whole task - edge cases,
error paths, no stubs or `TODO`s. If it is bigger than it looked, complete it and
say what it cost rather than quietly narrowing scope.

## Technology

- Use .NET for the API.
- Use MediatR, pinned to **12.5.0**. Do not upgrade. 12.5.0 is the last release
  under plain Apache-2.0; from 13.0.0 MediatR is commercially licensed, free only
  under a registered Community tier that lapses above $5M USD annual revenue.
- Use Microsoft.Extensions libraries and patterns: dependency injection, Options,
  and Configuration.
- Use Angular for the web client.

## Architecture and Design

- Implement requirements radically simply: the least code that satisfies the
  acceptance criteria, and nothing more. Simple in design, never reduced in scope.
- Apply SOLID principles throughout the codebase.
- Organize features and behaviors into vertical slices.
- Keep back-end code in `backend/` and front-end code in `frontend/`. Within each,
  keep source in `src` and tests in `tests`.
- A command-line tool is another project under `backend/src`, not a second root.

## Backend

- Use Clean Architecture. Dependencies point inward. `Domain` references nothing.
- Keep controllers thin: bind, dispatch through MediatR, return. No logic in a
  controller.
- Commands, queries, handlers, and validators live in `Application`.
- One file per type. Every class, interface, record, and enum gets its own file,
  named for the type it holds.
- Folders and namespaces agree. Controllers live in a `Controllers` folder and are
  namespaced `QuinntyneBrownStudio.Api.Controllers`.

## Frontend

- Organize the workspace into `api`, `components`, and `domain` library projects,
  plus one application project that consumes them and launches the app.
- Prefer signals over RxJS. Reach for RxJS only for genuine streams and events.
- No single-file components. Template, styles, and class each live in their own file.
- Consume services through an interface, never a concrete class.
- Keep components presentational. Behavior belongs in services, state in signals.

### Interface-driven service consumption — mandatory on the frontend

Every service an application consumes is reached through an interface and an `InjectionToken`. No component, store, or feature imports a concrete implementation.

- `IQuoteService` declares the behavioral contract and `QUOTE_SERVICE` is its `InjectionToken`. The interface, the token, and each implementation live in separate files.
- Contracts are named `I<Entity>Service` in the singular, with no `Api` suffix. The `I` prefix marks a swappable contract; data shapes (`QuoteResult`) take no prefix, and the production implementation takes the unprefixed name (`QuoteService`), never an `Impl` suffix.
- Consumers call `inject(QUOTE_SERVICE)` only. Application composition binds the token to the HTTP adapter in production and to a controlled mock under Playwright, so a test never reaches the real implementation.
- HTTP calls and observable-to-signal conversion stay inside the `api` implementations; `domain` types carry no HTTP dependency.

## Design System

The design system is a deliverable in its own right, not a folder inside the front
end. It sits at `design-system/`, beside `backend/` and `frontend/`, with its own
`package.json`, its own tests, and its own build, and it deploys as its own static
site. Build and review a component there before the application consumes it.

## Testing Approach

Use acceptance test-driven development (ATDD):

- Begin with a failing acceptance test.
- Link each test to explicit acceptance criteria written using the Given-When-Then
  format.
- Implement the behavior required to make the test pass.
- Keep acceptance criteria, tests, and implementation aligned.

Back end: integration tests against the API.

Front end: Playwright, using the Page Object Model.

- One page object per screen. It owns the selectors and the interactions.
- Tests state intent; page objects know the DOM. Never put a selector in a test.

### Never write architecture tests

Never add a test that asserts the shape of the codebase rather than its behavior:
no structure, layout, or naming tests; no banned-API scans; no traceability tests
that parse the specifications. Those constraints belong to the compiler, the
formatter, and review. A test suite exists to prove behavior.

## Folder Structure

```text
quinntyne-brown-studio/
|-- backend/
|   |-- src/
|   `-- tests/
|-- frontend/
|   |-- projects/
|       |-- api/
|       |-- components/
|       |-- domain/
        ├── marketing/            # Public site: galleries, quote calculator, prices
        ├── admin/                # Sessions, photos, schedules, rates, content
        └── client/               # Galleries, albums, print requests
|-- design-system/
|-- e2e/
|   |-- page-objects/
|   `-- specs/
`-- docs/
    `-- specs/
```
<!-- primer:end -->
