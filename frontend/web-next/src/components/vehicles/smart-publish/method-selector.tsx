'use client';

import { Camera, Keyboard, PenLine, FileSpreadsheet, HelpCircle } from 'lucide-react';
import type { WizardMode } from './smart-publish-wizard';

export type PublishMethod = 'vin-scan' | 'vin-keyboard' | 'manual' | 'csv';

interface MethodSelectorProps {
  mode: WizardMode;
  onSelect: (method: PublishMethod) => void;
}

const methods: {
  id: PublishMethod;
  icon: typeof Camera;
  title: string;
  subtitle: string;
  time: string;
  dealerOnly?: boolean;
}[] = [
  {
    id: 'vin-scan',
    icon: Camera,
    title: 'Escanear VIN',
    subtitle: 'Usa la cámara para leer el VIN',
    time: '~2 min',
  },
  {
    id: 'vin-keyboard',
    icon: Keyboard,
    title: 'Escribir VIN',
    subtitle: 'Ingresa el VIN manualmente',
    time: '~3 min',
  },
  {
    id: 'manual',
    icon: PenLine,
    title: 'Llenar manualmente',
    subtitle: 'Sin VIN disponible',
    time: '~5-8 min',
  },
  {
    id: 'csv',
    icon: FileSpreadsheet,
    title: 'Importar desde CSV/Excel',
    subtitle: 'Publica hasta 50 vehículos a la vez',
    time: 'Masivo',
    dealerOnly: true,
  },
];

export function MethodSelector({ mode, onSelect }: MethodSelectorProps) {
  const availableMethods = methods.filter(m => !m.dealerOnly || mode === 'dealer');

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="text-center">
        <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl">
          ¿Cómo quieres publicar tu vehículo?
        </h1>
        <p className="mt-2 text-gray-500">Elige el método más conveniente para ti</p>
      </div>

      {/* Method cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {availableMethods.map(method => {
          const Icon = method.icon;
          return (
            <button
              key={method.id}
              onClick={() => onSelect(method.id)}
              className="group flex flex-col items-center rounded-xl border-2 border-gray-200 p-6 text-center transition-all hover:border-emerald-500 hover:shadow-lg focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 focus:outline-none"
            >
              <div className="flex h-14 w-14 items-center justify-center rounded-full bg-emerald-50 text-emerald-600 transition-colors group-hover:bg-emerald-100">
                <Icon className="h-7 w-7" />
              </div>
              <h3 className="mt-4 text-lg font-semibold text-gray-900">{method.title}</h3>
              <p className="mt-1 text-sm text-gray-500">{method.subtitle}</p>
              <span className="mt-3 inline-flex items-center rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-600">
                {method.time}
              </span>
              {method.dealerOnly && (
                <span className="mt-2 inline-flex items-center rounded-full bg-blue-50 px-3 py-1 text-xs font-medium text-blue-600">
                  Solo Dealers
                </span>
              )}
            </button>
          );
        })}
      </div>

      {/* VIN help */}
      <div className="rounded-lg border border-blue-100 bg-blue-50 p-4">
        <div className="flex items-start gap-3">
          <HelpCircle className="mt-0.5 h-5 w-5 flex-shrink-0 text-blue-600" />
          <div>
            <h4 className="text-sm font-medium text-blue-900">¿Dónde encuentro el VIN?</h4>
            <p className="mt-1 text-sm text-blue-700">
              El VIN (Número de Identificación del Vehículo) tiene 17 caracteres y se encuentra en:
            </p>
            <ul className="mt-2 space-y-1 text-sm text-blue-700">
              <li>• Esquina inferior del parabrisas (lado del conductor)</li>
              <li>• Placa de identificación en la puerta del conductor</li>
              <li>• Documentos del vehículo (matrícula, seguro)</li>
              <li>• Compartimiento del motor</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
