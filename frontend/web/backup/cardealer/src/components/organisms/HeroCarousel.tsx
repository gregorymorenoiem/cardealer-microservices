/**
 * HeroCarousel Component
 * Cardealer - Copied exactly from original
 * 
 * Premium full-screen carousel for hero section
 * Auto-rotates enterprise/premium tier vehicles
 */

import { useState, useEffect, useCallback } from 'react';
import { ChevronLeft, ChevronRight, PlayCircle, PauseCircle, ChevronDown } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { Vehicle } from '@/data/mockVehicles';
import { DestacadoBadge, PremiumBadge, TopDealerBadge } from '@/components/atoms';

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
  className = '' 
}: HeroCarouselProps) {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isAutoPlaying, setIsAutoPlaying] = useState(true);
  const [isPaused, setIsPaused] = useState(false);
  const [touchStart, setTouchStart] = useState<number | null>(null);
  const [touchEnd, setTouchEnd] = useState<number | null>(null);

  const slides = vehicles.slice(0, 5);
  const totalSlides = slides.length;

  useEffect(() => {
    if (!isAutoPlaying || isPaused || totalSlides === 0) return;

    const interval = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % totalSlides);
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

  const renderBadge = () => {
    if (!currentVehicle.tier || currentVehicle.tier === 'basic') return null;
    
    switch (currentVehicle.tier) {
      case 'enterprise':
        return <TopDealerBadge size="lg" />;
      case 'premium':
        return <PremiumBadge size="lg" />;
      case 'featured':
        return <DestacadoBadge size="lg" />;
      default:
        return null;
    }
  };

  return (
    <div 
      className={`relative w-full h-[calc(100vh-4rem)] overflow-hidden bg-gray-900 ${className}`}
      onTouchStart={onTouchStart}
      onTouchMove={onTouchMove}
      onTouchEnd={onTouchEnd}
    >
      <div className="relative h-full">
        {slides.map((vehicle, index) => (
          <div
            key={vehicle.id}
            className={`absolute inset-0 transition-opacity duration-700 ${
              index === currentIndex ? 'opacity-100 z-10' : 'opacity-0 z-0'
            }`}
          >
            <div className="absolute inset-0">
              <img
                src={vehicle.images[0]}
                alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
                className="w-full h-full object-cover"
                loading={index === 0 ? 'eager' : 'lazy'}
              />
              <div className="absolute inset-0 bg-gradient-to-r from-black/80 via-black/50 to-transparent" />
            </div>

            <div className="relative h-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
              <div className="flex flex-col justify-center h-full max-w-2xl">
                {renderBadge() && (
                  <div className="mb-2 sm:mb-4">
                    {renderBadge()}
                  </div>
                )}

                <h1 className="text-3xl sm:text-4xl md:text-5xl lg:text-7xl font-bold text-white mb-2 sm:mb-4 leading-tight">
                  {vehicle.year} {vehicle.make}
                  <br />
                  <span className="text-blue-400">{vehicle.model}</span>
                </h1>

                <p className="text-2xl sm:text-3xl md:text-4xl font-bold text-white mb-3 sm:mb-6">
                  ${vehicle.price.toLocaleString('en-US')}
                </p>

                <div className="flex flex-wrap gap-2 sm:gap-4 mb-4 sm:mb-8 text-white/90">
                  <span className="flex items-center gap-1 sm:gap-2 text-sm sm:text-lg">
                    <span className="font-semibold">{vehicle.mileage.toLocaleString()}</span> mi
                  </span>
                  <span className="text-white/50 hidden sm:inline">•</span>
                  <span className="text-sm sm:text-lg">{vehicle.transmission}</span>
                  <span className="text-white/50 hidden sm:inline">•</span>
                  <span className="text-sm sm:text-lg">{vehicle.fuelType}</span>
                </div>

                <div className="flex flex-col sm:flex-row gap-3 sm:gap-4">
                  <Link
                    to={`/vehicles/${vehicle.id}`}
                    className="px-6 py-3 sm:px-8 sm:py-4 bg-blue-600 hover:bg-blue-700 active:bg-blue-800 text-white font-semibold rounded-lg transition-colors duration-200 text-base sm:text-lg text-center touch-manipulation"
                  >
                    Ver Detalles
                  </Link>
                  <button
                    className="px-6 py-3 sm:px-8 sm:py-4 bg-white/10 hover:bg-white/20 active:bg-white/30 backdrop-blur-sm text-white font-semibold rounded-lg transition-colors duration-200 text-base sm:text-lg border border-white/20 touch-manipulation"
                  >
                    Contactar Vendedor
                  </button>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      <button
        onClick={goToPrevious}
        className="hidden sm:block absolute left-2 sm:left-4 top-1/2 -translate-y-1/2 z-20 p-2 sm:p-3 bg-white/10 hover:bg-white/20 active:bg-white/30 backdrop-blur-sm rounded-full transition-colors duration-200 border border-white/20 touch-manipulation"
        aria-label="Previous slide"
      >
        <ChevronLeft size={24} className="sm:w-8 sm:h-8 text-white" />
      </button>
      <button
        onClick={goToNext}
        className="hidden sm:block absolute right-2 sm:right-4 top-1/2 -translate-y-1/2 z-20 p-2 sm:p-3 bg-white/10 hover:bg-white/20 active:bg-white/30 backdrop-blur-sm rounded-full transition-colors duration-200 border border-white/20 touch-manipulation"
        aria-label="Next slide"
      >
        <ChevronRight size={24} className="sm:w-8 sm:h-8 text-white" />
      </button>

      <div className="absolute bottom-4 sm:bottom-8 left-0 right-0 z-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between">
            <div className="flex gap-1.5 sm:gap-2">
              {slides.map((_, index) => (
                <button
                  key={index}
                  onClick={() => goToSlide(index)}
                  className={`h-1 rounded-full transition-all duration-300 touch-manipulation ${
                    index === currentIndex 
                      ? 'w-8 sm:w-12 bg-white' 
                      : 'w-6 sm:w-8 bg-white/40 hover:bg-white/60 active:bg-white/80'
                  }`}
                  aria-label={`Go to slide ${index + 1}`}
                />
              ))}
            </div>

            <button
              onClick={toggleAutoPlay}
              className="p-1.5 sm:p-2 bg-white/10 hover:bg-white/20 active:bg-white/30 backdrop-blur-sm rounded-full transition-colors duration-200 border border-white/20 touch-manipulation"
              aria-label={isAutoPlaying ? 'Pause auto-play' : 'Resume auto-play'}
            >
              {isAutoPlaying ? (
                <PauseCircle size={20} className="sm:w-6 sm:h-6 text-white" />
              ) : (
                <PlayCircle size={20} className="sm:w-6 sm:h-6 text-white" />
              )}
            </button>
          </div>
        </div>
      </div>

      {showScrollHint && (
        <div className="absolute bottom-20 sm:bottom-24 left-1/2 -translate-x-1/2 z-20 animate-bounce">
          <div className="flex flex-col items-center gap-1 text-white/70">
            <span className="text-xs sm:text-sm font-medium">Desliza para más</span>
            <ChevronDown size={20} className="sm:w-6 sm:h-6" />
          </div>
        </div>
      )}
    </div>
  );
}
