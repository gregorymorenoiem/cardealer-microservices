import { Skeleton } from '@/components/ui/skeleton';

export default function GuiaDetailLoading() {
  return (
    <div className="min-h-screen">
      {/* Breadcrumb skeleton */}
      <div className="border-border bg-muted/30 border-b py-3">
        <div className="container mx-auto px-4">
          <Skeleton className="h-4 w-64" />
        </div>
      </div>

      {/* Hero skeleton */}
      <div className="bg-gradient-to-br from-primary/10 to-primary/10 py-12">
        <div className="container mx-auto px-4">
          <div className="mx-auto max-w-3xl">
            <div className="mb-4 flex items-center gap-3">
              <Skeleton className="h-10 w-10 rounded-full" />
              <Skeleton className="h-4 w-32" />
            </div>
            <Skeleton className="h-10 w-3/4" />
            <Skeleton className="mt-3 h-5 w-full" />
            <div className="mt-4 flex gap-2">
              <Skeleton className="h-6 w-24 rounded-full" />
              <Skeleton className="h-6 w-28 rounded-full" />
              <Skeleton className="h-6 w-20 rounded-full" />
            </div>
          </div>
        </div>
      </div>

      {/* Content skeleton */}
      <div className="container mx-auto px-4 py-12">
        <div className="mx-auto grid max-w-6xl gap-10 lg:grid-cols-[1fr_300px]">
          {/* Article skeleton */}
          <div className="space-y-4">
            <Skeleton className="h-8 w-2/3" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-5/6" />
            <Skeleton className="mt-6 h-8 w-1/2" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-4/5" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="mt-6 h-8 w-2/3" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
          </div>

          {/* Sidebar skeleton */}
          <aside className="space-y-6">
            <div className="rounded-xl border p-6">
              <Skeleton className="mx-auto h-5 w-32" />
              <Skeleton className="mx-auto mt-2 h-4 w-48" />
              <Skeleton className="mt-4 h-10 w-full rounded-md" />
            </div>
            <div>
              <Skeleton className="mb-3 h-5 w-28" />
              <div className="space-y-3">
                <Skeleton className="h-20 w-full rounded-lg" />
                <Skeleton className="h-20 w-full rounded-lg" />
                <Skeleton className="h-20 w-full rounded-lg" />
              </div>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
}
