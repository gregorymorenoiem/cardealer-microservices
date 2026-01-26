/**
 * Security Session Service
 * Handles API calls for AUTH-SEC-002, AUTH-SEC-003, AUTH-SEC-004
 * Session management with enhanced security features
 */

import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const SECURITY_API_URL = `${API_BASE_URL}/api/auth/security`;

// ============================================================================
// TYPES - Matching backend DTOs from GetActiveSessionsResponse
// ============================================================================

/**
 * Active session DTO matching backend response
 * IP is partially masked for privacy (e.g., 192.168.1.***)
 */
export interface ActiveSessionDto {
  id: string;
  device: string;
  browser: string;
  operatingSystem: string;
  location: string;
  ipAddress: string; // Partially masked (e.g., "192.168.1.***")
  lastActive: string; // ISO 8601 format (mapped from lastActiveAt)
  lastActiveAt?: string; // Original field from backend
  createdAt: string;
  isCurrent: boolean;
  isExpiringSoon: boolean; // True if session expires in less than 1 hour
  expiresAt: string;
}

/**
 * Response from GET /api/auth/security/sessions
 */
export interface GetActiveSessionsResponse {
  success: boolean;
  message: string;
  sessions: ActiveSessionDto[];
  totalCount: number;
  currentSessionId: string | null;
}

/**
 * Response from DELETE /api/auth/security/sessions/{id}
 */
export interface RevokeSessionResponse {
  success: boolean;
  message: string;
  sessionId: string;
  revokedAt: string;
  wasCurrentSession: boolean;
  refreshTokenRevoked: boolean;
  remainingAttempts?: number;
}

/**
 * Response from POST /api/auth/security/sessions/{id}/request-revoke
 */
export interface RequestSessionRevocationResponse {
  success: boolean;
  message: string;
  sessionId?: string;
  codeExpiresAt?: string;
  remainingAttempts?: number;
}

/**
 * Response from POST /api/auth/security/sessions/revoke-all
 */
export interface RevokeAllSessionsResponse {
  success: boolean;
  message: string;
  sessionsRevoked: number;
  refreshTokensRevoked: number;
  currentSessionKept: boolean;
  securityAlertSent: boolean;
  revokedAt: string;
}

// ============================================================================
// API WRAPPER RESPONSE
// ============================================================================

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

// ============================================================================
// SECURITY SESSION SERVICE
// ============================================================================

class SecuritySessionService {
  private getAuthHeaders() {
    const token = localStorage.getItem('accessToken');
    return {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    };
  }

  /**
   * AUTH-SEC-002: Get all active sessions for the current user
   *
   * Features:
   * - IP addresses are partially masked for privacy
   * - Current session is marked
   * - Sessions expiring soon are flagged
   * - XSS sanitized output
   * - Deduplicates sessions from same device/browser
   */
  async getActiveSessions(): Promise<GetActiveSessionsResponse> {
    try {
      const response = await axios.get<ApiResponse<GetActiveSessionsResponse>>(
        `${SECURITY_API_URL}/sessions`,
        { headers: this.getAuthHeaders() }
      );

      let sessions: ActiveSessionDto[] = [];

      if (response.data.success && response.data.data) {
        sessions = response.data.data.sessions || [];
      } else {
        // Fallback for legacy API format
        const data = response.data as unknown as GetActiveSessionsResponse;
        sessions = data.sessions || [];
      }

      // Map lastActiveAt to lastActive (backend uses camelCase but field name differs)
      sessions = sessions.map((session) => ({
        ...session,
        lastActive: session.lastActive || session.lastActiveAt || session.createdAt,
      }));

      // Deduplicate sessions by device + browser + IP (keep most recent)
      const uniqueSessions = this.deduplicateSessions(sessions);

      return {
        success: true,
        message: 'Sessions retrieved successfully',
        sessions: uniqueSessions,
        totalCount: uniqueSessions.length,
        currentSessionId: response.data.data?.currentSessionId || null,
      };
    } catch (error) {
      console.error('Failed to fetch active sessions:', error);
      if (axios.isAxiosError(error) && error.response?.data) {
        throw new Error(error.response.data.error || 'Failed to fetch sessions');
      }
      throw new Error('Failed to fetch active sessions');
    }
  }

