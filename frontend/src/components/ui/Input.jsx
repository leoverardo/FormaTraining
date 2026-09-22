export function Input({ label, error, hint, className = '', id, ...props }) {
  return (
    <div className="space-y-1.5">
      {label && <label htmlFor={id} className="ds-label-sm">{label}</label>}
      <input
        id={id}
        className={`ds-input ${error ? 'border-[var(--color-danger)]' : ''} ${className}`}
        {...props}
      />
      {hint && !error && <p className="ds-hint">{hint}</p>}
      {error && <p className="ds-error">{error}</p>}
    </div>
  );
}

