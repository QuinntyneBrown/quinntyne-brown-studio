import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { SettingsPage } from "../page-objects/settings-page";
import { InquiryInboxPage } from "../page-objects/inquiry-inbox-page";

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

function inquiries() {
  const fixture = new StudioFixture();
  fixture.records["inquiries"] = [
    {
      id: "inquiry-older",
      version: 1,
      reference: "QB-IN-1041",
      name: "Daniel Okafor",
      email: "daniel@example.test",
      phone: null,
      interest: "Headshot",
      message: "Looking for two headshot looks. <b>Bold?</b>",
      consentAt: "2026-08-26T14:00:00Z",
      submittedAt: "2026-08-26T14:00:00Z",
      state: "Submitted",
      reviewedBy: null,
      reviewedAt: null,
    },
    {
      id: "inquiry-newer",
      version: 1,
      reference: "QB-IN-1042",
      name: "Priya Raman",
      email: "priya@example.test",
      phone: "416-555-0166",
      interest: "Wedding",
      message: "A small September wedding, about forty guests.",
      consentAt: "2026-09-02T09:30:00Z",
      submittedAt: "2026-09-02T09:30:00Z",
      state: "Submitted",
      reviewedBy: null,
      reviewedAt: null,
    },
  ];
  return fixture;
}

// Given stored inquiries, one containing markup, when an administrator opens the inbox and
// marks one reviewed, then inquiries list newest first with name, interest, submission time
// and state; opening one shows every value with the markup as text; and the reviewed inquiry
// shows Reviewed with its review time and cannot be reviewed twice.
test("P10 AC-L2-074-01 the inbox lists inquiries newest first, shows markup as text, and records a review", async ({
  page,
  context,
}) => {
  const fixture = inquiries();
  await fixture.install(context);
  const inbox = new InquiryInboxPage(page);
  await inbox.open();
  await inbox.inquiryCount(2);
  await inbox.rowOrder(["QB-IN-1042", "QB-IN-1041"]);
  await inbox.row("QB-IN-1042", "Priya Raman");
  await inbox.row("QB-IN-1042", "Wedding");
  await inbox.row("QB-IN-1042", "2026");
  await inbox.row("QB-IN-1042", "Submitted");
  await inbox.row("QB-IN-1041", "Headshots");
  await inbox.openInquiry("QB-IN-1041");
  await inbox.detail("Reference", "QB-IN-1041");
  await inbox.detail("Name", "Daniel Okafor");
  await inbox.detail("Email", "daniel@example.test");
  await inbox.detail("Phone", "Not provided");
  await inbox.detail("Interest", "Headshots");
  await inbox.detail("Message", "Looking for two headshot looks. <b>Bold?</b>");
  await inbox.click("Mark reviewed");
  await inbox.message("Inquiry marked reviewed.");
  await inbox.message("Reviewed on");
  await inbox.reviewDisabled();
  expect(
    fixture.records["inquiries"].find((row) => row.id === "inquiry-older"),
  ).toMatchObject({ state: "Reviewed", version: 2 });
  const review = fixture.calls.find(
    (call) => call.service === "inquiry" && call.method === "review",
  );
  expect(review?.args).toEqual(["inquiry-older", 1]);
  await inbox.filter("Reviewed");
  await inbox.inquiryCount(1);
  await inbox.row("QB-IN-1041", "Reviewed");
  await inbox.filter("Submitted");
  await inbox.inquiryCount(1);
  await inbox.row("QB-IN-1042", "Submitted");
});

// Given an unavailable inquiry service, when the inbox opens, then the failure is distinct
// from an empty inbox and retrying after recovery shows the empty state.
test("P10 AC-L2-074-01 failed inbox loading is distinct from no inquiries", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.failures.set("inquiry.list", {
    status: 503,
    message: "The inquiry inbox is unavailable.",
  });
  await fixture.install(context);
  const inbox = new InquiryInboxPage(page);
  await inbox.open();
  await inbox.message("The inquiry inbox is unavailable.");
  await inbox.noEmpty();
  fixture.failures.delete("inquiry.list");
  await inbox.retry();
  await inbox.message("No inquiries yet.");
});

// Given the website content editor, when an administrator publishes the About page copy under
// the about page key, then the published heading is stored for the server-rendered page.
test("P10 AC-L2-071-03 the content editor publishes the about page copy", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  await fixture.install(context);
  const settings = new SettingsPage(page);
  await settings.open("content");
  await settings.choose("Page", "about");
  await settings.fill("Heading", "Photographs with room to breathe.");
  await settings.fill("Body", "A published introduction.");
  await settings.check("Publish this revision");
  await settings.click("Save");
  await settings.message("Saved successfully.");
  expect(
    fixture.records["content"].find((row) => row.pageKey === "about"),
  ).toMatchObject({ publishedHeading: "Photographs with room to breathe." });
});
