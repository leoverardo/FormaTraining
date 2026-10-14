const variants = {
  success: 'bg-[var(--status-success-bg)]/90 text-[var(--status-success-text)] border-[var(--status-success-bg)]',
  warning: 'bg-[var(--status-warning-bg)]/90 text-[var(--status-warning-text)] border-[var(--status-warning-bg)]',
  danger: 'bg-[var(--status-danger-bg)]/90 text-[var(--status-danger-text)] border-[var(--status-danger-bg)]',
  info: 'bg-[var(--status-info-bg)]/90 text-[var(--status-info-text)] border-[var(--status-info-bg)]',
  gray: 'bg-[var(--surface-subtle)] text-slate-600 border-slate-200',
  purple: 'bg-[var(--status-purple-bg)]/90 text-[var(--status-purple-text)] border-[var(--status-purple-bg)]',
};

export function Badge({ children, variant = 'gray' }) {
  return (
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border ${variants[variant] || variants.gray}`}>
      {children}
    </span>
  );
}

