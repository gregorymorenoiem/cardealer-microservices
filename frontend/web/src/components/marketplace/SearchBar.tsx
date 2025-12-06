/**
 * SearchBar - Universal search bar for marketplace
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import type { MarketplaceVertical } from '@/types/marketplace';

interface SearchBarProps {
  vertical?: MarketplaceVertical | 'all';
  onSearch?: (query: string, vertical: MarketplaceVertical | 'all') => void;
  placeholder?: string;
  showVerticalSelector?: boolean;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export const SearchBar: React.FC<SearchBarProps> = ({
  vertical: initialVertical = 'all',
  onSearch,
  placeholder = '¿Qué estás buscando?',
  showVerticalSelector = true,
  size = 'md',
  className = '',
}) => {
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [vertical, setVertical] = useState<MarketplaceVertical | 'all'>(initialVertical);
  const [isFocused, setIsFocused] = useState(false);

  const sizeClasses = {
    sm: 'h-12',
    md: 'h-14',
    lg: 'h-16',
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (onSearch) {
      onSearch(query, vertical);
    } else {
      // Default navigation
      const params = new URLSearchParams();
      if (query) params.set('q', query);
      if (vertical !== 'all') params.set('vertical', vertical);
      navigate(`/browse?${params.toString()}`);
    }
  };

  const verticalOptions: Array<{ id: MarketplaceVertical | 'all'; label: string; icon: string }> = [
    { id: 'all', label: 'Todos', icon: '✨' },
    { id: 'vehicles', label: 'Vehículos', icon: '🚗' },
    { id: 'real-estate', label: 'Propiedades', icon: '🏠' },
  ];

  return (
    <form onSubmit={handleSubmit} className={className}>
      <motion.div
        animate={{ 
          boxShadow: isFocused 
            ? '0 4px 20px rgba(59, 130, 246, 0.15)' 
            : '0 1px 3px rgba(0, 0, 0, 0.1)' 
        }}
        className={`
          relative flex items-center bg-white rounded-2xl
          border-2 transition-colors
          ${isFocused 
            ? 'border-blue-500' 
            : 'border-gray-200'
          }
          ${sizeClasses[size]}
        `}
      >
        {/* Vertical selector */}
        {showVerticalSelector && (
          <div className="relative pl-2">
            <select
              value={vertical}
              onChange={(e) => setVertical(e.target.value as MarketplaceVertical | 'all')}
              className="appearance-none bg-gray-100 text-gray-700 text-sm font-medium rounded-xl px-3 py-2 pr-8 cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {verticalOptions.map((opt) => (
                <option key={opt.id} value={opt.id}>
                  {opt.icon} {opt.label}
                </option>
              ))}
            </select>
            <svg 
              className="absolute right-2 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" 
              fill="none" 
              viewBox="0 0 24 24" 
              stroke="currentColor"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </div>
        )}

        {/* Divider */}
        {showVerticalSelector && (
          <div className="h-8 w-px bg-gray-200 mx-3" />
        )}

        {/* Search icon */}
        <svg 
          className={`w-5 h-5 text-gray-400 ${!showVerticalSelector ? 'ml-4' : ''}`} 
          fill="none" 
          viewBox="0 0 24 24" 
          stroke="currentColor"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>

        {/* Input */}
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
          placeholder={placeholder}
          className="flex-1 bg-transparent border-none outline-none text-gray-900 placeholder-gray-400 px-3"
        />

        {/* Clear button */}
        <AnimatePresence>
          {query && (
            <motion.button
              type="button"
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.8 }}
              onClick={() => setQuery('')}
              className="p-2 text-gray-400 hover:text-gray-600"
            >
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </motion.button>
          )}
        </AnimatePresence>

        {/* Search button */}
        <button
          type="submit"
          className="h-full px-6 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-r-xl transition-colors flex items-center gap-2"
        >
          <span className="hidden sm:inline">Buscar</span>
          <svg className="w-5 h-5 sm:hidden" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </button>
      </motion.div>
    </form>
  );
};

export default SearchBar;
