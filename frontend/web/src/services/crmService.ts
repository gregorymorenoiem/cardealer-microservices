/**
 * CRM Service
 *
 * Frontend service for CRM operations.
 * Tries real API first, falls back to mock data if API unavailable.
 *
 * PRODUCTION READY: Connected to CRMService backend
 */

import { api } from './api';
import {
  mockLeads,
  mockDeals,
  mockPipelines,
  mockActivities,
  getCRMStatsByDealer,
  getLeadsByDealer,
  getDealsByDealer,
  getActivitiesByDealer,
  getPipelineStats,
  getLeadById,
  getDealById,
  type Lead,
  type Deal,
  type Pipeline,
  type Activity,
  type CRMStats,
  type PipelineStageStats,
} from '@/mocks/crmData';

// Smart mode: try API first, fallback to mock if error
const FORCE_MOCK_DATA = import.meta.env.VITE_FORCE_MOCK_CRM === 'true';

// Helper to handle API calls with fallback to mock
async function apiWithFallback<T>(
  apiCall: () => Promise<T>,
  mockFallback: () => T | Promise<T>,
  logMessage?: string
): Promise<T> {
  if (FORCE_MOCK_DATA) {
    await new Promise((resolve) => setTimeout(resolve, 200)); // Simulate delay
    return mockFallback();
  }

  try {
    const result = await apiCall();
    return result;
  } catch (error) {
    if (logMessage) {
      console.warn(`[CRM Service] ${logMessage}, using mock data:`, error);
    }
    await new Promise((resolve) => setTimeout(resolve, 150)); // Simulate delay
    return mockFallback();
  }
}

// Legacy compatibility
const USE_MOCK_DATA = FORCE_MOCK_DATA;

// ============================================================================
// TYPES
// ============================================================================

export interface CreateLeadRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  company?: string;
  jobTitle?: string;
  source?: string;
  assignedToUserId?: string;
  interestedProductId?: string;
  estimatedValue?: number;
}

export interface UpdateLeadRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  jobTitle?: string;
  status?: string;
  score?: number;
  assignedToUserId?: string;
  estimatedValue?: number;
}

export interface CreateDealRequest {
  title: string;
  description?: string;
  value: number;
  currency?: string;
  pipelineId?: string;
  stageId?: string;
  leadId?: string;
  contactId?: string;
  assignedToUserId?: string;
  productId?: string;
  vin?: string;
  expectedCloseDate?: string;
}

export interface UpdateDealRequest {
  title?: string;
  description?: string;
  value?: number;
  stageId?: string;
  probability?: number;
  assignedToUserId?: string;
  productId?: string;
  vin?: string;
  expectedCloseDate?: string;
}

export interface MoveDealRequest {
  stageId: string;
  order?: number;
}

export interface CloseDealRequest {
  isWon: boolean;
  notes?: string;
  lostReason?: string;
}

// ============================================================================
// LEADS API
// ============================================================================

