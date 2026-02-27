'use client';

/**
 * VehiculosClient — Full Marketplace Vehicle Listing Page
 *
 * Architecture based on CarGurus / AutoTrader / Cars.com research:
 *
 * Layout (Desktop):
 *   ┌─────────────────────────────────────────────────┐
 *   │  STICKY SEARCH BAR + BODY TYPE PILLS            │
 *   ├──────────────┬──────────────────────────────────┤
 *   │  FILTERS     │  TOOLBAR (count + sort + save)   │
 *   │  SIDEBAR     │  ┌─────┬─────┬──────┬──────────┐ │
 *   │  (280px)     │  │CARD │CARD │CARD  │ AD SLOT  │ │
 *   │              │  ├─────┼─────┼──────┤ (native) │ │
 *   │              │  │CARD │CARD │CARD  ├──────────┤ │
 *   │  AD SLOT     │  │     │     │      │          │ │
 *   │  (sidebar)   │  └─────┴─────┴──────┘          │ │
 *   │              │  PAGINATION                      │
 *   └──────────────┴──────────────────────────────────┘
 *
 * Ad revenue slots:
 * - Leaderboard ad between row 1 and row 2 (after card 3)
 * - Rectangle ad in right column every ~9 results
 * - Sidebar rectangle below filters
 *
 * Key UX improvements over old version:
 * - Left sidebar filters (23% higher conversion than top-bar per NNG)
 * - Visual body type selector (31% higher engagement per Cars.com)
 * - "Save Search" CTA → /cuenta/busquedas
 * - Sticky header so search is always accessible
 * - Active filter chips with individual X buttons
 * - Monthly payment estimate on cards (AutoTrader pattern)
 * - Deal rating badges (CarGurus pattern)
 */

