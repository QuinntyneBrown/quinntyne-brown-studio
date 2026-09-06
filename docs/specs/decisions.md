# Design decision baseline

Date: 2026-09-05. Authority: approved full-platform design plan and its planning decisions. These rules supersede conflicting prototype defaults. They refine the stable [L1](L1.md) and [L2](L2.md) requirements. Implementation defaults below form part of this baseline; operational evidence remains separate.

## OD-01 — Prices and routes

All amounts use CAD before tax. The estimate states that tax and final consultation adjustments are excluded. Photography, assistants, and studios use hourly rates. Equipment uses a standard rental-unit rate per session. Lunches use a per-person rate. Parking is an entered amount per location. Inventory rental rates are informational; the quote rate set controls equipment-unit pricing.

Durations use quarter-hour increments without minimum rounding beyond that input rule. Assistant cost is count multiplied by session hours and the configured hourly rate. Studio time is entered separately for each selected location. Lunch count is explicit. Rates and quantities are nonnegative; photography duration is positive. Counts are integers. Values outside the representable decimal range are rejected. Required unset rates prevent calculation; an explicit configured zero is valid.

The route starts at one configured studio base, visits geocoded session locations in entered order, and returns to base. Azure Maps calculates driving distance without traffic-dependent pricing or automatic stop reordering. Distance is the sum of route legs in metres divided by 1,000; intermediate distance is not rounded. The user resolves ambiguous addresses before calculation. Missing base configuration, unresolved addresses, and routing failures produce an unavailable estimate. A failed routing request never falls back to zero distance.

The server calculates each line with decimal arithmetic and rounds it to two places using midpoint-away-from-zero. It sums rounded lines, calculates and rounds the selected discount, and subtracts it from the subtotal. Each location contributes parking and studio time once. Every enabled studio is selectable; disabled studios disappear from new quote configuration. Saved rates and studio configuration apply to subsequent calculations.

## OD-02 — Discounts

Codes, advance eligibility, and eligible weekdays each configure a percentage between 0 and 100 inclusive. Rules start disabled with zero percentages; fictional prototype rates are not seed data. The initial advance threshold is 90 days. Thresholds are nonnegative integers. Lead time is the difference between the requested session date and today's date in `America/Toronto`, using a server clock. Past session dates are invalid.

Codes are trimmed and compared using invariant case-insensitive matching. Each code is unique after normalization. Optional validity dates are inclusive Toronto calendar dates evaluated on the calculation date. A disabled, unknown, or expired code grants no code discount and produces a visible code error. Otherwise-valid automatic discounts remain eligible. Slow-day eligibility uses a configurable set of weekdays of the session start date.

Only the largest eligible percentage applies to the full rounded subtotal. Ties use code, advance, then weekday order. Input changes remove eligibility immediately on recalculation. Each result identifies the applied rule, percentage, and amount. Configurations contain a revision so calculations use one consistent snapshot.

## OD-03 — Scheduling

Working windows, unavailable blocks, and session commitments are explicit intervals. Input uses Toronto dates and times on 15-minute increments. The UI requires an offset selection for ambiguous fall-back times and rejects nonexistent spring-forward times. The API carries instants with offsets and validates them against Toronto rules; persistence uses UTC plus the zone identifier.

Intervals are half-open: the end instant is excluded. A session has positive duration. It occupies its requested interval plus configurable buffers, initially 30 minutes on each side. The occupied interval lies within working time and overlaps no unavailable block or other occupied commitment. Adjacent occupied intervals are allowed. Saved schedule mutations serialize checks per photographer in one database transaction to prevent concurrent overlapping commitments.

A request is available when at least one active photographer satisfies these rules. A specific photographer selection limits the evaluation to that photographer. Quote availability is a read and creates no hold or reservation. Session records may remain unassigned; assigning a photographer applies the same conflict policy. The studio base and travel buffers are independent: routing distance does not silently change reserved time.

## OD-04 — Upload, previews, and retention

