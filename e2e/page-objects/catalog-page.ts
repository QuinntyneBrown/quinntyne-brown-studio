import { expect, Page } from "@playwright/test";

export class CatalogPage {
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4421",
  ) {}
  async open(resource: string) {
    await this.page.goto(`${this.origin}/${resource}`);
  }
  async add(name: string) {
    await this.page
      .getByRole("button", { name: `Add ${name}`, exact: true })
      .click();
  }
  async fill(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).fill(value);
  }
  async select(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).selectOption(value);
  }
  async choose(label: string, optionLabel: string) {
    await this.page
      .getByLabel(label, { exact: true })
      .selectOption({ label: optionLabel });
  }
  async check(label: string, checked = true) {
    await this.page.getByLabel(label, { exact: true }).setChecked(checked);
  }
  async save() {
    await this.page.getByRole("button", { name: "Save", exact: true }).click();
  }
  async edit(name: string) {
    await this.page
      .locator(".records__row")
      .filter({ hasText: name })
      .getByRole("button", { name: "Edit", exact: true })
      .click();
  }
  async openSession(name: string) {
    await this.page
      .locator(".records__row")
      .filter({ hasText: name })
      .getByRole("link", { name: "Open session" })
      .click();
  }
  async openSchedule(name: string) {
    await this.page
      .locator(".records__row")
      .filter({ hasText: name })
      .getByRole("link", { name: "Schedule" })
      .click();
  }
  async value(label: string, value: string) {
    await expect(this.page.getByLabel(label, { exact: true })).toHaveValue(
      value,
    );
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async photo(name: string) {
    await this.page
      .getByRole("checkbox", { name: `Select ${name}`, exact: true })
      .check();
  }
  async earlier(name: string) {
    await this.page
      .getByRole("button", { name: `Move ${name} earlier`, exact: true })
      .click();
  }
  async cover(name: string) {
    await this.page
      .getByRole("button", { name: `Use ${name} as cover`, exact: true })
      .click();
  }
  async noEmpty() {
    await expect(
      this.page.getByText("No equipment yet.", { exact: true }),
    ).toHaveCount(0);
  }
  async retry() {
    await this.page
      .getByRole("button", { name: "Retry loading", exact: true })
      .click();
  }
  async draftPublication() {
    await expect(
      this.page.getByLabel("Published", { exact: true }),
    ).not.toBeChecked();
  }
}
