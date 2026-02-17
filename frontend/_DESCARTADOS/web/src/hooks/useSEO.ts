/**
 * SEO Hook
 * 
 * Custom hook for managing SEO state and generating
 * structured data for different page types.
 */

import { useMemo } from 'react';
import type { SEOHeadProps } from '../components/seo/SEOHead';

interface VehicleSEO {
  make: string;
  model: string;
  year: number;
  price: number;
  mileage?: number;
  condition?: 'new' | 'used' | 'certified';
  color?: string;
  vin?: string;
  imageUrl?: string;
  dealerName?: string;
  dealerUrl?: string;
}

interface DealerSEO {
  name: string;
  description?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  imageUrl?: string;
  rating?: number;
  reviewCount?: number;
  openingHours?: string[];
}

interface ArticleSEO {
  title: string;
  description: string;
  author: string;
  publishedDate: string;
  modifiedDate?: string;
  imageUrl?: string;
  category?: string;
  tags?: string[];
}

/**
 * Generate SEO props for a vehicle listing page
 */
export function useVehicleSEO(vehicle: VehicleSEO, slug: string): SEOHeadProps {
  return useMemo(() => {
    const title = `${vehicle.year} ${vehicle.make} ${vehicle.model}`;
    const conditionText = vehicle.condition === 'new' ? 'Nuevo' : vehicle.condition === 'certified' ? 'Certificado' : 'Usado';
    const description = `${conditionText} ${title}${vehicle.mileage ? ` con ${vehicle.mileage.toLocaleString()} km` : ''}. Precio: $${vehicle.price.toLocaleString()}. Disponible en CarDealer.`;
    
    const keywords = [
      vehicle.make,
      vehicle.model,
      `${vehicle.make} ${vehicle.model}`,
      `${vehicle.year} ${vehicle.make}`,
      'carros en venta',
      'autos usados',
      'vehiculos republica dominicana',
      vehicle.condition === 'new' ? 'carros nuevos' : 'carros usados',
    ];

    const structuredData = {
      '@type': 'Vehicle',
      vehicleIdentificationNumber: vehicle.vin,
      manufacturer: vehicle.make,
      model: vehicle.model,
      vehicleModelDate: vehicle.year,
      mileageFromOdometer: vehicle.mileage ? {
        '@type': 'QuantitativeValue',
        value: vehicle.mileage,
        unitCode: 'KMT',
      } : undefined,
      color: vehicle.color,
      itemCondition: vehicle.condition === 'new' 
        ? 'https://schema.org/NewCondition' 
        : 'https://schema.org/UsedCondition',
      ...(vehicle.dealerName && {
        seller: {
          '@type': 'AutoDealer',
          name: vehicle.dealerName,
          url: vehicle.dealerUrl,
        },
      }),
    };

    return {
      title,
      description,
      canonicalUrl: `/vehicles/${slug}`,
      image: vehicle.imageUrl,
      type: 'product',
      keywords,
      price: vehicle.price,
      currency: 'USD',
      availability: 'in_stock',
      structuredData,
    };
  }, [vehicle, slug]);
}

/**
 * Generate SEO props for a dealer profile page
 */
export function useDealerSEO(dealer: DealerSEO, slug: string): SEOHeadProps {
  return useMemo(() => {
    const title = `${dealer.name} - Concesionario de Autos`;
    const description = dealer.description || 
      `${dealer.name}${dealer.city ? ` en ${dealer.city}` : ''}. Encuentra los mejores vehículos nuevos y usados. ${dealer.phone ? `Contacto: ${dealer.phone}` : ''}`;
    
    const keywords = [
      dealer.name,
      'concesionario',
      'dealer',
      'carros en venta',
      dealer.city || 'republica dominicana',
      'autos usados',
      'vehiculos nuevos',
    ];

    const structuredData = {
      '@type': 'AutoDealer',
      name: dealer.name,
      description: dealer.description,
      image: dealer.imageUrl,
      telephone: dealer.phone,
      email: dealer.email,
      address: dealer.address ? {
        '@type': 'PostalAddress',
        streetAddress: dealer.address,
        addressLocality: dealer.city,
        addressCountry: 'DO',
      } : undefined,
      ...(dealer.rating && {
        aggregateRating: {
          '@type': 'AggregateRating',
          ratingValue: dealer.rating,
          reviewCount: dealer.reviewCount || 0,
          bestRating: 5,
          worstRating: 1,
        },
      }),
      ...(dealer.openingHours && {
        openingHoursSpecification: dealer.openingHours.map(hours => ({
          '@type': 'OpeningHoursSpecification',
          dayOfWeek: hours.split(' ')[0],
          opens: hours.split(' ')[1]?.split('-')[0],
          closes: hours.split(' ')[1]?.split('-')[1],
        })),
      }),
    };

    return {
      title,
      description,
      canonicalUrl: `/dealers/${slug}`,
      image: dealer.imageUrl,
      type: 'website',
      keywords,
      structuredData,
    };
  }, [dealer, slug]);
}

