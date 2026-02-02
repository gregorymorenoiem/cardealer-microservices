/**
 * Messages Page
 *
 * Displays user's conversations (inquiries and received messages)
 *
 * Route: /cuenta/mensajes
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  MessageCircle,
  Search,
  Send,
  MoreVertical,
  Trash2,
  Archive,
  Check,
  CheckCheck,
  Loader2,
  ArrowLeft,
  Car,
  User,
  RefreshCw,
  AlertCircle,
  Inbox,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { cn } from '@/lib/utils';
import { toast } from 'sonner';
import {
  messagingService,
  type Conversation,
  type Message,
  type ConversationDetail,
} from '@/services/messaging';

// =============================================================================
// LOADING STATE
// =============================================================================

function ConversationsLoading() {
  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-gray-200 p-4">
        <Skeleton className="h-10 w-full" />
      </div>
      <div className="flex-1">
        {[1, 2, 3, 4].map(i => (
          <div key={i} className="flex gap-3 border-b border-gray-100 p-4">
            <Skeleton className="h-12 w-12 rounded-full" />
            <div className="flex-1">
              <Skeleton className="mb-2 h-4 w-32" />
              <Skeleton className="mb-2 h-3 w-48" />
              <Skeleton className="h-3 w-full" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function MessagesLoading() {
  return (
    <div className="flex h-full items-center justify-center">
      <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
    </div>
  );
}

// =============================================================================
// ERROR STATE
// =============================================================================

function ConversationsError({ onRetry }: { onRetry: () => void }) {
  return (
    <div className="flex h-full flex-col items-center justify-center p-6 text-center">
      <AlertCircle className="mb-4 h-12 w-12 text-red-400" />
      <h3 className="mb-2 font-semibold">Error al cargar</h3>
      <p className="mb-4 text-sm text-gray-500">No se pudieron cargar las conversaciones</p>
      <Button variant="outline" onClick={onRetry}>
        <RefreshCw className="mr-2 h-4 w-4" />
        Reintentar
      </Button>
    </div>
  );
}

// =============================================================================
// EMPTY STATE
// =============================================================================

function EmptyState() {
  return (
    <div className="flex h-full flex-col items-center justify-center p-6 text-center">
      <div className="mb-4 flex h-20 w-20 items-center justify-center rounded-full bg-gray-100">
        <Inbox className="h-10 w-10 text-gray-400" />
      </div>
      <h3 className="mb-2 font-semibold text-gray-900">No tienes mensajes</h3>
      <p className="mb-6 max-w-xs text-sm text-gray-500">
        Cuando contactes a vendedores o recibas consultas, aparecerán aquí.
      </p>
      <Button asChild>
        <Link href="/vehiculos">Explorar vehículos</Link>
      </Button>
    </div>
  );
}

// =============================================================================
// CONVERSATION ITEM
// =============================================================================

function ConversationItem({
  conversation,
  isSelected,
  onClick,
}: {
  conversation: Conversation;
  isSelected: boolean;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full border-b border-gray-100 p-4 text-left transition-colors hover:bg-gray-50',
        isSelected && 'bg-primary/5',
        conversation.unreadCount > 0 && 'bg-blue-50/50'
      )}
    >
      <div className="flex gap-3">
        {/* Avatar */}
        <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center overflow-hidden rounded-full bg-gray-100">
          {conversation.otherUser.avatarUrl ? (
            <Image
              src={conversation.otherUser.avatarUrl}
              alt={conversation.otherUser.name}
              width={48}
              height={48}
              className="h-full w-full object-cover"
            />
          ) : (
            <User className="h-6 w-6 text-gray-400" />
          )}
        </div>

        <div className="min-w-0 flex-1">
          <div className="flex items-center justify-between gap-2">
            <span
              className={cn(
                'truncate font-medium',
                conversation.unreadCount > 0 ? 'text-gray-900' : 'text-gray-700'
              )}
            >
              {conversation.otherUser.name}
            </span>
            <span className="flex-shrink-0 text-xs text-gray-500">
              {conversation.lastMessage
                ? messagingService.formatConversationTime(conversation.lastMessage.sentAt)
                : messagingService.formatConversationTime(conversation.createdAt)}
            </span>
          </div>

          <div className="mt-0.5 flex items-center gap-2">
            <p className="truncate text-sm text-gray-500">{conversation.vehicle.title}</p>
            <Badge variant="outline" className="shrink-0 text-xs">
              {conversation.type === 'inquiry' ? 'Enviado' : 'Recibido'}
            </Badge>
          </div>

          <div className="mt-1 flex items-center justify-between gap-2">
            <p
              className={cn(
                'truncate text-sm',
                conversation.unreadCount > 0 ? 'font-medium text-gray-900' : 'text-gray-600'
              )}
            >
              {conversation.lastMessage ? (
                <>
                  {conversation.lastMessage.isFromMe && (
                    <span className="mr-1 text-gray-400">Tú:</span>
                  )}
                  {conversation.lastMessage.content}
                </>
              ) : (
                'Sin mensajes'
              )}
            </p>
            {conversation.unreadCount > 0 && (
              <span className="bg-primary flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full text-xs text-white">
                {conversation.unreadCount}
              </span>
            )}
          </div>
        </div>
      </div>
    </button>
  );
}

