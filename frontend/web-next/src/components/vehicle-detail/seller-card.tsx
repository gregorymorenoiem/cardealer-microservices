/**
 * Seller Card Component
 * Displays seller information and contact buttons
 */

'use client';

import * as React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Phone, MessageCircle, Mail, MapPin, Star, Check, Clock, Shield, User } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import type { Vehicle } from '@/types';

interface SellerCardProps {
  vehicle: Vehicle;
  className?: string;
}

// Extended vehicle type with seller info (to be enhanced)
interface VehicleWithSeller extends Vehicle {
  seller?: {
    id: string;
    name: string;
    type: 'individual' | 'dealer';
    avatar?: string;
    phone?: string;
    email?: string;
    city?: string;
    rating?: number;
    reviewCount?: number;
    responseRate?: number;
    responseTime?: string;
    isVerified?: boolean;
    memberSince?: string;
    listingsCount?: number;
  };
}

export function SellerCard({ vehicle, className }: SellerCardProps) {
  const [showPhone, setShowPhone] = React.useState(false);
  const vehicleWithSeller = vehicle as VehicleWithSeller;
  const seller = vehicleWithSeller.seller;
  const isDealer = vehicle.sellerType === 'dealer';

  // Mock seller data if not provided
  const sellerData = seller || {
    id: vehicle.sellerId,
    name: isDealer ? 'Dealer Verificado' : 'Vendedor Particular',
    type: vehicle.sellerType,
    phone: '809-555-0123',
    city: vehicle.location.city,
    rating: 4.5,
    reviewCount: 24,
    isVerified: true,
  };

  const handleCall = () => {
    if (sellerData.phone) {
      window.location.href = `tel:${sellerData.phone}`;
    }
  };

  const handleWhatsApp = () => {
    const message = encodeURIComponent(
      `Hola, me interesa el ${vehicle.year} ${vehicle.make} ${vehicle.model} que vi en OKLA.`
    );
    const phone = sellerData.phone?.replace(/\D/g, '');
    window.open(`https://wa.me/1${phone}?text=${message}`, '_blank');
  };

  const handleShowPhone = () => {
    setShowPhone(true);
    // TODO: Track phone reveal event
  };

  return (
    <div className={cn('rounded-xl bg-white p-6 shadow-sm', className)} data-testid="seller-info">
      {/* Seller header */}
      <div className="flex items-start gap-4">
        {/* Avatar */}
        <div className="relative flex h-14 w-14 flex-shrink-0 items-center justify-center overflow-hidden rounded-full bg-gray-100">
          {sellerData.avatar ? (
            <Image
              src={sellerData.avatar}
              alt={sellerData.name}
              fill
              sizes="56px"
              className="object-cover"
            />
          ) : (
            <User className="h-6 w-6 text-gray-400" />
          )}
          {sellerData.isVerified && (
            <div className="bg-primary absolute -right-0.5 -bottom-0.5 flex h-5 w-5 items-center justify-center rounded-full border-2 border-white">
              <Check className="h-3 w-3 text-white" />
            </div>
          )}
        </div>

        {/* Seller info */}
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <h3 className="truncate font-semibold text-gray-900">{sellerData.name}</h3>
            {isDealer && (
              <Badge variant="secondary" className="text-xs">
                Dealer
              </Badge>
            )}
          </div>

          {/* Rating */}
          {sellerData.rating && (
            <div className="mt-1 flex items-center gap-1.5">
              <Star className="h-4 w-4 fill-amber-400 text-amber-400" />
              <span className="text-sm font-medium">{sellerData.rating.toFixed(1)}</span>
              {sellerData.reviewCount && (
                <span className="text-sm text-gray-500">({sellerData.reviewCount} reseñas)</span>
              )}
            </div>
          )}

          {/* Location */}
          <div className="mt-1 flex items-center gap-1.5 text-sm text-gray-500">
            <MapPin className="h-3.5 w-3.5" />
            <span>{sellerData.city || vehicle.location.city}</span>
          </div>
        </div>
      </div>

      {/* Stats */}
      {isDealer && (
        <div className="mt-4 grid grid-cols-2 gap-4 border-t pt-4">
          <div className="text-center">
            <p className="text-lg font-semibold text-gray-900">{sellerData.responseRate || 95}%</p>
            <p className="text-xs text-gray-500">Tasa de respuesta</p>
          </div>
          <div className="text-center">
            <p className="text-lg font-semibold text-gray-900">
              {sellerData.responseTime || '< 1h'}
            </p>
            <p className="text-xs text-gray-500">Tiempo de respuesta</p>
          </div>
        </div>
      )}

      {/* Contact Buttons */}
      <div className="mt-6 space-y-3">
        {/* WhatsApp - Primary */}
        <Button
          onClick={handleWhatsApp}
          className="w-full gap-2 bg-[#25D366] text-white hover:bg-[#20BD5A]"
        >
          <MessageCircle className="h-5 w-5" />
          WhatsApp
        </Button>

        {/* Phone */}
        {showPhone ? (
          <Button onClick={handleCall} variant="outline" className="w-full gap-2">
            <Phone className="h-5 w-5" />
            {sellerData.phone}
          </Button>
        ) : (
          <Button onClick={handleShowPhone} variant="outline" className="w-full gap-2">
            <Phone className="h-5 w-5" />
            Ver teléfono
          </Button>
        )}

        {/* Message */}
        <Button variant="outline" className="w-full gap-2" asChild>
          <Link href={`/mensajes/nuevo?vehicleId=${vehicle.id}`}>
            <Mail className="h-5 w-5" />
            Enviar mensaje
          </Link>
        </Button>
      </div>

      {/* Trust signals */}
      <div className="mt-6 border-t pt-4">
        <div className="flex items-center gap-2 text-sm text-gray-600">
          <Shield className="text-primary h-4 w-4" />
          <span>Contacto verificado por OKLA</span>
        </div>
        <p className="mt-2 text-xs text-gray-400">
          Nunca envíes dinero por adelantado. Conoce al vendedor en un lugar público.
        </p>
      </div>

      {/* View Seller Profile */}
      {isDealer && (
        <div className="mt-4">
          <Link
            href={`/dealers/${sellerData.id}`}
            className="text-primary flex items-center justify-center gap-1 text-sm hover:underline"
          >
            Ver perfil completo
            <span aria-hidden="true">→</span>
          </Link>
        </div>
      )}
    </div>
  );
}

export default SellerCard;
