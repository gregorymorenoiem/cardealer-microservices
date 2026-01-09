import React, { useEffect, useRef } from 'react';
import { MessageDto, MessageRole, MessageType, chatbotService } from '@/services/chatbotService';
import { FiUser, FiCpu, FiAlertCircle, FiCheckCircle } from 'react-icons/fi';

interface MessageListProps {
  messages: MessageDto[];
}

export const MessageList: React.FC<MessageListProps> = ({ messages }) => {
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Auto-scroll to bottom cuando llegan nuevos mensajes
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const renderMessage = (message: MessageDto) => {
    const isUser = message.role === MessageRole.User;
    const isSystem = message.role === MessageRole.System;

    // System messages (buying signal detected, handoff, etc.)
    if (isSystem || message.type !== MessageType.Text) {
      return (
        <div key={message.id} className="flex justify-center my-3">
          <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-2 max-w-md text-center">
            <div className="flex items-center justify-center space-x-2">
              {message.type === MessageType.BuyingSignalDetected && (
                <>
                  <FiAlertCircle className="text-yellow-600 w-4 h-4" />
                  <span className="text-sm text-gray-700">{message.content}</span>
                </>
              )}
              {message.type === MessageType.HandoffInitiated && (
                <>
                  <FiCheckCircle className="text-green-600 w-4 h-4" />
                  <span className="text-sm text-gray-700">{message.content}</span>
                </>
              )}
              {message.type === MessageType.ConversationEnded && (
                <>
                  <FiCheckCircle className="text-gray-600 w-4 h-4" />
                  <span className="text-sm text-gray-700">{message.content}</span>
                </>
              )}
            </div>
            <p className="text-xs text-gray-500 mt-1">
              {chatbotService.formatRelativeTime(message.sentAt)}
            </p>
          </div>
        </div>
      );
    }

    // User or Assistant messages
    return (
      <div key={message.id} className={`flex mb-4 ${isUser ? 'justify-end' : 'justify-start'}`}>
        <div className={`flex max-w-[80%] ${isUser ? 'flex-row-reverse' : 'flex-row'}`}>
          {/* Avatar */}
          <div
            className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center ${
              isUser ? 'bg-blue-600 ml-3' : 'bg-gray-200 mr-3'
            }`}
          >
            {isUser ? (
              <FiUser className="w-4 h-4 text-white" />
            ) : (
              <FiCpu className="w-4 h-4 text-gray-700" />
            )}
          </div>

          {/* Message Bubble */}
          <div>
            <div
              className={`rounded-lg px-4 py-2 ${
                isUser
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-800 border border-gray-200'
              }`}
            >
              <p className="text-sm whitespace-pre-wrap">{message.content}</p>
            </div>

            {/* Timestamp */}
            <p className={`text-xs text-gray-500 mt-1 ${isUser ? 'text-right' : 'text-left'}`}>
              {chatbotService.formatRelativeTime(message.sentAt)}
            </p>

            {/* Metadata (buying signals detected) */}
            {message.metadata?.buyingSignals && message.metadata.buyingSignals.length > 0 && (
              <div className="mt-2 flex flex-wrap gap-1">
                {message.metadata.buyingSignals.map((signal: string, idx: number) => (
                  <span
                    key={idx}
                    className="inline-flex items-center text-xs bg-yellow-100 text-yellow-800 px-2 py-1 rounded-full"
                  >
                    {chatbotService.getBuyingSignalEmoji(signal)} {signal}
                  </span>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="flex-1 overflow-y-auto p-4 bg-gray-50">
      {messages.length === 0 ? (
        <div className="h-full flex items-center justify-center text-gray-500">
          <p className="text-center">
            Inicia la conversación preguntando algo sobre el vehículo...
          </p>
        </div>
      ) : (
        <>
          {messages.map(renderMessage)}
          <div ref={messagesEndRef} />
        </>
      )}
    </div>
  );
};

export default MessageList;
