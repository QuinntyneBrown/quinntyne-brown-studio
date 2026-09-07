import type {
  QuoteDefinition,
  QuoteRequest,
  QuoteResult,
} from "reckoner/behavior";

export function quoteDefinition(): QuoteDefinition {
  return {
    currency: "CAD",
    timezone: "America/Toronto",
    locale: "en-CA",
    configurationRevision: 1,
    template: "photography",
    templateVersion: 1,
    serviceLineLabel: "Photography",
    ready: true,
    readinessIssues: [],
    services: [
      ["wedding", "Wedding"],
      ["event", "Event"],
      ["headshots", "Headshots"],
      ["familyPortraits", "Family portraits"],
    ].map(([id, label]) => ({
      id: id!,
      label: label!,
      inputLabel: "Service",
      unitLabel: "per hour",
      pricingUnit: "hour",
      rate: 250,
    })),
    factors: [
      {
        id: "travel",
        label: "Travel",
        inputLabel: "Travel",
        pricingUnit: "kilometre",
        unitLabel: "per kilometre",
        rate: 1.25,
        required: true,
      },
      {
        id: "equipment",
        label: "Equipment",
        inputLabel: "Equipment rental units",
        pricingUnit: "unit",
        unitLabel: "per unit",
        rate: 45,
        required: true,
      },
      {
        id: "lunch",
        label: "Lunches",
        inputLabel: "Lunches",
        pricingUnit: "person",
        unitLabel: "per person",
        rate: 22.5,
        required: true,
      },
      {
        id: "assistant",
        label: "Assistant",
        inputLabel: "Assistants",
        pricingUnit: "personHour",
        unitLabel: "per person hour",
        rate: 60,
        required: true,
      },
      {
        id: "parking",
        label: "Parking",
        inputLabel: "Parking",
        pricingUnit: "enteredAmountPerLocation",
        unitLabel: "per location",
        rate: null,
        required: false,
      },
      {
        id: "studio",
        label: "Studio time",
        inputLabel: "Studio",
        pricingUnit: "hourPerLocation",
        unitLabel: "per hour",
        rate: null,
        required: false,
      },
    ],
    studios: [{ id: "studio-a", name: "The daylight loft", fee: 80 }],
    theme: {
      surface: "#FFFFFF",
      surfaceMuted: "#F3F7F6",
      ink: "#101828",
      muted: "#475467",
      line: "#667085",
      accent: "#1F6F5F",
      accentInk: "#FFFFFF",
      danger: "#B42318",
      fontSans: "Arial, sans-serif",
      fontSerif: "Georgia, serif",
      radius: "8px",
      spaceUnit: "4px",
      focusRing: "2px solid #1F6F5F",
    },
    display: {
      title: "A little clarity, before we begin.",
      intro: "",
      disclaimer:
        "A live estimate to help you plan. Final details and availability are confirmed together; this does not reserve a session.",
      showCodeField: true,
      visibleInputs: {},
    },
  };
}

/** Hand-authored presentation outcomes, independent of the authoritative pricing engine. */
export function quoteResult(
  input: QuoteRequest,
  amount = "297.29",
  kind: string | null = "advance",
  available = true,
): QuoteResult {
  const fixtures: Record<string, [string, string]> = {
    "297.29": ["330.32", "33.03"],
    "240.00": ["266.67", "26.67"],
    "100.00": ["111.11", "11.11"],
    "420.00": ["466.67", "46.67"],
    "1360.73": ["1600.86", "240.13"],
  };
  const discount = kind ? fixtures[amount] : undefined;
  return {
    inputRevision: input.inputRevision,
    configurationRevision: 1,
    currency: "CAD",
    lines: [
      {
        kind: "service",
        label: "Photography",
        quantity: "1.00",
        unitPrice: discount?.[0] ?? amount,
        amount: discount?.[0] ?? amount,
        locationIndex: null,
      },
    ],
    subtotal: discount?.[0] ?? amount,
    total: amount,
    discount: {
      kind: discount ? kind : null,
      label: discount ? "Advance booking" : null,
      code: null,
      percentage: discount
        ? amount === "1360.73"
          ? "15.00"
          : "10.00"
        : "0.00",
      amount: discount?.[1] ?? "0.00",
      codeError: null,
    },
    availability: {
      date: input.date,
      available,
      reason: available ? null : "BlockedDate",
    },
  };
}