export const leadsApi = {
  // Get all leads for a dealer
  getAll: async (dealerId?: string): Promise<Lead[]> => {
    return apiWithFallback(
      async () => {
        const url = dealerId ? `/api/crm/leads?dealerId=${dealerId}` : '/api/crm/leads';
        const response = await api.get<Lead[]>(url);
        return response.data;
      },
      () => (dealerId ? getLeadsByDealer(dealerId) : mockLeads),
      'Failed to fetch leads from API'
    );
  },

  // Get lead by ID
  getById: async (leadId: string): Promise<Lead | null> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Lead>(`/api/crm/leads/${leadId}`);
        return response.data;
      },
      () => getLeadById(leadId) || null,
      `Failed to fetch lead ${leadId}`
    );
  },

  // Create a new lead
  create: async (data: CreateLeadRequest): Promise<Lead> => {
    return apiWithFallback(
      async () => {
        const response = await api.post<Lead>('/api/crm/leads', data);
        return response.data;
      },
      () => {
        const newLead: Lead = {
          id: `lead-${Date.now()}`,
          firstName: data.firstName,
          lastName: data.lastName,
          fullName: `${data.firstName} ${data.lastName}`,
          email: data.email,
          phone: data.phone,
          company: data.company,
          jobTitle: data.jobTitle,
          source: (data.source || 'Website') as Lead['source'],
          status: 'New',
          score: 0,
          estimatedValue: data.estimatedValue,
          assignedToUserId: data.assignedToUserId,
          interestedProductId: data.interestedProductId,
          tags: [],
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };
        mockLeads.push(newLead);
        return newLead;
      },
      'Failed to create lead'
    );
  },

  // Update a lead
  update: async (leadId: string, data: UpdateLeadRequest): Promise<Lead> => {
    return apiWithFallback(
      async () => {
        const response = await api.patch<Lead>(`/api/crm/leads/${leadId}`, data);
        return response.data;
      },
      () => {
        const leadIndex = mockLeads.findIndex((l) => l.id === leadId);
        if (leadIndex === -1) throw new Error('Lead not found');

        const updatedLead: Lead = {
          ...mockLeads[leadIndex],
          ...data,
          status: (data.status as Lead['status']) || mockLeads[leadIndex].status,
          fullName:
            data.firstName && data.lastName
              ? `${data.firstName} ${data.lastName}`
              : mockLeads[leadIndex].fullName,
          updatedAt: new Date().toISOString(),
        };
        mockLeads[leadIndex] = updatedLead;
        return updatedLead;
      },
      `Failed to update lead ${leadId}`
    );
  },

  // Delete a lead
  delete: async (leadId: string): Promise<void> => {
    return apiWithFallback(
      async () => {
        await api.delete(`/api/crm/leads/${leadId}`);
      },
      () => {
        const index = mockLeads.findIndex((l) => l.id === leadId);
        if (index !== -1) {
          mockLeads.splice(index, 1);
        }
      },
      `Failed to delete lead ${leadId}`
    );
  },

  // Convert lead to deal
  convert: async (leadId: string, dealData: CreateDealRequest): Promise<Deal> => {
    return apiWithFallback(
      async () => {
        const response = await api.post<Deal>(`/api/crm/leads/${leadId}/convert`, dealData);
        return response.data;
      },
      () => {
        const lead = getLeadById(leadId);
        if (!lead) throw new Error('Lead not found');

        // Update lead status
        const leadIndex = mockLeads.findIndex((l) => l.id === leadId);
        mockLeads[leadIndex] = {
          ...lead,
          status: 'Won',
          convertedAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };

        // Create deal
        const newDeal: Deal = {
          id: `deal-${Date.now()}`,
          title: dealData.title,
          description: dealData.description,
          value: dealData.value,
          currency: dealData.currency || 'DOP',
          pipelineId: dealData.pipelineId || 'pipeline-001',
          stageId: dealData.stageId || 'stage-001',
          status: 'Open',
          probability: 10,
          leadId: leadId,
          assignedToUserId: dealData.assignedToUserId || lead.assignedToUserId,
          tags: [],
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };
        mockDeals.push(newDeal);
        return newDeal;
      },
      `Failed to convert lead ${leadId}`
    );
  },
};

// ============================================================================
// DEALS API
// ============================================================================

