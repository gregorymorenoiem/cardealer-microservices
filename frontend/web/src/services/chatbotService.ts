import axios, { type AxiosInstance } from 'axios';
import * as signalR from '@microsoft/signalr';

// ============================================================================
// INTERFACES - Mapean DTOs del backend
// ============================================================================

export enum ConversationStatus {
  Active = 'Active',
  Ended = 'Ended',
  Abandoned = 'Abandoned',
}

export enum LeadTemperature {
  Unknown = 'Unknown',
  Cold = 'Cold',
  Warm = 'Warm',
  WarmHot = 'WarmHot',
  Hot = 'Hot',
}

export enum MessageRole {
  User = 'User',
  Assistant = 'Assistant',
  System = 'System',
}

export enum MessageType {
  Text = 'Text',
  BuyingSignalDetected = 'BuyingSignalDetected',
  HandoffInitiated = 'HandoffInitiated',
  ConversationEnded = 'ConversationEnded',
}

export interface ConversationDto {
  id: string;
  userId: string;
  userName: string | null;
  userEmail: string | null;
  userPhone: string | null;
  vehicleId: string | null;
  vehicleTitle: string | null;
  vehiclePrice: number | null;
  dealerId: string | null;
  dealerName: string | null;
  dealerWhatsApp: string | null;
  status: ConversationStatus;
  leadScore: number;
  leadTemperature: LeadTemperature;
  hasUrgency: boolean;
  hasBudget: boolean;
  hasTradeIn: boolean;
  wantsTestDrive: boolean;
  buyingSignals: string[];
  purchaseTimeframe: string | null;
  startedAt: string;
  endedAt: string | null;
  lastMessageAt: string | null;
  messageCount: number;
  handoffInitiatedAt: string | null;
  handoffMethod: string | null;
  handoffNotes: string | null;
}

export interface MessageDto {
  id: string;
  conversationId: string;
  role: MessageRole;
  type: MessageType;
  content: string | null;
  metadata: Record<string, any> | null;
  sentAt: string;
}

export interface StartConversationDto {
  userId: string;
  userName?: string;
  userEmail?: string;
  userPhone?: string;
  vehicleId?: string;
  vehicleTitle?: string;
  vehiclePrice?: number;
  dealerId?: string;
  dealerName?: string;
  dealerWhatsApp?: string;
  initialMessage?: string;
}

export interface SendMessageDto {
  content: string;
  role?: MessageRole;
}

export interface HandoffDto {
  method: string;
  notes?: string;
}

export interface ConversationStatisticsDto {
  totalConversations: number;
  activeConversations: number;
  endedConversations: number;
  abandonedConversations: number;
  totalMessages: number;
  averageMessagesPerConversation: number;
  hotLeads: number;
  warmLeads: number;
  coldLeads: number;
  handoffsInitiated: number;
  conversationsWithUrgency: number;
  conversationsWithBudget: number;
  conversationsWithTradeIn: number;
  conversationsWithTestDrive: number;
}

// ============================================================================
// SIGNALR EVENT TYPES
// ============================================================================

export interface MessageReceivedEvent {
  conversationId: string;
  message: MessageDto;
}

export interface TypingIndicatorEvent {
  conversationId: string;
  userId: string;
  userName: string;
  isTyping: boolean;
}

export interface HandoffRecommendedEvent {
  conversationId: string;
  leadScore: number;
  leadTemperature: LeadTemperature;
  buyingSignals: string[];
  recommendedAction: string;
}

// ============================================================================
// CHATBOT SERVICE
// ============================================================================

export class ChatbotService {
  private axiosInstance: AxiosInstance;
  private hubConnection: signalR.HubConnection | null = null;
  private baseUrl: string;
  private hubUrl: string;

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || import.meta.env.VITE_API_URL || 'https://api.okla.com.do';
    this.hubUrl = `${this.baseUrl}/chathub`;

    this.axiosInstance = axios.create({
      baseURL: this.baseUrl,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.axiosInstance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );
  }

  // REST API METHODS
  async startConversation(dto: StartConversationDto): Promise<ConversationDto> {
    const response = await this.axiosInstance.post<ConversationDto>('/api/conversations', dto);
    return response.data;
  }

  async sendMessage(conversationId: string, dto: SendMessageDto): Promise<MessageDto> {
    const response = await this.axiosInstance.post<MessageDto>(
      `/api/conversations/${conversationId}/messages`,
      dto
    );
    return response.data;
  }

