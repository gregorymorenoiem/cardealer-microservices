/**
 * KYC Verification Gate
 *
 * Blocks access to seller features if user is not KYC verified.
 * Shows appropriate message based on verification status.
 */

'use client';

import Link from 'next/link';
import { Shield, Clock, XCircle, ArrowRight, Loader2 } from 'lucide-react';
import { useCanSell } from '@/hooks/use-kyc';

interface VerificationGateProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export function VerificationGate({ children, fallback }: VerificationGateProps) {
  const { canSell, isPending, isRejected, isLoading, rejectionReason } = useCanSell();

  // Show loading state
  if (isLoading) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <div className="text-center">
          <Loader2 className="mx-auto h-8 w-8 animate-spin text-blue-600" />
          <p className="mt-2 text-gray-600">Verificando estado de cuenta...</p>
        </div>
      </div>
    );
  }

  // User is verified - show children
  if (canSell) {
    return <>{children}</>;
  }

  // Show fallback if provided
  if (fallback) {
    return <>{fallback}</>;
  }

  // Verification pending/in review
  if (isPending) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center px-4">
        <div className="w-full max-w-md text-center">
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-purple-100">
            <Clock className="h-10 w-10 text-purple-600" />
          </div>
          <h1 className="mb-3 text-2xl font-bold text-gray-900">Verificación en Proceso</h1>
          <p className="mb-6 text-gray-600">
            Tu solicitud de verificación está siendo revisada por nuestro equipo. Te notificaremos
            por correo cuando esté lista.
          </p>

          <div className="mb-6 rounded-xl bg-purple-50 p-4">
            <div className="flex items-center justify-center gap-2 font-medium text-purple-700">
              <Clock className="h-4 w-4" />
              <span>Tiempo estimado: 24-48 horas</span>
            </div>
          </div>

          <div className="flex flex-col justify-center gap-3 sm:flex-row">
            <Link
              href="/cuenta"
              className="rounded-lg border border-gray-300 px-6 py-3 font-medium text-gray-700 transition-colors hover:bg-gray-50"
            >
              Ir a mi cuenta
            </Link>
          </div>
        </div>
      </div>
    );
  }

  // Verification rejected
  if (isRejected) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center px-4">
        <div className="w-full max-w-md text-center">
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-red-100">
            <XCircle className="h-10 w-10 text-red-600" />
          </div>
          <h1 className="mb-3 text-2xl font-bold text-gray-900">Verificación Rechazada</h1>
          <p className="mb-4 text-gray-600">Tu solicitud de verificación fue rechazada.</p>

          {rejectionReason && (
            <div className="mb-6 rounded-xl bg-red-50 p-4 text-left">
              <p className="mb-1 text-sm font-medium text-red-800">Motivo:</p>
              <p className="text-sm text-red-700">{rejectionReason}</p>
            </div>
          )}

          <div className="flex flex-col justify-center gap-3 sm:flex-row">
            <Link
              href="/cuenta/verificacion"
              className="flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-medium text-white transition-colors hover:bg-blue-700"
            >
              Intentar Nuevamente
              <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              href="/ayuda/contacto"
              className="rounded-lg border border-gray-300 px-6 py-3 font-medium text-gray-700 transition-colors hover:bg-gray-50"
            >
              Contactar Soporte
            </Link>
          </div>
        </div>
      </div>
    );
  }

  // Needs verification (no profile or pending)
  return (
    <div className="flex min-h-[60vh] items-center justify-center px-4">
      <div className="w-full max-w-md text-center">
        <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-blue-100">
          <Shield className="h-10 w-10 text-blue-600" />
        </div>
        <h1 className="mb-3 text-2xl font-bold text-gray-900">Verificación Requerida</h1>
        <p className="mb-6 text-gray-600">
          Para publicar vehículos en OKLA necesitas verificar tu identidad. Es un proceso rápido y
          seguro que protege a compradores y vendedores.
        </p>

        {/* Benefits */}
        <div className="mb-6 rounded-xl bg-gray-50 p-4 text-left">
          <p className="mb-3 text-sm font-medium text-gray-900">¿Por qué verificamos?</p>
          <ul className="space-y-2 text-sm text-gray-600">
            <li className="flex items-start gap-2">
              <Shield className="mt-0.5 h-4 w-4 flex-shrink-0 text-green-600" />
              <span>Protege contra fraudes y estafas</span>
            </li>
            <li className="flex items-start gap-2">
              <Shield className="mt-0.5 h-4 w-4 flex-shrink-0 text-green-600" />
              <span>Genera confianza en los compradores</span>
            </li>
            <li className="flex items-start gap-2">
              <Shield className="mt-0.5 h-4 w-4 flex-shrink-0 text-green-600" />
              <span>Cumple con la Ley 155-17 de RD</span>
            </li>
          </ul>
        </div>

        {/* What you need */}
        <div className="mb-6 rounded-xl bg-blue-50 p-4 text-left">
          <p className="mb-2 text-sm font-medium text-blue-900">Necesitarás:</p>
          <ul className="space-y-1 text-sm text-blue-700">
            <li>• Tu cédula de identidad dominicana</li>
            <li>• Acceso a la cámara de tu dispositivo</li>
            <li>• 5 minutos de tu tiempo</li>
          </ul>
        </div>

        <div className="flex flex-col justify-center gap-3 sm:flex-row">
          <Link
            href="/cuenta/verificacion"
            className="flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-medium text-white transition-colors hover:bg-blue-700"
          >
            Verificar mi identidad
            <ArrowRight className="h-4 w-4" />
          </Link>
          <Link
            href="/vender"
            className="rounded-lg border border-gray-300 px-6 py-3 font-medium text-gray-700 transition-colors hover:bg-gray-50"
          >
            Volver
          </Link>
        </div>
      </div>
    </div>
  );
}

export default VerificationGate;
