import { expect, Page } from "@playwright/test";

export class SettingsPage {
  constructor(readonly page: Page) {}
  async open(path: string) {
    await this.page.goto(`http://localhost:4421/${path}`);
  }
  async fill(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).fill(value);
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
