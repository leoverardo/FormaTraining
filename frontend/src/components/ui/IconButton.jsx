export function IconButton({ icon: Icon, label, className = '', ...props }) {
  return (
    <button
      aria-label={label}
      className={`inline-flex h-9 w-9 items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-500 transition hover:bg-slate-50 hover:text-slate-700 ${className}`}
      {...props}
    >
      {Icon ? <Icon size={16} /> : null}
    </button>
  );
}


