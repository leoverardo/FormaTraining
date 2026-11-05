import { test, expect } from '@playwright/test';

for (const width of [390, 1440]) {
  test(`landing mantém apresentação sem escultura 3D em ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await page.emulateMedia({ reducedMotion: 'no-preference' });
    const requests = [];
    page.on('request', request => {
      if (request.url().includes('createBrandSculpture')) requests.push(request.url());
    });
    await page.goto('/landing');
    await expect(page.locator('.gallery-desktop-device')).toBeVisible();
    await expect(page.getByRole('link', { name: 'Conhecer os planos' })).toBeVisible();
    await expect(page.locator('.brand-sculpture, .gallery-product-stage canvas')).toHaveCount(0);
    expect(requests).toEqual([]);
  });
}
