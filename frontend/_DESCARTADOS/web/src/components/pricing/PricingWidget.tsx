import { useState, useEffect, useCallback } from 'react';
import vehicleIntelligenceService from '@/services/vehicleIntelligenceService';
import type { PriceAnalysisDto } from '@/services/vehicleIntelligenceService';
import { FiAlertCircle, FiTrendingUp, FiTrendingDown, FiCheck } from 'react-icons/fi';

interface PricingWidgetProps {
  vehicleId?: string;
  make: string;
  model: string;
  year: number;
  mileage: number;
  condition: string;
  fuelType: string;
  transmission: string;
  currentPrice: number;
  photoCount?: number;
  viewCount?: number;
  daysListed?: number;
  onPriceChange?: (suggestedPrice: number) => void;
}

export function PricingWidget({
  vehicleId,
  make,
  model,
  year,
  mileage,
  condition,
  fuelType,
  transmission,
  currentPrice,
  photoCount = 0,
  viewCount = 0,
  daysListed = 0,
  onPriceChange,
}: PricingWidgetProps) {
  const [analysis, setAnalysis] = useState<PriceAnalysisDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const analyzePrice = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const result = await vehicleIntelligenceService.analyzePricing({
        vehicleId: vehicleId || crypto.randomUUID(),
        make,
        model,
        year,
        mileage,
        condition,
        fuelType,
        transmission,
        currentPrice,
        photoCount,
        viewCount,
        daysListed,
      });

      setAnalysis(result);
    } catch (err: any) {
      setError(err.message || 'Error al analizar el precio');
    } finally {
      setLoading(false);
    }
  }, [
    vehicleId,
    make,
    model,
    year,
    mileage,
    condition,
    fuelType,
    transmission,
    currentPrice,
    photoCount,
    viewCount,
    daysListed,
  ]);

  useEffect(() => {
    if (currentPrice > 0) {
      analyzePrice();
    }
  }, [currentPrice, mileage, analyzePrice]);

  if (loading) {
    return (
      <div className="bg-white border border-gray-200 rounded-lg p-6">
        <div className="animate-pulse">
          <div className="h-4 bg-gray-200 rounded w-3/4 mb-4"></div>
          <div className="h-8 bg-gray-200 rounded w-1/2 mb-2"></div>
          <div className="h-3 bg-gray-200 rounded w-full"></div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
        <FiAlertCircle className="text-red-600 mt-0.5 flex-shrink-0" size={20} />
        <div>
          <p className="text-sm font-medium text-red-800">Error al analizar precio</p>
          <p className="text-sm text-red-600 mt-1">{error}</p>
        </div>
      </div>
    );
  }

  if (!analysis) {
    return null;
  }

  const savings = vehicleIntelligenceService.calculateSavings(
    analysis.currentPrice,
    analysis.suggestedPrice
  );

  const positionColor = vehicleIntelligenceService.getPricePositionColor(analysis.pricePosition);

  return (
    <div className="bg-gradient-to-br from-blue-50 to-indigo-50 border border-blue-200 rounded-lg p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
          🤖 Análisis de Precio IA
        </h3>
        <span className="text-xs text-gray-500">
          Confianza: {analysis.confidenceScore.toFixed(0)}%
        </span>
      </div>

      {/* Price Position */}
      <div
        className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-medium ${positionColor}`}
      >
        {analysis.pricePosition === 'Above Market' && <FiTrendingUp />}
        {analysis.pricePosition === 'Below Market' && <FiTrendingDown />}
        {analysis.pricePosition === 'Fair' && <FiCheck />}
        {analysis.pricePosition}
      </div>

      {/* Suggested Price */}
      <div className="bg-white rounded-lg p-4 border border-blue-300">
        <p className="text-sm text-gray-600 mb-1">Precio Sugerido</p>
        <p className="text-3xl font-bold text-blue-600">
          {vehicleIntelligenceService.formatPrice(analysis.suggestedPrice)}
        </p>
        <p className="text-xs text-gray-500 mt-1">
          Rango: {vehicleIntelligenceService.formatPrice(analysis.suggestedPriceMin)} -{' '}
          {vehicleIntelligenceService.formatPrice(analysis.suggestedPriceMax)}
        </p>
      </div>

      {/* Comparison */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <p className="text-xs text-gray-600">Tu Precio</p>
          <p className="text-lg font-semibold text-gray-900">
            {vehicleIntelligenceService.formatPrice(analysis.currentPrice)}
          </p>
        </div>
        <div>
          <p className="text-xs text-gray-600">Promedio Mercado</p>
          <p className="text-lg font-semibold text-gray-900">
            {vehicleIntelligenceService.formatPrice(analysis.marketAvgPrice)}
          </p>
        </div>
      </div>

      {/* Prediction */}
      <div className="bg-white rounded-lg p-4 border border-gray-200">
        <p className="text-sm font-medium text-gray-900 mb-2">⏱️ Tiempo Estimado de Venta</p>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-xs text-gray-600">Precio Actual</p>
            <p className="text-2xl font-bold text-gray-900">
              {analysis.predictedDaysToSaleAtCurrentPrice}
              <span className="text-sm font-normal text-gray-600"> días</span>
            </p>
          </div>
          <div className="text-4xl">→</div>
          <div>
            <p className="text-xs text-gray-600">Precio Sugerido</p>
            <p className="text-2xl font-bold text-green-600">
              {analysis.predictedDaysToSaleAtSuggestedPrice}
              <span className="text-sm font-normal text-gray-600"> días</span>
            </p>
          </div>
        </div>
      </div>

      {/* Recommendations */}
      {analysis.recommendations && analysis.recommendations.length > 0 && (
        <div className="space-y-2">
          <p className="text-sm font-medium text-gray-900">💡 Recomendaciones</p>
          {analysis.recommendations.slice(0, 3).map((rec) => (
            <div key={rec.id} className="bg-white rounded-lg p-3 border border-gray-200 text-sm">
              <p className="font-medium text-gray-900">{rec.reason}</p>
              {rec.impactDescription && (
                <p className="text-xs text-gray-600 mt-1">{rec.impactDescription}</p>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Action Button */}
      {onPriceChange && savings > 0 && (
        <button
          onClick={() => onPriceChange(analysis.suggestedPrice)}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2.5 px-4 rounded-lg transition-colors"
        >
          Usar Precio Sugerido ({vehicleIntelligenceService.formatPrice(analysis.suggestedPrice)})
        </button>
      )}

      {/* Summary */}
      <p className="text-xs text-gray-600 leading-relaxed">
        {vehicleIntelligenceService.generatePriceAnalysisSummary(analysis)}
      </p>
    </div>
  );
}