The supported boundary is 1,000 selected files per batch and 250,000,000 bytes per file. Larger batches are rejected before upload grants are issued. Individual invalid files are reported while valid files proceed. Extensions are `.jpg`, `.jpeg`, `.cr2`, `.cr3`, `.nef`, `.arw`, and `.dng`. The server validates content, not only extension. Supported camera model/encoding fixtures: `<TO SUPPLY>` under gate `G-RAW`.

Uploads use private block blobs, 8 MiB blocks, and at most four active file transfers per browser. The application issues short-lived grants for one generated staging blob name and a batch-owned photo identifier. Grants cannot list or read other blobs. The browser retains batch identifiers, full-file SHA-256 digests, and acknowledged-block progress; resuming after a reload requires reselection of matching local files. Digests are calculated incrementally in a browser worker before transfer. Finalization checks storage length, content, and association, then promotes the inspected revision to a server-owned original key before queueing conversion. Old write grants cannot overwrite that original. The processing worker verifies the original digest before review readiness. Success in transferring bytes is distinct from preview readiness.

The processing worker uses LibRaw conversion for supported RAW data and emits JPEG thumbnails and review previews with corrected orientation. Originals retain their exact uploaded bytes. Derivatives exclude GPS metadata. A hash stored after processing supports integrity evidence; it does not silently merge separate photo records. Preview-generation failures retain the original and offer an explicit retry.

The capacity acceptance profile is a current desktop Chromium build, 16 GB RAM, 100 Mbps upload, and 50 ms round-trip latency. A one-minute network interruption occurs after some completed files. Tests include 1,000 files at the size boundary using valid supported content. The acceptance report records actual duration and browser/worker memory; no unmeasured throughput guarantee is claimed. Fixture dataset and measured report: `<TO SUPPLY>` under `G-UPLOAD`.

A session owns a retention duration initially 12 calendar months. Each accepted photo receives an expiry date from its upload date in Toronto plus that duration, preserving local time and clamping invalid month-end days. Session expiry is the latest photo expiry; additional uploads or an explicit administrator extension can move it later. The first photo establishes expiry. The notice job runs daily and emits one administrator notice per expiry revision 30 days before expiry, or immediately if a new expiry is already inside that window.

At expiry, all client access to the session is denied on every metadata and photo request. Album entries retain order and identifiers but display an unavailable placeholder without private image bytes. Print requests retain submitted text and price snapshots. Public publication is separately controlled and is not changed by client expiry.

Physical deletion requires an administrator confirmation of the current reference-impact report. Published photos and photos in unreviewed print requests block deletion. The administrator first unpublishes or reviews those records. Confirmation is checked again in the deletion transaction. A durable deletion job then removes original and derivative blobs with retry; metadata becomes a tombstone. Album and reviewed print history retain identifiers and unavailable labels. Extending an expired session restores assigned-client access only while the photos remain stored. Deleted originals cannot be restored through extension.

## OD-05 — Technical-quality suggestions

The proposed adapter uses an Azure-hosted vision-enabled language model and JPEG review previews. The model deployment name, version, region, prompt version, and evaluation record are configuration evidence. Selection remains gated by `G-AI`; a model name alone is not evidence of photographic accuracy.

The rubric assesses visible sharpness, exposure, and closed-eye issues where visible. Each result contains the photo identifier, issue findings, advisory recommendation, explanation, and model/prompt version. Findings allow `uncertain` and `not-applicable`. Suggestions never publish photos, change gallery access, or become a final administrator selection. Malformed or mismatched-photo results are rejected. Failures remain visible alongside manual review.

The studio approves a representative set across weddings, events, headshots, and family portraits, including low light, intended motion blur, and groups. Studio annotations, usefulness threshold, and qualified Azure deployment: `<TO SUPPLY>` under `G-AI`. The design is complete without claiming that this evaluation has passed.

## OD-06 — Identity and assignments

ASP.NET Core Identity stores accounts in the application database. Administrators invite clients and assign session galleries. Public self-registration is absent. Invitation acceptance sets credentials using a single-use expiring token. Password recovery uses the same email delivery adapter with account-neutral responses. Initial administrator provisioning is an operator-only deployment command, outside public HTTP registration.

