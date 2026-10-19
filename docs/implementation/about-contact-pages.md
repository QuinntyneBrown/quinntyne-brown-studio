# About and Contact pages

Implemented on 2026-09-13 under [OD-13](../specs/decisions.md#od-13--server-rendered-about-and-contact-pages) from the three feature designs under [public-presentation](../detailed-designs/public-presentation/): [present-about-page](../detailed-designs/public-presentation/present-about-page/README.md), [receive-contact-inquiries](../detailed-designs/public-presentation/receive-contact-inquiries/README.md), and [serve-search-discoverable-pages](../detailed-designs/public-presentation/serve-search-discoverable-pages/README.md). Each behavior began with a failing acceptance test named for its criterion, and each slice was committed on its own once the suite it touched was green.

## What was delivered

- `/about` and `/contact` are Razor pages in `QuinntyneBrownStudio.Api` under the marketing shell the blog listing already used. `GetAboutPageHandler` and `GetContactPageHandler` compose published `MarketingContent` (or the prototype defaults), the active photographers, the enabled studios with the base studio first, the studio details, and the active advance-booking rule; each page carries a canonical link, description, Open Graph metadata, JSON-LD, and a weak ETag that answers 304 until a record it read changes. `/about/` and `/contact/` redirect with 308.
- `Inquiry` and `StudioDetails` are document records in the studio store; no migration was needed. `SubmitInquiryHandler` assigns `QB-IN-<n>` references (unique keys at the SQL layer) and queues a protected `Email` job through the new `IEmailQueue` port in the same unit of work when a studio address is configured. `InquiriesController` and `StudioDetailsController` expose the administrator API; the rate-limit policy `contact-inquiries` allows five posts per client address in ten minutes.
- The administration application gains `/studio-details` (a settings-page kind) and `/inquiries` (`InquiryInbox` with the `InquiryDetails` region). The marketing shell links About, Blog, Contact and Client login as full navigations, and the client-rendered contact route is retired.
- `/robots.txt` advertises `/sitemap.xml`, which lists the marketing home, About, Contact, the blog and every published article; `MarketingContent.PublishedAt` and `PublicGallery.PublishedAt` record publication times. The gateway definition, the release health check, and the deployed smoke cover the new addresses.
- The design system publishes the `detail-list` component and the `about-page`, `contact-page` and `inquiry-review` patterns before the applications consume them.
- Browser acceptance runs in Chromium only, as `AGENTS.md` now records; the Firefox and WebKit projects were removed from every Playwright configuration during this work.

## Decisions taken at implementation

- Team cards omit the working hours the prototype shows: `Photographer` carries no hours and no value is invented (confirmed with the studio). `Photographer.CreatedAt` orders the team so the founder is deterministic on SQL Server, whose record listing has no natural order.
- The notification job is deduplicated by its job identifier; one job exists per inquiry, so the effect the design describes (a retried delivery sends one message) holds without changing `JobProcessor`.
- A filled honeypot field answers the same 303 as a real submission and stores nothing, matching the prototype's silent reset.
- The studio details screen keeps the settings page's eyebrow and title convention ("Studio details") rather than the prototype's "Where people can reach you." heading; the field labels, hints, limits and aside copy follow the prototype.
- The marketing footer keeps the existing "Search articles" link beside the five designed links so the blog evidence for AC-L2-070-09 stays valid.

## Verification recorded on 2026-09-13

| Check | Result |
| --- | --- |
| Backend acceptance (`dotnet test backend/QuinntyneBrownStudio.slnx -c Release`) | 176 passed with `--filter FullyQualifiedName!~LocalDbAcceptanceTests`; the four `LocalDbAcceptanceTests` cases fail on this machine with `FileNotFoundException: Microsoft.Extensions.FileProviders.Composite` before and after this work (verified on `main`), an environment defect unrelated to the change |
| Design system (`npm test` in `design-system`) | Validator passed (26 components, 38 pattern states, 4 dialog scenarios); 36 browser executions passed in Chromium at the three widths |
| Angular builds (`npm run build:libs`, `npm run build:apps`) | Passed |
| Application acceptance (`npm test` in `e2e`) | 234 passed across Chromium mobile, tablet and desktop, including the new contact-administration and marketing-navigation specs |
| Packaged LocalDB workflow (`scripts/smoke-platform.ps1`) | 7 passed in Chromium: the empty About and Contact states at 390, 768 and 1440 px, the configured pages reached from the marketing shell with keyboard operation, submissions with and without script reviewed in the inbox, the blog workflow at three widths, and the LocalDB platform workflow |
| Deployment tests (`python3 -m unittest discover -s backend/tests/deployment`, under WSL Ubuntu) | 20 passed, including the gateway route and health-check cases |
| `python scripts/verify-architecture.py`, `python docs/detailed-designs/verify.py` | Passed |

The [acceptance register](../detailed-designs/acceptance.md) links every About and Contact criterion to these tests. AC-L2-076-04 and AC-L2-077-02 stay Partial until the deployed browser smoke runs against the next activated release; the gateway definition and release health check are covered by the deployment tests above.
