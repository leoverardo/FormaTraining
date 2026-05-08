export function SocialStats({ items = [] }) {
  return (
    <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
      {items.map((item) => (
        <div key={item.label} className="rounded-xl bg-white/90 border border-white/60 p-3 text-center">
          <p className="text-lg font-bold text-slate-900">{item.value}</p>
          <p className="text-xs text-slate-500">{item.label}</p>
        </div>
      ))}
    </div>
  );
}


