/**
 * TanStack Query hooks for NotificationService
 * Sprint 6: Notifications Integration
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import notificationService, {
  type Notification,
  type NotificationPreferences,
} from '@/services/notificationService';

// ============================================================================
// Query Keys
// ============================================================================

export const notificationKeys = {
  all: ['notifications'] as const,
  lists: () => [...notificationKeys.all, 'list'] as const,
  list: (filters: { page?: number; pageSize?: number; unreadOnly?: boolean }) =>
    [...notificationKeys.lists(), filters] as const,
  unreadCount: () => [...notificationKeys.all, 'unread-count'] as const,
  preferences: () => [...notificationKeys.all, 'preferences'] as const,
  stats: () => [...notificationKeys.all, 'stats'] as const,
  templates: () => [...notificationKeys.all, 'templates'] as const,
};

// ============================================================================
// Types
// ============================================================================

export interface NotificationListResponse {
  notifications: Notification[];
  total: number;
  unreadCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface NotificationStats {
  totalSent: number;
  totalRead: number;
  totalUnread: number;
  averageReadTime: number;
  notificationsByType: Record<string, number>;
  notificationsByDay: Array<{ date: string; count: number }>;
}

export interface NotificationTemplate {
  id: string;
  name: string;
  type: Notification['type'];
  title: string;
  message: string;
  variables: string[];
}

// ============================================================================
// Mock Data (fallback when backend is unavailable)
// ============================================================================

const mockNotifications: Notification[] = [
  {
    id: 'notif-1',
    userId: 'user-1',
    type: 'message',
    title: 'New message received',
    message: 'John Seller replied about Tesla Model 3',
    isRead: false,
    createdAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    link: '/messages',
    icon: '💬',
  },
  {
    id: 'notif-2',
    userId: 'user-1',
    type: 'favorite',
    title: 'Someone saved your listing',
    message: 'Your BMW 3 Series was added to favorites by 3 users',
    isRead: false,
    createdAt: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
    icon: '❤️',
  },
  {
    id: 'notif-3',
    userId: 'user-1',
    type: 'approval',
    title: 'Listing approved',
    message: 'Your Toyota Camry listing is now live on the marketplace',
    isRead: true,
    createdAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
    link: '/dealer/listings',
    icon: '✅',
  },
  {
    id: 'notif-4',
    userId: 'user-1',
    type: 'system',
    title: 'Price drop alert',
    message: 'A car in your saved searches dropped in price by $2,000',
    isRead: true,
    createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
    link: '/browse',
    icon: '💰',
  },
  {
    id: 'notif-5',
    userId: 'user-1',
    type: 'sale',
    title: 'Vehicle sold!',
    message: 'Congratulations! Your Honda Accord has been marked as sold.',
    isRead: true,
    createdAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
    icon: '🎉',
  },
];

const mockPreferences: NotificationPreferences = {
  emailNotifications: true,
  pushNotifications: false,
  smsNotifications: false,
  notifyOnMessage: true,
  notifyOnVehicleApproval: true,
  notifyOnVehicleSold: true,
  notifyOnPriceChange: true,
  notifyOnNewFavorite: false,
  notifyOnSystemUpdates: true,
};

const mockStats: NotificationStats = {
  totalSent: 1250,
  totalRead: 980,
  totalUnread: 270,
  averageReadTime: 45,
  notificationsByType: {
    message: 450,
    vehicle: 320,
    system: 280,
    approval: 100,
    sale: 60,
    favorite: 40,
  },
  notificationsByDay: [
    { date: '2026-01-01', count: 45 },
    { date: '2025-12-31', count: 62 },
    { date: '2025-12-30', count: 38 },
    { date: '2025-12-29', count: 55 },
    { date: '2025-12-28', count: 41 },
    { date: '2025-12-27', count: 72 },
    { date: '2025-12-26', count: 49 },
  ],
};

const mockTemplates: NotificationTemplate[] = [
  {
    id: 'tpl-1',
    name: 'Welcome Email',
    type: 'system',
    title: 'Welcome to CarDealer!',
    message: 'Hello {{userName}}, thank you for joining CarDealer!',
    variables: ['userName'],
  },
  {
    id: 'tpl-2',
    name: 'Listing Approved',
    type: 'approval',
    title: 'Your listing is live!',
    message: 'Your {{vehicleName}} listing has been approved and is now visible.',
    variables: ['vehicleName'],
  },
  {
    id: 'tpl-3',
    name: 'New Message',
    type: 'message',
    title: 'New message from {{senderName}}',
    message: '{{senderName}} sent you a message about {{vehicleName}}.',
    variables: ['senderName', 'vehicleName'],
  },
];

// ============================================================================
// Query Hooks
// ============================================================================

/**
 * Get paginated notifications for current user
 */
