// CRM Mock Data - Based on backend CRMService DTOs
// Optimized for Dominican Republic market (OKLA)

// ============================================
// TYPE DEFINITIONS (matching backend contracts)
// ============================================

export type LeadSource =
  | 'Website'
  | 'Referral'
  | 'Phone'
  | 'WalkIn'
  | 'SocialMedia'
  | 'Email'
  | 'Advertisement'
  | 'WhatsApp'
  | 'Other';
export type LeadStatus =
  | 'New'
  | 'Contacted'
  | 'Qualified'
  | 'Proposal'
  | 'Negotiation'
  | 'Won'
  | 'Lost'
  | 'Unqualified';
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
// MOCK LEADS - Dominican Republic focused
// ============================================

export const mockLeads: Lead[] = [
  // Hot leads - high scores
  {
    id: 'lead-001',
    firstName: 'Juan Carlos',
    lastName: 'Rodríguez',
    fullName: 'Juan Carlos Rodríguez',
    email: 'jcrodriguez@gmail.com',
    phone: '+1 809 555 0101',
    company: 'Inversiones JCR SRL',
    jobTitle: 'Gerente General',
    source: 'Website',
    status: 'Qualified',
    score: 92,
    estimatedValue: 2500000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'Toyota Land Cruiser 2024',
    tags: ['SUV', 'Financiamiento', 'Urgente'],
    notes: 'Cliente interesado en SUV de lujo, tiene presupuesto definido de RD$2.5M',
    createdAt: '2026-01-08T10:00:00Z',
    updatedAt: '2026-01-10T15:30:00Z',
  },
  {
    id: 'lead-002',
    firstName: 'María Isabel',
    lastName: 'Santos',
    fullName: 'María Isabel Santos',
    email: 'misantos@outlook.com',
    phone: '+1 809 555 0102',
    source: 'WhatsApp',
    status: 'Negotiation',
    score: 88,
    estimatedValue: 1800000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'Honda CR-V 2024',
    tags: ['SUV', 'Familiar', 'Contado'],
    notes: 'Busca vehículo familiar, pago de contado',
    createdAt: '2026-01-05T09:00:00Z',
    updatedAt: '2026-01-10T11:00:00Z',
  },
  {
    id: 'lead-003',
    firstName: 'Roberto',
    lastName: 'Jiménez',
    fullName: 'Roberto Jiménez',
    email: 'rjimenez@empresas.do',
    phone: '+1 829 555 1234',
    company: 'Transportes Dominicanos SA',
    jobTitle: 'Director de Operaciones',
    source: 'Referral',
    status: 'Proposal',
    score: 95,
    estimatedValue: 8500000,
    assignedToUserId: 'user-enterprise-001',
    interestedProductId: 'Flotilla Toyota Hilux',
    tags: ['Flotilla', 'Leasing', 'Corporativo'],
    notes: 'Interesado en renovar flotilla de 5 pickups',
    createdAt: '2026-01-03T14:00:00Z',
    updatedAt: '2026-01-10T08:00:00Z',
  },
  // Warm leads - medium scores
  {
    id: 'lead-004',
    firstName: 'Ana',
    lastName: 'Pérez',
    fullName: 'Ana Pérez',
    email: 'ana.perez@hotmail.com',
    phone: '+1 809 555 0103',
    source: 'SocialMedia',
    status: 'Contacted',
    score: 68,
    estimatedValue: 950000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'Hyundai Tucson 2023',
    tags: ['SUV Compacto', 'Primera compra'],
    createdAt: '2026-01-07T16:00:00Z',
    updatedAt: '2026-01-09T10:00:00Z',
  },
  {
    id: 'lead-005',
    firstName: 'Luis Miguel',
    lastName: 'Hernández',
    fullName: 'Luis Miguel Hernández',
    email: 'lmhernandez@gmail.com',
    phone: '+1 849 555 5678',
    source: 'Website',
    status: 'Qualified',
    score: 72,
    estimatedValue: 1200000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'Kia Sportage 2024',
    tags: ['SUV', 'Financiamiento'],
    createdAt: '2026-01-06T11:00:00Z',
    updatedAt: '2026-01-09T15:00:00Z',
  },
  {
    id: 'lead-006',
    firstName: 'Carmen',
    lastName: 'Díaz',
    fullName: 'Carmen Díaz',
    email: 'cdiaz@yahoo.com',
    phone: '+1 809 555 0104',
    source: 'Advertisement',
    status: 'New',
    score: 55,
    estimatedValue: 750000,
    assignedToUserId: 'user-basic-001',
    interestedProductId: 'Nissan Kicks 2023',
    tags: ['Crossover', 'Económico'],
    createdAt: '2026-01-09T09:00:00Z',
    updatedAt: '2026-01-09T09:00:00Z',
  },
  // Cold leads - lower scores
  {
    id: 'lead-007',
    firstName: 'Pedro',
    lastName: 'Martínez',
    fullName: 'Pedro Martínez',
    email: 'pmartinez@email.com',
    phone: '+1 809 555 0105',
    source: 'Email',
    status: 'New',
    score: 35,
    estimatedValue: 500000,
    assignedToUserId: 'user-basic-001',
    interestedProductId: 'Toyota Yaris 2022',
    tags: ['Económico', 'Usado'],
    notes: 'Pidió información pero no ha respondido',
    createdAt: '2026-01-04T08:00:00Z',
    updatedAt: '2026-01-04T08:00:00Z',
  },
  {
    id: 'lead-008',
    firstName: 'Fernanda',
    lastName: 'Castro',
    fullName: 'Fernanda Castro',
    email: 'fcastro@empresa.do',
    phone: '+1 829 111 2233',
    company: 'Grupo Empresarial FC',
    jobTitle: 'CFO',
    source: 'Referral',
    status: 'Qualified',
    score: 85,
    estimatedValue: 6500000,
    assignedToUserId: 'user-enterprise-001',
    interestedProductId: 'Mercedes-Benz GLE 2024',
    tags: ['Premium', 'Ejecutivo', 'Múltiples unidades'],
    createdAt: '2026-01-08T13:00:00Z',
    updatedAt: '2026-01-10T08:00:00Z',
  },
  {
    id: 'lead-009',
    firstName: 'Miguel Ángel',
    lastName: 'Reyes',
    fullName: 'Miguel Ángel Reyes',
    email: 'mareyes@gmail.com',
    phone: '+1 809 555 0106',
    source: 'WalkIn',
    status: 'Won',
    score: 100,
    estimatedValue: 1350000,
    assignedToUserId: 'user-basic-001',
    interestedProductId: 'Mazda CX-5 2023',
    tags: ['SUV', 'Financiado'],
    convertedAt: '2026-01-08T14:00:00Z',
    createdAt: '2026-01-02T10:00:00Z',
    updatedAt: '2026-01-08T14:00:00Z',
  },
  {
    id: 'lead-010',
    firstName: 'Sofía',
    lastName: 'Gómez',
    fullName: 'Sofía Gómez',
    email: 'sofiag@outlook.com',
    phone: '+1 849 555 9999',
    source: 'WhatsApp',
    status: 'Contacted',
    score: 45,
    estimatedValue: 650000,
    assignedToUserId: 'user-basic-001',
    tags: ['Económico', 'Primer auto'],
    notes: 'Estudiante universitaria buscando primer vehículo',
    createdAt: '2026-01-09T16:00:00Z',
    updatedAt: '2026-01-10T10:00:00Z',
  },
  {
    id: 'lead-011',
    firstName: 'Carlos',
    lastName: 'Mendoza',
    fullName: 'Carlos Mendoza',
    email: 'cmendoza@gmail.com',
    phone: '+1 809 555 0107',
    source: 'Website',
    status: 'Lost',
    score: 25,
    estimatedValue: 2000000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'BMW X3 2024',
    tags: ['Premium'],
    notes: 'No calificó para financiamiento',
    createdAt: '2026-01-01T08:00:00Z',
    updatedAt: '2026-01-06T16:00:00Z',
  },
  {
    id: 'lead-012',
    firstName: 'Daniela',
    lastName: 'Vásquez',
    fullName: 'Daniela Vásquez',
    email: 'dvasquez@empresa.do',
    phone: '+1 829 555 7777',
    company: 'Clínica San Rafael',
    jobTitle: 'Directora Médica',
    source: 'Advertisement',
    status: 'Negotiation',
    score: 82,
    estimatedValue: 3200000,
    assignedToUserId: 'user-pro-001',
    interestedProductId: 'Audi Q5 2024',
    tags: ['Premium', 'Profesional'],
    createdAt: '2026-01-07T10:00:00Z',
    updatedAt: '2026-01-10T12:00:00Z',
  },
];

