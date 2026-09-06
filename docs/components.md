# Angular presentation components

## Overview

This document identifies the Angular presentation components required to implement the marketing, admin, and client applications defined by the approved HTML prototype in [`docs/mocks`](mocks/). A presentation component owns markup, styles, and presentation logic only. It renders from typed inputs, reports intent through outputs, and holds no HTTP calls, routing decisions, or domain state.

The inventory is derived from the prototype's shared stylesheet [`assets/styles.css`](mocks/assets/styles.css) and the render functions in [`assets/app.js`](mocks/assets/app.js), which together define every visual block used by the 63 prototype pages. It supports requirements `L2-001`, `L2-002`, `L2-046`, `L2-047`, and `L2-066`, and the design-system slice at [`detailed-designs/design-system/browse-component-catalog`](detailed-designs/design-system/browse-component-catalog/README.md).

Status: implemented as the `@qbs/components` Angular library in `frontend/projects/components`. All 97 entries below have public exports and internal browser examples. The standalone design-system application and deployment remain deferred. See the [library README](../frontend/projects/components/README.md) for API details and [acceptance evidence](../frontend/projects/components/testing/ACCEPTANCE.md) for validation scope.

## Conventions

- Components live in the `components` library of the Angular workspace described in [`detailed-designs/architecture.md`](detailed-designs/architecture.md) and are reused by the `marketing`, `admin`, and `client` applications and rendered as examples by the root-level `design-system` catalog.
- Selector prefix is `qbs-`. Directory and file names are kebab-case. TypeScript, HTML, and CSS stay in separate files.
- Components are standalone, use `ChangeDetectionStrategy.OnPush`, signal-based `input()` / `model()`, and `output()` emitters.
- BEM class names belong to this library. Application screens reuse the block names rather than redefining styles. The prototype's flat class names (`.btn`, `.photo-card`, `.cost-row`) become BEM blocks (`qbs-button`, `qbs-photo-card`, `qbs-cost-row`) with element and modifier suffixes.
- Design tokens from `:root` in the prototype stylesheet (`--ink`, `--muted`, `--line`, `--paper`, `--soft`, `--accent`, `--danger`, `--success`, `--serif`, `--sans`) become the shared token stylesheet consumed by every component.
- Presentation components accept view models, not domain entities. Container components in each application map API contracts to these view models and handle navigation, persistence, and error recovery.
- Every component listed here has a named internal acceptance example. Publishing those examples through the standalone catalog remains required by `L2-047` and is deferred from this library-only increment.

Application column key: **M** marketing, **A** admin, **C** client, **D** design-system catalog.

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

## Notes on scope

- Components 1–97 cover presentation only. Data loading, routing, form submission, optimistic concurrency, and error recovery belong to container components in each application and to the services described in [`detailed-designs/contracts.md`](detailed-designs/contracts.md).
- Prototype behavior that simulates a backend — seeded records, local storage persistence, simulated uploads, and simulated AI suggestions — does not become a presentation concern. The corresponding components accept the resulting view model as an input.
- Responsive behavior at 390, 768, and 1440 CSS pixels stays inside each component's stylesheet, matching the prototype breakpoints at 480, 780, 1100, and 1500 pixels, per `L2-066`.
- Keyboard operability, focus-visible outlines, `aria-live` regions, and reduced-motion handling are component-level obligations carried over from the prototype stylesheet and markup.
- Adding a component to this library requires a matching catalog example and an update to this document, per `L2-047`.
