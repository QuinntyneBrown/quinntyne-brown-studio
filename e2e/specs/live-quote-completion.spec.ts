import { expect, test } from '@playwright/test';
import { QuotePage } from '../page-objects/quote-page';

test('Q09 AC-L2-066-01 selecting an address retains keyboard focus in the location editor', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.add();
  await quote.addressFocused();
});

test('Q01 AC-L2-009-01 AC-L2-010-01 AC-L2-055-01 two locations and all costs match the independent fixture', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  quote.calculate = async (input) => {
    const result = QuotePage.result(input, '306.94');
    const money = (amount: string) => ({ amount, currency: 'CAD' });
    const lines = [
      ['photography', null, '125.01'],
      ['travel', null, '9.26'],
      ['equipment', null, '30.00'],
      ['lunch', null, '30.00'],
      ['assistant', null, '100.00'],
      ['parking', 0, '1.01'],
      ['studio', 0, '31.25'],
      ['parking', 1, '2.01'],
      ['studio', 1, '12.50'],
    ].map(([kind, locationIndex, amount]) => ({
      kind,
      locationIndex,
      quantity: '1',
      amount: money(amount as string),
    }));
    return {
      ...result,
      lines,
      subtotal: money('341.04'),
      discount: { ...result.discount, amount: money('34.10') },
    };
  };
  await quote.open();
  await quote.fill('Session date', '2027-06-01');
  await quote.fill('End time', '11:15');
  await quote.add();
  await quote.add();
  await quote.fillLocation(1, 'Parking (CAD)', '1.005');
  await quote.fillLocation(2, 'Parking (CAD)', '2.005');
  await quote.chooseLocation(1, 'studio-a');
  await quote.fillLocation(1, 'Studio hours', '1.25');
  await quote.chooseLocation(2, 'studio-a');
  await quote.fillLocation(2, 'Studio hours', '0.5');
  await quote.fill('Assistants', '2');
  await quote.fill('Equipment rental units', '1');
  await quote.fill('Lunches', '2');
  await quote.noAmount();
  await quote.amount('306.94');
  await quote.line('Parking · location 1', '1.01');
  await quote.line('Parking · location 2', '2.01');
  await quote.line('Studio · location 1', '31.25');
  await quote.line('Studio · location 2', '12.50');
  await quote.line('Assistants', '100.00');
  await quote.line('Equipment', '30.00');
  await quote.line('Lunches', '30.00');
  expect(quote.calls.at(-1)).toMatchObject({
    assistantCount: 2,
    equipmentUnits: 1,
    lunchCount: 2,
    locations: [
      { parkingAmount: '1.005', studioId: 'studio-a', studioHours: '1.25' },
      { parkingAmount: '2.005', studioId: 'studio-a', studioHours: '0.5' },
    ],
  });
});
