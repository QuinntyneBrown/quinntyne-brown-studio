import { expect, test } from "@playwright/test";
import { QuotePage } from "../page-objects/quote-page";
// Reckoner L2-072,084: independent presentation oracle; API pricing is qualified separately.
test("Given a current address candidate When selected Then keyboard focus moves into its location editor", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.add();
  await quote.addressFocused();
});

test("Given the complete Studio fixture When two locations and all extras are entered Then the independent golden breakdown renders", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.calculate = async (input) => ({
    ...QuotePage.result(input, "1360.73"),
    lines: [
      {
        kind: "service",
        label: "Photography",
        quantity: "4.00",
        unitPrice: "250.00",
        amount: "1000.00",
        locationIndex: null,
      },
      {
        kind: "travel",
        label: "Travel",
        quantity: "36.69",
        unitPrice: "1.25",
        amount: "45.86",
        locationIndex: null,
      },
      {
        kind: "equipment",
        label: "Equipment",
        quantity: "2",
        unitPrice: "45.00",
        amount: "90.00",
        locationIndex: null,
      },
      {
        kind: "lunch",
        label: "Lunches",
        quantity: "2",
        unitPrice: "22.50",
        amount: "45.00",
        locationIndex: null,
      },
      {
        kind: "assistant",
        label: "Assistant",
        quantity: "4.00",
        unitPrice: "60.00",
        amount: "240.00",
        locationIndex: null,
      },
      {
        kind: "parking",
        label: "Parking",
        quantity: "1",
        unitPrice: "20.00",
        amount: "20.00",
        locationIndex: 1,
      },
      {
        kind: "studio",
        label: "Studio time",
        quantity: "2.00",
        unitPrice: "80.00",
        amount: "160.00",
        locationIndex: 1,
      },
    ],
  });
  await quote.open();
  await quote.completeSession();
  await quote.fill("Session date", "2026-12-19");
  await quote.add();
  await quote.add();
  await quote.fillLocation(1, "Parking (CAD)", "20.00");
  await quote.chooseLocation(1, "studio-a");
  await quote.fillLocation(1, "Studio hours", "2.00");
  await quote.fill("Assistants", "1");
  await quote.fill("Equipment rental units", "2");
  await quote.fill("Lunches", "2");
  await quote.noAmount();
  await quote.amount("1360.73");
  await quote.line("Parking, location 1", "20.00");
  await quote.line("Studio time, location 1", "160.00");
  await quote.line("Assistant", "240.00");
  await quote.line("Equipment", "90.00");
  await quote.line("Lunches", "45.00");
  expect(quote.calls.at(-1)!).toMatchObject({
    service: "wedding",
    date: "2026-12-19",
    startTime: "10:00",
    endTime: "14:00",
    factors: { assistant: 1, equipment: 2, lunch: 2 },
    locations: [
      { parkingAmount: 20, studioId: "studio-a", studioHours: 2 },
      { parkingAmount: 0, studioId: null, studioHours: 0 },
    ],
  });
});
