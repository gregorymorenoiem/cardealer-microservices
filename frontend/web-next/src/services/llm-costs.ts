/**
 * LLM Cost Monitoring Service - API client for LLM Gateway cost tracking
 * Connects via API Gateway to the LLM Gateway admin endpoints
 *
 * Endpoints:
 *   GET  /api/admin/llm-gateway/cost              → Full cost breakdown
 *   GET  /api/admin/llm-gateway/distribution       → Model distribution %
 *   GET  /api/admin/llm-gateway/health             → Provider health status
 *   GET  /api/admin/llm-gateway/config             → Gateway configuration
 *   POST /api/admin/llm-gateway/cost/aggressive-mode → Toggle aggressive cache mode
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// TYPES
// ============================================================

export interface CostThresholds {
  warningUsd: number;
  criticalUsd: number;
  aggressiveCacheUsd: number;
}

export interface CostBreakdown {
  month: string;
  monthlyTotalUsd: number;
  dailyTotalUsd: number;
  projectedMonthlyUsd: number;
  thresholds: CostThresholds;
  isAggressiveCacheModeActive: boolean;
  status: string;
  byProvider: Record<string, number>;
  byAgent: Record<string, number>;
  generatedAt: string;
}

export interface ModelDistribution {
  claude: number;
  gemini: number;
  llama: number;
  cache: number;
  totalRequests: number;
  since: string;
  summary: string;
}

export interface ProviderHealth {
  checkedAt: string;
  allHealthy: boolean;
  providers: Record<string, 'Healthy' | 'Unhealthy'>;
}

export interface GatewayConfig {
  providerTimeout: string;
  totalTimeout: string;
  enableCacheFallback: boolean;
  cacheTtl: string;
  forceDegradedMode: boolean;
  claude: {
    enabled: boolean;
    model: string;
    baseUrl: string;
    apiKey: string;
    enablePromptCaching: boolean;
  };
  gemini: { enabled: boolean; model: string; baseUrl: string; apiKey: string };
  llama: { enabled: boolean; model: string; baseUrl: string; apiKey: string };
}

export interface AggressiveModeResponse {
  aggressiveCacheModeActive: boolean;
  message: string;
}

// ============================================================
// SERVICE FUNCTIONS
// ============================================================

/** Get full monthly cost breakdown with per-agent and per-provider detail */
export async function getLlmCostBreakdown(): Promise<CostBreakdown> {
  const response = await apiClient.get<CostBreakdown>('/api/admin/llm-gateway/cost');
  return response.data;
}

/** Get current model distribution (% of requests per provider) */
export async function getLlmModelDistribution(): Promise<ModelDistribution> {
  const response = await apiClient.get<ModelDistribution>('/api/admin/llm-gateway/distribution');
  return response.data;
}

/** Get health status of all LLM providers */
export async function getLlmProviderHealth(): Promise<ProviderHealth> {
  const response = await apiClient.get<ProviderHealth>('/api/admin/llm-gateway/health');
  return response.data;
}

/** Get current gateway configuration (redacted secrets) */
export async function getLlmGatewayConfig(): Promise<GatewayConfig> {
  const response = await apiClient.get<GatewayConfig>('/api/admin/llm-gateway/config');
  return response.data;
}

/** Toggle aggressive cache mode (CTO override) */
export async function toggleAggressiveCacheMode(active: boolean): Promise<AggressiveModeResponse> {
  const response = await apiClient.post<AggressiveModeResponse>(
    '/api/admin/llm-gateway/cost/aggressive-mode',
    { active }
  );
  return response.data;
}
