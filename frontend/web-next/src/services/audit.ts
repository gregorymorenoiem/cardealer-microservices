/**
 * Audit Service - API client for audit logs management
 * Connects via API Gateway to AuditService (port 15112)
 *
 * Microservices involved:
 * - AuditService: Main audit log storage, querying, and statistics
 * - Gateway: Routes /api/audit/* → AuditService
 */

import { apiClient } from '@/lib/api-client';

// =============================================================================
// TYPES
// =============================================================================

/** Severity levels matching AuditSeverity enum in backend */
export type AuditSeverity = 'Debug' | 'Information' | 'Warning' | 'Error' | 'Critical';

/** Numeric severity values matching C# enum */
export const SEVERITY_VALUES: Record<AuditSeverity, number> = {
  Debug: 1,
  Information: 2,
  Warning: 3,
  Error: 4,
  Critical: 5,
};

/** Audit log entry from the backend */
export interface AuditLog {
  id: string;
  userId: string;
  action: string;
  resource: string;
  userIp: string;
  userAgent: string;
  additionalData: Record<string, unknown>;
  success: boolean;
  errorMessage?: string | null;
  durationMs?: number | null;
  correlationId?: string | null;
  serviceName: string;
  severity: number;
  createdAt: string;
  updatedAt?: string | null;
  // Display properties from DTO
  userDisplayName?: string;
  severityDisplayName?: string;
  severityCssClass?: string;
  actionCategory?: string;
  actionIcon?: string;
  summary?: string;
}

/** Paginated result from the backend */
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  sortBy?: string | null;
  sortDescending: boolean;
}

/** API response wrapper */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  metadata?: Record<string, unknown>;
  timestamp: string;
}

/** Query parameters for fetching audit logs */
export interface AuditLogsQuery {
  userId?: string;
  action?: string;
  resource?: string;
  serviceName?: string;
  severity?: string;
  success?: boolean;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  searchText?: string;
}

/** Stats query parameters */
export interface AuditStatsQuery {
  fromDate?: string;
  toDate?: string;
  serviceName?: string;
  userId?: string;
  action?: string;
}

/** Action frequency from stats */
export interface ActionFrequency {
  action: string;
  count: number;
  successCount: number;
  failureCount: number;
  successRate: number;
  averageDurationMs: number;
}

/** User activity from stats */
export interface UserActivity {
  userId: string;
  userDisplayName: string;
  totalActions: number;
  successfulActions: number;
  failedActions: number;
  successRate: number;
  firstActivity: string;
  lastActivity: string;
  mostFrequentActions: string[];
  averageActionsPerDay: number;
}

/** Service activity from stats */
export interface ServiceActivity {
  serviceName: string;
  totalLogs: number;
  errorCount: number;
  errorRate: number;
  averageDurationMs: number;
  lastActivity: string;
}