  async handoffToWhatsApp(conversationId: string, dto: HandoffDto): Promise<void> {
    await this.axiosInstance.post(`/api/conversations/${conversationId}/handoff`, dto);
  }

  async getConversation(conversationId: string): Promise<ConversationDto> {
    const response = await this.axiosInstance.get<ConversationDto>(
      `/api/conversations/${conversationId}`
    );
    return response.data;
  }

  async getMessages(conversationId: string): Promise<MessageDto[]> {
    const response = await this.axiosInstance.get<MessageDto[]>(
      `/api/conversations/${conversationId}/messages`
    );
    return response.data;
  }

  async getUserConversations(userId: string): Promise<ConversationDto[]> {
    const response = await this.axiosInstance.get<ConversationDto[]>(
      `/api/conversations/user/${userId}`
    );
    return response.data;
  }

  async getDealerConversations(dealerId: string): Promise<ConversationDto[]> {
    const response = await this.axiosInstance.get<ConversationDto[]>(
      `/api/conversations/dealer/${dealerId}`
    );
    return response.data;
  }

  async getHotLeads(minScore: number = 85): Promise<ConversationDto[]> {
    const response = await this.axiosInstance.get<ConversationDto[]>(
      '/api/conversations/hot-leads',
      { params: { minScore } }
    );
    return response.data;
  }

  async getStatistics(dealerId: string): Promise<ConversationStatisticsDto> {
    const response = await this.axiosInstance.get<ConversationStatisticsDto>(
      `/api/conversations/statistics/dealer/${dealerId}`
    );
    return response.data;
  }

  // SIGNALR METHODS
  async connectToHub(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      console.log('Hub already connected');
      return;
    }

