'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { AlertTriangle, ArrowLeft, RefreshCw, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';

export default function BrandError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error('Brand page error:', error);
  }, [error]);

  return (
    <div className="flex min-h-[60vh] items-center justify-center px-4">
      <div className="w-full max-w-md text-center">
        <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-red-100 dark:bg-red-900/30">
          <AlertTriangle className="h-8 w-8 text-red-600 dark:text-red-400" />
        </div>

        <h2 className="text-foreground mb-2 text-xl font-bold">
          Error cargando vehículos de esta marca
        </h2>
        <p className="text-muted-foreground mb-6 text-sm">
          No pudimos cargar los vehículos de esta marca. Esto puede ser un problema temporal.
        </p>

        <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
          <Button onClick={reset} className="bg-primary hover:bg-primary/90 gap-2">
            <RefreshCw className="h-4 w-4" />
            Intentar de nuevo
          </Button>
          <Button variant="outline" asChild className="gap-2">
            <Link href="/marcas">
              <Search className="h-4 w-4" />
              Ver todas las marcas
            </Link>
          </Button>
        </div>

        <div className="mt-6">
          <Link
            href="/"
            className="text-muted-foreground hover:text-primary inline-flex items-center gap-1 text-sm"
          >
            <ArrowLeft className="h-3.5 w-3.5" />
            Volver al inicio
          </Link>
        </div>

        {error.digest && (
          <p className="text-muted-foreground mt-4 text-xs">Código de error: {error.digest}</p>
        )}
      </div>
    </div>
  );
}
