import { Skeleton } from '@/components/ui/skeleton';

export default function DealerProfileLoading() {
  return (
    <div className="bg-muted/50 min-h-screen">
      {/* Cover Image skeleton */}
      <Skeleton className="h-48 w-full md:h-64 lg:h-80" />

      {/* Dealer Header */}
      <div className="relative z-10 container mx-auto -mt-16 px-4">
        <div className="bg-card rounded-xl p-6 shadow-lg">
          <div className="flex flex-col gap-6 md:flex-row">
            {/* Logo */}
            <Skeleton className="-mt-16 h-24 w-24 rounded-xl md:-mt-20 md:h-32 md:w-32" />

            {/* Info */}
            <div className="flex-1 space-y-3">
              <div className="flex items-center gap-2">
                <Skeleton className="h-8 w-48 md:w-64" />
                <Skeleton className="h-5 w-20 rounded-full" />
              </div>
              <Skeleton className="h-4 w-full max-w-md" />
              <div className="flex flex-wrap gap-4">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-28" />
                <Skeleton className="h-4 w-20" />
                <Skeleton className="h-4 w-32" />
              </div>
            </div>

            {/* Actions */}
            <div className="flex flex-col gap-2 md:min-w-[160px]">
              <Skeleton className="h-10 w-full rounded-md" />
              <Skeleton className="h-10 w-full rounded-md" />
              <Skeleton className="h-8 w-full rounded-md" />
            </div>
          </div>

          {/* Badges */}
          <div className="border-border mt-4 flex flex-wrap gap-2 border-t pt-4">
            <Skeleton className="h-6 w-28 rounded-full" />
            <Skeleton className="h-6 w-24 rounded-full" />
            <Skeleton className="h-6 w-32 rounded-full" />
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          {/* Left Column */}
          <div className="lg:col-span-2">
            {/* Tabs skeleton */}
            <div className="mb-6 flex gap-2">
              <Skeleton className="h-10 w-32 rounded-md" />
              <Skeleton className="h-10 w-28 rounded-md" />
              <Skeleton className="h-10 w-24 rounded-md" />
            </div>

            {/* Vehicle grid skeleton */}
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              {Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="overflow-hidden rounded-xl border">
                  <Skeleton className="h-48 w-full" />
                  <div className="space-y-2 p-4">
                    <Skeleton className="h-5 w-3/4" />
                    <Skeleton className="h-6 w-1/2" />
                    <div className="flex gap-2">
                      <Skeleton className="h-4 w-16" />
                      <Skeleton className="h-4 w-20" />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Right Sidebar */}
          <div className="space-y-6">
            {/* Contact card skeleton */}
            <div className="rounded-xl border p-6">
              <Skeleton className="mb-4 h-6 w-32" />
              <div className="space-y-3">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-3/4" />
                <Skeleton className="h-4 w-5/6" />
              </div>
              <Skeleton className="mt-4 h-10 w-full rounded-md" />
            </div>

            {/* Location card skeleton */}
            <div className="rounded-xl border p-6">
              <Skeleton className="mb-4 h-6 w-28" />
              <Skeleton className="h-40 w-full rounded-lg" />
              <Skeleton className="mt-3 h-4 w-full" />
            </div>

            {/* Schedule card skeleton */}
            <div className="rounded-xl border p-6">
              <Skeleton className="mb-4 h-6 w-24" />
              <div className="space-y-2">
                {Array.from({ length: 5 }).map((_, i) => (
                  <div key={i} className="flex justify-between">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-4 w-28" />
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
