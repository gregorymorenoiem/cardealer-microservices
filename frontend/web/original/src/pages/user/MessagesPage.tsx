import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FiSearch, FiSend } from 'react-icons/fi';
import { useMessages } from '@/hooks/useMessages';

const MessagesPage = () => {
  const { t, i18n } = useTranslation('user');
  const { conversations, selectedConversation, selectConversation, sendMessage } = useMessages();
  const [searchQuery, setSearchQuery] = useState('');
  const [messageInput, setMessageInput] = useState('');

  // Format relative time
  const formatTimeAgo = (timestamp: string) => {
    const now = new Date();
    const date = new Date(timestamp);
    const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (seconds < 60) return t('messages.time.justNow');
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return t('messages.time.minutesAgo', { count: minutes });
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return t('messages.time.hoursAgo', { count: hours });
    const days = Math.floor(hours / 24);
    if (days === 1) return t('messages.time.yesterday');
    if (days < 7) return t('messages.time.daysAgo', { count: days });
    return date.toLocaleDateString(i18n.language === 'es' ? 'es-MX' : 'en-US', { month: 'short', day: 'numeric' });
  };

  const filteredConversations = searchQuery
    ? conversations.filter(
        (c) =>
          c.vehicleTitle.toLowerCase().includes(searchQuery.toLowerCase()) ||
          c.sellerName.toLowerCase().includes(searchQuery.toLowerCase())
      )
    : conversations;

  const handleSendMessage = (e: React.FormEvent) => {
    e.preventDefault();
    if (!messageInput.trim() || !selectedConversation) return;

    sendMessage(selectedConversation.id, messageInput);
    setMessageInput('');
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat(i18n.language === 'es' ? 'es-MX' : 'en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
    }).format(price);
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="container mx-auto px-4">
        <h1 className="text-3xl font-bold mb-6">{t('messages.title')}</h1>

        <div className="bg-white rounded-xl shadow-lg overflow-hidden">
          <div className="grid lg:grid-cols-3 h-[calc(100vh-200px)]">
            {/* Inbox List */}
            <div className="lg:col-span-1 border-r border-gray-200 overflow-y-auto">
              {/* Search */}
              <div className="p-4 border-b border-gray-200 sticky top-0 bg-white z-10">
                <div className="relative">
                  <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    type="text"
                    placeholder={t('messages.searchPlaceholder')}
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                  />
                </div>
              </div>

              {/* Conversations List */}
              <div>
                {filteredConversations.length === 0 ? (
                  <div className="p-8 text-center text-gray-500">
                    <p>{t('messages.noConversations')}</p>
                  </div>
                ) : (
                  filteredConversations.map((conversation) => (
                    <button
                      key={conversation.id}
                      onClick={() => selectConversation(conversation.id)}
                      className={`w-full p-4 border-b border-gray-200 hover:bg-gray-50 transition-colors text-left ${
                        selectedConversation?.id === conversation.id ? 'bg-blue-50' : ''
                      }`}
                    >
                      <div className="flex gap-3">
                        <img
                          src={conversation.vehicleImage}
                          alt={conversation.vehicleTitle}
                          className="w-16 h-16 rounded-lg object-cover flex-shrink-0"
                        />
                        <div className="flex-1 min-w-0">
                          <div className="flex items-start justify-between gap-2">
                            <h3 className="font-semibold text-sm truncate">
                              {conversation.vehicleTitle}
                            </h3>
                            {conversation.unreadCount > 0 && (
                              <span className="bg-primary text-white text-xs rounded-full w-5 h-5 flex items-center justify-center flex-shrink-0">
                                {conversation.unreadCount}
                              </span>
                            )}
                          </div>
                          <p className="text-sm text-gray-600 mt-0.5">{conversation.sellerName}</p>
                          <p className="text-sm text-gray-500 truncate mt-1">
                            {conversation.lastMessage}
                          </p>
                          <p className="text-xs text-gray-400 mt-1">
                            {formatTimeAgo(conversation.lastMessageTime)}
                          </p>
                        </div>
                      </div>
                    </button>
                  ))
                )}
              </div>
            </div>

            {/* Conversation View */}
            <div className="lg:col-span-2 flex flex-col">
              {selectedConversation ? (
                <>
                  {/* Conversation Header */}
                  <div className="p-4 border-b border-gray-200 bg-white">
                    <div className="flex items-center gap-3">
                      <img
                        src={selectedConversation.vehicleImage}
                        alt={selectedConversation.vehicleTitle}
                        className="w-12 h-12 rounded-lg object-cover"
                      />
                      <div className="flex-1">
                        <h2 className="font-semibold">{selectedConversation.vehicleTitle}</h2>
                        <p className="text-sm text-gray-600">
                          {formatPrice(selectedConversation.vehiclePrice)} •{' '}
                          {selectedConversation.sellerName}
                        </p>
                      </div>
                      <a
                        href={`/vehicles/${selectedConversation.vehicleId}`}
                        className="text-sm text-primary hover:underline"
                      >
                        {t('messages.viewListing')}
                      </a>
                    </div>
                  </div>

                  {/* Messages */}
                  <div className="flex-1 overflow-y-auto p-4 space-y-4">
                    {selectedConversation.messages.map((message) => {
                      const isCurrentUser = message.senderId === 'buyer-1';
                      return (
                        <div
                          key={message.id}
                          className={`flex ${isCurrentUser ? 'justify-end' : 'justify-start'}`}
                        >
                          <div
                            className={`max-w-[70%] ${
                              isCurrentUser ? 'order-2' : 'order-1'
                            }`}
                          >
                            <div
                              className={`rounded-lg px-4 py-2 ${
                                isCurrentUser
                                  ? 'bg-primary text-white'
                                  : 'bg-gray-100 text-gray-900'
                              }`}
                            >
                              <p className="text-sm">{message.content}</p>
                            </div>
                            <p className="text-xs text-gray-400 mt-1 px-2">
                              {formatTimeAgo(message.timestamp)}
                            </p>
                          </div>
                        </div>
                      );
                    })}
                  </div>

                  {/* Message Input */}
                  <form onSubmit={handleSendMessage} className="p-4 border-t border-gray-200 bg-white">
                    <div className="flex gap-2">
                      <input
                        type="text"
                        placeholder={t('messages.typePlaceholder')}
                        value={messageInput}
                        onChange={(e) => setMessageInput(e.target.value)}
                        className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                      />
                      <button
                        type="submit"
                        disabled={!messageInput.trim()}
                        className="bg-primary text-white px-6 py-2 rounded-lg hover:bg-primary-dark transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                      >
                        <FiSend className="w-5 h-5" />
                        {t('messages.send')}
                      </button>
                    </div>
                  </form>
                </>
              ) : (
                <div className="flex-1 flex items-center justify-center text-gray-500">
                  <div className="text-center">
                    <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                      <FiSearch className="w-8 h-8 text-gray-400" />
                    </div>
                    <h3 className="text-lg font-semibold mb-2">{t('messages.selectConversation')}</h3>
                    <p className="text-sm">{t('messages.selectConversationHint')}</p>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MessagesPage;

