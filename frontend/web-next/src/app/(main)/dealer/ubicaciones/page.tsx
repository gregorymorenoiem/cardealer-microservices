/**
 * Dealer Locations Page
 *
 * Manage dealer branches and locations
 */

'use client';

import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { MapPin, Plus, Edit, Trash2, Phone, Clock, Building, Users, Loader2 } from 'lucide-react';
import { useCurrentDealer, useDealerLocations, useDeleteDealerLocation } from '@/hooks/use-dealers';
import type { DealerLocationDto, BusinessHours } from '@/services/dealers';
import { toast } from 'sonner';

// ============================================================================
// Helper Functions
// ============================================================================

function getLocationTypeLabel(type: DealerLocationDto['locationType']): string {
  const labels: Record<DealerLocationDto['locationType'], string> = {
    headquarters: 'Sede Principal',
    branch: 'Sucursal',
    showroom: 'Sala de Exhibición',
    serviceCenter: 'Centro de Servicio',
    warehouse: 'Almacén',
  };
  return labels[type] || type;
}

function formatBusinessHours(hours?: BusinessHours): {
  weekdays: string;
  saturday: string;
  sunday: string;
} {
  if (!hours) {
    return { weekdays: 'No disponible', saturday: 'No disponible', sunday: 'Cerrado' };
  }

  // Get weekday hours (using Monday as reference)
  const monday = hours.monday;
  const weekdays = monday && !monday.isClosed ? `${monday.open} - ${monday.close}` : 'Cerrado';

  const saturday =
    hours.saturday && !hours.saturday.isClosed
      ? `${hours.saturday.open} - ${hours.saturday.close}`
      : 'Cerrado';

  const sunday =
    hours.sunday && !hours.sunday.isClosed
      ? `${hours.sunday.open} - ${hours.sunday.close}`
      : 'Cerrado';

  return { weekdays, saturday, sunday };
}

// ============================================================================
// Loading Skeletons
// ============================================================================

