import { readFile } from 'node:fs/promises';
import { expect, test } from '@playwright/test';
import { CatalogPage } from './pages/catalog-page.js';
import { PreviewPage } from './pages/preview-page.js';

const manifest = JSON.parse(await readFile(new URL('../component-manifest.json', import.meta.url), 'utf8'));

/**
 * AC-L2-046-01, AC-L2-047-01: Given the published catalog, when a contributor
 * browses it, then every catalogued component is listed with a rendered example.
 */
test('the catalog lists every component in the manifest', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/');
  await catalog.expectHeading('A quiet, considered foundation.');
  await catalog.expectCoverage('components', 'Components', String(manifest.components.length));
  const patternStates = manifest.patterns.reduce((total, family) => total + family.scenarios.length, 0);
  await catalog.expectCoverage('patterns', 'Screen patterns', String(patternStates));
  const dialogScenarios = manifest.dialogs.reduce((total, family) => total + family.scenarios.length, 0);
  await catalog.expectCoverage('dialogs', 'Dialog scenarios', String(dialogScenarios));
  for (const component of manifest.components) {
    await expect(catalog.card('component', component.id)).toHaveCount(1);
  }
  for (const family of manifest.patterns) {
    await expect(catalog.card('pattern', family.id)).toHaveCount(1);
  }
  for (const family of manifest.dialogs) {
    await expect(catalog.card('dialog', family.id)).toHaveCount(1);
  }
});

/**
 * AC-L2-047-01, AC-L2-048-01: Given no studio backend, when every component
 * example is opened, then each renders and no product API request is made.
 */
test('every component example renders without a studio backend', async ({ page }) => {
  const catalog = new CatalogPage(page);
  const errors = [];
  page.on('pageerror', (error) => errors.push(error.message));
  await catalog.blockProductApi();
  for (const component of manifest.components) {
    for (const example of component.examples) {
      await catalog.open(`/components/${component.id}?example=${example.id}`);
      await catalog.expectHeading(component.name);
      await catalog.expectRenderedExample();
    }
  }
  expect(errors).toEqual([]);
  expect(catalog.productRequests).toBe(0);
});

/**
 * AC-L2-047-01, AC-L2-048-01: Given no studio backend, when every screen
 * pattern and dialog scenario is opened, then each renders in isolation.
 */
test('every pattern and dialog scenario renders without a studio backend', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.blockProductApi();
  for (const family of manifest.patterns) {
    for (const scenario of family.scenarios) {
      await catalog.open(`/patterns/${family.id}/${scenario.id}`);
      await catalog.expectHeading(family.name);
      await catalog.expectRenderedExample();
    }
  }
  for (const family of manifest.dialogs) {
    for (const scenario of family.scenarios) {
      await catalog.open(`/dialogs/${family.id}/${scenario.id}`);
      await catalog.expectHeading(family.name);
      await catalog.expectRenderedExample();
    }
  }
  expect(catalog.productRequests).toBe(0);
});

/**
 * AC-L2-066-01: Given the dialog example, when it is opened and Escape is
 * pressed, then the dialog closes and focus returns to the trigger.
 */
test('the dialog example supports Escape and returns keyboard focus', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/components/dialog');
  await catalog.openDialog();
  await expect(catalog.dialog()).toBeVisible();
  await catalog.pressEscape();
  await expect(catalog.dialog()).toBeHidden();
  await expect(catalog.trigger('Open dialog')).toBeFocused();
});

/**
 * AC-L2-046-01: Given a shared deep link, when it is opened directly, then the
 * catalog renders that entry, and its navigation continues to work.
 */
test('deep links open a single entry and keep the catalog navigable', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/components/photo-grid?example=unavailable');
  await catalog.expectHeading('Photo grid');
  await expect(catalog.example.getByText('Photo unavailable')).toBeVisible();
  await catalog.selectExample('Selection');
  await expect(page).toHaveURL(/\/components\/photo-grid\?example=selectable$/);
  await catalog.openFromNavigation('Quote calculator');
  await catalog.expectHeading('Quote calculator');
  await catalog.selectScenario('Rates not configured');
  await expect(catalog.example.getByRole('status')).toContainText('Quoting is unavailable');
});

/**
 * AC-L2-046-01: Given an entry that is not in the manifest, when it is
 * requested, then the catalog says so instead of rendering an empty page.
 */
test('an unknown entry reports that it is not catalogued', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/components/not-a-component');
  await catalog.expectHeading('That catalog entry does not exist.');
});

/**
 * AC-L2-047-01: Given a component example, when it is opened on its own, then
 * it renders without the catalog chrome for isolated review.
 */
test('an example can be reviewed in isolation', async ({ page }) => {
  const preview = new PreviewPage(page);
  await preview.openComponent('button', 'variants');
  await preview.expectRendered();
  await expect(page.getByRole('button', { name: 'Primary action' })).toBeVisible();
  await preview.openUnknown();
  await preview.expectUnknownNotice();
});

/**
 * AC-L2-047-01: Given a screen pattern or dialog scenario, when its isolated
 * preview is opened from the catalog, then it renders on its own.
 */
test('a pattern and a dialog scenario can be reviewed in isolation', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/patterns/print-review/inbox');
  await catalog.openIsolatedPreview();
  await expect(page).toHaveURL(/preview\.html\?type=pattern&id=print-review&scenario=inbox$/);

  const preview = new PreviewPage(page);
  await preview.expectRendered();
  await expect(page.getByRole('heading', { name: 'Print requests', level: 1 })).toBeVisible();

  await preview.openScenario('dialog', 'confirm', 'destructive');
  await preview.expectRendered();
  await expect(page.getByRole('button', { name: 'Delete photographs' })).toBeVisible();
});

/**
 * AC-L2-001-01, AC-L2-002-01: Given each supported viewport, when the catalog
 * is browsed, then content stays within the viewport width.
 */
test('the catalog fits the viewport without horizontal scrolling', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/components/form');
  await catalog.expectRenderedExample();
  const overflow = await page.evaluate(
    () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
  );
  expect(overflow).toBeLessThanOrEqual(1);
});

/**
 * AC-L2-046-01: Given the foundations page, when it is opened, then the
 * authoritative token values are readable.
 */
test('foundations publish the authoritative token values', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/foundations');
  await catalog.expectHeading('Design tokens');
  await expect(page.locator('[data-token="--ink"]')).toContainText('#242620');
  await expect(page.locator('[data-token="--accent"]')).toContainText('#5b654c');
});

/**
 * AC-L2-001-01, AC-L2-002-01: Given a narrow viewport, when an entry is opened,
 * then its content starts on the first screen and the catalog list is one tap away.
 */
test('a narrow viewport meets the content before the catalog list', async ({ page }) => {
  const catalog = new CatalogPage(page);
  await catalog.open('/components/photo-grid');
  const narrow = catalog.isNarrow();
  await catalog.expectNavigationCollapsed(narrow);
  await catalog.expectContentInFirstScreen();
  if (narrow) {
    await catalog.openNavigation();
    await expect(catalog.navigation.getByRole('link', { name: 'Hero', exact: true })).toBeVisible();
  }
});
