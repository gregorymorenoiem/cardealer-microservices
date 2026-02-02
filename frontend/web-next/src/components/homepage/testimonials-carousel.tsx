/**
 * Testimonials Carousel
 *
 * Professional testimonials section with avatars and ratings
 * Builds trust with real user experiences
 */

'use client';

import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ChevronLeft, ChevronRight, Star, Quote } from 'lucide-react';
import Image from 'next/image';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

interface Testimonial {
  id: string;
  name: string;
  role: string;
  avatarUrl?: string;
  content: string;
  rating: number;
  vehiclePurchased?: string;
  location?: string;
}

interface TestimonialsCarouselProps {
  testimonials?: Testimonial[];
  autoPlay?: boolean;
  autoPlayInterval?: number;
  className?: string;
}

// =============================================================================
// DEFAULT TESTIMONIALS
// =============================================================================

const DEFAULT_TESTIMONIALS: Testimonial[] = [
  {
    id: '1',
    name: 'María González',
    role: 'Compradora Verificada',
    content:
      'Encontré mi Toyota RAV4 en perfectas condiciones y a un precio increíble. El proceso fue súper fácil y el vendedor muy profesional. ¡100% recomendado!',
    rating: 5,
    vehiclePurchased: 'Toyota RAV4 2023',
    location: 'Santo Domingo',
  },
  {
    id: '2',
    name: 'Carlos Rodríguez',
    role: 'Vendedor Dealer',
    content:
      'Como dealer, OKLA me ha ayudado a aumentar mis ventas en un 40%. La plataforma es intuitiva y el soporte es excelente. Mis clientes confían más cuando ven el badge de verificado.',
    rating: 5,
    location: 'Santiago',
  },
  {
    id: '3',
    name: 'Ana Martínez',
    role: 'Compradora Verificada',
    content:
      'Vendí mi carro en menos de una semana. Las fotos profesionales que ofrece OKLA hicieron la diferencia. Recibí ofertas serias desde el primer día.',
    rating: 5,
    vehiclePurchased: 'Honda Civic 2022',
    location: 'La Romana',
  },
  {
    id: '4',
    name: 'Roberto Peña',
    role: 'Comprador Verificado',
    content:
      'La función de comparación me ayudó a tomar una decisión informada. Pude ver lado a lado tres opciones y elegir la mejor para mi presupuesto.',
    rating: 4,
    vehiclePurchased: 'Hyundai Tucson 2024',
    location: 'Puerto Plata',
  },
  {
    id: '5',
    name: 'Laura Díaz',
    role: 'Vendedora Individual',
    content:
      'Primera vez vendiendo un carro y OKLA me guió en todo el proceso. Las alertas de precio me ayudaron a ponerle el precio correcto a mi vehículo.',
    rating: 5,
    location: 'Punta Cana',
  },
];

// =============================================================================
// STAR RATING
// =============================================================================

function StarRating({ rating }: { rating: number }) {
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map(star => (
        <Star
          key={star}
          className={cn(
            'h-5 w-5',
            star <= rating ? 'fill-amber-400 text-amber-400' : 'fill-gray-200 text-gray-200'
          )}
        />
      ))}
    </div>
  );
}

// =============================================================================
// AVATAR
// =============================================================================

function Avatar({ name, imageUrl }: { name: string; imageUrl?: string }) {
  const initials = name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);

  if (imageUrl) {
    return (
      <div className="relative h-16 w-16 overflow-hidden rounded-full shadow-lg ring-4 ring-white">
        <Image src={imageUrl} alt={name} fill className="object-cover" />
      </div>
    );
  }

  return (
    <div className="flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-br from-[#00A870] to-[#009663] shadow-lg ring-4 ring-white">
      <span className="text-xl font-bold text-white">{initials}</span>
    </div>
  );
}

// =============================================================================
// TESTIMONIAL CARD
// =============================================================================

