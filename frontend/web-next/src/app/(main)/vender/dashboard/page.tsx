'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Car,
  MessageSquare,
  CreditCard,
  TrendingUp,
  Plus,
  Eye,
  Heart,
  Phone,
  Package,
  ArrowUpRight,
  BarChart3,
  Mail,
  Clock,
  CheckCircle,
  XCircle,
  AlertCircle,
} from 'lucide-react';

interface DashboardStats {
  activeVehicles: number;
  totalViews: number;
  totalLeads: number;
  newLeads: number;
  totalFavorites: number;
  publishCredits: number;
}

interface LeadSummary {
  id: string;
  vehicleTitle: string;
  buyerName: string;
  buyerEmail: string;
  message: string;
  status: string;
  createdAt: string;
}

interface VehicleSummary {
  id: string;
  title: string;
  price: number;
  status: string;
  viewCount: number;
  favoriteCount: number;
  inquiryCount: number;
  imageUrl?: string;
  createdAt: string;
}

export default function SellerDashboardPage() {
  const router = useRouter();
  const [stats, setStats] = useState<DashboardStats>({
    activeVehicles: 0,
    totalViews: 0,
    totalLeads: 0,
    newLeads: 0,
    totalFavorites: 0,
    publishCredits: 0,
  });
  const [recentLeads, setRecentLeads] = useState<LeadSummary[]>([]);
  const [myVehicles, setMyVehicles] = useState<VehicleSummary[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  async function loadDashboardData() {
    try {
      setLoading(true);

      // Fetch seller stats, leads, and vehicles in parallel
      const [statsRes, leadsRes, vehiclesRes] = await Promise.allSettled([
        fetch('/api/sellers/me/stats'),
        fetch('/api/leads/seller/me?pageSize=5'),
        fetch('/api/vehicles/seller/me?pageSize=6'),
      ]);

      if (statsRes.status === 'fulfilled' && statsRes.value.ok) {
        const data = await statsRes.value.json();
        setStats(data.data ?? data);
      }

      if (leadsRes.status === 'fulfilled' && leadsRes.value.ok) {
        const data = await leadsRes.value.json();
        setRecentLeads((data.data?.items ?? data.items ?? []).slice(0, 5));
      }

      if (vehiclesRes.status === 'fulfilled' && vehiclesRes.value.ok) {
        const data = await vehiclesRes.value.json();
        setMyVehicles((data.data?.items ?? data.items ?? []).slice(0, 6));
      }
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      setLoading(false);
    }
  }

  const statCards = [
    {
      title: 'Vehículos Activos',
      value: stats.activeVehicles,
      icon: Car,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
      href: '/mis-vehiculos',
    },
    {
      title: 'Vistas Totales',
      value: stats.totalViews,
      icon: Eye,
      color: 'text-green-600',
      bgColor: 'bg-green-50',
    },
    {
      title: 'Leads Nuevos',
      value: stats.newLeads,
      icon: MessageSquare,
      color: 'text-orange-600',
      bgColor: 'bg-orange-50',
      href: '/vender/leads',
      badge:
        stats.newLeads > 0 ? `${stats.newLeads} nuevo${stats.newLeads > 1 ? 's' : ''}` : undefined,
    },
    {
      title: 'Créditos Disponibles',
      value: stats.publishCredits,
      icon: CreditCard,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
      href: '/vender/creditos',
    },
  ];

  const getStatusBadge = (status: string) => {
    const variants: Record<
      string,
      {
        variant: 'default' | 'success' | 'secondary' | 'outline' | 'warning' | 'primary' | 'danger';
        label: string;
      }
    > = {
      New: { variant: 'primary', label: 'Nuevo' },
      Contacted: { variant: 'secondary', label: 'Contactado' },
      Negotiating: { variant: 'outline', label: 'Negociando' },
      Closed: { variant: 'success', label: 'Cerrado' },
      Lost: { variant: 'danger', label: 'Perdido' },
    };
    const config = variants[status] || { variant: 'outline' as const, label: status };
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="animate-pulse space-y-6">
          <div className="h-8 w-64 rounded bg-gray-200" />
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="h-32 rounded-lg bg-gray-200" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto space-y-8 px-4 py-8">
      {/* Header */}
      <div className="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <h1 className="text-3xl font-bold">Dashboard de Vendedor</h1>
          <p className="text-muted-foreground mt-1">Gestiona tus publicaciones, leads y pagos</p>
        </div>
        <Button asChild size="lg">
          <Link href="/publicar">
            <Plus className="mr-2 h-5 w-5" />
            Publicar Vehículo
          </Link>
        </Button>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
        {statCards.map(stat => (
          <Card key={stat.title} className="relative overflow-hidden">
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-muted-foreground text-sm">{stat.title}</p>
                  <p className="mt-1 text-3xl font-bold">{stat.value.toLocaleString('es-DO')}</p>
                  {stat.badge && (
                    <Badge variant="danger" className="mt-2 text-xs">
                      {stat.badge}
                    </Badge>
                  )}
                </div>
                <div className={`${stat.bgColor} ${stat.color} rounded-full p-3`}>
                  <stat.icon className="h-6 w-6" />
                </div>
              </div>
              {stat.href && (
                <Link
                  href={stat.href}
                  className="absolute inset-0"
                  aria-label={`Ver ${stat.title}`}
                />
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main Content — Tabs */}
      <Tabs defaultValue="leads" className="space-y-4">
        <TabsList>
          <TabsTrigger value="leads" className="gap-2">
            <MessageSquare className="h-4 w-4" />
            Leads Recientes
          </TabsTrigger>
          <TabsTrigger value="vehicles" className="gap-2">
            <Car className="h-4 w-4" />
            Mis Vehículos
          </TabsTrigger>
        </TabsList>

        {/* Recent Leads */}
        <TabsContent value="leads">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle>Leads Recientes</CardTitle>
                <CardDescription>Compradores interesados en tus vehículos</CardDescription>
              </div>
              <Button variant="outline" size="sm" asChild>
                <Link href="/vender/leads">
                  Ver todos
                  <ArrowUpRight className="ml-1 h-4 w-4" />
                </Link>
              </Button>
            </CardHeader>
            <CardContent>
              {recentLeads.length === 0 ? (
                <div className="py-12 text-center">
                  <MessageSquare className="text-muted-foreground/50 mx-auto h-12 w-12" />
                  <h3 className="mt-4 text-lg font-medium">Sin leads aún</h3>
                  <p className="text-muted-foreground mt-1">
                    Los compradores interesados aparecerán aquí
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {recentLeads.map(lead => (
                    <Link
                      key={lead.id}
                      href={`/vender/leads/${lead.id}`}
                      className="hover:bg-accent/50 flex items-start gap-4 rounded-lg border p-4 transition-colors"
                    >
                      <div className="bg-primary/10 text-primary shrink-0 rounded-full p-2">
                        <Mail className="h-5 w-5" />
                      </div>
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center gap-2">
                          <span className="truncate font-medium">{lead.buyerName}</span>
                          {getStatusBadge(lead.status)}
                        </div>
                        <p className="text-muted-foreground mt-1 truncate text-sm">
                          {lead.vehicleTitle}
                        </p>
                        <p className="text-muted-foreground truncate text-sm">{lead.message}</p>
                      </div>
                      <div className="text-muted-foreground text-xs whitespace-nowrap">
                        <Clock className="mr-1 inline h-3 w-3" />
                        {new Date(lead.createdAt).toLocaleDateString('es-DO', {
                          day: 'numeric',
                          month: 'short',
                        })}
                      </div>
                    </Link>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* My Vehicles */}
        <TabsContent value="vehicles">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle>Mis Vehículos</CardTitle>
                <CardDescription>Vehículos publicados y sus métricas</CardDescription>
              </div>
              <Button variant="outline" size="sm" asChild>
                <Link href="/mis-vehiculos">
                  Ver todos
                  <ArrowUpRight className="ml-1 h-4 w-4" />
                </Link>
              </Button>
            </CardHeader>
            <CardContent>
              {myVehicles.length === 0 ? (
                <div className="py-12 text-center">
                  <Car className="text-muted-foreground/50 mx-auto h-12 w-12" />
                  <h3 className="mt-4 text-lg font-medium">Sin vehículos publicados</h3>
                  <p className="text-muted-foreground mt-1">
                    Publica tu primer vehículo para comenzar a recibir leads
                  </p>
                  <Button className="mt-4" asChild>
                    <Link href="/publicar">
                      <Plus className="mr-2 h-4 w-4" />
                      Publicar Vehículo
                    </Link>
                  </Button>
                </div>
              ) : (
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
                  {myVehicles.map(vehicle => (
                    <Link
                      key={vehicle.id}
                      href={`/mis-vehiculos/${vehicle.id}`}
                      className="group overflow-hidden rounded-lg border transition-shadow hover:shadow-md"
                    >
                      <div className="bg-muted relative aspect-video">
                        {vehicle.imageUrl ? (
                          <img
                            src={vehicle.imageUrl}
                            alt={vehicle.title}
                            className="h-full w-full object-cover"
                          />
                        ) : (
                          <div className="flex h-full w-full items-center justify-center">
                            <Car className="text-muted-foreground/30 h-12 w-12" />
                          </div>
                        )}
                        <Badge
                          className="absolute top-2 right-2"
                          variant={vehicle.status === 'Active' ? 'default' : 'secondary'}
                        >
                          {vehicle.status === 'Active' ? 'Activo' : vehicle.status}
                        </Badge>
                      </div>
                      <div className="p-3">
                        <h4 className="group-hover:text-primary truncate font-medium transition-colors">
                          {vehicle.title}
                        </h4>
                        <p className="text-primary mt-1 text-lg font-bold">
                          RD${vehicle.price.toLocaleString('es-DO')}
                        </p>
                        <div className="text-muted-foreground mt-2 flex items-center gap-3 text-sm">
                          <span className="flex items-center gap-1">
                            <Eye className="h-3.5 w-3.5" />
                            {vehicle.viewCount}
                          </span>
                          <span className="flex items-center gap-1">
                            <Heart className="h-3.5 w-3.5" />
                            {vehicle.favoriteCount}
                          </span>
                          <span className="flex items-center gap-1">
                            <Phone className="h-3.5 w-3.5" />
                            {vehicle.inquiryCount}
                          </span>
                        </div>
                      </div>
                    </Link>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <Card className="transition-shadow hover:shadow-md">
          <Link href="/vender/creditos">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="rounded-full bg-purple-50 p-3 text-purple-600">
                <Package className="h-6 w-6" />
              </div>
              <div>
                <h3 className="font-medium">Comprar Créditos</h3>
                <p className="text-muted-foreground text-sm">Paquetes de publicaciones</p>
              </div>
            </CardContent>
          </Link>
        </Card>

        <Card className="transition-shadow hover:shadow-md">
          <Link href="/vender/facturas">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="rounded-full bg-green-50 p-3 text-green-600">
                <CreditCard className="h-6 w-6" />
              </div>
              <div>
                <h3 className="font-medium">Historial de Pagos</h3>
                <p className="text-muted-foreground text-sm">Facturas y recibos</p>
              </div>
            </CardContent>
          </Link>
        </Card>

        <Card className="transition-shadow hover:shadow-md">
          <Link href="/cuenta/perfil">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="rounded-full bg-blue-50 p-3 text-blue-600">
                <BarChart3 className="h-6 w-6" />
              </div>
              <div>
                <h3 className="font-medium">Mi Perfil</h3>
                <p className="text-muted-foreground text-sm">Configuración de cuenta</p>
              </div>
            </CardContent>
          </Link>
        </Card>
      </div>
    </div>
  );
}
