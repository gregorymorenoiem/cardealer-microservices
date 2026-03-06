/**
 * Publicar Preview Page
 *
 * Preview listing before publishing
 */

'use client';

import { useState } from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Car,
  MapPin,
  Fuel,
  Gauge,
  Settings,
  Palette,
  Check,
  ChevronLeft,
  ChevronRight,
  Share2,
  Heart,
  Phone,
  MessageSquare,
  AlertCircle,
  Edit,
  Sparkles,
  Clock,
} from 'lucide-react';
import Link from 'next/link';

// Mock vehicle data (would come from form state)
const vehicle = {
  make: 'Toyota',
  model: 'Corolla',
  year: 2022,
  version: 'XLE',
  price: 1450000,
  mileage: 28000,
  fuelType: 'Gasolina',
  transmission: 'Automática',
  color: 'Blanco Perla',
  condition: 'Usado - Excelente',
  doors: 4,
  seats: 5,
  province: 'Distrito Nacional',
  city: 'Naco',
  description:
    'Toyota Corolla XLE 2022 en excelente estado. Un solo dueño, todos los mantenimientos al día en dealer autorizado. Equipado con cámara de reversa, sunroof, asientos de cuero, y sistema de navegación. Listo para inspección.',
  features: [
    'Cámara de reversa',
    'Sunroof',
    'Asientos de cuero',
    'Navegación GPS',
    'Bluetooth',
    'Control crucero',
  ],
  negotiable: true,
  acceptTrades: false,
  photos: 8,
};

