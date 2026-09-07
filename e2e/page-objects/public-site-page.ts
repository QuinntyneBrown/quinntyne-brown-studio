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
  async navigate(label: string) {
    await this.page
      .getByRole("navigation", { name: "Main navigation" })
      .getByRole("link", { name: label, exact: true })
      .click();
  }
  async planSession() {
    await this.page.getByRole("link", { name: "Plan a session" }).click();
  }
  async openGallery(title: string) {
    await this.page.getByRole("link", { name: title }).first().click();
  }
  /** Public photographs share a name derived from their gallery, so the first one is opened. */
  async viewPhoto(name: string) {
    await this.page
      .getByRole("button", { name: `View ${name}`, exact: true })
      .first()
      .click();
    await expect(
      this.page.getByRole("dialog", { name, exact: true }),
    ).toBeVisible();
  }
  async closeDialog() {
    await this.page
      .getByRole("button", { name: "Close dialog", exact: true })
      .click();
  }
  async heroImageLoaded() {
    await expect(this.page.locator(".hero__image img")).toBeVisible();
    await expect
      .poll(() =>
        this.page
          .locator(".hero__image img")
          .evaluate(
            (image: HTMLImageElement) =>
              image.complete && image.naturalWidth > 0,
          ),
      )
      .toBe(true);
  }
  async capture(path: string, width = 1440, height = 900) {
    await this.page.setViewportSize({ width, height });
    await this.page.screenshot({ path, fullPage: true });
  }
}
