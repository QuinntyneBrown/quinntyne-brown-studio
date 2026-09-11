import { test, expect } from '@playwright/test';
import { resolve } from 'node:path';
import { AccountPage } from '../page-objects/account-page';
import { ArticleEditorPage } from '../page-objects/blog/article-editor.page';
import { PublicArticleDetailPage } from '../page-objects/blog/article-detail.page';

// AC-L2-070-01 through AC-L2-070-06. Imported page objects drive real Razor pages,
// studio Identity, persistent media, and the isolated LocalDB database.
for (const width of [390, 768, 1440]) {
  test(`Blog publishing and media at ${width}px`, async ({ page, browser }, info) => {
    await page.setViewportSize({ width, height: 900 });
    const origin = process.env.QBS_SMOKE_ORIGIN ?? 'https://localhost:7453';
    const email = process.env.Bootstrap__Email;
    const password = process.env.Bootstrap__Password;
    if (!email || !password) throw new Error('Supply isolated smoke administrator credentials.');
    const editor = new ArticleEditorPage(page, origin);
    await editor.openStudioLogin();
    await new AccountPage(page, origin).login(email, password);
    await expect(page).toHaveURL(/\/blog\/admin\/articles\/create$/);
    const title = `Portrait light ${width} ${Date.now()}`;
    await editor.fillArticle(title, '# Golden hour\nA portrait photographed in warm evening light.', 'A studio portrait story.');
    await editor.uploadFeaturedImage(resolve('../docs/mocks/assets/photos/portrait.jpg'));
    await expect(editor.featuredImagePreview).toBeVisible();
    await editor.save();
    await expect(page).toHaveURL(/\/blog\/admin\/articles\/edit\//);
    await expect(editor.statusBadge).toHaveText('Draft');
    await editor.publish();
    await expect(editor.statusBadge).toHaveText('Published');
    await editor.assertUsableLayout();
    await editor.capture(info.outputPath('editor.png'));
    const slug = title.toLowerCase().replaceAll(' ', '-');
    const visitor = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width, height: 900 } });
    try {
      const publicPage = await visitor.newPage();
      const article = new PublicArticleDetailPage(publicPage, origin);
      expect((await article.goto(slug))?.status()).toBe(200);
      await expect(article.title).toHaveText(title);
      await expect(article.body).toContainText('Golden hour');
      await expect(article.featuredImage).toBeVisible();
      await article.assertImageLoaded();
      await publicPage.screenshot({ path: info.outputPath('article.png'), fullPage: true });
      await editor.unpublish();
      await expect(editor.statusBadge).toHaveText('Draft');
      expect((await article.goto(slug))?.status()).toBe(404);
      await editor.confirmDelete();
      await expect(page).toHaveURL(/\/blog\/admin\/articles$/);
    } finally {
      await visitor.close();
    }
  });
}
