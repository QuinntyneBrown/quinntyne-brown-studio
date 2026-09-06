import { test } from '@playwright/test';
import { QuotePage } from '../page-objects/quote-page';

test('Q03 AC-L2-012-01 missing dates identify the date field and invalidate the estimate', async ({
  page,
}) => {
  const quote = new QuotePage(page);
  await quote.open();
  await quote.add();
  await quote.amount('297.29');
  await quote.fill('Session date', '');
  await quote.noAmount();
  await quote.invalidField('Session date');
  await quote.message('Enter a session date.');
  await quote.fill('Session date', '2027-06-01');
  await quote.amount('297.29');
  await quote.fill('End date', '');
  await quote.noAmount();
  await quote.invalidField('End date');
  await quote.fill('End date', '2027-06-01');
  await quote.amount('297.29');
});
