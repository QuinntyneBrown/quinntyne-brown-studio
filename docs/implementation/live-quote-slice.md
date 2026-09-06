# Marketing live quote slice

A visitor selects a photography service, enters Toronto session times, resolves an ordered list of locations, and receives a current itemized CAD estimate with optional costs, the best eligible discount, and indicative availability. The slice runs from saved configuration and scheduling data through the .NET API to the Angular marketing calculator. It creates no reservation, payment, or consultation submission.

## Requirements, mocks, and designs

| Baseline | Included behavior |
| --- | --- |
| [L2-008–013, L2-055–056](../specs/L2.md) | Four services; locations, parking and studios; assistants, equipment and lunches; live updates; validation; identified CAD amounts; OD-01 rounding and Azure Maps routes. |
| [L2-014–017, L2-057](../specs/L2.md) | Quote-side code, advance and weekday eligibility, largest discount, deterministic ties and removal. Existing administration supplies configuration. |
| [L2-022, L2-058](../specs/L2.md) | Read schedule availability with Toronto intervals and buffers; never create a hold. |
| [L2-001–002, L2-066](../specs/L2.md) | Responsive hierarchy, keyboard operation, readable validation and failures at the approved browser/viewport matrix. |
| [L2-038–045, L2-049–054](../specs/L2.md) | Applicable inward dependencies, interface consumption and acceptance-first delivery obligations. |

Visual references: [calculator](../mocks/marketing/quote.html), estimate presentation from [quote summary](../mocks/marketing/quote-summary.html), and [service failure](../mocks/marketing/service-error.html). Fictional prototype prices and consultation submission are not production behavior.

Designs implemented: [calculate-live-quote](../detailed-designs/quotations/calculate-live-quote/README.md), quote-side [apply-discounts](../detailed-designs/quotations/apply-discounts/README.md), and [check-session-availability](../detailed-designs/scheduling/check-session-availability/README.md). Reused configuration dependencies: [configure-rates](../detailed-designs/quotations/configure-rates/README.md) and [configure-studios](../detailed-designs/quotations/configure-studios/README.md). [OD-01–03 and OD-08](../specs/decisions.md) govern conflicts with prototype behavior.

## Acceptance criteria

Each scenario is exercised through API integration or Playwright page objects; tests reference the IDs below and the corresponding L2 IDs.

| ID | Given | When | Then |
| --- | --- | --- | --- |
| Q-01 | Saved rates and a base | Each of four services is quoted with multiple locations and optional costs | Independently calculated rounded CAD lines, subtotal, discount and total are returned with input/configuration revisions. |
| Q-02 | A current estimate | A cost, service, location, time or code changes | The old estimate is immediately invalid; valid inputs recalculate after 300 ms; older successes and failures cannot replace current state. |
| Q-03 | Empty, invalid or out-of-range inputs | A visitor edits or submits them | Readable correction is shown; no valid estimate is presented; API requests return controlled validation errors. |
| Q-04 | Missing rates/base, disabled studio or a failed provider | Calculation is attempted | The estimate is unavailable or invalid as appropriate; no zero-distance fallback is used; retry preserves the draft. |
| Q-05 | Ambiguous, absent or delayed address candidates | A visitor searches or changes the search | Selection is explicit, no-match/failure is readable, and obsolete responses are ignored. |
| Q-06 | Eligible code, advance and weekday rules | Inputs cross boundaries or code validity changes | Only the largest discount applies, ties prefer code/advance/weekday, invalid codes preserve automatic eligibility, and removed eligibility disappears. |
| Q-07 | Toronto dates and schedule windows | Session times include ordinary, DST, buffer or overlap boundaries | Valid offsets and positive quarter-hour intervals are required; availability is distinct from price and creates no reservation. |
| Q-08 | Saved configuration | Rates or studios change and a new calculation runs | Current persisted configuration is used; unset differs from explicit zero; disabled studios are unavailable for selection. |
| Q-09 | Any calculator state | The visitor uses keyboard or mobile/tablet/desktop layouts | Controls, errors, status and estimate remain readable and operable without horizontal overflow. |

## Implementation and evidence

`QuotesController` dispatches calculation, address resolution and enabled studio queries through MediatR 12.5.0. Existing route and decimal-string contracts remain compatible. Application validation rejects incomplete locations and invalid intervals/costs; Maps failures never become zero-distance quotes. Removed optional costs disappear from the itemization. No database migration or production sample pricing was added.

`IQuoteService` now exposes typed operations, with separate production and acceptance adapters. The route owns `IQuoteEditorService`; signal state drives the domain input form and summary. Address and quote revisions reject obsolete responses, input changes invalidate the estimate immediately, and valid edits debounce for 300 ms. Toronto offsets are automatic except when a repeated time requires explicit selection. Retry preserves input, and dynamic location selection preserves keyboard focus. Quote regions use mirrored design tokens and the reviewed standalone catalog patterns.

Acceptance sources: [API and pricing cases](../../backend/tests/Qbs.AcceptanceTests/LiveQuoteAcceptanceTests.cs), [Maps boundary cases](../../backend/tests/Qbs.AcceptanceTests/QuoteMapsAcceptanceTests.cs), [SQL reopening case](../../backend/tests/Qbs.AcceptanceTests/QuoteSqlAcceptanceTests.cs), [calculator browser cases](../../e2e/specs/live-quote.spec.ts), [multi-cost and keyboard cases](../../e2e/specs/live-quote-completion.spec.ts), and [missing-date validation](../../e2e/specs/live-quote-validation.spec.ts). The existing four-service fixtures and discount/time cases remain regression coverage.

The 2026-09-06 baseline passed 44 backend tests. Genuine red runs captured null-location failure, negative route acceptance, four removed-cost itemization failures, missing address correction, lost keyboard focus and incorrect missing-date field association. Their artifacts remain under `.artifacts/live-quote/red*`. Additional cases cover existing correct behavior; no historical red run is claimed for those. Fixture corrections and browser-runner startup/teardown failures are not counted as product defects. Browser execution is serial for reliable startup on this Windows environment.

The scoped implementation is complete. Final verification on 2026-09-06 passed against this uncommitted working tree:

| Check | Result / reproducible command |
| --- | --- |
| Backend Release acceptance | **85 passed, 0 failed, 0 skipped**, including four SQL LocalDB cases; `dotnet test backend/Qbs.slnx -c Release`. The slice adds 41 acceptance/adapter cases. |
| Browser matrix | **153 passed, 0 failed, 0 skipped, 0 flaky**: 117 quote executions plus 36 existing regressions across all nine browser/viewport combinations; `npm test --prefix e2e`. |
| Design system | **33 passed**; `npm test --prefix design-system`. Built-artifact check passed; standalone quote preview visually reviewed. |
| Packaging | Release solution build passed without warnings/errors; `npm run build:libs --prefix frontend` and `npm run build:apps --prefix frontend` passed for all four libraries and three apps. |
| Supporting checks | E2E typecheck, existing repository checker, documentation/diagram verification and whitespace check passed. Calculator desktop/mobile captures were reviewed. |

TRX and copied browser/catalog JSON results are in `.artifacts/live-quote/final/`; final browser captures are in `.artifacts/live-quote/final-matrix/`.  Live Azure credentials/deployment remain outside this delivery; the real Maps adapter is verified against controlled credential and HTTP boundaries. Broader administrator acceptance and the platform's external evidence gates retain their existing status.
