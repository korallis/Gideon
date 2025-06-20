import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for E2E testing
 * Focused on Electron app testing with cross-browser validation
 */
export default defineConfig({
  // Test directory
  testDir: './tests/e2e',
  
  // Global timeout settings
  timeout: 30 * 1000,
  expect: {
    timeout: 5000,
  },
  
  // Full parallel for faster testing
  fullyParallel: true,
  
  // Fail on CI if test source files have warnings
  forbidOnly: !!process.env.CI,
  
  // Retry on CI only
  retries: process.env.CI ? 2 : 0,
  
  // Opt out of parallel tests on CI
  workers: process.env.CI ? 1 : undefined,
  
  // Reporter to use
  reporter: [
    ['html'],
    ['json', { outputFile: 'playwright-report/results.json' }],
    ['junit', { outputFile: 'playwright-report/results.xml' }]
  ],
  
  // Shared settings for all projects
  use: {
    // Base URL for testing
    baseURL: 'http://localhost:5173',
    
    // Action timeout
    actionTimeout: 10 * 1000,
    
    // Collect trace when retrying the failed test
    trace: 'on-first-retry',
    
    // Take screenshots on failure
    screenshot: 'only-on-failure',
    
    // Record video on failure
    video: 'retain-on-failure',
  },

  // Configure projects for major browsers and Electron
  projects: [
    {
      name: 'electron',
      use: {
        ...devices['Desktop Chrome'],
        // Electron-specific configuration
        viewport: { width: 1200, height: 800 },
        ignoreHTTPSErrors: true,
      },
      testMatch: '**/electron/**/*.spec.ts',
    },
    
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
      testMatch: '**/browser/**/*.spec.ts',
    },

    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
      testMatch: '**/browser/**/*.spec.ts',
    },

    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
      testMatch: '**/browser/**/*.spec.ts',
    },
  ],

  /* Run your local dev server before starting the tests */
  // webServer: {
  //   command: 'npm run dev:vite',
  //   url: 'http://localhost:5173',
  //   reuseExistingServer: !process.env.CI,
  //   timeout: 120 * 1000,
  // },
});