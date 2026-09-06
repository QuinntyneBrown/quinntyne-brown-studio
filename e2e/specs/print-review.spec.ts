import { test, expect } from "@playwright/test";
import { ClientCollectionPage } from "../page-objects/client-collection-page";
import { StudioFixture } from "../page-objects/studio-fixture";

export function selections() {
  const fixture = new StudioFixture();
  fixture.role = "Client";
  fixture.records["sessions"] = [
    { id: "session-a", name: "Portrait session", clientIds: ["client-a"] },
  ];
  fixture.photos["session-a"] = [
    { id: "photo-a", name: "Portrait", state: "Ready", available: true },
  ];
  fixture.records["print-options"] = [
    {
      id: "option-a",
      version: 1,
      name: "Portrait print",
      dimensions: "8x10",
      finish: "Matte",
      unitPrice: "10.00",
      enabled: true,
    },
  ];
  fixture.operations.set("print-request.preview", (input) => ({
    inputRevision: input.inputRevision,
    lines: input.lines.map((line: any) => ({
      ...line,
      optionRevision: 2,
      name: "Portrait print",
      dimensions: "8x10",
      finish: "Matte",
      unitPrice: "25.55",
      amount: line.quantity === 3 ? "76.65" : "25.55",
    })),
    total: input.lines[0].quantity === 3 ? "76.65" : "25.55",
  }));
  fixture.operations.set("print-request.submit", (input) => ({
    id: "request-a",
    ...input,
    total: "76.65",
    state: "Submitted",
  }));
  return fixture;
}

// Given previously loaded prices, when a client changes quantities, then the current
// server-priced revision is reviewed and the exact reviewed revisions are submitted.
test("P06 AC-L2-035-01 AC-L2-064-01 print review uses the server price before submission", async ({
  page,
  context,
}) => {
  const fixture = selections();
  await fixture.install(context);
  const collection = new ClientCollectionPage(page, fixture);
  await collection.open();
  await collection.selectPhoto("Portrait");
  await collection.review();
  await collection.quantity("3");
  await collection.total("76.65");
  await collection.submit();
  await collection.message("Your print request has been received.");
  expect(
    fixture.calls.find((call) => call.method === "submit")?.args[0].lines,
  ).toEqual([
    {
      photoId: "photo-a",
      optionId: "option-a",
      optionRevision: 2,
      quantity: 3,
    },
  ]);
});

// Given a failed submission, when notes change, then the changed payload uses a new
// idempotency key while an unchanged retry reuses the original key.
test("P06 AC-L2-064-01 print retries distinguish an unchanged request from edited notes", async ({
  page,
  context,
}) => {
  const fixture = selections();
  fixture.failures.set("print-request.submit", {
    status: 503,
    message: "Response interrupted. Try again.",
  });
  await fixture.install(context);
  const collection = new ClientCollectionPage(page, fixture);
  await collection.open();
  await collection.selectPhoto("Portrait");
  await collection.review();
  await collection.total("25.55");
  await collection.submit();
  await collection.message("Response interrupted. Try again.");
  await collection.submit();
  await collection.message("Response interrupted. Try again.");
  await collection.notes("Please discuss a frame.");
  fixture.failures.delete("print-request.submit");
  await collection.submit();
  await collection.message("Your print request has been received.");
  const calls = fixture.calls
    .filter((call) => call.method === "submit")
    .map((call) => call.args[0]);
  expect(calls[1].idempotencyKey).toBe(calls[0].idempotencyKey);
  expect(calls[2].idempotencyKey).not.toBe(calls[0].idempotencyKey);
});

// Given cached option descriptions, when the server returns a newer price, then
// the review displays the exact unit price and finish that will be submitted.
test("P06 AC-L2-035-01 print review displays the authoritative unit price", async ({
  page,
  context,
}) => {
  const fixture = selections();
  await fixture.install(context);
  const collection = new ClientCollectionPage(page, fixture);
  await collection.open();
  await collection.selectPhoto("Portrait");
  await collection.review();
  await collection.message("Matte · 25.55 CAD each");
});

// Given an unavailable pricing service, when review fails, then submission remains
// disabled and a retry preserves the chosen photos and quantities.
test("P06 AC-L2-035-01 failed print review preserves selections and supports retry", async ({
  page,
  context,
}) => {
  const fixture = selections();
  fixture.failures.set("print-request.preview", {
    status: 503,
    message: "Price review is temporarily unavailable.",
  });
  await fixture.install(context);
  const collection = new ClientCollectionPage(page, fixture);
  await collection.open();
  await collection.selectPhoto("Portrait");
  await collection.review();
  await collection.quantity("3");
  await expect
    .poll(() =>
      fixture.calls.some(
        (call) =>
          call.method === "preview" && call.args[0].lines[0].quantity === 3,
      ),
    )
    .toBe(true);
  await collection.message("Price review is temporarily unavailable.");
  await collection.submissionDisabled();
  fixture.failures.delete("print-request.preview");
  await collection.retryReview();
  await collection.total("76.65");
});

// Given an earlier review still in flight, when a newer quantity is priced, then
// an obsolete success or error cannot replace the current price or enable stale submission.
for (const outcome of ["success", "failure"]) {
  test(`P06 AC-L2-035-01 obsolete print price ${outcome} is ignored`, async ({
    page,
    context,
  }) => {
    const fixture = selections();
    let finish!: () => void;
    const original = fixture.operations.get("print-request.preview")!;
    fixture.operations.set("print-request.preview", (input) =>
      input.lines[0].quantity === 1
        ? new Promise((resolve, reject) => {
            finish = () =>
              outcome === "success"
                ? resolve(original(input))
                : reject(new Error("Obsolete outage."));
          })
        : original(input),
    );
    await fixture.install(context);
    const collection = new ClientCollectionPage(page, fixture);
    await collection.open();
    await collection.selectPhoto("Portrait");
    await collection.review();
    await expect.poll(() => typeof finish).toBe("function");
    await collection.quantity("3");
    await collection.total("76.65");
    finish();
    await collection.total("76.65");
    await collection.submit();
    await collection.message("Your print request has been received.");
    expect(
      fixture.calls.find((call) => call.method === "submit")?.args[0].lines[0]
        .quantity,
    ).toBe(3);
  });
}

// Given a price change between review and submission, when submission conflicts,
// then a refreshed price must be reviewed before submitting again.
test("P06 AC-L2-036-01 a changed price requires a new review", async ({
  page,
  context,
}) => {
  const fixture = selections();
  fixture.failures.set("print-request.submit", {
    status: 409,
    message: "Print prices changed. Review the current price.",
  });
  await fixture.install(context);
  const collection = new ClientCollectionPage(page, fixture);
  await collection.open();
  await collection.selectPhoto("Portrait");
  await collection.review();
  await collection.total("25.55");
  await collection.submit();
  await collection.message("Print prices changed.");
  await collection.submissionDisabled();
  fixture.failures.delete("print-request.submit");
  await collection.retryReview();
  await collection.total("25.55");
  await collection.submit();
  await collection.message("Your print request has been received.");
});
