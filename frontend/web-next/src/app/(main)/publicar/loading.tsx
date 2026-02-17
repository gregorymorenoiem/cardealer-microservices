import { Skeleton } from '@/components/ui/skeleton';

export default function PublicarLoading() {
  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      {/* Header */}
      <Skeleton className="mx-auto mb-2 h-8 w-64" />
      <Skeleton className="mx-auto mb-8 h-4 w-96" />
      {/* Progress steps */}
      <div className="mb-8 flex justify-center gap-4">
        {[1, 2, 3, 4].map(i => (
          <Skeleton key={i} className="h-10 w-10 rounded-full" />
        ))}
      </div>
      {/* Form skeleton */}
      <div className="space-y-6 rounded-xl border p-6">
        <Skeleton className="h-6 w-48" />
        <div className="grid grid-cols-2 gap-4">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-10 w-full" />
      </div>
    </div>
  );
}
