/**
 * React Query hooks for Staff operations
 * Provides data fetching and mutations for staff management
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getStaffMembers,
  getStaffById,
  getStaffStats,
  updateStaff,
  changeStaffStatus,
  terminateStaff,
  deleteStaff,
  getInvitations,
  getInvitationById,
  createInvitation,
  resendInvitation,
  revokeInvitation,
  validateInvitationToken,
  acceptInvitation,
  getPendingInvitationsCount,
  getDepartments,
  getActiveDepartments,
  getDepartmentById,
  createDepartment,
  updateDepartment,
  deleteDepartment,
  getPositions,
  getActivePositions,
  getPositionById,
  createPosition,
  updatePosition,
  deletePosition,
  type StaffFilters,
  type InvitationFilters,
  type UpdateStaffRequest,
  type ChangeStatusRequest,
  type CreateInvitationRequest,
  type AcceptInvitationRequest,
  type CreateDepartmentRequest,
  type UpdateDepartmentRequest,
  type CreatePositionRequest,
  type UpdatePositionRequest,
} from '@/services/staff';

// Re-export types for convenience
export type {
  StaffMember,
  StaffInvitation,
  Department,
  Position,
  StaffStats,
  StaffRole,
  StaffStatus,
  InvitationStatus,
  StaffFilters,
  InvitationFilters,
} from '@/services/staff';

// =============================================================================
// QUERY KEYS
// =============================================================================

export const staffKeys = {
  all: ['staff'] as const,

  // Staff members
  members: () => [...staffKeys.all, 'members'] as const,
  membersList: (filters: StaffFilters) => [...staffKeys.members(), 'list', filters] as const,
  memberDetail: (id: string) => [...staffKeys.members(), 'detail', id] as const,
  stats: () => [...staffKeys.all, 'stats'] as const,

  // Invitations
  invitations: () => [...staffKeys.all, 'invitations'] as const,
  invitationsList: (filters: InvitationFilters) =>
    [...staffKeys.invitations(), 'list', filters] as const,
  invitationDetail: (id: string) => [...staffKeys.invitations(), 'detail', id] as const,
  invitationValidate: (token: string) => [...staffKeys.invitations(), 'validate', token] as const,
  pendingCount: () => [...staffKeys.invitations(), 'pendingCount'] as const,

  // Departments
  departments: () => [...staffKeys.all, 'departments'] as const,
  departmentsList: () => [...staffKeys.departments(), 'list'] as const,
  departmentsActive: () => [...staffKeys.departments(), 'active'] as const,
  departmentDetail: (id: string) => [...staffKeys.departments(), 'detail', id] as const,

  // Positions
  positions: () => [...staffKeys.all, 'positions'] as const,
  positionsList: () => [...staffKeys.positions(), 'list'] as const,
  positionsActive: (departmentId?: string) =>
    [...staffKeys.positions(), 'active', departmentId] as const,
  positionDetail: (id: string) => [...staffKeys.positions(), 'detail', id] as const,
};

// =============================================================================
// STAFF MEMBER HOOKS
// =============================================================================

/**
 * Get paginated staff members
 */
export function useStaffMembers(filters: StaffFilters = {}) {
  return useQuery({
    queryKey: staffKeys.membersList(filters),
    queryFn: () => getStaffMembers(filters),
    staleTime: 60 * 1000, // 1 minute
  });
}

/**
 * Get staff member by ID
 */
export function useStaffMember(id: string) {
  return useQuery({
    queryKey: staffKeys.memberDetail(id),
    queryFn: () => getStaffById(id),
    enabled: !!id,
  });
}

/**
 * Get staff statistics
 */
export function useStaffStats() {
  return useQuery({
    queryKey: staffKeys.stats(),
    queryFn: getStaffStats,
    staleTime: 60 * 1000,
  });
}

/**
 * Update staff member
 */
export function useUpdateStaff() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateStaffRequest }) => updateStaff(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: staffKeys.memberDetail(id) });
      queryClient.invalidateQueries({ queryKey: staffKeys.members() });
    },
  });
}

/**
 * Change staff status
 */
export function useChangeStaffStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ChangeStatusRequest }) =>
      changeStaffStatus(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: staffKeys.memberDetail(id) });
      queryClient.invalidateQueries({ queryKey: staffKeys.members() });
      queryClient.invalidateQueries({ queryKey: staffKeys.stats() });
    },
  });
}

/**
 * Terminate staff member
 */
export function useTerminateStaff() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => terminateStaff(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.members() });
      queryClient.invalidateQueries({ queryKey: staffKeys.stats() });
    },
  });
}

