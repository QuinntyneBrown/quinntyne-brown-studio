import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  timeout: 20000,
  fullyParallel: true,
  workers: 3,
  reporter: [['list'], ['json', { outputFile: 'test-results/results.json' }]],
  use: { trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  projects: ['chromium', 'firefox', 'webkit'].flatMap((browserName) => [
    {
      name: `${browserName}-mobile`,
      use: { browserName: browserName as 'chromium', viewport: { width: 390, height: 844 } },
    },
    {
      name: `${browserName}-tablet`,
      use: { browserName: browserName as 'chromium', viewport: { width: 768, height: 1024 } },
    },
    {
      name: `${browserName}-desktop`,
      use: { browserName: browserName as 'chromium', viewport: { width: 1440, height: 900 } },
    },
  ]),
  webServer: ['marketing', 'admin', 'client', 'design-system'].map((name, i) => ({
    command: `npx ng serve ${name} --port ${4300 + i}`,
    url: `http://localhost:${4300 + i}`,
    reuseExistingServer: !process.env['CI'],
    timeout: 120000,
  })),
});
