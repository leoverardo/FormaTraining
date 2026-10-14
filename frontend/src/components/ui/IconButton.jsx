export function IconButton({ icon: Icon, label, className = '', ...props }) {
  return (
    <button
      aria-label={label}
      className={`uc-icon-button border border-[var(--border)] bg-[var(--surface)] transition-colors ${className}`}
      {...props}
    >
      {Icon ? <Icon size={16} /> : null}
    </button>
  );
}


