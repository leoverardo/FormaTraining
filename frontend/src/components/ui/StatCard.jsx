import { CountUp } from './CountUp';

export function StatCard({ title, value, subtitle, icon: Icon, color = 'indigo' }) {
  const colors = {
    indigo: 'bg-[var(--primary-soft)]/70 text-[var(--link)]',
    emerald: 'bg-[var(--status-success-bg)]/70 text-[var(--status-success-text)]',
    amber: 'bg-[var(--status-warning-bg)]/70 text-[var(--status-warning-text)]',
    red: 'bg-[var(--status-danger-bg)]/70 text-[var(--status-danger-text)]',
    purple: 'bg-[var(--status-purple-bg)]/70 text-[var(--status-purple-text)]',
    blue: 'bg-[var(--status-info-bg)]/70 text-[var(--status-info-text)]',
  };
  return (
    <div className="ds-card p-[var(--card-padding)]">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm text-slate-500 mb-1">{title}</p>
          <p className="text-2xl sm:text-3xl font-semibold text-slate-900">
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