// ============================================
// MOCK DEALS - Linked to stages and pipelines (RD)
// ============================================

export const mockDeals: Deal[] = [
  // Vehicle Sales Pipeline Deals - Dominican Republic
  {
    id: 'deal-001',
    title: 'Toyota Land Cruiser 2024 - Juan Carlos Rodríguez',
    description: 'Venta de Land Cruiser VX, color negro, financiamiento aprobado',
    value: 2500000,
    currency: 'DOP',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-004',
    stageName: 'Negociación',
    stageColor: '#F59E0B',
    status: 'Open',
    probability: 85,
    expectedCloseDate: '2026-01-25T00:00:00Z',
    leadId: 'lead-001',
    assignedToUserId: 'user-pro-001',
    productId: 'vehicle-001',
    vin: 'JTERU5JR0N5123456',
    tags: ['SUV Premium', 'Financiamiento'],
    createdAt: '2026-01-10T15:30:00Z',
    updatedAt: '2026-01-12T10:00:00Z',
  },
  {
    id: 'deal-002',
    title: 'Flotilla Hilux - Transportes Dominicanos',
    description: 'Renovación de flotilla corporativa, 5 Toyota Hilux 4x4',
    value: 8500000,
    currency: 'DOP',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-004',
    stageName: 'Negociación',
    stageColor: '#F59E0B',
    status: 'Open',
    probability: 90,
    expectedCloseDate: '2026-02-15T00:00:00Z',
    leadId: 'lead-003',
    assignedToUserId: 'user-enterprise-001',
    tags: ['Flotilla', 'Corporativo', 'Leasing'],
    createdAt: '2026-01-03T14:00:00Z',
    updatedAt: '2026-01-10T11:00:00Z',
  },
  {
    id: 'deal-003',
    title: 'Honda CR-V 2024 - María Isabel Santos',
    description: 'CR-V EX-L, pago de contado, lista para entrega',
    value: 1800000,
    currency: 'DOP',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-004',
    stageName: 'Negociación',
    stageColor: '#F59E0B',
    status: 'Open',
    probability: 80,
    expectedCloseDate: '2026-01-20T00:00:00Z',
    leadId: 'lead-002',
    assignedToUserId: 'user-pro-001',
    productId: 'vehicle-crv',
    tags: ['SUV', 'Contado'],
    createdAt: '2026-01-05T09:00:00Z',
    updatedAt: '2026-01-10T11:00:00Z',
  },
  {
    id: 'deal-004',
    title: 'Audi Q5 2024 - Dra. Daniela Vásquez',
    description: 'Q5 Premium Plus, financiamiento APAP',
    value: 3200000,
    currency: 'DOP',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-003',
    stageName: 'Prueba de Manejo',
    stageColor: '#EC4899',
    status: 'Open',
    probability: 65,
    expectedCloseDate: '2026-01-28T00:00:00Z',
    leadId: 'lead-012',
    assignedToUserId: 'user-pro-001',
    productId: 'vehicle-q5',
    vin: 'WA1BNAFY5N2123456',
    tags: ['Premium', 'Profesional'],
    createdAt: '2026-01-07T10:00:00Z',
    updatedAt: '2026-01-10T12:00:00Z',
  },
  {
    id: 'deal-005',
    title: 'Mazda CX-5 2023 - Miguel Ángel Reyes',
    description: 'CX-5 Signature, financiamiento Banco Popular',
    value: 1350000,
    currency: 'DOP',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-005',
    stageName: 'Cerrado Ganado',
    stageColor: '#10B981',
    status: 'Won',
    probability: 100,
    expectedCloseDate: '2026-01-08T00:00:00Z',
    actualCloseDate: '2026-01-08T14:00:00Z',
    leadId: 'lead-009',
    assignedToUserId: 'user-basic-001',
    productId: 'vehicle-cx5',
    vin: 'JM3KFBCM5P0123456',
    tags: ['SUV', 'Financiado'],
    createdAt: '2026-01-02T10:00:00Z',
    updatedAt: '2026-01-08T14:00:00Z',
  },
  {
    id: 'deal-006',
    title: 'Mercedes-Benz GLE 2024 - Fernanda Castro',
    description: 'GLE 450 4MATIC, 2 unidades para ejecutivos del grupo',
    value: 6500000,
    currency: 'DOP',
    pipelineId: 'pipeline-001',
    pipelineName: 'Venta de Vehículos',
    stageId: 'stage-002',
    stageName: 'Contactado',
    stageColor: '#8B5CF6',
    status: 'Open',
    probability: 70,
    expectedCloseDate: '2026-02-28T00:00:00Z',
    leadId: 'lead-008',
    assignedToUserId: 'user-enterprise-001',
    tags: ['Premium', 'Ejecutivo', 'Múltiples'],
    createdAt: '2026-01-08T13:00:00Z',
    updatedAt: '2026-01-10T08:00:00Z',
  },
];

