/**
 * HomePage - Main marketplace landing page
 * Multi-vertical marketplace with clean, scalable design
 * Sprint 5: Integrated Featured Listings with HeroCarousel
 * Sprint 5.2: Removed SearchSection, moved to Navbar for space optimization
 */

import React, { useRef, useState, useMemo } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { FiArrowRight, FiSearch, FiShield, FiMessageCircle, FiZap, FiChevronLeft, FiChevronRight, FiStar, FiMapPin, FiChevronDown } from 'react-icons/fi';
import { FaCar, FaHome, FaKey, FaBed } from 'react-icons/fa';
import { HeroCarousel } from '@/components/organisms';
import { FeaturedListingGrid } from '@/components/molecules';
import { mockVehicles } from '@/data/mockVehicles';
import { mixFeaturedAndOrganic } from '@/utils/rankingAlgorithm';

// Search categories for the hero
const searchCategories = [
  { id: 'vehicles', label: 'Vehículos' },
  { id: 'vehicle-rental', label: 'Renta de Vehículos' },
  { id: 'properties', label: 'Propiedades' },
  { id: 'lodging', label: 'Hospedaje' },
];

// Mock data - Vehículos en Venta
const vehiculosListings = [
  {
    id: 'v1',
    title: 'Mercedes-Benz Clase C AMG 2024',
    price: 75000,
    image: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Miami, FL',
    rating: 4.9,
    reviews: 47,
  },
  {
    id: 'v2',
    title: 'BMW Serie 7 Executive Package',
    price: 95000,
    image: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Los Angeles, CA',
    rating: 4.8,
    reviews: 62,
  },
  {
    id: 'v3',
    title: 'Porsche 911 Carrera S',
    price: 135000,
    image: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'New York, NY',
    rating: 4.9,
    reviews: 89,
  },
  {
    id: 'v4',
    title: 'Audi RS7 Sportback 2024',
    price: 128000,
    image: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Dallas, TX',
    rating: 4.7,
    reviews: 31,
  },
  {
    id: 'v5',
    title: 'Tesla Model S Plaid',
    price: 108000,
    image: 'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'San Francisco, CA',
    rating: 4.8,
    reviews: 56,
  },
  {
    id: 'v6',
    title: 'Range Rover Sport HSE',
    price: 89000,
    image: 'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Phoenix, AZ',
    rating: 4.6,
    reviews: 28,
  },
];

// Mock data - Renta de Vehículos
const rentaVehiculosListings = [
  {
    id: 'rv1',
    title: 'BMW X5 - Renta por Día',
    price: 150,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Miami, FL',
    rating: 4.9,
    reviews: 124,
  },
  {
    id: 'rv2',
    title: 'Mercedes GLE Coupe - Renta Semanal',
    price: 850,
    priceLabel: '/semana',
    image: 'https://images.unsplash.com/photo-1563720223185-11003d516935?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Los Angeles, CA',
    rating: 4.8,
    reviews: 89,
  },
  {
    id: 'rv3',
    title: 'Porsche Cayenne - Renta Premium',
    price: 200,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Las Vegas, NV',
    rating: 5.0,
    reviews: 67,
  },
  {
    id: 'rv4',
    title: 'Cadillac Escalade - Eventos',
    price: 280,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Atlanta, GA',
    rating: 4.7,
    reviews: 45,
  },
  {
    id: 'rv5',
    title: 'Tesla Model X - Renta Ecológica',
    price: 175,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'San Diego, CA',
    rating: 4.9,
    reviews: 98,
  },
  {
    id: 'rv6',
    title: 'Range Rover Velar - Lujo',
    price: 190,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1551830820-330a71b99659?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Orlando, FL',
    rating: 4.6,
    reviews: 52,
  },
];

// Mock data - Propiedades en Venta
const propiedadesListings = [
  {
    id: 'p1',
    title: 'Penthouse de Lujo con Vista al Mar',
    price: 1250000,
    image: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Cancún, MX',
    rating: 5.0,
    reviews: 23,
  },
  {
    id: 'p2',
    title: 'Villa Contemporánea con Piscina',
    price: 875000,
    image: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Houston, TX',
    rating: 4.7,
    reviews: 35,
  },
  {
    id: 'p3',
    title: 'Apartamento Moderno Centro',
    price: 450000,
    image: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Chicago, IL',
    rating: 4.6,
    reviews: 41,
  },
  {
    id: 'p4',
    title: 'Casa Colonial con Jardín Amplio',
    price: 680000,
    image: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'San Antonio, TX',
    rating: 4.8,
    reviews: 29,
  },
  {
    id: 'p5',
    title: 'Loft Industrial Renovado',
    price: 395000,
    image: 'https://images.unsplash.com/photo-1600607688969-a5bfcd646154?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Brooklyn, NY',
    rating: 4.5,
    reviews: 18,
  },
  {
    id: 'p6',
    title: 'Mansión con Vista Panorámica',
    price: 2150000,
    image: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Beverly Hills, CA',
    rating: 4.9,
    reviews: 12,
  },
];

