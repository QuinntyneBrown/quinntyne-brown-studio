import { expect, test } from "@playwright/test";
import type { QuoteResult, ResolvedAddress } from "reckoner/behavior";
import { QuotePage } from "../page-objects/quote-page";

// Reckoner L2-007,011,016,017,019,024,073 replace the retired Studio quote wire contract.
test("Given no matching address When a visitor searches Then a readable correction leaves the estimate incomplete", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.candidates = [];
  await quote.open();
  await quote.search();
  await quote.message(
    "No matching addresses. Refine the address and try again.",
  );
  await quote.noAmount();
});

test("Given Studio services When selection changes Then discounts availability and CAD formatting follow the controlled result", async ({
  page,
}, testInfo) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.completeSession();
  await quote.add();
  await quote.amount("297.29");
  await quote.message("Advance booking, 10%");
  await quote.message("This date is currently available.");
  await quote.capture(testInfo.outputPath("quote-current.png"));
  for (const service of ["event", "headshots", "familyPortraits", "wedding"]) {
    await quote.choose("Photography service", service);
    await quote.noAmount();
    await quote.amount("297.29");
    expect(quote.calls.at(-1)!.service).toBe(service);
  }
  await quote.fill("Find a location", "Venue");
  await quote.layoutAndKeyboard();
});

test("Given an obsolete success When a newer request completes Then the old result cannot replace the current amount", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  let finish!: (value: QuoteResult) => void;
  quote.calculate = (input) =>
    input.factors.equipment === 0
      ? new Promise((resolve) => {
          finish = resolve;
        })
      : Promise.resolve(QuotePage.result(input, "240.00"));
  await quote.open();
  await quote.completeSession();
  await quote.expectCalls(1);
  await quote.fill("Equipment rental units", "2");
  await quote.amount("240.00");
  finish(QuotePage.result(quote.calls[0], "100.00"));
  await quote.settled(2);
  await quote.amount("240.00");
  await quote.fill("Equipment rental units", "-1");
  await quote.noAmount();
  await quote.message("Use whole counts from 0 to 1,000.");
});

test("Given an obsolete failure When a newer request completes Then the failure cannot clear the current amount", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  let fail!: (error: Error) => void;
  quote.calculate = (input) =>
    input.factors.lunch === 0
      ? new Promise((_, reject) => {
          fail = reject;
        })
      : Promise.resolve(QuotePage.result(input, "420.00"));
  await quote.open();
  await quote.completeSession();
  await quote.expectCalls(1);
  await quote.fill("Lunches", "2");
  await quote.amount("420.00");
  fail(new Error("Old request failed."));
  await quote.settled(2);
  await quote.amount("420.00");
});

test("Given old and ambiguous address results When the visitor selects a current candidate Then only that resolved address is added", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  let finish!: (value: ResolvedAddress[]) => void;
  quote.resolve = (address) =>
    address === "Old venue"
      ? new Promise((resolve) => {
          finish = resolve;
        })
      : Promise.resolve([
          { label: "Venue A", latitude: 43.7, longitude: -79.3 },
          { label: "Venue B", latitude: 43.8, longitude: -79.2 },
        ]);
  await quote.open();
  await quote.search("Old venue");
  await quote.message("Finding addresses…");
  await quote.search("New venue");
  await quote.candidate("Venue B");
  await quote.noAmount();
  expect(quote.calls).toHaveLength(0);
  finish([{ label: "Obsolete venue", latitude: 43, longitude: -79 }]);
  await quote.settled(2, true);
  await quote.candidate("Obsolete venue", false);
  await quote.click("Venue B");
  await quote.completeSession();
  await quote.amount("297.29");
  expect(quote.calls.at(-1)!.locations![0].label).toBe("Venue B");
});

test("Given definition and address failures When retried Then the loaded studios and preserved query become usable", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.getStudios = async () => {
    throw new Error("Offline");
  };
  quote.resolve = async () => {
    throw new Error("Offline");
  };
  await quote.open();
  await quote.message("Unable to load the calculator");
  quote.getStudios = async () => quote.studios;
  await quote.click("Retry loading");
  await quote.search("My venue");
  await quote.message("Address lookup failed. Try again.");
  await quote.value("Find a location", "My venue");
  quote.resolve = async () => quote.candidates;
  await quote.click("Find address");
  await quote.click("Venue A");
  await quote.completeSession();
  await quote.amount("297.29");
  await quote.choose("Studio", "studio-a");
  await quote.fill("Studio hours", "1");
  await quote.amount("297.29");
});

test("Given an invalid code When it is applied then removed Then the automatic discount remains and the code error clears", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.calculate = async (input) => {
    const result = QuotePage.result(input);
    return {
      ...result,
      discount: {
        ...result.discount,
        codeError: input.code ? "This code cannot be applied." : null,
      },
    };
  };
  await quote.open();
  await quote.completeSession();
  await quote.amount("297.29");
  await quote.applyCode("EXPIRED");
  await quote.blur("Discount or loyalty code");
  await quote.amount("297.29");
  await quote.message("This code cannot be applied.");
  await quote.message("Advance booking, 10%");
  await quote.applyCode("");
  await quote.noAmount();
  await quote.amount("297.29");
  expect(quote.calls.at(-1)!.code).toBe("");
});

test("Given a failed estimate When retried Then the draft survives and an unavailable date can still have a price", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.calculate = async () => {
    throw new Error("Provider unavailable");
  };
  await quote.open();
  await quote.completeSession();
  await quote.message("The estimate is unavailable right now.");
  await quote.noAmount();
  await quote.value("Equipment rental units", "");
  quote.calculate = async (input) =>
    QuotePage.result(input, "310.00", null, false);
  await quote.click("Retry estimate");
  await quote.amount("310.00");
  await quote.message("This date is not currently available.");
  await quote.message("does not reserve a session");
});

test("Given Toronto daylight-saving transitions When local times are entered Then nonexistent times fail and repeated times need no offset control", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.completeSession();
  await quote.fill("Session date", "2027-01-12");
  await quote.amount("297.29");
  expect(quote.calls.at(-1)!).toMatchObject({
    date: "2027-01-12",
    startTime: "10:00",
    endTime: "14:00",
  });
  await quote.fill("Session date", "2027-03-14");
  await quote.fill("Start time", "02:00");
  await quote.noAmount();
  await quote.message("This time does not exist on the selected date.");
  await quote.fill("Session date", "2027-11-07");
  await quote.fill("Start time", "01:00");
  await quote.amount("297.29");
});

test("Given location extras When studio and location are removed Then effective requests clear those costs", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.completeSession();
  await quote.add();
  await quote.amount("297.29");
  await quote.choose("Studio", "studio-a");
  await quote.fill("Studio hours", "1.25");
  await quote.fill("Assistants", "2");
  await quote.fill("Lunches", "3");
  await quote.amount("297.29");
  await expect.poll(() => quote.calls.at(-1)!.factors).toMatchObject({ assistant: 2, lunch: 3 });
  await quote.choose("Studio", "");
  await quote.amount("297.29");
  await expect.poll(() => quote.calls.at(-1)!.locations![0]).toMatchObject({
    studioId: null,
    studioHours: 0,
  });
  await quote.click("Remove location 1");
  await quote.amount("297.29");
  await expect.poll(() => quote.calls.at(-1)!.locations).toEqual([]);
});
