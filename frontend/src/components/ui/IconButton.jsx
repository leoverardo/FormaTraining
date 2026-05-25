export function IconButton({ icon: Icon, label, className = '', ...props }) {
  return (
    <button
      aria-label={label}
      className={`inline-flex h-9 w-9 items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-500 transition hover:bg-slate-50 hover:text-slate-700 dark:border-white/10 dark:bg-slate-900 dark:text-slate-300 dark:hover:bg-white/10 dark:hover:text-white ${className}`}
      {...props}
    >
      {Icon ? <Icon size={16} /> : null}
    </button>
  );
}



