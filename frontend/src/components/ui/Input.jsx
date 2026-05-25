export function Input({ label, error, hint, className = '', ...props }) {
  return (
    <div className="space-y-1.5">
      {label && <label className="block text-sm font-semibold text-slate-700 dark:text-slate-200">{label}</label>}
      <input
        className={`w-full px-3.5 py-2.5 border rounded-xl text-sm text-slate-800 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500 transition focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300 ${error ? 'border-red-300 bg-red-50/40' : 'border-slate-300 dark:border-white/10 bg-white dark:bg-slate-950 hover:border-slate-400'} ${className}`}
        {...props}
      />
      {hint && !error && <p className="text-xs text-slate-500 dark:text-slate-400">{hint}</p>}
      {error && <p className="text-xs font-medium text-red-600">{error}</p>}
    </div>
  );
}

