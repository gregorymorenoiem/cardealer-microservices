import { useState, useCallback } from 'react';
import { Line, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler,
} from 'chart.js';
import {
  FiTrendingUp,
  FiTrendingDown,
  FiEye,
  FiPhone,
  FiDollarSign,
  FiClock,
  FiAlertTriangle,
  FiAlertCircle,
  FiCheckCircle,
  FiBell,
  FiChevronRight,
  FiRefreshCw,
  FiDownload,
  FiBarChart2,
  FiTarget,
  FiAward,
  FiX,
  FiActivity,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler
);

// ============================================
// TYPES
// ============================================

interface KpiCardData {
  title: string;
  value: string | number;
  change: number;
  changeLabel: string;
  icon: React.ReactNode;
  trend: 'up' | 'down' | 'neutral';
  color: string;
}

interface FunnelStage {
  name: string;
  value: number;
  percentage: number;
  conversionRate: number;
}

interface AgingBucket {
  label: string;
  count: number;
  value: number;
  percentage: number;
  color: string;
}

interface VehiclePerformanceRow {
  id: string;
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  thumbnail: string;
  views: number;
  contacts: number;
  favorites: number;
  daysOnMarket: number;
  score: number;
}

interface AlertItem {
  id: string;
  type: string;
  severity: 'info' | 'warning' | 'critical';
  title: string;
  message: string;
  createdAt: string;
  isRead: boolean;
  actionUrl?: string;
}

interface BenchmarkMetric {
  name: string;
  dealerValue: number;
  marketAvg: number;
  percentile: number;
  isAboveAverage: boolean;
}

// ============================================
// MOCK DATA (Replace with API calls)
// ============================================

const mockKpis: KpiCardData[] = [
  {
    title: 'Total Vistas',
    value: '24,532',
    change: 12.5,
    changeLabel: 'vs mes anterior',
    icon: <FiEye className="w-6 h-6" />,
    trend: 'up',
    color: 'blue',
  },
  {
    title: 'Contactos',
    value: '842',
    change: 8.3,
    changeLabel: 'vs mes anterior',
    icon: <FiPhone className="w-6 h-6" />,
    trend: 'up',
    color: 'green',
  },
  {
    title: 'Leads Calificados',
    value: '156',
    change: -3.2,
    changeLabel: 'vs mes anterior',
    icon: <FiTarget className="w-6 h-6" />,
    trend: 'down',
    color: 'purple',
  },
  {
    title: 'Ventas',
    value: '28',
    change: 15.0,
    changeLabel: 'vs mes anterior',
    icon: <FiDollarSign className="w-6 h-6" />,
    trend: 'up',
    color: 'amber',
  },
  {
    title: 'Tasa de Conversión',
    value: '3.4%',
    change: 0.5,
    changeLabel: 'vs mes anterior',
    icon: <FiTrendingUp className="w-6 h-6" />,
    trend: 'up',
    color: 'emerald',
  },
  {
    title: 'Tiempo Resp. Promedio',
    value: '12 min',
    change: -25.0,
    changeLabel: 'mejor que antes',
    icon: <FiClock className="w-6 h-6" />,
    trend: 'up',
    color: 'indigo',
  },
];

const mockFunnelStages: FunnelStage[] = [
  { name: 'Impresiones', value: 125000, percentage: 100, conversionRate: 100 },
  { name: 'Vistas', value: 24532, percentage: 19.6, conversionRate: 19.6 },
  { name: 'Contactos', value: 842, percentage: 3.4, conversionRate: 3.4 },
  { name: 'Leads Calificados', value: 156, percentage: 18.5, conversionRate: 18.5 },
  { name: 'En Negociación', value: 68, percentage: 43.6, conversionRate: 43.6 },
  { name: 'Ventas', value: 28, percentage: 41.2, conversionRate: 41.2 },
];

