# Shared interface and state catalog

These contracts are the approved baseline for the delivered routes. The feature pages define the request fields and outcomes for their routes. [Architecture](architecture.md) supplies wire conventions, authorization, and transactions; [decisions](../specs/decisions.md) supplies policy values.

## Route ownership

Each route has one controller owner even when more than one feature describes its behavior. Supporting policies are invoked inside that route's handler; a policy description does not register another endpoint.

| Route family | Controller | Shared behavior |
| --- | --- | --- |
| `/api/public/quotes/calculate`, `/api/public/locations/resolve`, `/api/public/studios` | `QuotesController` | `CalculateQuoteHandler` invokes `QuoteCalculation`, `DiscountPolicy`, and `AvailabilityPolicy`; `ResolveQuoteLocationHandler` resolves addresses; `GetQuoteStudiosHandler` returns enabled studio options. |
| `/api/admin/discounts` | `DiscountConfigurationController` | Rule administration; public eligibility remains part of quote calculation. |
| `/api/admin/rates` | `RateConfigurationController` | Rate configuration and revision. |
| `/api/admin/studios` | `StudioController` | Private studio configuration. |
| `/api/admin/photographers`, `/api/admin/photographers/{id}/schedule` | `SchedulesController` | Photographer records and schedule intervals. |
| `/api/public/availability` | `AvailabilityController` | Shares the quote handler's `AvailabilityPolicy`. |
| `/api/admin/sessions`, `/api/admin/sessions/{id}`, upload routes and retry-preview | `UploadsController` | Session association, grants, finalization, and preview job requests. |
| `/api/admin/sessions/{id}/photos`, `/api/admin/photos/{id}/preview`, `/api/client/photos/{id}/preview` | `PhotosController` | Role-appropriate metadata and byte delivery; client authorization uses the gallery-access service. |
| `/api/admin/sessions/{id}/clients`, `/api/client/galleries` and individual gallery routes | `ClientGalleriesController` | Assignment administration and scoped client projections. |
| `/api/public/promotions`, `/api/public/print-options`, `/api/public/galleries`, `/api/public/content/{key}`, and the administrative content routes | `PresentationController` | One owner for every anonymous projection; each projection omits unpublished fields, storage keys, and client assignments. |
| `/api/public/galleries/{slug}/photos/{id}` | `PhotosController` | Public derivative bytes after a current publication check. |
| Administrative gallery, promotion, equipment, vendor, analysis, retention, print-option, print-request, album, and authentication routes | Corresponding controller named in the feature page | Each controller retains its declared feature route family. |

`IQuoteService`, `IDiscountService`, `IUploadService`, `IPhotoService`, and `IClientGalleryService` are separate consumer contracts. `IClientGalleryService` delegates private image retrieval to `IPhotoService`. The `PhotosController` route still has a single owner. Every consumer contract has a separately declared injection token and replaceable HTTP/mock implementations.

## Screen ownership

Feature diagrams name the screen a person uses. The delivered Angular applications consolidate those screens into fewer components driven by route data, so each designed screen maps to a delivered component and route below. A screen name in a diagram identifies the task, not a separate class.

