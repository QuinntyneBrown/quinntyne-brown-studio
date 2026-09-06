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
