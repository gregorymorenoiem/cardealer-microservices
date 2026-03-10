/**
 * React Query hooks for LLM Cost Monitoring
 * Used by the /admin/costos-llm dashboard page
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getLlmCostBreakdown,
  getLlmModelDistribution,
  getLlmProviderHealth,
  getLlmGatewayConfig,
  toggleAggressiveCacheMode,
} from '@/services/llm-costs';

// ============================================================
// QUERY KEYS
// ============================================================

export const llmCostKeys = {
  all: ['llm-costs'] as const,
  cost: () => [...llmCostKeys.all, 'breakdown'] as const,
  distribution: () => [...llmCostKeys.all, 'distribution'] as const,
  health: () => [...llmCostKeys.all, 'health'] as const,
  config: () => [...llmCostKeys.all, 'config'] as const,
};

// ============================================================
// QUERY HOOKS
// ============================================================

/** Full cost breakdown — refreshes every 30s for near-real-time monitoring */
export function useLlmCostBreakdown() {
  return useQuery({
    queryKey: llmCostKeys.cost(),
    queryFn: getLlmCostBreakdown,
    staleTime: 15_000,
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
  });
}

/** Model distribution % — refreshes every 30s */
export function useLlmModelDistribution() {
  return useQuery({
    queryKey: llmCostKeys.distribution(),
    queryFn: getLlmModelDistribution,
    staleTime: 15_000,
    refetchInterval: 30_000,
  });
}

/** Provider health check — refreshes every 60s */
export function useLlmProviderHealth() {
  return useQuery({
    queryKey: llmCostKeys.health(),
    queryFn: getLlmProviderHealth,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}

/** Gateway configuration — rarely changes, long stale time */
export function useLlmGatewayConfig() {
  return useQuery({
    queryKey: llmCostKeys.config(),
    queryFn: getLlmGatewayConfig,
    staleTime: 300_000, // 5 minutes
  });
}

// ============================================================
// MUTATION HOOKS
// ============================================================

/** Toggle aggressive cache mode — invalidates cost breakdown on success */
export function useToggleAggressiveCacheMode() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (active: boolean) => toggleAggressiveCacheMode(active),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: llmCostKeys.cost() });
      queryClient.invalidateQueries({ queryKey: llmCostKeys.distribution() });
    },
  });
}
