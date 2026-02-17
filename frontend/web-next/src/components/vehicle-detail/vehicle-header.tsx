/**
 * Vehicle Header Component
 * Displays vehicle title, price, badges, and actions
 */

'use client';

import * as React from 'react';
import { Heart, Share2, MapPin, Calendar, Gauge, Check, Shield, Clock } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DealRatingBadge } from '@/components/ui/deal-rating-badge';
import { cn, formatCurrency, formatNumber } from '@/lib/utils';
import type { Vehicle } from '@/types';

interface VehicleHeaderProps {
  vehicle: Vehicle;
  className?: string;
}

export function VehicleHeader({ vehicle, className }: VehicleHeaderProps) {
  const [isFavorite, setIsFavorite] = React.useState(false);

  const title = `${vehicle.year} ${vehicle.make} ${vehicle.model}`;

  const handleShare = async () => {
    const url = typeof window !== 'undefined' ? window.location.href : '';
    const text = `${title} - ${formatCurrency(vehicle.price)}`;

    if (typeof navigator !== 'undefined' && navigator.share) {
      try {
        await navigator.share({ title, text, url });
      } catch {
        // User cancelled or error - silently ignore
      }
    } else if (typeof navigator !== 'undefined') {
      await navigator.clipboard.writeText(url);
      // TODO: Show toast notification
    }
  };

  const handleFavorite = () => {
    setIsFavorite(!isFavorite);
    // TODO: Call API to toggle favorite
  };

  return (
    <div className={cn('rounded-xl bg-white p-6 shadow-sm', className)}>
      {/* Badges */}
      <div className="mb-3 flex flex-wrap gap-2">
        {vehicle.condition === 'new' && (
          <Badge className="bg-blue-100 text-blue-700 hover:bg-blue-100">Nuevo</Badge>
        )}
        {vehicle.condition === 'used' && (
          <Badge className="bg-muted text-foreground hover:bg-muted">Usado</Badge>
        )}
        {vehicle.condition === 'certified' && (
          <Badge className="gap-1 bg-purple-100 text-purple-700 hover:bg-purple-100">
            <Shield className="h-3 w-3" />
            Certificado
          </Badge>
        )}
        {vehicle.isFeatured && (
          <Badge className="bg-amber-100 text-amber-700 hover:bg-amber-100">Destacado</Badge>
        )}
      </div>

      {/* Title */}
      <h1 className="text-foreground text-2xl font-bold lg:text-3xl">{title}</h1>

      {/* Trim */}
      {vehicle.trim && <p className="text-muted-foreground mt-1 text-lg">{vehicle.trim}</p>}

      {/* Quick specs */}
      <div className="text-muted-foreground mt-4 flex flex-wrap items-center gap-4 text-sm">
        <div className="flex items-center gap-1.5">
          <Calendar className="text-muted-foreground h-4 w-4" />
          <span>{vehicle.year}</span>
        </div>
        {vehicle.mileage !== undefined && (
          <div className="flex items-center gap-1.5">
            <Gauge className="text-muted-foreground h-4 w-4" />
            <span>{formatNumber(vehicle.mileage)} km</span>
          </div>
        )}
        <div className="flex items-center gap-1.5">
          <MapPin className="text-muted-foreground h-4 w-4" />
          <span>{vehicle.location.city}</span>
        </div>
      </div>

      {/* Price Section */}
      <div className="border-border mt-6 border-t pt-6" data-testid="vehicle-price">
        {/* Deal Rating */}
        {vehicle.dealRating && (
          <div className="mb-3">
            <DealRatingBadge rating={vehicle.dealRating} size="lg" />
          </div>
        )}

        {/* Price */}
        <div className="flex items-baseline gap-3">
          <span className="text-foreground text-3xl font-bold lg:text-4xl">
            {formatCurrency(vehicle.price)}
          </span>
          {vehicle.originalPrice && vehicle.originalPrice > vehicle.price && (
            <span className="text-muted-foreground text-lg line-through">
              {formatCurrency(vehicle.originalPrice)}
            </span>
          )}
        </div>

        {/* Monthly Payment Estimate */}
        <p className="text-muted-foreground mt-2 text-sm">
          Estimado:{' '}
          <span className="font-medium">{formatCurrency(Math.round(vehicle.price / 60))}/mes</span>
          <span className="ml-1 text-xs">(60 meses)</span>
        </p>
      </div>

      {/* Actions */}
      <div className="mt-6 flex gap-3">
        <Button
          variant={isFavorite ? 'default' : 'outline'}
          onClick={handleFavorite}
          className={cn(
            'flex-1 gap-2',
            isFavorite && 'border-rose-500 bg-rose-500 hover:bg-rose-600'
          )}
        >
          <Heart className={cn('h-5 w-5', isFavorite && 'fill-current')} />
          {isFavorite ? 'Guardado' : 'Guardar'}
        </Button>

        <Button variant="outline" onClick={handleShare} className="gap-2">
          <Share2 className="h-5 w-5" />
          Compartir
        </Button>
      </div>

      {/* Published Date */}
      <div className="text-muted-foreground mt-4 flex items-center justify-center gap-1.5 text-xs">
        <Clock className="h-3 w-3" />
        <span>Publicado el {new Date(vehicle.createdAt).toLocaleDateString('es-DO')}</span>
      </div>
    </div>
  );
}

export default VehicleHeader;
