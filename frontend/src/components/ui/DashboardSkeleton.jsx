import { Skeleton } from './Skeleton';

export function DashboardSkeleton({ stats = 4 }) {
  return (
    <div className="space-y-5" aria-busy="true" aria-label="Carregando painel">
      <Skeleton className="h-36 rounded-3xl" />
      <div className="grid grid-cols-2 gap-4 xl:grid-cols-4">
        {Array.from({ length: stats }).map((_, i) => (
          <Skeleton key={i} className="h-28 rounded-2xl" />
        ))}
      </div>
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
        <Skeleton className="h-64 lg:col-span-8" />
        <Skeleton className="h-64 lg:col-span-4" />
      </div>
    </div>
  );
}
