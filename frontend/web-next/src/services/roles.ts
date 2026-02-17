/**
 * Roles Service - API client for RBAC RoleService
 * Connects via API Gateway to RoleService
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// TYPES
// ============================================================

/** API wrapper response from the backend */
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

/** Paginated result from the backend */
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// --- Roles ---

export interface RoleListItem {
  id: string;
  name: string;
  displayName: string;
  isActive: boolean;
  isSystemRole: boolean;
  userCount: number;
  permissionCount: number;
  createdAt: string;
}

export interface PermissionDto {
  id: string;
  name: string;
  displayName: string;
  description: string;
  resource: string;
  action: string;
  module: string;
}

export interface RoleDetails {
  id: string;
  name: string;
  displayName: string;
  description: string | null;
  isActive: boolean;
  isSystemRole: boolean;
  createdAt: string;
  updatedAt: string | null;
  createdBy: string;
  updatedBy: string | null;
  permissions: PermissionDto[];
}

export interface CreateRoleRequest {
  name: string;
  displayName: string;
  description?: string;
  isActive?: boolean;
  permissionIds?: string[];
}

export interface CreateRoleResponse {
  success: boolean;
  data: {
    id: string;
    name: string;
    displayName: string;
    description: string | null;
    isActive: boolean;
    permissionCount: number;
    createdAt: string;
  };
}

export interface UpdateRoleRequest {
  displayName?: string;
  description?: string;
  isActive?: boolean;
  permissionIds?: string[];
}

export interface UpdateRoleResponse {
  success: boolean;
  data: {
    id: string;
    name: string;
    displayName: string;
    description: string | null;
    isActive: boolean;
    isSystemRole: boolean;
    permissionCount: number;
    updatedAt: string;
  };
}

// --- Permissions ---

export interface PermissionListItem {
  id: string;
  name: string;
  displayName: string;
  module: string;
  isActive: boolean;
  isSystemPermission: boolean;
}

export interface CreatePermissionRequest {
  name: string;
  displayName: string;
  description?: string;
  resource: string;
  action: string;
  module: string;
}

// --- Role-Permissions ---

export interface AssignPermissionRequest {
  roleId: string;
  permissionId: string;
}

export interface AssignPermissionResponse {
  success: boolean;
  roleId: string;
  roleName: string;
  permissionId: string;
  permissionName: string;
  assignedAt: string;
  assignedBy: string;
}

export interface RemovePermissionResponse {
  success: boolean;
  roleId: string;
  roleName: string;
  permissionId: string;
  permissionName: string;
  removedAt: string;
  removedBy: string;
}

// ============================================================
// API FUNCTIONS
// ============================================================

// --- Roles ---

/**
 * Get all roles with pagination
 */
export async function getRoles(params?: {
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}): Promise<PaginatedResult<RoleListItem>> {
  const response = await apiClient.get<ApiResponse<PaginatedResult<RoleListItem>>>('/api/roles', {
    params,
  });
  return response.data.data;
}

/**
 * Get a single role by ID with all its permissions
 */
export async function getRoleById(id: string): Promise<RoleDetails> {
  const response = await apiClient.get<ApiResponse<RoleDetails>>(`/api/roles/${id}`);
  return response.data.data;
}

/**
 * Create a new role
 */
export async function createRole(request: CreateRoleRequest): Promise<CreateRoleResponse> {
  const response = await apiClient.post<ApiResponse<CreateRoleResponse>>('/api/roles', request);
  return response.data.data;
}

/**
 * Update an existing role
 */
export async function updateRole(
  id: string,
  request: UpdateRoleRequest
): Promise<UpdateRoleResponse> {
  const response = await apiClient.put<ApiResponse<UpdateRoleResponse>>(
    `/api/roles/${id}`,
    request
  );
  return response.data.data;
}

/**
 * Delete a role (only non-system roles)
 */
export async function deleteRole(id: string): Promise<boolean> {
  const response = await apiClient.delete<ApiResponse<boolean>>(`/api/roles/${id}`);
  return response.data.data;
}

// --- Permissions ---

/**
 * Get all permissions, optionally filtered by module
 */
export async function getPermissions(params?: {
  module?: string;
  resource?: string;
}): Promise<PermissionListItem[]> {
  const response = await apiClient.get<ApiResponse<PermissionListItem[]>>('/api/permissions', {
    params,
  });
  return response.data.data;
}

/**
 * Get all allowed modules
 */
export async function getAllowedModules(): Promise<string[]> {
  const response = await apiClient.get<ApiResponse<string[]>>('/api/permissions/modules');
  return response.data.data;
}

/**
 * Create a new permission
 */
export async function createPermission(
  request: CreatePermissionRequest
): Promise<{ success: boolean; data: { id: string } }> {
  const response = await apiClient.post<ApiResponse<{ success: boolean; data: { id: string } }>>(
    '/api/permissions',
    request
  );
  return response.data.data;
}

// --- Role-Permissions ---

/**
 * Assign a permission to a role
 */
export async function assignPermission(
  request: AssignPermissionRequest
): Promise<AssignPermissionResponse> {
  const response = await apiClient.post<ApiResponse<AssignPermissionResponse>>(
    '/api/role-permissions/assign',
    request
  );
  return response.data.data;
}

/**
 * Remove a permission from a role
 */
export async function removePermission(
  request: AssignPermissionRequest
): Promise<RemovePermissionResponse> {
  const response = await apiClient.post<ApiResponse<RemovePermissionResponse>>(
    '/api/role-permissions/remove',
    request
  );
  return response.data.data;
}

// ============================================================
// CONVENIENCE / AGGREGATED
// ============================================================

/**
 * Group permissions by module for UI display
 */
export function groupPermissionsByModule(
  permissions: PermissionDto[] | PermissionListItem[]
): Record<string, (PermissionDto | PermissionListItem)[]> {
  return permissions.reduce(
    (groups, perm) => {
      const moduleName = perm.module || 'other';
      if (!groups[moduleName]) {
        groups[moduleName] = [];
      }
      groups[moduleName].push(perm);
      return groups;
    },
    {} as Record<string, (PermissionDto | PermissionListItem)[]>
  );
}

/** Module display names in Spanish */
export const MODULE_LABELS: Record<string, string> = {
  users: 'Usuarios',
  vehicles: 'Vehículos',
  dealers: 'Dealers',
  analytics: 'Reportes y Analytics',
  admin: 'Configuración y Roles',
  media: 'Media',
  support: 'Soporte',
  auth: 'Autenticación',
  billing: 'Facturación',
  notifications: 'Notificaciones',
  kyc: 'Verificación KYC',
  compliance: 'Compliance',
  crm: 'CRM',
};
