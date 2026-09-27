export function Skeleton({ className = '', shimmer = true }) {
  return <div className={`${shimmer ? 'uc-shimmer' : 'animate-pulse bg-slate-200/80'} rounded-xl ${className}`} />;
}


