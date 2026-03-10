/**
 * Loading skeleton for /marcas/[marca]/[modelo] page.
 * Shows shimmer placeholders while the model page SSR resolves.
 */
export default function ModelLoading() {
  return (
    <div className="bg-background min-h-screen">
      {/* Hero Skeleton */}
      <section className="bg-gradient-to-br from-gray-900 to-gray-800 py-16">
        <div className="container mx-auto px-4">
          <div className="mb-6 h-4 w-48 animate-pulse rounded bg-white/20" />
          <div className="h-10 w-96 animate-pulse rounded bg-white/20" />
          <div className="mt-4 h-6 w-[32rem] animate-pulse rounded bg-white/10" />
        </div>
      </section>

      {/* Grid Skeleton */}
      <section className="container mx-auto px-4 py-10">
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="bg-muted h-80 animate-pulse rounded-xl" />
          ))}
        </div>
      </section>
    </div>
  );
}