| Designed screen | Delivered component | Route |
| --- | --- | --- |
| `MarketingPage` | `PublicPage` | `/`, `/portfolio`, `/services`, `/contact` |
| `PublicGalleryPage` | `PublicPage` | `/galleries/{slug}` |
| `PromotionsPage` | `PublicPage` | `/promotions` |
| `PublicPrintPricesPage` | `PublicPage` | `/prints` |
| `QuoteCalculatorPage`, `SessionTimingFields` | `QuotePage` | `/quote` |
| `AccountAccessPage` | `LoginPage` | `/login`, `/forgot-password`, `/reset-password`, `/accept-invitation` |
| `EquipmentPage`, `PreferredVendorsPage`, `SessionEditorPage`, `PromotionEditor`, `PublicGalleryEditor`, `PrintPricesPage` | `CatalogPage` | `/equipment`, `/vendors`, `/sessions`, `/promotions`, `/public-galleries`, `/print-options` |
| `QuoteRatesPage`, `DiscountRulesPage`, `StudiosPage`, `MarketingContentEditor`, `AdminInvitationsPage` | `SettingsPage` | `/rates`, `/discounts`, `/studios`, `/content`, `/invitations` |
| `PhotographerSchedulePage` | `SettingsPage` | `/schedule/{photographerId}` |
| `SessionUploadPage`, `SessionPhotoReviewPage`, `PhotoSuggestionsPanel`, `SessionRetentionPanel`, `SessionAccessEditor` | `SessionPage` | `/sessions/{id}` |
| `ClientGalleriesPage`, `AlbumEditorPage`, `ClientPrintSelectionPage`, `PrintRequestPage` | `ClientPage` | `/galleries`, `/galleries/{id}`, `/albums`, `/albums/{id}`, `/prints` |
| `PrintRequestInboxPage` | `PrintInbox` | `/print-requests` |
| `CatalogApplication` | Standalone catalog | The [design-system product](../../design-system/README.md) |

Every screen reaches its services through injection tokens, so a test binds a controlled implementation without touching a component.

## Behavior ownership

Feature diagrams name one handler per behavior. The delivered application layer keeps MediatR commands for catalog administration and quotation, and groups the remaining behaviors into cohesive services, so several designed handlers share one implementation. A behavior name identifies the operation, not a separate class.

| Designed handlers | Delivered implementation |
| --- | --- |
| `SaveEquipmentHandler`, `SavePreferredVendorHandler`, `SavePhotographerHandler`, `SaveSessionHandler`, `SavePromotionHandler`, `SavePublicGalleryHandler`, `SavePrintOptionHandler`, `SaveStudioHandler`, `SaveRateConfigurationHandler`, `SaveDiscountConfigurationHandler` | MediatR `Save*` commands and handlers in `QuinntyneBrownStudio.Application/Catalog` |
| `GetEquipmentHandler`, `GetPreferredVendorsHandler`, `GetRateConfigurationHandler`, and the other administrative reads | `AdminCatalog.List`, `AdminCatalog.Get` |
| `GetPublishedContentHandler`, `SaveMarketingContentHandler`, `GetPromotionsHandler`, `GetPublicGalleryHandler`, `GetStudioOptionsHandler`, `GetPrintOptionsHandler` | `Presentation.Public`, `Presentation.Save` |
| `CalculateQuoteHandler`, `ResolveQuoteLocationHandler` | `CalculateQuoteHandler` in `QuinntyneBrownStudio.Application/Quotations`, with addresses and distances through `IRouteDistanceService` |
| `GetPhotographerScheduleHandler`, `SavePhotographerScheduleHandler`, `GetAvailabilityHandler` | `Scheduling.Save`, `Scheduling.Check` |
| `CreateUploadBatchHandler`, `ResumeUploadFileHandler`, `FinalizeUploadFileHandler`, `GetOperationStatusHandler`, `CreatePhotoPreviewHandler`, `GetSessionPhotosHandler`, `GetPhotoPreviewHandler`, `GetClientPhotoHandler` | `PhotoWorkflows`, with authorization in `PhotoAccess` |
| `RequestPhotoAnalysisHandler`, `AnalyzePhotoHandler` | `AnalysisWorkflows` |
| `GetClientGalleriesHandler`, `SetGalleryAssignmentsHandler`, `GetAlbumHandler`, `SaveAlbumHandler`, `SubmitPrintRequestHandler`, `ReviewPrintRequestHandler`, `GetPrintRequestInboxHandler` | `ClientWorkflows` |
| `ExtendSessionRetentionHandler`, `ProcessRetentionHandler`, `DeleteSessionPhotosHandler`, `ConfirmPhotoDeletionHandler` | `RetentionWorkflows`, with scheduled runs in `QuinntyneBrownStudio.Worker` |
| `AuthenticateAccountHandler`, `SignOutHandler`, `InviteClientHandler`, `AcceptInvitationHandler`, `RecoverAccountHandler`, `ResetAccountPasswordHandler` | `AuthController` over ASP.NET Identity through `IIdentityAccounts` |

