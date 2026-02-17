/**
 * Dealer Employees Service
 *
 * Manages dealer team members and access permissions
 */

import { apiClient } from '@/lib/api-client';

// ============================================================================
// Types
// ============================================================================

export type DealerRole =
  | 'Owner'
  | 'Admin'
  | 'SalesManager'
  | 'Salesperson'
  | 'InventoryManager'
  | 'Viewer';

export type EmployeeStatus = 'Pending' | 'Active' | 'Suspended';

export interface DealerEmployee {
  id: string;
  userId: string;
  dealerId: string;
  name: string;
  email: string;
  phone?: string;
  role: DealerRole;
  status: EmployeeStatus;
  invitationDate: string;
  activationDate?: string;
  avatarUrl?: string;
  // Performance stats
  leadsCount?: number;
  salesCount?: number;
  rating?: number;
}

export interface DealerEmployeeInvitation {
  id: string;
  email: string;
  role: DealerRole;
  status: string;
  invitationDate: string;
  expirationDate: string;
}

export interface InviteEmployeeRequest {
  email: string;
  role: DealerRole;
  permissions?: string[];
  invitedBy?: string;
}

export interface UpdateEmployeeRoleRequest {
  role: DealerRole;
  permissions?: string[];
}

export interface EmployeeStats {
  totalEmployees: number;
  activeEmployees: number;
  pendingInvitations: number;
  totalLeads: number;
  totalSales: number;
  avgRating: number;
}

export interface RoleDefinition {
  id: string;
  name: string;
  description: string;
  permissions: string[];
}

// ============================================================================
// API Functions
// ============================================================================

/**
 * Get all employees for a dealer
 */
export async function getDealerEmployees(dealerId: string): Promise<DealerEmployee[]> {
  const response = await apiClient.get<DealerEmployee[]>(`/api/dealers/${dealerId}/employees`);
  return response.data;
}

/**
 * Get a single employee by ID
 */
export async function getEmployeeById(
  dealerId: string,
  employeeId: string
): Promise<DealerEmployee> {
  const response = await apiClient.get<DealerEmployee>(
    `/api/dealers/${dealerId}/employees/${employeeId}`
  );
  return response.data;
}

/**
 * Invite a new employee
 */
export async function inviteEmployee(
  dealerId: string,
  data: InviteEmployeeRequest
): Promise<DealerEmployeeInvitation> {
  const response = await apiClient.post<DealerEmployeeInvitation>(
    `/api/dealers/${dealerId}/employees/invite`,
    data
  );
  return response.data;
}

/**
 * Update employee role
 */
export async function updateEmployeeRole(
  dealerId: string,
  employeeId: string,
  data: UpdateEmployeeRoleRequest
): Promise<DealerEmployee> {
  const response = await apiClient.put<DealerEmployee>(
    `/api/dealers/${dealerId}/employees/${employeeId}/role`,
    data
  );
  return response.data;
}

/**
 * Suspend an employee
 */
export async function suspendEmployee(
  dealerId: string,
  employeeId: string
): Promise<DealerEmployee> {
  const response = await apiClient.put<DealerEmployee>(
    `/api/dealers/${dealerId}/employees/${employeeId}/suspend`
  );
  return response.data;
}

/**
 * Reactivate a suspended employee
 */
export async function reactivateEmployee(
  dealerId: string,
  employeeId: string
): Promise<DealerEmployee> {
  const response = await apiClient.put<DealerEmployee>(
    `/api/dealers/${dealerId}/employees/${employeeId}/reactivate`
  );
  return response.data;
}

/**
 * Remove an employee
 */
export async function removeEmployee(dealerId: string, employeeId: string): Promise<void> {
  await apiClient.delete(`/api/dealers/${dealerId}/employees/${employeeId}`);
}

/**
 * Resend invitation
 */
export async function resendInvitation(
  dealerId: string,
  invitationId: string
): Promise<DealerEmployeeInvitation> {
  const response = await apiClient.post<DealerEmployeeInvitation>(
    `/api/dealers/${dealerId}/employees/invitations/${invitationId}/resend`
  );
  return response.data;
}

/**
 * Cancel invitation
 */
export async function cancelInvitation(dealerId: string, invitationId: string): Promise<void> {
  await apiClient.delete(`/api/dealers/${dealerId}/employees/invitations/${invitationId}`);
}

/**
 * Get pending invitations
 */
export async function getPendingInvitations(dealerId: string): Promise<DealerEmployeeInvitation[]> {
  const response = await apiClient.get<DealerEmployeeInvitation[]>(
    `/api/dealers/${dealerId}/employees/invitations`
  );
  return response.data;
}

/**
 * Get employee statistics
 */
export async function getEmployeeStats(dealerId: string): Promise<EmployeeStats> {
  const response = await apiClient.get<EmployeeStats>(`/api/dealers/${dealerId}/employees/stats`);
  return response.data;
}

/**
 * Get available roles
 */
export async function getAvailableRoles(): Promise<RoleDefinition[]> {
  const response = await apiClient.get<RoleDefinition[]>('/api/dealers/roles');
  return response.data;
}

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * Get role label in Spanish
 */
export function getRoleLabel(role: DealerRole): string {
  const labels: Record<DealerRole, string> = {
    Owner: 'Propietario',
    Admin: 'Administrador',
    SalesManager: 'Gerente de Ventas',
    Salesperson: 'Vendedor',
    InventoryManager: 'Encargado de Inventario',
    Viewer: 'Solo Lectura',
  };
  return labels[role] || role;
}

/**
 * Get role color for badges
 */
export function getRoleColor(
  role: DealerRole
): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (role) {
    case 'Owner':
      return 'default';
    case 'Admin':
      return 'secondary';
    case 'SalesManager':
      return 'secondary';
    case 'Salesperson':
      return 'outline';
    case 'InventoryManager':
      return 'outline';
    case 'Viewer':
      return 'outline';
    default:
      return 'default';
  }
}

/**
 * Get status label in Spanish
 */
export function getStatusLabel(status: EmployeeStatus): string {
  const labels: Record<EmployeeStatus, string> = {
    Pending: 'Pendiente',
    Active: 'Activo',
    Suspended: 'Suspendido',
  };
  return labels[status] || status;
}

/**
 * Get status color for badges
 */
export function getStatusColor(
  status: EmployeeStatus
): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (status) {
    case 'Active':
      return 'default';
    case 'Pending':
      return 'secondary';
    case 'Suspended':
      return 'destructive';
    default:
      return 'default';
  }
}

/**
 * Get initials from name
 */
export function getInitials(name: string): string {
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

/**
 * Format date for display
 */
export function formatJoinDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString('es-DO', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

/**
 * Calculate employee stats from list
 */
export function calculateStats(employees: DealerEmployee[]): EmployeeStats {
  const activeEmployees = employees.filter(e => e.status === 'Active');
  const pendingInvitations = employees.filter(e => e.status === 'Pending');

  const totalLeads = employees.reduce((sum, e) => sum + (e.leadsCount || 0), 0);
  const totalSales = employees.reduce((sum, e) => sum + (e.salesCount || 0), 0);

  const ratingsSum = employees.reduce((sum, e) => sum + (e.rating || 0), 0);
  const ratingsCount = employees.filter(e => e.rating && e.rating > 0).length;
  const avgRating = ratingsCount > 0 ? ratingsSum / ratingsCount : 0;

  return {
    totalEmployees: employees.length,
    activeEmployees: activeEmployees.length,
    pendingInvitations: pendingInvitations.length,
    totalLeads,
    totalSales,
    avgRating: Math.round(avgRating * 10) / 10,
  };
}
