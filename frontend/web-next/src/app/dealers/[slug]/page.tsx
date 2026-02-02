/**
 * Dealer Profile Page
 *
 * Features:
 * - Dealer hero with logo, name, badges
 * - Contact info and location
 * - Inventory listing
 * - Reviews section
 * - About section
 *
 * Route: /dealers/[slug]
 */

'use client';

import * as React from 'react';
import { useParams, notFound } from 'next/navigation';
import Image from 'next/image';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { VehicleCard } from '@/components/ui/vehicle-card';
import { Skeleton } from '@/components/ui/skeleton';
import {
  MapPin,
  Phone,
  Mail,
  Globe,
  Clock,
  Star,
  Shield,
  CheckCircle,
  MessageSquare,
  Calendar,
  Car,
  ExternalLink,
  Share2,
  AlertCircle,
  Loader2,
} from 'lucide-react';
import { useDealerBySlug, useDealerLocations } from '@/hooks/use-dealers';
import { useReviewsForTarget, useReviewStats } from '@/hooks/use-reviews';
import { useVehiclesByDealer } from '@/hooks/use-vehicles';
import type { Review as ReviewType } from '@/services/reviews';
import type { VehicleCardData } from '@/types';

// =============================================================================
// LOADING SKELETON
// =============================================================================

function DealerProfileSkeleton() {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Cover Image Skeleton */}
      <Skeleton className="h-48 w-full md:h-64 lg:h-80" />

      {/* Header Skeleton */}
      <div className="relative z-10 container mx-auto -mt-16 px-4">
        <div className="rounded-xl bg-white p-6 shadow-lg">
          <div className="flex flex-col gap-6 md:flex-row">
            <Skeleton className="-mt-16 h-24 w-24 rounded-xl md:-mt-20 md:h-32 md:w-32" />
            <div className="flex-1 space-y-3">
              <Skeleton className="h-8 w-64" />
              <Skeleton className="h-4 w-96" />
              <div className="flex gap-4">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-24" />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Content Skeleton */}
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="space-y-4 lg:col-span-2">
            <Skeleton className="h-10 w-64" />
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              {[1, 2, 3, 4].map(i => (
                <Skeleton key={i} className="h-64 w-full" />
              ))}
            </div>
          </div>
          <div className="space-y-6">
            <Skeleton className="h-48 w-full" />
            <Skeleton className="h-32 w-full" />
          </div>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// ERROR STATE
// =============================================================================

function DealerNotFound() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <Card className="mx-4 max-w-md">
        <CardContent className="p-8 text-center">
          <AlertCircle className="mx-auto mb-4 h-16 w-16 text-gray-400" />
          <h1 className="mb-2 text-2xl font-bold text-gray-900">Dealer no encontrado</h1>
          <p className="mb-6 text-gray-600">
            El concesionario que buscas no existe o ya no está disponible.
          </p>
          <Button asChild className="bg-emerald-600 hover:bg-emerald-700">
            <Link href="/dealers">Ver todos los dealers</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}

// =============================================================================
// REVIEW ITEM COMPONENT
// =============================================================================

interface ReviewItemProps {
  review: ReviewType;
}

function ReviewItem({ review }: ReviewItemProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="mb-3 flex items-start justify-between">
          <div>
            <div className="font-semibold">{review.reviewerName}</div>
            <div className="text-sm text-gray-500">
              {new Date(review.createdAt).toLocaleDateString('es-DO', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
              })}
            </div>
          </div>
          <div className="flex items-center gap-0.5">
            {Array.from({ length: 5 }).map((_, i) => (
              <Star
                key={i}
                className={`h-4 w-4 ${
                  i < review.overallRating ? 'fill-amber-500 text-amber-500' : 'text-gray-300'
                }`}
              />
            ))}
          </div>
        </div>
        <p className="text-gray-700">{review.content}</p>
        {review.isVerifiedPurchase && (
          <div className="mt-3 text-sm text-emerald-600">
            <CheckCircle className="mr-1 inline h-4 w-4" />
            Compra verificada
          </div>
        )}
      </CardContent>
    </Card>
  );
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

import type { DayHours } from '@/services/dealers';
import type { Dealer } from '@/types';

function formatHours(hours?: DayHours): string {
  if (!hours || hours.isClosed) return 'Cerrado';
  return `${hours.open} - ${hours.close}`;
}

function getDealerBadges(dealer: Dealer): string[] {
  const badges: string[] = [];

  if (dealer.verificationStatus === 'verified') {
    badges.push('Verificado');
  }

  if (dealer.plan === 'pro') {
    badges.push('Pro');
  } else if (dealer.plan === 'enterprise') {
    badges.push('Enterprise');
  }

  if (dealer.responseRate && dealer.responseRate >= 90) {
    badges.push('Respuesta Rápida');
  }

  if (dealer.reviewCount && dealer.reviewCount >= 50 && dealer.rating && dealer.rating >= 4.5) {
    badges.push('Top Seller');
  }

  return badges;
}

