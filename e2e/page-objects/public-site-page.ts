import { BrowserContext, expect, Page } from "@playwright/test";

export class PublicSitePage {
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4420",
  ) {}
  async open(path = "") {
    await this.page.goto(`${this.origin}/${path}`);
  }
  /**
   * The blog is a page the API renders; under acceptance the dev server would answer `/blog`
   * with the application shell, so a stand-in blog page is served in its place.
   */
  async serveBlog(context: BrowserContext) {
    await context.route("**/blog", (route) =>
      route.fulfill({
        status: 200,
        contentType: "text/html",
        body: "<html><body><h1>From the studio</h1></body></html>",
      }),
    );
  }
  /** The visitor left the application for the blog rather than seeing a client-rendered page. */
  async arrivedAtBlog() {
    await expect(
      this.page.getByRole("heading", { name: "From the studio", exact: true }),
    ).toBeVisible();
    expect(new URL(this.page.url()).pathname).toBe("/blog");
  }
  /** The visitor stayed on the requested application page. */
  async stayedOn(path: string) {
    await this.loaded();
    expect(new URL(this.page.url()).pathname).toBe(path);
  }
  async brandLink(href: string) {
    await expect(this.page.locator(".shell__brand").first()).toHaveAttribute("href", href);
  }
  async navigationAbsent(label: string) {
    await expect(
      this.navigation().getByRole("link", { name: label, exact: true }),
    ).toHaveCount(0);
  }
  async footerLinkAbsent(label: string) {
    await expect(
      this.page
        .getByRole("contentinfo")
        .getByRole("link", { name: label, exact: true }),
    ).toHaveCount(0);
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  /** The page finished its published reads; a failed load offers the retry affordance instead. */
  async loaded() {
    await expect(
      this.page.getByRole("button", { name: "Retry loading", exact: true }),
    ).toHaveCount(0);
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
  navigation() {
    return this.page.getByRole("navigation", { name: "Main navigation" });
  }
  async navigate(label: string) {
    await this.navigation()
      .getByRole("link", { name: label, exact: true })
      .click();
  }
  /** On narrow viewports the navigation sits behind the Menu button; open it with Enter. */
  async openMenuFromKeyboard() {
    const toggle = this.page.getByRole("button", { name: "Menu", exact: true });
    if (!(await toggle.isVisible())) return;
    await toggle.focus();
    await this.page.keyboard.press("Enter");
    await expect(toggle).toHaveAttribute("aria-expanded", "true");
    await expect(this.navigation()).toBeVisible();
  }
  async closeMenuFromKeyboard() {
    const toggle = this.page.getByRole("button", { name: "Menu", exact: true });
    if (!(await toggle.isVisible())) return;
    await toggle.focus();
    await this.page.keyboard.press("Enter");
    await expect(toggle).toHaveAttribute("aria-expanded", "false");
    await expect(this.navigation()).toBeHidden();
  }
  async navigationOrder(labels: string[]) {
    await expect(this.navigation().locator("a")).toHaveText(labels);
  }
  async navigationLink(label: string, href: string) {
    await expect(
      this.navigation().getByRole("link", { name: label, exact: true }),
    ).toHaveAttribute("href", href);
  }
  async footerLink(label: string, href: string) {
    await expect(
      this.page
        .getByRole("contentinfo")
        .getByRole("link", { name: label, exact: true }),
    ).toHaveAttribute("href", href);
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
