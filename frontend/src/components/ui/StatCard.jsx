import { CountUp } from './CountUp';

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
    <div className="ds-card p-4 sm:p-6">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm text-slate-500 mb-1">{title}</p>
          <p className="text-2xl sm:text-3xl font-bold text-slate-900">
            {typeof value === 'number' && Number.isFinite(value) ? <CountUp value={value} /> : value}
          </p>
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

