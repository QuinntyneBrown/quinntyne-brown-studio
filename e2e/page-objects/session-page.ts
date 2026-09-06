import { expect, Page } from "@playwright/test";

export class SessionPage {
  imageAvailable = false;
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4421",
  ) {}
  async open(id = "session-a") {
    await this.page.goto(`${this.origin}/sessions/${id}`);
  }
  async unavailableResumeStorage() {
    await this.page.addInitScript(() => {
      Storage.prototype.setItem = () => {
        throw new DOMException("Storage is full.", "QuotaExceededError");
      };
    });
  }
  async corruptResumeStorage() {
    await this.page.addInitScript(() =>
      localStorage.setItem("qbs-upload:session-a", "{broken"),
    );
  }
  async unreadableFile(name: string) {
    await this.page.addInitScript((name) => {
      const NativeWorker = Worker;
      window.Worker = class extends NativeWorker {
        override postMessage(message: unknown) {
          if (message instanceof File && message.name === name)
            queueMicrotask(() =>
              this.dispatchEvent(
                new MessageEvent("message", {
                  data: { error: "Unable to read this file." },
                }),
              ),
            );
          else super.postMessage(message);
        }
      };
    }, name);
  }
  async upload(names: string[]) {
    await this.page.getByLabel("Select photos", { exact: true }).setInputFiles(
      names.map((name) => ({
        name,
        mimeType: "image/jpeg",
        buffer: Buffer.from([255, 216, 255, 224, 0, 2, 255, 217]),
      })),
    );
  }
  async fileState(name: string, state: string) {
    await expect(
      this.page.locator(".session__upload").filter({ hasText: name }),
    ).toContainText(state, { timeout: 30000 });
  }
  async message(text: string) {
    await expect(
      this.page.getByText(text, { exact: false }).first(),
    ).toBeVisible();
  }
  async click(name: string) {
    await this.page.getByRole("button", { name, exact: true }).click();
  }
  async photo(name: string) {
    await this.page
      .getByRole("checkbox", { name: `Select ${name}`, exact: true })
      .check();
  }
  async inspect(name: string) {
    await this.page
      .getByRole("button", { name: `View ${name}`, exact: true })
      .click();
  }
  async dialog(name: string) {
    await expect(
      this.page.getByRole("dialog", { name, exact: true }),
    ).toBeVisible();
  }
  async reload() {
    await this.page.reload();
  }
  async noEmpty() {
    await expect(
      this.page.getByText("Photos will appear here after upload.", {
        exact: true,
      }),
    ).toHaveCount(0);
  }
  async disabled(name: string) {
    await expect(
      this.page.getByRole("button", { name, exact: true }),
    ).toBeDisabled();
  }
  async field(name: string, value: string) {
    await this.page.getByLabel(name, { exact: true }).fill(value);
  }
  async uploadLarge(name: string) {
    await this.page.getByLabel("Select photos", { exact: true }).setInputFiles({
      name,
      mimeType: "image/jpeg",
      buffer: Buffer.alloc(9 * 1024 * 1024, 1),
    });
  }
  async uploadJpeg() {
    const jpeg = await this.page.evaluate(() => {
      const canvas = document.createElement("canvas");
      canvas.width = 900;
      canvas.height = 1200;
      const graphics = canvas.getContext("2d")!;
      graphics.fillStyle = "#7a8572";
      graphics.fillRect(0, 0, 900, 1200);
      return canvas.toDataURL("image/jpeg").split(",")[1];
    });
    await this.page.getByLabel("Select photos", { exact: true }).setInputFiles({
      name: "smoke.jpg",
      mimeType: "image/jpeg",
      buffer: Buffer.from(jpeg, "base64"),
    });
  }
  async assign(email: string, assigned = true) {
    await this.page.getByLabel(email, { exact: true }).setChecked(assigned);
    await this.click("Save gallery access");
    await this.message("Gallery access saved.");
  }
  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
  async selectionDisabled(name: string) {
    await expect(
      this.page.getByRole("checkbox", { name: "Select " + name, exact: true }),
    ).toBeDisabled();
  }
  async imageDependency() {
    await this.page.route("**/controlled-preview.jpg", (route) =>
      route.fulfill(
        this.imageAvailable
          ? {
              contentType: "image/svg+xml",
              body: '<svg xmlns="http://www.w3.org/2000/svg" width="10" height="10"><rect width="10" height="10" fill="gray"/></svg>',
            }
          : { status: 503, body: "Unavailable" },
      ),
    );
  }
  async imageLoaded(name: string) {
    await expect(
      this.page.getByRole("img", { name, exact: true }),
    ).toBeVisible();
    await expect
      .poll(() =>
        this.page
          .getByRole("img", { name, exact: true })
          .evaluate(
            (image: HTMLImageElement) =>
              image.complete && image.naturalWidth > 0,
          ),
      )
      .toBe(true);
  }
  async closeWithEscape(name: string) {
    await this.page.keyboard.press("Escape");
    await expect(this.page.getByRole("dialog")).toHaveCount(0);
    await expect(
      this.page.getByRole("button", { name: "View " + name, exact: true }),
    ).toBeFocused();
  }
  async touchTargets(name: string) {
    const close = await this.page
      .getByRole("button", { name: "Close dialog", exact: true })
      .boundingBox();
    expect(Math.round((close?.height ?? 0) * 100) / 100).toBeGreaterThanOrEqual(
      44,
    );
    const selection = await this.page
      .getByRole("checkbox", { name: "Select " + name, exact: true })
      .locator("..")
      .boundingBox();
    expect(
      Math.round((selection?.height ?? 0) * 100) / 100,
    ).toBeGreaterThanOrEqual(44);
  }
}
