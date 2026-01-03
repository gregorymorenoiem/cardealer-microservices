/**
 * Analytics Dashboard Page
 * 
 * Charts and metrics for dealer performance
 * Plan-gated features for advanced analytics
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import { LocalizedContent } from '@/components/common';
import {
  TrendingUp,
  Eye,
  Users,
  DollarSign,
  ShoppingCart,
  ArrowUpRight,
  ArrowDownRight,
  Lock,
  Crown,
  BarChart3,
  PieChart,
  LineChart,
  Download,
  RefreshCw,
  ChevronDown,
  Car,
  Building2,
  Phone,
  Mail,
  MessageSquare,
  Star,
} from 'lucide-react';
import { usePermissions } from '@/hooks/usePermissions';

// Mock analytics data
const mockAnalytics = {
  overview: {
    views: { current: 12543, previous: 10234, change: 22.5 },
    inquiries: { current: 234, previous: 189, change: 23.8 },
    conversions: { current: 45, previous: 38, change: 18.4 },
    revenue: { current: 4850000, previous: 3920000, change: 23.7 },
  },
  viewsByDay: [
    { date: '2025-01-09', views: 1234, inquiries: 28 },
    { date: '2025-01-10', views: 1456, inquiries: 35 },
    { date: '2025-01-11', views: 1123, inquiries: 22 },
    { date: '2025-01-12', views: 1678, inquiries: 41 },
    { date: '2025-01-13', views: 1890, inquiries: 48 },
    { date: '2025-01-14', views: 2012, inquiries: 32 },
    { date: '2025-01-15', views: 2150, inquiries: 28 },
  ],
  topListings: [
    { id: '1', title: 'Toyota Camry 2023', type: 'vehicle', views: 1234, inquiries: 45, conversion: 3.6 },
    { id: '2', title: 'Casa Polanco 4 Rec', type: 'property', views: 987, inquiries: 38, conversion: 3.8 },
    { id: '3', title: 'BMW X5 2024', type: 'vehicle', views: 876, inquiries: 29, conversion: 3.3 },
    { id: '4', title: 'Depto Santa Fe', type: 'property', views: 765, inquiries: 24, conversion: 3.1 },
    { id: '5', title: 'Mercedes C-Class', type: 'vehicle', views: 654, inquiries: 18, conversion: 2.8 },
  ],
  sourceBreakdown: [
    { source: 'Búsqueda Directa', visits: 4523, percentage: 36 },
    { source: 'Google', visits: 3876, percentage: 31 },
    { source: 'Facebook', visits: 1987, percentage: 16 },
    { source: 'Instagram', visits: 1234, percentage: 10 },
    { source: 'Referidos', visits: 923, percentage: 7 },
  ],
  inquiryTypes: [
    { type: 'WhatsApp', count: 98, icon: MessageSquare, color: '#25D366' },
    { type: 'Llamada', count: 67, icon: Phone, color: '#3B82F6' },
    { type: 'Email', count: 45, icon: Mail, color: '#6366F1' },
    { type: 'Formulario', count: 24, icon: Users, color: '#8B5CF6' },
  ],
  performance: {
    avgTimeToResponse: '2.3 hrs',
    avgTimeToClose: '12.5 días',
    customerSatisfaction: 4.7,
    repeatCustomers: '23%',
  },
};

// Simple Bar Chart Component (SVG-based)
const SimpleBarChart = ({ data }: { data: typeof mockAnalytics.viewsByDay }) => {
  const maxViews = Math.max(...data.map(d => d.views));
  
  return (
    <div className="h-48 flex items-end gap-2">
      {data.map((day, i) => {
        const height = (day.views / maxViews) * 100;
        const date = new Date(day.date);
        return (
          <div key={i} className="flex-1 flex flex-col items-center gap-1">
            <div className="w-full relative group">
              <div 
                className="w-full bg-blue-500 rounded-t transition-all hover:bg-blue-600"
                style={{ height: `${height}%`, minHeight: '4px' }}
              />
              {/* Tooltip */}
              <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-2 py-1 bg-gray-900 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap pointer-events-none">
                {day.views.toLocaleString()} vistas
              </div>
            </div>
            <span className="text-xs text-gray-500">
              {date.toLocaleDateString('es-MX', { weekday: 'short' })}
            </span>
          </div>
        );
      })}
    </div>
  );
};

