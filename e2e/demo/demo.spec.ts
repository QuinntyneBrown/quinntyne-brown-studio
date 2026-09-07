import { expect, Page, test } from "@playwright/test";
import { mkdirSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";
import { AccountPage } from "../page-objects/account-page";
import { CatalogPage } from "../page-objects/catalog-page";
import { ClientCollectionPage } from "../page-objects/client-collection-page";
import { FullstackFixture } from "../page-objects/fullstack-fixture";
import { PrintInboxPage } from "../page-objects/print-inbox-page";
import { PublicSitePage } from "../page-objects/public-site-page";
import { QuotePage } from "../page-objects/quote-page";
import { SessionPage } from "../page-objects/session-page";
import { SettingsPage } from "../page-objects/settings-page";
import { StudioFixture } from "../page-objects/studio-fixture";
import {
  Album,
  Client,
  Content,
  Discounts,
  Equipment,
  Gallery,
  PrintRequest,
  Prints,
  Promotion,
  Quote,
  Rates,
  Session,
  Studio,
  Vendor,
} from "./demo-data";
import { Narrator } from "./narrate";
import { photographNames, photographs } from "./photographs";

// Three demonstrations, not three tests.
//
// They assert as they go: every `expect` here is a recording refusing to show something that did
// not happen. But their purpose is to be watched. Each drives the packaged applications against
// the published API and a real LocalDB database, in one continuous take, with no compositing and
// no second attempt.
//
// They run in order and share one database. Administration sets the studio up; the public site
// shows what was published; the client's collection is built from what was assigned. The same
// names and figures are typed and then seen, so they live in demo-data.ts.
//
//     ./scripts/record-demo.ps1
//
// The recordings land in docs/demo. This file is excluded from the acceptance suites by living
// outside specs/, and it gates nothing.

test.describe.configure({ mode: "serial" });

const origin = process.env["QBS_DEMO_ORIGIN"] ?? "https://localhost:7463";
const documentation = resolve(__dirname, "../../docs/demo");
const chapterLog = resolve(__dirname, "../../.artifacts/demo/chapters");

/** What the first recording creates and the later ones need to find again. */
const shared = { invitation: "" };

/**
 * Closes the page so the recording finalises, then files it under docs/demo with its chapters.
 *
 * The video is written by the browser as the take runs; it cannot be read until the page is
 * closed. The chapter marks are what the README's tables are made from.
 */
async function file(page: Page, narrator: Narrator, name: string): Promise<void> {
  const video = page.video();
  const duration = narrator.elapsed;
  await page.close();
  await video?.saveAs(resolve(documentation, `${name}.webm`));
  mkdirSync(chapterLog, { recursive: true });
  writeFileSync(
    resolve(chapterLog, `${name}.json`),
    JSON.stringify({ name, duration, chapters: narrator.chapters }, null, 2),
  );
}

async function poster(page: Page, name: string): Promise<void> {
  await page.screenshot({ path: resolve(documentation, `${name}-poster.png`) });
}

test("Studio administration", async ({ page, context }) => {
  const narrator = new Narrator(page, "Administration");
  const admin = origin + "/admin";
  const account = new AccountPage(page, origin);
  const settings = new SettingsPage(page, admin);
  const catalog = new CatalogPage(page, admin);
  const session = new SessionPage(page, admin);
  const fixture = new FullstackFixture(context, origin);

  // ── Title ────────────────────────────────────────────────────────────────────────────────────
  await account.open("login", "admin");
  await narrator.chapter(
    "A demonstration",
    "Studio administration",
    "Rates, people, sessions, photographs, publication and clients: the studio's own workspace. "
      + "Everything that follows is the real application, driving itself against a real database.",
  );
  await narrator.quiet();
  await narrator.beat(1_200);

  // ── 1. Signing in ────────────────────────────────────────────────────────────────────────────
  await narrator.chapter("One", "Signing in", "The workspace is closed to the public.");
  await narrator.say(
    "Studio accounts are provisioned, never self-registered",
    "The administrator was created when the platform was installed. There is no sign-up form to find.",
  );
  await account.login(Studio.email, Studio.password);
  await account.heading("Sessions");
  await narrator.say(
    "It opens on sessions",
    "Empty, because nothing has been planned yet. The rest of this recording fills it in.",
  );

  // ── 2. Pricing ───────────────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "Two",
    "Pricing",
    "Quote rates, discount rules and a studio base drive the public calculator.",
  );
  await settings.open("rates");
  await narrator.say(
    "Four services, four hourly rates",
    "Session expenses are configured beside them: travel per kilometre, assistants per hour, lunches per person.",
  );
  for (const [service, rate] of Object.entries(Rates.services))
    await settings.fill(`${service} rate (CAD)`, rate);
  await narrator.reveal(260);
  for (const [cost, rate] of Object.entries(Rates.costs)) await settings.fill(cost, rate);
  await settings.click("Save");
  await settings.message("Saved successfully.");
  await narrator.say(
    "Saved, and versioned",
    "A conflicting edit from another window is refused with the draft kept, never silently overwritten.",
  );

  await settings.open("discounts");
  await narrator.say(
    "Three kinds of discount, one winner",
    "Advance booking, slow weekdays and codes. Only the largest eligible discount ever applies.",
  );
  await settings.check("Enable advance discount");
  await settings.fill("Days in advance", Discounts.advanceDays);
  await settings.fill("Advance percentage", Discounts.advancePercent);
  await settings.check("Enable weekday discount");
  await settings.fill("Weekday percentage", Discounts.weekdayPercent);
  for (const day of Discounts.weekdays) await settings.check(day);
  await narrator.reveal(320);
  await settings.click("Add code");
  await settings.fill("Code", Discounts.code);
  await settings.fill("Percentage", Discounts.codePercent);
  await settings.fill("Valid from", Discounts.codeValidFrom);
  await settings.check("Enabled");
  await narrator.say(
    "A code with a start date and no end",
    "Codes can be bounded on either side. This one begins this month and stays open.",
  );
  await settings.click("Save");
  await settings.message("Saved successfully.");

  await settings.open("studios");
  await narrator.say(
    "A studio is a point on a map",
    "Travel is priced from the base to every location and back, so the base must be a resolved address, not free text.",
  );
  await settings.fill("Studio name", Studio.name);
  await settings.fill("Hourly fee (CAD)", Studio.hourlyFee);
  await settings.fill("Address", Studio.address);
  await settings.click("Find address");
  await settings.click(`${Studio.resolvedAddress} · Select`);
  await settings.message(`Selected: ${Studio.resolvedAddress}`);
  await narrator.say(
    "Address lookup is a stand-in here",
    "In development the routing adapter answers with a controlled example. Production asks Azure Maps.",
  );
  await settings.check("Use as travel base");
  await settings.click("Save");
  await settings.message("Saved successfully.");
  await settings.message(Studio.name);

  // ── 3. People and time ───────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "Three",
    "People and time",
    "Photographers and their availability, and the equipment and vendors behind a session.",
  );
  await catalog.open("photographers");
  await catalog.add("photographer");
  await catalog.fill("Name", Studio.photographer);
  await catalog.check("Active");
  await catalog.save();
  await catalog.message("Saved successfully.");
  await narrator.say(
    "Availability lives on a schedule",
    "Working windows in Toronto time, on quarter hours, with a travel buffer either side of every booking.",
  );
  await catalog.openSchedule(Studio.photographer);
  await settings.click("Add working interval");
  await settings.fillNth("Start date and time", 0, `${Session.date}T${Session.window.start}`);
  await settings.fillNth("End date and time", 0, `${Session.date}T${Session.window.end}`);
  await settings.click("Add working interval");
  await settings.fillNth("Start date and time", 1, `${Quote.date}T${Quote.window.start}`);
  await settings.fillNth("End date and time", 1, `${Quote.date}T${Quote.window.end}`);
  await narrator.say(
    "Two windows: one past, one far ahead",
    "A recent Saturday for the session that follows, and a Saturday next summer the public quote will ask about.",
  );
  await settings.click("Save");
  await settings.message("Saved successfully.");

  await catalog.open("equipment");
  await catalog.add("equipment");
  await catalog.fill("Name", Equipment.name);
  await catalog.fill("Description", Equipment.description);
  await catalog.fill("Quantity", Equipment.quantity);
  await catalog.fill("Reference rental rate", Equipment.rentalRate);
  await narrator.say(
    "An inventory, with reference rental rates",
    "What the studio owns, and what it would cost to hire instead.",
  );
  await catalog.save();
  await catalog.message("Saved successfully.");

  await catalog.open("vendors");
  await catalog.add("vendor");
  await catalog.fill("Name", Vendor.name);
  await catalog.fill("Email", Vendor.email);
  await catalog.fill("Phone", Vendor.phone);
  await catalog.check(Vendor.role);
  await narrator.say(
    "Preferred vendors carry roles",
    "Make-up artists, second shooters and assistants, kept with the studio's records.",
  );
  await catalog.save();
  await catalog.message("Saved successfully.");

  // ── 4. A session and its photographs ─────────────────────────────────────────────────────────
  await narrator.chapter(
    "Four",
    "A session and its photographs",
    "Planning a session, uploading the originals, and reviewing them.",
  );
  await catalog.open("sessions");
  await catalog.add("session");
  await catalog.fill("Name", Session.name);
  await catalog.select("Photography service", Session.service);
  await catalog.fill("Start date and time", `${Session.date}T${Session.start}`);
  await catalog.fill("End date and time", `${Session.date}T${Session.end}`);
  await narrator.say(
    "Times are Toronto times",
    "The form resolves the offset. The API refuses any instant that does not exist and any that is not on a quarter hour.",
  );
  await catalog.choose("Photographer", Studio.photographer);
  await narrator.say(
    "The photographer must be free",
    "Saving checks the schedule and every other commitment before the session is accepted.",
  );
  await catalog.save();
  await catalog.message("Saved successfully.");
  await catalog.openSession(Session.name);
  await session.message(Session.name);
  await narrator.say(
    "Originals are uploaded in blocks, and resume",
    "Each file is hashed in the browser first. Reselect the same files after an interruption and the batch carries on.",
  );
  await session.uploadPhotographs(await photographs(page));
  for (const name of photographNames) await session.fileState(name, "Ready");
  await narrator.say(
    "Previews are made by the worker",
    "The original is kept as it arrived. A preview and a thumbnail are derived from it, and the grid fills in as each one lands.",
  );
  await narrator.reveal(520);
  await session.imageLoaded(photographNames[0]);
  await narrator.say(
    "Suggestions, on request",
    "Select photographs and ask. Findings are attached to the photograph with the model and prompt that produced them.",
  );
  await session.photo(photographNames[0]);
  await session.photo(photographNames[1]);
  await session.click("Suggest promising photos");
  await session.message("Review manually.");
  await session.revealGuidance();
  await narrator.beat(600);
  await narrator.say(
    "Advice is advisory",
    "Nothing is published or selected by a suggestion. Here the analysis adapter is a controlled example; production asks a vision model.",
  );
  await poster(page, "studio-administration");
  await session.inspect(photographNames[2]);
  await session.dialog(photographNames[2]);
  await narrator.say(
    "Manual review is always available",
    "The preview opens in a dialog that traps focus and closes on Escape.",
  );
  await session.click("Close dialog");

  // ── 5. Publishing ────────────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "Five",
    "Publishing",
    "Choosing what the public sees: galleries, print prices, packages and copy.",
  );
  await catalog.open("public-galleries");
  await catalog.add("gallery");
  await catalog.fill("Title", Gallery.title);
  await catalog.fill("Gallery URL name", Gallery.slug);
  await narrator.say(
    "A gallery is a draft until it is not",
    "Publication is a checkbox the studio ticks on purpose. New galleries start unpublished.",
  );
  for (const name of Gallery.photos) await catalog.photo(name);
  await narrator.reveal(560);
  await catalog.cover(Gallery.cover);
  await narrator.say(
    "Order and cover are explicit",
    "The cover leads the gallery on the website. Every ready photograph from every session is offered here.",
  );
  await narrator.top();
  await catalog.check("Published");
  await catalog.save();
  await catalog.message("Saved successfully.");

  await catalog.open("print-options");
  for (const print of Prints) {
    await catalog.add("print option");
    await catalog.fill("Name", print.name);
    await catalog.fill("Dimensions", print.dimensions);
    await catalog.fill("Finish", print.finish);
    await catalog.fill("Unit price (CAD)", print.unitPrice);
    await catalog.check("Enabled");
    await catalog.save();
    await catalog.message("Saved successfully.");
  }
  await narrator.say(
    "One print catalog, two audiences",
    "The same prices appear on the website and in every client's print request.",
  );

  await catalog.open("promotions");
  await catalog.add("promotion");
  await catalog.fill("Title", Promotion.title);
  await catalog.fill("Description", Promotion.description);
  await catalog.fill("Indicative price (CAD)", Promotion.indicativePrice);
  await catalog.check("Published");
  await narrator.say(
    "Packages are indicative",
    "Every package carries a consultation notice on the website. The calculator gives the real number.",
  );
  await catalog.save();
  await catalog.message("Saved successfully.");

  await settings.open("content");
  await settings.fill("Heading", Content.heading);
  await settings.fill("Body", Content.body);
  await settings.check("Publish this revision");
  await narrator.say(
    "Copy is versioned",
    "An unpublished revision never reaches the website. Only the revision marked published does.",
  );
  await settings.click("Save");
  await settings.message("Saved successfully.");

  // ── 6. Inviting a client ─────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "Six",
    "Inviting a client",
    "A private collection begins with an invitation, and access is assigned per session.",
  );
  await settings.open("invitations");
  await narrator.say(
    "An invitation is an email address",
    "The link it sends expires after 24 hours and sets the client's password once.",
  );
  shared.invitation = await fixture.invitationSentBy(async () => {
    await settings.fill("Client email", Client.email);
    await settings.click("Send invitation");
    await settings.message("Invitation queued.");
  });
  await narrator.say(
    "Captured here, delivered in production",
    "Development keeps outgoing mail in a mailbox the administrator can read. The link is real either way.",
  );
  await catalog.open("sessions");
  await catalog.openSession(Session.name);
  await narrator.reveal(900);
  await narrator.say(
    "An invitation is not a gallery",
    "Each session names the clients who may see it. Nothing is visible until this box is ticked.",
  );
  await session.assign(Client.email);
  await narrator.say(
    "Assigned",
    "From now the API serves her these photographs, and only these. The next recordings show what the public and the client see.",
  );

  // ── Closing ──────────────────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "Studio administration",
    "Set up, in one take",
    "Rates, rules, a base, a photographer, a session with six photographs, a published gallery, prices, "
      + "a package, copy and an invited client. The public site and the client's collection read from it.",
  );
  await file(page, narrator, "studio-administration");
});

