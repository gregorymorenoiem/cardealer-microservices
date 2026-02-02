/**
 * Professional Category Cards
 *
 * Visual category navigation with hover effects and gradients
 * Inspired by premium automotive marketplaces
 */

'use client';

import { useState } from 'react';
import { motion } from 'framer-motion';
import Link from 'next/link';
import Image from 'next/image';
import { ArrowRight, TrendingUp } from 'lucide-react';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

export interface CategoryCard {
  id: string;
  name: string;
  slug: string;
  description: string;
  vehicleCount: number;
  imageUrl: string;
  gradient: string;
  trending?: boolean;
}

interface CategoryCardsProps {
  categories?: CategoryCard[];
  className?: string;
}

// =============================================================================
// DEFAULT CATEGORIES
// =============================================================================

export const DEFAULT_CATEGORIES: CategoryCard[] = [
  {
    id: '1',
    name: 'SUV',
    slug: 'suv',
    description: 'Versatilidad y espacio para toda la familia',
    vehicleCount: 1250,
    imageUrl: '/categories/suv.jpg',
    gradient: 'from-blue-600 to-blue-800',
    trending: true,
  },
  {
    id: '2',
    name: 'Sedán',
    slug: 'sedan',
    description: 'Elegancia y eficiencia para el día a día',
    vehicleCount: 980,
    imageUrl: '/categories/sedan.jpg',
    gradient: 'from-emerald-600 to-emerald-800',
  },
  {
    id: '3',
    name: 'Camioneta',
    slug: 'camioneta',
    description: 'Potencia y capacidad de carga',
    vehicleCount: 650,
    imageUrl: '/categories/pickup.jpg',
    gradient: 'from-amber-600 to-amber-800',
  },
  {
    id: '4',
    name: 'Deportivo',
    slug: 'deportivo',
    description: 'Rendimiento y adrenalina pura',
    vehicleCount: 320,
    imageUrl: '/categories/sports.jpg',
    gradient: 'from-red-600 to-red-800',
  },
  {
    id: '5',
    name: 'Eléctrico',
    slug: 'electrico',
    description: 'El futuro de la movilidad sostenible',
    vehicleCount: 180,
    imageUrl: '/categories/electric.jpg',
    gradient: 'from-cyan-500 to-blue-600',
    trending: true,
  },
  {
    id: '6',
    name: 'Híbrido',
    slug: 'hibrido',
    description: 'Lo mejor de dos mundos',
    vehicleCount: 420,
    imageUrl: '/categories/hybrid.jpg',
    gradient: 'from-green-500 to-emerald-700',
  },
];

// =============================================================================
// SINGLE CATEGORY CARD
// =============================================================================