export function useNotifications(
  page: number = 1,
  pageSize: number = 20,
  unreadOnly: boolean = false
) {
  return useQuery({
    queryKey: notificationKeys.list({ page, pageSize, unreadOnly }),
    queryFn: async (): Promise<NotificationListResponse> => {
      try {
        const data = await notificationService.getNotifications(page, pageSize, unreadOnly);
        return data;
      } catch (error) {
        console.warn('Using mock notifications due to API error:', error);
        const filtered = unreadOnly
          ? mockNotifications.filter((n) => !n.isRead)
          : mockNotifications;
        return {
          notifications: filtered.slice((page - 1) * pageSize, page * pageSize),
          total: filtered.length,
          unreadCount: mockNotifications.filter((n) => !n.isRead).length,
          page,
          pageSize,
          totalPages: Math.ceil(filtered.length / pageSize),
        };
      }
    },
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // Refetch every minute for real-time updates
  });
}

/**
 * Get unread notification count
 */
export function useUnreadCount() {
  return useQuery({
    queryKey: notificationKeys.unreadCount(),
    queryFn: async (): Promise<number> => {
      try {
        return await notificationService.getUnreadCount();
      } catch (error) {
        console.warn('Using mock unread count:', error);
        return mockNotifications.filter((n) => !n.isRead).length;
      }
    },
    staleTime: 15 * 1000, // 15 seconds
    refetchInterval: 30 * 1000, // Refetch every 30 seconds
  });
}

/**
 * Get notification preferences
 */
export function useNotificationPreferences() {
  return useQuery({
    queryKey: notificationKeys.preferences(),
    queryFn: async (): Promise<NotificationPreferences> => {
      try {
        return await notificationService.getPreferences();
      } catch (error) {
        console.warn('Using mock preferences:', error);
        return mockPreferences;
      }
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get notification statistics (admin)
 */
export function useNotificationStats() {
  return useQuery({
    queryKey: notificationKeys.stats(),
    queryFn: async (): Promise<NotificationStats> => {
      try {
        return await notificationService.getNotificationStats();
      } catch (error) {
        console.warn('Using mock stats:', error);
        return mockStats;
      }
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get notification templates (admin)
 */
export function useNotificationTemplates() {
  return useQuery({
    queryKey: notificationKeys.templates(),
    queryFn: async (): Promise<NotificationTemplate[]> => {
      try {
        return await notificationService.getNotificationTemplates();
      } catch (error) {
        console.warn('Using mock templates:', error);
        return mockTemplates;
      }
    },
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

// ============================================================================
// Mutation Hooks
// ============================================================================

/**
 * Mark a single notification as read
 */
export function useMarkAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (notificationId: string) => {
      await notificationService.markAsRead(notificationId);
      return notificationId;
    },
    onMutate: async (notificationId) => {
      // Optimistic update
      await queryClient.cancelQueries({ queryKey: notificationKeys.all });

      const previousData = queryClient.getQueriesData({ queryKey: notificationKeys.lists() });

      queryClient.setQueriesData(
        { queryKey: notificationKeys.lists() },
        (old: NotificationListResponse | undefined) => {
          if (!old) return old;
          return {
            ...old,
            notifications: old.notifications.map((n) =>
              n.id === notificationId ? { ...n, isRead: true } : n
            ),
            unreadCount: Math.max(0, old.unreadCount - 1),
          };
        }
      );

      return { previousData };
    },
    onError: (_error, _notificationId, context) => {
      // Rollback on error
      if (context?.previousData) {
        context.previousData.forEach(([queryKey, data]) => {
          queryClient.setQueryData(queryKey, data);
        });
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });
}

/**
 * Mark all notifications as read
 */
export function useMarkAllAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      await notificationService.markAllAsRead();
    },
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: notificationKeys.all });

      const previousData = queryClient.getQueriesData({ queryKey: notificationKeys.lists() });

      queryClient.setQueriesData(
        { queryKey: notificationKeys.lists() },
        (old: NotificationListResponse | undefined) => {
          if (!old) return old;
          return {
            ...old,
            notifications: old.notifications.map((n) => ({ ...n, isRead: true })),
            unreadCount: 0,
          };
        }
      );

      queryClient.setQueryData(notificationKeys.unreadCount(), 0);

      return { previousData };
    },
    onError: (_error, _variables, context) => {
      if (context?.previousData) {
        context.previousData.forEach(([queryKey, data]) => {
          queryClient.setQueryData(queryKey, data);
        });
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });
}

/**
 * Delete a notification
 */
export function useDeleteNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (notificationId: string) => {
      await notificationService.deleteNotification(notificationId);
      return notificationId;
    },
    onMutate: async (notificationId) => {
      await queryClient.cancelQueries({ queryKey: notificationKeys.all });

      const previousData = queryClient.getQueriesData({ queryKey: notificationKeys.lists() });

      queryClient.setQueriesData(
        { queryKey: notificationKeys.lists() },
        (old: NotificationListResponse | undefined) => {
          if (!old) return old;
          const deletedNotification = old.notifications.find((n) => n.id === notificationId);
          return {
            ...old,
            notifications: old.notifications.filter((n) => n.id !== notificationId),
            total: old.total - 1,
            unreadCount: deletedNotification && !deletedNotification.isRead
              ? old.unreadCount - 1
              : old.unreadCount,
          };
        }
      );

      return { previousData };
    },
    onError: (_error, _notificationId, context) => {
      if (context?.previousData) {
        context.previousData.forEach(([queryKey, data]) => {
          queryClient.setQueryData(queryKey, data);
        });
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });
}

/**
 * Delete all read notifications
 */
export function useDeleteAllRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      await notificationService.deleteAllRead();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });
}

/**
 * Update notification preferences
 */
export function useUpdatePreferences() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (preferences: Partial<NotificationPreferences>) => {
      return await notificationService.updatePreferences(preferences);
    },
    onMutate: async (newPreferences) => {
      await queryClient.cancelQueries({ queryKey: notificationKeys.preferences() });

      const previousPrefs = queryClient.getQueryData<NotificationPreferences>(
        notificationKeys.preferences()
      );

      queryClient.setQueryData(
        notificationKeys.preferences(),
        (old: NotificationPreferences | undefined) => ({
          ...mockPreferences,
          ...old,
          ...newPreferences,
        })
      );

      return { previousPrefs };
    },
    onError: (_error, _variables, context) => {
      if (context?.previousPrefs) {
        queryClient.setQueryData(notificationKeys.preferences(), context.previousPrefs);
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.preferences() });
    },
  });
}