function TestimonialCard({ testimonial }: { testimonial: Testimonial }) {
  return (
    <div className="relative rounded-3xl bg-white p-8 shadow-xl md:p-10">
      {/* Quote Icon */}
      <div className="absolute -top-4 left-8">
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[#00A870] shadow-lg">
          <Quote className="h-5 w-5 fill-white text-white" />
        </div>
      </div>

      {/* Content */}
      <div className="pt-4">
        {/* Rating */}
        <StarRating rating={testimonial.rating} />

        {/* Testimonial Text */}
        <p className="mt-4 text-lg leading-relaxed text-gray-700">"{testimonial.content}"</p>

        {/* Vehicle Purchased */}
        {testimonial.vehiclePurchased && (
          <p className="mt-4 text-sm font-medium text-[#00A870]">
            Compró: {testimonial.vehiclePurchased}
          </p>
        )}

        {/* Author */}
        <div className="mt-6 flex items-center gap-4">
          <Avatar name={testimonial.name} imageUrl={testimonial.avatarUrl} />
          <div>
            <p className="font-semibold text-gray-900">{testimonial.name}</p>
            <p className="text-sm text-gray-500">{testimonial.role}</p>
            {testimonial.location && (
              <p className="text-xs text-gray-400">{testimonial.location}</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN CAROUSEL COMPONENT
// =============================================================================

export function TestimonialsCarousel({
  testimonials = DEFAULT_TESTIMONIALS,
  autoPlay = true,
  autoPlayInterval = 6000,
  className,
}: TestimonialsCarouselProps) {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isPaused, setIsPaused] = useState(false);

  // Auto-advance
  useEffect(() => {
    if (!autoPlay || isPaused) return;

    const interval = setInterval(() => {
      setCurrentIndex(prev => (prev + 1) % testimonials.length);
    }, autoPlayInterval);

    return () => clearInterval(interval);
  }, [autoPlay, isPaused, testimonials.length, autoPlayInterval]);

  const goToPrevious = () => {
    setCurrentIndex(prev => (prev === 0 ? testimonials.length - 1 : prev - 1));
  };

  const goToNext = () => {
    setCurrentIndex(prev => (prev + 1) % testimonials.length);
  };

  return (
    <div
      className={cn('relative', className)}
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
    >
      {/* Main Carousel */}
      <div className="relative overflow-hidden">
        <AnimatePresence mode="wait">
          <motion.div
            key={currentIndex}
            initial={{ opacity: 0, x: 100 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -100 }}
            transition={{ duration: 0.5, ease: 'easeInOut' }}
            className="mx-auto max-w-3xl"
          >
            <TestimonialCard testimonial={testimonials[currentIndex]} />
          </motion.div>
        </AnimatePresence>
      </div>

      {/* Navigation Arrows */}
      <div className="pointer-events-none absolute top-1/2 right-0 left-0 flex -translate-y-1/2 justify-between px-4">
        <button
          onClick={goToPrevious}
          className="pointer-events-auto flex h-12 w-12 items-center justify-center rounded-full bg-white text-gray-600 shadow-lg transition-all hover:text-[#00A870] hover:shadow-xl"
        >
          <ChevronLeft className="h-6 w-6" />
        </button>
        <button
          onClick={goToNext}
          className="pointer-events-auto flex h-12 w-12 items-center justify-center rounded-full bg-white text-gray-600 shadow-lg transition-all hover:text-[#00A870] hover:shadow-xl"
        >
          <ChevronRight className="h-6 w-6" />
        </button>
      </div>

      {/* Dots */}
      <div className="mt-8 flex justify-center gap-2">
        {testimonials.map((_, index) => (
          <button
            key={index}
            onClick={() => setCurrentIndex(index)}
            className={cn(
              'h-2.5 w-2.5 rounded-full transition-all duration-300',
              index === currentIndex ? 'w-8 bg-[#00A870]' : 'bg-gray-300 hover:bg-gray-400'
            )}
          />
        ))}
      </div>
    </div>
  );
}

// =============================================================================
// TESTIMONIALS GRID (ALTERNATIVE)
// =============================================================================

interface TestimonialsGridProps {
  testimonials?: Testimonial[];
  columns?: 2 | 3;
  className?: string;
}

export function TestimonialsGrid({
  testimonials = DEFAULT_TESTIMONIALS.slice(0, 3),
  columns = 3,
  className,
}: TestimonialsGridProps) {
  const columnClasses = {
    2: 'grid-cols-1 md:grid-cols-2',
    3: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
  };

  return (
    <div className={cn(`grid ${columnClasses[columns]} gap-6 md:gap-8`, className)}>
      {testimonials.map((testimonial, index) => (
        <motion.div
          key={testimonial.id}
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ delay: index * 0.1 }}
        >
          <TestimonialCard testimonial={testimonial} />
        </motion.div>
      ))}
    </div>
  );
}

export default TestimonialsCarousel;
