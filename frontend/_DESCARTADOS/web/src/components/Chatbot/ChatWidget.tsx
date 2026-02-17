import React, { useState } from 'react';
import { FiMessageCircle, FiX } from 'react-icons/fi';
import ChatWindow from './ChatWindow';

interface ChatWidgetProps {
  vehicleId?: string;
  vehicleTitle?: string;
  vehiclePrice?: number;
  dealerId?: string;
  dealerName?: string;
  dealerWhatsApp?: string;
}

/**
 * Floating chat widget button que abre ChatWindow
 * Se muestra en páginas de vehículos para iniciar conversación
 */
export const ChatWidget: React.FC<ChatWidgetProps> = ({
  vehicleId,
  vehicleTitle,
  vehiclePrice,
  dealerId,
  dealerName,
  dealerWhatsApp,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);

  const toggleChat = () => {
    setIsOpen(!isOpen);
    if (!isOpen) {
      setUnreadCount(0); // Reset unread cuando se abre
    }
  };

  return (
    <>
      {/* Floating Button */}
      {!isOpen && (
        <button
          onClick={toggleChat}
          className="fixed bottom-6 right-6 z-50 bg-blue-600 hover:bg-blue-700 text-white rounded-full p-4 shadow-lg transition-all duration-300 hover:scale-110 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          aria-label="Abrir chat"
        >
          <FiMessageCircle className="w-6 h-6" />
          {unreadCount > 0 && (
            <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs font-bold rounded-full w-5 h-5 flex items-center justify-center">
              {unreadCount > 9 ? '9+' : unreadCount}
            </span>
          )}
        </button>
      )}

      {/* Chat Window */}
      {isOpen && (
        <div className="fixed bottom-6 right-6 z-50 w-96 h-[600px] shadow-2xl rounded-lg overflow-hidden animate-slide-up">
          <ChatWindow
            vehicleId={vehicleId}
            vehicleTitle={vehicleTitle}
            vehiclePrice={vehiclePrice}
            dealerId={dealerId}
            dealerName={dealerName}
            dealerWhatsApp={dealerWhatsApp}
            onClose={toggleChat}
            onNewMessage={() => !isOpen && setUnreadCount((prev) => prev + 1)}
          />
        </div>
      )}

      {/* Animation Styles */}
      <style>{`
        @keyframes slide-up {
          from {
            transform: translateY(100%);
            opacity: 0;
          }
          to {
            transform: translateY(0);
            opacity: 1;
          }
        }
        .animate-slide-up {
          animation: slide-up 0.3s ease-out;
        }
      `}</style>
    </>
  );
};

export default ChatWidget;
