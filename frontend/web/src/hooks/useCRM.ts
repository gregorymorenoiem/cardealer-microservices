/**
 * CRM TanStack Query Hooks
 * 
 * Provides hooks for CRM operations: leads, deals, pipelines, activities.
 * Uses crmService.ts for API calls with mock data fallback.
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { leadsApi, dealsApi, pipelinesApi, activitiesApi, crmStatsApi } from '@/services/crmService';
import type { 
  CreateLeadRequest, 
  UpdateLeadRequest, 
  CreateDealRequest, 
  UpdateDealRequest,
  MoveDealRequest,
  CloseDealRequest 
} from '@/services/crmService';
import type { Lead, Deal, Pipeline, Activity, CRMStats, PipelineStageStats } from '@/mocks/crmData';

// ============================================================================
// QUERY KEYS
// ============================================================================

export const crmKeys = {
  all: ['crm'] as const,
  // Leads
  leads: () => [...crmKeys.all, 'leads'] as const,
  leadsList: (dealerId?: string) => [...crmKeys.leads(), 'list', { dealerId }] as const,
  leadsDetail: (leadId: string) => [...crmKeys.leads(), 'detail', leadId] as const,
  leadsStatus: (status: string) => [...crmKeys.leads(), 'status', status] as const,
  leadsSearch: (query: string) => [...crmKeys.leads(), 'search', query] as const,
  leadsAssigned: (userId: string) => [...crmKeys.leads(), 'assigned', userId] as const,
  leadsRecent: (count: number) => [...crmKeys.leads(), 'recent', count] as const,
  // Deals
  deals: () => [...crmKeys.all, 'deals'] as const,
  dealsList: (dealerId?: string) => [...crmKeys.deals(), 'list', { dealerId }] as const,
  dealsDetail: (dealId: string) => [...crmKeys.deals(), 'detail', dealId] as const,
  dealsPipeline: (pipelineId: string) => [...crmKeys.deals(), 'pipeline', pipelineId] as const,
  dealsStage: (stageId: string) => [...crmKeys.deals(), 'stage', stageId] as const,
  dealsStatus: (status: string) => [...crmKeys.deals(), 'status', status] as const,
  dealsClosingSoon: (days: number) => [...crmKeys.deals(), 'closing-soon', days] as const,
  // Pipelines
  pipelines: () => [...crmKeys.all, 'pipelines'] as const,
  pipelinesList: () => [...crmKeys.pipelines(), 'list'] as const,
  pipelinesDefault: () => [...crmKeys.pipelines(), 'default'] as const,
  pipelinesStats: (pipelineId: string) => [...crmKeys.pipelines(), 'stats', pipelineId] as const,
  // Activities
  activities: () => [...crmKeys.all, 'activities'] as const,
  activitiesList: (dealerId?: string) => [...crmKeys.activities(), 'list', { dealerId }] as const,
  activitiesDeal: (dealId: string) => [...crmKeys.activities(), 'deal', dealId] as const,
  activitiesLead: (leadId: string) => [...crmKeys.activities(), 'lead', leadId] as const,
  activitiesToday: () => [...crmKeys.activities(), 'today'] as const,
  activitiesOverdue: () => [...crmKeys.activities(), 'overdue'] as const,
  // Stats
  statsAll: () => [...crmKeys.all, 'stats'] as const,
  stats: (dealerId: string) => [...crmKeys.all, 'stats', dealerId] as const,
};

// ============================================================================
// LEADS HOOKS
// ============================================================================

export function useLeads(dealerId?: string, options?: Partial<UseQueryOptions<Lead[]>>) {
  return useQuery({
    queryKey: crmKeys.leadsList(dealerId),
    queryFn: () => leadsApi.getAll(dealerId),
    staleTime: 60 * 1000, // 1 minute
    ...options,
  });
}

export function useLead(leadId: string, options?: Partial<UseQueryOptions<Lead | null>>) {
  return useQuery({
    queryKey: crmKeys.leadsDetail(leadId),
    queryFn: () => leadsApi.getById(leadId),
    enabled: !!leadId,
    ...options,
  });
}

export function useLeadsByStatus(status: string, options?: Partial<UseQueryOptions<Lead[]>>) {
  return useQuery({
    queryKey: crmKeys.leadsStatus(status),
    queryFn: async () => {
      const leads = await leadsApi.getAll();
      return leads.filter(l => l.status === status);
    },
    staleTime: 60 * 1000,
    ...options,
  });
}

export function useSearchLeads(query: string, options?: Partial<UseQueryOptions<Lead[]>>) {
  return useQuery({
    queryKey: crmKeys.leadsSearch(query),
    queryFn: async () => {
      const leads = await leadsApi.getAll();
      const lowerQuery = query.toLowerCase();
      return leads.filter(l => 
        l.fullName.toLowerCase().includes(lowerQuery) ||
        l.email.toLowerCase().includes(lowerQuery) ||
        l.company?.toLowerCase().includes(lowerQuery)
      );
    },
    enabled: query.length >= 2,
    ...options,
  });
}

export function useLeadsByAssignedUser(userId: string, options?: Partial<UseQueryOptions<Lead[]>>) {
  return useQuery({
    queryKey: crmKeys.leadsAssigned(userId),
    queryFn: async () => {
      const leads = await leadsApi.getAll();
      return leads.filter(l => l.assignedToUserId === userId);
    },
    enabled: !!userId,
    ...options,
  });
}

export function useRecentLeads(count: number = 10, options?: Partial<UseQueryOptions<Lead[]>>) {
  return useQuery({
    queryKey: crmKeys.leadsRecent(count),
    queryFn: async () => {
      const leads = await leadsApi.getAll();
      return leads
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, count);
    },
    staleTime: 30 * 1000, // 30 seconds
    ...options,
  });
}

export function useCreateLead() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateLeadRequest) => leadsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: crmKeys.leads() });
      queryClient.invalidateQueries({ queryKey: crmKeys.statsAll() });
    },
  });
}

export function useUpdateLead() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ leadId, data }: { leadId: string; data: UpdateLeadRequest }) => 
      leadsApi.update(leadId, data),
    onSuccess: (_, { leadId }) => {
      queryClient.invalidateQueries({ queryKey: crmKeys.leads() });
      queryClient.invalidateQueries({ queryKey: crmKeys.leadsDetail(leadId) });
    },
  });
}

export function useDeleteLead() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (leadId: string) => leadsApi.delete(leadId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: crmKeys.leads() });
      queryClient.invalidateQueries({ queryKey: crmKeys.statsAll() });
    },
  });
}

export function useConvertLead() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ leadId, dealData }: { leadId: string; dealData: CreateDealRequest }) => 
      leadsApi.convert(leadId, dealData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: crmKeys.leads() });
      queryClient.invalidateQueries({ queryKey: crmKeys.deals() });
      queryClient.invalidateQueries({ queryKey: crmKeys.statsAll() });
    },
  });
}

// ============================================================================
// DEALS HOOKS
// ============================================================================

export function useDeals(dealerId?: string, options?: Partial<UseQueryOptions<Deal[]>>) {
  return useQuery({
    queryKey: crmKeys.dealsList(dealerId),
    queryFn: () => dealsApi.getAll(dealerId),
    staleTime: 60 * 1000,
    ...options,
  });
}

export function useDeal(dealId: string, options?: Partial<UseQueryOptions<Deal | null>>) {
  return useQuery({
    queryKey: crmKeys.dealsDetail(dealId),
    queryFn: () => dealsApi.getById(dealId),
    enabled: !!dealId,
    ...options,
  });
}

export function useDealsByPipeline(pipelineId: string, options?: Partial<UseQueryOptions<Deal[]>>) {
  return useQuery({
    queryKey: crmKeys.dealsPipeline(pipelineId),
    queryFn: () => dealsApi.getByPipeline(pipelineId),
    enabled: !!pipelineId,
    staleTime: 30 * 1000,
    ...options,
  });
}

export function useDealsByStage(stageId: string, options?: Partial<UseQueryOptions<Deal[]>>) {
  return useQuery({
    queryKey: crmKeys.dealsStage(stageId),
    queryFn: () => dealsApi.getByStage(stageId),
    enabled: !!stageId,
    ...options,
  });
}

export function useDealsByStatus(status: string, options?: Partial<UseQueryOptions<Deal[]>>) {
  return useQuery({
    queryKey: crmKeys.dealsStatus(status),
    queryFn: async () => {
      const deals = await dealsApi.getAll();
      return deals.filter(d => d.status === status);
    },
    staleTime: 60 * 1000,
    ...options,
  });
}

export function useDealsClosingSoon(days: number = 7, options?: Partial<UseQueryOptions<Deal[]>>) {
  return useQuery({
    queryKey: crmKeys.dealsClosingSoon(days),
    queryFn: async () => {
      const deals = await dealsApi.getAll();
      const threshold = new Date();
      threshold.setDate(threshold.getDate() + days);
      return deals.filter(d => 
        d.status === 'Open' && 
        d.expectedCloseDate && 
        new Date(d.expectedCloseDate) <= threshold
      );
    },
    staleTime: 60 * 1000,
    ...options,
  });
}

export function useCreateDeal() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateDealRequest) => dealsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: crmKeys.deals() });
      queryClient.invalidateQueries({ queryKey: crmKeys.pipelines() });
      queryClient.invalidateQueries({ queryKey: crmKeys.statsAll() });
    },
  });
}

export function useUpdateDeal() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ dealId, data }: { dealId: string; data: UpdateDealRequest }) => 
      dealsApi.update(dealId, data),
    onSuccess: (_, { dealId }) => {
      queryClient.invalidateQueries({ queryKey: crmKeys.deals() });
      queryClient.invalidateQueries({ queryKey: crmKeys.dealsDetail(dealId) });
    },
  });
}

export function useMoveDeal() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ dealId, data }: { dealId: string; data: MoveDealRequest }) => 
      dealsApi.move(dealId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: crmKeys.deals() });
      queryClient.invalidateQueries({ queryKey: crmKeys.pipelines() });
    },
  });
}

export function useCloseDeal() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ dealId, data }: { dealId: string; data: CloseDealRequest }) => 
      dealsApi.close(dealId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: crmKeys.deals() });
      queryClient.invalidateQueries({ queryKey: crmKeys.pipelines() });
      queryClient.invalidateQueries({ queryKey: crmKeys.statsAll() });
    },
  });
}

export function useDeleteDeal() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (dealId: string) => dealsApi.delete(dealId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: crmKeys.deals() });
      queryClient.invalidateQueries({ queryKey: crmKeys.pipelines() });
      queryClient.invalidateQueries({ queryKey: crmKeys.statsAll() });
    },
  });
}

// ============================================================================
// PIPELINES HOOKS
// ============================================================================

export function usePipelines(options?: Partial<UseQueryOptions<Pipeline[]>>) {
  return useQuery({
    queryKey: crmKeys.pipelinesList(),
    queryFn: () => pipelinesApi.getAll(),
    staleTime: 5 * 60 * 1000, // 5 minutes - pipelines rarely change
    ...options,
  });
}

export function useDefaultPipeline(options?: Partial<UseQueryOptions<Pipeline | null>>) {
  return useQuery({
    queryKey: crmKeys.pipelinesDefault(),
    queryFn: () => pipelinesApi.getDefault(),
    staleTime: 5 * 60 * 1000,
    ...options,
  });
}

export function usePipelineStats(pipelineId: string, options?: Partial<UseQueryOptions<PipelineStageStats[]>>) {
  return useQuery({
    queryKey: crmKeys.pipelinesStats(pipelineId),
    queryFn: () => pipelinesApi.getWithStats(pipelineId),
    enabled: !!pipelineId,
    staleTime: 30 * 1000, // 30 seconds - stats change more frequently
    ...options,
  });
}

// ============================================================================
// ACTIVITIES HOOKS
// ============================================================================

export function useActivities(dealerId?: string, options?: Partial<UseQueryOptions<Activity[]>>) {
  return useQuery({
    queryKey: crmKeys.activitiesList(dealerId),
    queryFn: () => activitiesApi.getAll(dealerId),
    staleTime: 60 * 1000,
    ...options,
  });
}

export function useDealActivities(dealId: string, options?: Partial<UseQueryOptions<Activity[]>>) {
  return useQuery({
    queryKey: crmKeys.activitiesDeal(dealId),
    queryFn: () => activitiesApi.getByDeal(dealId),
    enabled: !!dealId,
    ...options,
  });
}

export function useLeadActivities(leadId: string, options?: Partial<UseQueryOptions<Activity[]>>) {
  return useQuery({
    queryKey: crmKeys.activitiesLead(leadId),
    queryFn: () => activitiesApi.getByLead(leadId),
    enabled: !!leadId,
    ...options,
  });
}

export function useTodaysActivities(options?: Partial<UseQueryOptions<Activity[]>>) {
  return useQuery({
    queryKey: crmKeys.activitiesToday(),
    queryFn: () => activitiesApi.getToday(),
    staleTime: 60 * 1000,
    refetchInterval: 5 * 60 * 1000, // Refresh every 5 minutes
    ...options,
  });
}

export function useOverdueActivities(options?: Partial<UseQueryOptions<Activity[]>>) {
  return useQuery({
    queryKey: crmKeys.activitiesOverdue(),
    queryFn: () => activitiesApi.getOverdue(),
    staleTime: 60 * 1000,
    ...options,
  });
}

// ============================================================================
// STATS HOOKS
// ============================================================================

export function useCRMStats(dealerId: string, options?: Partial<UseQueryOptions<CRMStats>>) {
  return useQuery({
    queryKey: crmKeys.stats(dealerId),
    queryFn: () => crmStatsApi.getStats(dealerId),
    enabled: !!dealerId,
    staleTime: 2 * 60 * 1000, // 2 minutes
    ...options,
  });
}

// ============================================================================
// COMPOSITE HOOKS
// ============================================================================

/**
 * Hook for CRM Dashboard with all essential data
 */
