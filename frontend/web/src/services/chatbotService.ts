import * as signalR from '@microsoft/signalr';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080';
const CHATBOT_API = `${API_URL}/api/chat`;
const CHATBOT_HUB = `${API_URL}/hubs/chat`;

// ============================================
// Types
// ============================================

export interface Conversation {
  id: string;
  userId?: string;
  sessionId?: string;
  vehicleId?: string;
  status: ConversationStatus;
  userEmail?: string;
  userName?: string;
  messageCount: number;
  leadQualification: LeadQualification;
  leadScore?: number;
  createdAt: string;
  updatedAt: string;
  endedAt?: string;
  messages: ChatMessage[];
}

export interface ConversationSummary {
  id: string;
  vehicleId?: string;
  status: ConversationStatus;
  messageCount: number;
  leadQualification: LeadQualification;
  createdAt: string;
  updatedAt: string;
}

export interface ChatMessage {
  id: string;
  role: MessageRole;
  content: string;
  type: MessageType;
  createdAt: string;
  responseTime?: number;
  suggestedReplies?: QuickReply[];
}

export interface QuickReply {
  id: string;
  label: string;
  value: string;
  icon?: string;
}

export interface VehicleContext {
  vehicleId: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  transmission?: string;
  fuelType?: string;
  color?: string;
  description?: string;
  sellerName?: string;
  location?: string;
}

export interface SendMessageResponse {
  userMessage: ChatMessage;
  assistantMessage: ChatMessage;
  suggestedReplies: QuickReply[];
  shouldTransferToAgent: boolean;
  transferReason?: string;
}

export interface TypingIndicator {
  conversationId: string;
  isTyping: boolean;
}

export interface ChatAnalytics {
  totalConversations: number;
  activeConversations: number;
  totalMessages: number;
  averageMessagesPerConversation: number;
  averageResponseTimeMs: number;
  totalTokensUsed: number;
  totalCost: number;
  conversationsByStatus: Record<string, number>;
  leadsByQualification: Record<string, number>;
}

export type ConversationStatus = 'Active' | 'Paused' | 'Ended' | 'TransferredToAgent';
export type LeadQualification = 'Unknown' | 'Cold' | 'Warm' | 'Hot';
export type MessageRole = 'User' | 'Assistant' | 'System';
export type MessageType = 'Text' | 'Image' | 'System' | 'Action' | 'QuickReply';

// ============================================
// SignalR Connection Manager
// ============================================

class ChatbotSignalRConnection {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  async connect(accessToken?: string): Promise<signalR.HubConnection> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return this.connection;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(CHATBOT_HUB, {
        accessTokenFactory: () => accessToken || '',
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      this.reconnectAttempts = 0;
    });

    this.connection.onclose((error) => {
      console.log('SignalR connection closed:', error);
    });

