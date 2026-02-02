import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Search, Home, ArrowLeft } from 'lucide-react';

export default function NotFound() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="max-w-lg text-center">
        {/* 404 Number */}
        <div className="relative mb-6">
          <h1 className="text-[150px] leading-none font-bold text-gray-200 select-none">404</h1>
          <div className="absolute inset-0 flex items-center justify-center">
            <div className="flex h-24 w-24 items-center justify-center rounded-full bg-emerald-100">
              <Search className="h-12 w-12 text-emerald-600" />
            </div>
          </div>
        </div>

        <h2 className="mb-3 text-2xl font-bold text-gray-900">Página no encontrada</h2>

        <p className="mb-8 text-gray-600">
          Lo sentimos, no pudimos encontrar la página que buscas. Es posible que haya sido movida o
          eliminada.
        </p>

        <div className="flex flex-col justify-center gap-3 sm:flex-row">
          <Button variant="default" className="bg-emerald-600 hover:bg-emerald-700" asChild>
            <Link href="/">
              <Home className="mr-2 h-4 w-4" />
              Ir al inicio
            </Link>
          </Button>

          <Button variant="outline" asChild>
            <Link href="/vehiculos">
              <Search className="mr-2 h-4 w-4" />
              Buscar vehículos
            </Link>
          </Button>
        </div>

        {/* Suggestions */}
        <div className="mt-12 border-t border-gray-200 pt-8">
          <p className="mb-4 text-sm text-gray-500">¿Buscabas algo de esto?</p>
          <div className="flex flex-wrap justify-center gap-2">
            <Link
              href="/vehiculos"
              className="rounded-full bg-emerald-50 px-3 py-1 text-sm text-emerald-600 hover:underline"
            >
              Ver vehículos
            </Link>
            <Link
              href="/dealers"
              className="rounded-full bg-emerald-50 px-3 py-1 text-sm text-emerald-600 hover:underline"
            >
              Dealers
            </Link>
            <Link
              href="/buscar"
              className="rounded-full bg-emerald-50 px-3 py-1 text-sm text-emerald-600 hover:underline"
            >
              Búsqueda avanzada
            </Link>
            <Link
              href="/ayuda"
              className="rounded-full bg-emerald-50 px-3 py-1 text-sm text-emerald-600 hover:underline"
            >
              Centro de ayuda
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
