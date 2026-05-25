export function Button({ children, variant = 'primary', size = 'md', className = '', disabled, loading, ...props }) {
  const base = 'inline-flex items-center justify-center gap-2 rounded-xl font-semibold transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-white dark:focus:ring-offset-slate-950 disabled:cursor-not-allowed disabled:opacity-50 active:scale-[0.99]';
  const variants = {
    primary: 'bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-[0_10px_25px_rgba(79,70,229,0.28)] hover:from-indigo-700 hover:to-violet-700 focus:ring-indigo-400',
    secondary: 'bg-slate-900 text-white hover:bg-slate-800 focus:ring-slate-400',
    outline: 'border border-slate-300 bg-white text-slate-700 hover:border-slate-400 hover:bg-slate-50 focus:ring-indigo-300 dark:border-white/15 dark:bg-white/[0.03] dark:text-slate-200 dark:hover:bg-white/[0.08]',
    ghost: 'text-slate-600 hover:bg-slate-100 focus:ring-slate-300 dark:text-slate-300 dark:hover:bg-white/[0.08]',
    danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-400',
    success: 'bg-emerald-600 text-white hover:bg-emerald-700 focus:ring-emerald-400',
  };
  const sizes = {
    sm: 'px-3 py-2 text-sm',
    md: 'px-4 py-2.5 text-sm',
    lg: 'px-6 py-3 text-base',
  };
  return (
    <button className={`${base} ${variants[variant] || variants.primary} ${sizes[size] || sizes.md} ${className}`} disabled={disabled || loading} {...props}>
      {loading && <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>}
      {children}
    </button>
  );
}


