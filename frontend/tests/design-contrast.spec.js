import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { installVisualApi } from './visual-api';
import { setTheme } from './helpers';

const screens = [
  ['/landing', 'Trainer'], ['/branding', 'Trainer'], ['/login', 'Trainer'], ['/register', 'Trainer'],
  ['/trainer/dashboard', 'Trainer'], ['/trainer/profile', 'Trainer'], ['/trainer/students', 'Trainer'],
  ['/owner', 'Owner'], ['/student/dashboard', 'Student'], ['/student/access', 'Student'],
  ['/explore', 'Student'], ['/p/marina-coach', 'Trainer'],
];
for (const width of [390, 1440]) for (const theme of ['light', 'dark']) for (const [path, role] of screens) {
  test(`contraste AA ${path} ${width}px ${theme}`, async ({ page }) => {
    await page.setViewportSize({ width, height: 1000 });
    await setTheme(page, theme);
    await installVisualApi(page, role);
    await page.goto(path);
    await page.waitForLoadState('networkidle');
    const result = await new AxeBuilder({ page }).withRules(['color-contrast']).analyze();
    const violations = result.violations.flatMap(violation => violation.nodes.map(node => ({ target: node.target, summary: node.failureSummary })));
    expect(violations).toEqual([]);
  });
}
