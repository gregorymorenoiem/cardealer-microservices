/**
 * CRM Service
 * 
 * Frontend service for CRM operations.
 * Uses mock data when USE_MOCK_AUTH is true.
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

const USE_MOCK_DATA = import.meta.env.VITE_USE_MOCK_AUTH !== 'false';

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
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300)); // Simulate network delay
      if (dealerId) {
        return getLeadsByDealer(dealerId);
      }
      return mockLeads;
    }
    const response = await api.get<Lead[]>('/crm/leads');
    return response.data;
  },

  // Get lead by ID
  getById: async (leadId: string): Promise<Lead | null> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return getLeadById(leadId) || null;
    }
    const response = await api.get<Lead>(`/crm/leads/${leadId}`);
    return response.data;
  },

  // Create a new lead
  create: async (data: CreateLeadRequest): Promise<Lead> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 500));
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
    }
    const response = await api.post<Lead>('/crm/leads', data);
    return response.data;
  },

  // Update a lead
  update: async (leadId: string, data: UpdateLeadRequest): Promise<Lead> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 400));
      const leadIndex = mockLeads.findIndex(l => l.id === leadId);
      if (leadIndex === -1) throw new Error('Lead not found');
      
      const updatedLead: Lead = {
        ...mockLeads[leadIndex],
        ...data,
        status: (data.status as Lead['status']) || mockLeads[leadIndex].status,
        fullName: data.firstName && data.lastName 
          ? `${data.firstName} ${data.lastName}` 
          : mockLeads[leadIndex].fullName,
        updatedAt: new Date().toISOString(),
      };
      mockLeads[leadIndex] = updatedLead;
      return updatedLead;
    }
    const response = await api.patch<Lead>(`/crm/leads/${leadId}`, data);
    return response.data;
  },

  // Delete a lead
  delete: async (leadId: string): Promise<void> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      const index = mockLeads.findIndex(l => l.id === leadId);
      if (index !== -1) {
        mockLeads.splice(index, 1);
      }
      return;
    }
    await api.delete(`/crm/leads/${leadId}`);
  },

  // Convert lead to deal
  convert: async (leadId: string, dealData: CreateDealRequest): Promise<Deal> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 600));
      const lead = getLeadById(leadId);
      if (!lead) throw new Error('Lead not found');
      
      // Update lead status
      const leadIndex = mockLeads.findIndex(l => l.id === leadId);
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
        currency: dealData.currency || 'MXN',
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
    }
    const response = await api.post<Deal>(`/crm/leads/${leadId}/convert`, dealData);
    return response.data;
  },
};

// ============================================================================
// DEALS API
// ============================================================================

export const dealsApi = {
  // Get all deals for a dealer
  getAll: async (dealerId?: string): Promise<Deal[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      if (dealerId) {
        return getDealsByDealer(dealerId);
      }
      return mockDeals;
    }
    const response = await api.get<Deal[]>('/crm/deals');
    return response.data;
  },

  // Get deal by ID
  getById: async (dealId: string): Promise<Deal | null> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return getDealById(dealId) || null;
    }
    const response = await api.get<Deal>(`/crm/deals/${dealId}`);
    return response.data;
  },

  // Get deals by pipeline
  getByPipeline: async (pipelineId: string): Promise<Deal[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      return mockDeals.filter(d => d.pipelineId === pipelineId);
    }
    const response = await api.get<Deal[]>(`/crm/pipelines/${pipelineId}/deals`);
    return response.data;
  },

  // Get deals by stage
  getByStage: async (stageId: string): Promise<Deal[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockDeals.filter(d => d.stageId === stageId);
    }
    const response = await api.get<Deal[]>(`/crm/stages/${stageId}/deals`);
    return response.data;
  },

  // Create a new deal
  create: async (data: CreateDealRequest): Promise<Deal> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 500));
      const newDeal: Deal = {
        id: `deal-${Date.now()}`,
        title: data.title,
        description: data.description,
        value: data.value,
        currency: data.currency || 'MXN',
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
    }
    const response = await api.post<Deal>('/crm/deals', data);
    return response.data;
  },

  // Update a deal
  update: async (dealId: string, data: UpdateDealRequest): Promise<Deal> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 400));
      const dealIndex = mockDeals.findIndex(d => d.id === dealId);
      if (dealIndex === -1) throw new Error('Deal not found');
      
      const updatedDeal = {
        ...mockDeals[dealIndex],
        ...data,
        updatedAt: new Date().toISOString(),
      };
      mockDeals[dealIndex] = updatedDeal;
      return updatedDeal;
    }
    const response = await api.patch<Deal>(`/crm/deals/${dealId}`, data);
    return response.data;
  },

  // Move deal to another stage (for Kanban)
  move: async (dealId: string, data: MoveDealRequest): Promise<Deal> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      const dealIndex = mockDeals.findIndex(d => d.id === dealId);
      if (dealIndex === -1) throw new Error('Deal not found');
      
      // Get stage info
      const pipeline = mockPipelines.find(p => 
        p.stages.some(s => s.id === data.stageId)
      );
      const stage = pipeline?.stages.find(s => s.id === data.stageId);
      
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
    }
    const response = await api.post<Deal>(`/crm/deals/${dealId}/move`, data);
    return response.data;
  },

  // Close a deal (won or lost)
  close: async (dealId: string, data: CloseDealRequest): Promise<Deal> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 400));
      const dealIndex = mockDeals.findIndex(d => d.id === dealId);
      if (dealIndex === -1) throw new Error('Deal not found');
      
      const pipeline = mockPipelines.find(p => p.id === mockDeals[dealIndex].pipelineId);
      const closedStage = pipeline?.stages.find(s => 
        data.isWon ? s.isWonStage : s.isLostStage
      );
      
      const updatedDeal = {
        ...mockDeals[dealIndex],
        status: data.isWon ? 'Won' : 'Lost' as Deal['status'],
        stageId: closedStage?.id || mockDeals[dealIndex].stageId,
        stageName: closedStage?.name,
        stageColor: closedStage?.color,
        probability: data.isWon ? 100 : 0,
        actualCloseDate: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };
      mockDeals[dealIndex] = updatedDeal;
      return updatedDeal;
    }
    const response = await api.post<Deal>(`/crm/deals/${dealId}/close`, data);
    return response.data;
  },

  // Delete a deal
  delete: async (dealId: string): Promise<void> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      const index = mockDeals.findIndex(d => d.id === dealId);
      if (index !== -1) {
        mockDeals.splice(index, 1);
      }
      return;
    }
    await api.delete(`/crm/deals/${dealId}`);
  },
};

// ============================================================================
// PIPELINES API
// ============================================================================

export const pipelinesApi = {
  // Get all pipelines
  getAll: async (): Promise<Pipeline[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockPipelines;
    }
    const response = await api.get<Pipeline[]>('/crm/pipelines');
    return response.data;
  },

  // Get default pipeline
  getDefault: async (): Promise<Pipeline | null> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockPipelines.find(p => p.isDefault) || null;
    }
    const response = await api.get<Pipeline>('/crm/pipelines/default');
    return response.data;
  },

  // Get pipeline with stage stats (for Kanban board)
  getWithStats: async (pipelineId: string): Promise<PipelineStageStats[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      return getPipelineStats(pipelineId);
    }
    const response = await api.get<PipelineStageStats[]>(`/crm/pipelines/${pipelineId}/stats`);
    return response.data;
  },
};

// ============================================================================
// ACTIVITIES API
// ============================================================================

export const activitiesApi = {
  // Get all activities for a dealer
  getAll: async (dealerId?: string): Promise<Activity[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      if (dealerId) {
        return getActivitiesByDealer(dealerId);
      }
      return mockActivities;
    }
    const response = await api.get<Activity[]>('/crm/activities');
    return response.data;
  },

  // Get activities for a deal
  getByDeal: async (dealId: string): Promise<Activity[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockActivities.filter(a => a.dealId === dealId);
    }
    const response = await api.get<Activity[]>(`/crm/deals/${dealId}/activities`);
    return response.data;
  },

  // Get activities for a lead
  getByLead: async (leadId: string): Promise<Activity[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockActivities.filter(a => a.leadId === leadId);
    }
    const response = await api.get<Activity[]>(`/crm/leads/${leadId}/activities`);
    return response.data;
  },

  // Get today's activities
  getToday: async (): Promise<Activity[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      const today = new Date().toISOString().split('T')[0];
      return mockActivities.filter(a => 
        a.dueDate?.startsWith(today) && !a.completedAt
      );
    }
    const response = await api.get<Activity[]>('/crm/activities/today');
    return response.data;
  },

  // Get overdue activities
  getOverdue: async (): Promise<Activity[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      const now = new Date();
      return mockActivities.filter(a => 
        a.dueDate && new Date(a.dueDate) < now && !a.completedAt
      );
    }
    const response = await api.get<Activity[]>('/crm/activities/overdue');
    return response.data;
  },
};

// ============================================================================
// CRM STATS API
// ============================================================================

export const crmStatsApi = {
  // Get CRM stats for a dealer
  getStats: async (dealerId: string): Promise<CRMStats> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      return getCRMStatsByDealer(dealerId);
    }
    const response = await api.get<CRMStats>(`/crm/stats/${dealerId}`);
    return response.data;
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
