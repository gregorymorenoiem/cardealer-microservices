/**
 * AZUL Payment Approved Page
 *
 * Displayed when AZUL payment is successful
 */

import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { CheckCircle, Home, FileText, Loader2 } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';
import { azulService } from '@/services/azulService';

export const AzulApprovedPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [transaction, setTransaction] = useState<any>(null);

  // AZUL callback parameters
  const orderNumber = searchParams.get('OrderNumber');
  const authCode = searchParams.get('AuthorizationCode');
  const amount = searchParams.get('Amount');

  useEffect(() => {
    const fetchTransaction = async () => {
      if (!orderNumber) {
        setLoading(false);
        return;
      }

      try {
        const tx = await azulService.getTransaction(orderNumber);
        setTransaction(tx);
      } catch (error) {
        console.error('Error fetching transaction:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchTransaction();

    // Clear session storage
    sessionStorage.removeItem('azul_order_number');
    const context = sessionStorage.getItem('azul_payment_context');
    if (context) {
      sessionStorage.removeItem('azul_payment_context');
    }
  }, [orderNumber]);

  if (loading) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <Loader2 className="w-12 h-12 animate-spin text-blue-600 mx-auto mb-4" />
            <p className="text-gray-600">Verificando transacción...</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Success Card */}
          <div className="bg-white shadow-lg rounded-lg overflow-hidden">
            {/* Success Header */}
            <div className="bg-gradient-to-r from-green-500 to-green-600 px-6 py-12 text-center">
              <div className="inline-flex items-center justify-center w-20 h-20 bg-white rounded-full mb-4">
                <CheckCircle className="w-12 h-12 text-green-500" />
              </div>
              <h1 className="text-3xl font-bold text-white mb-2">¡Pago Aprobado!</h1>
              <p className="text-green-100 text-lg">
                Tu transacción ha sido procesada exitosamente
              </p>
            </div>

            {/* Transaction Details */}
            <div className="px-6 py-8 space-y-6">
              {/* Details Box */}
              <div className="bg-gray-50 rounded-lg p-6 space-y-4">
                <h2 className="text-lg font-semibold text-gray-900">Detalles de la transacción</h2>

                {transaction ? (
                  <>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-gray-600">Orden #</p>
                        <p className="font-mono font-medium text-gray-900">
                          {transaction.orderNumber}
                        </p>
                      </div>
                      <div>
                        <p className="text-gray-600">Código de Autorización</p>
                        <p className="font-mono font-medium text-gray-900">
                          {transaction.authorizationCode || 'N/A'}
                        </p>
                      </div>
                      <div>
                        <p className="text-gray-600">Monto</p>
                        <p className="font-semibold text-gray-900">
                          {azulService.formatAmount(transaction.amount)}
                        </p>
                      </div>
                      <div>
                        <p className="text-gray-600">ITBIS</p>
                        <p className="font-semibold text-gray-900">
                          {azulService.formatAmount(transaction.itbis)}
                        </p>
                      </div>
                      <div className="col-span-2">
                        <p className="text-gray-600">Fecha</p>
                        <p className="font-medium text-gray-900">
                          {new Date(transaction.transactionDateTime).toLocaleString('es-DO')}
                        </p>
                      </div>
                    </div>

                    <div className="border-t border-gray-200 pt-4">
                      <div className="flex justify-between items-center">
                        <span className="text-lg font-semibold text-gray-900">Total Pagado:</span>
                        <span className="text-2xl font-bold text-green-600">
                          {azulService.formatAmount(transaction.amount + transaction.itbis)}
                        </span>
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="space-y-2">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Orden #:</span>
                      <span className="font-mono font-medium">{orderNumber || 'N/A'}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Código de Autorización:</span>
                      <span className="font-mono font-medium">{authCode || 'N/A'}</span>
                    </div>
                    {amount && (
                      <div className="flex justify-between">
                        <span className="text-gray-600">Monto:</span>
                        <span className="font-semibold">
                          {azulService.formatAmount(parseFloat(amount) / 100)}
                        </span>
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Next Steps */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="text-sm font-semibold text-blue-900 mb-2">📧 Próximos pasos</h3>
                <ul className="text-sm text-blue-800 space-y-1">
                  <li>• Recibirás un email de confirmación con los detalles</li>
                  <li>• Tu publicación estará activa en los próximos minutos</li>
                  <li>• Puedes ver el estado en tu dashboard</li>
                </ul>
              </div>

              {/* Action Buttons */}
              <div className="flex flex-col sm:flex-row gap-3">
                <button
                  onClick={() => navigate('/dashboard')}
                  className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-3 px-6 rounded-lg font-semibold transition-colors flex items-center justify-center gap-2"
                >
                  <Home className="w-5 h-5" />
                  <span>Ir al Dashboard</span>
                </button>
                <button
                  onClick={() => navigate('/vehicles')}
                  className="flex-1 border-2 border-gray-300 hover:border-gray-400 text-gray-700 py-3 px-6 rounded-lg font-semibold transition-colors flex items-center justify-center gap-2"
                >
                  <FileText className="w-5 h-5" />
                  <span>Ver Publicaciones</span>
                </button>
              </div>

              {/* Receipt Link */}
              <div className="text-center text-sm text-gray-600">
                <p>
                  ¿Necesitas un recibo?{' '}
                  <button
                    onClick={() => window.print()}
                    className="text-blue-600 hover:underline font-medium"
                  >
                    Imprimir recibo
                  </button>
                </p>
              </div>
            </div>
          </div>

          {/* Support */}
          <div className="mt-6 text-center text-sm text-gray-600">
            <p>
              ¿Problemas con tu pago?{' '}
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
