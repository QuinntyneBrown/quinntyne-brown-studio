import { BrowserContext } from "@playwright/test";

/** Shared by page objects for cross-product scenarios; no production adapter is invoked. */
export class StudioFixture {
  authenticated = true;
  role = "Administrator";
  readonly calls: { service: string; method: string; args: any[] }[] = [];
  readonly records: Record<string, any[]> = Object.fromEntries(
    [
      "equipment",
      "vendors",
      "photographers",
      "sessions",
      "promotions",
      "print-options",
      "public-galleries",
      "studios",
      "content",
      "clients",
      "albums",
      "print-requests",
    ].map((key) => [key, []]),
  );
  readonly operations = new Map<
    string,
    (...args: any[]) => unknown | Promise<unknown>
  >();
  readonly failures = new Map<string, { message: string; status: number }>();
  rates = { id: "rates", version: 0, serviceRates: {}, costRates: {} };
  discounts = {
    id: "discounts",
    version: 0,
    advanceRule: { enabled: false, percentage: 0, threshold: 90 },
    weekdayRule: { enabled: false, percentage: 0, weekdays: [] },
    codeRules: [],
  };
  schedules: Record<string, any> = {};
  photos: Record<string, any[]> = {};
  retentions: Record<string, any> = {};
  private sequence = 1;

  async install(context: BrowserContext) {
    await context.exposeBinding(
      "__qbsControlled",
      async (_source, service: string, method: string, args: any[]) => {
        this.calls.push({ service, method, args: structuredClone(args) });
        try {
          const failure = this.failures.get(service + "." + method);
          if (failure) return { error: failure };
          const operation = this.operations.get(service + "." + method);
          return {
            value: await (operation
              ? operation(...args)
              : this.invoke(service, method, args)),
          };
        } catch (error) {
          return {
            error: {
              status: 503,
              message: error instanceof Error ? error.message : String(error),
            },
          };
        }
      },
    );
    // Reject any accidental fallback to an HTTP adapter during acceptance.
    await context.route("**/api/**", (route) => route.abort("blockedbyclient"));
    await context.addInitScript(() => {
      const windowFixture = window as any;
      const invoke = async (method: string, args: unknown[]) => {
        const result = await windowFixture.__qbsControlled(
          "quote",
          method,
          args,
        );
        if (result.error) throw new Error(result.error.message);
        return result.value;
      };
      windowFixture.__qbsQuoteMock = {
        getStudios: () => invoke("getStudios", []),
        resolveLocation: (address: string) =>
          invoke("resolveLocation", [address]).finally(() => {
            setTimeout(() => {
              windowFixture.__qbsLookupSettled = true;
            }, 0);
          }),
        calculate: (input: unknown) => invoke("calculate", [input]),
      };
    });
  }

  private save(key: string, value: any) {
    const records = this.records[key];
    const index = records.findIndex((row) => row.id === value.id);
    const saved = {
      ...structuredClone(value),
      id: value.id ?? `${key}-${this.sequence++}`,
      version: (index < 0 ? 0 : records[index].version) + 1,
    };
    if (index < 0) records.push(saved);
    else records[index] = saved;
    return structuredClone(saved);
  }