  /**
   * Deduplicate sessions by device + browser + IP address.
   * Keeps the most recently active session for each unique combination.
   * This handles cases where multiple sessions exist for the same device.
   */
  private deduplicateSessions(sessions: ActiveSessionDto[]): ActiveSessionDto[] {
    const sessionMap = new Map<string, ActiveSessionDto>();

    for (const session of sessions) {
      // Create a unique key based on device, browser, and IP
      const key = `${session.device}|${session.browser}|${session.ipAddress}`;

      const existing = sessionMap.get(key);
      if (!existing) {
        sessionMap.set(key, session);
      } else {
        // Keep the current session or the most recent one
        if (session.isCurrent) {
          sessionMap.set(key, session);
        } else if (!existing.isCurrent) {
          // Compare by lastActive date
          const existingDate = new Date(existing.lastActive || existing.createdAt);
          const sessionDate = new Date(session.lastActive || session.createdAt);
          if (sessionDate > existingDate) {
            sessionMap.set(key, session);
          }
        }
      }
    }

    // Sort: current session first, then by most recent activity
    return Array.from(sessionMap.values()).sort((a, b) => {
      if (a.isCurrent) return -1;
      if (b.isCurrent) return 1;
      const dateA = new Date(a.lastActive || a.createdAt);
      const dateB = new Date(b.lastActive || b.createdAt);
      return dateB.getTime() - dateA.getTime();
    });
  }

