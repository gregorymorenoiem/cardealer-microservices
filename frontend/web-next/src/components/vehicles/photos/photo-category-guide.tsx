'use client';

import { useMemo } from 'react';
import { Camera, Check, ChevronRight, MapPin } from 'lucide-react';

// ============================================================
// CONSTANTS
// ============================================================

export interface PhotoAngle {
  id: string;
  name: string;
  description: string;
  icon: string;
  required: boolean;
}

export const PHOTO_ANGLES: PhotoAngle[] = [
  {
    id: 'front',
    name: 'Frente',
    description: 'Vista frontal completa',
    icon: '🚗',
    required: true,
  },
  {
    id: 'rear',
    name: 'Trasera',
    description: 'Vista trasera completa',
    icon: '🔙',
    required: true,
  },
  {
    id: 'left',
    name: 'Lado izquierdo',
    description: 'Perfil izquierdo completo',
    icon: '⬅️',
    required: true,
  },
  {
    id: 'right',
    name: 'Lado derecho',
    description: 'Perfil derecho completo',
    icon: '➡️',
    required: true,
  },
  {
    id: 'front-left',
    name: 'Frontal izquierda',
    description: 'Ángulo 3/4 frontal',
    icon: '↗️',
    required: false,
  },
  {
    id: 'front-right',
    name: 'Frontal derecha',
    description: 'Ángulo 3/4 frontal',
    icon: '↖️',
    required: false,
  },
  {
    id: 'interior',
    name: 'Interior',
    description: 'Vista del habitáculo',
    icon: '🪑',
    required: true,
  },
  {
    id: 'dashboard',
    name: 'Tablero',
    description: 'Instrumentos y consola',
    icon: '🎛️',
    required: false,
  },
  {
    id: 'engine',
    name: 'Motor',
    description: 'Compartimiento del motor',
    icon: '⚙️',
    required: false,
  },
  { id: 'trunk', name: 'Maletero', description: 'Espacio de carga', icon: '📦', required: false },
  { id: 'wheels', name: 'Ruedas', description: 'Detalle de llantas', icon: '🛞', required: false },
  { id: 'detail', name: 'Detalles', description: 'Detalles o daños', icon: '🔍', required: false },
];

// ============================================================
// TYPES
// ============================================================

interface CategoryGuideProps {
  uploadedAngles: string[];
  onAngleSelect?: (angleId: string) => void;
  compact?: boolean;
}

// ============================================================
// COMPONENT
// ============================================================

export function PhotoCategoryGuide({
  uploadedAngles,
  onAngleSelect,
  compact = false,
}: CategoryGuideProps) {
  const stats = useMemo(() => {
    const required = PHOTO_ANGLES.filter(a => a.required);
    const requiredDone = required.filter(a => uploadedAngles.includes(a.id));
    const total = uploadedAngles.length;
    return {
      requiredTotal: required.length,
      requiredDone: requiredDone.length,
      totalUploaded: total,
      allRequiredDone: requiredDone.length === required.length,
      progress: Math.round((requiredDone.length / required.length) * 100),
    };
  }, [uploadedAngles]);

  if (compact) {
    return (
      <div className="flex items-center gap-3 rounded-lg border border-gray-200 bg-gray-50 px-3 py-2">
        <Camera className="h-4 w-4 text-gray-500" />
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <div className="h-1.5 flex-1 overflow-hidden rounded-full bg-gray-200">
              <div
                className={`h-full rounded-full transition-all ${stats.allRequiredDone ? 'bg-emerald-500' : 'bg-amber-500'}`}
                style={{ width: `${stats.progress}%` }}
              />
            </div>
            <span className="text-xs font-medium text-gray-600">
              {stats.requiredDone}/{stats.requiredTotal} obligatorias
            </span>
          </div>
        </div>
        {stats.allRequiredDone && <Check className="h-4 w-4 text-emerald-500" />}
      </div>
    );
  }

  return (
    <div className="rounded-xl border border-gray-200 bg-white">
      {/* Header */}
      <div className="border-b border-gray-100 px-4 py-3">
        <div className="flex items-center justify-between">
          <div>
            <h4 className="text-sm font-semibold text-gray-900">Guía de fotos</h4>
            <p className="mt-0.5 text-xs text-gray-500">
              {stats.requiredDone}/{stats.requiredTotal} ángulos obligatorios ·{' '}
              {stats.totalUploaded} fotos totales
            </p>
          </div>
          {stats.allRequiredDone && (
            <div className="flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-600">
              <Check className="h-3 w-3" />
              Completa
            </div>
          )}
        </div>
        {/* Progress */}
        <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-gray-100">
          <div
            className={`h-full rounded-full transition-all duration-500 ${stats.allRequiredDone ? 'bg-emerald-500' : 'bg-amber-500'}`}
            style={{ width: `${stats.progress}%` }}
          />
        </div>
      </div>

      {/* Angle list */}
      <div className="divide-y divide-gray-50">
        {PHOTO_ANGLES.map(angle => {
          const isDone = uploadedAngles.includes(angle.id);
          return (
            <button
              key={angle.id}
              type="button"
              onClick={() => onAngleSelect?.(angle.id)}
              className={`flex w-full items-center gap-3 px-4 py-2.5 text-left transition-colors ${
                isDone ? 'bg-emerald-50/50' : 'hover:bg-gray-50'
              }`}
            >
              <span className="text-lg">{angle.icon}</span>
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-1.5">
                  <span className="text-sm font-medium text-gray-900">{angle.name}</span>
                  {angle.required && !isDone && (
                    <span className="rounded bg-red-50 px-1 py-0.5 text-[10px] font-semibold text-red-500">
                      Obligatoria
                    </span>
                  )}
                </div>
                <span className="text-xs text-gray-500">{angle.description}</span>
              </div>
              {isDone ? (
                <Check className="h-4 w-4 text-emerald-500" />
              ) : (
                <ChevronRight className="h-4 w-4 text-gray-300" />
              )}
            </button>
          );
        })}
      </div>

      {/* Tips */}
      <div className="border-t border-gray-100 px-4 py-3">
        <div className="flex items-start gap-2 rounded-lg bg-blue-50 p-2.5">
          <MapPin className="mt-0.5 h-4 w-4 flex-shrink-0 text-blue-500" />
          <div className="text-xs text-blue-700">
            <p className="font-medium">Consejos para mejores fotos:</p>
            <ul className="mt-1 list-inside list-disc space-y-0.5 text-blue-600">
              <li>Toma las fotos con buena iluminación natural</li>
              <li>Asegúrate de que el vehículo esté limpio</li>
              <li>Evita reflejos y sombras fuertes</li>
              <li>Incluye fotos de cualquier daño o detalle especial</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
