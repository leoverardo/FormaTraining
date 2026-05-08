export function Checkbox({ checked, onChange, label }) {
  return (
    <label className="inline-flex items-center gap-2 text-sm text-slate-700">
      <input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} className="h-4 w-4 rounded border-slate-300 text-cyan-600 focus:ring-cyan-400" />
      {label}
    </label>
  );
}
