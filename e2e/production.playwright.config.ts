import { defineConfig } from "@playwright/test";

// The deployed studio is verified exactly as it is served: its own certificate, its
// own gateway routes and its live API. Nothing is mocked and nothing is started here.
export default defineConfig({
  testDir: "./production",
  timeout: 180000,
  expect: { timeout: 30000 },
  workers: 1,
  retries: 2,
  reporter: [
    ["list"],
    ["json", { outputFile: "../.artifacts/production/results.json" }],
  ],
  outputDir: "../.artifacts/production/browser",
  use: {
    browserName: "chromium",
    viewport: { width: 1440, height: 900 },
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
});
