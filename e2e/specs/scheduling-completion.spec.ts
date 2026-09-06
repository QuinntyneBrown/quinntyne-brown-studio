import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { CatalogPage } from "../page-objects/catalog-page";
import { SettingsPage } from "../page-objects/settings-page";

// Given local Toronto dates and times, when a session is saved, then the correct
// offset is supplied without requiring an administrator to type a timestamp.
test("P03 AC-L2-069-01 session timing uses Toronto date and time controls", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const sessions = new CatalogPage(page);
  await sessions.open("sessions");
  await sessions.add("session");
  await sessions.fill("Name", "Summer portraits");
  await sessions.fill("Start date and time", "2027-06-01T10:00");
  await sessions.fill("End date and time", "2027-06-01T12:00");
  await sessions.save();
  await sessions.message("Saved successfully.");
  expect(fixture.records["sessions"][0]).toMatchObject({
    startsAt: "2027-06-01T10:00:00-04:00",
    endsAt: "2027-06-01T12:00:00-04:00",
  });
});

// Given the repeated autumn hour, when a schedule is entered, then its occurrence
// must be chosen and the resulting saved interval is unambiguous.
test("P03 AC-L2-021-01 AC-L2-058-01 repeated Toronto schedule times require an occurrence", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const schedule = new SettingsPage(page);
  await schedule.open("schedule/person-a");
  await schedule.click("Add working interval");
  await schedule.fill("Start date and time", "2027-11-07T01:30");
  await schedule.fill("End date and time", "2027-11-07T03:00");
  await schedule.click("Save");
  await schedule.message("Choose an occurrence for the repeated Toronto time.");
  expect(
    fixture.calls.filter(
      (call) => call.service === "schedule" && call.method === "save",
    ),
  ).toHaveLength(0);
  await schedule.select("Start date and time occurrence", "-05:00");
  await schedule.click("Save");
  await schedule.message("Saved successfully.");
  expect(fixture.schedules["person-a"].workingWindows).toEqual([
    {
      startsAt: "2027-11-07T01:30:00-05:00",
      endsAt: "2027-11-07T03:00:00-05:00",
    },
  ]);
});

// Given the missing spring hour, when that time is entered, then the draft remains
// editable and cannot be saved until it describes a real Toronto instant.
test("P03 AC-L2-058-01 nonexistent Toronto times require correction", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const schedule = new SettingsPage(page);
  await schedule.open("schedule/person-a");
  await schedule.click("Add working interval");
  await schedule.fill("Start date and time", "2027-03-14T02:15");
  await schedule.fill("End date and time", "2027-03-14T04:00");
  await schedule.click("Save");
  await schedule.message("This time does not exist in Toronto.");
  await schedule.value("Start date and time", "2027-03-14T02:15");
  await schedule.fill("Start date and time", "2027-03-14T03:15");
  await schedule.click("Save");
  await schedule.message("Saved successfully.");
});
