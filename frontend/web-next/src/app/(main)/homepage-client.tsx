/**
 * Homepage Client Component
 *
 * Client-side interactive parts of the homepage.
 * Receives pre-fetched data from server component and handles interactivity.
 */

'use client';

import { useMemo } from 'react';
import {
  HeroCompact,
  FeaturedListingGrid,
  FeaturedSection,
  SectionContainer,
  CategoryCards,
  BrandSlider,
  WhyChooseUs,
  CTASection,
  SkeletonGrid,
  type FeaturedListingItem,
} from '@/components/homepage';
import {
  transformHomepageVehicleToVehicle,
  type HomepageSection,
  type HomepageVehicle,
  type Vehicle,
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
// PROPS
// =============================================

interface HomepageClientProps {
  sections: HomepageSection[];
}

// =============================================
// COMPONENT
// =============================================

export default function HomepageClient({ sections }: HomepageClientProps) {
  const getSection = (slug: string) => sections.find(s => s.slug === slug);

  const carousel = getSection('carousel');
  const destacados = getSection('destacados');
  const sedanes = getSection('sedanes');
  const suvs = getSection('suvs');
  const camionetas = getSection('camionetas');
  const deportivos = getSection('deportivos');
  const lujo = getSection('lujo');

  const heroVehicles: Vehicle[] = useMemo(() => {
    const source = carousel?.vehicles.length ? carousel : destacados;
    if (!source || source.vehicles.length === 0) return [];
    return source.vehicles.map(transformHomepageVehicleToVehicle);
  }, [carousel, destacados]);

  const gridVehicles: Vehicle[] = useMemo(() => {
    const source = destacados?.vehicles.length ? destacados : carousel;
    if (!source || source.vehicles.length === 0) return [];
    return source.vehicles.map(transformHomepageVehicleToVehicle);
  }, [destacados, carousel]);

  const categorySections = useMemo(() => {
    return [sedanes, suvs, camionetas, deportivos, destacados, lujo].filter(
      (section): section is HomepageSection => !!section && section.vehicles.length > 0
    );
  }, [sedanes, suvs, camionetas, deportivos, destacados, lujo]);

  return (
    <>
      {/* Hero Section - Compact with vehicles visible immediately */}
      <HeroCompact vehicles={heroVehicles} isLoading={false} />

      {/* Featured Vehicles Grid - IMMEDIATE after hero */}
      <SectionContainer
        title="Más Vehículos"
        subtitle="Explora nuestra selección premium de vehículos cuidadosamente verificados"
        background="gradient"
      >
        {gridVehicles.length > 0 ? (
          <FeaturedListingGrid vehicles={gridVehicles} maxItems={9} />
        ) : (
          <SkeletonGrid count={6} columns={3} />
        )}
      </SectionContainer>

      {/* Trusted Brands Slider */}
      <section className="bg-card py-8">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-muted-foreground mb-4 text-center text-xs font-semibold tracking-widest uppercase">
            Las marcas más buscadas en República Dominicana
          </h2>
          <BrandSlider autoScroll scrollSpeed={40} />
        </div>
      </section>

      {/* Browse by Category */}
      <section className="bg-muted/50 dark:bg-muted/20 py-12 lg:py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mb-8 text-center">
            <span className="bg-primary/10 text-primary mb-3 inline-block rounded-full px-4 py-1.5 text-sm font-semibold tracking-wide">
              Explora por Categoría
            </span>
            <h2 className="text-foreground mb-3 text-2xl leading-tight font-bold tracking-tight lg:text-3xl">
              Encuentra el tipo de vehículo perfecto
            </h2>
            <p className="text-muted-foreground mx-auto max-w-xl text-base leading-relaxed">
              Desde SUVs familiares hasta deportivos de alto rendimiento.
            </p>
          </div>
          <CategoryCards />
        </div>
      </section>

      {/* Dynamic Category Sections */}
      {categorySections.slice(0, 4).map(section => (
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
