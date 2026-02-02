/**
 * React Query hooks for Admin operations
 * Provides data fetching and mutations for admin panel
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getDashboardStats,
  getRecentActivity,
  getPendingActions,
  getUsers,
  getUserById,
  updateUserStatus,
  verifyUser,
  deleteUser,
  getUserStats,
  getAdminVehicles,
  getVehicleById,
  approveVehicle,
  rejectVehicle,
  toggleFeatured,
  deleteVehicle,
  getVehicleStats,
  getAdminDealers,
  getDealerById,
  verifyDealer,
  suspendDealer,
  reactivateDealer,
  deleteDealer,
  getDealerStats,
  getReports,
  getReportById,
  updateReportStatus,
  getReportStats,
  type UserFilters,
  type VehicleFilters,
  type DealerFilters,
  type ReportFilters,
} from '@/services/admin';

// =============================================================================
// QUERY KEYS
// =============================================================================

export const adminKeys = {
  all: ['admin'] as const,
  // Dashboard
  dashboard: () => [...adminKeys.all, 'dashboard'] as const,
  dashboardStats: () => [...adminKeys.dashboard(), 'stats'] as const,
  recentActivity: () => [...adminKeys.dashboard(), 'activity'] as const,
  pendingActions: () => [...adminKeys.dashboard(), 'pending'] as const,
  // Users
  users: () => [...adminKeys.all, 'users'] as const,
  usersList: (filters: UserFilters) => [...adminKeys.users(), 'list', filters] as const,
  userDetail: (id: string) => [...adminKeys.users(), 'detail', id] as const,
  userStats: () => [...adminKeys.users(), 'stats'] as const,
  // Vehicles
  vehicles: () => [...adminKeys.all, 'vehicles'] as const,
  vehiclesList: (filters: VehicleFilters) => [...adminKeys.vehicles(), 'list', filters] as const,
  vehicleDetail: (id: string) => [...adminKeys.vehicles(), 'detail', id] as const,
  vehicleStats: () => [...adminKeys.vehicles(), 'stats'] as const,
  // Dealers
  dealers: () => [...adminKeys.all, 'dealers'] as const,
  dealersList: (filters: DealerFilters) => [...adminKeys.dealers(), 'list', filters] as const,
  dealerDetail: (id: string) => [...adminKeys.dealers(), 'detail', id] as const,
  dealerStats: () => [...adminKeys.dealers(), 'stats'] as const,
  // Reports
  reports: () => [...adminKeys.all, 'reports'] as const,
  reportsList: (filters: ReportFilters) => [...adminKeys.reports(), 'list', filters] as const,
  reportDetail: (id: string) => [...adminKeys.reports(), 'detail', id] as const,
  reportStats: () => [...adminKeys.reports(), 'stats'] as const,
};

// =============================================================================
// DASHBOARD HOOKS
// =============================================================================

export function useDashboardStats() {
  return useQuery({
    queryKey: adminKeys.dashboardStats(),
    queryFn: getDashboardStats,
    staleTime: 60 * 1000, // 1 minute
  });
}

export function useRecentActivity(limit: number = 10) {
  return useQuery({
    queryKey: adminKeys.recentActivity(),
    queryFn: () => getRecentActivity(limit),
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function usePendingActions() {
  return useQuery({
    queryKey: adminKeys.pendingActions(),
    queryFn: getPendingActions,
    staleTime: 60 * 1000,
  });
}

// =============================================================================
// USER HOOKS
// =============================================================================

export function useAdminUsers(filters: UserFilters = {}) {
  return useQuery({
    queryKey: adminKeys.usersList(filters),
    queryFn: () => getUsers(filters),
    staleTime: 2 * 60 * 1000,
  });
}

export function useAdminUser(id: string) {
  return useQuery({
    queryKey: adminKeys.userDetail(id),
    queryFn: () => getUserById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

export function useUserStats() {
  return useQuery({
    queryKey: adminKeys.userStats(),
    queryFn: getUserStats,
    staleTime: 5 * 60 * 1000,
  });
}

export function useUpdateUserStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: 'active' | 'suspended' | 'banned' }) =>
      updateUserStatus(id, status),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
      queryClient.invalidateQueries({ queryKey: adminKeys.userDetail(id) });
    },
  });
}

export function useVerifyUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => verifyUser(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
      queryClient.invalidateQueries({ queryKey: adminKeys.userDetail(id) });
    },
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
    },
  });
}

// =============================================================================
// VEHICLE HOOKS
// =============================================================================

export function useAdminVehicles(filters: VehicleFilters = {}) {
  return useQuery({
    queryKey: adminKeys.vehiclesList(filters),
    queryFn: () => getAdminVehicles(filters),
    staleTime: 2 * 60 * 1000,
  });
}

export function useAdminVehicle(id: string) {
  return useQuery({
    queryKey: adminKeys.vehicleDetail(id),
    queryFn: () => getVehicleById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

export function useVehicleStats() {
  return useQuery({
    queryKey: adminKeys.vehicleStats(),
    queryFn: getVehicleStats,
    staleTime: 5 * 60 * 1000,
  });
}

export function useApproveVehicle() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => approveVehicle(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.vehicles() });
      queryClient.invalidateQueries({ queryKey: adminKeys.vehicleDetail(id) });
      queryClient.invalidateQueries({ queryKey: adminKeys.dashboardStats() });
    },
  });
}

export function useRejectVehicle() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => rejectVehicle(id, reason),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.vehicles() });
      queryClient.invalidateQueries({ queryKey: adminKeys.vehicleDetail(id) });
    },
  });
}

export function useToggleFeatured() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, featured }: { id: string; featured: boolean }) =>
      toggleFeatured(id, featured),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.vehicles() });
      queryClient.invalidateQueries({ queryKey: adminKeys.vehicleDetail(id) });
    },
  });
}

export function useDeleteVehicle() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteVehicle(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.vehicles() });
    },
  });
}

// =============================================================================
// DEALER HOOKS
// =============================================================================

export function useAdminDealers(filters: DealerFilters = {}) {
  return useQuery({
    queryKey: adminKeys.dealersList(filters),
    queryFn: () => getAdminDealers(filters),
    staleTime: 2 * 60 * 1000,
  });
}

export function useAdminDealer(id: string) {
  return useQuery({
    queryKey: adminKeys.dealerDetail(id),
    queryFn: () => getDealerById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

export function useDealerStatsAdmin() {
  return useQuery({
    queryKey: adminKeys.dealerStats(),
    queryFn: getDealerStats,
    staleTime: 5 * 60 * 1000,
  });
}

export function useVerifyDealer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => verifyDealer(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.dealers() });
      queryClient.invalidateQueries({ queryKey: adminKeys.dealerDetail(id) });
      queryClient.invalidateQueries({ queryKey: adminKeys.dashboardStats() });
    },
  });
}

export function useSuspendDealer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => suspendDealer(id, reason),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.dealers() });
      queryClient.invalidateQueries({ queryKey: adminKeys.dealerDetail(id) });
    },
  });
}

export function useReactivateDealer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => reactivateDealer(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.dealers() });
      queryClient.invalidateQueries({ queryKey: adminKeys.dealerDetail(id) });
    },
  });
}

export function useDeleteDealer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteDealer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.dealers() });
    },
  });
}

// =============================================================================
// REPORT HOOKS
// =============================================================================

export function useAdminReports(filters: ReportFilters = {}) {
  return useQuery({
    queryKey: adminKeys.reportsList(filters),
    queryFn: () => getReports(filters),
    staleTime: 60 * 1000,
  });
}

export function useAdminReport(id: string) {
  return useQuery({
    queryKey: adminKeys.reportDetail(id),
    queryFn: () => getReportById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

export function useReportStats() {
  return useQuery({
    queryKey: adminKeys.reportStats(),
    queryFn: getReportStats,
    staleTime: 60 * 1000,
  });
}

export function useUpdateReportStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      status,
      resolution,
    }: {
      id: string;
      status: 'investigating' | 'resolved' | 'dismissed';
      resolution?: string;
    }) => updateReportStatus(id, status, resolution),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.reports() });
      queryClient.invalidateQueries({ queryKey: adminKeys.reportDetail(id) });
      queryClient.invalidateQueries({ queryKey: adminKeys.dashboardStats() });
    },
  });
}

// =============================================================================
// RE-EXPORT TYPES
// =============================================================================

export type { UserFilters, VehicleFilters, DealerFilters, ReportFilters } from '@/services/admin';
