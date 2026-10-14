export function Tabs({ tabs, value, onChange, className = '' }) {
  return (
    <div className={`inline-flex flex-wrap gap-1 rounded-full bg-[var(--surface-muted)] p-1 ${className}`}>
      {tabs.map((tab) => {
        const active = value === tab.value;
        return (
          <button
            key={tab.value}
            onClick={() => onChange(tab.value)}
            aria-pressed={active}
            className={`min-h-[44px] px-4 py-2 text-sm rounded-full transition-colors ${active ? 'bg-[var(--primary-strong)] text-white' : 'text-[var(--text-secondary)] hover:bg-[var(--surface-subtle)]'}`}
          >
            {tab.label}
          </button>
        );
      })}
    </div>
  );
}