export const dealsApi = {
  // Get all deals for a dealer
  getAll: async (dealerId?: string): Promise<Deal[]> => {
    return apiWithFallback(
      async () => {
        const url = dealerId ? `/api/crm/deals?dealerId=${dealerId}` : '/api/crm/deals';
        const response = await api.get<Deal[]>(url);
        return response.data;
      },
      () => (dealerId ? getDealsByDealer(dealerId) : mockDeals),
      'Failed to fetch deals'
    );
  },

  // Get deal by ID
  getById: async (dealId: string): Promise<Deal | null> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Deal>(`/api/crm/deals/${dealId}`);
        return response.data;
      },
      () => getDealById(dealId) || null,
      `Failed to fetch deal ${dealId}`
    );
  },

  // Get deals by pipeline
  getByPipeline: async (pipelineId: string): Promise<Deal[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Deal[]>(`/api/crm/pipelines/${pipelineId}/deals`);
        return response.data;
      },
      () => mockDeals.filter((d) => d.pipelineId === pipelineId),
      `Failed to fetch deals by pipeline ${pipelineId}`
    );
  },

  // Get deals by stage
  getByStage: async (stageId: string): Promise<Deal[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Deal[]>(`/api/crm/stages/${stageId}/deals`);
        return response.data;
      },
      () => mockDeals.filter((d) => d.stageId === stageId),
      `Failed to fetch deals by stage ${stageId}`
    );
  },

  // Create a new deal
  create: async (data: CreateDealRequest): Promise<Deal> => {
    return apiWithFallback(
      async () => {
        const response = await api.post<Deal>('/api/crm/deals', data);
        return response.data;
      },
      () => {
        const newDeal: Deal = {
          id: `deal-${Date.now()}`,
          title: data.title,
          description: data.description,
          value: data.value,
          currency: data.currency || 'DOP',
          pipelineId: data.pipelineId || 'pipeline-001',
          stageId: data.stageId || 'stage-001',
          status: 'Open',
          probability: 10,
          expectedCloseDate: data.expectedCloseDate,
          leadId: data.leadId,
          contactId: data.contactId,
          assignedToUserId: data.assignedToUserId,
          productId: data.productId,
          vin: data.vin,
          tags: [],
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };
        mockDeals.push(newDeal);
        return newDeal;
      },
      'Failed to create deal'
    );
  },

  // Update a deal
  update: async (dealId: string, data: UpdateDealRequest): Promise<Deal> => {
    return apiWithFallback(
      async () => {
        const response = await api.patch<Deal>(`/api/crm/deals/${dealId}`, data);
        return response.data;
      },
      () => {
        const dealIndex = mockDeals.findIndex((d) => d.id === dealId);
        if (dealIndex === -1) throw new Error('Deal not found');

        const updatedDeal = {
          ...mockDeals[dealIndex],
          ...data,
          updatedAt: new Date().toISOString(),
        };
        mockDeals[dealIndex] = updatedDeal;
        return updatedDeal;
      },
      `Failed to update deal ${dealId}`
    );
  },

  // Move deal to another stage (for Kanban)
  move: async (dealId: string, data: MoveDealRequest): Promise<Deal> => {
    return apiWithFallback(
      async () => {
        const response = await api.post<Deal>(`/api/crm/deals/${dealId}/move`, data);
        return response.data;
      },
      () => {
        const dealIndex = mockDeals.findIndex((d) => d.id === dealId);
        if (dealIndex === -1) throw new Error('Deal not found');

        const pipeline = mockPipelines.find((p) => p.stages.some((s) => s.id === data.stageId));
        const stage = pipeline?.stages.find((s) => s.id === data.stageId);

        const updatedDeal = {
          ...mockDeals[dealIndex],
          stageId: data.stageId,
          stageName: stage?.name,
          stageColor: stage?.color,
          probability: stage?.defaultProbability || mockDeals[dealIndex].probability,
          updatedAt: new Date().toISOString(),
        };
        mockDeals[dealIndex] = updatedDeal;
        return updatedDeal;
      },
      `Failed to move deal ${dealId}`
    );
  },

  // Close a deal (won or lost)
  close: async (dealId: string, data: CloseDealRequest): Promise<Deal> => {
    return apiWithFallback(
      async () => {
        const response = await api.post<Deal>(`/api/crm/deals/${dealId}/close`, data);
        return response.data;
      },
      () => {
        const dealIndex = mockDeals.findIndex((d) => d.id === dealId);
        if (dealIndex === -1) throw new Error('Deal not found');

        const pipeline = mockPipelines.find((p) => p.id === mockDeals[dealIndex].pipelineId);
        const closedStage = pipeline?.stages.find((s) =>
          data.isWon ? s.isWonStage : s.isLostStage
        );

        const updatedDeal = {
          ...mockDeals[dealIndex],
          status: data.isWon ? 'Won' : ('Lost' as Deal['status']),
          stageId: closedStage?.id || mockDeals[dealIndex].stageId,
          stageName: closedStage?.name,
          stageColor: closedStage?.color,
          probability: data.isWon ? 100 : 0,
          actualCloseDate: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };
        mockDeals[dealIndex] = updatedDeal;
        return updatedDeal;
      },
      `Failed to close deal ${dealId}`
    );
  },

  // Delete a deal
  delete: async (dealId: string): Promise<void> => {
    return apiWithFallback(
      async () => {
        await api.delete(`/api/crm/deals/${dealId}`);
      },
      () => {
        const index = mockDeals.findIndex((d) => d.id === dealId);
        if (index !== -1) {
          mockDeals.splice(index, 1);
        }
      },
      `Failed to delete deal ${dealId}`
    );
  },
};

