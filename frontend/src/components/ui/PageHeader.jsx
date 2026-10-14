export function PageHeader({ title, description, actions, className = '' }) {
  return (
    <div className={`uc-page-header flex flex-wrap items-start justify-between gap-5 mb-8 ${className}`}>
      <div className="min-w-0">
        <h1 className="uc-heading text-[var(--text)] uc-heading">{title}</h1>
        {description && <p className="uc-lead mt-3">{description}</p>}
      </div>
      {actions && <div className="flex items-center gap-2 flex-wrap">{actions}</div>}
    </div>
  );
}


