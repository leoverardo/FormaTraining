import { test, expect } from '@playwright/test';
import { installVisualApi, publicPage } from './visual-api';
import { setTheme, contrastOf } from './helpers';

test('menu no tablet captura foco, limita Tab e fecha com Escape', async ({ page }) => {
  await page.setViewportSize({ width: 700, height: 700 });
  await installVisualApi(page);
  await page.goto('/trainer/dashboard');
  const trigger = page.getByRole('button', { name: 'Abrir menu' });
  await trigger.focus();
  await page.keyboard.press('Enter');
  const drawer = page.locator('aside').filter({ visible: true });
  await expect(drawer.getByRole('link').first()).toBeFocused();
  await drawer.getByRole('button', { name: 'Sair' }).focus();
  await page.keyboard.press('Tab');
  await expect(drawer.getByRole('link').first()).toBeFocused();
  await page.keyboard.press('Escape');
  await expect(drawer).toBeHidden();
  await expect(trigger).toBeFocused();
});

test('página pública usa a cor configurada pelo personal', async ({ page }) => {
  await installVisualApi(page);
  await page.goto('/p/marina-coach');
  const cta = page.getByRole('button', { name: 'Falar no WhatsApp', exact: true });
  await expect(cta).toBeVisible();
  await expect(cta).toHaveCSS('background-color', 'rgb(15, 118, 110)');
});

for (const primaryColor of ['#ffffff', '#747474', '#06b6d4']) {
  test(`CTA personalizado ${primaryColor} mantém leitura ao passar o mouse`, async ({ page }) => {
    await installVisualApi(page);
    await page.route('**/api/public/trainers/marina-coach', route => route.fulfill({ json: { success: true, data: { ...publicPage, primaryColor } } }));
    await page.goto('/p/marina-coach');
    const cta = page.getByRole('button', { name: 'Falar no WhatsApp', exact: true });
    await cta.hover();
    await cta.evaluate(element => Promise.all(element.getAnimations().map(animation => animation.finished)));
    expect(await contrastOf(cta, cta)).toBeGreaterThanOrEqual(4.5);
  });
}

for (const theme of ['light', 'dark']) {
  for (const color of ['#ffffff', '#000000']) {
    test(`texto de identidade personalizada ${color} em ${theme} permanece legível`, async ({ page }) => {
      await setTheme(page, theme);
      await installVisualApi(page);
      await page.route('**/api/public/trainers/marina-coach', route => route.fulfill({ json: { success: true, data: { ...publicPage, primaryColor: color, secondaryColor: color } } }));
      await page.goto('/p/marina-coach');
      const label = page.getByText('Perfil profissional', { exact: true });
      await expect(label).toBeVisible();
      expect(await contrastOf(label, label.locator('..'))).toBeGreaterThanOrEqual(4.5);
      const tag = page.locator('span').filter({ hasText: /^Força$/ }).first();
      expect(await contrastOf(tag, tag)).toBeGreaterThanOrEqual(4.5);
    });
  }
}

test('Shift+Tab não sai da confirmação sem campos de entrada', async ({ page }) => {
  await installVisualApi(page);
  await page.goto('/trainer/students');
  await page.locator('tbody tr').first().locator('button').nth(2).click();
  const dialog = page.getByRole('dialog', { name: 'Remover aluno' });
  await expect(dialog).toBeVisible();
  await page.keyboard.press('Shift+Tab');
  expect(await dialog.evaluate(element => element.contains(document.activeElement))).toBeTruthy();
});

test('CTA da landing mantém contraste durante hover no escuro', async ({ page }) => {
  await setTheme(page, 'dark');
  await page.goto('/landing');
  const cta = page.locator('.gallery-hero .gallery-button');
  await cta.hover();
  await expect.poll(() => contrastOf(cta, cta)).toBeGreaterThanOrEqual(4.5);
  // Poll once the CSS transition has finished, so an initial frame cannot hide a failure.
  await cta.evaluate(element => Promise.all(element.getAnimations().map(animation => animation.finished)));
  expect(await contrastOf(cta, cta)).toBeGreaterThanOrEqual(4.5);
});

test('nome do treino não invade ações no celular', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 900 });
  await installVisualApi(page);
  await page.goto('/trainer/workouts');
  const title = page.getByText('Força e mobilidade', { exact: true });
  await expect(title).toBeVisible();
  expect(await title.evaluate(element => element.scrollWidth <= element.clientWidth + 1)).toBeTruthy();
});

test('hero de branding preserva hierarquia acima dos títulos de seção', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto('/branding');
  const hero = await page.locator('h1').evaluate(element => parseFloat(getComputedStyle(element).fontSize));
  const section = await page.locator('#marca h2').evaluate(element => parseFloat(getComputedStyle(element).fontSize));
  expect(hero / section).toBeGreaterThanOrEqual(1.5);
});
