export function PreviewCard({ title, subtitle, children }) {
  return (
    <aside className="rounded-2xl border border-slate-200 bg-white p-5 shadow-[0_8px_22px_rgba(15,23,42,0.06)]">
      <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
      {subtitle ? <p className="mt-1 text-xs text-slate-500">{subtitle}</p> : null}
      <div className="mt-4">{children}</div>
    </aside>
  );
}