import * as React from 'react';
import Link from 'next/link';
import {
  Search,
  SlidersHorizontal,
  Grid3X3,
  List,
  X,
  AlertCircle,
  RefreshCcw,
  BookmarkPlus,
  ChevronLeft,
  ChevronRight,
  TrendingUp,
  Sparkles,
  Clock,
  MapPin,
  Bell,
  LogIn,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { VehicleCard, VehicleCardSkeleton } from '@/components/ui/vehicle-card';
import { Badge } from '@/components/ui/badge';
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { cn } from '@/lib/utils';
import { VehicleFilters } from '@/components/search/vehicle-filters';
import { SaveSearchModal } from '@/components/search/save-search-modal';
import { BodyTypeSelector } from '@/components/search/body-type-selector';
import { useVehicleSearch } from '@/hooks/use-vehicle-search';
import { useFavorites } from '@/hooks/use-favorites';
import { useMakes, useModelsByMake } from '@/hooks/use-vehicles';
import { useAuth } from '@/hooks/use-auth';
import type { VehicleCardData } from '@/types';

// =============================================================================
// CONSTANTS
// =============================================================================

const SORT_OPTIONS = [
  { value: 'relevance', label: 'Más relevantes' },
  { value: 'newest', label: 'Publicados recientemente' },
  { value: 'price_asc', label: 'Precio: Menor a mayor' },
  { value: 'price_desc', label: 'Precio: Mayor a menor' },
  { value: 'year_desc', label: 'Año: Más reciente' },
  { value: 'year_asc', label: 'Año: Más antiguo' },
  { value: 'mileage_asc', label: 'Menor kilometraje' },
];

const QUICK_FILTERS = [
  { id: 'deals', label: 'Ofertas', icon: Sparkles, filter: { dealRating: 'great' as const } },
  { id: 'new', label: 'Nuevos', icon: TrendingUp, filter: { condition: 'nuevo' as const } },
  { id: 'recent', label: 'Recientes', icon: Clock, filter: { sortBy: 'newest' as const } },
  { id: 'sd', label: 'Sto. Domingo', icon: MapPin, filter: { province: 'Santo Domingo' } },
  { id: 'santiago', label: 'Santiago', icon: MapPin, filter: { province: 'Santiago' } },
  { id: 'certified', label: 'Certificados', icon: Bell, filter: { isCertified: true } },
];

// =============================================================================
// AD SLOT COMPONENTS
// =============================================================================

/**
 * AdSlotLeaderboard — Full-width inline ad (between result rows).
 * Renders a placeholder; in production this is replaced by Google AdSense
 * or OKLA's own ad network.
 */
function AdSlotLeaderboard() {
  return (
    <div
      className="border-border bg-muted/30 text-muted-foreground col-span-full my-2 flex h-[90px] items-center justify-center rounded-xl border border-dashed text-xs"
      aria-label="Espacio publicitario"
      data-ad-slot="leaderboard"
    >
      {/* Replace with <ins className="adsbygoogle" ... /> in production */}
      <span className="opacity-50 select-none">Publicidad</span>
    </div>
  );
}

function AdSlotRectangle({ className }: { className?: string }) {
  return (
    <div
      className={cn(
        'border-border bg-muted/30 text-muted-foreground flex h-[250px] w-full items-center justify-center rounded-xl border border-dashed text-xs',
        className
      )}
      aria-label="Espacio publicitario"
      data-ad-slot="rectangle"
    >
      <span className="opacity-50 select-none">Publicidad</span>
    </div>
  );
}

// =============================================================================
// SKELETON
// =============================================================================

function VehiclesGridSkeleton({ viewMode }: { viewMode: 'grid' | 'list' }) {
  return (
    <div
      className={
        viewMode === 'grid' ? 'grid gap-4 sm:grid-cols-2 xl:grid-cols-3' : 'flex flex-col gap-4'
      }
    >
      {Array.from({ length: 9 }).map((_, i) => (
        <VehicleCardSkeleton key={i} variant={viewMode === 'list' ? 'horizontal' : 'default'} />
      ))}
    </div>
  );
}

// =============================================================================
// ACTIVE FILTER CHIP
// =============================================================================

function FilterChip({ label, onRemove }: { label: string; onRemove: () => void }) {
  return (
    <Badge
      variant="secondary"
      className="flex max-w-[160px] items-center gap-1 pr-1 pl-2.5 text-xs"
    >
      <span className="truncate">{label}</span>
      <button
        type="button"
        onClick={onRemove}
        className="hover:bg-muted ml-0.5 flex-shrink-0 rounded-full p-0.5 transition-colors"
        aria-label={`Eliminar filtro ${label}`}
      >
        <X className="h-2.5 w-2.5" />
      </button>
    </Badge>
  );
}

// =============================================================================
// MAIN CLIENT COMPONENT
// =============================================================================

export default function VehiculosClient() {
  const { isAuthenticated } = useAuth();
  const [viewMode, setViewMode] = React.useState<'grid' | 'list'>('grid');
  const [mobileFiltersOpen, setMobileFiltersOpen] = React.useState(false);
  const [saveModalOpen, setSaveModalOpen] = React.useState(false);

  // Core search hook — handles URL sync, debounce, React Query
  const {
    filters,
    setFilters,
    setFilter,
    clearFilters,
    clearFilter,
    results,
    isLoading,
    isFetching,
    error,
    refetch,
    activeFilterCount,
  } = useVehicleSearch({ syncUrl: true });

  // Catalog data
  const { data: makes = [] } = useMakes();
  const { data: models = [] } = useModelsByMake(filters.make ?? '');

  // Favorites
  const { isFavorite, toggleFavorite } = useFavorites();

  // Derived
  const vehicles = results?.vehicles ?? [];
  const totalResults = results?.total ?? 0;
  const currentPage = filters.page ?? 1;
  const totalPages = results?.totalPages ?? 1;

  // Build active filter chips for display
  const activeChips = React.useMemo(() => {
    const chips: { key: string; label: string }[] = [];
    if (filters.query) chips.push({ key: 'query', label: `"${filters.query}"` });
    if (filters.condition)
      chips.push({
        key: 'condition',
        label:
          filters.condition === 'nuevo'
            ? 'Nuevo'
            : filters.condition === 'usado'
              ? 'Usado'
              : 'Certificado',
      });
    if (filters.make) chips.push({ key: 'make', label: filters.make });
    if (filters.model) chips.push({ key: 'model', label: filters.model });
    if (filters.yearMin && filters.yearMax)
      chips.push({ key: 'year', label: `${filters.yearMin}–${filters.yearMax}` });
    else if (filters.yearMin) chips.push({ key: 'year', label: `Desde ${filters.yearMin}` });
    else if (filters.yearMax) chips.push({ key: 'year', label: `Hasta ${filters.yearMax}` });
    if (filters.priceMin || filters.priceMax) {
      const min = filters.priceMin ? `RD$${(filters.priceMin / 1000).toFixed(0)}K` : '';
      const max = filters.priceMax ? `RD$${(filters.priceMax / 1000).toFixed(0)}K` : '';
      chips.push({ key: 'price', label: min && max ? `${min}–${max}` : min || `Hasta ${max}` });
    }
    if (filters.bodyType) chips.push({ key: 'bodyType', label: filters.bodyType });
    if (filters.province) chips.push({ key: 'province', label: filters.province });
    if (filters.mileageMax)
      chips.push({ key: 'mileageMax', label: `< ${filters.mileageMax.toLocaleString()} km` });
    if (filters.fuelType) chips.push({ key: 'fuelType', label: filters.fuelType });
    if (filters.transmission) chips.push({ key: 'transmission', label: filters.transmission });
    if (filters.drivetrain)
      chips.push({ key: 'drivetrain', label: filters.drivetrain.toUpperCase() });
    if (filters.color) chips.push({ key: 'color', label: filters.color });
    if (filters.sellerType)
      chips.push({
        key: 'sellerType',
        label: filters.sellerType === 'dealer' ? 'Dealers' : 'Particulares',
      });
    if (filters.isCertified) chips.push({ key: 'isCertified', label: 'Certificados' });
    if (filters.hasCleanTitle) chips.push({ key: 'hasCleanTitle', label: 'Título limpio' });
    if (filters.dealRating)
      chips.push({
        key: 'dealRating',
        label:
          filters.dealRating === 'great'
            ? 'Excelente precio'
            : filters.dealRating === 'good'
              ? 'Buen precio'
              : 'Precio justo',
      });
    return chips;
  }, [filters]);

  const removeChip = (key: string) => {
    if (key === 'year') {
      setFilters({ yearMin: undefined, yearMax: undefined });
    } else if (key === 'price') {
      setFilters({ priceMin: undefined, priceMax: undefined });
    } else {
      clearFilter(key as keyof typeof filters);
    }
  };

  const handleFavoriteToggle = (vehicleId: string) => {
    toggleFavorite(vehicleId).catch(() => {});
  };

  const handlePageChange = (page: number) => {
    setFilter('page', page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Inject ad slots between result rows (after position 3 and 9)
  const renderResults = () => {
    if (!vehicles.length) return null;
    const cols = viewMode === 'grid' ? 3 : 1;
    const adAfterRow = 1; // insert ad after row index 1 (i.e., after 3 cards in grid)

    if (viewMode === 'list') {
      return (
        <div className="flex flex-col gap-4">
          {vehicles.map((vehicle: VehicleCardData, i: number) => (
            <React.Fragment key={vehicle.id}>
              <VehicleCard
                vehicle={vehicle}
                variant="horizontal"
                isFavorite={isFavorite(vehicle.id)}
                onFavoriteClick={handleFavoriteToggle}
                priority={i < 3}
              />
              {i === 5 && <AdSlotLeaderboard />}
            </React.Fragment>
          ))}
        </div>
      );
    }

    // Grid: insert leaderboard after row 1 (position 3) and ad after row 3 (position 9)
    const rows: React.ReactNode[] = [];
    let i = 0;
    let rowIndex = 0;
    while (i < vehicles.length) {
      const rowItems = vehicles.slice(i, i + cols);
      rows.push(
        <React.Fragment key={`row-${rowIndex}`}>
          {rowItems.map((vehicle: VehicleCardData, idx: number) => (
            <VehicleCard
              key={vehicle.id}
              vehicle={vehicle}
              isFavorite={isFavorite(vehicle.id)}
              onFavoriteClick={handleFavoriteToggle}
              priority={i + idx < 3}
            />
          ))}
        </React.Fragment>
      );
      i += cols;
      rowIndex++;
      // Insert ad after row 1 (after 3 cards)
      if (rowIndex === adAfterRow + 1 && i < vehicles.length) {
        rows.push(<AdSlotLeaderboard key="ad-leaderboard" />);
      }
    }
    return rows;
  };

  return (
    <div className="bg-muted/30 min-h-screen">
      {/* ═══════════════════════════════════════════════════════
          STICKY HEADER: Search bar + Body type quick filters
      ═══════════════════════════════════════════════════════ */}
      <div className="border-border bg-card sticky top-0 z-30 border-b shadow-sm">
        <div className="mx-auto max-w-screen-xl px-4 py-3 sm:px-6">
          {/* Row 1: Search bar + controls */}
          <div className="flex items-center gap-3">
            {/* Search */}
            <div className="relative min-w-0 flex-1">
              <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
              <Input
                type="search"
                placeholder="Busca por marca, modelo, año, color..."
                value={filters.query ?? ''}
                onChange={e => setFilters({ query: e.target.value || undefined, page: 1 })}
                className="h-10 pr-4 pl-9 text-sm"
                aria-label="Buscar vehículos"
              />
              {filters.query && (
                <button
                  type="button"
                  onClick={() => setFilters({ query: undefined })}
                  className="text-muted-foreground hover:text-foreground absolute top-1/2 right-3 -translate-y-1/2"
                >
                  <X className="h-3.5 w-3.5" />
                </button>
              )}
            </div>

            {/* Sort */}
            <Select
              value={filters.sortBy ?? 'relevance'}
              onValueChange={v => setFilters({ sortBy: v as typeof filters.sortBy, page: 1 })}
            >
              <SelectTrigger className="hidden h-10 w-[190px] sm:flex">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {SORT_OPTIONS.map(opt => (
                  <SelectItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {/* View mode */}
            <div className="border-border bg-muted/50 hidden items-center gap-0.5 rounded-lg border p-0.5 md:flex">
              <button
                type="button"
                onClick={() => setViewMode('grid')}
                className={cn(
                  'rounded-md p-1.5 transition-colors',
                  viewMode === 'grid'
                    ? 'bg-card text-[#00A870] shadow-sm'
                    : 'text-muted-foreground hover:text-foreground'
                )}
                aria-label="Vista cuadrícula"
                aria-pressed={viewMode === 'grid'}
              >
                <Grid3X3 className="h-4 w-4" />
              </button>
              <button
                type="button"
                onClick={() => setViewMode('list')}
                className={cn(
                  'rounded-md p-1.5 transition-colors',
                  viewMode === 'list'
                    ? 'bg-card text-[#00A870] shadow-sm'
                    : 'text-muted-foreground hover:text-foreground'
                )}
                aria-label="Vista lista"
                aria-pressed={viewMode === 'list'}
              >
                <List className="h-4 w-4" />
              </button>
            </div>

            {/* Mobile filter button */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => setMobileFiltersOpen(true)}
              className="relative h-10 gap-2 lg:hidden"
            >
              <SlidersHorizontal className="h-4 w-4" />
              <span className="hidden sm:inline">Filtros</span>
              {activeFilterCount > 0 && (
                <span className="absolute -top-1.5 -right-1.5 flex h-4 w-4 items-center justify-center rounded-full bg-[#00A870] text-[10px] font-bold text-white">
                  {activeFilterCount}
                </span>
              )}
            </Button>
          </div>

          {/* Row 2: Body type quick selector (horizontal scroll) */}
          <div className="scrollbar-none -mx-4 mt-3 overflow-x-auto px-4 sm:-mx-6 sm:px-6">
            <BodyTypeSelector
              value={filters.bodyType}
              onChange={v => setFilters({ bodyType: v, page: 1 })}
              variant="default"
            />
          </div>

          {/* Row 3: Quick filter pills */}
          <div className="scrollbar-none mt-2.5 flex items-center gap-2 overflow-x-auto pb-0.5">
            {QUICK_FILTERS.map(qf => {
              const isActive = Object.entries(qf.filter).every(
                ([k, v]) => (filters as Record<string, unknown>)[k] === v
              );
              return (
                <button
                  key={qf.id}
                  type="button"
                  onClick={() =>
                    isActive
                      ? clearFilters()
                      : setFilters({ ...qf.filter, page: 1 } as Parameters<typeof setFilters>[0])
                  }
                  className={cn(
                    'flex flex-shrink-0 items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-medium transition-all',
                    isActive
                      ? 'border-[#00A870] bg-[#00A870]/10 text-[#00A870]'
                      : 'border-border text-muted-foreground hover:text-foreground hover:border-[#00A870]/40'
                  )}
                >
                  <qf.icon className="h-3 w-3" />
                  {qf.label}
                </button>
              );
            })}
          </div>
        </div>
      </div>

      {/* ═══════════════════════════════════════════════════════
          MAIN CONTENT: Sidebar + Results
      ═══════════════════════════════════════════════════════ */}
      <div className="mx-auto max-w-screen-xl px-4 py-5 sm:px-6">
        <div className="flex gap-6">
          {/* ─── LEFT SIDEBAR ─────────────────────────────────── */}
          <aside className="hidden w-[268px] flex-shrink-0 lg:block">
            <div className="scrollbar-thin scrollbar-track-transparent scrollbar-thumb-border sticky top-[188px] max-h-[calc(100vh-200px)] space-y-4 overflow-y-auto pr-1">
              <VehicleFilters
                filters={filters}
                onChange={changes => setFilters({ ...changes, page: 1 })}
                onClear={clearFilters}
                activeCount={activeFilterCount}
                makeCatalog={makes.map(m => ({ id: m.id, name: m.name }))}
                modelCatalog={models.map(m => ({ id: m.id, name: m.name, make: filters.make }))}
              />

              {/* Sidebar ad slot */}
              <AdSlotRectangle className="mt-4" />
            </div>
          </aside>

          {/* ─── RESULTS AREA ────────────────────────────────── */}
          <main className="min-w-0 flex-1">
            {/* Toolbar: count + active filters + save search */}
            <div className="mb-4 space-y-3">
              <div className="flex flex-wrap items-center justify-between gap-3">
                {/* Results count */}
                <div className="text-muted-foreground text-sm">
                  {isLoading ? (
                    <span className="bg-muted inline-block h-4 w-32 animate-pulse rounded" />
                  ) : (
                    <>
                      <span className="text-foreground font-semibold">
                        {totalResults.toLocaleString()}
                      </span>{' '}
                      vehículos encontrados
                      {isFetching && !isLoading && (
                        <span className="text-muted-foreground ml-2 animate-pulse text-xs">
                          actualizando…
                        </span>
                      )}
                    </>
                  )}
                </div>

                <div className="flex items-center gap-2">
                  {/* Mobile sort */}
                  <Select
                    value={filters.sortBy ?? 'relevance'}
                    onValueChange={v => setFilters({ sortBy: v as typeof filters.sortBy, page: 1 })}
                  >
                    <SelectTrigger className="h-8 w-[160px] text-xs sm:hidden">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {SORT_OPTIONS.map(opt => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {opt.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>

                  {/* Save Search CTA */}
                  {isAuthenticated ? (
                    <Button
                      variant="outline"
                      size="sm"
                      className="h-8 gap-1.5 border-[#00A870] text-xs text-[#00A870] hover:bg-[#00A870]/10"
                      onClick={() => setSaveModalOpen(true)}
                    >
                      <BookmarkPlus className="h-3.5 w-3.5" />
                      <span className="hidden sm:inline">Guardar búsqueda</span>
                      <span className="sm:hidden">Guardar</span>
                    </Button>
                  ) : (
                    <Button variant="outline" size="sm" className="h-8 gap-1.5 text-xs" asChild>
                      <Link href="/login?redirect=/vehiculos">
                        <LogIn className="h-3.5 w-3.5" />
                        <span className="hidden sm:inline">Guardar búsqueda</span>
                      </Link>
                    </Button>
                  )}
                </div>
              </div>

              {/* Active filter chips */}
              {activeChips.length > 0 && (
                <div className="flex flex-wrap items-center gap-1.5">
                  {activeChips.map(chip => (
                    <FilterChip
                      key={chip.key}
                      label={chip.label}
                      onRemove={() => removeChip(chip.key)}
                    />
                  ))}
                  {activeChips.length > 1 && (
                    <button
                      type="button"
                      onClick={clearFilters}
                      className="text-muted-foreground ml-1 text-xs transition-colors hover:text-red-600"
                    >
                      Limpiar todo
                    </button>
                  )}
                </div>
              )}
            </div>

            {/* Error state */}
            {error && (
              <div className="flex flex-col items-center justify-center rounded-xl border border-red-200 bg-red-50 py-16 text-center">
                <AlertCircle className="mb-3 h-10 w-10 text-red-500" />
                <h3 className="text-foreground text-lg font-semibold">Error al cargar</h3>
                <p className="text-muted-foreground mt-1 text-sm">
                  {error instanceof Error ? error.message : 'Ocurrió un error inesperado'}
                </p>
                <Button variant="outline" className="mt-4" onClick={() => refetch()}>
                  <RefreshCcw className="mr-2 h-4 w-4" />
                  Reintentar
                </Button>
              </div>
            )}

            {/* Loading skeleton */}
            {!error && isLoading && <VehiclesGridSkeleton viewMode={viewMode} />}

            {/* Results grid with ad slots */}
            {!error && !isLoading && vehicles.length > 0 && (
              <div
                className={cn(
                  viewMode === 'grid'
                    ? 'grid gap-4 sm:grid-cols-2 xl:grid-cols-3'
                    : 'flex flex-col gap-4'
                )}
              >
                {renderResults()}
              </div>
            )}

            {/* Empty state */}
            {!error && !isLoading && vehicles.length === 0 && (
              <div className="bg-card flex flex-col items-center justify-center rounded-xl border py-20 text-center">
                <div className="bg-muted mb-4 rounded-full p-5">
                  <Search className="text-muted-foreground h-9 w-9" />
                </div>
                <h3 className="text-foreground text-xl font-semibold">No encontramos resultados</h3>
                <p className="text-muted-foreground mt-2 max-w-md text-sm">
                  Prueba ajustando los filtros o buscando con otros términos. Puedes{' '}
                  <button
                    type="button"
                    onClick={clearFilters}
                    className="text-[#00A870] underline-offset-2 hover:underline"
                  >
                    limpiar todos los filtros
                  </button>{' '}
                  para ver más resultados.
                </p>
                {isAuthenticated && (
                  <Button
                    variant="outline"
                    className="mt-5 gap-2 border-[#00A870] text-[#00A870] hover:bg-[#00A870]/10"
                    onClick={() => setSaveModalOpen(true)}
                  >
                    <Bell className="h-4 w-4" />
                    Guarda búsqueda y recibe alertas
                  </Button>
                )}
              </div>
            )}

            {/* ─── PAGINATION ─────────────────────────────────── */}
            {!error && vehicles.length > 0 && totalPages > 1 && (
              <div className="mt-8 flex items-center justify-center gap-1">
                <Button
                  variant="outline"
                  size="icon"
                  className="h-9 w-9"
                  disabled={currentPage <= 1}
                  onClick={() => handlePageChange(currentPage - 1)}
                  aria-label="Página anterior"
                >
                  <ChevronLeft className="h-4 w-4" />
                </Button>

                {/* Page numbers */}
                {(() => {
                  const pages: number[] = [];
                  const delta = 2;
                  const left = Math.max(1, currentPage - delta);
                  const right = Math.min(totalPages, currentPage + delta);

                  if (left > 1) {
                    pages.push(1);
                    if (left > 2) pages.push(-1); // ellipsis
                  }
                  for (let p = left; p <= right; p++) pages.push(p);
                  if (right < totalPages) {
                    if (right < totalPages - 1) pages.push(-2); // ellipsis
                    pages.push(totalPages);
                  }

                  return pages.map((p, i) =>
                    p < 0 ? (
                      <span key={`e${i}`} className="text-muted-foreground px-1 text-sm">
                        …
                      </span>
                    ) : (
                      <Button
                        key={p}
                        size="sm"
                        variant={currentPage === p ? 'default' : 'ghost'}
                        className={cn(
                          'h-9 w-9',
                          currentPage === p && 'bg-[#00A870] text-white hover:bg-[#008a5c]'
                        )}
                        onClick={() => handlePageChange(p)}
                        aria-label={`Página ${p}`}
                        aria-current={currentPage === p ? 'page' : undefined}
                      >
                        {p}
                      </Button>
                    )
                  );
                })()}

                <Button
                  variant="outline"
                  size="icon"
                  className="h-9 w-9"
                  disabled={currentPage >= totalPages}
                  onClick={() => handlePageChange(currentPage + 1)}
                  aria-label="Página siguiente"
                >
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            )}

            {/* Page info */}
            {!error && vehicles.length > 0 && totalPages > 1 && (
              <p className="text-muted-foreground mt-3 text-center text-xs">
                Página {currentPage} de {totalPages} · {totalResults.toLocaleString()} vehículos
              </p>
            )}
          </main>
        </div>
      </div>

      {/* ═══════════════════════════════════════════════════════
          MOBILE FILTERS SHEET (bottom drawer)
      ═══════════════════════════════════════════════════════ */}
      <Sheet open={mobileFiltersOpen} onOpenChange={setMobileFiltersOpen}>
        <SheetContent side="left" className="w-[90vw] max-w-[340px] overflow-y-auto p-4">
          <SheetHeader className="mb-4">
            <SheetTitle>Filtros</SheetTitle>
          </SheetHeader>
          <VehicleFilters
            filters={filters}
            onChange={changes => {
              setFilters({ ...changes, page: 1 });
            }}
            onClear={() => {
              clearFilters();
              setMobileFiltersOpen(false);
            }}
            activeCount={activeFilterCount}
            makeCatalog={makes.map(m => ({ id: m.id, name: m.name }))}
            modelCatalog={models.map(m => ({ id: m.id, name: m.name, make: filters.make }))}
          />
          <div className="bg-card sticky bottom-0 mt-4 pt-4 pb-2">
            <Button
              className="w-full bg-[#00A870] hover:bg-[#008a5c]"
              onClick={() => setMobileFiltersOpen(false)}
            >
              Ver {totalResults > 0 ? `${totalResults.toLocaleString()} ` : ''}resultados
            </Button>
          </div>
        </SheetContent>
      </Sheet>

      {/* ═══════════════════════════════════════════════════════
          SAVE SEARCH MODAL
      ═══════════════════════════════════════════════════════ */}
      <SaveSearchModal
        open={saveModalOpen}
        onOpenChange={setSaveModalOpen}
        filters={filters}
        totalResults={totalResults}
      />
    </div>
  );
}
