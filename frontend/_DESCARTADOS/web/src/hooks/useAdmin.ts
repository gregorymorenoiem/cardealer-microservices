/**
 * Admin hooks for TanStack Query
 * Sprint 10: Admin Panel & Dealer Management
 */
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as adminService from '@/services/adminService';
import type { 
  DashboardStats, 
  ActivityLog, 
  User, 
  ReportedContent
} from '@/services/adminService';
import { mockAdminStats, mockActivityLogs, mockAdminUsers, mockReports } from '@/data/mockAdmin';

// ============================================================================
// Query Keys Factory
// ============================================================================

export const adminKeys = {
  all: ['admin'] as const,
  
  // Dashboard
  stats: () => [...adminKeys.all, 'stats'] as const,
  platformStats: () => [...adminKeys.all, 'platformStats'] as const,
  revenueStats: (startDate?: string, endDate?: string) => 
    [...adminKeys.all, 'revenue', { startDate, endDate }] as const,
  
  // Users
  users: () => [...adminKeys.all, 'users'] as const,
  usersList: (filters?: { search?: string; role?: string; isActive?: boolean }) => 
    [...adminKeys.users(), 'list', filters] as const,
  user: (id: string) => [...adminKeys.users(), id] as const,
  
  // Activity Logs
  activityLogs: () => [...adminKeys.all, 'activityLogs'] as const,
  activityLogsList: (page: number, pageSize: number, filters?: Record<string, unknown>) => 
    [...adminKeys.activityLogs(), 'list', { page, pageSize, ...filters }] as const,
  
  // Reports
  reports: () => [...adminKeys.all, 'reports'] as const,
  reportsList: (page: number, pageSize: number, filters?: { contentType?: string; status?: string }) => 
    [...adminKeys.reports(), 'list', { page, pageSize, ...filters }] as const,
  
  // Settings
  settings: () => [...adminKeys.all, 'settings'] as const,
  
  // Pending Approvals
  pendingApprovals: () => [...adminKeys.all, 'pendingApprovals'] as const,
  pendingApprovalsList: (page?: number, pageSize?: number) => 
    [...adminKeys.pendingApprovals(), 'list', { page, pageSize }] as const,
};

// ============================================================================
// Dashboard Hooks
// ============================================================================

/**
 * Hook to get admin dashboard statistics
 */
export function useAdminStats() {
  return useQuery({
    queryKey: adminKeys.stats(),
    queryFn: async () => {
      try {
        return await adminService.getDashboardStats();
      } catch {
        // Fallback to mock data
        console.warn('Using mock admin stats');
        return mockAdminStats as unknown as DashboardStats;
      }
    },
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // Auto-refresh every minute
  });
}

/**
 * Hook to get platform statistics (growth, trends)
 */
export function usePlatformStats() {
  return useQuery({
    queryKey: adminKeys.platformStats(),
    queryFn: adminService.getPlatformStats,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Hook to get revenue statistics
 */
export function useRevenueStats(startDate?: string, endDate?: string) {
  return useQuery({
    queryKey: adminKeys.revenueStats(startDate, endDate),
    queryFn: () => adminService.getRevenueStats(startDate, endDate),
    staleTime: 5 * 60 * 1000,
  });
}

// ============================================================================
// User Management Hooks
// ============================================================================

/**
 * Hook to get users list with pagination and filters
 */
export function useUsers(
  page: number = 1,
  pageSize: number = 20,
  filters?: { search?: string; role?: string; isVerified?: boolean; isActive?: boolean }
) {
  return useQuery({
    queryKey: adminKeys.usersList(filters),
    queryFn: async () => {
      try {
        return await adminService.getUsers(page, pageSize, filters);
      } catch {
        // Fallback to mock data
        console.warn('Using mock users data');
        const filteredUsers = mockAdminUsers.filter((user: unknown) => {
          const u = user as User;
          if (filters?.search && !u.name.toLowerCase().includes(filters.search.toLowerCase()) && 
              !u.email.toLowerCase().includes(filters.search.toLowerCase())) {
            return false;
          }
          if (filters?.role && u.role !== filters.role) return false;
          if (filters?.isActive !== undefined && u.isActive !== filters.isActive) return false;
          return true;
        });
        
        const start = (page - 1) * pageSize;
        const paginatedUsers = filteredUsers.slice(start, start + pageSize);
        
        return {
          users: paginatedUsers as unknown as User[],
          total: filteredUsers.length,
          page,
          pageSize,
          totalPages: Math.ceil(filteredUsers.length / pageSize),
        };
      }
    },
    staleTime: 30 * 1000,
  });
}

/**
 * Hook to get a single user by ID
 */
export function useUser(id: string) {
  return useQuery({
    queryKey: adminKeys.user(id),
    queryFn: () => adminService.getUserById(id),
    enabled: !!id,
  });
}

/**
 * Hook to update a user
 */
export function useUpdateUser() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, updates }: { id: string; updates: Partial<User> }) =>
      adminService.updateUser(id, updates),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminKeys.user(id) });
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
    },
  });
}

