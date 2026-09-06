import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { SettingsPage } from "../page-objects/settings-page";
import { CatalogPage } from "../page-objects/catalog-page";
import { PublicSitePage } from "../page-objects/public-site-page";

// Given empty configuration, when all four rates and cost categories are saved,
// then reopening retains every amount. A rejected concurrent edit keeps its draft.
test("P02 AC-L2-018-01 rates retain all categories and conflicting drafts", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("rates");
  for (const [index, service] of [
    "Wedding",
    "Event",
    "Headshot",
    "FamilyPortrait",
  ].entries())
    await settings.fill(`${service} rate (CAD)`, String(100 + index));
  for (const label of [
    "travel · per kilometre",
    "equipment · per unit/session",
    "lunch · per person",
    "assistant · per hour",
  ])
    await settings.fill(label, "12.25");
  await settings.click("Save");
  await settings.message("Saved successfully.");
  await settings.open("rates");
  await settings.value("FamilyPortrait rate (CAD)", "103");
  expect(fixture.rates.costRates).toEqual({
    travel: 12.25,
    equipment: 12.25,
    lunch: 12.25,
    assistant: 12.25,
  });
  fixture.failures.set("rate.save", {
    status: 409,
    message: "Rates changed. Reload before saving.",
  });
  await settings.fill("Wedding rate (CAD)", "225");
  await settings.click("Save");
  await settings.message("Rates changed. Reload before saving.");
  await settings.value("Wedding rate (CAD)", "225");
  expect(fixture.rates.serviceRates).toMatchObject({ Wedding: 100 });
});

// Given all discount sources, when editable rules are saved, then threshold,
// weekdays, date limits, percentage, code and enabled flags are retained.
test("P02 AC-L2-014-01 AC-L2-015-01 AC-L2-016-01 AC-L2-057-01 discount configuration persists", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("discounts");
  await settings.check("Enable advance discount");
  await settings.fill("Days in advance", "45");
  await settings.fill("Advance percentage", "15");
  await settings.check("Enable weekday discount");
  await settings.fill("Weekday percentage", "10");
  await settings.check("Tuesday");
  await settings.click("Add code");
  await settings.fill("Code", "PORTRAIT");
  await settings.fill("Percentage", "20");
  await settings.fill("Valid from", "2026-10-01");
  await settings.fill("Valid through", "2026-12-31");
  await settings.check("Enabled");
  await settings.click("Save");
  await settings.message("Saved successfully.");
  await settings.open("discounts");
  await settings.value("Code", "PORTRAIT");
  expect(fixture.discounts).toMatchObject({
    advanceRule: { threshold: 45, percentage: 15, enabled: true },
    weekdayRule: { weekdays: ["Tuesday"], percentage: 10 },
    codeRules: [
      {
        code: "PORTRAIT",
        percentage: 20,
        enabled: true,
        validFrom: "2026-10-01",
        validTo: "2026-12-31",
      },
    ],
  });
});

// Given a resolved address, when a studio is saved as the travel base, then the
// geographic selection, fee, and availability survive reopening.
test("P02 AC-L2-019-01 studio configuration retains an explicitly resolved travel base", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.operations.set("quote.resolveLocation", () => [
    { label: "10 Studio Street, Toronto", latitude: 43.65, longitude: -79.38 },
  ]);
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("studios");
  await settings.fill("Studio name", "Daylight studio");
  await settings.fill("Hourly fee (CAD)", "65.50");
  await settings.fill("Address", "10 Studio");
  await settings.click("Find address");
  await settings.click("10 Studio Street, Toronto · Select");
  await settings.check("Use as travel base");
  await settings.click("Save");
  await settings.message("Saved successfully.");
  await settings.edit("Daylight studio");
  await settings.value("Hourly fee (CAD)", "65.5");
  expect(fixture.records["studios"][0]).toMatchObject({
    isBase: true,
    enabled: true,
    resolvedAddress: { latitude: 43.65, longitude: -79.38 },
  });
});

