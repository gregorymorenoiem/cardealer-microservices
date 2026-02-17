import React, { useState, useEffect, useCallback, useRef } from 'react';
import { FiMessageCircle, FiX, FiSend, FiMinimize2, FiMaximize2 } from 'react-icons/fi';
import {
  chatbotService,
  type ConversationDto,
  type MessageDto,
  MessageRole,
} from '@/services/chatbotService';

// Types for this component
interface VehicleContext {
  vehicleId: string;
  make: string;
  model: string;
  year: number;
  price: number;
  dealerId?: string;
  dealerName?: string;
  dealerWhatsApp?: string;
}

interface QuickReply {
  id: string;
  label: string;
  value: string;
}

interface ChatWidgetProps {
  vehicleContext?: VehicleContext;
  position?: 'bottom-right' | 'bottom-left';
  primaryColor?: string;
}

export default function ChatWidget({
  vehicleContext,
  position = 'bottom-right',
  primaryColor = '#2563eb',
}: ChatWidgetProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [isMinimized, setIsMinimized] = useState(false);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [conversation, setConversation] = useState<ConversationDto | null>(null);
  const [quickReplies, setQuickReplies] = useState<QuickReply[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isConnecting, setIsConnecting] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Auto-scroll to bottom
  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messages, scrollToBottom]);

  // Initialize conversation when chat opens
  const initializeChat = useCallback(async () => {
    if (conversation) return;

    setIsConnecting(true);
    setError(null);

    try {
      // Get or create session/user info
      const userId = localStorage.getItem('userId') || getOrCreateSessionId();
      const userName = localStorage.getItem('userName') || undefined;
      const userEmail = localStorage.getItem('userEmail') || undefined;

      // Start conversation via REST API
      const newConversation = await chatbotService.startConversation({
        userId,
        userName,
        userEmail,
        vehicleId: vehicleContext?.vehicleId,
        vehicleTitle: vehicleContext
          ? `${vehicleContext.year} ${vehicleContext.make} ${vehicleContext.model}`
          : undefined,
        vehiclePrice: vehicleContext?.price,
        dealerId: vehicleContext?.dealerId,
        dealerName: vehicleContext?.dealerName,
        dealerWhatsApp: vehicleContext?.dealerWhatsApp,
      });

      setConversation(newConversation);

      // Try to connect to SignalR for real-time updates
      try {
        await chatbotService.connectToHub();
        await chatbotService.joinConversation(newConversation.id);

        // Setup typing indicator listener
        chatbotService.onTypingIndicator((event) => {
          if (event.conversationId === newConversation.id) {
            setIsTyping(event.isTyping);
          }
        });

        // Setup message received listener
        chatbotService.onMessageReceived((event) => {
          if (event.conversationId === newConversation.id) {
            setMessages((prev) => [...prev, event.message]);
          }
        });
      } catch (hubError) {
        console.log('SignalR connection failed, using REST only:', hubError);
      }

      // Get existing messages if any
      const existingMessages = await chatbotService.getMessages(newConversation.id);
      if (existingMessages.length > 0) {
        setMessages(existingMessages);
      } else {
        // Add welcome message
        const welcomeMessage: MessageDto = {
          id: 'welcome',
          conversationId: newConversation.id,
          role: MessageRole.Assistant,
          type: 'Text' as MessageDto['type'],
          content: vehicleContext
            ? `¡Hola! 👋 Soy el asistente de OKLA. Veo que estás viendo el **${vehicleContext.year} ${vehicleContext.make} ${vehicleContext.model}** por RD$${vehicleContext.price.toLocaleString()}. ¿Tienes alguna pregunta sobre este vehículo?`
            : '¡Hola! 👋 Soy el asistente de OKLA, el marketplace #1 de vehículos en República Dominicana. ¿En qué puedo ayudarte hoy?',
          metadata: null,
          sentAt: new Date().toISOString(),
        };
        setMessages([welcomeMessage]);
      }

      // Set initial quick replies
      setQuickReplies(getInitialQuickReplies(vehicleContext));
    } catch (err) {
      console.error('Failed to initialize chat:', err);
      setError('No se pudo conectar. Por favor, intenta de nuevo.');
    } finally {
      setIsConnecting(false);
    }
  }, [conversation, vehicleContext]);

  useEffect(() => {
    if (isOpen && !conversation) {
      initializeChat();
    }
  }, [isOpen, conversation, initializeChat]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (conversation) {
        chatbotService.leaveConversation(conversation.id).catch(() => {});
      }
      chatbotService.offAllEvents();
      chatbotService.disconnectFromHub().catch(() => {});
    };
  }, [conversation]);

  const handleSend = async () => {
    if (!inputValue.trim() || isTyping || !conversation) return;

    const userMessage: MessageDto = {
      id: Date.now().toString(),
      conversationId: conversation.id,
      role: MessageRole.User,
      type: 'Text' as MessageDto['type'],
      content: inputValue.trim(),
      metadata: null,
      sentAt: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setInputValue('');
    setIsTyping(true);
    setQuickReplies([]);
    setError(null);

    try {
      // Send message via REST API
      const response = await chatbotService.sendMessage(conversation.id, {
        content: userMessage.content || '',
        role: MessageRole.User,
      });

      setMessages((prev) => [...prev, response]);
      setIsTyping(false);

      // Update quick replies based on context
      // In a real implementation, these would come from the AI response
      setQuickReplies([]);
    } catch (err) {
      console.error('Failed to send message:', err);
      setError('No se pudo enviar el mensaje. Intenta de nuevo.');
      setIsTyping(false);
    }
  };

  const handleQuickReply = (reply: QuickReply) => {
    setInputValue(reply.value);
    setTimeout(() => {
      handleSend();
    }, 100);
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const toggleChat = () => {
    setIsOpen(!isOpen);
    if (!isOpen) {
      setIsMinimized(false);
    }
  };

  const positionClasses = position === 'bottom-right' ? 'right-4' : 'left-4';

  return (
    <>
      {/* Chat Button */}
      <button
        onClick={toggleChat}
        className={`fixed bottom-4 ${positionClasses} z-50 w-14 h-14 rounded-full shadow-lg flex items-center justify-center transition-all duration-300 hover:scale-110`}
        style={{ backgroundColor: primaryColor }}
        aria-label={isOpen ? 'Cerrar chat' : 'Abrir chat'}
      >
        {isOpen ? (
          <FiX className="w-6 h-6 text-white" />
        ) : (
          <FiMessageCircle className="w-6 h-6 text-white" />
        )}
      </button>

      {/* Chat Window */}
      {isOpen && (
        <div
          className={`fixed ${positionClasses} bottom-20 z-50 w-96 max-w-[calc(100vw-2rem)] bg-white rounded-2xl shadow-2xl flex flex-col overflow-hidden transition-all duration-300`}
          style={{
            height: isMinimized ? '60px' : '500px',
            maxHeight: 'calc(100vh - 120px)',
          }}
        >
          {/* Header */}
          <div
            className="flex items-center justify-between px-4 py-3 text-white"
            style={{ backgroundColor: primaryColor }}
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-white/20 rounded-full flex items-center justify-center">
                <span className="text-xl">🚗</span>
              </div>
              <div>
                <h3 className="font-semibold">OKLA Assistant</h3>
                <span className="text-xs opacity-80">
                  {isTyping ? 'Escribiendo...' : 'En línea'}
                </span>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setIsMinimized(!isMinimized)}
                className="p-1.5 hover:bg-white/20 rounded-lg transition-colors"
              >
                {isMinimized ? (
                  <FiMaximize2 className="w-4 h-4" />
                ) : (
                  <FiMinimize2 className="w-4 h-4" />
                )}
              </button>
              <button
                onClick={toggleChat}
                className="p-1.5 hover:bg-white/20 rounded-lg transition-colors"
              >
                <FiX className="w-4 h-4" />
              </button>
            </div>
          </div>

          {!isMinimized && (
            <>
              {/* Messages */}
              <div className="flex-1 overflow-y-auto p-4 space-y-4 bg-gray-50">
                {isConnecting && (
                  <div className="text-center text-gray-500 py-4">
                    <div className="animate-spin w-6 h-6 border-2 border-blue-500 border-t-transparent rounded-full mx-auto mb-2" />
                    Conectando...
                  </div>
                )}

                {messages.map((message) => (
                  <MessageBubble key={message.id} message={message} primaryColor={primaryColor} />
                ))}

                {isTyping && (
                  <div className="flex items-center gap-2">
                    <div className="bg-white rounded-2xl px-4 py-3 shadow-sm">
                      <div className="flex gap-1">
                        <span
                          className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                          style={{ animationDelay: '0ms' }}
                        />
                        <span
                          className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                          style={{ animationDelay: '150ms' }}
                        />
                        <span
                          className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                          style={{ animationDelay: '300ms' }}
                        />
                      </div>
                    </div>
                  </div>
                )}

                {error && (
                  <div className="text-center text-red-500 text-sm py-2">
                    {error}
                    <button
                      onClick={() => setError(null)}
                      className="ml-2 underline hover:no-underline"
                    >
                      Reintentar
                    </button>
                  </div>
                )}

                <div ref={messagesEndRef} />
              </div>

              {/* Quick Replies */}
              {quickReplies.length > 0 && (
                <div className="px-4 py-2 border-t border-gray-100 flex flex-wrap gap-2">
                  {quickReplies.map((reply) => (
                    <button
                      key={reply.id}
                      onClick={() => handleQuickReply(reply)}
                      className="px-3 py-1.5 text-sm border border-gray-300 rounded-full hover:bg-gray-100 transition-colors"
                    >
                      {reply.label}
                    </button>
                  ))}
                </div>
              )}

              {/* Input */}
              <div className="p-4 border-t border-gray-200 bg-white">
                <div className="flex items-center gap-2">
                  <input
                    ref={inputRef}
                    type="text"
                    value={inputValue}
                    onChange={(e) => setInputValue(e.target.value)}
                    onKeyPress={handleKeyPress}
                    placeholder="Escribe tu mensaje..."
                    className="flex-1 px-4 py-2.5 bg-gray-100 rounded-full focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                    disabled={isTyping || !conversation}
                  />
                  <button
                    onClick={handleSend}
                    disabled={!inputValue.trim() || isTyping || !conversation}
                    className="w-10 h-10 rounded-full flex items-center justify-center text-white transition-colors disabled:opacity-50"
                    style={{ backgroundColor: primaryColor }}
                  >
                    <FiSend className="w-4 h-4" />
                  </button>
                </div>
                <p className="text-xs text-gray-400 mt-2 text-center">Powered by OKLA AI 🤖</p>
              </div>
            </>
          )}
        </div>
      )}
    </>
  );
}

