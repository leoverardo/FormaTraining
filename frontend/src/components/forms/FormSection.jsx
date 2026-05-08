export function FormSection({ icon: Icon, title, description, children, className = '' }) {
  return (
    <section className={`rounded-2xl border border-slate-200 bg-white p-5 shadow-[0_8px_22px_rgba(15,23,42,0.06)] sm:p-6 ${className}`}>
      <div className="mb-4">
        <h3 className="flex items-center gap-2 text-base font-semibold text-slate-900">
          {Icon ? <Icon size={16} className="text-cyan-700" /> : null}
          {title}
        </h3>
        {description ? <p className="mt-1 text-sm text-slate-500">{description}</p> : null}
      </div>
      {children}
    </section>
  );
}
