import { test } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { CatalogPage } from "../page-objects/catalog-page";
import { SettingsPage } from "../page-objects/settings-page";
import { ClientCollectionPage } from "../page-objects/client-collection-page";
import { PrintInboxPage } from "../page-objects/print-inbox-page";

// Given a failed initial request, when the service becomes available and loading
// is retried, then real data replaces the error; failure is never shown as an empty result.
test("P08 AC-L2-018-01 failed configuration loading supports retry", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.failures.set("rate.get", {
    status: 503,
    message: "Rates are unavailable.",
  });
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("rates");
  await settings.message("Rates are unavailable.");
  fixture.failures.delete("rate.get");
  await settings.click("Retry loading");
  await settings.fill("Wedding rate (CAD)", "100");
  await settings.click("Save");
  await settings.message("Saved successfully.");
});
test("P08 AC-L2-023-01 failed catalog loading is distinct from an empty catalog", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.failures.set("catalog.list", {
    status: 503,
    message: "Equipment is unavailable.",
  });
  await fixture.install(context);
  const catalog = new CatalogPage(page);
  await catalog.open("equipment");
  await catalog.message("Equipment is unavailable.");
  await catalog.noEmpty();
  fixture.failures.delete("catalog.list");
  await catalog.retry();
  await catalog.message("No equipment yet.");
});
test("P08 AC-L2-033-02 failed client gallery loading is distinct from no assignments", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.role = "Client";
  fixture.failures.set("client-gallery.list", {
    status: 503,
    message: "Collections are unavailable.",
  });
  await fixture.install(context);
  const collection = new ClientCollectionPage(page, fixture);
  await collection.open("galleries");
  await collection.message("Collections are unavailable.");
  await collection.noEmpty();
  fixture.failures.delete("client-gallery.list");
  await collection.retry();
  await collection.message("No galleries available yet.");
});
test("P08 AC-L2-064-01 failed inbox loading is distinct from no requests", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.failures.set("print-request.list", {
    status: 503,
    message: "The inbox is unavailable.",
  });
  await fixture.install(context);
  const inbox = new PrintInboxPage(page);
  await inbox.open();
  await inbox.message("The inbox is unavailable.");
  await inbox.noEmpty();
  fixture.failures.delete("print-request.list");
  await inbox.retry();
  await inbox.message("No print requests yet.");
});
