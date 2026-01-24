import { useState, useCallback } from 'react';
import { Bar, Doughnut } from 'react-chartjs-2';
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
  FiPackage,
  FiClock,
  FiAlertTriangle,
  FiDollarSign,
  FiDownload,
  FiRefreshCw,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

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

interface InventoryStats {
  totalVehicles: number;
  activeVehicles: number;
  pausedVehicles: number;
  soldThisMonth: number;
  totalValue: number;
  avgPrice: number;
  avgDaysOnMarket: number;
  turnoverRate: number;
}

interface AgingBucket {
  label: string;
  minDays: number;
  maxDays: number;
  count: number;
  value: number;
  percentage: number;
  color: string;
  status: 'healthy' | 'warning' | 'critical';
}

interface VehicleAgingItem {
  id: string;
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  daysOnMarket: number;
  views: number;
  contacts: number;
  priceReduction: number;
  thumbnail: string;
  status: 'active' | 'paused';
}

interface TurnoverData {
  period: string;
  vehiclesSold: number;
  avgInventory: number;
  turnoverRate: number;
}

// ============================================
// MOCK DATA
// ============================================

const mockStats: InventoryStats = {
  totalVehicles: 60,
  activeVehicles: 52,
  pausedVehicles: 8,
  soldThisMonth: 12,
  totalValue: 1580000,
  avgPrice: 26333,
  avgDaysOnMarket: 35,
  turnoverRate: 0.2,
};

const mockAgingBuckets: AgingBucket[] = [
  {
    label: '0-15 días',
    minDays: 0,
    maxDays: 15,
    count: 18,
    value: 485000,
    percentage: 30.7,
    color: '#10B981',
    status: 'healthy',
  },
  {
    label: '16-30 días',
    minDays: 16,
    maxDays: 30,
    count: 15,
    value: 420000,
    percentage: 26.6,
    color: '#22C55E',
    status: 'healthy',
  },
  {
    label: '31-45 días',
    minDays: 31,
    maxDays: 45,
    count: 12,
    value: 310000,
    percentage: 19.6,
    color: '#FBBF24',
    status: 'warning',
  },
  {
    label: '46-60 días',
    minDays: 46,
    maxDays: 60,
    count: 8,
    value: 195000,
    percentage: 12.3,
    color: '#F97316',
    status: 'warning',
  },
  {
    label: '61-90 días',
    minDays: 61,
    maxDays: 90,
    count: 5,
    value: 125000,
    percentage: 7.9,
    color: '#EF4444',
    status: 'critical',
  },
  {
    label: '90+ días',
    minDays: 91,
    maxDays: 9999,
    count: 2,
    value: 45000,
    percentage: 2.8,
    color: '#DC2626',
    status: 'critical',
  },
];

const mockAgingVehicles: VehicleAgingItem[] = [
  {
    id: '1',
    title: 'Honda Civic 2020',
    make: 'Honda',
    model: 'Civic',
    year: 2020,
    price: 22000,
    daysOnMarket: 95,
    views: 120,
    contacts: 3,
    priceReduction: 0,
    thumbnail: '',
    status: 'active',
  },
  {
    id: '2',
    title: 'Toyota Corolla 2019',
    make: 'Toyota',
    model: 'Corolla',
    year: 2019,
    price: 18500,
    daysOnMarket: 82,
    views: 89,
    contacts: 2,
    priceReduction: 5,
    thumbnail: '',
    status: 'active',
  },
  {
    id: '3',
    title: 'Nissan Sentra 2020',
    make: 'Nissan',
    model: 'Sentra',
    year: 2020,
    price: 19000,
    daysOnMarket: 75,
    views: 95,
    contacts: 4,
    priceReduction: 0,
    thumbnail: '',
    status: 'active',
  },
  {
    id: '4',
    title: 'Mazda 3 2021',
    make: 'Mazda',
    model: '3',
    year: 2021,
    price: 24500,
    daysOnMarket: 68,
    views: 145,
    contacts: 6,
    priceReduction: 3,
    thumbnail: '',
    status: 'active',
  },
  {
    id: '5',
    title: 'Hyundai Elantra 2020',
    make: 'Hyundai',
    model: 'Elantra',
    year: 2020,
    price: 20000,
    daysOnMarket: 62,
    views: 78,
    contacts: 2,
    priceReduction: 0,
    thumbnail: '',
    status: 'paused',
  },
];

