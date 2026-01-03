export { useAuth } from './useAuth';
export { useDebounce } from './useDebounce';
export { useLocalStorage } from './useLocalStorage';
export { usePermissions } from './usePermissions';
export { useDealerFeatures } from './useDealerFeatures';

// Vehicles hooks (TanStack Query)
export {
  useVehiclesList,
  useInfiniteVehicles,
  useVehicle,
  useFeaturedVehicles,
  useCreateVehicle,
  useUpdateVehicle,
  useDeleteVehicle,
} from './useVehicles';

// Media upload hooks (TanStack Query)
export {
  useUploadImage,
  useUploadMultipleImages,
  useUploadProfilePicture,
  useDeleteImage,
  useImageCompression,
  useImageValidation,
  useMediaUpload,
} from './useMediaUpload';

// Billing hooks (TanStack Query)
export {
  usePlans,
  usePlan,
  useComparePlans,
  useSubscription,
  useCreateSubscription,
  useUpgradeSubscription,
  useCancelSubscription,
  useReactivateSubscription,
  useInvoices,
  useInvoice,
  useDownloadInvoice,
  usePayInvoice,
  usePayments,
  usePayment,
  useRefundPayment,
  usePaymentMethods,
  useAddPaymentMethod,
  useSetDefaultPaymentMethod,
  useRemovePaymentMethod,
  useUsageMetrics,
  useBillingStats,
  useBillingDashboard,
  billingKeys,
} from './useBilling';

// Performance hooks for low-bandwidth optimization
export {
  useNetworkStatus,
  useLazyLoad,
  usePrefetch,
  useImagePreloader,
  useReducedMotion,
  useIdleCallback,
  useVirtualList,
  useDebouncedValue,
  useThrottledCallback,
  useProgressiveImage,
} from './usePerformance';

// Notification hooks (TanStack Query)
export {
  notificationKeys,
  useNotifications,
  useUnreadCount,
  useNotificationPreferences,
  useNotificationStats,
  useNotificationTemplates,
  useMarkAsRead,
  useMarkAllAsRead,
  useDeleteNotification,
  useDeleteAllRead,
  useUpdatePreferences,
  useCreateNotification,
  useSendBulkNotifications,
  useSendFromTemplate,
  useSubscribePush,
  useUnsubscribePush,
  useSendTestNotification,
  usePollNotifications,
  useNotificationCenter,
} from './useNotifications';

// CRM hooks (TanStack Query)
export {
  crmKeys,
  // Leads
  useLeads,
  useLead,
  useLeadsByStatus,
  useSearchLeads,
  useRecentLeads,
  useCreateLead,
  useUpdateLead,
  useDeleteLead,
  useConvertLead,
  // Deals
  useDeals,
  useDeal,
  useDealsByPipeline,
  useDealsByStage,
  useDealsClosingSoon,
  useCreateDeal,
  useUpdateDeal,
  useMoveDeal,
  useCloseDeal,
  useDeleteDeal,
  // Pipelines
  usePipelines,
  useDefaultPipeline,
  usePipelineStats,
  // Activities
  useActivities,
  useDealActivities,
  useLeadActivities,
  useTodaysActivities,
  useOverdueActivities,
  useCreateActivity,
  useUpdateActivity,
  useDeleteActivity,
  useCompleteActivity,
  // Stats
  useCRMStats,
  // Composite
  useCRMDashboard,
  useKanbanBoard,
  useLeadDetail,
  useDealDetail,
} from './useCRM';

// Messaging hooks (TanStack Query)
export {
  messagingKeys,
  // Conversations
  useConversations,
  useConversation,
  useSearchConversations,
  useStartConversation,
  useDeleteConversation,
  // Messages
  useMessages,
  useUnreadMessageCount,
  useSendMessage,
  useMarkMessageAsRead,
  useMarkConversationAsRead,
  useDeleteMessage,
  // Blocking
  useBlockedUsers,
  useBlockUser,
  useUnblockUser,
  // Reporting
  useReportConversation,
  // Admin
  useMessageStats,
  useReportedConversations,
  // Typing
  useSendTypingIndicator,
  // Composite
  useMessagesInbox,
  useChatWindow,
  useAdminMessaging,
} from './useMessaging';

// Search & Saved Searches hooks (TanStack Query)
export {
  searchKeys,
  // Vehicle search
  useVehicleSearch,
  useVehicleTextSearch,
  // Saved searches
  useSavedSearches,
  useSavedSearch,
  useSavedSearchResults,
  useCheckSavedSearchResults,
  useCreateSavedSearch,
  useUpdateSavedSearch,
  useDeleteSavedSearch,
  useToggleSavedSearchNotifications,
  // Recent searches
  useRecentSearches,
  useAddRecentSearch,
  useClearRecentSearches,
  getRecentSearches,
  addRecentSearch,
  clearRecentSearches,
  // Autocomplete
  usePopularSearches,
  useSearchSuggestions,
  // Composite
  useSearchPage,
  useSavedSearchesPage,
  useSavedSearchDetail,
  useSearchBar,
} from './useSearch';

// Admin hooks (TanStack Query)
export {
  adminKeys,
  // Dashboard
  useAdminStats,
  usePlatformStats,
  useRevenueStats,
  // Users
  useUsers,
  useUser,
  useUpdateUser,
  useDeleteUser,
  useBanUser,
  useUnbanUser,
  // Activity Logs
  useActivityLogs,
  // Reports/Moderation
  useReportedContent,
  useReviewReport,
  // Settings
  useSystemSettings,
  useUpdateSystemSettings,
  // Export & Notifications
  useExportData,
  useSendSystemNotification,
  // Pending Approvals
  usePendingApprovals,
  useApproveVehicle,
  useRejectVehicle,
  usePendingApprovalsPage,
  // Composite
  useAdminDashboard,
  useUsersManagement,
  useModerationPage,
  useSettingsPage,
} from './useAdmin';
