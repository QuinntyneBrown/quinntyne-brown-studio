# Acceptance design register

Every existing and added acceptance criterion is listed below. Status describes production implementation; prototype browser checks do not establish production acceptance coverage. The implementing layer column is a design assignment, not evidence that tests exist.

## Delivery conventions

Backend criteria use a failing WebApplicationFactory integration test with a controlled store fake before implementation. Frontend criteria use a failing Playwright Page Object scenario with mocked API responses. Criteria involving both layers receive separate evidence for each layer. Existing test paths and names replace the em dash only after those tests exist. Red/green run artifacts identify the criterion and source revision. Partial implementation records its uncovered behavior.

Architecture and process criteria use source, configuration, build, and development-history evidence. Browser tests do not establish licensing, provider accuracy, SQL isolation, or RAW compatibility. Staging integration checks supplement the controlled acceptance tests for those boundaries.

## Scenario coverage

| Acceptance ID | Planned layer / evidence | Status | Test file and name | Design |
| --- | --- | --- | --- | --- |
| `AC-L2-001-01` | Frontend; source/build evidence where applicable | Not implemented | — | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `AC-L2-002-01` | Frontend; source/build evidence where applicable | Not implemented | — | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `AC-L2-003-01` | Frontend + Backend | Not implemented | — | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `AC-L2-004-01` | Frontend + Backend | Not implemented | — | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `AC-L2-005-01` | Frontend + Backend | Not implemented | — | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `AC-L2-006-01` | Frontend + Backend | Not implemented | — | [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md) |
| `AC-L2-007-01` | Frontend + Backend | Not implemented | — | [publish-promotions](public-presentation/publish-promotions/README.md) |
| `AC-L2-008-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-009-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-009-02` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-010-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-011-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-011-02` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-012-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-012-02` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-013-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-014-01` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-014-02` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-015-01` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-015-02` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-015-03` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-016-01` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-017-01` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-017-02` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-018-01` | Frontend + Backend | Not implemented | — | [configure-rates](quotations/configure-rates/README.md) |
| `AC-L2-018-02` | Frontend + Backend | Not implemented | — | [configure-rates](quotations/configure-rates/README.md) |
| `AC-L2-019-01` | Frontend + Backend | Not implemented | — | [configure-studios](quotations/configure-studios/README.md) |
| `AC-L2-020-01` | Frontend + Backend | Not implemented | — | [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md) |
| `AC-L2-021-01` | Frontend + Backend | Not implemented | — | [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md) |
| `AC-L2-022-01` | Frontend + Backend | Not implemented | — | [check-session-availability](scheduling/check-session-availability/README.md) |
| `AC-L2-023-01` | Frontend + Backend | Not implemented | — | [manage-equipment](studio-resources/manage-equipment/README.md) |
| `AC-L2-024-01` | Frontend + Backend | Not implemented | — | [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md) |
| `AC-L2-025-01` | Frontend + Backend | Not implemented | — | [manage-preferred-vendors](studio-resources/manage-preferred-vendors/README.md) |
| `AC-L2-026-01` | Frontend + Backend | Not implemented | — | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `AC-L2-027-01` | Frontend + Backend | Not implemented | — | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `AC-L2-028-01` | Frontend + Backend | Not implemented | — | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `AC-L2-028-02` | Frontend + Backend | Not implemented | — | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `AC-L2-029-01` | Frontend + Backend | Not implemented | — | [review-session-photos](session-photos/review-session-photos/README.md) |
| `AC-L2-030-01` | Frontend + Backend; external qualification evidence | Not implemented | — | [suggest-promising-photos](session-photos/suggest-promising-photos/README.md) |
| `AC-L2-030-02` | Frontend + Backend; external qualification evidence | Not implemented | — | [suggest-promising-photos](session-photos/suggest-promising-photos/README.md) |
| `AC-L2-031-01` | Frontend + Backend; external qualification evidence | Not implemented | — | [suggest-promising-photos](session-photos/suggest-promising-photos/README.md) |
| `AC-L2-032-01` | Frontend + Backend | Not implemented | — | [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md) |
| `AC-L2-032-02` | Frontend + Backend | Not implemented | — | [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md) |
| `AC-L2-033-01` | Frontend + Backend | Not implemented | — | [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md) |
| `AC-L2-033-02` | Frontend + Backend | Not implemented | — | [assign-and-view-session-galleries](client-access/assign-and-view-session-galleries/README.md) |
| `AC-L2-034-01` | Frontend + Backend | Not implemented | — | [manage-photo-retention](session-photos/manage-photo-retention/README.md) |
| `AC-L2-035-01` | Frontend + Backend | Not implemented | — | [configure-and-display-print-prices](prints-and-albums/configure-and-display-print-prices/README.md) |
| `AC-L2-036-01` | Frontend + Backend | Not implemented | — | [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md) |
| `AC-L2-036-02` | Frontend + Backend | Not implemented | — | [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md) |
| `AC-L2-037-01` | Frontend + Backend | Not implemented | — | [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md) |
| `AC-L2-038-01` | Backend / deployment; source/build/history evidence | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-039-01` | Backend / deployment; source/build/history evidence | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-040-01` | Backend / deployment; source/build/history evidence | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-041-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-042-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-043-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-044-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-045-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-046-01` | Frontend; source/build evidence where applicable | Not implemented | — | [browse-component-catalog](design-system/browse-component-catalog/README.md) |
| `AC-L2-047-01` | Frontend; source/build evidence where applicable | Not implemented | — | [browse-component-catalog](design-system/browse-component-catalog/README.md) |
| `AC-L2-048-01` | Frontend; source/build evidence where applicable | Not implemented | — | [publish-static-catalog](design-system/publish-static-catalog/README.md) |
| `AC-L2-049-01` | Backend / deployment; source/build/history evidence | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-050-01` | Backend / deployment; source/build/history evidence | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-051-01` | Backend / deployment; source/build/history evidence | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-052-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-053-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-054-01` | Frontend; source/build evidence where applicable | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-055-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-056-01` | Frontend + Backend | Not implemented | — | [calculate-live-quote](quotations/calculate-live-quote/README.md) |
| `AC-L2-057-01` | Frontend + Backend | Not implemented | — | [apply-discounts](quotations/apply-discounts/README.md) |
| `AC-L2-058-01` | Frontend + Backend | Not implemented | — | [manage-photographer-schedules](scheduling/manage-photographer-schedules/README.md) |
| `AC-L2-059-01` | Frontend + Backend | Not implemented | — | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `AC-L2-060-01` | Frontend + Backend; external qualification evidence | Not implemented | — | [upload-session-photos](session-photos/upload-session-photos/README.md) |
| `AC-L2-061-01` | Frontend + Backend | Not implemented | — | [manage-photo-retention](session-photos/manage-photo-retention/README.md) |
| `AC-L2-062-01` | Frontend + Backend | Not implemented | — | [invite-and-authenticate-users](client-access/invite-and-authenticate-users/README.md) |
| `AC-L2-063-01` | Frontend + Backend | Not implemented | — | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `AC-L2-064-01` | Frontend + Backend | Not implemented | — | [submit-and-review-print-requests](prints-and-albums/submit-and-review-print-requests/README.md) |
| `AC-L2-065-01` | Frontend + Backend | Not implemented | — | [create-and-edit-albums](prints-and-albums/create-and-edit-albums/README.md) |
| `AC-L2-066-01` | Frontend; source/build evidence where applicable | Not implemented | — | [publish-galleries](public-presentation/publish-galleries/README.md) |
| `AC-L2-067-01` | Frontend + Backend; external qualification evidence | Not implemented | — | [suggest-promising-photos](session-photos/suggest-promising-photos/README.md) |
| `AC-L2-068-01` | Backend / deployment; source/build/history evidence | Not implemented | — | [deliver-traceable-feature-increments](engineering-delivery/deliver-traceable-feature-increments/README.md) |
| `AC-L2-069-01` | Frontend + Backend | Not implemented | — | [upload-session-photos](session-photos/upload-session-photos/README.md) |

