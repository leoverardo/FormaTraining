export function EmptyState({ icon: Icon, title, description, action }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 px-4 text-center rounded-[var(--radius-card)] border border-dashed border-[var(--border)] bg-[var(--surface)]/75">
      {Icon && <div className="p-4 bg-[var(--surface-muted)] rounded-[var(--radius-md)] mb-4"><Icon size={30} className="text-[var(--text-muted)]" /></div>}
      <h3 className="text-lg font-semibold text-slate-800 mb-1">{title}</h3>
      {description && <p className="text-sm text-slate-500 mb-5 max-w-sm">{description}</p>}
      {action}
    </div>
  );
}

