import { expect, test } from '@playwright/test';
import { QuotePage } from '../page-objects/quote-page';

// Given/When/Then criteria Q-01..Q-09 in docs/implementation/live-quote-slice.md.
test('Q05 AC-L2-056-01 no matching address has a readable correction', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.candidates = [];
  await quote.open();
  await quote.search();
  await quote.message(
    'No matching addresses. Refine the address and try again.',
  );
  await quote.noAmount();
});

test('Q01 Q09 AC-L2-008-01 AC-L2-013-01 AC-L2-066-01 service, discount, availability and CAD presentation', async ({
  page,
}, testInfo) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.add();
  await quote.amount('297.29');
  await quote.message('Advance booking · 10%');
  await quote.message('A photographer is currently available.');
  await quote.capture(testInfo.outputPath('quote-current.png'));
  for (const service of ['Event', 'Headshot', 'FamilyPortrait', 'Wedding']) {
    await quote.choose('Photography service', service);
    await quote.noAmount();
    await quote.amount('297.29');
    expect(quote.calls.at(-1).service).toBe(service);
  }
  await quote.fill('Address', 'Venue');
  await quote.layoutAndKeyboard();
});

test('Q02 AC-L2-011-01 AC-L2-011-02 obsolete successes cannot replace a newer result', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  let finish: (value: any) => void = () => {};
  quote.calculate = (input) =>
    input.equipmentUnits === 0
      ? new Promise((resolve) => {
          finish = resolve;
        })
      : Promise.resolve(QuotePage.result(input, '240.00'));
  await quote.open();
  await quote.add();
  await quote.expectCalls(1);
  await quote.fill('Equipment rental units', '2');
  await quote.amount('240.00');
  finish(QuotePage.result(quote.calls[0], '100.00'));
  await quote.settled(2);
  await quote.amount('240.00');
  await quote.fill('Equipment rental units', '-1');
  await quote.noAmount();
  await quote.message('Use nonnegative whole counts');
});

test('Q02 AC-L2-011-02 an obsolete failure cannot clear the current estimate', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  let fail: (error: Error) => void = () => {};
  quote.calculate = (input) =>
    input.lunchCount === 0
      ? new Promise((_, reject) => {
          fail = reject;
        })
      : Promise.resolve(QuotePage.result(input, '420.00'));
  await quote.open();
  await quote.add();
  await quote.expectCalls(1);
  await quote.fill('Lunches', '2');
  await quote.amount('420.00');
  fail(new Error('Old request failed.'));
  await quote.settled(2);
  await quote.amount('420.00');
});

test('Q05 AC-L2-056-01 ambiguous choices and old address responses require explicit current selection', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  let finish: (value: any[]) => void = () => {};
  quote.resolve = (address) =>
    address === 'Old venue'
      ? new Promise((resolve) => {
          finish = resolve;
        })
      : Promise.resolve([
          { label: 'Venue A', latitude: 43.7, longitude: -79.3 },
          { label: 'Venue B', latitude: 43.8, longitude: -79.2 },
        ]);
  await quote.open();
  await quote.search('Old venue');
  await quote.message('Finding addresses…');
  await quote.search('New venue');
  await quote.candidate('Venue B');
  await quote.noAmount();
  expect(quote.calls).toHaveLength(0);
  finish([{ label: 'Obsolete venue', latitude: 43, longitude: -79 }]);
  await quote.settled(2, true);
  await quote.candidate('Obsolete venue', false);
  await quote.click('Venue B · Select');
  await quote.amount('297.29');
  expect(quote.calls.at(-1).locations[0].location.label).toBe('Venue B');
});

test('Q04 Q05 studio and address failures can be retried without losing input', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.getStudios = async () => {
    throw new Error('Studio options are unavailable.');
  };
  quote.resolve = async () => {
    throw new Error('Address lookup failed.');
  };
  await quote.open();
  await quote.message('Studio options are unavailable.');
  quote.getStudios = async () => quote.studios;
  await quote.click('Retry studio options');
  await quote.search('My venue');
  await quote.message('Address lookup failed.');
  await quote.value('Address', 'My venue');
  quote.resolve = async () => quote.candidates;
  await quote.click('Find address');
  await quote.click('Venue A · Select');
  await quote.amount('297.29');
  await quote.choose('Studio', 'studio-a');
  await quote.fill('Studio hours', '1');
  await quote.amount('297.29');
});

test('Q06 AC-L2-014-02 invalid code retains an automatic discount and removal clears the error', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.calculate = async (input) => {
    const result = QuotePage.result(input);
    return {
      ...result,
      discount: {
        ...result.discount,
        codeError: input.code ? 'This code cannot be applied.' : null,
      },
    };
  };
  await quote.open();
  await quote.add();
  await quote.fill('Discount code', 'EXPIRED');
  await quote.amount('297.29');
  await quote.message('This code cannot be applied.');
  await quote.message('Advance booking · 10%');
  await quote.fill('Discount code', '');
  await quote.noAmount();
  await quote.amount('297.29');
  expect(quote.calls.at(-1).code).toBe('');
});

test('Q04 AC-L2-012-02 failure retry preserves inputs and replaces unavailable state', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.calculate = async () => {
    throw new Error('Driving distance could not be calculated.');
  };
  await quote.open();
  await quote.add();
  await quote.message('Driving distance could not be calculated.');
  await quote.noAmount();
  await quote.value('Equipment rental units', '0');
  quote.calculate = async (input) =>
    QuotePage.result(input, '310.00', null, false);
  await quote.click('Retry estimate');
  await quote.amount('310.00');
  await quote.message('No photographer is currently available for this time.');
  await quote.message('does not reserve a session');
});

test('Q03 Q07 AC-L2-055-01 Toronto time validation and automatic winter offset', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.fill('Session date', '2027-01-12');
  await quote.add();
  await quote.amount('297.29');
  expect(quote.calls.at(-1).startsAt).toContain('-05:00');
  await quote.fill('Session date', '2027-03-14');
  await quote.fill('Start time', '02:00');
  await quote.noAmount();
  await quote.message('This Toronto time does not exist');
  await quote.fill('Session date', '2027-11-07');
  await quote.fill('Start time', '01:00');
  await quote.message('Choose an offset for the repeated Toronto time');
  await quote.choose('Start time offset', '-04:00');
  await quote.amount('297.29');
});

test('Q02 Q05 AC-L2-009-02 location and optional cost removal updates the request', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.add();
  await quote.amount('297.29');
  await quote.choose('Studio', 'studio-a');
  await quote.fill('Studio hours', '1.25');
  await quote.fill('Assistants', '2');
  await quote.fill('Lunches', '3');
  await quote.amount('297.29');
  expect(quote.calls.at(-1)).toMatchObject({
    assistantCount: 2,
    lunchCount: 3,
  });
  await quote.choose('Studio', '');
  await quote.amount('297.29');
  expect(quote.calls.at(-1).locations[0]).toMatchObject({
    studioId: null,
    studioHours: '0',
  });
  await quote.click('Remove location 1');
  await quote.noAmount();
});
