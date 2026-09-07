import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { SettingsPage } from "../page-objects/settings-page";

// Reckoner L2-031,041,082: authenticated Studio administration owns a minted token and mounts the installed widgets.
test("Given a Studio administrator When quote availability opens Then the Reckoner weekday editor is available", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("quote-availability");
  await settings.quoteAvailability();
});

test("Given a token renewal failure When access recovers Then the unsaved weekday draft can be restored and saved", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.installClock();
  await settings.open("quote-availability");
  await settings.quoteAvailability();
  await settings.check("Monday", false);
  fixture.failures.set("reckoner-admin.mint", {
    status: 503,
    message: "Connection interrupted.",
  });
  await settings.reachRenewal();
  await settings.message("Connection interrupted.");
  await settings.checked("Monday", false);
  fixture.failures.delete("reckoner-admin.mint");
  fixture.operations.set("reckoner-admin.mint", () => ({
    apiBaseUrl: fixture.reckoner.origin,
    adminToken: "at_" + "r".repeat(43),
    expiresAt: new Date(Date.now() + 115 * 60_000).toISOString(),
  }));
  await settings.click("Retry quote administration");
  await settings.click("Restore unsaved changes");
  await settings.checked("Monday", false);
  await settings.saveQuote("availability");
  await settings.message("Saved successfully.");
  expect(fixture.reckoner.availability.weekdays.monday).toBe(false);
  await settings.privateTokenOnlyInMemory();
});

test("Given administrator access is lost When renewal is denied Then private editors are removed", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.installClock();
  await settings.open("quote-availability");
  await settings.quoteAvailability();
  fixture.failures.set("reckoner-admin.mint", {
    status: 403,
    message: "Administrator access required.",
  });
  await settings.reachRenewal();
  await settings.message("Administrator access required.");
  await settings.noQuoteEditors();
  await settings.privateTokenOnlyInMemory();
});

test("Given a quote editor When signing out Then the token and editor leave the page", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("quote-availability");
  await settings.quoteAvailability();
  await settings.click("Sign out");
  await settings.noQuoteEditors();
  await settings.privateTokenOnlyInMemory();
});

test("Given appearance settings When opening Studio administration Then theme and display editors are available", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("quote-appearance");
  await settings.value("Title", "A little clarity, before we begin.");
  await settings.privateTokenOnlyInMemory();
});
