import { useId } from 'react';

export function Select({ label, error, hint, children, className = '', ...props }) {
  const generatedId = useId();
  const id = props.id || generatedId;
  return (
    <div className="space-y-1.5">
      {label && <label htmlFor={id} className="block text-sm font-semibold text-[var(--text)]">{label}</label>}
      <select
        id={id}
        aria-invalid={Boolean(error)}
        className={`ds-control text-sm ${className}`}
        {...props}
      >
        {children}
      </select>
      {hint && !error && <p className="text-xs text-slate-500">{hint}</p>}
      {error && <p className="text-xs font-medium text-red-600">{error}</p>}
    </div>
  );
}

