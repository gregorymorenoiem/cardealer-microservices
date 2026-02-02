/**
 * Admin Vehicles Page
 *
 * Vehicle management and moderation page for administrators
 * Connected to real APIs - February 2026
 */

'use client';

import { useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog';
import {
  Car,
  Search,
  ChevronLeft,
  ChevronRight,
  Eye,
  CheckCircle,
  XCircle,
  Flag,
  Clock,
  Star,
  Trash2,
  RefreshCw,
  Loader2,
  AlertTriangle,
} from 'lucide-react';
import { toast } from 'sonner';
import {
  useAdminVehicles,
  useVehicleStats,
  useApproveVehicle,
  useRejectVehicle,
  useToggleFeatured,
  useDeleteVehicle,
  type VehicleFilters,
} from '@/hooks/use-admin';
import { Textarea } from '@/components/ui/textarea';

// =============================================================================
// SKELETON
// =============================================================================

function VehiclesSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-10 w-32" />
      </div>
      <div className="grid gap-4 md:grid-cols-5">
        {[1, 2, 3, 4, 5].map(i => (
          <Skeleton key={i} className="h-24" />
        ))}
      </div>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {[1, 2, 3, 4, 5, 6].map(i => (
          <Skeleton key={i} className="h-64" />
        ))}
      </div>
    </div>
  );
}

// =============================================================================
// HELPERS
// =============================================================================

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'active':
      return <Badge className="bg-emerald-100 text-emerald-700">Activo</Badge>;
    case 'pending':
      return <Badge className="bg-amber-100 text-amber-700">Pendiente</Badge>;
    case 'rejected':
      return <Badge className="bg-red-100 text-red-700">Rechazado</Badge>;
    case 'sold':
      return <Badge className="bg-blue-100 text-blue-700">Vendido</Badge>;
    case 'paused':
      return <Badge className="bg-gray-100 text-gray-700">Pausado</Badge>;
    default:
      return <Badge variant="secondary">{status}</Badge>;
  }
};

