import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests",
  fullyParallel: true,
  forbidOnly: Boolean(process.env.CI),
  reporter: [["list"], ["json", { outputFile: "test-results/results.json" }]],
  webServer: {
    command: "npm run serve:test",
    url: "http://127.0.0.1:4181/",
    reuseExistingServer: !process.env.CI,
    timeout: 60_000,
  },
  use: {
    baseURL: "http://127.0.0.1:4181/",
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
  workers: 1,
  // Chromium only (AGENTS.md): the three agreed widths are the whole matrix.
  projects: [
    {
      name: "chromium-mobile",
      use: { browserName: "chromium", viewport: { width: 390, height: 844 } },
    },
    {
      name: "chromium-tablet",
      use: { browserName: "chromium", viewport: { width: 768, height: 1024 } },
    },
    {
      name: "chromium-desktop",
      use: { browserName: "chromium", viewport: { width: 1440, height: 900 } },
    },
  ],
});
