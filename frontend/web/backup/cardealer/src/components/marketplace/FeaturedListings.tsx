/**
 * FeaturedListings - Hero section showing featured listings from all verticals
 */

import React from 'react';
import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { ListingCard } from './ListingCard';
import type { Listing } from '@/types/marketplace';

interface FeaturedListingsProps {
  listings: Listing[];
  isLoading?: boolean;
  title?: string;
  subtitle?: string;
  viewAllLink?: string;
  className?: string;
}

export const FeaturedListings: React.FC<FeaturedListingsProps> = ({
  listings,
  isLoading = false,
  title = 'Destacados',
  subtitle = 'Los mejores listados seleccionados para ti',
  viewAllLink,
  className = '',
}) => {
  if (isLoading) {
    return (
      <div className={className}>
        <div className="flex justify-between items-end mb-8">
          <div>
            <div className="h-8 w-48 bg-gray-200 rounded animate-pulse" />
            <div className="h-4 w-64 bg-gray-200 rounded animate-pulse mt-2" />
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {Array.from({ length: 4 }).map((_, i) => (
            <FeaturedCardSkeleton key={i} />
          ))}
        </div>
      </div>
    );
  }

  if (listings.length === 0) return null;

  // Take first as hero, rest as grid
  const [heroListing, ...gridListings] = listings;

  return (
    <section className={className}>
      {/* Header */}
      <div className="flex justify-between items-end mb-8">
        <div>
          <motion.h2
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-3xl font-bold text-gray-900"
          >
            {title}
          </motion.h2>
          <motion.p
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
            className="text-gray-500 mt-1"
          >
            {subtitle}
          </motion.p>
        </div>

        {viewAllLink && (
          <Link
            to={viewAllLink}
            className="hidden md:flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium transition-colors"
          >
            Ver todos
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </Link>
        )}
      </div>

      {/* Listings grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Hero listing (large) */}
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
          className="lg:row-span-2"
        >
          <ListingCard listing={heroListing} variant="featured" className="h-full" />
        </motion.div>

        {/* Grid listings */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {gridListings.slice(0, 4).map((listing, i) => (
            <motion.div
              key={listing.id}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: i * 0.1 }}
            >
              <ListingCard listing={listing} />
            </motion.div>
          ))}
        </div>
      </div>

      {/* Mobile view all link */}
      {viewAllLink && (
        <div className="mt-8 md:hidden text-center">
          <Link
            to={viewAllLink}
            className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-full transition-colors"
          >
            Ver todos los destacados
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </Link>
        </div>
      )}
    </section>
  );
};

// Loading skeleton for featured cards
const FeaturedCardSkeleton: React.FC = () => (
  <div className="bg-white rounded-2xl overflow-hidden shadow-sm animate-pulse">
    <div className="aspect-[4/3] bg-gray-200" />
    <div className="p-4 space-y-3">
      <div className="h-5 bg-gray-200 rounded w-3/4" />
      <div className="h-4 bg-gray-200 rounded w-1/2" />
    </div>
  </div>
);

export default FeaturedListings;
