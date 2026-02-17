/**
 * Dashboard Skeleton Components
 *
 * Reusable loading skeletons for dashboard pages
 */

import { Skeleton } from '@/components/ui/skeleton';
import { Card, CardContent, CardHeader } from '@/components/ui/card';

// =============================================================================
// FULL DASHBOARD SKELETON
// =============================================================================

/**
 * Full dashboard skeleton with stats cards, main content, and sidebar
 */
export function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-10 w-32" />
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {[1, 2, 3, 4].map(i => (
          <Skeleton key={i} className="h-32" />
        ))}
      </div>

      {/* Two Column Layout */}
      <div className="grid gap-6 lg:grid-cols-2">
        <Skeleton className="h-80" />
        <Skeleton className="h-80" />
      </div>
    </div>
  );
}

// =============================================================================
// STATS CARD SKELETON
// =============================================================================

/**
 * Single stats card skeleton
 */
export function StatsCardSkeleton() {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <Skeleton className="h-12 w-12 rounded-lg" />
          <Skeleton className="h-4 w-12" />
        </div>
        <div className="mt-4 space-y-2">
          <Skeleton className="h-8 w-24" />
          <Skeleton className="h-4 w-32" />
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Multiple stats cards skeleton grid
 */
export function StatsGridSkeleton({ count = 4 }: { count?: number }) {
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {Array.from({ length: count }).map((_, i) => (
        <StatsCardSkeleton key={i} />
      ))}
    </div>
  );
}

// =============================================================================
// ACTIVITY/LIST SKELETON
// =============================================================================

/**
 * Activity feed or list skeleton
 */
export function ActivityListSkeleton({ count = 5 }: { count?: number }) {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-40" />
      </CardHeader>
      <CardContent className="space-y-4">
        {Array.from({ length: count }).map((_, i) => (
          <div key={i} className="flex items-start gap-3">
            <Skeleton className="h-10 w-10 shrink-0 rounded-lg" />
            <div className="flex-1 space-y-2">
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-3 w-1/2" />
            </div>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}

// =============================================================================
// TABLE SKELETON
// =============================================================================

/**
 * Table skeleton for data tables
 */
export function TableSkeleton({ rows = 5, columns = 5 }: { rows?: number; columns?: number }) {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-40" />
      </CardHeader>
      <CardContent>
        {/* Header */}
        <div className="border-border mb-4 flex gap-4 border-b pb-3">
          {Array.from({ length: columns }).map((_, i) => (
            <Skeleton key={i} className="h-4 flex-1" />
          ))}
        </div>
        {/* Rows */}
        <div className="space-y-3">
          {Array.from({ length: rows }).map((_, i) => (
            <div key={i} className="flex gap-4">
              {Array.from({ length: columns }).map((_, j) => (
                <Skeleton key={j} className="h-6 flex-1" />
              ))}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

// =============================================================================
// QUICK ACTIONS SKELETON
// =============================================================================

/**
 * Quick actions grid skeleton
 */
export function QuickActionsSkeleton({ count = 4 }: { count?: number }) {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-5 w-32" />
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
          {Array.from({ length: count }).map((_, i) => (
            <div key={i} className="flex flex-col items-center gap-3 rounded-lg border p-4">
              <Skeleton className="h-12 w-12 rounded-lg" />
              <Skeleton className="h-4 w-20" />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

// =============================================================================
// NOTIFICATIONS SKELETON
// =============================================================================

/**
 * Notifications list skeleton
 */
export function NotificationsSkeleton({ count = 4 }: { count?: number }) {
  return (
    <Card>
      <div className="divide-y">
        {Array.from({ length: count }).map((_, i) => (
          <div key={i} className="flex gap-4 p-4">
            <Skeleton className="h-10 w-10 shrink-0 rounded-full" />
            <div className="flex-1 space-y-2">
              <Skeleton className="h-4 w-48" />
              <Skeleton className="h-3 w-full" />
              <Skeleton className="h-3 w-24" />
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
}

// =============================================================================
// PROFILE SKELETON
// =============================================================================

/**
 * User profile header skeleton
 */
export function ProfileHeaderSkeleton() {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-16 w-16 rounded-full" />
          <div className="space-y-2">
            <Skeleton className="h-6 w-40" />
            <Skeleton className="h-4 w-56" />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

// =============================================================================
// VEHICLE LIST SKELETON
// =============================================================================

/**
 * Vehicle list item skeleton
 */
export function VehicleListSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-4">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex gap-4 rounded-lg border p-4">
          <Skeleton className="h-18 w-24 shrink-0 rounded-lg" />
          <div className="flex-1 space-y-2">
            <Skeleton className="h-5 w-48" />
            <Skeleton className="h-6 w-32" />
            <div className="flex gap-4">
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-4 w-16" />
            </div>
          </div>
          <Skeleton className="h-9 w-20" />
        </div>
      ))}
    </div>
  );
}