// Progress Bar Component
const ProgressBar = ({ 
  percentage, 
  color = 'bg-blue-500' 
}: { 
  percentage: number;
  color?: string;
}) => (
  <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
    <div 
      className={`h-full ${color} rounded-full transition-all`}
      style={{ width: `${percentage}%` }}
    />
  </div>
);

// Stat Card Component
const StatCard = ({
  title,
  value,
  change,
  icon: Icon,
  iconColor,
  format = 'number',
}: {
  title: string;
  value: number;
  change: number;
  icon: React.ElementType;
  iconColor: string;
  format?: 'number' | 'currency' | 'percentage';
}) => {
  const formatValue = () => {
    switch (format) {
      case 'currency':
        if (value >= 1000000) return `$${(value / 1000000).toFixed(1)}M`;
        if (value >= 1000) return `$${(value / 1000).toFixed(0)}K`;
        return `$${value}`;
      case 'percentage':
        return `${value}%`;
      default:
        return value.toLocaleString();
    }
  };

  const isPositive = change >= 0;

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      <div className="flex items-center justify-between mb-3">
        <span className="text-gray-500 text-sm">{title}</span>
        <div className={`p-2 rounded-lg ${iconColor}`}>
          <Icon className="h-5 w-5" />
        </div>
      </div>
      <p className="text-2xl font-bold text-gray-900">{formatValue()}</p>
      <div className={`flex items-center gap-1 mt-1 text-sm ${isPositive ? 'text-green-600' : 'text-red-600'}`}>
        {isPositive ? (
          <ArrowUpRight className="h-4 w-4" />
        ) : (
          <ArrowDownRight className="h-4 w-4" />
        )}
        <span>{Math.abs(change)}%</span>
        <span className="text-gray-500">vs mes anterior</span>
      </div>
    </div>
  );
};

