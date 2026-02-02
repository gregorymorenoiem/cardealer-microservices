/**
 * HeroEnhanced Component
 *
 * Professional hero section inspired by CarGurus/AutoTrader/Cars.com
 * Features:
 * - Integrated search bar with make/model/price filters
 * - Animated trust badges
 * - Floating car statistics
 * - Smooth gradient backgrounds with animated shapes
 */

'use client';

import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, ChevronDown, Car, Shield, Star, CheckCircle2 } from 'lucide-react';
import Link from 'next/link';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

interface HeroEnhancedProps {
  className?: string;
}

interface TrustBadge {
  icon: typeof Shield;
  text: string;
}

// =============================================================================
// STATIC DATA
// =============================================================================

const TRUST_BADGES: TrustBadge[] = [
  { icon: Shield, text: 'Vendedores Verificados' },
  { icon: CheckCircle2, text: 'Historial Garantizado' },
  { icon: Star, text: 'Precios Transparentes' },
];

const POPULAR_MAKES = ['Toyota', 'Honda', 'Hyundai', 'Kia', 'Nissan', 'Mazda', 'Ford', 'Chevrolet'];

// =============================================================================
// ANIMATED BACKGROUND SHAPES
// =============================================================================

function AnimatedShapes() {
  return (
    <div className="pointer-events-none absolute inset-0 overflow-hidden">
      {/* Gradient Orbs */}
      <motion.div
        className="absolute -top-40 -right-40 h-96 w-96 rounded-full bg-gradient-to-br from-[#00A870]/30 to-[#00A870]/5 blur-3xl"
        animate={{
          scale: [1, 1.2, 1],
          opacity: [0.3, 0.5, 0.3],
        }}
        transition={{
          duration: 8,
          repeat: Infinity,
          ease: 'easeInOut',
        }}
      />
      <motion.div
        className="absolute -bottom-40 -left-40 h-80 w-80 rounded-full bg-gradient-to-tr from-blue-500/20 to-purple-500/10 blur-3xl"
        animate={{
          scale: [1.2, 1, 1.2],
          opacity: [0.2, 0.4, 0.2],
        }}
        transition={{
          duration: 10,
          repeat: Infinity,
          ease: 'easeInOut',
        }}
      />

      {/* Grid Pattern */}
      <div
        className="absolute inset-0 opacity-[0.02]"
        style={{
          backgroundImage: `
            linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px),
            linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)
          `,
          backgroundSize: '60px 60px',
        }}
      />

      {/* Floating Car Silhouettes */}
      <motion.div
        className="absolute top-1/4 right-20 opacity-5"
        animate={{ y: [-10, 10, -10] }}
        transition={{ duration: 6, repeat: Infinity, ease: 'easeInOut' }}
      >
        <svg width="200" height="80" viewBox="0 0 200 80" fill="white">
          <path d="M160 50c0-16.569-13.431-30-30-30H70c-16.569 0-30 13.431-30 30v10h120V50z" />
          <circle cx="60" cy="60" r="15" />
          <circle cx="140" cy="60" r="15" />
        </svg>
      </motion.div>
    </div>
  );
}

// =============================================================================
// SEARCH BAR COMPONENT
// =============================================================================

