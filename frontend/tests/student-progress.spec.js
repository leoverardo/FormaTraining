import { test, expect } from '@playwright/test';
import { installVisualApi } from './visual-api';
import { setTheme, assertNoPageOverflow, contrastOf } from './helpers';

test.use({ timezoneId: 'America/Sao_Paulo' });

const records = [
  { id: 'latest', progressDate: '2026-09-06T00:00:00Z', weight: 80.3, height: 178, chest: 102, waist: 82, abdomen: 85, hip: 96, bodyFatPercentage: 14.8, notes: 'Me sinto muito melhor! Roupas estão mais folgadas.\nMais disposição durante os treinos.' },
  { id: 'previous', progressDate: '2026-08-13T00:00:00Z', weight: 82.5, height: 178, chest: 100, waist: 85, abdomen: 88, hip: 97, bodyFatPercentage: 16.2, notes: 'Boa evolução no primeiro mês.' },
  { id: 'initial', progressDate: '2026-07-13T00:00:00Z', weight: 85, height: 178, chest: 98, waist: 88, abdomen: 91, hip: 99, bodyFatPercentage: 18.5, notes: 'Avaliação inicial — início do programa' },
];

async function openProgress(page, data = records) {
  await installVisualApi(page, 'Student');
  await page.route('**/api/student/progress', route => route.fulfill({ json: { success: true, data } }));
  await page.goto('/student/progress');
}

for (const width of [320, 390, 768, 1440]) {
  for (const theme of ['light', 'dark']) {
    test(`histórico de medidas legível em ${width}px ${theme}`, async ({ page }) => {
      await page.setViewportSize({ width, height: 1000 });
      await setTheme(page, theme);
      await openProgress(page);
      await expect(page.getByRole('heading', { name: 'Meu Progresso', exact: true })).toBeVisible();
      await page.screenshot({ path: `../artifacts/progress-${width}-${theme}.png`, fullPage: true });
      const history = page.getByRole('list', { name: 'Registros de evolução' });
      await expect(history.getByRole('listitem')).toHaveCount(3);
      const latest = history.getByRole('article').first();
      await expect(latest.getByRole('heading')).toHaveText('06/09/2026');
      await expect(latest.locator('dd').first()).toHaveText('80,3 kg');
      await expect(latest.getByText('Mais disposição durante os treinos.', { exact: false })).toBeVisible();
      await expect(page.getByText('-1,4 p.p.', { exact: true })).toBeVisible();
      const remove = latest.getByRole('button', { name: 'Remover registro de 06/09/2026', exact: true });
      const bounds = await remove.boundingBox();
      expect(bounds.width).toBeGreaterThanOrEqual(44);
      expect(bounds.height).toBeGreaterThanOrEqual(44);
      const note = latest.getByText('Me sinto muito melhor!', { exact: false });
      expect(await contrastOf(note, note.locator('..'))).toBeGreaterThanOrEqual(4.5);
      await assertNoPageOverflow(page);
    });
  }
}

test('registro parcial não inventa medidas ou comparação anterior', async ({ page }) => {
  await openProgress(page, [{ ...records[0], weight: null, waist: null, abdomen: null, bodyFatPercentage: 0 }]);
  await expect(page.getByText('Sem comparação anterior', { exact: true })).toBeVisible();
  const history = page.getByRole('list', { name: 'Registros de evolução' });
  await expect(history.locator('dt').filter({ hasText: /^Peso$/ })).toHaveCount(0);
  await expect(history.locator('dd').last()).toHaveText('0 %');
});

test('registro salva valores, permite cancelar exclusão e remove após confirmar', async ({ page }) => {
  await installVisualApi(page, 'Student');
  let data = [];
  let posted;
  await page.route('**/api/student/progress', async route => {
    if (route.request().method() === 'POST') {
      posted = route.request().postDataJSON();
      data = [{ id: 'new-record', ...posted }];
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.route('**/api/student/progress/new-record', async route => {
    data = [];
    await route.fulfill({ json: { success: true } });
  });
  await page.goto('/student/progress');
  await page.getByRole('button', { name: 'Registrar progresso', exact: true }).click();
  const form = page.getByRole('dialog', { name: 'Registrar progresso' });
  await form.getByLabel('Data', { exact: true }).fill('2026-09-06');
  await form.getByLabel('Peso (kg)', { exact: true }).fill('82.5');
  await form.getByLabel('Gordura corporal (%)', { exact: true }).fill('0');
  await form.getByLabel('Observações', { exact: true }).fill('Registro de teste');
  await form.getByRole('button', { name: 'Salvar', exact: true }).click();
  await expect(form).not.toBeVisible();
  expect(posted.weight).toBe(82.5);
  expect(posted.bodyFatPercentage).toBe(0);
  await page.getByRole('button', { name: 'Remover registro de 06/09/2026' }).click();
  const confirm = page.getByRole('dialog', { name: 'Remover registro' });
  await confirm.getByRole('button', { name: 'Cancelar' }).click();
  await expect(page.getByText('Registro de teste')).toBeVisible();
  await page.getByRole('button', { name: 'Remover registro de 06/09/2026' }).click();
  await confirm.getByRole('button', { name: 'Confirmar' }).click();
  await expect(page.getByRole('heading', { name: 'Nenhum registro ainda' })).toBeVisible();
});
