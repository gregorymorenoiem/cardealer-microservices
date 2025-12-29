/**
 * FeaturedHeroSection Component
 * Sprint 4: HeroCarousel Component
 * 
 * Complete hero section with carousel + quick search
 * Integrates HeroCarousel with search functionality
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, MapPin } from 'lucide-react';
import HeroCarousel from './HeroCarousel';
import type { Vehicle } from '@/data/mockVehicles';

interface FeaturedHeroSectionProps {
  vehicles: Vehicle[];
  autoPlayInterval?: number;
}

const CATEGORIES = [
  { value: 'all', label: 'Todos los Vehículos' },
  { value: 'sedan', label: 'Sedán' },
  { value: 'suv', label: 'SUV' },
  { value: 'truck', label: 'Camioneta' },
  { value: 'electric', label: 'Eléctrico' },
] as const;

export default function FeaturedHeroSection({ 
  vehicles, 
  autoPlayInterval = 5000 
}: FeaturedHeroSectionProps) {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');
  const [location, setLocation] = useState('');
  const [category, setCategory] = useState('all');

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Build query params
    const params = new URLSearchParams();
    if (searchQuery) params.set('q', searchQuery);
    if (location) params.set('location', location);
    if (category !== 'all') params.set('category', category);
    
    navigate(`/vehicles?${params.toString()}`);
  };

  return (
    <div className="relative">
      {/* Hero Carousel */}
      <HeroCarousel vehicles={vehicles} autoPlayInterval={autoPlayInterval} />

      {/* Floating Search Bar */}
      <div className="absolute bottom-0 left-0 right-0 z-30 pb-8 pointer-events-none">
        <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
          <form 
            onSubmit={handleSearch}
            className="bg-white rounded-2xl shadow-2xl p-6 pointer-events-auto"
          >
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              {/* Search Query */}
              <div className="md:col-span-1">
                <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-2">
                  ¿Qué buscas?
                </label>
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
                  <input
                    id="search"
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="Marca, modelo..."
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              {/* Category */}
              <div className="md:col-span-1">
                <label htmlFor="category" className="block text-sm font-medium text-gray-700 mb-2">
                  Categoría
                </label>
                <select
                  id="category"
                  value={category}
                  onChange={(e) => setCategory(e.target.value)}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  {CATEGORIES.map((cat) => (
                    <option key={cat.value} value={cat.value}>
                      {cat.label}
                    </option>
                  ))}
                </select>
              </div>

              {/* Location */}
              <div className="md:col-span-1">
                <label htmlFor="location" className="block text-sm font-medium text-gray-700 mb-2">
                  Ubicación
                </label>
                <div className="relative">
                  <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
                  <input
                    id="location"
                    type="text"
                    value={location}
                    onChange={(e) => setLocation(e.target.value)}
                    placeholder="Ciudad o estado"
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              {/* Search Button */}
              <div className="md:col-span-1 flex items-end">
                <button
                  type="submit"
                  className="w-full px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors duration-200"
                >
                  Buscar
                </button>
              </div>
            </div>

            {/* Quick Links */}
            <div className="mt-4 flex flex-wrap gap-2">
              <span className="text-sm text-gray-600">Búsquedas populares:</span>
              <button
                type="button"
                onClick={() => {
                  setSearchQuery('Tesla');
                  setCategory('electric');
                }}
                className="text-sm text-blue-600 hover:text-blue-700 font-medium"
              >
                Tesla
              </button>
              <span className="text-gray-300">•</span>
              <button
                type="button"
                onClick={() => {
                  setSearchQuery('BMW');
                  setCategory('sedan');
                }}
                className="text-sm text-blue-600 hover:text-blue-700 font-medium"
              >
                BMW Sedan
              </button>
              <span className="text-gray-300">•</span>
              <button
                type="button"
                onClick={() => {
                  setSearchQuery('');
                  setCategory('suv');
                }}
                className="text-sm text-blue-600 hover:text-blue-700 font-medium"
              >
                SUV
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
