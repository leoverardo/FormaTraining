export function PageHeader({ title, description, actions, className = '' }) {
  return (
    <div className={`flex flex-wrap items-start justify-between gap-4 mb-6 ${className}`}>
      <div>
        <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-900">{title}</h1>
        {description && <p className="text-sm text-slate-500 mt-1">{description}</p>}
      </div>
      {actions && <div className="flex items-center gap-2 flex-wrap">{actions}</div>}
    </div>
  );
}


