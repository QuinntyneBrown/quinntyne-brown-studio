import { defineConfig, devices } from "@playwright/test";

/**
 * The demonstration recordings.
 *
 * Separate from the acceptance configurations for three reasons. It records video, which the
 * ordinary suites have no reason to pay for. It runs deliberately slowly, because a recording
 * nobody can follow demonstrates nothing. And it must never gate a commit: it is a document, not
 * a test, even though it is a real browser driving the packaged applications against a real
 * LocalDB database.
 *
 *     ./scripts/record-demo.ps1
 *
 * The script publishes the API, prepares an isolated database, starts the HTTPS gateway and runs
 * this configuration; the recordings land in `docs/demo/`. What they show happened: there is no
 * compositing, no re-take and no mock. Every screen was rendered by the application and every
 * rule was enforced by the API.
 */
const origin = process.env["QBS_DEMO_ORIGIN"] ?? "https://localhost:7463";

export default defineConfig({
  testDir: "./demo",
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [["list"]],
  // Each walkthrough is minutes long by design; the ordinary minute is for a test.
  timeout: 20 * 60_000,
  // A step that cannot find its target fails within the minute rather than waiting out the
  // walkthrough. The whole take is long; no single click should be.
  expect: { timeout: 30_000 },
  outputDir: "../.artifacts/demo/recordings",
  use: {
    baseURL: origin,
    ignoreHTTPSErrors: true,
    // 720p: large enough to read the product's own type, small enough that the file is
    // something somebody can be sent.
    viewport: { width: 1280, height: 720 },
    video: { mode: "on", size: { width: 1280, height: 720 } },
    // A rhythm somebody can follow. Every click, keystroke and navigation is slowed to roughly
    // the pace a person works at, which is what makes the recording legible.
    launchOptions: { slowMo: 120 },
    actionTimeout: 30_000,
    navigationTimeout: 30_000,
    trace: "retain-on-failure",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"], viewport: { width: 1280, height: 720 } },
    },
  ],
});