// Message Bubble Component
interface MessageBubbleProps {
  message: MessageDto;
  primaryColor: string;
}

function MessageBubble({ message, primaryColor }: MessageBubbleProps) {
  const isUser = message.role === MessageRole.User;
  const isSystem = message.role === MessageRole.System;

  if (isSystem) {
    return (
      <div className="text-center">
        <span className="inline-block px-4 py-2 bg-yellow-100 text-yellow-800 text-sm rounded-lg">
          {message.content}
        </span>
      </div>
    );
  }

  return (
    <div className={`flex ${isUser ? 'justify-end' : 'justify-start'}`}>
      <div
        className={`max-w-[80%] rounded-2xl px-4 py-3 shadow-sm ${
          isUser ? 'text-white' : 'bg-white text-gray-800'
        }`}
        style={isUser ? { backgroundColor: primaryColor } : {}}
      >
        <p className="text-sm whitespace-pre-wrap">{message.content}</p>
        <span className={`text-xs mt-1 block ${isUser ? 'text-white/70' : 'text-gray-400'}`}>
          {formatTime(message.sentAt)}
        </span>
      </div>
    </div>
  );
}

// Helper functions
function getOrCreateSessionId(): string {
  let sessionId = sessionStorage.getItem('okla_chat_session');
  if (!sessionId) {
    sessionId = `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    sessionStorage.setItem('okla_chat_session', sessionId);
  }
  return sessionId;
}

function formatTime(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleTimeString('es-DO', { hour: '2-digit', minute: '2-digit' });
}

function getInitialQuickReplies(vehicleContext?: VehicleContext): QuickReply[] {
  if (vehicleContext) {
    return [
      {
        id: '1',
        label: '📋 Ver más detalles',
        value: '¿Puedes darme más detalles sobre este vehículo?',
      },
      { id: '2', label: '💰 Precio negociable?', value: '¿El precio es negociable?' },
      { id: '3', label: '🚗 Test drive', value: '¿Puedo agendar un test drive?' },
      { id: '4', label: '📞 Contactar vendedor', value: 'Quiero hablar con el vendedor' },
    ];
  }
  return [
    { id: '1', label: '🔍 Buscar vehículo', value: 'Ayúdame a encontrar un vehículo' },
    { id: '2', label: '💵 Vender mi carro', value: 'Quiero vender mi vehículo' },
    { id: '3', label: '❓ Cómo funciona', value: '¿Cómo funciona OKLA?' },
  ];
}
