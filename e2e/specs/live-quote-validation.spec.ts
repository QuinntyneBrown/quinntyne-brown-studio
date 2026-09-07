import { test } from "@playwright/test";
import { QuotePage } from "../page-objects/quote-page";
// Reckoner L2-011,073: one local session date; an earlier end time means the following day.
test("Given a current estimate When date or end time is emptied Then accessible field errors replace the amount until corrected", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.completeSession();
  await quote.amount("297.29");
  await quote.fill("Session date", "");
  await quote.noAmount();
  await quote.invalidField("Session date");
  await quote.message("Enter a session date to see your estimate.");
  await quote.fill("Session date", "2027-06-01");
  await quote.amount("297.29");
  await quote.fill("End time", "");
  await quote.noAmount();
  await quote.invalidField("End time");
  await quote.fill("End time", "01:00");
  await quote.amount("297.29");
  await quote.message("Ends next day.");
});
