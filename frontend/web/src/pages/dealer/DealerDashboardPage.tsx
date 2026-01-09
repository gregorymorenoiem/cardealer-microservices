/**
 * Dealer Dashboard Page
 *
 * Main dashboard for dealers that shows:
 * - Plan-based feature access
 * - Usage metrics and limits
 * - Quick actions based on permissions
 * - CRM overview (if available)
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Car,
  Users,
  TrendingUp,
  Calendar,
  DollarSign,
  Star,
  AlertTriangle,
  ArrowUp,
  BarChart3,
  Mail,
  Phone,
  FileText,
  Settings,
  Crown,
  Lock,
  Plus,
  Eye,
} from 'lucide-react';
import { usePermissions } from '@/hooks/usePermissions';
import { crmService } from '@/services/crmService';
import type { CRMStats } from '@/mocks/crmData';
import {
  vehicleIntelligenceService,
  type CategoryDemandDto,
} from '@/services/vehicleIntelligenceService';

// ============================================================================
// COMPONENTS
// ============================================================================

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  trend?: { value: number; isPositive: boolean };
  color?: string;
}

const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  subtitle,
  icon,
  trend,
  color = 'blue',
}) => {
  const colorClasses: Record<string, string> = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    purple: 'bg-purple-50 text-purple-600',
    amber: 'bg-amber-50 text-amber-600',
    red: 'bg-red-50 text-red-600',
    indigo: 'bg-indigo-50 text-indigo-600',
  };

  return (
    <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm font-medium text-gray-500">{title}</p>
          <p className="mt-2 text-3xl font-bold text-gray-900">{value}</p>
          {subtitle && <p className="mt-1 text-sm text-gray-500">{subtitle}</p>}
          {trend && (
            <div
              className={`mt-2 flex items-center text-sm ${
                trend.isPositive ? 'text-green-600' : 'text-red-600'
              }`}
            >
              <ArrowUp className={`h-4 w-4 ${!trend.isPositive && 'rotate-180'}`} />
              <span className="ml-1">{trend.value}% vs mes anterior</span>
            </div>
          )}
        </div>
        <div className={`p-3 rounded-lg ${colorClasses[color]}`}>{icon}</div>
      </div>
    </div>
  );
};

interface UsageBarProps {
  label: string;
  current: number;
  max: number;
  showWarning?: boolean;
}

const UsageBar: React.FC<UsageBarProps> = ({ label, current, max, showWarning }) => {
  const percentage = max === Infinity ? 0 : Math.min(100, (current / max) * 100);
  const isAtLimit = max !== Infinity && current >= max;
  const isNearLimit = max !== Infinity && percentage >= 80 && !isAtLimit;

  return (
    <div className="space-y-2">
      <div className="flex justify-between items-center">
        <span className="text-sm font-medium text-gray-700">{label}</span>
        <span className="text-sm text-gray-500">
          {current} / {max === Infinity ? '∞' : max}
        </span>
      </div>
      <div className="w-full h-2 bg-gray-100 rounded-full overflow-hidden">
        <div
          className={`h-full rounded-full transition-all ${
            isAtLimit ? 'bg-red-500' : isNearLimit ? 'bg-amber-500' : 'bg-blue-500'
          }`}
          style={{ width: max === Infinity ? '20%' : `${percentage}%` }}
        />
      </div>
      {showWarning && isAtLimit && (
        <p className="text-xs text-red-600 flex items-center gap-1">
          <AlertTriangle className="h-3 w-3" />
          Límite alcanzado
        </p>
      )}
      {showWarning && isNearLimit && (
        <p className="text-xs text-amber-600 flex items-center gap-1">
          <AlertTriangle className="h-3 w-3" />
          Cerca del límite
        </p>
      )}
    </div>
  );
};

interface FeatureCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  isAvailable: boolean;
  href?: string;
  requiredPlan?: string;
}

const FeatureCard: React.FC<FeatureCardProps> = ({
  title,
  description,
  icon,
  isAvailable,
  href,
  requiredPlan,
}) => {
  const content = (
    <div
      className={`relative bg-white rounded-xl p-6 shadow-sm border transition-all ${
        isAvailable
          ? 'border-gray-100 hover:border-blue-200 hover:shadow-md cursor-pointer'
          : 'border-gray-100 opacity-60'
      }`}
    >
      {!isAvailable && (
        <div className="absolute top-3 right-3">
          <div className="flex items-center gap-1 text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded-full">
            <Lock className="h-3 w-3" />
            {requiredPlan || 'Upgrade'}
          </div>
        </div>
      )}
      <div className="flex items-start gap-4">
        <div
          className={`p-3 rounded-lg ${
            isAvailable ? 'bg-blue-50 text-blue-600' : 'bg-gray-100 text-gray-400'
          }`}
        >
          {icon}
        </div>
        <div className="flex-1">
          <h3 className={`font-semibold ${isAvailable ? 'text-gray-900' : 'text-gray-500'}`}>
            {title}
          </h3>
          <p className="mt-1 text-sm text-gray-500">{description}</p>
        </div>
      </div>
    </div>
  );

  if (isAvailable && href) {
    return <Link to={href}>{content}</Link>;
  }

  return content;
};

// ============================================================================
// MAIN PAGE
// ============================================================================

export default function DealerDashboardPage() {
  const { t } = useTranslation(['dealer', 'common']);
  const {
    user,
    dealerPlan,
    portalAccess,
    serviceAccess,
    usage,
    limits,
    hasReachedLimit,
    getUsagePercentage,
    canAccessFeature,
    needsUpgrade,
    getNextPlan,
  } = usePermissions();

  const [crmStats, setCrmStats] = useState<CRMStats | null>(null);
  const [, setIsLoading] = useState(true);
  const [categoryDemand, setCategoryDemand] = useState<CategoryDemandDto[] | null>(null);
  const [categoryDemandLoading, setCategoryDemandLoading] = useState(false);
  const [categoryDemandError, setCategoryDemandError] = useState<string | null>(null);

  useEffect(() => {
    const loadData = async () => {
      setIsLoading(true);
      try {
        if (user?.dealerId && portalAccess.crm) {
          const stats = await crmService.stats.getStats(user.dealerId);
          setCrmStats(stats);
        }

        if (user) {
          setCategoryDemandLoading(true);
          setCategoryDemandError(null);
          try {
            const demand = await vehicleIntelligenceService.getCategoryDemand();
            setCategoryDemand(demand);
          } catch (error) {
            setCategoryDemand(null);
            setCategoryDemandError(
              error instanceof Error ? error.message : 'No se pudo cargar la demanda por categoría'
            );
          } finally {
            setCategoryDemandLoading(false);
          }
        }
      } catch (error) {
        console.error('Error loading dashboard data:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, [user, user?.dealerId, portalAccess.crm]);

  const nextPlan = getNextPlan();

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{t('dealer:dashboard.title')}</h1>
            <p className="text-gray-500">
              {t('dealer:dashboard.welcome', { name: user?.name?.split(' ')[0] || 'Usuario' })}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <div
              className={`flex items-center gap-2 px-4 py-2 rounded-full ${
                dealerPlan === 'enterprise'
                  ? 'bg-purple-100 text-purple-700'
                  : dealerPlan === 'pro'
                    ? 'bg-blue-100 text-blue-700'
                    : dealerPlan === 'basic'
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-700'
              }`}
            >
              <Crown className="h-4 w-4" />
              <span className="font-medium capitalize">{dealerPlan || 'Free'}</span>
            </div>

            {nextPlan && (
              <Link
                to="/dealer/plans"
                className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-blue-600 to-purple-600 text-white rounded-full hover:opacity-90 transition-opacity"
              >
                <ArrowUp className="h-4 w-4" />
                Upgrade a {nextPlan}
              </Link>
            )}
          </div>
        </div>

        {/* Usage Warnings */}
        {(hasReachedLimit('listings') || getUsagePercentage('listings') >= 80) && (
          <div
            className={`rounded-xl p-4 ${
              hasReachedLimit('listings')
                ? 'bg-red-50 border border-red-200'
                : 'bg-amber-50 border border-amber-200'
            }`}
          >
            <div className="flex items-start gap-3">
              <AlertTriangle
                className={`h-5 w-5 ${
                  hasReachedLimit('listings') ? 'text-red-600' : 'text-amber-600'
                }`}
              />
              <div className="flex-1">
                <h3
                  className={`font-medium ${
                    hasReachedLimit('listings') ? 'text-red-800' : 'text-amber-800'
                  }`}
                >
                  {hasReachedLimit('listings')
                    ? t('dealer:dashboard.limitReached')
                    : t('dealer:dashboard.nearLimit')}
                </h3>
                <p
                  className={`text-sm mt-1 ${
                    hasReachedLimit('listings') ? 'text-red-600' : 'text-amber-600'
                  }`}
                >
                  {hasReachedLimit('listings')
                    ? t('dealer:dashboard.limitReachedMessage', {
                        current: usage.listings,
                        max: limits.maxListings,
                      })
                    : t('dealer:dashboard.nearLimitMessage', {
                        current: usage.listings,
                        max: limits.maxListings,
                        percentage: Math.round(getUsagePercentage('listings')),
                      })}
                </p>
              </div>
              {nextPlan && (
                <Link
                  to="/dealer/plans"
                  className="px-3 py-1.5 bg-white rounded-lg text-sm font-medium hover:bg-gray-50"
                >
                  {t('dealer:dashboard.viewPlans')}
                </Link>
              )}
            </div>
          </div>
        )}

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            title="Publicaciones Activas"
            value={usage.listings}
            subtitle={`de ${limits.maxListings === Infinity ? '∞' : limits.maxListings} disponibles`}
            icon={<Car className="h-6 w-6" />}
            color="blue"
          />

          {portalAccess.crm && crmStats ? (
            <>
              <StatCard
                title="Leads Nuevos"
                value={crmStats.newLeadsThisMonth}
                subtitle="este mes"
                icon={<Users className="h-6 w-6" />}
                trend={{ value: 12, isPositive: true }}
                color="green"
              />
              <StatCard
                title="Deals Abiertos"
                value={crmStats.openDeals}
                subtitle={`$${(crmStats.expectedRevenue / 1000000).toFixed(1)}M potencial`}
                icon={<TrendingUp className="h-6 w-6" />}
                color="purple"
              />
              <StatCard
                title="Conversión"
                value={`${crmStats.conversionRate}%`}
                subtitle="leads a ventas"
                icon={<DollarSign className="h-6 w-6" />}
                trend={{ value: 5, isPositive: true }}
                color="amber"
              />
            </>
          ) : (
            <>
              <StatCard
                title="Destacados Usados"
                value={usage.featuredListings}
                subtitle={`de ${limits.maxFeaturedListings} disponibles`}
                icon={<Star className="h-6 w-6" />}
                color="amber"
              />
              <StatCard
                title="Vistas este Mes"
                value="1,234"
                icon={<Eye className="h-6 w-6" />}
                trend={{ value: 8, isPositive: true }}
                color="green"
              />
              <StatCard
                title="Contactos"
                value="56"
                subtitle="solicitudes recibidas"
                icon={<Phone className="h-6 w-6" />}
                color="purple"
              />
            </>
          )}
        </div>

        {/* Usage Section */}
        <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
          <h2 className="text-lg font-semibold text-gray-900 mb-6">Uso del Plan</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <UsageBar
              label="Publicaciones"
              current={usage.listings}
              max={limits.maxListings}
              showWarning
            />
            <UsageBar
              label="Destacados"
              current={usage.featuredListings}
              max={limits.maxFeaturedListings}
              showWarning
            />
          </div>
        </div>

        {/* Category Demand (Sprint 18) */}
        <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Demanda por categoría</h2>
          <p className="text-sm text-gray-500 mb-4">
            Señales del mercado para ayudarte a enfocar inventario y pricing.
          </p>

          {categoryDemandLoading && (
            <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
              <p className="text-sm text-gray-700">Cargando demanda…</p>
            </div>
          )}

          {categoryDemandError && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3">
              <p className="text-sm text-red-700">{categoryDemandError}</p>
            </div>
          )}

          {!categoryDemandLoading &&
            !categoryDemandError &&
            categoryDemand &&
            categoryDemand.length > 0 && (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-left text-gray-500 border-b">
                      <th className="py-2 pr-4 font-medium">Categoría</th>
                      <th className="py-2 pr-4 font-medium">Demanda</th>
                      <th className="py-2 pr-4 font-medium">Tendencia</th>
                      <th className="py-2 pr-4 font-medium">Actualizado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {categoryDemand.slice(0, 8).map((item) => {
                      const score = Math.round(item.demandScore);
                      const demandPillClass =
                        score >= 75
                          ? 'bg-green-50 text-green-700 border-green-200'
                          : score >= 50
                            ? 'bg-amber-50 text-amber-700 border-amber-200'
                            : 'bg-red-50 text-red-700 border-red-200';

                      const isUp = String(item.trend).toLowerCase() === 'up';
                      const isDown = String(item.trend).toLowerCase() === 'down';

                      return (
                        <tr
                          key={`${item.category}-${item.updatedAt}`}
                          className="border-b last:border-b-0"
                        >
                          <td className="py-3 pr-4 font-medium text-gray-900">{item.category}</td>
                          <td className="py-3 pr-4">
                            <span
                              className={`inline-flex items-center px-2.5 py-1 rounded-full border text-xs font-medium ${demandPillClass}`}
                            >
                              {score}/100
                            </span>
                          </td>
                          <td className="py-3 pr-4">
                            <span
                              className={`inline-flex items-center gap-1 text-sm ${
                                isUp ? 'text-green-600' : isDown ? 'text-red-600' : 'text-gray-500'
                              }`}
                            >
                              <ArrowUp
                                className={`h-4 w-4 ${isDown ? 'rotate-180' : ''} ${!isUp && !isDown ? 'opacity-40' : ''}`}
                              />
                              {isUp ? 'Subiendo' : isDown ? 'Bajando' : 'Estable'}
                            </span>
                          </td>
                          <td className="py-3 pr-4 text-gray-500">
                            {new Date(item.updatedAt).toLocaleDateString('es-DO')}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}

          {!categoryDemandLoading &&
            !categoryDemandError &&
            (!categoryDemand || categoryDemand.length === 0) && (
              <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
                <p className="text-sm text-gray-700">Aún no hay datos de demanda disponibles.</p>
              </div>
            )}
        </div>

        {/* Features Grid */}
        <div>
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Herramientas</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {/* Always Available */}
            <FeatureCard
              title="Mis Publicaciones"
              description="Administra tus vehículos publicados"
              icon={<Car className="h-6 w-6" />}
              isAvailable={true}
              href="/dealer/listings"
            />

            <FeatureCard
              title="Nueva Publicación"
              description={
                hasReachedLimit('listings')
                  ? 'Has alcanzado el límite de publicaciones'
                  : 'Publica un nuevo vehículo'
              }
              icon={<Plus className="h-6 w-6" />}
              isAvailable={!hasReachedLimit('listings')}
              href="/dealer/listings/new"
              requiredPlan={nextPlan || undefined}
            />

            <FeatureCard
              title="Citas"
              description="Gestiona citas con clientes"
              icon={<Calendar className="h-6 w-6" />}
              isAvailable={serviceAccess?.appointmentService || false}
              href="/dealer/appointments"
            />

            {/* Plan-based Features */}
            <FeatureCard
              title="CRM"
              description="Gestión de leads y pipeline de ventas"
              icon={<Users className="h-6 w-6" />}
              isAvailable={portalAccess.crm}
              href="/dealer/crm"
              requiredPlan={needsUpgrade('leadManagement') ? 'PRO' : undefined}
            />

            <FeatureCard
              title="Analytics"
              description="Estadísticas y métricas de rendimiento"
              icon={<BarChart3 className="h-6 w-6" />}
              isAvailable={canAccessFeature('analyticsAccess')}
              href="/dealer/analytics"
              requiredPlan={needsUpgrade('analyticsAccess') ? 'BASIC' : undefined}
            />

            <FeatureCard
              title="Marketing"
              description="Campañas de email y automatización"
              icon={<Mail className="h-6 w-6" />}
              isAvailable={canAccessFeature('emailAutomation')}
              href="/dealer/marketing"
              requiredPlan={needsUpgrade('emailAutomation') ? 'PRO' : undefined}
            />

            <FeatureCard
              title="Reportes"
              description="Reportes personalizados y exportación"
              icon={<FileText className="h-6 w-6" />}
              isAvailable={portalAccess.reports}
              href="/dealer/reports"
              requiredPlan={needsUpgrade('analyticsAccess') ? 'BASIC' : undefined}
            />

            <FeatureCard
              title="Facturación"
              description="Gestiona tu suscripción y pagos"
              icon={<DollarSign className="h-6 w-6" />}
              isAvailable={true}
              href="/dealer/billing"
            />

            <FeatureCard
              title="Configuración"
              description="Preferencias y configuración de cuenta"
              icon={<Settings className="h-6 w-6" />}
              isAvailable={true}
              href="/dealer/settings"
            />
          </div>
        </div>

        {/* CRM Quick View (if available) */}
        {portalAccess.crm && crmStats && (
          <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-lg font-semibold text-gray-900">Pipeline de Ventas</h2>
              <Link to="/dealer/crm" className="text-sm text-blue-600 hover:text-blue-700">
                Ver todo →
              </Link>
            </div>

            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center p-4 bg-gray-50 rounded-lg">
                <p className="text-2xl font-bold text-gray-900">{crmStats.totalLeads}</p>
                <p className="text-sm text-gray-500">Total Leads</p>
              </div>
              <div className="text-center p-4 bg-blue-50 rounded-lg">
                <p className="text-2xl font-bold text-blue-700">{crmStats.qualifiedLeads}</p>
                <p className="text-sm text-blue-600">Calificados</p>
              </div>
              <div className="text-center p-4 bg-green-50 rounded-lg">
                <p className="text-2xl font-bold text-green-700">{crmStats.wonDeals}</p>
                <p className="text-sm text-green-600">Cerrados</p>
              </div>
              <div className="text-center p-4 bg-amber-50 rounded-lg">
                <p className="text-2xl font-bold text-amber-700">{crmStats.activitiesDueToday}</p>
                <p className="text-sm text-amber-600">Actividades Hoy</p>
              </div>
            </div>
          </div>
        )}

        {/* Upgrade CTA for Free/Basic */}
        {(dealerPlan === 'free' || dealerPlan === 'basic') && (
          <div className="bg-gradient-to-r from-blue-600 to-purple-600 rounded-xl p-6 text-white">
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
              <div>
                <h2 className="text-xl font-bold">
                  {dealerPlan === 'free' ? '¿Listo para crecer?' : 'Desbloquea más herramientas'}
                </h2>
                <p className="mt-1 text-blue-100">
                  {dealerPlan === 'free'
                    ? 'Actualiza a BASIC para publicar hasta 50 vehículos y acceder a analytics'
                    : 'Actualiza a PRO para acceder a CRM, marketing y más'}
                </p>
              </div>
              <Link
                to="/dealer/plans"
                className="px-6 py-3 bg-white text-blue-600 rounded-lg font-semibold hover:bg-blue-50 transition-colors text-center"
              >
                Ver Planes
              </Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
