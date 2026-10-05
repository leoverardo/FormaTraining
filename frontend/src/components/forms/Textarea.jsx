export function Textarea({ className = '', ...props }) {
  return <textarea className={`ds-control ds-control--textarea text-sm placeholder:text-[var(--text-muted)] ${className}`} {...props} />;
}
