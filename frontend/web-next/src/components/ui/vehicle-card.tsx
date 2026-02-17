'use client';

import * as React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Heart, MapPin, Gauge, Calendar, Fuel, Zap } from 'lucide-react';
import { cn, formatCurrency, formatMileage } from '@/lib/utils';
import { Badge } from './badge';
import { DealRatingBadge, type DealRating } from './deal-rating-badge';
import { Skeleton } from './skeleton';
import type { VehicleCardData } from '@/types';

export interface VehicleCardProps {
  vehicle: VehicleCardData;
  variant?: 'default' | 'compact' | 'horizontal';
  showDealRating?: boolean;
  showFavoriteButton?: boolean;
  isFavorite?: boolean;
  onFavoriteClick?: (vehicleId: string) => void;
  priority?: boolean;
  className?: string;
}

export function VehicleCard({
  vehicle,
  variant = 'default',
  showDealRating = true,
  showFavoriteButton = true,
  isFavorite = false,
  onFavoriteClick,
  priority = false,
  className,
}: VehicleCardProps) {
  const [imageError, setImageError] = React.useState(false);
  const [isHovered, setIsHovered] = React.useState(false);

  const handleFavoriteClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onFavoriteClick?.(vehicle.id);
  };

  const vehicleUrl = `/vehiculos/${vehicle.slug}`;

  const fuelIcons: Record<string, React.ReactNode> = {
    Gasolina: <Fuel className="h-3.5 w-3.5" />,
    Diesel: <Fuel className="h-3.5 w-3.5" />,
    Eléctrico: <Zap className="h-3.5 w-3.5" />,
    Híbrido: <Zap className="h-3.5 w-3.5" />,
  };

  if (variant === 'horizontal') {
    return (
      <Link
        href={vehicleUrl}
        className={cn(
          'group border-border bg-card flex gap-4 rounded-xl border p-4 shadow-sm transition-all hover:shadow-md',
          className
        )}
      >
        {/* Image */}
        <div className="bg-muted relative h-32 w-48 shrink-0 overflow-hidden rounded-lg">
          {!imageError && vehicle.imageUrl ? (
            <Image
              src={vehicle.imageUrl}
              alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
              fill
              sizes="192px"
              className="object-cover transition-transform duration-300 group-hover:scale-105"
              priority={priority}
              onError={() => setImageError(true)}
            />
          ) : (
            <div className="flex h-full items-center justify-center">
              <span className="text-4xl">🚗</span>
            </div>
          )}
          {showDealRating && vehicle.dealRating && (
            <div className="absolute top-2 left-2">
              <DealRatingBadge rating={vehicle.dealRating as DealRating} size="sm" />
            </div>
          )}
        </div>

        {/* Content */}
        <div className="flex flex-1 flex-col justify-between">
          <div>
            <h3 className="text-card-foreground group-hover:text-primary font-semibold">
              {vehicle.year} {vehicle.make} {vehicle.model}
            </h3>
            {vehicle.trim && <p className="text-muted-foreground text-sm">{vehicle.trim}</p>}
          </div>

          <div className="text-muted-foreground flex flex-wrap gap-3 text-xs">
            <span className="flex items-center gap-1">
              <Gauge className="h-3.5 w-3.5" />
              {formatMileage(vehicle.mileage)}
            </span>
            <span className="flex items-center gap-1">
              <MapPin className="h-3.5 w-3.5" />
              {vehicle.location}
            </span>
          </div>

          <div className="flex items-center justify-between">
            <span className="text-primary text-lg font-bold">{formatCurrency(vehicle.price)}</span>
            {showFavoriteButton && (
              <button
                onClick={handleFavoriteClick}
                className={cn(
                  'rounded-full p-2 transition-colors',
                  isFavorite
                    ? 'text-red-500 hover:text-red-600'
                    : 'text-muted-foreground hover:text-foreground'
                )}
                aria-label={isFavorite ? 'Quitar de favoritos' : 'Agregar a favoritos'}
              >
                <Heart className={cn('h-5 w-5', isFavorite && 'fill-current')} />
              </button>
            )}
          </div>
        </div>
      </Link>
    );
  }

  if (variant === 'compact') {
    return (
      <Link
        href={vehicleUrl}
        className={cn(
          'group border-border bg-card block overflow-hidden rounded-lg border shadow-sm transition-all hover:shadow-md',
          className
        )}
      >
        {/* Image */}
        <div className="bg-muted relative aspect-[4/3] overflow-hidden">
          {!imageError && vehicle.imageUrl ? (
            <Image
              src={vehicle.imageUrl}
              alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
              fill
              sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
              className="object-cover transition-transform duration-300 group-hover:scale-105"
              priority={priority}
              onError={() => setImageError(true)}
            />
          ) : (
            <div className="flex h-full items-center justify-center">
              <span className="text-5xl">🚗</span>
            </div>
          )}
        </div>

        {/* Content */}
        <div className="p-3">
          <h3 className="text-card-foreground group-hover:text-primary truncate text-sm font-semibold">
            {vehicle.year} {vehicle.make} {vehicle.model}
          </h3>
          <p className="text-primary mt-1 text-sm font-bold">{formatCurrency(vehicle.price)}</p>
        </div>
      </Link>
    );
  }

  // Default variant
  return (
    <Link
      href={vehicleUrl}
      className={cn(
        'group border-border bg-card block overflow-hidden rounded-xl border shadow-sm transition-all hover:shadow-lg',
        className
      )}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Image Section */}
      <div className="bg-muted relative aspect-[16/10] overflow-hidden">
        {!imageError && vehicle.imageUrl ? (
          <Image
            src={vehicle.imageUrl}
            alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
            fill
            sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
            className="object-cover transition-transform duration-300 group-hover:scale-105"
            priority={priority}
            onError={() => setImageError(true)}
          />
        ) : (
          <div className="flex h-full items-center justify-center">
            <span className="text-6xl">🚗</span>
          </div>
        )}

        {/* Overlays */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent opacity-0 transition-opacity group-hover:opacity-100" />

        {/* Deal Rating Badge */}
        {showDealRating && vehicle.dealRating && (
          <div className="absolute top-3 left-3">
            <DealRatingBadge rating={vehicle.dealRating as DealRating} />
          </div>
        )}

        {/* Favorite Button */}
        {showFavoriteButton && (
          <button
            onClick={handleFavoriteClick}
            className={cn(
              'bg-background/90 absolute top-3 right-3 rounded-full p-2 shadow-md backdrop-blur-sm transition-all',
              isHovered ? 'opacity-100' : 'opacity-0 sm:opacity-100',
              isFavorite
                ? 'text-red-500 hover:bg-red-50 dark:hover:bg-red-950'
                : 'text-muted-foreground hover:bg-muted hover:text-red-500'
            )}
            aria-label={isFavorite ? 'Quitar de favoritos' : 'Agregar a favoritos'}
          >
            <Heart className={cn('h-5 w-5', isFavorite && 'fill-current')} />
          </button>
        )}

        {/* Photo Count */}
        {vehicle.photoCount && vehicle.photoCount > 1 && (
          <div className="absolute right-3 bottom-3 flex items-center gap-1 rounded-full bg-black/60 px-2 py-1 text-xs font-medium text-white">
            📷 {vehicle.photoCount}
          </div>
        )}

        {/* Badges Row */}
        <div className="absolute bottom-3 left-3 flex gap-1.5">
          {vehicle.isNew && (
            <Badge variant="primary" size="sm">
              Nuevo
            </Badge>
          )}
          {vehicle.isCertified && (
            <Badge variant="success" size="sm">
              Certificado
            </Badge>
          )}
        </div>
      </div>

      {/* Content Section */}
      <div className="p-4">
        {/* Title */}
        <h3 className="text-card-foreground group-hover:text-primary text-lg font-semibold transition-colors">
          {vehicle.year} {vehicle.make} {vehicle.model}
        </h3>
        {vehicle.trim && <p className="text-muted-foreground mt-0.5 text-sm">{vehicle.trim}</p>}

        {/* Specs Row */}
        <div className="text-muted-foreground mt-3 flex flex-wrap gap-x-4 gap-y-1 text-sm">
          <span className="flex items-center gap-1.5">
            <Gauge className="h-4 w-4" />
            {formatMileage(vehicle.mileage)}
          </span>
          <span className="flex items-center gap-1.5">
            <Calendar className="h-4 w-4" />
            {vehicle.year}
          </span>
          {vehicle.fuelType && (
            <span className="flex items-center gap-1.5">
              {fuelIcons[vehicle.fuelType] || <Fuel className="h-4 w-4" />}
              {vehicle.fuelType}
            </span>
          )}
        </div>

        {/* Location */}
        <div className="text-muted-foreground mt-2 flex items-center gap-1.5 text-sm">
          <MapPin className="h-4 w-4" />
          <span>{vehicle.location}</span>
        </div>

        {/* Price Section */}
        <div className="border-border mt-4 flex items-end justify-between border-t pt-4">
          <div>
            <span className="text-card-foreground text-2xl font-bold">
              {formatCurrency(vehicle.price)}
            </span>
            {vehicle.monthlyPayment && (
              <p className="text-muted-foreground mt-0.5 text-sm">
                Est. {formatCurrency(vehicle.monthlyPayment)}/mes
              </p>
            )}
          </div>
          {vehicle.dealerName && (
            <div className="text-right text-sm">
              <p className="text-foreground font-medium">{vehicle.dealerName}</p>
              {vehicle.dealerRating && (
                <p className="text-muted-foreground">⭐ {vehicle.dealerRating.toFixed(1)}</p>
              )}
            </div>
          )}
        </div>
      </div>
    </Link>
  );
}

