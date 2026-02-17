/**
 * DealerBenchmarksPage - Comparación con otros dealers del mercado
 * Ruta: /dealer/benchmarks
 */

import { useState, useEffect } from 'react';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiTrendingUp,
  FiTrendingDown,
  FiAward,
  FiTarget,
  FiClock,
  FiThumbsUp,
  FiStar,
  FiBarChart2,
  FiRefreshCw,
  FiInfo,
  FiArrowUp,
  FiArrowDown,
} from 'react-icons/fi';
import { useAuthStore } from '@/store/authStore';

// Types based on DealerAnalyticsService.Domain
interface DealerBenchmark {
  id: string;
  dealerId: string;
  period: string;

  // Dealer metrics
  avgDaysOnMarket: number;
  conversionRate: number;
  avgResponseTimeMinutes: number;
  customerSatisfaction: number;
  listingQualityScore: number;

  // Market averages
  marketAvgDaysOnMarket: number;
  marketAvgConversionRate: number;
  marketAvgResponseTime: number;
  marketAvgSatisfaction: number;

  // Percentiles
  conversionRatePercentile: number;
  responseTimePercentile: number;
  satisfactionPercentile: number;
  listingQualityPercentile: number;
  engagementPercentile: number;

  // Tier
  tier: 'Bronze' | 'Silver' | 'Gold' | 'Platinum' | 'Diamond';

  // Comparisons
  isBetterThanMarketDaysOnMarket: boolean;
  isBetterThanMarketConversion: boolean;
  isBetterThanMarketResponseTime: boolean;
  isBetterThanMarketSatisfaction: boolean;
}

interface MarketBenchmark {
  id: string;
  period: string;
  region: string;
  category: string;

  avgViewsPerListing: number;
  avgContactsPerListing: number;
  avgDaysToSell: number;
  avgConversionRate: number;
  avgPrice: number;
  avgResponseTimeMinutes: number;

  percentile25: number;
  percentile50: number;
  percentile75: number;
  percentile90: number;

  sampleSize: number;
}

// Mock data for demo - In production, fetch from API
const mockDealerBenchmark: DealerBenchmark = {
  id: '1',
  dealerId: 'dealer-1',
  period: '2026-01',
  avgDaysOnMarket: 18,
  conversionRate: 4.2,
  avgResponseTimeMinutes: 45,
  customerSatisfaction: 4.7,
  listingQualityScore: 92,
  marketAvgDaysOnMarket: 28,
  marketAvgConversionRate: 2.8,
  marketAvgResponseTime: 90,
  marketAvgSatisfaction: 4.2,
  conversionRatePercentile: 78,
  responseTimePercentile: 82,
  satisfactionPercentile: 85,
  listingQualityPercentile: 88,
  engagementPercentile: 75,
  tier: 'Platinum',
  isBetterThanMarketDaysOnMarket: true,
  isBetterThanMarketConversion: true,
  isBetterThanMarketResponseTime: true,
  isBetterThanMarketSatisfaction: true,
};

const mockMarketBenchmark: MarketBenchmark = {
  id: '1',
  period: '2026-01',
  region: 'Santo Domingo',
  category: 'SUVs',
  avgViewsPerListing: 245,
  avgContactsPerListing: 12,
  avgDaysToSell: 28,
  avgConversionRate: 2.8,
  avgPrice: 850000,
  avgResponseTimeMinutes: 90,
  percentile25: 18,
  percentile50: 28,
  percentile75: 42,
  percentile90: 65,
  sampleSize: 1250,
};

// Tier badge colors and icons
const tierConfig = {
  Bronze: { bg: 'bg-amber-100', text: 'text-amber-700', border: 'border-amber-300' },
  Silver: { bg: 'bg-gray-100', text: 'text-gray-700', border: 'border-gray-300' },
  Gold: { bg: 'bg-yellow-100', text: 'text-yellow-700', border: 'border-yellow-300' },
  Platinum: { bg: 'bg-blue-100', text: 'text-blue-700', border: 'border-blue-300' },
  Diamond: { bg: 'bg-purple-100', text: 'text-purple-700', border: 'border-purple-300' },
};

