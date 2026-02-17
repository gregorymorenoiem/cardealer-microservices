/**
 * AZUL Payment Cancelled Page
 *
 * Displayed when user cancels AZUL payment
 */

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Ban, ArrowLeft, Home } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';

export const AzulCancelledPage: React.FC = () => {
  const navigate = useNavigate();

  useEffect(() => {
    // Clear session storage
    sessionStorage.removeItem('azul_order_number');
    sessionStorage.removeItem('azul_payment_context');
  }, []);

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Cancelled Card */}
          <div className="bg-white shadow-lg rounded-lg overflow-hidden">
            {/* Header */}
            <div className="bg-gradient-to-r from-gray-500 to-gray-600 px-6 py-12 text-center">
              <div className="inline-flex items-center justify-center w-20 h-20 bg-white rounded-full mb-4">
                <Ban className="w-12 h-12 text-gray-500" />
              </div>
              <h1 className="text-3xl font-bold text-white mb-2">Pago Cancelado</h1>
              <p className="text-gray-100 text-lg">Has cancelado el proceso de pago</p>
            </div>

            {/* Content */}
            <div className="px-6 py-8 space-y-6">
              {/* Info Box */}
              <div className="bg-gray-50 border border-gray-200 rounded-lg p-6 text-center">
                <p className="text-gray-700 mb-4">No se ha realizado ningún cargo a tu tarjeta.</p>
                <p className="text-gray-600 text-sm">
                  Si cancelaste por error, puedes volver a intentar el pago.
                </p>
              </div>

              {/* Common Reasons */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="text-sm font-semibold text-blue-900 mb-3">
                  ℹ️ Razones comunes de cancelación
                </h3>
                <ul className="text-sm text-blue-800 space-y-2">
                  <li>• Decidiste usar otro método de pago</li>
                  <li>• Necesitas verificar los detalles de la compra</li>
                  <li>• Problema técnico durante el proceso</li>
                  <li>• Cambio de opinión sobre la compra</li>
                </ul>
              </div>

              {/* Action Buttons */}
              <div className="space-y-3">
                <button
                  onClick={() => navigate(-2)}
                  className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 px-6 rounded-lg font-semibold transition-colors flex items-center justify-center gap-2"
                >
                  <ArrowLeft className="w-5 h-5" />
                  <span>Volver a Intentar</span>
                </button>

                <button
                  onClick={() => navigate('/')}
                  className="w-full border-2 border-gray-300 hover:border-gray-400 text-gray-700 py-3 px-6 rounded-lg font-semibold transition-colors flex items-center justify-center gap-2"
                >
                  <Home className="w-5 h-5" />
                  <span>Volver al Inicio</span>
                </button>
              </div>

              {/* Alternative Options */}
              <div className="border-t border-gray-200 pt-6 space-y-4">
                <h3 className="text-center font-semibold text-gray-900">Otras opciones de pago</h3>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <button
                    onClick={() => navigate('/payment?method=azul')}
                    className="border-2 border-blue-300 hover:border-blue-400 bg-blue-50 text-blue-700 py-3 px-4 rounded-lg font-medium transition-colors"
                  >
                    🏦 AZUL (Tarjetas RD)
                  </button>

                  <button
                    onClick={() => navigate('/payment?method=stripe')}
                    className="border-2 border-purple-300 hover:border-purple-400 bg-purple-50 text-purple-700 py-3 px-4 rounded-lg font-medium transition-colors"
                  >
                    💳 Stripe (Internacional)
                  </button>
                </div>
              </div>

              {/* Help Text */}
              <div className="text-center text-sm text-gray-600">
                <p>¿Necesitas ayuda para completar tu pago?</p>
                <a href="/support" className="text-blue-600 hover:underline font-medium">
                  Contacta nuestro equipo de soporte
                </a>
              </div>
            </div>
          </div>

          {/* Additional Info */}
          <div className="mt-6 bg-yellow-50 border border-yellow-200 rounded-lg p-4">
            <p className="text-sm text-yellow-800 text-center">
              💡 <strong>Consejo:</strong> Si tienes problemas con AZUL, prueba con Stripe. Acepta
              tarjetas internacionales, Apple Pay y Google Pay.
            </p>
          </div>

          {/* Support */}
          <div className="mt-6 text-center text-sm text-gray-600">
            <p>
              ¿Preguntas sobre el proceso?{' '}
              <a href="/faq" className="text-blue-600 hover:underline">
                Ver preguntas frecuentes
              </a>
            </p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};