Consolidation does not change the described behavior, its authorization, or its failure outcomes. Each row keeps the transaction and version rules stated in [architecture](architecture.md).

## Quote contract

| Type | Fields |
| --- | --- |
| `QuoteInput` | `ServiceKind service`, `DateTimeOffset startsAt`, `DateTimeOffset endsAt`, `QuoteLocation[] locations`, `int assistantCount`, `int equipmentUnits`, `int lunchCount`, `string? code`, `Guid? photographerId`, `long inputRevision` |
| `QuoteLocation` | `ResolvedLocation location`, `decimal parkingAmount`, `Guid? studioId`, `decimal studioHours` |
| `ResolvedLocation` | `string label`, `decimal latitude`, `decimal longitude`; selected from a resolved address candidate |
| `QuoteLine` | `string kind`, `int? locationIndex`, `decimal quantity`, `Money unitPrice`, `Money amount` |
| `AppliedDiscount` | `Guid? ruleId`, `DiscountKind? kind`, `decimal percentage`, `Money amount`, `string? codeError`; no discount uses zero percentage/amount |
| `QuoteResult` | Echoed `inputRevision`, `configurationRevision`, `lines`, `subtotal`, `discount`, `total`, and `AvailabilityResult availability` |
| `AvailabilityResult` | `startsAt`, `endsAt`, `bool available`, `Guid[] photographerIds`, `string? reasonCode`; no client names or commitment descriptions |

Money values are decimal strings on the wire. Integer counts and revisions are integers within the safe JavaScript integer range. The server derives duration from the validated interval. It derives travel from provider results and all price values from saved configuration. Submitted totals or discounts are never trusted inputs.

Quote input revisions are client correlation values, not database versions. Rate/discount/base snapshots use one configuration revision advanced transactionally by configuration edits. A calculation reads those configuration records consistently. Schedule evaluation uses a separate current schedule snapshot and remains indicative; it does not claim a combined reservation transaction.

## Domain states

| Type | Values and transitions |
| --- | --- |
| `ServiceKind` | `Wedding`, `Event`, `Headshot`, `FamilyPortrait` |
| `BillingUnit` | `Hour`, `Kilometre`, `UnitPerSession`, `Person`, `FixedAmount` |
| `DiscountKind` | `Code`, `Advance`, `Weekday`; tie order follows this order |
| `WindowKind` | `Working`, `Unavailable`, `Commitment` |
| `VendorRole` | `MakeupArtist`, `SecondShooter`, `Assistant`; a vendor holds a set |
| `PhotoState` | `Uploading` → `Processing` → `Ready`; invalid content → `Rejected`; transfer/conversion error → `Failed`; explicit retry resumes the failed stage; deletion → `DeletionPending` → `Deleted` |
| `BatchState` | `Uploading`, `Processing`, `Complete`, `PartialFailure`, `Failed`; derived from file outcomes, with Complete only when every accepted file is Ready and no manifest rejection remains |
| `AnalysisState` | `Queued` → `Running` → `Succeeded` or `Failed`; explicit retry creates an attempt with a stable job ID |
| `FindingOutcome` | `Promising`, `Issue`, `Uncertain`, `NotApplicable`; advisory only |
| `RetentionState` | `Active`, `Expired`, `DeletionPending`, `Deleted`; expiry follows the clock, extension restores Active only before deletion, and cleanup completes Deleted |
| `AccountRole` | `Administrator`, `Client`; a role never implies access to a specific client gallery without assignment |
| `PrintRequestState` | `Submitted` → `Reviewed`; review stores administrator and time without changing submitted lines |
| `CoverageStatus` | `NotImplemented`, `Partial`, `Complete`; displayed as Not implemented, Partial, Complete in evidence tables |