// Mock data - Hospedaje
const hospedajeListings = [
  {
    id: 'h1',
    title: 'Suite Premium Frente al Mar',
    price: 250,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Playa del Carmen, MX',
    rating: 4.9,
    reviews: 156,
  },
  {
    id: 'h2',
    title: 'Apartamento Ejecutivo Centro',
    price: 120,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Miami Beach, FL',
    rating: 4.7,
    reviews: 89,
  },
  {
    id: 'h3',
    title: 'Villa Privada con Alberca',
    price: 380,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1602002418082-a4443e081dd1?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Tulum, MX',
    rating: 5.0,
    reviews: 67,
  },
  {
    id: 'h4',
    title: 'Cabaña Rústica en la Montaña',
    price: 95,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1587061949409-02df41d5e562?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Aspen, CO',
    rating: 4.8,
    reviews: 134,
  },
  {
    id: 'h5',
    title: 'Penthouse con Terraza Privada',
    price: 320,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'New York, NY',
    rating: 4.6,
    reviews: 78,
  },
  {
    id: 'h6',
    title: 'Casa de Playa con Jacuzzi',
    price: 275,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Malibu, CA',
    rating: 4.9,
    reviews: 92,
  },
];

// Vertical categories configuration
const verticals = [
  {
    id: 'vehicles',
    name: 'Vehículos',
    description: 'Compra y vende vehículos nuevos y usados',
    icon: FaCar,
    href: '/vehicles',
    gradient: 'from-blue-500 to-blue-600',
    bgLight: 'bg-blue-50',
    textColor: 'text-blue-600',
    image: 'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=400&h=400&fit=crop',
  },
  {
    id: 'vehicle-rental',
    name: 'Renta de Vehículos',
    description: 'Alquila el vehículo perfecto',
    icon: FaKey,
    href: '/vehicle-rental',
    gradient: 'from-amber-500 to-amber-600',
    bgLight: 'bg-amber-50',
    textColor: 'text-amber-600',
    image: 'https://images.unsplash.com/photo-1449965408869-eaa3f722e40d?w=400&h=400&fit=crop',
  },
  {
    id: 'properties',
    name: 'Propiedades',
    description: 'Encuentra tu próximo hogar',
    icon: FaHome,
    href: '/properties',
    gradient: 'from-emerald-500 to-emerald-600',
    bgLight: 'bg-emerald-50',
    textColor: 'text-emerald-600',
    image: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=400&h=400&fit=crop',
  },
  {
    id: 'lodging',
    name: 'Hospedaje',
    description: 'Alojamientos para cada viaje',
    icon: FaBed,
    href: '/lodging',
    gradient: 'from-purple-500 to-purple-600',
    bgLight: 'bg-purple-50',
    textColor: 'text-purple-600',
    image: 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400&h=400&fit=crop',
  },
];

