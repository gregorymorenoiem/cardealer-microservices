/**
 * VehiclesOnlyHomePage - Vehicle-only marketplace landing page
 * Same layout as HomePage but all sections focus on vehicle sales
 * 
 * Now uses REAL DATA from VehiclesSaleService API!
 * Images are stored as photoIds in the database and transformed to S3 URLs.
 */

import React, { useRef, useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { FiArrowRight, FiSearch, FiShield, FiMessageCircle, FiZap, FiChevronLeft, FiChevronRight, FiStar, FiMapPin, FiLoader } from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { HeroCarousel } from '@/components/organisms';
import { FeaturedListingGrid } from '@/components/molecules';
import { mockVehicles } from '@/data/mockVehicles';
import { mixFeaturedAndOrganic } from '@/utils/rankingAlgorithm';
import { useVehiclesSaleList, type VehicleListing } from '@/hooks/useVehiclesSale';
import { generateListingUrl } from '@/utils/seoSlug';

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
// MOCK DATA REMOVED - Now using REAL DATA from VehiclesSaleService API
// The API returns vehicles with BodyStyle: Sedan, SUV, Truck, Coupe
// These are mapped to: Sedanes, SUVs, Camionetas, Deportivos
// =============================================

// Transform VehicleListing from API to FeaturedSection format
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

const transformToFeaturedListing = (vehicle: VehicleListing): FeaturedListingItem => {
  // Map bodyStyle to category for display
  const categoryMap: Record<string, string> = {
    'Sedan': 'Sedanes',
    'Coupe': 'Sedanes',
    'SUV': 'SUVs',
    'Crossover': 'SUVs',
    'Wagon': 'SUVs',
    'Pickup': 'Camionetas',  // Fixed: was 'Truck'
    'SportsCar': 'Deportivos',  // Fixed: was 'Sports'
  };
  
  // Electric/Hybrid vehicles get special category
  let category = categoryMap[vehicle.bodyStyle] || 'Otros';
  if (vehicle.fuelType === 'Electric' || vehicle.fuelType === 'Hybrid') {
    category = 'Eléctricos';
  }
  if (vehicle.price >= 80000) {
    category = 'Lujo';
  }

  // Generate consistent pseudo-random rating based on vehicle ID (so it doesn't change on re-render)
  const idHash = vehicle.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const rating = 4.5 + (idHash % 50) / 100; // Range: 4.50 - 4.99
  const reviews = Math.max(10, Math.floor(vehicle.viewCount / 5) || Math.floor((idHash % 150) + 10));

  return {
    id: vehicle.id,
    title: vehicle.title,
    price: vehicle.price,
    image: vehicle.primaryImage,
    category,
    location: vehicle.location,
    rating: Number(rating.toFixed(1)), // Format to 1 decimal place
    reviews,
  };
};

// Category to color mapping
const getCategoryColor = (category: string): string => {
  switch (category) {
    case 'Sedanes':
      return 'blue';
    case 'SUVs':
      return 'emerald';
    case 'Camionetas':
      return 'amber';
    case 'Deportivos':
    case 'Lujo':
      return 'red';
    case 'Eléctricos':
      return 'green';
    default:
      return 'blue';
  }
};

// Transform VehicleListing from API to Vehicle format (for FeaturedListingGrid/HeroCarousel)
// This allows using real API data with existing components that expect mock Vehicle type
import type { Vehicle } from '@/data/mockVehicles';

const transformApiVehicleToVehicle = (v: VehicleListing): Vehicle => {
  // Map bodyStyle from API to bodyType expected by Vehicle type
  const bodyTypeMap: Record<string, Vehicle['bodyType']> = {
    'Sedan': 'Sedan',
    'Coupe': 'Coupe',
    'SUV': 'SUV',
    'Crossover': 'SUV',
    'Pickup': 'Truck',
    'Wagon': 'Wagon',
    'Hatchback': 'Hatchback',
    'Van': 'Van',
    'Convertible': 'Convertible',
    'SportsCar': 'Coupe',
  };

  // Map fuelType from API
  const fuelTypeMap: Record<string, Vehicle['fuelType']> = {
    'Gasoline': 'Gasoline',
    'Diesel': 'Diesel',
    'Electric': 'Electric',
    'Hybrid': 'Hybrid',
    'PlugInHybrid': 'Plug-in Hybrid',
  };

  // Map transmission from API
  const transmissionMap: Record<string, Vehicle['transmission']> = {
    'Automatic': 'Automatic',
    'Manual': 'Manual',
    'CVT': 'CVT',
  };

  // Map condition from API
  const conditionMap: Record<string, Vehicle['condition']> = {
    'New': 'New',
    'Used': 'Used',
    'CertifiedPreOwned': 'Certified Pre-Owned',
  };

  // Generate consistent pseudo-random rating based on vehicle ID (one digit, one decimal: 4.0 - 4.9)
  const idHash = v.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const sellerRating = parseFloat((4.0 + (idHash % 10) / 10).toFixed(1)); // Range: 4.0, 4.1, 4.2, ..., 4.9

  return {
    id: v.id,
    make: v.make,
    model: v.model,
    year: v.year,
    price: v.price,
    mileage: v.mileage,
    location: v.location,
    images: v.images.length > 0 ? v.images : [v.primaryImage],
    isFeatured: v.isFeatured,
    isNew: v.condition === 'New',
    transmission: transmissionMap[v.transmission] || 'Automatic',
    fuelType: fuelTypeMap[v.fuelType] || 'Gasoline',
    bodyType: bodyTypeMap[v.bodyStyle] || 'Sedan',
    drivetrain: 'FWD', // Default - API doesn't have driveType yet
    engine: `${v.make} Engine`, // Default
    horsepower: 200, // Default
    mpg: { city: 25, highway: 32 }, // Default
    color: v.exteriorColor || 'Unknown',
    interiorColor: 'Black', // Default
    vin: '', // Not exposed in listing
    condition: conditionMap[v.condition] || 'Used',
    features: [], // Not exposed in listing
    description: v.description,
    seller: {
      name: v.sellerName || 'Dealer',
      type: 'Dealer',
      rating: Number(sellerRating.toFixed(1)),
      phone: '+1 (555) 000-0000',
    },
    // Set tier for featured vehicles
    tier: v.isFeatured ? 'featured' : 'basic',
  };
};

// Get Tailwind classes for category colors
const getCategoryClasses = (category: string) => {
  const color = getCategoryColor(category);
  const colorClasses: Record<string, { badge: string; price: string }> = {
    blue: { badge: 'bg-blue-500', price: 'text-blue-600' },
    amber: { badge: 'bg-amber-500', price: 'text-amber-600' },
    emerald: { badge: 'bg-emerald-500', price: 'text-emerald-600' },
    red: { badge: 'bg-red-500', price: 'text-red-600' },
    green: { badge: 'bg-green-500', price: 'text-green-600' },
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
  const colorMap: Record<string, { border: string; text: string; hoverBg: string; link: string; linkHover: string }> = {
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

const FeaturedSection: React.FC<FeaturedSectionProps> = ({ title, subtitle, listings, viewAllHref, accentColor }) => {
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
                    <span className={`px-3 py-1 ${categoryClasses.badge} text-white text-xs font-medium rounded-full`}>
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
                      {listing.priceLabel && <span className="text-sm font-normal text-gray-500">{listing.priceLabel}</span>}
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

const VehiclesOnlyHomePage: React.FC = () => {
  // =============================================
  // REAL DATA FROM API - VehiclesSaleService
  // Fetches 50 vehicles and organizes by BodyStyle
  // =============================================
  const { vehicles: apiVehicles, isLoading, error } = useVehiclesSaleList(1, 50);

  // Transform API vehicles into section-specific arrays (organized by BodyStyle)
  const { sedanesListings, suvsListings, trucksListings, deportivosListings, apiLuxuryListings, apiFeaturedListings } = useMemo(() => {
    if (!apiVehicles || apiVehicles.length === 0) {
      return {
        sedanesListings: [],
        suvsListings: [],
        trucksListings: [],
        deportivosListings: [],
        apiLuxuryListings: [],
        apiFeaturedListings: [],
      };
    }

    const transformed = apiVehicles.map(transformToFeaturedListing);

    // Filter by body style / category
    const sedanes = apiVehicles
      .filter(v => v.bodyStyle === 'Sedan' || v.bodyStyle === 'Coupe')
      .map(transformToFeaturedListing)
      .map(v => ({ ...v, category: 'Sedanes' }));
    
    const suvs = apiVehicles
      .filter(v => v.bodyStyle === 'SUV' || v.bodyStyle === 'Crossover')
      .map(transformToFeaturedListing)
      .map(v => ({ ...v, category: 'SUVs' }));
    
    const trucks = apiVehicles
      .filter(v => v.bodyStyle === 'Pickup')  // Changed from 'Truck' to match backend enum
      .map(transformToFeaturedListing)
      .map(v => ({ ...v, category: 'Camionetas' }));
    
    // Deportivos - Coupe bodyStyle with higher prices or sports-specific models
    const deportivos = apiVehicles
      .filter(v => (v.bodyStyle === 'Coupe' && v.price >= 50000) || 
                   v.bodyStyle === 'SportsCar' ||  // Changed from 'Sports' to match backend enum
                   ['Porsche', 'Ferrari', 'Lamborghini', 'McLaren', 'Aston Martin'].includes(v.make) ||
                   ['911', 'M4', 'AMG GT', 'RS7', 'Corvette', 'Mustang', 'Challenger', 'GT-R', 'LC 500', 'Supra'].includes(v.model))
      .map(transformToFeaturedListing)
      .map(v => ({ ...v, category: 'Deportivos' }));
    
    const luxury = transformed.filter(v => 
      v.category === 'Lujo' || v.category === 'Deportivos' || v.price >= 80000
    ).slice(0, 10);

    // Featured = mix of all categories
    const featured = [
      ...sedanes.slice(0, 2),
      ...suvs.slice(0, 2),
      ...trucks.slice(0, 2),
      ...deportivos.slice(0, 2),
    ].slice(0, 8);

    return {
      sedanesListings: sedanes.length > 0 ? sedanes : [],
      suvsListings: suvs.length > 0 ? suvs : [],
      trucksListings: trucks.length > 0 ? trucks : [],
      deportivosListings: deportivos.length > 0 ? deportivos : [],
      apiLuxuryListings: luxury,
      apiFeaturedListings: featured.length > 0 ? featured : transformed.slice(0, 8),
    };
  }, [apiVehicles]);

  // Transform API vehicles to Vehicle format for FeaturedListingGrid and HeroCarousel
  const apiVehiclesAsVehicle = useMemo(() => {
    if (!apiVehicles || apiVehicles.length === 0) return [];
    return apiVehicles.map(transformApiVehicleToVehicle);
  }, [apiVehicles]);

  // Get featured vehicles for hero carousel (top 5)
  // Uses API data now! Falls back to mockVehicles only if API fails
  const heroVehicles = useMemo(() => {
    if (apiVehiclesAsVehicle.length > 0) {
      return mixFeaturedAndOrganic(apiVehiclesAsVehicle, 'home').slice(0, 5);
    }
    // Fallback to mockVehicles if API not loaded yet
    return mixFeaturedAndOrganic(mockVehicles, 'home').slice(0, 5);
  }, [apiVehiclesAsVehicle]);

  // Get featured vehicles for homepage grid (exclude hero vehicles)
  // Uses API data now! Falls back to mockVehicles only if API fails
  const gridVehicles = useMemo(() => {
    const heroIds = new Set(heroVehicles.map(v => v.id));
    if (apiVehiclesAsVehicle.length > 0) {
      return apiVehiclesAsVehicle.filter(v => !heroIds.has(v.id));
    }
    // Fallback to mockVehicles if API not loaded yet
    return mockVehicles.filter(v => !heroIds.has(v.id));
  }, [heroVehicles, apiVehiclesAsVehicle]);

  return (
    <MainLayout>
      {/* Hero Carousel */}
      <HeroCarousel 
        vehicles={heroVehicles} 
        autoPlayInterval={5000}
        showScrollHint={false}
      />

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
          SECCIONES CON REAL DATA - Organizados por BodyStyle
          ============================================= */}
      
      {/* Sedanes Section - from API (BodyStyle: Sedan) */}
      {!isLoading && sedanesListings.length > 0 && (
        <FeaturedSection
          title="Sedanes"
          subtitle="Elegancia y confort para el día a día"
          listings={sedanesListings}
          viewAllHref="/vehicles?bodyType=sedan"
          accentColor="blue"
        />
      )}

      {/* SUVs Section - from API (BodyStyle: SUV) */}
      {!isLoading && suvsListings.length > 0 && (
        <FeaturedSection
          title="SUVs"
          subtitle="Espacio, confort y versatilidad para toda la familia"
          listings={suvsListings}
          viewAllHref="/vehicles?bodyType=suv"
          accentColor="emerald"
        />
      )}

      {/* Camionetas Section - from API (BodyStyle: Truck) */}
      {!isLoading && trucksListings.length > 0 && (
        <FeaturedSection
          title="Camionetas"
          subtitle="Potencia y capacidad para trabajo y aventura"
          listings={trucksListings}
          viewAllHref="/vehicles?bodyType=truck"
          accentColor="amber"
        />
      )}

      {/* Deportivos Section - from API (BodyStyle: Coupe with high price or sports models) */}
      {!isLoading && deportivosListings.length > 0 && (
        <FeaturedSection
          title="Deportivos"
          subtitle="Velocidad, adrenalina y diseño espectacular"
          listings={deportivosListings}
          viewAllHref="/vehicles?bodyType=coupe"
          accentColor="red"
        />
      )}

      {/* Featured of the Week - REAL DATA from API (opcional, si hay datos) */}
      {!isLoading && apiFeaturedListings.length > 0 && (
        <FeaturedSection
          title="Destacados de la Semana (API)"
          subtitle="Vehículos reales de la base de datos"
          listings={apiFeaturedListings}
          viewAllHref="/vehicles?sort=year-desc"
          accentColor="purple"
        />
      )}

      {/* Sports & Luxury Section - REAL DATA from API (opcional) */}
      {!isLoading && apiLuxuryListings.length > 0 && (
        <FeaturedSection
          title="Lujo (API)"
          subtitle="Experiencias de conducción extraordinarias"
          listings={apiLuxuryListings}
          viewAllHref="/vehicles?bodyType=coupe&sort=year-desc"
          accentColor="pink"
        />
      )}

      {/* Categories Section removed - single category (vehicles) in first phase */}

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
                <h3 className="text-lg font-semibold text-gray-900 mb-1">
                  {feature.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {feature.description}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default VehiclesOnlyHomePage;