  /**
   * AUTH-SEC-003-A: Request a verification code to revoke a session
   *
   * Security features:
   * - Sends 6-digit code to user's email
   * - Code expires in 5 minutes
   * - Rate limited: max 3 requests per hour
   * - Cannot request for current session
   *
   * @param sessionId - GUID of the session to request revocation for
   */
  async requestSessionRevocation(sessionId: string): Promise<RequestSessionRevocationResponse> {
    try {
      const response = await axios.post<ApiResponse<RequestSessionRevocationResponse>>(
        `${SECURITY_API_URL}/sessions/${sessionId}/request-revoke`,
        null,
        { headers: this.getAuthHeaders() }
      );

      if (response.data.success && response.data.data) {
        return response.data.data;
      }

      return response.data as unknown as RequestSessionRevocationResponse;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 404) {
          throw new Error('Session not found');
        }
        if (error.response?.status === 400) {
          throw new Error(
            error.response.data?.error || error.response.data?.message || 'Request failed'
          );
        }
        throw new Error(error.response?.data?.error || 'Failed to request verification code');
      }
      throw new Error('Failed to request verification code');
    }
  }

  /**
   * AUTH-SEC-003: Revoke a specific session with verification code
   *
   * Security features:
   * - Requires verification code from email
   * - Validates session ownership (prevents IDOR)
   * - Revokes associated refresh token
   * - Sends notification to revoked device
   * - Logs audit trail
   *
   * @param sessionId - GUID of the session to revoke
   * @param code - 6-digit verification code from email
   */
  async revokeSession(sessionId: string, code: string): Promise<RevokeSessionResponse> {
    try {
      const response = await axios.delete<ApiResponse<RevokeSessionResponse>>(
        `${SECURITY_API_URL}/sessions/${sessionId}`,
        {
          headers: this.getAuthHeaders(),
          params: { code },
        }
      );

      if (response.data.success && response.data.data) {
        return response.data.data;
      }

      // Fallback for direct response
      return response.data as unknown as RevokeSessionResponse;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 404) {
          throw new Error('Session not found or already revoked');
        }
        if (error.response?.status === 400) {
          const errorData = error.response.data;
          const message = errorData?.error || errorData?.message || 'Invalid request';
          throw new Error(message);
        }
        throw new Error(error.response?.data?.error || 'Failed to revoke session');
      }
      throw new Error('Failed to revoke session');
    }
  }

  /**
   * AUTH-SEC-004: Revoke all sessions (logout from all devices)
   *
   * Security features:
   * - Option to keep current session active
   * - Revokes all refresh tokens
   * - Sends security alert email to user
   * - Returns count of revoked sessions
   *
   * @param keepCurrentSession - Keep the current session active (default: true)
   */
  async revokeAllSessions(keepCurrentSession: boolean = true): Promise<RevokeAllSessionsResponse> {
    try {
      const response = await axios.post<ApiResponse<RevokeAllSessionsResponse>>(
        `${SECURITY_API_URL}/sessions/revoke-all`,
        null,
        {
          headers: this.getAuthHeaders(),
          params: { keepCurrentSession },
        }
      );

      if (response.data.success && response.data.data) {
        return response.data.data;
      }

      // Fallback for direct response
      return response.data as unknown as RevokeAllSessionsResponse;
    } catch (error) {
      console.error('Failed to revoke all sessions:', error);
      if (axios.isAxiosError(error) && error.response?.data) {
        throw new Error(error.response.data.error || 'Failed to revoke sessions');
      }
      throw new Error('Failed to revoke all sessions');
    }
  }

  // ============================================================================
  // HELPER METHODS
  // ============================================================================

  /**
   * Format relative time for session activity
   * @param dateString - ISO 8601 date string
   */
  formatRelativeTime(dateString: string | undefined | null): string {
    if (!dateString) return 'Recently';

    const date = new Date(dateString);

    // Check for invalid date
    if (isNaN(date.getTime())) {
      return 'Recently';
    }

    const now = new Date();
    const diffMs = now.getTime() - date.getTime();

    // Handle future dates or negative diff
    if (diffMs < 0) return 'Just now';

    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min${diffMins > 1 ? 's' : ''} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;

    return date.toLocaleDateString();
  }

  /**
   * Get device icon name based on device string
   * @param device - Device string from session
   */
  getDeviceType(device: string): 'mobile' | 'tablet' | 'desktop' | 'unknown' {
    const deviceLower = device.toLowerCase();
    if (deviceLower.includes('mobile') || deviceLower.includes('phone')) return 'mobile';
    if (deviceLower.includes('tablet') || deviceLower.includes('ipad')) return 'tablet';
    if (
      deviceLower.includes('desktop') ||
      deviceLower.includes('windows') ||
      deviceLower.includes('mac')
    )
      return 'desktop';
    return 'unknown';
  }

  /**
   * Get browser icon name based on browser string
   * @param browser - Browser string from session
   */
  getBrowserType(browser: string): 'chrome' | 'firefox' | 'safari' | 'edge' | 'other' {
    const browserLower = browser.toLowerCase();
    if (browserLower.includes('chrome')) return 'chrome';
    if (browserLower.includes('firefox')) return 'firefox';
    if (browserLower.includes('safari')) return 'safari';
    if (browserLower.includes('edge')) return 'edge';
    return 'other';
  }

  /**
   * Check if session is suspicious (different location, new device, etc.)
   * This is a frontend heuristic - actual security checks are done on backend
   */
  isSessionSuspicious(session: ActiveSessionDto, allSessions: ActiveSessionDto[]): boolean {
    // If only one session, not suspicious
    if (allSessions.length <= 1) return false;

    // Check if location is different from majority of sessions
    const locations = allSessions.map((s) => s.location);
    const locationCounts = locations.reduce(
      (acc, loc) => {
        acc[loc] = (acc[loc] || 0) + 1;
        return acc;
      },
      {} as Record<string, number>
    );

    const maxLocationCount = Math.max(...Object.values(locationCounts));
    const thisLocationCount = locationCounts[session.location] || 0;

    // If this location appears only once and others appear more, it might be suspicious
    if (thisLocationCount === 1 && maxLocationCount > 2) {
      return true;
    }

    return false;
  }
}

// Export singleton instance
export const securitySessionService = new SecuritySessionService();

// Export types for use in components
export type { ActiveSessionDto as ActiveSession };
