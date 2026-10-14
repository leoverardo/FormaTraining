import { test, expect } from '@playwright/test';
import { setTheme, contrastOf, assertNoPageOverflow } from './helpers';

for (const theme of ['light', 'dark']) {
  test(`texto auxiliar legível e fonte local em ${theme}`, async ({ page }) => {
    await setTheme(page, theme);
    await page.goto('/branding');
    await expect(page.getByRole('heading', { name: 'Treine, gerencie, conquiste.' })).toBeVisible();
    const label = page.locator('#tipografia .font-mono').first();
    const card = page.locator('#tipografia .ds-card').first();
    expect(await contrastOf(label, card)).toBeGreaterThanOrEqual(4.5);
    expect(await page.locator('body').evaluate(el => getComputedStyle(el).fontFamily)).toContain('Inter');
    await assertNoPageOverflow(page);
  });
}
