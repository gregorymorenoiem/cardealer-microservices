/**
 * Compare Vehicles Page
 *
 * Allows users to compare up to 3 vehicles side by side
 *
 * Route: /comparar
 */

'use client';

import * as React from 'react';
import { Suspense } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { useSearchParams, useRouter } from 'next/navigation';
import {
  X,
  Plus,
  ArrowLeft,
  Check,
  Minus,
  Car,
  Gauge,
  Fuel,
  Settings,
  MapPin,
  Share2,
  Loader2,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import { useVehiclesByIds } from '@/hooks/use-vehicles';
import { useLocalComparison, useComparisonSpecs } from '@/hooks/use-comparisons';
import type { VehicleCardData } from '@/types';

// =============================================================================
// TYPES
// =============================================================================

interface CompareVehicle extends VehicleCardData {
  specs?: {
    engine?: string;
    horsepower?: number;
    mpg?: string;
    drivetrain?: string;
    doors?: number;
    seats?: number;
  };
  features?: string[];
}

// Spec comparison rows
const specRows = [
  {
    key: 'price',
    label: 'Precio',
    format: (v: CompareVehicle) => `RD$ ${v.price.toLocaleString()}`,
  },
  {
    key: 'mileage',
    label: 'Kilometraje',
    format: (v: CompareVehicle) => `${v.mileage.toLocaleString()} km`,
  },
  { key: 'transmission', label: 'Transmisión', format: (v: CompareVehicle) => v.transmission },
  { key: 'fuelType', label: 'Combustible', format: (v: CompareVehicle) => v.fuelType },
  { key: 'engine', label: 'Motor', format: (v: CompareVehicle) => v.specs?.engine || '—' },
  {
    key: 'horsepower',
    label: 'Potencia',
    format: (v: CompareVehicle) => (v.specs?.horsepower ? `${v.specs.horsepower} HP` : '—'),
  },
  { key: 'mpg', label: 'Consumo (MPG)', format: (v: CompareVehicle) => v.specs?.mpg || '—' },
  {
    key: 'drivetrain',
    label: 'Tracción',
    format: (v: CompareVehicle) => v.specs?.drivetrain || '—',
  },
  {
    key: 'doors',
    label: 'Puertas',
    format: (v: CompareVehicle) => v.specs?.doors?.toString() || '—',
  },
  {
    key: 'seats',
    label: 'Asientos',
    format: (v: CompareVehicle) => v.specs?.seats?.toString() || '—',
  },
  { key: 'location', label: 'Ubicación', format: (v: CompareVehicle) => v.location },
];

// =============================================================================
// COMPARE CONTENT COMPONENT (uses useSearchParams)
// =============================================================================

function CompareContent() {
  const router = useRouter();
  const searchParams = useSearchParams();

  // Get vehicle IDs from URL params or local storage
  const urlIds = searchParams.get('ids')?.split(',').filter(Boolean) || [];
  const localComparison = useLocalComparison();

  // Combine IDs (URL takes priority)
  const vehicleIds = urlIds.length > 0 ? urlIds : localComparison.vehicleIds;

  // Fetch vehicles data
  const { data: vehiclesData, isLoading, error } = useVehiclesByIds(vehicleIds);

  // Map to CompareVehicle type
  const vehicles: CompareVehicle[] = React.useMemo(() => {
    if (!vehiclesData) return [];
    return vehiclesData.map(v => ({
      ...v,
      specs: {},
      features: [],
    }));
  }, [vehiclesData]);

  const removeVehicle = (id: string) => {
    localComparison.removeVehicle(id);
    // Update URL if using URL params
    if (urlIds.length > 0) {
      const newIds = urlIds.filter(uid => uid !== id);
      if (newIds.length > 0) {
        router.replace(`/comparar?ids=${newIds.join(',')}`);
      } else {
        router.replace('/comparar');
      }
    }
  };

  const clearAll = () => {
    localComparison.clearAll();
    router.replace('/comparar');
  };

  const shareComparison = () => {
    const ids = vehicles.map(v => v.id).join(',');
    const url = `${window.location.origin}/comparar?ids=${ids}`;
    navigator.clipboard.writeText(url);
    // TODO: Show toast notification
    alert('Enlace copiado al portapapeles');
  };

  // Get unique features across all vehicles
  const featuresInComparison = React.useMemo(() => {
    const allVehicleFeatures = new Set<string>();
    vehicles.forEach(v => v.features?.forEach(f => allVehicleFeatures.add(f)));
    return Array.from(allVehicleFeatures).sort();
  }, [vehicles]);

  if (isLoading) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <div className="text-center">
          <div className="mx-auto h-8 w-8 animate-spin rounded-full border-4 border-[#00A870] border-t-transparent" />
          <p className="mt-2 text-gray-600">Cargando comparación...</p>
        </div>
      </div>
    );
  }

  // Empty state
  if (vehicles.length === 0) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="container mx-auto px-4">
          <Link
            href="/vehiculos"
            className="inline-flex items-center gap-1 text-sm text-gray-600 hover:text-gray-900"
          >
            <ArrowLeft className="h-4 w-4" />
            Volver a vehículos
          </Link>

          <div className="mt-8 flex min-h-[50vh] flex-col items-center justify-center text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
              <Car className="h-8 w-8 text-gray-400" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900">Sin vehículos para comparar</h1>
            <p className="mt-2 max-w-md text-gray-600">
              Agrega vehículos a la comparación desde la página de búsqueda o detalle de vehículo.
            </p>
            <Button asChild className="mt-6 gap-2 bg-[#00A870] hover:bg-[#009663]">
              <Link href="/vehiculos">
                <Car className="h-4 w-4" />
                Explorar vehículos
              </Link>
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="container mx-auto px-4">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div>
            <Link
              href="/vehiculos"
              className="inline-flex items-center gap-1 text-sm text-gray-600 hover:text-gray-900"
            >
              <ArrowLeft className="h-4 w-4" />
              Volver
            </Link>
            <h1 className="mt-2 text-2xl font-bold text-gray-900">Comparar vehículos</h1>
            <p className="text-gray-600">
              {vehicles.length} de 3 vehículos • Compara especificaciones y características
            </p>
          </div>

          <div className="flex gap-2">
            <Button variant="outline" onClick={shareComparison} className="gap-2">
              <Share2 className="h-4 w-4" />
              Compartir
            </Button>
            <Button
              variant="outline"
              onClick={clearAll}
              className="gap-2 text-red-600 hover:bg-red-50"
            >
              <X className="h-4 w-4" />
              Limpiar
            </Button>
          </div>
        </div>

        {/* Vehicle Cards Header */}
        <div
          className="mb-6 grid gap-4"
          style={{ gridTemplateColumns: `repeat(${Math.min(vehicles.length + 1, 4)}, 1fr)` }}
        >
          {/* Empty first column for labels */}
          <div className="hidden lg:block" />

          {/* Vehicle cards */}
          {vehicles.map(vehicle => (
            <Card key={vehicle.id} className="relative overflow-hidden">
              <button
                onClick={() => removeVehicle(vehicle.id)}
                className="absolute top-2 right-2 z-10 flex h-6 w-6 items-center justify-center rounded-full bg-black/50 text-white transition-colors hover:bg-black/70"
              >
                <X className="h-4 w-4" />
              </button>

              <div className="aspect-[16/10] overflow-hidden bg-gray-100">
                <img
                  src={vehicle.imageUrl}
                  alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
                  className="h-full w-full object-cover"
                />
              </div>

              <CardContent className="p-4">
                <h2 className="font-bold text-gray-900">
                  {vehicle.year} {vehicle.make} {vehicle.model}
                </h2>
                {vehicle.trim && <p className="text-sm text-gray-600">{vehicle.trim}</p>}
                <p className="mt-2 text-xl font-bold text-[#00A870]">
                  RD$ {vehicle.price.toLocaleString()}
                </p>
                {vehicle.dealRating && (
                  <Badge
                    className={cn(
                      'mt-2',
                      vehicle.dealRating === 'great' && 'bg-green-500',
                      vehicle.dealRating === 'good' && 'bg-blue-500',
                      vehicle.dealRating === 'fair' && 'bg-yellow-500',
                      vehicle.dealRating === 'high' && 'bg-red-500'
                    )}
                  >
                    {vehicle.dealRating === 'great' && 'Excelente precio'}
                    {vehicle.dealRating === 'good' && 'Buen precio'}
                    {vehicle.dealRating === 'fair' && 'Precio justo'}
                    {vehicle.dealRating === 'high' && 'Precio alto'}
                  </Badge>
                )}

                <div className="mt-4">
                  <Button asChild variant="outline" size="sm" className="w-full">
                    <Link href={`/vehiculos/${vehicle.slug}`}>Ver detalle</Link>
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}

          {/* Add vehicle card */}
          {vehicles.length < 3 && (
            <Card className="flex min-h-[300px] cursor-pointer items-center justify-center border-2 border-dashed transition-colors hover:border-[#00A870] hover:bg-[#00A870]/5">
              <Link href="/vehiculos" className="flex flex-col items-center p-6 text-center">
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-[#00A870]/10">
                  <Plus className="h-6 w-6 text-[#00A870]" />
                </div>
                <p className="mt-2 font-medium text-gray-900">Agregar vehículo</p>
                <p className="text-sm text-gray-500">Hasta 3 vehículos</p>
              </Link>
            </Card>
          )}
        </div>

        {/* Specifications Table */}
        <Card className="mb-6 overflow-hidden">
          <div className="border-b bg-gray-50 px-4 py-3">
            <h3 className="font-semibold text-gray-900">Especificaciones</h3>
          </div>

          <div className="divide-y">
            {specRows.map(row => (
              <div
                key={row.key}
                className="grid gap-4 p-4"
                style={{ gridTemplateColumns: `200px repeat(${vehicles.length}, 1fr)` }}
              >
                <div className="font-medium text-gray-700">{row.label}</div>
                {vehicles.map(vehicle => (
                  <div key={vehicle.id} className="text-gray-900">
                    {row.format(vehicle)}
                  </div>
                ))}
              </div>
            ))}
          </div>
        </Card>

        {/* Features Comparison */}
        <Card className="overflow-hidden">
          <div className="border-b bg-gray-50 px-4 py-3">
            <h3 className="font-semibold text-gray-900">Características</h3>
          </div>

          <div className="divide-y">
            {featuresInComparison.map(feature => (
              <div
                key={feature}
                className="grid gap-4 p-4"
                style={{ gridTemplateColumns: `200px repeat(${vehicles.length}, 1fr)` }}
              >
                <div className="font-medium text-gray-700">{feature}</div>
                {vehicles.map(vehicle => (
                  <div key={vehicle.id} className="flex items-center">
                    {vehicle.features?.includes(feature) ? (
                      <Check className="h-5 w-5 text-green-500" />
                    ) : (
                      <Minus className="h-5 w-5 text-gray-300" />
                    )}
                  </div>
                ))}
              </div>
            ))}
          </div>
        </Card>
      </div>
    </div>
  );
}

// =============================================================================
// LOADING FALLBACK
// =============================================================================

function CompareLoading() {
  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <div className="text-center">
        <div className="mx-auto h-8 w-8 animate-spin rounded-full border-4 border-[#00A870] border-t-transparent" />
        <p className="mt-2 text-gray-600">Cargando comparación...</p>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN PAGE COMPONENT WITH SUSPENSE
// =============================================================================

export default function CompararPage() {
  return (
    <Suspense fallback={<CompareLoading />}>
      <CompareContent />
    </Suspense>
  );
}
