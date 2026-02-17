import { useState, useCallback } from 'react';
import { Line } from 'react-chartjs-2';
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
  Filler,
} from 'chart.js';
import {
  FiFileText,
  FiDownload,
  FiCalendar,
  FiRefreshCw,
  FiMail,
  FiPrinter,
  FiClock,
  FiTrendingUp,
  FiTrendingDown,
  FiDollarSign,
  FiEye,
  FiUsers,
  FiPackage,
  FiCheckCircle,
  FiTarget,
  FiShare2,
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
  Filler
);

// ============================================
// TYPES
// ============================================

type ReportPeriod = 'daily' | 'weekly' | 'monthly' | 'custom';
type ExportFormat = 'pdf' | 'excel' | 'csv';

interface ReportMetric {
  label: string;
  current: number;
  previous: number;
  change: number;
  changePercent: number;
  trend: 'up' | 'down' | 'stable';
  format: 'number' | 'currency' | 'percentage' | 'time';
}

interface ReportData {
  period: string;
  dateRange: { start: string; end: string };
  generatedAt: string;
  metrics: {
    inventory: ReportMetric[];
    engagement: ReportMetric[];
    leads: ReportMetric[];
    revenue: ReportMetric[];
  };
  charts: {
    viewsTrend: { labels: string[]; values: number[] };
    leadsTrend: { labels: string[]; values: number[] };
    conversionsTrend: { labels: string[]; values: number[] };
  };
  highlights: string[];
  recommendations: string[];
}

// ============================================
// MOCK DATA
// ============================================

const mockReportData: ReportData = {
  period: 'weekly',
  dateRange: { start: '2024-01-22', end: '2024-01-28' },
  generatedAt: new Date().toISOString(),
  metrics: {
    inventory: [
      {
        label: 'Vehículos Activos',
        current: 45,
        previous: 42,
        change: 3,
        changePercent: 7.1,
        trend: 'up',
        format: 'number',
      },
      {
        label: 'Nuevos Listados',
        current: 8,
        previous: 5,
        change: 3,
        changePercent: 60,
        trend: 'up',
        format: 'number',
      },
      {
        label: 'Vendidos',
        current: 5,
        previous: 4,
        change: 1,
        changePercent: 25,
        trend: 'up',
        format: 'number',
      },
      {
        label: 'Días Promedio',
        current: 28,
        previous: 32,
        change: -4,
        changePercent: -12.5,
        trend: 'down',
        format: 'number',
      },
    ],
    engagement: [
      {
        label: 'Vistas Totales',
        current: 12450,
        previous: 10890,
        change: 1560,
        changePercent: 14.3,
        trend: 'up',
        format: 'number',
      },
      {
        label: 'CTR',
        current: 3.8,
        previous: 3.2,
        change: 0.6,
        changePercent: 18.8,
        trend: 'up',
        format: 'percentage',
      },
      {
        label: 'Favoritos',
        current: 234,
        previous: 198,
        change: 36,
        changePercent: 18.2,
        trend: 'up',
        format: 'number',
      },
      {
        label: 'Tiempo en Página',
        current: 4.2,
        previous: 3.8,
        change: 0.4,
        changePercent: 10.5,
        trend: 'up',
        format: 'time',
      },
    ],
    leads: [
      {
        label: 'Nuevos Leads',
        current: 68,
        previous: 52,
        change: 16,
        changePercent: 30.8,
        trend: 'up',
        format: 'number',
      },
      {
        label: 'Calificados',
        current: 28,
        previous: 24,
        change: 4,
        changePercent: 16.7,
        trend: 'up',
        format: 'number',
      },
      {
        label: 'Tasa Conversión',
        current: 8.8,
        previous: 7.2,
        change: 1.6,
        changePercent: 22.2,
        trend: 'up',
        format: 'percentage',
      },
      {
        label: 'Tiempo Respuesta',
        current: 12,
        previous: 18,
        change: -6,
        changePercent: -33.3,
        trend: 'down',
        format: 'time',
      },
    ],
    revenue: [
      {
        label: 'Ventas Cerradas',
        current: 142500,
        previous: 118000,
        change: 24500,
        changePercent: 20.8,
        trend: 'up',
        format: 'currency',
      },
      {
        label: 'Ticket Promedio',
        current: 28500,
        previous: 29500,
        change: -1000,
        changePercent: -3.4,
        trend: 'down',
        format: 'currency',
      },
      {
        label: 'Pipeline',
        current: 385000,
        previous: 342000,
        change: 43000,
        changePercent: 12.6,
        trend: 'up',
        format: 'currency',
      },
      {
        label: 'Forecast Mes',
        current: 580000,
        previous: 520000,
        change: 60000,
        changePercent: 11.5,
        trend: 'up',
        format: 'currency',
      },
    ],
  },
  charts: {
    viewsTrend: {
      labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
      values: [1680, 1920, 1780, 1890, 2100, 1650, 1430],
    },
    leadsTrend: {
      labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
      values: [8, 12, 9, 11, 15, 8, 5],
    },
    conversionsTrend: {
      labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
      values: [1, 1, 0, 2, 1, 0, 0],
    },
  },
  highlights: [
    'Las vistas aumentaron 14.3% comparado con la semana anterior',
    'El tiempo de respuesta a leads mejoró de 18 min a 12 min',
    'Se vendieron 5 vehículos, superando el promedio de 4/semana',
    'El CTR alcanzó 3.8%, por encima del promedio del mercado (3.2%)',
  ],
  recommendations: [
    'Considerar reducir precio del BMW X5 2022 que lleva 45 días sin contactos',
    'Responder más rápido a leads del fin de semana (tiempo actual: 28 min)',
    'Agregar más fotos al Honda Accord 2023 - tiene muchas vistas pero pocos contactos',
    'Aprovechar alta demanda de SUVs para expandir inventario en esa categoría',
  ],
};

