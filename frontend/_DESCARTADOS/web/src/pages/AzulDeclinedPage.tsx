/**
 * AZUL Payment Declined Page
 *
 * Displayed when AZUL payment is declined
 */

import React, { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { XCircle, ArrowLeft, HelpCircle, RefreshCw } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';

export const AzulDeclinedPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const orderNumber = searchParams.get('OrderNumber');
  const responseMessage = searchParams.get('ResponseMessage');
  const responseCode = searchParams.get('ResponseCode');

  useEffect(() => {
    // Clear session storage
    sessionStorage.removeItem('azul_order_number');
    sessionStorage.removeItem('azul_payment_context');
  }, []);

  const handleRetry = () => {
    // Go back to payment selection
    navigate(-2); // Go back 2 steps (payment page → azul page)
  };

  const getDeclineReason = () => {
    // Map common decline codes to user-friendly messages
    const code = responseCode?.toLowerCase();

    if (code?.includes('insufficient') || code?.includes('funds')) {
      return 'Fondos insuficientes en la tarjeta';
    }
    if (code?.includes('expired')) {
      return 'Tarjeta expirada';
    }
    if (code?.includes('declined') || code?.includes('denied')) {
      return 'Transacción declinada por el banco emisor';
    }
    if (code?.includes('invalid')) {
      return 'Datos de tarjeta inválidos';
    }

    return responseMessage || 'Transacción declinada';
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Declined Card */}
          <div className="bg-white shadow-lg rounded-lg overflow-hidden">
            {/* Error Header */}
            <div className="bg-gradient-to-r from-red-500 to-red-600 px-6 py-12 text-center">
              <div className="inline-flex items-center justify-center w-20 h-20 bg-white rounded-full mb-4">
                <XCircle className="w-12 h-12 text-red-500" />
              </div>
              <h1 className="text-3xl font-bold text-white mb-2">Pago Declinado</h1>
              <p className="text-red-100 text-lg">No se pudo procesar tu pago</p>
            </div>

            {/* Details */}
            <div className="px-6 py-8 space-y-6">
              {/* Error Details */}
              <div className="bg-red-50 border border-red-200 rounded-lg p-6">
                <h2 className="text-lg font-semibold text-red-900 mb-3">Motivo del rechazo</h2>
                <p className="text-red-800 font-medium mb-4">{getDeclineReason()}</p>

                {orderNumber && (
                  <div className="text-sm text-red-700">
                    <p>
                      Orden #: <span className="font-mono">{orderNumber}</span>
                    </p>
                  </div>
                )}

                {responseCode && (
                  <div className="text-sm text-red-700 mt-1">
                    <p>
                      Código: <span className="font-mono">{responseCode}</span>
                    </p>
                  </div>
                )}
              </div>

              {/* Common Reasons */}
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <h3 className="text-sm font-semibold text-yellow-900 mb-3 flex items-center gap-2">
                  <HelpCircle className="w-4 h-4" />
                  Razones comunes de rechazo
                </h3>
                <ul className="text-sm text-yellow-800 space-y-2">
                  <li>
                    • <strong>Fondos insuficientes:</strong> Verifica el saldo disponible
                  </li>
                  <li>
                    • <strong>Límite de compra excedido:</strong> Contacta a tu banco
                  </li>
                  <li>
                    • <strong>Tarjeta bloqueada:</strong> Tu banco puede haber bloqueado la
                    transacción por seguridad
                  </li>
                  <li>
                    • <strong>Datos incorrectos:</strong> Verifica número, CVV y fecha de expiración
                  </li>
                  <li>
                    • <strong>Restricciones internacionales:</strong> Algunas tarjetas no permiten
                    compras en línea
                  </li>
                </ul>
              </div>

              {/* Solutions */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="text-sm font-semibold text-blue-900 mb-2">💡 ¿Qué puedes hacer?</h3>
                <ul className="text-sm text-blue-800 space-y-1">
                  <li>1. Verifica los datos de tu tarjeta</li>
                  <li>2. Contacta a tu banco para autorizar la compra</li>
                  <li>3. Intenta con otra tarjeta</li>
                  <li>4. Usa otro método de pago (Stripe)</li>
                </ul>
              </div>

              {/* Action Buttons */}
              <div className="space-y-3">
                <button
                  onClick={handleRetry}
                  className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 px-6 rounded-lg font-semibold transition-colors flex items-center justify-center gap-2"
                >
                  <RefreshCw className="w-5 h-5" />
                  <span>Intentar Nuevamente</span>
                </button>

                <button
                  onClick={() => navigate('/')}
                  className="w-full border-2 border-gray-300 hover:border-gray-400 text-gray-700 py-3 px-6 rounded-lg font-semibold transition-colors flex items-center justify-center gap-2"
                >
                  <ArrowLeft className="w-5 h-5" />
                  <span>Volver al Inicio</span>
                </button>
              </div>

              {/* Alternative Payment */}
              <div className="text-center">
                <p className="text-sm text-gray-600 mb-2">¿Prefieres usar otro método de pago?</p>
                <button
                  onClick={() => navigate('/payment?method=stripe')}
                  className="text-purple-600 hover:underline font-medium text-sm"
                >
                  Pagar con Stripe (Tarjetas internacionales)
                </button>
              </div>
            </div>
          </div>

          {/* Support */}
          <div className="mt-6 text-center text-sm text-gray-600">
            <p>
              ¿Necesitas ayuda?{' '}
              <a href="/support" className="text-blue-600 hover:underline">
                Contacta soporte
              </a>{' '}
              o llama al{' '}
              <a href="tel:8095442985" className="text-blue-600 hover:underline">
                809-544-2985
              </a>
            </p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};
