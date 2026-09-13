import { test } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { PublicSitePage } from "../page-objects/public-site-page";
import { QuotePage } from "../page-objects/quote-page";

// The relaunch gate (OD-14): while the studio is coming soon, a visitor who is not signed in is
// sent from every client-rendered marketing page to the blog; the quote calculator and the
// server-rendered pages stay open, and a signed-in account sees the studio as before.

const gatedPaths = ["", "portfolio", "services", "prints", "promotions", "galleries/spring"];

// Given the gate applies to an anonymous visitor, when they open any client-rendered marketing
// page, then the browser leaves for the blog instead of showing the page.
test("P11 AC-L2-078-02 a gated visitor is sent from every client-rendered page to the blog", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.authenticated = false;
  fixture.comingSoon = true;
  await fixture.install(context);
  const site = new PublicSitePage(page);
  await site.serveBlog(context);
  for (const path of gatedPaths) {
    await site.open(path);
    await site.arrivedAtBlog();
  }
});

// Given the gate applies, when the visitor opens the quote calculator, then it works as before
// and its shell offers only the pages that exist: About, Blog, Contact, the quote and client login.
test("P11 AC-L2-078-02 AC-L2-078-03 the quote calculator stays open and the shell hides the gated pages", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.authenticated = false;
  fixture.comingSoon = true;
  await fixture.install(context);
  const site = new PublicSitePage(page);
  await site.serveBlog(context);
  const quote = new QuotePage(page);
  await quote.openLive();
  await site.stayedOn("/quote");
  await site.openMenuFromKeyboard();
  await site.navigationOrder(["About", "Blog", "Contact", "Plan a session ↗", "Client login"]);
  for (const label of ["Portfolio", "Services", "Prints", "Packages"])
    await site.navigationAbsent(label);
  await site.brandLink("/blog");
  await site.footerLinkAbsent("Our work");
  for (const [label, href] of [
    ["About the studio", "/about"],
    ["Get in touch", "/contact"],
    ["Blog", "/blog"],
    ["Client access", "/client/login"],
  ])
    await site.footerLink(label, href);
  await site.closeMenuFromKeyboard();
});

// Given the gate is configured but the visitor is signed in, when they open the marketing pages,
// then every page opens and the shell offers every link, exactly as before the relaunch.
test("P11 AC-L2-078-02 AC-L2-078-03 a signed-in visitor sees the whole studio", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.authenticated = true;
  // The API reports the gate for the requester: a signed-in account is never gated.
  fixture.comingSoon = false;
  await fixture.install(context);
  const site = new PublicSitePage(page);
  await site.open();
  await site.heading("Photography with feeling.");
  await site.stayedOn("/");
  await site.open("portfolio");
  await site.heading("Selected work");
  await site.stayedOn("/portfolio");
  await site.openMenuFromKeyboard();
  await site.navigationOrder([
    "Portfolio",
    "Services",
    "About",
    "Blog",
    "Prints",
    "Packages",
    "Contact",
    "Plan a session ↗",
    "Client login",
  ]);
  await site.brandLink("/");
  await site.footerLink("Our work", "/portfolio");
  await site.closeMenuFromKeyboard();
});

// Given the launch state cannot be read, when a visitor opens a marketing page, then nobody is
// gated: the blog the gate leads to comes from the same API, so there is nowhere better to go.
test("P11 AC-L2-078-02 an unavailable launch state gates nobody", async ({ page, context }) => {
  const fixture = new StudioFixture();
  fixture.authenticated = false;
  fixture.failures.set("launch.state", { status: 503, message: "Unavailable." });
  await fixture.install(context);
  const site = new PublicSitePage(page);
  await site.open("portfolio");
  await site.heading("Selected work");
  await site.stayedOn("/portfolio");
});
