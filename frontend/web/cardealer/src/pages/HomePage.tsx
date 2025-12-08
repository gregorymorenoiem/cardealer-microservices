/**
 * HomePage - Car Dealer Landing Page
 * Simplified design focused exclusively on vehicle sales
 */

import React, { useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { FiArrowRight, FiSearch, FiShield, FiMessageCircle, FiZap, FiChevronLeft, FiChevronRight } from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { mockVehicles } from '@/data/mockVehicles';
import FeaturedListingCard from '@/components/molecules/FeaturedListingCard';

// Popular vehicle types
const vehicleTypes = [
  {
    id: 'sedan',
    name: 'Sedán',
    image: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400&h=300&fit=crop',
    count: '245 vehículos',
  },
  {
    id: 'suv',
    name: 'SUV',
    image: 'https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=400&h=300&fit=crop',
    count: '312 vehículos',
  },
  {
    id: 'truck',
    name: 'Camionetas',
    image: 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=400&h=300&fit=crop',
    count: '189 vehículos',
  },
  {
    id: 'sports',
    name: 'Deportivos',
    image: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400&h=300&fit=crop',
    count: '98 vehículos',
  },
];

const features = [
  {
    icon: FiSearch,
    title: 'Encuentra tu Auto Ideal',
    description: 'Filtros avanzados para buscar por marca, modelo, precio y más.',
  },
  {
    icon: FiZap,
    title: 'Vende Rápido',
    description: 'Publica tu vehículo en minutos y conecta con compradores.',
  },
  {
    icon: FiShield,
    title: 'Compra Segura',
    description: 'Todas las transacciones están protegidas y verificadas.',
  },
  {
    icon: FiMessageCircle,
    title: 'Contacto Directo',
    description: 'Comunícate directamente con vendedores sin intermediarios.',
  },
];

// Featured Section Component
interface FeaturedSectionProps {
  title: string;
  subtitle: string;
  vehicles: Array<{
    id: string;
    year: number;
    make: string;
    model: string;
    price: number;
    mileage: number;
    location: string;
    images: string[];
    tier?: 'basic' | 'featured' | 'premium' | 'enterprise';
    [key: string]: any; // Allow other Vehicle properties
  }>;
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

        {/* Scrollable cards - Using FeaturedListingCard */}
        <div
          ref={scrollRef}
          onScroll={checkScroll}
          className="flex gap-4 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4"
          style={{ scrollSnapType: 'x mandatory' }}
        >
          {vehicles.map((vehicle) => (
            <div 
              key={vehicle.id}
              className="flex-shrink-0 w-72"
              style={{ scrollSnapAlign: 'start' }}
            >
              <FeaturedListingCard 
                vehicle={vehicle as any}
                priority="normal"
                context="grid"
              />
            </div>
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
  const navigate = useNavigate();
  const [make, setMake] = useState('');
  const [model, setModel] = useState('');
  const [priceMin, setPriceMin] = useState('');
  const [priceMax, setPriceMax] = useState('');

  const makes = ['Toyota', 'Honda', 'Ford', 'Chevrolet', 'BMW', 'Mercedes-Benz', 'Audi', 'Tesla', 'Nissan', 'Mazda'];

  const handleSearch = () => {
    const params = new URLSearchParams();
    if (make) params.set('make', make);
    if (model) params.set('model', model);
    if (priceMin) params.set('priceMin', priceMin);
    if (priceMax) params.set('priceMax', priceMax);
    navigate(`/browse?${params.toString()}`);
  };

  return (
    <MainLayout>
      {/* Hero Section */}
      <section className="relative h-[calc(100vh-4rem)] flex items-center overflow-hidden">
        {/* Background Image */}
        <div className="absolute inset-0 z-0">
          <img
            src="https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?q=80&w=2070"
            alt="Hero background"
            className="w-full h-full object-cover"
          />
          <div className="absolute inset-0 bg-gradient-to-r from-gray-900/80 via-gray-900/60 to-gray-900/40" />
        </div>

        <div className="relative z-10 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 w-full">
          <div className="text-center mb-6">
            <motion.h1
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="text-4xl sm:text-5xl lg:text-6xl font-bold text-white mb-2"
            >
              Encuentra el Auto{' '}
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-400 to-emerald-400">
                Ideal
              </span>
            </motion.h1>
            <motion.p
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className="text-xl text-gray-200 mb-4"
            >
              Busca entre miles de vehículos nuevos y usados
            </motion.p>
          </div>

          {/* Search Bar */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="flex flex-col md:flex-row gap-2 bg-white/10 backdrop-blur-md rounded-2xl p-2"
          >
            {/* Make Selector */}
            <div className="relative">
              <select
                value={make}
                onChange={(e) => setMake(e.target.value)}
                className="w-full md:w-48 px-4 py-3 bg-white rounded-xl text-gray-900 appearance-none focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
              >
                <option value="">Todas las marcas</option>
                {makes.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </div>

            {/* Model Input */}
            <div className="relative flex-1">
              <input
                type="text"
                placeholder="Modelo (ej: Corolla)"
                value={model}
                onChange={(e) => setModel(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="w-full px-4 py-3 bg-white rounded-xl text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            {/* Price Min */}
            <div className="relative">
              <input
                type="number"
                placeholder="Precio Min"
                value={priceMin}
                onChange={(e) => setPriceMin(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="w-full md:w-40 px-4 py-3 bg-white rounded-xl text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            {/* Price Max */}
            <div className="relative">
              <input
                type="number"
                placeholder="Precio Max"
                value={priceMax}
                onChange={(e) => setPriceMax(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="w-full md:w-40 px-4 py-3 bg-white rounded-xl text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <button 
              onClick={handleSearch}
              className="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-xl transition-colors"
            >
              Buscar
            </button>
          </motion.div>

          {/* Trust Indicators */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="flex flex-wrap justify-center gap-4 md:gap-6 mt-8 max-w-4xl mx-auto"
          >
            <div className="flex items-center gap-2 text-white">
              <div className="w-9 h-9 bg-white/20 rounded-lg flex items-center justify-center flex-shrink-0">
                <FiShield className="w-5 h-5" />
              </div>
              <span className="text-sm md:text-base font-medium whitespace-nowrap">Sitio Web Seguro</span>
            </div>
            <div className="flex items-center gap-2 text-white">
              <div className="w-9 h-9 bg-white/20 rounded-lg flex items-center justify-center flex-shrink-0">
                <FiZap className="w-5 h-5" />
              </div>
              <span className="text-sm md:text-base font-medium whitespace-nowrap">Transacciones Protegidas</span>
            </div>
            <div className="flex items-center gap-2 text-white">
              <div className="w-9 h-9 bg-white/20 rounded-lg flex items-center justify-center flex-shrink-0">
                <FiSearch className="w-5 h-5" />
              </div>
              <span className="text-sm md:text-base font-medium whitespace-nowrap">Plataforma Confiable</span>
            </div>
          </motion.div>
        </div>
      </section>

      {/* Vehicle Types Section */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Explora por Tipo
            </h2>
            <p className="text-gray-600">
              Encuentra el vehículo perfecto para ti
            </p>
          </div>

          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            {vehicleTypes.map((type) => (
              <Link
                key={type.id}
                to={`/browse?type=${type.id}`}
                className="group relative bg-white rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 overflow-hidden"
              >
                {/* Image */}
                <div className="aspect-[4/3] relative overflow-hidden">
                  <img
                    src={type.image}
                    alt={type.name}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-gray-900/80 via-gray-900/20 to-transparent" />
                  
                  {/* Content overlay */}
                  <div className="absolute bottom-0 left-0 right-0 p-4">
                    <h3 className="text-lg font-bold text-white mb-1">
                      {type.name}
                    </h3>
                    <p className="text-gray-300 text-sm">{type.count}</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Vehicles */}
      <FeaturedSection
        title="Vehículos Destacados"
        subtitle="Los mejores autos seleccionados para ti"
        vehicles={mockVehicles.slice(0, 6)}
        viewAllHref="/browse"
      />

      {/* Features Section */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              ¿Por Qué Elegirnos?
            </h2>
            <p className="text-gray-600">
              La mejor experiencia en compra y venta de vehículos
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
      <section className="py-6 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Cómo Funciona
            </h2>
            <p className="text-gray-600">
              Tres simples pasos para encontrar tu vehículo
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 max-w-4xl mx-auto">
            {[
              { step: '1', title: 'Busca', desc: 'Explora miles de vehículos y filtra por tus preferencias.' },
              { step: '2', title: 'Compara', desc: 'Compara especificaciones, precios y características.' },
              { step: '3', title: 'Compra', desc: 'Contacta al vendedor y cierra el trato de forma segura.' },
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

      {/* CTA Section */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-gradient-to-br from-blue-600 to-emerald-600 rounded-3xl p-6 lg:p-8 text-center text-white">
            <h2 className="text-2xl lg:text-3xl font-bold mb-3">
              ¿Tienes un auto para vender?
            </h2>
            <p className="text-blue-100 mb-6 max-w-xl mx-auto">
              Publica tu vehículo gratis y conecta con miles de compradores interesados
            </p>
            <Link
              to="/sell-your-car"
              className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-white text-blue-600 hover:bg-gray-100 font-medium rounded-xl transition-colors"
            >
              <FaCar className="w-5 h-5" />
              Vender mi Auto
            </Link>
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default HomePage;
