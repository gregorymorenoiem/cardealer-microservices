// CRM Mock Data - Based on backend CRMService DTOs

// ============================================
// TYPE DEFINITIONS (matching backend contracts)
// ============================================

export type LeadSource = 'Website' | 'Referral' | 'Phone' | 'WalkIn' | 'SocialMedia' | 'Email' | 'Advertisement' | 'Other';
export type LeadStatus = 'New' | 'Contacted' | 'Qualified' | 'Proposal' | 'Negotiation' | 'Won' | 'Lost' | 'Unqualified';
export type DealStatus = 'Open' | 'Won' | 'Lost';

export interface Lead {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  company?: string;
  jobTitle?: string;
  source: LeadSource;
  status: LeadStatus;
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

export interface Stage {
  id: string;
  pipelineId: string;
  name: string;
  order: number;
  color?: string;
  defaultProbability: number;
  isWonStage: boolean;
  isLostStage: boolean;
  dealsCount: number;
  totalValue: number;
}

export interface Pipeline {
  id: string;
  name: string;
  description?: string;
  isDefault: boolean;
  isActive: boolean;
  stages: Stage[];
  createdAt: string;
}

export interface Deal {
  id: string;
  title: string;
  description?: string;
  value: number;
  currency: string;
  pipelineId: string;
  pipelineName?: string;
  stageId: string;
  stageName?: string;
  stageColor?: string;
  status: DealStatus;
  probability: number;
  expectedCloseDate?: string;
  actualCloseDate?: string;
  leadId?: string;
  contactId?: string;
  assignedToUserId?: string;
  productId?: string;
  vin?: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

export interface Activity {
  id: string;
  type: 'call' | 'email' | 'meeting' | 'note' | 'task';
  title: string;
  description?: string;
  dealId?: string;
  leadId?: string;
  contactId?: string;
  assignedToUserId?: string;
  dueDate?: string;
  completedAt?: string;
  createdAt: string;
}

// ============================================
// MOCK PIPELINES & STAGES
// ============================================

const vehicleSalesStages: Stage[] = [
  {
    id: 'stage-001',
    pipelineId: 'pipeline-001',
    name: 'Nuevo Lead',
    order: 1,
    color: '#6366F1',
    defaultProbability: 10,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 8,
    totalValue: 2450000,
  },
  {
    id: 'stage-002',
    pipelineId: 'pipeline-001',
    name: 'Contactado',
    order: 2,
    color: '#8B5CF6',
    defaultProbability: 25,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 6,
    totalValue: 1890000,
  },
  {
    id: 'stage-003',
    pipelineId: 'pipeline-001',
    name: 'Prueba de Manejo',
    order: 3,
    color: '#EC4899',
    defaultProbability: 50,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 4,
    totalValue: 1320000,
  },
  {
    id: 'stage-004',
    pipelineId: 'pipeline-001',
    name: 'Negociación',
    order: 4,
    color: '#F59E0B',
    defaultProbability: 75,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 3,
    totalValue: 980000,
  },
  {
    id: 'stage-005',
    pipelineId: 'pipeline-001',
    name: 'Cerrado Ganado',
    order: 5,
    color: '#10B981',
    defaultProbability: 100,
    isWonStage: true,
    isLostStage: false,
    dealsCount: 12,
    totalValue: 4560000,
  },
  {
    id: 'stage-006',
    pipelineId: 'pipeline-001',
    name: 'Perdido',
    order: 6,
    color: '#EF4444',
    defaultProbability: 0,
    isWonStage: false,
    isLostStage: true,
    dealsCount: 5,
    totalValue: 1850000,
  },
];

const realEstateStages: Stage[] = [
  {
    id: 'stage-101',
    pipelineId: 'pipeline-002',
    name: 'Interesado',
    order: 1,
    color: '#06B6D4',
    defaultProbability: 10,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 5,
    totalValue: 15000000,
  },
  {
    id: 'stage-102',
    pipelineId: 'pipeline-002',
    name: 'Visita Programada',
    order: 2,
    color: '#3B82F6',
    defaultProbability: 30,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 3,
    totalValue: 8500000,
  },
  {
    id: 'stage-103',
    pipelineId: 'pipeline-002',
    name: 'Oferta Presentada',
    order: 3,
    color: '#8B5CF6',
    defaultProbability: 60,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 2,
    totalValue: 5800000,
  },
  {
    id: 'stage-104',
    pipelineId: 'pipeline-002',
    name: 'En Notaría',
    order: 4,
    color: '#F59E0B',
    defaultProbability: 90,
    isWonStage: false,
    isLostStage: false,
    dealsCount: 1,
    totalValue: 3200000,
  },
  {
    id: 'stage-105',
    pipelineId: 'pipeline-002',
    name: 'Vendido',
    order: 5,
    color: '#10B981',
    defaultProbability: 100,
    isWonStage: true,
    isLostStage: false,
    dealsCount: 8,
    totalValue: 28000000,
  },
  {
    id: 'stage-106',
    pipelineId: 'pipeline-002',
    name: 'Descartado',
    order: 6,
    color: '#EF4444',
    defaultProbability: 0,
    isWonStage: false,
    isLostStage: true,
    dealsCount: 2,
    totalValue: 6000000,
  },
];

export const mockPipelines: Pipeline[] = [
  {
    id: 'pipeline-001',
    name: 'Venta de Vehículos',
    description: 'Pipeline principal para venta de autos nuevos y usados',
    isDefault: true,
    isActive: true,
    stages: vehicleSalesStages,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'pipeline-002',
    name: 'Venta de Inmuebles',
    description: 'Pipeline para operaciones inmobiliarias',
    isDefault: false,
    isActive: true,
    stages: realEstateStages,
    createdAt: '2024-03-15T00:00:00Z',
  },
];

// ============================================
// MOCK LEADS - Linked to specific dealers
// ============================================

export const mockLeads: Lead[] = [
  // Leads for dealer-free-001 (limited CRM access)
  {
    id: 'lead-001',
    firstName: 'Carlos',
    lastName: 'Mendoza',
    fullName: 'Carlos Mendoza',
    email: 'carlos.mendoza@email.com',
    phone: '+52 55 1234 5678',
    company: 'Automotriz del Norte',
    jobTitle: 'Gerente de Compras',
    source: 'Website',
    status: 'Qualified',
    score: 85,
    estimatedValue: 450000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'vehicle-001',
    tags: ['SUV', 'Financiamiento', 'Urgente'],
    notes: 'Cliente interesado en SUV de lujo, tiene presupuesto definido',
    createdAt: '2025-01-10T10:00:00Z',
    updatedAt: '2025-01-12T15:30:00Z',
  },
  {
    id: 'lead-002',
    firstName: 'María',
    lastName: 'González',
    fullName: 'María González',
    email: 'maria.gonzalez@email.com',
    phone: '+52 55 9876 5432',
    source: 'Referral',
    status: 'New',
    score: 45,
    estimatedValue: 280000,
    assignedToUserId: 'user-basic-001',
    tags: ['Sedán', 'Primer auto'],
    createdAt: '2025-01-14T09:00:00Z',
    updatedAt: '2025-01-14T09:00:00Z',
  },
  {
    id: 'lead-003',
    firstName: 'Roberto',
    lastName: 'Sánchez',
    fullName: 'Roberto Sánchez',
    email: 'roberto.sanchez@empresa.com',
    phone: '+52 81 5555 1234',
    company: 'Transportes RST',
    jobTitle: 'Director General',
    source: 'Advertisement',
    status: 'Negotiation',
    score: 92,
    estimatedValue: 2500000,
    assignedToUserId: 'user-enterprise-001',
    interestedProductId: 'vehicle-fleet',
    tags: ['Flotilla', 'Leasing', 'Corporativo'],
    notes: 'Interesado en renovar flotilla de 10 unidades',
    createdAt: '2025-01-05T14:00:00Z',
    updatedAt: '2025-01-15T11:00:00Z',
  },
  {
    id: 'lead-004',
    firstName: 'Ana',
    lastName: 'Torres',
    fullName: 'Ana Torres',
    email: 'ana.torres@mail.com',
    phone: '+52 33 7777 8888',
    source: 'SocialMedia',
    status: 'Contacted',
    score: 60,
    estimatedValue: 350000,
    assignedToUserId: 'user-pro-001',
    tags: ['Híbrido', 'Ecológico'],
    createdAt: '2025-01-13T16:00:00Z',
    updatedAt: '2025-01-14T10:00:00Z',
  },
  {
    id: 'lead-005',
    firstName: 'Luis',
    lastName: 'Ramírez',
    fullName: 'Luis Ramírez',
    email: 'luis.ramirez@outlook.com',
    source: 'Phone',
    status: 'Proposal',
    score: 78,
    estimatedValue: 520000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'vehicle-003',
    tags: ['Deportivo', 'Contado'],
    createdAt: '2025-01-08T11:00:00Z',
    updatedAt: '2025-01-15T09:00:00Z',
  },
  {
    id: 'lead-006',
    firstName: 'Patricia',
    lastName: 'López',
    fullName: 'Patricia López',
    email: 'patricia.lopez@gmail.com',
    phone: '+52 55 4444 3333',
    source: 'WalkIn',
    status: 'Won',
    score: 100,
    estimatedValue: 380000,
    assignedToUserId: 'user-basic-001',
    tags: ['Familiar', 'Minivan'],
    convertedAt: '2025-01-12T14:00:00Z',
    createdAt: '2025-01-02T10:00:00Z',
    updatedAt: '2025-01-12T14:00:00Z',
  },
  {
    id: 'lead-007',
    firstName: 'Jorge',
    lastName: 'Hernández',
    fullName: 'Jorge Hernández',
    email: 'jorge.h@protonmail.com',
    source: 'Email',
    status: 'Lost',
    score: 35,
    estimatedValue: 200000,
    assignedToUserId: 'user-basic-001',
    tags: ['Económico', 'Usado'],
    notes: 'No calificó para financiamiento',
    createdAt: '2025-01-01T08:00:00Z',
    updatedAt: '2025-01-10T16:00:00Z',
  },
  {
    id: 'lead-008',
    firstName: 'Fernanda',
    lastName: 'Castro',
    fullName: 'Fernanda Castro',
    email: 'fer.castro@empresa.mx',
    phone: '+52 222 111 2233',
    company: 'Grupo Empresarial FC',
    jobTitle: 'CFO',
    source: 'Referral',
    status: 'Qualified',
    score: 88,
    estimatedValue: 1800000,
    assignedToUserId: 'user-enterprise-001',
    interestedProductId: 'vehicle-luxury',
    tags: ['Premium', 'Ejecutivo', 'Múltiples unidades'],
    createdAt: '2025-01-11T13:00:00Z',
    updatedAt: '2025-01-15T08:00:00Z',
  },
];

// ============================================
// MOCK DEALS - Linked to stages and pipelines
// ============================================

export const mockDeals: Deal[] = [
  // Vehicle Sales Pipeline Deals
  {
    id: 'deal-001',
    title: 'BMW X5 2024 - Carlos Mendoza',
    description: 'Venta de BMW X5 xDrive40i, color blanco',
    value: 1250000,
    currency: 'MXN',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-004',
    stageName: 'Negociación',
    stageColor: '#F59E0B',
    status: 'Open',
    probability: 75,
    expectedCloseDate: '2025-01-25T00:00:00Z',
    leadId: 'lead-001',
    assignedToUserId: 'user-pro-001',
    productId: 'vehicle-001',
    vin: 'WBAPH5C57BA123456',
    tags: ['Premium', 'Financiamiento'],
    createdAt: '2025-01-12T15:30:00Z',
    updatedAt: '2025-01-15T10:00:00Z',
  },
  {
    id: 'deal-002',
    title: 'Flotilla RST - 10 unidades',
    description: 'Renovación de flotilla corporativa, 10 Toyota Hilux',
    value: 4500000,
    currency: 'MXN',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-004',
    stageName: 'Negociación',
    stageColor: '#F59E0B',
    status: 'Open',
    probability: 80,
    expectedCloseDate: '2025-02-15T00:00:00Z',
    leadId: 'lead-003',
    assignedToUserId: 'user-enterprise-001',
    tags: ['Flotilla', 'Corporativo', 'Leasing'],
    createdAt: '2025-01-05T14:00:00Z',
    updatedAt: '2025-01-15T11:00:00Z',
  },
  {
    id: 'deal-003',
    title: 'Toyota Camry Híbrido - Ana Torres',
    description: 'Camry Híbrido XLE 2024, prueba de manejo programada',
    value: 680000,
    currency: 'MXN',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-003',
    stageName: 'Prueba de Manejo',
    stageColor: '#EC4899',
    status: 'Open',
    probability: 50,
    expectedCloseDate: '2025-01-30T00:00:00Z',
    leadId: 'lead-004',
    assignedToUserId: 'user-pro-001',
    productId: 'vehicle-camry',
    tags: ['Híbrido', 'Ecológico'],
    createdAt: '2025-01-14T10:00:00Z',
    updatedAt: '2025-01-15T09:00:00Z',
  },
  {
    id: 'deal-004',
    title: 'Mazda MX-5 - Luis Ramírez',
    description: 'MX-5 RF Grand Touring, pago de contado',
    value: 720000,
    currency: 'MXN',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-003',
    stageName: 'Prueba de Manejo',
    stageColor: '#EC4899',
    status: 'Open',
    probability: 60,
    expectedCloseDate: '2025-01-28T00:00:00Z',
    leadId: 'lead-005',
    assignedToUserId: 'user-pro-001',
    productId: 'vehicle-003',
    vin: 'JM1NDAL78M0123456',
    tags: ['Deportivo', 'Contado'],
    createdAt: '2025-01-15T09:00:00Z',
    updatedAt: '2025-01-15T09:00:00Z',
  },
  {
    id: 'deal-005',
    title: 'Honda Odyssey - Patricia López',
    description: 'Odyssey Touring, familia numerosa',
    value: 850000,
    currency: 'MXN',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-005',
    stageName: 'Cerrado Ganado',
    stageColor: '#10B981',
    status: 'Won',
    probability: 100,
    expectedCloseDate: '2025-01-12T00:00:00Z',
    actualCloseDate: '2025-01-12T14:00:00Z',
    leadId: 'lead-006',
    assignedToUserId: 'user-basic-001',
    productId: 'vehicle-odyssey',
    vin: '5FNRL6H71MB123456',
    tags: ['Familiar', 'Financiado'],
    createdAt: '2025-01-02T10:00:00Z',
    updatedAt: '2025-01-12T14:00:00Z',
  },
  {
    id: 'deal-006',
    title: 'Mercedes-Benz S-Class - Fernanda Castro',
    description: 'S 580 4MATIC, 2 unidades para ejecutivos',
    value: 5600000,
    currency: 'MXN',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-002',
    stageName: 'Contactado',
    stageColor: '#8B5CF6',
    status: 'Open',
    probability: 25,
    expectedCloseDate: '2025-02-28T00:00:00Z',
    leadId: 'lead-008',
    assignedToUserId: 'user-enterprise-001',
    tags: ['Premium', 'Ejecutivo', 'Múltiples'],
    createdAt: '2025-01-11T13:00:00Z',
    updatedAt: '2025-01-15T08:00:00Z',
  },
  // Real Estate Pipeline Deals
  {
    id: 'deal-101',
    title: 'Casa Residencial Polanco',
    description: 'Casa 4 recámaras, 450m2 construcción',
    value: 12500000,
    currency: 'MXN',
    pipelineId: 'pipeline-002',
    pipelineName: 'Venta de Inmuebles',
    stageId: 'stage-103',
    stageName: 'Oferta Presentada',
    stageColor: '#8B5CF6',
    status: 'Open',
    probability: 60,
    expectedCloseDate: '2025-02-15T00:00:00Z',
    assignedToUserId: 'user-enterprise-001',
    tags: ['Residencial', 'Premium'],
    createdAt: '2025-01-08T10:00:00Z',
    updatedAt: '2025-01-15T10:00:00Z',
  },
  {
    id: 'deal-102',
    title: 'Departamento Santa Fe',
    description: 'Penthouse 280m2, vista panorámica',
    value: 8900000,
    currency: 'MXN',
    pipelineId: 'pipeline-002',
    pipelineName: 'Venta de Inmuebles',
    stageId: 'stage-104',
    stageName: 'En Notaría',
    stageColor: '#F59E0B',
    status: 'Open',
    probability: 90,
    expectedCloseDate: '2025-01-22T00:00:00Z',
    assignedToUserId: 'user-pro-001',
    tags: ['Penthouse', 'Inversión'],
    createdAt: '2024-12-15T10:00:00Z',
    updatedAt: '2025-01-14T16:00:00Z',
  },
];

// ============================================
// MOCK ACTIVITIES
// ============================================

export const mockActivities: Activity[] = [
  {
    id: 'activity-001',
    type: 'call',
    title: 'Llamada de seguimiento - Carlos Mendoza',
    description: 'Confirmar disponibilidad para prueba de manejo',
    dealId: 'deal-001',
    leadId: 'lead-001',
    assignedToUserId: 'user-pro-001',
    dueDate: '2025-01-16T10:00:00Z',
    createdAt: '2025-01-15T09:00:00Z',
  },
  {
    id: 'activity-002',
    type: 'meeting',
    title: 'Presentación Flotilla RST',
    description: 'Presentación de cotización formal al director',
    dealId: 'deal-002',
    leadId: 'lead-003',
    assignedToUserId: 'user-enterprise-001',
    dueDate: '2025-01-18T15:00:00Z',
    createdAt: '2025-01-15T08:00:00Z',
  },
  {
    id: 'activity-003',
    type: 'task',
    title: 'Preparar documentación financiamiento',
    description: 'Solicitar pre-aprobación bancaria para cliente',
    dealId: 'deal-001',
    assignedToUserId: 'user-pro-001',
    dueDate: '2025-01-17T12:00:00Z',
    createdAt: '2025-01-15T10:00:00Z',
  },
  {
    id: 'activity-004',
    type: 'email',
    title: 'Enviar brochure Toyota Camry',
    description: 'Información completa del modelo híbrido',
    leadId: 'lead-004',
    assignedToUserId: 'user-pro-001',
    dueDate: '2025-01-15T18:00:00Z',
    completedAt: '2025-01-15T11:30:00Z',
    createdAt: '2025-01-14T16:00:00Z',
  },
  {
    id: 'activity-005',
    type: 'note',
    title: 'Notas de reunión con Fernanda',
    description: 'Interesada en 2 unidades S-Class, solicita descuento por volumen',
    dealId: 'deal-006',
    leadId: 'lead-008',
    assignedToUserId: 'user-enterprise-001',
    completedAt: '2025-01-14T14:00:00Z',
    createdAt: '2025-01-14T14:00:00Z',
  },
];

// ============================================
// CRM STATISTICS BY DEALER
// ============================================

export interface CRMStats {
  totalLeads: number;
  newLeadsThisMonth: number;
  qualifiedLeads: number;
  convertedLeads: number;
  conversionRate: number;
  totalDeals: number;
  openDeals: number;
  wonDeals: number;
  lostDeals: number;
  totalDealValue: number;
  wonDealValue: number;
  expectedRevenue: number;
  avgDealSize: number;
  avgDealCycledays: number;
  activitiesDueToday: number;
  overdueActivities: number;
}

export const getCRMStatsByDealer = (dealerId: string): CRMStats => {
  // Different stats based on dealer plan/id
  const statsByDealer: Record<string, CRMStats> = {
    'dealer-free-001': {
      totalLeads: 3,
      newLeadsThisMonth: 1,
      qualifiedLeads: 1,
      convertedLeads: 0,
      conversionRate: 0,
      totalDeals: 0,
      openDeals: 0,
      wonDeals: 0,
      lostDeals: 0,
      totalDealValue: 0,
      wonDealValue: 0,
      expectedRevenue: 0,
      avgDealSize: 0,
      avgDealCycledays: 0,
      activitiesDueToday: 0,
      overdueActivities: 0,
    },
    'dealer-basic-001': {
      totalLeads: 25,
      newLeadsThisMonth: 8,
      qualifiedLeads: 12,
      convertedLeads: 5,
      conversionRate: 20,
      totalDeals: 8,
      openDeals: 3,
      wonDeals: 5,
      lostDeals: 2,
      totalDealValue: 4250000,
      wonDealValue: 2100000,
      expectedRevenue: 1800000,
      avgDealSize: 420000,
      avgDealCycledays: 18,
      activitiesDueToday: 3,
      overdueActivities: 1,
    },
    'dealer-pro-001': {
      totalLeads: 85,
      newLeadsThisMonth: 28,
      qualifiedLeads: 45,
      convertedLeads: 22,
      conversionRate: 26,
      totalDeals: 32,
      openDeals: 10,
      wonDeals: 18,
      lostDeals: 4,
      totalDealValue: 18500000,
      wonDealValue: 12300000,
      expectedRevenue: 5200000,
      avgDealSize: 578000,
      avgDealCycledays: 14,
      activitiesDueToday: 8,
      overdueActivities: 2,
    },
    'dealer-enterprise-001': {
      totalLeads: 320,
      newLeadsThisMonth: 95,
      qualifiedLeads: 180,
      convertedLeads: 85,
      conversionRate: 27,
      totalDeals: 125,
      openDeals: 40,
      wonDeals: 72,
      lostDeals: 13,
      totalDealValue: 125000000,
      wonDealValue: 85000000,
      expectedRevenue: 32000000,
      avgDealSize: 1000000,
      avgDealCycledays: 21,
      activitiesDueToday: 25,
      overdueActivities: 5,
    },
  };

  return statsByDealer[dealerId] || statsByDealer['dealer-free-001'];
};

// ============================================
// HELPER FUNCTIONS
// ============================================

export const getLeadsByDealer = (dealerId: string): Lead[] => {
  // Map dealer IDs to user IDs
  const userIdMap: Record<string, string> = {
    'dealer-free-001': 'user-free-001',
    'dealer-basic-001': 'user-basic-001',
    'dealer-pro-001': 'user-pro-001',
    'dealer-enterprise-001': 'user-enterprise-001',
  };
  
  const userId = userIdMap[dealerId];
  if (!userId) return [];
  
  return mockLeads.filter(lead => lead.assignedToUserId === userId);
};

export const getDealsByDealer = (dealerId: string): Deal[] => {
  const userIdMap: Record<string, string> = {
    'dealer-free-001': 'user-free-001',
    'dealer-basic-001': 'user-basic-001',
    'dealer-pro-001': 'user-pro-001',
    'dealer-enterprise-001': 'user-enterprise-001',
  };
  
  const userId = userIdMap[dealerId];
  if (!userId) return [];
  
  return mockDeals.filter(deal => deal.assignedToUserId === userId);
};

export const getDealsByStage = (stageId: string): Deal[] => {
  return mockDeals.filter(deal => deal.stageId === stageId);
};

export const getDealsByPipeline = (pipelineId: string): Deal[] => {
  return mockDeals.filter(deal => deal.pipelineId === pipelineId);
};

export const getActivitiesByDealer = (dealerId: string): Activity[] => {
  const userIdMap: Record<string, string> = {
    'dealer-free-001': 'user-free-001',
    'dealer-basic-001': 'user-basic-001',
    'dealer-pro-001': 'user-pro-001',
    'dealer-enterprise-001': 'user-enterprise-001',
  };
  
  const userId = userIdMap[dealerId];
  if (!userId) return [];
  
  return mockActivities.filter(activity => activity.assignedToUserId === userId);
};

export const getLeadById = (id: string): Lead | undefined => {
  return mockLeads.find(lead => lead.id === id);
};

export const getDealById = (id: string): Deal | undefined => {
  return mockDeals.find(deal => deal.id === id);
};

export const getPipelineById = (id: string): Pipeline | undefined => {
  return mockPipelines.find(pipeline => pipeline.id === id);
};

export const getStageById = (pipelineId: string, stageId: string): Stage | undefined => {
  const pipeline = getPipelineById(pipelineId);
  if (!pipeline) return undefined;
  return pipeline.stages.find(stage => stage.id === stageId);
};

// Get pipeline stats for Kanban board
export interface PipelineStageStats {
  stageId: string;
  stageName: string;
  color: string;
  deals: Deal[];
  totalValue: number;
  count: number;
}

export const getPipelineStats = (pipelineId: string): PipelineStageStats[] => {
  const pipeline = getPipelineById(pipelineId);
  if (!pipeline) return [];
  
  return pipeline.stages.map(stage => {
    const deals = mockDeals.filter(deal => deal.stageId === stage.id);
    return {
      stageId: stage.id,
      stageName: stage.name,
      color: stage.color || '#6366F1',
      deals,
      totalValue: deals.reduce((sum, deal) => sum + deal.value, 0),
      count: deals.length,
    };
  });
};
