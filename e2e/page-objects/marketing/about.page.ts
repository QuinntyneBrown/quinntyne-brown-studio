import { expect, type Page } from "@playwright/test";
import { MarketingShellPage } from "./marketing-shell.page";

/** The About page the API renders at /about. */
export class AboutPage {
  readonly shell: MarketingShellPage;

  constructor(
    readonly page: Page,
    readonly origin: string,
  ) {
    this.shell = new MarketingShellPage(page);
  }

  open() {
    return this.page.goto(`${this.origin}/about`);
  }

  async heading(text: string) {
    await expect(this.page.getByRole("heading", { level: 1, name: text })).toBeVisible();
  }

  async teamMember(name: string, role?: string) {
    const card = this.page.locator(".team-card").filter({ hasText: name });
    await expect(card).toBeVisible();
    if (role) await expect(card).toContainText(role);
  }

  async noTeamMember(name: string) {
    await expect(this.page.locator(".team-card").filter({ hasText: name })).toHaveCount(0);
  }

  async emptyTeam() {
    await expect(
      this.page.getByRole("heading", { name: "Introductions coming soon", exact: true }),
    ).toBeVisible();
  }

  async heroPlaceholder() {
    await expect(this.page.getByText("A photograph is on its way", { exact: true })).toBeVisible();
    await expect(this.page.locator(".marketing-hero-image img")).toHaveCount(0);
  }

  async sections() {
    for (const title of [
      "Three things we won’t compromise on.",
      "Behind the camera.",
      "From hello to your gallery.",
    ])
      await expect(this.page.getByRole("heading", { name: title, exact: true })).toBeVisible();
  }

  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
}
