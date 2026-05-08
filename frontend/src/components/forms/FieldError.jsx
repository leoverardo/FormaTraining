export function FieldError({ children }) {
  if (!children) return null;
  return <p className="text-xs font-medium text-red-600">{children}</p>;
}
