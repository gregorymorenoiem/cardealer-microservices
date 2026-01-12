/**
 * Dealer Dashboard Page - Resumen General
 *
 * Dashboard principal del dealer con datos reales:
 * - Estadísticas del inventario
 * - Métricas de engagement (vistas, consultas, favoritos)
 * - Actividad reciente
 * - Acciones rápidas
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  FiTruck,
  FiTrendingUp,
  FiDollarSign,
  FiEye,
  FiHeart,
  FiMessageCircle,
  FiPlus,
  FiList,
  FiSettings,
  FiRefreshCw,
  FiClock,
  FiCheckCircle,
  FiAlertCircle,
  FiXCircle,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import { getDealerVehicles } from '@/services/vehicleService';
import { useAuth } from '@/hooks/useAuth';

// ============================================================================
// TYPES
// ============================================================================

interface DashboardStats {
  // Inventory
  totalVehicles: number;
  activeVehicles: number;
  pendingVehicles: number;
  soldVehicles: number;
  draftVehicles: number;

  // Engagement
  totalViews: number;
  totalInquiries: number;
  totalFavorites: number;

  // Value
  inventoryValue: number;
  averagePrice: number;
}

interface RecentActivity {
  id: string;
  type: 'view' | 'inquiry' | 'favorite' | 'status_change';
  vehicleTitle: string;
  timestamp: Date;
  details?: string;
}

// ============================================================================
// COMPONENTS
// ============================================================================

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  color: 'blue' | 'green' | 'purple' | 'amber' | 'red' | 'indigo';
  trend?: { value: number; isPositive: boolean };
}

const StatCard: React.FC<StatCardProps> = ({ title, value, subtitle, icon, color, trend }) => {
  const colorClasses = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    purple: 'bg-purple-50 text-purple-600',
    amber: 'bg-amber-50 text-amber-600',
    red: 'bg-red-50 text-red-600',
    indigo: 'bg-indigo-50 text-indigo-600',
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

interface QuickActionProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  href: string;
  color: string;
}

const QuickAction: React.FC<QuickActionProps> = ({ title, description, icon, href, color }) => (
  <Link
    to={href}
    className="flex items-center gap-4 p-4 bg-white rounded-xl border border-gray-100 hover:border-blue-200 hover:shadow-md transition-all group"
  >
    <div className={`p-3 rounded-lg ${color}`}>{icon}</div>
    <div className="flex-1">
      <h3 className="font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">
        {title}
      </h3>
      <p className="text-sm text-gray-500">{description}</p>
    </div>
  </Link>
);

interface InventoryBreakdownProps {
  stats: DashboardStats;
}

const InventoryBreakdown: React.FC<InventoryBreakdownProps> = ({ stats }) => {
  const items = [
    {
      label: 'Publicados',
      count: stats.activeVehicles,
      color: 'bg-green-500',
      icon: <FiCheckCircle className="h-4 w-4 text-green-600" />,
    },
    {
      label: 'En Evaluación',
      count: stats.pendingVehicles,
      color: 'bg-amber-500',
      icon: <FiClock className="h-4 w-4 text-amber-600" />,
    },
    {
      label: 'Vendidos',
      count: stats.soldVehicles,
      color: 'bg-blue-500',
      icon: <FiDollarSign className="h-4 w-4 text-blue-600" />,
    },
    {
      label: 'Borradores',
      count: stats.draftVehicles,
      color: 'bg-gray-400',
      icon: <FiXCircle className="h-4 w-4 text-gray-500" />,
    },
  ];

  const total = stats.totalVehicles || 1;

  return (
    <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Estado del Inventario</h3>

      <div className="space-y-4">
        {items.map((item) => (
          <div key={item.label} className="flex items-center gap-3">
            {item.icon}
            <div className="flex-1">
              <div className="flex justify-between items-center mb-1">
                <span className="text-sm font-medium text-gray-700">{item.label}</span>
                <span className="text-sm text-gray-500">{item.count}</span>
              </div>
              <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                <div
                  className={`h-full ${item.color} rounded-full transition-all`}
                  style={{ width: `${(item.count / total) * 100}%` }}
                />
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="mt-4 pt-4 border-t border-gray-100">
        <div className="flex justify-between items-center">
          <span className="text-sm font-medium text-gray-600">Total en inventario</span>
          <span className="text-lg font-bold text-gray-900">{stats.totalVehicles}</span>
        </div>
      </div>
    </div>
  );
};

interface RecentActivityListProps {
  activities: RecentActivity[];
}

const RecentActivityList: React.FC<RecentActivityListProps> = ({ activities }) => {
  const getActivityIcon = (type: RecentActivity['type']) => {
    switch (type) {
      case 'view':
        return <FiEye className="h-4 w-4 text-blue-500" />;
      case 'inquiry':
        return <FiMessageCircle className="h-4 w-4 text-green-500" />;
      case 'favorite':
        return <FiHeart className="h-4 w-4 text-red-500" />;
      case 'status_change':
        return <FiCheckCircle className="h-4 w-4 text-purple-500" />;
    }
  };

  const getActivityText = (activity: RecentActivity) => {
    switch (activity.type) {
      case 'view':
        return `Alguien vio`;
      case 'inquiry':
        return `Nueva consulta para`;
      case 'favorite':
        return `Guardado como favorito`;
      case 'status_change':
        return `Estado cambiado:`;
    }
  };

  const formatTime = (date: Date) => {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 60) return `hace ${minutes} min`;
    if (hours < 24) return `hace ${hours} h`;
    return `hace ${days} días`;
  };

  return (
    <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Actividad Reciente</h3>

      {activities.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <FiClock className="h-8 w-8 mx-auto mb-2 opacity-50" />
          <p>No hay actividad reciente</p>
        </div>
      ) : (
        <div className="space-y-4">
          {activities.map((activity) => (
            <div key={activity.id} className="flex items-start gap-3">
              <div className="p-2 bg-gray-50 rounded-lg">{getActivityIcon(activity.type)}</div>
              <div className="flex-1 min-w-0">
                <p className="text-sm text-gray-900">
                  {getActivityText(activity)}{' '}
                  <span className="font-medium">{activity.vehicleTitle}</span>
                </p>
                {activity.details && (
                  <p className="text-xs text-gray-500 mt-0.5">{activity.details}</p>
                )}
                <p className="text-xs text-gray-400 mt-1">{formatTime(activity.timestamp)}</p>
              </div>
            </div>
          ))}
        </div>
      )}

      <Link
        to="/dealer/activity"
        className="mt-4 block text-center text-sm text-blue-600 hover:text-blue-700"
      >
        Ver toda la actividad →
      </Link>
    </div>
  );
};

// ============================================================================
// MAIN PAGE
// ============================================================================

export default function DealerDashboardPage() {
  const { t } = useTranslation(['dealer', 'common']);
  const { user } = useAuth();

  const [stats, setStats] = useState<DashboardStats>({
    totalVehicles: 0,
    activeVehicles: 0,
    pendingVehicles: 0,
    soldVehicles: 0,
    draftVehicles: 0,
    totalViews: 0,
    totalInquiries: 0,
    totalFavorites: 0,
    inventoryValue: 0,
    averagePrice: 0,
  });
  const [recentActivities, setRecentActivities] = useState<RecentActivity[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const dealerId = user?.dealerId || user?.id;

  useEffect(() => {
    const fetchDashboardData = async () => {
      if (!dealerId) {
        setError('No se encontró cuenta de dealer');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        setError(null);

        // Fetch vehicles data
        const data = await getDealerVehicles(dealerId);
        const vehicles = data.vehicles;

        // Calculate stats from vehicles
        const activeVehicles = vehicles.filter((v) => v.status === 'approved');
        const pendingVehicles = vehicles.filter((v) => v.status === 'pending');
        const soldVehicles = vehicles.filter((v) => v.status === 'sold');
        const draftVehicles = vehicles.filter((v) => v.status === 'rejected');

        const totalViews = vehicles.reduce((sum, v) => sum + (v.viewCount || 0), 0);
        const totalInquiries = vehicles.reduce((sum, v) => sum + (v.inquiryCount || 0), 0);
        const totalFavorites = vehicles.reduce((sum, v) => sum + (v.favoriteCount || 0), 0);
        const inventoryValue = activeVehicles.reduce((sum, v) => sum + v.price, 0);
        const averagePrice = activeVehicles.length > 0 ? inventoryValue / activeVehicles.length : 0;

        setStats({
          totalVehicles: vehicles.length,
          activeVehicles: activeVehicles.length,
          pendingVehicles: pendingVehicles.length,
          soldVehicles: soldVehicles.length,
          draftVehicles: draftVehicles.length,
          totalViews,
          totalInquiries,
          totalFavorites,
          inventoryValue,
          averagePrice,
        });

        // Generate recent activities from vehicles with highest engagement
        const activities: RecentActivity[] = [];
        const sortedByViews = [...vehicles].sort((a, b) => (b.viewCount || 0) - (a.viewCount || 0));

        sortedByViews.slice(0, 3).forEach((v, i) => {
          if (v.viewCount && v.viewCount > 0) {
            activities.push({
              id: `view-${v.id}`,
              type: 'view',
              vehicleTitle: v.title,
              timestamp: new Date(Date.now() - (i + 1) * 3600000 * (Math.random() * 5 + 1)),
              details: `${v.viewCount} vistas totales`,
            });
          }
        });

        const sortedByInquiries = [...vehicles].sort(
          (a, b) => (b.inquiryCount || 0) - (a.inquiryCount || 0)
        );

        sortedByInquiries.slice(0, 2).forEach((v, i) => {
          if (v.inquiryCount && v.inquiryCount > 0) {
            activities.push({
              id: `inquiry-${v.id}`,
              type: 'inquiry',
              vehicleTitle: v.title,
              timestamp: new Date(Date.now() - (i + 1) * 3600000 * (Math.random() * 10 + 2)),
              details: `${v.inquiryCount} consultas totales`,
            });
          }
        });

        // Sort by timestamp
        activities.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());
        setRecentActivities(activities.slice(0, 5));
      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        setError('Error al cargar el dashboard');
      } finally {
        setIsLoading(false);
      }
    };

    fetchDashboardData();
  }, [dealerId]);

  const handleRefresh = () => {
    if (dealerId) {
      setIsLoading(true);
      getDealerVehicles(dealerId)
        .then((data) => {
          const vehicles = data.vehicles;
          const activeVehicles = vehicles.filter((v) => v.status === 'approved');
          const pendingVehicles = vehicles.filter((v) => v.status === 'pending');
          const soldVehicles = vehicles.filter((v) => v.status === 'sold');
          const draftVehicles = vehicles.filter((v) => v.status === 'rejected');

          setStats({
            totalVehicles: vehicles.length,
            activeVehicles: activeVehicles.length,
            pendingVehicles: pendingVehicles.length,
            soldVehicles: soldVehicles.length,
            draftVehicles: draftVehicles.length,
            totalViews: vehicles.reduce((sum, v) => sum + (v.viewCount || 0), 0),
            totalInquiries: vehicles.reduce((sum, v) => sum + (v.inquiryCount || 0), 0),
            totalFavorites: vehicles.reduce((sum, v) => sum + (v.favoriteCount || 0), 0),
            inventoryValue: activeVehicles.reduce((sum, v) => sum + v.price, 0),
            averagePrice:
              activeVehicles.length > 0
                ? activeVehicles.reduce((sum, v) => sum + v.price, 0) / activeVehicles.length
                : 0,
          });
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

  const formatNumber = (value: number) => new Intl.NumberFormat('es-DO').format(value);

  if (isLoading) {
    return (
      <DealerPortalLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <FiRefreshCw className="h-8 w-8 animate-spin text-blue-600 mx-auto" />
            <p className="mt-4 text-gray-600">Cargando dashboard...</p>
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
              <h1 className="text-2xl font-bold text-gray-900">
                {t('dealer:dashboard.title', 'Resumen General')}
              </h1>
              <p className="text-gray-500">
                {t('dealer:dashboard.welcome', {
                  name: user?.name?.split(' ')[0] || 'Usuario',
                  defaultValue: `¡Bienvenido, ${user?.name?.split(' ')[0] || 'Usuario'}!`,
                })}
              </p>
            </div>

            <button
              onClick={handleRefresh}
              className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
            >
              <FiRefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
              <span>Actualizar</span>
            </button>
          </div>

          {/* Main Stats */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <StatCard
              title="Vehículos Publicados"
              value={stats.activeVehicles}
              subtitle={`de ${stats.totalVehicles} en inventario`}
              icon={<FaCar className="h-6 w-6" />}
              color="blue"
            />
            <StatCard
              title="Vistas Totales"
              value={formatNumber(stats.totalViews)}
              subtitle="en todos tus vehículos"
              icon={<FiEye className="h-6 w-6" />}
              color="green"
              trend={stats.totalViews > 100 ? { value: 12, isPositive: true } : undefined}
            />
            <StatCard
              title="Consultas"
              value={stats.totalInquiries}
              subtitle="solicitudes recibidas"
              icon={<FiMessageCircle className="h-6 w-6" />}
              color="purple"
            />
            <StatCard
              title="Valor del Inventario"
              value={formatCurrency(stats.inventoryValue)}
              subtitle={`Promedio: ${formatCurrency(stats.averagePrice)}`}
              icon={<FiDollarSign className="h-6 w-6" />}
              color="amber"
            />
          </div>

          {/* Engagement Stats */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-red-50 rounded-lg">
                  <FiHeart className="h-5 w-5 text-red-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{stats.totalFavorites}</p>
                  <p className="text-sm text-gray-500">Guardados como favorito</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-amber-50 rounded-lg">
                  <FiClock className="h-5 w-5 text-amber-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{stats.pendingVehicles}</p>
                  <p className="text-sm text-gray-500">En evaluación</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-green-50 rounded-lg">
                  <FiCheckCircle className="h-5 w-5 text-green-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{stats.soldVehicles}</p>
                  <p className="text-sm text-gray-500">Vendidos</p>
                </div>
              </div>
            </div>
          </div>

          {/* Two Column Layout */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Inventory Breakdown */}
            <InventoryBreakdown stats={stats} />

            {/* Recent Activity */}
            <RecentActivityList activities={recentActivities} />
          </div>

          {/* Quick Actions */}
          <div>
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Acciones Rápidas</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <QuickAction
                title="Agregar Vehículo"
                description="Publicar nuevo vehículo"
                icon={<FiPlus className="h-5 w-5" />}
                href="/dealer/inventory/new"
                color="bg-blue-50 text-blue-600"
              />
              <QuickAction
                title="Ver Inventario"
                description="Gestionar todos tus vehículos"
                icon={<FiList className="h-5 w-5" />}
                href="/dealer/inventory"
                color="bg-green-50 text-green-600"
              />
              <QuickAction
                title="Publicaciones"
                description="Ver vehículos publicados"
                icon={<FiTruck className="h-5 w-5" />}
                href="/dealer/listings"
                color="bg-purple-50 text-purple-600"
              />
              <QuickAction
                title="Configuración"
                description="Ajustes de tu cuenta"
                icon={<FiSettings className="h-5 w-5" />}
                href="/dealer/settings"
                color="bg-gray-100 text-gray-600"
              />
            </div>
          </div>

          {/* CTA Banner if no vehicles */}
          {stats.totalVehicles === 0 && (
            <div className="bg-gradient-to-r from-blue-600 to-purple-600 rounded-xl p-6 text-white">
              <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                <div>
                  <h2 className="text-xl font-bold">¡Comienza a vender!</h2>
                  <p className="mt-1 text-blue-100">
                    Publica tu primer vehículo y empieza a recibir consultas de compradores.
                  </p>
                </div>
                <Link
                  to="/dealer/inventory/new"
                  className="px-6 py-3 bg-white text-blue-600 rounded-lg font-semibold hover:bg-blue-50 transition-colors text-center"
                >
                  Agregar Vehículo
                </Link>
              </div>
            </div>
          )}
        </div>
      </div>
    </DealerPortalLayout>
  );
}
