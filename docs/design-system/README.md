# Quinntyne Brown Studio design system

Extracted from [docs/mocks](../mocks/README.md) on 2026-10-18. Open [index.html](index.html) locally. Light and dark themes, WCAG 2.2 AA.

## Foundations

| Page | Covers |
| --- | --- |
| [Color](foundations/color.html) | Warm neutrals, a muted olive and three status hues, remapped per theme so components never change. |
| [Typography](foundations/typography.html) | Georgia headings, Arial body, nine sizes and twelve role shorthands. |
| [Spacing](foundations/spacing.html) | A 4px scale from 0 to 96px with rules for inside and between. |
| [Layout](foundations/layout.html) | Breakpoints, a 4/8/12 column grid, shell dimensions and page templates. |
| [Elevation](foundations/elevation.html) | Four shadow levels, surface hierarchy and a nine-layer z-order. |
| [Shape](foundations/shape.html) | Five radii and two border widths. |
| [Motion](foundations/motion.html) | Five durations, four easings, reduced-motion by default. |
| [Iconography](foundations/iconography.html) | A 24-unit stroke icon set that inherits text colour. |
| [Theming](foundations/theming.html) | Three token tiers, light and dark themes, and how to add another. |
| [Responsive](foundations/responsive.html) | Mobile-first rules and what each component does at 390, 768 and 1440 px. |
| [Accessibility](foundations/accessibility.html) | The WCAG 2.2 AA commitments the system makes and how they are tested. |
| [Content](foundations/content.html) | Voice, casing, numbers and the message formulas for errors, empty states and confirmations. |

## Components

