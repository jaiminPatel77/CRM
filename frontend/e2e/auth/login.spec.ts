import { test, expect } from '@playwright/test';

test.describe('Auth - Login', () => {
  test('login page loads correctly', async ({ page }) => {
    await page.goto('/auth/login');
    await expect(page.locator('form')).toBeVisible();
  });

  test('shows validation errors on empty submit', async ({ page }) => {
    await page.goto('/auth/login');
    await page.getByRole('button', { name: /login|sign in/i }).click();
    await expect(page.locator('.invalid-feedback, .error')).toBeVisible();
  });
});
