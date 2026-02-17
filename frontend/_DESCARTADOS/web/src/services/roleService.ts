import axios from 'axios';
import { addRefreshTokenInterceptor } from './api';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const ROLE_API_URL = `${API_BASE_URL}/api/roles`;

// =====================================================
// ENUMS
// =====================================================

export enum RoleType {
  System = 'System',
  Custom = 'Custom',
}

// =====================================================
// DTOs / INTERFACES
// =====================================================

export interface Role {
  id: string;
  name: string;
  displayName: string;
  description?: string;
  isActive: boolean;
  isSystem: boolean;
  userCount?: number;
  permissionCount?: number;
  createdAt: string;
  updatedAt?: string;
}

export interface RoleDetails extends Role {
  permissions: Permission[];
}

export interface Permission {
  id: string;
  name: string;
  displayName: string;
  description?: string;
  module: string;
  resource: string;
  action: string;
  isActive?: boolean;
  createdAt?: string;
}

export interface CreateRoleRequest {
  name: string;
  displayName: string;
  description?: string;
  isActive?: boolean;
  permissionIds?: string[];
}

export interface UpdateRoleRequest {
  displayName?: string;
  description?: string;
  isActive?: boolean;
  permissionIds?: string[];
}

export interface CreatePermissionRequest {
  name: string;
  displayName: string;
  description?: string;
  module: string;
  resource: string;
  action: string;
}

export interface AssignPermissionRequest {
  roleId: string;
  permissionId: string;
}

export interface CheckPermissionRequest {
  roleIds?: string[];
  userId?: string;
  resource: string;
  action: string;
}

export interface CheckPermissionResponse {
  hasPermission: boolean;
  permission: string;
  grantedByRole?: string;
  cached?: boolean;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

// =====================================================
// Axios instance with auth interceptor
// =====================================================

const roleApi = axios.create({
  baseURL: ROLE_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
roleApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Add refresh token interceptor for automatic token renewal
addRefreshTokenInterceptor(roleApi);

// =====================================================
// ROLES API
// =====================================================

/**
 * ROLE-GET-001: Listar roles con paginación
 */
export async function getRoles(params?: {
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}): Promise<PaginatedResult<Role>> {
  const response = await roleApi.get<ApiResponse<PaginatedResult<Role>>>('/roles', { params });
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Error fetching roles');
  }
  return response.data.data;
}

/**
 * ROLE-DTL-001: Obtener rol con permisos
 */
export async function getRoleById(id: string): Promise<RoleDetails> {
  const response = await roleApi.get<ApiResponse<RoleDetails>>(`/roles/${id}`);
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Role not found');
  }
  return response.data.data;
}

/**
 * ROLE-CRT-001: Crear nuevo rol
 */
export async function createRole(request: CreateRoleRequest): Promise<Role> {
  const response = await roleApi.post<ApiResponse<Role>>('/roles', request);
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Error creating role');
  }
  return response.data.data;
}

/**
 * ROLE-UPD-001: Actualizar rol
 */
export async function updateRole(id: string, request: UpdateRoleRequest): Promise<Role> {
  const response = await roleApi.put<ApiResponse<Role>>(`/roles/${id}`, request);
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Error updating role');
  }
  return response.data.data;
}

/**
 * Delete role
 */
export async function deleteRole(id: string): Promise<boolean> {
  const response = await roleApi.delete<ApiResponse<boolean>>(`/roles/${id}`);
  if (!response.data.success) {
    throw new Error(response.data.message || 'Error deleting role');
  }
  return true;
}

// =====================================================
// PERMISSIONS API
// =====================================================

/**
 * PERM-LST-001: Listar permisos
 */
export async function getPermissions(params?: {
  module?: string;
  resource?: string;
}): Promise<Permission[]> {
  const response = await roleApi.get<ApiResponse<Permission[]>>('/permissions', { params });
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Error fetching permissions');
  }
  return response.data.data;
}

/**
 * PERM-CRT-001: Crear permiso
 */
