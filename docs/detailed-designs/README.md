# Quinntyne Brown Studio — detailed designs

This design set is the approved baseline for the platform, written against the 2026-09-05 requirements. The [HTML prototype](../mocks/README.md) remains the visual reference, and the implementation is in the repository. Three screens differ between the prototype and the requirement set, and the prototype README names them. The [implementation report](../implementation/README.md) records the delivered structure, verification results, and remaining acceptance evidence.

The feature pages contain their background, concrete components and interfaces, exact L2 requirements with L1 parents, and inline rendered diagrams. The [shared architecture](architecture.md), [decision baseline](../specs/decisions.md), and [acceptance register](acceptance.md) establish common contracts and evidence gates.

## Feature navigation

| Subsystem | Feature | Primary L2 requirements |
| --- | --- | --- |
| `public-presentation` | [Publish galleries](public-presentation/publish-galleries/README.md) | `L2-004`, `L2-005`, `L2-063` |
| `public-presentation` | [Manage marketing content](public-presentation/manage-marketing-content/README.md) | `L2-005` |
| `public-presentation` | [Publish promotions](public-presentation/publish-promotions/README.md) | `L2-007` |
| `quotations` | [Calculate a live quote](quotations/calculate-live-quote/README.md) | `L2-008`, `L2-009`, `L2-010`, `L2-011`, `L2-012`, `L2-013`, `L2-055`, `L2-056` |
| `quotations` | [Apply discounts](quotations/apply-discounts/README.md) | `L2-014`, `L2-015`, `L2-016`, `L2-017`, `L2-057` |
| `quotations` | [Configure rates](quotations/configure-rates/README.md) | `L2-018`, `L2-055` |
| `quotations` | [Configure studios](quotations/configure-studios/README.md) | `L2-019`, `L2-055`, `L2-056` |
| `scheduling` | [Manage photographer schedules](scheduling/manage-photographer-schedules/README.md) | `L2-021`, `L2-058` |
| `scheduling` | [Check session availability](scheduling/check-session-availability/README.md) | `L2-022`, `L2-058` |
| `studio-resources` | [Manage equipment](studio-resources/manage-equipment/README.md) | `L2-023` |
| `studio-resources` | [Manage preferred vendors](studio-resources/manage-preferred-vendors/README.md) | `L2-024`, `L2-025` |
| `session-photos` | [Upload session photos](session-photos/upload-session-photos/README.md) | `L2-026`, `L2-027`, `L2-028`, `L2-059`, `L2-060`, `L2-069` |
| `session-photos` | [Review session photos](session-photos/review-session-photos/README.md) | `L2-029`, `L2-060` |
| `session-photos` | [Suggest promising photos](session-photos/suggest-promising-photos/README.md) | `L2-030`, `L2-031`, `L2-067` |
| `session-photos` | [Manage photo retention](session-photos/manage-photo-retention/README.md) | `L2-061`, `L2-034` |
| `client-access` | [Invite and authenticate users](client-access/invite-and-authenticate-users/README.md) | `L2-003`, `L2-032`, `L2-062` |
| `client-access` | [Assign and view session galleries](client-access/assign-and-view-session-galleries/README.md) | `L2-033`, `L2-034`, `L2-063`, `L2-061` |
| `prints-and-albums` | [Configure and display print prices](prints-and-albums/configure-and-display-print-prices/README.md) | `L2-006`, `L2-020`, `L2-035` |
| `prints-and-albums` | [Submit and review print requests](prints-and-albums/submit-and-review-print-requests/README.md) | `L2-035`, `L2-036`, `L2-034`, `L2-064` |
| `prints-and-albums` | [Create and edit albums](prints-and-albums/create-and-edit-albums/README.md) | `L2-037`, `L2-034`, `L2-065` |
| `design-system` | [Browse the component catalog](design-system/browse-component-catalog/README.md) | `L2-001`, `L2-002`, `L2-046`, `L2-047`, `L2-066` |
| `design-system` | [Publish the static catalog](design-system/publish-static-catalog/README.md) | `L2-046`, `L2-048` |
| `engineering-delivery` | [Deliver traceable feature increments](engineering-delivery/deliver-traceable-feature-increments/README.md) | `L2-038`, `L2-039`, `L2-040`, `L2-041`, `L2-042`, `L2-043`, `L2-044`, `L2-045`, `L2-049`, `L2-050`, `L2-051`, `L2-052`, `L2-053`, `L2-054`, `L2-068` |

## Complete requirement coverage

Every L2 has at least one linked feature. Shared UX and access obligations recur where applicable. Architecture and delivery obligations govern all production slices and have their detailed primary treatment in engineering-delivery.

