import React, { useState, useEffect, useRef } from 'react';
import { FiX, FiMinimize2 } from 'react-icons/fi';
import { useAuth } from '@/hooks/useAuth';
import { chatbotService, type ConversationDto, type MessageDto } from '@/services/chatbotService';
import MessageList from './MessageList';
import MessageInput from './MessageInput';
import LeadScoreIndicator from './LeadScoreIndicator';
import WhatsAppHandoffButton from './WhatsAppHandoffButton';

interface ChatWindowProps {
  vehicleId?: string;
  vehicleTitle?: string;
  vehiclePrice?: number;
  dealerId?: string;
  dealerName?: string;
  dealerWhatsApp?: string;
  conversationId?: string; // Si ya existe una conversación
  onClose: () => void;
  onNewMessage?: () => void;
}

export const ChatWindow: React.FC<ChatWindowProps> = ({
  vehicleId,
  vehicleTitle,
  vehiclePrice,
  dealerId,
  dealerName,
  dealerWhatsApp,
  conversationId: initialConversationId,
  onClose,
  onNewMessage,
}) => {
  const { user } = useAuth();
  const [conversation, setConversation] = useState<ConversationDto | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if (!user || hasInitialized.current) return;
    hasInitialized.current = true;

    initializeChat();

    return () => {
      cleanup();
    };
  }, [user, initialConversationId]);

  const initializeChat = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Conectar a SignalR Hub
      await chatbotService.connectToHub();
      setIsConnected(true);

      // Suscribirse a eventos
      chatbotService.onMessageReceived(handleMessageReceived);
      chatbotService.onHandoffRecommended(handleHandoffRecommended);

      let conv: ConversationDto;

      if (initialConversationId) {
        // Cargar conversación existente
        conv = await chatbotService.getConversation(initialConversationId);
        const msgs = await chatbotService.getMessages(initialConversationId);
        setMessages(msgs);
      } else {
        // Crear nueva conversación
        conv = await chatbotService.startConversation({
          userId: user!.id,
          userName: user!.fullName || user!.email,
          userEmail: user!.email,
          userPhone: user?.phone || undefined,
          vehicleId,
          vehicleTitle,
          vehiclePrice,
          dealerId,
          dealerName,
          dealerWhatsApp,
          initialMessage: chatbotService.constructor.generateWelcomeMessage(
            vehicleTitle,
            dealerName
          ),
        });
        setMessages([]);
      }

      setConversation(conv);

      // Unirse a la conversación (SignalR group)
      await chatbotService.joinConversation(conv.id);
    } catch (err: any) {
      console.error('Error initializing chat:', err);
      setError(err.message || 'Error al iniciar el chat');
    } finally {
      setIsLoading(false);
    }
  };

  const cleanup = async () => {
    if (conversation) {
      await chatbotService.leaveConversation(conversation.id);
    }
    chatbotService.offAllEvents();
    await chatbotService.disconnectFromHub();
  };

  const handleMessageReceived = (event: any) => {
    if (event.conversationId === conversation?.id) {
      setMessages((prev) => [...prev, event.message]);
      onNewMessage?.();

      // Actualizar conversation si el message incluye metadata de scoring
      if (event.message.metadata?.leadScore !== undefined) {
        setConversation((prev) =>
          prev
            ? {
                ...prev,
                leadScore: event.message.metadata.leadScore,
                leadTemperature: event.message.metadata.leadTemperature,
              }
            : null
        );
      }
    }
  };

  const handleHandoffRecommended = (event: any) => {
    if (event.conversationId === conversation?.id) {
      // Mostrar notificación de handoff recomendado
      console.log('Handoff recommended:', event);
      // Aquí podrías mostrar un toast o modal
    }
  };

  const handleSendMessage = async (content: string) => {
    if (!conversation) return;

    try {
      // Agregar mensaje del usuario inmediatamente (optimistic update)
      const tempMessage: MessageDto = {
        id: `temp-${Date.now()}`,
        conversationId: conversation.id,
        role: 'User' as any,
        type: 'Text' as any,
        content,
        metadata: null,
        sentAt: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, tempMessage]);

      // Enviar vía API (el backend responderá vía SignalR)
      await chatbotService.sendMessage(conversation.id, { content });
    } catch (err: any) {
      console.error('Error sending message:', err);
      setError('Error al enviar mensaje');
      // Remover mensaje temporal si falla
      setMessages((prev) => prev.filter((m) => m.id !== tempMessage.id));
    }
  };

  const handleTyping = async () => {
    if (!conversation) return;
    try {
      await chatbotService.sendTypingIndicator(conversation.id);
    } catch (err) {
      console.error('Error sending typing indicator:', err);
    }
  };

  const handleHandoff = async () => {
    if (!conversation) return;

    try {
      await chatbotService.handoffToWhatsApp(conversation.id, {
        method: 'WhatsApp',
        notes: `Lead Score: ${conversation.leadScore}, Temperature: ${conversation.leadTemperature}`,
      });

      // Actualizar conversation
      setConversation((prev) =>
        prev
          ? {
              ...prev,
              handoffInitiatedAt: new Date().toISOString(),
              handoffMethod: 'WhatsApp',
            }
          : null
      );

      alert('¡Solicitud enviada! El dealer te contactará por WhatsApp pronto.');
    } catch (err: any) {
      console.error('Error initiating handoff:', err);
      alert('Error al iniciar handoff');
    }
  };

  if (isLoading) {
    return (
      <div className="bg-white h-full flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Iniciando chat...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white h-full flex items-center justify-center p-6">
        <div className="text-center">
          <p className="text-red-600 mb-4">{error}</p>
          <button
            onClick={initializeChat}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
          >
            Reintentar
          </button>
        </div>
      </div>
    );
  }

  if (!conversation) return null;

  return (
    <div className="bg-white h-full flex flex-col">
      {/* Header */}
      <div className="bg-blue-600 text-white p-4 flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="w-10 h-10 bg-blue-700 rounded-full flex items-center justify-center">
            <span className="text-lg font-bold">🤖</span>
          </div>
          <div>
            <h3 className="font-semibold">Asistente OKLA</h3>
            <p className="text-xs text-blue-200">{isConnected ? 'Conectado' : 'Desconectado'}</p>
          </div>
        </div>
        <div className="flex items-center space-x-2">
          {/* Lead Score Badge (solo visible para dealers o en debug) */}
          {user?.accountType === 'Dealer' && (
            <LeadScoreIndicator
              score={conversation.leadScore}
              temperature={conversation.leadTemperature}
            />
          )}
          <button
            onClick={onClose}
            className="text-white hover:text-blue-200 transition-colors"
            aria-label="Cerrar chat"
          >
            <FiX className="w-5 h-5" />
          </button>
        </div>
      </div>

      {/* Vehicle Info (si aplica) */}
      {vehicleTitle && (
        <div className="bg-gray-50 border-b border-gray-200 p-3">
          <p className="text-sm text-gray-700">
            <span className="font-semibold">Consultando:</span> {vehicleTitle}
          </p>
          {vehiclePrice && (
            <p className="text-xs text-gray-500 mt-1">${vehiclePrice.toLocaleString('es-DO')}</p>
          )}
        </div>
      )}

      {/* Messages */}
      <MessageList messages={messages} />

      {/* Handoff Button (si lead es HOT) */}
      {chatbotService.constructor.shouldTriggerHandoff(conversation.leadScore) &&
        !conversation.handoffInitiatedAt && (
          <div className="p-3 bg-yellow-50 border-t border-yellow-200">
            <WhatsAppHandoffButton
              onHandoff={handleHandoff}
              dealerName={dealerName || 'el dealer'}
              leadScore={conversation.leadScore}
            />
          </div>
        )}

      {/* Handoff Status */}
      {conversation.handoffInitiatedAt && (
        <div className="p-3 bg-green-50 border-t border-green-200">
          <p className="text-sm text-green-800 text-center">
            ✅ Solicitud enviada. {dealerName || 'El dealer'} te contactará pronto por WhatsApp.
          </p>
        </div>
      )}

      {/* Input */}
      <MessageInput onSend={handleSendMessage} onTyping={handleTyping} />
    </div>
  );
};

export default ChatWindow;