    await this.connection.start();
    console.log('SignalR connected');
    return this.connection;
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  getConnection(): signalR.HubConnection | null {
    return this.connection;
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const signalRConnection = new ChatbotSignalRConnection();

// ============================================
// API Service
// ============================================

class ChatbotService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    };
  }

  // REST API Methods
  async createConversation(params: {
    userId?: string;
    sessionId?: string;
    vehicleId?: string;
    userEmail?: string;
    userName?: string;
    userPhone?: string;
    vehicleContext?: VehicleContext;
  }): Promise<Conversation> {
    const response = await fetch(`${CHATBOT_API}/conversations`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(params),
    });

    if (!response.ok) {
      throw new Error(`Failed to create conversation: ${response.statusText}`);
    }

    return response.json();
  }

  async getConversation(id: string): Promise<Conversation | null> {
    const response = await fetch(`${CHATBOT_API}/conversations/${id}`, {
      headers: this.getAuthHeaders(),
    });

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new Error(`Failed to get conversation: ${response.statusText}`);
    }

    return response.json();
  }

  async getUserConversations(userId: string, skip = 0, take = 20): Promise<ConversationSummary[]> {
    const response = await fetch(
      `${CHATBOT_API}/conversations/user/${userId}?skip=${skip}&take=${take}`,
      { headers: this.getAuthHeaders() }
    );

    if (!response.ok) {
      throw new Error(`Failed to get user conversations: ${response.statusText}`);
    }

    return response.json();
  }

  async sendMessage(
    conversationId: string,
    content: string,
    vehicleContext?: VehicleContext
  ): Promise<SendMessageResponse> {
    const response = await fetch(`${CHATBOT_API}/conversations/${conversationId}/messages`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ content, vehicleContext }),
    });

    if (!response.ok) {
      throw new Error(`Failed to send message: ${response.statusText}`);
    }

    return response.json();
  }

  async endConversation(conversationId: string, reason: string): Promise<Conversation> {
    const response = await fetch(`${CHATBOT_API}/conversations/${conversationId}/end`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ reason }),
    });

    if (!response.ok) {
      throw new Error(`Failed to end conversation: ${response.statusText}`);
    }

    return response.json();
  }

  async getAnalytics(from?: Date, to?: Date): Promise<ChatAnalytics> {
    const params = new URLSearchParams();
    if (from) params.append('from', from.toISOString());
    if (to) params.append('to', to.toISOString());

    const response = await fetch(`${CHATBOT_API}/analytics?${params.toString()}`, {
      headers: this.getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error(`Failed to get analytics: ${response.statusText}`);
    }

    return response.json();
  }

  // SignalR Methods (for real-time)
  async connectSignalR(accessToken?: string): Promise<signalR.HubConnection> {
    return signalRConnection.connect(accessToken);
  }

  async disconnectSignalR(): Promise<void> {
    return signalRConnection.disconnect();
  }

  async startConversationSignalR(params: {
    userId?: string;
    sessionId?: string;
    vehicleId?: string;
    userEmail?: string;
    userName?: string;
    userPhone?: string;
    vehicleContext?: VehicleContext;
  }): Promise<Conversation> {
    const connection = signalRConnection.getConnection();
    if (!connection) {
      throw new Error('SignalR not connected');
    }
    return connection.invoke('StartConversation', params);
  }

  async sendMessageSignalR(
    conversationId: string,
    content: string,
    vehicleContext?: VehicleContext
  ): Promise<SendMessageResponse> {
    const connection = signalRConnection.getConnection();
    if (!connection) {
      throw new Error('SignalR not connected');
    }
    return connection.invoke('SendMessage', { conversationId, content, vehicleContext });
  }

  async endConversationSignalR(conversationId: string, reason: string): Promise<Conversation> {
    const connection = signalRConnection.getConnection();
    if (!connection) {
      throw new Error('SignalR not connected');
    }
    return connection.invoke('EndConversation', conversationId, reason);
  }

  onNewMessage(callback: (response: SendMessageResponse) => void): void {
    const connection = signalRConnection.getConnection();
    if (connection) {
      connection.on('NewMessage', callback);
    }
  }

  onTypingIndicator(callback: (indicator: TypingIndicator) => void): void {
    const connection = signalRConnection.getConnection();
    if (connection) {
      connection.on('TypingIndicator', callback);
    }
  }

  onTransferToAgent(callback: (data: { conversationId: string; reason: string }) => void): void {
    const connection = signalRConnection.getConnection();
    if (connection) {
      connection.on('TransferToAgent', callback);
    }
  }

  onConversationEnded(callback: (data: { conversationId: string; reason: string }) => void): void {
    const connection = signalRConnection.getConnection();
    if (connection) {
      connection.on('ConversationEnded', callback);
    }
  }

  removeAllListeners(): void {
    const connection = signalRConnection.getConnection();
    if (connection) {
      connection.off('NewMessage');
      connection.off('TypingIndicator');
      connection.off('TransferToAgent');
      connection.off('ConversationEnded');
    }
  }
}

export const chatbotService = new ChatbotService();
export default chatbotService;