export function useCRMDashboard(dealerId: string) {
  const stats = useCRMStats(dealerId);
  const recentLeads = useRecentLeads(5);
  const todaysActivities = useTodaysActivities();
  const overdueActivities = useOverdueActivities();
  const dealsClosingSoon = useDealsClosingSoon(7);
  const defaultPipeline = useDefaultPipeline();

  return {
    stats: stats.data,
    recentLeads: recentLeads.data ?? [],
    todaysActivities: todaysActivities.data ?? [],
    overdueActivities: overdueActivities.data ?? [],
    dealsClosingSoon: dealsClosingSoon.data ?? [],
    defaultPipeline: defaultPipeline.data,
    isLoading: stats.isLoading || recentLeads.isLoading || todaysActivities.isLoading,
    isError: stats.isError || recentLeads.isError || todaysActivities.isError,
    refetchAll: () => {
      stats.refetch();
      recentLeads.refetch();
      todaysActivities.refetch();
      overdueActivities.refetch();
      dealsClosingSoon.refetch();
    },
  };
}

/**
 * Hook for Kanban Board (Pipeline view)
 */
export function useKanbanBoard(pipelineId: string) {
  const pipeline = usePipelines();
  const pipelineStats = usePipelineStats(pipelineId);
  const deals = useDealsByPipeline(pipelineId);
  const moveDeal = useMoveDeal();
  const closeDeal = useCloseDeal();

  const selectedPipeline = pipeline.data?.find(p => p.id === pipelineId);

  return {
    pipeline: selectedPipeline,
    stages: selectedPipeline?.stages ?? [],
    stageStats: pipelineStats.data ?? [],
    deals: deals.data ?? [],
    isLoading: pipeline.isLoading || deals.isLoading,
    isError: pipeline.isError || deals.isError,
    moveDeal: moveDeal.mutate,
    closeDeal: closeDeal.mutate,
    isMoving: moveDeal.isPending,
    isClosing: closeDeal.isPending,
  };
}