    const token = localStorage.getItem('token');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    try {
      await this.hubConnection.start();
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      throw err;
    }
  }

  async disconnectFromHub(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      console.log('SignalR Disconnected');
    }
  }

  async joinConversation(conversationId: string): Promise<void> {
    if (!this.hubConnection) throw new Error('Hub not connected');
    await this.hubConnection.invoke('JoinConversation', conversationId);
  }

  async leaveConversation(conversationId: string): Promise<void> {
    if (!this.hubConnection) throw new Error('Hub not connected');
    await this.hubConnection.invoke('LeaveConversation', conversationId);
  }

  async sendMessageViaHub(conversationId: string, content: string): Promise<void> {
    if (!this.hubConnection) throw new Error('Hub not connected');
    await this.hubConnection.invoke('SendMessage', conversationId, content);
  }

  async sendTypingIndicator(conversationId: string): Promise<void> {
    if (!this.hubConnection) throw new Error('Hub not connected');
    await this.hubConnection.invoke('TypingIndicator', conversationId);
  }

  // EVENT HANDLERS
  onMessageReceived(callback: (event: MessageReceivedEvent) => void): void {
    if (!this.hubConnection) {
      console.warn('Hub not connected');
      return;
    }
    this.hubConnection.on('MessageReceived', (conversationId: string, message: MessageDto) => {
      callback({ conversationId, message });
    });
  }

  onTypingIndicator(callback: (event: TypingIndicatorEvent) => void): void {
    if (!this.hubConnection) {
      console.warn('Hub not connected');
      return;
    }
    this.hubConnection.on(
      'UserTyping',
      (conversationId: string, userId: string, userName: string) => {
        callback({ conversationId, userId, userName, isTyping: true });
      }
    );
  }

  onHandoffRecommended(callback: (event: HandoffRecommendedEvent) => void): void {
    if (!this.hubConnection) {
      console.warn('Hub not connected');
      return;
    }
    this.hubConnection.on(
      'HandoffRecommended',
      (
        conversationId: string,
        leadScore: number,
        leadTemperature: string,
        buyingSignals: string[],
        recommendedAction: string
      ) => {
        callback({
          conversationId,
          leadScore,
          leadTemperature: leadTemperature as LeadTemperature,
          buyingSignals,
          recommendedAction,
        });
      }
    );
  }

  offAllEvents(): void {
    if (this.hubConnection) {
      this.hubConnection.off('MessageReceived');
      this.hubConnection.off('UserTyping');
      this.hubConnection.off('HandoffRecommended');
    }
  }

  // HELPER METHODS
  static getTemperatureColor(temperature: LeadTemperature): string {
    switch (temperature) {
      case LeadTemperature.Hot:
        return 'red';
      case LeadTemperature.WarmHot:
        return 'orange';
      case LeadTemperature.Warm:
        return 'yellow';
      case LeadTemperature.Cold:
        return 'blue';
      default:
        return 'gray';
    }
  }

  static getTemperatureLabel(temperature: LeadTemperature): string {
    switch (temperature) {
      case LeadTemperature.Hot:
        return 'CALIENTE 🔥';
      case LeadTemperature.WarmHot:
        return 'Muy Interesado';
      case LeadTemperature.Warm:
        return 'Interesado';
      case LeadTemperature.Cold:
        return 'Frío';
      default:
        return 'Desconocido';
    }
  }

  static shouldTriggerHandoff(leadScore: number): boolean {
    return leadScore >= 85;
  }

  static formatRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'ahora';
    if (diffMins < 60) return `hace ${diffMins} min`;
    if (diffHours < 24) return `hace ${diffHours}h`;
    if (diffDays < 7) return `hace ${diffDays}d`;
    return date.toLocaleDateString('es-DO');
  }

  static getConversationSummary(conversation: ConversationDto): string {
    const signals: string[] = [];
    if (conversation.hasUrgency) signals.push('Urgencia');
    if (conversation.hasBudget) signals.push('Presupuesto');
    if (conversation.hasTradeIn) signals.push('Trade-in');
    if (conversation.wantsTestDrive) signals.push('Test Drive');

    if (signals.length === 0) return 'Sin señales de compra';
    return signals.join(', ');
  }

  static isActive(conversation: ConversationDto): boolean {
    return conversation.status === ConversationStatus.Active;
  }

  static isAbandoned(conversation: ConversationDto): boolean {
    if (conversation.status === ConversationStatus.Abandoned) return true;

    if (conversation.lastMessageAt) {
      const lastMessage = new Date(conversation.lastMessageAt);
      const now = new Date();
      const diffMinutes = (now.getTime() - lastMessage.getTime()) / 60000;
      return diffMinutes > 30;
    }

    return false;
  }

  static calculateLeadProgress(conversation: ConversationDto): number {
    let progress = 0;
    if (conversation.hasUrgency) progress += 25;
    if (conversation.hasBudget) progress += 25;
    if (conversation.hasTradeIn) progress += 25;
    if (conversation.wantsTestDrive) progress += 25;
    return progress;
  }

  static getRecommendedAction(conversation: ConversationDto): string {
    if (conversation.leadScore >= 85) {
      return 'Contactar por WhatsApp INMEDIATAMENTE';
    }
    if (conversation.leadScore >= 70) {
      return 'Contactar en las próximas 2 horas';
    }
    if (conversation.leadScore >= 50) {
      return 'Seguimiento en 24 horas';
    }
    return 'Continuar conversación automática';
  }

  static formatWhatsAppNumber(phone: string): string {
    let cleaned = phone.replace(/\D/g, '');
    if (cleaned.length === 10) {
      cleaned = '1809' + cleaned;
    }
    return '+' + cleaned;
  }

  static isValidPhone(phone: string | null): boolean {
    if (!phone) return false;
    const cleaned = phone.replace(/\D/g, '');
    return cleaned.length >= 10 && cleaned.length <= 15;
  }

  static extractVehicleIdFromUrl(url: string): string | null {
    const match = url.match(/\/vehicles\/([a-f0-9-]+)/i);
    return match ? match[1] : null;
  }

  static generateWelcomeMessage(vehicleTitle?: string, dealerName?: string): string {
    if (vehicleTitle && dealerName) {
      return `¡Hola! 👋 Te ayudaré con información sobre el ${vehicleTitle} de ${dealerName}. ¿En qué puedo asistirte?`;
    }
    if (vehicleTitle) {
      return `¡Hola! 👋 Te ayudaré con información sobre el ${vehicleTitle}. ¿Qué te gustaría saber?`;
    }
    return '¡Hola! 👋 Soy el asistente de OKLA. ¿En qué puedo ayudarte hoy?';
  }

  static getBuyingSignalEmoji(signal: string): string {
    const lowerSignal = signal.toLowerCase();
    if (lowerSignal.includes('urgen')) return '⚡';
    if (lowerSignal.includes('budget') || lowerSignal.includes('presupuesto')) return '💰';
    if (lowerSignal.includes('trade') || lowerSignal.includes('cambio')) return '🔄';
    if (lowerSignal.includes('test') || lowerSignal.includes('prueba')) return '🚗';
    if (lowerSignal.includes('financ')) return '🏦';
    return '✅';
  }
}

export const chatbotService = new ChatbotService();