| L2 | Parent | Feature designs |
| --- | --- | --- |
| `L2-001` | `L1-001` | [publish-galleries](public-presentation/publish-galleries/README.md), [manage-marketing-content](public-presentation/manage-marketing-content/README.md), [publish-promotions](public-presentation/publish-promotions/README.md), [calculate-live-quote](quotations/calculate-live-quote/README.md), [apply-discounts](quotations/apply-discounts/README.md), [configure-rates](quotations/configure-rates/README.md), [configure-studios](quotations/configure-studios/README.md), [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md), [check-session-availability](scheduling/check-session-availability/README.md), [manage-equipment](studio-resources/manage-equipment/README.md), [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md), [upload-session-photos](session-photos/upload-session-photos/README.md), [review-session-photos](session-photos/review-session-photos/README.md), [suggest-promising-photos](session-photos/suggest-promising-photos/README.md), [manage-photo-retention](session-photos/manage-photo-retention/README.md), [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md), [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md), [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md), [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md), [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md), [browse-component-catalog](design-system/browse-component-catalog/README.md), [publish-static-catalog](design-system/publish-static-catalog/README.md) |
| `L2-002` | `L1-001` | [publish-galleries](public-presentation/publish-galleries/README.md), [manage-marketing-content](public-presentation/manage-marketing-content/README.md), [publish-promotions](public-presentation/publish-promotions/README.md), [calculate-live-quote](quotations/calculate-live-quote/README.md), [apply-discounts](quotations/apply-discounts/README.md), [configure-rates](quotations/configure-rates/README.md), [configure-studios](quotations/configure-studios/README.md), [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md), [check-session-availability](scheduling/check-session-availability/README.md), [manage-equipment](studio-resources/manage-equipment/README.md), [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md), [upload-session-photos](session-photos/upload-session-photos/README.md), [review-session-photos](session-photos/review-session-photos/README.md), [suggest-promising-photos](session-photos/suggest-promising-photos/README.md), [manage-photo-retention](session-photos/manage-photo-retention/README.md), [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md), [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md), [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md), [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md), [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md), [browse-component-catalog](design-system/browse-component-catalog/README.md), [publish-static-catalog](design-system/publish-static-catalog/README.md) |
| `L2-003` | `L1-001` | [publish-galleries](public-presentation/publish-galleries/README.md), [manage-marketing-content](public-presentation/manage-marketing-content/README.md), [publish-promotions](public-presentation/publish-promotions/README.md), [apply-discounts](quotations/apply-discounts/README.md), [configure-rates](quotations/configure-rates/README.md), [configure-studios](quotations/configure-studios/README.md), [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md), [manage-equipment](studio-resources/manage-equipment/README.md), [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md), [upload-session-photos](session-photos/upload-session-photos/README.md), [review-session-photos](session-photos/review-session-photos/README.md), [suggest-promising-photos](session-photos/suggest-promising-photos/README.md), [manage-photo-retention](session-photos/manage-photo-retention/README.md), [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md), [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md), [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md), [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md) |
| `L2-004` | `L1-002` | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `L2-005` | `L1-002` | [publish-galleries](public-presentation/publish-galleries/README.md), [manage-marketing-content](public-presentation/manage-marketing-content/README.md) |
| `L2-006` | `L1-002` | [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md) |
| `L2-007` | `L1-002` | [publish-promotions](public-presentation/publish-promotions/README.md) |
| `L2-008` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `L2-009` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `L2-010` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `L2-011` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `L2-012` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `L2-013` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `L2-014` | `L1-004` | [apply-discounts](quotations/apply-discounts/README.md) |
| `L2-015` | `L1-004` | [apply-discounts](quotations/apply-discounts/README.md) |
| `L2-016` | `L1-004` | [apply-discounts](quotations/apply-discounts/README.md) |
| `L2-017` | `L1-004` | [apply-discounts](quotations/apply-discounts/README.md) |
| `L2-018` | `L1-005` | [configure-rates](quotations/configure-rates/README.md) |
| `L2-019` | `L1-005` | [configure-studios](quotations/configure-studios/README.md) |
| `L2-020` | `L1-005` | [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md) |
| `L2-021` | `L1-006` | [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md) |
| `L2-022` | `L1-006` | [check-session-availability](scheduling/check-session-availability/README.md) |
| `L2-023` | `L1-007` | [manage-equipment](studio-resources/manage-equipment/README.md) |
| `L2-024` | `L1-007` | [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md) |
| `L2-025` | `L1-007` | [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md) |
| `L2-026` | `L1-008` | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `L2-027` | `L1-008` | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `L2-028` | `L1-008` | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `L2-029` | `L1-009` | [review-session-photos](session-photos/review-session-photos/README.md) |
| `L2-030` | `L1-009` | [suggest-promising-photos](session-photos/suggest-promising-photos/README.md) |
| `L2-031` | `L1-009` | [suggest-promising-photos](session-photos/suggest-promising-photos/README.md) |
| `L2-032` | `L1-010` | [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md) |
| `L2-033` | `L1-010` | [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md) |
| `L2-034` | `L1-010` | [manage-photo-retention](session-photos/manage-photo-retention/README.md), [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md), [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md), [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md), [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md) |
| `L2-035` | `L1-011` | [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md), [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md) |
| `L2-036` | `L1-011` | [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md) |
| `L2-037` | `L1-011` | [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md) |
| `L2-038` | `L1-012` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-039` | `L1-012` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-040` | `L1-012` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-041` | `L1-013` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-042` | `L1-013` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-043` | `L1-013` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-044` | `L1-013` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-045` | `L1-013` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-046` | `L1-014` | [browse-component-catalog](design-system/browse-component-catalog/README.md), [publish-static-catalog](design-system/publish-static-catalog/README.md) |
| `L2-047` | `L1-014` | [browse-component-catalog](design-system/browse-component-catalog/README.md) |
| `L2-048` | `L1-014` | [publish-static-catalog](design-system/publish-static-catalog/README.md) |
| `L2-049` | `L1-015` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-050` | `L1-015` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-051` | `L1-015` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-052` | `L1-016` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-053` | `L1-016` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-054` | `L1-016` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-055` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md), [configure-rates](quotations/configure-rates/README.md), [configure-studios](quotations/configure-studios/README.md) |
| `L2-056` | `L1-003` | [calculate-live-quote](quotations/calculate-live-quote/README.md), [configure-studios](quotations/configure-studios/README.md) |
| `L2-057` | `L1-004` | [apply-discounts](quotations/apply-discounts/README.md) |
| `L2-058` | `L1-006` | [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md), [check-session-availability](scheduling/check-session-availability/README.md) |
| `L2-059` | `L1-008` | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `L2-060` | `L1-008` | [upload-session-photos](session-photos/upload-session-photos/README.md), [review-session-photos](session-photos/review-session-photos/README.md) |
| `L2-061` | `L1-008` | [manage-photo-retention](session-photos/manage-photo-retention/README.md), [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md) |
| `L2-062` | `L1-010` | [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md) |
| `L2-063` | `L1-010` | [publish-galleries](public-presentation/publish-galleries/README.md), [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md) |
| `L2-064` | `L1-011` | [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md) |
| `L2-065` | `L1-011` | [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md) |
| `L2-066` | `L1-001` | [publish-galleries](public-presentation/publish-galleries/README.md), [manage-marketing-content](public-presentation/manage-marketing-content/README.md), [publish-promotions](public-presentation/publish-promotions/README.md), [calculate-live-quote](quotations/calculate-live-quote/README.md), [apply-discounts](quotations/apply-discounts/README.md), [configure-rates](quotations/configure-rates/README.md), [configure-studios](quotations/configure-studios/README.md), [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md), [check-session-availability](scheduling/check-session-availability/README.md), [manage-equipment](studio-resources/manage-equipment/README.md), [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md), [upload-session-photos](session-photos/upload-session-photos/README.md), [review-session-photos](session-photos/review-session-photos/README.md), [suggest-promising-photos](session-photos/suggest-promising-photos/README.md), [manage-photo-retention](session-photos/manage-photo-retention/README.md), [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md), [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md), [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md), [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md), [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md), [browse-component-catalog](design-system/browse-component-catalog/README.md), [publish-static-catalog](design-system/publish-static-catalog/README.md) |
| `L2-067` | `L1-009` | [suggest-promising-photos](session-photos/suggest-promising-photos/README.md) |
| `L2-068` | `L1-012` | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `L2-069` | `L1-008` | [upload-session-photos](session-photos/upload-session-photos/README.md) |

