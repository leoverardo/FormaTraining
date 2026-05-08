export function ContentGrid({ children, className = '' }) {
  return <div className={`grid grid-cols-1 lg:grid-cols-12 gap-4 sm:gap-5 ${className}`}>{children}</div>;
}


