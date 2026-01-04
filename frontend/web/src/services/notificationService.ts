import axios, { type AxiosInstance } from 'axios';

// API Base URL - Use Gateway
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to include auth token
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export interface Notification {
  id: string;
  userId: string;
  type: 'message' | 'vehicle' | 'system' | 'approval' | 'sale' | 'favorite';
  title: string;
  message: string;
  icon?: string;
  link?: string;
  isRead: boolean;
  createdAt: string;
  expiresAt?: string;
  metadata?: Record<string, unknown>;
}

export interface NotificationPreferences {
  emailNotifications: boolean;
  pushNotifications: boolean;
  smsNotifications: boolean;
  notifyOnMessage: boolean;
  notifyOnVehicleApproval: boolean;
  notifyOnVehicleSold: boolean;
  notifyOnPriceChange: boolean;
  notifyOnNewFavorite: boolean;
  notifyOnSystemUpdates: boolean;
}

// Get all notifications for current user
export const getNotifications = async (
  page: number = 1,
  pageSize: number = 20,
  unreadOnly: boolean = false
): Promise<{
  notifications: Notification[];
  total: number;
  unreadCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}> => {
  try {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (unreadOnly) params.append('unreadOnly', 'true');

    const response = await apiClient.get(`/api/notifications?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching notifications:', error);
    throw new Error('Failed to fetch notifications');
  }
};

// Get unread notification count
export const getUnreadCount = async (): Promise<number> => {
  try {
    const response = await apiClient.get(`/api/notifications/unread/count`);
    return response.data.count;
  } catch (error) {
    console.error('Error fetching unread count:', error);
    return 0;
  }
};

// Mark notification as read
export const markAsRead = async (notificationId: string): Promise<void> => {
  try {
    await apiClient.patch(`/api/notifications/${notificationId}/read`);
  } catch (error) {
    console.error('Error marking notification as read:', error);
    throw new Error('Failed to mark notification as read');
  }
};

// Mark all notifications as read
export const markAllAsRead = async (): Promise<void> => {
  try {
    await apiClient.patch(`/api/notifications/read-all`);
  } catch (error) {
    console.error('Error marking all notifications as read:', error);
    throw new Error('Failed to mark all notifications as read');
  }
};

// Delete notification
export const deleteNotification = async (notificationId: string): Promise<void> => {
  try {
    await apiClient.delete(`/api/notifications/${notificationId}`);
  } catch (error) {
    console.error('Error deleting notification:', error);
    throw new Error('Failed to delete notification');
  }
};

// Delete all read notifications
export const deleteAllRead = async (): Promise<void> => {
  try {
    await apiClient.delete(`/api/notifications/read`);
  } catch (error) {
    console.error('Error deleting read notifications:', error);
    throw new Error('Failed to delete read notifications');
  }
};

// Get notification preferences
export const getPreferences = async (): Promise<NotificationPreferences> => {
  try {
    const response = await apiClient.get(`/api/notifications/preferences`);
    return response.data;
  } catch (error) {
    console.error('Error fetching notification preferences:', error);
    throw new Error('Failed to fetch notification preferences');
  }
};

// Update notification preferences
export const updatePreferences = async (
  preferences: Partial<NotificationPreferences>
): Promise<NotificationPreferences> => {
  try {
    const response = await apiClient.put(`/api/notifications/preferences`, preferences);
    return response.data;
  } catch (error) {
    console.error('Error updating notification preferences:', error);
    throw new Error('Failed to update notification preferences');
  }
};

// Create notification (system/admin use)
export const createNotification = async (
  userId: string,
  notification: {
    type: Notification['type'];
    title: string;
    message: string;
    link?: string;
    icon?: string;
    expiresAt?: string;
    metadata?: Record<string, unknown>;
  }
): Promise<Notification> => {
  try {
    const response = await apiClient.post(`/api/notifications`, {
      userId,
      ...notification,
    });
    return response.data;
  } catch (error) {
    console.error('Error creating notification:', error);
    throw new Error('Failed to create notification');
  }
};

// Send notification to multiple users (admin)
export const sendBulkNotifications = async (
  userIds: string[],
  notification: {
    type: Notification['type'];
    title: string;
    message: string;
    link?: string;
    icon?: string;
  }
): Promise<void> => {
  try {
    await apiClient.post(`/api/admin/notifications/bulk`, {
      userIds,
      ...notification,
    });
  } catch (error) {
    console.error('Error sending bulk notifications:', error);
    throw new Error('Failed to send bulk notifications');
  }
};

// Get notification statistics (admin)
export const getNotificationStats = async (): Promise<{
  totalSent: number;
  totalRead: number;
  totalUnread: number;
  averageReadTime: number;
  notificationsByType: Record<string, number>;
  notificationsByDay: Array<{ date: string; count: number }>;
}> => {
  try {
    const response = await apiClient.get(`/api/admin/notifications/stats`);
    return response.data;
  } catch (error) {
    console.error('Error fetching notification stats:', error);
    throw new Error('Failed to fetch notification statistics');
  }
};

// Subscribe to push notifications
export const subscribePushNotifications = async (
  subscription: PushSubscription
): Promise<void> => {
  try {
    await apiClient.post(`/api/notifications/push/subscribe`, {
      subscription: subscription.toJSON(),
    });
  } catch (error) {
    console.error('Error subscribing to push notifications:', error);
    throw new Error('Failed to subscribe to push notifications');
  }
};

// Unsubscribe from push notifications
export const unsubscribePushNotifications = async (): Promise<void> => {
  try {
    await apiClient.post(`/api/notifications/push/unsubscribe`);
  } catch (error) {
    console.error('Error unsubscribing from push notifications:', error);
    throw new Error('Failed to unsubscribe from push notifications');
  }
};

// Test notification (development)
export const sendTestNotification = async (): Promise<void> => {
  try {
    await apiClient.post(`/api/notifications/test`);
  } catch (error) {
    console.error('Error sending test notification:', error);
    throw new Error('Failed to send test notification');
  }
};

// Poll for new notifications (for real-time updates without WebSocket)
export const pollNotifications = async (lastFetchTime?: string): Promise<Notification[]> => {
  try {
    const params = lastFetchTime ? `?since=${encodeURIComponent(lastFetchTime)}` : '';
    const response = await apiClient.get(`/api/notifications/poll${params}`);
    return response.data;
  } catch (error) {
    console.error('Error polling notifications:', error);
    return [];
  }
};

// Get notification templates (admin)
export const getNotificationTemplates = async (): Promise<Array<{
  id: string;
  name: string;
  type: Notification['type'];
  title: string;
  message: string;
  variables: string[];
}>> => {
  try {
    const response = await apiClient.get(`/api/admin/notifications/templates`);
    return response.data;
  } catch (error) {
    console.error('Error fetching notification templates:', error);
    throw new Error('Failed to fetch notification templates');
  }
};

// Send notification from template (admin)
export const sendFromTemplate = async (
  templateId: string,
  userIds: string[],
  variables: Record<string, string>
): Promise<void> => {
  try {
    await apiClient.post(`/api/admin/notifications/send-template`, {
      templateId,
      userIds,
      variables,
    });
  } catch (error) {
    console.error('Error sending notification from template:', error);
    throw new Error('Failed to send notification from template');
  }
};

export default {
  getNotifications,
  getUnreadCount,
  markAsRead,
  markAllAsRead,
  deleteNotification,
  deleteAllRead,
  getPreferences,
  updatePreferences,
  createNotification,
  sendBulkNotifications,
  getNotificationStats,
  subscribePushNotifications,
  unsubscribePushNotifications,
  sendTestNotification,
  pollNotifications,
  getNotificationTemplates,
  sendFromTemplate,
};
