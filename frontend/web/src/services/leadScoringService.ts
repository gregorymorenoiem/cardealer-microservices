import axios, { AxiosInstance } from 'axios';

// ============================================================================
// TYPES & INTERFACES
// ============================================================================

export type LeadTemperature = 'Hot' | 'Warm' | 'Cold';
export type LeadStatus =
  | 'New'
  | 'Contacted'
  | 'Qualified'
  | 'Nurturing'
  | 'Negotiating'
  | 'Converted'
  | 'Lost'
  | 'Archived';
export type LeadSource =
  | 'OrganicSearch'
  | 'DirectListing'
  | 'EmailCampaign'
  | 'SocialMedia'
  | 'Referral'
  | 'PaidAd'
  | 'Retargeting'
  | 'Unknown';
export type LeadActionType =
  | 'ViewListing'
  | 'ViewImages'
  | 'ViewSellerProfile'
  | 'ClickPhone'
  | 'ClickEmail'
  | 'ClickWhatsApp'
  | 'SendMessage'
  | 'AddToFavorites'
  | 'RemoveFromFavorites'
  | 'ShareListing'
  | 'AddToComparison'
  | 'ScheduleTestDrive'
  | 'RequestFinancing'
  | 'MakeOffer'
  | 'PurchaseCompleted'
  | 'ReportListing'
  | 'BlockSeller'
  | 'ScoreRecalculated';

export interface LeadActionDto {
  id: string;
  actionType: string;
  description: string;
  scoreImpact: number;
  occurredAt: string;
}

export interface LeadDto {
  id: string;
  userId: string;
  userEmail: string;
  userFullName: string;
  userPhone: string | null;
  vehicleId: string;
  vehicleTitle: string;
  vehiclePrice: number;
  dealerId: string;
  dealerName: string;
  score: number;
  temperature: LeadTemperature;
  conversionProbability: number;
  engagementScore: number;
  recencyScore: number;
  intentScore: number;
  viewCount: number;
  contactCount: number;
  favoriteCount: number;
  shareCount: number;
  comparisonCount: number;
  hasScheduledTestDrive: boolean;
  hasRequestedFinancing: boolean;
  totalTimeSpentSeconds: number;
  status: LeadStatus;
  source: LeadSource;
  firstInteractionAt: string;
  lastInteractionAt: string;
  lastContactedAt: string | null;
  convertedAt: string | null;
  dealerNotes: string | null;
  recentActions: LeadActionDto[];
}

export interface CreateLeadDto {
  userId: string;
  userEmail: string;
  userFullName: string;
  userPhone?: string | null;
  vehicleId: string;
  vehicleTitle: string;
  vehiclePrice: number;
  dealerId: string;
  dealerName: string;
  source: LeadSource;
}

export interface RecordLeadActionDto {
  leadId: string;
  actionType: LeadActionType;
  metadata?: string | null;
  ipAddress?: string | null;
  userAgent?: string | null;
}

export interface UpdateLeadStatusDto {
  status: LeadStatus;
  dealerNotes?: string | null;
}

export interface LeadTrendDto {
  date: string;
  averageScore: number;
  leadCount: number;
}

export interface LeadStatisticsDto {
  dealerId: string;
  totalLeads: number;
  hotLeads: number;
  warmLeads: number;
  coldLeads: number;
  newLeads: number;
  contactedLeads: number;
  convertedLeads: number;
  averageScore: number;
  conversionRate: number;
  scoreTrends: LeadTrendDto[];
}

export interface PagedLeadsResponse {
  leads: LeadDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================================================
// LEAD SCORING SERVICE
// ============================================================================

export class LeadScoringService {
  private api: AxiosInstance;

