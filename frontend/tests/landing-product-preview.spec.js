import { test, expect } from '@playwright/test';

for (const width of [390, 1440]) {
  test(`notebook apresenta a tela do painel em ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('/landing');
    const preview = page.getByRole('img', { name: 'Prévia do painel do personal UpCoach' });
    await expect(preview).toBeVisible();
    expect(await preview.evaluate(image => image.complete && image.naturalWidth > 0)).toBe(true);
    expect(await page.locator('.gallery-product-stage').evaluate(element => element.getBoundingClientRect().width)).toBeLessThanOrEqual(width);
  });
}
