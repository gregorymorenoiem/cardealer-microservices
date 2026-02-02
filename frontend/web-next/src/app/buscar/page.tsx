'use client';

import * as React from 'react';
import { Suspense } from 'react';
import { VehicleFilters, VehicleSearchResults } from '@/components/search';
import { useVehicleSearch } from '@/hooks/use-vehicle-search';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Filter, X, MapPin, Sparkles, TrendingUp, Clock } from 'lucide-react';
import { cn } from '@/lib/utils';

// Loading skeleton for search
function SearchLoading() {
  return (
    <div className="container mx-auto px-4 py-6">
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
        <aside className="lg:col-span-1">
          <Skeleton className="h-[600px] w-full rounded-lg" />
        </aside>
        <main className="lg:col-span-3">
          <Skeleton className="mb-4 h-12 w-full rounded-lg" />
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-[300px] w-full rounded-lg" />
            ))}
          </div>
        </main>
      </div>
    </div>
  );
}

// Quick filters component
interface QuickFilter {
  id: string;
  label: string;
  icon: React.ElementType;
  filters: Record<string, unknown>;
}

const quickFilters: QuickFilter[] = [
  {
    id: 'best-deals',
    label: 'Mejores Ofertas',
    icon: Sparkles,
    filters: { dealRating: ['great', 'good'] },
  },
  {
    id: 'new-listings',
    label: 'Nuevos',
    icon: Clock,
    filters: { sortBy: 'newest' },
  },
  {
    id: 'low-mileage',
    label: 'Bajo Kilometraje',
    icon: TrendingUp,
    filters: { maxMileage: 50000 },
  },
  {
    id: 'santo-domingo',
    label: 'Santo Domingo',
    icon: MapPin,
    filters: { location: 'santo-domingo' },
  },
  {
    id: 'santiago',
    label: 'Santiago',
    icon: MapPin,
    filters: { location: 'santiago' },
  },
];

