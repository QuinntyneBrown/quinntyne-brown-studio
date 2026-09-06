# Platform completion

The delivery scope is all remaining feature behavior and locally runnable acceptance under the existing L1/L2 requirements and OD-01 through OD-11. The local feature implementation is complete. External RAW-camera qualification, full-capacity measurements, AI usefulness evaluation, and live deployment evidence remain separately identified prerequisites.

Baseline: 97 backend tests; 20 of 82 acceptance entries Complete and 62 Partial. Existing correct behavior receives regression coverage without fabricated failing history. New behavior and defect fixes receive captured failing acceptance tests before implementation.

| Group | Given | When | Then |
| --- | --- | --- | --- |
| P01 Identity | Anonymous, authorized, wrong-role, and invalid-token visitors | They sign in, recover, accept invitations, or sign out | Access and corrections are explicit; unsuccessful operations preserve the draft and do not grant access. |
| P02 Configuration | Existing or unconfigured studio records | Administrators save valid, invalid, and conflicting edits | Valid changes persist and affect quotes/public pricing; rejected edits preserve the last valid configuration and draft. |
| P03 Scheduling | Toronto intervals, working windows, and commitments | Schedules and sessions are changed, including concurrent writes | Quarter-hour/DST rules and conflicts are enforced consistently without quote reservations. |
| P04 Publication | Private photos and draft/published content | Administrators select, order, publish, or unpublish | Public views contain only the intended published revision and eligible photo bytes. |
| P05 Ingestion | Supported, invalid, interrupted, and already-finalized files | A batch uploads, resumes, and processes | Originals and association are preserved; per-file status is accurate; retries do not duplicate accepted photos. |
| P06 Client work | Assigned and revoked galleries, albums, and changing print prices | Clients browse, edit, and submit; administrators review | Access is enforced at every boundary, albums retain order, and print snapshots are server-priced and idempotent. |
| P07 AI and retention | Ready photos, failed analysis, expiry boundaries, and references | Analysis or retention jobs run and are retried | Manual review remains available; advice never publishes; expiry and deletion protections hold. |
| P08 Product baseline | All supported tasks and adverse states | Visitors use each browser/viewport and keyboard controls | Tasks remain operable, feedback readable, drafts retained, and focus managed. |

The existing detailed-design feature pages and approved HTML mocks remain the reference. Payment, booking confirmation, contact submission, and the prototype-only dashboard remain outside scope. Implementation, test names and final verification are linked below.

## Final local verification

| Check | Result |
| --- | --- |
| Backend acceptance | 118 passed, zero failed or skipped, including disposable real LocalDB concurrency/isolation checks |
| Application acceptance | 68 scenarios across nine browser/viewport combinations: 612 distinct passing outcomes assembled from the full and focused runs |
| Design-system acceptance | 11 scenarios across nine combinations: 99 distinct passing outcomes, including the corrected aggregate-loop rerun and final component/dialog reruns |
| Packaged integration | Real HTTPS, Identity, LocalDB, browser hash/block upload, JPEG worker, invitation, assignment, albums, prints, inbox review and revocation passed |
| Packaging | Models entry point, four libraries, three production applications, API, worker and qualification CLI built; published worker started against LocalDB |
| Review | 26 Angular components inventoried; standalone catalog artifact and detailed-design documentation checks passed |

The [machine-readable report](platform-verification.json) retains original run statistics, selected reruns and scenario outcomes. The 612 browser passes are combined evidence, not a claim that one uninterrupted 612-test run passed. An initial price-retry fixture race was corrected; temporary browser timing failures and a subpixel geometry assertion were investigated and rerun. Current packaged mobile/desktop screenshots were visually reviewed; synthetic JPEG fixture images are intentionally solid-colour samples.

The preview runs at `https://localhost:7453`, with administration under `/admin` and client access under `/client`. It uses its own `QbsPlatformAcceptance_` database. The original data and runtime databases are separate from this fixture workspace.

## Increment evidence

- P01: three failing browser scenarios captured in `.artifacts/platform/red-identity`; all three passed after implementation. Missing invitation tokens, wrong-role sign-in, and sign-out failures now retain appropriate state and show recoverable feedback. Repeated under controlled feature bindings with the four existing platform scenarios: seven passed in `.artifacts/platform/controlled-identity.log`.
- Foundation: the models entry point builds independently, removing the API/domain component build cycle. `PhotoGrid` belongs to the domain library. Typed feature service contracts replace generic transport calls in application screens. Acceptance composition selects controlled implementations for all products and uses dedicated ports 4420–4422.
- Backend dispatch: catalog reads, identity commands, scheduling, publication, client workflows, and media operations dispatch through MediatR. All 97 baseline integration tests passed after this refactor (`.artifacts/platform/platform-dispatch.trx`). This is regression evidence for existing behavior, not invented test-first history.
- P06: four print-preview API cases failed against the missing endpoint (`red-print-review.trx`). The implemented preview prices current catalog revisions, enforces photo access and quantities, and creates no request. Four preview cases plus the existing immutable/idempotent submission case passed (`green-print-review.trx`). Two browser cases failed before UI integration (`red-print-browser.log`) and passed after it (`green-print-browser.log`): server-priced review and preserved-selection retry. Subsequent acceptance covers obsolete success/failure responses, changed-price re-review, edited-note submission keys, album editing, inbox filtering and the real client workflow.

