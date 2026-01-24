import { useState, useCallback } from 'react';
import { Line, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler,
} from 'chart.js';
import {
  FiTarget,
  FiUsers,
  FiTrendingUp,
  FiTrendingDown,
  FiClock,
  FiChevronRight,
  FiRefreshCw,
  FiDownload,
  FiDollarSign,
  FiZap,
  FiThermometer,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler
);

// ============================================
// TYPES
// ============================================

interface FunnelStage {
  id: string;
  name: string;
  value: number;
  previousValue?: number;
  conversionRate: number;
  dropoffRate: number;
  color: string;
}

interface LeadSummary {
  totalLeads: number;
  newLeads: number;
  qualifiedLeads: number;
  hotLeads: number;
  warmLeads: number;
  coldLeads: number;
  convertedLeads: number;
  lostLeads: number;
  avgResponseTime: number;
  avgConversionTime: number;
  overallConversionRate: number;
}

interface LeadSource {
  name: string;
  count: number;
  percentage: number;
  conversionRate: number;
  color: string;
}

interface LeadTrend {
  date: string;
  newLeads: number;
  qualifiedLeads: number;
  convertedLeads: number;
}

interface TopPerformingVehicle {
  id: string;
  title: string;
  leads: number;
  conversions: number;
  conversionRate: number;
}

// ============================================
// MOCK DATA
// ============================================

const mockSummary: LeadSummary = {
  totalLeads: 842,
  newLeads: 156,
  qualifiedLeads: 312,
  hotLeads: 68,
  warmLeads: 145,
  coldLeads: 99,
  convertedLeads: 28,
  lostLeads: 45,
  avgResponseTime: 12,
  avgConversionTime: 72,
  overallConversionRate: 8.97,
};

const mockFunnelStages: FunnelStage[] = [
  {
    id: '1',
    name: 'Impresiones',
    value: 125000,
    previousValue: 112000,
    conversionRate: 100,
    dropoffRate: 0,
    color: '#E5E7EB',
  },
  {
    id: '2',
    name: 'Vistas',
    value: 24532,
    previousValue: 21890,
    conversionRate: 19.6,
    dropoffRate: 80.4,
    color: '#93C5FD',
  },
  {
    id: '3',
    name: 'Contactos',
    value: 842,
    previousValue: 756,
    conversionRate: 3.4,
    dropoffRate: 96.6,
    color: '#60A5FA',
  },
  {
    id: '4',
    name: 'Leads Calificados',
    value: 312,
    previousValue: 298,
    conversionRate: 37.1,
    dropoffRate: 62.9,
    color: '#3B82F6',
  },
  {
    id: '5',
    name: 'En Negociación',
    value: 68,
    previousValue: 58,
    conversionRate: 21.8,
    dropoffRate: 78.2,
    color: '#2563EB',
  },
  {
    id: '6',
    name: 'Ventas Cerradas',
    value: 28,
    previousValue: 24,
    conversionRate: 41.2,
    dropoffRate: 58.8,
    color: '#1D4ED8',
  },
];

const mockLeadSources: LeadSource[] = [
  { name: 'Búsqueda', count: 337, percentage: 40, conversionRate: 11.2, color: '#3B82F6' },
  { name: 'Directo', count: 211, percentage: 25, conversionRate: 9.5, color: '#10B981' },
  { name: 'Referido', count: 126, percentage: 15, conversionRate: 15.8, color: '#8B5CF6' },
  { name: 'Redes Sociales', count: 84, percentage: 10, conversionRate: 6.2, color: '#F59E0B' },
  { name: 'Email', count: 59, percentage: 7, conversionRate: 8.4, color: '#EC4899' },
  { name: 'Otro', count: 25, percentage: 3, conversionRate: 4.0, color: '#6B7280' },
];

