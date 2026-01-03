/**
 * Messaging TanStack Query Hooks
 * 
 * Provides hooks for messaging/chat operations between users.
 * Uses messageService.ts for API calls.
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { UseQueryOptions } from '@tanstack/react-query';
import {
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
  type Conversation,
  type ConversationDetail,
  type Message,
} from '@/services/messageService';

// ============================================================================
// QUERY KEYS
// ============================================================================

export const messagingKeys = {
  all: ['messaging'] as const,
  // Conversations
  conversations: () => [...messagingKeys.all, 'conversations'] as const,
  conversationsList: () => [...messagingKeys.conversations(), 'list'] as const,
  conversationsSearch: (query: string) => [...messagingKeys.conversations(), 'search', query] as const,
  conversationDetail: (id: string) => [...messagingKeys.conversations(), 'detail', id] as const,
  // Messages
  messages: () => [...messagingKeys.all, 'messages'] as const,
  messagesList: (conversationId: string, page?: number) => 
    [...messagingKeys.messages(), 'list', conversationId, { page }] as const,
  unreadCount: () => [...messagingKeys.messages(), 'unreadCount'] as const,
  // Users
  blockedUsers: () => [...messagingKeys.all, 'blockedUsers'] as const,
  // Admin
  admin: () => [...messagingKeys.all, 'admin'] as const,
  adminStats: () => [...messagingKeys.admin(), 'stats'] as const,
  adminReports: () => [...messagingKeys.admin(), 'reports'] as const,
};

// ============================================================================
// CONVERSATIONS HOOKS
// ============================================================================

export function useConversations(options?: Partial<UseQueryOptions<Conversation[]>>) {
  return useQuery({
    queryKey: messagingKeys.conversationsList(),
    queryFn: getConversations,
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // Refresh every minute for new messages
    ...options,
  });
}

export function useConversation(id: string, options?: Partial<UseQueryOptions<ConversationDetail>>) {
  return useQuery({
    queryKey: messagingKeys.conversationDetail(id),
    queryFn: () => getConversationById(id),
    enabled: !!id,
    staleTime: 10 * 1000, // 10 seconds - messages change frequently
    refetchInterval: 10 * 1000, // Polling for new messages
    ...options,
  });
}

export function useSearchConversations(query: string, options?: Partial<UseQueryOptions<Conversation[]>>) {
  return useQuery({
    queryKey: messagingKeys.conversationsSearch(query),
    queryFn: () => searchConversations(query),
    enabled: query.length >= 2,
    staleTime: 30 * 1000,
    ...options,
  });
}

export function useStartConversation() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ vehicleId, sellerId }: { vehicleId: string; sellerId: string }) => 
      startConversation(vehicleId, sellerId),
    onSuccess: (newConversation) => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversations() });
      // Optionally set the new conversation in cache
      queryClient.setQueryData(
        messagingKeys.conversationDetail(newConversation.id),
        { ...newConversation, messages: [] }
      );
    },
  });
}

export function useDeleteConversation() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (conversationId: string) => deleteConversation(conversationId),
    onSuccess: (_, conversationId) => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversations() });
      queryClient.removeQueries({ queryKey: messagingKeys.conversationDetail(conversationId) });
    },
  });
}

// ============================================================================
// MESSAGES HOOKS
// ============================================================================

interface PaginatedMessages {
  messages: Message[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

export function useMessages(
  conversationId: string, 
  page: number = 1, 
  pageSize: number = 50,
  options?: Partial<UseQueryOptions<PaginatedMessages>>
) {
  return useQuery({
    queryKey: messagingKeys.messagesList(conversationId, page),
    queryFn: () => getMessages(conversationId, page, pageSize),
    enabled: !!conversationId,
    staleTime: 10 * 1000,
    ...options,
  });
}

export function useUnreadMessageCount(options?: Partial<UseQueryOptions<number>>) {
  return useQuery({
    queryKey: messagingKeys.unreadCount(),
    queryFn: getUnreadCount,
    staleTime: 30 * 1000,
    refetchInterval: 30 * 1000, // Poll every 30 seconds
    ...options,
  });
}

export function useSendMessage() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ conversationId, content }: { conversationId: string; content: string }) => 
      sendMessage(conversationId, content),
    onMutate: async ({ conversationId, content }) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: messagingKeys.conversationDetail(conversationId) });
      
      // Snapshot the previous value
      const previousConversation = queryClient.getQueryData<ConversationDetail>(
        messagingKeys.conversationDetail(conversationId)
      );
      
      // Optimistically update
      if (previousConversation) {
        const optimisticMessage: Message = {
          id: `temp-${Date.now()}`,
          conversationId,
          senderId: 'current-user', // Will be replaced by actual
          senderName: 'You',
          content,
          isRead: true,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };
        
        queryClient.setQueryData<ConversationDetail>(
          messagingKeys.conversationDetail(conversationId),
          {
            ...previousConversation,
            messages: [...previousConversation.messages, optimisticMessage],
            lastMessage: content,
            lastMessageTime: optimisticMessage.createdAt,
          }
        );
      }
      
      return { previousConversation };
    },
    onError: (_, { conversationId }, context) => {
      // Rollback on error
      if (context?.previousConversation) {
        queryClient.setQueryData(
          messagingKeys.conversationDetail(conversationId),
          context.previousConversation
        );
      }
    },
    onSettled: (_, __, { conversationId }) => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversationDetail(conversationId) });
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversations() });
    },
  });
}

export function useMarkMessageAsRead() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (messageId: string) => markMessageAsRead(messageId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.unreadCount() });
    },
  });
}

export function useMarkConversationAsRead() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (conversationId: string) => markConversationAsRead(conversationId),
    onSuccess: (_, conversationId) => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.unreadCount() });
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversationDetail(conversationId) });
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversations() });
    },
  });
}

export function useDeleteMessage() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (messageId: string) => deleteMessage(messageId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.messages() });
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversations() });
    },
  });
}

// ============================================================================
// USER BLOCKING HOOKS
// ============================================================================

export function useBlockedUsers(options?: Partial<UseQueryOptions<Array<{
  id: string;
  name: string;
  avatar?: string;
  blockedAt: string;
}>>>) {
  return useQuery({
    queryKey: messagingKeys.blockedUsers(),
    queryFn: getBlockedUsers,
    staleTime: 5 * 60 * 1000, // 5 minutes
    ...options,
  });
}

export function useBlockUser() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (userId: string) => blockUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.blockedUsers() });
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversations() });
    },
  });
}

export function useUnblockUser() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (userId: string) => unblockUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: messagingKeys.blockedUsers() });
    },
  });
}

// ============================================================================
// REPORTING HOOKS
// ============================================================================

export function useReportConversation() {
  return useMutation({
    mutationFn: ({ conversationId, reason, details }: { 
      conversationId: string; 
      reason: string; 
      details?: string; 
    }) => reportConversation(conversationId, reason, details),
  });
}

// ============================================================================
// ADMIN HOOKS
// ============================================================================

interface MessageStats {
  totalConversations: number;
  totalMessages: number;
  activeConversations: number;
  averageResponseTime: number;
  reportsCount: number;
}

interface Report {
  id: string;
  conversationId: string;
  reportedBy: string;
  reportedUser: string;
  reason: string;
  details?: string;
  status: 'pending' | 'reviewed' | 'resolved';
  createdAt: string;
}

export function useMessageStats(options?: Partial<UseQueryOptions<MessageStats>>) {
  return useQuery({
    queryKey: messagingKeys.adminStats(),
    queryFn: getMessageStats,
    staleTime: 2 * 60 * 1000, // 2 minutes
    ...options,
  });
}

export function useReportedConversations(options?: Partial<UseQueryOptions<Report[]>>) {
  return useQuery({
    queryKey: messagingKeys.adminReports(),
    queryFn: getReportedConversations,
    staleTime: 60 * 1000,
    ...options,
  });
}

// ============================================================================
// TYPING INDICATOR HOOK
// ============================================================================

export function useSendTypingIndicator() {
  return useMutation({
    mutationFn: (conversationId: string) => sendTypingIndicator(conversationId),
    // No need to invalidate anything - typing indicators are ephemeral
  });
}

// ============================================================================
// COMPOSITE HOOKS
// ============================================================================

/**
 * Hook for Messages Inbox page
 */
