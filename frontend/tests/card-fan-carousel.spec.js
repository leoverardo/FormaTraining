import { test, expect } from '@playwright/test';
import { assertNoPageOverflow } from './helpers';

for (const width of [390, 768, 1440]) {
  test(`leque de telas reais funciona em ${width}px com movimento reduzido`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('/landing');
    const fan = page.getByRole('region', { name: 'Telas reais do aplicativo', exact: true });
    await fan.scrollIntoViewIfNeeded();
    await expect(fan.locator('.cf-card')).toHaveCount(10);
    await expect(fan.locator('.cf-card[aria-hidden="false"]')).toHaveCount(7);
    await expect(fan.locator('.cf-caption')).toContainText('4 de 10');
    await fan.getByRole('button', { name: 'Próxima tela', exact: true }).click();
    await expect(fan.locator('.cf-caption')).toContainText('5 de 10');
    await page.keyboard.press('ArrowRight');
    await expect(fan.locator('.cf-caption')).toContainText('6 de 10');
    await page.keyboard.press('Home');
    await expect(fan.locator('.cf-caption')).toContainText('1 de 10');
    await page.keyboard.press('ArrowLeft');
    await expect(fan.locator('.cf-caption')).toContainText('10 de 10');
    for (const image of await fan.locator('.cf-card[aria-hidden="false"] img').all()) {
      expect(await image.evaluate(element => element.complete && element.naturalWidth === 390)).toBe(true);
    }
    await assertNoPageOverflow(page);
  });
}

test('tela pode ser ampliada e fechamento devolve o foco', async ({ page }) => {
  await page.goto('/landing');
  const fan = page.getByRole('region', { name: 'Telas reais do aplicativo', exact: true });
  await fan.scrollIntoViewIfNeeded();
  const card = fan.getByRole('button', { name: 'Ampliar Treino em andamento — tema claro', exact: true });
  await card.click();
  const dialog = page.getByRole('dialog', { name: 'Treino em andamento — tema claro', exact: true });
  await expect(dialog).toBeVisible();
  await expect(dialog.getByRole('img')).toHaveAttribute('src', '/marketing/mobile/aluno-treino-light.png');
  await page.keyboard.press('Escape');
  await expect(dialog).not.toBeVisible();
  await expect(card).toBeFocused();
});

test('navegação a partir de um cartão mantém o foco no cartão selecionado', async ({ page }) => {
  await page.goto('/landing');
  const fan = page.getByRole('region', { name: 'Telas reais do aplicativo', exact: true });
  await fan.scrollIntoViewIfNeeded();
  await fan.locator('.cf-card').first().focus();
  await page.keyboard.press('ArrowRight');
  await expect(fan.locator('.cf-card').nth(4)).toBeFocused();
  await page.keyboard.press('End');
  await expect(fan.locator('.cf-card').last()).toBeFocused();
  await page.keyboard.press('ArrowRight');
  await expect(fan.locator('.cf-card').first()).toBeFocused();
});

test('tema atualiza as prévias e o carrossel respeita pausa e preferência dinâmica', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'no-preference' });
  await page.goto('/landing');
  const fan = page.getByRole('region', { name: 'Telas reais do aplicativo', exact: true });
  await fan.scrollIntoViewIfNeeded();
  await expect(fan).toHaveAttribute('data-motion', 'enabled');
  await page.getByRole('button', { name: 'Pausar animações', exact: true }).click();
  await expect(fan).toHaveAttribute('data-motion', 'reduced');
  await fan.getByRole('button', { name: 'Próxima tela', exact: true }).click();
  await expect(fan.locator('.cf-caption')).toContainText('5 de 10');
  await page.getByRole('button', { name: 'Ativar tema escuro', exact: true }).click();
  await expect(fan).toHaveAttribute('data-theme', 'dark');
  await expect(page.locator('#experiencia .student-device-screen')).toHaveAttribute('src', '/marketing/mobile/aluno-painel-dark.png');
  await page.getByRole('button', { name: 'Ativar animações', exact: true }).click();
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await expect(fan).toHaveAttribute('data-motion', 'reduced');
  await expect(page.locator('.cf-card[inert]')).toHaveCount(3);
});

test('leque cabe em tela baixa sem rolagem horizontal', async ({ page }) => {
  await page.setViewportSize({ width: 900, height: 500 });
  await page.goto('/landing');
  const fan = page.getByRole('region', { name: 'Telas reais do aplicativo', exact: true });
  await fan.scrollIntoViewIfNeeded();
  await expect(fan.getByRole('button', { name: 'Próxima tela', exact: true })).toBeVisible();
  await assertNoPageOverflow(page);
});
