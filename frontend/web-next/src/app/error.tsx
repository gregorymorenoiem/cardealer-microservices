'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { AlertTriangle, RefreshCw, Home } from 'lucide-react';

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    // Log the error to an error reporting service
    console.error('Application error:', error);
  }, [error]);

  return (
    <div className="bg-background flex min-h-screen items-center justify-center px-4">
      <div className="max-w-md text-center">
        <div className="bg-destructive/10 mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full">
          <AlertTriangle className="text-destructive h-10 w-10" />
        </div>

        <h1 className="text-foreground mb-2 text-2xl font-bold">Algo salió mal</h1>

        <p className="text-muted-foreground mb-6">
          Ha ocurrido un error inesperado. Estamos trabajando para resolverlo.
        </p>

        {error.digest && (
          <p className="text-muted-foreground/70 mb-6 font-mono text-xs">
            Error ID: {error.digest}
          </p>
        )}

        <div className="flex flex-col justify-center gap-3 sm:flex-row">
          <Button onClick={reset} variant="default">
            <RefreshCw className="mr-2 h-4 w-4" />
            Intentar de nuevo
          </Button>

          <Button variant="outline" asChild>
            <Link href="/">
              <Home className="mr-2 h-4 w-4" />
              Ir al inicio
            </Link>
          </Button>
        </div>

        <p className="text-muted-foreground mt-8 text-sm">
          Si el problema persiste,{' '}
          <Link href="/contacto" className="text-primary hover:underline">
            contáctanos
          </Link>
        </p>
      </div>
    </div>
  );
}
