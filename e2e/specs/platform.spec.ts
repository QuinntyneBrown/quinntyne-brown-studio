import { test, expect } from '@playwright/test';
import { StudioPage } from '../page-objects/studio-page';

test('AC-L2-001-01 AC-L2-002-01 public presentation follows the studio visual hierarchy', async ({
  page,
}) => {
  const studio = new StudioPage(page);
  await studio.mock();
  await studio.open('marketing');
  await studio.heading('Photography with feeling.');
  await expect(page.getByRole('link', { name: 'Calculate your quote' })).toBeVisible();
});

test('AC-L2-023-01 equipment save records the entered quantity', async ({ page }) => {
  const studio = new StudioPage(page);
  await studio.mock();
  let submitted: Record<string, unknown> | undefined;
  await page.route('**/api/admin/equipment', async (route) => {
    if (route.request().method() === 'POST') {
      submitted = route.request().postDataJSON();
      await route.fulfill({ status: 201, json: { ...submitted, id: 'equipment-1', version: 1 } });
    } else
      await route.fulfill({
        json: submitted ? [{ ...submitted, id: 'equipment-1', version: 1 }] : [],
      });
  });
  await studio.open('admin', 'equipment');
  await studio.heading('Equipment');
  await studio.click('Add equipment');
  await studio.fill('Name', 'Camera');
  await studio.fill('Quantity', '2');
  await studio.click('Save');
  await studio.message('Saved');
  expect(submitted?.['quantity']).toBe(2);
});

test('AC-L2-033-02 client without assigned sessions sees an empty state', async ({ page }) => {
  const studio = new StudioPage(page);
  await studio.mock('Client');
  await studio.open('client', 'galleries');
  await studio.heading('Your sessions');
  await expect(page.getByText('No galleries available yet.')).toBeVisible();
});

test('AC-L2-011-01 AC-L2-011-02 live quote ignores an older response arriving last', async ({
  page,
}) => {
  const studio = new StudioPage(page);
  await studio.mock();
  await page.route('**/api/public/locations/resolve', (route) =>
    route.fulfill({ json: [{ label: 'Example venue', latitude: 43.6, longitude: -79.4 }] }),
  );
  let resolveFirst: (() => void) | undefined;
  const firstSeen = new Promise<void>((resolve) => {
    resolveFirst = resolve;
  });
  await page.route('**/api/public/quotes/calculate', async (route) => {
    const input = route.request().postDataJSON();
    const old = input.equipmentUnits === 0;
    if (old) {
      resolveFirst?.();
      await new Promise((resolve) => setTimeout(resolve, 1200));
    }
    const money = { amount: old ? '100.00' : '240.00', currency: 'CAD' };
    await route.fulfill({
      json: {
        inputRevision: input.inputRevision,
        configurationRevision: 1,
        lines: [],
        subtotal: money,
        discount: {
          percentage: '0',
          amount: { amount: '0.00', currency: 'CAD' },
          kind: null,
          codeError: null,
        },
        total: money,
        availability: { available: true, photographerIds: [], reasonCode: null },
      },
    });
  });
  await studio.open('marketing', 'quote');
  await studio.resolveLocation();
  await firstSeen;
  await studio.fill('Equipment rental units', '2');
  await studio.quoteAmount('240.00');
  await page.waitForTimeout(1400);
  await studio.quoteAmount('240.00');
});

test('AC-L2-023-01 failed save preserves the administrator draft', async ({ page }) => {
  const studio = new StudioPage(page);
  await studio.mock();
  await page.route('**/api/admin/equipment', (route) =>
    route.request().method() === 'POST'
      ? route.fulfill({
          status: 409,
          json: { title: 'This record changed. Reload before saving.' },
        })
      : route.fulfill({ json: [] }),
  );
  await studio.open('admin', 'equipment');
  await studio.click('Add equipment');
  await studio.fill('Name', 'My camera');
  await studio.fill('Quantity', '3');
  await studio.click('Save');
  await studio.message('This record changed');
  await expect(page.getByLabel('Name', { exact: true })).toHaveValue('My camera');
});