// =============================================================================
// MESSAGE BUBBLE
// =============================================================================

function MessageBubble({ message }: { message: Message }) {
  return (
    <div className={cn('flex', message.isFromMe ? 'justify-end' : 'justify-start')}>
      <div
        className={cn(
          'max-w-[75%] rounded-2xl px-4 py-2',
          message.isFromMe
            ? 'bg-primary rounded-br-md text-white'
            : 'rounded-bl-md bg-gray-100 text-gray-900'
        )}
      >
        <p className="whitespace-pre-wrap">{message.content}</p>
        <div
          className={cn(
            'mt-1 flex items-center justify-end gap-1 text-xs',
            message.isFromMe ? 'text-white/70' : 'text-gray-500'
          )}
        >
          <span>{messagingService.formatMessageTime(message.sentAt)}</span>
          {message.isFromMe &&
            (message.isRead ? <CheckCheck className="h-4 w-4" /> : <Check className="h-4 w-4" />)}
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN PAGE
// =============================================================================

export default function MessagesPage() {
  const queryClient = useQueryClient();

  const [selectedConversationId, setSelectedConversationId] = React.useState<string | null>(null);
  const [selectedType, setSelectedType] = React.useState<'inquiry' | 'received'>('inquiry');
  const [newMessage, setNewMessage] = React.useState('');
  const [searchQuery, setSearchQuery] = React.useState('');
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);

  const messagesEndRef = React.useRef<HTMLDivElement>(null);

  // Fetch conversations
  const {
    data: conversations = [],
    isLoading: conversationsLoading,
    error: conversationsError,
    refetch: refetchConversations,
  } = useQuery({
    queryKey: ['conversations'],
    queryFn: messagingService.getConversations,
    staleTime: 1000 * 30, // 30 seconds
  });

  // Fetch conversation detail when selected
  const { data: conversationDetail, isLoading: detailLoading } = useQuery({
    queryKey: ['conversation', selectedConversationId, selectedType],
    queryFn: () =>
      selectedConversationId
        ? messagingService.getConversationDetail(selectedConversationId, selectedType)
        : null,
    enabled: !!selectedConversationId,
  });

  // Send message mutation
  const sendMutation = useMutation({
    mutationFn: ({ conversationId, content }: { conversationId: string; content: string }) =>
      messagingService.sendMessage(conversationId, content),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversation', selectedConversationId] });
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
      setNewMessage('');
    },
    onError: () => {
      toast.error('Error al enviar el mensaje');
    },
  });

  // Archive mutation
  const archiveMutation = useMutation({
    mutationFn: messagingService.archiveConversation,
    onSuccess: () => {
      toast.success('Conversación archivada');
      setSelectedConversationId(null);
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
    onError: () => {
      toast.error('Error al archivar');
    },
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: messagingService.deleteConversation,
    onSuccess: () => {
      toast.success('Conversación eliminada');
      setSelectedConversationId(null);
      setDeleteDialogOpen(false);
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
    onError: () => {
      toast.error('Error al eliminar');
    },
  });

  // Get selected conversation
  const selectedConversation = React.useMemo(
    () => conversations.find(c => c.id === selectedConversationId),
    [conversations, selectedConversationId]
  );

  // Scroll to bottom on new messages
  React.useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [conversationDetail?.messages]);

  // Filter conversations
  const filteredConversations = React.useMemo(() => {
    if (!searchQuery) return conversations;
    const search = searchQuery.toLowerCase();
    return conversations.filter(
      c =>
        c.otherUser.name.toLowerCase().includes(search) ||
        c.vehicle.title.toLowerCase().includes(search)
    );
  }, [conversations, searchQuery]);

  // Handle send
  const handleSend = () => {
    if (!newMessage.trim() || !selectedConversationId) return;
    sendMutation.mutate({ conversationId: selectedConversationId, content: newMessage.trim() });
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  // Handle select conversation
  const handleSelectConversation = (conversation: Conversation) => {
    setSelectedConversationId(conversation.id);
    setSelectedType(conversation.type);
  };

  // Get messages
  const messages = conversationDetail?.messages || [];

  // Count stats
  const unreadCount = conversations.reduce((sum, c) => sum + c.unreadCount, 0);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mensajes</h1>
          <p className="text-gray-600">
            {conversations.length > 0
              ? `${conversations.length} conversaciones`
              : 'Tus conversaciones aparecerán aquí'}
            {unreadCount > 0 && (
              <span className="ml-2 font-medium text-blue-600">({unreadCount} sin leer)</span>
            )}
          </p>
        </div>
      </div>

      {/* Messages Container */}
      <Card className="overflow-hidden">
        <div className="flex h-[600px]">
          {/* Conversations List */}
          <div
            className={cn(
              'flex w-full flex-col border-r border-gray-200 md:w-80',
              selectedConversationId && 'hidden md:flex'
            )}
          >
            {/* Search */}
            <div className="border-b border-gray-200 p-4">
              <div className="relative">
                <Search className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <Input
                  type="text"
                  placeholder="Buscar conversaciones..."
                  value={searchQuery}
                  onChange={e => setSearchQuery(e.target.value)}
                  className="pl-9"
                />
              </div>
            </div>

            {/* List */}
            <div className="flex-1 overflow-y-auto">
              {conversationsLoading ? (
                <ConversationsLoading />
              ) : conversationsError ? (
                <ConversationsError onRetry={refetchConversations} />
              ) : filteredConversations.length > 0 ? (
                filteredConversations.map(conv => (
                  <ConversationItem
                    key={conv.id}
                    conversation={conv}
                    isSelected={selectedConversationId === conv.id}
                    onClick={() => handleSelectConversation(conv)}
                  />
                ))
              ) : conversations.length === 0 ? (
                <EmptyState />
              ) : (
                <div className="flex flex-col items-center justify-center p-6 text-center">
                  <p className="text-gray-500">No se encontraron conversaciones</p>
                  <Button variant="link" onClick={() => setSearchQuery('')} className="mt-2">
                    Limpiar búsqueda
                  </Button>
                </div>
              )}
            </div>
          </div>

          {/* Chat Area */}
          <div className={cn('flex flex-1 flex-col', !selectedConversationId && 'hidden md:flex')}>
            {selectedConversation ? (
              <>
                {/* Chat Header */}
                <div className="flex items-center gap-3 border-b border-gray-200 p-4">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="md:hidden"
                    onClick={() => setSelectedConversationId(null)}
                  >
                    <ArrowLeft className="h-5 w-5" />
                  </Button>

                  <div className="flex h-10 w-10 items-center justify-center overflow-hidden rounded-full bg-gray-100">
                    {selectedConversation.otherUser.avatarUrl ? (
                      <Image
                        src={selectedConversation.otherUser.avatarUrl}
                        alt={selectedConversation.otherUser.name}
                        width={40}
                        height={40}
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <User className="h-5 w-5 text-gray-400" />
                    )}
                  </div>

                  <div className="min-w-0 flex-1">
                    <h3 className="truncate font-medium text-gray-900">
                      {selectedConversation.otherUser.name}
                    </h3>
                    {selectedConversation.vehicle.slug ? (
                      <Link
                        href={`/vehiculos/${selectedConversation.vehicle.slug}`}
                        className="hover:text-primary block truncate text-sm text-gray-500"
                      >
                        {selectedConversation.vehicle.title}
                      </Link>
                    ) : (
                      <p className="truncate text-sm text-gray-500">
                        {selectedConversation.vehicle.title}
                      </p>
                    )}
                  </div>

                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon">
                        <MoreVertical className="h-5 w-5" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem
                        onClick={() => archiveMutation.mutate(selectedConversationId!)}
                        disabled={archiveMutation.isPending}
                      >
                        <Archive className="mr-2 h-4 w-4" />
                        Archivar
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        className="text-red-600"
                        onClick={() => setDeleteDialogOpen(true)}
                      >
                        <Trash2 className="mr-2 h-4 w-4" />
                        Eliminar
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>

                {/* Messages */}
                <div className="flex-1 space-y-4 overflow-y-auto p-4">
                  {detailLoading ? (
                    <MessagesLoading />
                  ) : (
                    <>
                      {/* Vehicle Card */}
                      <Card className="bg-gray-50">
                        <CardContent className="p-3">
                          <Link
                            href={
                              selectedConversation.vehicle.slug
                                ? `/vehiculos/${selectedConversation.vehicle.slug}`
                                : '#'
                            }
                            className={cn(
                              'flex gap-3 transition-opacity',
                              selectedConversation.vehicle.slug && 'hover:opacity-90'
                            )}
                          >
                            <div className="flex h-12 w-16 items-center justify-center overflow-hidden rounded bg-gray-200">
                              {selectedConversation.vehicle.imageUrl ? (
                                <Image
                                  src={selectedConversation.vehicle.imageUrl}
                                  alt={selectedConversation.vehicle.title}
                                  width={64}
                                  height={48}
                                  className="h-full w-full object-cover"
                                />
                              ) : (
                                <Car className="h-6 w-6 text-gray-400" />
                              )}
                            </div>
                            <div>
                              <p className="text-sm font-medium text-gray-900">
                                {selectedConversation.vehicle.title}
                              </p>
                              {selectedConversation.vehicle.price > 0 && (
                                <p className="text-primary text-sm font-semibold">
                                  {new Intl.NumberFormat('es-DO', {
                                    style: 'currency',
                                    currency: 'DOP',
                                    maximumFractionDigits: 0,
                                  }).format(selectedConversation.vehicle.price)}
                                </p>
                              )}
                            </div>
                          </Link>
                        </CardContent>
                      </Card>

                      {/* Messages */}
                      {messages.length > 0 ? (
                        messages.map(message => (
                          <MessageBubble key={message.id} message={message} />
                        ))
                      ) : (
                        <div className="py-8 text-center text-sm text-gray-500">
                          No hay mensajes en esta conversación
                        </div>
                      )}
                      <div ref={messagesEndRef} />
                    </>
                  )}
                </div>

                {/* Input */}
                <div className="border-t border-gray-200 p-4">
                  <div className="flex gap-2">
                    <Input
                      type="text"
                      placeholder="Escribe un mensaje..."
                      value={newMessage}
                      onChange={e => setNewMessage(e.target.value)}
                      onKeyDown={handleKeyDown}
                      disabled={sendMutation.isPending}
                      className="flex-1"
                    />
                    <Button
                      onClick={handleSend}
                      disabled={!newMessage.trim() || sendMutation.isPending}
                    >
                      {sendMutation.isPending ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : (
                        <Send className="h-4 w-4" />
                      )}
                    </Button>
                  </div>
                </div>
              </>
            ) : (
              <div className="flex flex-1 items-center justify-center text-gray-500">
                <div className="text-center">
                  <MessageCircle className="mx-auto mb-3 h-12 w-12 text-gray-300" />
                  <p>Selecciona una conversación</p>
                </div>
              </div>
            )}
          </div>
        </div>
      </Card>

      {/* Delete Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar conversación?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. Se eliminará la conversación y todos los mensajes.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={() =>
                selectedConversationId && deleteMutation.mutate(selectedConversationId)
              }
              className="bg-red-600 hover:bg-red-700"
              disabled={deleteMutation.isPending}
            >
              {deleteMutation.isPending ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
