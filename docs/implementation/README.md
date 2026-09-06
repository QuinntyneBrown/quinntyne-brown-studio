# Implementation and acceptance evidence

The subsequent [backend rename](backend-rename.md) changes .NET identities to
`QuinntyneBrownStudio` and records a fresh backend regression run. Earlier reports
retain their historical assembly names.

The repository now contains the public website, administration and client applications, the standalone design system, the controller API, a background worker, LocalDB migrations, automated checks, and Windows backend publishing. The former cloud/container deployment assets are a superseded archive. Product functionality includes saved quote configuration, server pricing and discounts, photographer scheduling, private photo processing and access, invitations, albums, immutable print requests, and retention controls.

The [platform completion brief](platform-completion.md) and [current machine-readable evidence](platform-verification.json) record the subsequent full-platform delivery and remaining external qualification gates.

See the [live quote slice brief](live-quote-slice.md) for the subsequent calculator implementation and its separate acceptance evidence. The historical results below describe the earlier platform baseline.

The subsequent [LocalDB persistence change](localdb-persistence.md) implements OD-10: normal development and production use persistent LocalDB; database fakes are test-only. Its verification supersedes the earlier runtime/deployment descriptions without rewriting historical test results.

## Verification recorded on 2026-09-05

| Check | Recorded result |
| --- | --- |
| Backend tests | 44 passed, including three real SQL LocalDB tests |
| Browser matrix | Five scenarios, 45 passing executions across Chromium/Firefox/WebKit and three viewport sizes, run from the root `e2e` project against the built applications |
| Packaged HTTPS smoke | Real administrator login, browser SHA-256 and block upload, worker JPEG preview, captured invitation, assigned gallery, album creation, print submission/review and public publication passed; the catalog moved to its own origin and carries its own checks |
| Backend release build | Passed with no warnings or errors |
| Angular packaging | Four libraries and three applications built with Node 24.18.0 after the workspace changes |
| Architecture checker | Passed; every one of 13 application components is inventoried |
| Design system | Contract check passed; 33 passing browser executions at three viewport widths; `design-system/dist` built and checked against the host navigation configuration |
| Layout change | After moving the projects under `backend/src` and the suite to `e2e/`: Release build clean, 41 of 44 backend acceptance tests passing, the three SQL LocalDB tests unrunnable on that machine, and the whole browser matrix passing |
| Azure Bicep | Template compiled locally; no resources deployed |
| Container execution | Not verified: the local Linux Docker engine returned HTTP 500 |

The [machine-readable results](verification.json) identify backend scenarios. Reproducible commands are in the [root README](../../README.md). Full local TRX, browser JSON and screenshots are under ignored `.artifacts/`, `frontend/test-results/` and `design-system/test-results/`; CI retains fresh artifacts for its own source revision. The local record describes an uncommitted implementation run, not a previously published commit or CI success.

## Implementation decisions

The domain and application projects have inward-only references. Application ports isolate persistence, storage, identity, email, queues, routing and AI. Concrete MediatR commands and handlers implement catalog administration and quotation; cohesive workflow services implement photo, client and retention operations. Controllers perform HTTP binding and authorization. These service names consolidate some of the individual participants in the proposed diagrams.

EF Core maps Identity tables and a versioned `Records` table containing typed aggregate JSON. A `(Kind, Id)` key, optimistic version token and selected unique business keys protect updates; SQL serializable transactions and application locks coordinate cross-record writes. The database tests demonstrate rollback with the outbox, stale-write exclusion and client-scoped print-key uniqueness. This aggregate schema consolidates the design's conceptual tables; large deployments should measure query plans and indexing before capacity qualification. It is not presented as a normalized table per domain entity.

Original files use immutable server-owned keys after manifest/content/hash validation. The worker verifies the original again and produces metadata-free JPEG previews up to 2,400 pixels and thumbnails up to 480 pixels. Processing and transfer states remain separate. Expiry checks run on every client read, independent of scheduler availability. Publication and unreviewed print references prevent deletion. Retried jobs use attempt leases and revision checks; email operation identifiers remain stable across retries.

Angular pages compose 26 inventoried components across the application, domain and presentation libraries. Typed per-feature contracts and named HTTP adapters replace generic transport calls in feature code. All route-owned drafts and state use signals; behavior lives in injectable services. The types-only `@qbs/domain/models` entry point prevents the API/domain component build cycle. Production and acceptance providers are bound only in application composition. Browser acceptance uses controlled service operations and blocks real API requests; the separate packaged LocalDB workflow exercises real cookies, HTTP, storage and worker processing. See the [component inventory](../components.md) for placement, reactive-stream justification and catalog examples.

## Acceptance gaps

Implementation availability is separate from proving every clause of all 82 acceptance criteria. The [acceptance register](../detailed-designs/acceptance.md) links existing tests and marks incomplete criterion evidence as Partial. Passing counts do not imply 82 fully accepted criteria.

The remaining evidence includes the following:

- Studio-supplied valid and corrupt camera RAW files, orientation and encoding coverage, and actual LibRaw results (**G-RAW**).
- The 1,000-file/250 MB boundary dataset, interruption/resume run and measured browser/worker memory and duration on the approved network profile (**G-UPLOAD**).
- Representative photographic annotations, a usefulness threshold, and the approved Azure vision deployment/model/prompt evaluation (**G-AI**).
- Windows host/account/TLS setup, LocalDB backup/restore, monitoring, environment isolation, and external Azure service credentials/roles, verified email sender and resource qualification (**G-ENV**, revised by OD-10).
The completion increment adds configuration/publication workflows, controlled failure recovery, Toronto input controls, resumable uploads, image/AI review, private-resource substitution, simultaneous LocalDB schedule/print writes, album ordering and authoritative print review. The current acceptance register identifies evidence for each criterion; passing controlled tests do not substitute for the four external gates.

MediatR **12.5.0** is pinned under the approved Apache-2.0 selection; **G-MEDIATR** is closed. See the [decision record](../specs/decisions.md#od-09--implementation-and-deployment) for the dated license sources.

Test-first history is partial. Initial controller and browser acceptance runs were captured failing before implementation; the invalid-upload-state and expired-worker-lease regressions also have captured failing runs followed by fixes. Other tests were added during or after implementation. The early four-service fixture contained arithmetic mistakes that were corrected; those failures are not claimed as product defects. There is no fabricated per-criterion red/green or source-history evidence. Requirements L2-049 and L2-052 retain historical evidence gaps for the earlier implementation. The completion brief records genuine new red/green increments, and the current register supplies test names for traceability; it does not manufacture missing historical runs.

## Maintaining the documentation

The detailed designs remain the approved design baseline. Implementation consolidation is described above rather than silently rewriting the design diagrams as an exact generated code map. `diagram-manifest.json` records the existing reviewed source/PNG pairs by normalized source content and image hash, so ordinary Git checkout timestamps do not create false stale-render failures. After changing a diagram, render it with PlantUML, review it, and update both hashes in that manifest. The verifier continues to validate syntax when PlantUML is installed, artifact dimensions, links and requirement traceability.