| Component | Variants | States | Source mocks |
| --- | --- | --- | --- |
| [Button](components/button.html) | Primary, Secondary, Ghost, Link, Danger | Default, Hover, Focus, Active, Disabled, Loading | `admin/sessions`, `admin/upload`, `client/albums` |
| [Button group](components/button-group.html) | Segmented, Standard | Unpressed, Pressed, Hover, Focus, Disabled | `admin/schedule`, `admin/upload`, `client/prints` |
| [Link](components/link.html) | Standalone, Inline, External | Default, Hover, Focus | `marketing/contact`, `marketing/quote`, `client/prints` |
| [Menu](components/menu.html) | Item, Danger item | Default, Hover, Active item, Disabled | `admin/upload`, `admin/rates`, `admin/session-editor` |
| [Checkbox](components/checkbox.html) | Checkbox, With label | Unchecked, Checked, Hover, Focus, Invalid, Disabled, Disabled checked | `client/request`, `client/prints`, `marketing/quote` |
| [Date navigator](components/date-navigator.html) | Previous button, Next button | Default, Hover, Focus, Disabled | `admin/schedule`, `admin/rates`, `admin/session-editor` |
| [File upload](components/file-upload.html) | Drop zone | Idle, Hover, Dragging, Disabled | `admin/upload`, `admin/session-editor`, `client/request` |
| [Form field](components/form-field.html) | Label + control, With hint | Default, Focus, Invalid, Disabled, Read-only | `marketing/contact`, `marketing/quote`, `admin/session-editor` |
| [Form layout](components/form-layout.html) | Save button | Idle, Submitting, Invalid, Saved | `admin/session-editor`, `marketing/contact`, `client/request` |
| [Radio group](components/radio-group.html) | Radio, Card | Unchosen, Chosen, Hover, Focus, Invalid, Disabled, Disabled chosen | `client/request`, `client/prints`, `marketing/quote` |
| [Select](components/select.html) | Chosen, Placeholder | Default, Hover, Focus, Invalid, Disabled | `client/prints`, `admin/rates`, `admin/session-editor` |
| [Switch](components/switch.html) | Switch, With label | Off, On, Hover, Focus, Disabled off, Disabled on | `admin/session-editor`, `admin/rates`, `client/request` |
| [Text field](components/text-field.html) | Empty, Filled | Default, Hover, Focus, Invalid, Disabled, Read-only | `admin/login`, `marketing/contact`, `marketing/quote` |
| [Textarea](components/textarea.html) | Empty, Filled | Default, Hover, Focus, Invalid, Disabled, Read-only | `marketing/contact`, `admin/session-editor`, `client/request` |
| [Breadcrumb](components/breadcrumb.html) | Item | Default, Focus, Current page | `client/gallery`, `admin/session`, `admin/gallery-editor` |
| [Footer](components/footer.html) | Footer link | Default, Focus | `marketing/home`, `marketing/contact`, `client/galleries` |
| [Pagination](components/pagination.html) | Page number, Previous | Default, Focus, Current, Disabled | `admin/sessions`, `admin/inquiries`, `marketing/blog` |
| [Sidebar](components/sidebar.html) | Link, Link with count | Default, Hover, Focus, Current | `admin/dashboard`, `admin/sessions`, `admin/upload` |
| [Skip link](components/skip-link.html) | Skip to content, Skip to states | Focused | `marketing/home`, `admin/dashboard`, `client/galleries` |
| [Tabs](components/tabs.html) | Pill, Underline | Default, Hover, Focus, Selected, Disabled | `marketing/portfolio` |
| [Top bar](components/top-bar.html) | Nav link | Default, Focus, Current page | `marketing/home`, `client/galleries`, `admin/dashboard` |
| [Accordion](components/accordion.html) | Item, Focused summary | Collapsed, Expanded | `marketing/about`, `marketing/contact` |
| [Card](components/card.html) | Default, Soft, Interactive, Selected | Default, Hover | `admin/dashboard`, `client/galleries`, `marketing/home` |
| [Dialog](components/dialog.html) | Confirm, Destructive | Default, Busy, Disabled | `admin/session-expired`, `admin/upload`, `client/photo` |
| [Divider](components/divider.html) | Horizontal, Vertical | Default | `marketing/contact`, `admin/studio-details`, `client/prints` |
| [Layout primitives](components/layout.html) | Container | Default | `marketing/home`, `admin/dashboard`, `admin/sessions` |
| [Tooltip](components/tooltip.html) | Text, With shortcut | Hover, Focus | `client/photo`, `admin/upload`, `admin/review` |
| [Avatar](components/avatar.html) | Avatar button | Default, Hover, Focus, Disabled | `admin/dashboard`, `admin/sessions` |
| [Badge](components/badge.html) | Neutral, Success, Pending, Error, Info | Text only, With dot | `admin/sessions`, `admin/review`, `admin/upload` |
| [Blog card](components/blog-card.html) | With image, No image | Default, Hover, Focus | `marketing/blog`, `marketing/blog-search` |
| [Chip](components/chip.html) | Filter chip | Default, Hover, Focus, Pressed, Disabled | `marketing/portfolio`, `admin/print-prices`, `client/requests` |
| [Cost summary](components/cost-summary.html) | Itemised, With discount, Recalculating, Empty, Error | Default | `marketing/quote-summary`, `marketing/quote`, `marketing/prints` |
| [Description list](components/description-list.html) | Text value, Link value, Missing value | Default, Link hover, Link focus | `client/request`, `marketing/contact` |
| [Line item](components/line-item.html) | Default, Field focus, Invalid quantity, Unavailable | Default | `client/request` |
| [Photo tile](components/photo-tile.html) | Tile | Default, Hover, Selected | `admin/review`, `client/gallery`, `admin/gallery-editor` |
| [Schedule calendar](components/calendar.html) | Appointment, Empty day | Default, Link hover, Link focus | `admin/schedule` |
| [Selection bar](components/selection-bar.html) | Items selected, Nothing selected, Working, One selected | Default | `client/gallery`, `admin/review`, `client/album` |
| [Stat](components/stat.html) | Value, Zero, Loading, Error | Default | `admin/dashboard` |
| [Stepper](components/stepper.html) | Step | Upcoming, Current | `marketing/about` |
| [Table](components/table.html) | Default, Row hover, Row selected, Loading, Empty, Error | Comfortable, Dense | `admin/sessions`, `admin/dashboard`, `admin/print-prices` |
| [Alert](components/alert.html) | Info, Success, Warning, Danger | Default | `admin/upload`, `admin/service-error`, `client/prints` |
| [Empty state](components/empty-state.html) | With action, Without action | Full, Compact | `admin/sessions`, `admin/media`, `client/galleries` |
| [Error page](components/error-page.html) | Service error, Sign in | Default, Retrying | `admin/not-found`, `admin/access-denied`, `admin/service-error` |
| [Inline message](components/inline-message.html) | Hint, Error | Default, Focus, Invalid | `admin/session-editor`, `admin/login`, `marketing/contact` |
| [Progress](components/progress.html) | Normal, Danger | 0%, Partial, Complete | `admin/upload`, `admin/media`, `assets/app.js` |
| [Skeleton](components/skeleton.html) | Block, Text, Circle | Shimmering, Reduced motion | `admin/sessions`, `admin/media`, `client/galleries` |
| [Spinner](components/spinner.html) | Spinner | Small, Medium, Large | `admin/upload`, `admin/media`, `client/albums` |
| [Toast](components/toast.html) | Message, With action, Error | Showing, Paused (hover or focus) | `admin/sessions`, `admin/upload`, `client/albums` |

