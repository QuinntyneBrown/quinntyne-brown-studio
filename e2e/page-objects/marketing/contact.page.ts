import { expect, type Locator, type Page } from "@playwright/test";
import { MarketingShellPage } from "./marketing-shell.page";

/** The Contact page the API renders at /contact, with its plain form post. */
export class ContactPage {
  readonly shell: MarketingShellPage;
  readonly form: Locator;

  constructor(
    readonly page: Page,
    readonly origin: string,
  ) {
    this.shell = new MarketingShellPage(page);
    this.form = page.locator("form.marketing-form");
  }

  open() {
    return this.page.goto(`${this.origin}/contact`);
  }

  async heading(text: string) {
    await expect(this.page.getByRole("heading", { level: 1, name: text })).toBeVisible();
  }

  private row(label: string) {
    return this.page
      .locator(".contact-card .detail-list__row")
      .filter({ has: this.page.locator("dt", { hasText: label }) });
  }

  async detail(label: string, value: string) {
    await expect(this.row(label).locator("dd")).toContainText(value);
  }

  async noDetail(label: string) {
    await expect(this.row(label)).toHaveCount(0);
  }

  async studio(name: string) {
    await expect(this.page.locator(".place").filter({ hasText: name })).toBeVisible();
  }

  async emptyStudios() {
    await expect(
      this.page.getByRole("heading", { name: "Studio spaces coming soon", exact: true }),
    ).toBeVisible();
  }

  async onLocation() {
    await this.studio("On location");
  }

  async formVisible() {
    await expect(this.form).toBeVisible();
    await expect(this.sendButton()).toBeVisible();
  }

  field(name: string) {
    return this.form.locator(`[name="${name}"]`);
  }

  async fill(values: Partial<Record<"name" | "email" | "phone" | "message", string>>) {
    for (const [name, value] of Object.entries(values)) await this.field(name).fill(value);
  }

  async interest(label: string) {
    await this.field("interest").selectOption({ label });
  }

  async consent(checked = true) {
    await this.field("consent").setChecked(checked);
  }

  sendButton() {
    return this.form.getByRole("button", { name: /Send your message/ });
  }

  async send() {
    await this.sendButton().click();
  }

  async value(name: string, value: string) {
    await expect(this.field(name)).toHaveValue(value);
  }

  async fieldError(name: string) {
    await expect(this.page.locator(`#error-${name}`)).toBeVisible();
    await expect(this.field(name)).toHaveAttribute("aria-invalid", "true");
  }

  /** The confirmation shown after a stored message; returns the visible reference. */
  async confirmation() {
    const notice = this.page.locator(".marketing-notice-success");
    await expect(notice).toBeVisible();
    const match = /Reference (QB-IN-\d+)/.exec((await notice.textContent()) ?? "");
    if (!match) throw new Error("The confirmation carries no reference.");
    await this.value("name", "");
    return match[1];
  }

  /** Every control is reachable in order from the keyboard; the hidden field is skipped. */
  async keyboardReachesEveryControl() {
    await this.field("name").focus();
    for (const expected of ["email", "phone", "interest", "message", "consent"]) {
      await this.page.keyboard.press("Tab");
      await expect(this.field(expected)).toBeFocused();
    }
    await this.page.keyboard.press("Tab");
    await expect(this.sendButton()).toBeFocused();
    const outline = await this.sendButton().evaluate((element) => getComputedStyle(element).outlineStyle);
    expect(outline, "keyboard focus is visible").not.toBe("none");
  }

  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
}
