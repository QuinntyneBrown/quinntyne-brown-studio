# Blog listing HTML mock

Add `docs/mocks/marketing/blog.html`, inspired by the [reference blog](https://www.rickbebbington.com/blog), using the existing mocks’ visual style.

## Implementation

- Use the shared marketing header, footer, container, and preview bar. Add “Blog” to public navigation and the prototype catalog.
- Create a compact “Blog” heading followed by nine sample posts, ordered newest first.
- Follow the reference’s image-first structure: landscape photograph, small date, and prominent title; generous spacing without card borders.
- Use the mocks’ serif headings, muted text, olive accents, and bundled photographs. Write original sample studio titles covering weddings, events, headshots, and family portraits.
- Display three columns on desktop, two on tablet, and one on mobile. Scope new styles to the blog.
- Integrate through the existing shared renderer and catalog; update the mocks README and page count.

## Visual review

Use `playwright-cli` to review desktop, tablet, and mobile screenshots, navigation, image loading, text wrapping, and direct-file opening. Check that the additional navigation item fits existing marketing pages.

No ATDD or automated tests.

## Assumptions and boundaries

Listing only, as selected. Post images and titles are noninteractive previews; no article pages, filters, pagination, or subscription popup. All changes remain within the HTML mocks; no production APIs or application interfaces change.

## Follow-up: article, search, and editor pages

The article page, blog search, and the blog administration screens were imported with the source blog's own dark Inter/Geist Mono interface, so they read as a different product from the studio. Five further mocks bring them into the studio's visual language through the same shared renderer, catalog, and stylesheet, reusing the listing's serif headings, muted text, spacing, tokens, and bundled photographs:

- `marketing/blog-post.html`: featured photograph, date, reading time, abstract, the Markdown body, related stories, and the studio quote strip. States: `missing-photo`, `not-found`.
- `marketing/blog-search.html`: reached from the search field on the listing; keyword search over published stories with highlighted matches and relevance or date ordering. States: `empty`, `no-results`.
- `admin/articles.html`: the record-list pattern used by every other studio list, with abstract and address under each title. States: `empty`, `no-results`; delete dialog.
- `admin/article-editor.html`: the studio editor pattern with a Markdown body and formatting toolbar, a featured-photograph chooser drawn from the media library, live word count and reading time, and save-draft, publish, unpublish, and delete actions. States: `validation`, `save-error`, `unsaved`; publish, featured-image, discard, and delete dialogs.
- `admin/media.html`: the upload dropzone pattern with a photograph grid, search, and delete. States: `empty`, `unsupported-image`, `oversized-image`; delete dialog.

Articles and media now live in the shared seed data, so the listing, post page, search, and administration read and write the same records. Published articles appear publicly; drafts do not.

