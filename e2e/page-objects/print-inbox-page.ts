import { expect, Page } from "@playwright/test";
export class PrintInboxPage {
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4421",
  ) {}
  async open() {
    await this.page.goto(this.origin + "/print-requests");
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async noEmpty() {
    await expect(
      this.page.getByText("No print requests yet.", { exact: true }),
    ).toHaveCount(0);
  }
  async retry() {
    await this.page
      .getByRole("button", { name: "Retry loading", exact: true })
      .click();
  }
  async click(name: string) {
    await this.page.getByRole("button", { name, exact: true }).click();
  }
  async filter(state: string) {
    await this.page
      .getByLabel("Request status", { exact: true })
      .selectOption(state);
    await expect(
      this.page.getByLabel("Request status", { exact: true }),
    ).toBeEnabled();
  }
  async openRequest(id: string) {
    await this.page
      .locator(".records__row")
      .filter({ hasText: id })
      .getByRole("button", { name: "Open request", exact: true })
      .click();
  }
  async requestCount(count: number) {
    await expect(
      this.page.getByRole("button", { name: "Open request", exact: true }),
    ).toHaveCount(count);
  }
}
