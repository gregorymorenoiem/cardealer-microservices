/**
 * MarketplaceHomePage - Modern marketplace homepage with multi-vertical support
 * Elegant, non-saturated design per spec
 */

import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { 
  CategorySelector, 
  SearchBar, 
  FeaturedListings,
  ListingGrid 
} from '@/components/marketplace';
import { 
  useCategories, 
  useFeaturedListings,
  useListings 
} from '@/hooks/useMarketplace';
import type { MarketplaceVertical } from '@/types/marketplace';

const MarketplaceHomePage: React.FC = () => {
  const [selectedCategory, setSelectedCategory] = useState<MarketplaceVertical | 'all'>('all');

  // Data hooks
  const { data: categories = [] } = useCategories();
  const { data: featuredListings = [], isLoading: featuredLoading } = useFeaturedListings(8);
  const { data: recentListings, isLoading: recentLoading } = useListings({ 
    pageSize: 12,
    vertical: selectedCategory === 'all' ? undefined : selectedCategory,
  });

  return (
    <MainLayout>
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-gradient-to-br from-gray-50 to-white">
        {/* Subtle background pattern */}
        <div className="absolute inset-0 opacity-[0.03]">
          <svg className="w-full h-full" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse">
                <path d="M 40 0 L 0 0 0 40" fill="none" stroke="currentColor" strokeWidth="1"/>
              </pattern>
            </defs>
            <rect width="100%" height="100%" fill="url(#grid)" />
          </svg>
        </div>

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 lg:py-24">
          {/* Heading */}
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center mb-10"
          >
            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold text-gray-900 mb-4">
              Tu marketplace de confianza
            </h1>
            <p className="text-xl text-gray-600 max-w-2xl mx-auto">
              Encuentra vehículos y propiedades verificadas de vendedores confiables
            </p>
          </motion.div>

          {/* Search Bar */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="max-w-3xl mx-auto mb-12"
          >
            <SearchBar size="lg" showVerticalSelector />
          </motion.div>

          {/* Category Selector */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
          >
            <CategorySelector
              categories={categories}
              selectedCategory={selectedCategory}
              onCategoryChange={setSelectedCategory}
              variant="cards"
              showStats
              className="max-w-4xl mx-auto"
            />
          </motion.div>
        </div>

        {/* Decorative elements */}
        <div className="absolute -bottom-20 -left-20 w-64 h-64 bg-blue-500/10 rounded-full blur-3xl" />
        <div className="absolute -top-20 -right-20 w-64 h-64 bg-emerald-500/10 rounded-full blur-3xl" />
      </section>

      {/* Featured Listings */}
      <section className="py-16 lg:py-24 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <FeaturedListings
            listings={featuredListings}
            isLoading={featuredLoading}
            title="Listados Destacados"
            subtitle="Los mejores vehículos y propiedades seleccionados para ti"
            viewAllLink="/browse?featured=true"
          />
        </div>
      </section>

      {/* Recent Listings */}
      <section className="py-16 lg:py-24 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header with category pills */}
          <div className="flex flex-col md:flex-row md:items-end md:justify-between gap-4 mb-10">
            <div>
              <motion.h2
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                className="text-3xl font-bold text-gray-900"
              >
                Publicaciones Recientes
              </motion.h2>
              <p className="text-gray-500 mt-1">
                Explora las últimas publicaciones del marketplace
              </p>
            </div>

            <CategorySelector
              categories={categories}
              selectedCategory={selectedCategory}
              onCategoryChange={setSelectedCategory}
              variant="minimal"
            />
          </div>

          {/* Listings Grid */}
          <ListingGrid
            listings={recentListings?.listings || []}
            isLoading={recentLoading}
            columns={4}
            emptyMessage="No hay listados disponibles en esta categoría"
          />

          {/* View all button */}
          <div className="mt-12 text-center">
            <Link
              to={`/browse${selectedCategory !== 'all' ? `?vertical=${selectedCategory}` : ''}`}
              className="inline-flex items-center gap-2 px-8 py-4 bg-gray-900 hover:bg-gray-800 text-white font-medium rounded-full transition-colors"
            >
              Ver todos los listados
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
              </svg>
            </Link>
          </div>
        </div>
      </section>

      {/* How it Works */}
      <section className="py-16 lg:py-24 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              ¿Cómo funciona?
            </h2>
            <p className="text-gray-500 max-w-2xl mx-auto">
              Compra o vende en tres simples pasos
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
            {[
              {
                icon: '🔍',
                title: 'Busca',
                description: 'Explora vehículos y propiedades con filtros avanzados para encontrar lo que buscas',
              },
              {
                icon: '💬',
                title: 'Contacta',
                description: 'Comunícate directamente con los anunciantes de forma segura',
              },
              {
                icon: '✅',
                title: 'Cierra el trato',
                description: 'Completa tu operación con la seguridad de nuestra plataforma',
              },
            ].map((step, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: i * 0.1 }}
                className="text-center"
              >
                <div className="w-20 h-20 bg-gray-100 rounded-2xl flex items-center justify-center mx-auto mb-6 text-4xl">
                  {step.icon}
                </div>
                <h3 className="text-xl font-bold text-gray-900 mb-3">
                  {step.title}
                </h3>
                <p className="text-gray-500">
                  {step.description}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-16 lg:py-24 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-3xl p-8 lg:p-16 shadow-xl text-center">
            <h2 className="text-3xl lg:text-4xl font-bold text-gray-900 mb-4">
              ¿Listo para vender?
            </h2>
            <p className="text-xl text-gray-500 mb-8 max-w-2xl mx-auto">
              Publica tu vehículo o propiedad en nuestra plataforma segura
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/vehicles/new"
                className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-full transition-colors"
              >
                <span className="text-xl">🚗</span>
                Publicar Vehículo
              </Link>
              <Link
                to="/properties/new"
                className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-full transition-colors"
              >
                <span className="text-xl">🏠</span>
                Publicar Propiedad
              </Link>
            </div>
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default MarketplaceHomePage;