// ============================================
// MOCK ACTIVITIES - Dominican Republic
// ============================================

export const mockActivities: Activity[] = [
  {
    id: 'activity-001',
    type: 'call',
    title: 'Llamada de seguimiento - Juan Carlos Rodríguez',
    description: 'Confirmar disponibilidad para prueba de Land Cruiser',
    dealId: 'deal-001',
    leadId: 'lead-001',
    assignedToUserId: 'user-pro-001',
    dueDate: '2026-01-13T10:00:00Z',
    createdAt: '2026-01-12T09:00:00Z',
  },
  {
    id: 'activity-002',
    type: 'meeting',
    title: 'Presentación Flotilla - Transportes Dominicanos',
    description: 'Presentación de cotización formal para 5 Toyota Hilux',
    dealId: 'deal-002',
    leadId: 'lead-003',
    assignedToUserId: 'user-enterprise-001',
    dueDate: '2026-01-15T15:00:00Z',
    createdAt: '2026-01-10T08:00:00Z',
  },
  {
    id: 'activity-003',
    type: 'task',
    title: 'Preparar documentos financiamiento APAP',
    description: 'Solicitar pre-aprobación bancaria para Dra. Vásquez',
    dealId: 'deal-004',
    assignedToUserId: 'user-pro-001',
    dueDate: '2026-01-14T12:00:00Z',
    createdAt: '2026-01-12T10:00:00Z',
  },
  {
    id: 'activity-004',
    type: 'email',
    title: 'Enviar brochure Honda CR-V',
    description: 'Información completa del modelo y financiamiento',
    leadId: 'lead-002',
    assignedToUserId: 'user-pro-001',
    dueDate: '2026-01-11T18:00:00Z',
    completedAt: '2026-01-11T11:30:00Z',
    createdAt: '2026-01-10T16:00:00Z',
  },
  {
    id: 'activity-005',
    type: 'note',
    title: 'Notas de reunión con Fernanda Castro',
    description: 'Interesada en 2 GLE para ejecutivos de Grupo FC, solicita descuento',
    dealId: 'deal-006',
    leadId: 'lead-008',
    assignedToUserId: 'user-enterprise-001',
    completedAt: '2026-01-10T14:00:00Z',
    createdAt: '2026-01-10T14:00:00Z',
  },
  {
    id: 'activity-006',
    type: 'call',
    title: 'WhatsApp - María Isabel Santos',
    description: 'Seguimiento por WhatsApp, coordinar visita al dealer',
    leadId: 'lead-002',
    assignedToUserId: 'user-pro-001',
    dueDate: '2026-01-12T16:00:00Z',
    createdAt: '2026-01-11T09:00:00Z',
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

  return mockLeads.filter((lead) => lead.assignedToUserId === userId);
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

  return mockDeals.filter((deal) => deal.assignedToUserId === userId);
};

export const getDealsByStage = (stageId: string): Deal[] => {
  return mockDeals.filter((deal) => deal.stageId === stageId);
};

export const getDealsByPipeline = (pipelineId: string): Deal[] => {
  return mockDeals.filter((deal) => deal.pipelineId === pipelineId);
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

  return mockActivities.filter((activity) => activity.assignedToUserId === userId);
};

export const getLeadById = (id: string): Lead | undefined => {
  return mockLeads.find((lead) => lead.id === id);
};

export const getDealById = (id: string): Deal | undefined => {
  return mockDeals.find((deal) => deal.id === id);
};

export const getPipelineById = (id: string): Pipeline | undefined => {
  return mockPipelines.find((pipeline) => pipeline.id === id);
};

export const getStageById = (pipelineId: string, stageId: string): Stage | undefined => {
  const pipeline = getPipelineById(pipelineId);
  if (!pipeline) return undefined;
  return pipeline.stages.find((stage) => stage.id === stageId);
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

  return pipeline.stages.map((stage) => {
    const deals = mockDeals.filter((deal) => deal.stageId === stage.id);
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