## Diagram conventions

Each feature carries a context, container, component, and class view, plus one sequence per behavior. Container views show the deployable units a feature touches. They omit the shared-origin gateway, the container registry, and the monitoring workspace, which serve every feature without carrying feature behavior; the [shared architecture](architecture.md) describes those. Screen and handler names identify tasks and operations rather than delivered classes, and the [screen ownership](contracts.md#screen-ownership) and [behavior ownership](contracts.md#behavior-ownership) tables give the delivered mapping.

## Verification

`python docs/detailed-designs/verify.py` checks requirement identity and wording, acceptance coverage, local links, feature heading order, C4 macros, sequence trace references, and source/image pairing. It also invokes PlantUML syntax checking when the local renderer is available.

Render changed sources with `python scripts/render-diagrams.py`, which renders every diagram whose source or image no longer matches its reviewed baseline and records the new pair in `diagram-manifest.json`. It finds PlantUML through `PLANTUML_JAR`, `C:/tools/plantuml.jar`, `~/plantuml.jar`, or `plantuml` on PATH. The PNG files are committed alongside their sources so GitHub displays them without a local renderer.

`G-RAW`, `G-UPLOAD`, `G-AI`, `G-MEDIATR`, and `G-ENV` retain explicit evidence prerequisites in the decision baseline. They do not represent completed product tests. No payments, confirmed booking, shipping, automated fulfilment, or procurement workflow is introduced.
