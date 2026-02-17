/**
 * Sitemap API Route
 *
 * Generate dynamic sitemap for SEO
 */

import { NextResponse } from 'next/server';

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'https://okla.com.do';
const BACKEND_URL = process.env.BACKEND_API_URL || 'http://localhost:8080';

interface Vehicle {
  slug: string;
  updatedAt: string;
}

interface Dealer {
  slug: string;
  updatedAt: string;
}

export async function GET() {
  try {
    // Static pages
    const staticPages = [
      { url: '/', changefreq: 'daily', priority: 1.0 },
      { url: '/vehiculos', changefreq: 'hourly', priority: 0.9 },
      { url: '/buscar', changefreq: 'daily', priority: 0.8 },
      { url: '/dealers', changefreq: 'daily', priority: 0.8 },
      { url: '/comparar', changefreq: 'weekly', priority: 0.6 },
      { url: '/ayuda', changefreq: 'weekly', priority: 0.5 },
      { url: '/contacto', changefreq: 'monthly', priority: 0.4 },
      { url: '/nosotros', changefreq: 'monthly', priority: 0.4 },
      { url: '/about', changefreq: 'monthly', priority: 0.4 },
      { url: '/terminos', changefreq: 'monthly', priority: 0.3 },
      { url: '/privacidad', changefreq: 'monthly', priority: 0.3 },
    ];

    // Fetch dynamic content
    let vehicles: Vehicle[] = [];
    let dealers: Dealer[] = [];

    try {
      const vehiclesResponse = await fetch(`${BACKEND_URL}/api/vehicles?limit=1000`, {
        next: { revalidate: 3600 }, // Cache for 1 hour
      });
      if (vehiclesResponse.ok) {
        const data = await vehiclesResponse.json();
        vehicles = data.items || [];
      }
    } catch (e) {
      console.error('Failed to fetch vehicles for sitemap:', e);
    }

    try {
      const dealersResponse = await fetch(`${BACKEND_URL}/api/dealers?limit=500`, {
        next: { revalidate: 3600 },
      });
      if (dealersResponse.ok) {
        const data = await dealersResponse.json();
        dealers = data.items || [];
      }
    } catch (e) {
      console.error('Failed to fetch dealers for sitemap:', e);
    }

    // Build sitemap XML
    const sitemap = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  ${staticPages
    .map(
      page => `
  <url>
    <loc>${SITE_URL}${page.url}</loc>
    <changefreq>${page.changefreq}</changefreq>
    <priority>${page.priority}</priority>
  </url>`
    )
    .join('')}
  ${vehicles
    .map(
      vehicle => `
  <url>
    <loc>${SITE_URL}/vehiculos/${vehicle.slug}</loc>
    <lastmod>${new Date(vehicle.updatedAt).toISOString()}</lastmod>
    <changefreq>weekly</changefreq>
    <priority>0.7</priority>
  </url>`
    )
    .join('')}
  ${dealers
    .map(
      dealer => `
  <url>
    <loc>${SITE_URL}/dealers/${dealer.slug}</loc>
    <lastmod>${new Date(dealer.updatedAt).toISOString()}</lastmod>
    <changefreq>weekly</changefreq>
    <priority>0.6</priority>
  </url>`
    )
    .join('')}
</urlset>`;

    return new NextResponse(sitemap, {
      headers: {
        'Content-Type': 'application/xml',
        'Cache-Control': 'public, max-age=3600, s-maxage=3600',
      },
    });
  } catch (error) {
    console.error('Sitemap generation error:', error);
    return NextResponse.json({ error: 'Failed to generate sitemap' }, { status: 500 });
  }
}
