import { expect, Page } from "@playwright/test";

export class PublicSitePage {
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4420",
  ) {}
  async open(path = "") {
    await this.page.goto(`${this.origin}/${path}`);
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async retry() {
    await this.page
      .getByRole("button", { name: "Retry loading", exact: true })
      .click();
  }
  async heading(name: string) {
    await expect(
      this.page.getByRole("heading", { name, exact: true }),
    ).toBeVisible();
  }
  async absent(text: string) {
    await expect(this.page.getByText(text, { exact: true })).toHaveCount(0);
  }
  async visiblePhotos(count: number) {
    await expect(this.page.locator(".photo-grid img")).toHaveCount(count);
  }
  async capture(path: string, width = 1440, height = 900) {
    await this.page.setViewportSize({ width, height });
    await this.page.screenshot({ path, fullPage: true });
  }
}
