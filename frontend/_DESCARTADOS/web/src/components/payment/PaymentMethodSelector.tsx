/**
 * PaymentMethodSelector Component
 *
 * Allows users to select between Stripe (international) and AZUL (Dominican Republic)
 * payment methods
 */

import React from 'react';
import { CreditCard, Building2, Check } from 'lucide-react';

export type PaymentMethod = 'stripe' | 'azul';

interface PaymentMethodSelectorProps {
  selectedMethod: PaymentMethod;
  onMethodChange: (method: PaymentMethod) => void;
  disabled?: boolean;
}

export const PaymentMethodSelector: React.FC<PaymentMethodSelectorProps> = ({
  selectedMethod,
  onMethodChange,
  disabled = false,
}) => {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-gray-900">Selecciona tu método de pago</h3>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* AZUL Payment Option */}
        <button
          type="button"
          onClick={() => onMethodChange('azul')}
          disabled={disabled}
          className={`
            relative flex flex-col items-start p-6 border-2 rounded-lg transition-all
            ${
              selectedMethod === 'azul'
                ? 'border-blue-600 bg-blue-50 ring-2 ring-blue-600'
                : 'border-gray-200 bg-white hover:border-gray-300'
            }
            ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}
          `}
        >
          {/* Check icon for selected */}
          {selectedMethod === 'azul' && (
            <div className="absolute top-4 right-4">
              <div className="bg-blue-600 rounded-full p-1">
                <Check className="w-4 h-4 text-white" />
              </div>
            </div>
          )}

          {/* AZUL Logo/Icon */}
          <div className="flex items-center gap-3 mb-3">
            <div className="p-2 bg-blue-100 rounded-lg">
              <Building2 className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <h4 className="text-lg font-semibold text-gray-900">AZUL</h4>
              <p className="text-sm text-gray-500">Banco Popular RD</p>
            </div>
          </div>

          {/* Description */}
          <div className="text-left space-y-2">
            <p className="text-sm text-gray-600">
              Paga con tarjetas dominicanas (Visa, Mastercard, AmEx)
            </p>
            <ul className="text-xs text-gray-500 space-y-1">
              <li>✓ Procesamiento local (RD)</li>
              <li>✓ Transacción en pesos (DOP)</li>
              <li>✓ Comisión ~2.5%</li>
              <li>✓ Depósito en 24-48 horas</li>
            </ul>
          </div>

          {/* Badge */}
          <div className="mt-3">
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
              Recomendado para RD
            </span>
          </div>
        </button>

        {/* Stripe Payment Option */}
        <button
          type="button"
          onClick={() => onMethodChange('stripe')}
          disabled={disabled}
          className={`
            relative flex flex-col items-start p-6 border-2 rounded-lg transition-all
            ${
              selectedMethod === 'stripe'
                ? 'border-purple-600 bg-purple-50 ring-2 ring-purple-600'
                : 'border-gray-200 bg-white hover:border-gray-300'
            }
            ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}
          `}
        >
          {/* Check icon for selected */}
          {selectedMethod === 'stripe' && (
            <div className="absolute top-4 right-4">
              <div className="bg-purple-600 rounded-full p-1">
                <Check className="w-4 h-4 text-white" />
              </div>
            </div>
          )}

          {/* Stripe Logo/Icon */}
          <div className="flex items-center gap-3 mb-3">
            <div className="p-2 bg-purple-100 rounded-lg">
              <CreditCard className="w-6 h-6 text-purple-600" />
            </div>
            <div>
              <h4 className="text-lg font-semibold text-gray-900">Stripe</h4>
              <p className="text-sm text-gray-500">Internacional</p>
            </div>
          </div>

          {/* Description */}
          <div className="text-left space-y-2">
            <p className="text-sm text-gray-600">Tarjetas internacionales, Apple Pay, Google Pay</p>
            <ul className="text-xs text-gray-500 space-y-1">
              <li>✓ Procesamiento internacional</li>
              <li>✓ Transacción en USD</li>
              <li>✓ Comisión ~3.5%</li>
              <li>✓ Depósito en 7 días</li>
            </ul>
          </div>

          {/* Badge */}
          <div className="mt-3">
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
              Internacional
            </span>
          </div>
        </button>
      </div>

      {/* Info Box */}
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-4">
        <div className="flex gap-3">
          <div className="flex-shrink-0">
            <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
              <path
                fillRule="evenodd"
                d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                clipRule="evenodd"
              />
            </svg>
          </div>
          <div className="text-sm text-gray-600">
            <p className="font-medium mb-1">💡 Consejo:</p>
            <p>
              {selectedMethod === 'azul' ? (
                <>
                  <strong>AZUL</strong> es ideal para compradores dominicanos con tarjetas locales.
                  Procesamiento más rápido y menores comisiones.
                </>
              ) : (
                <>
                  <strong>Stripe</strong> es mejor para tarjetas internacionales o métodos como
                  Apple Pay/Google Pay.
                </>
              )}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
