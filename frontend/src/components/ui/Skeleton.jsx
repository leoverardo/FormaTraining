export function Skeleton({ className = '', shimmer = true }) {
  return <div className={`${shimmer ? 'uc-shimmer' : 'animate-pulse bg-[var(--surface-control)]/80'} rounded-xl ${className}`} />;
}


