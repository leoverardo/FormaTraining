export function StatCard({ title, value, subtitle, icon: Icon, color = 'indigo' }) {
  const colors = {
    indigo: 'bg-indigo-100/70 text-indigo-600',
    emerald: 'bg-emerald-100/70 text-emerald-600',
    amber: 'bg-amber-100/70 text-amber-600',
    red: 'bg-red-100/70 text-red-600',
    purple: 'bg-indigo-100/70 text-indigo-600',
    blue: 'bg-indigo-100/70 text-indigo-600',
  };
  return (
    <div className="bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-white/10 p-6 shadow-none shadow-none">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm text-slate-500 dark:text-slate-400 mb-1">{title}</p>
          <p className="text-3xl font-bold text-slate-900 dark:text-white">{value}</p>
          {subtitle && <p className="text-xs text-slate-400 dark:text-slate-500 mt-1">{subtitle}</p>}
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