for (const item of [
  {
    resource: "equipment",
    singular: "equipment",
    name: "Softbox",
    fields: {
      Name: "Softbox",
      Description: "Large modifier",
      Quantity: "3",
      "Reference rental rate": "25.50",
    },
    checks: [],
  },
  {
    resource: "vendors",
    singular: "vendor",
    name: "Alex",
    fields: { Name: "Alex", Email: "alex@example.test", Phone: "555-0101" },
    checks: ["MakeupArtist", "SecondShooter", "Assistant"],
  },
  {
    resource: "photographers",
    singular: "photographer",
    name: "Jordan",
    fields: { Name: "Jordan" },
    checks: ["Active"],
  },
]) {
  // Given a catalog, when an administrator creates and reopens a record, then all
  // details persist and a rejected edit preserves both the draft and prior record.
  test(`P02 AC-L2-023-01 AC-L2-024-01 AC-L2-022-01 ${item.resource} creation and rejected edits`, async ({
    page,
    context,
  }) => {
    const fixture = new StudioFixture();
    await fixture.install(context);
    const catalog = new CatalogPage(page);
    await catalog.open(item.resource);
    await catalog.add(item.singular);
    for (const [label, value] of Object.entries(item.fields))
      await catalog.fill(label, value!);
    for (const label of item.checks) await catalog.check(label);
    await catalog.save();
    await catalog.message("Saved successfully.");
    await catalog.edit(item.name);
    await catalog.value("Name", item.name);
    // AC-L2-025-01: all three vendor roles survive the saved record.
    if (item.resource === "vendors")
      expect(fixture.records["vendors"][0].roles).toEqual(item.checks);
    fixture.failures.set("catalog.save", {
      status: 409,
      message: "This record has changed. Reload it before saving.",
    });
    await catalog.fill("Name", "Revised name");
    await catalog.save();
    await catalog.message("This record has changed.");
    await catalog.value("Name", "Revised name");
    expect(fixture.records[item.resource][0].name).toBe(item.name);
  });
}

// Given draft and published records, when editors save another draft or unpublish,
// then public pages retain the last published copy and omit withdrawn products.
test("P04 AC-L2-005-01 AC-L2-006-01 AC-L2-020-01 content and price publication follows administrator choices", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  const website = new PublicSitePage(page);
  const catalog = new CatalogPage(page);
  await settings.open("content");
  await settings.fill("Heading", "Published studio story");
  await settings.fill("Body", "Our published introduction.");
  await settings.check("Publish this revision");
  await settings.click("Save");
  await settings.message("Saved successfully.");
  await settings.edit("home");
  await settings.fill("Heading", "Unfinished next story");
  await settings.check("Publish this revision", false);
  await settings.click("Save");
  await settings.message("Saved successfully.");
  await website.open();
  await website.heading("Published studio story");
  await website.absent("Unfinished next story");
  await catalog.open("print-options");
  await catalog.add("print option");
  for (const [label, value] of Object.entries({
    Name: "Archival print",
    Dimensions: "8x10",
    Finish: "Matte",
    "Unit price (CAD)": "35.50",
  }))
    await catalog.fill(label, value);
  await catalog.check("Enabled");
  await catalog.save();
  await catalog.message("Saved successfully.");
  await website.open("prints");
  await website.message("Archival print");
  await website.message("35.5");
  await catalog.open("print-options");
  await catalog.edit("Archival print");
  await catalog.check("Enabled", false);
  await catalog.save();
  await catalog.message("Saved successfully.");
  await website.open("prints");
  await website.absent("Archival print");
});

// Given a successful save followed by a failed refresh, when the response arrives,
// then the reload error stays visible with a retry instead of a success notice.
test("P08 AC-L2-018-01 a failed refresh after saving remains recoverable", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.operations.set("rate.save", (input) => {
    fixture.failures.set("rate.get", {
      status: 503,
      message: "Saved, but reloading rates failed.",
    });
    return input;
  });
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("rates");
  await settings.fill("Wedding rate (CAD)", "125");
  await settings.click("Save");
  await settings.message("Saved, but reloading rates failed.");
});