Cookies are HttpOnly, Secure, and host-only. An application gateway serves all product applications and `/api` under one origin; the independent design-system host carries no product identity cookie. Mutations enforce antiforgery checks. Roles are `Administrator` and `Client`; gallery assignments are explicit database records. Resource queries enforce current assignments and expiry before producing metadata or image bytes. Client tokens and identifiers supplied in a body never establish ownership.

## OD-07 — Print requests and albums

Administrators configure print-option names, dimensions, descriptive finish, enabled state, and unit price. No fictional options or prices become initial production data. A request contains one or more photo/option/quantity lines, positive integer quantities, and optional notes. The server derives the client, verifies each photo, and snapshots current option descriptions and prices. The UI confirms a refreshed summary if prices have changed before submission. Delivery addresses, payment, shipping calculation, and automated fulfilment are outside this baseline.

Requests are stored for an administrator inbox with `Submitted` and `Reviewed` states. A retried submission uses a client-scoped idempotency key and the same canonical payload. A different payload using that key is rejected. Client confirmation includes the stable request identifier. Review records administrator and timestamp; it does not mean printing or payment occurred.

The client obtains its current review through `POST /api/client/print-requests/preview`. The server returns authoritative option descriptions, unit prices, revisions, quantities and rounded totals, echoing the input revision without storing a request. Edits invalidate the displayed result immediately; obsolete successes and failures cannot replace a current review. Submission uses precisely the reviewed revisions. An unchanged retry retains its idempotency key; changing notes or selections creates a new key.

Albums contain a required name and an ordered unique selection of accessible photos. Clients can create, view, rename, add, reorder, and remove photos from their own albums. Creation requires at least one photo; removing the last photo later leaves an empty album. Every added photo is authorized at write time, and every displayed photo is authorized at read time. Stale version updates return a conflict without silently overwriting another edit.

## OD-08 — Presentation

The [HTML prototype](../mocks/README.md) is the approved visual reference. Production follows its white space, task hierarchy, and visual language while applying this decision baseline. Prototype contact submission, request-history extras, permissive login, and simulated Azure responses do not independently establish production scope.

The viewport matrix is 390 × 844, 768 × 1024, and 1440 × 900 CSS pixels. The browser matrix is the Chromium, Firefox, and WebKit versions pinned by the production Playwright lockfile. Keyboard checks cover navigation, focus visibility, dialogs, input errors, and status announcements. Formal accessibility conformance and quantitative latency budgets are not claimed by this baseline.

## OD-09 — Implementation and deployment

