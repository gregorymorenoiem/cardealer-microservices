'use client';

import Link from 'next/link';

export default function OfflinePage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 px-4">
      <div className="w-full max-w-md text-center">
        {/* Offline Icon */}
        <div className="mb-8">
          <svg
            className="mx-auto h-32 w-32 text-muted-foreground"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={1.5}
              d="M18.364 5.636a9 9 0 010 12.728m0 0l-2.829-2.829m2.829 2.829L21 21M15.536 8.464a5 5 0 010 7.072m0 0l-2.829-2.829m-4.243 2.829a5 5 0 01-7.071-7.072M8.464 8.464L21 21"
            />
          </svg>
        </div>

        {/* Title */}
        <h1 className="mb-4 text-3xl font-bold text-foreground">Sin Conexión</h1>

        {/* Description */}
        <p className="mb-8 leading-relaxed text-muted-foreground">
          Parece que no tienes conexión a internet. Verifica tu conexión Wi-Fi o datos móviles e
          intenta nuevamente.
        </p>

        {/* What you can do section */}
        <div className="mb-8 rounded-xl bg-card p-6 text-left shadow-sm">
          <h2 className="mb-4 font-semibold text-foreground">¿Qué puedes hacer?</h2>
          <ul className="space-y-3">
            <li className="flex items-start gap-3">
              <span className="mt-0.5 text-green-500">✓</span>
              <span className="text-muted-foreground">
                Ver vehículos que visitaste recientemente (guardados en caché)
              </span>
            </li>
            <li className="flex items-start gap-3">
              <span className="mt-0.5 text-green-500">✓</span>
              <span className="text-muted-foreground">Revisar tus favoritos guardados</span>
            </li>
            <li className="flex items-start gap-3">
              <span className="mt-0.5 text-green-500">✓</span>
              <span className="text-muted-foreground">Continuar navegando cuando vuelvas a conectarte</span>
            </li>
          </ul>
        </div>

        {/* Actions */}
        <div className="space-y-3">
          <button
            onClick={() => window.location.reload()}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-green-600 px-6 py-3 font-medium text-white transition-colors hover:bg-green-700"
          >
            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
              />
            </svg>
            Reintentar Conexión
          </button>

          <Link
            href="/"
            className="block w-full rounded-lg bg-muted px-6 py-3 font-medium text-foreground transition-colors hover:bg-muted"
          >
            Ir al Inicio
          </Link>
        </div>

        {/* Cached pages hint */}
        <p className="mt-8 text-sm text-muted-foreground">
          💡 Algunas páginas pueden estar disponibles sin conexión si las visitaste antes.
        </p>

        {/* OKLA branding */}
        <div className="mt-12 border-t border-border pt-8">
          <div className="flex items-center justify-center gap-2 text-muted-foreground">
            <svg className="h-8 w-8" viewBox="0 0 40 40" fill="currentColor">
              <circle cx="20" cy="20" r="18" className="text-green-600" fill="currentColor" />
              <text x="20" y="26" textAnchor="middle" fill="white" fontSize="14" fontWeight="bold">
                O
              </text>
            </svg>
            <span className="font-semibold text-muted-foreground">OKLA</span>
          </div>
        </div>
      </div>
    </div>
  );
}
