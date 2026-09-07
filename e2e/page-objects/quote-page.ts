import { expect, Page } from "@playwright/test";

export class QuotePage {
  readonly calls: any[] = [];
  candidates = [{ label: "Venue A", latitude: 43.7, longitude: -79.3 }];
  studios = [
    {
      id: "studio-a",
      name: "Local studio",
      hourlyFee: "25",
      resolvedAddress: this.candidates[0],
    },
  ];
  resolve: (address: string) => Promise<any[]> = async () => this.candidates;
  calculate: (input: any) => Promise<any> = async (input) =>
    QuotePage.result(input);
  getStudios: () => Promise<any[]> = async () => this.studios;
  constructor(
    readonly page: Page,
    readonly origin = "http://localhost:4420",
  ) {}
  static result(
    input: any,
    amount = "297.29",
    kind: string | null = "Advance",
    available = true,
  ) {
    const money = (value: string) => ({ amount: value, currency: "CAD" });
    const fixtures: Record<string, [string, string]> = {
      "297.29": ["330.32", "33.03"],
      "240.00": ["266.67", "26.67"],
      "100.00": ["111.11", "11.11"],
      "420.00": ["466.67", "46.67"],
      "306.94": ["341.04", "34.10"],
    };
    const discount = kind ? fixtures[amount] : undefined;
    return {
      inputRevision: input.inputRevision,
      configurationRevision: 1,
      lines: [
        {
          kind: "photography",
          quantity: "1.25",
          amount: money(discount?.[0] ?? amount),
          locationIndex: null,
        },
      ],
      subtotal: money(discount?.[0] ?? amount),
      total: money(amount),
      discount: {
        kind: discount ? kind : null,
        ruleId: "rule-a",
        percentage: discount ? "10" : "0",
        amount: money(discount?.[1] ?? "0.00"),
        codeError: null,
      },
      availability: {
        available,
        photographerIds: [],
        reasonCode: available ? null : "NoPhotographer",
      },
    };
  }
  async open() {
    await this.page.exposeFunction("__quoteStudios", () => this.getStudios());
    await this.page.exposeFunction("__quoteResolve", (address: string) =>
      this.resolve(address),
    );
    await this.page.exposeFunction("__quoteCalculate", (input: any) => {
      this.calls.push(input);
      return this.calculate(input);
    });
    await this.page.addInitScript(() => {
      const w = window as any;
      w.__quoteSettled = 0;
      w.__lookupSettled = 0;
      w.__qbsQuoteMock = {
        getStudios: w.__quoteStudios,
        resolveLocation: (address: string) =>
          w.__quoteResolve(address).finally(() => w.__lookupSettled++),
        calculate: (input: unknown) =>
          w.__quoteCalculate(input).finally(() => w.__quoteSettled++),
      };
    });
    await this.page.goto(`${this.origin}/quote`);
    await this.ready();
  }
  /** Opens the calculator against whatever service the application binds; no fixture. */
  async openLive() {
    await this.page.goto(`${this.origin}/quote`);
    await this.ready();
  }
  private async ready() {
    await expect(
      this.page.getByRole("heading", {
        name: "Your session, thoughtfully priced.",
      }),
    ).toBeVisible();
  }
  async fill(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).fill(value);
  }
  async choose(label: string, value: string) {
    await this.page.getByLabel(label, { exact: true }).selectOption(value);
  }
  async click(name: string) {
    await this.page.getByRole("button", { name, exact: true }).click();
  }
  async search(address = "Venue") {
    await this.fill("Address", address);
    await this.click("Find address");
  }
  async add() {
    await this.search();
    await this.page.getByRole("button", { name: "Venue A · Select" }).click();
  }
  async amount(value: string) {
    await expect(this.page.getByTestId("quote-total")).toHaveText(
      `${value} CAD`,
    );
  }
  async noAmount() {
    await expect(this.page.getByTestId("quote-total")).toHaveCount(0);
  }
  async message(value: string) {
    await expect(
      this.page.getByText(value, { exact: false }).first(),
    ).toBeVisible();
  }
  async value(label: string, value: string) {
    await expect(this.page.getByLabel(label, { exact: true })).toHaveValue(
      value,
    );
  }
  async candidate(name: string, visible = true) {
    await expect(
      this.page.getByRole("button", { name: `${name} · Select` }),
    ).toHaveCount(visible ? 1 : 0);
  }
  async expectCalls(count: number) {
    await expect.poll(() => this.calls.length).toBe(count);
  }
  async settled(count: number, lookup = false) {
    await expect
      .poll(() =>
        this.page.evaluate(
          (lookup) =>
            (window as any)[lookup ? "__lookupSettled" : "__quoteSettled"],
          lookup,
        ),
      )
      .toBe(count);
  }
  async capture(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }
  async addressFocused() {
    await expect(
      this.page.getByLabel("Address", { exact: true }),
    ).toBeFocused();
  }
  async invalidField(label: string) {
    await expect(this.page.getByLabel(label, { exact: true })).toHaveAttribute(
      "aria-invalid",
      "true",
    );
  }
  async fillLocation(index: number, label: string, value: string) {
    await this.page
      .getByRole("region", { name: `Location ${index}`, exact: true })
      .getByLabel(label, { exact: true })
      .fill(value);
  }
  async chooseLocation(index: number, value: string) {
    await this.page
      .getByRole("region", { name: `Location ${index}`, exact: true })
      .getByLabel("Studio", { exact: true })
      .selectOption(value);
  }
  async chooseStudio(index: number, name: string) {
    await this.page
      .getByRole("region", { name: `Location ${index}`, exact: true })
      .getByLabel("Studio", { exact: true })
      .selectOption({ label: name });
  }
  async line(label: string, amount: string) {
    await expect(
      this.page
        .locator(".price__line")
        .filter({ has: this.page.getByText(label, { exact: true }) }),
    ).toContainText(amount + " CAD");
  }
  async layoutAndKeyboard() {
    expect(
      await this.page.evaluate(
        () => document.documentElement.scrollWidth <= innerWidth,
      ),
    ).toBe(true);
    await this.page.getByLabel("Address", { exact: true }).focus();
    await this.page.keyboard.press("Tab");
    await expect(
      this.page.getByRole("button", { name: "Find address", exact: true }),
    ).toBeFocused();
    await this.page.keyboard.press("Enter");
    await this.candidate("Venue A");
  }
}
