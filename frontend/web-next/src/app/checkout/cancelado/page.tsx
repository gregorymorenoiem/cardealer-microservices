/**
 * Checkout Cancelled Page
 *
 * Shown when user cancels the payment
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { XCircle, ArrowLeft, RefreshCw, HelpCircle, Home } from 'lucide-react';

export default function CheckoutCancelledPage() {
  const router = useRouter();

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-amber-50 to-white p-4">
      <Card className="w-full max-w-lg text-center">
        <CardContent className="pt-10 pb-8">
          {/* Cancel Icon */}
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-amber-100">
            <XCircle className="h-12 w-12 text-amber-600" />
          </div>

          {/* Title */}
          <h1 className="mb-2 text-2xl font-bold text-gray-900">Pago Cancelado</h1>

          {/* Description */}
          <p className="mb-6 text-gray-600">
            Has cancelado el proceso de pago. No se ha realizado ningún cargo a tu cuenta.
          </p>

          {/* Info Box */}
          <div className="mb-6 rounded-lg bg-amber-50 p-4 text-left">
            <h3 className="mb-2 font-semibold text-amber-800">No te preocupes</h3>
            <ul className="space-y-1 text-sm text-amber-700">
              <li>• Tu carrito de compras sigue guardado</li>
              <li>• No se realizó ningún cargo</li>
              <li>• Puedes intentar de nuevo cuando quieras</li>
            </ul>
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
              <Link href="/buscar">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Seguir Explorando
              </Link>
            </Button>

            <Button variant="ghost" asChild className="w-full">
              <Link href="/">
                <Home className="mr-2 h-4 w-4" />
                Volver al Inicio
              </Link>
            </Button>
          </div>

          {/* Support */}
          <div className="mt-6 border-t pt-6">
            <p className="flex items-center justify-center gap-2 text-sm text-gray-500">
              <HelpCircle className="h-4 w-4" />
              ¿Tuviste algún problema?{' '}
              <Link href="/contacto" className="font-medium text-emerald-600 hover:underline">
                Contáctanos
              </Link>
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