// Loading skeleton
export function VehicleCardSkeleton({
  variant = 'default',
}: {
  variant?: VehicleCardProps['variant'];
}) {
  if (variant === 'horizontal') {
    return (
      <div className="border-border bg-card flex gap-4 rounded-xl border p-4">
        <Skeleton className="h-32 w-48 shrink-0 rounded-lg" />
        <div className="flex flex-1 flex-col justify-between">
          <div>
            <Skeleton className="h-5 w-48" />
            <Skeleton className="mt-1 h-4 w-24" />
          </div>
          <Skeleton className="h-4 w-32" />
          <Skeleton className="h-6 w-28" />
        </div>
      </div>
    );
  }

  if (variant === 'compact') {
    return (
      <div className="border-border bg-card overflow-hidden rounded-lg border">
        <Skeleton className="aspect-[4/3] w-full" />
        <div className="p-3">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="mt-2 h-5 w-24" />
        </div>
      </div>
    );
  }

  return (
    <div className="border-border bg-card overflow-hidden rounded-xl border">
      <Skeleton className="aspect-[16/10] w-full" />
      <div className="p-4">
        <Skeleton className="h-6 w-3/4" />
        <Skeleton className="mt-1 h-4 w-1/2" />
        <div className="mt-3 flex gap-4">
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-4 w-16" />
          <Skeleton className="h-4 w-20" />
        </div>
        <Skeleton className="mt-2 h-4 w-32" />
        <div className="border-border mt-4 border-t pt-4">
          <Skeleton className="h-8 w-28" />
          <Skeleton className="mt-1 h-4 w-20" />
        </div>
      </div>
    </div>
  );
}
