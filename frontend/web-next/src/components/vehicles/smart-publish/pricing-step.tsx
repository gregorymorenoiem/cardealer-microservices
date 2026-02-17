'use client';

import { useState, useMemo, useCallback } from 'react';
import { useEstimatePrice } from '@/hooks/use-vehicles';
import { generateVehicleDescription } from '@/services/vehicles';
import { PriceSuggestionCard } from './price-suggestion-card';
import type { VehicleFormData } from './smart-publish-wizard';
import { sanitizePrice, sanitizeText } from '@/lib/security/sanitize';
import { DollarSign, Wand2, ToggleLeft, ToggleRight, ArrowLeftRight, FileText } from 'lucide-react';

// ============================================================
// Component
// ============================================================

interface PricingStepProps {
  data: VehicleFormData;
  onChange: (updates: Partial<VehicleFormData>) => void;
}

export function PricingStep({ data, onChange }: PricingStepProps) {
  const [showSuggestion, setShowSuggestion] = useState(true);

  // Build specs for price estimation
  const priceSpecs = useMemo(() => {
    if (!data.make || !data.model || !data.year) return null;
    return {
      make: data.make,
      model: data.model,
      year: data.year,
      mileage: data.mileage || 0,
      condition: data.condition || undefined,
      transmission: data.transmission || undefined,
      fuelType: data.fuelType || undefined,
      currentPrice: data.price || undefined,
    };
  }, [
    data.make,
    data.model,
    data.year,
    data.mileage,
    data.condition,
    data.transmission,
    data.fuelType,
    data.price,
  ]);

  const { data: priceSuggestion, isLoading: isPriceLoading } = useEstimatePrice(
    priceSpecs ?? { make: '', model: '', year: 0, mileage: 0 },
    { enabled: !!priceSpecs }
  );

  // Generate description
  const handleGenerateDescription = useCallback(() => {
    const desc = generateVehicleDescription({
      make: data.make,
      model: data.model,
      year: data.year,
      trim: data.trim,
      mileage: data.mileage,
      transmission: data.transmission,
      fuelType: data.fuelType,
      condition: data.condition,
      engineSize: data.engineSize,
      driveType: data.driveType,
    });
    onChange({ description: desc });
  }, [data, onChange]);

  const handleApplySuggestedPrice = useCallback(() => {
    if (priceSuggestion?.suggestedPrice) {
      onChange({ price: priceSuggestion.suggestedPrice });
    }
  }, [priceSuggestion, onChange]);

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: data.currency,
      maximumFractionDigits: 0,
    }).format(val);

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-xl font-bold text-gray-900">Precio y Descripción</h2>
        <p className="mt-1 text-sm text-gray-500">
          Establece un precio competitivo y una descripción atractiva
        </p>
      </div>

      {/* ── Price Input ── */}
      <section className="space-y-4">
        <h3 className="text-sm font-semibold tracking-wider text-gray-500 uppercase">Precio</h3>

        <div className="flex items-end gap-3">
          {/* Price Input */}
          <div className="flex-1">
            <label className="mb-1.5 block text-sm font-medium text-gray-700">
              Precio de Venta <span className="text-red-500">*</span>
            </label>
            <div className="relative">
              <DollarSign className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
              <input
                type="number"
                value={data.price || ''}
                onChange={e => onChange({ price: sanitizePrice(parseFloat(e.target.value) || 0) })}
                placeholder="0"
                min={0}
                className="w-full rounded-lg border border-gray-300 py-2.5 pr-3 pl-9 text-lg font-semibold text-gray-900 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
              />
            </div>
          </div>

          {/* Currency Toggle */}
          <div className="w-28">
            <label className="mb-1.5 block text-sm font-medium text-gray-700">Moneda</label>
            <select
              value={data.currency}
              onChange={e => onChange({ currency: e.target.value as 'DOP' | 'USD' })}
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:ring-2 focus:ring-emerald-500 focus:outline-none"
            >
              <option value="DOP">RD$ DOP</option>
              <option value="USD">US$ USD</option>
            </select>
          </div>
        </div>

        {/* Price display */}
        {data.price > 0 && (
          <p className="text-center text-lg text-gray-600">{formatCurrency(data.price)}</p>
        )}

        {/* Options */}
        <div className="flex flex-wrap gap-4">
          <button
            type="button"
            onClick={() => onChange({ isNegotiable: !data.isNegotiable })}
            className="flex items-center gap-2 text-sm text-gray-700"
          >
            {data.isNegotiable ? (
              <ToggleRight className="h-6 w-6 text-emerald-500" />
            ) : (
              <ToggleLeft className="h-6 w-6 text-gray-400" />
            )}
            Precio negociable
          </button>
          <button
            type="button"
            onClick={() => onChange({ acceptsTrades: !data.acceptsTrades })}
            className="flex items-center gap-2 text-sm text-gray-700"
          >
            {data.acceptsTrades ? (
              <ToggleRight className="h-6 w-6 text-emerald-500" />
            ) : (
              <ToggleLeft className="h-6 w-6 text-gray-400" />
            )}
            <ArrowLeftRight className="h-4 w-4" />
            Acepta intercambios
          </button>
        </div>
      </section>

      {/* ── Price Suggestion ── */}
      {priceSpecs && (
        <section className="space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold tracking-wider text-gray-500 uppercase">
              Precio de Mercado
            </h3>
            <button
              type="button"
              onClick={() => setShowSuggestion(!showSuggestion)}
              className="text-xs font-medium text-emerald-600 hover:text-emerald-700"
            >
              {showSuggestion ? 'Ocultar' : 'Mostrar'}
            </button>
          </div>

          {showSuggestion && (
            <>
              {isPriceLoading ? (
                <div className="rounded-xl border border-gray-200 bg-gray-50 p-8 text-center">
                  <div className="animate-pulse space-y-3">
                    <div className="mx-auto h-8 w-48 rounded bg-gray-200" />
                    <div className="mx-auto h-4 w-32 rounded bg-gray-200" />
                    <div className="mx-auto h-3 w-full rounded bg-gray-200" />
                  </div>
                  <p className="mt-4 text-sm text-gray-500">Analizando precios del mercado...</p>
                </div>
              ) : priceSuggestion ? (
                <>
                  <PriceSuggestionCard
                    suggestedPrice={priceSuggestion.suggestedPrice}
                    minPrice={priceSuggestion.suggestedPriceMin}
                    maxPrice={priceSuggestion.suggestedPriceMax}
                    averagePrice={priceSuggestion.marketAvgPrice}
                    currency={data.currency}
                    currentPrice={data.price}
                    confidence={
                      priceSuggestion.confidence >= 0.7
                        ? 'high'
                        : priceSuggestion.confidence >= 0.4
                          ? 'medium'
                          : 'low'
                    }
                  />
                  {data.price === 0 && (
                    <button
                      type="button"
                      onClick={handleApplySuggestedPrice}
                      className="w-full rounded-lg border border-emerald-300 bg-emerald-50 px-4 py-2.5 text-sm font-medium text-emerald-700 transition-colors hover:bg-emerald-100"
                    >
                      Usar precio sugerido: {formatCurrency(priceSuggestion.suggestedPrice)}
                    </button>
                  )}
                </>
              ) : (
                <div className="rounded-xl border border-gray-200 bg-gray-50 p-6 text-center text-sm text-gray-500">
                  No hay suficientes datos para sugerir un precio
                </div>
              )}
            </>
          )}
        </section>
      )}

      {/* ── Description ── */}
      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold tracking-wider text-gray-500 uppercase">
            Descripción
          </h3>
          <button
            type="button"
            onClick={handleGenerateDescription}
            className="flex items-center gap-1.5 rounded-lg border border-emerald-300 bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-700 transition-colors hover:bg-emerald-100"
          >
            <Wand2 className="h-3.5 w-3.5" />
            Auto-generar
          </button>
        </div>

        <div className="relative">
          <FileText className="absolute top-3 left-3 h-4 w-4 text-gray-400" />
          <textarea
            value={data.description}
            onChange={e =>
              onChange({ description: sanitizeText(e.target.value, { maxLength: 2000 }) })
            }
            placeholder="Describe tu vehículo: estado, historial de mantenimiento, características especiales, razón de venta..."
            rows={6}
            maxLength={2000}
            className="w-full resize-none rounded-lg border border-gray-300 py-2.5 pr-3 pl-9 text-sm text-gray-700 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
          />
          <p className="mt-1 text-right text-xs text-gray-400">
            {data.description.length}/2000 caracteres
          </p>
        </div>
      </section>
    </div>
  );
}