function StatsSkeleton() {
  return (
    <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
      {[1, 2, 3, 4].map(i => (
        <Card key={i}>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <Skeleton className="h-9 w-9 rounded-lg" />
              <div>
                <Skeleton className="mb-1 h-7 w-8" />
                <Skeleton className="h-4 w-20" />
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

function LocationsSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2].map(i => (
        <Card key={i}>
          <CardContent className="p-6">
            <div className="flex flex-col gap-6 lg:flex-row">
              <Skeleton className="h-40 w-full lg:w-64" />
              <div className="flex-1">
                <Skeleton className="mb-2 h-6 w-48" />
                <Skeleton className="mb-4 h-4 w-64" />
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                  {[1, 2, 3, 4].map(j => (
                    <Skeleton key={j} className="h-4 w-28" />
                  ))}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

export default function DealerLocationsPage() {
  // Get current dealer
  const { data: dealer, isLoading: dealerLoading } = useCurrentDealer();
  const dealerId = dealer?.id || '';

  // Get locations
  const { data: locations, isLoading: locationsLoading } = useDealerLocations(dealerId);

  // Mutations
  const deleteLocation = useDeleteDealerLocation(dealerId);

  const isLoading = dealerLoading || locationsLoading;

  // Calculate stats from locations
  const totalLocations = locations?.length || 0;
  const primaryLocation = locations?.find(l => l.isPrimary);
  const headquartersCount = locations?.filter(l => l.locationType === 'headquarters').length || 0;
  const branchCount = locations?.filter(l => l.locationType === 'branch').length || 0;

  // Handle delete
  const handleDelete = async (locationId: string, locationName: string) => {
    if (!confirm(`¿Eliminar la ubicación "${locationName}"?`)) return;

    try {
      await deleteLocation.mutateAsync(locationId);
      toast.success('Ubicación eliminada');
    } catch {
      toast.error('Error al eliminar la ubicación');
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex flex-col justify-between gap-4 sm:flex-row">
          <div>
            <Skeleton className="mb-2 h-8 w-40" />
            <Skeleton className="h-4 w-60" />
          </div>
          <Skeleton className="h-10 w-40" />
        </div>
        <StatsSkeleton />
        <LocationsSkeleton />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Ubicaciones</h1>
          <p className="text-muted-foreground">Gestiona tus sucursales y puntos de venta</p>
        </div>
        <Button className="bg-primary hover:bg-primary/90">
          <Plus className="mr-2 h-4 w-4" />
          Nueva Ubicación
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-primary/10 p-2">
                <Building className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{totalLocations}</p>
                <p className="text-sm text-muted-foreground">Ubicaciones</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Building className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{headquartersCount}</p>
                <p className="text-sm text-muted-foreground">Sedes</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <Users className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{branchCount}</p>
                <p className="text-sm text-muted-foreground">Sucursales</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-100 p-2">
                <MapPin className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{primaryLocation ? 1 : 0}</p>
                <p className="text-sm text-muted-foreground">Principal</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Locations List */}
      <div className="space-y-4">
        {locations && locations.length > 0 ? (
          locations.map(location => {
            const hours = formatBusinessHours(location.businessHours);
            return (
              <Card key={location.id} className={location.isPrimary ? 'border-primary' : ''}>
                <CardContent className="p-6">
                  <div className="flex flex-col gap-6 lg:flex-row">
                    {/* Map placeholder */}
                    <div className="flex h-40 items-center justify-center rounded-lg bg-muted lg:h-auto lg:w-64">
                      <div className="text-center text-muted-foreground">
                        <MapPin className="mx-auto mb-2 h-8 w-8" />
                        <p className="text-sm">Vista del Mapa</p>
                      </div>
                    </div>

                    {/* Details */}
                    <div className="flex-1">
                      <div className="mb-4 flex flex-col justify-between gap-2 sm:flex-row sm:items-start">
                        <div>
                          <div className="flex items-center gap-2">
                            <h3 className="text-lg font-semibold">{location.name}</h3>
                            {location.isPrimary && (
                              <Badge className="bg-primary/10 text-primary">Principal</Badge>
                            )}
                            <Badge variant="outline">
                              {getLocationTypeLabel(location.locationType)}
                            </Badge>
                          </div>
                          <p className="flex items-center gap-1 text-muted-foreground">
                            <MapPin className="h-4 w-4" />
                            {location.address}, {location.city}, {location.province}
                          </p>
                        </div>
                        <div className="flex gap-2">
                          <Button variant="outline" size="sm">
                            <Edit className="mr-1 h-4 w-4" />
                            Editar
                          </Button>
                          {!location.isPrimary && (
                            <Button
                              variant="outline"
                              size="sm"
                              className="text-red-600 hover:bg-red-50"
                              onClick={() => handleDelete(location.id, location.name)}
                              disabled={deleteLocation.isPending}
                            >
                              {deleteLocation.isPending ? (
                                <Loader2 className="h-4 w-4 animate-spin" />
                              ) : (
                                <Trash2 className="h-4 w-4" />
                              )}
                            </Button>
                          )}
                        </div>
                      </div>

                      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                        {location.phone && (
                          <div className="flex items-center gap-2">
                            <Phone className="h-4 w-4 text-muted-foreground" />
                            <span className="text-sm">{location.phone}</span>
                          </div>
                        )}
                        {location.email && (
                          <div className="flex items-center gap-2">
                            <Building className="h-4 w-4 text-muted-foreground" />
                            <span className="text-sm">{location.email}</span>
                          </div>
                        )}
                      </div>

                      <div className="mt-4 rounded-lg bg-muted/50 p-3">
                        <h4 className="mb-2 flex items-center gap-2 font-medium">
                          <Clock className="h-4 w-4" />
                          Horario de Atención
                        </h4>
                        <div className="grid grid-cols-3 gap-2 text-sm">
                          <div>
                            <p className="text-muted-foreground">Lun - Vie</p>
                            <p className="font-medium">{hours.weekdays}</p>
                          </div>
                          <div>
                            <p className="text-muted-foreground">Sábado</p>
                            <p className="font-medium">{hours.saturday}</p>
                          </div>
                          <div>
                            <p className="text-muted-foreground">Domingo</p>
                            <p className="font-medium">{hours.sunday}</p>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })
        ) : (
          <Card>
            <CardContent className="py-12 text-center">
              <MapPin className="mx-auto mb-4 h-12 w-12 text-gray-300" />
              <h3 className="mb-2 text-lg font-medium text-foreground">No hay ubicaciones</h3>
              <p className="mb-4 text-muted-foreground">Agrega tu primera ubicación para comenzar</p>
              <Button className="bg-primary hover:bg-primary/90">
                <Plus className="mr-2 h-4 w-4" />
                Agregar Ubicación
              </Button>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Tips */}
      <Card className="border-blue-200 bg-blue-50">
        <CardContent className="p-4">
          <h4 className="mb-2 font-medium text-blue-800">💡 Consejo</h4>
          <p className="text-sm text-blue-700">
            Añadir múltiples ubicaciones aumenta tu visibilidad. Los compradores pueden filtrar por
            ubicación y ver tu inventario más cercano a ellos.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
