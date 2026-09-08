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
  async workspaceNavigationHidden() {
    await expect(
      this.page.getByRole("navigation", {
        name: "Workspace navigation",
        includeHidden: true,
      }),
    ).toHaveCount(0);
    await expect(
      this.page.getByRole("button", {
        name: "Menu",
        exact: true,
        includeHidden: true,
      }),
    ).toHaveCount(0);
    await expect(
      this.page.getByRole("link", { name: /Quinntyne Brown/ }),
    ).toHaveAttribute("href", /\/login$/);
  }
  async workspaceNavigationVisible(site: string) {
    const menu = this.page.getByRole("button", { name: "Menu", exact: true });
    if (await menu.isVisible()) {
      if ((await menu.getAttribute("aria-expanded")) !== "true")
        await menu.click();
    }
    const navigation = this.page.getByRole("navigation", {
      name: "Workspace navigation",
    });
    await expect(navigation).toBeVisible();
    await expect(navigation.getByRole("link")).toHaveText(
      site === "admin"
        ? [
            "Sessions",
            "Photographers",
            "Equipment",
            "Preferred vendors",
            "Quote rates",
            "Studios",
            "Discount rules",
            "Quote availability",
            "Quote appearance",
            "Print pricing",
            "Public galleries",
            "Website content",
            "Package promotions",
            "Client invitations",
            "Print requests",
          ]
        : ["Your sessions", "Your albums", "Request prints"],
    );
    await expect(
      this.page.getByRole("link", { name: /Quinntyne Brown/ }),
    ).toHaveAttribute("href", /\/$/);
  }
  async navigateWorkspace(label: string) {
    await this.page
      .getByRole("navigation", { name: "Workspace navigation" })
      .getByRole("link", { name: label, exact: true })
      .click();
  }
  async signInRoute() {
    await expect(this.page).toHaveURL(/\/login$/);
    await this.heading("Welcome back.");
  }
  async recover(email: string) {
    await this.page.getByLabel("Email", { exact: true }).fill(email);
    await this.page
      .getByRole("button", { name: "Send recovery instructions", exact: true })
      .click();
  }
}
