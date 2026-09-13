import { test, expect } from "@playwright/test";
import { AccountPage } from "../page-objects/account-page";
import { FullstackFixture } from "../page-objects/fullstack-fixture";
import { InquiryInboxPage } from "../page-objects/inquiry-inbox-page";
import { PublicSitePage } from "../page-objects/public-site-page";
import { AboutPage } from "../page-objects/marketing/about.page";
import { ContactPage } from "../page-objects/marketing/contact.page";

// AC-L2-071-01, AC-L2-071-04, AC-L2-072-01 through AC-L2-072-03, AC-L2-073-01, AC-L2-073-02,
// AC-L2-073-05, AC-L2-074-01 and AC-L2-076-02 in Chromium against the packaged API and the
// isolated LocalDB database, at 390, 768 and 1440 CSS pixels.
const origin = process.env.QBS_SMOKE_ORIGIN ?? "https://localhost:7453";
const widths = [390, 768, 1440];

function credentials() {
  const email = process.env.Bootstrap__Email;
  const password = process.env.Bootstrap__Password;
  if (!email || !password) throw new Error("Supply isolated smoke administrator credentials.");
  return { email, password };
}

// Given nothing configured yet, when a visitor opens the pages at every width, then the
// About team and hero show their placeholders and the Contact page omits unset details and
// shows the studio-spaces empty state, without clipping or horizontal overflow.
test("AC-L2-071-04 AC-L2-072-02 the empty About and Contact states render at every width", async ({
  browser,
}) => {
  for (const width of widths) {
    const context = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width, height: 900 } });
    try {
      const page = await context.newPage();
      const about = new AboutPage(page, origin);
      expect((await about.open())?.status()).toBe(200);
      await about.heading("Photographs that feel like you.");
      await about.emptyTeam();
      await about.heroPlaceholder();
      await about.sections();
      await about.shell.assertNoOverflow();
      const contact = new ContactPage(page, origin);
      expect((await contact.open())?.status()).toBe(200);
      await contact.emptyStudios();
      await contact.onLocation();
      for (const label of ["Email", "Phone", "Hours", "Replies"]) await contact.noDetail(label);
      await contact.detail("Studio", "On location, by appointment");
      await contact.formVisible();
      await contact.shell.assertNoOverflow();
    } finally {
      await context.close();
    }
  }
});

// Given published copy, configured details, a base studio and active photographers, when a
// visitor follows the marketing shell to the pages at every width, then the API-rendered pages
// show the configured content, mark themselves current, keep the overlay menu operable by
// pointer and keyboard, and every control is reachable with visible focus.
test("AC-L2-071-01 AC-L2-072-01 AC-L2-072-03 AC-L2-076-02 configured pages are reached from the shell and usable by keyboard", async ({
  page,
  context,
  browser,
}, info) => {
  const { email, password } = credentials();
  const account = new AccountPage(page, origin);
  await account.open();
  await account.login(email, password);
  await account.heading("Sessions");
  const fixture = new FullstackFixture(context, origin);
  const about = await fixture.read("admin/content");
  if (!about.some((content: { pageKey: string }) => content.pageKey === "about")) {
    await fixture.save("PUT", "admin/content/about", {
      heading: "Photographs with room to breathe.",
      body: "A published introduction.",
      publish: true,
      expectedVersion: 0,
    });
    await fixture.save("PUT", "admin/content/contact", {
      heading: "Something beautiful starts with hello.",
      body: "Tell us what you have in mind.",
      publish: true,
      expectedVersion: 0,
    });
    await fixture.save("PUT", "admin/studio-details", {
      email: "hello@example.test",
      phone: "416-555-0100",
      hours: "Monday – Saturday · 09:00 – 18:00",
      replyNote: "Within two working days",
      expectedVersion: 0,
    });
    await fixture.save("POST", "admin/studios", {
      name: "Daylight Studio",
      hourlyFee: "95",
      enabled: true,
      isBase: true,
      resolvedAddress: { label: "120 Sample Street, Toronto", latitude: 43.65, longitude: -79.38 },
    });
    await fixture.save("POST", "admin/photographers", { name: "Quinntyne Brown", active: true });
    await fixture.save("POST", "admin/photographers", { name: "Mara Adeyemi", active: true });
  }

  for (const width of widths) {
    const visitor = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width, height: 900 } });
    try {
      const visitorPage = await visitor.newPage();
      const site = new PublicSitePage(visitorPage, origin);
      await site.open();
      await site.loaded();
      await site.openMenuFromKeyboard();
      await site.navigate("About");

      const aboutPage = new AboutPage(visitorPage, origin);
      await aboutPage.shell.canonical(origin, "/about");
      await aboutPage.heading("Photographs with room to breathe.");
      await aboutPage.teamMember("Quinntyne Brown", "Founder & lead photographer");
      await aboutPage.teamMember("Mara Adeyemi", "Photographer");
      await aboutPage.shell.current("About", "/about");
      await aboutPage.shell.footerLink("Get in touch", "/contact");
      await aboutPage.shell.assertNoOverflow();
      await aboutPage.shell.focusVisibleOnLink(/Say hello/);
      if (width <= 780) {
        await aboutPage.shell.assertOverlayMenu();
        await aboutPage.shell.assertKeyboardOverlayMenu();
      }
      await aboutPage.capture(info.outputPath(`about-${width}.png`));

      const contact = new ContactPage(visitorPage, origin);
      expect((await contact.open())?.status()).toBe(200);
      await contact.shell.canonical(origin, "/contact");
      await contact.shell.current("Contact", "/contact");
      await contact.heading("Something beautiful starts with hello.");
      await contact.detail("Email", "hello@example.test");
      await contact.detail("Phone", "416-555-0100");
      await contact.detail("Studio", "Daylight Studio");
      await contact.detail("Hours", "Monday – Saturday · 09:00 – 18:00");
      await contact.detail("Replies", "Within two working days");
      await contact.studio("Daylight Studio");
      await contact.onLocation();
      await contact.shell.assertNoOverflow();
      await contact.keyboardReachesEveryControl();
      await contact.capture(info.outputPath(`contact-${width}.png`));
    } finally {
      await visitor.close();
    }
  }
});

