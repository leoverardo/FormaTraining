export function FormPage({ children, className = '' }) {
  return <div className={`mx-auto w-full max-w-6xl space-y-6 ${className}`}>{children}</div>;
}
