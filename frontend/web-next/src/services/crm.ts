/**
 * CRM Service
 * 
 * API client for CRM operations: leads, deals, activities
 */

import { apiClient } from '@/lib/api-client';

// ============================================================================
// Types
// ============================================================================

export type LeadStatus = 'New' | 'Contacted' | 'Qualified' | 'Proposal' | 'Negotiating' | 'Won' | 'Lost';

export type LeadSource = 'Website' | 'WhatsApp' | 'Phone' | 'WalkIn' | 'Referral' | 'SocialMedia' | 'Email' | 'Other';

export interface LeadDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  company?: string;
  jobTitle?: string;
  source: string;
  status: string;
  score: number;
  estimatedValue?: number;
  assignedToUserId?: string;
  interestedProductId?: string;
  tags: string[];
  notes?: string;
  createdAt: string;
  updatedAt: string;
  convertedAt?: string;
}

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
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  company?: string;
  jobTitle?: string;
  status?: string;
  score?: number;
  assignedToUserId?: string;
  estimatedValue?: number;
}

export interface ConvertLeadRequest {
  dealTitle: string;
  value: number;
  pipelineId?: string;
  stageId?: string;
}

export interface LeadFilters {
  status?: LeadStatus;
  source?: LeadSource;
  assignedTo?: string;
  search?: string;
  minScore?: number;
  maxScore?: number;
}

export interface LeadStats {
  total: number;
  new: number;
  contacted: number;
  qualified: number;
  negotiating: number;
  won: number;
  lost: number;
  conversionRate: number;
  avgScore: number;
}

// ============================================================================
// API Functions
// ============================================================================

/**
 * Get all leads
 */
export async function getLeads(): Promise<LeadDto[]> {
  const response = await apiClient.get<LeadDto[]>('/api/crm/leads');
  return response.data;
}

/**
 * Get lead by ID
 */
export async function getLeadById(id: string): Promise<LeadDto> {
  const response = await apiClient.get<LeadDto>(`/api/crm/leads/${id}`);
  return response.data;
}

/**
 * Get leads by status
 */
export async function getLeadsByStatus(status: LeadStatus): Promise<LeadDto[]> {
  const response = await apiClient.get<LeadDto[]>(`/api/crm/leads/status/${status}`);
  return response.data;
}

/**
 * Search leads
 */
export async function searchLeads(query: string): Promise<LeadDto[]> {
  const response = await apiClient.get<LeadDto[]>('/api/crm/leads/search', {
    params: { query }
  });
  return response.data;
}

/**
 * Get leads assigned to a user
 */
export async function getLeadsByAssignedUser(userId: string): Promise<LeadDto[]> {
  const response = await apiClient.get<LeadDto[]>(`/api/crm/leads/assigned/${userId}`);
  return response.data;
}

/**
 * Get recent leads
 */
export async function getRecentLeads(count: number = 10): Promise<LeadDto[]> {
  const response = await apiClient.get<LeadDto[]>(`/api/crm/leads/recent/${count}`);
  return response.data;
}

/**
 * Create a new lead
 */
export async function createLead(request: CreateLeadRequest, dealerId: string): Promise<LeadDto> {
  const response = await apiClient.post<LeadDto>('/api/crm/leads', request, {
    headers: {
      'X-Dealer-Id': dealerId
    }
  });
  return response.data;
}

/**
 * Update a lead
 */
export async function updateLead(id: string, request: UpdateLeadRequest): Promise<LeadDto> {
  const response = await apiClient.put<LeadDto>(`/api/crm/leads/${id}`, request);
  return response.data;
}

/**
 * Delete a lead
 */
export async function deleteLead(id: string): Promise<void> {
  await apiClient.delete(`/api/crm/leads/${id}`);
}

/**
 * Calculate lead statistics from a list of leads
 */
export function calculateLeadStats(leads: LeadDto[]): LeadStats {
  const statusCounts = {
    new: 0,
    contacted: 0,
    qualified: 0,
    negotiating: 0,
    won: 0,
    lost: 0,
  };

  let totalScore = 0;

  for (const lead of leads) {
    const status = lead.status.toLowerCase() as keyof typeof statusCounts;
    if (status in statusCounts) {
      statusCounts[status]++;
    } else if (lead.status === 'New') {
      statusCounts.new++;
    } else if (lead.status === 'Contacted') {
      statusCounts.contacted++;
    } else if (lead.status === 'Qualified') {
      statusCounts.qualified++;
    } else if (lead.status === 'Proposal' || lead.status === 'Negotiating') {
      statusCounts.negotiating++;
    } else if (lead.status === 'Won') {
      statusCounts.won++;
    } else if (lead.status === 'Lost') {
      statusCounts.lost++;
    }
    totalScore += lead.score;
  }

  const closedDeals = statusCounts.won + statusCounts.lost;
  const conversionRate = closedDeals > 0 ? (statusCounts.won / closedDeals) * 100 : 0;

  return {
    total: leads.length,
    ...statusCounts,
    conversionRate: Math.round(conversionRate * 10) / 10,
    avgScore: leads.length > 0 ? Math.round(totalScore / leads.length) : 0,
  };
}

// ============================================================================
// Helper Functions
// ============================================================================

export function getLeadStatusColor(status: string): string {
  const colors: Record<string, string> = {
    New: 'bg-blue-100 text-blue-800',
    Contacted: 'bg-yellow-100 text-yellow-800',
    Qualified: 'bg-purple-100 text-purple-800',
    Proposal: 'bg-indigo-100 text-indigo-800',
    Negotiating: 'bg-orange-100 text-orange-800',
    Won: 'bg-green-100 text-green-800',
    Lost: 'bg-red-100 text-red-800',
  };
  return colors[status] || 'bg-gray-100 text-gray-800';
}

export function getLeadSourceIcon(source: string): string {
  const icons: Record<string, string> = {
    Website: '🌐',
    WhatsApp: '💬',
    Phone: '📞',
    WalkIn: '🚶',
    Referral: '🤝',
    SocialMedia: '📱',
    Email: '✉️',
    Other: '📋',
  };
  return icons[source] || '📋';
}

export function getLeadScoreColor(score: number): string {
  if (score >= 80) return 'text-green-600';
  if (score >= 60) return 'text-yellow-600';
  if (score >= 40) return 'text-orange-600';
  return 'text-red-600';
}

export function formatLeadName(lead: LeadDto): string {
  return lead.fullName || `${lead.firstName} ${lead.lastName}`.trim();
}
