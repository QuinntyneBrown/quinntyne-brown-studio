import { expect, type Locator, type Page } from "@playwright/test";

/**
 * The shell around every page the API renders (the blog listing, About, Contact): the
 * navigation with its current-page marker, the overlay menu on narrow viewports, the footer,
 * and the layout invariants shared by all of them.
 */
export class MarketingShellPage {
  readonly navigation: Locator;
  readonly menuButton: Locator;
  readonly menuOpenIcon: Locator;
  readonly menuCloseIcon: Locator;
  readonly menuBackdrop: Locator;

  constructor(readonly page: Page) {
    // The overlay menu is display:none on narrow viewports, so it is located by element, not role.
    this.navigation = page.locator("nav.marketing-blog-nav");
    this.menuButton = page.locator(".marketing-blog-menu");
    this.menuOpenIcon = this.menuButton.locator(".marketing-blog-menu-open");
    this.menuCloseIcon = this.menuButton.locator(".marketing-blog-menu-close");
    this.menuBackdrop = page.locator(".marketing-blog-backdrop");
  }

  /** The canonical link the API writes; a gateway falling back to the Angular shell has none. */
  async canonical(origin: string, path: string) {
    await expect(this.page.locator('link[rel="canonical"]')).toHaveAttribute("href", origin + path);
  }

  /** The link to the page being viewed is marked current, whether or not the menu is open. */
  async current(label: string, href: string) {
    const link = this.navigation.locator(`a[href="${href}"]`);
    await expect(link).toHaveText(label);
    await expect(link).toHaveAttribute("aria-current", "page");
  }

  async footerLink(label: string, href: string) {
    await expect(
      this.page.getByRole("contentinfo").getByRole("link", { name: label, exact: true }),
    ).toHaveAttribute("href", href);
  }

  async assertNoOverflow() {
    const overflow = await this.page.evaluate(
      () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
    );
    expect(overflow).toBeLessThanOrEqual(1);
  }

  /**
   * AC-L2-070-11, AC-L2-076-02: the closed menu offers a menu icon; opening it dims the page
   * behind a partially opaque backdrop and turns the control into a close icon that, like
   * the backdrop itself, dismisses the overlay.
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

    const backdrop = await this.menuBackdrop.boundingBox();
    if (!backdrop) throw new Error("The open overlay menu has no backdrop to dismiss.");
    await this.menuBackdrop.click({ position: { x: 10, y: backdrop.height - 10 } });
    await expect(this.navigation).toBeHidden();
    await expect(this.menuBackdrop).toBeHidden();
    await expect(this.menuButton).toHaveAttribute("aria-expanded", "false");
    await expect(this.menuOpenIcon).toBeVisible();
  }

  /** The same overlay from the keyboard: Enter on the focused button opens it, Escape closes it. */
  async assertKeyboardOverlayMenu() {
    await this.menuButton.focus();
    await this.page.keyboard.press("Enter");
    await expect(this.navigation).toBeVisible();
    await expect(this.menuButton).toHaveAttribute("aria-expanded", "true");
    await this.page.keyboard.press("Escape");
    await expect(this.navigation).toBeHidden();
    await expect(this.menuButton).toHaveAttribute("aria-expanded", "false");
  }

  /** A link focused after keyboard interaction shows a visible focus ring. */
  async focusVisibleOnLink(name: RegExp) {
    await this.page.locator("body").press("Tab");
    const link = this.page.locator("main a", { hasText: name }).first();
    await link.focus();
    await expect(link).toBeFocused();
    const style = await link.evaluate((element) => ({
      visible: element.matches(":focus-visible"),
      outline: getComputedStyle(element).outlineStyle,
    }));
    expect(style.visible, "the link is focus-visible").toBe(true);
    expect(style.outline, "keyboard focus is visible").not.toBe("none");
  }

  private async backdropOpacity() {
    const color = await this.menuBackdrop.evaluate(
      (element) => getComputedStyle(element).backgroundColor,
    );
    const alpha = /[/,]\s*([\d.]+)\s*\)$/.exec(color);
    return alpha ? Number(alpha[1]) : 1;
  }
}
