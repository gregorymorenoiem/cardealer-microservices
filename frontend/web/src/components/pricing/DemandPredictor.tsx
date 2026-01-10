import { useState, useEffect, useCallback } from 'react';
import vehicleIntelligenceService from '@/services/vehicleIntelligenceService';
import type { DemandPredictionDto } from '@/services/vehicleIntelligenceService';
import { FiInfo } from 'react-icons/fi';

interface DemandPredictorProps {
  make: string;
  model: string;
  year: number;
  fuelType?: string;
  transmission?: string;
}

export function DemandPredictor({
  make,
  model,
  year,
  fuelType,
  transmission,
}: DemandPredictorProps) {
  const [prediction, setPrediction] = useState<DemandPredictionDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const predictDemand = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const result = await vehicleIntelligenceService.predictDemand({
        make,
        model,
        year,
        fuelType,
        transmission,
      });

      setPrediction(result);
    } catch (err: any) {
      setError(err.message || 'Error al predecir demanda');
    } finally {
      setLoading(false);
    }
  }, [make, model, year, fuelType, transmission]);

  useEffect(() => {
    if (make && model && year) {
      predictDemand();
    }
  }, [make, model, year, predictDemand]);

  if (loading) {
    return (
      <div className="bg-white border border-gray-200 rounded-lg p-6">
        <div className="animate-pulse space-y-4">
          <div className="h-4 bg-gray-200 rounded w-1/2"></div>
          <div className="h-20 bg-gray-200 rounded"></div>
          <div className="h-16 bg-gray-200 rounded"></div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <p className="text-sm text-yellow-800">{error}</p>
      </div>
    );
  }

  if (!prediction) {
    return null;
  }

  const demandColor = vehicleIntelligenceService.getDemandLevelColor(prediction.currentDemand);
  const demandText = vehicleIntelligenceService.getDemandLevelText(prediction.currentDemand);
  const trendIcon = vehicleIntelligenceService.getTrendIcon(prediction.trend);
  const trendText = vehicleIntelligenceService.getTrendText(prediction.trend);
  const buyBadge = vehicleIntelligenceService.getBuyRecommendationBadge(
    prediction.buyRecommendation
  );

  return (
    <div className="bg-gradient-to-br from-purple-50 to-pink-50 border border-purple-200 rounded-lg p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">📊 Predicción de Demanda</h3>
        <span
          className={`inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-medium ${buyBadge.color}`}
        >
          {buyBadge.icon} {buyBadge.text}
        </span>
      </div>

      {/* Demand Level */}
      <div
        className={`inline-flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold ${demandColor}`}
      >
        {demandText}
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg p-4 border border-gray-200">
          <p className="text-xs text-gray-600 mb-1">Score</p>
          <p className="text-2xl font-bold text-purple-600">
            {prediction.demandScore.toFixed(0)}/100
          </p>
        </div>

        <div className="bg-white rounded-lg p-4 border border-gray-200">
          <p className="text-xs text-gray-600 mb-1">Tendencia</p>
          <p className="text-lg font-semibold text-gray-900">
            {trendIcon} {trendText}
          </p>
        </div>

        <div className="bg-white rounded-lg p-4 border border-gray-200">
          <p className="text-xs text-gray-600 mb-1">Búsquedas/Día</p>
          <p className="text-2xl font-bold text-blue-600">{prediction.searchesPerDay}</p>
        </div>

        <div className="bg-white rounded-lg p-4 border border-gray-200">
          <p className="text-xs text-gray-600 mb-1">Inventario</p>
          <p className="text-2xl font-bold text-orange-600">{prediction.availableInventory}</p>
        </div>
      </div>

      {/* Days to Sale */}
      <div className="bg-white rounded-lg p-4 border border-gray-200">
        <p className="text-sm font-medium text-gray-900 mb-2">⏱️ Tiempo Promedio de Venta</p>
        <p className="text-3xl font-bold text-green-600">
          {prediction.avgDaysToSale.toFixed(0)}
          <span className="text-lg font-normal text-gray-600"> días</span>
        </p>
      </div>

      {/* Future Predictions */}
      <div className="bg-white rounded-lg p-4 border border-gray-200 space-y-3">
        <p className="text-sm font-medium text-gray-900">🔮 Predicciones Futuras</p>

        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-600">En 30 días</span>
          <span
            className={`px-2 py-1 rounded text-xs font-medium ${vehicleIntelligenceService.getDemandLevelColor(prediction.predictedDemand30Days)}`}
          >
            {vehicleIntelligenceService.getDemandLevelText(prediction.predictedDemand30Days)}
          </span>
        </div>

        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-600">En 90 días</span>
          <span
            className={`px-2 py-1 rounded text-xs font-medium ${vehicleIntelligenceService.getDemandLevelColor(prediction.predictedDemand90Days)}`}
          >
            {vehicleIntelligenceService.getDemandLevelText(prediction.predictedDemand90Days)}
          </span>
        </div>
      </div>

      {/* Insights */}
      {prediction.insights && prediction.insights.length > 0 && (
        <div className="bg-white rounded-lg p-4 border border-gray-200 space-y-2">
          <p className="text-sm font-medium text-gray-900 flex items-center gap-2">
            <FiInfo className="text-blue-500" />
            Insights
          </p>
          <ul className="space-y-1">
            {prediction.insights.map((insight, index) => (
              <li key={index} className="text-sm text-gray-700 flex items-start gap-2">
                <span className="text-blue-500 mt-0.5">•</span>
                {insight}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Buy Recommendation */}
      <div className="bg-white rounded-lg p-4 border border-gray-200">
        <p className="text-sm font-medium text-gray-900 mb-2">💼 Recomendación para Dealers</p>
        <p className="text-sm text-gray-700 leading-relaxed">
          {prediction.buyRecommendationReason}
        </p>
      </div>

      {/* Summary */}
      <div className="bg-purple-100 border border-purple-300 rounded-lg p-3">
        <p className="text-xs text-purple-900 leading-relaxed">
          {vehicleIntelligenceService.generateDemandSummary(prediction)}
        </p>
      </div>
    </div>
  );
}
