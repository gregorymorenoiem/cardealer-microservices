/**
 * Account Dashboard Page
 *
 * Main dashboard showing user stats, recent activity, and quick actions
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import {
  Car,
  Eye,
  MessageSquare,
  Star,
  TrendingUp,
  ArrowRight,
  Plus,
  Clock,
  AlertCircle,
  Loader2,
  Shield,
  CheckCircle,
} from 'lucide-react';
import { useCanSell } from '@/hooks/use-kyc';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { userService, type UserVehicleDto } from '@/services/users';

export default function AccountDashboardPage() {
  const [stats, setStats] = React.useState<any>(null);
  const [recentVehicles, setRecentVehicles] = React.useState<UserVehicleDto[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    async function fetchData() {
      try {
        setLoading(true);
        setError(null);

        // Fetch user stats
        const statsData = await userService.getUserStats().catch(() => null);
        setStats(
          statsData ?? {
            vehiclesPublished: 0,
            totalViews: 0,
            totalInquiries: 0,
            averageRating: 0,
            reviewCount: 0,
          }
        );

        // Fetch recent vehicles
        const vehiclesResult = await userService
          .getUserVehicles({ limit: 3, status: 'all' })
          .catch(() => ({ vehicles: [], total: 0 }));
        setRecentVehicles(vehiclesResult.vehicles);
      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        setError('No se pudieron cargar los datos. Por favor, intenta de nuevo.');
      } finally {
        setLoading(false);
      }
    }

    fetchData();
  }, []);

  if (loading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="text-center">
          <Loader2 className="mx-auto h-12 w-12 animate-spin text-blue-600" />
          <p className="text-muted-foreground mt-4">Cargando tu dashboard...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="text-center">
          <AlertCircle className="mx-auto h-12 w-12 text-red-600" />
          <p className="text-muted-foreground mt-4">{error}</p>
          <Button onClick={() => window.location.reload()} className="mt-4">
            Reintentar
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-foreground text-2xl font-bold">Dashboard</h1>
          <p className="text-muted-foreground">Bienvenido a tu panel de control</p>
        </div>
        <Link href="/vender">
          <Button className="gap-2">
            <Plus className="h-4 w-4" />
            Publicar Vehículo
          </Button>
        </Link>
      </div>

      {/* Verification Banner */}
      <VerificationBanner />

      {/* Stats Grid */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatsCard
          title="Vehículos Activos"
          value={stats?.vehiclesPublished ?? 0}
          icon={Car}
          color="blue"
        />
        <StatsCard title="Vistas Totales" value={stats?.totalViews ?? 0} icon={Eye} color="green" />
        <StatsCard
          title="Consultas"
          value={stats?.totalInquiries ?? 0}
          icon={MessageSquare}
          color="purple"
        />
        <StatsCard
          title="Calificación"
          value={stats?.averageRating ? `${stats.averageRating.toFixed(1)}` : '-'}
          icon={Star}
          color="yellow"
          suffix={stats?.averageRating ? `(${stats.reviewCount})` : ''}
        />
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Acciones Rápidas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <QuickAction href="/vender" icon={Plus} label="Publicar Vehículo" color="green" />
            <QuickAction
              href="/cuenta/mis-vehiculos"
              icon={Car}
              label="Ver Mis Vehículos"
              color="blue"
            />
            <QuickAction
              href="/cuenta/mensajes"
              icon={MessageSquare}
              label="Ver Mensajes"
              color="purple"
            />
            <QuickAction
              href="/cuenta/favoritos"
              icon={Star}
              label="Mis Favoritos"
              color="yellow"
            />
          </div>
        </CardContent>
      </Card>

      {/* Recent Vehicles */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-lg">Mis Vehículos Recientes</CardTitle>
          <Link href="/cuenta/mis-vehiculos">
            <Button variant="ghost" size="sm" className="gap-1">
              Ver todos
              <ArrowRight className="h-4 w-4" />
            </Button>
          </Link>
        </CardHeader>
        <CardContent>
          {recentVehicles.length > 0 ? (
            <div className="space-y-4">
              {recentVehicles.map(vehicle => (
                <VehicleListItem key={vehicle.id} vehicle={vehicle} />
              ))}
            </div>
          ) : (
            <div className="py-8 text-center">
              <Car className="text-muted-foreground mx-auto mb-4 h-12 w-12" />
              <p className="text-muted-foreground mb-4">No tienes vehículos publicados</p>
              <Link href="/vender">
                <Button>Publicar mi primer vehículo</Button>
              </Link>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Performance Tips */}
      <Card className="border-blue-100 bg-blue-50">
        <CardContent className="pt-6">
          <div className="flex gap-4">
            <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-blue-100">
              <TrendingUp className="h-6 w-6 text-blue-600" />
            </div>
            <div>
              <h3 className="text-foreground mb-1 font-semibold">Mejora tus resultados</h3>
              <p className="text-muted-foreground mb-3 text-sm">
                Los vehículos con fotos profesionales y descripciones completas reciben hasta 3x más
                consultas.
              </p>
              <Link href="/ayuda/consejos-vendedor">
                <Button variant="outline" size="sm">
                  Ver consejos
                </Button>
              </Link>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

// Stats Card Component
function StatsCard({
  title,
  value,
  icon: Icon,
  color,
  suffix,
}: {
  title: string;
  value: number | string;
  icon: React.ElementType;
  color: 'blue' | 'green' | 'purple' | 'yellow';
  suffix?: string;
}) {
  const colorClasses = {
    blue: 'bg-blue-100 text-blue-600',
    green: 'bg-green-100 text-green-600',
    purple: 'bg-purple-100 text-purple-600',
    yellow: 'bg-yellow-100 text-yellow-600',
  };

  return (
    <Card>
      <CardContent className="pt-6">
        <div className="flex items-center gap-3">
          <div
            className={`flex h-10 w-10 items-center justify-center rounded-lg ${colorClasses[color]}`}
          >
            <Icon className="h-5 w-5" />
          </div>
          <div>
            <p className="text-foreground text-2xl font-bold">
              {typeof value === 'number' ? value.toLocaleString() : value}
              {suffix && (
                <span className="text-muted-foreground ml-1 text-sm font-normal">{suffix}</span>
              )}
            </p>
            <p className="text-muted-foreground text-sm">{title}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

// Quick Action Component
function QuickAction({
  href,
  icon: Icon,
  label,
  color,
}: {
  href: string;
  icon: React.ElementType;
  label: string;
  color: 'blue' | 'green' | 'purple' | 'yellow';
}) {
  const colorClasses = {
    blue: 'bg-blue-100 text-blue-600 group-hover:bg-blue-200',
    green: 'bg-green-100 text-green-600 group-hover:bg-green-200',
    purple: 'bg-purple-100 text-purple-600 group-hover:bg-purple-200',
    yellow: 'bg-yellow-100 text-yellow-600 group-hover:bg-yellow-200',
  };

  return (
    <Link
      href={href}
      className="group border-border hover:border-border flex flex-col items-center gap-3 rounded-lg border p-4 transition-all hover:shadow-sm"
    >
      <div
        className={`flex h-12 w-12 items-center justify-center rounded-lg transition-colors ${colorClasses[color]}`}
      >
        <Icon className="h-6 w-6" />
      </div>
      <span className="text-foreground text-center text-sm font-medium">{label}</span>
    </Link>
  );
}

// Vehicle List Item Component
function VehicleListItem({ vehicle }: { vehicle: UserVehicleDto }) {
  const statusConfig: Record<string, { label: string; className: string }> = {
    active: { label: 'Activo', className: 'bg-green-100 text-green-700' },
    pending: { label: 'Pendiente', className: 'bg-yellow-100 text-yellow-700' },
    paused: { label: 'Pausado', className: 'bg-muted text-foreground' },
    sold: { label: 'Vendido', className: 'bg-blue-100 text-blue-700' },
    expired: { label: 'Expirado', className: 'bg-red-100 text-red-700' },
    rejected: { label: 'Rechazado', className: 'bg-red-100 text-red-700' },
  };

  const config = statusConfig[vehicle.status] || statusConfig.pending;
  const daysUntilExpiry = Math.ceil(
    (new Date(vehicle.expiresAt).getTime() - Date.now()) / (1000 * 60 * 60 * 24)
  );
  const isExpiringSoon = daysUntilExpiry <= 7 && daysUntilExpiry > 0 && vehicle.status === 'active';

  return (
    <div className="border-border hover:border-border flex gap-4 rounded-lg border p-4 transition-colors">
      {/* Image */}
      <div className="bg-muted h-18 w-24 flex-shrink-0 overflow-hidden rounded-lg">
        <img
          src={vehicle.imageUrl || '/images/vehicle-placeholder.jpg'}
          alt={vehicle.title}
          className="h-full w-full object-cover"
        />
      </div>

      {/* Content */}
      <div className="min-w-0 flex-1">
        <div className="flex items-start justify-between gap-2">
          <div>
            <Link
              href={`/vehiculos/${vehicle.slug}`}
              className="hover:text-primary text-foreground line-clamp-1 font-medium"
            >
              {vehicle.title}
            </Link>
            <p className="text-primary text-lg font-bold">
              {vehicle.currency === 'USD' ? 'US$' : 'RD$'}
              {vehicle.price.toLocaleString()}
            </p>
          </div>
          <Badge className={config.className}>{config.label}</Badge>
        </div>

        <div className="text-muted-foreground mt-2 flex items-center gap-4 text-sm">
          <span className="flex items-center gap-1">
            <Eye className="h-4 w-4" />
            {vehicle.viewCount}
          </span>
          <span className="flex items-center gap-1">
            <MessageSquare className="h-4 w-4" />
            {vehicle.inquiryCount}
          </span>
          {isExpiringSoon && (
            <span className="flex items-center gap-1 text-amber-600">
              <Clock className="h-4 w-4" />
              Expira en {daysUntilExpiry} días
            </span>
          )}
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center">
        <Link href={`/cuenta/mis-vehiculos/${vehicle.id}/editar`}>
          <Button variant="outline" size="sm">
            Editar
          </Button>
        </Link>
      </div>
    </div>
  );
}

// Verification Banner Component
function VerificationBanner() {
  const { canSell, isPending, isRejected, needsVerification, isLoading, rejectionReason } =
    useCanSell();

  // Don't show anything while loading or if verified
  if (isLoading || canSell) return null;

  // Verification pending/under review
  if (isPending) {
    return (
      <Card className="border-purple-200 bg-purple-50">
        <CardContent className="pt-6">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-purple-100">
              <Clock className="h-6 w-6 text-purple-600" />
            </div>
            <div className="flex-1">
              <h3 className="mb-1 font-semibold text-purple-900">Verificación en proceso</h3>
              <p className="text-sm text-purple-700">
                Tu solicitud de verificación está siendo revisada. Te notificaremos cuando esté
                lista.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Verification rejected
  if (isRejected) {
    return (
      <Card className="border-red-200 bg-red-50">
        <CardContent className="pt-6">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-red-100">
              <AlertCircle className="h-6 w-6 text-red-600" />
            </div>
            <div className="flex-1">
              <h3 className="mb-1 font-semibold text-red-900">Verificación rechazada</h3>
              <p className="mb-3 text-sm text-red-700">
                Tu solicitud de verificación fue rechazada. Por favor, inténtalo de nuevo.
              </p>
              {rejectionReason && (
                <p className="mb-3 text-sm text-red-600">
                  <strong>Motivo:</strong> {rejectionReason}
                </p>
              )}
              <Link href="/cuenta/verificacion">
                <Button
                  size="sm"
                  variant="outline"
                  className="border-red-300 text-red-700 hover:bg-red-100"
                >
                  Intentar de nuevo
                </Button>
              </Link>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Needs verification
  if (needsVerification) {
    return (
      <Card className="border-blue-200 bg-blue-50">
        <CardContent className="pt-6">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-blue-100">
              <Shield className="h-6 w-6 text-blue-600" />
            </div>
            <div className="flex-1">
              <h3 className="mb-1 font-semibold text-blue-900">
                Verifica tu identidad para vender
              </h3>
              <p className="mb-3 text-sm text-blue-700">
                Para publicar vehículos necesitas verificar tu identidad con tu cédula. Es rápido y
                seguro.
              </p>
              <Link href="/cuenta/verificacion">
                <Button size="sm" className="gap-2 bg-blue-600 hover:bg-blue-700">
                  <Shield className="h-4 w-4" />
                  Verificar ahora
                </Button>
              </Link>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return null;
}
