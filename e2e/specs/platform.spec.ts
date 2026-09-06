import { test, expect } from "@playwright/test";
import { StudioPage } from "../page-objects/studio-page";

test("AC-L2-001-01 AC-L2-002-01 public presentation follows the studio visual hierarchy", async ({
  page,
}) => {
  const studio = new StudioPage(page);
  await studio.mock();
  await studio.open("marketing");
  await studio.heading("Photography with feeling.");
  await studio.quoteLink();
});

test("AC-L2-023-01 equipment save records the entered quantity", async ({
  page,
}) => {
  const studio = new StudioPage(page);
  await studio.mock();
  await studio.open("admin", "equipment");
  await studio.heading("Equipment");
  await studio.click("Add equipment");
  await studio.fill("Name", "Camera");
  await studio.fill("Quantity", "2");
  await studio.click("Save");
  await studio.message("Saved");
  expect(studio.fixture.records["equipment"][0].quantity).toBe(2);
});

test("AC-L2-033-02 client without assigned sessions sees an empty state", async ({
  page,
}) => {
  const studio = new StudioPage(page);
  await studio.mock("Client");
  await studio.open("client", "galleries");
  await studio.heading("Your sessions");
  await studio.text("No galleries available yet.");
});

test("AC-L2-023-01 failed save preserves the administrator draft", async ({
  page,
}) => {
  const studio = new StudioPage(page);
  await studio.mock();
  studio.fixture.failures.set("catalog.save", {
    status: 409,
    message: "This record changed. Reload before saving.",
  });
  await studio.open("admin", "equipment");
  await studio.click("Add equipment");
  await studio.fill("Name", "My camera");
  await studio.fill("Quantity", "3");
  await studio.click("Save");
  await studio.message("This record changed");
  await studio.value("Name", "My camera");
});
