import { Skeleton } from '@/components/ui/skeleton';

export default function FinancingCalculatorLoading() {
  return (
    <div className="min-h-screen">
      {/* Hero skeleton */}
      <div className="bg-gradient-to-br from-[#00A870] to-[#007850] py-12">
        <div className="container mx-auto px-4 text-center">
          <Skeleton className="mx-auto h-8 w-72 bg-white/20" />
          <Skeleton className="mx-auto mt-3 h-5 w-[28rem] max-w-full bg-white/20" />
        </div>
      </div>

      <div className="container mx-auto -mt-8 max-w-4xl px-4 pb-12">
        <div className="rounded-2xl border bg-white p-8 shadow-lg">
          {/* Input fields skeleton */}
          <div className="grid gap-6 md:grid-cols-2">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="space-y-2">
                <Skeleton className="h-4 w-28" />
                <Skeleton className="h-10 w-full rounded-lg" />
              </div>
            ))}
          </div>

          {/* Slider skeleton */}
          <div className="mt-6 space-y-2">
            <Skeleton className="h-4 w-36" />
            <Skeleton className="h-2 w-full rounded-full" />
            <div className="flex justify-between">
              <Skeleton className="h-3 w-16" />
              <Skeleton className="h-3 w-16" />
            </div>
          </div>

          {/* Result card skeleton */}
          <div className="mt-8 rounded-xl bg-gray-50 p-6">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="mt-4 h-10 w-40" />
            <div className="mt-4 grid grid-cols-3 gap-4">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="space-y-1">
                  <Skeleton className="h-3 w-20" />
                  <Skeleton className="h-5 w-28" />
                </div>
              ))}
            </div>
          </div>

          {/* CTA button skeleton */}
          <Skeleton className="mt-6 h-12 w-full rounded-lg" />
        </div>
      </div>
    </div>
  );
}
