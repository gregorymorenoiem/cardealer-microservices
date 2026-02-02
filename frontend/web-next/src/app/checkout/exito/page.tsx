/**
 * Checkout Success Page
 *
 * Shown after successful payment
 */

'use client';

import * as React from 'react';
import { Suspense } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { CheckCircle, ArrowRight, Download, Share2, Home } from 'lucide-react';

function SuccessContent() {
  const searchParams = useSearchParams();
  const orderId = searchParams.get('orderId') || 'N/A';

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-emerald-50 to-white p-4">
      <Card className="w-full max-w-lg text-center">
        <CardContent className="pt-10 pb-8">
          {/* Success Icon */}
          <div className="mx-auto mb-6 flex h-20 w-20 animate-bounce items-center justify-center rounded-full bg-emerald-100">
            <CheckCircle className="h-12 w-12 text-emerald-600" />
          </div>

          {/* Title */}
          <h1 className="mb-2 text-2xl font-bold text-gray-900">¡Pago Exitoso!</h1>

          {/* Description */}
          <p className="mb-4 text-gray-600">Tu transacción ha sido procesada correctamente.</p>

          {/* Order Number */}
          <div className="mb-6 rounded-lg bg-gray-50 p-4">
            <p className="mb-1 text-sm text-gray-500">Número de Orden</p>
            <p className="font-mono text-lg font-bold text-gray-900">#{orderId}</p>
          </div>

          {/* What's Next */}
          <div className="mb-6 space-y-2 rounded-lg bg-emerald-50 p-4 text-left">
            <h3 className="font-semibold text-emerald-800">¿Qué sigue?</h3>
            <ul className="space-y-1 text-sm text-emerald-700">
              <li className="flex items-start gap-2">
                <CheckCircle className="mt-0.5 h-4 w-4 shrink-0" />
                Recibirás un email con la confirmación
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle className="mt-0.5 h-4 w-4 shrink-0" />
                Tu servicio se activará en los próximos minutos
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle className="mt-0.5 h-4 w-4 shrink-0" />
                Puedes ver el estado en tu dashboard
              </li>
            </ul>
          </div>

          {/* Actions */}
          <div className="space-y-3">
            <Button asChild className="w-full bg-emerald-600 hover:bg-emerald-700">
              <Link href="/cuenta">
                Ver Mi Cuenta
                <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>

            <div className="flex gap-3">
              <Button variant="outline" className="flex-1">
                <Download className="mr-2 h-4 w-4" />
                Recibo
              </Button>
              <Button variant="outline" className="flex-1">
                <Share2 className="mr-2 h-4 w-4" />
                Compartir
              </Button>
            </div>

            <Button variant="ghost" asChild className="w-full">
              <Link href="/">
                <Home className="mr-2 h-4 w-4" />
                Volver al Inicio
              </Link>
            </Button>
          </div>

          {/* Support */}
          <p className="mt-6 text-xs text-gray-500">
            ¿Tienes preguntas?{' '}
            <Link href="/contacto" className="text-emerald-600 hover:underline">
              Contáctanos
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

export default function CheckoutSuccessPage() {
  return (
    <Suspense
      fallback={<div className="flex min-h-screen items-center justify-center">Cargando...</div>}
    >
      <SuccessContent />
    </Suspense>
  );
}
