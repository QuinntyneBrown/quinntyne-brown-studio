import { expect } from '@playwright/test';

/** The catalog screen: navigation, entry cards, examples, and dialogs. */
export class CatalogPage {
  constructor(page) {
    this.page = page;
    this.navigation = page.getByRole('navigation', { name: 'Catalog' });
    this.navigationDisclosure = page.locator('#catalog-navigation');
    this.heading = page.getByRole('heading', { level: 1 });
    this.coverage = page.getByLabel('Catalog coverage');
    this.example = page.locator('[data-example-frame]');
    this.examples = page.getByRole('navigation', { name: 'Examples' });
    this.scenarios = page.getByRole('navigation', { name: 'Scenarios' });
    this.markup = page.locator('.example__code pre code');
  }

  async open(path = '/') {
    await this.page.goto(path);
  }

  async blockProductApi() {
    this.productRequests = 0;
    await this.page.route('**/api/**', (route) => {
      this.productRequests += 1;
      return route.abort();
    });
  }

  async expectHeading(name) {
    await expect(this.page.getByRole('heading', { name, exact: true, level: 1 })).toBeVisible();
  }

  async expectCoverage(kind, label, value) {
    const entry = this.coverage.locator(`[data-coverage="${kind}"]`);
    await expect(entry.getByRole('term')).toHaveText(label);
    await expect(entry.getByRole('definition')).toHaveText(value);
  }

  card(kind, id) {
    return this.page.locator(`[data-${kind}-card="${id}"]`);
  }

  /** The list collapses below the sidebar breakpoint, so open it when it is closed. */
  async openFromNavigation(name) {
    if (!(await this.navigationDisclosure.evaluate((element) => element.open))) {
      await this.openNavigation();
    }
    await this.navigation.getByRole('link', { name, exact: true }).click();
  }

  isNarrow() {
    return this.page.viewportSize().width <= 850;
  }

  async selectExample(title) {
    await this.examples.getByRole('link', { name: title, exact: true }).click();
  }

  async selectScenario(name) {
    await this.scenarios.getByRole('link', { name, exact: true }).click();
  }

  async expectRenderedExample() {
    await expect(this.example).toBeVisible();
    await expect(this.example.locator(':scope > *')).not.toHaveCount(0);
  }

  async showMarkup() {
    await this.page.getByRole('group', { name: 'Markup' }).click();
  }

  async openIsolatedPreview() {
    await this.page.getByRole('link', { name: /on its own$/ }).click();
  }

  trigger(name) {
    return this.page.getByRole('button', { name, exact: true });
  }

  dialog() {
    return this.page.getByRole('dialog');
  }

  async openDialog(name = 'Open dialog') {
    await this.trigger(name).click();
  }

  async expectContentInFirstScreen() {
    const box = await this.heading.boundingBox();
    const viewport = this.page.viewportSize();
    expect(box).not.toBeNull();
    expect(box.y).toBeLessThan(viewport.height);
  }

  async expectNavigationCollapsed(collapsed) {
    await expect(this.navigationDisclosure).toHaveJSProperty('open', !collapsed);
  }

  async openNavigation() {
    await this.navigationDisclosure.getByText('Catalog', { exact: true }).click();
  }

  async pressEscape() {
    await this.page.keyboard.press('Escape');
  }
}
