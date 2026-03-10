/**
 * Odometer Alert Banner
 *
 * Visible alert shown to buyers when a vehicle's declared mileage
 * is lower than the historically-recorded mileage (VinAudit/CARFAX).
 * This is a trust & safety feature required by the OKLA verification system.
 *
 * Displayed prominently on the vehicle detail page when
 * vehicle.odometerRollbackDetected === true.
 */

'use client';

import { AlertTriangle, ExternalLink } from 'lucide-react';
import Link from 'next/link';
import { formatNumber } from '@/lib/utils';

interface OdometerAlertProps {
  declaredMileage: number;
  historicalMileage?: number;
}

export function OdometerAlert({ declaredMileage, historicalMileage }: OdometerAlertProps) {
  return (
    <div
      className="mb-4 rounded-xl border-2 border-amber-300 bg-amber-50 p-4 dark:border-amber-600 dark:bg-amber-950/30"
      role="alert"
      aria-label="Alerta de verificación de odómetro"
    >
      <div className="flex items-start gap-3">
        <div className="shrink-0 rounded-full bg-amber-200 p-2 dark:bg-amber-800">
          <AlertTriangle className="h-5 w-5 text-amber-700 dark:text-amber-300" />
        </div>
        <div className="flex-1">
          <h3 className="text-sm font-semibold text-amber-900 dark:text-amber-200">
            ⚠️ Verificar historial de odómetro
          </h3>
          <p className="mt-1 text-sm text-amber-800 dark:text-amber-300">
            El kilometraje declarado por el vendedor ({formatNumber(declaredMileage)} km) es{' '}
            {historicalMileage ? (
              <>
                <strong>menor</strong> al último kilometraje registrado en el historial del vehículo
                ({formatNumber(historicalMileage)} km).
              </>
            ) : (
              <>
                <strong>inconsistente</strong> con el historial registrado del vehículo.
              </>
            )}
          </p>
          <p className="mt-2 text-xs text-amber-700 dark:text-amber-400">
            Esto puede indicar un retroceso de odómetro. Te recomendamos solicitar un informe
            completo del historial del vehículo antes de tomar una decisión de compra.
          </p>
          <div className="mt-3 flex flex-wrap gap-2">
            <a
              href="https://www.vinaudit.com"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-1 rounded-md bg-amber-200 px-3 py-1.5 text-xs font-medium text-amber-900 transition-colors hover:bg-amber-300 dark:bg-amber-800 dark:text-amber-200 dark:hover:bg-amber-700"
            >
              Verificar en VinAudit
              <ExternalLink className="h-3 w-3" />
            </a>
            <Link
              href="/guias/como-verificar-vehiculo"
              className="inline-flex items-center gap-1 rounded-md border border-amber-300 px-3 py-1.5 text-xs font-medium text-amber-800 transition-colors hover:bg-amber-100 dark:border-amber-600 dark:text-amber-300 dark:hover:bg-amber-900"
            >
              Guía de verificación
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
