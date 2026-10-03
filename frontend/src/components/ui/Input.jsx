import { useId } from 'react';

export function Input({ label, error, hint, className = '', id, ...props }) {
  const generatedId = useId();
  const inputId = id || generatedId;
  return (
    <div className="space-y-1.5">
      {label && <label htmlFor={inputId} className="ds-label-sm">{label}</label>}
      <input
        id={inputId}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? `${inputId}-description` : undefined}
        className={`ds-input ${error ? 'border-[var(--color-danger)]' : ''} ${className}`}
        {...props}
      />
      {hint && !error && <p id={`${inputId}-description`} className="ds-hint">{hint}</p>}
      {error && <p id={`${inputId}-description`} className="ds-error">{error}</p>}
    </div>
  );
}

