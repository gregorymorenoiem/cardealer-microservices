/**
 * Checkout Error Page
 *
 * Shown when payment fails
 */

'use client';

import * as React from 'react';
import { Suspense } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { AlertTriangle, RefreshCw, CreditCard, Phone, Home } from 'lucide-react';

const ERROR_MESSAGES: Record<string, { title: string; description: string; suggestion: string }> = {
  'insufficient-funds': {
    title: 'Fondos Insuficientes',
    description: 'Tu tarjeta no tiene fondos suficientes para completar la transacción.',
    suggestion: 'Intenta con otra tarjeta o verifica tu saldo disponible.',
  },
  'card-declined': {
    title: 'Tarjeta Rechazada',
    description: 'Tu banco ha rechazado la transacción.',
    suggestion: 'Contacta a tu banco o intenta con otra tarjeta.',
  },
  'expired-card': {
    title: 'Tarjeta Expirada',
    description: 'La tarjeta que usaste ha expirado.',
    suggestion: 'Actualiza los datos de tu tarjeta e intenta de nuevo.',
  },
  'invalid-card': {
    title: 'Datos Inválidos',
    description: 'Los datos de la tarjeta ingresados son incorrectos.',
    suggestion: 'Verifica el número de tarjeta, fecha y CVV.',
  },
  'network-error': {
    title: 'Error de Conexión',
    description: 'Hubo un problema de conexión durante el proceso de pago.',
    suggestion: 'Verifica tu conexión a internet e intenta de nuevo.',
  },
  timeout: {
    title: 'Tiempo Agotado',
    description: 'El proceso de pago tardó demasiado tiempo.',
    suggestion: 'Intenta de nuevo. Si el problema persiste, contacta soporte.',
  },
  default: {
    title: 'Error en el Pago',
    description: 'Ocurrió un error inesperado al procesar tu pago.',
    suggestion: 'Por favor intenta de nuevo o contacta a soporte.',
  },
};

function ErrorContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const errorCode = searchParams.get('code') || 'default';
  const errorInfo = ERROR_MESSAGES[errorCode] || ERROR_MESSAGES.default;

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-red-50 to-white p-4">
      <Card className="w-full max-w-lg text-center">
        <CardContent className="pt-10 pb-8">
          {/* Error Icon */}
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-red-100">
            <AlertTriangle className="h-12 w-12 text-red-600" />
          </div>

          {/* Title */}
          <h1 className="mb-2 text-2xl font-bold text-gray-900">{errorInfo.title}</h1>

          {/* Description */}
          <p className="mb-4 text-gray-600">{errorInfo.description}</p>

          {/* Error Code */}
          {errorCode !== 'default' && (
            <p className="mb-4 text-xs text-gray-400">
              Código de error: <span className="font-mono">{errorCode}</span>
            </p>
          )}

          {/* Suggestion Box */}
          <div className="mb-6 rounded-lg bg-blue-50 p-4 text-left">
            <h3 className="mb-1 font-semibold text-blue-800">💡 Sugerencia</h3>
            <p className="text-sm text-blue-700">{errorInfo.suggestion}</p>
          </div>

          {/* Actions */}
          <div className="space-y-3">
            <Button
              onClick={() => router.back()}
              className="w-full bg-emerald-600 hover:bg-emerald-700"
            >
              <RefreshCw className="mr-2 h-4 w-4" />
              Intentar de Nuevo
            </Button>

            <Button variant="outline" asChild className="w-full">
              <Link href="/checkout?product=boost-basic">
                <CreditCard className="mr-2 h-4 w-4" />
                Cambiar Método de Pago
              </Link>
            </Button>

            <Button variant="ghost" asChild className="w-full">
              <Link href="/">
                <Home className="mr-2 h-4 w-4" />
                Volver al Inicio
              </Link>
            </Button>
          </div>

          {/* Support Section */}
          <div className="mt-6 border-t pt-6">
            <p className="mb-3 text-sm text-gray-600">¿Necesitas ayuda con tu pago?</p>
            <div className="flex justify-center gap-4">
              <Link
                href="/contacto"
                className="inline-flex items-center gap-1 text-sm text-emerald-600 hover:underline"
              >
                <Phone className="h-4 w-4" />
                Contactar Soporte
              </Link>
            </div>
          </div>

          {/* Security Notice */}
          <p className="mt-4 text-xs text-gray-400">
            Tu información de pago está protegida con encriptación SSL. No almacenamos los datos de
            tu tarjeta.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

export default function CheckoutErrorPage() {
  return (
    <Suspense
      fallback={<div className="flex min-h-screen items-center justify-center">Cargando...</div>}
    >
      <ErrorContent />
    </Suspense>
  );
}
