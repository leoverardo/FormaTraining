export function Card({ children, className = '' }) {
  return (
    <div className={`bg-white rounded-2xl border border-slate-200 shadow-[0_8px_28px_rgba(15,23,42,0.06)] ${className}`}>
      {children}
    </div>
  );
}

export function CardHeader({ children, className = '' }) {
  return <div className={`px-6 py-5 border-b border-slate-100 ${className}`}>{children}</div>;
}

export function CardContent({ children, className = '' }) {
  return <div className={`px-6 py-5 ${className}`}>{children}</div>;
}

