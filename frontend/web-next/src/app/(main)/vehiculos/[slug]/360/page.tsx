/**
 * Vehicle 360° Viewer Page
 *
 * Fullscreen immersive 360° viewer for vehicles
 *
 * Features:
 * - Fullscreen layout without navbar
 * - Touch/mouse drag to rotate
 * - Zoom controls
 * - Interior/Exterior toggle
 * - Hotspots for features
 * - Back button to vehicle detail
 *
 * Route: /vehiculos/[slug]/360
 */

'use client';

import * as React from 'react';
import { useParams, useRouter } from 'next/navigation';
import Image from 'next/image';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Slider } from '@/components/ui/slider';
import { Skeleton } from '@/components/ui/skeleton';
import {
  ArrowLeft,
  ZoomIn,
  ZoomOut,
  RotateCcw,
  Maximize2,
  Minimize2,
  Car,
  Armchair,
  Play,
  Pause,
  Info,
  X,
  AlertCircle,
  RefreshCw,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { vehicleService, type Hotspot360 } from '@/services/vehicles';

// =============================================================================
// LOADING SKELETON
// =============================================================================

function Vehicle360Skeleton() {
  return (
    <div className="fixed inset-0 flex flex-col bg-gray-900">
      {/* Top bar skeleton */}
      <div className="absolute top-0 right-0 left-0 z-20 flex items-center justify-between bg-gradient-to-b from-black/60 to-transparent p-4">
        <Skeleton className="h-9 w-24 bg-white/20" />
        <div className="flex items-center gap-2">
          <Skeleton className="h-9 w-40 bg-white/20" />
          <Skeleton className="h-9 w-9 bg-white/20" />
        </div>
      </div>

      {/* Main viewer skeleton */}
      <div className="flex flex-1 items-center justify-center">
        <div className="text-center text-white">
          <div className="mx-auto mb-4 h-16 w-16 animate-spin rounded-full border-4 border-primary border-t-transparent" />
          <p>Cargando vista 360°...</p>
        </div>
      </div>

      {/* Bottom controls skeleton */}
      <div className="absolute right-0 bottom-0 left-0 z-20 bg-gradient-to-t from-black/60 to-transparent p-4">
        <div className="mx-auto max-w-xl">
          <Skeleton className="mb-4 h-2 w-full bg-white/20" />
          <div className="flex items-center justify-center gap-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-10 w-10 rounded-full bg-white/20" />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// ERROR STATE
// =============================================================================

function Vehicle360Error({
  message,
  onRetry,
  onBack,
}: {
  message: string;
  onRetry: () => void;
  onBack: () => void;
}) {
  return (
    <div className="fixed inset-0 flex flex-col items-center justify-center bg-gray-900">
      <div className="text-center text-white">
        <AlertCircle className="mx-auto mb-4 h-16 w-16 text-red-400" />
        <h2 className="mb-2 text-xl font-semibold">Vista 360° no disponible</h2>
        <p className="mb-6 text-muted-foreground">{message}</p>
        <div className="flex gap-3">
          <Button variant="outline" onClick={onBack}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Volver
          </Button>
          <Button onClick={onRetry} className="bg-primary hover:bg-primary/90">
            <RefreshCw className="mr-2 h-4 w-4" />
            Reintentar
          </Button>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// NO 360 VIEW AVAILABLE
// =============================================================================

function No360ViewAvailable({
  vehicleTitle,
  images,
  onBack,
}: {
  vehicleTitle: string;
  images: string[];
  onBack: () => void;
}) {
  const [currentIndex, setCurrentIndex] = React.useState(0);

  return (
    <div className="fixed inset-0 flex flex-col bg-gray-900">
      {/* Top bar */}
      <div className="absolute top-0 right-0 left-0 z-20 flex items-center justify-between bg-gradient-to-b from-black/60 to-transparent p-4">
        <Button variant="ghost" size="sm" className="text-white hover:bg-white/20" onClick={onBack}>
          <ArrowLeft className="mr-2 h-5 w-5" />
          Volver
        </Button>
        <span className="font-medium text-white">{vehicleTitle}</span>
        <div className="w-20" />
      </div>

      {/* Gallery */}
      <div className="flex flex-1 items-center justify-center px-4">
        {images.length > 0 ? (
          <div className="relative aspect-video w-full max-w-4xl">
            <Image
              src={images[currentIndex]}
              alt={`${vehicleTitle} - Foto ${currentIndex + 1}`}
              fill
              className="object-contain"
            />
          </div>
        ) : (
          <div className="text-center text-white">
            <Car className="mx-auto mb-4 h-24 w-24 opacity-50" />
            <p>No hay imágenes disponibles</p>
          </div>
        )}
      </div>

      {/* Notice */}
      <div className="absolute right-0 bottom-0 left-0 z-20 bg-gradient-to-t from-black/60 to-transparent p-4">
        <div className="mx-auto max-w-xl text-center">
          <div className="mb-4 rounded-lg bg-yellow-500/20 p-3 text-yellow-200">
            <Info className="mr-2 inline-block h-4 w-4" />
            Este vehículo no tiene vista 360° disponible. Mostrando galería de fotos.
          </div>

          {images.length > 1 && (
            <div className="flex items-center justify-center gap-2">
              {images.map((_, i) => (
                <button
                  key={i}
                  className={cn(
                    'h-2 w-2 rounded-full transition-colors',
                    i === currentIndex ? 'bg-white' : 'bg-white/40'
                  )}
                  onClick={() => setCurrentIndex(i)}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function Vehicle360Page() {
  const params = useParams();
  const router = useRouter();
  const slug = params.slug as string;
  const containerRef = React.useRef<HTMLDivElement>(null);

  // Fetch 360 view data
  const {
    data: view360Data,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['vehicle360', slug],
    queryFn: () => vehicleService.get360View(slug),
    enabled: !!slug,
    staleTime: 1000 * 60 * 10, // 10 minutes
  });

  // State
  const [currentFrame, setCurrentFrame] = React.useState(0);
  const [isExterior, setIsExterior] = React.useState(true);
  const [zoom, setZoom] = React.useState(1);
  const [isAutoRotating, setIsAutoRotating] = React.useState(false);
  const [isDragging, setIsDragging] = React.useState(false);
  const [lastX, setLastX] = React.useState(0);
  const [selectedHotspot, setSelectedHotspot] = React.useState<Hotspot360 | null>(null);
  const [isFullscreen, setIsFullscreen] = React.useState(false);
  const [imageLoading, setImageLoading] = React.useState(true);

  // Compute current images
  const images = React.useMemo(() => {
    if (!view360Data) return [];
    return isExterior ? view360Data.exteriorImages : view360Data.interiorImages;
  }, [view360Data, isExterior]);

  const totalFrames = images.length;
  const hotspots = view360Data?.hotspots ?? [];
  const has360 = view360Data?.hasExterior360 || view360Data?.hasInterior360;

  // Auto-rotate effect
  React.useEffect(() => {
    if (!isAutoRotating || totalFrames === 0) return;

    const interval = setInterval(() => {
      setCurrentFrame(prev => (prev + 1) % totalFrames);
    }, 100);

    return () => clearInterval(interval);
  }, [isAutoRotating, totalFrames]);

  // Keyboard controls
  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (totalFrames === 0) return;

      switch (e.key) {
        case 'ArrowLeft':
          setCurrentFrame(prev => (prev - 1 + totalFrames) % totalFrames);
          break;
        case 'ArrowRight':
          setCurrentFrame(prev => (prev + 1) % totalFrames);
          break;
        case 'Escape':
          if (selectedHotspot) {
            setSelectedHotspot(null);
          } else {
            router.back();
          }
          break;
        case ' ':
          e.preventDefault();
          setIsAutoRotating(prev => !prev);
          break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [totalFrames, router, selectedHotspot]);

  // Mouse/Touch drag handlers
  const handleMouseDown = (e: React.MouseEvent) => {
    if (totalFrames === 0) return;
    setIsDragging(true);
    setLastX(e.clientX);
    setIsAutoRotating(false);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging || totalFrames === 0) return;

    const deltaX = e.clientX - lastX;
    const sensitivity = 5;

    if (Math.abs(deltaX) > sensitivity) {
      const direction = deltaX > 0 ? 1 : -1;
      setCurrentFrame(prev => (prev + direction + totalFrames) % totalFrames);
      setLastX(e.clientX);
    }
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const handleTouchStart = (e: React.TouchEvent) => {
    if (totalFrames === 0) return;
    setIsDragging(true);
    setLastX(e.touches[0].clientX);
    setIsAutoRotating(false);
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    if (!isDragging || totalFrames === 0) return;

    const deltaX = e.touches[0].clientX - lastX;
    const sensitivity = 5;

    if (Math.abs(deltaX) > sensitivity) {
      const direction = deltaX > 0 ? 1 : -1;
      setCurrentFrame(prev => (prev + direction + totalFrames) % totalFrames);
      setLastX(e.touches[0].clientX);
    }
  };

  const handleTouchEnd = () => {
    setIsDragging(false);
  };

  // Fullscreen toggle
  const toggleFullscreen = () => {
    if (!document.fullscreenElement) {
      containerRef.current?.requestFullscreen();
      setIsFullscreen(true);
    } else {
      document.exitFullscreen();
      setIsFullscreen(false);
    }
  };

  // Reset view
  const resetView = () => {
    setCurrentFrame(0);
    setZoom(1);
    setIsAutoRotating(false);
  };

  // Handle back
  const handleBack = () => router.back();

  // Loading state
  if (isLoading) {
    return <Vehicle360Skeleton />;
  }

  // Error state
  if (error) {
    return (
      <Vehicle360Error
        message="No se pudo cargar la vista 360° del vehículo"
        onRetry={() => refetch()}
        onBack={handleBack}
      />
    );
  }

  // No 360 view available
  if (view360Data && !has360) {
    return (
      <No360ViewAvailable
        vehicleTitle={view360Data.vehicleTitle}
        images={view360Data.exteriorImages}
        onBack={handleBack}
      />
    );
  }

  return (
    <div ref={containerRef} className="fixed inset-0 flex flex-col bg-gray-900 select-none">
      {/* Top Controls */}
      <div className="absolute top-0 right-0 left-0 z-20 flex items-center justify-between bg-gradient-to-b from-black/60 to-transparent p-4">
        <Button
          variant="ghost"
          size="sm"
          className="text-white hover:bg-white/20"
          onClick={handleBack}
        >
          <ArrowLeft className="mr-2 h-5 w-5" />
          Volver
        </Button>

        <span className="hidden max-w-[200px] truncate font-medium text-white sm:block">
          {view360Data?.vehicleTitle}
        </span>

        <div className="flex items-center gap-2">
          {/* View Toggle */}
          {view360Data?.hasExterior360 && view360Data?.hasInterior360 && (
            <div className="flex rounded-lg bg-black/40 p-1">
              <Button
                variant="ghost"
                size="sm"
                className={cn('text-white hover:bg-white/20', isExterior && 'bg-white/20')}
                onClick={() => {
                  setIsExterior(true);
                  setCurrentFrame(0);
                  setImageLoading(true);
                }}
              >
                <Car className="mr-1.5 h-4 w-4" />
                Exterior
              </Button>
              <Button
                variant="ghost"
                size="sm"
                className={cn('text-white hover:bg-white/20', !isExterior && 'bg-white/20')}
                onClick={() => {
                  setIsExterior(false);
                  setCurrentFrame(0);
                  setImageLoading(true);
                }}
              >
                <Armchair className="mr-1.5 h-4 w-4" />
                Interior
              </Button>
            </div>
          )}

          <Button
            variant="ghost"
            size="icon"
            className="text-white hover:bg-white/20"
            onClick={toggleFullscreen}
          >
            {isFullscreen ? <Minimize2 className="h-5 w-5" /> : <Maximize2 className="h-5 w-5" />}
          </Button>
        </div>
      </div>

      {/* 360 Viewer */}
      <div
        className="relative flex-1 cursor-grab overflow-hidden active:cursor-grabbing"
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
      >
        {/* Current Frame Image */}
        {images.length > 0 && (
          <div
            className="absolute inset-0 transition-transform duration-100"
            style={{ transform: `scale(${zoom})` }}
          >
            <Image
              src={images[currentFrame]}
              alt={`${view360Data?.vehicleTitle} - Vista 360° Frame ${currentFrame + 1}`}
              fill
              className="object-contain"
              priority
              onLoad={() => setImageLoading(false)}
            />
          </div>
        )}

        {/* Loading Indicator */}
        {imageLoading && (
          <div className="absolute inset-0 flex items-center justify-center bg-gray-900/50">
            <div className="text-center text-white">
              <div className="mx-auto mb-4 h-16 w-16 animate-spin rounded-full border-4 border-primary border-t-transparent" />
              <p>Cargando imagen...</p>
            </div>
          </div>
        )}

        {/* Hotspots (only on exterior) */}
        {isExterior &&
          hotspots.map(hotspot => (
            <button
              key={hotspot.id}
              className={cn(
                'absolute h-8 w-8 -translate-x-1/2 -translate-y-1/2',
                'rounded-full border-2 border-white bg-primary/100 shadow-lg',
                'flex items-center justify-center text-white',
                'transition-transform hover:scale-110',
                'animate-pulse'
              )}
              style={{
                left: `${hotspot.x}%`,
                top: `${hotspot.y}%`,
              }}
              onClick={e => {
                e.stopPropagation();
                setSelectedHotspot(hotspot);
              }}
            >
              <Info className="h-4 w-4" />
            </button>
          ))}

        {/* Hotspot Detail Modal */}
        {selectedHotspot && (
          <div className="absolute inset-0 z-30 flex items-center justify-center bg-black/50">
            <div className="mx-4 max-w-sm rounded-xl bg-card p-6 shadow-2xl">
              <div className="mb-3 flex items-start justify-between">
                <h3 className="text-lg font-semibold">{selectedHotspot.label}</h3>
                <Button
                  variant="ghost"
                  size="icon"
                  className="-mt-2 -mr-2 h-8 w-8"
                  onClick={() => setSelectedHotspot(null)}
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
              <p className="text-muted-foreground">{selectedHotspot.description}</p>
            </div>
          </div>
        )}

        {/* Drag Hint */}
        {!isDragging && !isAutoRotating && totalFrames > 1 && (
          <div className="absolute bottom-20 left-1/2 flex -translate-x-1/2 items-center gap-2 text-sm text-white/60">
            <span>← Arrastra para rotar →</span>
          </div>
        )}
      </div>

      {/* Bottom Controls */}
      <div className="absolute right-0 bottom-0 left-0 z-20 bg-gradient-to-t from-black/60 to-transparent p-4">
        <div className="mx-auto max-w-xl">
          {/* Frame Slider */}
          {totalFrames > 1 && (
            <div className="mb-4">
              <Slider
                value={[currentFrame]}
                min={0}
                max={totalFrames - 1}
                step={1}
                onValueChange={([value]) => {
                  setCurrentFrame(value);
                  setIsAutoRotating(false);
                }}
                className="w-full"
              />
              <div className="mt-1 flex justify-between text-xs text-white/60">
                <span>0°</span>
                <span>{Math.round((currentFrame / totalFrames) * 360)}°</span>
                <span>360°</span>
              </div>
            </div>
          )}

          {/* Control Buttons */}
          <div className="flex items-center justify-center gap-3">
            <Button
              variant="ghost"
              size="icon"
              className="text-white hover:bg-white/20"
              onClick={() => setZoom(prev => Math.max(1, prev - 0.25))}
              disabled={zoom <= 1}
            >
              <ZoomOut className="h-5 w-5" />
            </Button>

            <Button
              variant="ghost"
              size="icon"
              className="text-white hover:bg-white/20"
              onClick={() => setZoom(prev => Math.min(3, prev + 0.25))}
              disabled={zoom >= 3}
            >
              <ZoomIn className="h-5 w-5" />
            </Button>

            {totalFrames > 1 && (
              <Button
                variant="ghost"
                size="icon"
                className={cn(
                  'text-white hover:bg-white/20',
                  isAutoRotating && 'bg-primary/100 hover:bg-primary/80'
                )}
                onClick={() => setIsAutoRotating(prev => !prev)}
              >
                {isAutoRotating ? <Pause className="h-5 w-5" /> : <Play className="h-5 w-5" />}
              </Button>
            )}

            <Button
              variant="ghost"
              size="icon"
              className="text-white hover:bg-white/20"
              onClick={resetView}
            >
              <RotateCcw className="h-5 w-5" />
            </Button>
          </div>

          {/* Keyboard Shortcuts Hint */}
          {totalFrames > 1 && (
            <div className="mt-3 text-center text-xs text-white/40">
              ← → para rotar • Espacio para auto-rotar • ESC para salir
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