const formatPrice = (price: number, currency: string) => {
  const formatted = price.toLocaleString('es-DO');
  return currency === 'USD' ? `US$ ${formatted}` : `RD$ ${formatted}`;
};

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function AdminVehiclesPage() {
  const [filters, setFilters] = useState<VehicleFilters>({
    page: 1,
    pageSize: 12,
  });
  const [searchTerm, setSearchTerm] = useState('');
  const [rejectReason, setRejectReason] = useState('');
  const [rejectingId, setRejectingId] = useState<string | null>(null);

  const { data: vehiclesData, isLoading, refetch } = useAdminVehicles(filters);
  const { data: stats } = useVehicleStats();
  const approveMutation = useApproveVehicle();
  const rejectMutation = useRejectVehicle();
  const toggleFeaturedMutation = useToggleFeatured();
  const deleteMutation = useDeleteVehicle();

  const handleSearch = () => {
    setFilters(prev => ({ ...prev, search: searchTerm, page: 1 }));
  };

  const handleFilterChange = (key: string, value: string) => {
    setFilters(prev => ({
      ...prev,
      [key]: value === 'all' ? undefined : value,
      page: 1,
    }));
  };

  const handleApprove = async (id: string) => {
    try {
      await approveMutation.mutateAsync(id);
      toast.success('Vehículo aprobado');
    } catch {
      toast.error('Error al aprobar vehículo');
    }
  };

  const handleReject = async () => {
    if (!rejectingId || !rejectReason) return;
    try {
      await rejectMutation.mutateAsync({ id: rejectingId, reason: rejectReason });
      toast.success('Vehículo rechazado');
      setRejectingId(null);
      setRejectReason('');
    } catch {
      toast.error('Error al rechazar vehículo');
    }
  };

  const handleToggleFeatured = async (id: string, featured: boolean) => {
    try {
      await toggleFeaturedMutation.mutateAsync({ id, featured: !featured });
      toast.success(featured ? 'Quitado de destacados' : 'Agregado a destacados');
    } catch {
      toast.error('Error al actualizar');
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteMutation.mutateAsync(id);
      toast.success('Vehículo eliminado');
    } catch {
      toast.error('Error al eliminar vehículo');
    }
  };

  if (isLoading) {
    return <VehiclesSkeleton />;
  }

  const vehicles = vehiclesData?.items || [];
  const totalPages = vehiclesData?.totalPages || 1;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Vehículos</h1>
          <p className="text-gray-600">Modera y gestiona los vehículos publicados</p>
        </div>
        <div className="flex gap-2">
          <Link href="/admin/vehiculos/pendientes">
            <Button variant="outline">
              <Clock className="mr-2 h-4 w-4" />
              Pendientes ({stats?.pending || 0})
            </Button>
          </Link>
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="mr-2 h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Car className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-xl font-bold">{stats?.total.toLocaleString() || 0}</p>
                <p className="text-xs text-gray-600">Total</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-emerald-100 p-2">
                <CheckCircle className="h-5 w-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-xl font-bold">{stats?.active.toLocaleString() || 0}</p>
                <p className="text-xs text-gray-600">Activos</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <Clock className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-xl font-bold">{stats?.pending || 0}</p>
                <p className="text-xs text-gray-600">Pendientes</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-100 p-2">
                <Star className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-xl font-bold">{stats?.featured || 0}</p>
                <p className="text-xs text-gray-600">Destacados</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-red-100 p-2">
                <Flag className="h-5 w-5 text-red-600" />
              </div>
              <div>
                <p className="text-xl font-bold">{stats?.withReports || 0}</p>
                <p className="text-xs text-gray-600">Con reportes</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="p-4">
          <div className="flex flex-wrap gap-4">
            <div className="flex flex-1 gap-2">
              <Input
                placeholder="Buscar por marca, modelo, vendedor..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                onKeyDown={e => e.key === 'Enter' && handleSearch()}
                className="max-w-sm"
              />
              <Button onClick={handleSearch}>
                <Search className="h-4 w-4" />
              </Button>
            </div>
            <Select
              value={filters.status || 'all'}
              onValueChange={v => handleFilterChange('status', v)}
            >
              <SelectTrigger className="w-36">
                <SelectValue placeholder="Estado" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="active">Activos</SelectItem>
                <SelectItem value="pending">Pendientes</SelectItem>
                <SelectItem value="rejected">Rechazados</SelectItem>
                <SelectItem value="sold">Vendidos</SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={filters.sellerType || 'all'}
              onValueChange={v => handleFilterChange('sellerType', v)}
            >
              <SelectTrigger className="w-36">
                <SelectValue placeholder="Vendedor" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="individual">Individual</SelectItem>
                <SelectItem value="dealer">Dealer</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Vehicles Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {vehicles.map(vehicle => (
          <Card key={vehicle.id} className="overflow-hidden">
            <div className="relative aspect-[16/10]">
              <Image
                src={vehicle.image || '/placeholder-car.jpg'}
                alt={vehicle.title}
                fill
                className="object-cover"
              />
              <div className="absolute top-2 left-2 flex gap-2">
                {getStatusBadge(vehicle.status)}
                {vehicle.featured && (
                  <Badge className="bg-purple-600 text-white">
                    <Star className="mr-1 h-3 w-3" />
                    Destacado
                  </Badge>
                )}
              </div>
              {vehicle.reportsCount > 0 && (
                <div className="absolute top-2 right-2">
                  <Badge className="bg-red-600 text-white">
                    <Flag className="mr-1 h-3 w-3" />
                    {vehicle.reportsCount} reportes
                  </Badge>
                </div>
              )}
            </div>
            <CardContent className="p-4">
              <h3 className="mb-1 font-semibold text-gray-900">{vehicle.title}</h3>
              <p className="mb-2 text-lg font-bold text-emerald-600">
                {formatPrice(vehicle.price, vehicle.currency)}
              </p>
              <div className="mb-3 flex items-center gap-2 text-sm text-gray-600">
                <span>{vehicle.sellerName}</span>
                <Badge variant="outline" className="text-xs">
                  {vehicle.sellerType === 'dealer' ? 'Dealer' : 'Individual'}
                </Badge>
              </div>
              <div className="mb-3 flex items-center gap-4 text-sm text-gray-500">
                <span className="flex items-center gap-1">
                  <Eye className="h-4 w-4" />
                  {vehicle.views}
                </span>
                <span>{new Date(vehicle.createdAt).toLocaleDateString('es-DO')}</span>
              </div>
              <div className="flex flex-wrap gap-2">
                <Link href={`/vehiculos/${vehicle.id}`} target="_blank">
                  <Button variant="outline" size="sm">
                    <Eye className="mr-1 h-4 w-4" />
                    Ver
                  </Button>
                </Link>
                {vehicle.status === 'pending' && (
                  <>
                    <Button
                      size="sm"
                      className="bg-emerald-600 hover:bg-emerald-700"
                      onClick={() => handleApprove(vehicle.id)}
                      disabled={approveMutation.isPending}
                    >
                      {approveMutation.isPending ? (
                        <Loader2 className="mr-1 h-4 w-4 animate-spin" />
                      ) : (
                        <CheckCircle className="mr-1 h-4 w-4" />
                      )}
                      Aprobar
                    </Button>
                    <AlertDialog
                      open={rejectingId === vehicle.id}
                      onOpenChange={open => !open && setRejectingId(null)}
                    >
                      <AlertDialogTrigger asChild>
                        <Button
                          variant="outline"
                          size="sm"
                          className="text-red-600"
                          onClick={() => setRejectingId(vehicle.id)}
                        >
                          <XCircle className="mr-1 h-4 w-4" />
                          Rechazar
                        </Button>
                      </AlertDialogTrigger>
                      <AlertDialogContent>
                        <AlertDialogHeader>
                          <AlertDialogTitle>Rechazar vehículo</AlertDialogTitle>
                          <AlertDialogDescription>
                            Ingresa el motivo del rechazo. El vendedor será notificado.
                          </AlertDialogDescription>
                        </AlertDialogHeader>
                        <Textarea
                          placeholder="Motivo del rechazo..."
                          value={rejectReason}
                          onChange={e => setRejectReason(e.target.value)}
                          rows={3}
                        />
                        <AlertDialogFooter>
                          <AlertDialogCancel onClick={() => setRejectReason('')}>
                            Cancelar
                          </AlertDialogCancel>
                          <AlertDialogAction
                            onClick={handleReject}
                            disabled={!rejectReason || rejectMutation.isPending}
                            className="bg-red-600 hover:bg-red-700"
                          >
                            {rejectMutation.isPending && (
                              <Loader2 className="mr-1 h-4 w-4 animate-spin" />
                            )}
                            Rechazar
                          </AlertDialogAction>
                        </AlertDialogFooter>
                      </AlertDialogContent>
                    </AlertDialog>
                  </>
                )}
                {vehicle.status === 'active' && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleToggleFeatured(vehicle.id, vehicle.featured)}
                    disabled={toggleFeaturedMutation.isPending}
                  >
                    <Star
                      className={`mr-1 h-4 w-4 ${vehicle.featured ? 'fill-amber-500 text-amber-500' : ''}`}
                    />
                    {vehicle.featured ? 'Quitar' : 'Destacar'}
                  </Button>
                )}
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button variant="ghost" size="sm" className="text-red-600">
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>¿Eliminar vehículo?</AlertDialogTitle>
                      <AlertDialogDescription>
                        Esta acción no se puede deshacer. Se eliminará permanentemente este
                        vehículo.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancelar</AlertDialogCancel>
                      <AlertDialogAction
                        onClick={() => handleDelete(vehicle.id)}
                        className="bg-red-600 hover:bg-red-700"
                      >
                        Eliminar
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {vehicles.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center text-gray-500">
            <Car className="mx-auto mb-2 h-8 w-8" />
            <p>No se encontraron vehículos</p>
          </CardContent>
        </Card>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-gray-600">
            Página {filters.page} de {totalPages}
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setFilters(prev => ({ ...prev, page: (prev.page || 1) - 1 }))}
              disabled={(filters.page || 1) <= 1}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setFilters(prev => ({ ...prev, page: (prev.page || 1) + 1 }))}
              disabled={(filters.page || 1) >= totalPages}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
