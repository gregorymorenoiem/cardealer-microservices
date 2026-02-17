/**
 * Dealer Dashboard Page
 *
 * Main dashboard for dealers showing key metrics and actions
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Car,
  Users,
  Eye,
  TrendingUp,
  TrendingDown,
  MessageSquare,
  Calendar,
  Plus,
  ChevronRight,
  Clock,
  DollarSign,
  AlertCircle,
  CheckCircle,
  ArrowUpRight,
  RefreshCw,
} from 'lucide-react';

// Hooks
import { useCurrentDealer, useDealerStats } from '@/hooks/use-dealers';
import { useRecentLeads } from '@/hooks/use-crm';
import { useUpcomingAppointments } from '@/hooks/use-appointments';
import { formatLeadName, type LeadDto } from '@/services/crm';
import { getAppointmentTypeLabel } from '@/services/appointments';

// Types for display
interface StatItem {
  label: string;
  value: string | number;
  max?: number;
  change: string;
  trend: 'up' | 'down' | 'neutral';
  icon: React.ElementType;
  color: string;
  bg: string;
}

// Helper to format relative time
const formatRelativeTime = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 60) return `Hace ${diffMins} min`;
  if (diffHours < 24) return `Hace ${diffHours}h`;
  if (diffDays === 1) return 'Ayer';
  return `Hace ${diffDays} días`;
};

// Format appointment date for display
const formatAppointmentDate = (dateString: string): string => {
  const date = new Date(dateString);
  const today = new Date();
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);

  if (date.toDateString() === today.toDateString()) return 'Hoy';
  if (date.toDateString() === tomorrow.toDateString()) return 'Mañana';

  return date.toLocaleDateString('es-DO', { weekday: 'short', day: 'numeric', month: 'short' });
};

// Format time 24h → 12h
const formatAppointmentTime = (time: string): string => {
  if (!time) return '';
  const [h, m] = time.split(':');
  const hour = parseInt(h, 10);
  const suffix = hour >= 12 ? 'PM' : 'AM';
  const hour12 = hour > 12 ? hour - 12 : hour === 0 ? 12 : hour;
  return `${hour12}:${m} ${suffix}`;
};

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'new':
      return <Badge className="bg-primary/10 text-primary">Nuevo</Badge>;
    case 'contacted':
      return <Badge className="bg-blue-100 text-blue-700">Contactado</Badge>;
    case 'qualified':
      return <Badge className="bg-purple-100 text-purple-700">Calificado</Badge>;
    default:
      return <Badge variant="secondary">{status}</Badge>;
  }
};

// Format currency
function formatCurrencyCompact(value: number): string {
  if (value >= 1_000_000) {
    return `RD$ ${(value / 1_000_000).toFixed(1)}M`;
  }
  if (value >= 1_000) {
    return `RD$ ${(value / 1_000).toFixed(0)}K`;
  }
  return `RD$ ${value}`;
}

// Format number with K/M suffix
function formatNumber(value: number): string {
  if (value >= 1_000_000) {
    return `${(value / 1_000_000).toFixed(1)}M`;
  }
  if (value >= 1_000) {
    return `${(value / 1_000).toFixed(1)}K`;
  }
  return value.toString();
}

// Stats skeleton component
function StatsSkeleton() {
  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {[1, 2, 3, 4].map(i => (
        <Card key={i}>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div className="space-y-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-8 w-16" />
                <Skeleton className="h-3 w-12" />
              </div>
              <Skeleton className="h-12 w-12 rounded-full" />
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

export default function DealerDashboardPage() {
  // Get current dealer
  const { data: dealer, isLoading: dealerLoading } = useCurrentDealer();

  // Get dealer stats
  const {
    data: statsData,
    isLoading: statsLoading,
    error: statsError,
    refetch: refetchStats,
  } = useDealerStats(dealer?.id);

  // Get recent leads from CRM
  const { data: recentLeads, isLoading: leadsLoading } = useRecentLeads(5);

  // Get upcoming appointments from AppointmentService
  const { data: upcomingAppointments, isLoading: appointmentsLoading } = useUpcomingAppointments(
    dealer?.id || '',
    3
  );

  // Build stats items from API data
  const stats: StatItem[] = React.useMemo(() => {
    if (!statsData) return [];

    return [
      {
        label: 'Vehículos Activos',
        value: statsData.activeListings || 0,
        max: statsData.totalListings || 50,
        change: `${statsData.activeListings || 0}`,
        trend: 'up' as const,
        icon: Car,
        color: 'text-blue-600',
        bg: 'bg-blue-100',
      },
      {
        label: 'Vistas Este Mes',
        value: formatNumber(statsData.viewsThisMonth || 0),
        change: statsData.viewsChange
          ? `${statsData.viewsChange > 0 ? '+' : ''}${statsData.viewsChange}%`
          : '0%',
        trend: (statsData.viewsChange || 0) >= 0 ? ('up' as const) : ('down' as const),
        icon: Eye,
        color: 'text-primary',
        bg: 'bg-primary/10',
      },
      {
        label: 'Consultas Nuevas',
        value: statsData.inquiriesThisMonth || 0,
        change: statsData.inquiriesChange
          ? `${statsData.inquiriesChange > 0 ? '+' : ''}${statsData.inquiriesChange}%`
          : '0%',
        trend: (statsData.inquiriesChange || 0) >= 0 ? ('up' as const) : ('down' as const),
        icon: Users,
        color: 'text-purple-600',
        bg: 'bg-purple-100',
      },
      {
        label: 'Ingresos Este Mes',
        value: formatCurrencyCompact(statsData.revenueThisMonth || 0),
        change: statsData.revenueChange
          ? `${statsData.revenueChange > 0 ? '+' : ''}${statsData.revenueChange}%`
          : '0%',
        trend: (statsData.revenueChange || 0) >= 0 ? ('up' as const) : ('down' as const),
        icon: DollarSign,
        color: 'text-amber-600',
        bg: 'bg-amber-100',
      },
    ];
  }, [statsData]);

  // Alerts based on data
  const alerts = React.useMemo(() => {
    const items: { type: 'warning' | 'info'; message: string; action: string; href: string }[] = [];

    if (statsData?.pendingInquiries && statsData.pendingInquiries > 0) {
      items.push({
        type: 'warning',
        message: `${statsData.pendingInquiries} consultas pendientes de responder`,
        action: 'Revisar',
        href: '/dealer/mensajes?filter=pending',
      });
    }

    if (statsData?.responseRate && statsData.responseRate < 80) {
      items.push({
        type: 'info',
        message: `Tu tasa de respuesta es ${statsData.responseRate}%. Mejórala respondiendo más rápido.`,
        action: 'Ver consejos',
        href: '/ayuda/articulo/mejorar-respuestas',
      });
    }

    return items;
  }, [statsData]);

  const isLoading = dealerLoading || statsLoading;
  const dealerName = dealer?.businessName || 'Tu Dealer';

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-foreground text-2xl font-bold">Dashboard</h1>
          <p className="text-muted-foreground">Bienvenido de nuevo, {dealerName}</p>
        </div>
        <div className="flex gap-3">
          <Button
            variant="outline"
            size="icon"
            onClick={() => refetchStats()}
            disabled={statsLoading}
          >
            <RefreshCw className={`h-4 w-4 ${statsLoading ? 'animate-spin' : ''}`} />
          </Button>
          <Button variant="outline" asChild>
            <Link href="/dealer/reportes">Ver Reportes</Link>
          </Button>
          <Button className="bg-primary hover:bg-primary/90" asChild>
            <Link href="/dealer/inventario/nuevo">
              <Plus className="mr-2 h-4 w-4" />
              Agregar Vehículo
            </Link>
          </Button>
        </div>
      </div>

      {/* Alerts */}
      {alerts.length > 0 && (
        <div className="space-y-2">
          {alerts.map((alert, index) => (
            <div
              key={index}
              className={`flex items-center justify-between rounded-lg p-3 ${
                alert.type === 'warning'
                  ? 'border border-amber-200 bg-amber-50'
                  : 'border border-blue-200 bg-blue-50'
              }`}
            >
              <div className="flex items-center gap-3">
                <AlertCircle
                  className={`h-5 w-5 ${
                    alert.type === 'warning' ? 'text-amber-600' : 'text-blue-600'
                  }`}
                />
                <span
                  className={`text-sm ${
                    alert.type === 'warning' ? 'text-amber-800' : 'text-blue-800'
                  }`}
                >
                  {alert.message}
                </span>
              </div>
              <Link
                href={alert.href}
                className={`text-sm font-medium ${
                  alert.type === 'warning'
                    ? 'text-amber-700 hover:text-amber-800'
                    : 'text-blue-700 hover:text-blue-800'
                }`}
              >
                {alert.action} →
              </Link>
            </div>
          ))}
        </div>
      )}

      {/* Stats Grid */}
      {statsLoading ? (
        <StatsSkeleton />
      ) : statsError ? (
        <Card className="border-red-200 bg-red-50">
          <CardContent className="flex items-center justify-between p-4">
            <div className="flex items-center gap-2">
              <AlertCircle className="h-5 w-5 text-red-600" />
              <span className="text-red-800">Error al cargar estadísticas</span>
            </div>
            <Button variant="outline" size="sm" onClick={() => refetchStats()}>
              Reintentar
            </Button>
          </CardContent>
        </Card>
      ) : stats.length > 0 ? (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {stats.map(stat => {
            const Icon = stat.icon;
            const TrendIcon = stat.trend === 'up' ? TrendingUp : TrendingDown;
            return (
              <Card key={stat.label}>
                <CardContent className="p-4">
                  <div className="mb-3 flex items-center justify-between">
                    <div className={`rounded-lg p-2 ${stat.bg}`}>
                      <Icon className={`h-5 w-5 ${stat.color}`} />
                    </div>
                    <div
                      className={`flex items-center text-xs ${
                        stat.trend === 'up' ? 'text-primary' : 'text-red-600'
                      }`}
                    >
                      <TrendIcon className="mr-1 h-3 w-3" />
                      {stat.change}
                    </div>
                  </div>
                  <p className="text-2xl font-bold">{stat.value}</p>
                  <p className="text-muted-foreground text-xs">
                    {stat.label}
                    {stat.max && ` (${stat.max} máx)`}
                  </p>
                </CardContent>
              </Card>
            );
          })}
        </div>
      ) : null}

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Recent Leads */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2 text-lg">
              <Users className="h-5 w-5" />
              Leads Recientes
            </CardTitle>
            <Button variant="ghost" size="sm" asChild>
              <Link href="/dealer/leads">
                Ver todos <ChevronRight className="ml-1 h-4 w-4" />
              </Link>
            </Button>
          </CardHeader>
          <CardContent>
            {leadsLoading ? (
              <div className="space-y-3">
                {[1, 2, 3, 4].map(i => (
                  <div key={i} className="flex items-center justify-between p-3">
                    <div className="flex items-center gap-3">
                      <Skeleton className="h-10 w-10 rounded-full" />
                      <div className="space-y-1">
                        <Skeleton className="h-4 w-24" />
                        <Skeleton className="h-3 w-32" />
                      </div>
                    </div>
                    <div className="space-y-1 text-right">
                      <Skeleton className="h-5 w-16" />
                      <Skeleton className="h-3 w-12" />
                    </div>
                  </div>
                ))}
              </div>
            ) : recentLeads && recentLeads.length > 0 ? (
              <div className="space-y-3">
                {recentLeads.map(lead => (
                  <Link
                    key={lead.id}
                    href={`/dealer/leads/${lead.id}`}
                    className="hover:bg-muted/50 flex items-center justify-between rounded-lg p-3 transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      <div className="bg-muted flex h-10 w-10 items-center justify-center rounded-full">
                        <span className="text-muted-foreground text-sm font-medium">
                          {lead.firstName.charAt(0)}
                        </span>
                      </div>
                      <div>
                        <p className="text-sm font-medium">{formatLeadName(lead)}</p>
                        <p className="text-muted-foreground text-xs">{lead.email}</p>
                      </div>
                    </div>
                    <div className="text-right">
                      {getStatusBadge(lead.status)}
                      <p className="text-muted-foreground mt-1 text-xs">
                        {formatRelativeTime(lead.createdAt)}
                      </p>
                    </div>
                  </Link>
                ))}
              </div>
            ) : (
              <div className="text-muted-foreground py-8 text-center">
                <Users className="mx-auto mb-2 h-8 w-8 text-gray-300" />
                <p className="text-sm">No hay leads recientes</p>
                <p className="text-xs">Los leads aparecerán aquí cuando lleguen</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Upcoming Appointments */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2 text-lg">
              <Calendar className="h-5 w-5" />
              Próximas Citas
            </CardTitle>
            <Button variant="ghost" size="sm" asChild>
              <Link href="/dealer/citas">
                Ver todas <ChevronRight className="ml-1 h-4 w-4" />
              </Link>
            </Button>
          </CardHeader>
          <CardContent>
            {appointmentsLoading ? (
              <div className="space-y-3">
                {[1, 2, 3].map(i => (
                  <div key={i} className="flex items-center justify-between p-3">
                    <div className="flex items-center gap-3">
                      <Skeleton className="h-10 w-10 rounded-lg" />
                      <div className="space-y-1">
                        <Skeleton className="h-4 w-24" />
                        <Skeleton className="h-3 w-32" />
                      </div>
                    </div>
                    <div className="space-y-1 text-right">
                      <Skeleton className="h-4 w-12" />
                      <Skeleton className="h-3 w-16" />
                    </div>
                  </div>
                ))}
              </div>
            ) : upcomingAppointments && upcomingAppointments.length > 0 ? (
              <div className="space-y-3">
                {upcomingAppointments.map(apt => (
                  <Link
                    key={apt.id}
                    href="/dealer/citas/calendario"
                    className="flex items-center justify-between rounded-lg border p-3 transition-colors hover:border-primary"
                  >
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                        <Calendar className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <p className="text-sm font-medium">{apt.clientName}</p>
                        <p className="text-muted-foreground text-xs">
                          {apt.relatedEntityDescription || 'Cita programada'}
                        </p>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">
                        {formatAppointmentDate(apt.scheduledDate)}
                      </p>
                      <p className="text-muted-foreground text-xs">
                        {formatAppointmentTime(apt.scheduledTime)}
                      </p>
                      <Badge variant="outline" className="mt-1 text-xs">
                        {getAppointmentTypeLabel(apt.type)}
                      </Badge>
                    </div>
                  </Link>
                ))}
              </div>
            ) : (
              <div className="text-muted-foreground py-8 text-center">
                <Calendar className="mx-auto mb-2 h-8 w-8 text-gray-300" />
                <p className="text-sm">No hay citas próximas</p>
                <p className="text-xs">Las citas aparecerán aquí cuando se programen</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Acciones Rápidas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <Link
              href="/dealer/inventario/nuevo"
              className="flex flex-col items-center gap-2 rounded-lg border p-4 transition-colors hover:border-primary hover:bg-primary/10"
            >
              <Plus className="h-6 w-6 text-primary" />
              <span className="text-sm font-medium">Nuevo Vehículo</span>
            </Link>
            <Link
              href="/dealer/inventario/importar"
              className="flex flex-col items-center gap-2 rounded-lg border p-4 transition-colors hover:border-primary hover:bg-primary/10"
            >
              <ArrowUpRight className="h-6 w-6 text-blue-600" />
              <span className="text-sm font-medium">Importar CSV</span>
            </Link>
            <Link
              href="/dealer/mensajes"
              className="flex flex-col items-center gap-2 rounded-lg border p-4 transition-colors hover:border-primary hover:bg-primary/10"
            >
              <MessageSquare className="h-6 w-6 text-purple-600" />
              <span className="text-sm font-medium">Ver Mensajes</span>
            </Link>
            <Link
              href="/dealer/analytics"
              className="flex flex-col items-center gap-2 rounded-lg border p-4 transition-colors hover:border-primary hover:bg-primary/10"
            >
              <TrendingUp className="h-6 w-6 text-amber-600" />
              <span className="text-sm font-medium">Ver Analytics</span>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
