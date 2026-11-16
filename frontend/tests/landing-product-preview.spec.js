import { test, expect } from '@playwright/test';

for (const width of [390, 1440]) {
  test(`notebook apresenta a tela do painel em ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('/landing');
    const preview = page.getByRole('img', { name: 'Prévia do painel do personal UpCoach' });
    await expect(preview).toBeVisible();
    expect(await preview.evaluate(image => image.complete && image.naturalWidth > 0)).toBe(true);
    expect(await page.locator('.gallery-product-stage').evaluate(element => element.getBoundingClientRect().width)).toBeLessThanOrEqual(width);
  });
}

test('apresentação usa telas reais do personal e do aluno', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto('/landing');
  const experience = page.locator('#experiencia');
  await expect(experience.getByRole('img', { name: 'Tela do aluno: painel' })).toBeVisible();
  const story = page.getByRole('region', { name: 'Uma rotina melhor' });
  await story.getByRole('button', { name: 'Organize', exact: true }).click();
  await expect(story.locator('.gallery-story-stage').getByRole('img', { name: 'Tela do painel do personal' })).toBeVisible();
  await story.getByRole('button', { name: 'Acompanhe', exact: true }).click();
  await expect(story.locator('.gallery-story-stage').getByRole('img', { name: 'Tela do aluno: painel' })).toBeVisible();
});

test('telas do aluno trocam automaticamente a cada três segundos e respeitam pausa', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.emulateMedia({ reducedMotion: 'no-preference' });
  await page.clock.install();
  await page.goto('/landing');
  const section = page.locator('#experiencia');
  await section.scrollIntoViewIfNeeded();
  await expect(section.getByRole('img', { name: 'Tela do aluno: painel' })).toBeVisible();
  await page.clock.runFor(3100);
  await expect(section.getByRole('img', { name: 'Tela do aluno: treinos' })).toBeVisible();
  await page.clock.runFor(3000);
  await expect(section.getByRole('img', { name: 'Tela do aluno: progresso' })).toBeVisible();
  await page.getByRole('button', { name: 'Pausar animações', exact: true }).click();
  await page.clock.runFor(6500);
  await expect(section.getByRole('img', { name: 'Tela do aluno: progresso' })).toBeVisible();
  await page.getByRole('button', { name: 'Ativar animações', exact: true }).click();
  await page.clock.runFor(3100);
  await expect(section.getByRole('img', { name: 'Tela do aluno: check-in' })).toBeVisible();
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await page.clock.runFor(6500);
  await expect(section.getByRole('img', { name: 'Tela do aluno: painel' })).toBeVisible();
});

for (const width of [320, 390, 768, 1440]) {
  test(`fundo envolve o celular inteiro com margem em ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('/landing');
    const stage = page.locator('.gallery-student-stage');
    await stage.scrollIntoViewIfNeeded();
    const margins = await stage.evaluate(element => {
      const outer = element.getBoundingClientRect();
      const phone = element.querySelector('.student-device').getBoundingClientRect();
      return [phone.top - outer.top, outer.bottom - phone.bottom, phone.left - outer.left, outer.right - phone.right];
    });
    for (const margin of margins) expect(margin).toBeGreaterThanOrEqual(16);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width + 1);
  });
}
