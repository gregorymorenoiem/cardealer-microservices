/**
 * Seller Statistics Page
 *
 * Shows vehicle listing statistics for individual sellers
 */

'use client';

import * as React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { BarChart3, Eye, Heart, Car, TrendingUp, AlertCircle, MessageSquare } from 'lucide-react';
import { userService } from '@/services/users';

interface SellerStats {
  vehiclesPublished: number;
  vehiclesSold: number;
  totalViews: number;
  totalInquiries: number;
  responseRate: number;
  averageResponseTime: string;
  reviewCount: number;
  averageRating: number;
}

export default function EstadisticasPage() {
  const [stats, setStats] = React.useState<SellerStats | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    async function fetchStats() {
      try {
        setLoading(true);
        const data = await userService.getUserStats();
        setStats(data);
      } catch (err) {
        console.error('Error fetching stats:', err);
        setError('No se pudieron cargar las estadísticas.');
      } finally {
        setLoading(false);
      }
    }
    fetchStats();
  }, []);

  if (loading) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold">Estadísticas</h1>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i}>
              <CardContent className="p-6">
                <Skeleton className="h-20 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-[300px] items-center justify-center">
        <div className="text-center">
          <AlertCircle className="mx-auto h-12 w-12 text-red-500" />
          <p className="text-muted-foreground mt-4">{error}</p>
        </div>
      </div>
    );
  }

  const statCards = [
    {
      label: 'Vehículos Publicados',
      value: stats?.vehiclesPublished ?? 0,
      icon: Car,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
    },
    {
      label: 'Vistas Totales',
      value: stats?.totalViews ?? 0,
      icon: Eye,
      color: 'text-green-600',
      bgColor: 'bg-green-50',
    },
    {
      label: 'Consultas Recibidas',
      value: stats?.totalInquiries ?? 0,
      icon: MessageSquare,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
    },
    {
      label: 'Vehículos Vendidos',
      value: stats?.vehiclesSold ?? 0,
      icon: TrendingUp,
      color: 'text-amber-600',
      bgColor: 'bg-amber-50',
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-foreground text-2xl font-bold">Estadísticas</h1>
        <p className="text-muted-foreground">Resumen del rendimiento de tus publicaciones</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {statCards.map(stat => (
          <Card key={stat.label}>
            <CardContent className="p-6">
              <div className="flex items-center gap-4">
                <div className={`rounded-lg p-3 ${stat.bgColor}`}>
                  <stat.icon className={`h-6 w-6 ${stat.color}`} />
                </div>
                <div>
                  <p className="text-muted-foreground text-sm">{stat.label}</p>
                  <p className="text-2xl font-bold">{stat.value.toLocaleString()}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Additional Metrics */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <BarChart3 className="h-5 w-5" />
              Tasa de Respuesta
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">
              {stats?.responseRate ? `${stats.responseRate}%` : 'N/A'}
            </p>
            <p className="text-muted-foreground mt-1 text-sm">
              Tiempo promedio: {stats?.averageResponseTime ?? 'N/A'}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Heart className="h-5 w-5" />
              Calificación Promedio
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">
              {stats?.averageRating ? stats.averageRating.toFixed(1) : 'N/A'}
            </p>
            <p className="text-muted-foreground mt-1 text-sm">
              {stats?.reviewCount ?? 0} reseñas recibidas
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <TrendingUp className="h-5 w-5" />
              Conversión
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">
              {stats && stats.totalViews > 0
                ? `${((stats.totalInquiries / stats.totalViews) * 100).toFixed(1)}%`
                : 'N/A'}
            </p>
            <p className="text-muted-foreground mt-1 text-sm">Consultas / Vistas</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