The design baseline selects .NET 10 LTS, EF Core 10, Angular/CLI 22, Node.js 24.15 or a later compatible 24.x patch, and TypeScript 6.0.x. Exact package patches and toolchain lockfiles are recorded at implementation. The supported combinations are documented in the [Angular compatibility table](https://angular.dev/reference/versions); the runtime lifecycle is documented in the [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy).

MediatR remains mandatory. The dependency-selection review compares current official releases and their license terms against the project's actual eligibility. The [official license conditions](https://mediatr.io/) and [release history](https://github.com/LuckyPennySoftware/MediatR/releases) are the evidence sources. The implementation selects MediatR **12.5.0**, the final pre-v13 release, under the user-approved open-source selection on **2026-09-05**. The [tagged Apache-2.0 license](https://github.com/LuckyPennySoftware/MediatR/blob/v12.5.0/LICENSE) and [12.5.0 package record](https://www.nuget.org/packages/MediatR/12.5.0) establish freely usable terms without commercial/community eligibility assumptions. The project and package lock pin that exact release. G-MEDIATR is closed for this selection; changing the major version requires a new dependency review.

The original Azure SQL and Container Apps hosting selection is superseded by [OD-10](#od-10--localdb-persistence-and-windows-hosting). Blob Storage stores private originals and derivatives. Storage Queues transports processing work. Azure Communication Services Email delivers invitations and recovery messages. The component catalog deploys to Azure Static Web Apps and operates without the API. Development, staging, and production use distinct identities, secrets, databases, storage accounts, and queues. Subscription, region, DNS names, verified sender, capacity configuration, and model deployment identifiers remain environment evidence under `G-ENV`.

## OD-10 — LocalDB persistence and Windows hosting

Status: **Accepted**, 2026-09-06. Authority: the user's LocalDB-only selection and approved implementation plan. Supersedes OD-09's Azure SQL, Container Apps, and Linux backend deployment choices; other decisions remain in effect.

All normal development and production application, Identity, and outbox records use SQL Server Express LocalDB through EF Core's SQL Server provider. The API and worker run on one Windows host under the same owning Windows account. [Microsoft's LocalDB documentation](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) describes its local, per-user instance model. Linux backend containers, remote SQL Server, SQL Server service instances, and Azure SQL are outside this supported target. The prior deployment assets are retained only as a superseded historical archive.

`ConnectionStrings:Studio` is shared by API, worker, EF tooling, migration, and administrator provisioning. Development defaults to `(localdb)\MSSQLLocalDB`, database `QbsDevelopment`. Production requires an explicit connection with a named LocalDB instance, an explicit database, and Windows integrated authentication; the runbook uses `QbsProduction`. Environment databases remain separate. Normal startup verifies access and applied migrations without creating or modifying schema. The explicit `--migrate` command owns schema changes. Unavailable databases and invalid configuration fail startup with an actionable message and never select memory storage.

Controlled external-service adapters remain available in development independently of persistence. Database fakes and EF InMemory live only in the acceptance-test project and are supplied explicitly by test composition, preserving L2-050. Persistence acceptance cases exercise real LocalDB with disposable databases and the normal runtime registrations. Existing schema, decimal pricing, transactions, version conflicts, and outbox semantics remain unchanged.

This decision includes code, Windows publishing, startup tooling, and the [operating runbook](../../deploy/README.md). It does not install Windows services, provision cloud resources, migrate external production data, or establish unmeasured capacity or recovery guarantees. Backups, a restore exercise, Windows account configuration, and external adapter qualification remain operational evidence under `G-ENV`. See the [acceptance evidence](../implementation/localdb-persistence.md).

## OD-11 — Models entry point and feature contracts

Status: **Accepted**, 2026-09-06. Authority: the approved completion plan and the existing component-placement and interface-consumption rules.

`@qbs/domain/models` contains only the flat studio types. The API library imports that secondary entry point; domain components may then consume API tokens without creating a package build cycle. Build order is models, components, API, domain, application. The three product applications remain bootstrap projects.

Feature contracts declare named operations and finished data rather than inheriting generic HTTP transport methods. Route-owned editor contracts and tokens also live in the API library so domain editor regions can consume them; application services own their signal state, behavior, navigation and lifetime. Provider composition in the application library selects HTTP adapters in normal builds and explicit controlled implementations in acceptance builds. Acceptance fixtures block accidental product HTTP calls. A separate packaged integration suite deliberately exercises production adapters against an isolated LocalDB database.

This decision refines packaging and composition; it does not change OD-10's LocalDB runtime or introduce another database provider. The [completion report](../implementation/platform-completion.md) records implementation and verification.

## Evidence register

| Gate | Acceptance prerequisite | Status |
| --- | --- | --- |
| G-RAW | Studio camera models, valid/corrupt samples, conversion and orientation results | `<TO SUPPLY>` |
| G-UPLOAD | Boundary files and measured interruption/resume report on OD-04 profile | `<TO SUPPLY>` |
| G-AI | Studio-approved rubric, evaluation set, threshold, Azure model/version/region qualification | `<TO SUPPLY>` |
| G-MEDIATR | User-selected latest pre-v13 release and dated official licensing evidence | Closed: MediatR 12.5.0, Apache-2.0, 2026-09-05 |
| G-ENV | Windows account/host/TLS, LocalDB backup/restore, external service credentials, environment identifiers, email sender and isolation evidence (OD-10) | `<TO SUPPLY>` |

These gates prevent acceptance claims for unverified product behavior. They do not prevent the requirements and proposed designs from being reviewed.

The [qualification commands](../implementation/qualification.md) now emit measured, failed or blocked evidence reports. Command success alone never changes this register.
