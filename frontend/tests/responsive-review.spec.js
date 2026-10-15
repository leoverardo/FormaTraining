import { test, expect } from '@playwright/test';
import { demoId, installVisualApi } from './visual-api';
import { assertNoPageOverflow } from './helpers';

for (const width of [320, 360]) {
  test(`cabeçalho ${width}px mantém marca separada dos controles`, async ({ page }) => {
    await page.setViewportSize({ width, height: 800 });
    await installVisualApi(page);
    await page.goto('/trainer/dashboard');
    const header = page.locator('.app-shell header');
    const brand = await header.locator('.md\\:hidden.flex').boundingBox();
    const theme = await header.getByRole('button', { name: /Ativar tema/ }).boundingBox();
    expect(brand.x + brand.width + 4).toBeLessThanOrEqual(theme.x);
    await assertNoPageOverflow(page);
  });
}

test('FAQ em 320px mantém pergunta e ícone dentro do botão', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 800 });
  await page.goto('/landing');
  const question = page.getByRole('button', { name: 'Meus alunos também precisam assinar?' });
  await question.scrollIntoViewIfNeeded();
  expect(await question.evaluate(element => element.scrollWidth <= element.clientWidth + 1)).toBeTruthy();
  await question.click();
  await expect(page.locator('#faq-0')).toBeHidden();
});

test('controle de movimento no celular ocupa apenas um alvo de toque', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'no-preference' });
  await page.setViewportSize({ width: 360, height: 800 });
  await page.goto('/landing');
  const control = page.getByRole('button', { name: 'Pausar animações', exact: true });
  const bounds = await control.boundingBox();
  expect(bounds.width).toBeLessThanOrEqual(48);
  expect(bounds.height).toBeGreaterThanOrEqual(44);
  await control.click();
  await expect(page.locator('.gallery-landing')).toHaveAttribute('data-motion', 'reduced');
});

test('selo de branding em 320px mantém formato de pill sem quebrar o texto', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 800 });
  await page.goto('/branding');
  const badge = page.getByText('Uso interno', { exact: true });
  const bounds = await badge.boundingBox();
  expect(bounds.height).toBeLessThanOrEqual(36);
  await assertNoPageOverflow(page);
});

for (const [role, path] of [['Owner', '/owner/plans'], ['Trainer', '/trainer/subscription']]) {
  test(`planos ${role} em tablet deixam espaço para preço e ações`, async ({ page }) => {
    await page.setViewportSize({ width: 820, height: 900 });
    await installVisualApi(page, role);
    await page.goto(path);
    if (role === 'Trainer') await page.getByRole('button', { name: 'Trocar de plano' }).click();
    const card = page.getByRole('heading', { name: 'Basic', exact: true, level: 3 }).locator('../..');
    await expect(card).toBeVisible();
    expect((await card.boundingBox()).width).toBeGreaterThanOrEqual(220);
    await assertNoPageOverflow(page);
  });
}

for (const width of [320, 360]) test(`abas da ficha do aluno em ${width}px mostram rótulos completos`, async ({ page }) => {
  await page.setViewportSize({ width, height: 800 });
  await installVisualApi(page);
  await page.goto(`/trainer/students/${demoId}`);
  const progress = page.getByRole('button', { name: 'Progresso', exact: true });
  expect(await progress.evaluate(element => element.scrollWidth <= element.clientWidth + 1)).toBeTruthy();
  await progress.click();
  await expect(page.getByRole('button', { name: /Registrar/ }).first()).toBeVisible();
  await assertNoPageOverflow(page);
});