test("Marketing site", async ({ page }) => {
  const narrator = new Narrator(page, "Public site");
  const site = new PublicSitePage(page, origin);
  const quote = new QuotePage(page, origin);

  // ── Title ────────────────────────────────────────────────────────────────────────────────────
  await site.open();
  await site.heading(Content.heading);
  await narrator.chapter(
    "A demonstration",
    "The public site",
    "What a couple or a family sees: the studio's work, its prices, and a live quote calculated by "
      + "the API from the rates the studio configured a moment ago.",
  );
  await narrator.quiet();
  await narrator.beat(1_200);

  // ── 1. The front page ────────────────────────────────────────────────────────────────────────
  await narrator.chapter("One", "The front page", "Every word and picture here was published on purpose.");
  await site.heroImageLoaded();
  await narrator.say(
    "The heading is the published revision",
    "It was written and published in administration. A draft left unpublished is nowhere on this site.",
  );
  await narrator.reveal(420);
  await site.message(Gallery.title);
  await narrator.say(
    "Selected work is a published gallery",
    "Its cover was chosen by the studio. Private session photographs never appear here.",
  );

  // ── 2. The portfolio ─────────────────────────────────────────────────────────────────────────
  await narrator.chapter("Two", "The portfolio", "Only eligible photographs are served.");
  await narrator.top();
  await site.navigate("Portfolio");
  await site.message(Gallery.title);
  await site.openGallery(Gallery.title);
  await site.heading(Gallery.title);
  await site.visiblePhotos(Gallery.photos.length);
  await narrator.say(
    "Four of six",
    "The session holds six photographs. The gallery publishes four, in the order the studio chose. Ask the API for any other and it answers 404.",
  );
  await site.viewPhoto(Gallery.publicPhotoName);
  await narrator.say(
    "Larger, on request, and named for the gallery",
    "The public site never shows a file name. The preview opens in a dialog; the original is never served.",
  );
  await site.closeDialog();

  // ── 3. Services, prints and packages ─────────────────────────────────────────────────────────
  await narrator.chapter(
    "Three",
    "Services, prints and packages",
    "Prices are the studio's, in Canadian dollars, before tax.",
  );
  await site.navigate("Services");
  await site.message("Family portraits");
  await narrator.say(
    "Four services",
    "Weddings, events, headshots and family portraits. Each leads to the calculator.",
  );
  await site.navigate("Prints");
  await site.message(Prints[0].name);
  await site.message(Prints[1].name);
  await narrator.say(
    "The print catalog, as clients see it",
    "The same options and prices a client chooses from when requesting prints. There is no cart.",
  );
  await site.navigate("Packages");
  await site.message(Promotion.title);
  await site.message("Subject to change following detailed consultation.");
  await narrator.say(
    "Packages are indicative",
    "Every one carries the consultation notice. The calculator, next, gives a figure the studio stands behind.",
  );

  // ── 4. A live quote ──────────────────────────────────────────────────────────────────────────
  await narrator.chapter("Four", "A live quote", "Priced by the API from configured rates, as you type.");
  await site.planSession();
  await site.heading("Your session, thoughtfully priced.");
  await narrator.say(
    "Service, date and time",
    "Toronto times on quarter hours. A date in the past is refused before any request is made.",
  );
  await quote.choose("Photography service", Quote.service);
  await quote.fill("Session date", Quote.date);
  await quote.fill("End date", Quote.date);
  await quote.fill("Start time", Quote.start);
  await quote.fill("End time", Quote.end);
  await narrator.reveal(300);
  await narrator.say(
    "A location is a resolved address",
    "Type it, look it up, choose a candidate. The estimate waits until a real place has been selected.",
  );
  await quote.fill("Address", Quote.address);
  await quote.click("Find address");
  await quote.click(`${Quote.resolvedAddress} · Select`);
  await quote.amount(Quote.totals.afterLocation);
  await quote.line("Photography", Quote.lines.photography);
  await quote.line("Travel", Quote.lines.travel);
  await narrator.say(
    "Itemised, with a round trip",
    "Eight hours of photography at the wedding rate, and travel from the studio base to the venue and back, per kilometre.",
  );
  await quote.message(Discounts.advanceLine);
  await narrator.say(
    "Booking early earns ten percent",
    "The advance rule the studio switched on applies itself. Nobody typed a code.",
  );
  await quote.fillLocation(1, "Parking (CAD)", Quote.parking);
  await quote.chooseStudio(1, Studio.name);
  await quote.fillLocation(1, "Studio hours", Quote.studioHours);
  await narrator.reveal(360);
  await quote.fill("Assistants", Quote.assistants);
  await quote.fill("Equipment rental units", Quote.equipment);
  await quote.fill("Lunches", Quote.lunches);
  await quote.amount(Quote.totals.afterDetails);
  await narrator.say(
    "Every detail is a line",
    "Parking and studio time belong to the location. Assistants are priced per hour of the session; lunches per person.",
  );
  await quote.fill("Discount code", Discounts.code);
  await quote.amount(Quote.totals.afterCode);
  await quote.message(Discounts.codeLine);
  await narrator.say(
    "A code competes with the automatic discounts",
    "The largest wins: fifteen percent replaces ten. A code that cannot apply is named, and the automatic discount stays.",
  );
  await quote.message("A photographer is currently available.");
  await narrator.say(
    "Availability is indicative",
    "A photographer with a matching working window is free that day. Nothing has been reserved.",
  );
  await poster(page, "marketing-site");
  await narrator.beat(1_200);

  // ── Closing ──────────────────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "The public site",
    "Nothing here was typed twice",
    "Copy, galleries, prices, packages, rates, discounts and availability all came from administration. "
      + "The next recording follows the client who was invited.",
  );
  await file(page, narrator, "marketing-site");
});

