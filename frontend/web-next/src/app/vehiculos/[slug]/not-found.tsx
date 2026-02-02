/**
 * Vehicle Not Found Page
 * Shows when a vehicle doesn't exist or has been removed
 */

import Link from 'next/link';
import { Car, Search, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';

export default function VehicleNotFound() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 p-4">
      <div className="w-full max-w-md text-center">
        {/* Icon */}
        <div className="mx-auto mb-6 flex h-24 w-24 items-center justify-center rounded-full bg-gray-100">
          <Car className="h-12 w-12 text-gray-400" />
        </div>

        {/* Title */}
        <h1 className="mb-2 text-2xl font-bold text-gray-900">Vehículo no encontrado</h1>

        {/* Description */}
        <p className="mb-8 text-gray-600">
          El vehículo que buscas no está disponible. Puede que haya sido vendido o removido.
        </p>

        {/* Actions */}
        <div className="flex flex-col justify-center gap-3 sm:flex-row">
          <Button asChild variant="outline" className="gap-2">
            <Link href="/vehiculos">
              <ArrowLeft className="h-4 w-4" />
              Ver todos los vehículos
            </Link>
          </Button>
          <Button asChild className="gap-2">
            <Link href="/">
              <Search className="h-4 w-4" />
              Buscar otro vehículo
            </Link>
          </Button>
        </div>

        {/* Help text */}
        <p className="mt-8 text-sm text-gray-500">
          ¿Necesitas ayuda?{' '}
          <Link href="/ayuda" className="text-primary hover:underline">
            Contáctanos
          </Link>
        </p>
      </div>
    </div>
  );
}
