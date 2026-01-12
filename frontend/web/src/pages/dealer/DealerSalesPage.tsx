/**
 * DealerSalesPage - Historial de Ventas
 *
 * Muestra los vehículos vendidos del dealer con:
 * - Estadísticas de ventas
 * - Historial de ventas con fechas
 * - Métricas de rendimiento
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiDollarSign,
  FiTrendingUp,
  FiCalendar,
  FiClock,
  FiRefreshCw,
  FiAlertCircle,
  FiCheckCircle,
  FiEye,
  FiMessageCircle,
  FiSearch,
  FiFilter,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { getDealerVehicles } from '@/services/vehicleService';
import type { DealerVehicle } from '@/services/vehicleService';
import { useAuth } from '@/hooks/useAuth';

// ============================================================================
// TYPES
// ============================================================================

interface SalesStats {
  totalSold: number;
  totalRevenue: number;
  averagePrice: number;
  averageDaysToSell: number;
  thisMonth: number;
  thisMonthRevenue: number;
  lastMonth: number;
  lastMonthRevenue: number;
}

// ============================================================================
// COMPONENTS
// ============================================================================

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  color: 'blue' | 'green' | 'purple' | 'amber';
  trend?: { value: number; isPositive: boolean };
}

const StatCard: React.FC<StatCardProps> = ({ title, value, subtitle, icon, color, trend }) => {
  const colorClasses = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    purple: 'bg-purple-50 text-purple-600',
    amber: 'bg-amber-50 text-amber-600',
  };

  return (
    <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-500">{title}</p>
          <p className="mt-2 text-3xl font-bold text-gray-900">{value}</p>
          {subtitle && <p className="mt-1 text-sm text-gray-500">{subtitle}</p>}
          {trend && (
            <div
              className={`mt-2 flex items-center text-sm ${
                trend.isPositive ? 'text-green-600' : 'text-red-600'
              }`}
            >
              <FiTrendingUp className={`h-4 w-4 ${!trend.isPositive ? 'rotate-180' : ''}`} />
              <span className="ml-1">{trend.value}% vs mes anterior</span>
            </div>
          )}
        </div>
        <div className={`p-3 rounded-lg ${colorClasses[color]}`}>{icon}</div>
      </div>
    </div>
  );
};

interface SaleCardProps {
  vehicle: DealerVehicle;
  formatCurrency: (value: number) => string;
  formatDate: (date: string | Date | undefined) => string;
}

const SaleCard: React.FC<SaleCardProps> = ({ vehicle, formatCurrency, formatDate }) => {
  const primaryImage =
    vehicle.images && vehicle.images.length > 0 ? vehicle.images[0] : '/placeholder-car.jpg';

  // Calculate days to sell (from publishedAt to soldAt)
  const daysToSell = (() => {
    if (!vehicle.soldAt) return null;
    const soldDate = new Date(vehicle.soldAt);
    const publishedDate = vehicle.publishedAt
      ? new Date(vehicle.publishedAt)
      : vehicle.createdAt
        ? new Date(vehicle.createdAt)
        : soldDate;
    const diff = soldDate.getTime() - publishedDate.getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  })();

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow">
      <div className="flex flex-col md:flex-row">
        {/* Image */}
        <div className="md:w-48 h-40 md:h-auto bg-gray-100 flex-shrink-0">
          <img
            src={primaryImage}
            alt={vehicle.title}
            className="w-full h-full object-cover"
            onError={(e) => {
              (e.target as HTMLImageElement).src = '/placeholder-car.jpg';
            }}
          />
        </div>

        {/* Content */}
        <div className="flex-1 p-4">
          <div className="flex items-start justify-between">
            <div>
              <h3 className="text-lg font-semibold text-gray-900">{vehicle.title}</h3>
              <p className="text-sm text-gray-500">
                {vehicle.year} • {vehicle.mileage?.toLocaleString()} km
              </p>
            </div>
            <div className="flex items-center gap-1 px-2.5 py-1 bg-green-100 text-green-700 rounded-full text-sm font-medium">
              <FiCheckCircle className="h-4 w-4" />
              Vendido
            </div>
          </div>

          <div className="mt-4 grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <p className="text-xs text-gray-500 uppercase tracking-wide">Precio de Venta</p>
              <p className="text-lg font-bold text-green-600">{formatCurrency(vehicle.price)}</p>
            </div>
            <div>
              <p className="text-xs text-gray-500 uppercase tracking-wide">Fecha de Venta</p>
              <p className="text-sm font-medium text-gray-900">
                {vehicle.soldAt ? formatDate(vehicle.soldAt) : 'N/A'}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500 uppercase tracking-wide">Días en Venta</p>
              <p className="text-sm font-medium text-gray-900">
                {daysToSell !== null ? `${daysToSell} días` : 'N/A'}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500 uppercase tracking-wide">Engagement</p>
              <div className="flex items-center gap-3 text-sm text-gray-600">
                <span className="flex items-center gap-1">
                  <FiEye className="h-3.5 w-3.5" />
                  {vehicle.viewCount || 0}
                </span>
                <span className="flex items-center gap-1">
                  <FiMessageCircle className="h-3.5 w-3.5" />
                  {vehicle.inquiryCount || 0}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// ============================================================================
