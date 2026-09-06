# Shared production architecture

Status: approved production design baseline, 2026-09-05, implemented in this repository. The types, routes, and ports below define the intended shared interfaces; the [implementation report](../implementation/README.md) records where delivered names consolidate them and which evidence remains open. The [decision baseline](../specs/decisions.md) defines shared policy.

## Application boundaries

The Angular workspace contains `components`, `domain`, and `api` libraries and `marketing`, `admin`, and `client` applications. The design-system catalog is a separate first-class product at the repository root with its own entry point, build, and deployment. A separate `application` library owns feature state and orchestration. Domain types contain no HTTP dependencies. Components receive data and emit intent. Feature state uses writable and computed signals. HTTP observable conversion stays inside API implementations; no consumer imports an implementation class.

Each contract has an Angular `InjectionToken`; application composition binds it to a production HTTP adapter or a controlled mock. This follows the [interface-driven service consumption reference](https://github.com/QuinntyneBrown/interface-driven-service-consumption). A token and its interface use separate files. A feature contract is `I<Entity>Service` in the singular with the token `<ENTITY>_SERVICE`; the shared transport contract is `IStudioClient` behind `STUDIO_CLIENT`, implemented by `StudioClient` in production and `MockStudioClient` under acceptance tests. Component TypeScript, HTML, and CSS remain separate. BEM names belong to the components library and are reused by application screens.

The .NET solution contains `Qbs.Domain`, `Qbs.Application`, `Qbs.Infrastructure`, `Qbs.Api`, and `Qbs.Worker`. Domain depends on no outer project. Application references Domain and declares ports. Infrastructure implements ports using EF Core, Azure SDKs, Identity, and RAW conversion. API and Worker compose dependencies. All product HTTP operations, including authentication and image delivery, use controllers. Handlers use MediatR selected under `G-MEDIATR`.

The gateway serves built Angular product applications at `/`, `/admin`, and `/client`, forwarding `/api` to the API. Route fallback respects application boundaries. It serves static marketing shells; initial content is retrieved through public APIs. The design-system catalog builds and deploys from its own root-level directory using its own package, checks, and static host. It renders local fixtures rather than product providers, so no studio database or API is needed to browse its examples.

## Contracts and persistence

HTTP routes use `/api/public`, `/api/admin`, `/api/client`, and `/api/auth`. JSON uses camelCase names. GUID identifiers are opaque strings. Money is a decimal string with currency `CAD`; browser arithmetic does not determine charge totals. Dates use ISO calendar dates; schedule instants use ISO timestamps with offsets. Public query projections omit unpublished fields, private storage keys, email addresses, and client assignments.

Create operations return `201` with a resource identifier; reads and edits return `200`; durable asynchronous work returns `202` with a status resource. Field errors use `400` ProblemDetails with an `errors` map. Unauthenticated API requests use `401`; role failures use `403`; inaccessible client resources use `404`. Version, idempotency, or state conflicts use `409`. Dependency outages use `503`. No failure response includes a fabricated result.

All mutable aggregates carry a version. Updates carry `expectedVersion`; handlers reject stale versions. Aggregate changes commit in one transaction. Background work is recorded in a SQL outbox in the same transaction as its state change. The worker relays outbox entries to Storage Queues. Duplicate queue delivery is normal: jobs check a stable operation identifier and state revision before committing effects. Bounded retries record failure and leave an administrator-visible retry path.

`IStudioStore` is the application persistence port. It exposes aggregate reads, scoped queries, and transactional saves with expected versions. EF Core implements it through `StudioDbContext` and SQL Server Express LocalDB for all normal development and production runs, including Identity and the outbox. API and worker share one Windows host, owning Windows account, and explicit environment database under [OD-10](../specs/decisions.md#od-10--localdb-persistence-and-windows-hosting). `ConnectionStrings:Studio` selects the database; production requires an explicit LocalDB connection and development defaults to `QbsDevelopment`. Database accessibility and migration readiness are checked before startup; only the explicit migration command changes schema. Controlled external adapters never select a database fake. Acceptance-test composition supplies isolated database fakes from the test project; separate LocalDB cases verify runtime persistence, restarts, identity, transactions, and constraints.

| Shared type or port | Minimum contract |
| --- | --- |
| `Money` | `decimal Amount`, `string Currency`; currency-fixed rounded arithmetic |
| `Session` | `Guid Id`, `string Name`, `ServiceKind Service`, `DateTimeOffset StartsAt`, `DateTimeOffset EndsAt`, `Guid? PhotographerId`, `long Version` |
| `SessionPhoto` | `Guid Id`, `Guid SessionId`, `string OriginalKey`, `string? PreviewKey`, `PhotoState State`, `DateTimeOffset UploadedAt` |
| `GalleryAssignment` | `Guid SessionId`, `Guid ClientId`; unique pair |
| Authenticated caller | controllers read the identifier and roles from the request principal and pass them into application operations; no separate port |
| `IIdentityAccounts` | sign-in, sign-out, invitation, recovery, and client-account queries over the identity provider |
| `IJobQueue` | send, receive, and complete durable processing messages by job identifier |
| `IClock` | `UtcNow`; Toronto date conversion is explicit policy |
| `IRouteDistanceService` | ordered coordinate route → total metres and route legs, or typed failure |
| `IPhotoStorage` | issue scoped upload grant, inspect/finalize original, stream authorized derivative, delete keys |
| `IRawPreviewConverter` | original stream and format → oriented JPEG derivatives or conversion failure |
| `IPhotoAnalysisService` | photo ID, preview, rubric version → structured advisory findings or failure |
| `IEmailSender` | recipient, template, token link, deduplication ID → accepted delivery job |

Concrete feature requests and handlers appear in each feature's class and sequence diagrams. Diagram screen names identify tasks and diagram handler names identify operations; the [screen ownership](contracts.md#screen-ownership) and [behavior ownership](contracts.md#behavior-ownership) tables name the delivered component, route, and implementation for each one. The [shared interface and state catalog](contracts.md) fixes route ownership, quote fields, state transitions, and uniqueness constraints. Names repeated across diagrams denote the same contract. A diagram scoped to a slice shows only the relevant projection of a shared aggregate.

## Authorization and media

`Administrator` guards administrative pages and controller operations. `Client` requests derive ownership from `ICurrentUser`. Gallery, photo, album, and print queries enforce assignment and retention policies in the handler, including direct API access. UI guards provide navigation behavior and are not the authorization boundary.

Private client image bytes stream through `PhotosController` after current access checks. Responses use `Cache-Control: private, no-store`; the browser does not receive a reusable storage-read grant. Revocation affects subsequent requests, including direct byte requests. Already received bytes cannot be recalled. Upload grants are limited to administrator-authorized blob writes.

Public photos also use controlled projection routes. `PhotosController` verifies current publication before streaming a public derivative without private metadata. Originals are never served publicly. Publication and private gallery assignment are separate states; AI and upload completion change neither. Physical deletion uses a reference check and a durable cleanup job.

## Operations and rollout

The API and worker access LocalDB with the same owning Windows account and use environment-specific credentials for external blob, queue, and secret resources. Azure managed identity is used only on a host configured to provide it. Production credentials and photo data do not appear in fixtures or catalog examples. Structured logs record correlation ID, operation, duration, and result without tokens, photo bytes, or precise location content.

`GET /api/health` reports process liveness for the platform host and carries no studio data. `GET /api/admin/development-mail` returns locally captured account links under controlled development only and is absent in production. Azure Monitor/Application Insights tracks API errors, route-service failures, queue age, conversion failures, AI failures, and retention/deletion job outcomes. Deployment alerts initially trigger on any failed scheduled retention run or dead-lettered processing item; tuned latency and capacity thresholds are evidence under `G-ENV`.

Delivery order is identity and shared components, administration/configuration, public quoting, session ingestion/review, client galleries, then prints/albums and retention deletion. Every increment follows acceptance tests before behavior implementation. Staging uses synthetic data and faked external outcomes before scoped Azure integration checks. Production release requires the affected evidence gates to pass. Windows publishing and the LocalDB operating procedure replace the archived container deployment. Rollback restores the previous application publish directory against a compatible database; migrations are additive until the prior release no longer needs the old shape. Photo deletion has no application rollback.

## Verified references

Sources checked 2026-09-05: [Azure Maps routing API](https://learn.microsoft.com/en-us/rest/api/maps/route/post-route-directions?view=rest-maps-2025-01-01), [Azure background processing](https://learn.microsoft.com/en-us/azure/architecture/best-practices/background-jobs), [Identity account confirmation and recovery](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-10.0), [LibRaw supported cameras](https://www.libraw.org/supported-cameras), and [Azure vision-enabled models](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/gpt-with-vision). These establish adapter feasibility; they do not establish studio camera compatibility or AI usefulness.
