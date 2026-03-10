/**
 * Report Vehicle Modal — Buyer Listing Report System
 *
 * Allows ANY buyer (no registration required) to report a vehicle listing.
 * Categories match audit requirements:
 *  - Precio incorrecto
 *  - Vehículo no disponible
 *  - Fotos no corresponden
 *  - Posible fraude
 *
 * Submits to POST /api/contentreports/vehicle/anonymous
 * After ≥3 reports the listing is auto-suspended by the backend.
 */

'use client';

import * as React from 'react';
import { AlertTriangle, CheckCircle2, X, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { apiClient } from '@/lib/api-client';

// ── Report categories (must match backend ContentReportReason enum) ──
const REPORT_CATEGORIES = [
  {
    id: 'PrecioIncorrecto',
    label: 'Precio incorrecto',
    description: 'El precio publicado no corresponde al real',
    icon: '💰',
  },
  {
    id: 'VehiculoNoDisponible',
    label: 'Vehículo no disponible',
    description: 'Este vehículo ya fue vendido o no está disponible',
    icon: '🚫',
  },
  {
    id: 'FotosNoCorresponden',
    label: 'Fotos no corresponden',
    description: 'Las fotos del anuncio no son del vehículo real',
    icon: '📷',
  },
  {
    id: 'PosibleFraude',
    label: 'Posible fraude',
    description: 'Este anuncio parece ser una estafa',
    icon: '⚠️',
  },
] as const;

interface ReportVehicleModalProps {
  open: boolean;
  onClose: () => void;
  vehicleId: string;
  vehicleTitle: string;
}

export function ReportVehicleModal({
  open,
  onClose,
  vehicleId,
  vehicleTitle,
}: ReportVehicleModalProps) {
  const [selectedCategory, setSelectedCategory] = React.useState<string | null>(null);
  const [description, setDescription] = React.useState('');
  const [contactEmail, setContactEmail] = React.useState('');
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const [submitted, setSubmitted] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  // Reset state when modal opens
  React.useEffect(() => {
    if (open) {
      setSelectedCategory(null);
      setDescription('');
      setContactEmail('');
      setSubmitted(false);
      setError(null);
    }
  }, [open]);

  const handleSubmit = async () => {
    if (!selectedCategory) return;

    setIsSubmitting(true);
    setError(null);

    try {
      await apiClient.post('/api/contentreports/vehicle/anonymous', {
        vehicleId,
        vehicleTitle,
        reportCategory: selectedCategory,
        description: description.trim() || undefined,
        contactEmail: contactEmail.trim() || undefined,
      });

      setSubmitted(true);
    } catch {
      setError('No se pudo enviar el reporte. Intenta de nuevo.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
      <div
        className="relative mx-4 w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl dark:bg-slate-900"
        role="dialog"
        aria-modal="true"
        aria-labelledby="report-modal-title"
      >
        {/* Close button */}
        <button
          onClick={onClose}
          className="absolute top-4 right-4 rounded-full p-1 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
          aria-label="Cerrar"
        >
          <X className="h-5 w-5" />
        </button>

        {submitted ? (
          /* ── Success state ── */
          <div className="py-6 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-green-100 dark:bg-green-900/30">
              <CheckCircle2 className="h-8 w-8 text-green-600 dark:text-green-400" />
            </div>
            <h3 className="text-lg font-bold text-slate-900 dark:text-white">¡Reporte enviado!</h3>
            <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
              Nuestro equipo de moderación revisará este anuncio en los próximos minutos. Gracias
              por ayudar a mantener OKLA seguro.
            </p>
            <Button onClick={onClose} className="mt-6" variant="outline">
              Cerrar
            </Button>
          </div>
        ) : (
          /* ── Form state ── */
          <>
            {/* Header */}
            <div className="mb-5 flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-red-100 dark:bg-red-900/30">
                <AlertTriangle className="h-5 w-5 text-red-600 dark:text-red-400" />
              </div>
              <div>
                <h3
                  id="report-modal-title"
                  className="text-lg font-bold text-slate-900 dark:text-white"
                >
                  Reportar este vehículo
                </h3>
                <p className="mt-0.5 text-sm text-slate-500 dark:text-slate-400">
                  Selecciona el motivo del reporte. No necesitas una cuenta.
                </p>
              </div>
            </div>

            {/* Categories */}
            <div className="space-y-2">
              {REPORT_CATEGORIES.map(cat => (
                <button
                  key={cat.id}
                  onClick={() => setSelectedCategory(cat.id)}
                  className={`w-full rounded-xl border p-3 text-left transition-all ${
                    selectedCategory === cat.id
                      ? 'border-red-500 bg-red-50 ring-2 ring-red-200 dark:border-red-400 dark:bg-red-900/20 dark:ring-red-800'
                      : 'border-slate-200 hover:border-slate-300 hover:bg-slate-50 dark:border-slate-700 dark:hover:border-slate-600 dark:hover:bg-slate-800'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <span className="text-xl">{cat.icon}</span>
                    <div>
                      <p className="font-medium text-slate-900 dark:text-white">{cat.label}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">
                        {cat.description}
                      </p>
                    </div>
                  </div>
                </button>
              ))}
            </div>

            {/* Optional description */}
            {selectedCategory && (
              <div className="mt-4 space-y-3">
                <textarea
                  value={description}
                  onChange={e => setDescription(e.target.value)}
                  placeholder="Describe el problema (opcional)..."
                  className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-red-500 focus:ring-1 focus:ring-red-500 focus:outline-none dark:border-slate-700 dark:bg-slate-800 dark:text-white"
                  rows={3}
                  maxLength={500}
                />
                <input
                  type="email"
                  value={contactEmail}
                  onChange={e => setContactEmail(e.target.value)}
                  placeholder="Tu email (opcional, para seguimiento)"
                  className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-red-500 focus:ring-1 focus:ring-red-500 focus:outline-none dark:border-slate-700 dark:bg-slate-800 dark:text-white"
                />
              </div>
            )}

            {/* Error */}
            {error && (
              <p className="mt-3 text-center text-sm text-red-600 dark:text-red-400">{error}</p>
            )}

            {/* Submit */}
            <div className="mt-5 flex gap-3">
              <Button onClick={onClose} variant="outline" className="flex-1">
                Cancelar
              </Button>
              <Button
                onClick={handleSubmit}
                disabled={!selectedCategory || isSubmitting}
                className="flex-1 gap-2 bg-red-600 text-white hover:bg-red-700 disabled:opacity-50"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Enviando...
                  </>
                ) : (
                  'Enviar reporte'
                )}
              </Button>
            </div>

            {/* Legal note */}
            <p className="mt-3 text-center text-[11px] text-slate-400 dark:text-slate-500">
              Los reportes falsos reiterados pueden resultar en restricciones. Ley 53-07 sobre
              crímenes de alta tecnología.
            </p>
          </>
        )}
      </div>
    </div>
  );
}

export default ReportVehicleModal;