test("Client delivery", async ({ page }) => {
  const narrator = new Narrator(page, "Client collection");
  const admin = origin + "/admin";
  const account = new AccountPage(page, origin);
  const collection = new ClientCollectionPage(page, new StudioFixture(), origin + "/client");
  const inbox = new PrintInboxPage(page, admin);
  const catalog = new CatalogPage(page, admin);
  const session = new SessionPage(page, admin);

  // ── Title ────────────────────────────────────────────────────────────────────────────────────
  await account.open(`accept-invitation?token=${shared.invitation}`, "client");
  await account.heading("Your photographs await.");
  await narrator.chapter(
    "A demonstration",
    "The client's collection",
    `${Client.name} was invited by the studio. Her photographs, her albums, a print request, `
      + "and what the studio does with it.",
  );
  await narrator.quiet();
  await narrator.beat(1_200);

  // ── 1. Accepting the invitation ──────────────────────────────────────────────────────────────
  await narrator.chapter("One", "Accepting the invitation", "A password, set once, from a link that expires.");
  await narrator.say(
    "The link identifies the account",
    "It was captured in administration a recording ago. It expires after 24 hours and works only once.",
  );
  await account.savePassword(Client.password);
  await account.heading("Your sessions");
  await narrator.say(
    "Signed in, to one session",
    "Only sessions the studio assigned appear. The API checks that assignment on every request for a photograph.",
  );

  // ── 2. The gallery ───────────────────────────────────────────────────────────────────────────
  await narrator.chapter("Two", "The gallery", "Six photographs, served only to her.");
  await collection.openGallery(Session.name);
  await collection.heading(Session.name);
  await collection.visiblePhotos(photographNames.length);
  await collection.photosLoaded(3);
  await narrator.beat(600);
  await poster(page, "client-delivery");
  await narrator.say(
    "All six, not the four the public saw",
    "The gallery is the whole session. Thumbnails load lazily; the preview is derived from an original that is never served.",
  );
  await collection.viewPhoto(photographNames[3]);
  await narrator.say("Larger, in a dialog", "Escape closes it and returns focus to the photograph.");
  await collection.closeDialog();

  // ── 3. An album ──────────────────────────────────────────────────────────────────────────────
  await narrator.chapter("Three", "An album", "A collection of her own, in an order she chooses.");
  await collection.open("albums");
  await collection.click("Create album");
  await collection.nameAlbum(Album.name);
  for (const name of Album.selected) await collection.selectPhoto(name);
  await narrator.reveal(560);
  await narrator.say(
    "Order is chosen, and kept",
    "Move a photograph earlier or later. The album remembers its sequence, and shows a placeholder for anything later withdrawn.",
  );
  await collection.click(`Move ${Album.order[1]} earlier`);
  await collection.photoOrder(Album.order);
  await collection.click("Save album");
  await collection.heading(Album.name);
  await narrator.say("Saved", "Versioned like everything else; a stale edit is refused with the draft kept.");

  // ── 4. Requesting prints ─────────────────────────────────────────────────────────────────────
  await narrator.chapter("Four", "Requesting prints", "Priced on the server, reviewed by the studio, paid for nowhere.");
  await collection.open("prints");
  for (const name of PrintRequest.photos) await collection.selectPhoto(name);
  await narrator.say(
    "Choose the photographs first",
    "Then review sizes, finishes and current prices. Nothing is submitted yet.",
  );
  await collection.review();
  await narrator.say(
    "The total is a quotation from the API",
    "Not arithmetic in the page. Every change re-prices, and a stale price cannot be submitted.",
  );
  // Every line starts on whichever option the catalog lists first, so both are chosen on purpose.
  await collection.printOptionAt(0, Prints[0].name);
  await collection.printOptionAt(1, Prints[1].name);
  await collection.quantityAt(1, "2");
  await narrator.reveal(300);
  await collection.notes(PrintRequest.notes);
  await collection.total(PrintRequest.total);
  await narrator.say(
    `${PrintRequest.total} CAD, from the studio's own catalog`,
    "One fine art print and two gallery prints, at the prices the website shows.",
  );
  await collection.submit();
  await collection.message("Your request has been sent to the studio for review.");
  await narrator.say(
    "No payment, no fulfilment",
    "The request is a snapshot the studio reviews. Its prices are frozen at the moment of submission.",
  );

  // ── 5. The studio reviews it ─────────────────────────────────────────────────────────────────
  await narrator.chapter("Five", "The studio reviews it", "Back in administration, the request is waiting.");
  await account.signOut();
  await account.heading("Welcome back.");
  await account.open("login", "admin");
  await account.login(Studio.email, Studio.password);
  await account.heading("Sessions");
  await inbox.open();
  await inbox.filter("Submitted");
  await inbox.requestCount(1);
  await inbox.click("Open request");
  await inbox.message(PrintRequest.notes);
  await narrator.say(
    "The snapshot is immutable",
    "Lines, prices and the note are exactly what the client saw. Reviewing marks it; it does not edit it.",
  );
  await inbox.click("Mark reviewed");
  await inbox.message("Request marked reviewed.");
  await narrator.say("Reviewed", "It leaves the submitted list and keeps its figures for good.");

  // ── 6. Access is revocable ───────────────────────────────────────────────────────────────────
  await narrator.chapter("Six", "Access is revocable", "A checkbox, and the bytes stop.");
  await catalog.open("sessions");
  await catalog.openSession(Session.name);
  await narrator.reveal(900);
  await narrator.say(
    "Untick, save",
    "Assignment is the whole of a client's access. From the moment it is saved the API refuses her the photographs.",
  );
  await session.assign(Client.email, false);
  await account.signOut();
  await account.heading("Welcome back.");
  await account.open("login", "client");
  await account.login(Client.email, Client.password);
  await account.heading("Your sessions");
  await collection.message("No galleries available yet.");
  await narrator.say(
    "And the collection reflects it",
    "The session is gone from her list. The album she made keeps its shape, with placeholders where the photographs were.",
  );
  await collection.open("albums");
  await collection.openGallery(Album.name);
  await collection.heading(Album.name);
  await collection.unavailablePlaceholders(Album.order.length);
  await narrator.say(
    "Placeholders, not pictures",
    "The album's order survives. Nothing private is served, and nothing she made is lost.",
  );

  // ── Closing ──────────────────────────────────────────────────────────────────────────────────
  await narrator.chapter(
    "The client's collection",
    "Access is a decision, at every boundary",
    "Invitation, assignment, server-priced prints, an immutable review and revocation. "
      + "The three recordings are one database, driven in order by the real applications.",
  );
  await file(page, narrator, "client-delivery");
});
