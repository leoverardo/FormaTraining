export function PageHeader({ title, description, actions, className = '' }) {
  return (
    <div className={`flex flex-wrap items-start justify-between gap-4 mb-6 ${className}`}>
      <div className="ds-max-prose">
        <h1 className="ds-h1 text-[var(--color-text-primary)]">{title}</h1>
        {description && <p className="ds-body-sm text-[var(--color-text-secondary)] mt-2">{description}</p>}
      </div>
      {actions && <div className="flex items-center gap-2 flex-wrap">{actions}</div>}
    </div>
  );
}


