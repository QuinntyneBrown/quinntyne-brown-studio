import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { CatalogPage } from "../page-objects/catalog-page";
import { PublicSitePage } from "../page-objects/public-site-page";

// Given more than one page of ready photos, when a gallery is curated, then all
// eligible photos are selectable and the saved display order and cover are explicit.
test("P04 AC-L2-004-01 gallery curation includes later pages and explicit cover order", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.records["sessions"] = [{ id: "session-a", name: "Portrait session" }];
  fixture.photos["session-a"] = Array.from({ length: 51 }, (_, index) => ({
    id: `photo-${index + 1}`,
    name: `Portrait ${index + 1}`,
    state: "Ready",
  }));
  await fixture.install(context);
  const gallery = new CatalogPage(page);
  await gallery.open("public-galleries");
  await gallery.add("gallery");
  await gallery.fill("Title", "Selected portraits");
  await gallery.fill("Gallery URL name", "selected-portraits");
  await gallery.photo("Portrait 1");
  await gallery.photo("Portrait 51");
  await gallery.cover("Portrait 51");
  await gallery.save();
  await gallery.message("Saved successfully.");
  expect(fixture.records["public-galleries"][0].photoIds).toEqual([
    "photo-51",
    "photo-1",
  ]);
});

// Given an unavailable content service, when a visitor opens the site, then the
// outage is visible and retry loads the published copy.
test("P04 AC-L2-005-01 public content outages are recoverable", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.failures.set("content.published", {
    status: 503,
    message: "Website content is temporarily unavailable.",
  });
  await fixture.install(context);
  const website = new PublicSitePage(page);
  await website.open();
  await website.message("Website content is temporarily unavailable.");
  fixture.failures.delete("content.published");
  await website.retry();
  await website.heading("Photography with feeling.");
});

// Given a new public-facing record, when editing starts, then publication requires
// an explicit administrator choice.
test("P04 AC-L2-004-01 new galleries start as drafts", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const gallery = new CatalogPage(page);
  await gallery.open("public-galleries");
  await gallery.add("gallery");
  await gallery.draftPublication();
});
