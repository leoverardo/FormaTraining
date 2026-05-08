export function Tabs({ tabs, value, onChange, className = '' }) {
  return (
    <div className={`inline-flex flex-wrap gap-1 rounded-xl border border-slate-200 bg-white p-1 ${className}`}>
      {tabs.map((tab) => {
        const active = value === tab.value;
        return (
          <button
            key={tab.value}
            onClick={() => onChange(tab.value)}
            className={`px-3 py-1.5 text-sm rounded-lg transition ${active ? 'bg-indigo-600 text-white' : 'text-slate-600 hover:bg-slate-100'}`}
          >
            {tab.label}
          </button>
        );
      })}
    </div>
  );
}


