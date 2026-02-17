/**
 * MLDashboardPage - Machine Learning Dashboard para Admins
 * Monitoreo de modelos, métricas de inferencia y estadísticas de inteligencia
 */

import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  FiTrendingUp,
  FiTrendingDown,
  FiActivity,
  FiClock,
  FiCheckCircle,
  FiAlertCircle,
  FiBarChart2,
  FiRefreshCw,
  FiDownload,
} from 'react-icons/fi';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import vehicleIntelligenceService from '@/services/vehicleIntelligenceService';

interface ModelMetrics {
  modelName: string;
  accuracy: number;
  mae: number;
  rmse: number;
  lastTrained: string;
  nextTraining: string;
  status: 'healthy' | 'warning' | 'error';
}

interface InferenceMetrics {
  totalInferences: number;
  successRate: number;
  avgLatencyMs: number;
  p95LatencyMs: number;
  p99LatencyMs: number;
  errorsLast24h: number;
}

interface PerformanceData {
  timestamp: string;
  latency: number;
  throughput: number;
  errorRate: number;
}

export default function MLDashboardPage() {
  const [selectedModel, setSelectedModel] = useState<string>('pricing-model');

  // Fetch ML Statistics
  const {
    data: mlStats,
    isLoading: statsLoading,
    refetch: refetchStats,
  } = useQuery({
    queryKey: ['ml-statistics'],
    queryFn: () => vehicleIntelligenceService.getMLStatistics(),
    staleTime: 60 * 1000, // 1 minute
  });

  // Fetch Model Performance
  const {
    data: modelPerformance,
    isLoading: performanceLoading,
    refetch: refetchPerformance,
  } = useQuery({
    queryKey: ['model-performance'],
    queryFn: () => vehicleIntelligenceService.getModelPerformance(),
    staleTime: 60 * 1000,
  });

  // Fetch Inference Metrics
  const {
    data: inferenceMetrics,
    isLoading: metricsLoading,
    refetch: refetchMetrics,
  } = useQuery({
    queryKey: ['inference-metrics'],
    queryFn: () => vehicleIntelligenceService.getInferenceMetrics(),
    staleTime: 30 * 1000,
  });

  const isLoading = statsLoading || performanceLoading || metricsLoading;

  const handleRefreshAll = async () => {
    await Promise.all([refetchStats(), refetchPerformance(), refetchMetrics()]);
  };

  const handleExportMetrics = () => {
    // Generate and download metrics CSV
    const data = {
      timestamp: new Date().toISOString(),
      mlStats,
      modelPerformance,
      inferenceMetrics,
    };
    const csv = JSON.stringify(data, null, 2);
    const element = document.createElement('a');
    element.setAttribute('href', 'data:text/json;charset=utf-8,' + encodeURIComponent(csv));
    element.setAttribute('download', `ml-metrics-${Date.now()}.json`);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  };

  const getStatusColor = (status: 'healthy' | 'warning' | 'error'): string => {
    switch (status) {
      case 'healthy':
        return 'text-green-600 bg-green-50 border-green-200';
      case 'warning':
        return 'text-yellow-600 bg-yellow-50 border-yellow-200';
      case 'error':
        return 'text-red-600 bg-red-50 border-red-200';
    }
  };

  const getStatusIcon = (status: 'healthy' | 'warning' | 'error') => {
    if (status === 'healthy') return <FiCheckCircle className="w-5 h-5 text-green-600" />;
    if (status === 'warning') return <FiAlertCircle className="w-5 h-5 text-yellow-600" />;
    return <FiAlertCircle className="w-5 h-5 text-red-600" />;
  };

  // Mock data for charts (replace with real data from API)
  const performanceData: PerformanceData[] = [
    { timestamp: '00:00', latency: 45, throughput: 150, errorRate: 0.5 },
    { timestamp: '04:00', latency: 52, throughput: 180, errorRate: 0.3 },
    { timestamp: '08:00', latency: 38, throughput: 220, errorRate: 0.2 },
    { timestamp: '12:00', latency: 61, throughput: 280, errorRate: 1.2 },
    { timestamp: '16:00', latency: 48, throughput: 240, errorRate: 0.4 },
    { timestamp: '20:00', latency: 42, throughput: 200, errorRate: 0.1 },
    { timestamp: '23:59', latency: 39, throughput: 160, errorRate: 0.0 },
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto p-6">
        {/* Header */}
        <div className="flex justify-between items-start mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Dashboard ML</h1>
            <p className="text-gray-600 mt-2">
              Monitoreo de modelos de machine learning y métricas de inferencia
            </p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={handleRefreshAll}
              disabled={isLoading}
              className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
            >
              <FiRefreshCw className={isLoading ? 'animate-spin' : ''} />
              Actualizar
            </button>
            <button
              onClick={handleExportMetrics}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <FiDownload />
              Exportar
            </button>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        )}

        {!isLoading && (
          <>
            {/* Key Metrics */}
            <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-8">
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Total Inferencias</p>
                    <p className="text-2xl font-bold text-gray-900 mt-2">
                      {inferenceMetrics?.totalInferences?.toLocaleString() || '0'}
                    </p>
                  </div>
                  <FiBarChart2 className="w-10 h-10 text-blue-600 opacity-30" />
                </div>
              </div>

              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Success Rate</p>
                    <div className="flex items-center mt-2">
                      <p className="text-2xl font-bold text-gray-900">
                        {inferenceMetrics?.successRate?.toFixed(1) || '0'}%
                      </p>
                      <FiTrendingUp className="w-5 h-5 text-green-600 ml-2" />
                    </div>
                  </div>
                  <div className="w-10 h-10 rounded-full bg-green-50 flex items-center justify-center">
                    <FiCheckCircle className="w-6 h-6 text-green-600" />
                  </div>
                </div>
              </div>

              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Latencia Prom.</p>
                    <p className="text-2xl font-bold text-gray-900 mt-2">
                      {inferenceMetrics?.avgLatencyMs?.toFixed(0) || '0'}ms
                    </p>
                  </div>
                  <FiClock className="w-10 h-10 text-orange-600 opacity-30" />
                </div>
              </div>

              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">P95 Latencia</p>
                    <p className="text-2xl font-bold text-gray-900 mt-2">
                      {inferenceMetrics?.p95LatencyMs?.toFixed(0) || '0'}ms
                    </p>
                  </div>
                  <FiActivity className="w-10 h-10 text-purple-600 opacity-30" />
                </div>
              </div>

              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Errores 24h</p>
                    <div className="flex items-center mt-2">
                      <p className="text-2xl font-bold text-gray-900">
                        {inferenceMetrics?.errorsLast24h || '0'}
                      </p>
                      {(inferenceMetrics?.errorsLast24h || 0) > 0 && (
                        <FiAlertCircle className="w-5 h-5 text-red-600 ml-2" />
                      )}
                    </div>
                  </div>
                  <div className="w-10 h-10 rounded-full bg-red-50 flex items-center justify-center">
                    <span className="text-sm text-red-600 font-semibold">!</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Model Status */}
            <div className="bg-white rounded-lg border border-gray-200 p-6 mb-8">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Estado de Modelos</h2>
              <div className="space-y-4">
                {[
                  {
                    name: 'Pricing Model v2.3.1',
                    accuracy: 0.89,
                    mae: 45000,
                    status: 'healthy' as const,
                    lastTrained: '2025-01-25T14:30:00Z',
                  },
                  {
                    name: 'Demand Model v1.5.0',
                    accuracy: 0.87,
                    mae: 0,
                    status: 'healthy' as const,
                    lastTrained: '2025-01-25T08:00:00Z',
                  },
                  {
                    name: 'Time-to-Sale v1.2.0',
                    accuracy: 0.82,
                    mae: 0,
                    status: 'warning' as const,
                    lastTrained: '2025-01-23T10:00:00Z',
                  },
                ].map((model) => (
                  <div
                    key={model.name}
                    className={`border rounded-lg p-4 ${getStatusColor(model.status)}`}
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        {getStatusIcon(model.status)}
                        <div>
                          <h3 className="font-semibold">{model.name}</h3>
                          <p className="text-sm opacity-75">
                            Accuracy: {(model.accuracy * 100).toFixed(1)}%
                            {model.mae > 0 && ` | MAE: ${model.mae.toLocaleString()}`}
                          </p>
                        </div>
                      </div>
                      <div className="text-right text-sm">
                        <p className="opacity-75">
                          Último entrenamiento: {new Date(model.lastTrained).toLocaleDateString()}
                        </p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Performance Charts */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
              {/* Latency Chart */}
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Latencia (últimas 24h)</h2>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={performanceData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="timestamp" />
                    <YAxis label={{ value: 'ms', angle: -90, position: 'insideLeft' }} />
                    <Tooltip formatter={(value: any) => `${value.toFixed(0)}ms`} />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="latency"
                      stroke="#3b82f6"
                      strokeWidth={2}
                      name="Latencia Promedio"
                      dot={false}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>

              {/* Throughput Chart */}
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  Throughput (últimas 24h)
                </h2>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={performanceData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="timestamp" />
                    <YAxis label={{ value: 'req/min', angle: -90, position: 'insideLeft' }} />
                    <Tooltip formatter={(value: any) => `${value.toFixed(0)} req/min`} />
                    <Legend />
                    <Bar dataKey="throughput" fill="#10b981" name="Throughput" />
                  </BarChart>
                </ResponsiveContainer>
              </div>

              {/* Error Rate Chart */}
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  Error Rate (últimas 24h)
                </h2>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={performanceData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="timestamp" />
                    <YAxis label={{ value: '%', angle: -90, position: 'insideLeft' }} />
                    <Tooltip formatter={(value: any) => `${value.toFixed(2)}%`} />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="errorRate"
                      stroke="#ef4444"
                      strokeWidth={2}
                      name="Error Rate"
                      dot={false}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>

              {/* Inference Distribution */}
              <div className="bg-white rounded-lg border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  Distribución de Inferencias
                </h2>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart
                    data={[
                      { name: 'Pricing', value: 45000 },
                      { name: 'Demand', value: 32000 },
                      { name: 'TimeToSale', value: 28000 },
                      { name: 'Recommendations', value: 18000 },
                    ]}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis label={{ value: 'inferencias', angle: -90, position: 'insideLeft' }} />
                    <Tooltip formatter={(value: any) => value.toLocaleString()} />
                    <Legend />
                    <Bar dataKey="value" fill="#8b5cf6" name="Inferencias" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* System Health */}
            <div className="bg-white rounded-lg border border-gray-200 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Salud del Sistema</h2>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <p className="text-sm text-gray-600 mb-2">API Availability</p>
                  <div className="flex items-center gap-2">
                    <div className="w-4 h-4 bg-green-500 rounded-full" />
                    <span className="text-sm font-medium text-gray-900">99.99%</span>
                  </div>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-2">Database Health</p>
                  <div className="flex items-center gap-2">
                    <div className="w-4 h-4 bg-green-500 rounded-full" />
                    <span className="text-sm font-medium text-gray-900">Óptimo</span>
                  </div>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-2">Cache Hit Rate</p>
                  <div className="flex items-center gap-2">
                    <div className="w-4 h-4 bg-green-500 rounded-full" />
                    <span className="text-sm font-medium text-gray-900">87.3%</span>
                  </div>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
