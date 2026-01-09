import React, { useState } from 'react';
import { FaWhatsapp } from 'react-icons/fa';
import { FiZap } from 'react-icons/fi';

interface WhatsAppHandoffButtonProps {
  onHandoff: () => void;
  dealerName: string;
  leadScore: number;
  disabled?: boolean;
}

/**
 * Botón para iniciar handoff a WhatsApp
 * Se muestra cuando el lead es HOT (score >= 85)
 */
export const WhatsAppHandoffButton: React.FC<WhatsAppHandoffButtonProps> = ({
  onHandoff,
  dealerName,
  leadScore,
  disabled,
}) => {
  const [isLoading, setIsLoading] = useState(false);

  const handleClick = async () => {
    setIsLoading(true);
    try {
      await onHandoff();
    } catch (error) {
      console.error('Handoff error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-2">
      {/* Alert Message */}
      <div className="flex items-start space-x-2 bg-yellow-50 border border-yellow-200 rounded-lg p-3">
        <FiZap className="text-yellow-600 w-5 h-5 mt-0.5 flex-shrink-0" />
        <div className="flex-1">
          <p className="text-sm font-semibold text-yellow-800">
            ¡Eres un lead HOT! (Score: {leadScore})
          </p>
          <p className="text-xs text-yellow-700 mt-1">
            {dealerName} puede atenderte inmediatamente por WhatsApp para cerrar la compra.
          </p>
        </div>
      </div>

      {/* WhatsApp Button */}
      <button
        onClick={handleClick}
        disabled={disabled || isLoading}
        className="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-3 px-4 rounded-lg flex items-center justify-center space-x-2 transition-colors focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 disabled:bg-gray-400 disabled:cursor-not-allowed"
      >
        {isLoading ? (
          <>
            <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
            <span>Conectando...</span>
          </>
        ) : (
          <>
            <FaWhatsapp className="w-5 h-5" />
            <span>Contactar por WhatsApp Ahora</span>
          </>
        )}
      </button>

      {/* Info Text */}
      <p className="text-xs text-center text-gray-600">
        Tu información será enviada a {dealerName} para atenderte personalmente
      </p>
    </div>
  );
};

export default WhatsAppHandoffButton;
