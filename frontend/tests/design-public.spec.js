import { test, expect } from '@playwright/test';
import { setTheme, contrastOf, assertNoPageOverflow } from './helpers';

for (const width of [390, 768, 1440]) {
  for (const theme of ['light', 'dark']) {
    test(`landing UpCoach ${width}px ${theme}`, async ({ page }) => {
      await page.setViewportSize({ width, height: 1000 });
      await setTheme(page, theme);
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      await page.goto('/landing');
      await expect(page.locator('main h1')).toBeVisible();
      expect(await page.locator('body').innerText()).not.toContain('Forma Training');
      expect(await contrastOf(page.locator('.gallery-hero-description'), page.locator('.gallery-landing'))).toBeGreaterThanOrEqual(4.5);
      if (theme === 'dark') {
        const bg = await page.locator('.gallery-landing').evaluate(el => getComputedStyle(el).backgroundColor);
        expect(bg).not.toBe('rgb(255, 255, 255)');
      }
      await expect(page.getByRole('button', { name: theme === 'dark' ? 'Ativar tema claro' : 'Ativar tema escuro' })).toBeVisible();
      if (width < 768) {
        await page.getByRole('button', { name: 'Abrir menu' }).click();
        await page.getByRole('navigation').getByRole('link', { name: 'Planos', exact: true }).click();
        await expect(page).toHaveURL(/#planos$/);
      }
      await page.getByRole('button', { name: 'Meus alunos também precisam assinar?' }).click();
      await expect(page.locator('#faq-0')).toBeHidden();
      await assertNoPageOverflow(page);
      expect(errors).toEqual([]);
    });
  }
}

test('login preserva arte oficial proporcional e permite mudar tema', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 1000 });
  await setTheme(page, 'light');
  await page.goto('/login');
  for (const asset of ['Capa.png', 'SubCapaUpCoach.png']) {
    const image = page.locator(`img[src="/brand/${asset}"]`);
    await expect(image).toBeVisible();
    expect(await image.evaluate(el => Math.abs(el.clientWidth / el.clientHeight - el.naturalWidth / el.naturalHeight))).toBeLessThan(0.03);
  }
  await page.getByRole('button', { name: 'Ativar tema escuro' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
});

test('página legal mostra falha de rede em vez de erro silencioso', async ({ page }) => {
  await page.route('**/api/legal/privacy-policy', route => route.fulfill({ status: 503, json: { success: false, message: 'Indisponível' } }));
  await page.goto('/privacy-policy');
  await expect(page.getByRole('alert')).toContainText('Não foi possível carregar');
});
