'use client';

import { useState, useEffect, useCallback } from 'react';
import {
  Activity,
  TrendingUp,
  Eye,
  MousePointerClick,
  DollarSign,
  Users,
  RefreshCw,
  Zap,
  Phone,
  MessageSquare,
  Trophy,
  Clock,
  BarChart3,
  Target,
  ArrowUpRight,
  ArrowDownRight,
} from 'lucide-react';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

interface HourlyDataPoint {
  hour: number;
  label: string;
  impressions: number;
  clicks: number;
  ctr: number;
  spend: number;
  leads: number;
}

interface CampaignLive {
  id: string;
  name: string;
  vehicleTitle: string;
  vehicleImage?: string;
  status: 'active' | 'paused';
  placement: string;
  budget: { total: number; spent: number; daily: number; spentToday: number };
  metricsToday: { impressions: number; clicks: number; ctr: number; leads: number; spend: number };
  metricsLifetime: {
    impressions: number;
    clicks: number;
    ctr: number;
    leads: number;
    spend: number;
  };
  qualityScore: number;
  lastClickAt: string;
  lastLeadAt: string;
}

interface ActivityItem {
  type: 'click' | 'impression' | 'lead' | 'milestone';
  message: string;
  time: string;
  campaign: string;
}

interface LiveDashboardData {
  ownerId: string;
  date: string;
  lastUpdated: string;
  refreshIntervalMs: number;
  summary: {
    activeCampaigns: number;
    totalBudget: number;
    totalSpent: number;
    todayImpressions: number;
    todayClicks: number;
    todayCTR: number;
    todaySpend: number;
    todayLeads: number;
    todayCostPerLead: number;
    avgQualityScore: number;
  };
  hourlyData: HourlyDataPoint[];
  campaigns: CampaignLive[];
  recentActivity: ActivityItem[];
}

// =============================================================================
// MAIN PAGE
// =============================================================================