const mockTurnoverData: TurnoverData[] = [
  { period: 'Ene', vehiclesSold: 8, avgInventory: 55, turnoverRate: 0.145 },
  { period: 'Feb', vehiclesSold: 10, avgInventory: 58, turnoverRate: 0.172 },
  { period: 'Mar', vehiclesSold: 12, avgInventory: 60, turnoverRate: 0.2 },
  { period: 'Abr', vehiclesSold: 9, avgInventory: 57, turnoverRate: 0.158 },
  { period: 'May', vehiclesSold: 14, avgInventory: 62, turnoverRate: 0.226 },
  { period: 'Jun', vehiclesSold: 11, avgInventory: 59, turnoverRate: 0.186 },
];

// ============================================
// COMPONENTS
// ============================================

const StatCard = ({
  title,
  value,
  subtitle,
  icon,
  trend,
  trendValue,
  color = 'blue',
}: {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  trend?: 'up' | 'down' | 'neutral';
  trendValue?: number;
  color?: string;
}) => {
  const colorClasses: Record<string, { bg: string; text: string; iconBg: string }> = {
    blue: { bg: 'bg-blue-50', text: 'text-blue-600', iconBg: 'bg-blue-100' },
    green: { bg: 'bg-green-50', text: 'text-green-600', iconBg: 'bg-green-100' },
    amber: { bg: 'bg-amber-50', text: 'text-amber-600', iconBg: 'bg-amber-100' },
    red: { bg: 'bg-red-50', text: 'text-red-600', iconBg: 'bg-red-100' },
    purple: { bg: 'bg-purple-50', text: 'text-purple-600', iconBg: 'bg-purple-100' },
  };

  const colors = colorClasses[color] || colorClasses.blue;

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
      <div className="flex items-center justify-between">
        <div className={`p-3 rounded-lg ${colors.iconBg} ${colors.text}`}>{icon}</div>
        {trend && trendValue !== undefined && (
          <div
            className={`flex items-center gap-1 text-sm font-medium ${
              trend === 'up'
                ? 'text-green-600'
                : trend === 'down'
                  ? 'text-red-600'
                  : 'text-gray-500'
            }`}
          >
            {trend === 'up' ? <FiTrendingUp /> : trend === 'down' ? <FiTrendingDown /> : null}
            {Math.abs(trendValue)}%
          </div>
        )}
      </div>
      <div className="mt-4">
        <p className="text-2xl font-bold text-gray-900">{value}</p>
        <p className="text-sm text-gray-500 mt-1">{title}</p>
        {subtitle && <p className="text-xs text-gray-400 mt-0.5">{subtitle}</p>}
      </div>
    </div>
  );
};

const AgingDistributionChart = ({ buckets }: { buckets: AgingBucket[] }) => {
  const barData = {
    labels: buckets.map((b) => b.label),
    datasets: [
      {
        label: 'Vehículos',
        data: buckets.map((b) => b.count),
        backgroundColor: buckets.map((b) => b.color),
        borderRadius: 6,
      },
    ],
  };

  const barOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          afterLabel: (context: { dataIndex: number }) => {
            const bucket = buckets[context.dataIndex];
            return `Valor: $${bucket.value.toLocaleString()}`;
          },
        },
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        grid: { color: 'rgba(0,0,0,0.05)' },
      },
      x: {
        grid: { display: false },
      },
    },
  };

  return (
    <div className="h-64">
      <Bar data={barData} options={barOptions} />
    </div>
  );
};

const AgingValueChart = ({ buckets }: { buckets: AgingBucket[] }) => {
  const data = {
    labels: buckets.map((b) => b.label),
    datasets: [
      {
        data: buckets.map((b) => b.value),
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
        position: 'bottom' as const,
        labels: {
          usePointStyle: true,
          padding: 15,
          font: { size: 11 },
        },
      },
      tooltip: {
        callbacks: {
          label: (context: { parsed: number; label: string }) => {
            return `${context.label}: $${context.parsed.toLocaleString()}`;
          },
        },
      },
    },
    cutout: '60%',
  };

  return (
    <div className="h-64">
      <Doughnut data={data} options={options} />
    </div>
  );
};

