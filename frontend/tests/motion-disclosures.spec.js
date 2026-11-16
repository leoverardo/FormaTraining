import { test, expect } from '@playwright/test';
import { installVisualApi, demoId } from './visual-api';
import { assertNoPageOverflow } from './helpers';

test.use({ reducedMotion: 'no-preference' });

// Observe actual frames, rather than assuming a declared duration means motion.
async function watchHeight(page, selector) {
  await page.evaluate(target => {
    window.disclosureHeights = [];
    window.disclosureFinished = false;
    document.addEventListener('click', () => {
      const start = performance.now();
      const sample = now => {
        window.disclosureHeights.push(document.querySelector(target)?.getBoundingClientRect().height || 0);
        if (now - start < 400) requestAnimationFrame(sample);
        else window.disclosureFinished = true;
      };
      requestAnimationFrame(sample);
    }, { once: true, capture: true });
  }, selector);
}

for (const width of [390, 1440]) {
  test(`treino expande e recolhe suavemente em ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await installVisualApi(page);
    await page.goto('/trainer/workouts');
    const trigger = page.getByRole('button', { name: 'Exercícios de Força e mobilidade', exact: true });
    const selector = `#workout-panel-${demoId}`;
    const panel = page.locator(selector);
    await expect(trigger).toHaveAttribute('aria-expanded', 'false');
    await watchHeight(page, selector);
    await trigger.click();
    await expect(trigger).toHaveAttribute('aria-expanded', 'true');
    await expect.poll(() => page.evaluate(() => window.disclosureFinished)).toBe(true);
    const opening = await page.evaluate(() => window.disclosureHeights);
    const fullHeight = opening.at(-1);
    expect(fullHeight).toBeGreaterThan(40);
    expect(opening.some(h => h > 1 && h < fullHeight - 1)).toBe(true);
    await expect(panel.getByText('Agachamento livre', { exact: true })).toBeVisible();
    await watchHeight(page, selector);
    await trigger.click();
    await expect.poll(() => page.evaluate(() => window.disclosureFinished)).toBe(true);
    const closing = await page.evaluate(() => window.disclosureHeights);
    expect(closing.some(h => h > 1 && h < fullHeight - 1)).toBe(true);
    expect(closing.at(-1)).toBe(0);
    await expect(panel).toHaveAttribute('inert', '');
    expect(await panel.getByRole('button', { includeHidden: true }).evaluate(button => {
      button.focus();
      return document.activeElement === button;
    })).toBe(false);
    await assertNoPageOverflow(page);
  });
}

test('expansão pode ser revertida rapidamente e continua operável por teclado', async ({ page }) => {
  await installVisualApi(page);
  await page.goto('/trainer/workouts');
  const trigger = page.getByRole('button', { name: 'Exercícios de Força e mobilidade', exact: true });
  await trigger.click();
  await trigger.click();
  await trigger.click();
  await expect(trigger).toHaveAttribute('aria-expanded', 'true');
  await expect(page.getByText('Agachamento livre', { exact: true })).toBeVisible();
  await trigger.focus();
  await page.keyboard.press('Space');
  await expect(trigger).toHaveAttribute('aria-expanded', 'false');
  await page.keyboard.press('Enter');
  await expect(trigger).toHaveAttribute('aria-expanded', 'true');
});

test('movimento reduzido muda o estado sem transição e pode mudar ao vivo', async ({ page }) => {
  await installVisualApi(page);
  await page.goto('/trainer/workouts');
  const trigger = page.getByRole('button', { name: 'Exercícios de Força e mobilidade', exact: true });
  const panel = page.locator(`#workout-panel-${demoId}`);
  await trigger.click();
  await page.emulateMedia({ reducedMotion: 'reduce' });
  expect(await panel.evaluate(element => getComputedStyle(element).transitionDuration.split(',').every(value => parseFloat(value) < .001))).toBe(true);
  await trigger.click();
  await expect(panel).toHaveAttribute('aria-hidden', 'true');
  await expect.poll(() => panel.evaluate(element => element.getBoundingClientRect().height)).toBe(0);
});

