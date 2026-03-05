/**
 * HeroCompact Component
 *
 * Compact hero section that shows vehicles immediately "above the fold"
 * Follows UX best practice: Product visibility without scroll
 *
 * Features:
 * - Compact search bar at top
 * - Trust badges inline
 * - Featured vehicles grid visible without scrolling
 * - Full dark/light theme support
 */

'use client';

import { useState, useMemo, useCallback, useRef } from 'react';
import {
  Search,
  ChevronDown,
  Shield,
  CheckCircle2,
  Star,
  Heart,
  Gauge,
  MapPin,
  Sparkles,
  Loader2,
} from 'lucide-react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import Image from 'next/image';
import { cn, formatCurrency, formatMileage } from '@/lib/utils';
import type { Vehicle } from '@/services/homepage-sections';
import { aiSearch, aiFiltersToUrlParams } from '@/services/search-agent';

// =============================================================================
// TYPES
// =============================================================================

interface HeroCompactProps {
  className?: string;
}

interface TrustBadge {
  icon: typeof Shield;
  text: string;
}

// =============================================================================
// STATIC DATA
// =============================================================================

const TRUST_BADGES: TrustBadge[] = [
  { icon: Shield, text: 'Vendedores Verificados' },
  { icon: CheckCircle2, text: 'Historial Garantizado' },
  { icon: Star, text: 'Precios Transparentes' },
];

const POPULAR_MAKES = ['Toyota', 'Honda', 'Hyundai', 'Kia', 'Nissan', 'Mazda', 'Ford', 'Chevrolet'];

const QUICK_FILTERS = ['SUV', 'Sedán', 'Camioneta', 'Deportivo', 'Híbrido', 'Eléctrico'];

const modelsByMake: Record<string, string[]> = {
  Toyota: ['Corolla', 'Camry', 'RAV4', 'Hilux', 'Land Cruiser', 'Fortuner', 'Yaris', '4Runner'],
  Honda: ['Civic', 'Accord', 'CR-V', 'HR-V', 'Pilot', 'Odyssey', 'Fit'],
  Hyundai: ['Tucson', 'Santa Fe', 'Elantra', 'Sonata', 'Kona', 'Palisade', 'Accent'],
  Kia: ['Sportage', 'Sorento', 'Seltos', 'K5', 'Carnival', 'Soul', 'Forte'],
  Nissan: ['Sentra', 'Altima', 'Rogue', 'Pathfinder', 'Kicks', 'Frontier', 'Murano'],
  Mazda: ['CX-5', 'CX-30', 'CX-9', 'Mazda3', 'Mazda6', 'MX-5'],
  Ford: ['F-150', 'Explorer', 'Escape', 'Bronco', 'Ranger', 'Mustang', 'Edge'],
  Chevrolet: ['Silverado', 'Equinox', 'Tahoe', 'Traverse', 'Malibu', 'Camaro', 'Colorado'],
};

// =============================================================================
// NATURAL LANGUAGE HERO SEARCH BAR (replaces filter dropdowns)
// =============================================================================

