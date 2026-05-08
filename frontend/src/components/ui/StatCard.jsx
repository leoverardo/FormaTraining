export function StatCard({ title, value, subtitle, icon: Icon, color = 'indigo' }) {
  const colors = {
    indigo: 'bg-indigo-100/70 text-indigo-600',
    emerald: 'bg-emerald-100/70 text-emerald-600',
    amber: 'bg-amber-100/70 text-amber-600',
    red: 'bg-red-100/70 text-red-600',
    purple: 'bg-violet-100/70 text-violet-600',
    blue: 'bg-cyan-100/70 text-cyan-600',
  };
  return (
    <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-[0_10px_24px_rgba(15,23,42,0.06)]">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm text-slate-500 mb-1">{title}</p>
          <p className="text-3xl font-bold text-slate-900">{value}</p>
          {subtitle && <p className="text-xs text-slate-400 mt-1">{subtitle}</p>}
        </div>
        {Icon && (
          <div className={`p-3 rounded-xl ${colors[color]}`}>
            <Icon size={22} />
          </div>
        )}
      </div>
    </div>
  );
}

