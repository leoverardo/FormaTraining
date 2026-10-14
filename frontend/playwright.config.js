import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 45000,
  expect: { timeout: 8000 },
  workers: 2,
  fullyParallel: true,
  reporter: [['list'], ['html', { outputFolder: '../artifacts/design/report', open: 'never' }]],
  outputDir: '../artifacts/design/test-results',
  use: {
    actionTimeout: 8000,
    navigationTimeout: 15000,
    baseURL: 'http://localhost:5173',
    channel: 'chrome',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  webServer: {
    command: 'npm run dev -- --host 127.0.0.1',
    url: 'http://localhost:5173',
    reuseExistingServer: true,
    timeout: 30000,
  },
});
