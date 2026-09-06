import { StudioFixture } from "./studio-fixture";
import { expect, Page } from "@playwright/test";

export class StudioPage {
  constructor(readonly page: Page) {}
  async open(site: "marketing" | "admin" | "client", path = "") {
    await this.page.goto(
      `http://localhost:${{ marketing: 4420, admin: 4421, client: 4422 }[site]}/${path}`,
    );
  }
  readonly fixture = new StudioFixture();
  async mock(role = "Administrator") {
    this.fixture.role = role;
    await this.fixture.install(this.page.context());
  }
  async quoteLink() {
    await expect(
      this.page.getByRole("link", { name: "Calculate your quote" }),
    ).toBeVisible();
  }
  async value(label: string, value: string) {
    await expect(this.page.getByLabel(label, { exact: true })).toHaveValue(
      value,
    );
  }
  async text(text: string) {
    await expect(this.page.getByText(text, { exact: true })).toBeVisible();
  }
  async heading(text: string) {
    await expect(
      this.page.getByRole("heading", { name: text, exact: true }),
    ).toBeVisible();
  }
  async fill(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).fill(value);
  }
  async click(label: string) {
    await this.page.getByRole("button", { name: label, exact: true }).click();
  }
  async message(text: string) {
    await expect(this.page.getByRole("status")).toContainText(text);
  }
  async resolveLocation() {
    await this.fill("Address", "Example venue");
    await this.click("Find address");
    await this.page
      .getByRole("button", { name: "Example venue · Select", exact: true })
      .click();
  }
  async quoteAmount(amount: string) {
    await expect(this.page.locator(".price__total")).toContainText(amount);
  }
}
