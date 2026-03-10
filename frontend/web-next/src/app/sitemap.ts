/**
 * Dynamic Sitemap Generation
 *
 * Generates XML sitemap including:
 * - Static pages (home, about, etc.)
 * - Vehicle listings
 * - Dealer pages
 * - Category pages
 *
 * SEO FIX: Added lastModified from vehicle.updatedAt for faster Google
 * re-crawl scheduling. Reduced revalidation to 15min for new listings
 * to appear in sitemap within ~15 minutes of publication.
 *
 * NOTE: When vehicle count exceeds 40,000, implement sitemap index pattern
 * by splitting into /sitemap/vehicles.xml, /sitemap/dealers.xml, etc.
 * Google limit: 50,000 URLs or 50MB per sitemap file.
 */

import { MetadataRoute } from 'next';
import { blogPosts } from './(main)/blog/blog-data';
import { guides as guideData } from './(main)/guias/guide-data';

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'https://okla.com.do';

// Types for dynamic data fetching
interface VehicleSitemapEntry {
  slug: string;
  updatedAt?: string;
}

interface DealerSitemapEntry {
  slug: string;
  updatedAt?: string;
}

/**
 * Fetch vehicles for sitemap
 * In production, this would call the API
 */
async function getVehiclesForSitemap(): Promise<VehicleSitemapEntry[]> {
  try {
    // BFF pattern: server-side uses INTERNAL_API_URL for direct internal calls
    const apiUrl =
      process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';
    const response = await fetch(`${apiUrl}/api/vehicles/sitemap`, {
      next: { revalidate: 900 }, // INDEXATION FIX: Reduced from 3600 to 900 (15 min) for faster new listing discovery
    });

    if (!response.ok) {
      console.error('Failed to fetch vehicles for sitemap, status:', response.status);
      return [];
    }

    const data = await response.json();

    // INDEXATION FIX C1: Backend returns { items: [...], total: N, generatedAt: "..." }
    // but we need VehicleSitemapEntry[]. Handle both formats for compatibility.
    const items = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);

    // INDEXATION FIX H4: Guard against empty responses from backend errors
    if (!Array.isArray(items) || items.length === 0) {
      console.warn(
        'Sitemap: Backend returned 0 vehicles — possible API issue. Returning empty to avoid caching bad data.'
      );
      return [];
    }

    console.log(`Sitemap: Loaded ${items.length} vehicle URLs from backend`);
    return items;
  } catch (error) {
    console.error('Error fetching vehicles for sitemap:', error);
    // Return mock data in development only
    if (process.env.NODE_ENV === 'development') {
      return [
        { slug: 'toyota-corolla-2024-abc123', updatedAt: new Date().toISOString() },
        { slug: 'honda-civic-2023-def456', updatedAt: new Date().toISOString() },
        { slug: 'hyundai-tucson-2024-ghi789', updatedAt: new Date().toISOString() },
      ];
    }
    return [];
  }
}

/**
 * Fetch dealers for sitemap
 */
async function getDealersForSitemap(): Promise<DealerSitemapEntry[]> {
  try {
    // BFF pattern: server-side uses INTERNAL_API_URL for direct internal calls
    const apiUrl =
      process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';
    const response = await fetch(`${apiUrl}/api/dealers/sitemap`, {
      next: { revalidate: 900 }, // INDEXATION FIX: Reduced from 3600 to 900 (15 min)
    });

    if (!response.ok) {
      console.error('Failed to fetch dealers for sitemap, status:', response.status);
      return [];
    }

    const data = await response.json();
    // INDEXATION FIX: Handle both array and wrapper object formats
    const items = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);
    return Array.isArray(items) ? items : [];
  } catch (error) {
    console.error('Error fetching dealers for sitemap:', error);
    if (process.env.NODE_ENV === 'development') {
      return [
        { slug: 'auto-plaza-santo-domingo', updatedAt: new Date().toISOString() },
        { slug: 'premium-motors-santiago', updatedAt: new Date().toISOString() },
      ];
    }
    return [];
  }
}