/**
 * Hook to delete a user
 */
export function useDeleteUser() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: adminService.deleteUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
      queryClient.invalidateQueries({ queryKey: adminKeys.stats() });
    },
  });
}

/**
 * Hook to ban a user
 */
export function useBanUser() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, reason, duration }: { id: string; reason: string; duration?: number }) =>
      adminService.banUser(id, reason, duration),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
    },
  });
}

/**
 * Hook to unban a user
 */
export function useUnbanUser() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: adminService.unbanUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
    },
  });
}

// ============================================================================
// Activity Log Hooks
// ============================================================================

/**
 * Hook to get activity logs with pagination
 */
export function useActivityLogs(
  page: number = 1,
  pageSize: number = 50,
  filters?: {
    userId?: string;
    entityType?: string;
    startDate?: string;
    endDate?: string;
  }
) {
  return useQuery({
    queryKey: adminKeys.activityLogsList(page, pageSize, filters),
    queryFn: async () => {
      try {
        return await adminService.getActivityLogs(page, pageSize, filters);
      } catch {
        // Fallback to mock data
        console.warn('Using mock activity logs');
        const start = (page - 1) * pageSize;
        const paginatedLogs = mockActivityLogs.slice(start, start + pageSize);
        
        return {
          logs: paginatedLogs as unknown as ActivityLog[],
          total: mockActivityLogs.length,
          page,
          pageSize,
          totalPages: Math.ceil(mockActivityLogs.length / pageSize),
        };
      }
    },
    staleTime: 30 * 1000,
  });
}

// ============================================================================
// Reports/Moderation Hooks
// ============================================================================

/**
 * Hook to get reported content
 */
export function useReportedContent(
  page: number = 1,
  pageSize: number = 20,
  filters?: { contentType?: string; status?: string }
) {
  return useQuery({
    queryKey: adminKeys.reportsList(page, pageSize, filters),
    queryFn: async () => {
      try {
        return await adminService.getReportedContent(page, pageSize, filters);
      } catch {
        // Fallback to mock data
        console.warn('Using mock reported content');
        const filteredReports = mockReports.filter((report: unknown) => {
          const r = report as ReportedContent;
          if (filters?.contentType && r.contentType !== filters.contentType) return false;
          if (filters?.status && r.status !== filters.status) return false;
          return true;
        });
        
        const start = (page - 1) * pageSize;
        const paginatedReports = filteredReports.slice(start, start + pageSize);
        
        return {
          reports: paginatedReports as unknown as ReportedContent[],
          total: filteredReports.length,
          page,
          pageSize,
          totalPages: Math.ceil(filteredReports.length / pageSize),
        };
      }
    },
    staleTime: 30 * 1000,
  });
}

/**
 * Hook to review (resolve/dismiss) a report
 */
export function useReviewReport() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, action, notes }: { id: string; action: 'resolve' | 'dismiss'; notes?: string }) =>
      adminService.reviewReport(id, action, notes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.reports() });
      queryClient.invalidateQueries({ queryKey: adminKeys.stats() });
    },
  });
}

// ============================================================================
// System Settings Hooks
// ============================================================================

/**
 * Hook to get system settings
 */
export function useSystemSettings() {
  return useQuery({
    queryKey: adminKeys.settings(),
    queryFn: adminService.getSystemSettings,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Hook to update system settings
 */
export function useUpdateSystemSettings() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: adminService.updateSystemSettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.settings() });
    },
  });
}

// ============================================================================
// Export & Notification Hooks
// ============================================================================

/**
 * Hook to export data
 */
export function useExportData() {
  return useMutation({
    mutationFn: ({ dataType, format, filters }: {
      dataType: 'users' | 'vehicles' | 'transactions' | 'reports';
      format: 'csv' | 'json' | 'xlsx';
      filters?: Record<string, unknown>;
    }) => adminService.exportData(dataType, format, filters),
    onSuccess: (blob, { dataType, format }) => {
      // Download the file
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${dataType}-export-${new Date().toISOString().split('T')[0]}.${format}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    },
  });
}

/**
 * Hook to send system notification
 */
export function useSendSystemNotification() {
  return useMutation({
    mutationFn: ({ title, message, targetUsers }: {
      title: string;
      message: string;
      targetUsers?: 'all' | 'verified' | 'admins';
    }) => adminService.sendSystemNotification(title, message, targetUsers),
  });
}

// ============================================================================
// Composite Hooks
// ============================================================================

/**
 * Composite hook for admin dashboard - combines stats, activity, and platform data
 */
