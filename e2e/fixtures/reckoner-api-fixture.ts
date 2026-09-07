/** Test provisioning stays in the Node runner; admin credentials never enter the visitor page. */
export async function provisionReckoner() {
  const provisioned = await fetch("http://127.0.0.1:4390/__test/scenario", {
    method: "POST",
  });
  if (!provisioned.ok)
    throw new Error("Reckoner acceptance provisioning failed.");
  const fixture = (await provisioned.json()) as {
    apiBaseUrl: string;
    adminToken: string;
    publishableKey: string;
  };
  const headers = {
    Authorization: "Bearer " + fixture.adminToken,
    "Content-Type": "application/json",
  };
  async function read(resource: string) {
    const response = await fetch(
      fixture.apiBaseUrl + "/api/v1/admin/" + resource,
      { headers },
    );
    if (!response.ok)
      throw new Error("Cannot read acceptance configuration: " + resource);
    return response.json();
  }
  async function save(resource: string, document: object) {
    const response = await fetch(
      fixture.apiBaseUrl + "/api/v1/admin/" + resource,
      { method: "PUT", headers, body: JSON.stringify(document) },
    );
    if (!response.ok)
      throw new Error(
        "Cannot save acceptance configuration: " +
          resource +
          " " +
          (await response.text()),
      );
  }
  const services = await read("services");
  services.services.wedding = { enabled: true, rate: 250 };
  await save("services", { ...services, expectedVersion: services.version });
  await save("rates", {
    expectedVersion: 0,
    rates: { travel: 1.25, equipment: 45, lunch: 22.5, assistant: 60 },
  });
  await save("locations", {
    expectedVersion: 0,
    base: { label: "Base", latitude: 43.64, longitude: -79.38 },
    studios: [
      {
        name: "The daylight loft",
        address: { label: "Loft", latitude: 43.65, longitude: -79.39 },
        fee: 80,
        enabled: true,
      },
    ],
  });
  await save("discounts", {
    expectedVersion: 0,
    advance: { enabled: true, percentage: 15, thresholdDays: 90 },
    slowDay: { enabled: true, percentage: 12, weekdays: ["saturday"] },
    codes: [{ code: "PHOTO10", enabled: true, percentage: 10 }],
  });
  const display = await read("display");
  await save("display", {
    ...display,
    expectedVersion: display.version,
    title: "A little clarity, before we begin.",
    disclaimer:
      "A live estimate to help you plan. Final details and availability are confirmed together; this does not reserve a session.",
  });
  return {
    apiBaseUrl: fixture.apiBaseUrl,
    publishableKey: fixture.publishableKey,
  };
}