// ============================================
// HELPER FUNCTIONS
// ============================================

const formatValue = (value: number, format: string) => {
  switch (format) {
    case 'currency':
      return `$${value.toLocaleString()}`;
    case 'percentage':
      return `${value}%`;
    case 'time':
      return `${value} min`;
    default:
      return value.toLocaleString();
  }
};

// ============================================
// COMPONENTS
// ============================================

const MetricCard = ({ metric }: { metric: ReportMetric }) => {
  // For metrics where "down" is good (like time)
  const isGood =
    metric.label.includes('Tiempo') || metric.label.includes('Días')
      ? metric.changePercent < 0
      : metric.changePercent > 0;

  return (
    <div className="bg-gray-50 rounded-lg p-4">
      <p className="text-sm text-gray-500 mb-1">{metric.label}</p>
      <p className="text-xl font-bold text-gray-900">
        {formatValue(metric.current, metric.format)}
      </p>
      <div
        className={`flex items-center gap-1 mt-1 text-sm font-medium ${
          isGood ? 'text-green-600' : 'text-red-600'
        }`}
      >
        {isGood ? <FiTrendingUp className="w-4 h-4" /> : <FiTrendingDown className="w-4 h-4" />}
        {Math.abs(metric.changePercent).toFixed(1)}%
        <span className="text-gray-400 font-normal ml-1">vs anterior</span>
      </div>
    </div>
  );
};

const TrendChart = ({
  title,
  data,
  color,
}: {
  title: string;
  data: { labels: string[]; values: number[] };
  color: string;
}) => {
  const chartData = {
    labels: data.labels,
    datasets: [
      {
        data: data.values,
        borderColor: color,
        backgroundColor: color + '20',
        fill: true,
        tension: 0.4,
        pointRadius: 4,
        pointBackgroundColor: color,
      },
    ],
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
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
    <div className="bg-white rounded-xl border border-gray-200 p-6">
      <h3 className="text-sm font-semibold text-gray-900 mb-4">{title}</h3>
      <div className="h-48">
        <Line data={chartData} options={options} />
      </div>
    </div>
  );
};

const ScheduledReportCard = ({
  type,
  nextRun,
  enabled,
  onToggle,
}: {
  type: string;
  nextRun: string;
  enabled: boolean;
  onToggle: () => void;
}) => {
  return (
    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
      <div className="flex items-center gap-3">
        <div
          className={`p-2 rounded-lg ${enabled ? 'bg-blue-100 text-blue-600' : 'bg-gray-200 text-gray-400'}`}
        >
          <FiCalendar className="w-5 h-5" />
        </div>
        <div>
          <p className="font-medium text-gray-900">{type}</p>
          <p className="text-xs text-gray-500">
            Próximo:{' '}
            {new Date(nextRun).toLocaleDateString('es-DO', {
              weekday: 'short',
              month: 'short',
              day: 'numeric',
            })}
          </p>
        </div>
      </div>
      <button
        onClick={onToggle}
        className={`relative w-12 h-6 rounded-full transition-colors ${
          enabled ? 'bg-blue-600' : 'bg-gray-300'
        }`}
      >
        <span
          className={`absolute top-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform ${
            enabled ? 'translate-x-6' : 'translate-x-0.5'
          }`}
        />
      </button>
    </div>
  );
};

// ============================================
// MAIN COMPONENT
// ============================================

