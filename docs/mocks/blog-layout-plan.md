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
