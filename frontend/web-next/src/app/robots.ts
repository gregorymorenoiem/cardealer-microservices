/**
 * Robots.txt Configuration
 *
 * Controls search engine crawling behavior
 */

import { MetadataRoute } from 'next';

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'https://okla.com.do';

export default function robots(): MetadataRoute.Robots {
  return {
    rules: [
      {
        // Main search engines
        userAgent: '*',
        allow: '/',
        disallow: [
          // Private/authenticated areas
          '/dashboard/',
          '/mis-vehiculos/',
          '/dealer/',
          '/admin/',
          '/settings/',
          '/mensajes/',
          '/favoritos/',

          // Auth pages
          '/login',
          '/registro',
          '/recuperar-contrasena',
          '/restablecer-contrasena',
          '/verificar-email',

          // API routes
          '/api/',

          // Search with too many parameters
          '/vehiculos?*&*&*&*',

          // Checkout/payment flows
          '/checkout/',
          '/pago/',

          // Temporary/preview pages
          '/preview/',
          '/draft/',
        ],
      },
      {
        // Googlebot specific rules
        userAgent: 'Googlebot',
        allow: '/',
        disallow: ['/dashboard/', '/mis-vehiculos/', '/dealer/', '/admin/', '/api/'],
      },
      {
        // Block bad bots
        userAgent: ['AhrefsBot', 'SemrushBot', 'MJ12bot', 'DotBot'],
        disallow: '/',
      },
    ],
    sitemap: `${SITE_URL}/sitemap.xml`,
    host: SITE_URL,
  };
}
