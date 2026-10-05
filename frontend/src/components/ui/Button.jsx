export function Button({ children, variant = 'primary', size = 'md', className = '', disabled, loading, ...props }) {
  const base = 'inline-flex items-center justify-center gap-2 font-semibold rounded-full transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-white disabled:opacity-50 disabled:cursor-not-allowed active:scale-[0.99]';
  const variants = {
    primary: 'bg-[var(--primary)] text-white hover:bg-[var(--primary-strong)] focus:ring-[var(--focus-ring)]',
    secondary: 'bg-slate-900 text-white hover:bg-slate-800 focus:ring-slate-400',
    outline: 'bg-[var(--surface)] text-[var(--text-secondary)] border border-[var(--border)] hover:border-[var(--border-strong)] hover:bg-[var(--surface-muted)] focus:ring-[var(--focus-ring)]',
    ghost: 'text-[var(--text-secondary)] hover:bg-[var(--surface-muted)] focus:ring-[var(--focus-ring)]',
    danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-400',
    success: 'bg-emerald-600 text-white hover:bg-emerald-700 focus:ring-emerald-400',
  };
  const sizes = {
    sm: 'px-3 py-2 text-xs min-h-[36px]',
    md: 'px-5 py-2.5 text-sm min-h-[44px]',
    lg: 'px-6 py-3 text-base min-h-[48px]',
  };
  return (
    <button className={`${base} ${variants[variant] || variants.primary} ${sizes[size] || sizes.md} ${className}`} disabled={disabled || loading} {...props}>
      {loading && <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>}
      {children}
    </button>
  );
}

