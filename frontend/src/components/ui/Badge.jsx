const variants = {
  success: 'bg-emerald-100/90 text-emerald-700 border-emerald-200',
  warning: 'bg-amber-100/90 text-amber-700 border-amber-200',
  danger: 'bg-red-100/90 text-red-700 border-red-200',
  info: 'bg-indigo-100/90 text-indigo-700 border-indigo-200',
  gray: 'bg-slate-100 text-slate-600 border-slate-200',
  purple: 'bg-indigo-100/90 text-indigo-700 border-indigo-200',
};

export function Badge({ children, variant = 'gray' }) {
  return (
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border ${variants[variant] || variants.gray}`}>
      {children}
    </span>
  );
}

