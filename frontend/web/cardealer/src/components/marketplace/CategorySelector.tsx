/**
 * CategorySelector - Elegant vertical/category selector for marketplace
 * Allows users to switch between marketplace verticals (vehicles, real-estate)
 */

import React from 'react';
import { motion } from 'framer-motion';
import type { MarketplaceVertical, VerticalCategory } from '@/types/marketplace';

interface CategorySelectorProps {
  categories: VerticalCategory[];
  selectedCategory: MarketplaceVertical | 'all';
  onCategoryChange: (category: MarketplaceVertical | 'all') => void;
  variant?: 'pills' | 'cards' | 'minimal';
  showStats?: boolean;
  className?: string;
}

export const CategorySelector: React.FC<CategorySelectorProps> = ({
  categories,
  selectedCategory,
  onCategoryChange,
  variant = 'pills',
  showStats = true,
  className = '',
}) => {
  if (variant === 'cards') {
    return (
      <div className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 ${className}`}>
        {/* All categories option */}
        <CategoryCard
          id="all"
          name="Todos"
          description="Explora todo el marketplace"
          icon="🌟"
          gradient="from-purple-500 to-pink-500"
          isSelected={selectedCategory === 'all'}
          onClick={() => onCategoryChange('all')}
          stats={showStats ? {
            listingCount: categories.reduce((sum, c) => sum + c.stats.listingCount, 0),
            label: 'publicaciones'
          } : undefined}
        />
        
        {categories.map(category => (
          <CategoryCard
            key={category.id}
            id={category.id}
            name={category.name}
            description={category.description}
            icon={category.icon}
            gradient={category.gradient}
            isSelected={selectedCategory === category.id}
            onClick={() => onCategoryChange(category.id)}
            stats={showStats ? category.stats : undefined}
            subcategories={category.subcategories}
          />
        ))}
      </div>
    );
  }

  if (variant === 'minimal') {
    return (
      <div className={`flex items-center gap-2 ${className}`}>
        <button
          onClick={() => onCategoryChange('all')}
          className={`px-3 py-1.5 text-sm rounded-md transition-all ${
            selectedCategory === 'all'
              ? 'bg-gray-900 text-white'
              : 'text-gray-600 hover:text-gray-900'
          }`}
        >
          Todos
        </button>
        {categories.map(category => (
          <button
            key={category.id}
            onClick={() => onCategoryChange(category.id)}
            className={`px-3 py-1.5 text-sm rounded-md transition-all flex items-center gap-1.5 ${
              selectedCategory === category.id
                ? 'bg-gray-900 text-white'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            <span>{category.icon}</span>
            <span>{category.name}</span>
          </button>
        ))}
      </div>
    );
  }

  // Default: pills variant
  return (
    <div className={`flex flex-wrap items-center justify-center gap-2 ${className}`}>
      <CategoryPill
        id="all"
        name="Todos"
        icon="✨"
        isSelected={selectedCategory === 'all'}
        onClick={() => onCategoryChange('all')}
        count={showStats ? categories.reduce((sum, c) => sum + c.stats.listingCount, 0) : undefined}
      />
      {categories.map(category => (
        <CategoryPill
          key={category.id}
          id={category.id}
          name={category.name}
          icon={category.icon}
          isSelected={selectedCategory === category.id}
          onClick={() => onCategoryChange(category.id)}
          count={showStats ? category.stats.listingCount : undefined}
        />
      ))}
    </div>
  );
};

// Sub-component: Category Pill
interface CategoryPillProps {
  id: string;
  name: string;
  icon: string;
  isSelected: boolean;
  onClick: () => void;
  count?: number;
}

const CategoryPill: React.FC<CategoryPillProps> = ({
  name,
  icon,
  isSelected,
  onClick,
  count,
}) => {
  return (
    <motion.button
      onClick={onClick}
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
      className={`
        relative px-5 py-2.5 rounded-full font-medium text-sm
        transition-all duration-200 ease-out
        flex items-center gap-2
        ${isSelected
          ? 'bg-gradient-to-r from-blue-600 to-indigo-600 text-white shadow-lg shadow-blue-500/25'
          : 'bg-white/80/80 text-gray-700 hover:bg-white border border-gray-200'
        }
      `}
    >
      <span className="text-lg">{icon}</span>
      <span>{name}</span>
      {count !== undefined && (
        <span className={`
          px-2 py-0.5 rounded-full text-xs
          ${isSelected
            ? 'bg-white/20 text-white'
            : 'bg-gray-100 text-gray-600'
          }
        `}>
          {count.toLocaleString()}
        </span>
      )}
    </motion.button>
  );
};

// Sub-component: Category Card
interface CategoryCardProps {
  id: string;
  name: string;
  description: string;
  icon: string;
  gradient: string;
  isSelected: boolean;
  onClick: () => void;
  stats?: {
    listingCount: number;
    label: string;
  };
  subcategories?: Array<{ id: string; name: string; count: number }>;
}

const CategoryCard: React.FC<CategoryCardProps> = ({
  name,
  description,
  icon,
  gradient,
  isSelected,
  onClick,
  stats,
  subcategories,
}) => {
  return (
    <motion.button
      onClick={onClick}
      whileHover={{ scale: 1.02, y: -4 }}
      whileTap={{ scale: 0.98 }}
      className={`
        relative p-6 rounded-2xl text-left w-full
        transition-all duration-300 ease-out
        ${isSelected
          ? `bg-gradient-to-br ${gradient} text-white shadow-xl`
          : 'bg-white hover:shadow-lg border border-gray-100'
        }
      `}
    >
      {/* Icon */}
      <div className={`
        text-4xl mb-4
        ${isSelected ? '' : 'grayscale-0'}
      `}>
        {icon}
      </div>

      {/* Name & Description */}
      <h3 className={`
        text-xl font-bold mb-1
        ${isSelected ? 'text-white' : 'text-gray-900'}
      `}>
        {name}
      </h3>
      <p className={`
        text-sm mb-4
        ${isSelected ? 'text-white/80' : 'text-gray-500'}
      `}>
        {description}
      </p>

      {/* Stats */}
      {stats && (
        <div className={`
          text-sm font-medium
          ${isSelected ? 'text-white/90' : 'text-gray-700'}
        `}>
          <span className="text-2xl font-bold">
            {stats.listingCount.toLocaleString()}
          </span>
          <span className="ml-1 text-xs opacity-80">{stats.label}</span>
        </div>
      )}

      {/* Subcategories preview */}
      {subcategories && subcategories.length > 0 && (
        <div className="mt-4 flex flex-wrap gap-1">
          {subcategories.slice(0, 4).map(sub => (
            <span
              key={sub.id}
              className={`
                px-2 py-0.5 rounded-full text-xs
                ${isSelected
                  ? 'bg-white/20 text-white'
                  : 'bg-gray-100 text-gray-600'
                }
              `}
            >
              {sub.name}
            </span>
          ))}
        </div>
      )}

      {/* Selected indicator */}
      {isSelected && (
        <motion.div
          layoutId="selectedIndicator"
          className="absolute top-4 right-4 w-6 h-6 bg-white/20 rounded-full flex items-center justify-center"
        >
          <svg className="w-4 h-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </motion.div>
      )}
    </motion.button>
  );
};

export default CategorySelector;
