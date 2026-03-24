/**
 * ModelVehiclesClient — Client component for /marcas/[marca]/[modelo]
 *
 * Fetches and displays vehicles for a specific make+model combination.
 * Includes sorting, price statistics, and links to related models.
 */

'use client';

import { useState, useEffect, useMemo } from 'react';
import { VehicleCard } from '@/components/ui/vehicle-card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Car, ArrowRight, Loader2 } from 'lucide-react';
import Link from 'next/link';
import type { VehicleCardData } from '@/types';
import { formatPrice } from '@/lib/format';

interface ModelVehiclesClientProps {
  brand: string;
  model: string;
  brandName: string;
  modelName: string;
}

interface VehiclesResponse {
  items?: VehicleCardData[];
  data?: VehicleCardData[];
  totalCount?: number;
  total?: number;
}

export function ModelVehiclesClient({
  brand,
  model,
  brandName,
  modelName,
}: ModelVehiclesClientProps) {
  const [vehicles, setVehicles] = useState<VehicleCardData[]>([]);
  const [loading, setLoading] = useState(true);
  const [_error, setError] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState<string>('recent');
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    async function fetchVehicles() {
      try {
        setLoading(true);
        const params = new URLSearchParams({
          make: brand,
          model: model,
          pageSize: '24',
          page: '1',
        });

        if (sortBy === 'price-asc') params.set('sortBy', 'price');
        else if (sortBy === 'price-desc') params.set('sortBy', 'price_desc');
        else if (sortBy === 'year') params.set('sortBy', 'year_desc');
        else if (sortBy === 'mileage') params.set('sortBy', 'mileage');

        const response = await fetch(`/api/vehicles?${params.toString()}`);
        if (!response.ok) throw new Error('Failed to fetch vehicles');

        const data: VehiclesResponse = await response.json();
        const items = data.items || data.data || [];
        setVehicles(items);
        setTotalCount(data.totalCount ?? data.total ?? items.length);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Error loading vehicles');
        setVehicles([]);
      } finally {
        setLoading(false);
      }
    }

    fetchVehicles();
  }, [brand, model, sortBy]);

  // Price statistics
  const priceRange = useMemo(() => {
    if (vehicles.length === 0) return null;
    const prices = vehicles.map(v => v.price).filter(p => p > 0);
    if (prices.length === 0) return null;
    return {
      min: Math.min(...prices),
      max: Math.max(...prices),
      avg: Math.round(prices.reduce((a, b) => a + b, 0) / prices.length),
    };
  }, [vehicles]);

  // Year range
  const yearRange = useMemo(() => {
    if (vehicles.length === 0) return null;
    const years = vehicles.map(v => v.year).filter(y => y > 0);
    if (years.length === 0) return null;
    return {
      min: Math.min(...years),
      max: Math.max(...years),
    };
  }, [vehicles]);

  const combo = `${brandName} ${modelName}`;

  if (loading) {
    return (
      <div className="flex min-h-[300px] items-center justify-center">
        <div className="text-muted-foreground flex items-center gap-2">
          <Loader2 className="h-5 w-5 animate-spin" />
          <span>Cargando {combo}...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Stats Bar */}
      <div className="flex flex-wrap items-center gap-4 text-sm">
        <div className="text-foreground font-semibold">
          {totalCount} {combo} disponibles
        </div>

        {priceRange && (
          <>
            <div className="text-muted-foreground">
              Desde{' '}
              <span className="text-foreground font-medium">{formatPrice(priceRange.min)}</span>
            </div>
            <div className="text-muted-foreground">
              Promedio:{' '}
              <span className="text-foreground font-medium">{formatPrice(priceRange.avg)}</span>
            </div>
          </>
        )}

        {yearRange && yearRange.min !== yearRange.max && (
          <div className="text-muted-foreground">
            Años: {yearRange.min} – {yearRange.max}
          </div>
        )}

        <div className="ml-auto flex items-center gap-2">
          <Select value={sortBy} onValueChange={setSortBy}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Ordenar por" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="recent">Más recientes</SelectItem>
              <SelectItem value="price-asc">Menor precio</SelectItem>
              <SelectItem value="price-desc">Mayor precio</SelectItem>
              <SelectItem value="year">Más nuevo</SelectItem>
              <SelectItem value="mileage">Menor kilometraje</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Vehicle Grid */}
      {vehicles.length > 0 ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {vehicles.map((vehicle, index) => (
            <VehicleCard key={vehicle.id} vehicle={vehicle} priority={index < 3} />
          ))}
        </div>
      ) : (
        <div className="rounded-lg border p-12 text-center">
          <Car className="text-muted-foreground/40 mx-auto h-12 w-12" />
          <p className="mt-4 text-lg font-medium">No hay {combo} disponibles en este momento</p>
          <p className="text-muted-foreground mt-2 text-sm">
            Vuelve pronto o{' '}
            <Link href={`/marcas/${brand}`} className="text-primary underline">
              explora otros modelos de {brandName}
            </Link>
            .
          </p>
        </div>
      )}

      {/* View All CTA */}
      {totalCount > 24 && (
        <div className="text-center">
          <Link
            href={`/vehiculos?make=${brand}&model=${encodeURIComponent(model)}`}
            className="bg-primary text-primary-foreground hover:bg-primary/90 inline-flex items-center gap-2 rounded-lg px-6 py-3 text-sm font-medium transition-colors"
          >
            Ver todos los {combo} ({totalCount})
            <ArrowRight className="h-4 w-4" />
          </Link>
        </div>
      )}
    </div>
  );
}
