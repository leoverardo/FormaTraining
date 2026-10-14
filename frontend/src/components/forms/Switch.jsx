export function Switch({ checked, onChange, label }) {
  return (
    <label className="inline-flex cursor-pointer items-center gap-2">
      <button
        type="button"
        onClick={() => onChange(!checked)}
        className="inline-flex min-h-[44px] w-11 items-center"
        role="switch"
        aria-checked={checked}
        aria-label={label}
      >
        <span className={`relative block h-6 w-11 rounded-full transition-colors ${checked ? 'bg-[var(--primary-strong)]' : 'bg-[var(--control-border)]'}`}>
          <span className={`absolute top-0.5 h-5 w-5 rounded-full bg-white transition-transform ${checked ? 'translate-x-5 left-0.5' : 'left-0.5'}`} />
        </span>
      </button>
      {label ? <span className="text-sm text-slate-700">{label}</span> : null}
    </label>
  );
}
