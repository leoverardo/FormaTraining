export function Switch({ checked, onChange, label }) {
  return (
    <label className="inline-flex cursor-pointer items-center gap-2">
      <button
        type="button"
        onClick={() => onChange(!checked)}
        className={`relative h-6 w-11 rounded-full transition ${checked ? 'bg-emerald-500' : 'bg-slate-300'}`}
        aria-pressed={checked}
      >
        <span className={`absolute top-0.5 h-5 w-5 rounded-full bg-white transition ${checked ? 'left-5' : 'left-0.5'}`} />
      </button>
      {label ? <span className="text-sm text-slate-700">{label}</span> : null}
    </label>
  );
}
