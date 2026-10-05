export function Tabs({ tabs, value, onChange, className = '' }) {
  return (
    <div className={`inline-flex flex-wrap gap-1 rounded-xl border border-slate-200 bg-white p-1 ${className}`}>
      {tabs.map((tab) => {
        const active = value === tab.value;
        return (
          <button
            key={tab.value}
            onClick={() => onChange(tab.value)}
            className={`px-3 py-1.5 text-sm rounded-full transition-colors ${active ? 'bg-[var(--primary)] text-white' : 'text-[var(--text-secondary)] hover:bg-[var(--surface-muted)]'}`}
          >
            {tab.label}
          </button>
        );
      })}
    </div>
  );
}


