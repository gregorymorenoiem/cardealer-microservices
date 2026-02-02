/**
 * OKLA Homepage
 *
 * Professional homepage with premium visual components
 * Inspired by CarGurus, AutoTrader, and Cars.com
 */

'use client';

import { useMemo } from 'react';

// Homepage components (all styling is encapsulated)
import {
  HeroCarousel,
  HeroEnhanced,
  FeaturedListingGrid,
  FeaturedSection,
  SectionContainer,
  CategoryCards,
  BrandSlider,
  WhyChooseUs,
  CTASection,
  LoadingSection,
  ErrorSection,
  SkeletonGrid,
  type FeaturedListingItem,
} from '@/components/homepage';

// Data fetching
import { useHomepageSections } from '@/hooks/use-homepage-sections';
import {
  transformHomepageVehicleToVehicle,
  type HomepageSection,
  type HomepageVehicle,
} from '@/services/homepage-sections';

// =============================================
// TRANSFORM HELPERS
// =============================================

const transformToFeaturedListing = (
  v: HomepageVehicle,
  categoryName: string
): FeaturedListingItem => ({
  id: v.id,
  title: `${v.year} ${v.make} ${v.model}`,
  price: v.price,
  mileage: v.mileage,
  location: 'Santo Domingo, RD',
  imageUrl: v.imageUrl || (v.imageUrls.length > 0 ? v.imageUrls[0] : '/placeholder-car.jpg'),
  category: categoryName,
  year: v.year,
  make: v.make,
  model: v.model,
  fuelType: v.fuelType,
  transmission: v.transmission,
});

const transformSectionVehicles = (section: HomepageSection | undefined): FeaturedListingItem[] => {
  if (!section || section.vehicles.length === 0) return [];
  return section.vehicles.map(v => transformToFeaturedListing(v, section.name));
};

// =============================================
// PAGE COMPONENT
// =============================================

export default function HomePage() {
  // Fetch homepage data
  const { isLoading, error, carousel, sedanes, suvs, camionetas, deportivos, destacados, lujo } =
    useHomepageSections();

  // Transform vehicles for components
  const heroVehicles = useMemo(() => {
    if (!carousel || carousel.vehicles.length === 0) return [];
    return carousel.vehicles.map(transformHomepageVehicleToVehicle);
  }, [carousel]);

  const gridVehicles = useMemo(() => {
    const source = destacados?.vehicles.length ? destacados : carousel;
    if (!source || source.vehicles.length === 0) return [];
    return source.vehicles.map(transformHomepageVehicleToVehicle);
  }, [destacados, carousel]);

  // Category sections to render
  const categorySections = useMemo(() => {
    return [sedanes, suvs, camionetas, deportivos, destacados, lujo].filter(
      (section): section is HomepageSection => !!section && section.vehicles.length > 0
    );
  }, [sedanes, suvs, camionetas, deportivos, destacados, lujo]);

  return (
    <>
      {/* Hero Section - Premium with Search */}
      {heroVehicles.length > 0 ? (
        <HeroCarousel vehicles={heroVehicles} autoPlayInterval={5000} showScrollHint={false} />
      ) : (
        <HeroEnhanced />
      )}

      {/* Trusted Brands Slider */}
      <section className="bg-white py-12">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="mb-8 text-center text-sm font-semibold tracking-widest text-slate-400 uppercase">
            Las marcas más buscadas en República Dominicana
          </h2>
          <BrandSlider autoScroll scrollSpeed={40} />
        </div>
      </section>

      {/* Featured Vehicles Grid */}
      <SectionContainer
        title="Vehículos Destacados"
        subtitle="Explora nuestra selección premium de vehículos cuidadosamente verificados"
        background="gradient"
      >
        {gridVehicles.length > 0 ? (
          <FeaturedListingGrid vehicles={gridVehicles} maxItems={9} />
        ) : (
          <SkeletonGrid count={6} columns={3} />
        )}
      </SectionContainer>

      {/* Browse by Category */}
      <section className="bg-gray-50 py-16 lg:py-24">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mb-12 text-center">
            <span className="mb-4 inline-block rounded-full bg-[#00A870]/10 px-4 py-1.5 text-sm font-semibold tracking-wide text-[#00A870]">
              Explora por Categoría
            </span>
            <h2 className="mb-4 text-3xl leading-tight font-bold tracking-tight text-slate-900 lg:text-4xl">
              Encuentra el tipo de vehículo perfecto
            </h2>
            <p className="mx-auto max-w-2xl text-lg leading-relaxed text-slate-600">
              Desde SUVs familiares hasta deportivos de alto rendimiento, tenemos el vehículo ideal
              para ti.
            </p>
          </div>
          <CategoryCards />
        </div>
      </section>

      {/* Loading State */}
      {isLoading && <LoadingSection />}

      {/* Error State */}
      {error && <ErrorSection message={error} />}

      {/* Dynamic Category Sections */}
      {!isLoading &&
        categorySections
          .slice(0, 4)
          .map(section => (
            <FeaturedSection
              key={section.slug}
              title={section.name}
              subtitle={section.subtitle}
              listings={transformSectionVehicles(section)}
              viewAllHref={section.viewAllHref}
              accentColor={section.accentColor}
            />
          ))}

      {/* Why Choose OKLA */}
      <WhyChooseUs variant="grid" />

      {/* CTA */}
      <CTASection
        title="¿Listo para vender tu vehículo?"
        subtitle="Publica tu vehículo en minutos y llega a compradores interesados."
        primaryButton={{ label: 'Publicar Gratis', href: '/vender' }}
        secondaryButton={{ label: 'Para Dealers', href: '/dealers' }}
      />
    </>
  );
}
