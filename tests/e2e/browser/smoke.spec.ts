import { test, expect } from '@playwright/test';

/**
 * Smoke tests to verify Playwright setup is working
 */

test.describe('Playwright Setup Verification', () => {
  test('should access external website', async ({ page }) => {
    // Basic test to ensure Playwright browser is working
    await page.goto('https://httpbin.org/json');
    
    // Check if we can navigate and interact with a simple page
    const body = await page.textContent('body');
    expect(body).toContain('json');
  });

  test('should handle JavaScript execution', async ({ page }) => {
    await page.goto('data:text/html,<html><body><h1 id="test">Hello World</h1></body></html>');
    
    // Test JavaScript execution
    const title = await page.evaluate(() => document.querySelector('#test')?.textContent);
    expect(title).toBe('Hello World');
  });

  test('should capture screenshots', async ({ page }) => {
    await page.goto('data:text/html,<html><body style="background:red;width:100px;height:100px;"></body></html>');
    
    // Verify we can take screenshots
    const screenshot = await page.screenshot();
    expect(screenshot).toBeTruthy();
    expect(screenshot.length).toBeGreaterThan(0);
  });

  test('should handle viewport changes', async ({ page }) => {
    await page.goto('data:text/html,<html><body><div id="test">Test</div></body></html>');
    
    // Test viewport manipulation
    await page.setViewportSize({ width: 800, height: 600 });
    const viewportSize = page.viewportSize();
    
    expect(viewportSize?.width).toBe(800);
    expect(viewportSize?.height).toBe(600);
  });
});