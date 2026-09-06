import { defineConfig } from "@playwright/test";
export default defineConfig({
  expect: { timeout: 30000 },
  testDir: "./integration",
  workers: 1,
  timeout: 180000,
  reporter: [
    ["list"],
    ["json", { outputFile: "../.artifacts/platform/fullstack-results.json" }],
  ],
  outputDir: "../.artifacts/platform/fullstack-browser",
  use: {
    browserName: "chromium",
    ignoreHTTPSErrors: true,
    viewport: { width: 1440, height: 900 },
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
});
