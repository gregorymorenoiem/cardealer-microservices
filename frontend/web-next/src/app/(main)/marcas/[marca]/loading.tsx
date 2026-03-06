import { Skeleton } from '@/components/ui/skeleton';

export default function BrandPageLoading() {
  return (
    <div className="bg-background min-h-screen">
      {/* Hero Section skeleton */}
      <section className="bg-gradient-to-br from-gray-900 to-gray-800 py-16">
        <div className="container mx-auto px-4">
          {/* Breadcrumb */}
          <Skeleton className="mb-6 h-4 w-48 bg-white/10" />

          {/* Title */}
          <Skeleton className="h-10 w-80 bg-white/10 md:h-14 md:w-[420px]" />

          {/* Description */}
          <Skeleton className="mt-4 h-5 w-full max-w-2xl bg-white/10" />
          <Skeleton className="mt-2 h-5 w-2/3 max-w-xl bg-white/10" />
        </div>
      </section>

      {/* Main Content skeleton */}
      <section className="container mx-auto px-4 py-10">
        {/* Filter bar */}
        <div className="mb-6 flex flex-wrap items-center gap-3">
          <Skeleton className="h-10 w-32 rounded-md" />
          <Skeleton className="h-10 w-28 rounded-md" />
          <Skeleton className="h-10 w-36 rounded-md" />
        </div>

        {/* Vehicle grid */}
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="overflow-hidden rounded-xl border">
              <Skeleton className="h-48 w-full" />
              <div className="space-y-2 p-4">
                <Skeleton className="h-5 w-3/4" />
                <Skeleton className="h-6 w-1/2" />
                <div className="flex gap-2">
                  <Skeleton className="h-4 w-16 rounded-full" />
                  <Skeleton className="h-4 w-20 rounded-full" />
                  <Skeleton className="h-4 w-14 rounded-full" />
                </div>
                <div className="flex items-center justify-between pt-2">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-8 w-20 rounded-md" />
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* SEO Content Section skeleton */}
      <section className="bg-muted/50 py-14">
        <div className="container mx-auto max-w-3xl px-4">
          <Skeleton className="mb-4 h-8 w-72" />
          <div className="space-y-3">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-5/6" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-4/5" />
          </div>
        </div>
      </section>
    </div>
  );
}
