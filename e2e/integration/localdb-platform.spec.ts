import { test, expect } from "@playwright/test";
import { AccountPage } from "../page-objects/account-page";
import { SessionPage } from "../page-objects/session-page";
import { ClientCollectionPage } from "../page-objects/client-collection-page";
import { PrintInboxPage } from "../page-objects/print-inbox-page";
import { PublicSitePage } from "../page-objects/public-site-page";
import { FullstackFixture } from "../page-objects/fullstack-fixture";
import { StudioFixture } from "../page-objects/studio-fixture";

// Given packaged applications and a disposable LocalDB database, when the studio
// uploads, publishes and assigns work, then clients can create albums and submit
// server-priced requests, administrators review snapshots, and revocation denies bytes.
test("P04 P05 P06 AC-L2-027-01 AC-L2-034-01 AC-L2-063-01 AC-L2-064-01 AC-L2-065-01 AC-L2-069-01 complete LocalDB workflow", async ({
  page,
  context,
  browser,
}, info) => {
  const origin = process.env.QBS_SMOKE_ORIGIN ?? "https://localhost:7453";
  const email = process.env.Bootstrap__Email,
    password = process.env.Bootstrap__Password;
  if (!email || !password)
    throw new Error("Supply isolated smoke administrator credentials.");
  const account = new AccountPage(page, origin);
  await account.open();
  await account.login(email, password);
  await account.heading("Sessions");
  const fixture = new FullstackFixture(context, origin),
    suffix = Date.now();
  const record = await fixture.createSession("Local smoke portrait " + suffix);
  const session = new SessionPage(page, origin + "/admin");
  await session.open(record.id);
  await session.uploadJpeg();
  await session.fileState("smoke.jpg", "Ready");
  await session.capture(info.outputPath("admin-session.png"));
  const photo = await fixture.publish(record.id, "selected-" + suffix);
  await fixture.printOption();
  const clientEmail = `smoke-${suffix}@example.test`,
    token = await fixture.invite(clientEmail);
  await session.open(record.id);
  await session.assign(clientEmail);
  const clientContext = await browser.newContext({ ignoreHTTPSErrors: true });
  try {
    const clientPage = await clientContext.newPage(),
      clientAccount = new AccountPage(clientPage, origin);
    await clientAccount.open("accept-invitation?token=" + token, "client");
    await clientAccount.savePassword(password);
    await clientAccount.heading("Your sessions");
    const collection = new ClientCollectionPage(
      clientPage,
      new StudioFixture(),
      origin + "/client",
    );
    await collection.open("galleries/" + record.id);
    await collection.visiblePhotos(1);
    await collection.capture(info.outputPath("client-gallery.png"));
    await collection.open("albums");
    await collection.click("Create album");
    await collection.nameAlbum("Smoke favourites");
    await collection.selectPhoto("smoke.jpg");
    await collection.click("Save album");
    await collection.heading("Smoke favourites");
    const priorRequests = new Set(
      (await fixture.requests()).map((request: { id: string }) => request.id),
    );
    await collection.open();
    await collection.selectPhoto("smoke.jpg");
    await collection.review();
    await collection.total("29.95");
    await collection.submit();
    await collection.message("Your print request has been received.");
    const submitted = (await fixture.requests()).find(
      (request: { id: string }) => !priorRequests.has(request.id),
    );
    expect(submitted.total).toBe("29.95");
    const inbox = new PrintInboxPage(page, origin + "/admin");
    await inbox.open();
    await inbox.filter("Submitted");
    await inbox.openRequest(submitted.id);
    await inbox.click("Mark reviewed");
    await inbox.message("Request marked reviewed.");
    await session.open(record.id);
    await session.assign(clientEmail, false);
    await fixture.clientPhotoDenied(clientContext, photo.id);
    const publicSite = new PublicSitePage(clientPage, origin);
    await publicSite.open("galleries/selected-" + suffix);
    await publicSite.heading("Local selected work");
    await publicSite.visiblePhotos(1);
    await publicSite.open();
    await publicSite.message("Local selected work");
    await publicSite.capture(info.outputPath("marketing-desktop.png"));
    await publicSite.capture(info.outputPath("marketing-mobile.png"), 390, 844);
  } finally {
    await clientContext.close();
  }
});
