/**
 * Dealer Directory Page
 *
 * Public directory of verified dealers on the OKLA platform.
 * Allows buyers to browse and find dealers by city/province.
 *
 * Route: /dealers/directorio
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Search,
  MapPin,
  Star,
  Car,
  Shield,
  Building2,
  Phone,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { useDealers } from '@/hooks/use-dealers';
import type { Dealer } from '@/types';

// =============================================================================
// HELPERS
// =============================================================================

function generateDealerSlug(dealer: Dealer): string {
  return dealer.businessName
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '');
}

const PROVINCES = [
  'Distrito Nacional',
  'Santo Domingo',
  'Santiago',
  'Puerto Plata',
  'La Vega',
  'San Pedro de Macorís',
  'La Romana',
  'San Cristóbal',
  'Duarte',
  'La Altagracia',
];

// =============================================================================
// SKELETON
// =============================================================================

function DirectorySkeleton() {
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      {[1, 2, 3, 4, 5, 6].map(i => (
        <Card key={i}>
          <CardContent className="p-4">
            <div className="flex items-start gap-4">
              <Skeleton className="h-16 w-16 rounded-lg" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-5 w-40" />
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-4 w-24" />
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

// =============================================================================
// DEALER CARD
// =============================================================================

function DealerCard({ dealer }: { dealer: Dealer }) {
  const slug = generateDealerSlug(dealer);

  return (
    <Link href={`/dealers/${slug}`}>
      <Card className="hover:border-primary/50 h-full transition-colors">
        <CardContent className="p-4">
          <div className="flex items-start gap-4">
            {/* Logo */}
            <div className="bg-muted flex h-16 w-16 flex-shrink-0 items-center justify-center overflow-hidden rounded-lg">
              {dealer.logoUrl ? (
                <Image
                  src={dealer.logoUrl}
                  alt={dealer.businessName}
                  width={64}
                  height={64}
                  className="h-16 w-16 rounded-lg object-cover"
                />
              ) : (
                <Building2 className="text-muted-foreground h-8 w-8" />
              )}
            </div>

            {/* Info */}
            <div className="min-w-0 flex-1">
              <div className="flex items-center gap-2">
                <h3 className="text-foreground truncate font-semibold">{dealer.businessName}</h3>
                {dealer.verificationStatus === 'verified' && (
                  <Shield className="h-4 w-4 flex-shrink-0 text-blue-500" />
                )}
              </div>

              <div className="text-muted-foreground mt-1 flex items-center gap-1 text-sm">
                <MapPin className="h-3.5 w-3.5" />
                <span>
                  {dealer.city}, {dealer.province}
                </span>
              </div>

              <div className="mt-2 flex items-center gap-3 text-sm">
                {dealer.rating != null && dealer.rating > 0 && (
                  <div className="flex items-center gap-1">
                    <Star className="h-3.5 w-3.5 fill-amber-400 text-amber-400" />
                    <span className="font-medium">{dealer.rating.toFixed(1)}</span>
                    {dealer.reviewCount != null && dealer.reviewCount > 0 && (
                      <span className="text-muted-foreground">({dealer.reviewCount})</span>
                    )}
                  </div>
                )}
                <div className="text-muted-foreground flex items-center gap-1">
                  <Car className="h-3.5 w-3.5" />
                  <span>{dealer.currentActiveListings} vehículos</span>
                </div>
              </div>

              {/* Plan badge */}
              {dealer.plan !== 'libre' && (
                <Badge variant="secondary" className="mt-2 text-xs capitalize">
                  {dealer.plan}
                </Badge>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}

// =============================================================================
// MAIN PAGE
// =============================================================================

export default function DealerDirectoryPage() {
  const [search, setSearch] = React.useState('');
  const [province, setProvince] = React.useState('all');
  const [page, setPage] = React.useState(1);

  const { data, isLoading, error } = useDealers({
    page,
    pageSize: 12,
    status: 'active',
    searchTerm: search || undefined,
    province: province !== 'all' ? province : undefined,
  });

  const dealers = data?.items ?? [];
  const pagination = data?.pagination;

  return (
    <div className="bg-muted/50 min-h-screen">
      {/* Header */}
      <section className="bg-card border-b">
        <div className="container mx-auto px-4 py-8">
          <div className="flex items-center gap-3">
            <div className="bg-primary/10 flex h-12 w-12 items-center justify-center rounded-xl">
              <Building2 className="text-primary h-6 w-6" />
            </div>
            <div>
              <h1 className="text-foreground text-2xl font-bold">Directorio de Dealers</h1>
              <p className="text-muted-foreground">
                Encuentra concesionarios verificados en República Dominicana
              </p>
            </div>
          </div>

          {/* Filters */}
          <div className="mt-6 flex flex-col gap-3 sm:flex-row">
            <div className="relative flex-1">
              <Search className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
              <Input
                placeholder="Buscar por nombre..."
                value={search}
                onChange={e => {
                  setSearch(e.target.value);
                  setPage(1);
                }}
                className="pl-10"
              />
            </div>
            <Select
              value={province}
              onValueChange={v => {
                setProvince(v);
                setPage(1);
              }}
            >
              <SelectTrigger className="w-full sm:w-56">
                <SelectValue placeholder="Todas las provincias" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas las provincias</SelectItem>
                {PROVINCES.map(p => (
                  <SelectItem key={p} value={p}>
                    {p}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </section>

      {/* Results */}
      <div className="container mx-auto px-4 py-8">
        {isLoading ? (
          <DirectorySkeleton />
        ) : error ? (
          <div className="py-16 text-center">
            <Building2 className="text-muted-foreground mx-auto mb-4 h-12 w-12" />
            <h2 className="text-foreground mb-2 text-lg font-semibold">
              Error al cargar el directorio
            </h2>
            <p className="text-muted-foreground">Intenta de nuevo más tarde.</p>
          </div>
        ) : dealers.length === 0 ? (
          <div className="py-16 text-center">
            <Building2 className="text-muted-foreground mx-auto mb-4 h-12 w-12" />
            <h2 className="text-foreground mb-2 text-lg font-semibold">
              No se encontraron dealers
            </h2>
            <p className="text-muted-foreground">
              {search || province !== 'all'
                ? 'Intenta con otros filtros de búsqueda.'
                : 'Aún no hay dealers registrados en la plataforma.'}
            </p>
          </div>
        ) : (
          <>
            {/* Count */}
            <p className="text-muted-foreground mb-4 text-sm">
              {pagination?.totalItems ?? dealers.length} dealers encontrados
            </p>

            {/* Grid */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {dealers.map(dealer => (
                <DealerCard key={dealer.id} dealer={dealer} />
              ))}
            </div>

            {/* Pagination */}
            {pagination && pagination.totalPages > 1 && (
              <div className="mt-8 flex items-center justify-center gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!pagination.hasPreviousPage}
                  onClick={() => setPage(p => p - 1)}
                >
                  <ChevronLeft className="mr-1 h-4 w-4" />
                  Anterior
                </Button>
                <span className="text-muted-foreground text-sm">
                  Página {page} de {pagination.totalPages}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!pagination.hasNextPage}
                  onClick={() => setPage(p => p + 1)}
                >
                  Siguiente
                  <ChevronRight className="ml-1 h-4 w-4" />
                </Button>
              </div>
            )}
          </>
        )}

        {/* CTA for dealers */}
        <div className="mt-12 rounded-xl bg-gradient-to-r from-slate-900 to-slate-800 p-8 text-center text-white">
          <h2 className="mb-2 text-xl font-bold">¿Eres un concesionario?</h2>
          <p className="mb-4 text-slate-300">
            Únete a OKLA y llega a miles de compradores en República Dominicana.
          </p>
          <Button asChild className="bg-primary hover:bg-primary/90">
            <Link href="/dealers/registro">
              Registra tu dealer
              <Phone className="ml-2 h-4 w-4" />
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
