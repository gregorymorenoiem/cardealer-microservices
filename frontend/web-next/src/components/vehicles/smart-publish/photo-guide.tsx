'use client';

import {
  Camera,
  RotateCcw,
  ArrowUp,
  ArrowDown,
  ArrowLeft,
  ArrowRight,
  Circle,
  Gauge,
} from 'lucide-react';

const PHOTO_ANGLES = [
  { id: 'front', label: 'Frente', icon: ArrowUp, tip: 'Vista frontal centrada del vehículo' },
  { id: 'rear', label: 'Trasera', icon: ArrowDown, tip: 'Vista trasera completa' },
  {
    id: 'left',
    label: 'Lateral Izquierdo',
    icon: ArrowLeft,
    tip: 'Perfil completo del lado izquierdo',
  },
  {
    id: 'right',
    label: 'Lateral Derecho',
    icon: ArrowRight,
    tip: 'Perfil completo del lado derecho',
  },
  {
    id: 'front-left',
    label: '¾ Frontal Izq.',
    icon: RotateCcw,
    tip: 'Ángulo de 45° frontal izquierdo',
  },
  {
    id: 'front-right',
    label: '¾ Frontal Der.',
    icon: RotateCcw,
    tip: 'Ángulo de 45° frontal derecho',
  },
  { id: 'interior', label: 'Interior', icon: Circle, tip: 'Tablero, asientos y consola central' },
  { id: 'dashboard', label: 'Tablero', icon: Gauge, tip: 'Instrumentos y odómetro visible' },
];

interface PhotoGuideProps {
  uploadedAngles?: string[];
}

export function PhotoGuide({ uploadedAngles = [] }: PhotoGuideProps) {
  return (
    <div className="rounded-xl border border-gray-200 bg-gray-50 p-4">
      <div className="mb-3 flex items-center gap-2">
        <Camera className="h-4 w-4 text-gray-600" />
        <h4 className="text-sm font-semibold text-gray-700">Guía de Fotos Recomendadas</h4>
        <span className="ml-auto text-xs text-gray-500">
          {uploadedAngles.length}/{PHOTO_ANGLES.length} ángulos
        </span>
      </div>
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        {PHOTO_ANGLES.map(angle => {
          const done = uploadedAngles.includes(angle.id);
          const Icon = angle.icon;
          return (
            <div
              key={angle.id}
              className={`flex flex-col items-center gap-1 rounded-lg border p-2.5 text-center transition-colors ${
                done ? 'border-emerald-300 bg-emerald-50' : 'border-gray-200 bg-white'
              }`}
            >
              <div
                className={`flex h-8 w-8 items-center justify-center rounded-full ${
                  done ? 'bg-emerald-100' : 'bg-gray-100'
                }`}
              >
                <Icon className={`h-4 w-4 ${done ? 'text-emerald-600' : 'text-gray-400'}`} />
              </div>
              <p className={`text-xs font-medium ${done ? 'text-emerald-700' : 'text-gray-600'}`}>
                {angle.label}
              </p>
            </div>
          );
        })}
      </div>
      <p className="mt-3 text-center text-xs text-gray-500">
        Las fotos de buena calidad aumentan las visitas hasta un 80%
      </p>
    </div>
  );
}