`ServiceKind`, `VendorRole`, `PhotoState`, `AnalysisState`, and `FindingOutcome` are declared enumerations in `QuinntyneBrownStudio.Domain`. The other rows fix a value vocabulary rather than a declared type: `DiscountKind`, `RetentionState`, `PrintRequestState`, `BatchState`, and `AccountRole` travel and store as those exact strings; `WindowKind` is carried by the working, unavailable, and commitment interval sets; `BillingUnit` names the unit a configured rate is charged in; and `CoverageStatus` belongs to the acceptance register rather than to any API response.

Stored aggregate `Version` fields are optimistic concurrency tokens. `expectedVersion` is required for updates. `Revision` identifies configuration, impact, or evidence versions where named explicitly; it is not interchangeable with an unrelated aggregate version.

## Storage and job contracts

`IStudioStore` exposes scoped queries and aggregate operations rather than an unrestricted database connection. Queries include gallery membership and retention predicates when handling client data. `Run(lockKey, action)` scopes one unit of work: the action receives an `IStudioTransaction` for reads and version-checked saves, and the aggregate changes commit with their outbox records together when it returns. A failed action rolls the whole scope back. Query-only domain results such as `QuoteCalculation` and `AvailabilityResult` are not stored aggregates.

`UploadBatch` owns its manifest entries; each accepted `UploadFile` refers to exactly one `SessionPhoto`. Rejected manifest entries may have no persisted photo. `Session` associates batches and photos without embedding binary data. A preview worker loads the photo identified by its job rather than repeating the entire batch. A photo remains associated with its original session throughout retries.

`JobEnvelope` contains `jobId`, `kind`, `resourceId`, `expectedRevision`, and `attempt`. The delivered kinds are `Preview`, `Analysis`, `Delete`, and `Email`; a retention notice is an `Email` job carrying the expiry revision that produced it. Queue messages contain identifiers, not image bytes or identity tokens. The SQL job record owns state and retry history. Outbox relay tolerates repeat delivery. Handlers acknowledge only after durable outcome recording; transient failures retry with bounded backoff. After five attempts, the job records Failed and an administrator-visible retry action is required. Retention notice retries use the same expiry revision to avoid duplicate notices. `GetOperationStatusHandler` supplies the authorized job-state projection through each feature's status route; it does not rerun background work.

`PhotoAccess` checks authenticated identity, assignment, retention, photo readiness, and deletion state. Publication checks instead use explicit public-gallery membership. New publication and print-request writes acquire the same session reference lock used by deletion confirmation. The lock and final reference check occur inside the SQL transaction, preventing a new protected reference from appearing after deletion has been authorized.

`GalleryAssignment` uniqueness is `(sessionId, clientId)`. `AlbumPhoto` uniqueness is `(albumId, photoId)` and `(albumId, position)`. Print idempotency uniqueness is `(clientId, idempotencyKey)`. Normalized discount codes and public-gallery slugs are unique. Duplicate-key races return the existing idempotent result or a conflict as appropriate.

## Static and acceptance interfaces

`ComponentCatalog` is a typed manifest of `ComponentExample` records. A record identifies the component, route, and state; the example host supplies input/output demonstrations through `ComponentContract` metadata. `CatalogArtifact` contains build revision, entry point, and asset manifest. `StaticArtifactPublisher` consumes only a validated artifact.

`CoverageRecord` refers to `AcceptanceCriterion` without owning its lifecycle. It records layer, status, real test-file link, test name, uncovered behavior, and red/green artifacts. Missing evidence is represented explicitly rather than by a nonexistent path. Architecture checks inspect source and build configuration independently of behavior tests.
