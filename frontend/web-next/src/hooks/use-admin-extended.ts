/**
 * React Query hooks for extended Admin operations
 * Covers: Analytics, Compliance, Content, Billing, System, Support, Reviews, Dealer Detail
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  // Analytics
  getPlatformAnalytics,
  getAnalyticsOverview,
  getWeeklyData,
  getTopVehicleSearches,
  getTrafficSources,
  getDeviceBreakdown,
  getConversionRates,
  getRevenueByChannel,
  exportAnalyticsReport,
  // Compliance
  getAmlAlerts,
  getDgiiReports,
  getComplianceStats,
  updateAmlAlertStatus,
  submitDgiiReport,
  // Content
  getContentOverview,
  getBanners,
  createBanner,
  updateBanner,
  deleteBanner,
  getStaticPages,
  getBlogPosts,
  // Billing
  getRevenueStats,
  getRecentTransactions,
  getPendingPayments,
  getRevenueByPlan,
  // System
  getServicesHealth,
  getDatabasesHealth,
  getInfrastructureHealth,
  getSystemMetrics,
  restartService,
  // Support
  getSupportTickets,
  getSupportTicket,
  replySupportTicket,
  updateTicketStatus,
  assignTicket,
  getTicketStats,
  // Messages
  getAdminMessages,
  markMessageRead,
  replyToMessage,
  // Reviews
  getAdminReviews,
  getReportedReviews,
  approveReview,
  rejectReview,
  deleteReview,
  getAdminReviewStats,
  // Dealer Detail
  getAdminDealerDetail,
  getDealerVehicles,
  getDealerDocuments,
  getDealerBillingHistory,
  // Types
  type TicketFilters,
  type Banner,
} from '@/services/admin-extended';

// =============================================================================
// QUERY KEYS
// =============================================================================

export const adminExtKeys = {
  all: ['admin-ext'] as const,
  // Analytics
  analytics: () => [...adminExtKeys.all, 'analytics'] as const,
  analyticsFull: (period: string) => [...adminExtKeys.analytics(), 'full', period] as const,
  analyticsOverview: (period: string) => [...adminExtKeys.analytics(), 'overview', period] as const,
  weeklyData: (period: string) => [...adminExtKeys.analytics(), 'weekly', period] as const,
  topVehicles: () => [...adminExtKeys.analytics(), 'top-vehicles'] as const,
  trafficSources: (period: string) => [...adminExtKeys.analytics(), 'traffic', period] as const,
  deviceBreakdown: (period: string) => [...adminExtKeys.analytics(), 'devices', period] as const,
  conversions: (period: string) => [...adminExtKeys.analytics(), 'conversions', period] as const,
  revenueByChannel: (period: string) =>
    [...adminExtKeys.analytics(), 'revenue-channels', period] as const,
  // Compliance
  compliance: () => [...adminExtKeys.all, 'compliance'] as const,
  amlAlerts: () => [...adminExtKeys.compliance(), 'aml'] as const,
  dgiiReports: () => [...adminExtKeys.compliance(), 'dgii'] as const,
  complianceStats: () => [...adminExtKeys.compliance(), 'stats'] as const,
  // Content
  content: () => [...adminExtKeys.all, 'content'] as const,
  contentOverview: () => [...adminExtKeys.content(), 'overview'] as const,
  banners: () => [...adminExtKeys.content(), 'banners'] as const,
  staticPages: () => [...adminExtKeys.content(), 'pages'] as const,
  blogPosts: () => [...adminExtKeys.content(), 'blog'] as const,
  // Billing
  billing: () => [...adminExtKeys.all, 'billing'] as const,
  revenueStats: () => [...adminExtKeys.billing(), 'revenue'] as const,
  transactions: () => [...adminExtKeys.billing(), 'transactions'] as const,
  pendingPayments: () => [...adminExtKeys.billing(), 'pending'] as const,
  revenueByPlan: () => [...adminExtKeys.billing(), 'by-plan'] as const,
  // System
  system: () => [...adminExtKeys.all, 'system'] as const,
  servicesHealth: () => [...adminExtKeys.system(), 'services'] as const,
  databasesHealth: () => [...adminExtKeys.system(), 'databases'] as const,
  infraHealth: () => [...adminExtKeys.system(), 'infra'] as const,
  systemMetrics: () => [...adminExtKeys.system(), 'metrics'] as const,
  // Support
  support: () => [...adminExtKeys.all, 'support'] as const,
  tickets: (filters: TicketFilters) => [...adminExtKeys.support(), 'tickets', filters] as const,
  ticket: (id: string) => [...adminExtKeys.support(), 'ticket', id] as const,
  ticketStats: () => [...adminExtKeys.support(), 'stats'] as const,
  // Messages
  messages: () => [...adminExtKeys.all, 'messages'] as const,
  messagesList: (filters: Record<string, string | undefined>) =>
    [...adminExtKeys.messages(), 'list', filters] as const,
  // Reviews
  reviews: () => [...adminExtKeys.all, 'reviews'] as const,
  reviewsList: (filters: Record<string, string | number | undefined>) =>
    [...adminExtKeys.reviews(), 'list', filters] as const,
  reportedReviews: () => [...adminExtKeys.reviews(), 'reported'] as const,
  reviewStats: () => [...adminExtKeys.reviews(), 'stats'] as const,
  // Dealer Detail
  dealerDetail: (id: string) => [...adminExtKeys.all, 'dealer-detail', id] as const,
  dealerVehicles: (id: string) => [...adminExtKeys.all, 'dealer-vehicles', id] as const,
  dealerDocuments: (id: string) => [...adminExtKeys.all, 'dealer-documents', id] as const,
  dealerBilling: (id: string) => [...adminExtKeys.all, 'dealer-billing', id] as const,
};

// =============================================================================
// ANALYTICS HOOKS
// =============================================================================

export function usePlatformAnalytics(period: string = '7d') {
  return useQuery({
    queryKey: adminExtKeys.analyticsFull(period),
    queryFn: () => getPlatformAnalytics(period),
    staleTime: 60_000,
    refetchInterval: 120_000,
  });
}

export function useAnalyticsOverview(period: string = '7d') {
  return useQuery({
    queryKey: adminExtKeys.analyticsOverview(period),
    queryFn: () => getAnalyticsOverview(period),
    staleTime: 60_000,
  });
}

export function useWeeklyData(period: string = '7d') {
  return useQuery({
    queryKey: adminExtKeys.weeklyData(period),
    queryFn: () => getWeeklyData(period),
    staleTime: 60_000,
  });
}

export function useTopVehicleSearches(limit: number = 5) {
  return useQuery({
    queryKey: adminExtKeys.topVehicles(),
    queryFn: () => getTopVehicleSearches(limit),
    staleTime: 5 * 60_000,
  });
}

export function useTrafficSources(period: string = '7d') {
  return useQuery({
    queryKey: adminExtKeys.trafficSources(period),
    queryFn: () => getTrafficSources(period),
    staleTime: 60_000,
  });
}

export function useDeviceBreakdown(period: string = '7d') {
  return useQuery({
    queryKey: adminExtKeys.deviceBreakdown(period),
    queryFn: () => getDeviceBreakdown(period),
    staleTime: 5 * 60_000,
  });
}

export function useConversionRates(period: string = '7d') {
  return useQuery({
    queryKey: adminExtKeys.conversions(period),
    queryFn: () => getConversionRates(period),
    staleTime: 5 * 60_000,
  });
}

export function useRevenueByChannel(period: string = '7d') {
  return useQuery({
    queryKey: adminExtKeys.revenueByChannel(period),
    queryFn: () => getRevenueByChannel(period),
    staleTime: 5 * 60_000,
  });
}

export function useExportAnalyticsReport() {
  return useMutation({
    mutationFn: ({ period, format }: { period: string; format: 'pdf' | 'csv' }) =>
      exportAnalyticsReport(period, format),
  });
}

// =============================================================================
// COMPLIANCE HOOKS
// =============================================================================

export function useAmlAlerts() {
  return useQuery({
    queryKey: adminExtKeys.amlAlerts(),
    queryFn: getAmlAlerts,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}

export function useDgiiReports() {
  return useQuery({
    queryKey: adminExtKeys.dgiiReports(),
    queryFn: getDgiiReports,
    staleTime: 5 * 60_000,
  });
}

export function useComplianceStats() {
  return useQuery({
    queryKey: adminExtKeys.complianceStats(),
    queryFn: getComplianceStats,
    staleTime: 30_000,
  });
}

export function useUpdateAmlAlertStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      status,
    }: {
      id: string;
      status: 'under_investigation' | 'resolved' | 'dismissed';
    }) => updateAmlAlertStatus(id, status),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.compliance() });
    },
  });
}

export function useSubmitDgiiReport() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => submitDgiiReport(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.dgiiReports() });
    },
  });
}

// =============================================================================
// CONTENT HOOKS
// =============================================================================

export function useContentOverview() {
  return useQuery({
    queryKey: adminExtKeys.contentOverview(),
    queryFn: getContentOverview,
    staleTime: 60_000,
  });
}

export function useBanners() {
  return useQuery({
    queryKey: adminExtKeys.banners(),
    queryFn: getBanners,
    staleTime: 60_000,
  });
}

export function useCreateBanner() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Banner>) => createBanner(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.content() });
    },
  });
}

export function useUpdateBanner() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Banner> }) => updateBanner(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.content() });
    },
  });
}

export function useDeleteBanner() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteBanner(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.content() });
    },
  });
}

export function useStaticPages() {
  return useQuery({
    queryKey: adminExtKeys.staticPages(),
    queryFn: getStaticPages,
    staleTime: 60_000,
  });
}

export function useBlogPosts() {
  return useQuery({
    queryKey: adminExtKeys.blogPosts(),
    queryFn: getBlogPosts,
    staleTime: 60_000,
  });
}

// =============================================================================
// BILLING HOOKS
// =============================================================================

export function useRevenueStats() {
  return useQuery({
    queryKey: adminExtKeys.revenueStats(),
    queryFn: getRevenueStats,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}

export function useRecentTransactions(limit: number = 10) {
  return useQuery({
    queryKey: adminExtKeys.transactions(),
    queryFn: () => getRecentTransactions(limit),
    staleTime: 20_000,
    refetchInterval: 30_000,
  });
}

export function usePendingPaymentsAdmin() {
  return useQuery({
    queryKey: adminExtKeys.pendingPayments(),
    queryFn: getPendingPayments,
    staleTime: 30_000,
  });
}

export function useRevenueByPlan() {
  return useQuery({
    queryKey: adminExtKeys.revenueByPlan(),
    queryFn: getRevenueByPlan,
    staleTime: 5 * 60_000,
  });
}

// =============================================================================
// SYSTEM HOOKS
// =============================================================================

export function useServicesHealth() {
  return useQuery({
    queryKey: adminExtKeys.servicesHealth(),
    queryFn: getServicesHealth,
    staleTime: 10_000,
    refetchInterval: 15_000,
  });
}

export function useDatabasesHealth() {
  return useQuery({
    queryKey: adminExtKeys.databasesHealth(),
    queryFn: getDatabasesHealth,
    staleTime: 10_000,
    refetchInterval: 15_000,
  });
}

export function useInfrastructureHealth() {
  return useQuery({
    queryKey: adminExtKeys.infraHealth(),
    queryFn: getInfrastructureHealth,
    staleTime: 10_000,
    refetchInterval: 15_000,
  });
}

export function useSystemMetrics() {
  return useQuery({
    queryKey: adminExtKeys.systemMetrics(),
    queryFn: getSystemMetrics,
    staleTime: 10_000,
    refetchInterval: 15_000,
  });
}

export function useRestartService() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (serviceName: string) => restartService(serviceName),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.system() });
    },
  });
}

// =============================================================================
// SUPPORT HOOKS
// =============================================================================

export function useSupportTickets(filters: TicketFilters = {}) {
  return useQuery({
    queryKey: adminExtKeys.tickets(filters),
    queryFn: () => getSupportTickets(filters),
    staleTime: 15_000,
    refetchInterval: 30_000,
  });
}

export function useSupportTicket(id: string) {
  return useQuery({
    queryKey: adminExtKeys.ticket(id),
    queryFn: () => getSupportTicket(id),
    enabled: !!id,
    staleTime: 10_000,
    refetchInterval: 15_000,
  });
}

export function useReplySupportTicket() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, message }: { id: string; message: string }) =>
      replySupportTicket(id, message),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: adminExtKeys.ticket(id) });
      qc.invalidateQueries({ queryKey: adminExtKeys.support() });
    },
  });
}

export function useUpdateTicketStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: 'in_progress' | 'resolved' | 'closed' }) =>
      updateTicketStatus(id, status),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: adminExtKeys.ticket(id) });
      qc.invalidateQueries({ queryKey: adminExtKeys.support() });
    },
  });
}

export function useAssignTicket() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, agentId }: { id: string; agentId: string }) => assignTicket(id, agentId),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: adminExtKeys.ticket(id) });
    },
  });
}

export function useTicketStats() {
  return useQuery({
    queryKey: adminExtKeys.ticketStats(),
    queryFn: getTicketStats,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}

// =============================================================================
// MESSAGES HOOKS
// =============================================================================

export function useAdminMessages(
  filters: { search?: string; status?: string; priority?: string } = {}
) {
  return useQuery({
    queryKey: adminExtKeys.messagesList(filters),
    queryFn: () => getAdminMessages(filters),
    staleTime: 15_000,
    refetchInterval: 30_000,
  });
}

export function useMarkMessageRead() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => markMessageRead(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.messages() });
    },
  });
}

export function useReplyToMessage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, message }: { id: string; message: string }) => replyToMessage(id, message),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.messages() });
    },
  });
}

// =============================================================================
// REVIEWS HOOKS (Admin)
// =============================================================================

export function useAdminReviews(
  filters: { status?: string; search?: string; page?: number; pageSize?: number } = {}
) {
  return useQuery({
    queryKey: adminExtKeys.reviewsList(filters),
    queryFn: () => getAdminReviews(filters),
    staleTime: 20_000,
    refetchInterval: 30_000,
  });
}

export function useReportedReviews() {
  return useQuery({
    queryKey: adminExtKeys.reportedReviews(),
    queryFn: getReportedReviews,
    staleTime: 20_000,
  });
}

export function useApproveReview() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => approveReview(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.reviews() });
    },
  });
}

export function useRejectReview() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string }) => rejectReview(id, reason),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.reviews() });
    },
  });
}

export function useDeleteReviewAdmin() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteReview(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: adminExtKeys.reviews() });
    },
  });
}

export function useAdminReviewStats() {
  return useQuery({
    queryKey: adminExtKeys.reviewStats(),
    queryFn: getAdminReviewStats,
    staleTime: 30_000,
  });
}

// =============================================================================
// DEALER DETAIL HOOKS (Admin)
// =============================================================================

export function useAdminDealerDetail(id: string) {
  return useQuery({
    queryKey: adminExtKeys.dealerDetail(id),
    queryFn: () => getAdminDealerDetail(id),
    enabled: !!id,
    staleTime: 5 * 60_000,
  });
}

export function useDealerVehiclesAdmin(dealerId: string) {
  return useQuery({
    queryKey: adminExtKeys.dealerVehicles(dealerId),
    queryFn: () => getDealerVehicles(dealerId),
    enabled: !!dealerId,
    staleTime: 60_000,
  });
}

export function useDealerDocumentsAdmin(dealerId: string) {
  return useQuery({
    queryKey: adminExtKeys.dealerDocuments(dealerId),
    queryFn: () => getDealerDocuments(dealerId),
    enabled: !!dealerId,
    staleTime: 60_000,
  });
}

export function useDealerBillingAdmin(dealerId: string) {
  return useQuery({
    queryKey: adminExtKeys.dealerBilling(dealerId),
    queryFn: () => getDealerBillingHistory(dealerId),
    enabled: !!dealerId,
    staleTime: 60_000,
  });
}
