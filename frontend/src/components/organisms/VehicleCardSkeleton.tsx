export default function VehicleCardSkeleton() {
  return (
    <div className="card overflow-hidden animate-pulse">
      {/* Image Skeleton */}
      <div className="relative h-48 bg-gray-300"></div>

      {/* Content Skeleton */}
      <div className="p-4 space-y-3">
        {/* Title */}
        <div className="h-6 bg-gray-300 rounded w-3/4"></div>

        {/* Price */}
        <div className="h-8 bg-gray-300 rounded w-1/2"></div>

        {/* Details */}
        <div className="flex items-center gap-4">
          <div className="h-4 bg-gray-300 rounded w-20"></div>
          <div className="h-4 bg-gray-300 rounded w-24"></div>
        </div>

        {/* Chips */}
        <div className="flex gap-2">
          <div className="h-6 bg-gray-300 rounded-full w-16"></div>
          <div className="h-6 bg-gray-300 rounded-full w-16"></div>
        </div>

        {/* Button */}
        <div className="h-10 bg-gray-300 rounded-lg w-full mt-4"></div>
      </div>
    </div>
  );
}
