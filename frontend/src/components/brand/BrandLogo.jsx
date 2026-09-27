import { useId } from 'react';

const U_PATH = 'M20 15 v19 c0 9 5.5 14 12 14 s12 -5 12 -14 V15';
const ARROW_PATH = 'M44 5 L51.5 16 H36.5 Z';

const SIZES = {
  xs: { symbol: 'h-7 w-7', text: 'text-base' },
  sm: { symbol: 'h-8 w-8', text: 'text-lg' },
  md: { symbol: 'h-10 w-10', text: 'text-xl' },
  lg: { symbol: 'h-12 w-12', text: 'text-2xl' },
};

export function UpSymbol({ className = '', mono = false, title = 'UpCoach' }) {
  const id = useId().replace(/[^a-zA-Z0-9]/g, '');
  const gradId = `uc-grad-${id}`;
  const stroke = mono ? 'currentColor' : `url(#${gradId})`;
  return (
    <svg viewBox="0 0 64 64" className={className} role="img" aria-label={title} aria-hidden={title ? undefined : true}>
      {!mono && (
        <defs>
          <linearGradient id={gradId} x1="0" y1="0" x2="1" y2="1">
            <stop offset="0" stopColor="#6366F1" />
            <stop offset="0.55" stopColor="#4F46E5" />
            <stop offset="1" stopColor="#06B6D4" />
          </linearGradient>
        </defs>
      )}
      <path d={U_PATH} fill="none" stroke={stroke} strokeWidth="8.5" strokeLinecap="round" />
      <path d={ARROW_PATH} fill={mono ? 'currentColor' : '#06B6D4'} />
    </svg>
  );
}

export function BrandLogo({
  size = 'md',
  tone = 'light',
  variant = 'horizontal',
  showText = true,
  className = '',
  textClassName = '',
}) {
  const s = SIZES[size] || SIZES.md;
  const dark = tone === 'dark';
  const upClass = dark ? 'text-indigo-300' : 'text-indigo-600';
  const coachClass = dark ? 'text-white' : 'text-slate-900';

  if (variant === 'symbol' || !showText) {
    return (
      <span className={`inline-flex items-center ${className}`}>
        <UpSymbol className={s.symbol} />
      </span>
    );
  }

  if (variant === 'mono') {
    return (
      <span className={`inline-flex items-center gap-2 ${className}`}>
        <UpSymbol mono className={`${s.symbol} ${coachClass}`} title="" />
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
      <UpSymbol className={s.symbol} />
      {showText && (
        <span className={`font-extrabold tracking-tight ${s.text} ${textClassName}`}>
          <span className={upClass}>Up</span>
          <span className={coachClass}>Coach</span>
        </span>
      )}
    </span>
  );
}
