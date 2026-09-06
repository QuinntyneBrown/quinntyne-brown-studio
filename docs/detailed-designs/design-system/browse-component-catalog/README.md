# Browse the component catalog

## Overview

Quinntyne Brown Studio supports photography discovery, studio administration, and client deliverables. The component catalog is a contributor-facing product showing the platform's visual inventory. Each entry demonstrates a component the platform ships, with its classes, its states, and rendered examples. Examples make the approved visual language and task states reviewable without studio data.

## Description

Status: implemented as the standalone `design-system` product beside `backend/` and `frontend/`. The names below identify its published entries and its files.

`ComponentManifest` (`design-system/component-manifest.json`) is the versioned inventory. Each `CatalogEntry` carries an identifier, a category, the classes it owns, its states, and at least one `ComponentExample` with rendered markup. `CatalogApplication` reads that manifest and renders the navigation, the entry pages, the token foundations, and the deep-linkable routes. `CatalogFixtures` supplies the screen-pattern and dialog scenarios that compose those classes into whole screens. `IsolatedPreview` renders a single example without catalog chrome for focused review.

The catalog holds its own tokens, component styles, fixtures, and checks, and has no build, test, or runtime dependency on the Angular workspace, the studio API, or a database. `design-system/scripts/validate.mjs` compares the manifest with the stylesheet and the fixtures: a documented class without a definition, an example that renders none of its own classes, a scenario without markup, an unbalanced package script, or a reference outside the folder fails the catalog check before any browser starts.

Examples cover the states each entry declares, including validation, dependency failure, empty, processing, and revoked outcomes. Dialog scenarios keep native modal behavior, so Escape closes a dialog and focus returns to the control that opened it. The catalog's Playwright suite, written with page objects, opens every manifest entry and scenario at 390, 768, and 1440 CSS pixels with product API requests blocked, and asserts that no such request is attempted.

The Angular `components` library re-implements these classes for the product, and application screens reuse the block names rather than redefining them. The catalog stays the reference a contributor reviews before an application consumes a component. The catalog publishes every class the applications ship, including the application chrome, and each delivered component names the catalog entry that shows it, so a component added without an example fails the repository check. The [presentation inventory](../../../components.md) records the wider component target that is not built yet, and the screen patterns mirror delivered screens with local fixtures, so `L2-047` stays partially satisfied rather than claiming complete coverage.

Acceptance covers manifest completeness, recognizable examples, local-only fixtures, keyboard dialogs, error readability, and the supported viewport widths.

**Interfaces**

- `Component manifest ← entryId, exampleId → navigable rendered example with classes and states`
- `Isolated preview ← type, id, example → one rendered example without catalog chrome`

**Behavior ownership**

| Operation | Owner | Responsibility |
| --- | --- | --- |
| `BrowseComponentExample` | `CatalogApplication` | Resolve a manifest entry and render its example from local fixtures. |

The [shared architecture](../../architecture.md) defines authorization, wire conventions, persistence, environment boundaries, and delivery constraints. The [decision baseline](../../../specs/decisions.md) supplies exact policies and remaining evidence gates. Shared architecture requirements `L2-038` through `L2-045` and delivery requirements `L2-049` through `L2-054` apply to the implemented layers of this slice.

**Acceptance mapping**

The [acceptance register](../../acceptance.md) lists each applicable scenario with its implementing layer and current status. Feature tests exercise the success and failure behaviors described here. No production acceptance test exists merely because its scenario is designed.

## Requirements

Source: [L2 requirements](../../../specs/L2.md). Shared interface and delivery obligations have primary coverage in the engineering-delivery slice.

| L2 ID | Refines (L1) | Requirement |
| --- | --- | --- |
| `L2-001` | `L1-001` | The public and administrative applications shall support their specified tasks across the agreed desktop, tablet, and mobile viewport matrix. |
| `L2-002` | `L1-001` | The platform's interfaces shall use generous white space, clear task hierarchy, and controls limited to the specified workflows. |
| `L2-046` | `L1-014` | The design system shall be a maintained deliverable with its own entry point and build/deployment instructions, using the [referenced example](https://github.com/QuinntyneBrown/saturdaze/tree/main/design-system) as guidance. |
| `L2-047` | `L1-014` | The design system shall list every platform UI component with a recognizable name and a viewable example, and shall be updated when components are added or changed. |
| `L2-066` | `L1-001` | Platform interfaces shall follow the approved HTML prototype at 390, 768, and 1440 CSS-pixel widths across Chromium, Firefox, and WebKit, with keyboard-operable controls and readable validation and failure states. |

## Diagrams

The context identifies the people and systems involved in this capability. External services participate only where the description requires their data or effects.

![c4 context for browse the component catalog](diagrams/c4-context.png)

The container view locates the catalog, its checks, and its static host, and keeps them separate from the studio applications.

![c4 container for browse the component catalog](diagrams/c4-container.png)

The component view separates the catalog application from the manifest, the fixtures, and the authoritative stylesheet it renders.

![c4 component for browse the component catalog](diagrams/c4-component.png)

The class view shows the manifest structure: a `CatalogEntry` and its `ComponentExample` values, and the pattern and dialog families that reuse them.

![class structure for browse the component catalog](diagrams/class-structure.png)

`BrowseComponentExample`: Resolve a manifest entry and render its example from local fixtures. Unknown entry: catalog not-found view; declared failure state: the fixture for that state. This behavior has no studio backend participant; delivery tooling is shown separately.

![sequence browse component example for browse the component catalog](diagrams/sequence-browse-component-example.png)