// Given the form, when a visitor sends an invalid and then a valid message, with and without
// script, then errors appear beside their fields with the entries kept, each valid message is
// confirmed with its reference, and the administrator inbox lists them newest first and records a
// review. The four posts stay within the five-per-address window.
test("AC-L2-073-01 AC-L2-073-02 AC-L2-073-05 AC-L2-074-01 messages are sent with and without script and reviewed in the inbox", async ({
  page,
  browser,
}, info) => {
  const { email, password } = credentials();
  const references: string[] = [];

  const visitor = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1440, height: 900 } });
  try {
    const contact = new ContactPage(await visitor.newPage(), origin);
    await contact.open();
    await contact.fill({ name: "Priya Raman", email: "not-an-address", phone: "416-555-0166", message: "A small September wedding." });
    await contact.interest("Wedding");
    await contact.consent();
    await contact.send();
    await contact.fieldError("email");
    await contact.value("name", "Priya Raman");
    await contact.value("phone", "416-555-0166");
    await contact.value("message", "A small September wedding.");
    await contact.fill({ email: "priya@example.test" });
    await contact.consent();
    await contact.send();
    references.push(await contact.confirmation());
    await contact.capture(info.outputPath("contact-sent.png"));
  } finally {
    await visitor.close();
  }

  const scriptless = await browser.newContext({
    ignoreHTTPSErrors: true,
    javaScriptEnabled: false,
    viewport: { width: 1440, height: 900 },
  });
  try {
    const contact = new ContactPage(await scriptless.newPage(), origin);
    await contact.open();
    await contact.fill({ name: "Daniel Okafor", email: "daniel@example.test", message: "Two headshot looks for a new role." });
    await contact.interest("Headshots");
    await contact.send();
    await contact.fieldError("consent");
    await contact.value("name", "Daniel Okafor");
    await contact.value("message", "Two headshot looks for a new role.");
    await contact.consent();
    await contact.send();
    references.push(await contact.confirmation());
  } finally {
    await scriptless.close();
  }
  expect(new Set(references).size).toBe(2);

  const account = new AccountPage(page, origin);
  await account.open();
  await account.login(email, password);
  await account.heading("Sessions");
  const inbox = new InquiryInboxPage(page, origin + "/admin");
  await inbox.open();
  await inbox.rowOrder([references[1], references[0]]);
  await inbox.row(references[1], "Daniel Okafor");
  await inbox.row(references[1], "Headshots");
  await inbox.openInquiry(references[0]);
  await inbox.detail("Reference", references[0]);
  await inbox.detail("Name", "Priya Raman");
  await inbox.detail("Message", "A small September wedding.");
  await inbox.click("Mark reviewed");
  await inbox.message("Inquiry marked reviewed.");
  await inbox.reviewDisabled();
  await inbox.filter("Reviewed");
  await inbox.inquiryCount(1);
  await page.screenshot({ path: info.outputPath("admin-inquiries.png"), fullPage: true });
});
