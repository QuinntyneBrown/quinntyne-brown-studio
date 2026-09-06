export class UploadPage {
  constructor(page, expect, origin) {
    this.page = page;
    this.expect = expect;
    this.origin = origin;
  }
  async signIn(email, password) {
    await this.page.goto(this.origin + "/admin/login");
    await this.page.getByLabel("Email", { exact: true }).fill(email);
    await this.page.getByLabel("Password", { exact: true }).fill(password);
    await this.page
      .getByRole("button", { name: "Sign in", exact: true })
      .click();
    await this.expect(
      this.page.getByRole("heading", { name: "Sessions", exact: true }),
    ).toBeVisible();
  }
  async open(sessionId) {
    await this.page.goto(
      this.origin + "/admin/sessions/" + encodeURIComponent(sessionId),
    );
    await this.expect(
      this.page.getByLabel("Select photos", { exact: true }),
    ).toBeEnabled();
  }
  async select(paths) {
    await this.page
      .getByLabel("Select photos", { exact: true })
      .setInputFiles(paths, { timeout: 120000 });
  }
  async states() {
    return this.page.locator(".session__upload").allTextContents();
  }
  async resume(paths) {
    await this.expect(
      this.page.getByLabel("Select photos", { exact: true }),
    ).toBeEnabled({ timeout: 120000 });
    await this.page.reload();
    await this.select(paths);
  }
}
