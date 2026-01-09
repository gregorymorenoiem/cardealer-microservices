/**
 * HomePage - Vehicle-only marketplace landing page
 * Main landing page focused on vehicle sales
 *
 * Now uses REAL DATA from /api/homepagesections/homepage API!
 * Sections are configured in database with assigned vehicles.
 * Images use S3 URLs from the backend.
 */

import React, { useRef, useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import {
  FiArrowRight,
  FiSearch,
  FiShield,
  FiMessageCircle,
  FiZap,
  FiChevronLeft,
  FiChevronRight,
  FiStar,
  FiMapPin,
  FiLoader,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { HeroCarousel } from '@/components/organisms';
import { FeaturedListingGrid } from '@/components/molecules';
import { ForYouSection } from '@/components/recommendations/ForYouSection';
import {
  useHomepageSections,
  type HomepageSection,
  type HomepageVehicle,
} from '@/hooks/useHomepageSections';
import { generateListingUrl } from '@/utils/seoSlug';
import type { Vehicle } from '@/data/mockVehicles';
import { useAuth } from '@/hooks/useAuth';

// Note: Vehicle categories removed - single category (vehicles) in first phase

const features = [
  {
    icon: FiSearch,
    title: 'Encuentra tu Vehículo',
    description: 'Filtros avanzados y búsqueda inteligente para encontrar el vehículo perfecto.',
  },
  {
    icon: FiZap,
    title: 'Vende Más Rápido',
    description: 'Publica en minutos y conecta con compradores interesados.',
  },
  {
    icon: FiShield,
    title: 'Compra con Confianza',
    description: 'Todas las transacciones son seguras y protegidas.',
  },
  {
    icon: FiMessageCircle,
    title: 'Contacto Directo',
    description: 'Habla directamente con vendedores sin intermediarios.',
  },
];

// Format price helper
const formatPrice = (price: number, currency = 'USD') => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(price);
};

// =============================================
// NOW USING /api/homepagesections/homepage API
// Sections are configured in database with assigned vehicles
// =============================================

// Transform HomepageVehicle to FeaturedSection format
interface FeaturedListingItem {
  id: string;
  title: string;
  price: number;
  priceLabel?: string;
  image: string;
  category: string;
  location: string;
  rating: number;
  reviews: number;
}

// Transform HomepageVehicle to FeaturedListingItem (for FeaturedSection component)
const transformToFeaturedListing = (
  vehicle: HomepageVehicle,
  sectionName: string
): FeaturedListingItem => {
  // Generate consistent pseudo-random rating based on vehicle ID
  const idHash = vehicle.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const rating = 4.5 + (idHash % 50) / 100; // Range: 4.50 - 4.99
  const reviews = Math.floor((idHash % 150) + 10);

  return {
    id: vehicle.id,
    title: vehicle.name,
    price: vehicle.price,
    image: vehicle.imageUrl,
    category: sectionName,
    location: 'Santo Domingo', // Default location - API doesn't return this yet
    rating: Number(rating.toFixed(1)),
    reviews,
  };
};

// Transform HomepageVehicle to Vehicle format (for HeroCarousel and FeaturedListingGrid)
const transformHomepageVehicleToVehicle = (v: HomepageVehicle): Vehicle => {
  // Map bodyStyle from API to bodyType expected by Vehicle type
  const bodyTypeMap: Record<string, Vehicle['bodyType']> = {
    Sedan: 'Sedan',
    Coupe: 'Coupe',
    SUV: 'SUV',
    Crossover: 'SUV',
    Pickup: 'Truck',
    Wagon: 'Wagon',
    Hatchback: 'Hatchback',
    Van: 'Van',
    Convertible: 'Convertible',
    SportsCar: 'Coupe',
  };

  // Map fuelType from API
  const fuelTypeMap: Record<string, Vehicle['fuelType']> = {
    Gasoline: 'Gasoline',
    Diesel: 'Diesel',
    Electric: 'Electric',
    Hybrid: 'Hybrid',
    PlugInHybrid: 'Plug-in Hybrid',
  };

  // Map transmission from API
  const transmissionMap: Record<string, Vehicle['transmission']> = {
    Automatic: 'Automatic',
    Manual: 'Manual',
    CVT: 'CVT',
    DualClutch: 'Automatic',
  };

  // Generate consistent pseudo-random rating
  const idHash = v.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const sellerRating = parseFloat((4.0 + (idHash % 10) / 10).toFixed(1));

  return {
    id: v.id,
    make: v.make,
    model: v.model,
    year: v.year,
    price: v.price,
    mileage: v.mileage,
    location: 'Santo Domingo',
    images: v.imageUrls.length > 0 ? v.imageUrls : [v.imageUrl],
    isFeatured: v.isPinned,
    isNew: false,
    transmission: transmissionMap[v.transmission] || 'Automatic',
    fuelType: fuelTypeMap[v.fuelType] || 'Gasoline',
    bodyType: bodyTypeMap[v.bodyStyle] || 'Sedan',
    drivetrain: 'FWD',
    engine: `${v.make} Engine`,
    horsepower: 200,
    mpg: { city: 25, highway: 32 },
    color: v.exteriorColor || 'Unknown',
    interiorColor: 'Black',
    vin: '',
    condition: 'Used',
    features: [],
    description: '',
    seller: {
      name: 'Dealer',
      type: 'Dealer',
      rating: Number(sellerRating.toFixed(1)),
      phone: '+1 (555) 000-0000',
    },
    tier: v.isPinned ? 'featured' : 'basic',
  };
};

// Category to color mapping
const getCategoryColor = (accentColor: string): string => {
  // Map from API accentColor to Tailwind color
  const colorMap: Record<string, string> = {
    blue: 'blue',
    emerald: 'emerald',
    amber: 'amber',
    red: 'red',
    green: 'green',
    purple: 'purple',
    pink: 'pink',
  };
  return colorMap[accentColor] || 'blue';
};

// Get Tailwind classes for category colors
const getCategoryClasses = (accentColor: string) => {
  const color = getCategoryColor(accentColor);
  const colorClasses: Record<string, { badge: string; price: string }> = {
    blue: { badge: 'bg-blue-500', price: 'text-blue-600' },
    amber: { badge: 'bg-amber-500', price: 'text-amber-600' },
    emerald: { badge: 'bg-emerald-500', price: 'text-emerald-600' },
    red: { badge: 'bg-red-500', price: 'text-red-600' },
    green: { badge: 'bg-green-500', price: 'text-green-600' },
    purple: { badge: 'bg-purple-500', price: 'text-purple-600' },
    pink: { badge: 'bg-pink-500', price: 'text-pink-600' },
  };
  return colorClasses[color] || colorClasses.blue;
};

// Featured Section Component
interface FeaturedSectionProps {
  title: string;
  subtitle: string;
  listings: Array<{
    id: string;
    title: string;
    price: number;
    priceLabel?: string;
    image: string;
    category: string;
    location: string;
    rating: number;
    reviews: number;
  }>;
  viewAllHref: string;
  accentColor: string;
}

// Helper para obtener las clases de color de los botones de navegación
const getAccentClasses = (color: string) => {
  const colorMap: Record<
    string,
    { border: string; text: string; hoverBg: string; link: string; linkHover: string }
  > = {
    blue: {
      border: 'border-blue-500',
      text: 'text-blue-600',
      hoverBg: 'hover:bg-blue-500',
      link: 'text-blue-600',
      linkHover: 'hover:text-blue-700',
    },
    amber: {
      border: 'border-amber-500',
      text: 'text-amber-600',
      hoverBg: 'hover:bg-amber-500',
      link: 'text-amber-600',
      linkHover: 'hover:text-amber-700',
    },
    emerald: {
      border: 'border-emerald-500',
      text: 'text-emerald-600',
      hoverBg: 'hover:bg-emerald-500',
      link: 'text-emerald-600',
      linkHover: 'hover:text-emerald-700',
    },
    red: {
      border: 'border-red-500',
      text: 'text-red-600',
      hoverBg: 'hover:bg-red-500',
      link: 'text-red-600',
      linkHover: 'hover:text-red-700',
    },
  };
  return colorMap[color] || colorMap.blue;
};

const FeaturedSection: React.FC<FeaturedSectionProps> = ({
  title,
  subtitle,
  listings,
  viewAllHref,
  accentColor,
}) => {
  const scrollRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(true);
  const colorClasses = getAccentClasses(accentColor);

  const checkScroll = () => {
    if (scrollRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollRef.current;
      setCanScrollLeft(scrollLeft > 0);
      setCanScrollRight(scrollLeft < scrollWidth - clientWidth - 10);
    }
  };

  const scroll = (direction: 'left' | 'right') => {
    if (scrollRef.current) {
      scrollRef.current.scrollBy({
        left: direction === 'left' ? -350 : 350,
        behavior: 'smooth',
      });
      setTimeout(checkScroll, 300);
    }
  };

  return (
    <section className="py-6 bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="flex items-end justify-between mb-4">
          <div>
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-1">{title}</h2>
            <p className="text-gray-600">{subtitle}</p>
          </div>
          <div className="flex items-center gap-3">
            {/* Navigation arrows */}
            <button
              onClick={() => scroll('left')}
              disabled={!canScrollLeft}
              className={`p-2 rounded-full border-2 transition-all ${
                canScrollLeft
                  ? `${colorClasses.border} ${colorClasses.text} ${colorClasses.hoverBg} hover:text-white`
                  : 'border-gray-200 text-gray-300 cursor-not-allowed'
              }`}
            >
              <FiChevronLeft className="w-5 h-5" />
            </button>
            <button
              onClick={() => scroll('right')}
              disabled={!canScrollRight}
              className={`p-2 rounded-full border-2 transition-all ${
                canScrollRight
                  ? `${colorClasses.border} ${colorClasses.text} ${colorClasses.hoverBg} hover:text-white`
                  : 'border-gray-200 text-gray-300 cursor-not-allowed'
              }`}
            >
              <FiChevronRight className="w-5 h-5" />
            </button>
            <Link
              to={viewAllHref}
              className={`hidden sm:flex items-center gap-1 ${colorClasses.link} ${colorClasses.linkHover} font-medium`}
            >
              Ver todo
              <FiArrowRight className="w-4 h-4" />
            </Link>
          </div>
        </div>

        {/* Scrollable cards */}
        <div
          ref={scrollRef}
          onScroll={checkScroll}
          className="flex gap-4 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4"
          style={{ scrollSnapType: 'x mandatory' }}
        >
          {listings.map((listing) => {
            const categoryClasses = getCategoryClasses(listing.category);
            const listingUrl = generateListingUrl(listing.id, listing.title); // URL descriptiva SEO-friendly

            return (
              <Link
                key={listing.id}
                to={listingUrl}
                className="flex-shrink-0 w-72 bg-white rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 overflow-hidden group"
                style={{ scrollSnapAlign: 'start' }}
              >
                {/* Image */}
                <div className="relative h-48 overflow-hidden">
                  <img
                    src={listing.image}
                    alt={listing.title}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <div className="absolute top-3 left-3">
                    <span
                      className={`px-3 py-1 ${categoryClasses.badge} text-white text-xs font-medium rounded-full`}
                    >
                      {listing.category}
                    </span>
                  </div>
                </div>

                {/* Content */}
                <div className="p-4">
                  <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2 group-hover:text-blue-600 transition-colors">
                    {listing.title}
                  </h3>

                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-3">
                    <FiMapPin className="w-4 h-4" />
                    {listing.location}
                  </div>

                  <div className="flex items-center justify-between">
                    <p className={`text-xl font-bold ${categoryClasses.price}`}>
                      {formatPrice(listing.price)}
                      {listing.priceLabel && (
                        <span className="text-sm font-normal text-gray-500">
                          {listing.priceLabel}
                        </span>
                      )}
                    </p>
                    <div className="flex items-center gap-1 text-sm">
                      <FiStar className="w-4 h-4 text-amber-400 fill-current" />
                      <span className="font-medium">{listing.rating.toFixed(1)}</span>
                      <span className="text-gray-400">({listing.reviews})</span>
                    </div>
                  </div>
                </div>
              </Link>
            );
          })}
        </div>

        {/* Mobile view all link */}
        <div className="sm:hidden text-center mt-4">
          <Link
            to={viewAllHref}
            className={`inline-flex items-center gap-1 ${colorClasses.link} font-medium`}
          >
            Ver todo
            <FiArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </div>
    </section>
  );
};

const HomePage: React.FC = () => {
  // =============================================
  // REAL DATA FROM /api/homepagesections/homepage
  // Sections and vehicles configured in database
  // =============================================
  const {
    sections,
    isLoading,
    error,
    carousel,
    sedanes,
    suvs,
    camionetas,
    deportivos,
    destacados,
    lujo,
  } = useHomepageSections();

  const { isAuthenticated } = useAuth();

  // Transform carousel vehicles to Vehicle format for HeroCarousel component
  const heroVehicles = useMemo(() => {
    if (!carousel || carousel.vehicles.length === 0) return [];
    return carousel.vehicles.map(transformHomepageVehicleToVehicle);
  }, [carousel]);

  // Transform destacados vehicles to Vehicle format for FeaturedListingGrid component
  const gridVehicles = useMemo(() => {
    if (!destacados || destacados.vehicles.length === 0) {
      // Fallback: use carousel vehicles if no destacados
      if (!carousel || carousel.vehicles.length === 0) return [];
      return carousel.vehicles.map(transformHomepageVehicleToVehicle);
    }
    return destacados.vehicles.map(transformHomepageVehicleToVehicle);
  }, [destacados, carousel]);

  // Transform section vehicles to FeaturedListingItem format
  const transformSectionVehicles = (section: HomepageSection | undefined) => {
    if (!section || section.vehicles.length === 0) return [];
    return section.vehicles.map((v) => transformToFeaturedListing(v, section.name));
  };

  return (
    <MainLayout>
      {/* Hero Carousel */}
      <HeroCarousel vehicles={heroVehicles} autoPlayInterval={5000} showScrollHint={false} />

      {/* Featured Listings Grid */}
      <section className="py-6 bg-gradient-to-b from-white to-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-6">
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-2">
              Vehículos Destacados
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Explora nuestra selección premium de vehículos cuidadosamente verificados
            </p>
          </div>

          <FeaturedListingGrid vehicles={gridVehicles} maxItems={9} />
        </div>
      </section>

      {/* For You Section - Personalized Recommendations (Authenticated Users Only) */}
      {isAuthenticated && (
        <section className="py-6 bg-white">
          <ForYouSection />
        </section>
      )}

      {/* Loading State */}
      {isLoading && (
        <section className="py-12 bg-gray-50">
          <div className="max-w-7xl mx-auto px-4 text-center">
            <FiLoader className="w-12 h-12 mx-auto animate-spin text-blue-600 mb-4" />
            <p className="text-gray-600">Cargando vehículos...</p>
          </div>
        </section>
      )}

      {/* Error State */}
      {error && (
        <section className="py-12 bg-red-50">
          <div className="max-w-7xl mx-auto px-4 text-center">
            <p className="text-red-600 mb-2">Error al cargar vehículos</p>
            <p className="text-sm text-red-500">{error}</p>
          </div>
        </section>
      )}

      {/* =============================================
          SECCIONES DESDE /api/homepagesections/homepage
          Configuradas en base de datos con vehículos asignados
          ============================================= */}

      {/* Sedanes Section */}
      {!isLoading && sedanes && sedanes.vehicles.length > 0 && (
        <FeaturedSection
          title={sedanes.name}
          subtitle={sedanes.subtitle}
          listings={transformSectionVehicles(sedanes)}
          viewAllHref={sedanes.viewAllHref}
          accentColor={sedanes.accentColor}
        />
      )}

      {/* SUVs Section */}
      {!isLoading && suvs && suvs.vehicles.length > 0 && (
        <FeaturedSection
          title={suvs.name}
          subtitle={suvs.subtitle}
          listings={transformSectionVehicles(suvs)}
          viewAllHref={suvs.viewAllHref}
          accentColor={suvs.accentColor}
        />
      )}

      {/* Camionetas Section */}
      {!isLoading && camionetas && camionetas.vehicles.length > 0 && (
        <FeaturedSection
          title={camionetas.name}
          subtitle={camionetas.subtitle}
          listings={transformSectionVehicles(camionetas)}
          viewAllHref={camionetas.viewAllHref}
          accentColor={camionetas.accentColor}
        />
      )}

      {/* Deportivos Section */}
      {!isLoading && deportivos && deportivos.vehicles.length > 0 && (
        <FeaturedSection
          title={deportivos.name}
          subtitle={deportivos.subtitle}
          listings={transformSectionVehicles(deportivos)}
          viewAllHref={deportivos.viewAllHref}
          accentColor={deportivos.accentColor}
        />
      )}

      {/* Destacados Section */}
      {!isLoading && destacados && destacados.vehicles.length > 0 && (
        <FeaturedSection
          title={destacados.name}
          subtitle={destacados.subtitle}
          listings={transformSectionVehicles(destacados)}
          viewAllHref={destacados.viewAllHref}
          accentColor={destacados.accentColor}
        />
      )}

      {/* Lujo Section */}
      {!isLoading && lujo && lujo.vehicles.length > 0 && (
        <FeaturedSection
          title={lujo.name}
          subtitle={lujo.subtitle}
          listings={transformSectionVehicles(lujo)}
          viewAllHref={lujo.viewAllHref}
          accentColor={lujo.accentColor}
        />
      )}

      {/* Features Section */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Todo lo que Necesitas
            </h2>
            <p className="text-gray-600">
              Compra y vende vehículos de manera fácil, rápida y segura
            </p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="text-center p-4 bg-gray-50 rounded-2xl"
              >
                <div className="w-14 h-14 bg-blue-100 rounded-xl flex items-center justify-center mx-auto mb-3">
                  <feature.icon className="w-6 h-6 text-blue-600" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-1">{feature.title}</h3>
                <p className="text-gray-600 text-sm">{feature.description}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default HomePage;
