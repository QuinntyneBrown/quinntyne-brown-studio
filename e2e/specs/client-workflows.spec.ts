import { test, expect } from "@playwright/test";
import { StudioFixture } from "../page-objects/studio-fixture";
import { ClientCollectionPage } from "../page-objects/client-collection-page";
import { PrintInboxPage } from "../page-objects/print-inbox-page";

// Given assigned photographs, when an album is created, reordered and reopened,
// then its order persists; revoked photographs remain removable placeholders.
test("P06 AC-L2-037-01 AC-L2-065-01 albums retain order and removable unavailable placeholders", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.role = "Client";
  fixture.records["sessions"] = [
    { id: "session-a", name: "Portraits", clientIds: ["client-a"] },
  ];
  fixture.photos["session-a"] = ["A", "B"].map((name) => ({
    id: name,
    name,
    available: true,
    state: "Ready",
  }));
  await fixture.install(context);
  const album = new ClientCollectionPage(page, fixture);
  await album.open("albums");
  await album.click("Create album");
  await album.nameAlbum("Keepsakes");
  await album.selectPhoto("A");
  await album.selectPhoto("B");
  await album.click("Move B earlier");
  await album.photoOrder(["B", "A"]);
  await album.click("Save album");
  await album.heading("Keepsakes");
  const saved = fixture.records["albums"][0];
  expect(saved.orderedPhotoIds).toEqual(["B", "A"]);
  fixture.photos["session-a"] = [fixture.photos["session-a"][0]];
  saved.photos[0] = { id: "B", name: "Unavailable photo", available: false };
  await album.open(`albums/${saved.id}`);
  await album.click("Edit album");
  await album.photoOrder(["Unavailable photo", "A"]);
  await album.click("Remove Unavailable photo");
  fixture.failures.set("album.save", {
    status: 409,
    message: "This album changed. Reload before saving.",
  });
  await album.nameAlbum("Revised keepsakes");
  await album.click("Save album");
  await album.message("This album changed.");
  await album.albumName("Revised keepsakes");
  fixture.failures.delete("album.save");
  await album.click("Save album");
  await album.heading("Revised keepsakes");
  expect(fixture.records["albums"][0].orderedPhotoIds).toEqual(["A"]);
});

// Given unavailable photo discovery, when album editing is requested, then an
// actionable error leaves the existing album available for another attempt.
test("P06 AC-L2-065-01 album photo discovery failure supports another attempt", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.role = "Client";
  fixture.failures.set("client-gallery.list", {
    status: 503,
    message: "Photographs unavailable. Try creating the album again.",
  });
  await fixture.install(context);
  const album = new ClientCollectionPage(page, fixture);
  await album.open("albums");
  await album.click("Create album");
  await album.message("Photographs unavailable.");
  fixture.failures.delete("client-gallery.list");
  await album.click("Create album");
  await album.nameAlbum("A new album");
});

// Given submitted and reviewed requests, when administrators filter the inbox,
// then only the requested state appears and reviewing updates that same snapshot.
test("P06 AC-L2-064-01 print inbox filters and preserves immutable review details", async ({
  page,
  context,
}) => {
  const fixture = new StudioFixture();
  fixture.records["print-requests"] = [
    {
      id: "request-a",
      version: 1,
      state: "Submitted",
      total: "76.65",
      notes: "Please discuss a frame.",
      lines: [
        {
          name: "Portrait print",
          dimensions: "8x10",
          finish: "Matte",
          quantity: 3,
          unitPrice: "25.55",
          amount: "76.65",
        },
      ],
    },
    {
      id: "request-b",
      version: 1,
      state: "Reviewed",
      total: "10.00",
      notes: "",
      lines: [],
    },
  ];
  await fixture.install(context);
  const inbox = new PrintInboxPage(page);
  await inbox.open();
  await inbox.filter("Submitted");
  await inbox.requestCount(1);
  await inbox.click("Open request");
  await inbox.message("Please discuss a frame.");
  fixture.failures.set("print-request.review", {
    status: 503,
    message: "Review unavailable. Try again.",
  });
  await inbox.click("Mark reviewed");
  await inbox.message("Review unavailable.");
  fixture.failures.delete("print-request.review");
  await inbox.click("Mark reviewed");
  await inbox.message("Request marked reviewed.");
  await inbox.requestCount(0);
  await inbox.filter("Reviewed");
  await inbox.requestCount(2);
  expect(fixture.records["print-requests"][0].total).toBe("76.65");
});
