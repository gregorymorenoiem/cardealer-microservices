/**
 * Homepage components barrel export
 *
 * All homepage components are centralized here with their own styling.
 * Pages should import and use these without additional styling.
 */

// Hero components
export { default as HeroCarousel } from './hero-carousel';
export { HeroStatic } from './hero-static';
export { HeroEnhanced } from './hero-enhanced';
export { HeroCompact } from './hero-compact';

// Section components
export { default as FeaturedSection } from './featured-section';
export { default as FeaturedListingGrid } from './featured-listing-grid';
export { SectionContainer } from './section-container';
export { SectionHeader } from './section-header';

// Category components
export { CategoryCards, FeaturedCategory, DEFAULT_CATEGORIES } from './category-cards';
export type { CategoryCard } from './category-cards';

// Brand components
export { BrandSlider, BrandGrid } from './brand-slider';

// Testimonials
export { TestimonialsCarousel, TestimonialsGrid } from './testimonials-carousel';

// Why Choose Us / Value Props
export { WhyChooseUs } from './why-choose-us';

// Feature components
export { FeaturesGrid } from './features-grid';
export type { Feature } from './features-grid';

// CTA components
export { CTASection } from './cta-section';

// Loading states
export { LoadingSection, ErrorSection, SkeletonGrid } from './loading-states';

// Types
export type { FeaturedListingItem } from './featured-section';
