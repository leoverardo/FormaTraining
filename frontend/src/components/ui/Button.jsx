export function Button({ children, variant = 'primary', size = 'md', className = '', disabled, loading, ...props }) {
  const base = 'inline-flex items-center justify-center gap-2 font-normal rounded-full transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed active:scale-[0.99]';
  const variants = {
    primary: 'bg-[var(--primary-strong)] text-[var(--on-primary)] hover:bg-[var(--primary-hover)]',
    secondary: 'bg-[var(--surface-subtle)] text-[var(--text)] hover:bg-[var(--surface-control)]',
    outline: 'bg-[var(--surface)] text-[var(--text-secondary)] border border-[var(--border)] hover:border-[var(--border-strong)] hover:bg-[var(--surface-muted)] focus:ring-[var(--focus-ring)]',
    ghost: 'text-[var(--text-secondary)] hover:bg-[var(--surface-muted)] focus:ring-[var(--focus-ring)]',
    danger: 'bg-[var(--danger-fill)] text-white hover:bg-[var(--danger-fill)] focus:ring-red-400',
    success: 'bg-[var(--success-fill)] text-white hover:bg-[var(--success-fill)] focus:ring-emerald-400',
  };
  const sizes = {
    sm: 'px-4 py-2 text-xs min-h-[44px]',
    md: 'px-6 py-2.5 text-sm min-h-[44px]',
    lg: 'px-7 py-3 text-base min-h-[48px]',
  };
  return (
    <button className={`${base} ${variants[variant] || variants.primary} ${sizes[size] || sizes.md} ${className}`} disabled={disabled || loading} {...props}>
      {loading && <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>}
      {children}
    </button>
  );
}

