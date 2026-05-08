export function ProgressBar({ value = 0, className = '', tone = 'indigo' }) {
  const width = Math.max(0, Math.min(100, value));
  const toneClass = tone === 'success' ? 'bg-emerald-500' : tone === 'warning' ? 'bg-amber-500' : 'bg-indigo-500';
  return (
    <div className={`h-2.5 rounded-full bg-slate-100 overflow-hidden ${className}`}>
      <div className={`h-full rounded-full transition-all duration-500 ${toneClass}`} style={{ width: `${width}%` }} />
    </div>
  );
}


