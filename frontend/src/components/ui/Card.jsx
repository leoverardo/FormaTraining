export function Card({ children, className = '' }) {
  return (
    <div className={`ds-card ${className}`}>
      {children}
    </div>
  );
}

export function CardHeader({ children, className = '' }) {
  return <div className={`p-[var(--card-padding)] border-b border-[var(--border)] ${className}`}>{children}</div>;
}

export function CardContent({ children, className = '' }) {
  return <div className={`p-[var(--card-padding)] ${className}`}>{children}</div>;
}

