import { expect, Page } from "@playwright/test";

export class SettingsPage {
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4421",
  ) {}
  async open(path: string) {
    await this.page.goto(`${this.origin}/${path}`);
  }
  async quoteAvailability() {
    await expect(
      this.page.getByRole("heading", {
        name: "Quote availability",
        exact: true,
      }),
    ).toBeVisible();
    await expect(
      this.page.getByRole("checkbox", { name: "Monday", exact: true }),
    ).toBeVisible();
  }
  async saveQuote(resource: string) {
    await this.page
      .locator("reckoner-admin-" + resource)
      .getByRole("button", { name: "Save", exact: true })
      .click();
  }
  async installClock() {
    await this.page.clock.install();
  }
  async reachRenewal() {
    await this.page.clock.fastForward(55 * 60_000);
  }
  async privateTokenOnlyInMemory() {
    const exposure = await this.page.evaluate(() => ({
      html: document.documentElement.outerHTML,
      local: JSON.stringify(localStorage),
      session: JSON.stringify(sessionStorage),
    }));
    for (const value of Object.values(exposure))
      expect(value).not.toMatch(/(?:at|sk)_[A-Za-z0-9_-]{43}/);
  }
  async noQuoteEditors() {
    await expect(
      this.page.locator(
        "qbs-studio-quote-settings reckoner-admin-availability",
      ),
    ).toHaveCount(0);
  }
  async checked(label: string, value: boolean) {
    if (value)
      await expect(this.page.getByLabel(label, { exact: true })).toBeChecked();
    else
      await expect(
        this.page.getByLabel(label, { exact: true }),
      ).not.toBeChecked();
  }
  async resolveAddress(label: string, query: string, candidate: string) {
    const input = this.page.getByLabel(label, { exact: true });
    await input.fill(query);
    await input.press("Enter");
    await this.page
      .getByRole("button", { name: candidate, exact: true })
      .click();
  }
  async beginAddress(label: string, query: string) {
    const input = this.page.getByLabel(label, { exact: true });
    await input.fill(query);
    await input.press("Enter");
  }
  async noQuoteCandidate(label: string) {
    await expect(
      this.page.getByRole("button", { name: label, exact: true }),
    ).toHaveCount(0);
  }
  async fill(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).fill(value);
  }
  async fillNth(label: string, index: number, value: string) {
    await this.page.getByLabel(label, { exact: true }).nth(index).fill(value);
  }
  async select(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).selectOption(value);
  }
  async check(label: string, checked = true) {
    await this.page.getByLabel(label, { exact: true }).setChecked(checked);
  }
  async click(label: string) {
    await this.page.getByRole("button", { name: label, exact: true }).click();
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async value(label: string, value: string) {
    await expect(this.page.getByLabel(label, { exact: true })).toHaveValue(
      value,
    );
  }
  async edit(name: string) {
    await this.page
      .locator(".records__row")
      .filter({ hasText: name })
      .getByRole("button", { name: "Edit", exact: true })
      .click();
  }
  async noCandidate(label: string) {
    await this.page.waitForFunction(() => (window as any).__qbsLookupSettled);
    await expect(
      this.page.getByRole("button", { name: label + " · Select", exact: true }),
    ).toHaveCount(0);
  }
}