/**
 * Main sitemap generator function
 */
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const now = new Date().toISOString();

  // Static pages
  const staticPages: MetadataRoute.Sitemap = [
    {
      url: SITE_URL,
      lastModified: now,
      changeFrequency: 'daily',
      priority: 1.0,
    },
    {
      url: `${SITE_URL}/vehiculos`,
      lastModified: now,
      changeFrequency: 'hourly',
      priority: 0.9,
    },
    {
      url: `${SITE_URL}/dealers`,
      lastModified: now,
      changeFrequency: 'daily',
      priority: 0.8,
    },
    {
      url: `${SITE_URL}/vender`,
      lastModified: now,
      changeFrequency: 'weekly',
      priority: 0.8,
    },
    {
      url: `${SITE_URL}/publicar`,
      lastModified: now,
      changeFrequency: 'weekly',
      priority: 0.7,
    },
    {
      url: `${SITE_URL}/nosotros`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.5,
    },
    {
      url: `${SITE_URL}/contacto`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.5,
    },
    {
      url: `${SITE_URL}/terminos`,
      lastModified: now,
      changeFrequency: 'yearly',
      priority: 0.3,
    },
    {
      url: `${SITE_URL}/privacidad`,
      lastModified: now,
      changeFrequency: 'yearly',
      priority: 0.3,
    },
    {
      url: `${SITE_URL}/precios`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.7,
    },
    {
      url: `${SITE_URL}/ayuda`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.5,
    },
    {
      url: `${SITE_URL}/blog`,
      lastModified: now,
      changeFrequency: 'weekly',
      priority: 0.6,
    },
    {
      url: `${SITE_URL}/guias`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.5,
    },
    {
      url: `${SITE_URL}/seguridad`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.4,
    },
    {
      url: `${SITE_URL}/reportar`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.3,
    },
    {
      url: `${SITE_URL}/comparar`,
      lastModified: now,
      changeFrequency: 'daily',
      priority: 0.6,
    },
    {
      url: `${SITE_URL}/faq`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.5,
    },
    {
      url: `${SITE_URL}/prensa`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.4,
    },
    {
      url: `${SITE_URL}/empleos`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.3,
    },
    {
      url: `${SITE_URL}/cookies`,
      lastModified: now,
      changeFrequency: 'yearly',
      priority: 0.2,
    },
    {
      url: `${SITE_URL}/politica-reembolso`,
      lastModified: now,
      changeFrequency: 'yearly',
      priority: 0.2,
    },
    {
      url: `${SITE_URL}/herramientas`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.6,
    },
    {
      url: `${SITE_URL}/herramientas/calculadora-financiamiento`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.7,
    },
    {
      url: `${SITE_URL}/herramientas/calculadora-importacion`,
      lastModified: now,
      changeFrequency: 'monthly',
      priority: 0.7,
    },
    {
      url: `${SITE_URL}/buscar`,
      lastModified: now,
      changeFrequency: 'hourly',
      priority: 0.8,
    },
    // OKLA Score — high-priority landing page for SEO keyword capture
    {
      url: `${SITE_URL}/okla-score`,
      lastModified: now,
      changeFrequency: 'weekly',
      priority: 0.9,
    },
  ];

  // Category pages (makes)
  const popularMakes = [
    'toyota',
    'honda',
    'hyundai',
    'kia',
    'nissan',
    'ford',
    'chevrolet',
    'mazda',
    'bmw',
    'mercedes-benz',
    'audi',
    'volkswagen',
    'jeep',
    'mitsubishi',
    'suzuki',
    'lexus',
    'subaru',
    'dodge',
  ];

  const makePages: MetadataRoute.Sitemap = popularMakes.map(make => ({
    url: `${SITE_URL}/vehiculos?make=${make}`,
    lastModified: now,
    changeFrequency: 'daily' as const,
    priority: 0.7,
  }));

  // Brand landing pages (SEO-optimized dedicated pages) + /marcas index
  const brandLandingPages: MetadataRoute.Sitemap = [
    {
      url: `${SITE_URL}/marcas`,
      lastModified: now,
      changeFrequency: 'weekly' as const,
      priority: 0.8,
    },
    ...popularMakes.map(make => ({
      url: `${SITE_URL}/marcas/${make}`,
      lastModified: now,
      changeFrequency: 'daily' as const,
      priority: 0.8,
    })),
  ];

  // Model SEO landing pages — /marcas/[marca]/[modelo]
  // Synced with generateStaticParams in marcas/[marca]/[modelo]/page.tsx
  const popularCombinations: Array<{ make: string; model: string }> = [
    // Toyota (14)
    { make: 'toyota', model: 'corolla' },
    { make: 'toyota', model: 'camry' },
    { make: 'toyota', model: 'rav4' },
    { make: 'toyota', model: 'hilux' },
    { make: 'toyota', model: 'fortuner' },
    { make: 'toyota', model: 'land-cruiser' },
    { make: 'toyota', model: 'prado' },
    { make: 'toyota', model: 'yaris' },
    { make: 'toyota', model: 'c-hr' },
    { make: 'toyota', model: 'highlander' },
    { make: 'toyota', model: 'tacoma' },
    { make: 'toyota', model: '4runner' },
    { make: 'toyota', model: 'tundra' },
    { make: 'toyota', model: 'sequoia' },
    // Honda (8)
    { make: 'honda', model: 'civic' },
    { make: 'honda', model: 'accord' },
    { make: 'honda', model: 'cr-v' },
    { make: 'honda', model: 'hr-v' },
    { make: 'honda', model: 'pilot' },
    { make: 'honda', model: 'odyssey' },
    { make: 'honda', model: 'fit' },
    { make: 'honda', model: 'ridgeline' },
    // Hyundai (8)
    { make: 'hyundai', model: 'elantra' },
    { make: 'hyundai', model: 'sonata' },
    { make: 'hyundai', model: 'tucson' },
    { make: 'hyundai', model: 'santa-fe' },
    { make: 'hyundai', model: 'accent' },
    { make: 'hyundai', model: 'kona' },
    { make: 'hyundai', model: 'palisade' },
    { make: 'hyundai', model: 'creta' },
    // Kia (7)
    { make: 'kia', model: 'sportage' },
    { make: 'kia', model: 'sorento' },
    { make: 'kia', model: 'forte' },
    { make: 'kia', model: 'seltos' },
    { make: 'kia', model: 'rio' },
    { make: 'kia', model: 'soul' },
    { make: 'kia', model: 'carnival' },
    // Nissan (8)
    { make: 'nissan', model: 'sentra' },
    { make: 'nissan', model: 'altima' },
    { make: 'nissan', model: 'pathfinder' },
    { make: 'nissan', model: 'frontier' },
    { make: 'nissan', model: 'kicks' },
    { make: 'nissan', model: 'rogue' },
    { make: 'nissan', model: 'versa' },
    { make: 'nissan', model: 'x-trail' },
    // Ford (6)
    { make: 'ford', model: 'explorer' },
    { make: 'ford', model: 'escape' },
    { make: 'ford', model: 'ranger' },
    { make: 'ford', model: 'f-150' },
    { make: 'ford', model: 'bronco' },
    { make: 'ford', model: 'expedition' },
    // Chevrolet (6)
    { make: 'chevrolet', model: 'silverado' },
    { make: 'chevrolet', model: 'equinox' },
    { make: 'chevrolet', model: 'tahoe' },
    { make: 'chevrolet', model: 'trailblazer' },
    { make: 'chevrolet', model: 'traverse' },
    { make: 'chevrolet', model: 'colorado' },
    // Jeep (5)
    { make: 'jeep', model: 'wrangler' },
    { make: 'jeep', model: 'grand-cherokee' },
    { make: 'jeep', model: 'compass' },
    { make: 'jeep', model: 'renegade' },
    { make: 'jeep', model: 'gladiator' },
    // Mitsubishi (5)
    { make: 'mitsubishi', model: 'outlander' },
    { make: 'mitsubishi', model: 'l200' },
    { make: 'mitsubishi', model: 'asx' },
    { make: 'mitsubishi', model: 'montero' },
    { make: 'mitsubishi', model: 'eclipse-cross' },
    // Suzuki (4)
    { make: 'suzuki', model: 'vitara' },
    { make: 'suzuki', model: 'swift' },
    { make: 'suzuki', model: 'jimny' },
    { make: 'suzuki', model: 'grand-vitara' },
    // Mazda (4)
    { make: 'mazda', model: 'cx-5' },
    { make: 'mazda', model: 'cx-30' },
    { make: 'mazda', model: 'mazda3' },
    { make: 'mazda', model: 'cx-9' },
    // BMW (3)
    { make: 'bmw', model: 'x3' },
    { make: 'bmw', model: 'x5' },
    { make: 'bmw', model: 'serie-3' },
    // Mercedes-Benz (3)
    { make: 'mercedes-benz', model: 'clase-c' },
    { make: 'mercedes-benz', model: 'gle' },
    { make: 'mercedes-benz', model: 'glc' },
  ];

  const modelLandingPages: MetadataRoute.Sitemap = popularCombinations.map(combo => ({
    url: `${SITE_URL}/marcas/${combo.make}/${combo.model}`,
    lastModified: now,
    changeFrequency: 'daily' as const,
    priority: 0.75,
  }));

  // Body type pages
  const bodyTypes = ['sedan', 'suv', 'camioneta', 'pickup', 'coupe', 'hatchback', 'van'];

  const bodyTypePages: MetadataRoute.Sitemap = bodyTypes.map(type => ({
    url: `${SITE_URL}/vehiculos?bodyType=${type}`,
    lastModified: now,
    changeFrequency: 'daily' as const,
    priority: 0.6,
  }));

  // Province pages
  const provinces = [
    'santo-domingo',
    'santiago',
    'la-vega',
    'puerto-plata',
    'san-cristobal',
    'la-romana',
    'san-pedro-de-macoris',
    'punta-cana',
  ];

  const provincePages: MetadataRoute.Sitemap = provinces.map(province => ({
    url: `${SITE_URL}/vehiculos?province=${province}`,
    lastModified: now,
    changeFrequency: 'daily' as const,
    priority: 0.6,
  }));

  // Dynamic vehicle pages
  const vehicles = await getVehiclesForSitemap();
  const vehiclePages: MetadataRoute.Sitemap = vehicles.map(vehicle => ({
    url: `${SITE_URL}/vehiculos/${vehicle.slug}`,
    lastModified: vehicle.updatedAt || now,
    changeFrequency: 'weekly' as const,
    priority: 0.8,
  }));

  // Dynamic dealer pages
  const dealers = await getDealersForSitemap();
  const dealerPages: MetadataRoute.Sitemap = dealers.map(dealer => ({
    url: `${SITE_URL}/dealers/${dealer.slug}`,
    lastModified: dealer.updatedAt || now,
    changeFrequency: 'weekly' as const,
    priority: 0.7,
  }));

  // Dynamic blog post pages
  const blogPages: MetadataRoute.Sitemap = blogPosts.map(post => ({
    url: `${SITE_URL}/blog/${post.slug}`,
    lastModified: post.date,
    changeFrequency: 'monthly' as const,
    priority: 0.6,
  }));

  // Guide pages
  const guidePages: MetadataRoute.Sitemap = guideData.map(guide => ({
    url: `${SITE_URL}/guias/${guide.slug}`,
    lastModified: guide.lastUpdated,
    changeFrequency: 'monthly' as const,
    priority: 0.6,
  }));

  // Combine all pages
  return [
    ...staticPages,
    ...makePages,
    ...brandLandingPages,
    ...modelLandingPages,
    ...bodyTypePages,
    ...provincePages,
    ...vehiclePages,
    ...dealerPages,
    ...blogPages,
    ...guidePages,
  ];
}