export default function LiveDashboardPage() {
  const [data, setData] = useState<LiveDashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());
  const [pulse, setPulse] = useState(false);

  const fetchData = useCallback(async () => {
    try {
      const res = await fetch('/api/advertising/live-dashboard?ownerType=dealer');
      if (res.ok) {
        const json = await res.json();
        setData(json.data);
        setLastRefresh(new Date());
        setPulse(true);
        setTimeout(() => setPulse(false), 1000);
      }
    } catch {
      /* silent */
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    if (!autoRefresh) return;
    const interval = setInterval(fetchData, 30000); // Refresh every 30s
    return () => clearInterval(interval);
  }, [autoRefresh, fetchData]);

  if (loading || !data) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <div className="text-center">
          <RefreshCw className="mx-auto mb-3 h-8 w-8 animate-spin text-blue-600" />
          <p className="text-gray-600">Cargando dashboard en vivo...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-7xl space-y-6 px-4 py-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="flex items-center gap-2 text-2xl font-bold text-gray-900">
              <Activity className="h-7 w-7 text-green-500" />
              Dashboard en Vivo
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              Resultados en tiempo real de tu inversión publicitaria
            </p>
          </div>
          <div className="flex items-center gap-3">
            {/* Live indicator */}
            <div className="flex items-center gap-2 rounded-lg border bg-white px-3 py-2 shadow-sm">
              <span
                className={cn(
                  'h-2.5 w-2.5 rounded-full',
                  autoRefresh ? 'animate-pulse bg-green-500' : 'bg-gray-400'
                )}
              />
              <span className="text-sm text-gray-600">{autoRefresh ? 'EN VIVO' : 'Pausado'}</span>
            </div>

            <button
              onClick={() => setAutoRefresh(!autoRefresh)}
              className={cn(
                'rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                autoRefresh
                  ? 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  : 'bg-green-600 text-white hover:bg-green-700'
              )}
            >
              {autoRefresh ? 'Pausar' : 'Reanudar'}
            </button>

            <button
              onClick={fetchData}
              className="rounded-lg bg-blue-50 p-2 text-blue-600 transition-colors hover:bg-blue-100"
            >
              <RefreshCw className={cn('h-4 w-4', pulse && 'animate-spin')} />
            </button>

            <span className="text-xs text-gray-400">
              Última: {lastRefresh.toLocaleTimeString('es-DO')}
            </span>
          </div>
        </div>

        {/* Live KPI Cards */}
        <div
          className={cn(
            'grid grid-cols-2 gap-4 transition-all duration-500 md:grid-cols-4',
            pulse && 'rounded-xl ring-2 ring-green-300'
          )}
        >
          <LiveKPICard
            icon={<Eye className="h-5 w-5" />}
            label="Apariciones hoy"
            value={data.summary.todayImpressions.toLocaleString('es-DO')}
            color="blue"
            trend={12.5}
          />
          <LiveKPICard
            icon={<MousePointerClick className="h-5 w-5" />}
            label="Toques hoy"
            value={data.summary.todayClicks.toLocaleString('es-DO')}
            color="green"
            trend={8.3}
          />
          <LiveKPICard
            icon={<DollarSign className="h-5 w-5" />}
            label="Gasto Hoy"
            value={`RD$${data.summary.todaySpend.toLocaleString('es-DO')}`}
            color="amber"
            subtext={`Efectividad: ${data.summary.todayCTR.toFixed(1)}%`}
          />
          <LiveKPICard
            icon={<Users className="h-5 w-5" />}
            label="Interesados hoy"
            value={data.summary.todayLeads.toString()}
            color="purple"
            subtext={`Costo/interesado: RD$${Math.round(data.summary.todayCostPerLead).toLocaleString('es-DO')}`}
          />
        </div>

        {/* Budget Overview */}
        <div className="rounded-xl border bg-white p-5 shadow-sm">
          <div className="mb-3 flex items-center justify-between">
            <h3 className="flex items-center gap-2 font-semibold text-gray-900">
              <Target className="h-5 w-5 text-blue-600" />
              Resumen de Inversión
            </h3>
            <span className="text-sm text-gray-500">
              {data.summary.activeCampaigns} campañas activas
            </span>
          </div>
          <div className="flex items-center gap-4">
            <div className="flex-1">
              <div className="mb-1 flex items-center justify-between text-sm">
                <span className="text-gray-600">Presupuesto total</span>
                <span className="font-semibold">
                  RD${data.summary.totalSpent.toLocaleString('es-DO')} / RD$
                  {data.summary.totalBudget.toLocaleString('es-DO')}
                </span>
              </div>
              <div className="h-3 w-full rounded-full bg-gray-200">
                <div
                  className="h-3 rounded-full bg-gradient-to-r from-blue-500 to-blue-600 transition-all duration-700"
                  style={{
                    width: `${Math.min(100, (data.summary.totalSpent / data.summary.totalBudget) * 100)}%`,
                  }}
                />
              </div>
            </div>
            <div className="text-right">
              <div className="text-lg font-bold text-blue-600">
                {((data.summary.totalSpent / data.summary.totalBudget) * 100).toFixed(1)}%
              </div>
              <div className="text-xs text-gray-500">utilizado</div>
            </div>
          </div>
        </div>

        {/* Two-column layout */}
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          {/* Hourly Chart (2/3 width) */}
          <div className="rounded-xl border bg-white p-5 shadow-sm lg:col-span-2">
            <h3 className="mb-4 flex items-center gap-2 font-semibold text-gray-900">
              <BarChart3 className="h-5 w-5 text-blue-600" />
              Rendimiento por Hora — Hoy
            </h3>
            <HourlyChart data={data.hourlyData} />
          </div>

          {/* Activity Feed (1/3 width) */}
          <div className="rounded-xl border bg-white p-5 shadow-sm">
            <h3 className="mb-4 flex items-center gap-2 font-semibold text-gray-900">
              <Zap className="h-5 w-5 text-amber-500" />
              Actividad Reciente
            </h3>
            <div className="max-h-[320px] space-y-3 overflow-y-auto">
              {data.recentActivity.map((item, idx) => (
                <ActivityFeedItem key={idx} item={item} />
              ))}
            </div>
          </div>
        </div>

        {/* Campaign Cards */}
        <div>
          <h3 className="mb-4 flex items-center gap-2 font-semibold text-gray-900">
            <Trophy className="h-5 w-5 text-amber-500" />
            Campañas Activas — Métricas en Vivo
          </h3>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
            {data.campaigns.map(campaign => (
              <CampaignLiveCard key={campaign.id} campaign={campaign} />
            ))}
          </div>
        </div>

        {/* Motivational CTA */}
        <div className="rounded-xl bg-gradient-to-r from-blue-600 to-purple-600 p-6 text-white">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="flex items-center gap-2 text-lg font-bold">
                <TrendingUp className="h-6 w-6" />
                ¡Tu publicidad está generando resultados! 🚀
              </h3>
              <p className="mt-1 text-blue-100">
                Hoy <strong>{data.summary.todayLeads} personas se interesaron</strong> en tus
                vehículos, y la efectividad de tus anuncios es de{' '}
                <strong>{data.summary.todayCTR.toFixed(1)}%</strong>.
                {data.summary.todayCTR > 2
                  ? ' Tu publicidad está funcionando mejor que el promedio. ¡Sigue así!'
                  : ' Considera aumentar tu presupuesto para llegar a más personas.'}
              </p>
            </div>
            <a
              href="/dealer/publicidad/nueva"
              className="flex-shrink-0 rounded-lg bg-white px-5 py-2.5 font-semibold text-blue-600 transition-colors hover:bg-blue-50"
            >
              Crear Nueva Campaña
            </a>
          </div>
        </div>

        {/* Quality Score Tips */}
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-5">
          <h3 className="mb-2 flex items-center gap-2 font-semibold text-amber-900">
            <Trophy className="h-5 w-5 text-amber-600" />
            Mejora la calidad de tus anuncios para pagar menos
          </h3>
          <div className="mt-3 grid grid-cols-1 gap-3 md:grid-cols-3">
            <div className="rounded-lg border border-amber-100 bg-white p-3">
              <div className="text-sm font-medium text-amber-900">📸 Más fotos</div>
              <p className="mt-1 text-xs text-amber-700">
                Publicaciones con 10+ fotos reciben un 20% más de toques
              </p>
            </div>
            <div className="rounded-lg border border-amber-100 bg-white p-3">
              <div className="text-sm font-medium text-amber-900">💰 Precio competitivo</div>
              <p className="mt-1 text-xs text-amber-700">
                Precios dentro del rango de mercado mejoran relevancia
              </p>
            </div>
            <div className="rounded-lg border border-amber-100 bg-white p-3">
              <div className="text-sm font-medium text-amber-900">📝 Descripción completa</div>
              <p className="mt-1 text-xs text-amber-700">
                Descripciones detalladas mejoran la calidad del anuncio
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// COMPONENTS
// =============================================================================

function LiveKPICard({
  icon,
  label,
  value,
  color,
  trend,
  subtext,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
  color: 'blue' | 'green' | 'amber' | 'purple';
  trend?: number;
  subtext?: string;
}) {
  const colorMap = {
    blue: 'bg-blue-50 text-blue-600 border-blue-100',
    green: 'bg-green-50 text-green-600 border-green-100',
    amber: 'bg-amber-50 text-amber-600 border-amber-100',
    purple: 'bg-purple-50 text-purple-600 border-purple-100',
  };

  return (
    <div className={cn('rounded-xl border p-4', colorMap[color])}>
      <div className="flex items-center justify-between">
        {icon}
        {trend !== undefined && (
          <span
            className={cn(
              'flex items-center text-xs font-medium',
              trend > 0 ? 'text-green-600' : 'text-red-500'
            )}
          >
            {trend > 0 ? (
              <ArrowUpRight className="h-3 w-3" />
            ) : (
              <ArrowDownRight className="h-3 w-3" />
            )}
            {Math.abs(trend).toFixed(1)}%
          </span>
        )}
      </div>
      <div className="mt-2">
        <div className="text-2xl font-bold">{value}</div>
        <div className="text-xs opacity-70">{label}</div>
        {subtext && <div className="mt-1 text-xs font-medium opacity-80">{subtext}</div>}
      </div>
    </div>
  );
}

function HourlyChart({ data }: { data: HourlyDataPoint[] }) {
  const maxImpressions = Math.max(...data.map(d => d.impressions), 1);

  return (
    <div className="flex h-[200px] items-end gap-1">
      {data.map(point => {
        const barHeight = (point.impressions / maxImpressions) * 100;
        const clickBarHeight = (point.clicks / maxImpressions) * 100;
        return (
          <div key={point.hour} className="group relative flex flex-1 flex-col items-center">
            {/* Tooltip */}
            <div className="absolute bottom-full z-10 mb-2 hidden rounded-lg bg-gray-900 p-2 text-xs whitespace-nowrap text-white group-hover:block">
              <div className="font-semibold">{point.label}</div>
              <div>👁️ {point.impressions} apariciones</div>
              <div>👆 {point.clicks} toques</div>
              <div>📊 {point.ctr.toFixed(1)}% efectividad</div>
              <div>💰 RD${point.spend.toLocaleString('es-DO')}</div>
            </div>
            {/* Bars */}
            <div
              className="flex w-full flex-col items-center gap-0.5"
              style={{ height: '180px', justifyContent: 'flex-end' }}
            >
              <div
                className="w-full rounded-t bg-blue-200 transition-all duration-700 group-hover:bg-blue-400"
                style={{ height: `${barHeight}%`, minHeight: point.impressions > 0 ? '2px' : '0' }}
              />
              <div
                className="w-full rounded-t bg-green-400 transition-all duration-700 group-hover:bg-green-500"
                style={{ height: `${clickBarHeight}%`, minHeight: point.clicks > 0 ? '2px' : '0' }}
              />
            </div>
            {/* Label */}
            {point.hour % 3 === 0 && (
              <span className="mt-1 text-[10px] text-gray-400">{point.label}</span>
            )}
          </div>
        );
      })}
    </div>
  );
}

function ActivityFeedItem({ item }: { item: ActivityItem }) {
  const iconMap = {
    click: <MousePointerClick className="h-3.5 w-3.5 text-blue-500" />,
    impression: <Eye className="h-3.5 w-3.5 text-gray-400" />,
    lead: <Phone className="h-3.5 w-3.5 text-green-500" />,
    milestone: <Trophy className="h-3.5 w-3.5 text-amber-500" />,
  };

  const bgMap = {
    click: 'bg-blue-50',
    impression: 'bg-gray-50',
    lead: 'bg-green-50',
    milestone: 'bg-amber-50',
  };

  return (
    <div className={cn('flex items-start gap-2 rounded-lg p-2', bgMap[item.type])}>
      <div className="mt-0.5">{iconMap[item.type]}</div>
      <div className="min-w-0 flex-1">
        <p className="text-xs leading-relaxed text-gray-700">{item.message}</p>
        <div className="mt-1 flex items-center gap-1">
          <Clock className="h-3 w-3 text-gray-400" />
          <span className="text-[10px] text-gray-400">hace {item.time}</span>
        </div>
      </div>
    </div>
  );
}

function CampaignLiveCard({ campaign }: { campaign: CampaignLive }) {
  const budgetPercent = (campaign.budget.spent / campaign.budget.total) * 100;
  const dailyPercent = (campaign.budget.spentToday / campaign.budget.daily) * 100;

  return (
    <div className="rounded-xl border bg-white p-4 shadow-sm transition-shadow hover:shadow-md">
      <div className="mb-3 flex items-start justify-between">
        <div className="min-w-0 flex-1">
          <h4 className="truncate text-sm font-semibold text-gray-900">{campaign.name}</h4>
          <span className="text-xs text-gray-500">{campaign.placement}</span>
        </div>
        <div className="flex items-center gap-1 rounded-full bg-green-50 px-2 py-0.5 text-xs font-medium text-green-700">
          <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-green-500" />
          Activa
        </div>
      </div>

      {/* Today's metrics */}
      <div className="mb-3 grid grid-cols-4 gap-2">
        <div className="text-center">
          <div className="text-sm font-bold text-blue-600">
            {campaign.metricsToday.impressions.toLocaleString('es-DO')}
          </div>
          <div className="text-[10px] text-gray-500">Apariciones</div>
        </div>
        <div className="text-center">
          <div className="text-sm font-bold text-green-600">{campaign.metricsToday.clicks}</div>
          <div className="text-[10px] text-gray-500">Toques</div>
        </div>
        <div className="text-center">
          <div className="text-sm font-bold text-amber-600">
            {campaign.metricsToday.ctr.toFixed(1)}%
          </div>
          <div className="text-[10px] text-gray-500">Efectividad</div>
        </div>
        <div className="text-center">
          <div className="text-sm font-bold text-purple-600">{campaign.metricsToday.leads}</div>
          <div className="text-[10px] text-gray-500">Interesados</div>
        </div>
      </div>

      {/* Budget bars */}
      <div className="space-y-2">
        <div>
          <div className="mb-0.5 flex justify-between text-[10px] text-gray-500">
            <span>Presupuesto total</span>
            <span>{budgetPercent.toFixed(0)}%</span>
          </div>
          <div className="h-1.5 w-full rounded-full bg-gray-100">
            <div
              className={cn(
                'h-1.5 rounded-full transition-all',
                budgetPercent > 80
                  ? 'bg-red-500'
                  : budgetPercent > 50
                    ? 'bg-amber-500'
                    : 'bg-blue-500'
              )}
              style={{ width: `${Math.min(100, budgetPercent)}%` }}
            />
          </div>
        </div>
        <div>
          <div className="mb-0.5 flex justify-between text-[10px] text-gray-500">
            <span>Presupuesto diario</span>
            <span>{dailyPercent.toFixed(0)}%</span>
          </div>
          <div className="h-1.5 w-full rounded-full bg-gray-100">
            <div
              className={cn(
                'h-1.5 rounded-full transition-all',
                dailyPercent > 90
                  ? 'bg-red-500'
                  : dailyPercent > 60
                    ? 'bg-amber-500'
                    : 'bg-green-500'
              )}
              style={{ width: `${Math.min(100, dailyPercent)}%` }}
            />
          </div>
        </div>
      </div>

      {/* Quality Score + Last Activity */}
      <div className="mt-3 flex items-center justify-between border-t border-gray-100 pt-3">
        <div className="flex items-center gap-1">
          <Trophy className="h-3.5 w-3.5 text-amber-500" />
          <span className="text-xs text-gray-600">
            Calidad: <strong>{campaign.qualityScore.toFixed(1)}/10</strong>
          </span>
        </div>
        <div className="flex items-center gap-1">
          <MessageSquare className="h-3.5 w-3.5 text-gray-400" />
          <span className="text-[10px] text-gray-400">
            Último interesado: {formatTimeAgo(campaign.lastLeadAt)}
          </span>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// HELPERS
// =============================================================================

function formatTimeAgo(isoDate: string): string {
  const diff = Date.now() - new Date(isoDate).getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'Justo ahora';
  if (minutes < 60) return `hace ${minutes}m`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `hace ${hours}h`;
  return `hace ${Math.floor(hours / 24)}d`;
}
