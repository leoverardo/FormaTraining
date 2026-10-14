import { test, expect } from '@playwright/test';
import { setTheme, contrastOf, assertNoPageOverflow } from './helpers';
import { installVisualApi } from './visual-api';

test('feedback de acesso continua legível no tema escuro', async ({ page }) => {
  await setTheme(page, 'dark');
  await installVisualApi(page, 'Student');
  await page.goto('/student/access');
  const message = page.getByText('Seu acesso está ativo.', { exact: true });
  await expect(message).toBeVisible();
  expect(await contrastOf(message, message.locator('..'))).toBeGreaterThanOrEqual(4.5);
});

test('tabela com nomes extensos fica dentro da área de rolagem no celular', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 900 });
  await installVisualApi(page);
  await page.goto('/trainer/students');
  await expect(page.getByText('Marina Costa Almeida — acompanhamento de força e mobilidade', { exact: true })).toBeVisible();
  await assertNoPageOverflow(page);
});

test('aluno sem dados recebe estado vazio sem perder navegação', async ({ page }) => {
  await installVisualApi(page, 'Trainer', { emptyStudents: true });
  await page.goto('/trainer/students');
  await expect(page.getByText('Nenhum aluno cadastrado')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Novo aluno', exact: true }).first()).toBeEnabled();
});
