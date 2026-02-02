/**
 * Dealer Analytics Page
 *
 * Analytics and reporting dashboard for dealers
 * Connected to real APIs for overview stats - January 2026
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  BarChart3,
  TrendingUp,
  TrendingDown,
  Eye,
  Users,
  Car,
  DollarSign,
  Calendar,
  Download,
  ArrowUpRight,
  ArrowDownRight,
  RefreshCw,
  AlertCircle,
} from 'lucide-react';
import { useCurrentDealer, useDealerStats } from '@/hooks/use-dealers';

// Mock data for charts (these need specific APIs)
const topVehicles = [
  { title: 'Toyota Camry 2023', views: 1234, leads: 24, conversion: 1.9 },
  { title: 'Honda CR-V 2024', views: 987, leads: 18, conversion: 1.8 },
  { title: 'BMW X5 2022', views: 856, leads: 32, conversion: 3.7 },
  { title: 'Mercedes GLC 2023', views: 743, leads: 21, conversion: 2.8 },
  { title: 'Audi Q7 2023', views: 654, leads: 15, conversion: 2.3 },
];

const trafficSources = [
  { source: 'Búsqueda Directa', visits: 12450, percentage: 45 },
  { source: 'Google Orgánico', visits: 8234, percentage: 30 },
  { source: 'Redes Sociales', visits: 4123, percentage: 15 },
  { source: 'Referidos', visits: 2756, percentage: 10 },
];

const weeklyData = [
  { day: 'Lun', views: 1200, leads: 12 },
  { day: 'Mar', views: 1450, leads: 15 },
  { day: 'Mié', views: 1300, leads: 14 },
  { day: 'Jue', views: 1650, leads: 18 },
  { day: 'Vie', views: 1890, leads: 22 },
  { day: 'Sáb', views: 2100, leads: 28 },
  { day: 'Dom', views: 1500, leads: 16 },
];

// Skeleton for stats loading
function StatsSkeleton() {
  return (
    <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
      {[1, 2, 3, 4].map(i => (
        <Card key={i}>
          <CardContent className="p-4">
            <Skeleton className="mb-2 h-5 w-5" />
            <Skeleton className="mb-1 h-8 w-24" />
            <Skeleton className="h-3 w-16" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

export default function AnalyticsPage() {
  // Get current dealer
  const { data: dealer, isLoading: isDealerLoading } = useCurrentDealer();

  // Get dealer stats from API
  const {
    data: stats,
    isLoading: isStatsLoading,
    error: statsError,
    refetch,
  } = useDealerStats(dealer?.id);

  // Format currency
  const formatCurrency = (value: number) => {
    if (value >= 1000000) {
      return `RD$ ${(value / 1000000).toFixed(1)}M`;
    }
    return `RD$ ${value.toLocaleString()}`;
  };

  // Format number with K suffix
  const formatNumber = (value: number) => {
    if (value >= 1000) {
      return `${(value / 1000).toFixed(1)}K`;
    }
    return value.toString();
  };

  // Build overview stats from API data
  const overviewStats = stats
    ? [
        {
          label: 'Vistas Totales',
          value: formatNumber(stats.viewsThisMonth),
          change: stats.viewsChange
            ? `${stats.viewsChange > 0 ? '+' : ''}${stats.viewsChange.toFixed(1)}%`
            : '+0%',
          trend: (stats.viewsChange || 0) >= 0 ? 'up' : 'down',
          icon: Eye,
          period: 'vs mes anterior',
        },
        {
          label: 'Leads Generados',
          value: stats.inquiriesThisMonth.toString(),
          change: stats.inquiriesChange
            ? `${stats.inquiriesChange > 0 ? '+' : ''}${stats.inquiriesChange.toFixed(1)}%`
            : '+0%',
          trend: (stats.inquiriesChange || 0) >= 0 ? 'up' : 'down',
          icon: Users,
          period: 'vs mes anterior',
        },
        {
          label: 'Tasa de Respuesta',
          value: `${stats.responseRate.toFixed(1)}%`,
          change: stats.responseRate >= 80 ? '+Buena' : '-Mejorar',
          trend: stats.responseRate >= 80 ? 'up' : 'down',
          icon: TrendingUp,
          period: 'respuestas a consultas',
        },
        {
          label: 'Ingresos Mes',
          value: formatCurrency(stats.revenueThisMonth),
          change: stats.revenueChange
            ? `${stats.revenueChange > 0 ? '+' : ''}${stats.revenueChange.toFixed(1)}%`
            : '+0%',
          trend: (stats.revenueChange || 0) >= 0 ? 'up' : 'down',
          icon: DollarSign,
          period: 'vs mes anterior',
        },
      ]
    : [];

  const isLoading = isDealerLoading || isStatsLoading;

  // Error state
  if (statsError) {
    return (
      <div className="space-y-6">
        <div className="flex flex-col justify-between gap-4 sm:flex-row">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
            <p className="text-gray-600">Métricas y rendimiento de tu dealer</p>
          </div>
        </div>
        <Card className="border-red-200 bg-red-50">
          <CardContent className="flex flex-col items-center justify-center gap-4 p-8">
            <AlertCircle className="h-12 w-12 text-red-500" />
            <p className="text-center text-red-700">
              Error al cargar las estadísticas. Por favor intenta de nuevo.
            </p>
            <Button variant="outline" onClick={() => refetch()}>
              <RefreshCw className="mr-2 h-4 w-4" />
              Reintentar
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
          <p className="text-gray-600">Métricas y rendimiento de tu dealer</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Actualizar
          </Button>
          <Button variant="outline">
            <Calendar className="mr-2 h-4 w-4" />
            Últimos 30 días
          </Button>
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Exportar
          </Button>
        </div>
      </div>

      {/* Overview Stats */}
      {isLoading ? (
        <StatsSkeleton />
      ) : (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {overviewStats.map(stat => {
            const Icon = stat.icon;
            const TrendIcon = stat.trend === 'up' ? ArrowUpRight : ArrowDownRight;
            return (
              <Card key={stat.label}>
                <CardContent className="p-4">
                  <div className="mb-2 flex items-center justify-between">
                    <Icon className="h-5 w-5 text-gray-400" />
                    <div
                      className={`flex items-center text-sm ${
                        stat.trend === 'up' ? 'text-emerald-600' : 'text-red-600'
                      }`}
                    >
                      <TrendIcon className="h-4 w-4" />
                      {stat.change}
                    </div>
                  </div>
                  <p className="text-2xl font-bold">{stat.value}</p>
                  <p className="text-xs text-gray-500">{stat.period}</p>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* Tabs */}
      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Resumen</TabsTrigger>
          <TabsTrigger value="vehicles">Vehículos</TabsTrigger>
          <TabsTrigger value="traffic">Tráfico</TabsTrigger>
          <TabsTrigger value="leads">Leads</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <div className="grid gap-6 lg:grid-cols-2">
            {/* Weekly Performance Chart */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Rendimiento Semanal</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {weeklyData.map(day => (
                    <div key={day.day} className="flex items-center gap-4">
                      <span className="w-8 text-sm font-medium text-gray-500">{day.day}</span>
                      <div className="flex-1">
                        <div className="mb-1 flex items-center gap-2">
                          <div
                            className="h-3 rounded-full bg-emerald-500"
                            style={{ width: `${(day.views / 2100) * 100}%` }}
                          />
                          <span className="text-xs text-gray-500">{day.views}</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <div
                            className="h-2 rounded-full bg-blue-400"
                            style={{ width: `${(day.leads / 28) * 100}%` }}
                          />
                          <span className="text-xs text-gray-400">{day.leads}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
                <div className="mt-4 flex items-center justify-center gap-6 border-t pt-4">
                  <div className="flex items-center gap-2">
                    <div className="h-3 w-3 rounded-full bg-emerald-500" />
                    <span className="text-sm text-gray-600">Vistas</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="h-2 w-3 rounded-full bg-blue-400" />
                    <span className="text-sm text-gray-600">Leads</span>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Traffic Sources */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Fuentes de Tráfico</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {trafficSources.map(source => (
                    <div key={source.source}>
                      <div className="mb-1 flex items-center justify-between">
                        <span className="text-sm font-medium">{source.source}</span>
                        <span className="text-sm text-gray-500">
                          {source.visits.toLocaleString()} ({source.percentage}%)
                        </span>
                      </div>
                      <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                        <div
                          className="h-full rounded-full bg-emerald-500"
                          style={{ width: `${source.percentage}%` }}
                        />
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Top Vehicles */}
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-lg">Top Vehículos</CardTitle>
              <Button variant="ghost" size="sm" asChild>
                <Link href="/dealer/analytics/inventario">Ver todos</Link>
              </Button>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b">
                      <th className="py-3 text-left text-sm font-medium text-gray-500">Vehículo</th>
                      <th className="py-3 text-center text-sm font-medium text-gray-500">Vistas</th>
                      <th className="py-3 text-center text-sm font-medium text-gray-500">Leads</th>
                      <th className="py-3 text-center text-sm font-medium text-gray-500">
                        Conversión
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {topVehicles.map((vehicle, index) => (
                      <tr key={index} className="border-b last:border-0">
                        <td className="py-3">
                          <div className="flex items-center gap-2">
                            <span className="w-5 text-xs text-gray-400">{index + 1}</span>
                            <span className="font-medium">{vehicle.title}</span>
                          </div>
                        </td>
                        <td className="py-3 text-center">{vehicle.views.toLocaleString()}</td>
                        <td className="py-3 text-center">{vehicle.leads}</td>
                        <td className="py-3 text-center">
                          <Badge
                            className={
                              vehicle.conversion >= 3
                                ? 'bg-emerald-100 text-emerald-700'
                                : vehicle.conversion >= 2
                                  ? 'bg-amber-100 text-amber-700'
                                  : 'bg-gray-100 text-gray-700'
                            }
                          >
                            {vehicle.conversion}%
                          </Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="vehicles">
          <Card>
            <CardContent className="p-8 text-center">
              <BarChart3 className="mx-auto mb-4 h-12 w-12 text-gray-300" />
              <h3 className="mb-2 font-medium text-gray-900">Analytics de Vehículos</h3>
              <p className="mb-4 text-gray-500">
                Análisis detallado del rendimiento de cada vehículo
              </p>
              <Button variant="outline" asChild>
                <Link href="/dealer/analytics/inventario">Ver Analytics de Inventario</Link>
              </Button>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="traffic">
          <Card>
            <CardContent className="p-8 text-center">
              <Eye className="mx-auto mb-4 h-12 w-12 text-gray-300" />
              <h3 className="mb-2 font-medium text-gray-900">Analytics de Tráfico</h3>
              <p className="text-gray-500">
                Análisis detallado de fuentes de tráfico y comportamiento
              </p>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="leads">
          <Card>
            <CardContent className="p-8 text-center">
              <Users className="mx-auto mb-4 h-12 w-12 text-gray-300" />
              <h3 className="mb-2 font-medium text-gray-900">Analytics de Leads</h3>
              <p className="mb-4 text-gray-500">Análisis de conversión y calidad de leads</p>
              <Button variant="outline" asChild>
                <Link href="/dealer/analytics/ventas">Ver Analytics de Ventas</Link>
              </Button>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
