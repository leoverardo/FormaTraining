export function PreviewCard({ title, subtitle, children }) {
  return (
    <aside className="ds-card p-5">
      <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
      {subtitle ? <p className="mt-1 text-xs text-slate-500">{subtitle}</p> : null}
      <div className="mt-4">{children}</div>
    </aside>
  );
}
