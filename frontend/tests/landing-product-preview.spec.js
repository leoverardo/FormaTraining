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

test('telas do aluno acompanham a rolagem e respeitam movimento reduzido', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.emulateMedia({ reducedMotion: 'no-preference' });
  await page.goto('/landing');
  const section = page.locator('#experiencia');
  const scrollToProgress = async progress => {
    await page.evaluate(value => {
      const section = document.querySelector('#experiencia');
      const rect = section.getBoundingClientRect();
      window.scrollTo({ top: rect.top + window.scrollY - window.innerHeight * 0.8 + value * (rect.height + window.innerHeight * 0.45), behavior: 'instant' });
    }, progress);
  };
  await scrollToProgress(0.15);
  await expect(section.getByRole('img', { name: 'Tela do aluno: painel' })).toBeVisible();
  await scrollToProgress(0.5);
  await expect(section.getByRole('img', { name: 'Tela do aluno: treinos' })).toBeVisible();
  await scrollToProgress(0.85);
  await expect(section.getByRole('img', { name: 'Tela do aluno: progresso' })).toBeVisible();
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await expect(section.getByRole('img', { name: 'Tela do aluno: painel' })).toBeVisible();
});