const mockLeadTrends: LeadTrend[] = [
  { date: '2024-01-01', newLeads: 45, qualifiedLeads: 12, convertedLeads: 3 },
  { date: '2024-01-08', newLeads: 52, qualifiedLeads: 15, convertedLeads: 4 },
  { date: '2024-01-15', newLeads: 48, qualifiedLeads: 18, convertedLeads: 5 },
  { date: '2024-01-22', newLeads: 61, qualifiedLeads: 22, convertedLeads: 6 },
  { date: '2024-01-29', newLeads: 58, qualifiedLeads: 20, convertedLeads: 5 },
  { date: '2024-02-05', newLeads: 55, qualifiedLeads: 19, convertedLeads: 5 },
];

const mockTopVehicles: TopPerformingVehicle[] = [
  { id: '1', title: 'Toyota Camry 2023', leads: 45, conversions: 8, conversionRate: 17.8 },
  { id: '2', title: 'Honda Accord 2022', leads: 38, conversions: 6, conversionRate: 15.8 },
  { id: '3', title: 'BMW X5 2023', leads: 25, conversions: 3, conversionRate: 12.0 },
  { id: '4', title: 'Mercedes GLE 2022', leads: 22, conversions: 2, conversionRate: 9.1 },
  { id: '5', title: 'Hyundai Tucson 2023', leads: 35, conversions: 4, conversionRate: 11.4 },
];

// ============================================
// COMPONENTS
// ============================================

