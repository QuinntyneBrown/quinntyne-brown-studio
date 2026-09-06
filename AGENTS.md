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
- Keep back-end source in `backend/src/` and integration tests in `backend/tests/`.
  Angular source lives in `frontend/projects/<project>/src/`; application
  acceptance tests live in the standalone `e2e/` project.
- A command-line tool is another project under `backend/src`, not a second root.

## Backend

- Use Clean Architecture. Dependencies point inward. `Domain` references nothing.
- Keep controllers thin: bind, dispatch through MediatR, return. No logic in a
  controller.
- Commands, queries, handlers, and validators live in `Application`.
- One file per type. Every class, interface, record, and enum gets its own file,
  named for the type it holds.
- Folders and namespaces agree under the existing `Qbs.*` project roots. Controllers
  live in `Controllers` and use `Qbs.Api.Controllers`; request models live in `Models`
  and use `Qbs.Api.Models`.
- Group Domain types into `Entities`, `Enums`, `ValueObjects`, `Models`, `Policies`,
  and `Exceptions`. Keep Application commands and handlers together by feature;
  catalog operations live in `Catalog/<EntityOrConfiguration>/`.

## Frontend

- The Angular workspace contains `api`, `application`, `components`, and `domain`
  libraries, consumed by the `marketing`, `admin`, and `client` applications.
- Prefer signals over RxJS. Reach for RxJS only for genuine streams and events.
- No single-file components. Template, styles, and class each live in their own file.
- Consume services through an interface, never a concrete class.
- State lives in signals, behavior lives in services. A component class wires the
  two to a template and does nothing else.

### Component placement — settle this before creating the folder

Three libraries hold components. Which one is not a matter of taste. Ask these
questions in order and stop at the first `yes`.

1. **Is it reached by a route, or does it compose a whole screen?** → `application`
2. **Does it inject a token from `@qbs/api`, or name a type from `@qbs/domain`?** → `domain`
3. **Neither.** → `components`

Dependencies run one way and never back:

```text
components  ->  (nothing)
domain      ->  components, api
application ->  domain, components, api
marketing / admin / client  ->  application
```

An import pointing the other way is a defect, not a shortcut.

#### `components` — dumb, studio-agnostic, publishable

Buttons, cards, pills, badges, dialogs, notices, empty states, spinners, form
fields. The plain vocabulary of a user interface.

- It imports Angular and nothing else of ours. No `@qbs/api`, no `@qbs/domain`,
  no router, no `HttpClient`, no studio concepts.
- Treat it as a package that ships to npm and drops into an unrelated product.
  That constraint is the whole point of the library; honor it even while the
  package stays private.
- Data in through `input()`, events out through `output()`. It never fetches,
  never persists, never navigates, and injects no service of ours.
- A component belongs here only if the design system can render it from a literal
  object. If it needs the studio to make sense, it is in the wrong library.

#### `domain` — studio-aware components and the types they speak

The shared types (`Money`, `QuoteResult`, `PhotoView`) and the components that
understand them. This is the studio's vocabulary and it is not publishable.

- A component here may `inject()` an `@qbs/api` token, hold signal state, and map
  results into what a template needs.
- It composes `components` for presentation and passes plain values down. It does
  not restyle its children.
- It is a self-contained region of a screen — a quote summary, a photo picker —
  never a screen. No routing, no page chrome.
- Types stay flat in `domain/src/lib/`; each component gets its own folder beside
  them, class and template and styles in separate files as everywhere else.

#### `application` — pages, shell, routing, composition

Page components, the shell, routes, providers, and the state a route owns.

- A page arranges `domain` regions and `components` primitives. Finding a widget
  built inline in a page means it was placed wrong.
- Binding every service token to its implementation happens here, and only here.

#### `marketing`, `admin`, `client` — bootstrap only

`index.html`, `main.ts`, `styles.css`, and the provider set for that product.
Components do not live in an application project.

### Design tokens — one source, referenced never repeated

- `design-system/assets/tokens.css` is authoritative. Nothing under `frontend/`
  imports it at runtime; `frontend/styles.css` mirrors the values and each app
  pulls in that one file.
- Component stylesheets reference `var(--token)`. A raw hex, a named color, a
  font stack, or an invented pixel gap in a component stylesheet is a defect —
  it is drift that no compiler will catch.
- Color: `--ink`, `--muted`, `--line`, `--soft`, `--paper`, `--accent`, `--danger`.
- Type: `--serif` for headings, `--sans` for everything else.
- Space: `--space-1` through `--space-6` for padding, margin, and grid gaps.
- Shape: `--radius`, and `--tap-target` as the minimum interactive height.
- To add a token, put it in `design-system/assets/tokens.css`, mirror it into
  `frontend/styles.css`, then use it. Never introduce a loose value in a component
  and promote it to a token later.
- Vary a component through an input, or by overriding a custom property on its
  host. Never reach into a child with `::ng-deep`.

### Interface-driven service consumption — mandatory on the frontend

Every service an application consumes is reached through an interface and an `InjectionToken`. No component, store, or feature imports a concrete implementation.

