import { Skeleton } from '@/components/ui/skeleton';

export default function CompararLoading() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      {/* Header */}
      <Skeleton className="mx-auto mb-2 h-8 w-64" />
      <Skeleton className="mx-auto mb-8 h-4 w-80" />
      {/* Vehicle columns */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {[1, 2, 3].map(i => (
          <div key={i} className="space-y-4 rounded-xl border p-4">
            <Skeleton className="aspect-[16/10] w-full rounded-lg" />
            <Skeleton className="h-6 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
            <div className="space-y-2 pt-4">
              {[1, 2, 3, 4, 5].map(j => (
                <div key={j} className="flex justify-between">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-24" />
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
