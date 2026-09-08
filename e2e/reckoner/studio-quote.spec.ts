import { test } from "@playwright/test";
import { QuotePage } from "../page-objects/quote-page";
import {
  provisionReckoner,
  provisionReckonerWithAdministration,
} from "../fixtures/reckoner-api-fixture";
import { StudioFixture } from "../page-objects/studio-fixture";
import { SettingsPage } from "../page-objects/settings-page";

// Reckoner L2-015,028,072,082,084: actual packed library + Studio route + Reckoner API + controlled Azure HTTP.
test("Given Studio administration and a visitor quote When rates, studio fees, discounts and availability are saved Then the next quote uses each committed Reckoner change (L2-082.7)", async ({
  page,
  browser,
}) => {
  const seeded = await provisionReckonerWithAdministration();
  const adminContext = await browser.newContext({
    viewport: page.viewportSize()!,
  });
  try {
    const fixture = new StudioFixture();
    await fixture.installWithReckoner(adminContext, seeded.adminSession);
    const settings = new SettingsPage(await adminContext.newPage());
    const quote = new QuotePage(page);
    await quote.openReckoner(seeded.publicOptions);
    await quote.completeSession();
    await quote.amount("1000.00");
    await settings.open("rates");
    await settings.fill("Wedding rate (CAD)", "300");
    await settings.saveQuote("services");
    await settings.message("Saved successfully.");
    await quote.fill("End time", "15:00");
    await quote.amount("1500.00");
    await settings.open("studios");
    await settings.fill("Studio 1 hourly fee (CAD)", "100");
    await settings.saveQuote("locations");
    await settings.message("Saved successfully.");
    await quote.addNamedLocation("123 King Street West, Toronto");
    await quote.chooseStudio(1, "The daylight loft");
    await quote.fillLocation(1, "Studio hours", "2");
    await quote.line("Studio time, location 1", "200.00");
    await quote.amount("1730.86");
    await settings.open("discounts");
    await settings.fill("Code 1 percentage", "25");
    await settings.saveQuote("discounts");
    await settings.message("Saved successfully.");
    await quote.applyCode("PHOTO10");
    await quote.amount("1298.14");
    await settings.open("quote-availability");
    await settings.check("Tuesday", false);
    await settings.saveQuote("availability");
    await settings.message("Saved successfully.");
    fixture.rates.serviceRates = { wedding: 1 };
    fixture.discounts.advanceRule = {
      enabled: true,
      percentage: 99,
      threshold: 0,
    };
    await quote.fill("Lunches", "1");
    await quote.amount("1315.02");
    await quote.message("This date is not currently available.");
    await quote.expectReckonerPrivacy();
    await settings.privateTokenOnlyInMemory();
  } finally {
    await adminContext.close();
  }
});

test("Given a provisioned Studio calculator When the golden session is entered Then authoritative totals and private transport hold", async ({
  page,
}, testInfo) => {
  const quote = new QuotePage(page);
  await quote.openReckoner(await provisionReckoner());
  await quote.reckonerRegion();
  await quote.completeSession();
  await quote.fill("Session date", "2026-12-19");
  await quote.addNamedLocation("123 King Street West, Toronto");
  await quote.addNamedLocation("Distillery District, Toronto");
  await quote.fillLocation(1, "Parking (CAD)", "20.00");
  await quote.chooseStudio(1, "The daylight loft");
  await quote.fillLocation(1, "Studio hours", "2.00");
  await quote.fill("Equipment rental units", "2");
  await quote.fill("Lunches", "2");
  await quote.fill("Assistants", "1");
  await quote.applyCode("PHOTO10");
  await quote.amount("1360.73");
  await quote.line("Travel", "45.86");
  await quote.line("Studio time, location 1", "160.00");
  await quote.message("Advance booking, 15%");
  await quote.expectReckonerPrivacy();
  await quote.noOverflow();
  await quote.capture(testInfo.outputPath("studio-reckoner-golden.png"));
  await quote.mirrorWithoutRecalculation();
  await quote.amount("1360.73");
  await quote.noOverflow();
});

test("Given a failed API calculation When the visitor retries Then the preserved Studio draft yields a current quote with managed focus", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.openReckoner(await provisionReckoner());
  await quote.reckonerRegion();
  await quote.failNextReckonerCalculation();
  await quote.completeSession();
  await quote.message("The estimate is unavailable right now.");
  await quote.noAmount();
  await quote.value("Start time", "10:00");
  await quote.retryReckoner();
  await quote.amount("1000.00");
  await quote.expectReckonerPrivacy();
});

test("Given no public options When Studio mounts the calculator Then the existing shell contains a configuration message without Reckoner requests", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.openReckoner({ apiBaseUrl: "", publishableKey: "" });
  await quote.message("This calculator is not configured.");
  await quote.expectReckonerRequestCount(0);
});
