import { test, expect } from "@playwright/test";
import { SessionPage } from "../page-objects/session-page";
import { uploadFixture } from "../page-objects/upload-fixture";

// Given an expired storage grant, when a block is denied, then one renewed grant
// retries that block and finalizes the original batch without duplicate photos.
test("P05 AC-L2-059-01 an expired grant is renewed during transfer", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  let attempts = 0;
  fixture.failures.set("upload.block", {
    status: 403,
    message: "Storage grant expired.",
  });
  fixture.operations.set("upload.renew", () => {
    if (++attempts === 2) fixture.failures.delete("upload.block");
    return {
      url: "https://controlled-storage.invalid/upload",
      expiresAt: "2099-01-01T00:00:00Z",
    };
  });
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.upload(["portrait.jpg"]);
  await session.fileState("portrait.jpg", "Ready");
  expect(attempts).toBe(2);
  expect(
    fixture.calls.filter((call) => call.method === "complete"),
  ).toHaveLength(1);
});

// Given an interrupted second block, when the browser reloads and the same files
// are reselected, then only unfinished blocks transfer and finalization occurs once.
test("P05 AC-L2-059-01 interrupted blocks resume after reload", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  let transfers = 0;
  fixture.operations.set("upload.block", () => {
    if (++transfers === 2) throw new Error("Connection interrupted.");
  });
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.uploadLarge("large.jpg");
  await session.fileState("large.jpg", "Interrupted");
  await session.reload();
  await session.uploadLarge("large.jpg");
  await session.fileState("large.jpg", "Ready");
  expect(transfers).toBe(3);
  expect(fixture.calls.filter((call) => call.method === "create")).toHaveLength(
    1,
  );
  expect(
    fixture.calls.filter((call) => call.method === "complete"),
  ).toHaveLength(1);
  await session.reload();
  await session.uploadLarge("large.jpg");
  await session.fileState("large.jpg", "Ready");
  expect(
    fixture.calls.filter((call) => call.method === "complete"),
  ).toHaveLength(1);
});

// Given unavailable or corrupt browser resume storage, when supported photos are
// selected, then transfers can finish and the loss of local resume information is explicit.
for (const storage of ["unavailable", "corrupt"] as const) {
  test(`P05 AC-L2-028-01 AC-L2-059-01 ${storage} resume storage does not prevent file transfer`, async ({
    page,
    context,
  }) => {
    const fixture = uploadFixture();
    await fixture.install(context);
    const session = new SessionPage(page);
    if (storage === "unavailable") await session.unavailableResumeStorage();
    else await session.corruptResumeStorage();
    await session.open();
    await session.upload(["portrait.jpg"]);
    await session.fileState("portrait.jpg", "Ready");
    await session.message(
      storage === "unavailable"
        ? "Resume information could not be saved in this browser."
        : "Saved resume information could not be read. A new batch was started.",
    );
    expect(
      fixture.calls.filter(
        (call) => call.service === "upload" && call.method === "complete",
      ),
    ).toHaveLength(1);
  });
}

// Given mixed formats, when the batch is selected, then supported files complete
// and unsupported files are independently rejected.
test("P05 AC-L2-026-01 AC-L2-028-01 mixed file outcomes remain independent", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.open();
  await session.upload(["portrait.jpg", "notes.txt"]);
  await session.fileState("portrait.jpg", "Ready");
  await session.fileState("notes.txt", "Rejected");
});

// Given one unreadable local file, when a mixed batch is checked, then the other
// files still transfer and the failed file is identified individually.
test("P05 AC-L2-028-01 one unreadable file does not abort the batch", async ({
  page,
  context,
}) => {
  const fixture = uploadFixture();
  await fixture.install(context);
  const session = new SessionPage(page);
  await session.unreadableFile("unreadable.jpg");
  await session.open();
  await session.upload(["unreadable.jpg", "portrait.jpg"]);
  await session.fileState("unreadable.jpg", "Rejected");
  await session.fileState("portrait.jpg", "Ready");
});
