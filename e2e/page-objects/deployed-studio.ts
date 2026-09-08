import { APIRequestContext, expect } from "@playwright/test";

/**
 * Real HTTPS checks against a deployed studio origin. Nothing is intercepted: the
 * subject is the certificate the gateway presents, the routes it publishes, and the
 * API answering from its own database.
 */
export class DeployedStudio {
  constructor(
    readonly request: APIRequestContext,
    readonly origin: string,
  ) {}
  /** The deployed origin under test. A run without one is a configuration mistake, not a pass. */
  static origin() {
    const origin = process.env["QBS_PRODUCTION_ORIGIN"] ?? "";
    if (!/^https:\/\/[a-z0-9][a-z0-9.-]*$/.test(origin))
      throw new Error(
        "Set QBS_PRODUCTION_ORIGIN to the deployed HTTPS origin, for example https://qbs-example.canadacentral.cloudapp.azure.com",
      );
    return origin;
  }
  /** Liveness only. It does not prove storage, email, AI or database readiness. */
  async live() {
    expect((await this.request.get(this.origin + "/api/health")).status()).toBe(
      200,
    );
  }
  /** A published read the API can only answer by reaching its database. */
  async reads(path: string) {
    const response = await this.request.get(`${this.origin}/api/public/${path}`);
    expect(response.status(), path).toBe(200);
    return response.json();
  }
  /** A studio route the gateway must answer with the application shell, not a 404. */
  async application(path: string) {
    const response = await this.request.get(this.origin + path);
    expect(response.status(), path).toBe(200);
    expect(response.headers()["content-type"] ?? "", path).toContain(
      "text/html",
    );
  }
  async redirects(path: string, location: string) {
    const response = await this.request.get(this.origin + path, {
      maxRedirects: 0,
    });
    expect(response.status(), path).toBe(308);
    expect(response.headers()["location"], path).toBe(location);
  }
  async refusesAnonymously(path: string) {
    expect(
      (await this.request.get(`${this.origin}/api/${path}`)).status(),
      path,
    ).toBe(401);
  }
}