/** Audit statistics */
export interface AuditStats {
  totalLogs: number;
  successfulLogs: number;
  failedLogs: number;
  systemLogs: number;
  userLogs: number;
  anonymousLogs: number;
  successRate: number;
  firstLogDate?: string | null;
  lastLogDate?: string | null;
  logsBySeverity: Record<string, number>;
  logsByService: Record<string, number>;
  logsByAction: Record<string, number>;
  logsByResource: Record<string, number>;
  dailyCounts: Record<string, number>;
  hourlyAverages: Record<string, number>;
  averageDurationMs: number;
  maxDurationMs: number;
  minDurationMs: number;
  topActions: ActionFrequency[];
  topUsers: UserActivity[];
  topServices: ServiceActivity[];
  commonErrors: Record<string, number>;
  totalErrorsLast24h: number;
  errorRateTrend: number;
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

/** Get the human-readable name for a severity level */
export function getSeverityName(severity: number): AuditSeverity {
  switch (severity) {
    case 1:
      return 'Debug';
    case 2:
      return 'Information';
    case 3:
      return 'Warning';
    case 4:
      return 'Error';
    case 5:
      return 'Critical';
    default:
      return 'Information';
  }
}

/** Map a backend action string to a friendly category for UI filtering */
export function getActionCategory(action: string): string {
  const a = action.toUpperCase();
  if (
    a.includes('LOGIN') ||
    a.includes('LOGOUT') ||
    a.includes('REGISTER') ||
    a.includes('AUTH') ||
    a.includes('REFRESH_TOKEN')
  )
    return 'auth';
  if (
    a.includes('PASSWORD') ||
    a.includes('2FA') ||
    a.includes('RECOVERY') ||
    a.includes('BRUTE') ||
    a.includes('SUSPICIOUS') ||
    a.includes('ACCOUNT_LOCKED') ||
    a.includes('RATE_LIMIT') ||
    a.includes('INVALID_TOKEN') ||
    a.includes('TOKEN_EXPIRED')
  )
    return 'security';
  if (
    a.includes('ROLE') ||
    a.includes('PERMISSION') ||
    a.includes('CREATE_USER') ||
    a.includes('UPDATE_USER') ||
    a.includes('DELETE_USER') ||
    a.includes('ENABLE_USER') ||
    a.includes('DISABLE_USER') ||
    a.includes('LOCK_USER') ||
    a.includes('UNLOCK_USER')
  )
    return 'admin';
  if (
    a.includes('SYSTEM') ||
    a.includes('BACKUP') ||
    a.includes('RESTORE') ||
    a.includes('CLEANUP') ||
    a.includes('API_CALL')
  )
    return 'system';
  if (
    a.includes('VEHICLE') ||
    a.includes('DEAL') ||
    a.includes('CUSTOMER') ||
    a.includes('APPROVE') ||
    a.includes('REJECT')
  )
    return 'moderation';
  if (
    a.includes('PAYMENT') ||
    a.includes('INVOICE') ||
    a.includes('REFUND') ||
    a.includes('SUBSCRIPTION') ||
    a.includes('BILLING')
  )
    return 'billing';
  if (
    a.includes('NOTIFICATION') ||
    a.includes('SEND_EMAIL') ||
    a.includes('SEND_SMS') ||
    a.includes('SEND_PUSH')
  )
    return 'notifications';
  if (a.includes('KYC') || a.includes('VERIF')) return 'kyc';
  if (a.includes('CONFIG') || a.includes('SETTING') || a.includes('FEATURE'))
    return 'configuration';
  return 'other';
}

// =============================================================================
// API FUNCTIONS
// =============================================================================

/**
 * Fetch paginated audit logs with filtering
 */
export async function getAuditLogs(
  params: AuditLogsQuery = {}
): Promise<ApiResponse<PaginatedResult<AuditLog>>> {
  const queryParams = new URLSearchParams();

  if (params.userId) queryParams.set('userId', params.userId);
  if (params.action) queryParams.set('action', params.action);
  if (params.resource) queryParams.set('resource', params.resource);
  if (params.serviceName) queryParams.set('serviceName', params.serviceName);
  if (params.severity) queryParams.set('severity', params.severity);
  if (params.success !== undefined) queryParams.set('success', String(params.success));
  if (params.fromDate) queryParams.set('fromDate', params.fromDate);
  if (params.toDate) queryParams.set('toDate', params.toDate);
  if (params.page) queryParams.set('page', String(params.page));
  if (params.pageSize) queryParams.set('pageSize', String(params.pageSize));
  if (params.sortBy) queryParams.set('sortBy', params.sortBy);
  if (params.sortDescending !== undefined)
    queryParams.set('sortDescending', String(params.sortDescending));
  if (params.searchText) queryParams.set('searchText', params.searchText);

  const qs = queryParams.toString();
  const url = `/api/audit${qs ? `?${qs}` : ''}`;

  const response = await apiClient.get<ApiResponse<PaginatedResult<AuditLog>>>(url);
  return response.data;
}

/**
 * Fetch a single audit log by ID
 */
export async function getAuditLogById(id: string): Promise<ApiResponse<AuditLog>> {
  const response = await apiClient.get<ApiResponse<AuditLog>>(`/api/audit/${id}`);
  return response.data;
}

/**
 * Fetch audit statistics
 */
export async function getAuditStats(
  params: AuditStatsQuery = {}
): Promise<ApiResponse<AuditStats>> {
  const queryParams = new URLSearchParams();

  if (params.fromDate) queryParams.set('fromDate', params.fromDate);
  if (params.toDate) queryParams.set('toDate', params.toDate);
  if (params.serviceName) queryParams.set('serviceName', params.serviceName);
  if (params.userId) queryParams.set('userId', params.userId);
  if (params.action) queryParams.set('action', params.action);

  const qs = queryParams.toString();
  const url = `/api/audit/stats${qs ? `?${qs}` : ''}`;

  const response = await apiClient.get<ApiResponse<AuditStats>>(url);
  return response.data;
}

/**
 * Fetch audit logs for a specific user
 */
export async function getUserAuditLogs(
  userId: string,
  params: { fromDate?: string; toDate?: string; page?: number; pageSize?: number } = {}
): Promise<ApiResponse<PaginatedResult<AuditLog>>> {
  const queryParams = new URLSearchParams();

  if (params.fromDate) queryParams.set('fromDate', params.fromDate);
  if (params.toDate) queryParams.set('toDate', params.toDate);
  if (params.page) queryParams.set('page', String(params.page));
  if (params.pageSize) queryParams.set('pageSize', String(params.pageSize));

  const qs = queryParams.toString();
  const url = `/api/audit/user/${userId}${qs ? `?${qs}` : ''}`;

  const response = await apiClient.get<ApiResponse<PaginatedResult<AuditLog>>>(url);
  return response.data;
}

/**
 * Export audit logs as CSV
 * Fetches all matching logs and converts to CSV client-side
 */
export async function exportAuditLogs(params: AuditLogsQuery = {}): Promise<string> {
  // Fetch up to 10000 records for export
  const exportParams = { ...params, page: 1, pageSize: 10000 };
  const response = await getAuditLogs(exportParams);

  if (!response.success || !response.data) {
    throw new Error(response.error || 'Error al exportar logs');
  }

  const logs = response.data.items;

  // CSV header
  const headers = [
    'ID',
    'Fecha',
    'Usuario',
    'Acción',
    'Recurso',
    'Servicio',
    'Severidad',
    'Éxito',
    'IP',
    'Duración (ms)',
    'Error',
    'Correlation ID',
  ];

  // CSV rows
  const rows = logs.map(log => [
    log.id,
    new Date(log.createdAt).toLocaleString('es-DO'),
    log.userId,
    log.action,
    log.resource,
    log.serviceName,
    getSeverityName(log.severity),
    log.success ? 'Sí' : 'No',
    log.userIp,
    log.durationMs ?? '',
    log.errorMessage ?? '',
    log.correlationId ?? '',
  ]);

  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.map(cell => `"${String(cell).replace(/"/g, '""')}"`).join(',')),
  ].join('\n');

  return csvContent;
}
