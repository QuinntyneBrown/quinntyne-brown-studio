import { defineConfig } from '@playwright/test';

// The suite is its own project: page objects under page-objects/, specs under specs/.
// Each application is served from the Angular workspace next door with mocked APIs.
export default defineConfig({
  testDir: './specs',
  timeout: 60000,
  fullyParallel: true,
  workers: 1,
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
  webServer: ['marketing', 'admin', 'client'].map((name, index) => ({
    command: `npx ng serve ${name} --port ${4320 + index}${name === 'marketing' ? ' --configuration acceptance' : ''}`,
    cwd: '../frontend',
    url: `http://localhost:${4320 + index}`,
    reuseExistingServer: !process.env['CI'],
    timeout: 120000,
  })),
});
