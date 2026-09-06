# Angular components

The maintained inventory is [component-catalog.json](../frontend/component-catalog.json): 26 Angular components, each linked to its independently rendered design-system example. The [catalog manifest](../design-system/component-manifest.json) covers the shared visual primitives, complete screen patterns and dialog states. The applications implement the approved behavior with reusable regions and native HTML controls; the older prototype vocabulary below explains visual references rather than requiring a separate Angular wrapper for each HTML element.

## Placement and contracts

`components` contains studio-independent primitives with literal inputs and intent outputs. `domain` contains studio-aware regions and flat shared models. `application` contains pages, routing, shells and route-owned behavior services. The three application projects contain bootstrap files only. [AGENTS.md](../AGENTS.md#frontend) specifies the dependency direction.

Each consumed service has a separate interface and injection token in `api`; application composition binds production adapters or controlled acceptance implementations. HTTP details stay inside API adapters. Editor drafts, loading, errors and selections live in signals. Route subscriptions use `takeUntilDestroyed` because Angular Router exposes navigation as an observable event stream; HTTP adapters use `firstValueFrom` to finish HTTP requests. Primitive and read-only regions use OnPush. Forms use default change detection for nested draft fields bound through Angular forms.

Template, class and stylesheet remain separate. BEM classes reference the authoritative [design tokens](../design-system/assets/tokens.css), mirrored once in [frontend/styles.css](../frontend/styles.css). The design system has its own build, static hosting instructions and browser acceptance suite. New ordered-selection, Toronto date/time and image-retry examples were rendered and reviewed before consumption.

## Implemented inventory

| Component | Library | Contract / catalog example |
| --- | --- | --- |
| [CatalogPage](../frontend/projects/application/src/lib/catalog-page/catalog-page.ts) | `application` | `pattern:admin-records` · [catalog](../design-system/README.md) |
| [ClientPage](../frontend/projects/application/src/lib/client-page/client-page.ts) | `application` | `pattern:client-gallery` · [catalog](../design-system/README.md) |
| [LoginPage](../frontend/projects/application/src/lib/login-page/login-page.ts) | `application` | `component:login` · [catalog](../design-system/README.md) |
| [PrintInbox](../frontend/projects/application/src/lib/print-inbox/print-inbox.ts) | `application` | `pattern:print-review` · [catalog](../design-system/README.md) |
| [PublicPage](../frontend/projects/application/src/lib/public-page/public-page.ts) | `application` | `pattern:marketing-home` · [catalog](../design-system/README.md) |
| [QuotePage](../frontend/projects/application/src/lib/quote-page/quote-page.ts) | `application` | `pattern:quote-calculator` · [catalog](../design-system/README.md) |
| [SessionPage](../frontend/projects/application/src/lib/session-page/session-page.ts) | `application` | `pattern:session-review` · [catalog](../design-system/README.md) |
| [SettingsPage](../frontend/projects/application/src/lib/settings-page/settings-page.ts) | `application` | `pattern:admin-settings` · [catalog](../design-system/README.md) |
| [Shell](../frontend/projects/application/src/lib/shell/shell.ts) | `application` | `component:shell` · [catalog](../design-system/README.md) |
| [Dialog](../frontend/projects/components/src/lib/dialog/dialog.ts) | `components` | `component:dialog` · [catalog](../design-system/README.md) |
| [EmptyState](../frontend/projects/components/src/lib/empty-state/empty-state.ts) | `components` | `component:empty-state` · [catalog](../design-system/README.md) |
| [Notice](../frontend/projects/components/src/lib/notice/notice.ts) | `components` | `component:notice` · [catalog](../design-system/README.md) |
| [PhotoGrid](../frontend/projects/domain/src/lib/photo-grid/photo-grid.ts) | `domain` | `component:photo-grid` · [catalog](../design-system/README.md) |
| [QuoteInputForm](../frontend/projects/domain/src/lib/quote-input-form/quote-input-form.ts) | `domain` | `pattern:quote-calculator` · [catalog](../design-system/README.md) |
| [QuoteSummary](../frontend/projects/domain/src/lib/quote-summary/quote-summary.ts) | `domain` | `pattern:quote-calculator` · [catalog](../design-system/README.md) |
| [DateTimeField](../frontend/projects/components/src/lib/date-time-field/date-time-field.ts) | `components` | `component:date-time-field` · [catalog](../design-system/README.md) |
| [OrderedSelection](../frontend/projects/components/src/lib/ordered-selection/ordered-selection.ts) | `components` | `component:ordered-selection` · [catalog](../design-system/README.md) |
| [PhotoOrder](../frontend/projects/domain/src/lib/photo-order/photo-order.ts) | `domain` | `component:ordered-selection` · [catalog](../design-system/README.md) |
| [CatalogEditor](../frontend/projects/domain/src/lib/catalog-editor/catalog-editor.ts) | `domain` | `pattern:admin-records` · [catalog](../design-system/README.md) |
| [SettingsEditor](../frontend/projects/domain/src/lib/settings-editor/settings-editor.ts) | `domain` | `pattern:admin-settings` · [catalog](../design-system/README.md) |
| [AlbumEditor](../frontend/projects/domain/src/lib/album-editor/album-editor.ts) | `domain` | `pattern:client-gallery` · [catalog](../design-system/README.md) |
| [PrintRequestEditor](../frontend/projects/domain/src/lib/print-request-editor/print-request-editor.ts) | `domain` | `pattern:client-gallery` · [catalog](../design-system/README.md) |
| [SessionUpload](../frontend/projects/domain/src/lib/session-upload/session-upload.ts) | `domain` | `pattern:session-review` · [catalog](../design-system/README.md) |
| [SessionPhotoReview](../frontend/projects/domain/src/lib/session-photo-review/session-photo-review.ts) | `domain` | `pattern:session-review` · [catalog](../design-system/README.md) |
| [SessionDelivery](../frontend/projects/domain/src/lib/session-delivery/session-delivery.ts) | `domain` | `pattern:session-review` · [catalog](../design-system/README.md) |
| [PrintRequestDetails](../frontend/projects/domain/src/lib/print-request-details/print-request-details.ts) | `domain` | `pattern:print-review` · [catalog](../design-system/README.md) |

PhotoGrid preserves unavailable placeholders, prevents new selection of processing/failed photos, supports retry after a temporary image failure and emits manual inspection intent. OrderedSelection handles keyboard-accessible ordering; PhotoOrder maps studio photographs to that literal contract. DateTimeField requires an explicit occurrence for ambiguous Toronto times. The catalog, settings, album, print, session-upload, session-review, session-delivery and inbox-details regions consume their typed route service token. Pages compose those regions with navigation and shared feedback.

## Prototype vocabulary reference

The following original inventory records visual concepts from the [63 HTML prototypes](mocks/README.md). Names and selectors in these tables are conceptual references; the maintained implementation names, library placement and catalog bindings are the table above. Prototype-only dashboard, payment/fulfilment and booking concepts do not expand the accepted L1/L2 functionality. Routing and persistence belong to application services, never a presentation primitive.

## Primitives

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 1 | `ButtonComponent` | `qbs-button` | `.btn`, `.secondary`, `.quiet`, `.danger`, `.small` | `variant`, `size`, `disabled`, `type`, `ariaLabel`; `clicked` | M A C D |
| 2 | `LinkButtonComponent` | `qbs-link-button` | anchor carrying `btn` classes | `href` / `routerLink`, `variant`, `size`, `external` | M A C D |
| 3 | `TextLinkComponent` | `qbs-text-link` | `.text-link` | `href` / `routerLink`, `withArrow` | M A C D |
| 4 | `IconComponent` | `qbs-icon` | `.arrow`, `.symbol`, inline glyphs `↗ ← → ↑ ◇ ✓ ×` | `name`, `size`, `decorative` | M A C D |
| 5 | `IconButtonComponent` | `qbs-icon-button` | `.icon-btn` | `icon`, `ariaLabel`, `disabled`; `clicked` | M A C D |
| 6 | `BadgeComponent` | `qbs-badge` | `.badge`, `.badge.pending`, `.badge.error` | `label`, `tone` | M A C D |
| 7 | `AvatarComponent` | `qbs-avatar` | `.avatar` | `initials`, `label` | A |
| 8 | `EyebrowComponent` | `qbs-eyebrow` | `.eyebrow` | content projection | M A C D |
| 9 | `BrandLockupComponent` | `qbs-brand-lockup` | `.brand` | `homeLink`, `tagline` | M A C D |
| 10 | `SkipLinkComponent` | `qbs-skip-link` | `.skip-link` | `targetId` | M A C |
| 11 | `SeparatorComponent` | `qbs-separator` | `hr.separator` | `spacing` | M A C |
| 12 | `SkeletonComponent` | `qbs-skeleton` | `.skeleton` | `height`, `ariaLabel` | A C D |
| 13 | `ProgressBarComponent` | `qbs-progress-bar` | `progress` | `value`, `max`, `ariaLabel` | A |
| 14 | `TabListComponent` | `qbs-tab-list` | `.tabs`, `.tab`, `.tab.active` | `tabs`, `activeId`; `tabSelected` | M D |
| 15 | `MoneyPipe` (support) | `qbsMoney` | `Intl.NumberFormat('en-CA', CAD)` | value formatting used by cost, table, and cart components | M A C D |
| 16 | `VisuallyHiddenDirective` (support) | `qbsVisuallyHidden` | `.visually-hidden` | applies the shared screen-reader-only rule | M A C D |

## Layout

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 17 | `PageContainerComponent` | `qbs-page-container` | `.container`, `.client-main`, `.app-main` | `width` | M A C D |
| 18 | `PageHeadingComponent` | `qbs-page-heading` | `.page-head` | `eyebrow`, `title`, `description`; actions slot | M A C |
| 19 | `SectionComponent` | `qbs-section` | `.section` | `spacing` | M A C |
| 20 | `SectionHeadComponent` | `qbs-section-head` | `.section-head` | `eyebrow`, `title`; action slot | M A C |
| 21 | `GridComponent` | `qbs-grid` | `.grid.two`, `.grid.three`, `.grid.four` | `columns`, `gap` | M A C D |
| 22 | `SplitLayoutComponent` | `qbs-split-layout` | `.split` | main and aside slots | M A C |
| 23 | `StickyAsideComponent` | `qbs-sticky-aside` | `.sticky` | `offset` | M A C |
| 24 | `PanelComponent` | `qbs-panel` | `.panel`, `.panel.soft` | `tone`, `heading` | M A C D |
| 25 | `StackComponent` | `qbs-stack` | `.stack` | `gap` | M A C |
| 26 | `InlineRowComponent` | `qbs-inline-row` | `.row`, `.between` | `align`, `justify`, `wrap` | M A C D |
| 27 | `ToolbarComponent` | `qbs-toolbar` | `.toolbar` | leading and trailing slots | A C |
| 28 | `IntroComponent` | `qbs-intro` | `.intro` | `eyebrow`, `title`, `body` | M C |
| 29 | `CenteredLayoutComponent` | `qbs-centered-layout` | `.centered` | content projection | M A C |

## Application chrome and navigation

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 30 | `PublicHeaderComponent` | `qbs-public-header` | `.public-header` | `links`, `activeLink`, `actions`; `signOutRequested` | M C |
| 31 | `PublicNavComponent` | `qbs-public-nav` | `.public-nav`, `.public-nav.open`, `a.active` | `links`, `activeLink`, `open`; `navigated` | M C |
| 32 | `NavToggleButtonComponent` | `qbs-nav-toggle` | `.nav-toggle` with `aria-expanded` / `aria-controls` | `expanded`, `controls`; `toggled` | M A C |
| 33 | `PublicFooterComponent` | `qbs-public-footer` | `.public-footer` | `links`, `tagline`, `copyright` | M C |
| 34 | `WorkspaceShellComponent` | `qbs-workspace-shell` | `.workspace`, `.workspace-main` | sidebar, topbar, and main slots; `sidebarOpen` | A |
| 35 | `SidebarNavComponent` | `qbs-sidebar-nav` | `.sidebar`, `.sidebar.open` | `groups`, `activeLink`, `open` | A |
| 36 | `SidebarNavGroupComponent` | `qbs-sidebar-nav-group` | `.nav-group` | `label` | A |
| 37 | `SidebarNavLinkComponent` | `qbs-sidebar-nav-link` | `.side-link`, `.side-link.active` | `link`, `label`, `active` | A |
| 38 | `SidebarFooterComponent` | `qbs-sidebar-footer` | `.sidebar-bottom` | action slot; `signOutRequested` | A |
| 39 | `AppTopbarComponent` | `qbs-app-topbar` | `.app-topbar`, `.mobile-brand`, `.optional` | `breadcrumb`, `userName`, `initials`; `menuToggled` | A |
| 40 | `AuthLayoutComponent` | `qbs-auth-layout` | `.auth`, `.auth-photo`, `.auth-main` | `photo`, `eyebrow`, `title`, `description`; form slot | M A C |

## Forms

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 41 | `FormFieldComponent` | `qbs-form-field` | `.field`, `.field > span`, `.hint`, `.field-error` | `label`, `hint`, `required`, `errorText`, `for` | M A C |
| 42 | `TextInputComponent` | `qbs-text-input` | `input` with `.invalid` state; `text`, `email`, `tel`, `password`, `date`, `time`, `number` types | `type`, `value` model, `placeholder`, `min`, `max`, `step`, `autocomplete`, `invalid` | M A C |
| 43 | `SelectComponent` | `qbs-select` | `select` inside `.field` | `options`, `value` model, `disabledOptions` | M A C |
| 44 | `TextareaComponent` | `qbs-textarea` | `textarea` inside `.field` | `value` model, `rows`, `required` | M A C |
| 45 | `CheckboxComponent` | `qbs-checkbox` | `.checkline` | `checked` model, `label`, `required` | M A C |
| 46 | `SearchInputComponent` | `qbs-search-input` | `input[type=search]` in `.toolbar` | `value` model, `placeholder`, `ariaLabel`; `cleared` | A C |
| 47 | `FieldErrorComponent` | `qbs-field-error` | `.field-error`, `.inline-error` | `message` | M A C |
| 48 | `FormSectionComponent` | `qbs-form-section` | `.form-section` with numbered `h3` | `step`, `title` | M A |
| 49 | `FormActionsComponent` | `qbs-form-actions` | `.form-actions` | content projection, `note` | M A C |
| 50 | `RepeatableRowComponent` | `qbs-repeatable-row` | `.location-row` plus the add-another control | `rows` model, `addLabel`, `removeLabel`; `rowAdded`, `rowRemoved` | M |
| 51 | `FileDropzoneComponent` | `qbs-file-dropzone` | `.dropzone`, `.dropzone.drag`, hidden file input with label-as-button | `accept`, `maxFileSizeMb`, `multiple`; `filesChosen`, `dragStateChanged` | A |

## Feedback and status

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 52 | `NoticeComponent` | `qbs-notice` | `.notice`, `.notice.error`, `.notice.success` | `message`, `tone`, `dismissible`, action slot; `dismissed` | M A C |
| 53 | `ToastComponent` | `qbs-toast` | `.toast` with `role="status"` | `message`, `durationMs`; `expired` | M A C |
| 54 | `EmptyStateComponent` | `qbs-empty-state` | `.empty`, `.empty .symbol`, `.empty-filter` | `symbol`, `title`, `description`, action slot | M A C |
| 55 | `ImageFallbackComponent` | `qbs-image-fallback` | `.image-fallback` | `message` | M A C |
| 56 | `SystemMessageComponent` | `qbs-system-message` | `.centered`, `.system-number` | `code`, `title`, `description`, action slot; `retryRequested` | M A C |
| 57 | `SkeletonGridComponent` | `qbs-skeleton-grid` | `.grid` of `.skeleton` blocks | `count`, `columns`, `message` | A C |

## Overlays

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 58 | `DialogComponent` | `qbs-dialog` | native `dialog`, `::backdrop`, focus restore | `open` model, `title`, `wide`, `labelledBy`; `closed` | M A C |
| 59 | `DialogHeaderComponent` | `qbs-dialog-header` | `.dialog-head` | `title`; `closeRequested` | M A C |
| 60 | `DialogBodyComponent` | `qbs-dialog-body` | `.dialog-body` | content projection | M A C |
| 61 | `ConfirmDialogComponent` | `qbs-confirm-dialog` | delete, discard, sign-out, cancel-upload, publish, and reset dialog bodies | `title`, `message`, `confirmLabel`, `cancelLabel`, `destructive`; `confirmed`, `cancelled` | M A C |
| 62 | `LightboxDialogComponent` | `qbs-lightbox-dialog` | `.lightbox-dialog`, `.lightbox-image` | `photo`, `index`, `total`; `closed` | M A C |
| 63 | `LightboxNavComponent` | `qbs-lightbox-nav` | `.lightbox-nav` | `index`, `total`; `previousRequested`, `nextRequested` | M A C |

## Photography and media

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 64 | `PhotoImageComponent` | `qbs-photo-image` | `img` rules with `object-fit`, `loading`, `fetchpriority`, fallback swap | `src`, `alt`, `priority`, `fit`; `loadFailed` | M A C |
| 65 | `HeroComponent` | `qbs-hero` | `.hero`, `.hero-copy`, `.hero-meta` | `eyebrow`, `title`, `body`, `meta`; action slot | M |
| 66 | `HeroImageComponent` | `qbs-hero-image` | `.hero-image`, `.hero-image .caption` | `photo`, `caption`, `height` | M C |
| 67 | `PhotoCardComponent` | `qbs-photo-card` | `.photo-card`, `.photo-card .image`, `.caption` | `photo`, `title`, `subtitle`, `link`; footer slot | M A C |
| 68 | `GalleryCardComponent` | `qbs-gallery-card` | `galleryCard` composition of `.photo-card` | `gallery` view model, `link` | M C |
| 69 | `PhotoGridComponent` | `qbs-photo-grid` | `.photo-grid` | `photos`, `selectable`, `selection` model, `showSuggestions` | M A C |
| 70 | `PhotoTileComponent` | `qbs-photo-tile` | `.photo-tile`, `.photo-open`, `.photo-label`, tile `.badge` | `photo`, `index`, `label`, `suggested`, `unavailable`; `opened` | M A C |
| 71 | `PhotoSelectCheckboxComponent` | `qbs-photo-select` | `.photo-select` | `checked` model, `photoLabel`; `toggled` | A C |
| 72 | `PhotoViewerComponent` | `qbs-photo-viewer` | `.photo-view` | `photo`, `fallbackMessage` | C |
| 73 | `SelectionBarComponent` | `qbs-selection-bar` | `.selection-bar` | `selectedCount`, action slot; `selectAllRequested`, `clearRequested` | A C |

## Data display

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 74 | `DataTableComponent` | `qbs-data-table` | `.table-wrap`, `table`, `th`, `td` | `columns`, `rows`, `caption`, `emptyTemplate` | A C |
| 75 | `TableNameCellComponent` | `qbs-table-name-cell` | `.table-name` plus muted sub-line | `label`, `sublabel`, `link` | A C |
| 76 | `TableActionsCellComponent` | `qbs-table-actions-cell` | `.table-actions` | `actions`; `actionSelected` | A C |
| 77 | `TableThumbComponent` | `qbs-table-thumb` | `.table-thumb` | `photo`, `alt` | A |
| 78 | `StatCardComponent` | `qbs-stat-card` | `.stat`, `.stat .number` | `label`, `value`, `description` | A |
| 79 | `CostRowComponent` | `qbs-cost-row` | `.cost-row`, `.cost-row.total`, `.cost-row.discount` | `label`, `sublabel`, `amount`, `variant` | M A C |
| 80 | `CostSummaryComponent` | `qbs-cost-summary` | quote breakdown and print estimate panels | `lines`, `discount`, `total`, `currencyNote` | M C |
| 81 | `CartItemComponent` | `qbs-cart-item` | `.cart-item`, `.qty-field` | `photo`, `title`, `sizeOptions`, `size` model, `quantity` model; `removed` | C |
| 82 | `RequestLineComponent` | `qbs-request-line` | `.cart-item` read-only variant, `.request-lines` | `photo`, `title`, `details`, `amount` | C |
| 83 | `CalendarWeekComponent` | `qbs-calendar-week` | `.calendar`, `.calendar-wrap` | `days`, `weekStart` | A |
| 84 | `CalendarDayComponent` | `qbs-calendar-day` | `.day` | `weekdayLabel`, `dayNumber`, `entries` | A |
| 85 | `AppointmentChipComponent` | `qbs-appointment-chip` | `.appointment` | `time`, `title`, `subtitle`, `link`, `interactive`; `selected` | A |
| 86 | `DateNavComponent` | `qbs-date-nav` | `.date-nav` | `label`; `previousRequested`, `nextRequested` | A |
| 87 | `UploadQueueComponent` | `qbs-upload-queue` | `#upload-queue` list | `files`; `retryRequested`, `retryAllRequested`, `cancelRequested` | A |
| 88 | `UploadRowComponent` | `qbs-upload-row` | `.upload-row` with `progress` and status badge | `fileName`, `sizeMb`, `progress`, `status`; `retryRequested` | A |
| 89 | `AiSuggestionListComponent` | `qbs-ai-suggestion-list` | `.ai-reason` entries in the AI review dialog | `suggestions`, `disclaimer`; `accepted`, `dismissed` | A |
| 90 | `EditorPreviewPanelComponent` | `qbs-editor-preview-panel` | `.preview`, `#editor-preview`, gallery preview aside | `eyebrow`, `heading`, `body`, `photo` | A |
| 91 | `QuoteStripComponent` | `qbs-quote-strip` | `.quote-strip` | `eyebrow`, `title`, `body`; action slot | M C |
| 92 | `PromotionCardComponent` | `qbs-promotion-card` | promotions `.photo-card` plus `.panel` composition | `promotion` view model; `detailsRequested` | M |
| 93 | `ServiceCardComponent` | `qbs-service-card` | services `.photo-card` with rate link | `service` view model, `fromRate`, `quoteLink` | M |

## Design-system catalog

| # | Component | Selector | Prototype source | Key inputs / outputs | Apps |
| --- | --- | --- | --- | --- | --- |
| 94 | `CatalogHeroComponent` | `qbs-catalog-hero` | `.index-hero` | `eyebrow`, `title`, `body`, `badges` | D |
| 95 | `CatalogSiteCardComponent` | `qbs-catalog-site-card` | `.site-card` | `site`, `title`, `description`, `entryLink` | D |
| 96 | `CatalogEntryRowComponent` | `qbs-catalog-entry-row` | `.index-row`, `.index-links`, `.dialog-link` | `name`, `description`, `stateLinks`, `dialogLinks` | D |
| 97 | `CatalogStateSwitcherComponent` | `qbs-catalog-state-switcher` | `.prototype-bar`, `.prototype-label` | `states`, `dialogs`, `activeState`; `stateSelected`, `resetRequested` | D |

## Coverage by application

| Prototype area | Representative pages | Primary components |
| --- | --- | --- |
| Marketing home and portfolio | `home`, `portfolio`, `gallery` | 30, 31, 33, 65, 66, 14, 67, 68, 69, 91, 54 |
| Marketing services, prints, packages | `services`, `prints`, `promotions` | 93, 92, 79, 24, 22, 54 |
| Marketing quote and contact | `quote`, `quote-summary`, `contact` | 48, 41–45, 50, 80, 79, 52, 23 |
| Admin workspace shell | every admin page | 34, 35, 36, 37, 38, 39, 32, 7 |
| Admin dashboards and lists | `dashboard`, entity list pages | 78, 74, 75, 76, 6, 46, 54, 27 |
| Admin editors | `*-editor` pages | 41–49, 69, 71, 90, 61, 52 |
| Admin scheduling | `schedule` | 83, 84, 85, 86, 58, 61, 52 |
| Admin upload and review | `upload`, `review` | 51, 87, 88, 13, 73, 69, 89, 57, 61 |
| Client galleries and albums | `galleries`, `gallery`, `photo`, `albums`, `album`, `album-editor` | 68, 69, 70, 71, 72, 73, 62, 63, 54, 46 |
| Client prints | `prints`, `print-review`, `print-confirmation`, `requests`, `request` | 81, 82, 79, 80, 74, 6, 56, 52 |
| Authentication | `login`, `forgot-password`, `reset-password` | 40, 41–44, 47, 52, 1, 3 |
| System states | `not-found`, `access-denied`, `session-expired`, `service-error` | 56, 29, 1, 3 |
| Component catalog | design-system entry point | 94, 95, 96, 97 |
