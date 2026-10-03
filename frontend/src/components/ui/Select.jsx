import { useId } from 'react';

export function Select({ label, error, hint, children, className = '', id, ...props }) {
  const generatedId = useId();
  const selectId = id || generatedId;
  return (
    <div className="space-y-1.5">
      {label && <label htmlFor={selectId} className="ds-label-sm">{label}</label>}
      <select
        id={selectId}
        aria-invalid={error ? true : undefined}
        className={`ds-select ${error ? 'border-red-300 bg-red-50/40' : ''} ${className}`}
        {...props}
      >
        {children}
      </select>
      {hint && !error && <p className="text-xs text-slate-500 dark:text-slate-400">{hint}</p>}
      {error && <p className="text-xs font-medium text-red-600">{error}</p>}
    </div>
  );
}