function SingleCategoryCard({ category, index }: { category: CategoryCard; index: number }) {
  const [isHovered, setIsHovered] = useState(false);
  const [imageError, setImageError] = useState(false);

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, margin: '-50px' }}
      transition={{ delay: index * 0.1, duration: 0.5 }}
    >
      <Link
        href={`/vehiculos?bodyType=${category.slug}`}
        className="group block"
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
      >
        <div className="relative h-72 overflow-hidden rounded-2xl shadow-lg transition-all duration-500 hover:shadow-2xl md:h-80">
          {/* Background Image or Gradient */}
          {!imageError && category.imageUrl ? (
            <Image
              src={category.imageUrl}
              alt={category.name}
              fill
              className={cn(
                'object-cover transition-transform duration-700',
                isHovered ? 'scale-110' : 'scale-100'
              )}
              onError={() => setImageError(true)}
            />
          ) : (
            <div className={cn('absolute inset-0 bg-gradient-to-br', category.gradient)} />
          )}

          {/* Overlay Gradient */}
          <div
            className={cn(
              'absolute inset-0 bg-gradient-to-t from-black/80 via-black/40 to-transparent',
              'transition-opacity duration-500',
              isHovered ? 'opacity-90' : 'opacity-70'
            )}
          />

          {/* Trending Badge */}
          {category.trending && (
            <div className="absolute top-4 left-4">
              <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-orange-500 to-red-500 px-3 py-1.5 text-xs font-semibold tracking-wide text-white shadow-lg"
              >
                <TrendingUp className="h-3.5 w-3.5" />
                Trending
              </motion.div>
            </div>
          )}

          {/* Content */}
          <div className="absolute inset-0 flex flex-col justify-end p-6">
            {/* Category Name */}
            <motion.h3
              className="mb-2 text-2xl leading-tight font-bold tracking-tight text-white md:text-3xl"
              animate={{ y: isHovered ? -8 : 0 }}
              transition={{ duration: 0.3 }}
            >
              {category.name}
            </motion.h3>

            {/* Description - Shows on hover */}
            <motion.p
              className="mb-3 line-clamp-2 text-sm leading-relaxed text-white/85"
              initial={{ opacity: 0, y: 10 }}
              animate={{
                opacity: isHovered ? 1 : 0,
                y: isHovered ? 0 : 10,
              }}
              transition={{ duration: 0.3 }}
            >
              {category.description}
            </motion.p>

            {/* Vehicle Count & CTA */}
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-white/80">
                {category.vehicleCount.toLocaleString()} vehículos
              </span>

              <motion.div
                className="flex items-center gap-2 text-sm font-semibold text-[#00A870]"
                animate={{ x: isHovered ? 5 : 0 }}
                transition={{ duration: 0.3 }}
              >
                <span>Explorar</span>
                <ArrowRight
                  className={cn(
                    'h-4 w-4 transition-transform duration-300',
                    isHovered ? 'translate-x-1' : ''
                  )}
                />
              </motion.div>
            </div>
          </div>

          {/* Decorative Border Glow on Hover */}
          <motion.div
            className="pointer-events-none absolute inset-0 rounded-2xl"
            style={{
              boxShadow: isHovered
                ? 'inset 0 0 0 2px rgba(0, 168, 112, 0.5)'
                : 'inset 0 0 0 0px rgba(0, 168, 112, 0)',
            }}
            transition={{ duration: 0.3 }}
          />
        </div>
      </Link>
    </motion.div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function CategoryCards({ categories = DEFAULT_CATEGORIES, className }: CategoryCardsProps) {
  return (
    <div className={cn('grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3', className)}>
      {categories.map((category, index) => (
        <SingleCategoryCard key={category.id} category={category} index={index} />
      ))}
    </div>
  );
}

// =============================================================================
// FEATURED CATEGORY (LARGER)
// =============================================================================

interface FeaturedCategoryProps {
  category: CategoryCard;
  className?: string;
}

export function FeaturedCategory({ category, className }: FeaturedCategoryProps) {
  const [isHovered, setIsHovered] = useState(false);
  const [imageError, setImageError] = useState(false);

  return (
    <Link
      href={`/vehiculos?bodyType=${category.slug}`}
      className={cn('group block', className)}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div className="relative h-96 overflow-hidden rounded-3xl shadow-xl transition-all duration-500 hover:shadow-2xl md:h-[500px]">
        {/* Background */}
        {!imageError && category.imageUrl ? (
          <Image
            src={category.imageUrl}
            alt={category.name}
            fill
            className={cn(
              'object-cover transition-transform duration-1000',
              isHovered ? 'scale-105' : 'scale-100'
            )}
            onError={() => setImageError(true)}
          />
        ) : (
          <div className={cn('absolute inset-0 bg-gradient-to-br', category.gradient)} />
        )}

        {/* Dark Overlay */}
        <div className="absolute inset-0 bg-gradient-to-r from-black/70 via-black/40 to-transparent" />

        {/* Content */}
        <div className="absolute inset-0 flex max-w-xl flex-col justify-center p-8 md:p-12">
          {category.trending && (
            <div className="mb-4 flex w-fit items-center gap-1.5 rounded-full bg-gradient-to-r from-orange-500 to-red-500 px-3 py-1.5 text-xs font-semibold text-white shadow-lg">
              <TrendingUp className="h-3.5 w-3.5" />
              Lo más buscado
            </div>
          )}

          <h2 className="mb-4 text-4xl font-bold text-white md:text-5xl lg:text-6xl">
            {category.name}
          </h2>

          <p className="mb-6 text-lg text-white/80">{category.description}</p>

          <div className="flex items-center gap-4">
            <motion.button
              className="flex items-center gap-2 rounded-xl bg-[#00A870] px-6 py-3 font-semibold text-white shadow-lg shadow-[#00A870]/30 transition-all hover:bg-[#009663]"
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
            >
              Explorar {category.vehicleCount.toLocaleString()} vehículos
              <ArrowRight className="h-5 w-5" />
            </motion.button>
          </div>
        </div>
      </div>
    </Link>
  );
}

export default CategoryCards;
