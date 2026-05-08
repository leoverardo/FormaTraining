export function Select({ label, error, hint, children, className = '', ...props }) {
  return (
    <div className="space-y-1.5">
      {label && <label className="block text-sm font-semibold text-slate-700">{label}</label>}
      <select
        className={`w-full px-3.5 py-2.5 border rounded-xl text-sm text-slate-800 transition focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300 ${error ? 'border-red-300 bg-red-50/40' : 'border-slate-300 bg-white hover:border-slate-400'} ${className}`}
        {...props}
      >
        {children}
      </select>
      {hint && !error && <p className="text-xs text-slate-500">{hint}</p>}
      {error && <p className="text-xs font-medium text-red-600">{error}</p>}
    </div>
  );
}

