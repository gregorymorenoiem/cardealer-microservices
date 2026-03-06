'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { AlertTriangle, RefreshCw, ArrowLeft, ShieldCheck } from 'lucide-react';

export default function CheckoutError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error('Checkout error:', error);
  }, [error]);

  return (
    <div className="bg-background flex min-h-[60vh] items-center justify-center px-4">
      <div className="max-w-md text-center">
        <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-amber-50">
          <AlertTriangle className="h-10 w-10 text-amber-500" />
        </div>

        <h1 className="text-foreground mb-2 text-2xl font-bold">Error en el proceso de pago</h1>

        <p className="text-muted-foreground mb-4">
          Ha ocurrido un error durante el proceso de pago. No se ha realizado ningún cargo a tu
          tarjeta.
        </p>

        <div className="mx-auto mb-6 flex items-center justify-center gap-2 rounded-lg bg-green-50 p-3 text-sm text-green-700">
          <ShieldCheck className="h-4 w-4 flex-shrink-0" />
          <span>Tu información de pago está segura — no se procesó ninguna transacción.</span>
        </div>

        {error.digest && (
          <p className="text-muted-foreground/70 mb-6 font-mono text-xs">Ref: {error.digest}</p>
        )}

        <div className="flex flex-col justify-center gap-3 sm:flex-row">
          <Button onClick={reset} className="bg-[#00A870] hover:bg-[#007850]">
            <RefreshCw className="mr-2 h-4 w-4" />
            Intentar de nuevo
          </Button>

          <Button variant="outline" asChild>
            <Link href="/">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Volver al inicio
            </Link>
          </Button>
        </div>

        <p className="text-muted-foreground mt-8 text-sm">
          ¿Necesitas ayuda?{' '}
          <Link href="/contacto" className="text-[#00A870] hover:underline">
            Contacta nuestro soporte
          </Link>
        </p>
      </div>
    </div>
  );
}
