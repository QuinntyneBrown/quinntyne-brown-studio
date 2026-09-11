import { StudioFixture } from "./studio-fixture";
import { expect, Page } from "@playwright/test";

export class AccountPage {
  readonly fixture = new StudioFixture();
  get submissions() {
    return this.fixture.calls.filter(
      (call) =>
        call.service === "auth" &&
        ["login", "acceptInvitation", "resetPassword", "recover"].includes(
          call.method,
        ),
    ).length;
  }
  get authenticated() {
    return this.fixture.authenticated;
  }
  set authenticated(value: boolean) {
    this.fixture.authenticated = value;
  }
  get role() {
    return this.fixture.role;
  }
  set role(value: string) {
    this.fixture.role = value;
  }
  set failLogout(value: boolean) {
    if (value)
      this.fixture.failures.set("auth.logout", {
        status: 503,
        message: "Sign out is unavailable. Try again.",
      });
    else this.fixture.failures.delete("auth.logout");
  }
  constructor(
    readonly page: Page,
    readonly origin?: string,
  ) {
    this.fixture.authenticated = false;
  }
  async mock() {
    await this.fixture.install(this.page.context());
  }
  async open(path = "login", site = "admin") {
    await this.page.goto(
      `${this.origin ? this.origin + "/" + site : `http://localhost:${site === "admin" ? 4421 : 4422}`}/${path}`,
    );
  }
  async login(email = "client@example.test", password = "Test-only!12345") {
    await this.page.getByLabel("Email", { exact: true }).fill(email);
    await this.page.getByLabel("Password", { exact: true }).fill(password);
    await this.page
      .getByRole("button", { name: "Sign in", exact: true })
      .click();
  }
  async savePassword(password = "Test-only!12345") {
    await this.page.getByLabel("Password", { exact: true }).fill(password);
    await this.page
      .getByRole("button", { name: "Save password", exact: true })
      .click();
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async heading(text: string) {
    await expect(
      this.page.getByRole("heading", { name: text, exact: true }),
    ).toBeVisible();
  }
  async passwordRetained() {
    await expect(this.page.getByLabel("Password", { exact: true })).toHaveValue(
      "Test-only!12345",
    );
  }
  async signOut() {
    await this.page
      .getByRole("button", { name: "Sign out", exact: true })
      .click();
  }
  /** The compact layout folds the workspace navigation behind its Menu toggle. */
  private async openNavigation() {
    const toggle = this.page.getByRole("button", { name: "Menu", exact: true });
    if (!(await toggle.isVisible())) return;
    if ((await toggle.getAttribute("aria-expanded")) !== "true")
      await toggle.click();
    await expect(toggle).toHaveAttribute("aria-expanded", "true");
  }
  private navLink(name: string) {
    return this.page
      .getByRole("navigation", { name: "Workspace navigation" })
      .getByRole("link", { name, exact: true });
  }
  async navLinkShown(name: string) {
    await this.openNavigation();
    await expect(this.navLink(name)).toBeVisible();
  }
  async navLinkHidden(name: string) {
    await this.openNavigation();
    await expect(this.navLink(name)).toBeHidden();
  }
  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
  async recover(email: string) {
    await this.page.getByLabel("Email", { exact: true }).fill(email);
    await this.page
      .getByRole("button", { name: "Send recovery instructions", exact: true })
      .click();
  }
}