export default function DealerBenchmarksPage() {
  const user = useAuthStore((state) => state.user);
  const [isLoading, setIsLoading] = useState(false);
  const [selectedPeriod, setSelectedPeriod] = useState('2026-01');
  const [benchmark, setBenchmark] = useState<DealerBenchmark>(mockDealerBenchmark);
  const [marketData, setMarketData] = useState<MarketBenchmark>(mockMarketBenchmark);

  // In production, fetch from API
  const refreshData = async () => {
    setIsLoading(true);
    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 1000));
    setIsLoading(false);
  };

  useEffect(() => {
    refreshData();
  }, [selectedPeriod]);

  const calculateAveragePercentile = () => {
    const percentiles = [
      benchmark.conversionRatePercentile,
      benchmark.responseTimePercentile,
      benchmark.satisfactionPercentile,
      benchmark.listingQualityPercentile,
      benchmark.engagementPercentile,
    ];
    return Math.round(percentiles.reduce((a, b) => a + b, 0) / percentiles.length);
  };

  const MetricCard = ({
    title,
    yourValue,
    marketValue,
    unit,
    percentile,
    isBetter,
    lowerIsBetter = false,
    icon: Icon,
  }: {
    title: string;
    yourValue: number;
    marketValue: number;
    unit: string;
    percentile: number;
    isBetter: boolean;
    lowerIsBetter?: boolean;
    icon: React.ElementType;
  }) => {
    const difference = lowerIsBetter
      ? ((marketValue - yourValue) / marketValue) * 100
      : ((yourValue - marketValue) / marketValue) * 100;

    return (
      <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
        <div className="flex items-center gap-3 mb-4">
          <div className={`p-3 rounded-xl ${isBetter ? 'bg-green-100' : 'bg-red-100'}`}>
            <Icon className={`w-5 h-5 ${isBetter ? 'text-green-600' : 'text-red-600'}`} />
          </div>
          <div>
            <h3 className="text-sm font-medium text-gray-500">{title}</h3>
            <div className="flex items-center gap-2">
              <span className="text-2xl font-bold text-gray-900">{yourValue.toLocaleString()}</span>
              <span className="text-gray-500">{unit}</span>
            </div>
          </div>
        </div>

        <div className="space-y-3">
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-500">Promedio mercado</span>
            <span className="font-medium">
              {marketValue.toLocaleString()} {unit}
            </span>
          </div>

          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-500">Tu posición</span>
            <div className="flex items-center gap-2">
              <div className="w-20 bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full ${isBetter ? 'bg-green-500' : 'bg-red-500'}`}
                  style={{ width: `${percentile}%` }}
                />
              </div>
              <span className="font-medium text-gray-700">Top {100 - percentile}%</span>
            </div>
          </div>

          <div
            className={`flex items-center gap-1 text-sm font-medium ${isBetter ? 'text-green-600' : 'text-red-600'}`}
          >
            {isBetter ? <FiArrowUp className="w-4 h-4" /> : <FiArrowDown className="w-4 h-4" />}
            <span>
              {Math.abs(difference).toFixed(1)}% {isBetter ? 'mejor' : 'peor'} que el mercado
            </span>
          </div>
        </div>
      </div>
    );
  };

  if (isLoading) {
    return (
      <DealerPortalLayout>
        <div className="flex items-center justify-center h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </DealerPortalLayout>
    );
  }

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 space-y-6">
        {/* Header */}
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Benchmarks</h1>
            <p className="text-gray-500 mt-1">
              Compara tu rendimiento con otros dealers del mercado
            </p>
          </div>
          <div className="flex items-center gap-3">
            <select
              value={selectedPeriod}
              onChange={(e) => setSelectedPeriod(e.target.value)}
              className="px-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <option value="2026-01">Enero 2026</option>
              <option value="2025-12">Diciembre 2025</option>
              <option value="2025-11">Noviembre 2025</option>
            </select>
            <button
              onClick={refreshData}
              className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50"
            >
              <FiRefreshCw className="w-4 h-4" />
              <span>Actualizar</span>
            </button>
          </div>
        </div>

        {/* Tier Badge & Overall Score */}
        <div className="bg-gradient-to-br from-blue-600 to-indigo-700 rounded-2xl p-6 lg:p-8 text-white">
          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
            <div className="flex items-center gap-6">
              <div
                className={`w-20 h-20 rounded-2xl ${tierConfig[benchmark.tier].bg} ${tierConfig[benchmark.tier].border} border-2 flex items-center justify-center`}
              >
                <FiAward className={`w-10 h-10 ${tierConfig[benchmark.tier].text}`} />
              </div>
              <div>
                <div className="text-blue-200 text-sm mb-1">Tu nivel actual</div>
                <h2 className="text-3xl font-bold">{benchmark.tier}</h2>
                <div className="text-blue-200 mt-1">
                  Percentil promedio:{' '}
                  <span className="text-white font-semibold">{calculateAveragePercentile()}%</span>
                </div>
              </div>
            </div>

            <div className="grid grid-cols-2 lg:grid-cols-5 gap-4 lg:gap-6">
              {[
                { label: 'Conversión', value: benchmark.conversionRatePercentile },
                { label: 'Respuesta', value: benchmark.responseTimePercentile },
                { label: 'Satisfacción', value: benchmark.satisfactionPercentile },
                { label: 'Calidad', value: benchmark.listingQualityPercentile },
                { label: 'Engagement', value: benchmark.engagementPercentile },
              ].map((item, idx) => (
                <div key={idx} className="text-center">
                  <div className="text-3xl font-bold">{item.value}%</div>
                  <div className="text-blue-200 text-sm">{item.label}</div>
                </div>
              ))}
            </div>
          </div>

          {/* Tier Progress */}
          <div className="mt-6 pt-6 border-t border-blue-500/30">
            <div className="flex items-center justify-between text-sm text-blue-200 mb-2">
              <span>Progreso hacia Diamond</span>
              <span>{calculateAveragePercentile()}/90 puntos</span>
            </div>
            <div className="flex items-center gap-2">
              {['Bronze', 'Silver', 'Gold', 'Platinum', 'Diamond'].map((tier, idx) => (
                <div key={tier} className="flex-1">
                  <div
                    className={`h-2 rounded-full ${
                      tier === benchmark.tier
                        ? 'bg-white'
                        : idx <
                            ['Bronze', 'Silver', 'Gold', 'Platinum', 'Diamond'].indexOf(
                              benchmark.tier
                            )
                          ? 'bg-blue-400'
                          : 'bg-blue-900'
                    }`}
                  />
                  <div className="text-xs text-center mt-1 text-blue-200">{tier}</div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Metrics Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6">
          <MetricCard
            title="Tiempo promedio de venta"
            yourValue={benchmark.avgDaysOnMarket}
            marketValue={benchmark.marketAvgDaysOnMarket}
            unit="días"
            percentile={benchmark.responseTimePercentile}
            isBetter={benchmark.isBetterThanMarketDaysOnMarket}
            lowerIsBetter={true}
            icon={FiClock}
          />
          <MetricCard
            title="Tasa de conversión"
            yourValue={benchmark.conversionRate}
            marketValue={benchmark.marketAvgConversionRate}
            unit="%"
            percentile={benchmark.conversionRatePercentile}
            isBetter={benchmark.isBetterThanMarketConversion}
            icon={FiTarget}
          />
          <MetricCard
            title="Tiempo de respuesta"
            yourValue={benchmark.avgResponseTimeMinutes}
            marketValue={benchmark.marketAvgResponseTime}
            unit="min"
            percentile={benchmark.responseTimePercentile}
            isBetter={benchmark.isBetterThanMarketResponseTime}
            lowerIsBetter={true}
            icon={FiClock}
          />
          <MetricCard
            title="Satisfacción cliente"
            yourValue={benchmark.customerSatisfaction}
            marketValue={benchmark.marketAvgSatisfaction}
            unit="★"
            percentile={benchmark.satisfactionPercentile}
            isBetter={benchmark.isBetterThanMarketSatisfaction}
            icon={FiThumbsUp}
          />
        </div>

        {/* Market Data Section */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Market Overview */}
          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-6">
              <FiBarChart2 className="w-5 h-5 text-blue-600" />
              <h3 className="text-lg font-semibold text-gray-900">Datos del Mercado</h3>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="p-4 bg-gray-50 rounded-xl">
                <div className="text-sm text-gray-500">Vistas promedio/listing</div>
                <div className="text-xl font-bold text-gray-900">
                  {marketData.avgViewsPerListing}
                </div>
              </div>
              <div className="p-4 bg-gray-50 rounded-xl">
                <div className="text-sm text-gray-500">Contactos promedio</div>
                <div className="text-xl font-bold text-gray-900">
                  {marketData.avgContactsPerListing}
                </div>
              </div>
              <div className="p-4 bg-gray-50 rounded-xl">
                <div className="text-sm text-gray-500">Días promedio venta</div>
                <div className="text-xl font-bold text-gray-900">{marketData.avgDaysToSell}</div>
              </div>
              <div className="p-4 bg-gray-50 rounded-xl">
                <div className="text-sm text-gray-500">Precio promedio</div>
                <div className="text-xl font-bold text-gray-900">
                  RD${(marketData.avgPrice / 1000).toFixed(0)}K
                </div>
              </div>
            </div>

            <div className="mt-4 pt-4 border-t border-gray-100 flex items-center justify-between text-sm text-gray-500">
              <span>Región: {marketData.region}</span>
              <span>Muestra: {marketData.sampleSize.toLocaleString()} dealers</span>
            </div>
          </div>

          {/* Distribution Chart */}
          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-6">
              <FiTrendingUp className="w-5 h-5 text-blue-600" />
              <h3 className="text-lg font-semibold text-gray-900">Distribución del Mercado</h3>
            </div>

            <div className="space-y-4">
              {[
                {
                  label: 'Top 10% (Elite)',
                  percentile: marketData.percentile90,
                  color: 'bg-purple-500',
                },
                { label: 'Top 25%', percentile: marketData.percentile75, color: 'bg-blue-500' },
                {
                  label: 'Top 50% (Mediana)',
                  percentile: marketData.percentile50,
                  color: 'bg-green-500',
                },
                { label: 'Top 75%', percentile: marketData.percentile25, color: 'bg-yellow-500' },
              ].map((item, idx) => (
                <div key={idx}>
                  <div className="flex items-center justify-between text-sm mb-1">
                    <span className="text-gray-600">{item.label}</span>
                    <span className="font-medium">{item.percentile} días para vender</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-3">
                    <div
                      className={`h-3 rounded-full ${item.color}`}
                      style={{
                        width: `${100 - (item.percentile / marketData.percentile90) * 100 + 30}%`,
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>

            <div className="mt-6 p-4 bg-blue-50 rounded-xl flex items-start gap-3">
              <FiInfo className="w-5 h-5 text-blue-600 mt-0.5" />
              <div className="text-sm text-blue-800">
                <strong>Tu posición:</strong> Vendes en promedio en {benchmark.avgDaysOnMarket}{' '}
                días, lo que te coloca en el{' '}
                <strong>Top {100 - benchmark.responseTimePercentile}%</strong> del mercado.
              </div>
            </div>
          </div>
        </div>

        {/* Tips Section */}
        <div className="bg-gradient-to-r from-amber-50 to-orange-50 rounded-2xl p-6 border border-amber-100">
          <div className="flex items-start gap-4">
            <div className="p-3 bg-amber-100 rounded-xl">
              <FiStar className="w-6 h-6 text-amber-600" />
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                ¿Cómo mejorar tu posición?
              </h3>
              <ul className="space-y-2 text-gray-700">
                <li className="flex items-center gap-2">
                  <span className="w-2 h-2 bg-amber-400 rounded-full"></span>
                  Responde a los leads en menos de 30 minutos para mejorar tu tiempo de respuesta
                </li>
                <li className="flex items-center gap-2">
                  <span className="w-2 h-2 bg-amber-400 rounded-full"></span>
                  Mejora las fotos de tus vehículos para aumentar las vistas y contactos
                </li>
                <li className="flex items-center gap-2">
                  <span className="w-2 h-2 bg-amber-400 rounded-full"></span>
                  Mantén precios competitivos analizando las tendencias del mercado
                </li>
                <li className="flex items-center gap-2">
                  <span className="w-2 h-2 bg-amber-400 rounded-full"></span>
                  Solicita reseñas a clientes satisfechos para mejorar tu puntuación
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </DealerPortalLayout>
  );
}