/**
 * Hook for Lead Detail page
 */
export function useLeadDetail(leadId: string) {
  const lead = useLead(leadId);
  const activities = useLeadActivities(leadId);
  const updateLead = useUpdateLead();
  const convertLead = useConvertLead();
  const deleteLead = useDeleteLead();

  return {
    lead: lead.data,
    activities: activities.data ?? [],
    isLoading: lead.isLoading || activities.isLoading,
    isError: lead.isError,
    updateLead: (data: UpdateLeadRequest) => updateLead.mutate({ leadId, data }),
    convertLead: (dealData: CreateDealRequest) => convertLead.mutate({ leadId, dealData }),
    deleteLead: () => deleteLead.mutate(leadId),
    isUpdating: updateLead.isPending,
    isConverting: convertLead.isPending,
    isDeleting: deleteLead.isPending,
  };
}

/**
 * Hook for Deal Detail page
 */
export function useDealDetail(dealId: string) {
  const deal = useDeal(dealId);
  const activities = useDealActivities(dealId);
  const updateDeal = useUpdateDeal();
  const moveDeal = useMoveDeal();
  const closeDeal = useCloseDeal();
  const deleteDeal = useDeleteDeal();

  return {
    deal: deal.data,
    activities: activities.data ?? [],
    isLoading: deal.isLoading || activities.isLoading,
    isError: deal.isError,
    updateDeal: (data: UpdateDealRequest) => updateDeal.mutate({ dealId, data }),
    moveDeal: (data: MoveDealRequest) => moveDeal.mutate({ dealId, data }),
    closeDeal: (data: CloseDealRequest) => closeDeal.mutate({ dealId, data }),
    deleteDeal: () => deleteDeal.mutate(dealId),
    isUpdating: updateDeal.isPending,
    isMoving: moveDeal.isPending,
    isClosing: closeDeal.isPending,
    isDeleting: deleteDeal.isPending,
  };
}

export default {
  keys: crmKeys,
  // Leads
  useLeads,
  useLead,
  useLeadsByStatus,
  useSearchLeads,
  useLeadsByAssignedUser,
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
  useDealsByStatus,
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
  // Stats
  useCRMStats,
  // Composite
  useCRMDashboard,
  useKanbanBoard,
  useLeadDetail,
  useDealDetail,
};