const mockAgingBuckets: AgingBucket[] = [
  { label: '0-15 días', count: 18, value: 485000, percentage: 30, color: '#10B981' },
  { label: '16-30 días', count: 15, value: 420000, percentage: 25, color: '#22C55E' },
  { label: '31-45 días', count: 12, value: 310000, percentage: 20, color: '#FBBF24' },
  { label: '46-60 días', count: 8, value: 195000, percentage: 13.3, color: '#F97316' },
  { label: '61-90 días', count: 5, value: 125000, percentage: 8.3, color: '#EF4444' },
  { label: '90+ días', count: 2, value: 45000, percentage: 3.3, color: '#DC2626' },
];

const mockVehiclePerformance: VehiclePerformanceRow[] = [
  {
    id: '1',
    title: 'Toyota Camry 2023',
    make: 'Toyota',
    model: 'Camry',
    year: 2023,
    price: 35000,
    thumbnail: '/vehicles/camry.jpg',
    views: 1250,
    contacts: 45,
    favorites: 32,
    daysOnMarket: 12,
    score: 92,
  },
  {
    id: '2',
    title: 'Honda Accord 2022',
    make: 'Honda',
    model: 'Accord',
    year: 2022,
    price: 32000,
    thumbnail: '/vehicles/accord.jpg',
    views: 980,
    contacts: 38,
    favorites: 28,
    daysOnMarket: 18,
    score: 85,
  },
  {
    id: '3',
    title: 'BMW X5 2023',
    make: 'BMW',
    model: 'X5',
    year: 2023,
    price: 68000,
    thumbnail: '/vehicles/x5.jpg',
    views: 890,
    contacts: 25,
    favorites: 45,
    daysOnMarket: 25,
    score: 78,
  },
  {
    id: '4',
    title: 'Mercedes GLE 2022',
    make: 'Mercedes',
    model: 'GLE',
    year: 2022,
    price: 72000,
    thumbnail: '/vehicles/gle.jpg',
    views: 756,
    contacts: 22,
    favorites: 38,
    daysOnMarket: 32,
    score: 72,
  },
  {
    id: '5',
    title: 'Hyundai Tucson 2023',
    make: 'Hyundai',
    model: 'Tucson',
    year: 2023,
    price: 28000,
    thumbnail: '/vehicles/tucson.jpg',
    views: 650,
    contacts: 35,
    favorites: 20,
    daysOnMarket: 8,
    score: 88,
  },
];

const mockAlerts: AlertItem[] = [
  {
    id: '1',
    type: 'aging',
    severity: 'critical',
    title: '3 vehículos > 90 días',
    message: 'Considera reducir precio o promocionar',
    createdAt: '2024-01-15T10:00:00Z',
    isRead: false,
    actionUrl: '/inventory?filter=aging',
  },
  {
    id: '2',
    type: 'lead',
    severity: 'warning',
    title: '5 leads sin responder',
    message: 'Responde antes de 24 horas para mejor conversión',
    createdAt: '2024-01-15T08:00:00Z',
    isRead: false,
    actionUrl: '/leads?filter=pending',
  },
  {
    id: '3',
    type: 'performance',
    severity: 'info',
    title: 'BMW X5 con alta demanda',
    message: '45 favoritos, considera destacar en homepage',
    createdAt: '2024-01-14T15:00:00Z',
    isRead: true,
    actionUrl: '/vehicles/3',
  },
  {
    id: '4',
    type: 'benchmark',
    severity: 'warning',
    title: 'Tiempo de respuesta alto',
    message: 'Tu promedio es 12 min, el mercado es 8 min',
    createdAt: '2024-01-14T12:00:00Z',
    isRead: true,
  },
];

const mockBenchmarks: BenchmarkMetric[] = [
  { name: 'Días en Mercado', dealerValue: 32, marketAvg: 45, percentile: 75, isAboveAverage: true },
  {
    name: 'Tasa de Conversión',
    dealerValue: 3.4,
    marketAvg: 2.8,
    percentile: 68,
    isAboveAverage: true,
  },
  {
    name: 'Vistas por Vehículo',
    dealerValue: 420,
    marketAvg: 380,
    percentile: 62,
    isAboveAverage: true,
  },
  {
    name: 'Contactos/Vista',
    dealerValue: 3.4,
    marketAvg: 3.8,
    percentile: 42,
    isAboveAverage: false,
  },
  {
    name: 'Tiempo de Respuesta',
    dealerValue: 12,
    marketAvg: 8,
    percentile: 35,
    isAboveAverage: false,
  },
];

