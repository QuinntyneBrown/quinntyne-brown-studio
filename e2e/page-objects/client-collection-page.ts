import { expect, Page } from "@playwright/test";
import { StudioFixture } from "./studio-fixture";

export class ClientCollectionPage {
  constructor(
    readonly page: Page,
    readonly fixture: StudioFixture,
    readonly origin = "http://localhost:4422",
  ) {}
  async open(path = "prints") {
    await this.page.goto(`${this.origin}/${path}`);
  }
  async selectPhoto(name: string) {
    await this.page
      .getByRole("checkbox", { name: `Select ${name}`, exact: true })
      .check();
  }
  async review() {
    await this.page
      .getByRole("button", { name: "Review print options", exact: true })
      .click();
  }
  async quantity(value: string) {
    await this.page.getByLabel("Quantity", { exact: true }).fill(value);
  }
  async notes(value: string) {
    await this.page.getByLabel("Notes (optional)", { exact: true }).fill(value);
  }
  async albumName(value: string) {
    await expect(
      this.page.getByLabel("Album name", { exact: true }),
    ).toHaveValue(value);
  }
  async photoOrder(names: string[]) {
    await expect(
      this.page.locator("qbs-photo-order .records__row > span"),
    ).toHaveText(names.map((name, index) => `${index + 1}. ${name}`));
  }
  async total(value: string) {
    await expect(
      this.page.getByText(`Current total: ${value} CAD`, { exact: true }),
    ).toBeVisible();
  }
  async submit() {
    await this.page
      .getByRole("button", { name: "Submit print request", exact: true })
      .click();
  }
  async submissionDisabled() {
    await expect(
      this.page.getByRole("button", {
        name: "Submit print request",
        exact: true,
      }),
    ).toBeDisabled();
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async retryReview() {
    await this.page
      .getByRole("button", { name: "Retry price review", exact: true })
      .click();
  }
  async nameAlbum(name: string) {
    await this.page.getByLabel("Album name", { exact: true }).fill(name);
  }
  async click(name: string) {
    await this.page.getByRole("button", { name, exact: true }).click();
  }
  async heading(name: string) {
    await expect(
      this.page.getByRole("heading", { name, exact: true }),
    ).toBeVisible();
  }
  async noEmpty() {
    await expect(
      this.page.getByText("No galleries available yet.", { exact: true }),
    ).toHaveCount(0);
  }
  async retry() {
    await this.page
      .getByRole("button", { name: "Retry loading", exact: true })
      .click();
  }
  async visiblePhotos(count: number) {
    await expect(this.page.locator(".photo-grid img")).toHaveCount(count);
  }
  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
}
