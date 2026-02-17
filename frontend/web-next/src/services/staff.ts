/**
 * Staff Service - API client for StaffService operations
 * Connects via API Gateway to StaffService
 *
 * Manages internal staff: SuperAdmin, Admin, Moderator, Support, Analyst, Compliance
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// TYPES
// ============================================================

// Staff Types
export type StaffRole = 'SuperAdmin' | 'Admin' | 'Moderator' | 'Support' | 'Analyst' | 'Compliance';

export type StaffStatus = 'Pending' | 'Active' | 'Suspended' | 'OnLeave' | 'Terminated';

export type InvitationStatus = 'Pending' | 'Accepted' | 'Expired' | 'Revoked';

// Department (matches DepartmentDto)
export interface Department {
  id: string;
  name: string;
  description?: string;
  code?: string;
  parentDepartmentId?: string;
  parentDepartmentName?: string;
  headId?: string;
  headName?: string;
  staffCount: number;
  isActive: boolean;
}

// Position (matches PositionDto)
export interface Position {
  id: string;
  title: string;
  description?: string;
  code?: string;
  departmentId?: string;
  departmentName?: string;
  defaultRole?: StaffRole;
  level: number;
  staffCount: number;
  isActive: boolean;
}

// Staff Member (matches StaffDto + StaffListItemDto)
export interface StaffMember {
  id: string;
  authUserId?: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName: string;
  phoneNumber?: string;
  avatarUrl?: string;
  employeeCode?: string;
  departmentId?: string;
  departmentName?: string;
  positionId?: string;
  positionTitle?: string;
  supervisorId?: string;
  supervisorName?: string;
  status: StaffStatus;
  role: StaffRole;
  hireDate?: string;
  lastLoginAt?: string;
  twoFactorEnabled?: boolean;
  createdAt?: string;
}

// Staff Invitation (matches InvitationDto)
export interface StaffInvitation {
  id: string;
  email: string;
  assignedRole: StaffRole;
  departmentName?: string;
  positionTitle?: string;
  status: InvitationStatus;
  expiresAt: string;
  acceptedAt?: string;
  invitedByName: string;
  createdAt: string;
  isExpired: boolean;
  isValid: boolean;
}

// Staff Stats (matches StaffSummaryDto)
export interface StaffStats {
  total: number;
  active: number;
  pending: number;
  suspended: number;
  onLeave: number;
  terminated: number;
  byRole: Record<string, number>;
  byDepartment: Record<string, number>;
}

// ============================================================
// FILTERS
// ============================================================

export interface StaffFilters {
  search?: string;
  role?: StaffRole;
  departmentId?: string;
  status?: StaffStatus;
  page?: number;
  pageSize?: number;
}

export interface InvitationFilters {
  search?: string;
  status?: InvitationStatus;
  role?: StaffRole;
  page?: number;
  pageSize?: number;
}

export interface DepartmentFilters {
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PositionFilters {
  search?: string;
  departmentId?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

// ============================================================
// PAGINATED RESPONSE
// ============================================================

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================================
// REQUEST TYPES
// ============================================================

export interface CreateInvitationRequest {
  email: string;
  role: StaffRole;
  departmentId?: string;
  positionId?: string;
  supervisorId?: string;
  message?: string;
}

export interface UpdateStaffRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  employeeCode?: string;
  departmentId?: string;
  positionId?: string;
  supervisorId?: string;
  role?: StaffRole;
  notes?: string;
}

export interface ChangeStatusRequest {
  status: StaffStatus;
  reason?: string;
}

export interface CreateDepartmentRequest {
  name: string;
  description?: string;
  code?: string;
  parentDepartmentId?: string;
  headId?: string;
}

export interface UpdateDepartmentRequest {
  name?: string;
  description?: string;
  code?: string;
  parentDepartmentId?: string;
  headId?: string;
  isActive?: boolean;
}

export interface CreatePositionRequest {
  title: string;
  description?: string;
  code?: string;
  departmentId: string;
  defaultRole?: StaffRole;
  level: number;
}

export interface UpdatePositionRequest {
  title?: string;
  description?: string;
  code?: string;
  departmentId?: string;
  defaultRole?: StaffRole;
  level?: number;
  isActive?: boolean;
}

export interface ValidateInvitationResponse {
  isValid: boolean;
  email?: string;
  assignedRole?: StaffRole;
  departmentName?: string;
  positionTitle?: string;
  message?: string;
  expiresAt?: string;
}

export interface AcceptInvitationRequest {
  token: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}

// ============================================================
// API FUNCTIONS - STAFF
// ============================================================

/**
 * Get paginated list of staff members
 */
export async function getStaffMembers(
  filters: StaffFilters = {}
): Promise<PaginatedResponse<StaffMember>> {
  const params = new URLSearchParams();
  if (filters.search) params.append('search', filters.search);
  if (filters.role) params.append('role', filters.role);
  if (filters.departmentId) params.append('departmentId', filters.departmentId);
  if (filters.status) params.append('status', filters.status);
  if (filters.page) params.append('page', filters.page.toString());
  if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

  const response = await apiClient.get(`/api/staff?${params.toString()}`);
  return response.data;
}

/**
 * Get staff member by ID
 */
export async function getStaffById(id: string): Promise<StaffMember> {
  const response = await apiClient.get(`/api/staff/${id}`);
  return response.data;
}

/**
 * Get staff statistics/summary
 */
export async function getStaffStats(): Promise<StaffStats> {
  const response = await apiClient.get('/api/staff/summary');
  return response.data;
}

/**
 * Update staff member
 */
export async function updateStaff(id: string, data: UpdateStaffRequest): Promise<StaffMember> {
  const response = await apiClient.put(`/api/staff/${id}`, data);
  return response.data;
}

