export function Avatar({ src, alt = '', name = '', className = '' }) {
  if (src) {
    return <img src={src} alt={alt || name} className={`rounded-full object-cover bg-slate-100 ${className}`} />;
  }
  const initials = name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((s) => s[0]?.toUpperCase())
    .join('') || 'FP';
  return <div className={`rounded-full bg-indigo-100 text-indigo-700 font-semibold flex items-center justify-center ${className}`}>{initials}</div>;
}


