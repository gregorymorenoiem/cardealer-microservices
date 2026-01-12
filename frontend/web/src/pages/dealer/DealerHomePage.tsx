/**
 * DealerHomePage - Dashboard principal del Portal Dealer
 *
 * Vista general con métricas, actividad reciente y acciones rápidas
 * Usa el mismo estilo visual que el Homepage principal
 */

import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import { useAuth } from '@/hooks/useAuth';
import { dealerManagementService } from '@/services/dealerManagementService';
import type { Dealer, DealerSubscription } from '@/services/dealerManagementService';
import {
  FiTrendingUp,
  FiTrendingDown,
  FiEye,
  FiMessageSquare,
  FiTarget,
  FiDollarSign,
  FiArrowRight,
  FiPlusCircle,
  FiUsers,
  FiCalendar,
  FiAlertCircle,
  FiStar,
  FiBarChart2,
  FiLoader,
  FiRefreshCw,
  FiEdit,
  FiZap,
  FiLock,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { Crown, Sparkles } from 'lucide-react';

// Tipos para métricas
interface DashboardMetrics {
  vehiclesActive: number;
  vehiclesMax: number;
  viewsThisMonth: number;
  viewsChange: number;
  leadsTotal: number;
  leadsHot: number;
  leadsNew: number;
  inquiriesTotal: number;
  inquiriesUnread: number;
  inventoryValue: number;
  conversionRate: number;
}

// Tipos para actividad reciente
interface RecentActivity {
  id: string;
  type: 'view' | 'inquiry' | 'lead' | 'sale' | 'review';
  title: string;
  description: string;
  time: string;
  icon: React.ElementType;
  iconColor: string;
  iconBg: string;
}

// Mock data para demo
const mockMetrics: DashboardMetrics = {
  vehiclesActive: 12,
  vehiclesMax: 50,
  viewsThisMonth: 2450,
  viewsChange: 15.3,
  leadsTotal: 48,
  leadsHot: 8,
  leadsNew: 12,
  inquiriesTotal: 156,
  inquiriesUnread: 7,
  inventoryValue: 1250000,
  conversionRate: 12.5,
};

const mockActivities: RecentActivity[] = [
  {
    id: '1',
    type: 'lead',
    title: 'Nuevo lead caliente',
    description: 'Juan Pérez interesado en Toyota Corolla 2022',
    time: 'Hace 5 minutos',
    icon: FiTarget,
    iconColor: 'text-red-600',
    iconBg: 'bg-red-100',
  },
  {
    id: '2',
    type: 'inquiry',
    title: 'Nueva consulta recibida',
    description: 'María García preguntó sobre Honda Civic 2021',
    time: 'Hace 1 hora',
    icon: FiMessageSquare,
    iconColor: 'text-blue-600',
    iconBg: 'bg-blue-100',
  },
  {
    id: '3',
    type: 'view',
    title: 'Vehículo popular',
    description: 'BMW X5 2023 alcanzó 100 vistas',
    time: 'Hace 3 horas',
    icon: FiEye,
    iconColor: 'text-purple-600',
    iconBg: 'bg-purple-100',
  },
  {
    id: '4',
    type: 'review',
    title: 'Nueva reseña ⭐⭐⭐⭐⭐',
    description: 'Cliente satisfecho con su compra',
    time: 'Hace 6 horas',
    icon: FiStar,
    iconColor: 'text-amber-600',
    iconBg: 'bg-amber-100',
  },
];

// Componente de tarjeta de métrica
const MetricCard = ({
  title,
  value,
  subtitle,
  icon: Icon,
  iconColor,
  iconBg,
  trend,
  trendValue,
}: {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ElementType;
  iconColor: string;
  iconBg: string;
  trend?: 'up' | 'down' | 'neutral';
  trendValue?: string;
}) => (
  <motion.div
    initial={{ opacity: 0, y: 20 }}
    animate={{ opacity: 1, y: 0 }}
    className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 hover:shadow-md transition-all"
  >
    <div className="flex items-start justify-between mb-4">
      <div className={`p-3 rounded-xl ${iconBg}`}>
        <Icon className={`w-6 h-6 ${iconColor}`} />
      </div>
      {trend && trendValue && (
        <div
          className={`flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
            trend === 'up'
              ? 'bg-green-100 text-green-700'
              : trend === 'down'
                ? 'bg-red-100 text-red-700'
                : 'bg-gray-100 text-gray-600'
          }`}
        >
          {trend === 'up' ? (
            <FiTrendingUp className="w-3 h-3" />
          ) : (
            <FiTrendingDown className="w-3 h-3" />
          )}
          {trendValue}
        </div>
      )}
    </div>
    <div>
      <h3 className="text-3xl font-bold text-gray-900 mb-1">{value}</h3>
      <p className="text-sm text-gray-500 font-medium">{title}</p>
      {subtitle && <p className="text-xs text-gray-400 mt-1">{subtitle}</p>}
    </div>
  </motion.div>
);

// Componente de acción rápida
const QuickAction = ({
  title,
  description,
  icon: Icon,
  href,
  gradient,
}: {
  title: string;
  description: string;
  icon: React.ElementType;
  href: string;
  gradient: string;
}) => (
  <Link
    to={href}
    className={`group relative overflow-hidden rounded-2xl p-5 text-white shadow-lg hover:shadow-xl transition-all hover:scale-[1.02] ${gradient}`}
  >
    <div className="relative z-10">
      <Icon className="w-8 h-8 mb-3 opacity-90" />
      <h3 className="text-lg font-bold mb-1">{title}</h3>
      <p className="text-sm opacity-80">{description}</p>
    </div>
    <div className="absolute top-0 right-0 -mt-4 -mr-4 w-24 h-24 bg-white/10 rounded-full blur-2xl" />
    <FiArrowRight className="absolute bottom-5 right-5 w-5 h-5 opacity-70 group-hover:translate-x-1 transition-transform" />
  </Link>
);

export default function DealerHomePage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [dealer, setDealer] = useState<Dealer | null>(null);
  const [subscription, setSubscription] = useState<DealerSubscription | null>(null);
  const [metrics, setMetrics] = useState<DashboardMetrics>(mockMetrics);
  const [activities] = useState<RecentActivity[]>(mockActivities);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDealerData();
  }, []);

  const loadDealerData = async () => {
    try {
      setLoading(true);
      const userId = localStorage.getItem('userId') || user?.id;
      if (!userId) {
        navigate('/login?redirect=/dealer/dashboard');
        return;
      }

      const dealerData = await dealerManagementService.getDealerByUserId(userId);
      setDealer(dealerData);

      // Cargar suscripción con límites y uso
      try {
        const subData = await dealerManagementService.getDealerSubscription(dealerData.id);
        setSubscription(subData);

        // Usar datos reales de la suscripción
        setMetrics({
          ...mockMetrics,
          vehiclesActive: subData.usage.currentListings,
          vehiclesMax: subData.limits.maxListings,
        });
      } catch (subErr) {
        console.log('No subscription found, using defaults');
        setMetrics({
          ...mockMetrics,
          vehiclesActive: dealerData.currentActiveListings || 0,
          vehiclesMax: dealerData.maxActiveListings || 3,
        });
      }
    } catch (err: any) {
      // Si no tiene perfil de dealer (404), mostramos el dashboard con datos vacíos
      // y un banner para completar el perfil
      if (err.response?.status === 404) {
        console.log('🐛 Dealer profile not found, showing empty dashboard');
        setDealer(null);
        setMetrics(mockMetrics);
      } else {
        setError(err.message || 'Error al cargar datos');
      }
    } finally {
      setLoading(false);
    }
  };

  // Plan badge helper
  const getPlanBadge = () => {
    if (!subscription) return null;

    const planStyles: Record<string, { bg: string; text: string; icon: React.ReactNode }> = {
      Free: { bg: 'bg-gray-100', text: 'text-gray-700', icon: null },
      Basic: { bg: 'bg-blue-100', text: 'text-blue-700', icon: <FiZap className="w-3 h-3" /> },
      Pro: { bg: 'bg-purple-100', text: 'text-purple-700', icon: <Crown className="w-3 h-3" /> },
      Enterprise: {
        bg: 'bg-amber-100',
        text: 'text-amber-700',
        icon: <Sparkles className="w-3 h-3" />,
      },
    };

    const style = planStyles[subscription.plan] || planStyles.Free;

    return (
      <span
        className={`inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-semibold ${style.bg} ${style.text}`}
      >
        {style.icon}
        Plan {subscription.planDisplayName}
      </span>
    );
  };

  if (loading) {
    return (
      <DealerPortalLayout>
        <div className="flex items-center justify-center min-h-[calc(100vh-4rem)]">
          <FiLoader className="w-10 h-10 animate-spin text-blue-600" />
        </div>
      </DealerPortalLayout>
    );
  }

  if (error) {
    return (
      <DealerPortalLayout>
        <div className="p-6">
          <div className="bg-red-50 border border-red-200 text-red-700 px-6 py-4 rounded-xl">
            {error}
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  const getStatusBadge = (status: string) => {
    const badges: Record<string, { bg: string; text: string; label: string }> = {
      Pending: { bg: 'bg-yellow-100', text: 'text-yellow-700', label: 'Pendiente' },
      UnderReview: { bg: 'bg-blue-100', text: 'text-blue-700', label: 'En Revisión' },
      Active: { bg: 'bg-green-100', text: 'text-green-700', label: 'Activo' },
      Suspended: { bg: 'bg-red-100', text: 'text-red-700', label: 'Suspendido' },
      Rejected: { bg: 'bg-red-100', text: 'text-red-700', label: 'Rechazado' },
    };
    const badge = badges[status] || badges.Pending;
    return (
      <span className={`${badge.bg} ${badge.text} px-3 py-1 rounded-full text-xs font-semibold`}>
        {badge.label}
      </span>
    );
  };

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 space-y-8">
        {/* ==================== BANNER: Complete Profile ==================== */}
        {!dealer && (
          <div className="bg-gradient-to-r from-amber-50 to-orange-50 border border-amber-200 rounded-2xl p-6">
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
              <div className="flex items-start gap-4">
                <div className="p-3 bg-amber-100 rounded-xl">
                  <FiAlertCircle className="w-6 h-6 text-amber-600" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-gray-900">Completa tu perfil de dealer</h3>
                  <p className="text-gray-600 mt-1">
                    Para publicar vehículos y acceder a todas las funciones, necesitas completar tu
                    información de negocio.
                  </p>
                </div>
              </div>
              <Link
                to="/dealer/register"
                className="inline-flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-amber-500 to-orange-500 text-white rounded-xl font-semibold hover:shadow-lg transition-all whitespace-nowrap"
              >
                <FiEdit className="w-4 h-4" />
                Completar Perfil
              </Link>
            </div>
          </div>
        )}

        {/* ==================== HEADER ==================== */}
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
          <div>
            <div className="flex items-center gap-3 mb-2 flex-wrap">
              <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">
                ¡Hola, {dealer?.businessName || user?.name || 'Dealer'}! 👋
              </h1>
              {dealer?.status && getStatusBadge(dealer.status)}
              {getPlanBadge()}
            </div>
            <p className="text-gray-500">
              Aquí tienes un resumen de tu negocio hoy,{' '}
              {new Date().toLocaleDateString('es-DO', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
              })}
            </p>
          </div>

          <div className="flex items-center gap-3">
            {subscription?.canUpgrade && (
              <Link
                to="/dealer/pricing"
                className="flex items-center gap-2 px-4 py-2.5 text-purple-600 hover:text-purple-700 hover:bg-purple-50 rounded-xl transition-colors border border-purple-200"
              >
                <FiZap className="w-4 h-4" />
                <span className="hidden sm:inline">Mejorar Plan</span>
              </Link>
            )}
            <button
              onClick={loadDealerData}
              className="flex items-center gap-2 px-4 py-2.5 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-xl transition-colors"
            >
              <FiRefreshCw className="w-4 h-4" />
              <span className="hidden sm:inline">Actualizar</span>
            </button>
            <Link
              to="/dealer/inventory/new"
              className={`flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold shadow-lg transition-all hover:scale-105 ${
                subscription?.limits.hasReachedListingLimit
                  ? 'bg-gray-400 cursor-not-allowed'
                  : 'bg-gradient-to-r from-blue-600 to-blue-500 text-white shadow-blue-600/30 hover:shadow-xl'
              }`}
              onClick={(e) => {
                if (subscription?.limits.hasReachedListingLimit) {
                  e.preventDefault();
                  alert(
                    `Has alcanzado el límite de ${subscription.limits.maxListings} publicaciones de tu plan ${subscription.planDisplayName}. Mejora tu plan para publicar más vehículos.`
                  );
                }
              }}
            >
              {subscription?.limits.hasReachedListingLimit ? (
                <FiLock className="w-5 h-5" />
              ) : (
                <FiPlusCircle className="w-5 h-5" />
              )}
              <span>Publicar Vehículo</span>
            </Link>
          </div>
        </div>

        {/* ==================== SUBSCRIPTION USAGE CARD ==================== */}
        {subscription && (
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-2xl p-6"
          >
            <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
              <div className="flex-1">
                <div className="flex items-center gap-3 mb-4">
                  <div className="p-2 bg-blue-100 rounded-xl">
                    <FiBarChart2 className="w-5 h-5 text-blue-600" />
                  </div>
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">
                      Uso de tu Plan {subscription.planDisplayName}
                    </h3>
                    <p className="text-sm text-gray-500">
                      {subscription.plan === 'Enterprise'
                        ? 'Publicaciones ilimitadas'
                        : `${subscription.limits.remainingListings} publicaciones disponibles de ${subscription.limits.maxListings}`}
                    </p>
                  </div>
                </div>

                {/* Progress Bar */}
                {subscription.plan !== 'Enterprise' && (
                  <div className="relative pt-1">
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-xs font-semibold text-blue-700">
                        {subscription.usage.currentListings} / {subscription.limits.maxListings}{' '}
                        publicaciones
                      </span>
                      <span
                        className={`text-xs font-semibold ${
                          subscription.limits.listingsUsagePercent >= 90
                            ? 'text-red-600'
                            : subscription.limits.listingsUsagePercent >= 70
                              ? 'text-amber-600'
                              : 'text-green-600'
                        }`}
                      >
                        {subscription.limits.listingsUsagePercent.toFixed(0)}%
                      </span>
                    </div>
                    <div className="overflow-hidden h-3 text-xs flex rounded-full bg-blue-200">
                      <motion.div
                        initial={{ width: 0 }}
                        animate={{
                          width: `${Math.min(subscription.limits.listingsUsagePercent, 100)}%`,
                        }}
                        transition={{ duration: 1, ease: 'easeOut' }}
                        className={`shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center rounded-full ${
                          subscription.limits.listingsUsagePercent >= 90
                            ? 'bg-red-500'
                            : subscription.limits.listingsUsagePercent >= 70
                              ? 'bg-amber-500'
                              : 'bg-blue-600'
                        }`}
                      />
                    </div>
                    {subscription.limits.hasReachedListingLimit && (
                      <p className="text-xs text-red-600 mt-2 flex items-center gap-1">
                        <FiAlertCircle className="w-3 h-3" />
                        Has alcanzado el límite de tu plan. Mejora para publicar más vehículos.
                      </p>
                    )}
                  </div>
                )}
              </div>

              {/* Features Grid */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 lg:gap-4">
                <div
                  className={`flex items-center gap-2 px-3 py-2 rounded-lg ${
                    subscription.features.analyticsAccess
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-400'
                  }`}
                >
                  <FiBarChart2 className="w-4 h-4" />
                  <span className="text-xs font-medium">Analytics</span>
                </div>
                <div
                  className={`flex items-center gap-2 px-3 py-2 rounded-lg ${
                    subscription.features.bulkUpload
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-400'
                  }`}
                >
                  <FiUsers className="w-4 h-4" />
                  <span className="text-xs font-medium">Carga Masiva</span>
                </div>
                <div
                  className={`flex items-center gap-2 px-3 py-2 rounded-lg ${
                    subscription.features.leadManagement
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-400'
                  }`}
                >
                  <FiTarget className="w-4 h-4" />
                  <span className="text-xs font-medium">CRM Leads</span>
                </div>
                <div
                  className={`flex items-center gap-2 px-3 py-2 rounded-lg ${
                    subscription.features.prioritySupport
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-400'
                  }`}
                >
                  <FiStar className="w-4 h-4" />
                  <span className="text-xs font-medium">Soporte Pro</span>
                </div>
              </div>

              {subscription.canUpgrade && (
                <Link
                  to="/dealer/pricing"
                  className="flex items-center gap-2 px-5 py-3 bg-gradient-to-r from-purple-600 to-indigo-600 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all whitespace-nowrap"
                >
                  <FiZap className="w-5 h-5" />
                  Mejorar a {subscription.nextPlan}
                </Link>
              )}
            </div>
          </motion.div>
        )}

        {/* ==================== VERIFICATION ALERT ==================== */}
        {dealer?.verificationStatus !== 'Verified' && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-gradient-to-r from-amber-50 to-orange-50 border border-amber-200 rounded-2xl p-5"
          >
            <div className="flex items-start gap-4">
              <div className="p-2 bg-amber-100 rounded-xl">
                <FiAlertCircle className="w-6 h-6 text-amber-600" />
              </div>
              <div className="flex-1">
                <h3 className="text-amber-800 font-semibold mb-1">Verificación Pendiente</h3>
                <p className="text-amber-700 text-sm mb-3">
                  Tu cuenta está en proceso de verificación. Sube los documentos requeridos para
                  activar todas las funciones.
                </p>
                <Link
                  to="/dealer/settings/documents"
                  className="inline-flex items-center gap-2 px-4 py-2 bg-amber-600 text-white rounded-lg text-sm font-medium hover:bg-amber-700 transition-colors"
                >
                  Subir Documentos
                  <FiArrowRight className="w-4 h-4" />
                </Link>
              </div>
            </div>
          </motion.div>
        )}

        {/* ==================== MÉTRICAS PRINCIPALES ==================== */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6">
          <MetricCard
            title="Vehículos Activos"
            value={metrics.vehiclesActive}
            subtitle={`${metrics.vehiclesMax - metrics.vehiclesActive} disponibles`}
            icon={FaCar}
            iconColor="text-blue-600"
            iconBg="bg-blue-100"
          />
          <MetricCard
            title="Vistas Este Mes"
            value={metrics.viewsThisMonth.toLocaleString()}
            icon={FiEye}
            iconColor="text-purple-600"
            iconBg="bg-purple-100"
            trend={metrics.viewsChange >= 0 ? 'up' : 'down'}
            trendValue={`${metrics.viewsChange >= 0 ? '+' : ''}${metrics.viewsChange}%`}
          />
          <MetricCard
            title="Leads Totales"
            value={metrics.leadsTotal}
            subtitle={`${metrics.leadsHot} calientes 🔥`}
            icon={FiTarget}
            iconColor="text-red-600"
            iconBg="bg-red-100"
          />
          <MetricCard
            title="Consultas"
            value={metrics.inquiriesTotal}
            subtitle={`${metrics.inquiriesUnread} sin responder`}
            icon={FiMessageSquare}
            iconColor="text-emerald-600"
            iconBg="bg-emerald-100"
          />
        </div>

        {/* ==================== MÉTRICAS SECUNDARIAS ==================== */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 lg:gap-6">
          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <span className="text-gray-500 font-medium">Valor Inventario</span>
              <FiDollarSign className="w-5 h-5 text-green-600" />
            </div>
            <p className="text-2xl font-bold text-gray-900">
              ${(metrics.inventoryValue / 1000000).toFixed(2)}M
            </p>
            <p className="text-xs text-gray-400 mt-1">
              Promedio: $
              {Math.round(
                metrics.inventoryValue / Math.max(metrics.vehiclesActive, 1)
              ).toLocaleString()}
              /vehículo
            </p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <span className="text-gray-500 font-medium">Tasa de Conversión</span>
              <FiTrendingUp className="w-5 h-5 text-blue-600" />
            </div>
            <p className="text-2xl font-bold text-gray-900">{metrics.conversionRate}%</p>
            <p className="text-xs text-gray-400 mt-1">Lead → Venta</p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <span className="text-gray-500 font-medium">Nuevos Leads</span>
              <FiUsers className="w-5 h-5 text-amber-600" />
            </div>
            <p className="text-2xl font-bold text-gray-900">{metrics.leadsNew}</p>
            <p className="text-xs text-gray-400 mt-1">Últimos 7 días</p>
          </div>
        </div>

        {/* ==================== ACCIONES RÁPIDAS ==================== */}
        <div>
          <h2 className="text-lg font-bold text-gray-900 mb-4">Acciones Rápidas</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <QuickAction
              title="Publicar Vehículo"
              description="Agrega un nuevo vehículo a tu inventario"
              icon={FiPlusCircle}
              href="/dealer/inventory/new"
              gradient="bg-gradient-to-br from-blue-600 to-blue-700"
            />
            <QuickAction
              title="Ver Leads"
              description={`${metrics.leadsHot} leads calientes esperan`}
              icon={FiTarget}
              href="/dealer/crm"
              gradient="bg-gradient-to-br from-red-500 to-orange-500"
            />
            <QuickAction
              title="Consultas"
              description={`${metrics.inquiriesUnread} mensajes sin leer`}
              icon={FiMessageSquare}
              href="/dealer/inquiries"
              gradient="bg-gradient-to-br from-emerald-500 to-teal-500"
            />
            <QuickAction
              title="Analytics"
              description="Revisa el rendimiento de tu negocio"
              icon={FiBarChart2}
              href="/dealer/analytics"
              gradient="bg-gradient-to-br from-purple-600 to-indigo-600"
            />
          </div>
        </div>

        {/* ==================== CONTENIDO PRINCIPAL ==================== */}
        <div className="grid lg:grid-cols-3 gap-6">
          {/* Actividad Reciente */}
          <div className="lg:col-span-2 bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="p-5 border-b border-gray-100 flex items-center justify-between">
              <h2 className="text-lg font-bold text-gray-900">Actividad Reciente</h2>
              <Link
                to="/dealer/activity"
                className="text-blue-600 text-sm font-medium hover:underline"
              >
                Ver todo
              </Link>
            </div>
            <div className="divide-y divide-gray-50">
              {activities.map((activity) => (
                <div key={activity.id} className="p-5 hover:bg-gray-50 transition-colors">
                  <div className="flex items-start gap-4">
                    <div className={`p-2.5 rounded-xl ${activity.iconBg}`}>
                      <activity.icon className={`w-5 h-5 ${activity.iconColor}`} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-semibold text-gray-900">{activity.title}</p>
                      <p className="text-sm text-gray-500 truncate">{activity.description}</p>
                    </div>
                    <span className="text-xs text-gray-400 whitespace-nowrap">{activity.time}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Panel Lateral */}
          <div className="space-y-6">
            {/* Leads Calientes */}
            <div className="bg-gradient-to-br from-red-500 to-orange-500 rounded-2xl p-6 text-white">
              <div className="flex items-center gap-3 mb-4">
                <div className="p-2 bg-white/20 rounded-xl">
                  <FiTarget className="w-6 h-6" />
                </div>
                <h3 className="text-lg font-bold">Leads Calientes 🔥</h3>
              </div>
              <p className="text-3xl font-bold mb-1">{metrics.leadsHot}</p>
              <p className="text-sm opacity-80 mb-4">Listos para contactar</p>
              <Link
                to="/dealer/crm"
                className="flex items-center justify-center gap-2 w-full py-2.5 bg-white text-red-600 rounded-xl font-semibold hover:bg-gray-100 transition-colors"
              >
                Ver Leads
                <FiArrowRight className="w-4 h-4" />
              </Link>
            </div>

            {/* Próximas Citas */}
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-bold text-gray-900">Próximas Citas</h3>
                <FiCalendar className="w-5 h-5 text-gray-400" />
              </div>
              <div className="space-y-3">
                <div className="flex items-center gap-3 p-3 bg-blue-50 rounded-xl">
                  <div className="w-12 h-12 bg-blue-100 rounded-lg flex flex-col items-center justify-center">
                    <span className="text-xs text-blue-600 font-medium">HOY</span>
                    <span className="text-lg font-bold text-blue-700">15</span>
                  </div>
                  <div className="flex-1">
                    <p className="font-semibold text-gray-900 text-sm">Test Drive - BMW X5</p>
                    <p className="text-xs text-gray-500">3:00 PM - Carlos Méndez</p>
                  </div>
                </div>
                <div className="flex items-center gap-3 p-3 bg-gray-50 rounded-xl">
                  <div className="w-12 h-12 bg-gray-100 rounded-lg flex flex-col items-center justify-center">
                    <span className="text-xs text-gray-500 font-medium">MAR</span>
                    <span className="text-lg font-bold text-gray-700">16</span>
                  </div>
                  <div className="flex-1">
                    <p className="font-semibold text-gray-900 text-sm">Entrega - Toyota Corolla</p>
                    <p className="text-xs text-gray-500">10:00 AM - Ana López</p>
                  </div>
                </div>
              </div>
              <Link
                to="/dealer/appointments"
                className="mt-4 flex items-center justify-center gap-2 w-full py-2.5 border border-gray-200 rounded-xl text-gray-700 font-medium hover:bg-gray-50 transition-colors"
              >
                Ver Todas
              </Link>
            </div>

            {/* Upgrade Banner */}
            <div className="bg-gradient-to-br from-purple-600 to-indigo-600 rounded-2xl p-6 text-white">
              <Sparkles className="w-8 h-8 mb-3 opacity-90" />
              <h3 className="text-lg font-bold mb-2">Desbloquea más poder</h3>
              <p className="text-sm opacity-80 mb-4">
                Actualiza a Pro y obtén CRM avanzado, analytics y más.
              </p>
              <Link
                to="/dealer/plans"
                className="flex items-center justify-center gap-2 w-full py-2.5 bg-white text-purple-600 rounded-xl font-semibold hover:bg-gray-100 transition-colors"
              >
                <Crown className="w-4 h-4" />
                Ver Planes
              </Link>
            </div>
          </div>
        </div>
      </div>
    </DealerPortalLayout>
  );
}
