import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { PublicSitePage } from "../page-objects/public-site-page";

// Given the marketing application shell, when a visitor reads the navigation and the footer,
// then About and Contact are offered alongside Portfolio, Services, Blog, Prints, Packages,
// the quote call to action and client login, and the mobile menu opens from the keyboard.
test("P10 AC-L2-076-02 the marketing shell offers About and Contact in its navigation and footer", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const site = new PublicSitePage(page);
  await site.open();
  await site.loaded();
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
  for (const [label, href] of [
    ["About", "/about"],
    ["Blog", "/blog"],
    ["Contact", "/contact"],
    ["Client login", "/client/login"],
  ])
    await site.navigationLink(label, href);
  for (const [label, href] of [
    ["About the studio", "/about"],
    ["Get in touch", "/contact"],
    ["Our work", "/portfolio"],
    ["Blog", "/blog"],
    ["Client access", "/client/login"],
  ])
    await site.footerLink(label, href);
  await site.closeMenuFromKeyboard();
});

// Given the About and Contact pages are rendered by the API, when a visitor follows either
// link from the marketing shell, then the browser performs a full navigation to that address
// rather than a client-side route change.
test("P10 AC-L2-076-02 About and Contact links load the server-rendered pages", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  for (const path of ["/about", "/contact"]) {
    await context.route(`**${path}`, (route) =>
      route.fulfill({
        status: 200,
        contentType: "text/html",
        body: `<html><body><h1>Server rendered ${path}</h1></body></html>`,
      }),
    );
  }
  const site = new PublicSitePage(page);
  for (const [label, path] of [
    ["About", "/about"],
    ["Contact", "/contact"],
  ]) {
    await site.open();
    await site.loaded();
    await site.openMenuFromKeyboard();
    await site.navigate(label);
    await expect(
      page.getByRole("heading", { name: `Server rendered ${path}` }),
    ).toBeVisible();
    expect(new URL(page.url()).pathname).toBe(path);
  }
});