// Main Analytics Page
const AnalyticsPage = () => {
  const { portalAccess, dealerPlan } = usePermissions();
  
  const [dateRange, setDateRange] = useState('7d');

  // Check analytics access
  if (!portalAccess.reports) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="text-center max-w-md">
          <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Lock className="h-8 w-8 text-gray-400" />
          </div>
          <h2 className="text-xl font-bold text-gray-900 mb-2">Analytics No Disponible</h2>
          <p className="text-gray-600 mb-6">
            El módulo de Analytics está disponible a partir del plan <span className="font-medium text-blue-600">PRO</span>.
            Actualiza tu plan para acceder a métricas detalladas, reportes y más.
          </p>
          <Link
            to="/dealer/plans"
            className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Crown className="h-5 w-5" />
            Actualizar a PRO
          </Link>
        </div>
      </div>
    );
  }

  const data = mockAnalytics;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
          <p className="text-gray-600 mt-1">
            Métricas y rendimiento de tus publicaciones
          </p>
        </div>
        <div className="flex items-center gap-3">
          {/* Date Range Selector */}
          <div className="relative">
            <select
              value={dateRange}
              onChange={e => setDateRange(e.target.value)}
              className="appearance-none bg-white border border-gray-300 rounded-lg px-4 py-2 pr-10 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="7d">Últimos 7 días</option>
              <option value="30d">Últimos 30 días</option>
              <option value="90d">Últimos 90 días</option>
              <option value="12m">Último año</option>
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400 pointer-events-none" />
          </div>
          
          <button className="flex items-center gap-2 px-3 py-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors">
            <RefreshCw className="h-4 w-4" />
          </button>
          
          <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
            <Download className="h-4 w-4" />
            Exportar
          </button>
        </div>
      </div>

      {/* Overview Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="Vistas Totales"
          value={data.overview.views.current}
          change={data.overview.views.change}
          icon={Eye}
          iconColor="bg-blue-100 text-blue-600"
        />
        <StatCard
          title="Consultas"
          value={data.overview.inquiries.current}
          change={data.overview.inquiries.change}
          icon={Users}
          iconColor="bg-green-100 text-green-600"
        />
        <StatCard
          title="Conversiones"
          value={data.overview.conversions.current}
          change={data.overview.conversions.change}
          icon={ShoppingCart}
          iconColor="bg-purple-100 text-purple-600"
        />
        <StatCard
          title="Ingresos"
          value={data.overview.revenue.current}
          change={data.overview.revenue.change}
          icon={DollarSign}
          iconColor="bg-yellow-100 text-yellow-600"
          format="currency"
        />
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Views Chart */}
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <h3 className="font-semibold text-gray-900 flex items-center gap-2">
              <BarChart3 className="h-5 w-5 text-blue-500" />
              Vistas por Día
            </h3>
            <span className="text-sm text-gray-500">Últimos 7 días</span>
          </div>
          <SimpleBarChart data={data.viewsByDay} />
        </div>

        {/* Source Breakdown */}
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <h3 className="font-semibold text-gray-900 flex items-center gap-2">
              <PieChart className="h-5 w-5 text-purple-500" />
              Fuentes de Tráfico
            </h3>
          </div>
          <div className="space-y-4">
            {data.sourceBreakdown.map((source, i) => {
              const colors = ['bg-blue-500', 'bg-green-500', 'bg-purple-500', 'bg-yellow-500', 'bg-pink-500'];
              return (
                <div key={i}>
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-sm text-gray-700">{source.source}</span>
                    <span className="text-sm font-medium text-gray-900">
                      {source.visits.toLocaleString()} ({source.percentage}%)
                    </span>
                  </div>
                  <ProgressBar percentage={source.percentage} color={colors[i]} />
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Top Listings & Inquiry Types */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Top Listings */}
        <div className="lg:col-span-2 bg-white rounded-lg border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <h3 className="font-semibold text-gray-900 flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-green-500" />
              Top Publicaciones
            </h3>
            <Link to="/dealer/listings" className="text-sm text-blue-600 hover:text-blue-700">
              Ver todas →
            </Link>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="pb-3 text-left text-xs font-medium text-gray-500 uppercase">Publicación</th>
                  <th className="pb-3 text-right text-xs font-medium text-gray-500 uppercase">Vistas</th>
                  <th className="pb-3 text-right text-xs font-medium text-gray-500 uppercase">Consultas</th>
                  <th className="pb-3 text-right text-xs font-medium text-gray-500 uppercase">Conversión</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.topListings.map((listing, i) => (
                  <tr key={listing.id} className="hover:bg-gray-50">
                    <td className="py-3">
                      <div className="flex items-center gap-3">
                        <span className="text-sm font-medium text-gray-400">#{i + 1}</span>
                        {listing.type === 'vehicle' ? (
                          <Car className="h-4 w-4 text-blue-500" />
                        ) : (
                          <Building2 className="h-4 w-4 text-green-500" />
                        )}
                        <span className="text-sm font-medium text-gray-900">
                          <LocalizedContent content={listing.title} showBadge={false} />
                        </span>
                      </div>
                    </td>
                    <td className="py-3 text-right text-sm text-gray-600">
                      {listing.views.toLocaleString()}
                    </td>
                    <td className="py-3 text-right text-sm text-gray-600">
                      {listing.inquiries}
                    </td>
                    <td className="py-3 text-right">
                      <span className="text-sm font-medium text-green-600">{listing.conversion}%</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Inquiry Types */}
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h3 className="font-semibold text-gray-900 flex items-center gap-2 mb-6">
            <MessageSquare className="h-5 w-5 text-orange-500" />
            Canales de Contacto
          </h3>
          <div className="space-y-4">
            {data.inquiryTypes.map((type) => {
              const Icon = type.icon;
              const total = data.inquiryTypes.reduce((sum, t) => sum + t.count, 0);
              const percentage = ((type.count / total) * 100).toFixed(0);
              return (
                <div key={type.type} className="flex items-center gap-3">
                  <div 
                    className="p-2 rounded-lg"
                    style={{ backgroundColor: `${type.color}20` }}
                  >
                    <Icon className="h-5 w-5" style={{ color: type.color }} />
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-sm font-medium text-gray-900">{type.type}</span>
                      <span className="text-sm text-gray-600">{type.count}</span>
                    </div>
                    <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                      <div 
                        className="h-full rounded-full"
                        style={{ width: `${percentage}%`, backgroundColor: type.color }}
                      />
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Performance Metrics */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        <h3 className="font-semibold text-gray-900 flex items-center gap-2 mb-6">
          <LineChart className="h-5 w-5 text-indigo-500" />
          Métricas de Rendimiento
        </h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
          <div className="text-center">
            <div className="text-3xl font-bold text-gray-900">{data.performance.avgTimeToResponse}</div>
            <p className="text-sm text-gray-500 mt-1">Tiempo Promedio de Respuesta</p>
          </div>
          <div className="text-center">
            <div className="text-3xl font-bold text-gray-900">{data.performance.avgTimeToClose}</div>
            <p className="text-sm text-gray-500 mt-1">Tiempo Promedio de Cierre</p>
          </div>
          <div className="text-center">
            <div className="flex items-center justify-center gap-1">
              <span className="text-3xl font-bold text-gray-900">{data.performance.customerSatisfaction}</span>
              <Star className="h-6 w-6 text-yellow-400 fill-current" />
            </div>
            <p className="text-sm text-gray-500 mt-1">Satisfacción del Cliente</p>
          </div>
          <div className="text-center">
            <div className="text-3xl font-bold text-gray-900">{data.performance.repeatCustomers}</div>
            <p className="text-sm text-gray-500 mt-1">Clientes Recurrentes</p>
          </div>
        </div>
      </div>

      {/* Export & Reports Section (Enterprise feature teaser) */}
      {dealerPlan !== 'enterprise' && (
        <div className="bg-gradient-to-r from-purple-50 to-blue-50 rounded-lg border border-purple-100 p-6">
          <div className="flex items-start gap-4">
            <div className="p-3 bg-purple-100 rounded-lg">
              <Crown className="h-6 w-6 text-purple-600" />
            </div>
            <div className="flex-1">
              <h3 className="font-semibold text-gray-900">Desbloquea Reportes Avanzados</h3>
              <p className="text-gray-600 text-sm mt-1">
                Con el plan <span className="font-medium text-purple-600">Enterprise</span> obtén acceso a:
              </p>
              <ul className="mt-2 space-y-1 text-sm text-gray-600">
                <li className="flex items-center gap-2">
                  <div className="w-1.5 h-1.5 bg-purple-500 rounded-full" />
                  Reportes programados automáticos
                </li>
                <li className="flex items-center gap-2">
                  <div className="w-1.5 h-1.5 bg-purple-500 rounded-full" />
                  Análisis de competencia
                </li>
                <li className="flex items-center gap-2">
                  <div className="w-1.5 h-1.5 bg-purple-500 rounded-full" />
                  Predicciones de ventas con IA
                </li>
                <li className="flex items-center gap-2">
                  <div className="w-1.5 h-1.5 bg-purple-500 rounded-full" />
                  Exportación a Excel/PDF ilimitada
                </li>
              </ul>
              <Link
                to="/dealer/plans"
                className="inline-flex items-center gap-2 mt-4 text-sm font-medium text-purple-600 hover:text-purple-700"
              >
                Ver planes disponibles
                <ArrowUpRight className="h-4 w-4" />
              </Link>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AnalyticsPage;
