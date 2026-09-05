import { expect, Page } from '@playwright/test';

export class StudioPage {
  constructor(readonly page: Page) {}
  async open(site: 'marketing' | 'admin' | 'client' | 'design-system', path = '') {
    await this.page.goto(
      `http://localhost:${{ marketing: 4300, admin: 4301, client: 4302, 'design-system': 4303 }[site]}/${path}`,
    );
  }
  async mock(role = 'Administrator') {
    await this.page.route('**/api/**', async (route) => {
      const path = new URL(route.request().url()).pathname;
      const response = path.endsWith('/antiforgery')
        ? { requestToken: 'controlled-token' }
        : path.endsWith('/auth/session')
          ? { authenticated: true, id: 'client-a', roles: [role] }
          : path.includes('/content/')
            ? {
                heading: 'Photography with feeling.',
                body: 'Honest moments. Thoughtfully captured.',
              }
            : [];
      await route.fulfill({ json: response });
    });
  }
  async heading(text: string) {
    await expect(this.page.getByRole('heading', { name: text, exact: true })).toBeVisible();
  }
  async fill(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).fill(value);
  }
  async click(label: string) {
    await this.page.getByRole('button', { name: label, exact: true }).click();
  }
  async message(text: string) {
    await expect(this.page.getByRole('status')).toContainText(text);
  }
  async resolveLocation() {
    await this.fill('Address', 'Example venue');
    await this.click('Find address');
    await this.page.getByRole('button', { name: 'Example venue · Select', exact: true }).click();
  }
  async quoteAmount(amount: string) {
    await expect(this.page.locator('.price__total')).toContainText(amount);
  }
}