const StatCard = ({
  title,
  value,
  subtitle,
  icon,
  color = 'blue',
  trend,
  trendValue,
}: {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  color?: string;
  trend?: 'up' | 'down';
  trendValue?: number;
}) => {
  const colorClasses: Record<string, string> = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    amber: 'bg-amber-50 text-amber-600',
    red: 'bg-red-50 text-red-600',
    purple: 'bg-purple-50 text-purple-600',
    pink: 'bg-pink-50 text-pink-600',
    indigo: 'bg-indigo-50 text-indigo-600',
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
      <div className="flex items-center justify-between">
        <div className={`p-3 rounded-lg ${colorClasses[color]}`}>{icon}</div>
        {trend && trendValue !== undefined && (
          <div
            className={`flex items-center gap-1 text-sm font-medium ${
              trend === 'up' ? 'text-green-600' : 'text-red-600'
            }`}
          >
            {trend === 'up' ? <FiTrendingUp /> : <FiTrendingDown />}
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

const FunnelVisualization = ({ stages }: { stages: FunnelStage[] }) => {
  const maxValue = stages[0]?.value || 1;

  return (
    <div className="space-y-4">
      {stages.map((stage, index) => {
        const width = Math.max((stage.value / maxValue) * 100, 8);
        const change = stage.previousValue
          ? (((stage.value - stage.previousValue) / stage.previousValue) * 100).toFixed(1)
          : null;
        const isPositive = change && parseFloat(change) > 0;

        return (
          <div key={stage.id}>
            <div className="flex items-center justify-between mb-2">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-gray-700">{stage.name}</span>
                {change && (
                  <span
                    className={`text-xs font-medium ${isPositive ? 'text-green-600' : 'text-red-600'}`}
                  >
                    {isPositive ? '+' : ''}
                    {change}%
                  </span>
                )}
              </div>
              <div className="flex items-center gap-4">
                <span className="text-sm font-bold text-gray-900">
                  {stage.value.toLocaleString()}
                </span>
                {index > 0 && (
                  <span className="text-xs text-red-500 w-16 text-right">
                    -{stage.dropoffRate.toFixed(1)}%
                  </span>
                )}
              </div>
            </div>
            <div className="relative h-10 bg-gray-100 rounded-lg overflow-hidden">
              <div
                className="absolute left-0 top-0 h-full rounded-lg transition-all duration-700 flex items-center justify-end pr-3"
                style={{
                  width: `${width}%`,
                  backgroundColor: stage.color,
                }}
              >
                {index > 0 && (
                  <span className="text-xs text-white font-semibold">
                    {stage.conversionRate.toFixed(1)}%
                  </span>
                )}
              </div>
            </div>
          </div>
        );
      })}

      {/* Funnel Summary */}
      <div className="mt-6 pt-6 border-t border-gray-200">
        <div className="grid grid-cols-3 gap-4">
          <div className="text-center">
            <p className="text-2xl font-bold text-blue-600">
              {((stages[stages.length - 1].value / stages[0].value) * 100).toFixed(2)}%
            </p>
            <p className="text-xs text-gray-500">Conversión Total</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-green-600">{stages[stages.length - 1].value}</p>
            <p className="text-xs text-gray-500">Ventas Cerradas</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-amber-600">
              ${(stages[stages.length - 1].value * 28500).toLocaleString()}
            </p>
            <p className="text-xs text-gray-500">Revenue Generado</p>
          </div>
        </div>
      </div>
    </div>
  );
};

const LeadSourcesChart = ({ sources }: { sources: LeadSource[] }) => {
  const data = {
    labels: sources.map((s) => s.name),
    datasets: [
      {
        data: sources.map((s) => s.count),
        backgroundColor: sources.map((s) => s.color),
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
          padding: 12,
          font: { size: 11 },
        },
      },
    },
    cutout: '55%',
  };

  return (
    <div className="h-64">
      <Doughnut data={data} options={options} />
    </div>
  );
};

const LeadTrendChart = ({ trends }: { trends: LeadTrend[] }) => {
  const data = {
    labels: trends.map((t) => {
      const date = new Date(t.date);
      return date.toLocaleDateString('es-DO', { month: 'short', day: 'numeric' });
    }),
    datasets: [
      {
        label: 'Nuevos Leads',
        data: trends.map((t) => t.newLeads),
        borderColor: '#3B82F6',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        fill: true,
        tension: 0.4,
      },
      {
        label: 'Calificados',
        data: trends.map((t) => t.qualifiedLeads),
        borderColor: '#8B5CF6',
        backgroundColor: 'rgba(139, 92, 246, 0.1)',
        fill: true,
        tension: 0.4,
      },
      {
        label: 'Convertidos',
        data: trends.map((t) => t.convertedLeads),
        borderColor: '#10B981',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        fill: true,
        tension: 0.4,
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
        beginAtZero: true,
        grid: { color: 'rgba(0,0,0,0.05)' },
      },
      x: {
        grid: { display: false },
      },
    },
  };

  return (
    <div className="h-72">
      <Line data={data} options={options} />
    </div>
  );
};

const LeadTemperatureGauge = ({ hot, warm, cold }: { hot: number; warm: number; cold: number }) => {
  const total = hot + warm + cold;
  const hotPct = (hot / total) * 100;
  const warmPct = (warm / total) * 100;
  const coldPct = (cold / total) * 100;

  return (
    <div className="space-y-4">
      {/* Temperature Bar */}
      <div className="h-8 rounded-full overflow-hidden flex">
        <div
          className="bg-red-500 flex items-center justify-center"
          style={{ width: `${hotPct}%` }}
        >
          {hotPct > 15 && <span className="text-xs text-white font-bold">{hot}</span>}
        </div>
        <div
          className="bg-orange-400 flex items-center justify-center"
          style={{ width: `${warmPct}%` }}
        >
          {warmPct > 15 && <span className="text-xs text-white font-bold">{warm}</span>}
        </div>
        <div
          className="bg-blue-400 flex items-center justify-center"
          style={{ width: `${coldPct}%` }}
        >
          {coldPct > 15 && <span className="text-xs text-white font-bold">{cold}</span>}
        </div>
      </div>

      {/* Legend */}
      <div className="flex justify-between">
        <div className="flex items-center gap-2">
          <span className="w-3 h-3 bg-red-500 rounded-full"></span>
          <span className="text-sm text-gray-600">Hot ({hot})</span>
        </div>
        <div className="flex items-center gap-2">
          <span className="w-3 h-3 bg-orange-400 rounded-full"></span>
          <span className="text-sm text-gray-600">Warm ({warm})</span>
        </div>
        <div className="flex items-center gap-2">
          <span className="w-3 h-3 bg-blue-400 rounded-full"></span>
          <span className="text-sm text-gray-600">Cold ({cold})</span>
        </div>
      </div>
    </div>
  );
};

const LeadSourcesTable = ({ sources }: { sources: LeadSource[] }) => {
  return (
    <div className="space-y-3">
      {sources.map((source) => (
        <div
          key={source.name}
          className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
        >
          <div className="flex items-center gap-3">
            <span className="w-3 h-3 rounded-full" style={{ backgroundColor: source.color }} />
            <span className="text-sm font-medium text-gray-700">{source.name}</span>
          </div>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-500">{source.count} leads</span>
            <span className="text-sm font-bold text-gray-900">{source.conversionRate}%</span>
          </div>
        </div>
      ))}
    </div>
  );
};

const TopVehiclesTable = ({ vehicles }: { vehicles: TopPerformingVehicle[] }) => {
  return (
    <div className="overflow-x-auto">
      <table className="w-full">
        <thead>
          <tr className="text-left text-xs text-gray-500 uppercase tracking-wider">
            <th className="pb-3 font-medium">Vehículo</th>
            <th className="pb-3 font-medium text-center">Leads</th>
            <th className="pb-3 font-medium text-center">Conversiones</th>
            <th className="pb-3 font-medium text-center">Tasa</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {vehicles.map((vehicle, index) => (
            <tr key={vehicle.id} className="hover:bg-gray-50">
              <td className="py-3">
                <div className="flex items-center gap-2">
                  <span
                    className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                      index === 0
                        ? 'bg-yellow-100 text-yellow-700'
                        : index === 1
                          ? 'bg-gray-100 text-gray-700'
                          : index === 2
                            ? 'bg-orange-100 text-orange-700'
                            : 'bg-gray-50 text-gray-500'
                    }`}
                  >
                    {index + 1}
                  </span>
                  <span className="text-sm font-medium text-gray-900">{vehicle.title}</span>
                </div>
              </td>
              <td className="py-3 text-center">
                <span className="text-sm font-medium text-gray-700">{vehicle.leads}</span>
              </td>
              <td className="py-3 text-center">
                <span className="text-sm font-bold text-green-600">{vehicle.conversions}</span>
              </td>
              <td className="py-3 text-center">
                <span
                  className={`inline-flex px-2 py-1 rounded-full text-xs font-semibold ${
                    vehicle.conversionRate >= 15
                      ? 'bg-green-100 text-green-700'
                      : vehicle.conversionRate >= 10
                        ? 'bg-yellow-100 text-yellow-700'
                        : 'bg-red-100 text-red-700'
                  }`}
                >
                  {vehicle.conversionRate}%
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

// ============================================
// MAIN COMPONENT
// ============================================

interface LeadFunnelPageProps {
  dealerId?: string;
}

export const LeadFunnelPage = ({ dealerId = 'demo' }: LeadFunnelPageProps) => {
  const [isLoading, setIsLoading] = useState(false);
  const [dateRange, setDateRange] = useState('30');

  // Log dealerId for future API integration
  console.debug('Loading lead funnel for dealer:', dealerId);

  const handleRefresh = useCallback(() => {
    setIsLoading(true);
    setTimeout(() => setIsLoading(false), 1000);
  }, []);

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                  <FiTarget className="w-7 h-7 text-purple-600" />
                  Embudo de Leads
                </h1>
                <p className="text-sm text-gray-500 mt-1">
                  Análisis de conversión y rendimiento de leads
                </p>
              </div>

              <div className="flex items-center gap-3">
                <select
                  value={dateRange}
                  onChange={(e) => setDateRange(e.target.value)}
                  className="px-3 py-2 border border-gray-300 rounded-lg text-sm"
                >
                  <option value="7">Últimos 7 días</option>
                  <option value="30">Últimos 30 días</option>
                  <option value="90">Últimos 90 días</option>
                </select>

                <button
                  onClick={handleRefresh}
                  disabled={isLoading}
                  className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
                >
                  <FiRefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
                </button>

                <button className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700">
                  <FiDownload className="w-4 h-4" />
                  Exportar
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Stats Row */}
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4 mb-8">
            <StatCard
              title="Total Leads"
              value={mockSummary.totalLeads}
              icon={<FiUsers className="w-6 h-6" />}
              color="blue"
              trend="up"
              trendValue={11.4}
            />
            <StatCard
              title="Nuevos Esta Semana"
              value={mockSummary.newLeads}
              icon={<FiZap className="w-6 h-6" />}
              color="amber"
              trend="up"
              trendValue={8.2}
            />
            <StatCard
              title="Calificados"
              value={mockSummary.qualifiedLeads}
              icon={<FiTarget className="w-6 h-6" />}
              color="purple"
            />
            <StatCard
              title="Convertidos"
              value={mockSummary.convertedLeads}
              icon={<FiDollarSign className="w-6 h-6" />}
              color="green"
              trend="up"
              trendValue={16.7}
            />
            <StatCard
              title="Tiempo Respuesta"
              value={`${mockSummary.avgResponseTime}m`}
              subtitle="Promedio"
              icon={<FiClock className="w-6 h-6" />}
              color="indigo"
              trend="down"
              trendValue={25}
            />
            <StatCard
              title="Tasa Conversión"
              value={`${mockSummary.overallConversionRate}%`}
              icon={<FiTrendingUp className="w-6 h-6" />}
              color="pink"
              trend="up"
              trendValue={1.2}
            />
          </div>

          {/* Main Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
            {/* Funnel - 2 columns */}
            <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-6">Embudo de Conversión</h2>
              <FunnelVisualization stages={mockFunnelStages} />
            </div>

            {/* Lead Temperature */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-6 flex items-center gap-2">
                <FiThermometer className="w-5 h-5 text-red-500" />
                Temperatura de Leads
              </h2>
              <LeadTemperatureGauge
                hot={mockSummary.hotLeads}
                warm={mockSummary.warmLeads}
                cold={mockSummary.coldLeads}
              />

              <div className="mt-6 space-y-4">
                <div className="p-4 bg-red-50 rounded-lg">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-red-700">Hot Leads</span>
                    <span className="text-lg font-bold text-red-700">{mockSummary.hotLeads}</span>
                  </div>
                  <p className="text-xs text-red-600 mt-1">Alta probabilidad de compra inmediata</p>
                </div>

                <div className="p-4 bg-orange-50 rounded-lg">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-orange-700">Warm Leads</span>
                    <span className="text-lg font-bold text-orange-700">
                      {mockSummary.warmLeads}
                    </span>
                  </div>
                  <p className="text-xs text-orange-600 mt-1">Interesados, requieren seguimiento</p>
                </div>

                <div className="p-4 bg-blue-50 rounded-lg">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-blue-700">Cold Leads</span>
                    <span className="text-lg font-bold text-blue-700">{mockSummary.coldLeads}</span>
                  </div>
                  <p className="text-xs text-blue-600 mt-1">En etapa inicial de investigación</p>
                </div>
              </div>
            </div>
          </div>

          {/* Second Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            {/* Lead Trends */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-6">Tendencia de Leads</h2>
              <LeadTrendChart trends={mockLeadTrends} />
            </div>

            {/* Lead Sources */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-6">Fuentes de Leads</h2>
              <LeadSourcesChart sources={mockLeadSources} />
            </div>
          </div>

          {/* Third Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Source Performance */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-6">Rendimiento por Fuente</h2>
              <LeadSourcesTable sources={mockLeadSources} />
            </div>

            {/* Top Converting Vehicles */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-lg font-semibold text-gray-900">
                  Vehículos con Más Conversiones
                </h2>
                <a
                  href="/inventory"
                  className="text-sm text-purple-600 hover:underline flex items-center gap-1"
                >
                  Ver todos <FiChevronRight className="w-4 h-4" />
                </a>
              </div>
              <TopVehiclesTable vehicles={mockTopVehicles} />
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default LeadFunnelPage;