/**
 * Create a notification (admin/system use)
 */
export function useCreateNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      userId,
      notification,
    }: {
      userId: string;
      notification: {
        type: Notification['type'];
        title: string;
        message: string;
        link?: string;
        icon?: string;
        expiresAt?: string;
        metadata?: Record<string, unknown>;
      };
    }) => {
      return await notificationService.createNotification(userId, notification);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });
}

/**
 * Send bulk notifications (admin)
 */
export function useSendBulkNotifications() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      userIds,
      notification,
    }: {
      userIds: string[];
      notification: {
        type: Notification['type'];
        title: string;
        message: string;
        link?: string;
        icon?: string;
      };
    }) => {
      await notificationService.sendBulkNotifications(userIds, notification);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.stats() });
    },
  });
}

/**
 * Send notification from template (admin)
 */
export function useSendFromTemplate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      templateId,
      userIds,
      variables,
    }: {
      templateId: string;
      userIds: string[];
      variables: Record<string, string>;
    }) => {
      await notificationService.sendFromTemplate(templateId, userIds, variables);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.stats() });
    },
  });
}

/**
 * Subscribe to push notifications
 */
export function useSubscribePush() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (subscription: PushSubscription) => {
      await notificationService.subscribePushNotifications(subscription);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.preferences() });
    },
  });
}

/**
 * Unsubscribe from push notifications
 */
export function useUnsubscribePush() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      await notificationService.unsubscribePushNotifications();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.preferences() });
    },
  });
}

/**
 * Send test notification (development)
 */
export function useSendTestNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      await notificationService.sendTestNotification();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });
}

// ============================================================================
// Utility Hooks
// ============================================================================

/**
 * Poll for new notifications (alternative to WebSocket)
 */
export function usePollNotifications(lastFetchTime?: string) {
  return useQuery({
    queryKey: ['notifications', 'poll', lastFetchTime],
    queryFn: async () => {
      try {
        return await notificationService.pollNotifications(lastFetchTime);
      } catch (error) {
        console.warn('Poll notifications error:', error);
        return [];
      }
    },
    enabled: !!lastFetchTime,
    refetchInterval: 30 * 1000, // Poll every 30 seconds
    staleTime: 0,
  });
}

/**
 * Combined hook for notification center
 * Returns notifications, unread count, and common actions
 */
export function useNotificationCenter(page: number = 1, pageSize: number = 20) {
  const notifications = useNotifications(page, pageSize);
  const unreadCount = useUnreadCount();
  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();
  const deleteNotification = useDeleteNotification();
  const deleteAllRead = useDeleteAllRead();

  return {
    // Data
    notifications: notifications.data?.notifications || [],
    total: notifications.data?.total || 0,
    totalPages: notifications.data?.totalPages || 1,
    unreadCount: unreadCount.data || 0,

    // Loading states
    isLoading: notifications.isLoading,
    isRefetching: notifications.isRefetching,

    // Errors
    error: notifications.error,

    // Actions
    markAsRead: markAsRead.mutate,
    markAllAsRead: markAllAsRead.mutate,
    deleteNotification: deleteNotification.mutate,
    deleteAllRead: deleteAllRead.mutate,

    // Action states
    isMarkingAsRead: markAsRead.isPending,
    isMarkingAllAsRead: markAllAsRead.isPending,
    isDeleting: deleteNotification.isPending,

    // Refetch
    refetch: notifications.refetch,
  };
}
