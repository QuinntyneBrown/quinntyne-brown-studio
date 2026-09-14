import { expect, Page } from "@playwright/test";

export class SettingsPage {
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4421",
  ) {}
  async open(path: string) {
    await this.page.goto(`${this.origin}/${path}`);
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
  /** A select wrapped by its label carries the chosen option in its name, so it is found by role. */
  async choose(label: string, value: string) {
    await this.page
      .getByRole("combobox", { name: label, exact: true })
      .selectOption(value);
  }
  async check(label: string, checked = true) {
    await this.page.getByLabel(label, { exact: true }).setChecked(checked);
  }
  /**
   * The editor stays mounted across Edit, so a record's flag lands in its checkbox one
   * change-detection cycle after the click. Assert it before toggling: `setChecked` treats
   * a box that already shows the wanted state as done, and would skip a click that is
   * still pending.
   */
  async checked(label: string, checked = true) {
    await expect(this.page.getByLabel(label, { exact: true })).toBeChecked({
      checked,
    });
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
