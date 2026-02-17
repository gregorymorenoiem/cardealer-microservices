/**
 * Maintenance Service - API client for MaintenanceService
 * Manages maintenance mode windows and status checks
 */

import { apiClient } from '@/lib/api-client';

// =============================================================================
// TYPES
// =============================================================================

export interface MaintenanceStatusResponse {
  isMaintenanceMode: boolean;
  maintenanceWindow: MaintenanceWindowDto | null;
}

export interface MaintenanceWindowDto {
  id: string;
  title: string;
  description: string;
  type: string;
  status: string;
  scheduledStart: string;
  scheduledEnd: string;
  actualStart: string | null;
  actualEnd: string | null;
  createdBy: string;
  createdAt: string;
  updatedAt: string | null;
  notes: string | null;
  notifyUsers: boolean;
  notifyMinutesBefore: number;
  affectedServices: string[];
  isActive: boolean;
  isUpcoming: boolean;
}

export interface CreateMaintenanceWindowRequest {
  title: string;
  description: string;
  type: number; // MaintenanceType enum
  scheduledStart: string;
  scheduledEnd: string;
  notifyUsers?: boolean;
  notifyMinutesBefore?: number;
  affectedServices?: string[];
}

export interface CancelMaintenanceRequest {
  reason: string;
}

export interface UpdateScheduleRequest {
  newStart: string;
  newEnd: string;
}

export interface UpdateNotesRequest {
  notes: string;
}

// MaintenanceType enum values
export const MaintenanceType = {
  Scheduled: 1,
  Emergency: 2,
  Database: 3,
  Deployment: 4,
  Infrastructure: 5,
  Other: 99,
} as const;

// MaintenanceStatus enum values
export const MaintenanceStatus = {
  Scheduled: 'Scheduled',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
} as const;

// =============================================================================
// API CLIENT
// =============================================================================

export const maintenanceService = {
  /**
   * Check if the system is currently in maintenance mode (public, no auth required)
   */
  getStatus: async (): Promise<MaintenanceStatusResponse> => {
    const response = await apiClient.get<MaintenanceStatusResponse>('/api/maintenance/status');
    return response.data;
  },

  /**
   * Get all maintenance windows (admin only)
   */
  getAll: async (): Promise<MaintenanceWindowDto[]> => {
    const response = await apiClient.get<MaintenanceWindowDto[]>('/api/maintenance', {
      _silentAuth: true,
    } as Record<string, unknown>);
    return response.data;
  },

  /**
   * Get upcoming maintenance windows (public)
   */
  getUpcoming: async (days: number = 7): Promise<MaintenanceWindowDto[]> => {
    const response = await apiClient.get<MaintenanceWindowDto[]>('/api/maintenance/upcoming', {
      params: { days },
    });
    return response.data;
  },

  /**
   * Get a specific maintenance window by ID (admin only)
   */
  getById: async (id: string): Promise<MaintenanceWindowDto> => {
    const response = await apiClient.get<MaintenanceWindowDto>(`/api/maintenance/${id}`);
    return response.data;
  },

  /**
   * Create a new maintenance window (admin only)
   */
  create: async (request: CreateMaintenanceWindowRequest): Promise<MaintenanceWindowDto> => {
    const response = await apiClient.post<MaintenanceWindowDto>('/api/maintenance', request);
    return response.data;
  },

  /**
   * Start a maintenance window (admin only)
   */
  start: async (id: string): Promise<MaintenanceWindowDto> => {
    const response = await apiClient.post<MaintenanceWindowDto>(`/api/maintenance/${id}/start`);
    return response.data;
  },

  /**
   * Complete a maintenance window (admin only)
   */
  complete: async (id: string): Promise<MaintenanceWindowDto> => {
    const response = await apiClient.post<MaintenanceWindowDto>(`/api/maintenance/${id}/complete`);
    return response.data;
  },

  /**
   * Cancel a maintenance window (admin only)
   */
  cancel: async (id: string, reason: string): Promise<MaintenanceWindowDto> => {
    const response = await apiClient.post<MaintenanceWindowDto>(`/api/maintenance/${id}/cancel`, {
      reason,
    });
    return response.data;
  },

  /**
   * Update maintenance window schedule (admin only)
   */
  updateSchedule: async (
    id: string,
    request: UpdateScheduleRequest
  ): Promise<MaintenanceWindowDto> => {
    const response = await apiClient.put<MaintenanceWindowDto>(
      `/api/maintenance/${id}/schedule`,
      request
    );
    return response.data;
  },

  /**
   * Update maintenance window notes (admin only)
   */
  updateNotes: async (id: string, notes: string): Promise<MaintenanceWindowDto> => {
    const response = await apiClient.put<MaintenanceWindowDto>(`/api/maintenance/${id}/notes`, {
      notes,
    });
    return response.data;
  },

  /**
   * Delete a maintenance window (admin only)
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/maintenance/${id}`);
  },

  /**
   * Quick toggle: Activate maintenance mode immediately
   * Creates an emergency maintenance window and starts it right away
   */
  activateImmediate: async (message: string): Promise<MaintenanceWindowDto> => {
    const now = new Date();
    const endTime = new Date(now.getTime() + 24 * 60 * 60 * 1000); // 24h from now

    const window = await maintenanceService.create({
      title: 'Modo Mantenimiento',
      description: message || 'Estamos realizando mejoras en el sitio. Volveremos pronto.',
      type: MaintenanceType.Emergency,
      scheduledStart: now.toISOString(),
      scheduledEnd: endTime.toISOString(),
      notifyUsers: true,
      notifyMinutesBefore: 0,
      affectedServices: ['all'],
    });

    // Immediately start it
    const started = await maintenanceService.start(window.id);
    return started;
  },

  /**
   * Quick toggle: Deactivate maintenance mode
   * Completes the currently active maintenance window
   */
  deactivateImmediate: async (): Promise<void> => {
    const status = await maintenanceService.getStatus();
    if (status.isMaintenanceMode && status.maintenanceWindow) {
      await maintenanceService.complete(status.maintenanceWindow.id);
    }
  },
};

export default maintenanceService;
