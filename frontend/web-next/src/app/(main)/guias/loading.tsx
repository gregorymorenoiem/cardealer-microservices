import { Skeleton } from '@/components/ui/skeleton';

export default function GuiasLoading() {
  return (
    <div className="min-h-screen">
      {/* Hero skeleton */}
      <div className="bg-gradient-to-br from-primary/10 to-primary/10 py-14">
        <div className="container mx-auto px-4 text-center">
          <Skeleton className="mx-auto mb-4 h-14 w-14 rounded-full" />
          <Skeleton className="mx-auto h-10 w-64" />
          <Skeleton className="mx-auto mt-3 h-5 w-96" />
        </div>
      </div>

      {/* Guides grid skeleton */}
      <div className="container mx-auto px-4 py-14">
        <Skeleton className="mb-8 h-8 w-48" />
        <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="border-border overflow-hidden rounded-xl border p-6">
              <Skeleton className="mb-3 h-11 w-11 rounded-full" />
              <Skeleton className="h-5 w-3/4" />
              <Skeleton className="mt-2 h-4 w-full" />
              <Skeleton className="mt-1 h-4 w-2/3" />
              <div className="mt-3 space-y-1">
                <Skeleton className="h-3 w-1/2" />
                <Skeleton className="h-3 w-2/3" />
                <Skeleton className="h-3 w-1/3" />
              </div>
              <Skeleton className="mt-4 h-9 w-full rounded-md" />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
