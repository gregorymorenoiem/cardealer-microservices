/**
 * HeroCarousel Component
 *
 * Premium full-screen carousel for hero section
 * Auto-rotates enterprise/premium tier vehicles
 * 100% visible - no search overlay for maximum ad visibility
 */

'use client';

import { useState, useEffect, useCallback, useMemo } from 'react';
import { ChevronLeft, ChevronRight, PlayCircle, PauseCircle, ChevronDown } from 'lucide-react';
import Link from 'next/link';
import Image from 'next/image';
import { motion, AnimatePresence } from 'framer-motion';
import { cn, formatCurrency, formatMileage } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import type { Vehicle } from '@/services/homepage-sections';

interface HeroCarouselProps {
  vehicles: Vehicle[];
  autoPlayInterval?: number;
  showScrollHint?: boolean;
  className?: string;
}

export default function HeroCarousel({
  vehicles,
  autoPlayInterval = 5000,
  showScrollHint = true,
  className = '',
}: HeroCarouselProps) {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isAutoPlaying, setIsAutoPlaying] = useState(true);
  const [isPaused, setIsPaused] = useState(false);
  const [touchStart, setTouchStart] = useState<number | null>(null);
  const [touchEnd, setTouchEnd] = useState<number | null>(null);

  // Ensure we have slides
  const slides = useMemo(() => vehicles.slice(0, 5), [vehicles]);
  const totalSlides = slides.length;

  // Auto-advance slides
  useEffect(() => {
    if (!isAutoPlaying || isPaused || totalSlides === 0) return;

    const interval = setInterval(() => {
      setCurrentIndex(prev => (prev + 1) % totalSlides);
    }, autoPlayInterval);

    return () => clearInterval(interval);
  }, [isAutoPlaying, isPaused, totalSlides, autoPlayInterval]);

  const goToSlide = useCallback((index: number) => {
    setCurrentIndex(index);
    setIsPaused(true);
    setTimeout(() => setIsPaused(false), 10000);
  }, []);

  const goToPrevious = useCallback(() => {
    const newIndex = currentIndex === 0 ? totalSlides - 1 : currentIndex - 1;
    goToSlide(newIndex);
  }, [currentIndex, totalSlides, goToSlide]);

  const goToNext = useCallback(() => {
    const newIndex = (currentIndex + 1) % totalSlides;
    goToSlide(newIndex);
  }, [currentIndex, totalSlides, goToSlide]);

  const toggleAutoPlay = () => {
    setIsAutoPlaying(!isAutoPlaying);
    setIsPaused(false);
  };

  // Touch gesture handlers for mobile swipe
  const minSwipeDistance = 50;

  const onTouchStart = (e: React.TouchEvent) => {
    setTouchEnd(null);
    setTouchStart(e.targetTouches[0].clientX);
  };

  const onTouchMove = (e: React.TouchEvent) => {
    setTouchEnd(e.targetTouches[0].clientX);
  };

  const onTouchEnd = () => {
    if (!touchStart || !touchEnd) return;

    const distance = touchStart - touchEnd;
    const isLeftSwipe = distance > minSwipeDistance;
    const isRightSwipe = distance < -minSwipeDistance;

    if (isLeftSwipe) {
      goToNext();
    } else if (isRightSwipe) {
      goToPrevious();
    }
  };

  if (totalSlides === 0) return null;

  const currentVehicle = slides[currentIndex];

  // Render badge based on tier
  const renderBadge = () => {
    if (!currentVehicle.tier || currentVehicle.tier === 'basic') return null;

    switch (currentVehicle.tier) {
      case 'enterprise':
        return (
          <Badge className="border-0 bg-gradient-to-r from-amber-500 to-orange-600 text-white">
            Top Dealer
          </Badge>
        );
      case 'premium':
        return (
          <Badge className="border-0 bg-gradient-to-r from-purple-500 to-indigo-600 text-white">
            Premium
          </Badge>
        );
      case 'featured':
        return (
          <Badge className="border-0 bg-gradient-to-r from-emerald-500 to-teal-600 text-white">
            Destacado
          </Badge>
        );
      default:
        return null;
    }
  };

  // Generate URL
  const generateVehicleUrl = (vehicle: Vehicle) => {
    const slug = `${vehicle.year}-${vehicle.make}-${vehicle.model}`
      .toLowerCase()
      .replace(/\s+/g, '-');
    return `/vehiculos/${slug}-${vehicle.id}`;
  };

  return (
    <div
      className={cn('relative h-[calc(100vh-4rem)] w-full overflow-hidden bg-gray-900', className)}
      onTouchStart={onTouchStart}
      onTouchMove={onTouchMove}
      onTouchEnd={onTouchEnd}
    >
      {/* Slides */}
      <div className="relative h-full">
        <AnimatePresence mode="wait">
          {slides.map(
            (vehicle, index) =>
              index === currentIndex && (
                <motion.div
                  key={vehicle.id}
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  transition={{ duration: 0.7 }}
                  className="absolute inset-0"
                >
                  {/* Background Image with Overlay */}
                  <div className="absolute inset-0">
                    <Image
                      src={vehicle.images[0] || '/placeholder-car.jpg'}
                      alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
                      fill
                      className="object-cover"
                      priority={index === 0}
                    />
                    {/* Gradient Overlay */}
                    <div className="absolute inset-0 bg-gradient-to-r from-black/80 via-black/50 to-transparent" />
                  </div>

                  {/* Content */}
                  <div className="relative mx-auto h-full max-w-7xl px-4 sm:px-6 lg:px-8">
                    <div className="flex h-full max-w-2xl flex-col justify-center">
                      {/* Badge */}
                      {renderBadge() && <div className="mb-2 sm:mb-4">{renderBadge()}</div>}

                      {/* Title */}
                      <h1 className="mb-2 text-3xl leading-[1.1] font-bold tracking-tight text-white sm:mb-4 sm:text-4xl md:text-5xl lg:text-7xl">
                        {vehicle.year} {vehicle.make}
                        <br />
                        <span className="text-primary">{vehicle.model}</span>
                      </h1>

                      {/* Price */}
                      <p className="mb-3 text-2xl font-bold tracking-tight text-white sm:mb-6 sm:text-3xl md:text-4xl">
                        {formatCurrency(vehicle.price)}
                      </p>

                      {/* Quick Details */}
                      <div className="mb-4 flex flex-wrap gap-2 text-white/90 sm:mb-8 sm:gap-4">
                        <span className="flex items-center gap-1 text-sm sm:gap-2 sm:text-lg">
                          <span className="font-semibold">{formatMileage(vehicle.mileage)}</span>
                        </span>
                        <span className="hidden text-white/50 sm:inline">•</span>
                        <span className="text-sm font-medium sm:text-lg">
                          {vehicle.transmission}
                        </span>
                        <span className="hidden text-white/50 sm:inline">•</span>
                        <span className="text-sm font-medium sm:text-lg">{vehicle.fuelType}</span>
                      </div>

                      {/* CTA Buttons */}
                      <div className="flex flex-col gap-3 sm:flex-row sm:gap-4">
                        <Link
                          href={generateVehicleUrl(vehicle)}
                          className="bg-primary hover:bg-primary/90 active:bg-primary/80 touch-manipulation rounded-lg px-6 py-3 text-center text-base font-semibold tracking-wide text-white transition-colors duration-200 sm:px-8 sm:py-4 sm:text-lg"
                        >
                          Ver Detalles
                        </Link>
                        <button className="touch-manipulation rounded-lg border border-white/30 bg-white/10 px-6 py-3 text-base font-semibold tracking-wide text-white backdrop-blur-sm transition-colors duration-200 hover:bg-white/20 active:bg-white/30 sm:px-8 sm:py-4 sm:text-lg">
                          Contactar Vendedor
                        </button>
                      </div>
                    </div>
                  </div>
                </motion.div>
              )
          )}
        </AnimatePresence>
      </div>

      {/* Navigation Arrows - Hidden on mobile, visible on tablet+ */}
      <button
        onClick={goToPrevious}
        className="absolute top-1/2 left-2 z-20 hidden -translate-y-1/2 touch-manipulation rounded-full border border-white/20 bg-white/10 p-2 backdrop-blur-sm transition-colors duration-200 hover:bg-white/20 active:bg-white/30 sm:left-4 sm:block sm:p-3"
        aria-label="Previous slide"
      >
        <ChevronLeft className="h-5 w-5 text-white sm:h-6 sm:w-6" />
      </button>

      <button
        onClick={goToNext}
        className="absolute top-1/2 right-2 z-20 hidden -translate-y-1/2 touch-manipulation rounded-full border border-white/20 bg-white/10 p-2 backdrop-blur-sm transition-colors duration-200 hover:bg-white/20 active:bg-white/30 sm:right-4 sm:block sm:p-3"
        aria-label="Next slide"
      >
        <ChevronRight className="h-5 w-5 text-white sm:h-6 sm:w-6" />
      </button>

      {/* Bottom Controls */}
      <div className="absolute bottom-4 left-1/2 z-20 flex -translate-x-1/2 items-center gap-4 sm:bottom-8">
        {/* Slide Indicators */}
        <div className="flex items-center gap-2">
          {slides.map((_, index) => (
            <button
              key={index}
              onClick={() => goToSlide(index)}
              className={cn(
                'h-2 w-2 rounded-full transition-all duration-300 sm:h-3 sm:w-3',
                index === currentIndex ? 'w-6 bg-white sm:w-8' : 'bg-white/50 hover:bg-white/75'
              )}
              aria-label={`Go to slide ${index + 1}`}
            />
          ))}
        </div>

        {/* Auto-play Toggle */}
        <button
          onClick={toggleAutoPlay}
          className="rounded-full border border-white/20 bg-white/10 p-1.5 backdrop-blur-sm transition-colors duration-200 hover:bg-white/20 sm:p-2"
          aria-label={isAutoPlaying ? 'Pause autoplay' : 'Start autoplay'}
        >
          {isAutoPlaying ? (
            <PauseCircle className="h-4 w-4 text-white sm:h-5 sm:w-5" />
          ) : (
            <PlayCircle className="h-4 w-4 text-white sm:h-5 sm:w-5" />
          )}
        </button>
      </div>

      {/* Scroll Hint */}
      {showScrollHint && (
        <motion.div
          className="absolute bottom-4 left-1/2 z-10 hidden -translate-x-1/2 flex-col items-center text-white/70 md:flex"
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 2, duration: 0.5 }}
        >
          <span className="mb-1 text-sm">Desliza para más</span>
          <motion.div animate={{ y: [0, 5, 0] }} transition={{ repeat: Infinity, duration: 1.5 }}>
            <ChevronDown className="h-5 w-5" />
          </motion.div>
        </motion.div>
      )}
    </div>
  );
}