export async function createPermission(request: CreatePermissionRequest): Promise<Permission> {
  const response = await roleApi.post<ApiResponse<Permission>>('/permissions', request);
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Error creating permission');
  }
  return response.data.data;
}

// =====================================================
// ROLE-PERMISSIONS API
// =====================================================

/**
 * RPERM-ASN-001: Asignar permiso a rol
 */
export async function assignPermission(request: AssignPermissionRequest): Promise<boolean> {
  const response = await roleApi.post<ApiResponse<boolean>>('/role-permissions/assign', request);
  if (!response.data.success) {
    throw new Error(response.data.message || 'Error assigning permission');
  }
  return true;
}

/**
 * RPERM-REM-001: Remover permiso de rol
 */
export async function removePermission(request: AssignPermissionRequest): Promise<boolean> {
  const response = await roleApi.post<ApiResponse<boolean>>('/role-permissions/remove', request);
  if (!response.data.success) {
    throw new Error(response.data.message || 'Error removing permission');
  }
  return true;
}

/**
 * RPERM-CHK-001: Verificar permiso
 */
export async function checkPermission(
  request: CheckPermissionRequest
): Promise<CheckPermissionResponse> {
  const response = await roleApi.post<ApiResponse<CheckPermissionResponse>>(
    '/role-permissions/check',
    request
  );
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Error checking permission');
  }
  return response.data.data;
}

// =====================================================
// UTILITY FUNCTIONS
// =====================================================

/**
 * Get available modules for filtering
 */
export function getModules(): string[] {
  return [
    'auth',
    'users',
    'vehicles',
    'dealers',
    'billing',
    'media',
    'notifications',
    'reports',
    'analytics',
    'kyc',
    'aml',
    'compliance',
    'admin',
    'crm',
  ];
}

/**
 * Get available actions for a resource
 */
export function getActions(): string[] {
  return [
    'create',
    'read',
    'update',
    'delete',
    'publish',
    'feature',
    'import',
    'export',
    'ban',
    'verify',
    'approve',
    'reject',
    'assign-roles',
    'manage-roles',
    'manage-permissions',
    'access',
    'view-logs',
    'manage-settings',
  ];
}

/**
 * Get predefined roles
 */
export function getSystemRoles(): { name: string; displayName: string; description: string }[] {
  return [
    {
      name: 'SuperAdmin',
      displayName: 'Super Administrador',
      description: 'Control total del sistema',
    },
    { name: 'Admin', displayName: 'Administrador', description: 'Gestión de la plataforma' },
    {
      name: 'DealerOwner',
      displayName: 'Dueño de Concesionario',
      description: 'Acceso completo a su dealer',
    },
    {
      name: 'DealerEmployee',
      displayName: 'Empleado de Dealer',
      description: 'Gestión de vehículos',
    },
    {
      name: 'Seller',
      displayName: 'Vendedor Individual',
      description: 'Venta de vehículos personales',
    },
    { name: 'Buyer', displayName: 'Comprador', description: 'Búsqueda y compra de vehículos' },
    {
      name: 'ComplianceOfficer',
      displayName: 'Oficial de Compliance',
      description: 'Reportes AML/KYC',
    },
    {
      name: 'CustomerSupport',
      displayName: 'Soporte al Cliente',
      description: 'Gestión de tickets',
    },
  ];
}

/**
 * Format permission name for display
 */
export function formatPermissionName(name: string): string {
  const [resource, action] = name.split(':');
  return `${capitalize(resource)} - ${capitalize(action.replace(/-/g, ' '))}`;
}

function capitalize(str: string): string {
  return str.charAt(0).toUpperCase() + str.slice(1);
}

// =====================================================
// EXPORT SERVICE OBJECT
// =====================================================

const roleService = {
  // Roles
  getRoles,
  getRoleById,
  createRole,
  updateRole,
  deleteRole,
  // Permissions
  getPermissions,
  createPermission,
  // Role-Permissions
  assignPermission,
  removePermission,
  checkPermission,
  // Utilities
  getModules,
  getActions,
  getSystemRoles,
  formatPermissionName,
};

export default roleService;