/**
 * Generate SEO props for a blog article page
 */
export function useArticleSEO(article: ArticleSEO, slug: string): SEOHeadProps {
  return useMemo(() => {
    const keywords = [
      ...(article.tags || []),
      article.category || 'autos',
      'noticias automotrices',
      'consejos carros',
    ];

    return {
      title: article.title,
      description: article.description,
      canonicalUrl: `/blog/${slug}`,
      image: article.imageUrl,
      type: 'article',
      author: article.author,
      publishedDate: article.publishedDate,
      modifiedDate: article.modifiedDate,
      keywords,
    };
  }, [article, slug]);
}

/**
 * Generate SEO props for search results page
 */
export function useSearchSEO(query: string, filters?: Record<string, string>): SEOHeadProps {
  return useMemo(() => {
    const filterParts: string[] = [];
    if (filters?.make) filterParts.push(filters.make);
    if (filters?.model) filterParts.push(filters.model);
    if (filters?.year) filterParts.push(`año ${filters.year}`);
    if (filters?.priceMax) filterParts.push(`hasta $${filters.priceMax}`);
    
    const titleParts = query || filterParts.join(', ') || 'Todos los vehículos';
    const title = `Buscar: ${titleParts}`;
    const description = `Resultados de búsqueda${query ? ` para "${query}"` : ''}${filterParts.length > 0 ? `: ${filterParts.join(', ')}` : ''}. Encuentra tu próximo vehículo en CarDealer.`;
    
    return {
      title,
      description,
      canonicalUrl: '/search',
      noIndex: true, // Search results typically shouldn't be indexed
      keywords: [
        query,
        ...filterParts,
        'buscar carros',
        'vehiculos en venta',
      ].filter(Boolean),
    };
  }, [query, filters]);
}

/**
 * Generate SEO props for static pages
 */
export function usePageSEO(
  pageType: 'home' | 'about' | 'contact' | 'terms' | 'privacy' | 'faq'
): SEOHeadProps {
  return useMemo(() => {
    const pages: Record<string, SEOHeadProps> = {
      home: {
        title: undefined, // Uses site name only
        description: 'CarDealer es el marketplace líder en República Dominicana para compra y venta de vehículos nuevos y usados. Encuentra tu próximo auto hoy.',
        canonicalUrl: '/',
        keywords: ['carros en venta', 'autos usados', 'vehiculos nuevos', 'comprar carro', 'republica dominicana'],
      },
      about: {
        title: 'Sobre Nosotros',
        description: 'Conoce la historia y misión de CarDealer, el marketplace de vehículos líder en República Dominicana.',
        canonicalUrl: '/about',
        keywords: ['sobre cardealer', 'quienes somos', 'marketplace vehiculos'],
      },
      contact: {
        title: 'Contacto',
        description: 'Contáctanos para cualquier pregunta sobre compra o venta de vehículos en CarDealer.',
        canonicalUrl: '/contact',
        keywords: ['contacto cardealer', 'soporte', 'ayuda'],
      },
      terms: {
        title: 'Términos y Condiciones',
        description: 'Lee los términos y condiciones de uso de la plataforma CarDealer.',
        canonicalUrl: '/terms',
        noIndex: true,
      },
      privacy: {
        title: 'Política de Privacidad',
        description: 'Conoce cómo CarDealer protege y maneja tu información personal.',
        canonicalUrl: '/privacy',
        noIndex: true,
      },
      faq: {
        title: 'Preguntas Frecuentes',
        description: 'Encuentra respuestas a las preguntas más comunes sobre comprar y vender vehículos en CarDealer.',
        canonicalUrl: '/faq',
        keywords: ['preguntas frecuentes', 'ayuda', 'faq cardealer'],
      },
    };

    return pages[pageType] || pages.home;
  }, [pageType]);
}

export default {
  useVehicleSEO,
  useDealerSEO,
  useArticleSEO,
  useSearchSEO,
  usePageSEO,
};
