import axios, { type AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';

const ADMIN_API_URL = import.meta.env.VITE_ADMIN_SERVICE_URL || 'http://localhost:5007/api';

// Create axios instance with interceptors
const adminApi: AxiosInstance = axios.create({
  baseURL: ADMIN_API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add auth token
adminApi.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Add refresh token interceptor for automatic token refresh on 401
addRefreshTokenInterceptor(adminApi);

export interface DashboardStats {
  totalUsers: number;
  totalVehicles: number;
  totalConversations: number;
  pendingApprovals: number;
  activeListings: number;
  soldVehicles: number;
  revenue: number;
  newUsersThisMonth: number;
  newListingsThisMonth: number;
}

export interface ActivityLog {
  id: string;
  userId: string;
  userName: string;
  action: string;
  entityType: 'user' | 'vehicle' | 'conversation' | 'report' | 'system';
  entityId: string;
  details: string;
  ipAddress?: string;
  userAgent?: string;
  timestamp: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  phone?: string;
  avatar?: string;
  role: 'user' | 'admin';
  isVerified: boolean;
  isActive: boolean;
  createdAt: string;
  lastLogin?: string;
  listingsCount: number;
  conversationsCount: number;
}

export interface ReportedContent {
  id: string;
  reportedBy: string;
  reporterName: string;
  reportedUser: string;
  reportedUserName: string;
  contentType: 'vehicle' | 'message' | 'user';
  contentId: string;
  reason: string;
  details?: string;
  status: 'pending' | 'reviewed' | 'resolved' | 'dismissed';
  reviewedBy?: string;
  reviewedAt?: string;
  resolutionNotes?: string;
  createdAt: string;
}

export interface SystemSettings {
  maintenanceMode: boolean;
  allowRegistrations: boolean;
  requireEmailVerification: boolean;
  requireListingApproval: boolean;
  maxListingsPerUser: number;
  maxImagesPerListing: number;
  featuredListingPrice: number;
  commissionRate: number;
}

// Get dashboard statistics
export const getDashboardStats = async (): Promise<DashboardStats> => {
  try {
    const response = await adminApi.get(`/admin/dashboard/stats`);
    return response.data;
  } catch (error) {
    console.error('Error fetching dashboard stats:', error);
    throw new Error('Failed to fetch dashboard statistics');
  }
};

// Get activity logs with pagination
export const getActivityLogs = async (
  page: number = 1,
  pageSize: number = 50,
  filters?: {
    userId?: string;
    entityType?: string;
    startDate?: string;
    endDate?: string;
  }
): Promise<{
  logs: ActivityLog[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}> => {
  try {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value) params.append(key, value.toString());
      });
    }

    const response = await adminApi.get(`/admin/activity-logs?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching activity logs:', error);
    throw new Error('Failed to fetch activity logs');
  }
};

// Get all users with pagination and filters
export const getUsers = async (
  page: number = 1,
  pageSize: number = 20,
  filters?: {
    search?: string;
    role?: string;
    isVerified?: boolean;
    isActive?: boolean;
  }
): Promise<{
  users: User[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}> => {
  try {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString());
        }
      });
    }

    const response = await adminApi.get(`/admin/users?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching users:', error);
    throw new Error('Failed to fetch users');
  }
};

// Get user by ID
export const getUserById = async (id: string): Promise<User> => {
  try {
    const response = await adminApi.get(`/admin/users/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching user:', error);
    throw new Error('Failed to fetch user details');
  }
};

// Update user (admin)
export const updateUser = async (id: string, updates: Partial<User>): Promise<User> => {
  try {
    const response = await adminApi.put(`/admin/users/${id}`, updates);
    return response.data;
  } catch (error) {
    console.error('Error updating user:', error);
    throw new Error('Failed to update user');
  }
};

// Delete user (admin)
export const deleteUser = async (id: string): Promise<void> => {
  try {
    await adminApi.delete(`/admin/users/${id}`);
  } catch (error) {
    console.error('Error deleting user:', error);
    throw new Error('Failed to delete user');
  }
};

// Ban user
export const banUser = async (id: string, reason: string, duration?: number): Promise<void> => {
  try {
    await adminApi.post(`/admin/users/${id}/ban`, { reason, duration });
  } catch (error) {
    console.error('Error banning user:', error);
    throw new Error('Failed to ban user');
  }
};

// Unban user
export const unbanUser = async (id: string): Promise<void> => {
  try {
    await adminApi.post(`/admin/users/${id}/unban`);
  } catch (error) {
    console.error('Error unbanning user:', error);
    throw new Error('Failed to unban user');
  }
};

// Get reported content
export const getReportedContent = async (
  page: number = 1,
  pageSize: number = 20,
  filters?: {
    contentType?: string;
    status?: string;
  }
): Promise<{
  reports: ReportedContent[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}> => {
  try {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value) params.append(key, value.toString());
      });
    }

    const response = await adminApi.get(`/admin/reports?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching reported content:', error);
    throw new Error('Failed to fetch reported content');
  }
};

// Review report
export const reviewReport = async (
  id: string,
  action: 'resolve' | 'dismiss',
  notes?: string
): Promise<ReportedContent> => {
  try {
    const response = await adminApi.post(`/admin/reports/${id}/review`, {
      action,
      notes,
    });
    return response.data;
  } catch (error) {
    console.error('Error reviewing report:', error);
    throw new Error('Failed to review report');
  }
};

// Get system settings
export const getSystemSettings = async (): Promise<SystemSettings> => {
  try {
    const response = await adminApi.get(`/admin/settings`);
    return response.data;
  } catch (error) {
    console.error('Error fetching system settings:', error);
    throw new Error('Failed to fetch system settings');
  }
};

// Update system settings
export const updateSystemSettings = async (
  settings: Partial<SystemSettings>
): Promise<SystemSettings> => {
  try {
    const response = await adminApi.put(`/admin/settings`, settings);
    return response.data;
  } catch (error) {
    console.error('Error updating system settings:', error);
    throw new Error('Failed to update system settings');
  }
};

// Get revenue statistics
export const getRevenueStats = async (
  startDate?: string,
  endDate?: string
): Promise<{
  totalRevenue: number;
  featuredListings: number;
  commissions: number;
  subscriptions: number;
  revenueByMonth: Array<{ month: string; revenue: number }>;
}> => {
  try {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const response = await adminApi.get(`/admin/revenue?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching revenue stats:', error);
    throw new Error('Failed to fetch revenue statistics');
  }
};

// Export data (users, vehicles, etc.)
export const exportData = async (
  dataType: 'users' | 'vehicles' | 'transactions' | 'reports',
  format: 'csv' | 'json' | 'xlsx',
  filters?: Record<string, unknown>
): Promise<Blob> => {
  try {
    const response = await adminApi.post(
      `/admin/export`,
      { dataType, filters },
      {
        params: { format },
        responseType: 'blob',
      }
    );
    return response.data;
  } catch (error) {
    console.error('Error exporting data:', error);
    throw new Error('Failed to export data');
  }
};

// Send system notification to all users
export const sendSystemNotification = async (
  title: string,
  message: string,
  targetUsers?: 'all' | 'verified' | 'admins'
): Promise<void> => {
  try {
    await adminApi.post(`/admin/notifications/send`, {
      title,
      message,
      targetUsers: targetUsers || 'all',
    });
  } catch (error) {
    console.error('Error sending system notification:', error);
    throw new Error('Failed to send system notification');
  }
};

// Get platform statistics
export const getPlatformStats = async (): Promise<{
  userGrowth: Array<{ date: string; count: number }>;
  listingGrowth: Array<{ date: string; count: number }>;
  activeUsers: number;
  averageListingPrice: number;
  mostPopularMake: string;
  conversionRate: number;
}> => {
  try {
    const response = await adminApi.get(`/admin/stats/platform`);
    return response.data;
  } catch (error) {
    console.error('Error fetching platform stats:', error);
    throw new Error('Failed to fetch platform statistics');
  }
};

export default {
  getDashboardStats,
  getActivityLogs,
  getUsers,
  getUserById,
  updateUser,
  deleteUser,
  banUser,
  unbanUser,
  getReportedContent,
  reviewReport,
  getSystemSettings,
  updateSystemSettings,
  getRevenueStats,
  exportData,
  sendSystemNotification,
  getPlatformStats,
};
