import { expect, type Locator, type Page } from "@playwright/test";

/** The public image-first article listing rendered by the API. */
export class BlogListPage {
  readonly heading: Locator;
  readonly eyebrow: Locator;
  readonly introduction: Locator;
  readonly grid: Locator;
  readonly articles: Locator;
  readonly emptyState: Locator;
  readonly menuButton: Locator;
  readonly menuOpenIcon: Locator;
  readonly menuCloseIcon: Locator;
  readonly menuBackdrop: Locator;
  readonly header: Locator;
  readonly navigation: Locator;
  readonly footerSearch: Locator;

  constructor(
    readonly page: Page,
    readonly origin: string,
  ) {
    this.heading = page.getByRole("heading", { level: 1, name: "Blog" });
    this.eyebrow = page.getByText("From the studio", { exact: true });
    this.introduction = page.getByText(
      "Notes on making photographs feel easy, personal, and true to the people in them.",
      { exact: true },
    );
    this.grid = page.locator(".blog-grid");
    this.articles = page.locator(".blog-post");
    this.emptyState = page.getByRole("heading", { level: 2, name: "No articles yet" });
    this.menuButton = page.locator(".marketing-blog-menu");
    this.menuOpenIcon = this.menuButton.locator(".marketing-blog-menu-open");
    this.menuCloseIcon = this.menuButton.locator(".marketing-blog-menu-close");
    this.menuBackdrop = page.locator(".marketing-blog-backdrop");
    this.header = page.locator(".marketing-blog-header");
    this.navigation = page.getByRole("navigation", { name: "Main navigation" });
    this.footerSearch = page.getByRole("contentinfo").getByRole("link", { name: "Search articles" });
  }

  open(path: "/blog" | "/blog/articles" = "/blog") {
    return this.page.goto(`${this.origin}${path}`);
  }

  async listed() {
    await expect(this.heading).toBeVisible();
    await expect(this.eyebrow).toBeVisible();
    await expect(this.articles.first().or(this.emptyState)).toBeVisible();
  }

  article(title: string) {
    return this.articles.filter({ hasText: title });
  }

  async assertMatchesMock(width: number, title: string) {
    await this.listed();
    await expect(this.introduction).toBeVisible();
    await expect(this.article(title)).toHaveCount(1);
    await expect(this.article(title).getByRole("link")).toHaveAttribute("href", /\/blog\/articles\//);
    await expect(this.article(title).locator("img")).toBeVisible();
    await expect(this.page.locator(".article-card-abstract")).toHaveCount(0);
    await expect(this.footerSearch).toHaveAttribute("href", "/blog/search");

    const columns = await this.grid.evaluate((element) =>
      getComputedStyle(element).gridTemplateColumns.split(" ").filter(Boolean).length,
    );
    expect(columns).toBe(width <= 600 ? 1 : width <= 1100 ? 2 : 3);

    if (width <= 780) {
      await this.assertOverlayMenu();
    }

    const overflow = await this.page.evaluate(
      () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
    );
    expect(overflow).toBeLessThanOrEqual(1);
  }

  /**
   * AC-L2-070-11: the closed menu offers a menu icon; opening it dims the page
   * behind a partially opaque backdrop and turns the control into a close icon
   * that, like the backdrop itself, dismisses the overlay.
   */
  async assertOverlayMenu() {
    await expect(this.navigation).toBeHidden();
    await expect(this.menuBackdrop).toBeHidden();
    await expect(this.menuOpenIcon).toBeVisible();
    await expect(this.menuButton).toHaveAttribute("aria-label", "Open menu");

    await this.menuButton.click();
    await expect(this.navigation).toBeVisible();
    await expect(this.menuButton).toHaveAttribute("aria-expanded", "true");
    await expect(this.menuButton).toHaveAttribute("aria-label", "Close menu");
    await expect(this.menuCloseIcon).toBeVisible();
    await expect(this.menuOpenIcon).toBeHidden();
    await expect(this.menuBackdrop).toBeVisible();
    expect(await this.backdropOpacity()).toBeGreaterThan(0);
    expect(await this.backdropOpacity()).toBeLessThan(1);
    await this.assertOverlayReachesTop();

    const backdrop = await this.menuBackdrop.boundingBox();
    if (!backdrop) throw new Error("The open overlay menu has no backdrop to dismiss.");
    await this.menuBackdrop.click({ position: { x: 10, y: backdrop.height - 10 } });
    await expect(this.navigation).toBeHidden();
    await expect(this.menuBackdrop).toBeHidden();
    await expect(this.menuButton).toHaveAttribute("aria-expanded", "false");
    await expect(this.menuOpenIcon).toBeVisible();
  }

  /**
   * AC-L2-070-12: the open overlay is one unbroken light surface from the top
   * edge of the viewport. The brand and close control sit on the same undimmed
   * surface as the menu links, so the backdrop dims only the listing below.
   */
  private async assertOverlayReachesTop() {
    const header = await this.header.boundingBox();
    const backdrop = await this.menuBackdrop.boundingBox();
    if (!header || !backdrop) throw new Error("The open overlay menu has no header or backdrop.");
    expect(header.y).toBe(0);
    expect(backdrop.y).toBeGreaterThanOrEqual(header.y + header.height - 1);

    const dimmed = await this.page.evaluate((middle) => {
      const width = document.documentElement.clientWidth;
      return [1, width - 1]
        .map((x) => document.elementFromPoint(x, middle))
        .map((element) => Boolean(element?.closest(".marketing-blog-backdrop")));
    }, header.height / 2);
    expect(dimmed).toEqual([false, false]);
  }

  private async backdropOpacity() {
    const color = await this.menuBackdrop.evaluate(
      (element) => getComputedStyle(element).backgroundColor,
    );
    const alpha = /[/,]\s*([\d.]+)\s*\)$/.exec(color);
    return alpha ? Number(alpha[1]) : 1;
  }

  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
}
