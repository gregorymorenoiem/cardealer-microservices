/**
 * Advertising Service — API client for OKLA Homepage Advertising System
 * Connects via API Gateway to AdvertisingService
 */

import { apiClient } from '@/lib/api-client';
import type {
  AdCampaign,
  AdCampaignSummary,
  CreateCampaignRequest,
  RotationConfig,
  HomepageRotation,
  CategoryImageConfig,
  BrandConfig,
  UpdateCategoryRequest,
  UpdateBrandRequest,
  CampaignReport,
  OwnerReport,
  PlatformReport,
  PricingEstimate,
  RecordImpressionRequest,
  RecordClickRequest,
  AdPlacementType,
  CampaignStatus,
} from '@/types/advertising';
import type { PaginatedResponse } from '@/types';

// ============================================================
// CAMPAIGNS
// ============================================================

export async function createCampaign(data: CreateCampaignRequest): Promise<AdCampaign> {
  const response = await apiClient.post<{ success: boolean; data: AdCampaign }>(
    '/api/advertising/campaigns',
    data
  );
  return response.data.data;
}

export async function getCampaignById(id: string): Promise<AdCampaign> {
  const response = await apiClient.get<{ success: boolean; data: AdCampaign }>(
    `/api/advertising/campaigns/${id}`
  );
  return response.data.data;
}

export async function getCampaignsByOwner(
  ownerId: string,
  ownerType: string = 'Individual',
  status?: CampaignStatus,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<AdCampaignSummary>> {
  const params: Record<string, string | number> = { ownerType, page, pageSize };
  if (status) params.status = status;

  const response = await apiClient.get<{
    success: boolean;
    data: {
      items: AdCampaignSummary[];
      totalCount: number;
      page: number;
      pageSize: number;
    };
  }>(`/api/advertising/campaigns/owner/${ownerId}`, { params });

  const d = response.data.data;
  return {
    items: d.items,
    pagination: {
      page: d.page,
      pageSize: d.pageSize,
      totalItems: d.totalCount,
      totalPages: Math.ceil(d.totalCount / d.pageSize),
      hasNextPage: d.page * d.pageSize < d.totalCount,
      hasPreviousPage: d.page > 1,
    },
  };
}

export async function pauseCampaign(id: string): Promise<void> {
  await apiClient.post(`/api/advertising/campaigns/${id}/pause`);
}

export async function resumeCampaign(id: string): Promise<void> {
  await apiClient.post(`/api/advertising/campaigns/${id}/resume`);
}

export async function cancelCampaign(id: string): Promise<void> {
  await apiClient.post(`/api/advertising/campaigns/${id}/cancel`);
}

// ============================================================
// TRACKING
// ============================================================

export async function recordImpression(data: RecordImpressionRequest): Promise<void> {
  await apiClient.post('/api/advertising/tracking/impression', data);
}

export async function recordClick(data: RecordClickRequest): Promise<void> {
  await apiClient.post('/api/advertising/tracking/click', data);
}

// ============================================================
// ROTATION
// ============================================================

export async function getHomepageRotation(
  section: AdPlacementType
): Promise<HomepageRotation | null> {
  try {
    const response = await apiClient.get<{ success: boolean; data: HomepageRotation }>(
      `/api/advertising/rotation/${section}`
    );
    return response.data.data;
  } catch {
    return null;
  }
}

export async function getRotationConfig(section: AdPlacementType): Promise<RotationConfig | null> {
  try {
    const response = await apiClient.get<{ success: boolean; data: RotationConfig }>(
      `/api/advertising/rotation/config/${section}`
    );
    return response.data.data;
  } catch {
    return null;
  }
}

export async function updateRotationConfig(
  data: Partial<RotationConfig> & { section: AdPlacementType }
): Promise<RotationConfig> {
  const response = await apiClient.put<{ success: boolean; data: RotationConfig }>(
    '/api/advertising/rotation/config',
    data
  );
  return response.data.data;
}

export async function refreshRotation(section?: AdPlacementType): Promise<void> {
  const params = section ? { section } : {};
  await apiClient.post('/api/advertising/rotation/refresh', null, { params });
}

// ============================================================
// HOMEPAGE CONFIG
// ============================================================

export async function getCategories(
  includeHidden: boolean = false
): Promise<CategoryImageConfig[]> {
  const response = await apiClient.get<{ success: boolean; data: CategoryImageConfig[] }>(
    '/api/advertising/homepage/categories',
    { params: { includeHidden } }
  );
  return response.data.data;
}

export async function updateCategory(data: UpdateCategoryRequest): Promise<CategoryImageConfig> {
  const response = await apiClient.put<{ success: boolean; data: CategoryImageConfig }>(
    '/api/advertising/homepage/categories',
    data
  );
  return response.data.data;
}

export async function getBrands(includeHidden: boolean = false): Promise<BrandConfig[]> {
  const response = await apiClient.get<{ success: boolean; data: BrandConfig[] }>(
    '/api/advertising/homepage/brands',
    { params: { includeHidden } }
  );
  return response.data.data;
}

export async function updateBrand(data: UpdateBrandRequest): Promise<BrandConfig> {
  const response = await apiClient.put<{ success: boolean; data: BrandConfig }>(
    '/api/advertising/homepage/brands',
    data
  );
  return response.data.data;
}

// ============================================================
// REPORTS
// ============================================================

export async function getCampaignReport(
  campaignId: string,
  daysBack: number = 30
): Promise<CampaignReport> {
  const response = await apiClient.get<{ success: boolean; data: CampaignReport }>(
    `/api/advertising/reports/campaign/${campaignId}`,
    { params: { daysBack } }
  );
  return response.data.data;
}

export async function getPlatformReport(daysBack: number = 30): Promise<PlatformReport> {
  const response = await apiClient.get<{ success: boolean; data: PlatformReport }>(
    '/api/advertising/reports/platform',
    { params: { daysBack } }
  );
  return response.data.data;
}

export async function getOwnerReport(
  ownerId: string,
  ownerType: string = 'Individual',
  daysBack: number = 30
): Promise<OwnerReport> {
  const response = await apiClient.get<{ success: boolean; data: OwnerReport }>(
    `/api/advertising/reports/owner/${ownerId}`,
    { params: { ownerType, daysBack } }
  );
  return response.data.data;
}

// ============================================================
// PRICING
// ============================================================

export async function getPricingEstimate(placementType: AdPlacementType): Promise<PricingEstimate> {
  const response = await apiClient.get<{ success: boolean; data: PricingEstimate }>(
    `/api/advertising/reports/pricing/${placementType}`
  );
  return response.data.data;
}

// ============================================================
// SERVICE EXPORT
// ============================================================

export const advertisingService = {
  // Campaigns
  createCampaign,
  getCampaignById,
  getCampaignsByOwner,
  pauseCampaign,
  resumeCampaign,
  cancelCampaign,
  // Tracking
  recordImpression,
  recordClick,
  // Rotation
  getHomepageRotation,
  getRotationConfig,
  updateRotationConfig,
  refreshRotation,
  // Homepage config
  getCategories,
  updateCategory,
  getBrands,
  updateBrand,
  // Reports
  getCampaignReport,
  getPlatformReport,
  getOwnerReport,
  // Pricing
  getPricingEstimate,
};

export default advertisingService;