// =============================================================================
// PAGE COMPONENT
// =============================================================================

interface PageProps {
  params: Promise<{ slug: string }>;
}

export default function DealerProfilePage({ params }: PageProps) {
  const { slug } = React.use(params);

  // Fetch dealer data
  const { data: dealerData, isLoading: dealerLoading, error: dealerError } = useDealerBySlug(slug);
  const { data: locations } = useDealerLocations(dealerData?.id || '');
  const { data: reviewStats } = useReviewStats('dealer', dealerData?.id || '');
  const { data: reviewsData } = useReviewsForTarget('dealer', dealerData?.id || '');
  const { data: vehiclesData, isLoading: vehiclesLoading } = useVehiclesByDealer(
    dealerData?.id || ''
  );

  // Loading state
  if (dealerLoading) {
    return <DealerProfileSkeleton />;
  }

  // Error state
  if (dealerError || !dealerData) {
    return <DealerNotFound />;
  }

  // Transform dealer data to expected format
  const primaryLocation = locations?.find(l => l.isPrimary) || locations?.[0];
  const dealer = {
    id: dealerData.id,
    slug: slug,
    name: dealerData.businessName,
    logo: dealerData.logoUrl || '/placeholder-dealer.png',
    coverImage: dealerData.bannerUrl || '/placeholder-dealer-banner.jpg',
    description:
      dealerData.description ||
      `${dealerData.businessName} es un concesionario en ${dealerData.city}, ${dealerData.province}.`,
    shortDescription:
      dealerData.description?.slice(0, 100) || `Concesionario en ${dealerData.city}`,
    isVerified: dealerData.verificationStatus === 'verified',
    isPremium: dealerData.plan === 'pro' || dealerData.plan === 'enterprise',
    memberSince: dealerData.createdAt
      ? new Date(dealerData.createdAt).getFullYear().toString()
      : 'N/A',
    rating: dealerData.rating || reviewStats?.averageRating || 0,
    totalReviews: dealerData.reviewCount || reviewStats?.totalReviews || 0,
    totalVehicles: dealerData.currentActiveListings || 0,
    totalSold: 0, // Will be added when we have sales data
    responseTime: dealerData.avgResponseTimeMinutes
      ? dealerData.avgResponseTimeMinutes < 60
        ? `< ${dealerData.avgResponseTimeMinutes} min`
        : `< ${Math.round(dealerData.avgResponseTimeMinutes / 60)} hora${Math.round(dealerData.avgResponseTimeMinutes / 60) > 1 ? 's' : ''}`
      : 'N/A',
    location: {
      address: primaryLocation?.address || dealerData.address,
      city: primaryLocation?.city || dealerData.city,
      province: primaryLocation?.province || dealerData.province,
      coordinates:
        primaryLocation?.latitude && primaryLocation?.longitude
          ? { lat: primaryLocation.latitude, lng: primaryLocation.longitude }
          : undefined,
    },
    contact: {
      phone: dealerData.phone,
      whatsapp: dealerData.mobilePhone,
      email: dealerData.email,
      website: dealerData.website,
    },
    hours: primaryLocation?.businessHours
      ? {
          weekdays: formatHours(primaryLocation.businessHours.monday),
          saturday: formatHours(primaryLocation.businessHours.saturday),
          sunday: formatHours(primaryLocation.businessHours.sunday),
        }
      : {
          weekdays: '8:00 AM - 6:00 PM',
          saturday: '9:00 AM - 5:00 PM',
          sunday: 'Cerrado',
        },
    badges: getDealerBadges(dealerData),
    specialties: [], // Will be fetched from dealer specialties when available
  };

  // Get data from hooks
  const reviews = reviewsData?.items || [];
  const vehicles = vehiclesData?.items || [];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Cover Image */}
      <div className="relative h-48 bg-gray-300 md:h-64 lg:h-80">
        <Image
          src={dealer.coverImage}
          alt={`${dealer.name} cover`}
          fill
          className="object-cover"
          priority
        />
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
      </div>

      {/* Dealer Header */}
      <div className="relative z-10 container mx-auto -mt-16 px-4">
        <div className="rounded-xl bg-white p-6 shadow-lg">
          <div className="flex flex-col gap-6 md:flex-row">
            {/* Logo */}
            <div className="relative -mt-16 h-24 w-24 overflow-hidden rounded-xl border-4 border-white bg-white shadow-lg md:-mt-20 md:h-32 md:w-32">
              <Image src={dealer.logo} alt={dealer.name} fill className="object-cover" />
            </div>

            {/* Info */}
            <div className="flex-1">
              <div className="mb-2 flex flex-wrap items-center gap-2">
                <h1 className="text-2xl font-bold md:text-3xl">{dealer.name}</h1>
                {dealer.isVerified && (
                  <Badge className="gap-1 bg-emerald-100 text-emerald-700">
                    <CheckCircle className="h-3 w-3" />
                    Verificado
                  </Badge>
                )}
                {dealer.isPremium && <Badge className="bg-amber-100 text-amber-700">Premium</Badge>}
              </div>

              <p className="mb-4 text-gray-600">{dealer.shortDescription}</p>

              {/* Quick Stats */}
              <div className="flex flex-wrap gap-4 text-sm">
                <div className="flex items-center gap-1.5">
                  <Star className="h-4 w-4 fill-amber-500 text-amber-500" />
                  <span className="font-semibold">{dealer.rating}</span>
                  <span className="text-gray-500">({dealer.totalReviews} reseñas)</span>
                </div>
                <div className="flex items-center gap-1.5 text-gray-600">
                  <Car className="h-4 w-4" />
                  {dealer.totalVehicles} vehículos
                </div>
                <div className="flex items-center gap-1.5 text-gray-600">
                  <MapPin className="h-4 w-4" />
                  {dealer.location.city}
                </div>
                <div className="flex items-center gap-1.5 text-gray-600">
                  <Clock className="h-4 w-4" />
                  Responde {dealer.responseTime}
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex flex-col gap-2 md:min-w-[160px]">
              <Button className="bg-emerald-600 hover:bg-emerald-700">
                <MessageSquare className="mr-2 h-4 w-4" />
                Contactar
              </Button>
              <Button variant="outline">
                <Phone className="mr-2 h-4 w-4" />
                Llamar
              </Button>
              <Button variant="ghost" size="sm">
                <Share2 className="mr-2 h-4 w-4" />
                Compartir
              </Button>
            </div>
          </div>

          {/* Badges */}
          {dealer.badges.length > 0 && (
            <div className="mt-4 flex flex-wrap gap-2 border-t pt-4">
              {dealer.badges.map(badge => (
                <Badge key={badge} variant="outline" className="gap-1">
                  <Shield className="h-3 w-3" />
                  {badge}
                </Badge>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Main Content */}
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          {/* Left Column - Main Content */}
          <div className="lg:col-span-2">
            <Tabs defaultValue="inventory" className="w-full">
              <TabsList className="w-full justify-start">
                <TabsTrigger value="inventory">Inventario ({dealer.totalVehicles})</TabsTrigger>
                <TabsTrigger value="reviews">Reseñas ({dealer.totalReviews})</TabsTrigger>
                <TabsTrigger value="about">Acerca de</TabsTrigger>
              </TabsList>

              {/* Inventory Tab */}
              <TabsContent value="inventory" className="mt-6">
                {vehiclesLoading ? (
                  <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                    {[1, 2, 3, 4].map(i => (
                      <Skeleton key={i} className="h-64 w-full" />
                    ))}
                  </div>
                ) : vehicles.length === 0 ? (
                  <Card>
                    <CardContent className="py-12 text-center">
                      <Car className="mx-auto mb-4 h-12 w-12 text-gray-400" />
                      <p className="text-gray-600">
                        Este dealer no tiene vehículos disponibles actualmente.
                      </p>
                    </CardContent>
                  </Card>
                ) : (
                  <>
                    <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                      {vehicles.slice(0, 4).map(vehicle => (
                        <VehicleCard key={vehicle.id} vehicle={vehicle} />
                      ))}
                    </div>

                    {vehicles.length > 4 && (
                      <div className="mt-6 text-center">
                        <Button variant="outline" size="lg" asChild>
                          <Link href={`/buscar?dealer=${dealer.id}`}>
                            Ver todos los vehículos ({vehicles.length})
                          </Link>
                        </Button>
                      </div>
                    )}
                  </>
                )}
              </TabsContent>

              {/* Reviews Tab */}
              <TabsContent value="reviews" className="mt-6">
                <div className="space-y-4">
                  {/* Rating Summary */}
                  <Card>
                    <CardContent className="p-6">
                      <div className="flex items-center gap-6">
                        <div className="text-center">
                          <div className="text-4xl font-bold text-emerald-600">{dealer.rating}</div>
                          <div className="mt-1 flex items-center gap-0.5">
                            {Array.from({ length: 5 }).map((_, i) => (
                              <Star
                                key={i}
                                className={`h-4 w-4 ${
                                  i < Math.floor(dealer.rating)
                                    ? 'fill-amber-500 text-amber-500'
                                    : 'text-gray-300'
                                }`}
                              />
                            ))}
                          </div>
                          <div className="mt-1 text-sm text-gray-500">
                            {dealer.totalReviews} reseñas
                          </div>
                        </div>

                        <div className="flex-1 space-y-1">
                          {[5, 4, 3, 2, 1].map(stars => (
                            <div key={stars} className="flex items-center gap-2">
                              <span className="w-3 text-sm">{stars}</span>
                              <Star className="h-3 w-3 fill-amber-500 text-amber-500" />
                              <div className="h-2 flex-1 overflow-hidden rounded-full bg-gray-200">
                                <div
                                  className="h-full rounded-full bg-amber-500"
                                  style={{
                                    width: `${
                                      stars === 5
                                        ? 75
                                        : stars === 4
                                          ? 20
                                          : stars === 3
                                            ? 3
                                            : stars === 2
                                              ? 1
                                              : 1
                                    }%`,
                                  }}
                                />
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    </CardContent>
                  </Card>

                  {/* Reviews List */}
                  {reviews.length === 0 ? (
                    <Card>
                      <CardContent className="py-8 text-center">
                        <MessageSquare className="mx-auto mb-3 h-10 w-10 text-gray-400" />
                        <p className="text-gray-600">Aún no hay reseñas para este dealer.</p>
                        <p className="mt-1 text-sm text-gray-500">
                          ¡Sé el primero en dejar una reseña!
                        </p>
                      </CardContent>
                    </Card>
                  ) : (
                    <>
                      {reviews.slice(0, 5).map(review => (
                        <ReviewItem key={review.id} review={review} />
                      ))}

                      {reviews.length > 5 && (
                        <div className="text-center">
                          <Button variant="outline">
                            Ver todas las reseñas ({reviews.length})
                          </Button>
                        </div>
                      )}
                    </>
                  )}
                </div>
              </TabsContent>

              {/* About Tab */}
              <TabsContent value="about" className="mt-6">
                <Card>
                  <CardHeader>
                    <CardTitle>Acerca de {dealer.name}</CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <p className="whitespace-pre-line text-gray-700">{dealer.description}</p>

                    {dealer.specialties.length > 0 && (
                      <div>
                        <h4 className="mb-2 font-semibold">Especialidades</h4>
                        <div className="flex flex-wrap gap-2">
                          {dealer.specialties.map(specialty => (
                            <Badge key={specialty} variant="secondary">
                              {specialty}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    )}

                    <div className="grid grid-cols-2 gap-4 border-t pt-4">
                      <div>
                        <span className="text-gray-500">Miembro desde</span>
                        <p className="font-semibold">{dealer.memberSince}</p>
                      </div>
                      <div>
                        <span className="text-gray-500">Vehículos vendidos</span>
                        <p className="font-semibold">{dealer.totalSold}+</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </TabsContent>
            </Tabs>
          </div>

          {/* Right Column - Contact Info */}
          <div className="space-y-6">
            {/* Contact Card */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Información de Contacto</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-start gap-3">
                  <MapPin className="mt-0.5 h-5 w-5 text-gray-400" />
                  <div>
                    <p className="font-medium">{dealer.location.address}</p>
                    <p className="text-gray-600">
                      {dealer.location.city}, {dealer.location.province}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <Phone className="h-5 w-5 text-gray-400" />
                  <a href={`tel:${dealer.contact.phone}`} className="hover:text-emerald-600">
                    {dealer.contact.phone}
                  </a>
                </div>

                <div className="flex items-center gap-3">
                  <Mail className="h-5 w-5 text-gray-400" />
                  <a href={`mailto:${dealer.contact.email}`} className="hover:text-emerald-600">
                    {dealer.contact.email}
                  </a>
                </div>

                {dealer.contact.website && (
                  <div className="flex items-center gap-3">
                    <Globe className="h-5 w-5 text-gray-400" />
                    <a
                      href={`https://${dealer.contact.website}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex items-center gap-1 hover:text-emerald-600"
                    >
                      {dealer.contact.website}
                      <ExternalLink className="h-3 w-3" />
                    </a>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Hours Card */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Horario de Atención</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-600">Lunes - Viernes</span>
                  <span className="font-medium">{dealer.hours.weekdays}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Sábado</span>
                  <span className="font-medium">{dealer.hours.saturday}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Domingo</span>
                  <span className="font-medium">{dealer.hours.sunday}</span>
                </div>
              </CardContent>
            </Card>

            {/* Schedule Visit CTA */}
            <Card className="border-emerald-200 bg-emerald-50">
              <CardContent className="p-6 text-center">
                <Calendar className="mx-auto mb-3 h-10 w-10 text-emerald-600" />
                <h3 className="mb-2 font-semibold">¿Te interesa un vehículo?</h3>
                <p className="mb-4 text-sm text-gray-600">
                  Agenda una visita para ver el vehículo en persona
                </p>
                <Button className="w-full bg-emerald-600 hover:bg-emerald-700">
                  Agendar Visita
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}
