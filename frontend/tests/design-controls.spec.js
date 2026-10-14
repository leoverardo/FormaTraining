import { test, expect } from '@playwright/test';
import { setTheme, signIn, assertNoPageOverflow } from './helpers';

test('botão compacto continua acessível ao toque', async ({ page }) => {
  await page.goto('/branding');
  const small = page.getByRole('button', { name: 'Small', exact: true });
  const bounds = await small.boundingBox();
  expect(bounds.height).toBeGreaterThanOrEqual(44);
});

test('tema escolhido na página pública persiste na navegação', async ({ page }) => {
  await setTheme(page, 'light');
  await page.goto('/branding');
  await page.getByRole('button', { name: 'Ativar tema escuro' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await page.goto('/login');
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
});

test('menu disponível no tablet e modal operável por teclado', async ({ page, request }) => {
  await page.setViewportSize({ width: 700, height: 900 });
  // This test exercises the empty list and modal without changing the account's subscription.
  await page.route('**/api/students', route => route.fulfill({ json: { success: true, data: [], errors: [] } }));
  await signIn(page, request);
  await page.goto('/trainer/students');
  await page.getByRole('button', { name: 'Abrir menu' }).click();
  const drawer = page.locator('aside').filter({ visible: true });
  await expect(drawer).toBeVisible();
  await drawer.getByRole('link', { name: 'Alunos', exact: true }).click();
  await page.getByRole('button', { name: 'Novo aluno', exact: true }).first().click();
  const dialog = page.getByRole('dialog', { name: 'Novo aluno' });
  await expect(dialog).toBeVisible();
  await expect(dialog.getByLabel('Nome', { exact: true })).toBeFocused();
  await dialog.getByLabel('E-mail', { exact: true }).focus();
  await page.keyboard.type('marina@example.invalid', { delay: 20 });
  await expect(dialog.getByLabel('E-mail', { exact: true })).toHaveValue('marina@example.invalid');
  await page.keyboard.press('Escape');
  await expect(dialog).not.toBeVisible();
  await assertNoPageOverflow(page);
});
