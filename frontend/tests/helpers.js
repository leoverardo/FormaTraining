import { expect } from '@playwright/test';

export async function setTheme(page, theme) {
  await page.addInitScript(value => {
    if (!localStorage.getItem('fitplatform-theme')) localStorage.setItem('fitplatform-theme', value);
  }, theme);
}

export async function signIn(page, request, role = 'Trainer') {
  const accounts = { Trainer: 'trainer@test.com', Owner: 'admin@test.com', Student: 'aluno@test.com' };
  const response = await request.post('/api/auth/login', { data: { email: accounts[role], password: '123456' } });
  expect(response.ok(), `Backend local disponível e login ${role}`).toBeTruthy();
  const { data } = await response.json();
  expect(data?.token).toBeTruthy();
  await page.addInitScript(({ token, user }) => {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
  }, data);
  return data;
}

export async function assertNoPageOverflow(page) {
  const dimensions = await page.evaluate(() => ({ content: document.documentElement.scrollWidth, viewport: innerWidth }));
  expect(dimensions.content, `Overflow de página: ${page.url()}`).toBeLessThanOrEqual(dimensions.viewport + 1);
}

// WCAG luminance applied to the actual rendered foreground/background, not CSS source.
export async function contrastOf(locator, backgroundLocator) {
  const readColor = async (target, property) => target.evaluate((element, prop) => {
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = 1;
    const context = canvas.getContext('2d');
    context.fillStyle = getComputedStyle(element)[prop];
    context.fillRect(0, 0, 1, 1);
    const channels = Array.from(context.getImageData(0, 0, 1, 1).data).slice(0, 3);
    let brightness = 1;
    for (let node = element; node; node = node.parentElement) {
      const match = getComputedStyle(node).filter.match(/brightness\(([\d.]+)(%)?\)/);
      if (match) brightness *= Number(match[1]) / (match[2] ? 100 : 1);
    }
    return channels.map(channel => Math.min(255, Math.round(channel * brightness)));
  }, property);
  const foreground = await readColor(locator, 'color');
  const background = await readColor(backgroundLocator, 'backgroundColor');
  const luminance = color => {
    const channels = color.map(value => {
      const c = value / 255;
      return c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
    });
    return channels[0] * 0.2126 + channels[1] * 0.7152 + channels[2] * 0.0722;
  };
  const values = [luminance(foreground), luminance(background)].sort((a, b) => b - a);
  return (values[0] + 0.05) / (values[1] + 0.05);
}
