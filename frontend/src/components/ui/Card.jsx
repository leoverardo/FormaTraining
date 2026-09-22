export function Card({ children, className = '' }) {
  return (
    <div className={`ds-card ${className}`}>
      {children}
    </div>
  );
}

export function CardHeader({ children, className = '' }) {
  return <div className={`border-b border-[var(--color-border-subtle)] px-6 py-5 ${className}`}>{children}</div>;
}

export function CardContent({ children, className = '' }) {
  return <div className={`px-6 py-5 ${className}`}>{children}</div>;
}