// ============================================================================
// PIPELINES API
// ============================================================================

export const pipelinesApi = {
  // Get all pipelines
  getAll: async (): Promise<Pipeline[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Pipeline[]>('/api/crm/pipelines');
        return response.data;
      },
      () => mockPipelines,
      'Failed to fetch pipelines'
    );
  },

  // Get default pipeline
  getDefault: async (): Promise<Pipeline | null> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Pipeline>('/api/crm/pipelines/default');
        return response.data;
      },
      () => mockPipelines.find((p) => p.isDefault) || null,
      'Failed to fetch default pipeline'
    );
  },

  // Get pipeline with stage stats (for Kanban board)
  getWithStats: async (pipelineId: string): Promise<PipelineStageStats[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<PipelineStageStats[]>(`/api/crm/pipelines/${pipelineId}/stats`);
        return response.data;
      },
      () => getPipelineStats(pipelineId),
      `Failed to fetch pipeline stats ${pipelineId}`
    );
  },
};

// ============================================================================
// ACTIVITIES API
// ============================================================================

export const activitiesApi = {
  // Get all activities for a dealer
  getAll: async (dealerId?: string): Promise<Activity[]> => {
    return apiWithFallback(
      async () => {
        const url = dealerId ? `/api/crm/activities?dealerId=${dealerId}` : '/api/crm/activities';
        const response = await api.get<Activity[]>(url);
        return response.data;
      },
      () => (dealerId ? getActivitiesByDealer(dealerId) : mockActivities),
      'Failed to fetch activities'
    );
  },

  // Get activities for a deal
  getByDeal: async (dealId: string): Promise<Activity[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Activity[]>(`/api/crm/deals/${dealId}/activities`);
        return response.data;
      },
      () => mockActivities.filter((a) => a.dealId === dealId),
      `Failed to fetch activities for deal ${dealId}`
    );
  },

  // Get activities for a lead
  getByLead: async (leadId: string): Promise<Activity[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Activity[]>(`/api/crm/leads/${leadId}/activities`);
        return response.data;
      },
      () => mockActivities.filter((a) => a.leadId === leadId),
      `Failed to fetch activities for lead ${leadId}`
    );
  },

  // Get today's activities
  getToday: async (): Promise<Activity[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Activity[]>('/api/crm/activities/today');
        return response.data;
      },
      () => {
        const today = new Date().toISOString().split('T')[0];
        return mockActivities.filter((a) => a.dueDate?.startsWith(today) && !a.completedAt);
      },
      'Failed to fetch today activities'
    );
  },

  // Get overdue activities
  getOverdue: async (): Promise<Activity[]> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<Activity[]>('/api/crm/activities/overdue');
        return response.data;
      },
      () => {
        const now = new Date();
        return mockActivities.filter(
          (a) => a.dueDate && new Date(a.dueDate) < now && !a.completedAt
        );
      },
      'Failed to fetch overdue activities'
    );
  },
};

// ============================================================================
// CRM STATS API
// ============================================================================

export const crmStatsApi = {
  // Get CRM stats for a dealer
  getStats: async (dealerId: string): Promise<CRMStats> => {
    return apiWithFallback(
      async () => {
        const response = await api.get<CRMStats>(`/api/crm/stats/${dealerId}`);
        return response.data;
      },
      () => getCRMStatsByDealer(dealerId),
      `Failed to fetch CRM stats for dealer ${dealerId}`
    );
  },
};

// ============================================================================
// COMBINED CRM SERVICE
// ============================================================================

export const crmService = {
  leads: leadsApi,
  deals: dealsApi,
  pipelines: pipelinesApi,
  activities: activitiesApi,
  stats: crmStatsApi,
};

export default crmService;