const TurnoverChart = ({ data }: { data: TurnoverData[] }) => {
  const chartData = {
    labels: data.map((d) => d.period),
    datasets: [
      {
        label: 'Tasa de Rotación (%)',
        data: data.map((d) => d.turnoverRate * 100),
        backgroundColor: 'rgba(59, 130, 246, 0.8)',
        borderColor: '#3B82F6',
        borderWidth: 1,
        yAxisID: 'y',
        barThickness: 25,
      },
      {
        label: 'Ventas',
        data: data.map((d) => d.vehiclesSold),
        backgroundColor: 'rgba(16, 185, 129, 0.8)',
        borderColor: '#10B981',
        borderWidth: 1,
        yAxisID: 'y1',
        barThickness: 25,
      },
    ],
  };

  const options = {
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
        title: {
          display: true,
          text: 'Rotación %',
        },
      },
      y1: {
        type: 'linear' as const,
        display: true,
        position: 'right' as const,
        title: {
          display: true,
          text: 'Ventas',
        },
        grid: {
          drawOnChartArea: false,
        },
      },
    },
  };

  return (
    <div className="h-72">
      <Bar data={chartData} options={options} />
    </div>
  );
};

const AgingVehiclesList = ({ vehicles }: { vehicles: VehicleAgingItem[] }) => {
  const getDaysColor = (days: number) => {
    if (days > 90) return 'text-red-600 bg-red-50';
    if (days > 60) return 'text-orange-600 bg-orange-50';
    if (days > 45) return 'text-yellow-600 bg-yellow-50';
    return 'text-gray-600 bg-gray-50';
  };

  return (
    <div className="overflow-x-auto">
      <table className="w-full">
        <thead>
          <tr className="text-left text-xs text-gray-500 uppercase tracking-wider border-b border-gray-100">
            <th className="pb-3 font-medium">Vehículo</th>
            <th className="pb-3 font-medium text-center">Días</th>
            <th className="pb-3 font-medium text-right">Precio</th>
            <th className="pb-3 font-medium text-center">Vistas</th>
            <th className="pb-3 font-medium text-center">Contactos</th>
            <th className="pb-3 font-medium text-center">Reducción</th>
            <th className="pb-3 font-medium text-center">Acción</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {vehicles.map((vehicle) => (
            <tr key={vehicle.id} className="hover:bg-gray-50">
              <td className="py-4">
                <div className="flex items-center gap-3">
                  <div className="w-16 h-12 bg-gray-200 rounded-lg overflow-hidden flex-shrink-0">
                    {vehicle.thumbnail ? (
                      <img
                        src={vehicle.thumbnail}
                        alt={vehicle.title}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-gray-400">
                        <FiPackage />
                      </div>
                    )}
                  </div>
                  <div>
                    <p className="font-medium text-gray-900 text-sm">{vehicle.title}</p>
                    <p className="text-xs text-gray-500">
                      {vehicle.make} {vehicle.model}
                    </p>
                  </div>
                </div>
              </td>
              <td className="py-4 text-center">
                <span
                  className={`inline-flex px-2 py-1 rounded-full text-xs font-bold ${getDaysColor(vehicle.daysOnMarket)}`}
                >
                  {vehicle.daysOnMarket}d
                </span>
              </td>
              <td className="py-4 text-right">
                <span className="text-sm font-medium text-gray-900">
                  ${vehicle.price.toLocaleString()}
                </span>
              </td>
              <td className="py-4 text-center">
                <span className="text-sm text-gray-600">{vehicle.views}</span>
              </td>
              <td className="py-4 text-center">
                <span className="text-sm text-gray-600">{vehicle.contacts}</span>
              </td>
              <td className="py-4 text-center">
                {vehicle.priceReduction > 0 ? (
                  <span className="text-sm text-green-600 font-medium">
                    -{vehicle.priceReduction}%
                  </span>
                ) : (
                  <span className="text-sm text-gray-400">—</span>
                )}
              </td>
              <td className="py-4 text-center">
                <button className="text-blue-600 hover:text-blue-700 text-sm font-medium">
                  Gestionar
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

const HealthIndicator = ({
  label,
  value,
  target,
  unit = '%',
  isInverse = false,
}: {
  label: string;
  value: number;
  target: number;
  unit?: string;
  isInverse?: boolean;
}) => {
  const percentage = Math.min((value / target) * 100, 100);
  const isHealthy = isInverse ? value <= target : value >= target;

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <span className="text-sm text-gray-600">{label}</span>
        <span className={`text-sm font-bold ${isHealthy ? 'text-green-600' : 'text-red-600'}`}>
          {value}
          {unit}
        </span>
      </div>
      <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
        <div
          className={`h-full rounded-full transition-all duration-500 ${
            isHealthy ? 'bg-green-500' : 'bg-red-500'
          }`}
          style={{ width: `${percentage}%` }}
        />
      </div>
      <p className="text-xs text-gray-400">
        Meta: {target}
        {unit}
      </p>
    </div>
  );
};

// ============================================
// MAIN COMPONENT
// ============================================

interface InventoryAnalyticsPageProps {
  dealerId?: string;
}

export const InventoryAnalyticsPage = ({ dealerId = 'demo' }: InventoryAnalyticsPageProps) => {
  const [isLoading, setIsLoading] = useState(false);
  const [dateRange, setDateRange] = useState('30');
  const [showAtRiskOnly, setShowAtRiskOnly] = useState(false);

  // Log dealerId for future API integration
  console.debug('Loading inventory analytics for dealer:', dealerId);

  const handleRefresh = useCallback(() => {
    setIsLoading(true);
    setTimeout(() => setIsLoading(false), 1000);
  }, []);

  const filteredVehicles = showAtRiskOnly
    ? mockAgingVehicles.filter((v) => v.daysOnMarket > 60)
    : mockAgingVehicles;

  const atRiskCount = mockAgingBuckets
    .filter((b) => b.status === 'critical')
    .reduce((acc, b) => acc + b.count, 0);

  const atRiskValue = mockAgingBuckets
    .filter((b) => b.status === 'critical')
    .reduce((acc, b) => acc + b.value, 0);

  const freshPercentage = mockAgingBuckets
    .filter((b) => b.status === 'healthy')
    .reduce((acc, b) => acc + b.percentage, 0);

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                  <FiPackage className="w-7 h-7 text-blue-600" />
                  Análisis de Inventario
                </h1>
                <p className="text-sm text-gray-500 mt-1">
                  Antigüedad, rotación y rendimiento del inventario
                </p>
              </div>

              <div className="flex items-center gap-3">
                <select
                  value={dateRange}
                  onChange={(e) => setDateRange(e.target.value)}
                  className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="30">Últimos 30 días</option>
                  <option value="60">Últimos 60 días</option>
                  <option value="90">Últimos 90 días</option>
                </select>

                <button
                  onClick={handleRefresh}
                  disabled={isLoading}
                  className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
                >
                  <FiRefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
                </button>

                <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
                  <FiDownload className="w-4 h-4" />
                  Exportar
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Stats Cards */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
            <StatCard
              title="Total Vehículos"
              value={mockStats.totalVehicles}
              subtitle={`${mockStats.activeVehicles} activos, ${mockStats.pausedVehicles} pausados`}
              icon={<FiPackage className="w-6 h-6" />}
              color="blue"
            />
            <StatCard
              title="Valor Total"
              value={`$${(mockStats.totalValue / 1000000).toFixed(2)}M`}
              subtitle={`Promedio $${mockStats.avgPrice.toLocaleString()}`}
              icon={<FiDollarSign className="w-6 h-6" />}
              color="green"
            />
            <StatCard
              title="Días Promedio"
              value={mockStats.avgDaysOnMarket}
              subtitle="Tiempo en mercado"
              icon={<FiClock className="w-6 h-6" />}
              trend="down"
              trendValue={8}
              color="amber"
            />
            <StatCard
              title="Rotación"
              value={`${(mockStats.turnoverRate * 100).toFixed(1)}%`}
              subtitle="Tasa mensual"
              icon={<FiTrendingUp className="w-6 h-6" />}
              trend="up"
              trendValue={12}
              color="purple"
            />
          </div>

          {/* Alert Banner */}
          {atRiskCount > 0 && (
            <div className="mb-8 p-4 bg-red-50 border border-red-200 rounded-xl flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-red-100 rounded-lg">
                  <FiAlertTriangle className="w-6 h-6 text-red-600" />
                </div>
                <div>
                  <p className="font-medium text-red-800">
                    {atRiskCount} vehículos en riesgo (60+ días)
                  </p>
                  <p className="text-sm text-red-600">
                    Valor en riesgo: ${atRiskValue.toLocaleString()} — Considera reducir precios o
                    promocionar
                  </p>
                </div>
              </div>
              <button
                onClick={() => setShowAtRiskOnly(true)}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 text-sm font-medium"
              >
                Ver Detalles
              </button>
            </div>
          )}

          {/* Main Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            {/* Aging Distribution */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-6">
                Distribución por Antigüedad
              </h2>
              <AgingDistributionChart buckets={mockAgingBuckets} />
              <div className="mt-6 grid grid-cols-3 gap-4">
                <div className="text-center p-3 bg-green-50 rounded-lg">
                  <p className="text-2xl font-bold text-green-700">{freshPercentage.toFixed(1)}%</p>
                  <p className="text-xs text-green-600">Inventario Fresco</p>
                </div>
                <div className="text-center p-3 bg-yellow-50 rounded-lg">
                  <p className="text-2xl font-bold text-yellow-700">
                    {mockAgingBuckets
                      .filter((b) => b.status === 'warning')
                      .reduce((a, b) => a + b.count, 0)}
                  </p>
                  <p className="text-xs text-yellow-600">En Observación</p>
                </div>
                <div className="text-center p-3 bg-red-50 rounded-lg">
                  <p className="text-2xl font-bold text-red-700">{atRiskCount}</p>
                  <p className="text-xs text-red-600">En Riesgo</p>
                </div>
              </div>
            </div>

            {/* Value Distribution */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-6">Valor por Antigüedad</h2>
              <AgingValueChart buckets={mockAgingBuckets} />
              <div className="mt-6 space-y-3">
                {mockAgingBuckets.slice(0, 3).map((bucket) => (
                  <div key={bucket.label} className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <span
                        className="w-3 h-3 rounded-full"
                        style={{ backgroundColor: bucket.color }}
                      />
                      <span className="text-sm text-gray-600">{bucket.label}</span>
                    </div>
                    <span className="text-sm font-medium text-gray-900">
                      ${bucket.value.toLocaleString()}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Turnover Chart */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-8">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-lg font-semibold text-gray-900">Rotación de Inventario</h2>
              <div className="flex items-center gap-4">
                <span className="flex items-center gap-2 text-sm text-blue-600">
                  <span className="w-3 h-0.5 bg-blue-500"></span>
                  Tasa de Rotación
                </span>
                <span className="flex items-center gap-2 text-sm text-green-600">
                  <span className="w-3 h-3 bg-green-500 rounded"></span>
                  Ventas
                </span>
              </div>
            </div>
            <TurnoverChart data={mockTurnoverData} />
          </div>

          {/* Health Metrics */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-8">
            <h2 className="text-lg font-semibold text-gray-900 mb-6">Indicadores de Salud</h2>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
              <HealthIndicator
                label="Inventario Fresco (0-30d)"
                value={freshPercentage}
                target={60}
              />
              <HealthIndicator
                label="Inventario en Riesgo (60+d)"
                value={(atRiskCount / mockStats.totalVehicles) * 100}
                target={15}
                isInverse={true}
              />
              <HealthIndicator
                label="Días Promedio"
                value={mockStats.avgDaysOnMarket}
                target={45}
                unit=" días"
                isInverse={true}
              />
              <HealthIndicator
                label="Tasa de Rotación"
                value={mockStats.turnoverRate * 100}
                target={20}
              />
            </div>
          </div>

          {/* At-Risk Vehicles Table */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-lg font-semibold text-gray-900">
                Vehículos con Mayor Antigüedad
              </h2>
              <div className="flex items-center gap-3">
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={showAtRiskOnly}
                    onChange={(e) => setShowAtRiskOnly(e.target.checked)}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  Solo en riesgo (60+ días)
                </label>
              </div>
            </div>
            <AgingVehiclesList vehicles={filteredVehicles} />
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default InventoryAnalyticsPage;
