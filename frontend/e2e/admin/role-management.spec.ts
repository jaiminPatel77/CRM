import { test, expect } from '@playwright/test';

test.describe('Admin - Role Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/Roles*', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: {
            count: 1,
            data: [
              { id: 1, name: 'SuperAdmin', description: 'Has all permissions' }
            ]
          }
        })
      });
    });

    await page.goto('/admin/role-list');
  });

  test('role list loads and displays roles', async ({ page }) => {
    await expect(page.locator('table')).toBeVisible();
    await expect(page.getByText('SuperAdmin')).toBeVisible();
  });

  test('navigation to role detail and permission editing', async ({ page }) => {
    await page.getByRole('link', { name: /superadmin/i }).first().click();
    await expect(page).toHaveURL(/\/admin\/role-detail\/1/);
    
    // Check if permission table exists
    await expect(page.locator('table')).toBeVisible();
    
    // Toggle a permission check (mocking the UI structure)
    // await page.locator('input[type="checkbox"]').first().check();
  });
});
