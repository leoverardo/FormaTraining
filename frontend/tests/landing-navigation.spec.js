import { test, expect } from '@playwright/test';

test.use({ reducedMotion: 'no-preference' });

async function watchScroll(page) {
  await page.evaluate(() => {
    window.navigationPositions = [];
    const sample = () => {
      window.navigationPositions.push(window.scrollY);
      if (window.navigationPositions.length < 120) requestAnimationFrame(sample);
    };
    document.addEventListener('click', () => requestAnimationFrame(sample), { once: true, capture: true });
  });
}

for (const width of [390, 768, 1440]) {
  test(`links internos percorrem a página suavemente em ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('/landing');
    const menu = page.getByRole('button', { name: 'Abrir menu', exact: true });
    if (await menu.isVisible()) await menu.click();
    await watchScroll(page);
    await page.getByRole('navigation', { name: 'Navegação principal' }).getByRole('link', { name: 'Planos', exact: true }).click();
    await expect(page).toHaveURL(/#planos$/);
    await expect.poll(() => page.locator('#planos').evaluate(element => Math.round(element.getBoundingClientRect().top))).toBe(69);
    const finalY = await page.evaluate(() => window.scrollY);
    expect(await page.evaluate(end => window.navigationPositions.some(y => y > 10 && y < end - 10), finalY)).toBe(true);
    if (width === 390) await expect(menu).toHaveAttribute('aria-expanded', 'false');
    const headerBottom = await page.locator('.gallery-nav').evaluate(element => element.getBoundingClientRect().bottom);
    expect(headerBottom).toBeLessThanOrEqual(69);
    await page.goBack();
    await expect(page).toHaveURL(/\/landing$/);
    await expect.poll(() => page.evaluate(() => Math.round(window.scrollY))).toBe(0);
  });
}

test('pausa e preferência dinâmica de movimento removem suavização', async ({ page }) => {
  await page.goto('/landing');
  const root = page.locator('html');
  await expect(root).toHaveCSS('scroll-behavior', 'smooth');
  await page.getByRole('button', { name: 'Pausar animações', exact: true }).click();
  await expect(root).toHaveCSS('scroll-behavior', 'auto');
  await page.getByRole('link', { name: 'Explore a plataforma' }).click();
  await expect.poll(() => page.locator('#produto').evaluate(element => Math.round(element.getBoundingClientRect().top))).toBe(69);
  await page.getByRole('button', { name: 'Ativar animações', exact: true }).click();
  await expect(root).toHaveCSS('scroll-behavior', 'smooth');
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await expect(root).toHaveCSS('scroll-behavior', 'auto');
});

test('links do conteúdo também deslizam e sair da landing restaura scroll normal', async ({ page }) => {
  await page.goto('/landing');
  await watchScroll(page);
  await page.getByRole('link', { name: 'Explore a plataforma' }).click();
  await expect(page).toHaveURL(/#produto$/);
  await expect.poll(() => page.locator('#produto').evaluate(element => Math.round(element.getBoundingClientRect().top))).toBe(69);
  const finalY = await page.evaluate(() => window.scrollY);
  expect(await page.evaluate(end => window.navigationPositions.some(y => y > 10 && y < end - 10), finalY)).toBe(true);
  await page.getByRole('link', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.locator('html')).toHaveCSS('scroll-behavior', 'auto');
});
