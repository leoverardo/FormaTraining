export function HelperText({ children }) {
  if (!children) return null;
  return <p className="text-xs text-slate-500">{children}</p>;
}
