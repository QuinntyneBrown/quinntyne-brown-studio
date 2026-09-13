import { expect, Page } from "@playwright/test";

/** The administrator inbox of contact-page inquiries and the opened inquiry. */
export class InquiryInboxPage {
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4421",
  ) {}
  async open() {
    await this.page.goto(this.origin + "/inquiries");
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async noEmpty() {
    await expect(
      this.page.getByText("No inquiries yet.", { exact: true }),
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
      .getByLabel("Inquiry status", { exact: true })
      .selectOption(state);
    await expect(
      this.page.getByLabel("Inquiry status", { exact: true }),
    ).toBeEnabled();
  }
  async inquiryCount(count: number) {
    await expect(
      this.page.getByRole("button", { name: "Open inquiry", exact: true }),
    ).toHaveCount(count);
  }
  /** The rows in the order shown; the inbox lists the newest inquiry first. */
  async rowOrder(references: string[]) {
    const rows = this.page.locator(".records__row");
    await expect(rows).toHaveCount(references.length);
    for (const [index, reference] of references.entries())
      await expect(rows.nth(index)).toContainText(reference);
  }
  async row(reference: string, text: string) {
    await expect(
      this.page.locator(".records__row").filter({ hasText: reference }),
    ).toContainText(text);
  }
  async openInquiry(reference: string) {
    await this.page
      .locator(".records__row")
      .filter({ hasText: reference })
      .getByRole("button", { name: "Open inquiry", exact: true })
      .click();
  }
  /** A labelled value on the opened inquiry, read as text so stored markup never renders. */
  async detail(label: string, value: string) {
    await expect(
      this.page
        .locator(".detail-list__row")
        .filter({ has: this.page.locator("dt", { hasText: label }) })
        .locator("dd"),
    ).toHaveText(value);
  }
  async reviewDisabled() {
    await expect(
      this.page.getByRole("button", { name: "Mark reviewed", exact: true }),
    ).toBeDisabled();
  }
}