## Cross-feature acceptance scenarios

- Save rates, studio base, and discounts; recalculate independently expected four-service quotes. Delay an earlier response beyond a newer response and verify it cannot replace the current quote.
- Exercise Toronto midnight, lead-time thresholds, both DST changes, occupied-interval adjacency, and simultaneous conflicting schedule writes. Availability queries create no commitments.
- Upload valid boundary batches and mixed invalid content under OD-04 network conditions. Interrupt, reselect local files, resume blocks, repeat finalization, and verify correct session association and original integrity.
- Revoke a gallery assignment between metadata and photo requests. Substitute other-client gallery, photo, album, and print identifiers directly. Verify absence of protected metadata and image bytes.
- Inject AI outage and malformed or wrong-photo results while browsing ready images. Validate model usefulness and camera previews only with the G-AI and G-RAW evidence sets.
- Reach retention notice/expiry boundaries, rerun the scheduler, extend expiry, and confirm that direct client requests enforce expiry even during a scheduler outage. Test publication and request races against deletion confirmation.
- Change print prices between selection and submission, retry a lost submission response, and race duplicate requests. Verify one immutable snapshot and accurate administrator review state.
- Edit albums concurrently and revoke selected photos. Preserve ordered unavailable placeholders without disclosing private bytes.
- Browse every catalog component and applicable state across the nine browser/viewport combinations with keyboard-only interaction. Block all studio-backend network calls during static catalog checks.

## Evidence gates

The [decision baseline](../specs/decisions.md#evidence-register) owns the remaining fixture, model, license, and environment evidence. Those fields remain `<TO SUPPLY>` until recorded evidence exists. All production acceptance statuses above remain Not implemented for this documentation-only deliverable.
