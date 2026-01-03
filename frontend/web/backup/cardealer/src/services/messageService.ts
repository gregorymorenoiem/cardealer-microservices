import axios from 'axios';

const MESSAGE_API_URL = import.meta.env.VITE_MESSAGE_SERVICE_URL || 'http://localhost:5004/api';

export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  content: string;
  isRead: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Conversation {
  id: string;
  vehicleId: string;
  vehicleTitle: string;
  vehicleImage?: string;
  buyerId: string;
  buyerName: string;
  buyerAvatar?: string;
  sellerId: string;
  sellerName: string;
  sellerAvatar?: string;
  lastMessage: string;
  lastMessageTime: string;
  unreadCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface ConversationDetail extends Conversation {
  messages: Message[];
}

// Get all conversations for current user
export const getConversations = async (): Promise<Conversation[]> => {
  try {
    const response = await axios.get(`${MESSAGE_API_URL}/conversations`);
    return response.data;
  } catch (error) {
    console.error('Error fetching conversations:', error);
    throw new Error('Failed to fetch conversations');
  }
};

// Get conversation by ID with all messages
export const getConversationById = async (id: string): Promise<ConversationDetail> => {
  try {
    const response = await axios.get(`${MESSAGE_API_URL}/conversations/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching conversation:', error);
    throw new Error('Failed to fetch conversation details');
  }
};

// Start new conversation (or get existing one)
export const startConversation = async (vehicleId: string, sellerId: string): Promise<Conversation> => {
  try {
    const response = await axios.post(`${MESSAGE_API_URL}/conversations`, {
      vehicleId,
      sellerId,
    });
    return response.data;
  } catch (error) {
    console.error('Error starting conversation:', error);
    throw new Error('Failed to start conversation');
  }
};

// Send message in conversation
export const sendMessage = async (conversationId: string, content: string): Promise<Message> => {
  try {
    const response = await axios.post(`${MESSAGE_API_URL}/conversations/${conversationId}/messages`, {
      content,
    });
    return response.data;
  } catch (error) {
    console.error('Error sending message:', error);
    throw new Error('Failed to send message');
  }
};

// Mark message as read
export const markMessageAsRead = async (messageId: string): Promise<void> => {
  try {
    await axios.patch(`${MESSAGE_API_URL}/messages/${messageId}/read`);
  } catch (error) {
    console.error('Error marking message as read:', error);
    throw new Error('Failed to mark message as read');
  }
};

// Mark all messages in conversation as read
export const markConversationAsRead = async (conversationId: string): Promise<void> => {
  try {
    await axios.patch(`${MESSAGE_API_URL}/conversations/${conversationId}/read`);
  } catch (error) {
    console.error('Error marking conversation as read:', error);
    throw new Error('Failed to mark conversation as read');
  }
};

// Delete message (soft delete)
export const deleteMessage = async (messageId: string): Promise<void> => {
  try {
    await axios.delete(`${MESSAGE_API_URL}/messages/${messageId}`);
  } catch (error) {
    console.error('Error deleting message:', error);
    throw new Error('Failed to delete message');
  }
};

// Delete conversation (soft delete)
export const deleteConversation = async (conversationId: string): Promise<void> => {
  try {
    await axios.delete(`${MESSAGE_API_URL}/conversations/${conversationId}`);
  } catch (error) {
    console.error('Error deleting conversation:', error);
    throw new Error('Failed to delete conversation');
  }
};

// Get unread message count
export const getUnreadCount = async (): Promise<number> => {
  try {
    const response = await axios.get(`${MESSAGE_API_URL}/messages/unread/count`);
    return response.data.count;
  } catch (error) {
    console.error('Error fetching unread count:', error);
    return 0;
  }
};

// Search conversations
export const searchConversations = async (query: string): Promise<Conversation[]> => {
  try {
    const response = await axios.get(`${MESSAGE_API_URL}/conversations/search?q=${encodeURIComponent(query)}`);
    return response.data;
  } catch (error) {
    console.error('Error searching conversations:', error);
    throw new Error('Failed to search conversations');
  }
};

// Get messages with pagination
export const getMessages = async (
  conversationId: string,
  page: number = 1,
  pageSize: number = 50
): Promise<{
  messages: Message[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}> => {
  try {
    const response = await axios.get(
      `${MESSAGE_API_URL}/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`
    );
    return response.data;
  } catch (error) {
    console.error('Error fetching messages:', error);
    throw new Error('Failed to fetch messages');
  }
};

// Block user
export const blockUser = async (userId: string): Promise<void> => {
  try {
    await axios.post(`${MESSAGE_API_URL}/users/${userId}/block`);
  } catch (error) {
    console.error('Error blocking user:', error);
    throw new Error('Failed to block user');
  }
};

// Unblock user
export const unblockUser = async (userId: string): Promise<void> => {
  try {
    await axios.delete(`${MESSAGE_API_URL}/users/${userId}/block`);
  } catch (error) {
    console.error('Error unblocking user:', error);
    throw new Error('Failed to unblock user');
  }
};

// Get blocked users
export const getBlockedUsers = async (): Promise<Array<{
  id: string;
  name: string;
  avatar?: string;
  blockedAt: string;
}>> => {
  try {
    const response = await axios.get(`${MESSAGE_API_URL}/users/blocked`);
    return response.data;
  } catch (error) {
    console.error('Error fetching blocked users:', error);
    throw new Error('Failed to fetch blocked users');
  }
};

// Report conversation/message
export const reportConversation = async (
  conversationId: string,
  reason: string,
  details?: string
): Promise<void> => {
  try {
    await axios.post(`${MESSAGE_API_URL}/conversations/${conversationId}/report`, {
      reason,
      details,
    });
  } catch (error) {
    console.error('Error reporting conversation:', error);
    throw new Error('Failed to report conversation');
  }
};

// Get message statistics (admin)
export const getMessageStats = async (): Promise<{
  totalConversations: number;
  totalMessages: number;
  activeConversations: number;
  averageResponseTime: number;
  reportsCount: number;
}> => {
  try {
    const response = await axios.get(`${MESSAGE_API_URL}/admin/messages/stats`);
    return response.data;
  } catch (error) {
    console.error('Error fetching message stats:', error);
    throw new Error('Failed to fetch message statistics');
  }
};

// Get all reported conversations (admin)
export const getReportedConversations = async (): Promise<Array<{
  id: string;
  conversationId: string;
  reportedBy: string;
  reportedUser: string;
  reason: string;
  details?: string;
  status: 'pending' | 'reviewed' | 'resolved';
  createdAt: string;
}>> => {
  try {
    const response = await axios.get(`${MESSAGE_API_URL}/admin/reports`);
    return response.data;
  } catch (error) {
    console.error('Error fetching reported conversations:', error);
    throw new Error('Failed to fetch reported conversations');
  }
};

// Typing indicator (real-time)
export const sendTypingIndicator = async (conversationId: string): Promise<void> => {
  try {
    await axios.post(`${MESSAGE_API_URL}/conversations/${conversationId}/typing`);
  } catch {
    // Silently fail - typing indicators are not critical
    console.debug('Failed to send typing indicator');
  }
};

export default {
  getConversations,
  getConversationById,
  startConversation,
  sendMessage,
  markMessageAsRead,
  markConversationAsRead,
  deleteMessage,
  deleteConversation,
  getUnreadCount,
  searchConversations,
  getMessages,
  blockUser,
  unblockUser,
  getBlockedUsers,
  reportConversation,
  getMessageStats,
  getReportedConversations,
  sendTypingIndicator,
};
