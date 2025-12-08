/**
 * HomePage Component - Exact Copy from Original (Cars Only)
 * 
 * Architecture Philosophy: "mostrando vehiculo porque eso es dinero"
 * - Maximum vehicle density = Maximum revenue opportunity
 * - Uses EXACT components from original: HeroCarousel, FeaturedListingGrid
 * - Removed all non-vehicle sections (rentals, properties, lodging)
 * - Amazon-style spacing (py-6, gap-4, mb-4) for optimal content density
 * - 40% featured ratio enforced by rankingAlgorithm
 */

import React, { useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { FiArrowRight, FiSearch, FiShield, FiMessageCircle, FiZap, FiChevronLeft, FiChevronRight, FiStar, FiMapPin } from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { HeroCarousel } from '@/components/organisms';
import { FeaturedListingGrid } from '@/components/molecules';
import { mockVehicles } from '@/data/mockVehicles';
import type { Vehicle } from '@/data/mockVehicles';
import { mixFeaturedAndOrganic } from '@/utils/rankingAlgorithm';

const features = [
  {
    icon: FiSearch,
    title: 'Encuentra tu Auto Ideal',
    description: 'Filtros avanzados para encontrar exactamente lo que buscas.',
  },
  {
    icon: FiZap,
    title: 'Vende Rápido',
    description: 'Publica en minutos y conecta con compradores verificados.',
  },
  {
    icon: FiShield,
    title: 'Compra con Confianza',
    description: 'Todos los vehículos verificados con historial completo.',
  },
  {
    icon: FiMessageCircle,
    title: 'Contacto Directo',
    description: 'Comunícate directamente con vendedores sin intermediarios.',
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

// Featured Section Component - Scrollable Horizontal Carousel
interface FeaturedSectionProps {
  title: string;
  subtitle: string;
  vehicles: Vehicle[];
  viewAllHref: string;
}

const FeaturedSection: React.FC<FeaturedSectionProps> = ({ title, subtitle, vehicles, viewAllHref }) => {
  const scrollRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(true);

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
                  ? 'border-blue-500 text-blue-600 hover:bg-blue-500 hover:text-white'
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
                  ? 'border-blue-500 text-blue-600 hover:bg-blue-500 hover:text-white'
                  : 'border-gray-200 text-gray-300 cursor-not-allowed'
              }`}
            >
              <FiChevronRight className="w-5 h-5" />
            </button>
            <Link
              to={viewAllHref}
              className="hidden sm:flex items-center gap-1 text-blue-600 hover:text-blue-700 font-medium"
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
          {vehicles.map((vehicle) => (
            <Link
              key={vehicle.id}
              to={`/vehicles/${vehicle.id}`}
              className="flex-shrink-0 w-72 bg-white rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 overflow-hidden group"
              style={{ scrollSnapAlign: 'start' }}
            >
              {/* Image */}
              <div className="relative h-48 overflow-hidden">
                <img
                  src={vehicle.images[0]}
                  alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
                  className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                />
                {vehicle.tier && (
                  <div className="absolute top-3 left-3">
                    <span className={`px-3 py-1 ${
                      vehicle.tier === 'enterprise' ? 'bg-purple-500' :
                      vehicle.tier === 'premium' ? 'bg-blue-500' :
                      vehicle.tier === 'featured' ? 'bg-amber-500' :
                      'bg-gray-500'
                    } text-white text-xs font-medium rounded-full`}>
                      {vehicle.featuredBadge || vehicle.tier}
                    </span>
                  </div>
                )}
              </div>

              {/* Content */}
              <div className="p-4">
                <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2 group-hover:text-blue-600 transition-colors">
                  {vehicle.year} {vehicle.make} {vehicle.model}
                </h3>

                <div className="flex items-center gap-2 text-sm text-gray-500 mb-3">
                  <FiMapPin className="w-4 h-4" />
                  {vehicle.location}
                </div>

                <div className="flex items-center justify-between">
                  <p className="text-xl font-bold text-blue-600">
                    {formatPrice(vehicle.price)}
                  </p>
                  <div className="flex items-center gap-1 text-sm">
                    <FiStar className="w-4 h-4 text-amber-400 fill-current" />
                    <span className="font-medium">{vehicle.seller.rating}</span>
                    <span className="text-gray-400">({Math.floor(Math.random() * 50) + 10})</span>
                  </div>
                </div>
              </div>
            </Link>
          ))}
        </div>

        {/* Mobile view all link */}
        <div className="sm:hidden text-center mt-4">
          <Link
            to={viewAllHref}
            className="inline-flex items-center gap-1 text-blue-600 font-medium"
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
  // Get featured vehicles for hero carousel (top 5 by ranking)
  const heroVehicles = useMemo(() => {
    return mixFeaturedAndOrganic(mockVehicles, 'home').slice(0, 5);
  }, []);

  // Get featured vehicles for top grid section (6 vehicles, exclude hero)
  const topFeaturedGrid = useMemo(() => {
    const heroIds = new Set(heroVehicles.map(v => v.id));
    return mockVehicles
      .filter(v => !heroIds.has(v.id))
      .slice(0, 6);
  }, [heroVehicles]);

  // Get vehicles for "Destacados de la Semana" scrollable (10 vehicles)
  const weeklyFeatured = useMemo(() => {
    const excludeIds = new Set([...heroVehicles.map(v => v.id), ...topFeaturedGrid.map(v => v.id)]);
    return mockVehicles
      .filter(v => !excludeIds.has(v.id))
      .slice(0, 10);
  }, [heroVehicles, topFeaturedGrid]);

  // Get vehicles for "Ofertas del Día" scrollable (10 vehicles)
  const dailyDeals = useMemo(() => {
    const excludeIds = new Set([
      ...heroVehicles.map(v => v.id),
      ...topFeaturedGrid.map(v => v.id),
      ...weeklyFeatured.map(v => v.id)
    ]);
    return mockVehicles
      .filter(v => !excludeIds.has(v.id))
      .slice(0, 10);
  }, [heroVehicles, topFeaturedGrid, weeklyFeatured]);

  // Get vehicles for "SUVs y Camionetas" scrollable (10 vehicles)
  const suvTrucks = useMemo(() => {
    const excludeIds = new Set([
      ...heroVehicles.map(v => v.id),
      ...topFeaturedGrid.map(v => v.id),
      ...weeklyFeatured.map(v => v.id),
      ...dailyDeals.map(v => v.id)
    ]);
    return mockVehicles
      .filter(v => !excludeIds.has(v.id) && (v.bodyType === 'SUV' || v.bodyType === 'Truck'))
      .slice(0, 10);
  }, [heroVehicles, topFeaturedGrid, weeklyFeatured, dailyDeals]);

  // Get premium vehicles for premium section (10 vehicles)
  const premiumVehicles = useMemo(() => {
    const excludeIds = new Set([
      ...heroVehicles.map(v => v.id),
      ...topFeaturedGrid.map(v => v.id),
      ...weeklyFeatured.map(v => v.id),
      ...dailyDeals.map(v => v.id),
      ...suvTrucks.map(v => v.id)
    ]);
    return mockVehicles
      .filter(v => !excludeIds.has(v.id) && (v.tier === 'enterprise' || v.tier === 'premium'))
      .slice(0, 10);
  }, [heroVehicles, topFeaturedGrid, weeklyFeatured, dailyDeals, suvTrucks]);

  // Get vehicles for "Eléctricos e Híbridos" scrollable (10 vehicles)
  const electricHybrid = useMemo(() => {
    const excludeIds = new Set([
      ...heroVehicles.map(v => v.id),
      ...topFeaturedGrid.map(v => v.id),
      ...weeklyFeatured.map(v => v.id),
      ...dailyDeals.map(v => v.id),
      ...suvTrucks.map(v => v.id),
      ...premiumVehicles.map(v => v.id)
    ]);
    return mockVehicles
      .filter(v => !excludeIds.has(v.id) && (v.fuelType === 'Electric' || v.fuelType === 'Hybrid' || v.fuelType === 'Plug-in Hybrid'))
      .slice(0, 10);
  }, [heroVehicles, topFeaturedGrid, weeklyFeatured, dailyDeals, suvTrucks, premiumVehicles]);

  // Get vehicles for "Recién Agregados" scrollable (10 vehicles, newest first)
  const recentlyAdded = useMemo(() => {
    const excludeIds = new Set([
      ...heroVehicles.map(v => v.id),
      ...topFeaturedGrid.map(v => v.id),
      ...weeklyFeatured.map(v => v.id),
      ...dailyDeals.map(v => v.id),
      ...suvTrucks.map(v => v.id),
      ...premiumVehicles.map(v => v.id),
      ...electricHybrid.map(v => v.id)
    ]);
    return mockVehicles
      .filter(v => !excludeIds.has(v.id))
      .slice(0, 10);
  }, [heroVehicles, topFeaturedGrid, weeklyFeatured, dailyDeals, suvTrucks, premiumVehicles, electricHybrid]);

  return (
    <MainLayout>
      {/* Hero Carousel - Full Screen, No Search Overlay */}
      <HeroCarousel 
        vehicles={heroVehicles} 
        autoPlayInterval={5000}
        showScrollHint={false}
      />

      {/* Vehículos Destacados - Top 6 vehicles in grid (eso es dinero!) */}
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
          
          <FeaturedListingGrid vehicles={topFeaturedGrid} columns={3} />
        </div>
      </section>

      {/* Destacados de la Semana - Scrollable Section with 10 vehicles */}
      <FeaturedSection
        title="Destacados de la Semana"
        subtitle="Selección especial de los mejores vehículos disponibles"
        vehicles={weeklyFeatured}
        viewAllHref="/vehicles"
      />

      {/* Ofertas del Día - Scrollable Section with 10 vehicles */}
      <FeaturedSection
        title="Ofertas del Día"
        subtitle="Aprovecha estas oportunidades especiales por tiempo limitado"
        vehicles={dailyDeals}
        viewAllHref="/vehicles?deals=daily"
      />

      {/* SUVs y Camionetas - Scrollable Section with 10 vehicles */}
      {suvTrucks.length > 0 && (
        <FeaturedSection
          title="SUVs y Camionetas"
          subtitle="Vehículos ideales para familias y aventuras"
          vehicles={suvTrucks}
          viewAllHref="/vehicles?bodyType=suv,truck"
        />
      )}

      {/* Premium Vehicles Section - 10 vehicles scrollable */}
      {premiumVehicles.length > 0 && (
        <FeaturedSection
          title="Vehículos Premium"
          subtitle="Selección exclusiva de vehículos de alta gama"
          vehicles={premiumVehicles}
          viewAllHref="/vehicles?tier=premium"
        />
      )}

      {/* Eléctricos e Híbridos - Scrollable Section with 10 vehicles */}
      {electricHybrid.length > 0 && (
        <FeaturedSection
          title="Eléctricos e Híbridos"
          subtitle="Vehículos eficientes y amigables con el medio ambiente"
          vehicles={electricHybrid}
          viewAllHref="/vehicles?fuelType=electric,hybrid"
        />
      )}

      {/* Recién Agregados - Scrollable Section with 10 vehicles */}
      <FeaturedSection
        title="Recién Agregados"
        subtitle="Los vehículos más recientes en nuestra plataforma"
        vehicles={recentlyAdded}
        viewAllHref="/vehicles?sort=newest"
      />

      {/* Features Section - Compact, Amazon-style spacing */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Por qué Elegirnos
            </h2>
            <p className="text-gray-600">
              La mejor plataforma para comprar y vender vehículos
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

      {/* How it Works - Compact Version */}
      <section className="py-6 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Cómo Funciona
            </h2>
            <p className="text-gray-600">
              Tres simples pasos para encontrar tu vehículo ideal
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 max-w-4xl mx-auto">
            {[
              { step: '1', title: 'Explora', desc: 'Navega por miles de vehículos en nuestro catálogo.' },
              { step: '2', title: 'Conecta', desc: 'Contacta directamente con vendedores para resolver dudas.' },
              { step: '3', title: 'Disfruta', desc: 'Cierra el trato y disfruta tu nuevo vehículo.' },
            ].map((item, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="relative text-center"
              >
                {index < 2 && (
                  <div className="hidden md:block absolute top-10 left-[60%] w-[80%] h-0.5 bg-gray-300" />
                )}
                
                <div className="relative z-10 w-20 h-20 bg-white rounded-2xl shadow-md flex items-center justify-center mx-auto mb-3">
                  <span className="text-2xl font-bold text-blue-600">{item.step}</span>
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-1">
                  {item.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {item.desc}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section - Compact */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-gradient-to-br from-blue-600 to-blue-800 rounded-3xl p-6 lg:p-8 text-center text-white">
            <h2 className="text-2xl lg:text-3xl font-bold mb-3">
              ¿Listo para Vender tu Vehículo?
            </h2>
            <p className="text-blue-100 mb-6 max-w-xl mx-auto">
              Publica tu anuncio hoy y conecta con miles de compradores interesados
            </p>
            <Link
              to="/vehicles/sell"
              className="inline-flex items-center justify-center gap-2 px-8 py-3 bg-white text-blue-600 hover:bg-gray-100 font-medium rounded-xl transition-colors"
            >
              <FaCar className="w-5 h-5" />
              Publicar mi Vehículo
              <FiArrowRight className="w-5 h-5" />
            </Link>
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default HomePage;
