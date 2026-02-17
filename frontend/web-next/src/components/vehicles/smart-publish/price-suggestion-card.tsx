'use client';

import { DollarSign, TrendingUp, TrendingDown, Minus, Info, BarChart3 } from 'lucide-react';

interface PriceSuggestionCardProps {
  suggestedPrice: number;
  minPrice: number;
  maxPrice: number;
  averagePrice: number;
  currency: 'DOP' | 'USD';
  currentPrice: number;
  confidence?: 'high' | 'medium' | 'low';
  comparableCount?: number;
}

function formatPrice(amount: number, currency: 'DOP' | 'USD'): string {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(amount);
}

export function PriceSuggestionCard({
  suggestedPrice,
  minPrice,
  maxPrice,
  averagePrice,
  currency,
  currentPrice,
  confidence = 'medium',
  comparableCount = 0,
}: PriceSuggestionCardProps) {
  const diff = currentPrice ? currentPrice - suggestedPrice : 0;
  const diffPercent = suggestedPrice > 0 ? Math.round((diff / suggestedPrice) * 100) : 0;

  // Position of current price in range
  const range = maxPrice - minPrice;
  const pricePosition =
    range > 0 ? Math.max(0, Math.min(100, ((currentPrice - minPrice) / range) * 100)) : 50;
  const suggestedPosition =
    range > 0 ? Math.max(0, Math.min(100, ((suggestedPrice - minPrice) / range) * 100)) : 50;

  const priceLabel =
    currentPrice > suggestedPrice * 1.05
      ? { text: 'Por encima del mercado', icon: TrendingUp, color: 'text-amber-600' }
      : currentPrice < suggestedPrice * 0.95
        ? { text: 'Por debajo del mercado', icon: TrendingDown, color: 'text-blue-600' }
        : { text: 'Precio competitivo', icon: Minus, color: 'text-emerald-600' };

  const PriceIcon = priceLabel.icon;

  const confidenceConfig = {
    high: { label: 'Alta', color: 'text-emerald-600', bg: 'bg-emerald-100' },
    medium: { label: 'Media', color: 'text-amber-600', bg: 'bg-amber-100' },
    low: { label: 'Baja', color: 'text-red-600', bg: 'bg-red-100' },
  };

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      {/* Header */}
      <div className="mb-4 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-100">
            <BarChart3 className="h-4 w-4 text-emerald-600" />
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-900">Precio Sugerido</h3>
            <p className="text-xs text-gray-500">Basado en {comparableCount} vehículos similares</p>
          </div>
        </div>
        <span
          className={`rounded-full px-2 py-0.5 text-xs font-medium ${confidenceConfig[confidence].bg} ${confidenceConfig[confidence].color}`}
        >
          Confianza: {confidenceConfig[confidence].label}
        </span>
      </div>

      {/* Suggested Price */}
      <div className="py-3 text-center">
        <p className="text-3xl font-bold text-gray-900">{formatPrice(suggestedPrice, currency)}</p>
        {currentPrice > 0 && (
          <div
            className={`mt-1 flex items-center justify-center gap-1 text-sm ${priceLabel.color}`}
          >
            <PriceIcon className="h-4 w-4" />
            <span className="font-medium">{priceLabel.text}</span>
            {diffPercent !== 0 && (
              <span className="font-normal text-gray-400">
                ({diffPercent > 0 ? '+' : ''}
                {diffPercent}%)
              </span>
            )}
          </div>
        )}
      </div>

      {/* Price Range Bar */}
      <div className="mt-4">
        <div className="mb-1.5 flex items-center justify-between">
          <span className="text-xs text-gray-500">{formatPrice(minPrice, currency)}</span>
          <span className="text-xs text-gray-500">{formatPrice(maxPrice, currency)}</span>
        </div>
        <div className="relative h-3 rounded-full bg-gradient-to-r from-emerald-200 via-emerald-400 to-amber-300">
          {/* Suggested price marker */}
          <div
            className="absolute top-1/2 -translate-x-1/2 -translate-y-1/2"
            style={{ left: `${suggestedPosition}%` }}
          >
            <div className="h-5 w-1 rounded-full bg-gray-800" title="Sugerido" />
          </div>
          {/* Current price marker */}
          {currentPrice > 0 && (
            <div
              className="absolute top-1/2 -translate-x-1/2 -translate-y-1/2"
              style={{ left: `${pricePosition}%` }}
            >
              <div className="flex h-5 w-5 items-center justify-center rounded-full border-2 border-white bg-emerald-600 shadow-md">
                <DollarSign className="h-3 w-3 text-white" />
              </div>
            </div>
          )}
        </div>
        <div className="mt-2 flex items-center justify-center gap-4 text-xs text-gray-500">
          <span className="flex items-center gap-1">
            <span className="h-2 w-2 rounded-full bg-gray-800" /> Sugerido
          </span>
          {currentPrice > 0 && (
            <span className="flex items-center gap-1">
              <span className="h-2 w-2 rounded-full bg-emerald-600" /> Tu precio
            </span>
          )}
        </div>
      </div>

      {/* Stats */}
      <div className="mt-4 grid grid-cols-3 divide-x divide-gray-200 rounded-lg bg-gray-50 p-3">
        <div className="text-center">
          <p className="text-xs text-gray-500">Mínimo</p>
          <p className="text-sm font-semibold text-gray-900">{formatPrice(minPrice, currency)}</p>
        </div>
        <div className="text-center">
          <p className="text-xs text-gray-500">Promedio</p>
          <p className="text-sm font-semibold text-gray-900">
            {formatPrice(averagePrice, currency)}
          </p>
        </div>
        <div className="text-center">
          <p className="text-xs text-gray-500">Máximo</p>
          <p className="text-sm font-semibold text-gray-900">{formatPrice(maxPrice, currency)}</p>
        </div>
      </div>

      {/* Info note */}
      <div className="mt-3 flex items-start gap-2 text-xs text-gray-500">
        <Info className="mt-0.5 h-3.5 w-3.5 flex-shrink-0" />
        <p>Los precios se basan en listados similares recientes en el mercado dominicano.</p>
      </div>
    </div>
  );
}
