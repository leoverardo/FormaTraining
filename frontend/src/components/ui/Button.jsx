export function Button({ children, variant = 'primary', size = 'md', className = '', disabled, loading, ...props }) {
  const base = 'ds-btn ds-focus-visible disabled:cursor-not-allowed disabled:opacity-50';
  const variants = {
    primary: 'ds-btn--primary',
    secondary: 'ds-btn--secondary',
    outline: 'ds-btn--outline',
    ghost: 'ds-btn--ghost',
    danger: 'ds-btn--primary bg-[var(--color-danger)] shadow-none',
    success: 'ds-btn--primary bg-[var(--color-success)] shadow-none',
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


