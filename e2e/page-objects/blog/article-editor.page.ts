import { Page, Locator, expect } from '@playwright/test';

export class ArticleEditorPage {
  readonly page: Page;
  readonly titleInput: Locator;
  readonly abstractInput: Locator;
  readonly bodyEditor: Locator;
  readonly saveDraftButton: Locator;
  readonly publishButton: Locator;
  readonly statusBadge: Locator;
  readonly slugText: Locator;
  readonly deleteButton: Locator;
  readonly featuredImageButton: Locator;
  readonly featuredImagePreview: Locator;
  readonly removeFeaturedImageButton: Locator;

  constructor(page: Page, readonly origin: string) {
    this.page = page;
    this.titleInput = page.locator('input[name="title"]');
    this.abstractInput = page.locator('textarea[name="abstract"]');
    this.bodyEditor = page.locator('textarea[name="body"]');
    this.saveDraftButton = page.locator('[data-testid="save-draft-btn"]');
    this.publishButton = page.locator('[data-testid="publish-btn"]');
    this.statusBadge = page.locator('[data-testid="toolbar-badge"]');
    this.slugText = page.locator('[data-testid="article-slug"]');
    this.deleteButton = page.locator('[data-testid="delete-btn"]');
    this.featuredImageButton = page.locator('[data-testid="featured-image-button"]');
    this.featuredImagePreview = page.locator('[data-testid="featured-image-preview"]');
    this.removeFeaturedImageButton = page.locator('[data-testid="remove-featured-image"]');
  }

  async goto(id?: string) {
    if (id) {
      await this.page.goto(`${this.origin}/blog/admin/articles/edit/${id}`);
    } else {
      await this.page.goto(`${this.origin}/blog/admin/articles/create`);
    }
  }

  async fillArticle(title: string, body: string, abstract: string) {
    await this.titleInput.fill(title);
    await this.abstractInput.fill(abstract);
    await this.bodyEditor.fill(body);
  }

  async save() {
    const url = this.page.url();
    if (url.includes('/create')) {
      await this.page.locator('[data-testid="create-btn"]').click();
    } else {
      await this.saveDraftButton.click();
    }
  }

  async publish() {
    await this.publishButton.click();
  }

  async unpublish() {
    await this.publishButton.click();
  }

  async delete() {
    await this.deleteButton.click();
  }

  async uploadFeaturedImage(file: string) {
    await this.featuredImageButton.click();
    await this.page.locator('#chooser-file-input').setInputFiles(file);
    await this.page.locator('#upload-btn').click();
    await this.page.locator('#image-chooser-modal').waitFor({ state: 'hidden' });
  }

  async confirmDelete() {
    await this.delete();
    await this.page.locator('#delete-modal').getByRole('button', { name: 'Delete', exact: true }).click();
  }

  async openStudioLogin() {
    await this.goto();
    await this.page.waitForURL('**/admin/login?returnUrl=*');
  }

  async assertUsableLayout() {
    const width = this.page.viewportSize()!.width;
    const bounds = await this.titleInput.boundingBox();
    expect(bounds!.width).toBeGreaterThan(Math.min(280, width - 80));
    expect(bounds!.x + bounds!.width).toBeLessThanOrEqual(width);
  }

  async capture(path: string) {
    await this.page.evaluate(() => window.scrollTo(0, 0));
    await this.page.screenshot({ path, fullPage: true, animations: "disabled" });
  }

  async getStatusText() {
    return this.statusBadge.textContent();
  }

  async getSlugText() {
    return this.slugText.textContent();
  }

  async getValidationErrorTexts(): Promise<string[]> {
    const errors = this.page.locator('.form-error');
    return errors.allTextContents();
  }
}

/** @deprecated Use ArticleEditorPage */
export { ArticleEditorPage as AdminArticleEditorPage };
