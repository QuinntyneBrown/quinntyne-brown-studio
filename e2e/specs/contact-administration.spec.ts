import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { SettingsPage } from "../page-objects/settings-page";

const details = {
  email: "hello@example.test",
  phone: "416-555-0100",
  hours: "Monday – Saturday · 09:00 – 18:00",
  replyNote: "Within two working days",
};

async function fillDetails(settings: SettingsPage) {
  await settings.fill("Studio email address", details.email);
  await settings.fill("Phone number", details.phone);
  await settings.fill("Opening hours", details.hours);
  await settings.fill("Typical reply time", details.replyNote);
}

// Given the studio details screen, when an administrator saves an email address, phone
// number, opening hours and reply time, then the values are saved with their version and
// reopening the screen reads them back.
test("P10 AC-L2-075-01 studio details save with their version and read back", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("studio-details");
  await fillDetails(settings);
  await settings.click("Save studio details");
  await settings.message("Saved successfully.");
  const saved = fixture.calls.find(
    (call) => call.service === "studio-details" && call.method === "save",
  );
  expect(saved?.args[0]).toMatchObject({ ...details, version: 0 });
  expect(fixture.studioDetails).toMatchObject({ ...details, version: 1 });
  await settings.open("studio-details");
  await settings.value("Studio email address", details.email);
  await settings.value("Phone number", details.phone);
  await settings.value("Opening hours", details.hours);
  await settings.value("Typical reply time", details.replyNote);
});

// Given an invalid email address, when the administrator saves, then the rejection is shown,
// the typed values stay in the form, and the previous details remain in effect.
test("P10 AC-L2-075-02 an invalid detail is rejected beside the form and keeps the previous details", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.failures.set("studio-details.save", {
    status: 400,
    message: "Enter a valid email.",
  });
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("studio-details");
  await settings.fill("Studio email address", "not-an-address");
  await settings.fill("Phone number", details.phone);
  await settings.click("Save studio details");
  await settings.message("Enter a valid email.");
  await settings.value("Studio email address", "not-an-address");
  await settings.value("Phone number", details.phone);
  expect(fixture.studioDetails.email).toBeNull();
  expect(fixture.studioDetails.version).toBe(0);
});

// Given details saved by another administrator, when a stale copy is saved, then the conflict
// is reported and the newer details are not overwritten.
test("P10 AC-L2-075-03 a stale save is rejected as a conflict without overwriting newer details", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.studioDetails = {
    ...fixture.studioDetails,
    version: 2,
    email: "second@example.test",
  };
  fixture.failures.set("studio-details.save", {
    status: 409,
    message: "This record changed. Reload before saving.",
  });
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("studio-details");
  await settings.value("Studio email address", "second@example.test");
  await settings.fill("Studio email address", "stale@example.test");
  await settings.click("Save studio details");
  await settings.message("This record changed. Reload before saving.");
  expect(fixture.studioDetails.email).toBe("second@example.test");
  expect(fixture.studioDetails.version).toBe(2);
});
