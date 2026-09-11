import { Page, Locator, expect } from "@playwright/test";

/**
 * The public blog list at `/blog`. It is the page the API renders, so the checks here are
 * deliberately about its own markup: the gateway answers 200 with the marketing shell for
 * any address it does not route to the API, and a status code cannot tell the two apart.
 */
export class BlogListPage {
  readonly tag: Locator;
  readonly title: Locator;
  readonly articles: Locator;
  readonly emptyState: Locator;

  constructor(
    readonly page: Page,
    readonly origin: string,
  ) {
    this.tag = page.locator(".hero-tag");
    this.title = page.locator(".hero-title");
    this.articles = page.locator(".article-card");
    this.emptyState = page.locator(".empty-state-title");
  }

  async open() {
    return this.page.goto(`${this.origin}/blog`);
  }

  /** Published articles or the page's own empty state; both are the blog, neither is the shell. */
  async listed() {
    await expect(this.tag).toHaveText("Studio Blog");
    await expect(this.articles.first().or(this.emptyState)).toBeVisible();
  }

  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
}
