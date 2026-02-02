/**
 * 403 Forbidden Page
 *
 * Shown when user doesn't have permission to access a resource
 */

import { Metadata } from 'next';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { ShieldX, Home, ArrowLeft, LogIn } from 'lucide-react';

export const metadata: Metadata = {
  title: 'Acceso Denegado | OKLA',
  description: 'No tienes permiso para acceder a esta página.',
};

export default function ForbiddenPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 p-4">
      <Card className="w-full max-w-md text-center">
        <CardContent className="pt-10 pb-8">
          {/* Icon */}
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-red-100">
            <ShieldX className="h-10 w-10 text-red-600" />
          </div>

          {/* Title */}
          <h1 className="mb-2 text-2xl font-bold text-gray-900">Acceso Denegado</h1>

          {/* Description */}
          <p className="mb-8 text-gray-600">
            No tienes permiso para acceder a esta página. Si crees que esto es un error, por favor
            contacta a soporte.
          </p>

          {/* Error Code */}
          <p className="mb-8 text-6xl font-bold text-gray-200">403</p>

          {/* Actions */}
          <div className="flex flex-col justify-center gap-3 sm:flex-row">
            <Button variant="outline" asChild>
              <Link href="javascript:history.back()">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Volver
              </Link>
            </Button>
            <Button className="bg-emerald-600 hover:bg-emerald-700" asChild>
              <Link href="/">
                <Home className="mr-2 h-4 w-4" />
                Ir al Inicio
              </Link>
            </Button>
          </div>

          {/* Additional Help */}
          <div className="mt-8 border-t pt-6">
            <p className="mb-4 text-sm text-gray-500">¿Necesitas acceso a esta función?</p>
            <div className="flex justify-center gap-4 text-sm">
              <Link
                href="/login"
                className="flex items-center gap-1 text-emerald-600 hover:underline"
              >
                <LogIn className="h-3 w-3" />
                Iniciar Sesión
              </Link>
              <Link href="/contacto" className="text-emerald-600 hover:underline">
                Contactar Soporte
              </Link>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
