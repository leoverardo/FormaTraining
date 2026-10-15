import { test, expect } from '@playwright/test';
test.use({ reducedMotion: 'no-preference', viewport: { width: 1440, height: 1000 } });

test('3D é única, pausa, responde à preferência e limpa ao sair', async ({ page }) => {
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/landing');
  const sculpture = page.locator('.brand-sculpture');
  await expect(sculpture.locator('canvas')).toHaveCount(1);
  await page.getByRole('button', { name: 'Pausar animações', exact: true }).click();
  await expect(sculpture.locator('canvas')).toHaveCount(0);
  await page.getByRole('button', { name: 'Ativar animações', exact: true }).click();
  await expect(sculpture.locator('canvas')).toHaveCount(1);
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await expect(sculpture.locator('canvas')).toHaveCount(0);
  await page.getByRole('link', { name: 'Entrar', exact: true }).click();
  await expect(page.locator('.auth-screen')).toBeVisible();
  await expect(page.locator('canvas')).toHaveCount(0);
  expect(errors).toEqual([]);
});

test('WebGL indisponível mantém ilustração estática e CTA', async ({ page }) => {
  await page.addInitScript(() => {
    const original = HTMLCanvasElement.prototype.getContext;
    HTMLCanvasElement.prototype.getContext = function (type, ...args) {
      return type.startsWith('webgl') ? null : original.call(this, type, ...args);
    };
  });
  await page.goto('/landing');
  await expect(page.locator('.brand-sculpture')).toHaveAttribute('data-renderer', 'unavailable');
  await expect(page.locator('.brand-sculpture-fallback')).toBeVisible();
  await expect(page.getByRole('link', { name: 'Conhecer os planos' })).toBeEnabled();
});

test('celular não carrega motor 3D e resize desktop restaura fallback', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 900 });
  const requests = [];
  page.on('request', request => { if (/createBrandSculpture|three(?:\.js|\/|_)/i.test(request.url())) requests.push(request.url()); });
  await page.goto('/landing');
  await expect(page.locator('.brand-sculpture-fallback')).toBeVisible();
  expect(requests).toEqual([]);
  await expect(page.locator('.brand-sculpture canvas')).toHaveCount(0);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await expect(page.locator('.brand-sculpture canvas')).toHaveCount(1);
  await page.setViewportSize({ width: 768, height: 1000 });
  await expect(page.locator('.brand-sculpture canvas')).toHaveCount(0);
});

test('perda do contexto WebGL restaura alternativa sem erro de página', async ({ page }) => {
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/landing');
  const canvas = page.locator('.brand-sculpture canvas');
  await expect(canvas).toHaveCount(1);
  await canvas.evaluate(element => element.dispatchEvent(new Event('webglcontextlost', { cancelable: true })));
  await expect(canvas).toHaveCount(0);
  await expect(page.locator('.brand-sculpture-fallback')).toBeVisible();
  expect(errors).toEqual([]);
});
