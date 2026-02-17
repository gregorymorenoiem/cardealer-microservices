'use client';

import type { SmartVinDecodeResult, FieldConfidence } from '@/services/vehicles';
import { Check, AlertTriangle, ChevronRight, Edit, Info } from 'lucide-react';

// ============================================================
// Helpers
// ============================================================

type ConfidenceLevel = 'high' | 'medium' | 'low';

function toLevel(fc: FieldConfidence | null | undefined): ConfidenceLevel | null {
  if (!fc) return null;
  if (fc.confidence >= 0.7) return 'high';
  if (fc.confidence >= 0.4) return 'medium';
  return 'low';
}

function computeOverallConfidence(
  confidences: Record<string, FieldConfidence>
): ConfidenceLevel {
  const vals = Object.values(confidences);
  if (vals.length === 0) return 'medium';
  const avg = vals.reduce((sum, v) => sum + v.confidence, 0) / vals.length;
  if (avg >= 0.7) return 'high';
  if (avg >= 0.4) return 'medium';
  return 'low';
}

// ============================================================
// Confidence Badge
// ============================================================

function ConfidenceBadge({ level }: { level: ConfidenceLevel }) {
  const config = {
    high: { label: 'Alta', bg: 'bg-emerald-100', text: 'text-emerald-700', dot: 'bg-emerald-500' },
    medium: { label: 'Media', bg: 'bg-amber-100', text: 'text-amber-700', dot: 'bg-amber-500' },
    low: { label: 'Baja', bg: 'bg-red-100', text: 'text-red-700', dot: 'bg-red-500' },
  };
  const c = config[level];
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${c.bg} ${c.text}`}
    >
      <span className={`h-1.5 w-1.5 rounded-full ${c.dot}`} />
      {c.label}
    </span>
  );
}

// ============================================================
// Component
// ============================================================

interface VinDecodeResultsProps {
  result: SmartVinDecodeResult;
  onContinue: () => void;
  onEditManual: () => void;
}

export function VinDecodeResults({ result, onContinue, onEditManual }: VinDecodeResultsProps) {
  const overallConfidence = computeOverallConfidence(result.fieldConfidences ?? {});

  const fields = [
    { label: 'Marca', value: result.make, confidence: toLevel(result.fieldConfidences?.make) },
    { label: 'Modelo', value: result.model, confidence: toLevel(result.fieldConfidences?.model) },
    {
      label: 'Año',
      value: result.year?.toString(),
      confidence: toLevel(result.fieldConfidences?.year),
    },
    { label: 'Trim', value: result.trim, confidence: toLevel(result.fieldConfidences?.trim) },
    {
      label: 'Motor',
      value: result.engineSize,
      confidence: toLevel(result.fieldConfidences?.engineSize),
    },
    {
      label: 'Transmisión',
      value: result.transmission,
      confidence: toLevel(result.fieldConfidences?.transmission),
    },
    {
      label: 'Tracción',
      value: result.driveType,
      confidence: toLevel(result.fieldConfidences?.driveType),
    },
    {
      label: 'Combustible',
      value: result.fuelType,
      confidence: toLevel(result.fieldConfidences?.fuelType),
    },
    {
      label: 'Carrocería',
      value: result.bodyStyle,
      confidence: toLevel(result.fieldConfidences?.bodyStyle),
    },
    { label: 'Puertas', value: result.doors?.toString(), confidence: null },
    { label: 'País de Origen', value: result.plantCountry, confidence: null },
  ].filter(f => f.value);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center">
        <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-emerald-100">
          <Check className="h-6 w-6 text-emerald-600" />
        </div>
        <h2 className="text-xl font-bold text-gray-900">VIN Decodificado</h2>
        <p className="mt-1 font-mono text-sm tracking-wider text-gray-500">{result.vin}</p>
      </div>

      {/* Vehicle Summary */}
      <div className="rounded-xl border-2 border-emerald-200 bg-emerald-50/50 p-5">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-xs font-medium tracking-wider text-emerald-600 uppercase">
              Vehículo Detectado
            </p>
            <p className="mt-1 text-2xl font-bold text-gray-900">
              {result.year} {result.make} {result.model}
            </p>
            {result.trim && (
              <p className="text-sm text-gray-600">{result.trim}</p>
            )}
          </div>
          <ConfidenceBadge level={overallConfidence} />
        </div>
      </div>

      {/* Generated Description */}
      {result.suggestedDescription && (
        <div className="rounded-lg bg-gray-50 p-4">
          <div className="mb-2 flex items-center gap-2">
            <Info className="h-4 w-4 text-gray-500" />
            <p className="text-xs font-medium tracking-wider text-gray-500 uppercase">
              Descripción Auto-generada
            </p>
          </div>
          <p className="text-sm leading-relaxed text-gray-700">{result.suggestedDescription}</p>
        </div>
      )}

      {/* Field Details */}
      <div className="overflow-hidden rounded-xl border border-gray-200">
        <div className="border-b border-gray-200 bg-gray-50 px-4 py-2.5">
          <h3 className="text-sm font-semibold text-gray-700">Detalles Detectados</h3>
        </div>
        <div className="divide-y divide-gray-100">
          {fields.map(field => (
            <div key={field.label} className="flex items-center justify-between px-4 py-3">
              <span className="text-sm text-gray-500">{field.label}</span>
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-gray-900">{field.value}</span>
                {field.confidence && <ConfidenceBadge level={field.confidence} />}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Duplicate Warning */}
      {result.isDuplicate && (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-4">
          <div className="flex items-start gap-3">
            <AlertTriangle className="mt-0.5 h-5 w-5 flex-shrink-0 text-amber-600" />
            <div>
              <p className="text-sm font-medium text-amber-800">
                Este VIN ya tiene un listado en OKLA
              </p>
              <p className="mt-1 text-sm text-amber-700">
                Puedes continuar, pero verifica que no sea un duplicado.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Confidence note */}
      {overallConfidence === 'low' && (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-4">
          <div className="flex items-start gap-3">
            <AlertTriangle className="mt-0.5 h-5 w-5 flex-shrink-0 text-amber-600" />
            <div>
              <p className="text-sm font-medium text-amber-800">Confianza baja en algunos campos</p>
              <p className="mt-1 text-sm text-amber-700">
                Algunos datos pueden no coincidir exactamente. Revisa y corrige la información en el
                siguiente paso.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex gap-3">
        <button
          onClick={onEditManual}
          className="flex flex-1 items-center justify-center gap-2 rounded-lg border-2 border-gray-300 bg-white px-4 py-3 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
        >
          <Edit className="h-4 w-4" />
          Editar Manualmente
        </button>
        <button
          onClick={onContinue}
          className="flex flex-1 items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 py-3 text-sm font-medium text-white transition-colors hover:bg-emerald-700"
        >
          Continuar con estos datos
          <ChevronRight className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}
