# Shared production architecture

Status: proposed production design, 2026-09-05. Existing implementation consists of standalone HTML mocks. The types and routes below are proposed interfaces, not existing application code. The [decision baseline](../specs/decisions.md) defines shared policy.

## Application boundaries

The Angular workspace contains `components`, `domain`, and `api` libraries and `marketing`, `admin`, and `client` applications. The design-system catalog is a separate first-class product at the repository root with its own entry point, build, and deployment. A separate `application` library owns feature state and orchestration. Domain types contain no HTTP dependencies. Components receive data and emit intent. Feature state uses writable and computed signals. HTTP observable conversion stays inside API implementations; no consumer imports an implementation class.

Each contract has an Angular `InjectionToken`; application composition binds it to a production HTTP adapter or a controlled mock. This follows the [interface-driven service consumption reference](https://github.com/QuinntyneBrown/interface-driven-service-consumption). A token and its interface use separate files. Component TypeScript, HTML, and CSS remain separate. BEM names belong to the components library and are reused by application screens.

The .NET solution contains `Qbs.Domain`, `Qbs.Application`, `Qbs.Infrastructure`, `Qbs.Api`, and `Qbs.Worker`. Domain depends on no outer project. Application references Domain and declares ports. Infrastructure implements ports using EF Core, Azure SDKs, Identity, and RAW conversion. API and Worker compose dependencies. All product HTTP operations, including authentication and image delivery, use controllers. Handlers use MediatR selected under `G-MEDIATR`.

The gateway serves built Angular product applications at `/`, `/admin`, and `/client`, forwarding `/api` to the API. Route fallback respects application boundaries. It serves static marketing shells; initial content is retrieved through public APIs. The design-system catalog builds and deploys from its own root-level directory and is independently hosted with mocks. No product database or API is needed to browse its examples.

## Contracts and persistence

HTTP routes use `/api/public`, `/api/admin`, `/api/client`, and `/api/auth`. JSON uses camelCase names. GUID identifiers are opaque strings. Money is a decimal string with currency `CAD`; browser arithmetic does not determine charge totals. Dates use ISO calendar dates; schedule instants use ISO timestamps with offsets. Public query projections omit unpublished fields, private storage keys, email addresses, and client assignments.

Create operations return `201` with a resource identifier; reads and edits return `200`; durable asynchronous work returns `202` with a status resource. Field errors use `400` ProblemDetails with an `errors` map. Unauthenticated API requests use `401`; role failures use `403`; inaccessible client resources use `404`. Version, idempotency, or state conflicts use `409`. Dependency outages use `503`. No failure response includes a fabricated result.

All mutable aggregates carry a version. Updates carry `expectedVersion`; handlers reject stale versions. Aggregate changes commit in one transaction. Background work is recorded in a SQL outbox in the same transaction as its state change. The worker relays outbox entries to Storage Queues. Duplicate queue delivery is normal: jobs check a stable operation identifier and state revision before committing effects. Bounded retries record failure and leave an administrator-visible retry path.

`IStudioStore` is the application persistence port. It exposes aggregate reads, scoped queries, and transactional saves with expected versions. EF Core implements it through `StudioDbContext`. Acceptance tests substitute an isolated controlled store fake. The fake exercises controller/handler behavior; separate later SQL tests verify transaction isolation, mappings, and constraints.

| Shared type or port | Minimum contract |
| --- | --- |
| `Money` | `decimal Amount`, `string Currency`; currency-fixed rounded arithmetic |
| `Session` | `Guid Id`, `string Name`, `ServiceKind Service`, `DateTimeOffset StartsAt`, `DateTimeOffset EndsAt`, `Guid? PhotographerId`, `long Version` |
| `SessionPhoto` | `Guid Id`, `Guid SessionId`, `string OriginalKey`, `string? PreviewKey`, `PhotoState State`, `DateTimeOffset UploadedAt` |
| `GalleryAssignment` | `Guid SessionId`, `Guid ClientId`; unique pair |
| `ICurrentUser` | authenticated identifier and roles from server authentication |
| `IClock` | `UtcNow`; Toronto date conversion is explicit policy |
| `IRouteDistanceService` | ordered coordinate route → total metres and route legs, or typed failure |
| `IPhotoStorage` | issue scoped upload grant, inspect/finalize original, stream authorized derivative, delete keys |
| `IRawPreviewConverter` | original stream and format → oriented JPEG derivatives or conversion failure |
| `IPhotoAnalysisService` | photo ID, preview, rubric version → structured advisory findings or failure |
| `IEmailSender` | recipient, template, token link, deduplication ID → accepted delivery job |

Concrete feature requests and handlers appear in each feature's class and sequence diagrams. The [shared interface and state catalog](contracts.md) fixes route ownership, quote fields, state transitions, and uniqueness constraints. Names repeated across diagrams denote the same contract. A diagram scoped to a slice shows only the relevant projection of a shared aggregate.

## Authorization and media

`Administrator` guards administrative pages and controller operations. `Client` requests derive ownership from `ICurrentUser`. Gallery, photo, album, and print queries enforce assignment and retention policies in the handler, including direct API access. UI guards provide navigation behavior and are not the authorization boundary.

Private client image bytes stream through `PhotosController` after current access checks. Responses use `Cache-Control: private, no-store`; the browser does not receive a reusable storage-read grant. Revocation affects subsequent requests, including direct byte requests. Already received bytes cannot be recalled. Upload grants are limited to administrator-authorized blob writes.

Public photos also use controlled projection routes. `PublicGalleriesController` verifies current publication before streaming a public derivative without private metadata. Originals are never served publicly. Publication and private gallery assignment are separate states; AI and upload completion change neither. Physical deletion uses a reference check and a durable cleanup job.

## Operations and rollout

Environment-specific managed identities grant the API and worker only access to their own SQL, blob, queue, and secret resources. Production credentials and photo data do not appear in fixtures or catalog examples. Structured logs record correlation ID, operation, duration, and result without tokens, photo bytes, or precise location content.

Azure Monitor/Application Insights tracks API errors, route-service failures, queue age, conversion failures, AI failures, and retention/deletion job outcomes. Deployment alerts initially trigger on any failed scheduled retention run or dead-lettered processing item; tuned latency and capacity thresholds are evidence under `G-ENV`.

Delivery order is identity and shared components, administration/configuration, public quoting, session ingestion/review, client galleries, then prints/albums and retention deletion. Every increment follows acceptance tests before behavior implementation. Staging uses synthetic data and faked external outcomes before scoped Azure integration checks. Production release requires the affected evidence gates to pass. Rollback redeploys the previous application image; database migrations are additive until the prior release no longer needs the old shape. Photo deletion has no application rollback.

## Verified references

Sources checked 2026-09-05: [Azure Maps routing API](https://learn.microsoft.com/en-us/rest/api/maps/route/post-route-directions?view=rest-maps-2025-01-01), [Azure background processing](https://learn.microsoft.com/en-us/azure/architecture/best-practices/background-jobs), [Identity account confirmation and recovery](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-10.0), [LibRaw supported cameras](https://www.libraw.org/supported-cameras), and [Azure vision-enabled models](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/gpt-with-vision). These establish adapter feasibility; they do not establish studio camera compatibility or AI usefulness.
