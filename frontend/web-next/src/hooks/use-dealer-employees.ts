/**
 * Dealer Employees Hooks
 *
 * React Query hooks for dealer team management
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getDealerEmployees,
  getEmployeeById,
  inviteEmployee,
  updateEmployeeRole,
  suspendEmployee,
  reactivateEmployee,
  removeEmployee,
  getPendingInvitations,
  resendInvitation,
  cancelInvitation,
  getEmployeeStats,
  getAvailableRoles,
  calculateStats,
  type DealerEmployee,
  type InviteEmployeeRequest,
  type UpdateEmployeeRoleRequest,
} from '@/services/dealer-employees';

// ============================================================================
// Query Keys
// ============================================================================

export const dealerEmployeesKeys = {
  all: ['dealer-employees'] as const,
  lists: () => [...dealerEmployeesKeys.all, 'list'] as const,
  list: (dealerId: string) => [...dealerEmployeesKeys.lists(), dealerId] as const,
  details: () => [...dealerEmployeesKeys.all, 'detail'] as const,
  detail: (dealerId: string, employeeId: string) =>
    [...dealerEmployeesKeys.details(), dealerId, employeeId] as const,
  invitations: () => [...dealerEmployeesKeys.all, 'invitations'] as const,
  invitation: (dealerId: string) => [...dealerEmployeesKeys.invitations(), dealerId] as const,
  stats: () => [...dealerEmployeesKeys.all, 'stats'] as const,
  dealerStats: (dealerId: string) => [...dealerEmployeesKeys.stats(), dealerId] as const,
  roles: () => [...dealerEmployeesKeys.all, 'roles'] as const,
};

// ============================================================================
// Query Hooks
// ============================================================================

/**
 * Get all employees for a dealer
 */
export function useDealerEmployees(dealerId: string) {
  return useQuery({
    queryKey: dealerEmployeesKeys.list(dealerId),
    queryFn: () => getDealerEmployees(dealerId),
    enabled: !!dealerId,
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

/**
 * Get a single employee by ID
 */
export function useEmployee(dealerId: string, employeeId: string) {
  return useQuery({
    queryKey: dealerEmployeesKeys.detail(dealerId, employeeId),
    queryFn: () => getEmployeeById(dealerId, employeeId),
    enabled: !!dealerId && !!employeeId,
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

/**
 * Get pending invitations
 */
export function usePendingInvitations(dealerId: string) {
  return useQuery({
    queryKey: dealerEmployeesKeys.invitation(dealerId),
    queryFn: () => getPendingInvitations(dealerId),
    enabled: !!dealerId,
    staleTime: 1 * 60 * 1000, // 1 minute
  });
}

/**
 * Get employee stats for a dealer
 */
export function useEmployeeStats(dealerId: string) {
  return useQuery({
    queryKey: dealerEmployeesKeys.dealerStats(dealerId),
    queryFn: () => getEmployeeStats(dealerId),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get available roles
 */
export function useAvailableRoles() {
  return useQuery({
    queryKey: dealerEmployeesKeys.roles(),
    queryFn: getAvailableRoles,
    staleTime: 30 * 60 * 1000, // 30 minutes (roles rarely change)
  });
}

/**
 * Get computed stats from employees list
 */
export function useComputedEmployeeStats(employees: DealerEmployee[] | undefined) {
  if (!employees || employees.length === 0) {
    return {
      totalEmployees: 0,
      activeEmployees: 0,
      pendingInvitations: 0,
      totalLeads: 0,
      totalSales: 0,
      avgRating: 0,
    };
  }
  return calculateStats(employees);
}

// ============================================================================
// Mutation Hooks
// ============================================================================

/**
 * Invite a new employee
 */
export function useInviteEmployee(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: InviteEmployeeRequest) => inviteEmployee(dealerId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.list(dealerId),
      });
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.invitation(dealerId),
      });
    },
  });
}

/**
 * Update employee role
 */
export function useUpdateEmployeeRole(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ employeeId, data }: { employeeId: string; data: UpdateEmployeeRoleRequest }) =>
      updateEmployeeRole(dealerId, employeeId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.list(dealerId),
      });
    },
  });
}

/**
 * Suspend an employee
 */
export function useSuspendEmployee(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (employeeId: string) => suspendEmployee(dealerId, employeeId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.list(dealerId),
      });
    },
  });
}

/**
 * Reactivate an employee
 */
export function useReactivateEmployee(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (employeeId: string) => reactivateEmployee(dealerId, employeeId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.list(dealerId),
      });
    },
  });
}

/**
 * Remove an employee
 */
export function useRemoveEmployee(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (employeeId: string) => removeEmployee(dealerId, employeeId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.list(dealerId),
      });
    },
  });
}

/**
 * Resend invitation
 */
export function useResendInvitation(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (invitationId: string) => resendInvitation(dealerId, invitationId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.invitation(dealerId),
      });
    },
  });
}

/**
 * Cancel invitation
 */
export function useCancelInvitation(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (invitationId: string) => cancelInvitation(dealerId, invitationId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerEmployeesKeys.invitation(dealerId),
      });
    },
  });
}

// ============================================================================
// Derived Hooks
// ============================================================================

/**
 * Get employees filtered by role
 */
export function useEmployeesByRole(dealerId: string, role: string | undefined) {
  const { data: employees, ...rest } = useDealerEmployees(dealerId);

  const filteredEmployees = employees?.filter(e => !role || e.role === role);

  return {
    data: filteredEmployees,
    ...rest,
  };
}

/**
 * Get employees filtered by status
 */
export function useEmployeesByStatus(dealerId: string, status: string | undefined) {
  const { data: employees, ...rest } = useDealerEmployees(dealerId);

  const filteredEmployees = employees?.filter(e => !status || e.status === status);

  return {
    data: filteredEmployees,
    ...rest,
  };
}

/**
 * Get active employees only
 */
export function useActiveEmployees(dealerId: string) {
  const { data: employees, ...rest } = useDealerEmployees(dealerId);

  const activeEmployees = employees?.filter(e => e.status === 'Active');

  return {
    data: activeEmployees,
    ...rest,
  };
}