const features = [
  {
    icon: FiSearch,
    title: 'Encuentra lo que Buscas',
    description: 'Filtros avanzados y búsqueda inteligente para encontrar el producto perfecto.',
  },
  {
    icon: FiZap,
    title: 'Vende Más Rápido',
    description: 'Publica en minutos y conecta con compradores interesados en lo que ofreces.',
  },
  {
    icon: FiShield,
    title: 'Compra con Confianza',
    description: 'Todas las transacciones son seguras y protegidas.',
  },
  {
    icon: FiMessageCircle,
    title: 'Contacto Directo',
    description: 'Habla directamente con vendedores o compradores sin intermediarios.',
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

// Category to color mapping
const getCategoryColor = (category: string): string => {
  switch (category) {
    case 'Vehículos':
      return 'blue';
    case 'Renta de Vehículos':
      return 'amber';
    case 'Propiedades':
      return 'emerald';
    case 'Hospedaje':
      return 'purple';
    default:
      return 'blue';
  }
};

// Get Tailwind classes for category colors (to ensure they're included in build)
const getCategoryClasses = (category: string) => {
  const color = getCategoryColor(category);
  const colorClasses: Record<string, { badge: string; price: string }> = {
    blue: { badge: 'bg-blue-500', price: 'text-blue-600' },
    amber: { badge: 'bg-amber-500', price: 'text-amber-600' },
    emerald: { badge: 'bg-emerald-500', price: 'text-emerald-600' },
    purple: { badge: 'bg-purple-500', price: 'text-purple-600' },
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
    purple: {
      border: 'border-purple-500',
      text: 'text-purple-600',
      hoverBg: 'hover:bg-purple-500',
      link: 'text-purple-600',
      linkHover: 'hover:text-purple-700',
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
    <section className="py-12 bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="flex items-end justify-between mb-8">
          <div>
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">{title}</h2>
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
          className="flex gap-6 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4"
          style={{ scrollSnapType: 'x mandatory' }}
        >
          {listings.map((listing) => {
            const categoryClasses = getCategoryClasses(listing.category);
            return (
              <Link
                key={listing.id}
                to={`/listing/${listing.id}`}
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
                      <span className="font-medium">{listing.rating}</span>
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
            className={`inline-flex items-center gap-1 text-${accentColor}-600 font-medium`}
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
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('vehicles');
  const [isCategoryOpen, setIsCategoryOpen] = useState(false);

  // Get featured vehicles for hero carousel (top 5 by ranking)
  const heroVehicles = useMemo(() => {
    return mixFeaturedAndOrganic(mockVehicles, 'home').slice(0, 5);
  }, []);

  // Get featured vehicles for homepage grid (exclude hero vehicles)
  const gridVehicles = useMemo(() => {
    const heroIds = new Set(heroVehicles.map(v => v.id));
    return mockVehicles.filter(v => !heroIds.has(v.id));
  }, [heroVehicles]);

  // Vehicle search states
  const [make, setMake] = useState('');
  const [model, setModel] = useState('');
  const [priceMin, setPriceMin] = useState('');
  const [priceMax, setPriceMax] = useState('');

  // Vehicle rental states
  const [vehicleType, setVehicleType] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [location, setLocation] = useState('');

  // Property states
  const [propertyType, setPropertyType] = useState('');
  const [propertyLocation, setPropertyLocation] = useState('');
  const [propertyPriceMin, setPropertyPriceMin] = useState('');
  const [propertyPriceMax, setPropertyPriceMax] = useState('');
  const [bedrooms, setBedrooms] = useState('');

  // Lodging states
  const [checkIn, setCheckIn] = useState('');
  const [checkOut, setCheckOut] = useState('');
  const [guests, setGuests] = useState('');
  const [lodgingLocation, setLodgingLocation] = useState('');

  const vehicleMakes = ['Toyota', 'Honda', 'Ford', 'Chevrolet', 'BMW', 'Mercedes-Benz', 'Audi', 'Tesla', 'Nissan', 'Mazda'];
  const vehicleTypes = ['SUV', 'Sedán', 'Deportivo', 'Camioneta', 'Compacto', 'Convertible'];
  const propertyTypes = ['Casa', 'Apartamento', 'Villa', 'Condominio', 'Terreno'];

  const handleSearch = () => {
    const params = new URLSearchParams();
    
    switch (selectedCategory) {
      case 'vehicles':
        if (make) params.set('make', make);
        if (model) params.set('model', model);
        if (priceMin) params.set('priceMin', priceMin);
        if (priceMax) params.set('priceMax', priceMax);
        navigate(`/vehicles?${params.toString()}`);
        break;
      
      case 'vehicle-rental':
        if (vehicleType) params.set('type', vehicleType);
        if (startDate) params.set('startDate', startDate);
        if (endDate) params.set('endDate', endDate);
        if (location) params.set('location', location);
        navigate(`/vehicle-rental?${params.toString()}`);
        break;
      
      case 'properties':
        if (propertyType) params.set('type', propertyType);
        if (propertyLocation) params.set('location', propertyLocation);
        if (propertyPriceMin) params.set('priceMin', propertyPriceMin);
        if (propertyPriceMax) params.set('priceMax', propertyPriceMax);
        if (bedrooms) params.set('bedrooms', bedrooms);
        navigate(`/properties?${params.toString()}`);
        break;
      
      case 'lodging':
        if (checkIn) params.set('checkIn', checkIn);
        if (checkOut) params.set('checkOut', checkOut);
        if (guests) params.set('guests', guests);
        if (lodgingLocation) params.set('location', lodgingLocation);
        navigate(`/lodging?${params.toString()}`);
        break;
    }
  };

  return (
    <MainLayout>
      {/* Hero Carousel - Sprint 5.2: 100% Visible, No Search Overlay */}
      <HeroCarousel 
        vehicles={heroVehicles} 
        autoPlayInterval={5000}
        showScrollHint={false}
      />

      {/* Featured Listings Grid - Sprint 5.2: Immediately After Hero */}
      <section className="py-16 bg-gradient-to-b from-white to-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
              Vehículos Destacados
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Explora nuestra selección premium de vehículos cuidadosamente verificados
            </p>
          </div>
          
          <FeaturedListingGrid vehicles={gridVehicles} />
          
          <div className="text-center mt-10">
            <Link
              to="/vehicles"
              className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-xl hover:bg-blue-700 transition-colors font-medium"
            >
              Ver Todos los Vehículos
              <FiArrowRight className="w-5 h-5" />
            </Link>
          </div>
        </div>
      </section>

      {/* Featured of the Week - Mixed from all categories */}
      <FeaturedSection
        title="Destacados de la Semana"
        subtitle="Selección exclusiva de nuestros mejores anuncios"
        listings={[
          ...vehiculosListings.slice(0, 2),
          ...propiedadesListings.slice(0, 2),
          ...rentaVehiculosListings.slice(0, 1),
          ...hospedajeListings.slice(0, 1),
        ]}
        viewAllHref="/browse"
        accentColor="blue"
      />

      {/* Other Category Sections */}
      
      <FeaturedSection
        title="Renta de Vehículos"
        subtitle="Alquila el vehículo perfecto para cualquier ocasión"
        listings={rentaVehiculosListings}
        viewAllHref="/vehicle-rental"
        accentColor="amber"
      />

      <FeaturedSection
        title="Propiedades Destacadas"
        subtitle="Encuentra tu próximo hogar o inversión"
        listings={propiedadesListings}
        viewAllHref="/properties"
        accentColor="emerald"
      />

      <FeaturedSection
        title="Hospedaje Destacado"
        subtitle="Apartamentos, hoteles y alojamientos"
        listings={hospedajeListings}
        viewAllHref="/lodging"
        accentColor="purple"
      />

      {/* Categories Section */}
      <section className="py-12 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-3">
              Explora por Categoría
            </h2>
            <p className="text-gray-600">
              Encuentra exactamente lo que buscas
            </p>
          </div>

          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
            {verticals.map((vertical) => (
              <Link
                key={vertical.id}
                to={vertical.href}
                className="group relative bg-white rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 overflow-hidden"
              >
                {/* Image */}
                <div className="aspect-square relative overflow-hidden">
                  <img
                    src={vertical.image}
                    alt={vertical.name}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-gray-900/80 via-gray-900/20 to-transparent" />
                  
                  {/* Content overlay */}
                  <div className="absolute bottom-0 left-0 right-0 p-4">
                    <div className={`w-12 h-12 ${vertical.bgLight} rounded-xl flex items-center justify-center mb-3`}>
                      <vertical.icon className={`w-6 h-6 ${vertical.textColor}`} />
                    </div>
                    <h3 className="text-lg font-bold text-white mb-1">
                      {vertical.name}
                    </h3>
                    <p className="text-gray-300 text-sm">{vertical.description}</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-12 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-3">
              Todo lo que Necesitas
            </h2>
            <p className="text-gray-600">
              Compra y vende de manera fácil, rápida y segura
            </p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="text-center p-6 bg-gray-50 rounded-2xl"
              >
                <div className="w-14 h-14 bg-blue-100 rounded-xl flex items-center justify-center mx-auto mb-4">
                  <feature.icon className="w-6 h-6 text-blue-600" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
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

      {/* How it Works */}
      <section className="py-12 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-3">
              Cómo Funciona
            </h2>
            <p className="text-gray-600">
              Tres simples pasos para encontrar lo que buscas
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 max-w-4xl mx-auto">
            {[
              { step: '1', title: 'Explora', desc: 'Navega por miles de anuncios en las categorías que te interesan.' },
              { step: '2', title: 'Conecta', desc: 'Contacta directamente con vendedores para resolver tus dudas.' },
              { step: '3', title: 'Disfruta', desc: 'Cierra el trato y disfruta de tu nueva adquisición.' },
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
                
                <div className="relative z-10 w-20 h-20 bg-white rounded-2xl shadow-md flex items-center justify-center mx-auto mb-4">
                  <span className="text-2xl font-bold text-blue-600">{item.step}</span>
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
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

      {/* CTA Section */}
      <section className="py-12 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-gradient-to-br from-blue-600 to-emerald-600 rounded-3xl p-8 lg:p-12 text-center text-white">
            <h2 className="text-2xl lg:text-3xl font-bold mb-4">
              ¿Listo para empezar?
            </h2>
            <p className="text-blue-100 mb-8 max-w-xl mx-auto">
              Publica tu anuncio hoy y conecta con miles de compradores interesados
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/vehicles/sell"
                className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-white text-blue-600 hover:bg-gray-100 font-medium rounded-xl transition-colors"
              >
                <FaCar className="w-5 h-5" />
                Publicar Vehículo
              </Link>
              <Link
                to="/properties/new"
                className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-white/20 hover:bg-white/30 text-white font-medium rounded-xl transition-colors border border-white/30"
              >
                <FaHome className="w-5 h-5" />
                Publicar Propiedad
              </Link>
            </div>
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default HomePage;