export function useMessagesInbox() {
  const conversations = useConversations();
  const unreadCount = useUnreadMessageCount();
  const startConversationMutation = useStartConversation();
  const deleteConversationMutation = useDeleteConversation();
  const markAsRead = useMarkConversationAsRead();

  return {
    conversations: conversations.data ?? [],
    unreadCount: unreadCount.data ?? 0,
    isLoading: conversations.isLoading,
    isError: conversations.isError,
    startConversation: startConversationMutation.mutate,
    deleteConversation: deleteConversationMutation.mutate,
    markAsRead: markAsRead.mutate,
    isStarting: startConversationMutation.isPending,
    isDeleting: deleteConversationMutation.isPending,
    refetch: conversations.refetch,
  };
}

/**
 * Hook for Chat Window/Conversation Detail
 */
export function useChatWindow(conversationId: string) {
  const conversation = useConversation(conversationId);
  const sendMessageMutation = useSendMessage();
  const markAsRead = useMarkConversationAsRead();
  const sendTyping = useSendTypingIndicator();
  const blockUserMutation = useBlockUser();
  const reportMutation = useReportConversation();

  // Mark as read when opening
  const markCurrentAsRead = () => {
    if (conversationId) {
      markAsRead.mutate(conversationId);
    }
  };

  return {
    conversation: conversation.data,
    messages: (conversation.data as ConversationDetail | undefined)?.messages ?? [],
    isLoading: conversation.isLoading,
    isError: conversation.isError,
    sendMessage: (content: string) => sendMessageMutation.mutate({ conversationId, content }),
    sendTypingIndicator: () => sendTyping.mutate(conversationId),
    markAsRead: markCurrentAsRead,
    blockUser: (userId: string) => blockUserMutation.mutate(userId),
    reportConversation: (reason: string, details?: string) => 
      reportMutation.mutate({ conversationId, reason, details }),
    isSending: sendMessageMutation.isPending,
    isBlocking: blockUserMutation.isPending,
    isReporting: reportMutation.isPending,
    refetch: conversation.refetch,
  };
}

/**
 * Hook for Admin Messages Dashboard
 */
export function useAdminMessaging() {
  const stats = useMessageStats();
  const reports = useReportedConversations();

  return {
    stats: stats.data,
    reports: reports.data ?? [],
    isLoading: stats.isLoading || reports.isLoading,
    isError: stats.isError || reports.isError,
    refetch: () => {
      stats.refetch();
      reports.refetch();
    },
  };
}

export default {
  keys: messagingKeys,
  // Conversations
  useConversations,
  useConversation,
  useSearchConversations,
  useStartConversation,
  useDeleteConversation,
  // Messages
  useMessages,
  useUnreadMessageCount,
  useSendMessage,
  useMarkMessageAsRead,
  useMarkConversationAsRead,
  useDeleteMessage,
  // Blocking
  useBlockedUsers,
  useBlockUser,
  useUnblockUser,
  // Reporting
  useReportConversation,
  // Admin
  useMessageStats,
  useReportedConversations,
  // Typing
  useSendTypingIndicator,
  // Composite
  useMessagesInbox,
  useChatWindow,
  useAdminMessaging,
};
