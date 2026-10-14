export function FieldError({ children }) {
  if (!children) return null;
  return <p className="text-xs font-medium text-[var(--status-danger-text)]">{children}</p>;
}