/**
 * Delete staff member (hard delete)
 */
export function useDeleteStaff() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteStaff,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.members() });
      queryClient.invalidateQueries({ queryKey: staffKeys.stats() });
    },
  });
}

// =============================================================================
// INVITATION HOOKS
// =============================================================================

/**
 * Get paginated invitations
 */
export function useInvitations(filters: InvitationFilters = {}) {
  return useQuery({
    queryKey: staffKeys.invitationsList(filters),
    queryFn: () => getInvitations(filters),
    staleTime: 30 * 1000, // 30 seconds
  });
}

/**
 * Get invitation by ID
 */
export function useInvitation(id: string) {
  return useQuery({
    queryKey: staffKeys.invitationDetail(id),
    queryFn: () => getInvitationById(id),
    enabled: !!id,
  });
}

/**
 * Create new invitation
 */
export function useCreateInvitation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateInvitationRequest) => createInvitation(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.invitations() });
      queryClient.invalidateQueries({ queryKey: staffKeys.stats() });
    },
  });
}

/**
 * Resend invitation
 */
export function useResendInvitation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: resendInvitation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.invitations() });
    },
  });
}

/**
 * Revoke invitation
 */
export function useRevokeInvitation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: revokeInvitation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.invitations() });
      queryClient.invalidateQueries({ queryKey: staffKeys.stats() });
      queryClient.invalidateQueries({ queryKey: staffKeys.pendingCount() });
    },
  });
}

/**
 * Validate invitation token (public)
 */
export function useValidateInvitation(token: string) {
  return useQuery({
    queryKey: staffKeys.invitationValidate(token),
    queryFn: () => validateInvitationToken(token),
    enabled: !!token,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: false,
  });
}

/**
 * Accept invitation (public)
 */
export function useAcceptInvitation() {
  return useMutation({
    mutationFn: (data: AcceptInvitationRequest) => acceptInvitation(data),
  });
}

/**
 * Get pending invitations count
 */
export function usePendingInvitationsCount() {
  return useQuery({
    queryKey: staffKeys.pendingCount(),
    queryFn: getPendingInvitationsCount,
    staleTime: 30 * 1000,
  });
}

// =============================================================================
// DEPARTMENT HOOKS
// =============================================================================

/**
 * Get all departments
 */
export function useDepartments() {
  return useQuery({
    queryKey: staffKeys.departmentsList(),
    queryFn: () => getDepartments(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get all active departments (for dropdowns)
 */
export function useActiveDepartments() {
  return useQuery({
    queryKey: staffKeys.departmentsActive(),
    queryFn: getActiveDepartments,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get department by ID
 */
export function useDepartment(id: string) {
  return useQuery({
    queryKey: staffKeys.departmentDetail(id),
    queryFn: () => getDepartmentById(id),
    enabled: !!id,
  });
}

/**
 * Create new department
 */
export function useCreateDepartment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateDepartmentRequest) => createDepartment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.departments() });
    },
  });
}

/**
 * Update department
 */
export function useUpdateDepartment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateDepartmentRequest }) =>
      updateDepartment(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: staffKeys.departmentDetail(id) });
      queryClient.invalidateQueries({ queryKey: staffKeys.departments() });
    },
  });
}

/**
 * Delete department
 */
export function useDeleteDepartment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteDepartment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.departments() });
    },
  });
}

// =============================================================================
// POSITION HOOKS
// =============================================================================

/**
 * Get all positions
 */
export function usePositions() {
  return useQuery({
    queryKey: staffKeys.positionsList(),
    queryFn: () => getPositions(),
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get all active positions (for dropdowns)
 */
export function useActivePositions(departmentId?: string) {
  return useQuery({
    queryKey: staffKeys.positionsActive(departmentId),
    queryFn: () => getActivePositions(departmentId),
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get position by ID
 */
export function usePosition(id: string) {
  return useQuery({
    queryKey: staffKeys.positionDetail(id),
    queryFn: () => getPositionById(id),
    enabled: !!id,
  });
}

/**
 * Create new position
 */
export function useCreatePosition() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePositionRequest) => createPosition(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.positions() });
    },
  });
}

/**
 * Update position
 */
export function useUpdatePosition() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePositionRequest }) =>
      updatePosition(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: staffKeys.positionDetail(id) });
      queryClient.invalidateQueries({ queryKey: staffKeys.positions() });
    },
  });
}

/**
 * Delete position
 */
export function useDeletePosition() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deletePosition,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: staffKeys.positions() });
    },
  });
}