// MAIN PAGE
// ============================================================================

export default function DealerSalesPage() {
  const { user } = useAuth();

  const [soldVehicles, setSoldVehicles] = useState<DealerVehicle[]>([]);
  const [stats, setStats] = useState<SalesStats>({
    totalSold: 0,
    totalRevenue: 0,
    averagePrice: 0,
    averageDaysToSell: 0,
    thisMonth: 0,
    thisMonthRevenue: 0,
    lastMonth: 0,
    lastMonthRevenue: 0,
  });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState<'date' | 'price'>('date');

  const dealerId = user?.dealerId || user?.id;

  useEffect(() => {
    const fetchSalesData = async () => {
      if (!dealerId) {
        setError('No se encontró cuenta de dealer');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        setError(null);

        const data = await getDealerVehicles(dealerId);
        const sold = data.vehicles.filter((v) => v.status === 'sold');

        // Sort by soldAt date (most recent first)
        sold.sort((a, b) => {
          const dateA = a.soldAt ? new Date(a.soldAt).getTime() : 0;
          const dateB = b.soldAt ? new Date(b.soldAt).getTime() : 0;
          return dateB - dateA;
        });

        setSoldVehicles(sold);

        // Calculate stats
        const totalRevenue = sold.reduce((sum, v) => sum + v.price, 0);
        const averagePrice = sold.length > 0 ? totalRevenue / sold.length : 0;

        // Calculate average days to sell
        const daysToSellArray = sold
          .map((v) => {
            if (!v.soldAt) return null;
            const soldDate = new Date(v.soldAt);
            const publishedDate = v.publishedAt
              ? new Date(v.publishedAt)
              : v.createdAt
                ? new Date(v.createdAt)
                : soldDate;
            return Math.ceil(
              (soldDate.getTime() - publishedDate.getTime()) / (1000 * 60 * 60 * 24)
            );
          })
          .filter((d): d is number => d !== null);

        const averageDaysToSell =
          daysToSellArray.length > 0
            ? Math.round(daysToSellArray.reduce((a, b) => a + b, 0) / daysToSellArray.length)
            : 0;

        // This month stats
        const now = new Date();
        const thisMonthStart = new Date(now.getFullYear(), now.getMonth(), 1);
        const lastMonthStart = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        const lastMonthEnd = new Date(now.getFullYear(), now.getMonth(), 0);

        const thisMonthSales = sold.filter((v) => {
          if (!v.soldAt) return false;
          return new Date(v.soldAt) >= thisMonthStart;
        });

        const lastMonthSales = sold.filter((v) => {
          if (!v.soldAt) return false;
          const soldDate = new Date(v.soldAt);
          return soldDate >= lastMonthStart && soldDate <= lastMonthEnd;
        });

        setStats({
          totalSold: sold.length,
          totalRevenue,
          averagePrice,
          averageDaysToSell,
          thisMonth: thisMonthSales.length,
          thisMonthRevenue: thisMonthSales.reduce((sum, v) => sum + v.price, 0),
          lastMonth: lastMonthSales.length,
          lastMonthRevenue: lastMonthSales.reduce((sum, v) => sum + v.price, 0),
        });
      } catch (err) {
        console.error('Error fetching sales data:', err);
        setError('Error al cargar las ventas');
      } finally {
        setIsLoading(false);
      }
    };

    fetchSalesData();
  }, [dealerId]);

  const handleRefresh = () => {
    if (dealerId) {
      setIsLoading(true);
      getDealerVehicles(dealerId)
        .then((data) => {
          const sold = data.vehicles.filter((v) => v.status === 'sold');
          sold.sort((a, b) => {
            const dateA = a.soldAt ? new Date(a.soldAt).getTime() : 0;
            const dateB = b.soldAt ? new Date(b.soldAt).getTime() : 0;
            return dateB - dateA;
          });
          setSoldVehicles(sold);
        })
        .catch(console.error)
        .finally(() => setIsLoading(false));
    }
  };

  const formatCurrency = (value: number) =>
    new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);

  const formatDate = (date: string | Date | undefined) => {
    if (!date) return 'N/A';
    return new Intl.DateTimeFormat('es-DO', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    }).format(new Date(date));
  };

  // Filter and sort vehicles
  const filteredVehicles = soldVehicles
    .filter((v) => {
      if (!searchTerm) return true;
      return (
        v.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        v.make.toLowerCase().includes(searchTerm.toLowerCase()) ||
        v.model.toLowerCase().includes(searchTerm.toLowerCase())
      );
    })
    .sort((a, b) => {
      if (sortBy === 'price') {
        return b.price - a.price;
      }
      // Default: sort by date
      const dateA = a.soldAt ? new Date(a.soldAt).getTime() : 0;
      const dateB = b.soldAt ? new Date(b.soldAt).getTime() : 0;
      return dateB - dateA;
    });

  if (isLoading) {
    return (
      <DealerPortalLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <FiRefreshCw className="h-8 w-8 animate-spin text-blue-600 mx-auto" />
            <p className="mt-4 text-gray-600">Cargando historial de ventas...</p>
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  if (error) {
    return (
      <DealerPortalLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <FiAlertCircle className="h-12 w-12 text-red-500 mx-auto" />
            <p className="mt-4 text-gray-900 font-medium">{error}</p>
            <button
              onClick={handleRefresh}
              className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Reintentar
            </button>
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  return (
    <DealerPortalLayout>
      <div className="min-h-screen bg-gray-50 p-6">
        <div className="max-w-7xl mx-auto space-y-6">
          {/* Header */}
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Historial de Ventas</h1>
              <p className="text-gray-500">Registro completo de tus vehículos vendidos</p>
            </div>

            <button
              onClick={handleRefresh}
              className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
            >
              <FiRefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
              <span>Actualizar</span>
            </button>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <StatCard
              title="Total Vendidos"
              value={stats.totalSold}
              subtitle="vehículos"
              icon={<FaCar className="h-6 w-6" />}
              color="blue"
            />
            <StatCard
              title="Ingresos Totales"
              value={formatCurrency(stats.totalRevenue)}
              subtitle="acumulado"
              icon={<FiDollarSign className="h-6 w-6" />}
              color="green"
              trend={
                stats.lastMonth > 0
                  ? {
                      value: Math.round(
                        ((stats.thisMonthRevenue - stats.lastMonthRevenue) /
                          stats.lastMonthRevenue) *
                          100
                      ),
                      isPositive: stats.thisMonthRevenue >= stats.lastMonthRevenue,
                    }
                  : undefined
              }
            />
            <StatCard
              title="Precio Promedio"
              value={formatCurrency(stats.averagePrice)}
              subtitle="por vehículo"
              icon={<FiTrendingUp className="h-6 w-6" />}
              color="purple"
            />
            <StatCard
              title="Tiempo Promedio"
              value={`${stats.averageDaysToSell} días`}
              subtitle="en vender"
              icon={<FiClock className="h-6 w-6" />}
              color="amber"
            />
          </div>

          {/* Monthly Comparison */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-green-50 rounded-lg">
                  <FiCalendar className="h-5 w-5 text-green-600" />
                </div>
                <div>
                  <p className="text-sm text-gray-500">Este Mes</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.thisMonth} ventas</p>
                  <p className="text-sm text-green-600">{formatCurrency(stats.thisMonthRevenue)}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-gray-100 rounded-lg">
                  <FiCalendar className="h-5 w-5 text-gray-600" />
                </div>
                <div>
                  <p className="text-sm text-gray-500">Mes Anterior</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.lastMonth} ventas</p>
                  <p className="text-sm text-gray-600">{formatCurrency(stats.lastMonthRevenue)}</p>
                </div>
              </div>
            </div>
          </div>

          {/* Filters */}
          <div className="bg-white rounded-xl p-4 shadow-sm border border-gray-100">
            <div className="flex flex-col md:flex-row gap-4">
              {/* Search */}
              <div className="flex-1 relative">
                <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 h-5 w-5" />
                <input
                  type="text"
                  placeholder="Buscar por marca, modelo..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Sort */}
              <div className="flex items-center gap-2">
                <FiFilter className="text-gray-400 h-5 w-5" />
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value as 'date' | 'price')}
                  className="px-4 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="date">Más recientes</option>
                  <option value="price">Mayor precio</option>
                </select>
              </div>
            </div>
          </div>

          {/* Sales List */}
          <div className="space-y-4">
            <h2 className="text-lg font-semibold text-gray-900">
              Vehículos Vendidos ({filteredVehicles.length})
            </h2>

            {filteredVehicles.length === 0 ? (
              <div className="bg-white rounded-xl p-12 text-center border border-gray-100">
                <FaCar className="h-16 w-16 text-gray-300 mx-auto" />
                <h3 className="mt-4 text-lg font-medium text-gray-900">
                  {searchTerm ? 'No se encontraron ventas' : 'Sin ventas registradas'}
                </h3>
                <p className="mt-2 text-gray-500">
                  {searchTerm
                    ? 'Intenta con otros términos de búsqueda'
                    : 'Cuando vendas un vehículo, aparecerá aquí'}
                </p>
                {!searchTerm && (
                  <Link
                    to="/dealer/inventory"
                    className="mt-6 inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  >
                    Ver Inventario
                  </Link>
                )}
              </div>
            ) : (
              <div className="space-y-4">
                {filteredVehicles.map((vehicle) => (
                  <SaleCard
                    key={vehicle.id}
                    vehicle={vehicle}
                    formatCurrency={formatCurrency}
                    formatDate={formatDate}
                  />
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </DealerPortalLayout>
  );
}