interface ReportsPageProps {
  dealerId?: string;
}

export const ReportsPage = ({ dealerId = 'demo' }: ReportsPageProps) => {
  const [selectedPeriod, setSelectedPeriod] = useState<ReportPeriod>('weekly');
  const [isLoading, setIsLoading] = useState(false);
  const [isExporting, setIsExporting] = useState(false);

  // Log dealerId for future API integration
  console.debug('Loading reports for dealer:', dealerId);
  const [scheduledReports, setScheduledReports] = useState({
    daily: { enabled: false, nextRun: new Date(Date.now() + 86400000).toISOString() },
    weekly: { enabled: true, nextRun: new Date(Date.now() + 604800000).toISOString() },
    monthly: { enabled: true, nextRun: new Date(Date.now() + 2592000000).toISOString() },
  });

  const handleRefresh = useCallback(() => {
    setIsLoading(true);
    setTimeout(() => setIsLoading(false), 1500);
  }, []);

  const handleExport = useCallback((format: ExportFormat) => {
    setIsExporting(true);
    setTimeout(() => {
      setIsExporting(false);
      alert(`Reporte exportado en formato ${format.toUpperCase()}`);
    }, 2000);
  }, []);

  const handleToggleSchedule = useCallback((type: keyof typeof scheduledReports) => {
    setScheduledReports((prev) => ({
      ...prev,
      [type]: { ...prev[type], enabled: !prev[type].enabled },
    }));
  }, []);

  const periods: { id: ReportPeriod; label: string }[] = [
    { id: 'daily', label: 'Diario' },
    { id: 'weekly', label: 'Semanal' },
    { id: 'monthly', label: 'Mensual' },
    { id: 'custom', label: 'Personalizado' },
  ];

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                  <FiFileText className="w-7 h-7 text-green-600" />
                  Reportes
                </h1>
                <p className="text-sm text-gray-500 mt-1">
                  Informes detallados de rendimiento y métricas
                </p>
              </div>

              <div className="flex items-center gap-3">
                <button
                  onClick={handleRefresh}
                  disabled={isLoading}
                  className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
                >
                  <FiRefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
                </button>

                <div className="relative">
                  <button
                    onClick={() => handleExport('pdf')}
                    disabled={isExporting}
                    className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
                  >
                    <FiDownload className="w-4 h-4" />
                    {isExporting ? 'Exportando...' : 'Exportar'}
                  </button>
                </div>
              </div>
            </div>

            {/* Period Tabs */}
            <div className="flex gap-2 mt-6">
              {periods.map((period) => (
                <button
                  key={period.id}
                  onClick={() => setSelectedPeriod(period.id)}
                  className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                    selectedPeriod === period.id
                      ? 'bg-green-100 text-green-700'
                      : 'text-gray-500 hover:bg-gray-100'
                  }`}
                >
                  {period.label}
                </button>
              ))}
            </div>
          </div>
        </div>

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Report Header */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-8">
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
              <div>
                <h2 className="text-xl font-bold text-gray-900">
                  Reporte{' '}
                  {selectedPeriod === 'weekly'
                    ? 'Semanal'
                    : selectedPeriod === 'daily'
                      ? 'Diario'
                      : 'Mensual'}
                </h2>
                <p className="text-sm text-gray-500 flex items-center gap-2 mt-1">
                  <FiCalendar className="w-4 h-4" />
                  {new Date(mockReportData.dateRange.start).toLocaleDateString('es-DO', {
                    weekday: 'long',
                    month: 'long',
                    day: 'numeric',
                  })}{' '}
                  -{' '}
                  {new Date(mockReportData.dateRange.end).toLocaleDateString('es-DO', {
                    month: 'long',
                    day: 'numeric',
                    year: 'numeric',
                  })}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <button className="flex items-center gap-2 px-3 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-lg">
                  <FiMail className="w-4 h-4" />
                  Enviar
                </button>
                <button className="flex items-center gap-2 px-3 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-lg">
                  <FiPrinter className="w-4 h-4" />
                  Imprimir
                </button>
                <button className="flex items-center gap-2 px-3 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-lg">
                  <FiShare2 className="w-4 h-4" />
                  Compartir
                </button>
              </div>
            </div>
          </div>

          {/* Metrics Sections */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            {/* Inventory Metrics */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <FiPackage className="w-5 h-5 text-blue-600" />
                Inventario
              </h3>
              <div className="grid grid-cols-2 gap-3">
                {mockReportData.metrics.inventory.map((metric) => (
                  <MetricCard key={metric.label} metric={metric} />
                ))}
              </div>
            </div>

            {/* Engagement Metrics */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <FiEye className="w-5 h-5 text-purple-600" />
                Engagement
              </h3>
              <div className="grid grid-cols-2 gap-3">
                {mockReportData.metrics.engagement.map((metric) => (
                  <MetricCard key={metric.label} metric={metric} />
                ))}
              </div>
            </div>

            {/* Leads Metrics */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <FiUsers className="w-5 h-5 text-amber-600" />
                Leads
              </h3>
              <div className="grid grid-cols-2 gap-3">
                {mockReportData.metrics.leads.map((metric) => (
                  <MetricCard key={metric.label} metric={metric} />
                ))}
              </div>
            </div>

            {/* Revenue Metrics */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <FiDollarSign className="w-5 h-5 text-green-600" />
                Revenue
              </h3>
              <div className="grid grid-cols-2 gap-3">
                {mockReportData.metrics.revenue.map((metric) => (
                  <MetricCard key={metric.label} metric={metric} />
                ))}
              </div>
            </div>
          </div>

          {/* Trend Charts */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <TrendChart
              title="Vistas por Día"
              data={mockReportData.charts.viewsTrend}
              color="#8B5CF6"
            />
            <TrendChart
              title="Leads por Día"
              data={mockReportData.charts.leadsTrend}
              color="#F59E0B"
            />
            <TrendChart
              title="Conversiones por Día"
              data={mockReportData.charts.conversionsTrend}
              color="#10B981"
            />
          </div>

          {/* Highlights & Recommendations */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            {/* Highlights */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <FiCheckCircle className="w-5 h-5 text-green-600" />
                Logros de la Semana
              </h3>
              <ul className="space-y-3">
                {mockReportData.highlights.map((highlight, index) => (
                  <li key={index} className="flex items-start gap-3">
                    <span className="w-6 h-6 bg-green-100 text-green-600 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
                      <FiTrendingUp className="w-3.5 h-3.5" />
                    </span>
                    <span className="text-sm text-gray-700">{highlight}</span>
                  </li>
                ))}
              </ul>
            </div>

            {/* Recommendations */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <FiTarget className="w-5 h-5 text-amber-600" />
                Recomendaciones
              </h3>
              <ul className="space-y-3">
                {mockReportData.recommendations.map((rec, index) => (
                  <li key={index} className="flex items-start gap-3">
                    <span className="w-6 h-6 bg-amber-100 text-amber-600 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5 text-xs font-bold">
                      {index + 1}
                    </span>
                    <span className="text-sm text-gray-700">{rec}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>

          {/* Scheduled Reports */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <FiClock className="w-5 h-5 text-blue-600" />
              Reportes Programados
            </h3>
            <p className="text-sm text-gray-500 mb-4">
              Recibe reportes automáticamente en tu email
            </p>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <ScheduledReportCard
                type="Reporte Diario"
                nextRun={scheduledReports.daily.nextRun}
                enabled={scheduledReports.daily.enabled}
                onToggle={() => handleToggleSchedule('daily')}
              />
              <ScheduledReportCard
                type="Reporte Semanal"
                nextRun={scheduledReports.weekly.nextRun}
                enabled={scheduledReports.weekly.enabled}
                onToggle={() => handleToggleSchedule('weekly')}
              />
              <ScheduledReportCard
                type="Reporte Mensual"
                nextRun={scheduledReports.monthly.nextRun}
                enabled={scheduledReports.monthly.enabled}
                onToggle={() => handleToggleSchedule('monthly')}
              />
            </div>
          </div>

          {/* Export Options */}
          <div className="mt-8 bg-gradient-to-r from-green-600 to-green-700 rounded-xl p-6 text-white">
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
              <div>
                <h3 className="text-xl font-bold mb-2">Exportar Reporte Completo</h3>
                <p className="text-green-100 text-sm">
                  Descarga este reporte en tu formato preferido
                </p>
              </div>
              <div className="flex gap-3">
                <button
                  onClick={() => handleExport('pdf')}
                  className="flex items-center gap-2 px-4 py-2 bg-white text-green-700 rounded-lg hover:bg-green-50 font-medium"
                >
                  <FiDownload className="w-4 h-4" />
                  PDF
                </button>
                <button
                  onClick={() => handleExport('excel')}
                  className="flex items-center gap-2 px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-400 font-medium"
                >
                  <FiDownload className="w-4 h-4" />
                  Excel
                </button>
                <button
                  onClick={() => handleExport('csv')}
                  className="flex items-center gap-2 px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-400 font-medium"
                >
                  <FiDownload className="w-4 h-4" />
                  CSV
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default ReportsPage;