function HeroSearchBar() {
  const [condition, setCondition] = useState('');
  const [make, setMake] = useState('');
  const [model, setModel] = useState('');

  // Models depend on selected make
  const modelsByMake: Record<string, string[]> = {
    Toyota: ['Corolla', 'Camry', 'RAV4', 'Hilux', 'Land Cruiser', 'Fortuner', 'Yaris', '4Runner'],
    Honda: ['Civic', 'Accord', 'CR-V', 'HR-V', 'Pilot', 'Odyssey', 'Fit'],
    Hyundai: ['Tucson', 'Santa Fe', 'Elantra', 'Sonata', 'Kona', 'Palisade', 'Accent'],
    Kia: ['Sportage', 'Sorento', 'Seltos', 'K5', 'Carnival', 'Soul', 'Forte'],
    Nissan: ['Sentra', 'Altima', 'Rogue', 'Pathfinder', 'Kicks', 'Frontier', 'Murano'],
    Mazda: ['CX-5', 'CX-30', 'CX-9', 'Mazda3', 'Mazda6', 'MX-5'],
    Ford: ['F-150', 'Explorer', 'Escape', 'Bronco', 'Ranger', 'Mustang', 'Edge'],
    Chevrolet: ['Silverado', 'Equinox', 'Tahoe', 'Traverse', 'Malibu', 'Camaro', 'Colorado'],
  };

  const availableModels = make ? modelsByMake[make] || [] : [];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.3, duration: 0.6 }}
      className="mx-auto w-full max-w-4xl"
    >
      {/* Main Search Container */}
      <div className="rounded-2xl border border-white/20 bg-white p-2 shadow-2xl shadow-black/10">
        <div className="flex flex-col gap-2 md:flex-row">
          {/* Condition Select */}
          <div className="group relative flex-1">
            <select
              value={condition}
              onChange={e => setCondition(e.target.value)}
              className="h-14 w-full cursor-pointer appearance-none rounded-xl border border-gray-200 bg-gray-50 px-4 pr-10 font-medium text-gray-900 transition-all hover:border-gray-300 focus:border-[#00A870] focus:bg-white focus:ring-2 focus:ring-[#00A870]/20 focus:outline-none"
            >
              <option value="">Estado</option>
              <option value="nuevo">Nuevo</option>
              <option value="recien-importado">Recién Importado</option>
              <option value="usado">Usado</option>
            </select>
            <ChevronDown className="pointer-events-none absolute top-1/2 right-3 h-5 w-5 -translate-y-1/2 text-gray-400" />
          </div>

          {/* Make Select */}
          <div className="group relative flex-1">
            <select
              value={make}
              onChange={e => {
                setMake(e.target.value);
                setModel(''); // Reset model when make changes
              }}
              className="h-14 w-full cursor-pointer appearance-none rounded-xl border border-gray-200 bg-gray-50 px-4 pr-10 font-medium text-gray-900 transition-all hover:border-gray-300 focus:border-[#00A870] focus:bg-white focus:ring-2 focus:ring-[#00A870]/20 focus:outline-none"
            >
              <option value="">Marca</option>
              {POPULAR_MAKES.map(m => (
                <option key={m} value={m}>
                  {m}
                </option>
              ))}
            </select>
            <ChevronDown className="pointer-events-none absolute top-1/2 right-3 h-5 w-5 -translate-y-1/2 text-gray-400" />
          </div>

          {/* Model Select */}
          <div className="group relative flex-1">
            <select
              value={model}
              onChange={e => setModel(e.target.value)}
              disabled={!make}
              className="h-14 w-full cursor-pointer appearance-none rounded-xl border border-gray-200 bg-gray-50 px-4 pr-10 font-medium text-gray-900 transition-all hover:border-gray-300 focus:border-[#00A870] focus:bg-white focus:ring-2 focus:ring-[#00A870]/20 focus:outline-none disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="">{make ? 'Modelo' : 'Selecciona marca'}</option>
              {availableModels.map(m => (
                <option key={m} value={m}>
                  {m}
                </option>
              ))}
            </select>
            <ChevronDown className="pointer-events-none absolute top-1/2 right-3 h-5 w-5 -translate-y-1/2 text-gray-400" />
          </div>

          {/* Search Button */}
          <Link
            href={`/vehiculos?condition=${condition}&make=${make}&model=${model}`}
            className="flex h-14 items-center justify-center gap-2 rounded-xl bg-[#00A870] px-10 font-semibold whitespace-nowrap text-white shadow-lg shadow-[#00A870]/30 transition-all duration-300 hover:bg-[#009663] hover:shadow-xl hover:shadow-[#00A870]/40"
          >
            <Search className="h-5 w-5" />
            <span>Buscar</span>
          </Link>
        </div>
      </div>

      {/* Quick Filters */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.6 }}
        className="mt-4 flex flex-wrap justify-center gap-2"
      >
        {['SUV', 'Sedán', 'Camioneta', 'Deportivo', 'Híbrido', 'Eléctrico'].map(filter => (
          <Link
            key={filter}
            href={`/vehiculos?bodyType=${filter}`}
            className="rounded-full border border-white/30 bg-white/15 px-5 py-2.5 text-sm font-semibold text-white shadow-sm backdrop-blur-md transition-all duration-200 hover:border-[#00A870] hover:bg-[#00A870]/90 hover:text-white hover:shadow-md hover:shadow-[#00A870]/20"
          >
            {filter}
          </Link>
        ))}
      </motion.div>
    </motion.div>
  );
}

// =============================================================================
// TRUST BADGES
// =============================================================================

function TrustBadgesBar({ badges }: { badges: TrustBadge[] }) {
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ delay: 0.8 }}
      className="flex flex-wrap justify-center gap-4 md:gap-8"
    >
      {badges.map((badge, index) => (
        <motion.div
          key={badge.text}
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: 0.9 + index * 0.1 }}
          className="flex items-center gap-2 text-white"
        >
          <badge.icon className="h-5 w-5 text-[#00A870]" />
          <span className="text-sm font-medium">{badge.text}</span>
        </motion.div>
      ))}
    </motion.div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function HeroEnhanced({ className }: HeroEnhancedProps) {
  return (
    <section
      className={cn(
        'relative flex min-h-[90vh] flex-col items-center justify-center overflow-hidden',
        'bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900',
        className
      )}
    >
      {/* Animated Background */}
      <AnimatedShapes />

      {/* Content Container */}
      <div className="relative z-10 mx-auto w-full max-w-7xl px-4 py-16 sm:px-6 md:py-24 lg:px-8">
        {/* Headline */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="mb-8 text-center md:mb-12"
        >
          <h1 className="mb-4 text-4xl leading-[1.1] font-bold tracking-tight text-white sm:text-5xl md:mb-6 md:text-6xl lg:text-7xl">
            Tu próximo vehículo
            <br />
            está en <span className="text-[#00A870]">OKLA</span>
          </h1>
          <p className="mx-auto max-w-2xl text-lg leading-relaxed text-white/90 md:text-xl">
            El nuevo marketplace de vehículos en República Dominicana. Encuentra, compara y compra
            con total confianza.
          </p>
        </motion.div>

        {/* Search Bar */}
        <HeroSearchBar />

        {/* Trust Badges */}
        <div className="mt-12 md:mt-16">
          <TrustBadgesBar badges={TRUST_BADGES} />
        </div>
      </div>

      {/* Scroll Indicator */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 1.2 }}
        className="absolute bottom-8 left-1/2 -translate-x-1/2"
      >
        <motion.div
          animate={{ y: [0, 10, 0] }}
          transition={{ duration: 2, repeat: Infinity }}
          className="flex flex-col items-center gap-2 text-white/80"
        >
          <span className="text-xs font-medium tracking-widest uppercase">Explorar</span>
          <ChevronDown className="h-5 w-5" />
        </motion.div>
      </motion.div>
    </section>
  );
}

// Named export for convenience
export { HeroEnhanced as default };
