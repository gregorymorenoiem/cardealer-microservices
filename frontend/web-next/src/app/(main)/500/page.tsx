'use client';

/**
 * 500 Server Error Page
 */

import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { ServerCrash, Home, RefreshCw } from 'lucide-react';

export default function ServerErrorPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/50 p-4">
      <div className="max-w-md text-center">
        <div className="mb-6">
          <div className="mb-4 inline-flex h-20 w-20 items-center justify-center rounded-full bg-red-100">
            <ServerCrash className="h-10 w-10 text-red-600" />
          </div>
        </div>

        <h1 className="mb-2 text-4xl font-bold text-foreground">500</h1>
        <h2 className="mb-4 text-xl font-semibold text-foreground">Error del Servidor</h2>

        <p className="mb-8 text-muted-foreground">
          Ocurrió un error inesperado. Nuestro equipo ha sido notificado y estamos trabajando para
          solucionarlo.
        </p>

        <div className="flex flex-col justify-center gap-4 sm:flex-row">
          <Button
            className="bg-primary hover:bg-primary/90"
            onClick={() => window.location.reload()}
          >
            <RefreshCw className="mr-2 h-4 w-4" />
            Reintentar
          </Button>
          <Link href="/">
            <Button variant="outline">
              <Home className="mr-2 h-4 w-4" />
              Ir al Inicio
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
