# Quinntyne Brown Studio — interactive HTML mocks

Open **index.html** in a modern browser. No installation, backend, build, or internet connection is required. The index links all 63 pages, create/edit variants, error and empty states, and dialog previews.

For consistent shared browser storage across all pages, you can also serve this directory with a static server, for example `npx --yes http-server docs/mocks`, then open the displayed address. This is optional; direct file opening is supported. Some browsers isolate or disable storage on file URLs.

## Exploring the prototype

- **Marketing:** Start at `marketing/home.html`. Browse photographs, services, print prices, and packages. The quote calculator itemizes photography, mileage, rental units, parking, meals, assistants, and studio hire.
- **Admin:** Start at `admin/dashboard.html`. Manage sessions, photographers, schedules, equipment, studios, vendors, pricing, discounts, galleries, content, and packages. Create/edit changes persist in this browser and update the public pages. Uploads and Azure photo suggestions are simulated.
- **Client:** Start at `client/galleries.html`. Select photographs, create and edit albums, choose print sizes and quantities, and send a simulated print request. Requests appear in the client request history.
- **Access:** Any syntactically valid email and a password of at least eight characters works. Do not enter real credentials. No credentials are saved or sent anywhere.
- **States:** Use the bottom preview selector or index links. `?state=save-error` and similar links reproduce failures; `?dialog=photo` and other dialog links open the relevant dialog. For recoverable submit/AI failures, the first attempt fails and a retry succeeds. Required-field and real input errors must be corrected.
- **Reset:** Use “Reset demo” in the bottom bar and confirm to restore the original sample records. Storage falls back to page memory if disabled; in that case edits do not carry across navigation.

## Defaults and boundaries

All people, prices, locations, sessions, and requests are fictional. “Failing portraits” in the brief is interpreted as **family portraits**. Currency is CAD. Quotes and package prices are estimates subject to consultation; taxes, shipping, and final venue/travel details are confirmed by the studio. There is no payment flow.

The largest eligible discount applies, without stacking. Sample rules are 10% for at least 90 days in advance, 8% for Tuesdays, and 12% for code `HELLO12`. The advance-day threshold, percentages, weekday, codes, and enabled status are editable. `EXPIRED` and `NOTACODE` demonstrate code errors. Code eligibility can also be previewed from the state selector.

Active photographers appear in quoting. In this prototype, a session or unavailable block reserves that photographer’s whole date, and “All photographers” blocks the date for everybody. Calendar blocks include times for editing and conflict validation. Typical working-hours text is informational. Availability does not constitute a reservation.

Equipment inventory tracks individual rental rates; the quote calculator uses the configurable standard rental-unit rate in **Quote rates**. Studios contribute their own hourly fee. Published gallery/content/package changes appear publicly; draft content does not replace the public copy. The first selected gallery photograph is its cover.

Uploads accept common photos and camera RAW formats, with a 250 MB per-file demo limit. File names and sizes are inspected locally; file contents are not uploaded or persisted. “Try a sample batch” demonstrates four successes and two retryable failures. AI recommendations use fixed sample reasons and require human selection. No Azure service is called.

Session photographs are representative fixed sample collections. Admin session records and marketing galleries are editable, while the client gallery is a fictional signed-in client’s prepared example. New session records do not ingest actual photographs. Password resets, contact requests, and print requests never send email. System error pages do not enforce access control.

This deliverable is the approved standalone HTML prototype and remains the visual reference for `L2-066`. The production Angular applications now live in [`frontend/`](../../frontend/README.md) and the design system is the separate product in [`design-system/`](../../design-system/README.md); this folder is not built, imported, or deployed by either.

## Relationship to the requirements

The prototype predates parts of the approved requirement set, so three screens differ from the platform:

- `admin/dashboard.html` ("Studio overview") has no requirement or feature design behind it. The delivered administration opens the session list instead, and the navigation entry points there.
- Administrative print-request review (`L2-036`, `L2-064`) has no prototype screen. The delivered inbox reuses the record-list and detail patterns shown throughout this catalog.
- Client invitation management (`L2-032`, `L2-062`) has no prototype screen. The delivered screen reuses the form patterns shown here.

Every other catalogued screen maps to a requirement and a feature design. The visual language in this folder stays the reference for `L2-066` in all three cases, and the [design system](../../design-system/README.md) publishes the classes the platform ships.

## Organization

- `index.html`: Complete page, state, and dialog catalog.
- `marketing/`, `admin/`, `client/`: Individual HTML page entry points.
- `assets/catalog.js`: Page and state coverage manifest.
- `assets/data.js`: Shared seed records and photograph metadata.
- `assets/app.js`: Shared rendering and interactive behavior; no modules or fetch needed for file URLs.
- `assets/styles.css`: Shared responsive layouts and visual language, with system fonts.
- `assets/photos/`: Bundled stock photography and license/source attribution. These images are not the studio’s work.
- `tests/verify.cjs`: Browser checks for page/state coverage, workflows, storage, responsive layout, and direct-file operation.

## Verification

Run the browser suite with Node.js, Playwright, and Chrome installed:

```powershell
node docs/mocks/tests/verify.cjs
```

If Playwright is installed outside the repository, set `PLAYWRIGHT_MODULE` to its absolute package directory first. `BROWSER_CHANNEL` optionally selects another installed Playwright browser channel, such as `msedge`. No test dependency or package manifest is required in this prototype folder.

The suite writes a report and desktop/mobile screenshots into `.verification/`. It uses an ephemeral local HTTP server and isolated browser contexts, and does not change the user’s normal browser demo data.
