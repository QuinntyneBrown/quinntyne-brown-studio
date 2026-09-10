import { Page, Locator, expect } from '@playwright/test';

export class PublicArticleDetailPage {
  readonly page: Page;
  readonly article: Locator;
  readonly title: Locator;
  readonly body: Locator;
  readonly featuredImage: Locator;
  readonly publishDate: Locator;
  readonly readingTime: Locator;
  readonly articleMeta: Locator;
  readonly backLink: Locator;

  // Legacy aliases
  readonly articleTitle: Locator;
  readonly articleBody: Locator;

  constructor(page: Page, readonly origin: string) {
    this.page = page;
    this.article = page.locator('article');
    this.title = page.locator('.article-title');
    this.body = page.locator('.article-body');
    this.featuredImage = page.locator('.article-featured-image img');
    this.publishDate = page.locator('.article-meta time');
    this.readingTime = page.locator('.article-meta');
    this.articleMeta = page.locator('.article-meta');
    this.backLink = page.locator('.back-link');

    // Legacy aliases
    this.articleTitle = this.title;
    this.articleBody = this.body;
  }

  async goto(slug: string) {
    return this.page.goto(`${this.origin}/blog/articles/${slug}`);
  }

  async assertImageLoaded() {
    await expect.poll(() => this.featuredImage.evaluate(image => (image as HTMLImageElement).naturalWidth)).toBeGreaterThan(0);
  }

  async getTitleText() {
    return (await this.title.textContent()) ?? '';
  }

  async getBodyHtml() {
    return (await this.body.innerHTML()) ?? '';
  }

  async getReadingTimeText() {
    return (await this.readingTime.textContent()) ?? '';
  }

  async getTitle() {
    return this.title.textContent();
  }

  async clickBack() {
    await this.backLink.click();
  }
}