- `IQuoteService` declares the behavioral contract and `QUOTE_SERVICE` is its `InjectionToken`. The interface, the token, and each implementation live in separate files.
- Contracts are named `I<Entity>Service` in the singular, with no `Api` suffix. The `I` prefix marks a swappable contract; data shapes (`QuoteResult`) take no prefix, and the production implementation takes the unprefixed name (`QuoteService`), never an `Impl` suffix.
- Consumers call `inject(QUOTE_SERVICE)` only. Application composition binds the token to the HTTP adapter in production and to a controlled mock under Playwright, so a test never reaches the real implementation.
- HTTP calls and observable-to-signal conversion stay inside the `api` implementations. Nothing outside `api` names `HttpClient`, a URL, or a status code: a `domain` component injects a token and receives finished data.

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

The tree shows the maintained repository layout. Generated output, dependencies,
and local tooling folders are omitted. Each Angular project keeps its source in
its own `src/` directory.

```text
quinntyne-brown-studio/
|-- .github/
|   `-- workflows/                    # Verification, packaging, and design-system deployment
|-- backend/
|   |-- Qbs.slnx                      # API, worker, libraries, and acceptance-test solution
|   |-- src/
|   |   |-- Qbs.Api/                  # Controllers and API composition
|   |   |   |-- Controllers/
|   |   |   |-- Filters/
|   |   |   |-- Models/
|   |   |   `-- Persistence/          # EF design-time context factory
|   |   |-- Qbs.Application/          # Use cases, MediatR handlers, and ports
|   |   |   |-- Catalog/              # Commands and handlers grouped by entity or configuration
|   |   |   |-- Clients/
|   |   |   |-- Photos/
|   |   |   |-- Ports/
|   |   |   |-- Presentation/
|   |   |   |-- Quotations/
|   |   |   `-- Scheduling/
|   |   |-- Qbs.Domain/               # Domain types and business rules
|   |   |   |-- Entities/             # Identity-bearing types and the Entity base class
|   |   |   |-- Enums/
|   |   |   |-- Exceptions/
|   |   |   |-- Models/               # Inputs, results, and supporting data types
|   |   |   |-- Policies/
|   |   |   `-- ValueObjects/
|   |   |-- Qbs.Infrastructure/       # Persistence, identity, storage, and external adapters
|   |   |   |-- Adapters/
|   |   |   |-- DependencyInjection/
|   |   |   |-- Identity/
|   |   |   |-- Persistence/
|   |   |   |   `-- Migrations/
|   |   |   |-- Processing/
|   |   |   |-- Serialization/
|   |   |   `-- Storage/
|   |   `-- Qbs.Worker/               # Background processing host
|   `-- tests/
|       `-- Qbs.AcceptanceTests/      # API integration and infrastructure acceptance tests
|-- frontend/
|   |-- angular.json                  # Angular workspace and build targets
|   |-- component-catalog.json        # Application component inventory and contracts
|   |-- styles.css                    # Mirrored design tokens and global element styles
|   |-- package.json
|   `-- projects/
|       |-- api/                      # Service contracts, tokens, HTTP adapters, and mocks
|       |   `-- src/lib/              # <entity>.contract.ts and <entity>.token.ts pairs
|       |-- components/               # Dumb presentation components: no api, no domain, publishable
|       |   `-- src/lib/<component>/  # One folder per component: class, template, styles
|       |-- domain/                   # Shared types and the studio-aware components that use them
|       |   |-- src/lib/              # Shared types, one file per type
|       |   `-- src/lib/<component>/  # Components that inject api tokens
|       |-- application/              # Pages, shell, routing, providers, and route-owned state
|       |   `-- src/lib/<page>/       # One folder per page
|       |-- marketing/                # Public site bootstrap: galleries, quote calculator, prices
|       |-- admin/                    # Admin bootstrap: sessions, photos, schedules, rates, content
|       `-- client/                   # Client bootstrap: galleries, albums, print requests
|-- design-system/
|   |-- package.json                  # Independent static-site build and test commands
|   |-- component-manifest.json       # Catalog components, classes, states, and examples
|   |-- index.html                    # Catalog entry point
|   |-- preview.html                  # Isolated component and screen previews
|   |-- assets/                       # Design tokens, styles, catalog content, and behavior
|   |-- scripts/                      # Catalog validation and built-artifact checks
|   `-- tests/                        # Design-system Playwright specs
|       `-- pages/                    # Catalog and preview page objects
|-- e2e/
|   |-- package.json                  # Independent application acceptance-test project
|   |-- page-objects/                 # Screen selectors, interactions, and API mocks
|   `-- specs/                        # Application Playwright acceptance scenarios
|-- deploy/                           # Docker images, gateway, Azure Bicep, and database setup
|-- scripts/                          # Development startup, smoke checks, and documentation tooling
`-- docs/
    |-- components.md                 # Angular presentation-component documentation
    |-- specs/                        # L1/L2 requirements and decision baseline
    |-- detailed-designs/             # Feature designs, diagrams, contracts, and acceptance register
    |-- implementation/               # Implementation status, gaps, and verification evidence
    `-- mocks/                        # HTML prototypes, shared assets, and prototype browser checks
```