export function useAdminDashboard() {
  const stats = useAdminStats();
  const activityLogs = useActivityLogs(1, 5); // Last 5 activities
  const platformStats = usePlatformStats();
  
  return {
    stats: stats.data,
    activityLogs: activityLogs.data?.logs ?? [],
    platformStats: platformStats.data,
    isLoading: stats.isLoading || activityLogs.isLoading,
    isError: stats.isError && activityLogs.isError,
    refetch: () => {
      stats.refetch();
      activityLogs.refetch();
      platformStats.refetch();
    },
  };
}

/**
 * Composite hook for users management page
 */
export function useUsersManagement(
  page: number = 1,
  pageSize: number = 20,
  filters?: { search?: string; role?: string; isActive?: boolean }
) {
  const users = useUsers(page, pageSize, filters);
  const updateUser = useUpdateUser();
  const deleteUser = useDeleteUser();
  const banUser = useBanUser();
  const unbanUser = useUnbanUser();
  
  return {
    users: users.data?.users ?? [],
    total: users.data?.total ?? 0,
    totalPages: users.data?.totalPages ?? 0,
    isLoading: users.isLoading,
    isError: users.isError,
    refetch: users.refetch,
    updateUser,
    deleteUser,
    banUser,
    unbanUser,
  };
}

/**
 * Composite hook for moderation/reports page
 */
export function useModerationPage(
  page: number = 1,
  pageSize: number = 20,
  filters?: { contentType?: string; status?: string }
) {
  const reports = useReportedContent(page, pageSize, filters);
  const reviewReport = useReviewReport();
  
  return {
    reports: reports.data?.reports ?? [],
    total: reports.data?.total ?? 0,
    totalPages: reports.data?.totalPages ?? 0,
    isLoading: reports.isLoading,
    isError: reports.isError,
    refetch: reports.refetch,
    reviewReport,
  };
}

/**
 * Composite hook for settings page
 */
export function useSettingsPage() {
  const settings = useSystemSettings();
  const updateSettings = useUpdateSystemSettings();
  
  return {
    settings: settings.data,
    isLoading: settings.isLoading,
    isError: settings.isError,
    updateSettings,
    refetch: settings.refetch,
  };
}

// ============================================================================
// Pending Approvals Hooks
// ============================================================================

import { mockPendingVehicles } from '@/data/mockAdmin';
import type { PendingVehicle } from '@/types/admin';

/**
 * Hook to get pending vehicle approvals
 */
export function usePendingApprovals(page: number = 1, pageSize: number = 20) {
  return useQuery({
    queryKey: adminKeys.pendingApprovalsList(page, pageSize),
    queryFn: async (): Promise<{ vehicles: PendingVehicle[]; total: number; page: number; pageSize: number }> => {
      try {
        // When backend endpoint exists:
        // return await adminService.getPendingApprovals(page, pageSize);
        
        // For now, use mock data
        console.warn('Using mock pending approvals');
        const start = (page - 1) * pageSize;
        const paginatedVehicles = mockPendingVehicles.slice(start, start + pageSize);
        
        return {
          vehicles: paginatedVehicles,
          total: mockPendingVehicles.length,
          page,
          pageSize,
        };
      } catch {
        // Fallback to mock data
        console.warn('Using mock pending approvals (error fallback)');
        const start = (page - 1) * pageSize;
        const paginatedVehicles = mockPendingVehicles.slice(start, start + pageSize);
        
        return {
          vehicles: paginatedVehicles,
          total: mockPendingVehicles.length,
          page,
          pageSize,
        };
      }
    },
    staleTime: 30 * 1000,
  });
}

/**
 * Hook to approve a vehicle
 */
export function useApproveVehicle() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (vehicleId: string) => {
      // When backend endpoint exists:
      // return await adminService.approveVehicle(vehicleId);
      
      // For now, simulate API call
      await new Promise(resolve => setTimeout(resolve, 500));
      return { success: true, vehicleId };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.pendingApprovals() });
      queryClient.invalidateQueries({ queryKey: adminKeys.stats() });
    },
  });
}

/**
 * Hook to reject a vehicle
 */
export function useRejectVehicle() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ vehicleId, reason }: { vehicleId: string; reason: string }) => {
      // When backend endpoint exists:
      // return await adminService.rejectVehicle(vehicleId, reason);
      
      // For now, simulate API call
      await new Promise(resolve => setTimeout(resolve, 500));
      return { success: true, vehicleId, reason };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.pendingApprovals() });
      queryClient.invalidateQueries({ queryKey: adminKeys.stats() });
    },
  });
}

/**
 * Composite hook for pending approvals page
 */
export function usePendingApprovalsPage(page: number = 1, pageSize: number = 20) {
  const pendingQuery = usePendingApprovals(page, pageSize);
  const approveVehicle = useApproveVehicle();
  const rejectVehicle = useRejectVehicle();
  
  return {
    vehicles: pendingQuery.data?.vehicles ?? [],
    total: pendingQuery.data?.total ?? 0,
    isLoading: pendingQuery.isLoading,
    isError: pendingQuery.isError,
    refetch: pendingQuery.refetch,
    approveVehicle,
    rejectVehicle,
  };
}
