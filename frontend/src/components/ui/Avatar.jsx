export function Avatar({ src, alt = '', name = '', className = '' }) {
  if (src) {
    return <img src={src} alt={alt || name} className={`rounded-full object-cover bg-[var(--surface-subtle)] ${className}`} />;
  }
  const initials = name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((s) => s[0]?.toUpperCase())
    .join('') || 'FP';
  return <div className={`rounded-full bg-[var(--primary-soft)] text-[var(--link)] font-semibold flex items-center justify-center ${className}`}>{initials}</div>;
}


