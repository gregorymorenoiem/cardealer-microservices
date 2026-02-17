'use client';

import { useChatbot } from '@/hooks/useChatbot';
import { ChatBubble } from './ChatBubble';
import { ChatPanel } from './ChatPanel';

interface ChatWidgetProps {
  /** Optional dealer ID to scope the chat */
  dealerId?: string;
}

/**
 * ChatWidget — Floating AI chatbot widget for the OKLA platform.
 *
 * Renders a floating bubble button and expandable chat panel.
 * Uses Llama 3 8B fine-tuned model for vehicle sales assistance.
 *
 * Usage: Add <ChatWidget /> to the main layout.
 */
export function ChatWidget({ dealerId }: ChatWidgetProps) {
  const chat = useChatbot({
    dealerId,
    maxRetries: 2,
    onLeadGenerated: leadId => {
      // Could track with analytics
      console.log('[OKLA Chat] Lead generated:', leadId);
    },
    onTransfer: agentName => {
      console.log('[OKLA Chat] Transferred to:', agentName);
    },
    onLimitReached: () => {
      console.log('[OKLA Chat] Interaction limit reached');
    },
  });

  return (
    <>
      {/* Floating bubble button */}
      <ChatBubble isOpen={chat.isOpen} onClick={chat.toggle} />

      {/* Chat panel */}
      <ChatPanel
        messages={chat.messages}
        isOpen={chat.isOpen}
        isLoading={chat.isLoading}
        isConnected={chat.isConnected}
        isLimitReached={chat.isLimitReached}
        botName={chat.botName}
        botAvatarUrl={chat.botAvatarUrl}
        remainingInteractions={chat.remainingInteractions}
        error={chat.error}
        onSend={chat.sendMessage}
        onQuickReply={chat.selectQuickReply}
        onClose={chat.close}
        onMinimize={chat.close}
        onReset={chat.resetChat}
        onTransfer={() => chat.requestTransfer()}
        onClearError={chat.clearError}
      />
    </>
  );
}
