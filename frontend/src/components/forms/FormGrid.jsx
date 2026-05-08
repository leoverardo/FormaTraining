export function FormGrid({ children, cols = '2', className = '' }) {
  const map = {
    '1': 'grid-cols-1',
    '2': 'grid-cols-1 md:grid-cols-2',
    '3': 'grid-cols-1 md:grid-cols-2 xl:grid-cols-3',
  };
  return <div className={`grid gap-4 ${map[cols] || map['2']} ${className}`}>{children}</div>;
}
