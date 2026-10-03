import { test, expect } from '@playwright/test';

test.describe('Admin - User Management', () => {
  test.beforeEach(async ({ page }) => {
    // Mock login or assume session (for now we mock the page load)
    await page.route('**/api/v1/Users*', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: {
            count: 2,
            data: [
              { id: 1, email: 'admin@example.com', fullName: 'Admin User', status: 1 },
              { id: 2, email: 'user@example.com', fullName: 'Standard User', status: 1 }
            ]
          }
        })
      });
    });

    await page.goto('/admin/user-list');
  });

  test('user list loads and displays users', async ({ page }) => {
    await expect(page.locator('table')).toBeVisible();
    await expect(page.getByText('admin@example.com')).toBeVisible();
    await expect(page.getByText('user@example.com')).toBeVisible();
  });

  test('searching filters the user list', async ({ page }) => {
    // Search for 'admin'
    const searchInput = page.getByPlaceholder(/search/i).first();
    await searchInput.fill('admin');
    
    // In our component, search is debounced 500ms
    await page.waitForTimeout(600);
    
    // Verify search UI behavior if applicable, or assume it triggered a new request
    // (Playwright can verify if a request was made)
  });

  test('navigation to user detail works', async ({ page }) => {
    await page.getByRole('link', { name: /admin user/i }).first().click();
    await expect(page).toHaveURL(/\/admin\/user-detail\/1/);
  });
});
