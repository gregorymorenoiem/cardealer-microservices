/**
 * AZUL Payment Page
 *
 * Handles the AZUL payment flow:
 * 1. Shows payment summary
 * 2. Calls backend to initiate payment
 * 3. Redirects to AZUL Payment Page
 */

import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Loader2, AlertCircle, CreditCard, ArrowLeft } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';
import { azulService } from '@/services/azulService';

export const AzulPaymentPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Payment details from URL params
  const amount = parseFloat(searchParams.get('amount') || '0');
  const listingId = searchParams.get('listingId');
  const planType = searchParams.get('planType'); // 'individual' | 'dealer-basic' | 'dealer-premium'

  const itbis = azulService.calculateITBIS(amount);
  const total = amount + itbis;

  useEffect(() => {
    // Validate required params
    if (!amount || amount <= 0) {
      setError('Monto de pago inválido');
    }

    if (!listingId && !planType) {
      setError('Información de pago incompleta');
    }
  }, [amount, listingId, planType]);

  const handlePayment = async () => {
    setLoading(true);
    setError(null);

    try {
      // Generate unique order number
      const orderNumber = `OKLA-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

      // Call backend to initiate payment
      const response = await azulService.initiatePayment({
        amount,
        itbis,
        orderNumber,
        description: listingId ? `Publicación de vehículo ${listingId}` : `Plan ${planType}`,
      });

      // Store order number in sessionStorage for callback verification
      sessionStorage.setItem('azul_order_number', orderNumber);
      sessionStorage.setItem(
        'azul_payment_context',
        JSON.stringify({
          listingId,
          planType,
          amount,
          itbis,
          total,
        })
      );

      // Submit form to AZUL Payment Page
      azulService.submitAzulForm(response);
    } catch (err) {
      console.error('Payment initiation error:', err);
      setError(
        err instanceof Error
          ? err.message
          : 'Error al procesar el pago. Por favor intenta nuevamente.'
      );
      setLoading(false);
    }
  };

  const getPlanName = () => {
    switch (planType) {
      case 'individual':
        return 'Publicación Individual';
      case 'dealer-basic':
        return 'Plan Dealer Básico';
      case 'dealer-premium':
        return 'Plan Dealer Premium';
      default:
        return 'Servicio OKLA';
    }
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Back Button */}
          <button
            onClick={() => navigate(-1)}
            className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
            disabled={loading}
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Volver</span>
          </button>

          {/* Payment Card */}
          <div className="bg-white shadow-lg rounded-lg overflow-hidden">
            {/* Header */}
            <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-8 text-white">
              <div className="flex items-center gap-3 mb-4">
                <CreditCard className="w-8 h-8" />
                <h1 className="text-2xl font-bold">Pago con AZUL</h1>
              </div>
              <p className="text-blue-100">
                Serás redirigido a la plataforma segura de AZUL (Banco Popular)
              </p>
            </div>

            {/* Payment Details */}
            <div className="px-6 py-6 space-y-6">
              {/* Error Alert */}
              {error && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                  <div className="flex gap-3">
                    <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0" />
                    <div>
                      <h3 className="text-sm font-medium text-red-800">Error</h3>
                      <p className="text-sm text-red-700 mt-1">{error}</p>
                    </div>
                  </div>
                </div>
              )}

              {/* Summary */}
              <div>
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Resumen del pago</h2>

                <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600">Concepto:</span>
                    <span className="font-medium text-gray-900">{getPlanName()}</span>
                  </div>

                  {listingId && (
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Vehículo ID:</span>
                      <span className="font-mono text-gray-900">{listingId}</span>
                    </div>
                  )}

                  <div className="border-t border-gray-200 pt-3">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Subtotal:</span>
                      <span className="text-gray-900">{azulService.formatAmount(amount)}</span>
                    </div>
                    <div className="flex justify-between text-sm mt-2">
                      <span className="text-gray-600">ITBIS (18%):</span>
                      <span className="text-gray-900">{azulService.formatAmount(itbis)}</span>
                    </div>
                  </div>

                  <div className="border-t border-gray-200 pt-3">
                    <div className="flex justify-between">
                      <span className="text-lg font-semibold text-gray-900">Total:</span>
                      <span className="text-2xl font-bold text-blue-600">
                        {azulService.formatAmount(total)}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Payment Info */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="text-sm font-semibold text-blue-900 mb-2">
                  ℹ️ Información del proceso
                </h3>
                <ul className="text-sm text-blue-800 space-y-1">
                  <li>• Serás redirigido a AZUL Payment Page</li>
                  <li>• Ingresa los datos de tu tarjeta de forma segura</li>
                  <li>• La transacción es procesada por Banco Popular</li>
                  <li>• Recibirás confirmación inmediata del resultado</li>
                </ul>
              </div>

              {/* Payment Button */}
              <button
                onClick={handlePayment}
                disabled={loading || !!error}
                className={`
                  w-full py-4 px-6 rounded-lg font-semibold text-white
                  transition-all flex items-center justify-center gap-2
                  ${
                    loading || error
                      ? 'bg-gray-400 cursor-not-allowed'
                      : 'bg-blue-600 hover:bg-blue-700 active:scale-95'
                  }
                `}
              >
                {loading ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin" />
                    <span>Procesando...</span>
                  </>
                ) : (
                  <>
                    <CreditCard className="w-5 h-5" />
                    <span>Continuar con AZUL</span>
                  </>
                )}
              </button>

              {/* Security Info */}
              <div className="text-center text-xs text-gray-500">
                <p>🔒 Pago 100% seguro procesado por AZUL (Banco Popular)</p>
                <p className="mt-1">No almacenamos información de tarjetas de crédito</p>
              </div>
            </div>
          </div>

          {/* Support */}
          <div className="mt-6 text-center text-sm text-gray-600">
            <p>
              ¿Problemas con el pago?{' '}
              <a href="/support" className="text-blue-600 hover:underline">
                Contacta soporte
              </a>
            </p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};
