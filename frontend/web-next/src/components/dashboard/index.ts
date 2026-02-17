/**
 * Dashboard Components
 *
 * Centralized exports for all dashboard-related components.
 * Import from '@/components/dashboard' for clean imports.
 */

// Status badges
export {
  StatusBadge,
  TicketStatusBadge,
  VehicleStatusBadge,
  KYCStatusBadge,
  PriorityBadge,
  ContentStatusBadge,
  // Legacy functions (deprecated)
  getStatusBadge,
  getTicketStatusBadge,
  getVehicleStatusBadge,
  getKYCStatusBadge,
  getPriorityBadge,
} from './status-badge';

export type {
  GeneralStatus,
  TicketStatus,
  VehicleStatus,
  KYCStatus,
  Priority,
  ContentStatus,
} from './status-badge';

// Stats cards
export { StatsCard, CompactStatsCard, HorizontalStatsCard } from './stats-card';

export type {
  StatsCardProps,
  CompactStatsCardProps,
  StatColor,
  TrendDirection,
} from './stats-card';

// Quick actions
export { QuickAction, QuickActionButton, QuickActionCard } from './quick-action';

export type { QuickActionProps, ActionColor } from './quick-action';

// Skeletons
export {
  DashboardSkeleton,
  StatsCardSkeleton,
  StatsGridSkeleton,
  ActivityListSkeleton,
  TableSkeleton,
  QuickActionsSkeleton,
  NotificationsSkeleton,
  ProfileHeaderSkeleton,
  VehicleListSkeleton,
} from './skeleton';

// User profile card
export {
  UserProfileCard,
  VerificationBadge,
  RatingDisplay,
  getInitials,
  getDisplayName,
} from './user-profile-card';

export type {
  UserProfileCardProps,
  UserProfileVariant,
  AccountTypeBadge,
} from './user-profile-card';
