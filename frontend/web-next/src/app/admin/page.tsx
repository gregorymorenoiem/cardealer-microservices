/**
 * Admin Dashboard Page
 *
 * Main dashboard for administrators showing platform overview
 * Connected to real APIs - February 2026
 */

'use client';

import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Users,
  Car,
  Building,
  DollarSign,
  TrendingUp,
  TrendingDown,
  AlertTriangle,
  CheckCircle,
  Clock,
  Flag,
  ChevronRight,
  RefreshCw,
} from 'lucide-react';
import {
  useDashboardStats,
  useRecentActivity,
  usePendingActions,
} from '@/hooks/use-admin';

// =============================================================================
// SKELETON
// =============================================================================

function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-10 w-32" />
      </div>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {[1, 2, 3, 4].map(i => (
          <Skeleton key={i} className="h-32" />
        ))}
      </div>
      <div className="grid gap-6 lg:grid-cols-2">
        <Skeleton className="h-80" />
        <Skeleton className="h-80" />
      </div>
    </div>
  );
}

// =============================================================================
// HELPERS
// =============================================================================

const getActionIcon = (type: string) => {
  switch (type) {
    case 'moderation':
      return Clock;
    case 'verification':
      return Building;
    case 'report':
      return Flag;
    case 'support':
      return AlertTriangle;
    default:
      return Clock;
  }
};

const getPriorityBadge = (priority: string) => {
  switch (priority) {
    case 'high':
      return <Badge className="bg-red-100 text-red-700">Alta</Badge>;
    case 'medium':
      return <Badge className="bg-amber-100 text-amber-700">Media</Badge>;
    default:
      return <Badge variant="secondary">Baja</Badge>;
  }
};

const getActivityIcon = (type: string) => {
  switch (type) {
    case 'dealer':
      return Building;
    case 'vehicle':
      return Car;
    case 'user':
      return Users;
    case 'payment':
      return DollarSign;
    case 'report':
      return Flag;
    default:
      return CheckCircle;
  }
};

