import { expect, test } from '@playwright/test';
import { assertNoPageOverflow, contrastOf, setTheme } from './helpers';

test('login ocupa a metade da tela com a arte e permite trocar o tema', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await setTheme(page, 'light');
  await page.goto('/login');

  const artwork = page.getByLabel('Arte de apresentação da UpCoach');
  const form = page.getByRole('heading', { name: 'Bem-vindo de volta' });
  const artBox = await artwork.boundingBox();
  expect(artBox.x).toBe(0);
  expect(artBox.width).toBe(720);
  expect(artBox.height).toBeGreaterThanOrEqual(900);
  await expect(artwork.locator('a')).toHaveCount(0);
  await expect(form).toBeVisible();
  await page.getByRole('button', { name: 'Ativar tema escuro' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await expect(form).toBeVisible();
  await assertNoPageOverflow(page);
});

test('formulário continua acessível no celular', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/login');
  await expect(page.getByLabel('E-mail')).toBeVisible();
  await expect(page.getByLabel('Senha')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Entrar' })).toBeVisible();
  await assertNoPageOverflow(page);
});

test('painel do formulário tem contraste nos temas claro e escuro', async ({ page }) => {
  await setTheme(page, 'light');
  await page.goto('/login');
  const panel = page.locator('.login-panel');
  const card = page.locator('.login-panel .ds-card');
  const heading = page.getByRole('heading', { name: 'Bem-vindo de volta' });
  const lightBackground = await panel.evaluate(el => getComputedStyle(el).backgroundColor);
  expect(lightBackground).not.toBe('rgb(255, 255, 255)');
  expect(await contrastOf(heading, card)).toBeGreaterThanOrEqual(4.5);
  await page.getByRole('button', { name: 'Ativar tema escuro' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  const darkBackground = await panel.evaluate(el => getComputedStyle(el).backgroundColor);
  expect(darkBackground).not.toBe(lightBackground);
  expect(await contrastOf(heading, card)).toBeGreaterThanOrEqual(4.5);
});
