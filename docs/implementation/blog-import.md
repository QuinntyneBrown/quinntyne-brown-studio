# Blog import

Source: `C:/projects/Blog`, commit `a432cf54965f1b9eaf1524f315739826ba1a5e07` (MIT, Quinntyne Brown). Source files are copied, not linked to the source checkout. Article entities, repositories, commands, queries, validation, Markdown sanitization, image processing, SEO generation, Razor layouts, and editor pages are reused.

User-approved exceptions preserve the imported Razor UI and its existing styles instead of converting it to Angular or recreating it in the design system. These exceptions apply only to the imported blog. Existing studio architecture and conventions continue elsewhere.

Source users, uploaded images, databases, secrets, newsletters, events, subscriptions, and About are not imported. Studio Identity protects the editor. Tables are added to the existing studio database through explicit migrations.

## Routes and operation

Public routes are `/blog/`, `/blog/articles/{slug}`, and `/blog/search?q=…`. The editor at `/blog/admin/articles` and media library at `/blog/admin/digital-assets` use studio Identity. Anonymous editor requests return through `/admin/login`; client accounts cannot use the editor.

Article and media APIs live under `/blog/api/`. Cookie-authenticated mutations require the existing `X-XSRF-TOKEN` antiforgery header. Article updates retain Blog's ETag/If-Match contract. RSS, Atom, JSON feed, sitemap, and llms.txt live under `/blog`; root `/robots.txt` advertises the sitemap. `PublicOrigin` supplies absolute URLs.

Apply `AddBlogArticles` through the existing explicit API migration command before starting API and worker. It adds `Articles` and `DigitalAssets`, including an Identity foreign key, without changing existing studio rows or seeding content. Development uses the shared LocalDB connection; production uses the shared Azure SQL connection and managed identity.

`Blog:StoragePath` must be an absolute persistent directory writable by the API account. The development startup script uses `.artifacts/blog-media`; smoke runs use a per-database directory. Other development launches default to the current user's local application-data directory under `QuinntyneBrownStudio/BlogMedia`. The Ubuntu service uses `/var/lib/studio/blog-media`, outside immutable releases. Manual production launches require this setting.

Back up blog media together with SQL. Restore a matching database and media set while the API is stopped, preserve service-account permissions, then verify an article and image after startup. Retain media during application rollback. Uploads are not deployment artifacts.

The imported processor generates WebP and optionally AVIF using `avifenc` on PATH. Failed optional AVIF encoding removes partial files and falls back to WebP or the original; encoder absence does not prevent uploads or viewing images.

## Integration changes

Existing types were relocated into studio layers and split into one file per type. Required edits cover authentication, antiforgery, route prefixes, media URLs, source mobile layout defects, inline click handlers incompatible with nonce CSP, duplicate featured-image fields, and safe error rendering. Failed validation retains entered article text.

MediatR remains 12.5.0. HtmlSanitizer was updated to 9.2.1039 because the source package and its AngleSharp dependency failed NuGet vulnerability checks: [sanitizer advisory](https://github.com/advisories/GHSA-j92c-7v7g-gj3f), [parser advisory](https://github.com/advisories/GHSA-pgww-w46g-26qg).

## Verification

The initial three acceptance tests failed with missing-route responses before implementation.

- [API acceptance](../../backend/tests/QuinntyneBrownStudio.AcceptanceTests/BlogAcceptanceTests.cs): lifecycle, privacy, search, feeds, conflicts, validation, antiforgery, authorization, and editor rendering.
- [Media acceptance](../../backend/tests/QuinntyneBrownStudio.AcceptanceTests/BlogMediaAcceptanceTests.cs): upload, WebP, optional AVIF fallback, references, and corrupt-upload cleanup.
- [LocalDB acceptance](../../backend/tests/QuinntyneBrownStudio.AcceptanceTests/BlogPersistenceAcceptanceTests.cs): migration, reopened persistence, and SQL concurrency.
- [Browser acceptance](../../e2e/integration/blog.spec.ts): imported page objects drive real login, upload, publication, viewing, unpublication, and deletion at 390, 768, and 1440 pixels. The full-stack configuration includes Chromium, Firefox, and WebKit for the blog.

Run `dotnet test backend/QuinntyneBrownStudio.slnx`, frontend library/application builds, e2e type checking, and `scripts/smoke-platform.ps1`. Browser traces and screenshots are generated under `.artifacts/platform/fullstack-browser`. Linux release tests run under Linux or WSL.

Verified on 2026-09-10:

- Release backend suite: 141 passed, including 11 blog cases; `.artifacts/blog-verification/final-backend.trx`.
- Packaged LocalDB browser suite: 10 passed (nine blog viewport/browser combinations and the existing studio workflow); `.artifacts/platform/fullstack-results.json`.
- Existing Angular acceptance scenarios: 204 passed across Chromium mobile, tablet, and desktop; `e2e/test-results/results.json`.
- All frontend library/application builds and e2e TypeScript checking passed.
- Linux deployment tests: 13 passed under WSL; architecture and documentation verification passed.

Persistence evidence combines an actual LocalDB migration/reopen test and the existing runtime startup checks with review of the upload directory outside release folders. Mobile editor and public article screenshots were visually reviewed after the source layout corrections.

No source data was migrated and no live deployment was performed.
