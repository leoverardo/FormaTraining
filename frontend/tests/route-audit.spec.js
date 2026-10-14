import { test, expect } from '@playwright/test';
import { readFileSync, mkdirSync, writeFileSync } from 'node:fs';
import { setTheme, assertNoPageOverflow } from './helpers';
import { demoId, installVisualApi } from './visual-api';

const app = readFileSync(new URL('../src/App.jsx', import.meta.url), 'utf8');
const routePatterns = [...app.matchAll(/<Route path="([^"]+)"/g)].map(match => match[1]);
const publicPatterns = routePatterns.filter(path => !path.startsWith('/trainer') && !path.startsWith('/student') && !path.startsWith('/explore') && !path.startsWith('/owner') && !['/', '*'].includes(path));
publicPatterns.push('/student/register');

for (const width of [390, 768, 1440]) {
  for (const theme of ['light', 'dark']) {
    for (const role of ['Public', 'Trainer', 'Owner', 'Student']) {
      test(`rotas ${role} ${width}px ${theme}`, async ({ page }) => {
        test.setTimeout(90000);
        await page.setViewportSize({ width, height: 1000 });
        await setTheme(page, theme);
        const api = await installVisualApi(page, role === 'Public' ? 'Trainer' : role);
        const patterns = role === 'Public' ? publicPatterns : routePatterns.filter(path =>
          role === 'Trainer' ? path.startsWith('/trainer') : role === 'Owner' ? path.startsWith('/owner') :
            (path.startsWith('/student') && path !== '/student/register') || path.startsWith('/explore'));
        const errors = [];
        page.on('pageerror', error => errors.push(error.message));
        const results = [];
        for (const pattern of patterns) {
          const path = pattern.replace(':id', demoId).replace(':slug', 'marina-coach');
          await page.goto(path);
          await page.waitForLoadState('networkidle');
          const content = page.locator('.app-content').count().then(count => count ? page.locator('.app-content') : page.locator('#root'));
          expect((await (await content).innerText()).trim().length, `Conteúdo em ${path}`).toBeGreaterThan(12);
          const allowedDestination = pattern === '/student' ? '/student/dashboard' : path;
          expect(new URL(page.url()).pathname, `Destino da rota ${pattern}`).toBe(allowedDestination);
          await assertNoPageOverflow(page);
          results.push({ pattern, path, destination: new URL(page.url()).pathname, errors: [...errors] });
        }
        expect(errors).toEqual([]);
        expect([...new Set(api.unhandled)], 'Completar fixtures de contratos, sem ocultar endpoints').toEqual([]);
        mkdirSync('../artifacts/design/routes', { recursive: true });
        writeFileSync(`../artifacts/design/routes/${role}-${width}-${theme}.json`, JSON.stringify(results, null, 2));
      });
    }
  }
}

test('redirecionamentos conservam destinos e permissões', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveURL(/\/login$/);
  await page.goto('/trainer/dashboard');
  await expect(page).toHaveURL(/\/login$/);
  await page.goto('/rota-inexistente');
  await expect(page).toHaveURL(/\/login$/);
});
