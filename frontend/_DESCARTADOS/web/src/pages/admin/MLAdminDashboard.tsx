/**
 * MLAdminDashboard - Dashboard de Modelos ML para Administradores
 *
 * Permite a los administradores y data scientists monitorear y gestionar
 * los modelos de Machine Learning del sistema de recomendaciones.
 *
 * @module pages/admin/MLAdminDashboard
 * @version 1.0.0
 * @since Enero 25, 2026
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  FiCpu,
  FiActivity,
  FiRefreshCw,
  FiAlertTriangle,
  FiCheckCircle,
  FiClock,
  FiTrendingUp,
  FiDatabase,
  FiZap,
  FiSettings,
  FiArrowLeft,
  FiDownload,
  FiPlay,
  FiPause,
} from 'react-icons/fi';
import AdminLayout from '../../layouts/AdminLayout';

interface MLModel {
  id: string;
  name: string;
  version: string;
  type: 'recommendation' | 'lead_scoring' | 'pricing' | 'fraud_detection';
  status: 'active' | 'training' | 'inactive' | 'error';
  accuracy: number;
  lastTrained: string;
  nextTraining: string;
  inferenceCount: number;
  avgLatencyMs: number;
  featureCount: number;
  datasetSize: string;
}

interface ModelMetrics {
  totalInferences: number;
  avgLatency: number;
  successRate: number;
  errorRate: number;
  activeModels: number;
  trainingJobs: number;
}

const MLAdminDashboard = () => {
  const [models, setModels] = useState<MLModel[]>([]);
  const [metrics, setMetrics] = useState<ModelMetrics | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedModel, setSelectedModel] = useState<string | null>(null);
  const [isRetraining, setIsRetraining] = useState(false);

  // Mock data
  useEffect(() => {
    const mockModels: MLModel[] = [
      {
        id: '1',
        name: 'Vehicle Recommendations',
        version: 'v2.4.1',
        type: 'recommendation',
        status: 'active',
        accuracy: 87.5,
        lastTrained: '2026-01-24T08:00:00',
        nextTraining: '2026-01-31T08:00:00',
        inferenceCount: 145230,
        avgLatencyMs: 23,
        featureCount: 48,
        datasetSize: '2.3M records',
      },
      {
        id: '2',
        name: 'Similar Vehicles',
        version: 'v1.8.0',
        type: 'recommendation',
        status: 'active',
        accuracy: 92.1,
        lastTrained: '2026-01-23T10:00:00',
        nextTraining: '2026-01-30T10:00:00',
        inferenceCount: 89450,
        avgLatencyMs: 18,
        featureCount: 32,
        datasetSize: '1.8M records',
      },
      {
        id: '3',
        name: 'Lead Scoring',
        version: 'v3.1.2',
        type: 'lead_scoring',
        status: 'active',
        accuracy: 78.3,
        lastTrained: '2026-01-22T06:00:00',
        nextTraining: '2026-01-29T06:00:00',
        inferenceCount: 34560,
        avgLatencyMs: 45,
        featureCount: 56,
        datasetSize: '850K records',
      },
      {
        id: '4',
        name: 'Price Intelligence',
        version: 'v2.0.5',
        type: 'pricing',
        status: 'training',
        accuracy: 85.7,
        lastTrained: '2026-01-21T14:00:00',
        nextTraining: '2026-01-28T14:00:00',
        inferenceCount: 67890,
        avgLatencyMs: 35,
        featureCount: 42,
        datasetSize: '1.2M records',
      },
      {
        id: '5',
        name: 'Fraud Detection',
        version: 'v1.2.0',
        type: 'fraud_detection',
        status: 'active',
        accuracy: 94.8,
        lastTrained: '2026-01-20T12:00:00',
        nextTraining: '2026-01-27T12:00:00',
        inferenceCount: 12340,
        avgLatencyMs: 15,
        featureCount: 28,
        datasetSize: '500K records',
      },
    ];

    const mockMetrics: ModelMetrics = {
      totalInferences: 349470,
      avgLatency: 27.2,
      successRate: 99.7,
      errorRate: 0.3,
      activeModels: 4,
      trainingJobs: 1,
    };

    setTimeout(() => {
      setModels(mockModels);
      setMetrics(mockMetrics);
      setIsLoading(false);
    }, 500);
  }, []);

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'active':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
            <FiCheckCircle className="mr-1" /> Activo
          </span>
        );
      case 'training':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
            <FiRefreshCw className="mr-1 animate-spin" /> Entrenando
          </span>
        );
      case 'inactive':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
            <FiPause className="mr-1" /> Inactivo
          </span>
        );
      case 'error':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
            <FiAlertTriangle className="mr-1" /> Error
          </span>
        );
      default:
        return null;
    }
  };

  const getTypeLabel = (type: string) => {
    const labels: Record<string, string> = {
      recommendation: 'Recomendación',
      lead_scoring: 'Lead Scoring',
      pricing: 'Pricing',
      fraud_detection: 'Detección de Fraude',
    };
    return labels[type] || type;
  };

  const getTypeColor = (type: string) => {
    const colors: Record<string, string> = {
      recommendation: 'bg-blue-100 text-blue-800',
      lead_scoring: 'bg-purple-100 text-purple-800',
      pricing: 'bg-green-100 text-green-800',
      fraud_detection: 'bg-red-100 text-red-800',
    };
    return colors[type] || 'bg-gray-100 text-gray-800';
  };

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat('es-DO').format(num);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const handleRetrain = async (modelId: string) => {
    setIsRetraining(true);
    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 2000));
    setModels((prev) =>
      prev.map((m) => (m.id === modelId ? { ...m, status: 'training' as const } : m))
    );
    setIsRetraining(false);
  };

  if (isLoading) {
    return (
      <AdminLayout>
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
        </div>
      </AdminLayout>
    );
  }

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between">
          <div>
            <div className="flex items-center">
              <div className="p-3 bg-purple-100 rounded-lg mr-4">
                <FiCpu className="w-6 h-6 text-purple-600" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Dashboard ML</h1>
                <p className="text-gray-600">Monitoreo de modelos de Machine Learning</p>
              </div>
            </div>
          </div>
          <div className="mt-4 sm:mt-0 flex gap-3">
            <Link
              to="/admin/ml/features"
              className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
            >
              <FiDatabase className="mr-2" />
              Feature Store
            </Link>
            <button className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-purple-600 hover:bg-purple-700">
              <FiPlay className="mr-2" />
              Nuevo entrenamiento
            </button>
          </div>
        </div>

        {/* Metrics Cards */}
        {metrics && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Inferencias totales</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatNumber(metrics.totalInferences)}
                  </p>
                </div>
                <div className="p-3 bg-blue-100 rounded-lg">
                  <FiZap className="w-6 h-6 text-blue-600" />
                </div>
              </div>
              <div className="mt-2 text-sm text-green-600 flex items-center">
                <FiTrendingUp className="mr-1" /> +12.5% vs semana anterior
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Latencia promedio</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {metrics.avgLatency.toFixed(1)} ms
                  </p>
                </div>
                <div className="p-3 bg-green-100 rounded-lg">
                  <FiActivity className="w-6 h-6 text-green-600" />
                </div>
              </div>
              <div className="mt-2 text-sm text-green-600 flex items-center">
                <FiCheckCircle className="mr-1" /> Dentro del SLA
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Tasa de éxito</p>
                  <p className="text-2xl font-bold text-gray-900">{metrics.successRate}%</p>
                </div>
                <div className="p-3 bg-purple-100 rounded-lg">
                  <FiCheckCircle className="w-6 h-6 text-purple-600" />
                </div>
              </div>
              <div className="mt-2 text-sm text-gray-500">Error rate: {metrics.errorRate}%</div>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Modelos activos</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {metrics.activeModels} / {models.length}
                  </p>
                </div>
                <div className="p-3 bg-yellow-100 rounded-lg">
                  <FiCpu className="w-6 h-6 text-yellow-600" />
                </div>
              </div>
              <div className="mt-2 text-sm text-yellow-600 flex items-center">
                <FiRefreshCw className="mr-1" /> {metrics.trainingJobs} en entrenamiento
              </div>
            </div>
          </div>
        )}

        {/* Models Table */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900">Modelos Registrados</h2>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Modelo
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tipo
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Estado
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Accuracy
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Latencia
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Inferencias
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Último entrenamiento
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {models.map((model) => (
                  <tr key={model.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div>
                        <div className="text-sm font-medium text-gray-900">{model.name}</div>
                        <div className="text-sm text-gray-500">{model.version}</div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getTypeColor(model.type)}`}
                      >
                        {getTypeLabel(model.type)}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">{getStatusBadge(model.status)}</td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="w-16 bg-gray-200 rounded-full h-2 mr-2">
                          <div
                            className={`h-2 rounded-full ${
                              model.accuracy >= 90
                                ? 'bg-green-500'
                                : model.accuracy >= 80
                                  ? 'bg-yellow-500'
                                  : 'bg-red-500'
                            }`}
                            style={{ width: `${model.accuracy}%` }}
                          />
                        </div>
                        <span className="text-sm text-gray-900">{model.accuracy}%</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-900">{model.avgLatencyMs} ms</span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-900">
                        {formatNumber(model.inferenceCount)}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">{formatDate(model.lastTrained)}</div>
                      <div className="text-xs text-gray-500 flex items-center">
                        <FiClock className="mr-1" /> Próximo: {formatDate(model.nextTraining)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex items-center justify-end space-x-2">
                        <button
                          onClick={() => handleRetrain(model.id)}
                          disabled={model.status === 'training' || isRetraining}
                          className="text-purple-600 hover:text-purple-900 disabled:opacity-50 disabled:cursor-not-allowed"
                          title="Reentrenar"
                        >
                          <FiRefreshCw
                            className={model.status === 'training' ? 'animate-spin' : ''}
                          />
                        </button>
                        <button
                          className="text-gray-600 hover:text-gray-900"
                          title="Descargar modelo"
                        >
                          <FiDownload />
                        </button>
                        <button className="text-gray-600 hover:text-gray-900" title="Configuración">
                          <FiSettings />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Training Schedule */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            <FiClock className="mr-2 text-gray-500" />
            Calendario de Entrenamiento
          </h2>
          <div className="overflow-x-auto">
            <div className="inline-flex gap-2">
              {models.map((model) => (
                <div
                  key={model.id}
                  className="flex-shrink-0 w-64 border border-gray-200 rounded-lg p-4"
                >
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-medium text-gray-900">{model.name}</span>
                    {getStatusBadge(model.status)}
                  </div>
                  <div className="text-xs text-gray-500 space-y-1">
                    <div className="flex items-center">
                      <FiCheckCircle className="mr-1 text-green-500" />
                      Último: {formatDate(model.lastTrained)}
                    </div>
                    <div className="flex items-center">
                      <FiClock className="mr-1 text-blue-500" />
                      Próximo: {formatDate(model.nextTraining)}
                    </div>
                    <div className="flex items-center">
                      <FiDatabase className="mr-1 text-gray-400" />
                      Dataset: {model.datasetSize}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Feature Store Link */}
        <div className="bg-gradient-to-r from-purple-500 to-indigo-600 rounded-lg shadow-sm p-6 text-white">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-lg font-semibold mb-1">Feature Store</h3>
              <p className="text-purple-100">
                Gestiona las features utilizadas por los modelos ML. Revisa el catálogo, monitorea
                drift y crea nuevas transformaciones.
              </p>
            </div>
            <Link
              to="/admin/ml/features"
              className="inline-flex items-center px-4 py-2 bg-white text-purple-600 rounded-md font-medium hover:bg-purple-50"
            >
              <FiDatabase className="mr-2" />
              Ir al Feature Store
            </Link>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
};

export default MLAdminDashboard;
