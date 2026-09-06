import { test, expect } from "@playwright/test";
import { SessionPage } from "../page-objects/session-page";
import { uploadFixture } from "../page-objects/upload-fixture";

// Given a failed session read, when the administrator retries, then the workspace
// becomes available without having presented an empty collection on failure.
test("P07 AC-L2-029-01 session loading is recoverable", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  fixture.failures.set("session.get", {
    status: 503,
    message: "Session unavailable.",
  });
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.message("Session unavailable.");
  await session.noEmpty();
  fixture.failures.delete("session.get");
  await session.click("Retry loading");
  await session.message("Portrait session");
});

// Given mixed successful and failed AI results, when they are shown, then each
// result identifies its photograph and model, manual viewing remains available,
// and retry requests only the failures.
test("P07 AC-L2-030-01 AC-L2-031-01 AI guidance identifies the photo and model", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  fixture.photos["session-a"] = [
    {
      id: "photo-a",
      name: "Portrait A",
      state: "Ready",
      available: true,
      url: 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="10" height="10"/>',
    },
    {
      id: "photo-b",
      name: "Portrait B",
      state: "Ready",
      available: true,
      url: 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="10" height="10"/>',
    },
  ];
  const batch = {
    id: "analysis-a",
    photos: [
      {
        photoId: "photo-a",
        state: "Completed",
        result: {
          photoId: "photo-a",
          recommendation: "Promising",
          modelVersion: "studio-model-2",
          promptVersion: "review-v3",
          findings: [
            {
              criterion: "Lighting",
              outcome: "Pass",
              explanation: "Soft light.",
            },
          ],
        },
      },
      { photoId: "photo-b", state: "Failed", error: "Provider unavailable." },
    ],
  };
  fixture.operations.set("analysis.start", () => batch);
  fixture.operations.set("analysis.get", () => batch);
  fixture.operations.set("analysis.retry", () => batch);
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.photo("Portrait A");
  await session.photo("Portrait B");
  await session.click("Suggest promising photos");
  await session.message("studio-model-2");
  await session.message("review-v3");
  await session.inspect("Portrait A");
  await session.dialog("Portrait A");
  await session.click("Close dialog");
  await session.click("Retry failed analysis");
  await expect
    .poll(() => fixture.calls.find((call) => call.method === "retry")?.args[1])
    .toEqual(["photo-b"]);
  expect(
    fixture.calls.some(
      (call) =>
        ["public-gallery", "client-gallery"].includes(call.service) &&
        call.method === "save",
    ),
  ).toBe(false);
});

// Given current deletion impact, when references change before confirmation, then
// the stale confirmation is rejected and the refreshed blockers prevent deletion.
test("P07 AC-L2-061-01 retention changes and stale deletion impact are explicit", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  fixture.retentions["session-a"] = {
    months: 12,
    version: 1,
    state: "Active",
    photoCount: 2,
    publishedReferences: 0,
    unreviewedRequests: 0,
    impactRevision: "original",
  };
  fixture.operations.set(
    "retention.extend",
    (_id, months, expiresAt) =>
      (fixture.retentions["session-a"] = {
        ...fixture.retentions["session-a"],
        months,
        expiresAt,
        version: 2,
      }),
  );
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.field("Retention months for new uploads", "18");
  await session.field("Extend expiry date and time", "2027-07-01T12:00");
  await session.click("Update retention");
  await session.message("Retention updated.");
  expect(fixture.retentions["session-a"].expiresAt).toBe(
    "2027-07-01T12:00:00-04:00",
  );
  await session.click("Delete session photos");
  await session.dialog("Delete session photographs?");
  await session.click("Cancel");
  expect(fixture.calls.some((call) => call.method === "deletePhotos")).toBe(
    false,
  );
  await session.click("Delete session photos");
  fixture.retentions["session-a"].publishedReferences = 1;
  fixture.failures.set("retention.deletePhotos", {
    status: 409,
    message: "Photo references changed. Review current impact.",
  });
  await session.click("Confirm permanent deletion");
  await session.message("Photo references changed.");
  await session.disabled("Delete session photos");
});

// Given a later photo page fails, when loading is retried, then the existing
// collection stays available and additional photographs appear once.
test("P07 AC-L2-029-01 manual photo pagination survives a failed page", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  fixture.photos["session-a"] = Array.from({ length: 51 }, (_, i) => ({
    id: `photo-${i}`,
    name: `Portrait ${i}`,
    state: "Ready",
  }));
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.message("Portrait 0");
  fixture.failures.set("photo.list", {
    status: 503,
    message: "More photographs unavailable. Try again.",
  });
  await session.click("Load more photographs");
  await session.message("More photographs unavailable.");
  fixture.failures.delete("photo.list");
  await session.click("Load more photographs");
  await session.message("Portrait 50");
});
