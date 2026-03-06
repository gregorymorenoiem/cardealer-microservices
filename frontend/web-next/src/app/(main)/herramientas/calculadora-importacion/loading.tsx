import { Skeleton } from '@/components/ui/skeleton';

export default function ImportCalculatorLoading() {
  return (
    <div className="min-h-screen">
      {/* Hero skeleton */}
      <div className="bg-gradient-to-br from-[#00A870] to-[#007850] py-12">
        <div className="container mx-auto px-4 text-center">
          <Skeleton className="mx-auto h-8 w-80 bg-white/20" />
          <Skeleton className="mx-auto mt-3 h-5 w-[30rem] max-w-full bg-white/20" />
        </div>
      </div>

      <div className="container mx-auto -mt-8 max-w-4xl px-4 pb-12">
        <div className="rounded-2xl border bg-white p-8 shadow-lg">
          {/* Input fields skeleton */}
          <div className="grid gap-6 md:grid-cols-2">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="space-y-2">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-10 w-full rounded-lg" />
              </div>
            ))}
          </div>

          {/* Result breakdown skeleton */}
          <div className="mt-8 space-y-4 rounded-xl bg-gray-50 p-6">
            <Skeleton className="h-6 w-56" />
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex justify-between border-b pb-3 last:border-0">
                <Skeleton className="h-4 w-40" />
                <Skeleton className="h-4 w-28" />
              </div>
            ))}
            <div className="flex justify-between pt-2">
              <Skeleton className="h-6 w-28" />
              <Skeleton className="h-6 w-36" />
            </div>
          </div>

          {/* CTA button skeleton */}
          <Skeleton className="mt-6 h-12 w-full rounded-lg" />
        </div>
      </div>
    </div>
  );
}
