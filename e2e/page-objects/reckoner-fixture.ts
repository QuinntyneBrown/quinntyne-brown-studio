import { BrowserContext } from "@playwright/test";
import type {
  ServicesDocument,
  RatesDocument,
  LocationsDocument,
  DiscountsDocument,
  AvailabilityDocument,
  LoyaltyDocument,
  ThemeDocument,
  DisplayDocument,
  ResolvedAddress,
} from "reckoner/behavior";
import { quoteDefinition } from "../fixtures/quote-fixture";

/** HTTP boundary fixture for the installed Reckoner widgets; Studio access uses its own service fixture. */
export class ReckonerFixture {
  readonly origin = "https://reckoner.acceptance.example";
  private readonly definition = quoteDefinition();
  private readonly context = {
    ...this.definition,
    calculatorIdentity: "studio-acceptance",
  };
  services: ServicesDocument = {
    version: 1,
    context: this.context,
    services: Object.fromEntries(
      this.definition.services.map((s) => [
        s.id,
        { enabled: true, rate: null },
      ]),
    ),
  };
  rates: RatesDocument = {
    version: 1,
    context: this.context,
    rates: { travel: null, equipment: null, lunch: null, assistant: null },
  };
  discounts: DiscountsDocument = {
    version: 1,
    context: this.context,
    advance: { enabled: false, percentage: 0, thresholdDays: 90 },
    slowDay: { enabled: false, percentage: 0, weekdays: [] },
    codes: [],
  };
  locations: LocationsDocument = {
    version: 1,
    context: this.context,
    base: null,
    studios: [],
  };
  availability: AvailabilityDocument = {
    version: 1,
    context: this.context,
    weekdays: {
      monday: true,
      tuesday: true,
      wednesday: true,
      thursday: true,
      friday: true,
      saturday: true,
      sunday: true,
    },
    exceptions: [],
  };
  loyalty: LoyaltyDocument = {
    version: 1,
    context: this.context,
    enabled: false,
    percentage: 0,
  };
  theme: ThemeDocument = {
    version: 1,
    context: this.context,
    ...this.definition.theme,
    warnings: [],
  };
  display: DisplayDocument = {
    version: 1,
    context: this.context,
    ...this.definition.display,
  };
  readonly failures = new Map<string, { status: number; message: string }>();
  readonly requests: {
    resource: string;
    method: string;
    authorization: string | undefined;
  }[] = [];
  resolve: (query: string) => Promise<readonly ResolvedAddress[]> =
    async () => [
      {
        label: "10 Studio Street, Toronto",
        latitude: 43.65,
        longitude: -79.38,
      },
    ];
  lookupSettled = 0;
  async install(context: BrowserContext) {
    await context.route(this.origin + "/api/**", async (route) => {
      const request = route.request(),
        resource = new URL(request.url()).pathname.replace(
          "/api/v1/admin/",
          "",
        ),
        method = request.method();
      this.requests.push({
        resource,
        method,
        authorization: request.headers()["authorization"],
      });
      const headers = {
        Date: new Date().toUTCString(),
        "Access-Control-Allow-Origin": "*",
        "Access-Control-Expose-Headers": "Date",
      };
      const failure = this.failures.get(resource + "." + method);
      if (failure) {
        await route.fulfill({
          status: failure.status,
          json: { type: "controlled-failure", detail: failure.message },
          headers,
        });
        return;
      }
      if (resource === "locations/resolve") {
        const candidates = await this.resolve(request.postDataJSON().query);
        this.lookupSettled++;
        await route.fulfill({ json: candidates, headers });
        return;
      }
      if (resource === "loyalty/codes") {
        await route.fulfill({ json: { codes: [], cursor: null }, headers });
        return;
      }
      if (
        ![
          "services",
          "rates",
          "discounts",
          "locations",
          "availability",
          "loyalty",
          "theme",
          "display",
        ].includes(resource)
      )
        throw new Error("No Reckoner fixture for " + resource);
      const key = resource as
        | "services"
        | "rates"
        | "discounts"
        | "locations"
        | "availability"
        | "loyalty"
        | "theme"
        | "display";
      if (method === "PUT") {
        const input = request.postDataJSON();
        if (input.expectedVersion !== this[key].version) {
          await route.fulfill({
            status: 409,
            json: {
              type: "version-conflict",
              detail: "Settings changed. Reload before saving.",
            },
            headers,
          });
          return;
        }
        this[key] = {
          ...input,
          context: this.context,
          version: this[key].version + 1,
        };
      }
      await route.fulfill({ json: this[key], headers });
    });
  }
}
