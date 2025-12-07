// Message types for Sprint 7

export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  content: string;
  timestamp: string;
  read: boolean;
}

export interface Conversation {
  id: string;
  vehicleId: string;
  vehicleTitle: string;
  vehicleImage: string;
  vehiclePrice: number;
  sellerId: string;
  sellerName: string;
  sellerAvatar?: string;
  buyerId: string;
  buyerName: string;
  buyerAvatar?: string;
  lastMessage: string;
  lastMessageTime: string;
  unreadCount: number;
  status: 'active' | 'archived';
  messages: Message[];
}

export interface ContactSellerFormData {
  name: string;
  email: string;
  phone?: string;
  message: string;
}

export interface Notification {
  id: string;
  type: 'message' | 'listing' | 'favorite' | 'review' | 'system';
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  actionUrl?: string;
  icon?: string;
}

export interface EmailPreferences {
  newMessages: boolean;
  listingUpdates: boolean;
  priceDrops: boolean;
  weeklyDigest: boolean;
  marketingEmails: boolean;
}
