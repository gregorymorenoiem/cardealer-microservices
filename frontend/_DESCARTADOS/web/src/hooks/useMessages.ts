import { useState, useCallback } from 'react';
import type { Conversation, Message, Notification } from '../types/message';
import { mockConversations, mockNotifications } from '../data/mockMessages';

export const useMessages = () => {
  const [conversations, setConversations] = useState<Conversation[]>(mockConversations);
  const [selectedConversation, setSelectedConversation] = useState<Conversation | null>(null);

  const selectConversation = useCallback((conversationId: string) => {
    const conversation = conversations.find((c) => c.id === conversationId);
    if (conversation) {
      // Mark messages as read
      const updatedConversation = {
        ...conversation,
        unreadCount: 0,
        messages: conversation.messages.map((m) => ({ ...m, read: true })),
      };
      
      setConversations((prev) =>
        prev.map((c) => (c.id === conversationId ? updatedConversation : c))
      );
      
      setSelectedConversation(updatedConversation);
    }
  }, [conversations]);

  const sendMessage = useCallback((conversationId: string, content: string) => {
    const newMessage: Message = {
      id: `msg-${Date.now()}`,
      conversationId,
      senderId: 'buyer-1',
      senderName: 'You',
      content,
      timestamp: new Date().toISOString(),
      read: true,
    };

    setConversations((prev) =>
      prev.map((c) => {
        if (c.id === conversationId) {
          const updatedConversation = {
            ...c,
            messages: [...c.messages, newMessage],
            lastMessage: content,
            lastMessageTime: newMessage.timestamp,
          };
          
          if (selectedConversation?.id === conversationId) {
            setSelectedConversation(updatedConversation);
          }
          
          return updatedConversation;
        }
        return c;
      })
    );
  }, [selectedConversation]);

  const searchConversations = useCallback((query: string) => {
    if (!query.trim()) return conversations;
    
    const lowerQuery = query.toLowerCase();
    return conversations.filter(
      (c) =>
        c.vehicleTitle.toLowerCase().includes(lowerQuery) ||
        c.sellerName.toLowerCase().includes(lowerQuery) ||
        c.lastMessage.toLowerCase().includes(lowerQuery)
    );
  }, [conversations]);

  const unreadCount = conversations.reduce((sum, c) => sum + c.unreadCount, 0);

  return {
    conversations,
    selectedConversation,
    selectConversation,
    sendMessage,
    searchConversations,
    unreadCount,
  };
};

export const useNotifications = () => {
  const [notifications, setNotifications] = useState<Notification[]>(mockNotifications);

  const markAsRead = useCallback((notificationId: string) => {
    setNotifications((prev) =>
      prev.map((n) => (n.id === notificationId ? { ...n, read: true } : n))
    );
  }, []);

  const markAllAsRead = useCallback(() => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  }, []);

  const unreadCount = notifications.filter((n) => !n.read).length;

  return {
    notifications,
    markAsRead,
    markAllAsRead,
    unreadCount,
  };
};
