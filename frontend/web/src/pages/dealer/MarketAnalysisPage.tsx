/**
 * MarketAnalysisPage - Análisis de Mercado para Dealers
 * Dashboard de inteligencia de precios y demanda por categoría
 */

import { useState, useEffect } from 'react';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiTrendingUp,
  FiTrendingDown,
  FiBarChart2,
  FiTarget,
  FiShoppingCart,
  FiDollarSign,
  FiCalendar,
  FiFilter,
  FiRefreshCw,
  FiDownload,
  FiSearch,
} from 'react-icons/fi';
import { useQuery } from '@tanstack/react-query';
import vehicleIntelligenceService, {
  CategoryDemandDto,
} from '@/services/vehicleIntelligenceService';
import { useAuthStore } from '@/store/authStore';

interface MarketAnalysisState {
  selectedCategory?: string;
  timeRange: 'week' | 'month' | 'quarter' | 'year';
  searchTerm?: string;
}

export default function MarketAnalysisPage() {
  const user = useAuthStore((state) => state.user);
  const [state, setState] = useState<MarketAnalysisState>({
    timeRange: 'month',
  });

  // Fetch demand by category
  const {
    data: demandByCategory = [],
    isLoading: demandLoading,
    error: demandError,
    refetch: refetchDemand,
  } = useQuery({
    queryKey: ['demand-categories'],
    queryFn: () => vehicleIntelligenceService.getDemandByCategory(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  // Filter demand data based on search
  const filteredDemand = demandByCategory.filter(
    (item) =>
      !state.searchTerm || item.category.toLowerCase().includes(state.searchTerm.toLowerCase())
  );

  const getDemandLevelColor = (level: string): string => {
    const levelStr = String(level).toLowerCase();
    if (levelStr.includes('veryhigh')) return 'text-green-700 bg-green-100 border-green-300';
    if (levelStr.includes('high')) return 'text-green-600 bg-green-50 border-green-200';
    if (levelStr.includes('medium')) return 'text-yellow-600 bg-yellow-50 border-yellow-200';
    if (levelStr.includes('low')) return 'text-orange-600 bg-orange-50 border-orange-200';
    if (levelStr.includes('verylow')) return 'text-red-700 bg-red-100 border-red-300';
    return 'text-gray-600 bg-gray-50 border-gray-200';
  };

  const getDemandLevelText = (level: string): string => {
    const levelStr = String(level);
    if (levelStr.includes('VeryHigh')) return 'Muy Alta';
    if (levelStr.includes('High')) return 'Alta';
    if (levelStr.includes('Medium')) return 'Media';
    if (levelStr.includes('Low')) return 'Baja';
    if (levelStr.includes('VeryLow')) return 'Muy Baja';
    return level;
  };

  const getTrendIcon = (trend: any) => {
    const trendStr = String(trend).toLowerCase();
    if (trendStr.includes('rising')) return <FiTrendingUp className="text-green-600" />;
    if (trendStr.includes('falling')) return <FiTrendingDown className="text-red-600" />;
    return <FiTarget className="text-gray-400" />;
  };

  const handleExportData = async () => {
    try {
      const csv = generateCSV(demandByCategory);
      downloadCSV(csv, 'market-analysis.csv');
    } catch (error) {
      console.error('Error exporting data:', error);
    }
  };

  const generateCSV = (data: CategoryDemandDto[]): string => {
    const headers = [
      'Categoría',
      'Demanda',
      'Score',
      'Promedio Días Venta',
      'Búsquedas',
      'Listados Activos',
    ];
    const rows = data.map((item) => [
      item.category,
      getDemandLevelText(item.demandLevel),
      item.demandScore.toFixed(2),
      item.avgDaysToSale.toString(),
      item.totalSearches.toString(),
      item.activeListings.toString(),
    ]);

    return [headers, ...rows].map((row) => row.join(',')).join('\n');
  };

  const downloadCSV = (csv: string, filename: string) => {
    const element = document.createElement('a');
    element.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv));
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  };

  return (
    <DealerPortalLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Análisis de Mercado</h1>
            <p className="text-gray-600 mt-2">
              Inteligencia de demanda y precios por categoría de vehículos
            </p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => refetchDemand()}
              disabled={demandLoading}
              className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
            >
              <FiRefreshCw className={demandLoading ? 'animate-spin' : ''} />
              Actualizar
            </button>
            <button
              onClick={handleExportData}
              disabled={demandLoading || demandByCategory.length === 0}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              <FiDownload />
              Exportar
            </button>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex gap-4 flex-wrap">
            <div className="flex-1 min-w-64">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Buscar categoría
              </label>
              <div className="relative">
                <FiSearch className="absolute left-3 top-3 text-gray-400" />
                <input
                  type="text"
                  placeholder="Ej: Sedanes, SUVs..."
                  value={state.searchTerm || ''}
                  onChange={(e) => setState({ ...state, searchTerm: e.target.value })}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            <div className="flex-1 min-w-48">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Rango de tiempo
              </label>
              <select
                value={state.timeRange}
                onChange={(e) =>
                  setState({
                    ...state,
                    timeRange: e.target.value as 'week' | 'month' | 'quarter' | 'year',
                  })
                }
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              >
                <option value="week">Última Semana</option>
                <option value="month">Último Mes</option>
                <option value="quarter">Último Trimestre</option>
                <option value="year">Último Año</option>
              </select>
            </div>
          </div>
        </div>

        {/* Error State */}
        {demandError && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-red-600">Error al cargar análisis de mercado</p>
          </div>
        )}

        {/* Loading State */}
        {demandLoading && (
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        )}

        {/* Market Analysis Grid */}
        {!demandLoading && filteredDemand.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {filteredDemand.map((item) => (
              <div
                key={item.category}
                className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-lg transition-shadow"
              >
                {/* Category Header */}
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">{item.category}</h3>
                    <p className="text-sm text-gray-600 mt-1">
                      {item.activeListings} listados activos
                    </p>
                  </div>
                  <div className="text-2xl">{getTrendIcon(item.demandLevel)}</div>
                </div>

                {/* Demand Level Badge */}
                <div className="mb-4">
                  <span
                    className={`inline-block px-3 py-1 rounded-full text-sm font-medium border ${getDemandLevelColor(item.demandLevel)}`}
                  >
                    {getDemandLevelText(item.demandLevel)}
                  </span>
                </div>

                {/* Metrics */}
                <div className="space-y-3">
                  {/* Demand Score */}
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600 text-sm">Score Demanda</span>
                    <div className="flex items-center gap-2">
                      <div className="w-24 h-2 bg-gray-200 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-blue-600 rounded-full"
                          style={{ width: `${item.demandScore}%` }}
                        />
                      </div>
                      <span className="text-sm font-semibold text-gray-900">
                        {item.demandScore.toFixed(0)}/100
                      </span>
                    </div>
                  </div>

                  {/* Days to Sell */}
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600 text-sm flex items-center gap-2">
                      <FiCalendar className="w-4 h-4" />
                      Días Promedio
                    </span>
                    <span className="text-sm font-semibold text-gray-900">
                      {item.avgDaysToSale} días
                    </span>
                  </div>

                  {/* Searches */}
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600 text-sm flex items-center gap-2">
                      <FiSearch className="w-4 h-4" />
                      Búsquedas
                    </span>
                    <span className="text-sm font-semibold text-gray-900">
                      {item.totalSearches.toLocaleString()}
                    </span>
                  </div>
                </div>

                {/* Action Button */}
                <button className="w-full mt-4 px-4 py-2 bg-blue-50 text-blue-600 hover:bg-blue-100 rounded-lg font-medium text-sm transition-colors">
                  Ver Detalles
                </button>
              </div>
            ))}
          </div>
        )}

        {/* Empty State */}
        {!demandLoading && filteredDemand.length === 0 && (
          <div className="bg-gray-50 rounded-lg border border-gray-200 p-12 text-center">
            <FiBarChart2 className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">No hay datos disponibles</h3>
            <p className="text-gray-600">
              {state.searchTerm
                ? 'Intenta con otro término de búsqueda'
                : 'Los datos de mercado se actualizan diariamente'}
            </p>
          </div>
        )}

        {/* Summary Stats */}
        {!demandLoading && filteredDemand.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-8">
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Categorías</p>
                  <p className="text-2xl font-bold text-gray-900 mt-2">{filteredDemand.length}</p>
                </div>
                <FiBarChart2 className="w-10 h-10 text-blue-600 opacity-50" />
              </div>
            </div>

            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Demanda Promedio</p>
                  <p className="text-2xl font-bold text-gray-900 mt-2">
                    {(
                      filteredDemand.reduce((sum, item) => sum + item.demandScore, 0) /
                      filteredDemand.length
                    ).toFixed(0)}
                  </p>
                </div>
                <FiTarget className="w-10 h-10 text-green-600 opacity-50" />
              </div>
            </div>

            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Total Búsquedas</p>
                  <p className="text-2xl font-bold text-gray-900 mt-2">
                    {filteredDemand
                      .reduce((sum, item) => sum + item.totalSearches, 0)
                      .toLocaleString()}
                  </p>
                </div>
                <FiSearch className="w-10 h-10 text-orange-600 opacity-50" />
              </div>
            </div>

            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Listados Activos</p>
                  <p className="text-2xl font-bold text-gray-900 mt-2">
                    {filteredDemand
                      .reduce((sum, item) => sum + item.activeListings, 0)
                      .toLocaleString()}
                  </p>
                </div>
                <FiShoppingCart className="w-10 h-10 text-purple-600 opacity-50" />
              </div>
            </div>
          </div>
        )}
      </div>
    </DealerPortalLayout>
  );
}