const formatCurrency = (value: number) => {
  if (value >= 1000000) {
    return `RD$ ${(value / 1000000).toFixed(1)}M`;
  }
  if (value >= 1000) {
    return `RD$ ${(value / 1000).toFixed(0)}K`;
  }
  return `RD$ ${value.toLocaleString()}`;
};

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function AdminDashboardPage() {
  const { data: stats, isLoading: statsLoading, refetch: refetchStats } = useDashboardStats();
  const { data: activity, isLoading: activityLoading } = useRecentActivity(5);
  const { data: pendingActions, isLoading: pendingLoading } = usePendingActions();

  const isLoading = statsLoading || activityLoading || pendingLoading;

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  const platformStats = [
    {
      label: 'Usuarios Totales',
      value: stats?.totalUsers.toLocaleString() || '0',
      change: stats?.usersChange ? `${stats.usersChange > 0 ? '+' : ''}${stats.usersChange}%` : '0%',
      trend: (stats?.usersChange || 0) >= 0 ? 'up' : 'down',
      icon: Users,
      color: 'text-blue-600',
      bg: 'bg-blue-100',
    },
    {
      label: 'Vehículos Activos',
      value: stats?.activeVehicles.toLocaleString() || '0',
      change: stats?.vehiclesChange
        ? `${stats.vehiclesChange > 0 ? '+' : ''}${stats.vehiclesChange}%`
        : '0%',
      trend: (stats?.vehiclesChange || 0) >= 0 ? 'up' : 'down',
      icon: Car,
      color: 'text-emerald-600',
      bg: 'bg-emerald-100',
    },
    {
      label: 'Dealers Activos',
      value: stats?.activeDealers.toLocaleString() || '0',
      change: stats?.dealersChange
        ? `${stats.dealersChange > 0 ? '+' : ''}${stats.dealersChange}`
        : '0',
      trend: (stats?.dealersChange || 0) >= 0 ? 'up' : 'down',
      icon: Building,
      color: 'text-purple-600',
      bg: 'bg-purple-100',
    },
    {
      label: 'MRR',
      value: formatCurrency(stats?.mrr || 0),
      change: stats?.mrrChange ? `${stats.mrrChange > 0 ? '+' : ''}${stats.mrrChange}%` : '0%',
      trend: (stats?.mrrChange || 0) >= 0 ? 'up' : 'down',
      icon: DollarSign,
      color: 'text-amber-600',
      bg: 'bg-amber-100',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600">Bienvenido al panel de administración de OKLA</p>
        </div>
        <Button variant="outline" onClick={() => refetchStats()}>
          <RefreshCw className="mr-2 h-4 w-4" />
          Actualizar
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {platformStats.map(stat => {
          const Icon = stat.icon;
          const TrendIcon = stat.trend === 'up' ? TrendingUp : TrendingDown;
          return (
            <Card key={stat.label}>
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className={`rounded-lg ${stat.bg} p-3`}>
                    <Icon className={`h-6 w-6 ${stat.color}`} />
                  </div>
                  <div
                    className={`flex items-center gap-1 text-sm ${
                      stat.trend === 'up' ? 'text-emerald-600' : 'text-red-600'
                    }`}
                  >
                    <TrendIcon className="h-4 w-4" />
                    {stat.change}
                  </div>
                </div>
                <div className="mt-4">
                  <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                  <p className="text-sm text-gray-600">{stat.label}</p>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Pending Actions */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-amber-500" />
              Acciones Pendientes
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {pendingActions && pendingActions.length > 0 ? (
                pendingActions.map((action, index) => {
                  const Icon = getActionIcon(action.type);
                  return (
                    <Link
                      key={index}
                      href={action.href}
                      className="flex items-center justify-between rounded-lg border p-4 transition-colors hover:bg-gray-50"
                    >
                      <div className="flex items-center gap-3">
                        <div className="rounded-lg bg-gray-100 p-2">
                          <Icon className="h-5 w-5 text-gray-600" />
                        </div>
                        <div>
                          <p className="font-medium text-gray-900">{action.title}</p>
                          <p className="text-sm text-gray-500">{action.count} pendientes</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        {getPriorityBadge(action.priority)}
                        <ChevronRight className="h-5 w-5 text-gray-400" />
                      </div>
                    </Link>
                  );
                })
              ) : (
                <div className="py-8 text-center text-gray-500">
                  <CheckCircle className="mx-auto mb-2 h-8 w-8 text-emerald-500" />
                  <p>No hay acciones pendientes</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Recent Activity */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5 text-blue-500" />
              Actividad Reciente
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {activity && activity.length > 0 ? (
                activity.map(item => {
                  const Icon = getActivityIcon(item.subjectType);
                  return (
                    <div key={item.id} className="flex items-start gap-3">
                      <div className="rounded-lg bg-gray-100 p-2">
                        <Icon className="h-4 w-4 text-gray-600" />
                      </div>
                      <div className="flex-1">
                        <p className="text-sm font-medium text-gray-900">{item.action}</p>
                        <p className="text-sm text-gray-600">{item.subject}</p>
                        <p className="text-xs text-gray-400">
                          {new Date(item.timestamp).toLocaleString('es-DO', {
                            hour: '2-digit',
                            minute: '2-digit',
                            day: 'numeric',
                            month: 'short',
                          })}
                        </p>
                      </div>
                    </div>
                  );
                })
              ) : (
                <div className="py-8 text-center text-gray-500">
                  <p>No hay actividad reciente</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Quick Links */}
      <Card>
        <CardHeader>
          <CardTitle>Accesos Rápidos</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <Link href="/admin/usuarios">
              <Button variant="outline" className="w-full justify-start">
                <Users className="mr-2 h-4 w-4" />
                Gestionar Usuarios
              </Button>
            </Link>
            <Link href="/admin/vehiculos">
              <Button variant="outline" className="w-full justify-start">
                <Car className="mr-2 h-4 w-4" />
                Moderar Vehículos
              </Button>
            </Link>
            <Link href="/admin/dealers">
              <Button variant="outline" className="w-full justify-start">
                <Building className="mr-2 h-4 w-4" />
                Gestionar Dealers
              </Button>
            </Link>
            <Link href="/admin/reportes">
              <Button variant="outline" className="w-full justify-start">
                <Flag className="mr-2 h-4 w-4" />
                Ver Reportes
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
