import { test, expect } from '@playwright/test';

/**
 * Basic browser E2E tests for Gideon application
 * Tests the renderer process in web environment
 */

test.describe('Gideon Browser Tests', () => {
  test('should load development server', async ({ page }) => {
    // Basic smoke test to ensure Playwright is working
    await page.goto('/');
    
    // Check if we can access the page (even if it shows errors)
    const title = await page.title();
    expect(title).toBeDefined();
  });

  test('should handle basic page navigation', async ({ page }) => {
    await page.goto('/');
    
    // Wait for page to load
    await page.waitForLoadState('domcontentloaded');
    
    // Check if page is responsive
    await page.setViewportSize({ width: 1200, height: 800 });
    
    // Verify we can interact with the page
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('should have secure context', async ({ page }) => {
    await page.goto('/');
    
    // Check for secure context (when served over HTTPS or localhost)
    const isSecureContext = await page.evaluate(() => window.isSecureContext);
    expect(isSecureContext).toBe(true);
  });

  test('should load without critical JavaScript errors', async ({ page }) => {
    const jsErrors: string[] = [];
    
    page.on('pageerror', (error) => {
      jsErrors.push(error.message);
    });

    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');

    // Filter out expected development warnings
    const criticalErrors = jsErrors.filter(error => 
      !error.includes('deprecated') && 
      !error.includes('warning') &&
      !error.includes('404')
    );

    expect(criticalErrors).toHaveLength(0);
  });

  test('should support modern browser features', async ({ page }) => {
    await page.goto('/');
    
    // Check for ES6+ support
    const hasModernFeatures = await page.evaluate(() => {
      return !!(window.fetch && window.Promise && window.Map);
    });
    
    expect(hasModernFeatures).toBe(true);
  });
});