/**
 * Change staff status (activate, suspend, etc.) - uses POST
 */
export async function changeStaffStatus(
  id: string,
  data: ChangeStatusRequest
): Promise<{ message: string }> {
  const response = await apiClient.post(`/api/staff/${id}/status`, data);
  return response.data;
}

/**
 * Terminate staff member
 */
export async function terminateStaff(id: string, reason: string): Promise<{ message: string }> {
  const response = await apiClient.post(`/api/staff/${id}/terminate`, { reason });
  return response.data;
}

/**
 * Delete staff member (hard delete - SuperAdmin only)
 */
export async function deleteStaff(id: string): Promise<void> {
  await apiClient.delete(`/api/staff/${id}`);
}

// ============================================================
// API FUNCTIONS - INVITATIONS
// ============================================================

/**
 * Get paginated list of invitations
 */
export async function getInvitations(
  filters: InvitationFilters = {}
): Promise<PaginatedResponse<StaffInvitation>> {
  const params = new URLSearchParams();
  if (filters.status) params.append('status', filters.status);
  if (filters.page) params.append('page', filters.page.toString());
  if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

  const response = await apiClient.get(`/api/staff/invitations?${params.toString()}`);
  return response.data;
}

/**
 * Get invitation by ID
 */
export async function getInvitationById(id: string): Promise<StaffInvitation> {
  const response = await apiClient.get(`/api/staff/invitations/${id}`);
  return response.data;
}

/**
 * Create new staff invitation
 */
export async function createInvitation(data: CreateInvitationRequest): Promise<StaffInvitation> {
  const response = await apiClient.post('/api/staff/invitations', data);
  return response.data;
}

/**
 * Resend invitation email
 */
export async function resendInvitation(id: string): Promise<void> {
  await apiClient.post(`/api/staff/invitations/${id}/resend`);
}

/**
 * Revoke/cancel invitation
 */
export async function revokeInvitation(id: string): Promise<{ message: string }> {
  const response = await apiClient.post(`/api/staff/invitations/${id}/revoke`);
  return response.data;
}

/**
 * Validate invitation token (public - no auth required)
 */
export async function validateInvitationToken(token: string): Promise<ValidateInvitationResponse> {
  const response = await apiClient.get(`/api/staff/invitations/validate/${token}`);
  return response.data;
}

/**
 * Accept invitation and create account (public - no auth required)
 */
export async function acceptInvitation(
  data: AcceptInvitationRequest
): Promise<{ staffId: string; message: string }> {
  const response = await apiClient.post('/api/staff/invitations/accept', data);
  return response.data;
}

/**
 * Get pending invitations count
 */
export async function getPendingInvitationsCount(): Promise<number> {
  const response = await apiClient.get('/api/staff/invitations/pending/count');
  return response.data.count;
}

// ============================================================
// API FUNCTIONS - DEPARTMENTS
// ============================================================

/**
 * Get all departments (backend returns array, not paginated)
 */
export async function getDepartments(): Promise<Department[]> {
  const response = await apiClient.get('/api/staff/departments');
  return response.data;
}

/**
 * Get all active departments (for dropdowns)
 */
export async function getActiveDepartments(): Promise<Department[]> {
  const response = await apiClient.get('/api/staff/departments');
  const departments: Department[] = Array.isArray(response.data)
    ? response.data
    : response.data.items || [];
  return departments.filter((d: Department) => d.isActive);
}

/**
 * Get department by ID
 */
export async function getDepartmentById(id: string): Promise<Department> {
  const response = await apiClient.get(`/api/staff/departments/${id}`);
  return response.data;
}

/**
 * Create new department
 */
export async function createDepartment(data: CreateDepartmentRequest): Promise<Department> {
  const response = await apiClient.post('/api/staff/departments', data);
  return response.data;
}

/**
 * Update department
 */
export async function updateDepartment(
  id: string,
  data: UpdateDepartmentRequest
): Promise<Department> {
  const response = await apiClient.put(`/api/staff/departments/${id}`, data);
  return response.data;
}

/**
 * Delete department
 */
export async function deleteDepartment(id: string): Promise<void> {
  await apiClient.delete(`/api/staff/departments/${id}`);
}

// ============================================================
// API FUNCTIONS - POSITIONS
// ============================================================

/**
 * Get all positions (backend returns array, not paginated)
 */
export async function getPositions(): Promise<Position[]> {
  const response = await apiClient.get('/api/staff/positions');
  return response.data;
}

/**
 * Get all active positions (for dropdowns)
 */
export async function getActivePositions(departmentId?: string): Promise<Position[]> {
  const url = departmentId
    ? `/api/staff/positions/department/${departmentId}`
    : '/api/staff/positions';
  const response = await apiClient.get(url);
  const positions: Position[] = Array.isArray(response.data)
    ? response.data
    : response.data.items || [];
  return positions.filter((p: Position) => p.isActive);
}

/**
 * Get position by ID
 */
export async function getPositionById(id: string): Promise<Position> {
  const response = await apiClient.get(`/api/staff/positions/${id}`);
  return response.data;
}

/**
 * Create new position
 */
export async function createPosition(data: CreatePositionRequest): Promise<Position> {
  const response = await apiClient.post('/api/staff/positions', data);
  return response.data;
}

/**
 * Update position
 */
export async function updatePosition(id: string, data: UpdatePositionRequest): Promise<Position> {
  const response = await apiClient.put(`/api/staff/positions/${id}`, data);
  return response.data;
}

/**
 * Delete position
 */
export async function deletePosition(id: string): Promise<void> {
  await apiClient.delete(`/api/staff/positions/${id}`);
}
