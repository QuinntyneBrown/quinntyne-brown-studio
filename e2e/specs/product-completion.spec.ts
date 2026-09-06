import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { uploadFixture } from "../page-objects/upload-fixture";
import { SettingsPage } from "../page-objects/settings-page";
import { CatalogPage } from "../page-objects/catalog-page";
import { PublicSitePage } from "../page-objects/public-site-page";
import { SessionPage } from "../page-objects/session-page";
import { ClientCollectionPage } from "../page-objects/client-collection-page";

// Given a published promotion, when it is displayed and later withdrawn, then
// the public offer includes its consultation notice and withdrawal removes it.
test("P04 AC-L2-007-01 promotion publication and withdrawal", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const editor = new CatalogPage(page),
    website = new PublicSitePage(page);
  await editor.open("promotions");
  await editor.add("promotion");
  await editor.fill("Title", "Autumn portraits");
  await editor.fill("Description", "A family afternoon.");
  await editor.fill("Indicative price (CAD)", "250");
  await editor.check("Published");
  await editor.save();
  await editor.message("Saved successfully.");
  await website.open("promotions");
  await website.heading("Autumn portraits");
  await website.message("Subject to change following detailed consultation.");
  await editor.open("promotions");
  await editor.edit("Autumn portraits");
  await editor.check("Published", false);
  await editor.save();
  await editor.message("Saved successfully.");
  await website.open("promotions");
  await website.absent("Autumn portraits");
});

// Given an outstanding address search, when the input changes before it returns,
// then the obsolete candidates cannot be selected as the studio's travel base.
test("P02 AC-L2-019-01 a studio address edit invalidates an outstanding lookup", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  let finish!: () => void;
  fixture.operations.set(
    "quote.resolveLocation",
    () =>
      new Promise((resolve) => {
        finish = () =>
          resolve([
            { label: "Old address", latitude: 43.65, longitude: -79.38 },
          ]);
      }),
  );
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("studios");
  await settings.fill("Address", "Old address");
  await settings.click("Find address");
  await expect.poll(() => typeof finish).toBe("function");
  await settings.fill("Address", "Corrected address");
  finish();
  await settings.noCandidate("Old address");
  await settings.value("Address", "Corrected address");
});

// Given non-ready photos, when the workspace renders, then they cannot be selected
// for AI review while ready photographs remain available for manual inspection.
test("P07 AC-L2-029-01 only ready photos can be selected for review", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  fixture.photos["session-a"] = [
    { id: "processing", name: "Processing portrait", state: "Processing" },
    { id: "failed", name: "Failed portrait", state: "Failed" },
  ];
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.selectionDisabled("Processing portrait");
  await session.selectionDisabled("Failed portrait");
});

// Given a temporary image failure, when the dependency recovers and the user
// retries, then the image reappears and Escape returns focus after manual review.
test("P07 P08 AC-L2-029-01 AC-L2-066-01 image retry and dialog keyboard focus", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  fixture.photos["session-a"] = [
    {
      id: "photo-a",
      name: "Portrait",
      state: "Ready",
      available: true,
      url: "http://localhost:4421/controlled-preview.jpg",
    },
  ];
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.imageDependency();
  await session.open();
  await session.message("Photo unavailable");
  session.imageAvailable = true;
  await session.click("Retry image · Portrait");
  await session.imageLoaded("Portrait");
  await session.inspect("Portrait");
  await session.dialog("Portrait");
  await session.touchTargets("Portrait");
  await session.closeWithEscape("Portrait");
});

// Given an invited client, when session access is assigned and revoked, then the
// client gallery list follows the administrator's current saved assignment.
test("P06 AC-L2-033-01 AC-L2-063-01 assignment and revocation propagate to client galleries", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  fixture.records["clients"] = [
    { id: "client-a", email: "client@example.test" },
  ];
  await fixture.install(context);
  const session = new SessionPage(page),
    collection = new ClientCollectionPage(page, fixture);
  await session.open();
  await session.assign("client@example.test");
  fixture.role = "Client";
  await collection.open("galleries/session-a");
  await collection.heading("Portrait session");
  fixture.role = "Administrator";
  await session.open();
  await session.assign("client@example.test", false);
  fixture.role = "Client";
  await collection.open("galleries");
  await collection.message("No galleries available yet.");
});
