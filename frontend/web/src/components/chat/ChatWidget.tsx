import { useState, useEffect, useCallback, useRef } from 'react';
import { FiMessageCircle, FiX, FiSend, FiMinimize2, FiMaximize2 } from 'react-icons/fi';
import { chatbotService } from '@/services/chatbotService';
import type {
  VehicleContext,
  ChatMessage,
  QuickReply,
  Conversation,
} from '@/services/chatbotService';

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
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [conversation, setConversation] = useState<Conversation | null>(null);
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
      // Try SignalR first, fallback to REST
      const token = localStorage.getItem('token');

      try {
        await chatbotService.connectSignalR(token || undefined);

        // Setup listeners
        chatbotService.onTypingIndicator((indicator) => {
          setIsTyping(indicator.isTyping);
        });

        const newConversation = await chatbotService.startConversationSignalR({
          sessionId: getOrCreateSessionId(),
          vehicleContext,
        });

        setConversation(newConversation);
        if (newConversation.messages.length > 0) {
          setMessages(newConversation.messages);
        }
      } catch {
        // Fallback to REST API
        console.log('SignalR failed, using REST API');
        const newConversation = await chatbotService.createConversation({
          sessionId: getOrCreateSessionId(),
          vehicleContext,
        });

        setConversation(newConversation);
        if (newConversation.messages.length > 0) {
          setMessages(newConversation.messages);
        }
      }

      // Add welcome message if no messages
      if (messages.length === 0) {
        const welcomeMessage: ChatMessage = {
          id: 'welcome',
          role: 'Assistant',
          content: vehicleContext
            ? `¡Hola! 👋 Soy el asistente de OKLA. Veo que estás viendo el **${vehicleContext.year} ${vehicleContext.make} ${vehicleContext.model}** por RD$${vehicleContext.price.toLocaleString()}. ¿Tienes alguna pregunta sobre este vehículo?`
            : '¡Hola! 👋 Soy el asistente de OKLA, el marketplace #1 de vehículos en República Dominicana. ¿En qué puedo ayudarte hoy?',
          type: 'Text',
          createdAt: new Date().toISOString(),
          suggestedReplies: getInitialQuickReplies(vehicleContext),
        };
        setMessages([welcomeMessage]);
        setQuickReplies(welcomeMessage.suggestedReplies || []);
      }
    } catch (err) {
      console.error('Failed to initialize chat:', err);
      setError('No se pudo conectar. Por favor, intenta de nuevo.');
    } finally {
      setIsConnecting(false);
    }
  }, [conversation, vehicleContext, messages.length]);

  useEffect(() => {
    if (isOpen && !conversation) {
      initializeChat();
    }
  }, [isOpen, conversation, initializeChat]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      chatbotService.removeAllListeners();
    };
  }, []);

  const handleSend = async () => {
    if (!inputValue.trim() || isTyping || !conversation) return;

    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      role: 'User',
      content: inputValue.trim(),
      type: 'Text',
      createdAt: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setInputValue('');
    setIsTyping(true);
    setQuickReplies([]);
    setError(null);

    try {
      // Try SignalR, fallback to REST
      let response;
      try {
        response = await chatbotService.sendMessageSignalR(
          conversation.id,
          userMessage.content,
          vehicleContext
        );
      } catch {
        response = await chatbotService.sendMessage(
          conversation.id,
          userMessage.content,
          vehicleContext
        );
      }

      setMessages((prev) => [...prev, response.assistantMessage]);
      setQuickReplies(response.suggestedReplies || []);

      if (response.shouldTransferToAgent) {
        // Show transfer notification
        const transferMessage: ChatMessage = {
          id: 'transfer-' + Date.now(),
          role: 'System',
          content: '📞 Tu consulta será transferida a un agente humano. Pronto te contactaremos.',
          type: 'System',
          createdAt: new Date().toISOString(),
        };
        setMessages((prev) => [...prev, transferMessage]);
      }
    } catch (err) {
      console.error('Failed to send message:', err);
      setError('No se pudo enviar el mensaje. Intenta de nuevo.');
    } finally {
      setIsTyping(false);
    }
  };

  const handleQuickReply = (reply: QuickReply) => {
    setInputValue(reply.value);
    setTimeout(() => handleSend(), 100);
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
  message: ChatMessage;
  primaryColor: string;
}

function MessageBubble({ message, primaryColor }: MessageBubbleProps) {
  const isUser = message.role === 'User';
  const isSystem = message.role === 'System';

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
          {formatTime(message.createdAt)}
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
