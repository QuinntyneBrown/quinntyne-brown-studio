import { expect } from '@playwright/test';

/** The isolated preview screen: one example, no catalog chrome. */
export class PreviewPage {
  constructor(page) {
    this.page = page;
    this.content = page.locator('#preview');
    this.navigation = page.getByRole('navigation', { name: 'Catalog' });
  }

  async openComponent(id, example) {
    await this.page.goto(`/preview.html?type=component&id=${id}&example=${example}`);
  }

  async openScenario(type, id, scenario) {
    await this.page.goto(`/preview.html?type=${type}&id=${id}&scenario=${scenario}`);
  }

  async openUnknown() {
    await this.page.goto('/preview.html?type=component&id=not-a-component&example=none');
  }

  async expectRendered() {
    await expect(this.content.locator(':scope > *')).not.toHaveCount(0);
    await expect(this.navigation).toHaveCount(0);
  }

  async expectUnknownNotice() {
    await expect(this.content.getByRole('status')).toContainText('not in the component manifest');
  }

  async expectBlogListing(columns) {
    await expect(this.content.getByRole('heading', { name: 'Blog', level: 1 })).toBeVisible();
    await expect(this.content.getByText('From the studio', { exact: true })).toBeVisible();
    await expect(this.content.locator('.blog-post')).toHaveCount(6);
    const renderedColumns = await this.content.locator('.blog-grid').evaluate((element) =>
      getComputedStyle(element).gridTemplateColumns.split(' ').filter(Boolean).length,
    );
    expect(renderedColumns).toBe(columns);
    const overflow = await this.page.evaluate(
      () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
    );
    expect(overflow).toBeLessThanOrEqual(1);
  }
}
