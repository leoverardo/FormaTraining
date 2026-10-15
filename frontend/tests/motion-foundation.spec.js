import { test, expect } from '@playwright/test';
import { installVisualApi } from './visual-api';

test.use({ reducedMotion: 'no-preference' });

async function observeEntrance(page, selector) {
  await page.addInitScript(target => {
    window.sawEntrance = false;
    const observe = () => {
      const element = document.querySelector(target);
      if (element && getComputedStyle(element).transform !== 'none') window.sawEntrance = true;
      requestAnimationFrame(observe);
    };
    requestAnimationFrame(observe);
  }, selector);
}

test('login entra com movimento sem interromper digitação', async ({ page }) => {
  await observeEntrance(page, '.auth-screen h2');
  await page.goto('/login');
  await expect.poll(() => page.evaluate(() => window.sawEntrance)).toBe(true);
  const email = page.getByLabel('E-mail', { exact: true });
  await email.fill('marina@example.invalid');
  await expect(email).toHaveValue('marina@example.invalid');
  await expect(page.locator('.auth-screen h2')).toHaveCSS('transform', 'none');
});

test('painel interno recebe entrada e respeita preferência alterada ao vivo', async ({ page }) => {
  await installVisualApi(page);
  await observeEntrance(page, '.app-content h1');
  await page.goto('/trainer/dashboard');
  await expect.poll(() => page.evaluate(() => window.sawEntrance)).toBe(true);
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await expect(page.locator('.app-content h1')).toHaveCSS('transform', 'none');
  await expect(page.locator('.app-shell')).toHaveAttribute('data-motion', 'reduced');
});

test('movimento reduzido mantém login estático e acessível', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await observeEntrance(page, '.auth-screen h2');
  await page.goto('/login');
  await expect(page.locator('.auth-screen')).toHaveAttribute('data-motion', 'reduced');
  expect(await page.evaluate(() => window.sawEntrance)).toBe(false);
  await expect(page.getByRole('button', { name: 'Entrar', exact: true })).toBeEnabled();
});

test('tabela e ações permanecem imóveis durante entrada da página', async ({ page }) => {
  await installVisualApi(page);
  await observeEntrance(page, '.app-content .ds-card:has(table)');
  await page.goto('/trainer/students');
  await expect(page.locator('.app-shell')).toHaveAttribute('data-motion', 'enabled');
  await expect(page.locator('.app-content h1')).toHaveCSS('transform', 'none');
  expect(await page.evaluate(() => window.sawEntrance)).toBe(false);
});

test('título do login não atravessa a descrição durante a entrada', async ({ page }) => {
  await page.addInitScript(() => {
    window.motionOverlap = false;
    window.sawEntrance = false;
    const observe = () => {
      const heading = document.querySelector('.auth-screen h2');
      const description = heading?.nextElementSibling;
      if (heading && description && getComputedStyle(heading).transform !== 'none') {
        window.sawEntrance = true;
        if (heading.getBoundingClientRect().bottom > description.getBoundingClientRect().top + 1) window.motionOverlap = true;
      }
      requestAnimationFrame(observe);
    };
    requestAnimationFrame(observe);
  });
  await page.goto('/login');
  await expect.poll(() => page.evaluate(() => window.sawEntrance)).toBe(true);
  await expect(page.locator('.auth-screen h2')).toHaveCSS('transform', 'none');
  expect(await page.evaluate(() => window.motionOverlap)).toBe(false);
});
