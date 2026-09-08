import { test } from "@playwright/test";
import { PublicSitePage } from "../page-objects/public-site-page";
import { QuotePage } from "../page-objects/quote-page";
import { StudioFixture } from "../page-objects/studio-fixture";

test.beforeEach(async ({ context }) => {
  const fixture = new StudioFixture();
  fixture.operations.set("quote.getStudios", () => []);
  await fixture.install(context);
});

// Given a scrolled home page, when the lower session invitation is activated,
// then the quote page starts at (0, 0) with its heading in the viewport.
for (const input of ["pointer", "keyboard"]) {
  test(`AC-L2-070-01 home-to-quote navigation starts at the top using ${input}`, async ({
    page,
  }) => {
    const website = new PublicSitePage(page);
    const quote = new QuotePage(page);
    await website.open();
    await website.heading("Photography with feeling.");
    await website.revealSessionInvitation();
    await website.followSessionInvitation(input === "keyboard");
    await quote.startsAtTop();
  });
}

// Given independently scrolled home and quote pages, when Back and Forward are
// used, then each history entry restores its own saved scroll position.
test("AC-L2-070-02 browser history restores each public page's scroll position", async ({
  page,
}) => {
  const website = new PublicSitePage(page);
  const quote = new QuotePage(page);
  await website.open();
  await website.heading("Photography with feeling.");
  const homePosition = await website.revealSessionInvitation();
  await website.followSessionInvitation();
  await quote.startsAtTop();
  const quotePosition = await quote.scrollDown();
  await website.backToHome(homePosition);
  await quote.forwardToQuote(quotePosition);
});

// Given a public page, when Skip to content is activated with the keyboard,
// then the main content receives focus and is scrolled into view.
test("AC-L2-070-03 skip to content preserves fragment navigation", async ({
  page,
}) => {
  const website = new PublicSitePage(page);
  await website.open();
  await website.heading("Photography with feeling.");
  await website.skipToContent();
});