  private invoke(service: string, method: string, args: any[]): unknown {
    const key = (
      {
        vendor: "vendors",
        photographer: "photographers",
        session: "sessions",
        promotion: "promotions",
        "print-option": "print-options",
        "public-gallery": "public-galleries",
        studio: "studios",
        equipment: "equipment",
      } as Record<string, string>
    )[service];
    if (key) {
      if (method === "list") return this.records[key];
      if (method === "get")
        return this.records[key].find((row) => row.id === args[0]);
      if (method === "save") return this.save(key, args[0]);
      if (method === "published")
        return this.records[key]
          .filter((row) => row.published || row.enabled)
          .map((row) => ({
            ...row,
            revision: row.version,
            consultationNotice:
              "Subject to change following detailed consultation.",
            photos: (row.photoIds ?? []).map((id: string) => this.photo(id)),
          }));
      if (method === "publishedGallery") {
        const gallery = this.records[key].find(
          (row) => row.slug === args[0] && row.published,
        );
        if (!gallery) throw new Error("Gallery not found.");
        return {
          ...gallery,
          photos: gallery.photoIds.map((id: string) => this.photo(id)),
        };
      }
    }
    if (service === "catalog")
      return method === "list"
        ? this.records[args[0]]
        : this.save(args[0], args[1]);
    if (service === "auth") {
      if (method === "session")
        return {
          authenticated: this.authenticated,
          id: this.authenticated ? "client-a" : null,
          roles: this.authenticated ? [this.role] : [],
        };
      if (method === "login") {
        this.authenticated = true;
        return;
      }
      if (method === "logout") {
        this.authenticated = false;
        return;
      }
      if (method === "recover") return;
      if (method === "acceptInvitation" || method === "resetPassword")
        throw new Error("This link is invalid or expired.");
    }
    if (service === "rate") {
      if (method === "save")
        this.rates = { ...args[0], version: this.rates.version + 1 };
      return this.rates;
    }
    if (service === "discount") {
      if (method === "save")
        this.discounts = { ...args[0], version: this.discounts.version + 1 };
      return this.discounts;
    }
    if (service === "schedule") {
      if (method === "save")
        this.schedules[args[0]] = {
          ...args[1],
          version: (this.schedules[args[0]]?.version ?? 0) + 1,
        };
      return (
        this.schedules[args[0]] ?? {
          id: args[0],
          version: 0,
          workingWindows: [],
          unavailableWindows: [],
          buffers: { before: 30, after: 30 },
        }
      );
    }
    if (service === "content") {
      if (method === "list") return this.records["content"];
      if (method === "save") {
        const prior = this.records["content"].find(
          (row) => row.pageKey === args[0],
        );
        return this.save("content", {
          ...args[1],
          pageKey: args[0],
          publishedHeading: args[1].publish
            ? args[1].heading
            : prior?.publishedHeading,
          publishedBody: args[1].publish ? args[1].body : prior?.publishedBody,
        });
      }
      const content = this.records["content"].find(
        (row) => row.pageKey === args[0],
      );
      return content?.publishedHeading
        ? { heading: content.publishedHeading, body: content.publishedBody }
        : {
            heading: "Photography with feeling.",
            body: "Honest moments. Thoughtfully captured.",
          };
    }
    if (service === "client-gallery") {
      if (method === "clients") return this.records["clients"];
      if (method === "invite") {
        this.records["clients"].push({
          id: `client-${this.sequence++}`,
          email: args[0],
        });
        return { invitationId: `invitation-${this.sequence++}` };
      }
      if (method === "assign") {
        const session = this.records["sessions"].find(
          (row) => row.id === args[0],
        );
        return this.save("sessions", { ...session, clientIds: args[1] });
      }
      const galleries = this.records["sessions"].filter((row) =>
        row.clientIds?.includes("client-a"),
      );
      if (method === "list") return galleries;
      const gallery = galleries.find((row) => row.id === args[0]);
      if (!gallery) throw new Error("Gallery not found.");
      return { ...gallery, photos: this.photos[gallery.id] ?? [] };
    }
    if (service === "album") {
      if (method === "list") return this.records["albums"];
      if (method === "get")
        return this.records["albums"].find((row) => row.id === args[0]);
      if (method === "save")
        return this.save("albums", {
          ...args[0],
          photos: args[0].orderedPhotoIds.map((id: string) => this.photo(id)),
        });
    }
    if (service === "photo" && method === "list") {
      const offset = Number(args[1] ?? 0),
        photos = this.photos[args[0]] ?? [];
      return {
        photos: photos.slice(offset, offset + 50),
        nextCursor: offset + 50 < photos.length ? String(offset + 50) : null,
      };
    }
    if (service === "retention" && method === "get")
      return (
        this.retentions[args[0]] ?? {
          id: args[0],
          months: 12,
          version: 1,
          state: "Active",
          photoCount: this.photos[args[0]]?.length ?? 0,
          publishedReferences: 0,
          unreviewedRequests: 0,
          impactRevision: "current",
        }
      );
    if (service === "print-request") {
      if (method === "list")
        return this.records["print-requests"].filter(
          (request) => !args[0] || request.state === args[0],
        );
      if (method === "get" || method === "submitted")
        return this.records["print-requests"].find((row) => row.id === args[0]);
      if (method === "review") {
        const prior = this.records["print-requests"].find(
          (row) => row.id === args[0],
        );
        return this.save("print-requests", { ...prior, state: "Reviewed" });
      }
    }
    throw new Error(`No controlled result supplied for ${service}.${method}.`);
  }

  photo(id: string) {
    return (
      Object.values(this.photos)
        .flat()
        .find((photo) => photo.id === id) ?? {
        id,
        name: "Unavailable photo",
        available: false,
      }
    );
  }
}
