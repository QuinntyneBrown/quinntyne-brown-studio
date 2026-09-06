import { BrowserContext, expect } from "@playwright/test";

/** Real HTTP setup for the isolated LocalDB smoke database. No interception or mocks. */
export class FullstackFixture {
  constructor(
    readonly context: BrowserContext,
    readonly origin: string,
  ) {}
  async read(path: string) {
    const response = await this.context.request.get(
      this.origin + "/api/" + path,
    );
    expect(response.ok()).toBeTruthy();
    return response.json();
  }
  async save(method: string, path: string, data: unknown) {
    const token = await this.read("auth/antiforgery");
    const response = await this.context.request.fetch(
      this.origin + "/api/" + path,
      { method, data, headers: { "X-XSRF-TOKEN": token.requestToken } },
    );
    if (!response.ok())
      throw new Error(`${path}: ${response.status()} ${await response.text()}`);
    return response.status() === 204 ? undefined : response.json();
  }
  async createSession(name: string) {
    return this.save("POST", "admin/sessions", {
      name,
      service: "Headshot",
      startsAt: "2027-06-01T10:00:00-04:00",
      endsAt: "2027-06-01T11:00:00-04:00",
    });
  }
  async publish(sessionId: string, slug: string) {
    const photos = await this.read(`admin/sessions/${sessionId}/photos`);
    await this.save("POST", "admin/public-galleries", {
      title: "Local selected work",
      slug,
      published: true,
      photoIds: [photos.photos[0].id],
    });
    return photos.photos[0];
  }
  async printOption() {
    return this.save("POST", "admin/print-options", {
      name: "Studio print",
      dimensions: "8 × 10 in",
      finish: "Matte",
      unitPrice: "29.95",
      enabled: true,
    });
  }
  async invite(email: string) {
    const prior = new Set(
      (await this.read("admin/development-mail")).map(
        (message: any) => message.id,
      ),
    );
    await this.save("POST", "admin/invitations", { email });
    let invitation = "";
    await expect(async () => {
      const mail = await this.read("admin/development-mail");
      invitation =
        mail
          .filter((message: any) => !prior.has(message.id))
          .map(
            (message: any) =>
              message.body.match(/accept-invitation\?token=([A-F0-9]+)/)?.[1],
          )
          .find(Boolean) ?? "";
      expect(invitation).not.toBe("");
    }).toPass({ timeout: 30000 });
    return invitation;
  }
  async requests() {
    return this.read("admin/print-requests");
  }
  async clientPhotoDenied(context: BrowserContext, photoId: string) {
    expect(
      (
        await context.request.get(
          this.origin + `/api/client/photos/${photoId}/preview`,
        )
      ).status(),
    ).toBe(404);
  }
}
