import { useTheme } from '../../contexts/ThemeContext';
import { BRAND_ASSETS } from './brandAssets';

const SIZES = {
  xs: { symbol: 'h-7 w-7', text: 'text-base' },
  sm: { symbol: 'h-8 w-8', text: 'text-lg' },
  md: { symbol: 'h-10 w-10', text: 'text-xl' },
  lg: { symbol: 'h-12 w-12', text: 'text-2xl' },
};

export function UpSymbol({ className = '', mono = false, title = 'UpCoach', tone = 'light', primary = false }) {
  const src = primary ? BRAND_ASSETS.icons.primary : mono ? BRAND_ASSETS.icons.secondary : BRAND_ASSETS.icons[tone] || BRAND_ASSETS.icons.light;
  return (
    <img src={src} alt={title} width={1254} height={1254}
      className={`shrink-0 object-contain ${className}`} aria-hidden={title ? undefined : true} />
  );
}

export function BrandLogo({
  size = 'md',
  tone = 'auto',
  variant = 'horizontal',
  showText = true,
  className = '',
  textClassName = '',
}) {
  const { theme } = useTheme();
  const resolvedTone = tone === 'auto' ? theme : tone;
  const s = SIZES[size] || SIZES.md;
  const dark = resolvedTone === 'dark';
  const upClass = dark ? 'text-indigo-300' : 'text-[var(--brand-indigo-strong)]';
  const coachClass = dark ? 'text-white' : 'text-[var(--brand-ink)]';

  if (variant === 'symbol' || !showText) {
    return (
      <span className={`inline-flex items-center ${className}`}>
        <UpSymbol primary className={s.symbol} />
      </span>
    );
  }

  if (variant === 'mono') {
    return (
      <span className={`inline-flex items-center gap-2 ${className}`}>
        <UpSymbol mono className={s.symbol} title="" />
        {showText && (
          <span className={`font-extrabold tracking-tight ${s.text} ${coachClass} ${textClassName}`}>
            UpCoach
          </span>
        )}
      </span>
    );
  }

  return (
    <span className={`inline-flex items-center gap-2 ${className}`}>
      <UpSymbol tone={resolvedTone} className={s.symbol} title="" />
      {showText && (
        <span className={`font-extrabold tracking-tight ${s.text} ${textClassName}`}>
          <span className={upClass}>Up</span>
          <span className={coachClass}>Coach</span>
        </span>
      )}
    </span>
  );
}
