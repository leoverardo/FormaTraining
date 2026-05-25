export function Card({ children, className = '' }) {
  return (
    <div className={`rounded-2xl border border-slate-200 bg-white shadow-[0_8px_28px_rgba(15,23,42,0.06)] dark:border-white/10 dark:bg-slate-900 dark:shadow-[0_14px_30px_rgba(2,6,23,0.45)] ${className}`}>
      {children}
    </div>
  );
}

export function CardHeader({ children, className = '' }) {
  return <div className={`border-b border-slate-100 px-6 py-5 dark:border-white/10 ${className}`}>{children}</div>;
}

export function CardContent({ children, className = '' }) {
  return <div className={`px-6 py-5 ${className}`}>{children}</div>;
}