## Patterns

| Pattern | Covers |
| --- | --- |
| [Forms](patterns/forms.html) | Layout, validation timing, error summary and submit states for every form in the studio apps. |
| [Feedback and loading](patterns/feedback-and-loading.html) | Which signal to use for which event, and how to show work in progress. |
| [Empty and error states](patterns/empty-and-error-states.html) | Copy formulas and layouts for when there is nothing to show or something went wrong. |
| [Navigation and page structure](patterns/navigation-and-page-structure.html) | The shells, page headers and wayfinding shared by the public site, admin and client apps. |
| [Dialogs and overlays](patterns/dialogs-and-overlays.html) | When to use a dialog, menu, tooltip or a full page, and how overlays stack. |
| [Data tables and lists](patterns/data-tables-and-lists.html) | Density, sorting, filtering, selection and the responsive strategy for admin data. |
| [Notifications](patterns/notifications.html) | Toasts, banners and alerts: priority, grouping and dismissal rules. |
| [Content and tone](patterns/content-and-tone.html) | How the studio writes: voice, button verbs, numbers, error and empty-state copy. |

## Tokens

- [`tokens/tokens.css`](tokens/tokens.css): source of truth (primitives, semantic, themes, preference overrides).
- [`tokens/tokens.json`](tokens/tokens.json): DTCG export generated from `tokens.css`.
- [`tokens/contrast-pairs.json`](tokens/contrast-pairs.json): every foreground/background pair the components rely on, checked in both themes.
- [`assets/components.css`](assets/components.css): the token-driven component stylesheet.

## Drift found in the mocks

