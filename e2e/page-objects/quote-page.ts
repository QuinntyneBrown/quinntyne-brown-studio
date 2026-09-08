import { expect, type Page, type Request } from '@playwright/test';
import type { QuoteRequest, QuoteResult, ResolvedAddress } from 'reckoner/behavior';
import { quoteDefinition, quoteResult } from '../fixtures/quote-fixture';
export class QuotePage {
  private readonly reckonerRequests: Request[] = [];
  private publicKey = '';
  readonly calls: QuoteRequest[] = [];
  candidates: ResolvedAddress[] = [{ label: 'Venue A', latitude: 43.7, longitude: -79.3 }];
  studios = [{ id: 'studio-a', name: 'The daylight loft', fee: 80 }];
  resolve: (address: string) => Promise<readonly ResolvedAddress[]> = async () => this.candidates;
  calculate: (input: QuoteRequest) => Promise<QuoteResult> = async input => QuotePage.result(input);
  getStudios = async () => this.studios;
  definition = async () => ({ ...quoteDefinition(), studios: await this.getStudios() });
  constructor(readonly page: Page, readonly origin = 'http://localhost:4420') {}
  static result = quoteResult;
  async open() {
    await this.page.exposeFunction('__quoteDefinition', () => this.definition());
    await this.page.exposeFunction('__quoteResolve', (address: string) => this.resolve(address));
    await this.page.exposeFunction('__quoteCalculate', (input: QuoteRequest) => { this.calls.push(input); return this.calculate(input); });
    await this.page.addInitScript(() => {
      const w = window as any;
      w.__quoteSettled = 0; w.__lookupSettled = 0;
      const task = (promise: Promise<unknown>) => ({ promise, cancel() {} });
      w.__qbsQuoteMock = {
        configured: true,
        loadDefinition: () => task(w.__quoteDefinition().then((definition: unknown) => ({ definition, serverDate: 'Mon, 07 Sep 2026 16:00:00 GMT' }))),
        resolveAddress: (query: string) => task(w.__quoteResolve(query).finally(() => w.__lookupSettled++)),
        calculate: (input: unknown) => task(w.__quoteCalculate(input).finally(() => w.__quoteSettled++)),
        loadAvailability: (month: string) => task(Promise.resolve({ month, configurationRevision: 1, unavailable: [] })),
      };
    });
    await this.page.goto(this.origin + '/quote'); await this.ready();
  }
  /** Uses the service bound by the host, without installing a controlled adapter. */
  async openLive() { await this.page.goto(this.origin + '/quote'); await this.ready(); }
  async openReckoner(options: { apiBaseUrl: string; publishableKey: string }) {
    this.publicKey = options.publishableKey;
    this.page.on('request', request => { if (request.url().startsWith('http://127.0.0.1:4390/')) this.reckonerRequests.push(request); });
    await this.page.addInitScript(options => { (globalThis as typeof globalThis & { __reckonerPublicOptions?: typeof options }).__reckonerPublicOptions = options; }, options);
    await this.openLive();
  }
  async addNamedLocation(name: string) { await this.search('Toronto'); await this.click(name); }
  async expectReckonerRequestCount(count: number) { expect(this.reckonerRequests).toHaveLength(count); }
  async expectReckonerPrivacy() {
    expect(this.reckonerRequests.length).toBeGreaterThan(0);
    for (const request of this.reckonerRequests) {
      expect(new URL(request.url()).pathname).toMatch(/^\/api\/v1\/public\//);
      const headers = await request.allHeaders();
      expect(headers.authorization).toBe('Bearer ' + this.publicKey);
      expect(headers.cookie).toBeUndefined(); expect(headers['x-csrf-token']).toBeUndefined(); expect(headers['x-xsrf-token']).toBeUndefined();
    }
    const storage = await this.page.evaluate(() => JSON.stringify([Object.entries(localStorage), Object.entries(sessionStorage)]));
    expect(storage).not.toContain(this.publicKey);
  }
  async noOverflow() { expect(await this.page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true); }
  async failNextReckonerCalculation() { await this.page.route('**/api/v1/public/quotes/calculate', route => route.fulfill({ status: 503, headers: { 'Access-Control-Allow-Origin': '*' }, contentType: 'application/problem+json', body: JSON.stringify({ type: 'provider-unavailable', detail: 'Unavailable' }) }), { times: 1 }); }
  async retryReckoner() { await this.click('Retry estimate'); await expect(this.page.getByRole('complementary', { name: 'Your estimate' })).toBeFocused(); }
  async mirrorWithoutRecalculation() {
    const count = this.reckonerRequests.length;
    await this.page.evaluate(() => document.documentElement.dir = 'rtl');
    const region = this.page.locator('reckoner-angular-quote');
    if ((await region.boundingBox())!.width >= 768) {
      const form = await region.getByRole('textbox', { name: 'Session date', exact: true }).boundingBox();
      const estimate = await region.getByRole('complementary', { name: 'Your estimate' }).boundingBox();
      expect(estimate!.x).toBeLessThan(form!.x);
    }
    await this.page.waitForTimeout(400); expect(this.reckonerRequests.length).toBe(count);
  }
  private async ready() {
    await expect(this.page.getByRole('heading', { name: 'Your session, thoughtfully priced.' })).toBeVisible();
    await expect(this.page.getByTestId('quote-status').or(
      this.page.getByRole('status').filter({ hasText: /This calculator is not configured|Quotes are not available yet|Unable to load the calculator/ }),
    )).toBeVisible();
  }
  async startsAtTop() {
    await expect(this.page).toHaveURL(this.origin + "/quote");
    await this.ready();
    await expect
      .poll(() => this.page.evaluate(() => [window.scrollX, window.scrollY]))
      .toEqual([0, 0]);
    await expect(
      this.page.getByRole("heading", {
        name: "Your session, thoughtfully priced.",
      }),
    ).toBeInViewport();
  }
  async scrollDown() {
    await this.page.evaluate(() => window.scrollTo(0, 250));
    await expect.poll(() => this.page.evaluate(() => window.scrollY)).toBe(250);
    return 250;
  }
  async forwardToQuote(position: number) {
    await this.page.goForward();
    await expect(this.page).toHaveURL(this.origin + "/quote");
    await this.ready();
    // History restoration can round fractional CSS pixels in Firefox.
    await expect
      .poll(() =>
        this.page.evaluate(
          (saved) => Math.abs(window.scrollY - saved),
          position,
        ),
      )
      .toBeLessThanOrEqual(1);
  }

  async reckonerRegion() { await expect(this.page.locator('reckoner-angular-quote')).toBeVisible(); await expect(this.page.getByRole('heading', { name: 'A little clarity, before we begin.', exact: true })).toBeVisible(); }
  async completeSession() { await this.choose('Photography service', 'wedding'); await this.fill('Start time', '10:00'); await this.fill('End time', '14:00'); }
  async fill(label: string, value: string) { await this.page.getByLabel(label, { exact: true }).fill(value); }
  async blur(label: string) { await this.page.getByLabel(label, { exact: true }).press('Tab'); }
  async choose(label: string, value: string) { await this.page.getByLabel(label, { exact: true }).selectOption(value); }
  async click(name: string) { await this.page.getByRole('button', { name, exact: true }).click(); }
  async search(address = 'Venue') { await this.fill('Find a location', address); await this.click('Find address'); }
  async add() { await this.search(); await this.click('Venue A'); }
  async amount(value: string) { await expect(this.page.getByTestId('quote-total')).toHaveText(new Intl.NumberFormat('en-CA', { style: 'currency', currency: 'CAD' }).format(Number(value))); }
  async noAmount() { await expect(this.page.getByTestId('quote-total')).toHaveCount(0); }
  async message(value: string) { await expect(this.page.getByText(value, { exact: false }).first()).toBeVisible(); }
  async value(label: string, value: string) { await expect(this.page.getByLabel(label, { exact: true })).toHaveValue(value); }
  async candidate(name: string, visible = true) { await expect(this.page.getByRole('button', { name, exact: true })).toHaveCount(visible ? 1 : 0); }
  async expectCalls(count: number) { await expect.poll(() => this.calls.length).toBe(count); }
  async settled(count: number, lookup = false) { await expect.poll(() => this.page.evaluate(lookup => (window as any)[lookup ? '__lookupSettled' : '__quoteSettled'], lookup)).toBe(count); }
  async capture(path: string) { await this.page.screenshot({ path, fullPage: true }); }
  async addressFocused() { await expect(this.page.getByLabel('Parking (CAD)', { exact: true }).first()).toBeFocused(); }
  async invalidField(label: string) { await this.blur(label); await expect(this.page.getByLabel(label, { exact: true })).toHaveAttribute('aria-invalid', 'true'); }
  async fillLocation(index: number, label: string, value: string) { await this.page.getByRole('group', { name: 'Location ' + index, exact: true }).getByLabel(label, { exact: true }).fill(value); }
  async chooseLocation(index: number, value: string) { await this.page.getByRole('group', { name: 'Location ' + index, exact: true }).getByLabel('Studio', { exact: true }).selectOption(value); }
  async chooseStudio(index: number, name: string) { await this.page.getByRole('group', { name: 'Location ' + index, exact: true }).getByLabel('Studio', { exact: true }).selectOption({ label: name }); }
  async line(label: string, amount: string) { await expect(this.page.getByRole('row').filter({ has: this.page.getByRole('rowheader', { name: label, exact: true }) })).toContainText(new Intl.NumberFormat('en-CA', { style: 'currency', currency: 'CAD' }).format(Number(amount))); }
  async applyCode(value: string) { await this.fill('Discount or loyalty code', value); await this.page.getByLabel('Discount or loyalty code').press('Enter'); }
  async layoutAndKeyboard() { expect(await this.page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true); await this.page.getByLabel('Find a location', { exact: true }).focus(); await this.page.keyboard.press('Tab'); await expect(this.page.getByRole('button', { name: 'Find address', exact: true })).toBeFocused(); await this.page.keyboard.press('Enter'); await this.candidate('Venue A'); }
}