// ============================================
// COMPONENTS
// ============================================

const KpiCard = ({ kpi }: { kpi: KpiCardData }) => {
  const colorClasses: Record<string, string> = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    purple: 'bg-purple-50 text-purple-600',
    amber: 'bg-amber-50 text-amber-600',
    emerald: 'bg-emerald-50 text-emerald-600',
    indigo: 'bg-indigo-50 text-indigo-600',
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between">
        <div className={`p-3 rounded-lg ${colorClasses[kpi.color]}`}>{kpi.icon}</div>
        <div
          className={`flex items-center gap-1 text-sm font-medium ${
            kpi.trend === 'up'
              ? 'text-green-600'
              : kpi.trend === 'down'
                ? 'text-red-600'
                : 'text-gray-500'
          }`}
        >
          {kpi.trend === 'up' ? <FiTrendingUp /> : kpi.trend === 'down' ? <FiTrendingDown /> : null}
          {Math.abs(kpi.change)}%
        </div>
      </div>
      <div className="mt-4">
        <p className="text-2xl font-bold text-gray-900">{kpi.value}</p>
        <p className="text-sm text-gray-500 mt-1">{kpi.title}</p>
        <p className="text-xs text-gray-400 mt-0.5">{kpi.changeLabel}</p>
      </div>
    </div>
  );
};