| Mock | Issue | Resolution |
| --- | --- | --- |
| `assets/styles.css` | 57 spacing values, 32 type sizes (9-95px), many off the 4px grid | Snapped to the 4px spacing scale and a nine-step type scale; nothing below 12px. |
| `assets/styles.css` | Six breakpoints (480, 600, 780, 900, 1100, 1500px) | Five tokens: 480, 768, 1100, 1280, 1500; 600 and 780 merge to 768, 900 to 1100. |
| `assets/styles.css` | 41 colour literals incl. near-duplicates (#f5f6ef/#f5f5f0/#f7f7f2, #e9ebe2/#eceee5) | Collapsed to two ramps (stone, olive) and four status ramps; semantic roles per theme. |
| `assets/styles.css` | Input border #d7d9ce at 1.4:1 fails 1.4.11 | Control edges use `--color-border-strong` (4.6:1). |
| `assets/styles.css` | Muted text #74766e and eyebrow text at 9-11px | Muted text darkened to meet 4.5:1 on tinted surfaces; eyebrows raised to 12px. |
| `assets/styles.css` | Seven shadows incl. a 100px blur and `#000` text shadow | Four elevation levels; the caption text shadow is dropped in favour of a surface label. |
| `assets/styles.css` | Five radii (2, 3, 4, 30px, 50%) | xs 2, sm 4, md 6, lg 8, xl 12, full; 3px merged into 4px. |
| `assets/styles.css` | z-index values 20, 25, 30, 90, 100 | Named layers `--z-*`. |
| `assets/styles.css` | `.btn` was the ink button and `.secondary` the outlined one | System default is secondary; ink is `.btn--primary`. Class renames listed on each component page. |
| `assets/styles.css` | Typographic glyphs used as icons | Replaced by the inline-SVG icon set. |
| `assets/styles.css` | Dark mode absent from the mocks | Dark theme built from roles and verified (128 contrast checks). |
| `admin/sessions` | Button: the mocks' base .btn was the dark filled button and .btn.secondary the outlined one | the mocks' base .btn was the dark filled button and .btn.secondary the outlined one; the system inverts the naming so the default is secondary and the ink button is .btn--primary. Mock button text at 11-12px is raised to 14px, and the 43px minimum height becomes the 44px token. |
| `admin/schedule` | Button group: the mocks used loose .btn siblings and pill .tab buttons for view switches | the mocks used loose .btn siblings and pill .tab buttons for view switches; the system gives them one container with a group label and aria-pressed instead of a visual-only .active class. |
| `marketing/contact` | Link: the mocks' .text-link (bottom border, 12px text) is renamed .link and raised to inherit the body size. Inline links gain a real underline instead of the border so wrapped lines render correctly | the mocks' .text-link (bottom border, 12px text) is renamed .link and raised to inherit the body size. Inline links gain a real underline instead of the border so wrapped lines render correctly. |
| `admin/upload` | Menu: the mocks repeated Edit, Delete and Remove as separate text buttons in every row (.table-actions) | the mocks repeated Edit, Delete and Remove as separate text buttons in every row (.table-actions); the system collapses three or more row actions into one menu and orders the destructive one last. |
| `client/request` | Checkbox: the mocks used a 16px .checkline with 12px text and a 15px .photo-select input | the mocks used a 16px .checkline with 12px text and a 15px .photo-select input; the system uses one 20px box in a 44px row with 14px text. Add-ons gain a card style. |
| `admin/schedule` | Date navigator: the mocks used arrows as raw text characters with a 24px serif label and no live announcement | the mocks used arrows as raw text characters with a 24px serif label and no live announcement; the system uses icon buttons with period-specific names and a polite live region. |
| `admin/upload` | File upload: the mocks' progress bar was 5px high with an unnamed &lt | the mocks' progress bar was 5px high with an unnamed &lt;progress&gt; and the dropzone background used raw colours; the system names every bar, uses tokens and moves the 'prototype' notice out of the component. |
| `marketing/contact` | Form field: the mocks' .field &gt | the mocks' .field &gt; span, .hint, .field-error and .inline-error were four spellings of the same parts; the system has .field__label, .field__hint and .field__error. Labels rise from 11px to 14px and hints from 11px to the 12px caption. |
| `admin/session-editor` | Form layout: the mocks showed field errors inline only and never gathered them | the mocks showed field errors inline only and never gathered them; the system adds the summary. The mocks' .form-section h3 serif 23px rule is kept but moved to the --text-h3 token. |
| `client/request` | Radio group: the mocks used selects for two-way choices and a single .checkline style for radios | the mocks used selects for two-way choices and a single .checkline style for radios; the system uses the same .check row with a fieldset legend, and cards for options with prices. |
| `client/prints` | Select: the mocks styled select globally and let toolbar selects shrink with width:auto | the mocks styled select globally and let toolbar selects shrink with width:auto; the system keeps one .input class and adds .input--sm for toolbar use. |
| `admin/session-editor` | Switch: the mocks used plain checkboxes for live settings | the mocks used plain checkboxes for live settings; the system adds a switch for settings that apply instantly, and keeps checkboxes for choices submitted with a form. |
| `admin/login` | Text field: the mocks styled bare input elements globally with a hard-coded border colour | the mocks styled bare input elements globally with a hard-coded border colour; the system uses a single .input class with a border token that passes 3:1. Mock text at 12px rises to the 14px body-small size (16px on touch). |
| `marketing/contact` | Textarea: the mocks had a global textarea{min-height:110px} with no counter and no label link | the mocks had a global textarea{min-height:110px} with no counter and no label link; the system keeps the three-row minimum, adds rows as the sizing control and documents the counter pattern. |
| `client/gallery` | Breadcrumb: the mocks have no breadcrumb component. They show the hierarchy as an uppercase eyebrow ("Your sessions / October 17, 2026") above the title and a "&larr | the mocks have no breadcrumb component. They show the hierarchy as an uppercase eyebrow ("Your sessions / October 17, 2026") above the title and a "&larr; Back to ..." text link. The system turns the eyebrow into a real &lt;nav&gt;&lt;ol&gt; with links and keeps the back link as the one-level form. The collapsed variant is a system addition. |
| `marketing/home` | Footer: the mocks build the footer in assets/app.js as .public-footer and include an "Explore the prototype" link that is not product content | the mocks build the footer in assets/app.js as .public-footer and include an "Explore the prototype" link that is not product content; the system drops it. The mock's unlabelled &lt;nav&gt; gets aria-label="Footer", the 11px text is raised to 12px, and the multi-column variant is a system addition. |
| `admin/dashboard` | Sidebar: the mocks call the pieces .sidebar, .nav-group, .side-link.active and .sidebar-bottom, with active as a class. The system uses BEM-style .sidebar__* and aria-current="page" so the visual state and the announced state cannot diverge. The mock sidebar is an &lt | the mocks call the pieces .sidebar, .nav-group, .side-link.active and .sidebar-bottom, with active as a class. The system uses BEM-style .sidebar__* and aria-current="page" so the visual state and the announced state cannot diverge. The mock sidebar is an &lt;aside&gt; with no inner &lt;nav&gt;; the system adds one so it is a navigation landmark. The count badge on a link is a system addition; no mock shows one. |
| `marketing/home` | Skip link: each mock shell starts with a bare &lt | each mock shell starts with a bare &lt;a class="skip-link" href="#main"&gt; and its &lt;main&gt; has no tabindex. The system keeps the link, parks it off screen until focus, layers it at --z-skip so it clears the sticky header, and requires tabindex="-1" on main so focus lands in the content instead of only scrolling. |
| `marketing/portfolio` | Tabs: in the mocks the portfolio .tab buttons toggle an .active class and filter a list (data-action="gallery-filter") | in the mocks the portfolio .tab buttons toggle an .active class and filter a list (data-action="gallery-filter"); there is no tab panel and no aria-selected. The system makes tabs a real APG tablist and says plainly that a strip that only filters should be chips or a button group. The mock's .active becomes aria-selected="true"; the underline, count and scrollable variants are system additions that no mock shows. |
| `marketing/home` | Top bar: the mocks build the bar in assets/app.js as .public-header/.public-nav and .app-topbar | the mocks build the bar in assets/app.js as .public-header/.public-nav and .app-topbar; the system names them .site-header/.site-nav and .app-bar. Mock nav text at 12px is raised to 14px, the mobile panel gets aria-expanded kept in sync on every toggle, and its 25px padding becomes spacing tokens. |
| `marketing/about` | Accordion: the mocks show their questions as an open grid of .faq blocks (&lt | the mocks show their questions as an open grid of .faq blocks (&lt;h3&gt; plus &lt;p&gt;) with no collapsing. The system adds the collapsible behaviour with native &lt;details&gt;, keeps the serif heading style, and documents single-open grouping. |
| `admin/dashboard` | Card: the mocks call the container .panel and its tinted form .panel.soft, and style the card's hover only for gallery tiles. The system renames them .card and .card--soft, adds --interactive and --selected as explicit variants, requires a single control in an interactive card, and moves the title/footer styling into __title and __footer | the mocks call the container .panel and its tinted form .panel.soft, and style the card's hover only for gallery tiles. The system renames them .card and .card--soft, adds --interactive and --selected as explicit variants, requires a single control in an interactive card, and moves the title/footer styling into __title and __footer. |
| `admin/session-expired` | Dialog: the mocks open one shared &lt | the mocks open one shared &lt;dialog id="mock-dialog"&gt; with showModal() and render its content from assets/app.js: .dialog-head, .dialog-body, .lightbox-dialog and a text close button "&times;". The system renames them .dialog__head, .dialog__body, .dialog--lg and makes the close an icon button with aria-label. The mock moves initial focus to the first button, which in a confirmation is the close button; the system focuses the safe action instead and adds aria-describedby and aria-modal. Restoring focus to the opener on close is kept from the mock (lastFocus). |
| `marketing/contact` | Divider: the mocks draw separators as bottom borders on specific panels and not as a reusable element. The system gives the rule a name, a vertical form, and the guidance that a rule is the last resort after spacing and headings | the mocks draw separators as bottom borders on specific panels and not as a reusable element. The system gives the rule a name, a vertical form, and the guidance that a rule is the last resort after spacing and headings. |
| `marketing/home` | Layout primitives: the mocks use .container, .grid.two/.three/.four and .split, with hard-coded breakpoints (900px, 1100px) and 20-48px gaps in various rules. The system renames the modifiers to .grid--2/3/4, takes gutters and margins from tokens, and adds .stack and .cluster to replace the mocks' repeated per-block margins and inline flex rules | the mocks use .container, .grid.two/.three/.four and .split, with hard-coded breakpoints (900px, 1100px) and 20-48px gaps in various rules. The system renames the modifiers to .grid--2/3/4, takes gutters and margins from tokens, and adds .stack and .cluster to replace the mocks' repeated per-block margins and inline flex rules. |
| `admin/dashboard` | Avatar: the mocks drew a 30px disc with 10px initials | the mocks drew a 30px disc with 10px initials; the system moves to the 32px size and the 12px caption type. Photos and status dots were not in the mocks and are not part of the component. |
| `admin/sessions` | Badge: the mocks hard-coded three hex pairs (neutral, pending, error) and a 10px label, and a regular expression mapped words such as Scheduled to the pending look while Delivered fell back to neutral | the mocks hard-coded three hex pairs (neutral, pending, error) and a 10px label, and a regular expression mapped words such as Scheduled to the pending look while Delivered fell back to neutral; the system reads status tokens and the 12px caption, and adds explicit success and info tones. The mock's .pending class is now .badge--pending, backed by the warning tokens. |
| `marketing/blog` | Blog card: the mocks used .blog-post with a .blog-post-abstract class | the mocks used .blog-post with a .blog-post-abstract class; the system renames it .blog-card and styles the plain time, h2 and p. The 13px abstract becomes the body-sm token. Search highlighting (mark) is not part of the component. |
| `marketing/portfolio` | Chip: the mocks drew filters as 30px pill .tab buttons with an active class | the mocks drew filters as 30px pill .tab buttons with an active class; the system keeps pills for tabs that switch views and introduces the chip for filters and tags, using aria-pressed instead of a class. Padding moves to token steps. |
| `marketing/quote-summary` | Cost summary: the mocks used .cost-row.total and .discount as stacked classes with 12px text and a separate .serif 32px figure | the mocks used .cost-row.total and .discount as stacked classes with 12px text and a separate .serif 32px figure; the system uses BEM modifiers, the 14px body-sm token and an explicit live region on the total. |
| `client/request` | Description list: the mocks used the same dl markup with 12px terms and a 100px column | the mocks used the same dl markup with 12px terms and a 100px column; the system moves to the 6rem token-sized column and the overline term style, and adds the muted em dash for missing values. |
| `client/request` | Line item: the mocks called this .cart-item with a 130px select and a 76px price | the mocks called this .cart-item with a 130px select and a 76px price; the system renames it .line-item, gives the quantity field a visible label and moves widths to rem. The mock's mobile rule that pushed the last field to column 2 is replaced by the natural wrap. |
| `admin/review` | Photo tile: the mocks used .photo-select and .photo-label with 10 to 11px type and hard-coded white chips | the mocks used .photo-select and .photo-label with 10 to 11px type and hard-coded white chips; the system renames them .photo-tile__select and .photo-tile__label, uses surface tokens and the caption size, and adds an aria-selected outline. The mock's open button (.photo-open) is not yet in the stylesheet. |
| `admin/schedule` | Schedule calendar: the mocks named the column .day, abbreviated the weekday and made each appointment a whole-block link with inline display: block | the mocks named the column .day, abbreviated the weekday and made each appointment a whole-block link with inline display: block; the system renames it .calendar__day, writes the full weekday as a heading, adds a &lt;time&gt; date and puts the link inside the appointment block. The mock's hex appointment fill becomes --color-brand-subtle. |
| `client/gallery` | Selection bar: the mocks hard-coded a count element with an id (#selected-count) and used quiet buttons with no states | the mocks hard-coded a count element with an id (#selected-count) and used quiet buttons with no states; the system makes the count a polite status region and composes the existing button variants. |
| `admin/dashboard` | Stat: the mocks named the figure .number and used a 37px size and a small label | the mocks named the figure .number and used a 37px size and a small label; the system renames it .stat__number and uses the 2xl token with tabular figures. Loading, zero and error versions were added; the mock showed only a value or a literal 0. |
| `marketing/about` | Stepper: the mocks hand-numbered each step in a .number block with a pale-olive hex colour that fails 4.5:1 | the mocks hand-numbered each step in a .number block with a pale-olive hex colour that fails 4.5:1; the system generates the numeral with a CSS counter in brand ink. Current and complete states are new. |
| `admin/sessions` | Table: the mocks styled the bare table, th and td elements globally and set header text at 9px | the mocks styled the bare table, th and td elements globally and set header text at 9px; the system scopes everything to .table and raises header text to the 12px overline token. Sortable headers, selection, dense, loading, empty and error did not exist in the mocks and were added from the component catalog. |
| `admin/upload` | Alert: the mocks named this .notice with .error and .success modifiers and always used role="alert" for errors, including on page load. The system renames it .alert, adds a warning tone, and reserves role="alert" for messages that appear after load | the mocks named this .notice with .error and .success modifiers and always used role="alert" for errors, including on page load. The system renames it .alert, adds a warning tone, and reserves role="alert" for messages that appear after load. |
| `admin/sessions` | Empty state: the mocks' .empty and .symbol classes are renamed .empty-state and .empty-state__symbol, and a compact density is added for use inside cards | the mocks' .empty and .symbol classes are renamed .empty-state and .empty-state__symbol, and a compact density is added for use inside cards. |
| `admin/not-found` | Error page: the mocks used .centered and .system-number | the mocks used .centered and .system-number; the system renames them .error-page and .error-page__code, gives the code aria-hidden, and adds focus management for client-side navigation. |
| `admin/session-editor` | Inline message: the mocks used .small.muted for hints and .error text with no link to the control. The system gives them named classes and requires aria-describedby and aria-invalid | the mocks used .small.muted for hints and .error text with no link to the control. The system gives them named classes and requires aria-describedby and aria-invalid. |
| `admin/upload` | Progress: the mocks drew a bare &lt | the mocks drew a bare &lt;progress&gt; beside the file name with no label link. The system requires an accessible name and a visible status in words. |
| `admin/sessions` | Skeleton: the mocks used one tall skeleton block regardless of what was loading. The system adds text, title and circle variants and requires aria-busy on the region with aria-hidden on the bars | the mocks used one tall skeleton block regardless of what was loading. The system adds text, title and circle variants and requires aria-busy on the region with aria-hidden on the bars. |
| `admin/upload` | Spinner: the mocks' loading state used a bare .spinner with no text. The system requires a text equivalent and a live-region role, and splits sizes into three tokens-based steps | the mocks' loading state used a bare .spinner with no text. The system requires a text equivalent and a live-region role, and splits sizes into three tokens-based steps. |
| `admin/sessions` | Toast: the mocks' toast() cleared the message after 5 seconds with no pause and used one polite region for errors as well. The system lengthens the minimum to 8 seconds, pauses on hover and focus, makes errors persistent with role="alert", and adds a dismiss button | the mocks' toast() cleared the message after 5 seconds with no pause and used one polite region for errors as well. The system lengthens the minimum to 8 seconds, pauses on hover and focus, makes errors persistent with role="alert", and adds a dismiss button. |

## Verification

- `check_contrast.py tokens/tokens.css`: 128 pair checks across light and dark, all pass.
- `check_design_system.py docs/design-system`: exits 0 for 69 pages (12 foundations, 48 components, 8 patterns, index).
- `tokens_to_json.py`: `tokens/tokens.json` regenerated (DTCG).
- Chromium at 390 px: no horizontal page overflow on any of the 69 pages; spot screenshots of the index, colour, button, sidebar, dialog and table pages reviewed in light and dark at 390 and 1440 px.
- Not done: `docs/mocks` still uses its own `assets/styles.css` and was not re-pointed at these tokens (the mock class names differ from the system's; see the drift log). `check_mocks.py` cannot run because `docs/mocks/manifest.json` does not exist.
- Page examples were authored from the mock stylesheet and `app.js`; the mocks render markup with JavaScript, so per-page "Sources" name the mocks each component appears on rather than line-level evidence.