  constructor(baseURL?: string) {
    const apiUrl = baseURL || import.meta.env.VITE_API_URL || 'http://localhost:18443';

    this.api = axios.create({
      baseURL: `${apiUrl}/api/leads`,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Interceptor para agregar token JWT
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }

  // ============================================================================
  // CRUD OPERATIONS
  // ============================================================================

  /**
   * Obtener leads de un dealer con paginación y filtros
   */
  async getLeadsByDealer(
    dealerId: string,
    page: number = 1,
    pageSize: number = 20,
    temperature?: LeadTemperature | null,
    status?: LeadStatus | null,
    searchTerm?: string | null
  ): Promise<PagedLeadsResponse> {
    const params: any = { page, pageSize };
    if (temperature) params.temperature = temperature;
    if (status) params.status = status;
    if (searchTerm) params.searchTerm = searchTerm;

    const response = await this.api.get<PagedLeadsResponse>(`/dealer/${dealerId}`, { params });
    return response.data;
  }

  /**
   * Obtener un lead por ID con todo su historial
   */
  async getLeadById(leadId: string): Promise<LeadDto> {
    const response = await this.api.get<LeadDto>(`/${leadId}`);
    return response.data;
  }

  /**
   * Obtener estadísticas de leads de un dealer
   */
  async getLeadStatistics(dealerId: string): Promise<LeadStatisticsDto> {
    const response = await this.api.get<LeadStatisticsDto>(`/dealer/${dealerId}/statistics`);
    return response.data;
  }

  /**
   * Crear o actualizar un lead
   */
  async createOrUpdateLead(dto: CreateLeadDto): Promise<LeadDto> {
    const response = await this.api.post<LeadDto>('/', dto);
    return response.data;
  }

  /**
   * Registrar una acción de un lead (view, contact, etc.)
   */
  async recordLeadAction(dto: RecordLeadActionDto): Promise<LeadDto> {
    const response = await this.api.post<LeadDto>('/actions', dto);
    return response.data;
  }

  /**
   * Actualizar el estado de un lead
   */
  async updateLeadStatus(leadId: string, dto: UpdateLeadStatusDto): Promise<LeadDto> {
    const response = await this.api.put<LeadDto>(`/${leadId}/status`, dto);
    return response.data;
  }

  // ============================================================================
  // HELPER METHODS
  // ============================================================================

  /**
   * Obtener color del badge según temperatura
   */
  getTemperatureColor(temperature: LeadTemperature): string {
    switch (temperature) {
      case 'Hot':
        return 'red';
      case 'Warm':
        return 'orange';
      case 'Cold':
        return 'blue';
      default:
        return 'gray';
    }
  }

  /**
   * Obtener emoji según temperatura
   */
  getTemperatureEmoji(temperature: LeadTemperature): string {
    switch (temperature) {
      case 'Hot':
        return '🔥';
      case 'Warm':
        return '🟡';
      case 'Cold':
        return '❄️';
      default:
        return '⚪';
    }
  }

  /**
   * Obtener descripción de temperatura
   */
  getTemperatureDescription(temperature: LeadTemperature): string {
    switch (temperature) {
      case 'Hot':
        return 'Alta probabilidad de conversión - ¡Contactar ahora!';
      case 'Warm':
        return 'Interesado pero necesita seguimiento';
      case 'Cold':
        return 'Interés bajo - Requiere nurturing';
      default:
        return 'Sin clasificar';
    }
  }

  /**
   * Formatear score como porcentaje visual
   */
  formatScore(score: number): string {
    return `${score}/100`;
  }

  /**
   * Obtener CSS class para score bar
   */
  getScoreBarClass(score: number): string {
    if (score >= 70) return 'bg-red-500';
    if (score >= 40) return 'bg-orange-500';
    return 'bg-blue-500';
  }

  /**
   * Calcular días desde última interacción
   */
  getDaysSinceLastInteraction(lastInteractionAt: string): number {
    const lastInteraction = new Date(lastInteractionAt);
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - lastInteraction.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  /**
   * Formatear fecha relativa (hace X días)
   */
  formatRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffTime = now.getTime() - date.getTime();
    const diffMinutes = Math.floor(diffTime / (1000 * 60));
    const diffHours = Math.floor(diffTime / (1000 * 60 * 60));
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));

    if (diffMinutes < 60) {
      return `Hace ${diffMinutes} minutos`;
    } else if (diffHours < 24) {
      return `Hace ${diffHours} horas`;
    } else if (diffDays === 1) {
      return 'Ayer';
    } else {
      return `Hace ${diffDays} días`;
    }
  }