export default function PublicarPreviewPage() {
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handlePublish = () => {
    setIsSubmitting(true);
    setTimeout(() => {
      window.location.href = '/mis-vehiculos';
    }, 2000);
  };

  const nextImage = () => {
    setCurrentImageIndex(prev => (prev + 1) % vehicle.photos);
  };

  const prevImage = () => {
    setCurrentImageIndex(prev => (prev - 1 + vehicle.photos) % vehicle.photos);
  };

  return (
    <div className="bg-muted/50 min-h-screen">
      <div className="mx-auto max-w-6xl px-4 py-8">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div>
            <Badge className="mb-2 bg-blue-100 text-blue-800">Vista Previa</Badge>
            <h1 className="text-foreground text-2xl font-bold">Así se verá tu publicación</h1>
            <p className="text-muted-foreground">Revisa que todo esté correcto antes de publicar</p>
          </div>
          <Link href="/publicar">
            <Button variant="outline">
              <Edit className="mr-2 h-4 w-4" />
              Editar
            </Button>
          </Link>
        </div>

        {/* Preview Alert */}
        <div className="mb-6 flex gap-3 rounded-lg border border-yellow-200 bg-yellow-50 p-4">
          <AlertCircle className="h-5 w-5 flex-shrink-0 text-yellow-600" />
          <div className="text-sm">
            <p className="font-medium text-yellow-900">Esto es una vista previa</p>
            <p className="text-yellow-700">
              Tu publicación será revisada por nuestro equipo antes de aparecer en el sitio. Este
              proceso toma aproximadamente 24 horas.
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          {/* Main Content */}
          <div className="space-y-6 lg:col-span-2">
            {/* Image Gallery */}
            <Card className="overflow-hidden">
              <div className="bg-muted relative aspect-[16/10]">
                <div className="text-muted-foreground absolute inset-0 flex items-center justify-center">
                  <Car className="h-20 w-20" />
                </div>

                {/* Image navigation */}
                <button
                  onClick={prevImage}
                  className="absolute top-1/2 left-4 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
                  aria-label="Imagen anterior"
                >
                  <ChevronLeft className="h-6 w-6" />
                </button>
                <button
                  onClick={nextImage}
                  className="absolute top-1/2 right-4 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
                  aria-label="Imagen siguiente"
                >
                  <ChevronRight className="h-6 w-6" />
                </button>

                {/* Image counter */}
                <div className="absolute bottom-4 left-1/2 -translate-x-1/2 rounded-full bg-black/50 px-3 py-1 text-sm text-white">
                  {currentImageIndex + 1} / {vehicle.photos}
                </div>

                {/* Action buttons */}
                <div className="absolute top-4 right-4 flex gap-2">
                  <Button
                    size="icon"
                    variant="secondary"
                    className="bg-white/90 hover:bg-white"
                    aria-label="Agregar a favoritos"
                  >
                    <Heart className="h-5 w-5" />
                  </Button>
                  <Button
                    size="icon"
                    variant="secondary"
                    className="bg-white/90 hover:bg-white"
                    aria-label="Compartir"
                  >
                    <Share2 className="h-5 w-5" />
                  </Button>
                </div>
              </div>

              {/* Thumbnail strip */}
              <div className="flex gap-2 overflow-x-auto p-4">
                {Array.from({ length: vehicle.photos }).map((_, i) => (
                  <button
                    key={i}
                    onClick={() => setCurrentImageIndex(i)}
                    className={`bg-muted h-16 w-16 flex-shrink-0 rounded-lg ${
                      i === currentImageIndex ? 'ring-primary ring-2' : ''
                    }`}
                  />
                ))}
              </div>
            </Card>

            {/* Vehicle Info */}
            <Card>
              <CardContent className="p-6">
                <div className="mb-4 flex items-start justify-between">
                  <div>
                    <h2 className="text-foreground text-2xl font-bold">
                      {vehicle.make} {vehicle.model} {vehicle.version}
                    </h2>
                    <p className="text-muted-foreground">
                      {vehicle.year} • {vehicle.condition}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-primary text-3xl font-bold">
                      RD$ {vehicle.price.toLocaleString()}
                    </p>
                    <div className="mt-1 flex gap-2">
                      {vehicle.negotiable && (
                        <Badge variant="outline" className="text-xs">
                          Negociable
                        </Badge>
                      )}
                      {vehicle.acceptTrades && (
                        <Badge variant="outline" className="text-xs">
                          Acepta Intercambios
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>

                <div className="text-muted-foreground mb-6 flex items-center gap-2">
                  <MapPin className="h-4 w-4" />
                  <span>
                    {vehicle.city}, {vehicle.province}
                  </span>
                </div>

                {/* Specs Grid */}
                <div className="mb-6 grid grid-cols-2 gap-4 md:grid-cols-4">
                  <div className="bg-muted/50 rounded-lg p-3 text-center">
                    <Gauge className="text-muted-foreground mx-auto mb-1 h-5 w-5" />
                    <p className="text-muted-foreground text-sm">Kilometraje</p>
                    <p className="font-medium">{vehicle.mileage.toLocaleString()} km</p>
                  </div>
                  <div className="bg-muted/50 rounded-lg p-3 text-center">
                    <Fuel className="text-muted-foreground mx-auto mb-1 h-5 w-5" />
                    <p className="text-muted-foreground text-sm">Combustible</p>
                    <p className="font-medium">{vehicle.fuelType}</p>
                  </div>
                  <div className="bg-muted/50 rounded-lg p-3 text-center">
                    <Settings className="text-muted-foreground mx-auto mb-1 h-5 w-5" />
                    <p className="text-muted-foreground text-sm">Transmisión</p>
                    <p className="font-medium">{vehicle.transmission}</p>
                  </div>
                  <div className="bg-muted/50 rounded-lg p-3 text-center">
                    <Palette className="text-muted-foreground mx-auto mb-1 h-5 w-5" />
                    <p className="text-muted-foreground text-sm">Color</p>
                    <p className="font-medium">{vehicle.color}</p>
                  </div>
                </div>

                {/* Description */}
                <div className="mb-6">
                  <h3 className="text-foreground mb-2 font-semibold">Descripción</h3>
                  <p className="text-muted-foreground whitespace-pre-line">{vehicle.description}</p>
                </div>

                {/* Features */}
                <div>
                  <h3 className="text-foreground mb-2 font-semibold">Características</h3>
                  <div className="flex flex-wrap gap-2">
                    {vehicle.features.map((feature, i) => (
                      <Badge key={i} variant="secondary" className="bg-muted">
                        <Check className="mr-1 h-3 w-3" />
                        {feature}
                      </Badge>
                    ))}
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Contact Card (Preview) */}
            <Card className="border-primary bg-primary/10">
              <CardContent className="p-6">
                <h3 className="text-foreground mb-4 font-semibold">Contactar Vendedor</h3>
                <div className="space-y-3">
                  <Button className="bg-primary hover:bg-primary/90 w-full" disabled>
                    <Phone className="mr-2 h-4 w-4" />
                    Ver Teléfono
                  </Button>
                  <Button className="w-full" variant="outline" disabled>
                    <MessageSquare className="mr-2 h-4 w-4" />
                    Enviar Mensaje
                  </Button>
                </div>
                <p className="text-muted-foreground mt-3 text-center text-xs">
                  * Los botones estarán activos cuando se publique
                </p>
              </CardContent>
            </Card>

            {/* Boost Option */}
            <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-orange-50">
              <CardContent className="p-6">
                <div className="mb-3 flex items-center gap-2">
                  <Sparkles className="h-5 w-5 text-amber-600" />
                  <h3 className="text-foreground font-semibold">Destacar Publicación</h3>
                </div>
                <p className="text-muted-foreground mb-4 text-sm">
                  Obtén hasta 10x más vistas destacando tu anuncio
                </p>
                <Button className="w-full bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600">
                  Agregar Boost
                </Button>
              </CardContent>
            </Card>

            {/* Publication Status */}
            <Card>
              <CardContent className="p-6">
                <h3 className="text-foreground mb-4 font-semibold">Estado de Publicación</h3>
                <div className="space-y-3">
                  <div className="flex items-center gap-3">
                    <div className="bg-primary/10 flex h-8 w-8 items-center justify-center rounded-full">
                      <Check className="text-primary h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-sm font-medium">Información completa</p>
                      <p className="text-muted-foreground text-xs">Todos los campos requeridos</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="bg-primary/10 flex h-8 w-8 items-center justify-center rounded-full">
                      <Check className="text-primary h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-sm font-medium">{vehicle.photos} fotos subidas</p>
                      <p className="text-muted-foreground text-xs">Mínimo requerido: 5</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="bg-muted flex h-8 w-8 items-center justify-center rounded-full">
                      <Clock className="text-muted-foreground h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-muted-foreground text-sm font-medium">
                        Pendiente de revisión
                      </p>
                      <p className="text-muted-foreground text-xs">Aproximadamente 24 horas</p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="bg-card mt-8 flex items-center justify-between rounded-lg border p-6 shadow-sm">
          <Link href="/publicar/fotos">
            <Button variant="outline">
              <ChevronLeft className="mr-2 h-4 w-4" />
              Volver a Fotos
            </Button>
          </Link>

          <div className="flex gap-3">
            <Button variant="outline">Guardar Borrador</Button>
            <Button
              className="bg-primary hover:bg-primary/90 px-8"
              onClick={handlePublish}
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <div className="mr-2 h-4 w-4 animate-spin rounded-full border-b-2 border-white" />
                  Publicando...
                </>
              ) : (
                <>
                  <Check className="mr-2 h-4 w-4" />
                  Publicar Vehículo
                </>
              )}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
