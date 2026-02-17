'use client';

import { useMemo } from 'react';
import type { VehicleFormData } from './smart-publish-wizard';
import {
  Shield,
  Camera,
  FileText,
  DollarSign,
  Barcode,
  MapPin,
  Star,
  CheckCircle,
  AlertCircle,
} from 'lucide-react';

// ============================================================
// Score Calculation
// ============================================================

interface ScoreItem {
  label: string;
  points: number;
  maxPoints: number;
  icon: typeof Shield;
  tip: string;
}

function calculateQualityScore(data: VehicleFormData): {
  total: number;
  max: number;
  items: ScoreItem[];
} {
  const items: ScoreItem[] = [];

  // Photos (30 pts max)
  const photoCount = data.images.length;
  const photoPoints = photoCount >= 8 ? 30 : photoCount >= 5 ? 20 : photoCount >= 3 ? 10 : 0;
  items.push({
    label: 'Fotos',
    points: photoPoints,
    maxPoints: 30,
    icon: Camera,
    tip:
      photoCount >= 8
        ? '8+ fotos: excelente'
        : `Agrega ${8 - photoCount} fotos más para máximo puntaje`,
  });

  // Description (20 pts max)
  const descLen = data.description.length;
  const descPoints =
    descLen >= 200 ? 20 : descLen >= 100 ? 15 : descLen >= 50 ? 10 : descLen > 0 ? 5 : 0;
  items.push({
    label: 'Descripción',
    points: descPoints,
    maxPoints: 20,
    icon: FileText,
    tip: descLen >= 200 ? 'Descripción detallada' : 'Agrega más detalles para mejorar',
  });

  // Price (15 pts max)
  const pricePoints = data.price > 0 ? 15 : 0;
  items.push({
    label: 'Precio',
    points: pricePoints,
    maxPoints: 15,
    icon: DollarSign,
    tip: data.price > 0 ? 'Precio establecido' : 'Define un precio',
  });

  // VIN (10 pts max)
  const vinPoints = data.vin.length === 17 ? 10 : 0;
  items.push({
    label: 'VIN',
    points: vinPoints,
    maxPoints: 10,
    icon: Barcode,
    tip: data.vin.length === 17 ? 'VIN verificado' : 'Agregar VIN aumenta la confianza',
  });

  // Features (10 pts max)
  const featCount = data.features.length;
  const featPoints = featCount >= 5 ? 10 : featCount >= 3 ? 7 : featCount > 0 ? 4 : 0;
  items.push({
    label: 'Características',
    points: featPoints,
    maxPoints: 10,
    icon: Star,
    tip: featCount >= 5 ? '5+ características' : 'Agrega más características',
  });

  // Location (10 pts max)
  const locPoints = data.province && data.city ? 10 : data.province ? 5 : 0;
  items.push({
    label: 'Ubicación',
    points: locPoints,
    maxPoints: 10,
    icon: MapPin,
    tip: data.province && data.city ? 'Ubicación completa' : 'Agrega ciudad y provincia',
  });

  // Contact (5 pts max)
  const contactPoints = data.sellerPhone ? 5 : 0;
  items.push({
    label: 'Contacto',
    points: contactPoints,
    maxPoints: 5,
    icon: Shield,
    tip: data.sellerPhone ? 'Teléfono de contacto' : 'Agrega teléfono para contacto directo',
  });

  const total = items.reduce((sum, item) => sum + item.points, 0);
  const max = items.reduce((sum, item) => sum + item.maxPoints, 0);

  return { total, max, items };
}

// ============================================================
// Component
// ============================================================

interface ListingQualityScoreProps {
  data: VehicleFormData;
}

export function ListingQualityScore({ data }: ListingQualityScoreProps) {
  const { total, max, items } = useMemo(() => calculateQualityScore(data), [data]);
  const percent = Math.round((total / max) * 100);

  const grade =
    percent >= 90
      ? { label: 'Excelente', color: 'text-emerald-600', bg: 'bg-emerald-500' }
      : percent >= 70
        ? { label: 'Bueno', color: 'text-blue-600', bg: 'bg-blue-500' }
        : percent >= 50
          ? { label: 'Regular', color: 'text-amber-600', bg: 'bg-amber-500' }
          : { label: 'Mejorable', color: 'text-red-600', bg: 'bg-red-500' };

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5">
      {/* Score circle */}
      <div className="mb-4 flex items-center gap-4">
        <div className="relative h-16 w-16 flex-shrink-0">
          <svg className="h-16 w-16 -rotate-90" viewBox="0 0 36 36">
            <path
              d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
              fill="none"
              stroke="#e5e7eb"
              strokeWidth="3"
            />
            <path
              d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
              fill="none"
              stroke="currentColor"
              strokeWidth="3"
              strokeDasharray={`${percent}, 100`}
              className={grade.color}
            />
          </svg>
          <span className="absolute inset-0 flex items-center justify-center text-sm font-bold text-gray-900">
            {percent}%
          </span>
        </div>
        <div>
          <p className={`text-lg font-bold ${grade.color}`}>{grade.label}</p>
          <p className="text-xs text-gray-500">
            {total}/{max} puntos
          </p>
        </div>
      </div>

      {/* Score items */}
      <div className="space-y-2">
        {items.map(item => {
          const Icon = item.icon;
          const isFull = item.points === item.maxPoints;
          return (
            <div key={item.label} className="flex items-center gap-3">
              <div
                className={`flex h-7 w-7 items-center justify-center rounded-full ${
                  isFull ? 'bg-emerald-100' : 'bg-gray-100'
                }`}
              >
                <Icon className={`h-3.5 w-3.5 ${isFull ? 'text-emerald-600' : 'text-gray-400'}`} />
              </div>
              <div className="min-w-0 flex-1">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-medium text-gray-700">{item.label}</span>
                  <span className="text-xs text-gray-500">
                    {item.points}/{item.maxPoints}
                  </span>
                </div>
                <div className="mt-0.5 h-1.5 overflow-hidden rounded-full bg-gray-100">
                  <div
                    className={`h-full rounded-full transition-all ${isFull ? 'bg-emerald-500' : 'bg-amber-400'}`}
                    style={{ width: `${(item.points / item.maxPoints) * 100}%` }}
                  />
                </div>
              </div>
              {isFull ? (
                <CheckCircle className="h-4 w-4 flex-shrink-0 text-emerald-500" />
              ) : (
                <AlertCircle className="h-4 w-4 flex-shrink-0 text-gray-300" />
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
