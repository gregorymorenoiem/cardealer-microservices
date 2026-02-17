import { Skeleton } from '@/components/ui/skeleton';

export default function VenderLoading() {
  return (
    <div className="bg-background min-h-screen">
      {/* Hero skeleton */}
      <div className="bg-[#00A870] py-16 lg:py-24">
        <div className="mx-auto max-w-3xl px-4 text-center">
          <Skeleton className="mx-auto mb-6 h-8 w-64 rounded-full bg-white/20" />
          <Skeleton className="mx-auto mb-4 h-12 w-3/4 bg-white/20" />
          <Skeleton className="mx-auto mb-8 h-6 w-2/3 bg-white/20" />
          <Skeleton className="mx-auto h-12 w-48 rounded-lg bg-white/20" />
        </div>
      </div>
      {/* Stats skeleton */}
      <div className="border-b py-12">
        <div className="mx-auto grid max-w-5xl grid-cols-2 gap-8 px-4 md:grid-cols-4">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="text-center">
              <Skeleton className="mx-auto h-10 w-24" />
              <Skeleton className="mx-auto mt-2 h-4 w-32" />
            </div>
          ))}
        </div>
      </div>
      {/* Steps skeleton */}
      <div className="py-16">
        <div className="mx-auto max-w-5xl px-4">
          <Skeleton className="mx-auto mb-12 h-8 w-48" />
          <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-4">
            {[1, 2, 3, 4].map(i => (
              <Skeleton key={i} className="h-48 w-full rounded-xl" />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
