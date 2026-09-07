/**
 * Everything the demonstration types, and the figures it must then see.
 *
 * The three recordings share one database: administration sets these values up, the public site
 * shows them, and the client's collection is built from them. Keeping every name and number here
 * means a caption in one recording cannot disagree with a form in another.
 */

/** The calendar date, `YYYY-MM-DD`, of the Saturday at least `days` days on either side of today. */
function saturday(days: number): string {
  const date = new Date();
  date.setHours(12, 0, 0, 0);
  date.setDate(date.getDate() + days);
  // Saturday is 6; step forward for a future date and back for a past one until it is one.
  const step = days >= 0 ? 1 : -1;
  while (date.getDay() !== 6) date.setDate(date.getDate() + step);
  return date.toISOString().slice(0, 10);
}

function firstOfThisMonth(): string {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-01`;
}

/** Cents, rounded away from zero on a half like the API's `Rules.Round`. */
function cents(amount: number): number {
  return Math.round(amount);
}

function money(centsValue: number): string {
  return (centsValue / 100).toFixed(2);
}

export const Studio = {
  email: process.env["Bootstrap__Email"] ?? "",
  password: process.env["Bootstrap__Password"] ?? "",
  photographer: "Quinntyne Brown",
  name: "Junction studio",
  hourlyFee: "65",
  address: "2 Junction Road, Toronto",
  /** The controlled routing adapter labels every candidate this way. */
  resolvedAddress: "Controlled example: 2 Junction Road, Toronto",
};

export const Rates = {
  services: { Wedding: "250", Event: "175", Headshot: "150", FamilyPortrait: "195" },
  costs: {
    "travel · per kilometre": "0.85",
    "equipment · per unit/session": "45",
    "lunch · per person": "22",
    "assistant · per hour": "40",
  },
};

export const Discounts = {
  advanceDays: "60",
  advancePercent: "10",
  weekdayPercent: "5",
  weekdays: ["Tuesday", "Wednesday", "Thursday"],
  code: "AUTUMN26",
  codePercent: "15",
  codeValidFrom: firstOfThisMonth(),
  /** The quote summary names the winning rule and its percentage, to two places like every decimal. */
  advanceLine: "Advance booking · 10.00%",
  codeLine: "Discount code · 15.00%",
};

export const Equipment = {
  name: "Profoto B10 lighting kit",
  description: "Two heads, stands and a large octabox.",
  quantity: "2",
  rentalRate: "85",
};

export const Vendor = {
  name: "Lina Moreau",
  email: "lina@example.com",
  phone: "416 555 0134",
  role: "MakeupArtist",
};

export const Client = {
  name: "Amara Okafor",
  email: "amara.okafor@example.com",
  password: "Portraits-2026!",
};

/** A session that has already happened, so its photographs are ready to upload. */
export const Session = {
  name: "Okafor family portraits",
  service: "FamilyPortrait",
  date: saturday(-7),
  start: "10:00",
  end: "12:00",
  /** The working window that lets the photographer accept it: buffers of 30 minutes each side. */
  window: { start: "09:00", end: "13:00" },
};

export const Gallery = {
  title: "Family portraits, autumn light",
  slug: "family-portraits-autumn-light",
  photos: ["okafor-01.jpg", "okafor-02.jpg", "okafor-03.jpg", "okafor-05.jpg"],
  cover: "okafor-03.jpg",
  /** The public site never shows file names; every photograph is named for its gallery. */
  publicPhotoName: "Photograph from Family portraits, autumn light",
};

export const Prints = [
  { name: "Fine art print", dimensions: "8 × 10 in", finish: "Matte", unitPrice: "45" },
  { name: "Gallery print", dimensions: "16 × 20 in", finish: "Lustre", unitPrice: "120" },
];

export const Promotion = {
  title: "Autumn family sessions",
  description:
    "A relaxed hour at the studio or in a park you love, with ten retouched photographs.",
  indicativePrice: "495",
};

export const Content = {
  heading: "Your story, thoughtfully photographed.",
  body: "Weddings, events, headshots and family portraits across Toronto.",
};

export const Album = {
  name: "Wall gallery",
  selected: ["okafor-02.jpg", "okafor-04.jpg", "okafor-05.jpg"],
  /** After `okafor-05.jpg` is moved one place earlier. */
  order: ["okafor-02.jpg", "okafor-05.jpg", "okafor-04.jpg"],
};

export const PrintRequest = {
  photos: ["okafor-01.jpg", "okafor-03.jpg"],
  notes: "Could the 16 × 20 be framed in oak? We would love to see it before it ships.",
  /** One fine art print at 45.00 and two gallery prints at 120.00. */
  total: money(cents(45_00 + 2 * 120_00)),
};

/**
 * The quote the public site asks for: a Saturday wedding far enough ahead for the advance discount,
 * eight hours, one location, priced from the rates above and the controlled 12 km round trip.
 */
const quoteHours = 8;
const roundTripKm = 12;
const photographyCents = quoteHours * 250_00;
const travelCents = cents(roundTripKm * 85);
const detailCents = 2 * 45_00 + 3 * 22_00 + 1 * quoteHours * 40_00 + 25_00 + cents(1.5 * 65_00);
const afterLocation = photographyCents + travelCents;
const afterDetails = afterLocation + detailCents;

export const Quote = {
  service: "Wedding",
  date: saturday(270),
  start: "10:00",
  end: "18:00",
  address: "St. Lawrence Hall, Toronto",
  resolvedAddress: "Controlled example: St. Lawrence Hall, Toronto",
  parking: "25",
  studioHours: "1.5",
  assistants: "1",
  equipment: "2",
  lunches: "3",
  /** The photographer's working window on that day, wide enough for the buffers. */
  window: { start: "08:00", end: "20:00" },
  lines: { photography: money(photographyCents), travel: money(travelCents) },
  totals: {
    afterLocation: money(afterLocation - cents((afterLocation * 10) / 100)),
    afterDetails: money(afterDetails - cents((afterDetails * 10) / 100)),
    afterCode: money(afterDetails - cents((afterDetails * 15) / 100)),
  },
};
