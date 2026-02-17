import { Skeleton } from '@/components/ui/skeleton';

export default function CheckoutLoading() {
  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      {/* Header */}
      <Skeleton className="mb-8 h-8 w-48" />
      <div className="grid gap-8 lg:grid-cols-3">
        {/* Form column */}
        <div className="space-y-6 lg:col-span-2">
          <div className="rounded-xl border p-6">
            <Skeleton className="mb-4 h-6 w-40" />
            <div className="space-y-4">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <div className="grid grid-cols-2 gap-4">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </div>
            </div>
          </div>
          <div className="rounded-xl border p-6">
            <Skeleton className="mb-4 h-6 w-48" />
            <Skeleton className="h-40 w-full" />
          </div>
        </div>
        {/* Summary column */}
        <div className="rounded-xl border p-6">
          <Skeleton className="mb-4 h-6 w-32" />
          <div className="space-y-3">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-3/4" />
            <div className="border-t pt-3">
              <Skeleton className="h-6 w-full" />
            </div>
            <Skeleton className="mt-4 h-12 w-full" />
          </div>
        </div>
      </div>
    </div>
  );
}