// Main search content
function SearchContent() {
  const {
    filters,
    setFilters,
    clearFilters,
    results,
    isLoading,
    isFetching,
    error,
    activeFilterCount,
  } = useVehicleSearch();

  const [showMobileFilters, setShowMobileFilters] = React.useState(false);
  const [activeQuickFilters, setActiveQuickFilters] = React.useState<string[]>([]);

  // Toggle quick filter
  const toggleQuickFilter = (filter: QuickFilter) => {
    const isActive = activeQuickFilters.includes(filter.id);

    if (isActive) {
      setActiveQuickFilters(prev => prev.filter(id => id !== filter.id));
      // Remove the filter values - simplified approach
      clearFilters();
    } else {
      setActiveQuickFilters(prev => [...prev, filter.id]);
      setFilters({ ...filters, ...filter.filters } as typeof filters);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Hero Section */}
      <div className="bg-gradient-to-r from-emerald-600 to-emerald-700 py-12 text-white">
        <div className="container mx-auto px-4">
          <h1 className="mb-2 text-3xl font-bold md:text-4xl">Búsqueda de Vehículos</h1>
          <p className="mb-6 text-lg text-emerald-100">
            Encuentra tu próximo vehículo entre miles de opciones
          </p>

          {/* Quick Filters */}
          <div className="flex flex-wrap gap-2">
            {quickFilters.map(filter => {
              const Icon = filter.icon;
              const isActive = activeQuickFilters.includes(filter.id);

              return (
                <Button
                  key={filter.id}
                  variant={isActive ? 'secondary' : 'outline'}
                  size="sm"
                  className={cn(
                    'rounded-full transition-all',
                    isActive
                      ? 'bg-white text-emerald-700 hover:bg-gray-100'
                      : 'border-white/30 bg-white/10 text-white hover:bg-white/20'
                  )}
                  onClick={() => toggleQuickFilter(filter)}
                >
                  <Icon className="mr-1.5 h-4 w-4" />
                  {filter.label}
                </Button>
              );
            })}
          </div>
        </div>
      </div>

      {/* Active Filters Bar */}
      {activeFilterCount > 0 && (
        <div className="border-b bg-white py-3">
          <div className="container mx-auto px-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="text-sm text-gray-600">Filtros activos:</span>
                <Badge variant="secondary" className="bg-emerald-100 text-emerald-700">
                  {activeFilterCount}
                </Badge>

                {/* Show some active filters as badges */}
                {filters.make && (
                  <Badge variant="outline" className="gap-1">
                    Marca: {filters.make}
                  </Badge>
                )}
                {(filters.priceMin || filters.priceMax) && (
                  <Badge variant="outline" className="gap-1">
                    Precio: {filters.priceMin ? `$${filters.priceMin.toLocaleString()}` : '$0'} -{' '}
                    {filters.priceMax ? `$${filters.priceMax.toLocaleString()}` : '∞'}
                  </Badge>
                )}
              </div>

              <Button
                variant="ghost"
                size="sm"
                className="text-gray-500 hover:text-gray-700"
                onClick={() => {
                  clearFilters();
                  setActiveQuickFilters([]);
                }}
              >
                <X className="mr-1 h-4 w-4" />
                Limpiar todo
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Main Content */}
      <div className="container mx-auto px-4 py-6">
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
          {/* Desktop Filters Sidebar */}
          <aside className="hidden lg:col-span-1 lg:block">
            <div className="sticky top-4">
              <VehicleFilters
                filters={filters}
                onChange={setFilters}
                onClear={clearFilters}
                activeCount={activeFilterCount}
              />
            </div>
          </aside>

          {/* Results */}
          <main className="lg:col-span-3">
            {/* Mobile Filter Button */}
            <div className="mb-4 lg:hidden">
              <Button
                variant="outline"
                className="w-full justify-center"
                onClick={() => setShowMobileFilters(true)}
              >
                <Filter className="mr-2 h-4 w-4" />
                Filtros
                {activeFilterCount > 0 && (
                  <Badge variant="secondary" className="ml-2 bg-emerald-100 text-emerald-700">
                    {activeFilterCount}
                  </Badge>
                )}
              </Button>
            </div>

            {/* Results with toolbar */}
            <VehicleSearchResults
              vehicles={results?.vehicles ?? []}
              total={results?.total ?? 0}
              page={results?.page ?? 1}
              totalPages={results?.totalPages ?? 1}
              filters={filters}
              isLoading={isLoading}
              isFetching={isFetching}
              onFilterChange={setFilters}
              onClearFilters={clearFilters}
              activeFilterCount={activeFilterCount}
              onPageChange={page => setFilters({ page })}
            />
          </main>
        </div>
      </div>

      {/* SEO Content */}
      <section className="border-t bg-white py-12">
        <div className="container mx-auto px-4">
          <h2 className="mb-6 text-2xl font-bold">
            Encuentra el vehículo perfecto en República Dominicana
          </h2>

          <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
            <div>
              <h3 className="mb-2 text-lg font-semibold">Miles de opciones</h3>
              <p className="text-gray-600">
                Explora la mayor selección de vehículos en RD. Desde económicos hasta lujo, nuevos y
                usados.
              </p>
            </div>

            <div>
              <h3 className="mb-2 text-lg font-semibold">Precios transparentes</h3>
              <p className="text-gray-600">
                Nuestro sistema de Deal Rating te ayuda a identificar las mejores ofertas del
                mercado.
              </p>
            </div>

            <div>
              <h3 className="mb-2 text-lg font-semibold">Vendedores verificados</h3>
              <p className="text-gray-600">
                Dealers y vendedores verificados para que compres con total confianza y seguridad.
              </p>
            </div>
          </div>

          {/* Popular Searches */}
          <div className="mt-10">
            <h3 className="mb-4 text-lg font-semibold">Búsquedas Populares</h3>
            <div className="flex flex-wrap gap-2">
              {[
                'Toyota Corolla',
                'Honda CR-V',
                'Hyundai Tucson',
                'Kia Sportage',
                'Toyota RAV4',
                'Nissan Kicks',
                'Mazda CX-5',
                'Hyundai Santa Fe',
                'Toyota Hilux',
                'Jeep Wrangler',
              ].map(search => (
                <a
                  key={search}
                  href={`/buscar?q=${encodeURIComponent(search)}`}
                  className="rounded-full bg-gray-100 px-3 py-1.5 text-sm text-gray-700 transition-colors hover:bg-gray-200"
                >
                  {search}
                </a>
              ))}
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}

// Page export with Suspense
export default function BuscarPage() {
  return (
    <Suspense fallback={<SearchLoading />}>
      <SearchContent />
    </Suspense>
  );
}
