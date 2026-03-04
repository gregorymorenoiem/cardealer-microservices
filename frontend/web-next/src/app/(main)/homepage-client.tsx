/**
 * Homepage Client Component
 *
 * Client-side interactive parts of the homepage.
 * Receives pre-fetched data from server component and handles interactivity.
 */

'use client';

import { useMemo } from 'react';
import { HeroCompact, BrandSlider, WhyChooseUs, CTASection } from '@/components/homepage';
import VehicleTypeSection from '@/components/homepage/vehicle-type-section';
import FeaturedVehicles from '@/components/advertising/featured-vehicles';
import { NativeBannerAd } from '@/components/advertising/native-ads';
import { DealerPromoSection } from '@/components/homepage/dealer-promo-section';
import { useBrands } from '@/hooks/use-advertising';
import type { BrandConfig } from '@/types/advertising';

// =============================================
// COMPONENT
// =============================================

export default function HomepageClient() {
  // Dynamic brands from AdvertisingService
  const { data: apiBrands } = useBrands();

  // Sponsored dealers for brand slider
  const dealerSponsors = useMemo(() => {
    if (!apiBrands || apiBrands.length === 0) return undefined;
    return apiBrands
      .filter(
        (b: BrandConfig) =>
          b.isActive &&
          b.logoUrl &&
          'dealerId' in b &&
          (b as unknown as Record<string, unknown>).dealerId
      )
      .sort((a: BrandConfig, b: BrandConfig) => a.displayOrder - b.displayOrder)
      .slice(0, 10)
      .map((b: BrandConfig) => ({
        id: b.id,
        name: b.displayName,
        slug: b.brandKey.toLowerCase(),
        logoUrl: b.logoUrl || undefined,
        vehicleCount: b.vehicleCount,
        isDealer: true,
        portalSlug: b.brandKey.toLowerCase().replace(/\s+/g, '-'),
      }));
  }, [apiBrands]);

  const dynamicBrands = useMemo(() => {
    if (!apiBrands || apiBrands.length === 0) return undefined;
    return apiBrands
      .filter((b: BrandConfig) => b.isActive)
      .sort((a: BrandConfig, b: BrandConfig) => a.displayOrder - b.displayOrder)
      .map((b: BrandConfig) => ({
        id: b.id,
        name: b.displayName,
        slug: b.brandKey.toLowerCase(),
        logoUrl: b.logoUrl || undefined,
        vehicleCount: b.vehicleCount,
      }));
  }, [apiBrands]);

  // Stable impression token for the dealer banner
  const bannerImpressionToken = 'banner-dealer-cta-homepage';

  return (
    <>
      {/* ── 1. HERO — NL search + vehicle photos ─────────────────────────── */}
      <HeroCompact />

      {/* ── SECCIONES PAGADAS CON FOTOS GRANDES (primeras) ───────────────── */}

      {/* 2. ⭐ Vehículos Destacados — espacio pagado FeaturedSpot */}
      <FeaturedVehicles title="⭐ Vehículos Destacados" placementType="FeaturedSpot" maxItems={8} />

      {/* 3. 💎 Vehículos Premium — espacio pagado PremiumSpot */}
      <FeaturedVehicles title="💎 Vehículos Premium" placementType="PremiumSpot" maxItems={8} />

      {/* 4. Dealers Patrocinados — espacios pagados por dealers */}
      <DealerPromoSection dealers={dealerSponsors} totalSlots={8} />

      {/* 5. Marcas más buscadas en República Dominicana */}
      <section className="bg-muted/30 py-8">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-muted-foreground mb-4 text-center text-xs font-semibold tracking-widest uppercase">
            Las marcas más buscadas en República Dominicana
          </h2>
          <BrandSlider
            brands={dynamicBrands}
            autoScroll
            scrollSpeed={40}
            showDealerSponsors={false}
          />
        </div>
      </section>

      {/* ── SECCIONES POR TIPO DE VEHÍCULO ──────────────────────────────── */}
      {/* Same card size as ⭐ Destacados — 4-column large-photo grid.      */}
      {/* Each section fetches live data from the vehicles API by type.     */}

      {/* 6. SUVs */}
      <VehicleTypeSection
        filterValue="SUV"
        title="SUVs"
        subtitle="Los más solicitados en República Dominicana"
        icon="🚙"
        viewAllHref="/vehiculos?bodyType=suv"
        accentColor="blue"
      />

      {/* 7. Sedanes */}
      <VehicleTypeSection
        filterValue="Sedan"
        title="Sedanes"
        subtitle="Comodidad y eficiencia para el día a día"
        icon="🚗"
        viewAllHref="/vehiculos?bodyType=sedan"
        accentColor="emerald"
      />

      {/* Dealer CTA banner between type sections */}
      <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <NativeBannerAd
          title="¿Eres dealer? Llega a más compradores"
          subtitle="Destaca tu inventario con publicidad inteligente y paga solo por resultados reales."
          ctaText="Conocer más"
          ctaUrl="/dealers"
          backgroundGradient="from-emerald-600 to-teal-700"
          impressionToken={bannerImpressionToken}
        />
      </div>

      {/* 8. Camionetas / Pickups */}
      <VehicleTypeSection
        filterValue="Pickup"
        title="Camionetas"
        subtitle="Potencia y versatilidad para trabajo y aventura"
        icon="🛻"
        viewAllHref="/vehiculos?bodyType=pickup"
        accentColor="amber"
      />

      {/* 9. Deportivos — Coupe */}
      <VehicleTypeSection
        filterValue="Coupe"
        title="Deportivos"
        subtitle="Rendimiento y estilo en cada curva"
        icon="🏎️"
        viewAllHref="/vehiculos?bodyType=coupe"
        accentColor="rose"
      />

      {/* 10. Híbridos — filter by fuelType */}
      <VehicleTypeSection
        filterType="fuelType"
        filterValue="Hybrid"
        title="Híbridos"
        subtitle="Mayor eficiencia, menor impacto ambiental"
        icon="🌿"
        viewAllHref="/vehiculos?fuelType=hibrido"
        accentColor="teal"
      />

      {/* 11. Eléctricos — filter by fuelType */}
      <VehicleTypeSection
        filterType="fuelType"
        filterValue="Electric"
        title="Eléctricos"
        subtitle="El futuro de la movilidad ya está aquí"
        icon="⚡"
        viewAllHref="/vehiculos?fuelType=electrico"
        accentColor="indigo"
      />

      {/* ── FINAL ──────────────────────────────────────────────────────────── */}

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