const FunnelChart = ({ stages }: { stages: FunnelStage[] }) => {
  const maxValue = stages[0]?.value || 1;

  return (
    <div className="space-y-3">
      {stages.map((stage, index) => {
        const width = (stage.value / maxValue) * 100;
        const nextStage = stages[index + 1];
        const dropoff = nextStage
          ? (((stage.value - nextStage.value) / stage.value) * 100).toFixed(1)
          : null;

        return (
          <div key={stage.name} className="relative">
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm font-medium text-gray-700">{stage.name}</span>
              <div className="flex items-center gap-3">
                <span className="text-sm font-bold text-gray-900">
                  {stage.value.toLocaleString()}
                </span>
                {dropoff && <span className="text-xs text-red-500">-{dropoff}%</span>}
              </div>
            </div>
            <div className="h-8 bg-gray-100 rounded-lg overflow-hidden">
              <div
                className="h-full bg-gradient-to-r from-blue-500 to-blue-600 rounded-lg flex items-center justify-end pr-2 transition-all duration-500"
                style={{ width: `${Math.max(width, 5)}%` }}
              >
                <span className="text-xs text-white font-medium">
                  {stage.conversionRate.toFixed(1)}%
                </span>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
};

const AgingChart = ({ buckets }: { buckets: AgingBucket[] }) => {
  const data = {
    labels: buckets.map((b) => b.label),
    datasets: [
      {
        data: buckets.map((b) => b.count),
        backgroundColor: buckets.map((b) => b.color),
        borderWidth: 0,
      },
    ],
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'right' as const,
        labels: {
          usePointStyle: true,
          padding: 15,
          font: { size: 11 },
        },
      },
    },
    cutout: '65%',
  };

  return (
    <div className="h-64">
      <Doughnut data={data} options={options} />
    </div>
  );
};

const VehiclePerformanceTable = ({ vehicles }: { vehicles: VehiclePerformanceRow[] }) => {
  const getScoreColor = (score: number) => {
    if (score >= 80) return 'bg-green-100 text-green-800';
    if (score >= 60) return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
  };

  return (
    <div className="overflow-x-auto">
      <table className="w-full">
        <thead>
          <tr className="text-left text-xs text-gray-500 uppercase tracking-wider">
            <th className="pb-3 font-medium">Vehículo</th>
            <th className="pb-3 font-medium text-center">Vistas</th>
            <th className="pb-3 font-medium text-center">Contactos</th>
            <th className="pb-3 font-medium text-center">Favoritos</th>
            <th className="pb-3 font-medium text-center">Días</th>
            <th className="pb-3 font-medium text-center">Score</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {vehicles.map((vehicle) => (
            <tr key={vehicle.id} className="hover:bg-gray-50 cursor-pointer">
              <td className="py-3">
                <div className="flex items-center gap-3">
                  <div className="w-12 h-9 bg-gray-200 rounded-lg overflow-hidden">
                    <img
                      src={vehicle.thumbnail}
                      alt={vehicle.title}
                      className="w-full h-full object-cover"
                      onError={(e) => ((e.target as HTMLImageElement).style.display = 'none')}
                    />
                  </div>
                  <div>
                    <p className="font-medium text-gray-900 text-sm">{vehicle.title}</p>
                    <p className="text-xs text-gray-500">${vehicle.price.toLocaleString()}</p>
                  </div>
                </div>
              </td>
              <td className="py-3 text-center">
                <span className="text-sm font-medium text-gray-900">
                  {vehicle.views.toLocaleString()}
                </span>
              </td>
              <td className="py-3 text-center">
                <span className="text-sm font-medium text-gray-900">{vehicle.contacts}</span>
              </td>
              <td className="py-3 text-center">
                <span className="text-sm font-medium text-gray-900">{vehicle.favorites}</span>
              </td>
              <td className="py-3 text-center">
                <span
                  className={`text-sm font-medium ${vehicle.daysOnMarket > 60 ? 'text-red-600' : vehicle.daysOnMarket > 30 ? 'text-yellow-600' : 'text-gray-900'}`}
                >
                  {vehicle.daysOnMarket}
                </span>
              </td>
              <td className="py-3 text-center">
                <span
                  className={`inline-flex px-2 py-1 rounded-full text-xs font-semibold ${getScoreColor(vehicle.score)}`}
                >
                  {vehicle.score}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

const AlertsPanel = ({
  alerts,
  onDismiss,
}: {
  alerts: AlertItem[];
  onDismiss: (id: string) => void;
}) => {
  const severityStyles = {
    critical: 'bg-red-50 border-red-200 text-red-800',
    warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
    info: 'bg-blue-50 border-blue-200 text-blue-800',
  };

  const severityIcons = {
    critical: <FiAlertTriangle className="w-5 h-5 text-red-500" />,
    warning: <FiAlertCircle className="w-5 h-5 text-yellow-500" />,
    info: <FiCheckCircle className="w-5 h-5 text-blue-500" />,
  };

  return (
    <div className="space-y-3">
      {alerts.map((alert) => (
        <div
          key={alert.id}
          className={`p-4 rounded-lg border ${severityStyles[alert.severity]} ${!alert.isRead ? 'ring-2 ring-offset-1' : 'opacity-75'}`}
        >
          <div className="flex items-start gap-3">
            {severityIcons[alert.severity]}
            <div className="flex-1">
              <p className="font-medium text-sm">{alert.title}</p>
              <p className="text-xs mt-1 opacity-80">{alert.message}</p>
            </div>
            <button onClick={() => onDismiss(alert.id)} className="p-1 hover:bg-white/50 rounded">
              <FiX className="w-4 h-4" />
            </button>
          </div>
          {alert.actionUrl && (
            <a
              href={alert.actionUrl}
              className="mt-2 inline-flex items-center gap-1 text-xs font-medium hover:underline"
            >
              Ver detalles <FiChevronRight className="w-3 h-3" />
            </a>
          )}
        </div>
      ))}
    </div>
  );
};

const BenchmarkComparison = ({ benchmarks }: { benchmarks: BenchmarkMetric[] }) => {
  return (
    <div className="space-y-4">
      {benchmarks.map((metric) => (
        <div key={metric.name} className="space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-gray-700">{metric.name}</span>
            <div className="flex items-center gap-2">
              <span
                className={`text-sm font-bold ${metric.isAboveAverage ? 'text-green-600' : 'text-red-600'}`}
              >
                {metric.dealerValue}
              </span>
              <span className="text-xs text-gray-400">vs {metric.marketAvg} mercado</span>
            </div>
          </div>
          <div className="relative h-2 bg-gray-200 rounded-full overflow-hidden">
            <div
              className={`absolute left-0 top-0 h-full rounded-full transition-all duration-500 ${
                metric.isAboveAverage ? 'bg-green-500' : 'bg-red-500'
              }`}
              style={{ width: `${metric.percentile}%` }}
            />
            <div
              className="absolute top-1/2 w-0.5 h-4 bg-gray-400 -translate-y-1/2"
              style={{ left: '50%' }}
            />
          </div>
          <div className="flex justify-between text-xs text-gray-400">
            <span>Bajo</span>
            <span>Percentil {metric.percentile}</span>
            <span>Alto</span>
          </div>
        </div>
      ))}
    </div>
  );
};

// ============================================
// MAIN COMPONENT
// ============================================

interface AdvancedAnalyticsDashboardProps {
  dealerId?: string;
}

export const AdvancedAnalyticsDashboard = ({
  dealerId = 'demo',
}: AdvancedAnalyticsDashboardProps) => {
  const [dateRange, setDateRange] = useState('30');
  const [isLoading, setIsLoading] = useState(false);

  // Log dealerId for future API integration
  console.debug('Loading analytics for dealer:', dealerId);
  const [alerts, setAlerts] = useState(mockAlerts);
  const [activeTab, setActiveTab] = useState<'overview' | 'inventory' | 'leads' | 'benchmark'>(
    'overview'
  );

  const handleRefresh = useCallback(() => {
    setIsLoading(true);
    // Simular carga
    setTimeout(() => setIsLoading(false), 1000);
  }, []);

  const handleDismissAlert = useCallback((id: string) => {
    setAlerts((prev) => prev.filter((a) => a.id !== id));
  }, []);

  // Trend chart data
  const trendData = {
    labels: ['Ene 1', 'Ene 5', 'Ene 10', 'Ene 15', 'Ene 20', 'Ene 25', 'Ene 30'],
    datasets: [
      {
        label: 'Vistas',
        data: [850, 920, 780, 1100, 980, 1250, 1150],
        borderColor: '#3B82F6',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        fill: true,
        tension: 0.4,
      },
      {
        label: 'Contactos',
        data: [28, 35, 25, 42, 38, 52, 45],
        borderColor: '#10B981',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        fill: true,
        tension: 0.4,
        yAxisID: 'y1',
      },
    ],
  };

  const trendOptions = {
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
      mode: 'index' as const,
      intersect: false,
    },
    plugins: {
      legend: {
        position: 'top' as const,
      },
    },
    scales: {
      y: {
        type: 'linear' as const,
        display: true,
        position: 'left' as const,
      },
      y1: {
        type: 'linear' as const,
        display: true,
        position: 'right' as const,
        grid: {
          drawOnChartArea: false,
        },
      },
    },
  };

  const unreadAlerts = alerts.filter((a) => !a.isRead).length;

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                  <FiBarChart2 className="w-7 h-7 text-blue-600" />
                  Analytics Dashboard
                </h1>
                <p className="text-sm text-gray-500 mt-1">
                  Métricas de rendimiento y análisis de tu inventario
                </p>
              </div>

              <div className="flex items-center gap-3">
                {/* Date Range Selector */}
                <div className="flex items-center gap-2 bg-gray-100 rounded-lg p-1">
                  {['7', '30', '90'].map((days) => (
                    <button
                      key={days}
                      onClick={() => setDateRange(days)}
                      className={`px-3 py-1.5 text-sm font-medium rounded-md transition-colors ${
                        dateRange === days
                          ? 'bg-white text-blue-600 shadow-sm'
                          : 'text-gray-600 hover:text-gray-900'
                      }`}
                    >
                      {days}D
                    </button>
                  ))}
                </div>

                {/* Alerts Badge */}
                <button className="relative p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg">
                  <FiBell className="w-5 h-5" />
                  {unreadAlerts > 0 && (
                    <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs font-bold rounded-full flex items-center justify-center">
                      {unreadAlerts}
                    </span>
                  )}
                </button>

                {/* Refresh Button */}
                <button
                  onClick={handleRefresh}
                  disabled={isLoading}
                  className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg disabled:opacity-50"
                >
                  <FiRefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
                </button>

                {/* Export Button */}
                <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
                  <FiDownload className="w-4 h-4" />
                  Exportar
                </button>
              </div>
            </div>

            {/* Tabs */}
            <div className="flex gap-6 mt-6 border-b border-gray-200 -mb-px">
              {[
                { id: 'overview', label: 'Resumen', icon: <FiActivity /> },
                { id: 'inventory', label: 'Inventario', icon: <FiBarChart2 /> },
                { id: 'leads', label: 'Leads & Funnel', icon: <FiTarget /> },
                { id: 'benchmark', label: 'Benchmark', icon: <FiAward /> },
              ].map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id as typeof activeTab)}
                  className={`flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 -mb-px transition-colors ${
                    activeTab === tab.id
                      ? 'border-blue-600 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  {tab.icon}
                  {tab.label}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Main Content */}
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* KPI Cards */}
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 mb-8">
            {mockKpis.map((kpi, index) => (
              <KpiCard key={index} kpi={kpi} />
            ))}
          </div>

          {/* Main Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Left Column - 2/3 width */}
            <div className="lg:col-span-2 space-y-6">
              {/* Trend Chart */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-lg font-semibold text-gray-900">Tendencia de Rendimiento</h2>
                  <div className="flex items-center gap-2">
                    <span className="flex items-center gap-1 text-sm text-blue-600">
                      <span className="w-3 h-3 bg-blue-500 rounded-full"></span>
                      Vistas
                    </span>
                    <span className="flex items-center gap-1 text-sm text-green-600">
                      <span className="w-3 h-3 bg-green-500 rounded-full"></span>
                      Contactos
                    </span>
                  </div>
                </div>
                <div className="h-72">
                  <Line data={trendData} options={trendOptions} />
                </div>
              </div>

              {/* Funnel */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-lg font-semibold text-gray-900">Embudo de Conversión</h2>
                  <span className="text-sm text-gray-500">Últimos {dateRange} días</span>
                </div>
                <FunnelChart stages={mockFunnelStages} />
              </div>

              {/* Vehicle Performance Table */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-lg font-semibold text-gray-900">Rendimiento por Vehículo</h2>
                  <a
                    href="/inventory"
                    className="text-sm text-blue-600 hover:underline flex items-center gap-1"
                  >
                    Ver todo <FiChevronRight className="w-4 h-4" />
                  </a>
                </div>
                <VehiclePerformanceTable vehicles={mockVehiclePerformance} />
              </div>
            </div>

            {/* Right Column - 1/3 width */}
            <div className="space-y-6">
              {/* Alerts */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <FiBell className="w-5 h-5 text-amber-500" />
                    Alertas
                  </h2>
                  {unreadAlerts > 0 && (
                    <span className="px-2 py-1 bg-red-100 text-red-700 text-xs font-bold rounded-full">
                      {unreadAlerts} nuevas
                    </span>
                  )}
                </div>
                <AlertsPanel alerts={alerts} onDismiss={handleDismissAlert} />
              </div>

              {/* Inventory Aging */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-semibold text-gray-900">Antigüedad de Inventario</h2>
                </div>
                <AgingChart buckets={mockAgingBuckets} />
                <div className="mt-4 grid grid-cols-2 gap-3">
                  <div className="bg-green-50 rounded-lg p-3">
                    <p className="text-xs text-green-600 font-medium">Fresco (0-30d)</p>
                    <p className="text-lg font-bold text-green-700">55%</p>
                  </div>
                  <div className="bg-red-50 rounded-lg p-3">
                    <p className="text-xs text-red-600 font-medium">En riesgo (60+d)</p>
                    <p className="text-lg font-bold text-red-700">11.7%</p>
                  </div>
                </div>
              </div>

              {/* Benchmark */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <FiAward className="w-5 h-5 text-purple-500" />
                    vs Mercado
                  </h2>
                  <span className="px-2 py-1 bg-purple-100 text-purple-700 text-xs font-bold rounded-full">
                    Top 25%
                  </span>
                </div>
                <BenchmarkComparison benchmarks={mockBenchmarks} />
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default AdvancedAnalyticsDashboard;