function NaturalLanguageHeroSearch() {
  const router = useRouter();
  const inputRef = useRef<HTMLInputElement>(null);
  const [query, setQuery] = useState('');
  const [isSearching, setIsSearching] = useState(false);
  const [isFocused, setIsFocused] = useState(false);

  const handleSearch = useCallback(
    async (searchQuery?: string) => {
      const q = (searchQuery ?? query).trim();
      if (!q) return;

      setIsSearching(true);
      try {
        const result = await aiSearch({ query: q });
        if (result.aiFilters?.filtros_exactos) {
          const params = aiFiltersToUrlParams(result.aiFilters.filtros_exactos);
          const searchParams = new URLSearchParams();
          // Store reformulated query for display ONLY — NOT as 'q' which would create
          // an extra text-filter chip alongside the structured AI filters.
          const displayQuery = result.aiFilters.query_reformulada ?? q;
          searchParams.set('ai_query', displayQuery);
          Object.entries(params).forEach(([key, value]) => {
            if (value !== undefined && value !== null && value !== '') {
              searchParams.set(key, String(value));
            }
          });
          if (result.aiFilters.ordenar_por) {
            searchParams.set('sortBy', result.aiFilters.ordenar_por);
          }
          router.push(`/vehiculos?${searchParams.toString()}`);
        } else {
          router.push(`/vehiculos?q=${encodeURIComponent(q)}`);
        }
      } catch {
        router.push(`/vehiculos?q=${encodeURIComponent(q)}`);
      } finally {
        setIsSearching(false);
      }
    },
    [query, router]
  );

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        handleSearch();
      }
    },
    [handleSearch]
  );

  return (
    <div className="w-full">
      {/* Main Container — same style as the old CompactSearchBar */}
      <div
        className={cn(
          'bg-card/95 dark:bg-card/90 mx-auto w-full max-w-5xl rounded-2xl border p-2 shadow-2xl shadow-black/20 backdrop-blur-sm transition-all duration-300',
          isFocused ? 'border-primary shadow-primary/20' : 'border-white/20 dark:border-white/10'
        )}
      >
        <div className="flex flex-col gap-2 md:flex-row">
          {/* Sparkle icon + text input */}
          <div className="border-border bg-muted relative flex flex-1 items-center gap-3 rounded-xl border px-4">
            <Sparkles
              className={cn(
                'h-5 w-5 shrink-0 transition-colors',
                isFocused ? 'text-primary' : 'text-muted-foreground'
              )}
            />
            <input
              ref={inputRef}
              type="text"
              value={query}
              onChange={e => setQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              onFocus={() => setIsFocused(true)}
              onBlur={() => setIsFocused(false)}
              placeholder="Busca tu vehículo ideal"
              className="text-foreground placeholder:text-muted-foreground h-12 flex-1 bg-transparent text-sm font-medium focus:outline-none"
              disabled={isSearching}
              autoComplete="off"
            />
          </div>

          {/* Search Button — identical style to old filter search button */}
          <button
            onClick={() => handleSearch()}
            disabled={isSearching}
            className="bg-primary text-primary-foreground shadow-primary/30 hover:bg-primary/90 hover:shadow-primary/40 flex h-12 cursor-pointer items-center justify-center gap-2 rounded-xl px-8 text-sm font-semibold whitespace-nowrap shadow-lg transition-all duration-300 hover:shadow-xl disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSearching ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                <span>Buscando...</span>
              </>
            ) : (
              <>
                <Search className="h-4 w-4" />
                <span>Buscar</span>
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// COMPACT SEARCH BAR (kept for reference, no longer used in hero)
// =============================================================================

// =============================================================================
// QUICK FILTERS (Theme-aware for light backgrounds)
// =============================================================================

export function QuickFilters() {
  return (
    <div suppressHydrationWarning className="flex flex-wrap justify-center gap-2">
      {QUICK_FILTERS.map(filter => (
        <Link
          key={filter}
          href={`/vehiculos?body_type=${encodeURIComponent(filter)}`}
          className="border-border bg-muted/50 text-foreground hover:border-primary hover:bg-primary hover:text-primary-foreground rounded-full border px-4 py-2 text-xs font-semibold transition-all duration-200"
        >
          {filter}
        </Link>
      ))}
    </div>
  );
}

// =============================================================================
// QUICK FILTERS (For dark hero background)
// =============================================================================

function QuickFiltersHero() {
  return (
    <div className="flex flex-wrap justify-center gap-2">
      {QUICK_FILTERS.map(filter => (
        <Link
          key={filter}
          href={`/vehiculos?body_type=${encodeURIComponent(filter)}`}
          className="hover:border-primary hover:bg-primary rounded-full border border-white/50 bg-white/20 px-4 py-2 text-sm font-medium text-white backdrop-blur-md transition-all duration-200 hover:text-white"
        >
          {filter}
        </Link>
      ))}
    </div>
  );
}

// =============================================================================
// TRUST BADGES (For dark hero background)
// =============================================================================

function TrustBadgesHero() {
  return (
    <div className="flex flex-wrap items-center justify-center gap-6">
      {TRUST_BADGES.map(badge => (
        <div key={badge.text} className="flex items-center gap-2 text-white">
          <badge.icon className="text-primary h-5 w-5" />
          <span className="text-sm font-medium">{badge.text}</span>
        </div>
      ))}
    </div>
  );
}

// =============================================================================
// TRUST BADGES (INLINE - Theme-aware for light backgrounds)
// =============================================================================

export function TrustBadgesInline() {
  return (
    <div
      suppressHydrationWarning
      className="flex flex-wrap items-center justify-center gap-4 md:gap-6"
    >
      {TRUST_BADGES.map(badge => (
        <div key={badge.text} className="text-muted-foreground flex items-center gap-1.5">
          <badge.icon className="text-primary h-4 w-4" />
          <span className="text-xs font-medium">{badge.text}</span>
        </div>
      ))}
    </div>
  );
}

// =============================================================================
// VEHICLE CARD (COMPACT) - Exported for future use
// =============================================================================

interface VehicleCardCompactProps {
  vehicle: Vehicle;
  index: number;
}

export function VehicleCardCompact({ vehicle, index }: VehicleCardCompactProps) {
  const [isFavorite, setIsFavorite] = useState(false);

  const vehicleUrl = useMemo(() => {
    const slug = `${vehicle.year}-${vehicle.make}-${vehicle.model}`
      .toLowerCase()
      .replace(/\s+/g, '-');
    const shortId = (vehicle.id || '').replace(/-/g, '').slice(0, 8).toLowerCase();
    return `/vehiculos/${slug}-${shortId}`;
  }, [vehicle]);

  return (
    <div className="animate-slide-up group" style={{ animationDelay: `${index * 50}ms` }}>
      <Link href={vehicleUrl} className="block">
        <div className="border-border bg-card overflow-hidden rounded-xl border shadow-md transition-all duration-300 hover:-translate-y-1 hover:shadow-xl">
          {/* Image */}
          <div className="relative aspect-[4/3] overflow-hidden">
            <Image
              src={vehicle.images[0] || '/placeholder-car.jpg'}
              alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
              fill
              sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
              className="object-cover transition-transform duration-300 group-hover:scale-105"
              priority={index < 4}
            />

            {/* Favorite Button */}
            <button
              onClick={e => {
                e.preventDefault();
                setIsFavorite(!isFavorite);
              }}
              className="absolute top-2 left-2 rounded-full bg-white/90 p-1.5 shadow-md transition-colors hover:bg-white dark:bg-black/50 dark:hover:bg-black/70"
              aria-label={isFavorite ? 'Quitar de favoritos' : 'Agregar a favoritos'}
            >
              <Heart
                size={16}
                fill={isFavorite ? '#ef4444' : 'none'}
                stroke={isFavorite ? '#ef4444' : '#6b7280'}
              />
            </button>

            {/* Condition Badge */}
            {vehicle.condition === 'New' && (
              <div className="absolute bottom-2 left-2">
                <span className="bg-primary rounded-full px-2 py-0.5 text-[10px] font-semibold text-white">
                  Nuevo
                </span>
              </div>
            )}

            {/* Tier Badge */}
            {vehicle.tier && vehicle.tier !== 'basic' && (
              <div className="absolute top-2 right-2">
                {vehicle.tier === 'enterprise' && (
                  <span className="rounded-full bg-gradient-to-r from-amber-500 to-orange-600 px-2 py-0.5 text-[10px] font-semibold text-white">
                    Top Dealer
                  </span>
                )}
                {vehicle.tier === 'premium' && (
                  <span className="rounded-full bg-gradient-to-r from-purple-500 to-indigo-600 px-2 py-0.5 text-[10px] font-semibold text-white">
                    Premium
                  </span>
                )}
                {vehicle.tier === 'featured' && (
                  <span className="from-primary rounded-full bg-gradient-to-r to-teal-600 px-2 py-0.5 text-[10px] font-semibold text-white">
                    Destacado
                  </span>
                )}
              </div>
            )}
          </div>

          {/* Content */}
          <div className="p-3">
            <h3 className="group-hover:text-primary text-foreground truncate text-sm font-bold transition-colors">
              {vehicle.year} {vehicle.make} {vehicle.model}
            </h3>
            <p className="text-primary mt-0.5 text-lg font-bold">{formatCurrency(vehicle.price)}</p>

            {/* Quick Details */}
            <div className="text-muted-foreground mt-2 flex items-center gap-3 text-xs">
              <span className="flex items-center gap-1">
                <Gauge size={12} />
                {formatMileage(vehicle.mileage)}
              </span>
              <span className="flex items-center gap-1">
                <MapPin size={12} />
                {vehicle.location?.split(',')[0] || 'RD'}
              </span>
            </div>
          </div>
        </div>
      </Link>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function HeroCompact({ className }: HeroCompactProps) {
  return (
    <section className={cn('relative flex flex-col', className)}>
      {/* Hero Section - Full Screen with Background Image */}
      <div
        suppressHydrationWarning
        className="relative h-[calc(100svh-4rem)] w-full overflow-hidden bg-gray-900"
      >
        {/* Background Image */}
        <div className="absolute inset-0">
          <Image
            src="/hero-bg.jpg"
            alt="OKLA - Marketplace de Vehículos"
            fill
            className="object-cover"
            priority
            quality={80}
            sizes="100vw"
          />
          {/* Dark Overlay for text readability */}
          <div className="absolute inset-0 bg-gradient-to-r from-black/80 via-black/60 to-black/40" />
        </div>

        {/* Animated Glow Effects (subtle, over the image) */}
        <div className="pointer-events-none absolute inset-0 overflow-hidden">
          <div className="from-primary/20 animate-pulse-orb absolute -top-40 -right-40 h-96 w-96 rounded-full bg-gradient-to-br to-transparent blur-3xl" />
        </div>

        {/* Content - Centered */}
        <div className="relative z-10 mx-auto flex h-full max-w-7xl flex-col items-center justify-center px-4 sm:px-6 lg:px-8">
          {/* Headline */}
          <div className="animate-slide-down mb-8 text-center">
            <h1 className="mb-4 text-3xl leading-[1.1] font-bold tracking-tight text-white sm:text-4xl md:text-5xl lg:text-6xl xl:text-7xl">
              Tu próximo vehículo
              <br />
              está en <span className="text-primary">OKLA</span>
            </h1>
            <p className="mx-auto max-w-2xl text-base leading-relaxed text-white/80 sm:text-lg md:text-xl lg:text-2xl">
              Encuentra, compara y compra con total confianza en República Dominicana.
            </p>
          </div>

          {/* Search Bar — Natural Language */}
          <div
            className="animate-slide-up mb-6 w-full max-w-4xl"
            style={{ animationDelay: '300ms' }}
          >
            <NaturalLanguageHeroSearch />
          </div>

          {/* Quick Filters */}
          <div className="animate-fade-in mb-6" style={{ animationDelay: '500ms' }}>
            <QuickFiltersHero />
          </div>

          {/* Trust Badges */}
          <div className="animate-fade-in" style={{ animationDelay: '700ms' }}>
            <TrustBadgesHero />
          </div>
        </div>

        {/* Scroll Hint */}
        <div
          className="animate-fade-in absolute bottom-8 left-1/2 z-10 hidden -translate-x-1/2 flex-col items-center text-white/70 sm:flex"
          style={{ animationDelay: '1500ms' }}
        >
          <span className="mb-2 text-sm font-medium">Explora vehículos destacados</span>
          <div className="animate-bounce-gentle">
            <ChevronDown className="h-6 w-6" />
          </div>
        </div>
      </div>
    </section>
  );
}

export default HeroCompact;
