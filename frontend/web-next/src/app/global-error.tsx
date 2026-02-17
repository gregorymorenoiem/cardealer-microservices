'use client';

import { useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { AlertTriangle, RefreshCw } from 'lucide-react';

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    // Log the error to an error reporting service
    console.error('Global application error:', error);
  }, [error]);

  return (
    <html>
      <body>
        <div className="flex min-h-screen items-center justify-center bg-slate-900 px-4">
          <div className="max-w-md text-center">
            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-red-900/30">
              <AlertTriangle className="h-10 w-10 text-red-400" />
            </div>

            <h1 className="mb-2 text-2xl font-bold text-white">Error crítico</h1>

            <p className="mb-6 text-muted-foreground">
              Ha ocurrido un error grave en la aplicación. Por favor, intenta recargar la página.
            </p>

            {error.digest && (
              <p className="mb-6 font-mono text-xs text-muted-foreground">Error ID: {error.digest}</p>
            )}

            <Button onClick={reset} className="bg-primary hover:bg-primary/90">
              <RefreshCw className="mr-2 h-4 w-4" />
              Reiniciar aplicación
            </Button>

            <p className="mt-8 text-sm text-muted-foreground">
              Si el problema persiste, por favor contacta a soporte.
            </p>
          </div>
        </div>
      </body>
    </html>
  );
}