test('FAQ usa expansão suave e respeita o botão de pausar animações', async ({ page }) => {
  await page.goto('/landing');
  const trigger = page.getByRole('button', { name: 'Posso usar minha identidade visual?', exact: true });
  await trigger.scrollIntoViewIfNeeded();
  await watchHeight(page, '#faq-1');
  await trigger.click();
  await expect.poll(() => page.evaluate(() => window.disclosureFinished)).toBe(true);
  const heights = await page.evaluate(() => window.disclosureHeights);
  expect(heights.some(h => h > 1 && h < heights.at(-1) - 1)).toBe(true);
  await page.getByRole('button', { name: 'Pausar animações', exact: true }).click();
  await expect(page.locator('#faq-1')).toHaveAttribute('data-motion', 'reduced');
  await trigger.click();
  await expect(page.locator('#faq-1')).toHaveCSS('grid-template-rows', '0px');
});

test('modal preserva foco, anima saída e permite reabrir imediatamente', async ({ page }) => {
  await installVisualApi(page);
  await page.goto('/trainer/workouts');
  const opener = page.getByRole('button', { name: 'Novo treino', exact: true }).first();
  await opener.click();
  const dialog = page.getByRole('dialog', { name: 'Novo treino', exact: true });
  await expect(dialog.getByLabel('Nome do treino')).toBeFocused();
  await dialog.getByLabel('Nome do treino').fill('Teste de movimento');
  await page.evaluate(() => {
    window.sawModalExit = false;
    const observe = () => {
      const closing = document.querySelector('.uc-modal[data-open="false"] .uc-modal__panel');
      const opacity = closing ? Number(getComputedStyle(closing).opacity) : 0;
      if (opacity > 0 && opacity < 1) window.sawModalExit = true;
      if (document.querySelector('.uc-modal')) requestAnimationFrame(observe);
    };
    requestAnimationFrame(observe);
  });
  await page.keyboard.press('Escape');
  await expect(opener).toBeFocused();
  await expect(dialog).not.toBeVisible();
  await expect.poll(() => page.evaluate(() => window.sawModalExit)).toBe(true);
  await opener.click();
  await expect(dialog).toBeVisible();
  await expect(dialog.getByLabel('Nome do treino')).toBeFocused();
  await dialog.getByRole('button', { name: 'Cancelar', exact: true }).click();
  await expect(page.locator('.uc-modal')).toHaveCount(0);
  expect(await page.evaluate(() => document.body.style.overflow)).not.toBe('hidden');
});

test('menu móvel desliza sem perder navegação nem prender foco', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 900 });
  await installVisualApi(page);
  await page.goto('/trainer/workouts');
  const opener = page.getByRole('button', { name: 'Abrir menu', exact: true });
  await opener.click();
  const drawer = page.getByRole('dialog', { name: 'Menu de navegação', exact: true });
  await expect(drawer).toBeVisible();
  await expect(drawer.getByRole('link').first()).toBeFocused();
  await drawer.getByRole('link', { name: 'Alunos', exact: true }).click();
  await expect(page).toHaveURL(/\/trainer\/students$/);
  await expect(page.locator('.uc-drawer')).toHaveCount(0);
  await expect(opener).toBeFocused();
  expect(await page.evaluate(() => document.body.style.overflow)).not.toBe('hidden');
  await assertNoPageOverflow(page);
});

test('notificações podem fechar e reabrir sem acumular painéis', async ({ page }) => {
  await installVisualApi(page);
  await page.goto('/trainer/workouts');
  const trigger = page.getByRole('button', { name: 'Notificações', exact: true });
  await trigger.click();
  await expect(trigger).toHaveAttribute('aria-expanded', 'true');
  await expect(page.getByText('Nenhuma notificação', { exact: true })).toBeVisible();
  await trigger.click({ force: true });
  await expect(trigger).toHaveAttribute('aria-expanded', 'false');
  await trigger.click({ force: true });
  await expect(trigger).toHaveAttribute('aria-expanded', 'true');
  await expect(page.locator('.uc-popover')).toHaveCount(1);
  await trigger.click({ force: true });
  await expect(page.locator('.uc-popover')).toHaveCount(0);
});
