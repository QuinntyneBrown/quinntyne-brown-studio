import { defineConfig } from "@playwright/test";
import { randomUUID } from 'node:crypto';

process.env.RECKONER_TEST_RUN_ID ??= randomUUID();
export default defineConfig({
  globalTeardown: './reckoner-global-teardown.ts',
  testDir: "./reckoner",
  workers: 1,
  timeout: 60000,
  use: {
    trace: {
      mode: "retain-on-failure",
      screenshots: false,
      snapshots: true,
      sources: true,
    },
    screenshot: "only-on-failure",
  },
  projects: ["chromium", "firefox", "webkit"].flatMap((browserName) =>
    [390, 768, 1440].map((width) => ({
      name: browserName + "-" + width,
      use: {
        browserName: browserName as "chromium",
        viewport: { width, height: 1000 },
      },
    })),
  ),
  webServer: [
    {
      command: "node scripts/serve-reckoner.mjs",
      cwd: ".",
      url: "http://127.0.0.1:4390/health/ready",
      reuseExistingServer: false,
      timeout: 90000,
    },
    {
      command: "node scripts/serve-reckoner-marketing.mjs",
      cwd: ".",
      url: "http://localhost:4420",
      reuseExistingServer: false,
      timeout: 120000,
    },
  ],
});