  /**
   * Obtener descripción de acción
   */
  getActionDescription(actionType: string): string {
    const descriptions: Record<string, string> = {
      ViewListing: 'Vio el vehículo',
      ViewImages: 'Vio las imágenes',
      ViewSellerProfile: 'Vio tu perfil',
      ClickPhone: 'Hizo clic en tu teléfono',
      ClickEmail: 'Hizo clic en tu email',
      ClickWhatsApp: 'Hizo clic en WhatsApp',
      SendMessage: 'Te envió un mensaje',
      AddToFavorites: 'Agregó a favoritos',
      RemoveFromFavorites: 'Quitó de favoritos',
      ShareListing: 'Compartió el vehículo',
      AddToComparison: 'Agregó a comparación',
      ScheduleTestDrive: 'Agendó un test drive 🎉',
      RequestFinancing: 'Solicitó financiamiento 💰',
      MakeOffer: 'Hizo una oferta 🤝',
      PurchaseCompleted: '¡COMPRÓ EL VEHÍCULO! 🎊',
    };
    return descriptions[actionType] || actionType;
  }

  /**
   * Obtener icono de acción
   */
  getActionIcon(actionType: string): string {
    const icons: Record<string, string> = {
      ViewListing: '👁️',
      ViewImages: '🖼️',
      ViewSellerProfile: '👤',
      ClickPhone: '📞',
      ClickEmail: '📧',
      ClickWhatsApp: '💬',
      SendMessage: '✉️',
      AddToFavorites: '❤️',
      RemoveFromFavorites: '💔',
      ShareListing: '🔗',
      AddToComparison: '⚖️',
      ScheduleTestDrive: '🚗',
      RequestFinancing: '💰',
      MakeOffer: '🤝',
      PurchaseCompleted: '🎉',
    };
    return icons[actionType] || '📋';
  }

  /**
   * Verificar si un lead es "hot" y debe notificarse
   */
  isHotLead(lead: LeadDto): boolean {
    return lead.temperature === 'Hot';
  }

  /**
   * Obtener prioridad de seguimiento (1-5, 5 = máxima)
   */
  getFollowUpPriority(lead: LeadDto): number {
    if (lead.temperature === 'Hot' && lead.status === 'New') return 5;
    if (lead.temperature === 'Hot' && lead.status === 'Contacted') return 4;
    if (lead.temperature === 'Warm' && lead.status === 'New') return 3;
    if (lead.temperature === 'Warm' && lead.status === 'Contacted') return 2;
    return 1;
  }

  /**
   * Generar recomendación de acción para el dealer
   */
  getRecommendedAction(lead: LeadDto): string {
    if (lead.temperature === 'Hot' && lead.status === 'New') {
      return '🔥 ¡Contacta AHORA! Este lead está muy interesado';
    }
    if (lead.hasScheduledTestDrive && lead.status !== 'Contacted') {
      return '🚗 Confirmar test drive agendado';
    }
    if (lead.hasRequestedFinancing) {
      return '💰 Enviar opciones de financiamiento';
    }
    if (
      lead.temperature === 'Hot' &&
      this.getDaysSinceLastInteraction(lead.lastInteractionAt) > 1
    ) {
      return '⏰ Seguimiento urgente - No dejar enfriar';
    }
    if (lead.temperature === 'Warm' && lead.contactCount === 0) {
      return '📞 Hacer primer contacto';
    }
    if (
      lead.temperature === 'Warm' &&
      this.getDaysSinceLastInteraction(lead.lastInteractionAt) > 3
    ) {
      return '📧 Enviar email de seguimiento';
    }
    if (lead.temperature === 'Cold') {
      return '📬 Agregar a campaña de nurturing';
    }
    return '✅ Mantener seguimiento regular';
  }
}

// Exportar instancia singleton
const leadScoringService = new LeadScoringService();
export default leadScoringService;
