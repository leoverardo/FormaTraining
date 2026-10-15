import { test, expect } from '@playwright/test';
import { assertNoPageOverflow } from './helpers';

test.use({ reducedMotion: 'no-preference' });

test('carrossel oferece setas, indicadores, teclado e pausa', async ({ page }) => {
  await page.goto('/landing');
  const carousel = page.getByRole('region', { name: 'Destaques da plataforma' });
  await carousel.scrollIntoViewIfNeeded();
  await carousel.getByRole('button', { name: 'Próximo destaque' }).click();
  await expect(carousel.getByRole('button', { name: 'Mostrar destaque 2' })).toHaveAttribute('aria-pressed', 'true');
  await carousel.getByRole('button', { name: 'Destaque anterior' }).click();
  await expect(carousel.getByRole('button', { name: 'Mostrar destaque 1' })).toHaveAttribute('aria-pressed', 'true');
  await carousel.getByRole('button', { name: 'Mostrar destaque 3' }).click();
  await expect(carousel.getByRole('button', { name: 'Mostrar destaque 3' })).toHaveAttribute('aria-pressed', 'true');
  const viewport = carousel.locator('.gallery-features');
  await viewport.focus();
  await page.keyboard.press('ArrowLeft');
  await expect(carousel.getByRole('button', { name: 'Mostrar destaque 2' })).toHaveAttribute('aria-pressed', 'true');
  await expect(carousel.getByRole('button', { name: 'Reproduzir destaques' })).toBeVisible();
});

test('scroll nativo no celular atualiza o indicador sem vazar a página', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 900 });
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await page.goto('/landing');
  const viewport = page.locator('.gallery-features');
  await viewport.scrollIntoViewIfNeeded();
  await viewport.evaluate(element => element.scrollTo({ left: element.children[1].offsetLeft, behavior: 'instant' }));
  await expect(page.getByRole('button', { name: 'Mostrar destaque 2' })).toHaveAttribute('aria-pressed', 'true');
  await assertNoPageOverflow(page);
});

test('hero acompanha scroll, narrativa muda de capítulo e pausa restaura estado', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto('/landing');
  const device = page.locator('.gallery-desktop-device');
  await expect(page.getByRole('button', { name: 'Pausar animações', exact: true })).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 420));
  await expect.poll(() => device.evaluate(element => getComputedStyle(element).transform)).not.toBe('none');
  const story = page.getByRole('region', { name: 'Uma rotina melhor' });
  await story.getByRole('button', { name: 'Acompanhe', exact: true }).click();
  await expect(story).toHaveAttribute('data-chapter', '2');
  await expect(story.locator('.gallery-story-stage .student-device')).toBeVisible();
  await page.getByRole('button', { name: 'Pausar animações', exact: true }).click();
  await expect(device).toHaveCSS('transform', 'none');
});

test('movimento reduzido exibe conteúdo e não reproduz destaques automaticamente', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await page.goto('/landing');
  await expect(page.locator('.gallery-landing')).toHaveAttribute('data-motion', 'reduced');
  await page.locator('#produto').scrollIntoViewIfNeeded();
  await expect(page.getByRole('button', { name: 'Reproduzir destaques' })).toBeDisabled();
  await expect(page.locator('.gallery-desktop-device')).toHaveCSS('transform', 'none');
});

for (const width of [390, 768]) test(`narrativa móvel ${width}px apresenta demonstração junto do texto`, async ({ page }) => {
  await page.setViewportSize({ width, height: 900 });
  await page.goto('/landing');
  const chapter = page.locator('.gallery-story-chapter[data-chapter="1"]');
  await chapter.scrollIntoViewIfNeeded();
  await expect(chapter.locator('.gallery-prescription-preview')).toBeVisible();
  await expect(page.locator('.gallery-story-stage')).toHaveCSS('position', 'static');
  await assertNoPageOverflow(page);
});

test('carrossel avança sozinho e respeita pausa por foco e pelo controle', async ({ page }) => {
  await page.setViewportSize({ width: 768, height: 1000 });
  await page.clock.install();
  await page.goto('/landing');
  const carousel = page.getByRole('region', { name: 'Destaques da plataforma' });
  await carousel.scrollIntoViewIfNeeded();
  await page.mouse.move(2, 80);
  await page.clock.runFor(6500);
  const second = carousel.getByRole('button', { name: 'Mostrar destaque 2' });
  await expect(second).toHaveAttribute('aria-pressed', 'true');
  await second.focus();
  await page.clock.runFor(6500);
  await expect(second).toHaveAttribute('aria-pressed', 'true');
  await carousel.getByRole('button', { name: 'Pausar destaques' }).click();
  await second.blur();
  await page.mouse.move(2, 80);
  await page.clock.runFor(6500);
  await expect(second).toHaveAttribute('aria-pressed', 'true');
});

test('gesto horizontal do trackpad mantém escolha e para reprodução automática', async ({ page }) => {
  await page.goto('/landing');
  const carousel = page.getByRole('region', { name: 'Destaques da plataforma' });
  const viewport = carousel.locator('.gallery-features');
  await viewport.scrollIntoViewIfNeeded();
  await viewport.hover();
  await page.mouse.wheel(650, 0);
  await expect(carousel.getByRole('button', { name: 'Mostrar destaque 2' })).toHaveAttribute('aria-pressed', 'true');
  await expect(carousel.getByRole('button', { name: 'Reproduzir destaques' })).toBeVisible();
});
