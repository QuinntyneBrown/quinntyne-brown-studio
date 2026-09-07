// Reckoner L2-081,082,083: Studio owns the shell and the packed library owns the quote region.
import { test } from "@playwright/test";
import { QuotePage } from "../page-objects/quote-page";

test("Given the Studio quote route When its controlled definition loads Then the packed Angular quote renders inside the existing shell", async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.reckonerRegion();
});
