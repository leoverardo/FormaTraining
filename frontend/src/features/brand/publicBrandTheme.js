function normalizeHex(value) {
  if (typeof value !== 'string' || !/^#(?:[0-9a-f]{3}|[0-9a-f]{6})$/i.test(value)) return null;
  return value.length === 4 ? '#' + [...value.slice(1)].map(character => character.repeat(2)).join('') : value;
}

const rgb = color => color.slice(1).match(/../g).map(hex => parseInt(hex, 16));
function luminance(values) {
  const channels = values.map(value => value / 255)
    .map(channel => channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4);
  return channels[0] * 0.2126 + channels[1] * 0.7152 + channels[2] * 0.0722;
}
function onColor(color) {
  return 1.05 / (luminance(rgb(color)) + 0.05) >= 4.5 ? '#ffffff' : '#000000';
}
function readableAccent(color, theme) {
  const original = rgb(color);
  const surfaces = (theme === 'dark' ? ['#12192a', '#171f32'] : ['#ffffff', '#f5f5f7']).map(rgb);
  const backgrounds = [...surfaces, surfaces[0].map((value, index) => original[index] * 0.12 + value * 0.88)];
  const target = theme === 'dark' ? 255 : 0;
  for (let step = 0; step <= 20; step++) {
    const candidate = original.map(value => Math.round(value + (target - value) * step / 20));
    const foreground = luminance(candidate);
    if (backgrounds.every(background => {
      const behind = luminance(background);
      return (Math.max(foreground, behind) + 0.05) / (Math.min(foreground, behind) + 0.05) >= 4.6;
    })) return '#' + candidate.map(value => value.toString(16).padStart(2, '0')).join('');
  }
  return theme === 'dark' ? '#ffffff' : '#000000';
}

export function getPublicBrandTheme(profile, theme) {
  const primary = normalizeHex(profile?.primaryColor);
  const secondary = normalizeHex(profile?.secondaryColor);
  if (!primary && !secondary) return undefined;
  return {
    ...(primary && {
      '--primary': primary,
      '--primary-strong': primary,
      '--primary-soft': `color-mix(in srgb, ${primary} 12%, var(--surface))`,
      '--on-primary': onColor(primary),
      '--primary-hover': `color-mix(in srgb, ${primary} 92%, ${onColor(primary) === '#ffffff' ? 'black' : 'white'})`,
      '--link': readableAccent(primary, theme),
    }),
    ...(secondary && {
      '--brand-cyan': secondary,
      '--status-info-text': readableAccent(secondary, theme),
      '--status-info-bg': `color-mix(in srgb, ${secondary} 12%, var(--surface))`,
    }),
  };
}
