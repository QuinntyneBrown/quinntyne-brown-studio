# Publish the static catalog

## Overview

Quinntyne Brown Studio supports photography discovery, studio administration, and client deliverables. A catalog artifact is the built static HTML, JavaScript, CSS, and contracts for the root-level `design-system` product. Contributors publish it independently from the studio backend, and a static host serves its navigation and component examples.

## Description

Status: implemented as the standalone `design-system` product beside `backend/` and `frontend/`. The names below identify its build, its artifact, and the delivery steps around them.

`CatalogBuild` runs inside `design-system` alone. It validates the manifest against the stylesheet and the fixtures, runs the browser checks with product API requests blocked, and then compiles the static entry point, the isolated preview page, and the hashed assets. `CatalogArtifact` identifies the revision, the entry point, the asset manifest, and the navigation configuration. The build carries no studio secret, no private-photo URL, and no product API call.

The build command is `npm run build` inside `design-system`, and the artifact directory is `design-system/dist`. `npm run check:artifact` then serves that directory under the checked-in navigation configuration and exercises the deep links, the published manifest, the isolated preview, and the missing-asset response, so a routing mistake stops the pipeline before deployment. Deployment copies the complete directory to Azure Static Web Apps. The copied `staticwebapp.config.json` supplies the navigation fallback that keeps component, pattern, and dialog links resolvable, the response override for an unknown path, and the security headers. Hash-named assets use immutable caching; the entry point and the published manifest use revalidation.

`PublishCatalog` runs in the delivery pipeline defined by `deploy-design-system.yml`, which validates, tests, and builds before uploading. A failed contract check, browser check, or compile step leaves the deployed revision unchanged. A failed deployment smoke check restores the prior artifact. Hosting identifiers remain `G-ENV` evidence, so no environment claim follows from the pipeline definition alone.

Acceptance covers documented build prerequisites, direct deep-link navigation, a missing asset, backend isolation, and returning to the prior artifact.

**Interfaces**

- `CLI npm run build in design-system → design-system/dist with its navigation configuration and manifest`
- `Static HTTP GET / and /components/... → catalog entry point and bundled assets`

**Behavior ownership**

| Operation | Owner | Responsibility |
| --- | --- | --- |
| `BuildCatalog` | `CatalogBuild` | Validate contracts, run browser checks with the backend blocked, and compile the artifact. |
| `PublishCatalog` | `StaticArtifactPublisher` | Publish the validated versioned artifact and check deep links. |

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
| `L2-048` | `L1-014` | The design system shall be deployable and browsable as a static web application without a running studio backend. |
| `L2-066` | `L1-001` | Platform interfaces shall follow the approved HTML prototype at 390, 768, and 1440 CSS-pixel widths across Chromium, Firefox, and WebKit, with keyboard-operable controls and readable validation and failure states. |

## Diagrams

The context identifies the people and systems involved in this capability. External services participate only where the description requires their data or effects.

![c4 context for publish the static catalog](diagrams/c4-context.png)

The container view locates the catalog, its checks, and its static host, and keeps them separate from the studio applications.

![c4 container for publish the static catalog](diagrams/c4-container.png)

The component view separates the catalog application from the manifest, the fixtures, and the authoritative stylesheet that the build compiles.

![c4 component for publish the static catalog](diagrams/c4-component.png)

The class view shows `CatalogArtifact` and the build and publisher roles that produce and place it.

![class structure for publish the static catalog](diagrams/class-structure.png)

`BuildCatalog`: Validate contracts, run browser checks with the backend blocked, and compile the artifact. Contract, browser, or compile failure: the deployed revision is unchanged. This behavior has no studio backend participant; delivery tooling is shown separately.

![sequence build catalog for publish the static catalog](diagrams/sequence-build-catalog.png)

`PublishCatalog`: Publish the validated versioned artifact and check deep links. Deployment smoke failure: restore the prior artifact. This behavior has no studio backend participant; delivery tooling is shown separately.

![sequence publish catalog for publish the static catalog](diagrams/sequence-publish-catalog.png)
