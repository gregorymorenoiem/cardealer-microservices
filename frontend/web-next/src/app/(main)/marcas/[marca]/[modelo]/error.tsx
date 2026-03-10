'use client';

/**
 * Error boundary for /marcas/[marca]/[modelo] page.
 * Catches rendering errors and provides a retry + fallback UI.
 */
import Link from 'next/link';

export default function ModelError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="bg-background flex min-h-screen items-center justify-center">
      <div className="mx-auto max-w-md text-center">
        <h2 className="mb-4 text-2xl font-bold">No pudimos cargar este modelo</h2>
        <p className="text-muted-foreground mb-6">
          {error.message ||
            'Ocurrió un error al cargar los vehículos de este modelo. Intenta de nuevo.'}
        </p>
        <div className="flex items-center justify-center gap-4">
          <button
            onClick={reset}
            className="bg-primary text-primary-foreground rounded-lg px-6 py-2 font-medium transition hover:opacity-90"
          >
            Reintentar
          </button>
          <Link href="/marcas" className="text-primary underline">
            Ver todas las marcas
          </Link>
        </div>
      </div>
    </div>
  );
}
