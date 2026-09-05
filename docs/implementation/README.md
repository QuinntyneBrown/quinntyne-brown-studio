# Implementation and acceptance evidence

The repository now contains the public website, administration and client applications, independent component catalog, controller API, background worker, SQL migration, automated checks, and Azure packaging assets. Product functionality includes saved quote configuration, server pricing and discounts, photographer scheduling, private photo processing and access, invitations, albums, immutable print requests, and retention controls.

## Verification recorded on 2026-09-05

| Check | Recorded result |
| --- | --- |
| Backend tests | 44 passed, including three real SQL LocalDB tests |
| Browser matrix | Eight scenarios, 72 passing executions across Chromium/Firefox/WebKit and three viewport sizes |
| Packaged HTTPS smoke | Real administrator login, browser SHA-256 and block upload, worker JPEG preview, captured invitation, assigned gallery, album creation, print submission/review, public publication and standalone catalog passed |
| Backend release build | Passed with no warnings or errors |
| Angular packaging | Four applications and four libraries built |
| Architecture checker | Passed; every one of 14 components is inventoried |
| Azure Bicep | Template compiled locally; no resources deployed |
| Container execution | Not verified: the local Linux Docker engine returned HTTP 500 |

The [machine-readable results](verification.json) identify backend scenarios. Reproducible commands are in the [root README](../../README.md). Full local TRX, browser JSON and screenshots are under ignored `.artifacts/` and `frontend/test-results/`; CI retains fresh artifacts for its own source revision. The local record describes an uncommitted implementation run, not a previously published commit or CI success.

## Implementation decisions

The domain and application projects have inward-only references. Application ports isolate persistence, storage, identity, email, queues, routing and AI. Concrete MediatR commands and handlers implement catalog administration and quotation; cohesive workflow services implement photo, client and retention operations. Controllers perform HTTP binding and authorization. These service names consolidate some of the individual participants in the proposed diagrams.

EF Core maps Identity tables and a versioned `Records` table containing typed aggregate JSON. A `(Kind, Id)` key, optimistic version token and selected unique business keys protect updates; SQL serializable transactions and application locks coordinate cross-record writes. The database tests demonstrate rollback with the outbox, stale-write exclusion and client-scoped print-key uniqueness. This aggregate schema consolidates the design's conceptual tables; large deployments should measure query plans and indexing before capacity qualification. It is not presented as a normalized table per domain entity.

Original files use immutable server-owned keys after manifest/content/hash validation. The worker verifies the original again and produces metadata-free JPEG previews up to 2,400 pixels and thumbnails up to 480 pixels. Processing and transfer states remain separate. Expiry checks run on every client read, independent of scheduler availability. Publication and unreviewed print references prevent deletion. Retried jobs use attempt leases and revision checks; email operation identifiers remain stable across retries.

Angular components use separate TypeScript, HTML and CSS files, signals for state, and injected API contracts. The four apps share four buildable libraries. The static catalog imports actual product components with controlled service providers; the manifest supplies its displayed component contracts. Responsive screenshots were visually reviewed at desktop and mobile sizes. The unconfigured home page intentionally has a branded empty image area until actual work is published.

## Acceptance gaps

Implementation availability is separate from proving every clause of all 82 acceptance criteria. The [acceptance register](../detailed-designs/acceptance.md) links existing tests and marks incomplete criterion evidence as Partial. Passing counts do not imply 82 fully accepted criteria.

The remaining evidence includes the following:

- Studio-supplied valid and corrupt camera RAW files, orientation and encoding coverage, and actual LibRaw results (**G-RAW**).
- The 1,000-file/250 MB boundary dataset, interruption/resume run and measured browser/worker memory and duration on the approved network profile (**G-UPLOAD**).
- Representative photographic annotations, a usefulness threshold, and the approved Azure vision deployment/model/prompt evaluation (**G-AI**).
- Azure subscription/region/DNS, verified email sender, service-role grants, managed-identity calls, environment isolation, container execution, monitoring and backup/restore verification (**G-ENV**).
- Full browser workflows and adverse-state coverage for every remaining requirement, including all pricing editors, private-resource substitution at each route, concurrent schedule/reference races and all catalog states. Existing representative checks and source implementations are linked without claiming those broader scenarios passed.

MediatR **12.5.0** is pinned under the approved Apache-2.0 selection; **G-MEDIATR** is closed. See the [decision record](../specs/decisions.md#od-09--implementation-and-deployment) for the dated license sources.

Test-first history is partial. Initial controller and browser acceptance runs were captured failing before implementation; the invalid-upload-state and expired-worker-lease regressions also have captured failing runs followed by fixes. Other tests were added during or after implementation. The early four-service fixture contained arithmetic mistakes that were corrected; those failures are not claimed as product defects. There is no fabricated per-criterion red/green or source-history evidence. Requirements L2-049, L2-051, L2-052 and L2-054 therefore retain open evidence.

## Maintaining the documentation

The detailed designs remain the approved design baseline. Implementation consolidation is described above rather than silently rewriting the design diagrams as an exact generated code map. `diagram-manifest.json` records the existing reviewed source/PNG pairs by normalized source content and image hash, so ordinary Git checkout timestamps do not create false stale-render failures. After changing a diagram, render it with PlantUML, review it, and update both hashes in that manifest. The verifier continues to validate syntax when PlantUML is installed, artifact dimensions, links and requirement traceability.