- P03/P04: captured failures for Toronto controls, explicit draft publication, multi-page curation and public outage recovery passed after implementation. The date/time field requires a fall-back occurrence and rejects nonexistent local times. Gallery order and cover selection include photos beyond the first 50 records.
- P05: two resume-storage failures and an unreadable-file failure were captured before fixes. Grant renewal was subsequently captured failing, then passed. Reload/reselection preserves acknowledged blocks and repeated finalization does not duplicate photos. The mixed-format case and existing block-resume behavior are regression evidence, not invented red history.
- P06: captured failures for edited-note submission keys, authoritative unit-price display and inbox filtering passed after implementation. Additional passing checks cover obsolete pricing success/failure, conflict re-review, album ordering, unavailable placeholders, preserved concurrent-edit drafts and immutable inbox details.
- P07: six malformed-advice cases failed because invalid results were recorded as successful. Domain validation now requires the correct photo, all three distinct rubric criteria, defined outcomes, explanations, recommendation and provenance. These cases, provider outage/manual access/retry, existing processing cases and qualification cases passed together: 16 tests (`green-analysis.trx`). The first outage test had an incorrect retry field name; that test correction is not claimed as a product defect.
- P08: four page-load recovery cases failed before implementation and passed afterward. A failed refresh following a successful save now remains visible with a retry. Typed signal-backed route services own behavior; reusable catalog, configuration, album, print and session regions live in the domain library.
- Fullstack: the packaged applications completed the real HTTPS/API/LocalDB workflow in 31 seconds (`fullstack.log`, `fullstack-results.json`). It includes browser hashing/block transfer, worker JPEG conversion, captured invitation, assignment, client album, server-priced print submission, inbox review and denied private bytes after revocation. The run uses a uniquely named acceptance database and controlled development external adapters; normal production persistence remains LocalDB.
- P02/P07/P08: three final browser failures demonstrated stale studio-address candidates, selection of non-ready photos and missing image retry. All passed after implementation. A further failing touch-target check led to the shared 44-pixel dialog/selection fix. Vendor-role persistence, public promotion withdrawal and gallery assignment/revocation are regression checks for existing behavior.
- P02: the saved lead-time threshold API regression changes eligibility, restores the prior threshold and verifies the next quote each time (`saved-threshold.trx`); the final 118-test suite includes this case.
- Qualification commands: six captured failing command cases passed after implementation (`green-qualification.trx`). They prove real JPEG measurement, pinned-digest rejection and explicit blocked reports when external evidence inputs are absent. They do not close camera, capacity, AI or environment gates.

## Requirements, mocks and designs

The slice implements the public-to-studio-to-client workflow across all 69 L2 requirements. The register records 74 Complete and 8 Partial criteria; the eight retain external qualification or historical test-first evidence gaps. The detailed Given/When/Then clauses remain in [L2](../specs/L2.md); the [82-entry acceptance register](../detailed-designs/acceptance.md) owns criterion status. The [23 feature designs](../detailed-designs/README.md#feature-navigation) supply contracts, policies and reviewed diagrams.

| Feature group | Requirements | Approved mock/design reference | Controlled acceptance behavior |
| --- | --- | --- | --- |
| Identity and client access | L2-003, 032–034, 062–063 | [Client access designs](../detailed-designs/client-access/invite-and-authenticate-users/README.md), prototype authentication/client screens | Roles, authenticated sessions, invitation/recovery success and rejection, assignments and empty galleries |
| Marketing publication | L2-004–007 | [Publication designs](../detailed-designs/public-presentation/publish-galleries/README.md), prototype marketing/content/gallery screens | Published snapshots, drafts, withdrawal, ordered photo pages, public errors and retries |
| Quotes and configuration | L2-008–020, 055–057 | [Live quote](live-quote-slice.md), configuration designs and prototype rate/studio/discount/print forms | Typed configuration records, explicit resolved addresses, controlled quote results and injected save/load conflicts |
| Scheduling and resources | L2-021–025, 058, 069 | [Scheduling design](../detailed-designs/scheduling/manage-photographer-schedules/README.md), prototype session/schedule/equipment/vendor screens | Toronto intervals, photographer records, versions and rejected updates |
| Ingestion and photo review | L2-026–031, 059–061, 067 | [Photo upload design](../detailed-designs/session-photos/upload-session-photos/README.md), prototype upload/review/retention states | Manifest decisions, block acknowledgments, interruptions, grant expiry, per-photo statuses, analysis and retention impact |
| Albums and print requests | L2-035–037, 064–065 | [Print design](../detailed-designs/prints-and-albums/submit-and-review-print-requests/README.md), prototype client collection/print screens and design-system inbox pattern | Album order/placeholders, authoritative price review, response loss/conflicts, submitted/reviewed snapshots |
| Shared product and delivery | L2-001–002, 038–054, 066, 068 | [Design-system catalog](../../design-system/README.md), [engineering design](../detailed-designs/engineering-delivery/deliver-traceable-feature-increments/README.md) | Standalone visual examples, keyboard/dialog states, explicit service replacement and isolated local integration |

Mocks live in the standalone E2E page-object fixtures and are selected only by acceptance composition. Unsupported fixture operations fail explicitly. Camera quality, provider usefulness and capacity are never mocked into an acceptance claim. The original [HTML prototypes](../mocks/README.md) remain visual references and are not runtime dependencies.
