import { chromium, FullConfig } from '@playwright/test';

/**
 * Global setup for Playwright E2E tests
 * Handles authentication, data seeding, and test environment preparation
 */

async function globalSetup(config: FullConfig) {
  console.log('🚀 Starting Playwright global setup...');

  // Create a browser instance for setup tasks
  const browser = await chromium.launch();
  const context = await browser.newContext();
  const page = await context.newPage();

  try {
    // Check if the development server is running
    console.log('📡 Checking development server availability...');
    
    const baseURL = config.webServer?.url || 'http://localhost:5173';
    
    try {
      await page.goto(baseURL, { waitUntil: 'networkidle', timeout: 30000 });
      console.log('✅ Development server is accessible');
    } catch (error) {
      console.error('❌ Development server is not accessible:', error);
      throw new Error(
        `Development server at ${baseURL} is not accessible. Make sure to run 'npm run dev' first.`
      );
    }

    // Perform any global authentication or setup here
    console.log('🔧 Performing global test setup...');
    
    // Example: Set up test data, authenticate users, etc.
    // await setupTestData();
    // await authenticateTestUser();

    console.log('✅ Global setup completed successfully');

  } catch (error) {
    console.error('❌ Global setup failed:', error);
    throw error;
  } finally {
    await context.close();
    await browser.close();
  }
}

export default globalSetup;