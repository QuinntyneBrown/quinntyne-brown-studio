import { chromium, expect } from '../e2e/node_modules/@playwright/test/index.mjs';
import { mkdirSync } from 'node:fs';
import { resolve } from 'node:path';

const origin = process.env.QBS_SMOKE_ORIGIN ?? 'https://localhost:7443';
const email = process.env.Bootstrap__Email, password = process.env.Bootstrap__Password;
if (!email || !password) throw new Error('Set local administrator credentials before running the HTTPS smoke check.');
const output = resolve('.artifacts/smoke'); mkdirSync(output, { recursive: true });
const browser = await chromium.launch();
try {
  const context = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1440, height: 900 } });
  const page = await context.newPage();
  const errors = []; page.on('pageerror', error => errors.push(error.message));
  for (let n = 0; n < 60; n++) {
    try { if ((await context.request.get(origin + '/api/health')).ok()) break; } catch {}
    if (n === 59) throw new Error('Local gateway/API did not start.');
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  await page.goto(origin); await expect(page.getByRole('heading', { name: 'Photography with feeling.', exact: true })).toBeVisible();
  await page.screenshot({ path: resolve(output, 'marketing-desktop.png'), fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({ path: resolve(output, 'marketing-mobile.png'), fullPage: true });
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto(origin + '/admin/login');
  await page.getByLabel('Email', { exact: true }).fill(email);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Sessions', exact: true })).toBeVisible();
  async function send(method, path, body) {
    const token = await (await context.request.get(origin + '/api/auth/antiforgery')).json();
    const response = await context.request.fetch(origin + '/api/' + path, { method, headers: { 'X-XSRF-TOKEN': token.requestToken }, data: body });
    if (!response.ok()) throw new Error(path + ': ' + response.status() + ' ' + await response.text());
    return response.json();
  }
  const session = await send('POST', 'admin/sessions', { name: 'Local smoke portrait', service: 'Headshot', startsAt: '2027-06-01T10:00:00-04:00', endsAt: '2027-06-01T11:00:00-04:00' });
  await page.goto(origin + '/admin/sessions/' + session.id);
  await expect(page.getByRole('heading', { name: 'Local smoke portrait', exact: true })).toBeVisible();
  // A browser-generated JPEG exercises real hashing, block transfer and worker decoding.
  const jpeg = await page.evaluate(() => { const canvas = document.createElement('canvas'); canvas.width = 900; canvas.height = 1200; const ctx = canvas.getContext('2d'); ctx.fillStyle = '#7a8572'; ctx.fillRect(0, 0, 900, 1200); return canvas.toDataURL('image/jpeg').split(',')[1]; });
  await page.getByLabel('Select photos', { exact: true }).setInputFiles({ name: 'smoke.jpg', mimeType: 'image/jpeg', buffer: Buffer.from(jpeg, 'base64') });
  await expect(page.locator('.session__upload')).toContainText('Ready', { timeout: 30000 });
  await expect(page.locator('.photo-grid img')).toHaveCount(1);
  await expect(page.getByText('1 photos · Active', { exact: true })).toBeVisible();
  await page.screenshot({ path: resolve(output, 'admin-session.png'), fullPage: true });
  const photoPage = await (await context.request.get(origin + '/api/admin/sessions/' + session.id + '/photos')).json();
  await send('POST', 'admin/public-galleries', { title: 'Local selected work', slug: 'local-selected-work', published: true, photoIds: [photoPage.photos[0].id] });
  await send('POST', 'admin/print-options', { name: 'Studio print', dimensions: '8 × 10 in', finish: 'Matte', unitPrice: '29.95', enabled: true });
  await send('POST', 'admin/invitations', { email: 'smoke-client@example.test' });
  const clients = await (await context.request.get(origin + '/api/admin/clients')).json();
  const client = clients.find(x => x.email === 'smoke-client@example.test');
  const latest = await (await context.request.get(origin + '/api/admin/sessions/' + session.id)).json();
  await send('PUT', 'admin/sessions/' + session.id + '/clients', { clientIds: [client.id], expectedVersion: latest.version });
  let invitation;
  await expect(async () => { const mail = await (await context.request.get(origin + '/api/admin/development-mail')).json(); invitation = mail.map(x => x.body.match(/https:\/\/[^ ]+accept-invitation\?token=[A-F0-9]+/)).find(Boolean)?.[0]; expect(invitation).toBeTruthy(); }).toPass({ timeout: 15000 });
  const clientContext = await browser.newContext({ ignoreHTTPSErrors: true });
  const clientPage = await clientContext.newPage();
  await clientPage.goto(invitation);
  await clientPage.getByLabel('Password', { exact: true }).fill(password);
  await clientPage.getByRole('button', { name: 'Save password', exact: true }).click();
  await expect(clientPage.getByRole('heading', { name: 'Your sessions', exact: true })).toBeVisible();
  await clientPage.getByRole('link', { name: /Local smoke portrait/ }).click();
  await expect(clientPage.locator('.photo-grid img')).toHaveCount(1);
  await clientPage.screenshot({ path: resolve(output, 'client-gallery.png'), fullPage: true });
  await clientPage.goto(origin + '/client/albums');
  await clientPage.getByRole('button', { name: 'Create album', exact: true }).click();
  await clientPage.getByLabel('Album name', { exact: true }).fill('Smoke favourites');
  await clientPage.getByRole('checkbox', { name: 'Select smoke.jpg', exact: true }).check();
  await clientPage.getByRole('button', { name: 'Save album', exact: true }).click();
  await expect(clientPage.getByRole('heading', { name: 'Smoke favourites', exact: true })).toBeVisible();
  await clientPage.goto(origin + '/client/prints');
  await clientPage.getByRole('checkbox', { name: 'Select smoke.jpg', exact: true }).check();
  await clientPage.getByRole('button', { name: 'Review print options', exact: true }).click();
  await expect(clientPage.getByText('Current total: 29.95 CAD', { exact: true })).toBeVisible();
  await clientPage.getByRole('button', { name: 'Submit print request', exact: true }).click();
  await expect(clientPage.getByText('Your request has been sent to the studio for review.', { exact: true })).toBeVisible();
  const requests = await (await context.request.get(origin + '/api/admin/print-requests')).json();
  expect(requests).toHaveLength(1); expect(requests[0].total).toBe('29.95');
  await send('POST', 'admin/print-requests/' + requests[0].id + '/review', { expectedVersion: requests[0].version });
  await clientPage.goto(origin + '/galleries/local-selected-work');
  await expect(clientPage.getByRole('heading', { name: 'Local selected work', exact: true })).toBeVisible();
  await expect(clientPage.locator('.photo-grid img')).toHaveCount(1);
  expect(errors).toEqual([]);
  console.log('PASS: packaged HTTPS apps, real login, JPEG upload/preview, captured invitation, assigned gallery, album creation, priced print submission/review, public publication.');
  // The catalog is a separate static product on its own origin, so it cannot receive
  // the host-only product cookie. Its own suite in design-system/ proves that isolation.
} finally { await browser.close(); }
