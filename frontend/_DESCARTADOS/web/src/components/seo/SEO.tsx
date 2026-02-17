/**
 * SEO Components - Dynamic meta tags and structured data
 * Essential for search engine visibility and rich snippets
 * 
 * Features:
 * - Dynamic meta tags per page/listing
 * - JSON-LD structured data for vehicles and properties
 * - Open Graph tags for social sharing
 * - Twitter Card support
 * - Canonical URLs
 */

import React, { useEffect, useMemo } from 'react';
import type { VehicleListing, PropertyListing } from '@/types/marketplace';

// ============================================
// Types
// ============================================

interface SEOProps {
  title: string;
  description: string;
  image?: string;
  url?: string;
  type?: 'website' | 'article' | 'product';
  noIndex?: boolean;
  keywords?: string[];
  author?: string;
  publishedTime?: string;
  modifiedTime?: string;
}

interface VehicleSEOProps {
  vehicle: VehicleListing;
  dealerName?: string;
  dealerPhone?: string;
}

interface PropertySEOProps {
  property: PropertyListing;
  agentName?: string;
  agentPhone?: string;
}

// ============================================
// Base SEO Component
// ============================================

export const SEO: React.FC<SEOProps> = ({
  title,
  description,
  image,
  url,
  type = 'website',
  noIndex = false,
  keywords = [],
  author,
  publishedTime,
  modifiedTime,
}) => {
  const siteTitle = 'CarDealer Marketplace';
  const fullTitle = title ? `${title} | ${siteTitle}` : siteTitle;
  const canonicalUrl = url || (typeof window !== 'undefined' ? window.location.href : '');
  const defaultImage = '/images/og-default.jpg';

  useEffect(() => {
    // Update document title
    document.title = fullTitle;

    // Helper to update or create meta tag
    const updateMeta = (property: string, content: string, isName = false) => {
      const attribute = isName ? 'name' : 'property';
      let element = document.querySelector(`meta[${attribute}="${property}"]`);
      
      if (!element) {
        element = document.createElement('meta');
        element.setAttribute(attribute, property);
        document.head.appendChild(element);
      }
      
      element.setAttribute('content', content);
    };

    // Basic meta tags
    updateMeta('description', description, true);
    if (keywords.length > 0) {
      updateMeta('keywords', keywords.join(', '), true);
    }
    if (author) {
      updateMeta('author', author, true);
    }
    if (noIndex) {
      updateMeta('robots', 'noindex, nofollow', true);
    }

    // Open Graph tags
    updateMeta('og:title', fullTitle);
    updateMeta('og:description', description);
    updateMeta('og:type', type);
    updateMeta('og:url', canonicalUrl);
    updateMeta('og:image', image || defaultImage);
    updateMeta('og:site_name', siteTitle);
    updateMeta('og:locale', 'es_DO');

    // Twitter Card tags
    updateMeta('twitter:card', 'summary_large_image', true);
    updateMeta('twitter:title', fullTitle, true);
    updateMeta('twitter:description', description, true);
    updateMeta('twitter:image', image || defaultImage, true);

    // Article-specific tags
    if (type === 'article') {
      if (publishedTime) {
        updateMeta('article:published_time', publishedTime);
      }
      if (modifiedTime) {
        updateMeta('article:modified_time', modifiedTime);
      }
    }

    // Update canonical link
    let canonical = document.querySelector('link[rel="canonical"]');
    if (!canonical) {
      canonical = document.createElement('link');
      canonical.setAttribute('rel', 'canonical');
      document.head.appendChild(canonical);
    }
    canonical.setAttribute('href', canonicalUrl);

    // Cleanup on unmount
    return () => {
      document.title = siteTitle;
    };
  }, [fullTitle, description, image, canonicalUrl, type, noIndex, keywords, author, publishedTime, modifiedTime]);

  return null;
};

// ============================================
// Vehicle Structured Data
// ============================================

export const VehicleSEO: React.FC<VehicleSEOProps> = ({
  vehicle,
  dealerName = 'CarDealer Marketplace',
  dealerPhone,
}) => {
  const structuredData = useMemo(() => {
    const baseUrl = typeof window !== 'undefined' ? window.location.origin : '';
    
    // Vehicle structured data (Schema.org Car)
    const vehicleSchema = {
      '@context': 'https://schema.org',
      '@type': 'Car',
      '@id': `${baseUrl}/vehicles/${vehicle.id}`,
      name: vehicle.title,
      description: vehicle.description,
      image: vehicle.images.map(img => img.url),
      brand: {
        '@type': 'Brand',
        name: vehicle.make,
      },
      model: vehicle.model,
      vehicleModelDate: vehicle.year.toString(),
      mileageFromOdometer: {
        '@type': 'QuantitativeValue',
        value: vehicle.mileage,
        unitCode: 'KMT',
      },
      vehicleTransmission: vehicle.transmission === 'automatic' 
        ? 'https://schema.org/AutomaticTransmission' 
        : 'https://schema.org/ManualTransmission',
      fuelType: vehicle.fuelType,
      color: vehicle.exteriorColor,
      vehicleInteriorColor: vehicle.interiorColor,
      vehicleEngine: vehicle.engine ? {
        '@type': 'EngineSpecification',
        name: vehicle.engine,
      } : undefined,
      driveWheelConfiguration: vehicle.drivetrain,
      offers: {
        '@type': 'Offer',
        price: vehicle.price,
        priceCurrency: vehicle.currency,
        availability: vehicle.status === 'active' 
          ? 'https://schema.org/InStock' 
          : 'https://schema.org/OutOfStock',
        seller: {
          '@type': 'AutoDealer',
          name: dealerName,
          telephone: dealerPhone,
        },
        priceValidUntil: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      },
      url: `${baseUrl}/vehicles/${vehicle.id}`,
      itemCondition: vehicle.condition === 'new' 
        ? 'https://schema.org/NewCondition' 
        : 'https://schema.org/UsedCondition',
    };

    // Breadcrumb
    const breadcrumbSchema = {
      '@context': 'https://schema.org',
      '@type': 'BreadcrumbList',
      itemListElement: [
        {
          '@type': 'ListItem',
          position: 1,
          name: 'Inicio',
          item: baseUrl,
        },
        {
          '@type': 'ListItem',
          position: 2,
          name: 'Vehículos',
          item: `${baseUrl}/vehicles`,
        },
        {
          '@type': 'ListItem',
          position: 3,
          name: `${vehicle.make} ${vehicle.model}`,
          item: `${baseUrl}/vehicles/${vehicle.id}`,
        },
      ],
    };

    return [vehicleSchema, breadcrumbSchema];
  }, [vehicle, dealerName, dealerPhone]);

  useEffect(() => {
    // Remove existing structured data
    const existingScripts = document.querySelectorAll('script[type="application/ld+json"][data-seo="vehicle"]');
    existingScripts.forEach(script => script.remove());

    // Add new structured data
    structuredData.forEach((data) => {
      const script = document.createElement('script');
      script.type = 'application/ld+json';
      script.setAttribute('data-seo', 'vehicle');
      script.textContent = JSON.stringify(data);
      document.head.appendChild(script);
    });

    return () => {
      const scripts = document.querySelectorAll('script[type="application/ld+json"][data-seo="vehicle"]');
      scripts.forEach(script => script.remove());
    };
  }, [structuredData]);

  const keywords = [
    vehicle.make,
    vehicle.model,
    vehicle.year.toString(),
    vehicle.condition === 'new' ? 'nuevo' : 'usado',
    vehicle.fuelType,
    vehicle.transmission,
    'venta',
    'República Dominicana',
  ].filter(Boolean);

  return (
    <SEO
      title={`${vehicle.year} ${vehicle.make} ${vehicle.model} - ${vehicle.condition === 'new' ? 'Nuevo' : 'Usado'}`}
      description={`${vehicle.year} ${vehicle.make} ${vehicle.model} en venta. ${vehicle.mileage.toLocaleString()} km, ${vehicle.transmission}, ${vehicle.fuelType}. ${vehicle.description.slice(0, 150)}...`}
      image={vehicle.primaryImageUrl || vehicle.images[0]?.url}
      type="product"
      keywords={keywords}
    />
  );
};

// ============================================
// Property Structured Data
// ============================================

export const PropertySEO: React.FC<PropertySEOProps> = ({
  property,
  agentName = 'CarDealer Marketplace',
  agentPhone,
}) => {
  const structuredData = useMemo(() => {
    const baseUrl = typeof window !== 'undefined' ? window.location.origin : '';
    
    // Get property type for schema
    const getSchemaType = () => {
      switch (property.propertyType) {
        case 'house': return 'House';
        case 'apartment': return 'Apartment';
        case 'condo': return 'Apartment';
        case 'townhouse': return 'House';
        case 'land': return 'LandOrLot';
        case 'commercial': return 'RealEstateListing';
        case 'office': return 'RealEstateListing';
        case 'warehouse': return 'RealEstateListing';
        case 'building': return 'RealEstateListing';
        default: return 'RealEstateListing';
      }
    };

    // Property structured data
    const propertySchema = {
      '@context': 'https://schema.org',
      '@type': getSchemaType(),
      '@id': `${baseUrl}/properties/${property.id}`,
      name: property.title,
      description: property.description,
      image: property.images.map(img => img.url),
      url: `${baseUrl}/properties/${property.id}`,
      address: {
        '@type': 'PostalAddress',
        streetAddress: property.location.address,
        addressLocality: property.location.city,
        addressRegion: property.location.state,
        postalCode: property.location.zipCode,
        addressCountry: 'DO',
      },
      geo: property.location.latitude && property.location.longitude ? {
        '@type': 'GeoCoordinates',
        latitude: property.location.latitude,
        longitude: property.location.longitude,
      } : undefined,
      floorSize: property.totalArea ? {
        '@type': 'QuantitativeValue',
        value: property.totalArea,
        unitCode: 'MTK',
      } : undefined,
      numberOfRooms: (property.bedrooms || 0) + (property.bathrooms || 0),
      numberOfBedrooms: property.bedrooms,
      numberOfBathroomsTotal: property.bathrooms,
      amenityFeature: property.amenities?.map(amenity => ({
        '@type': 'LocationFeatureSpecification',
        name: amenity,
        value: true,
      })),
      offers: {
        '@type': 'Offer',
        price: property.price,
        priceCurrency: property.currency,
        availability: property.status === 'active' 
          ? 'https://schema.org/InStock' 
          : 'https://schema.org/OutOfStock',
        priceSpecification: {
          '@type': 'PriceSpecification',
          price: property.price,
          priceCurrency: property.currency,
          ...(property.listingType === 'rent' && {
            billingDuration: 'P1M',
          }),
        },
        seller: {
          '@type': 'RealEstateAgent',
          name: agentName,
          telephone: agentPhone,
        },
      },
    };

    // Breadcrumb
    const breadcrumbSchema = {
      '@context': 'https://schema.org',
      '@type': 'BreadcrumbList',
      itemListElement: [
        {
          '@type': 'ListItem',
          position: 1,
          name: 'Inicio',
          item: baseUrl,
        },
        {
          '@type': 'ListItem',
          position: 2,
          name: 'Inmuebles',
          item: `${baseUrl}/properties`,
        },
        {
          '@type': 'ListItem',
          position: 3,
          name: property.title,
          item: `${baseUrl}/properties/${property.id}`,
        },
      ],
    };

    return [propertySchema, breadcrumbSchema];
  }, [property, agentName, agentPhone]);

  useEffect(() => {
    // Remove existing structured data
    const existingScripts = document.querySelectorAll('script[type="application/ld+json"][data-seo="property"]');
    existingScripts.forEach(script => script.remove());

    // Add new structured data
    structuredData.forEach((data) => {
      const script = document.createElement('script');
      script.type = 'application/ld+json';
      script.setAttribute('data-seo', 'property');
      script.textContent = JSON.stringify(data);
      document.head.appendChild(script);
    });

    return () => {
      const scripts = document.querySelectorAll('script[type="application/ld+json"][data-seo="property"]');
      scripts.forEach(script => script.remove());
    };
  }, [structuredData]);

  const propertyTypeLabel: Record<string, string> = {
    house: 'Casa',
    apartment: 'Apartamento',
    condo: 'Condominio',
    townhouse: 'Casa Adosada',
    land: 'Terreno',
    commercial: 'Local Comercial',
    office: 'Oficina',
    warehouse: 'Almacén',
    building: 'Edificio',
  };

  const listingTypeLabel = property.listingType === 'rent' ? 'en Alquiler' : 'en Venta';
  const typeLabel = propertyTypeLabel[property.propertyType] || 'Propiedad';

  const keywords = [
    typeLabel,
    listingTypeLabel,
    property.location.city,
    property.location.state,
    `${property.bedrooms || 0} habitaciones`,
    `${property.bathrooms || 0} baños`,
    property.totalArea ? `${property.totalArea} m²` : '',
    'bienes raíces',
    'República Dominicana',
  ].filter(Boolean);

  return (
    <SEO
      title={`${typeLabel} ${listingTypeLabel} en ${property.location.city}`}
      description={`${typeLabel} ${listingTypeLabel}. ${property.bedrooms || 0} hab, ${property.bathrooms || 0} baños${property.totalArea ? `, ${property.totalArea} m²` : ''}. ${property.description.slice(0, 150)}...`}
      image={property.primaryImageUrl || property.images[0]?.url}
      type="product"
      keywords={keywords}
    />
  );
};

// ============================================
// Organization Schema (for homepage/about)
// ============================================

export const OrganizationSchema: React.FC = () => {
  useEffect(() => {
    const baseUrl = typeof window !== 'undefined' ? window.location.origin : '';
    
    const schema = {
      '@context': 'https://schema.org',
      '@type': 'Organization',
      name: 'CarDealer Marketplace',
      description: 'Marketplace de vehículos e inmuebles en República Dominicana',
      url: baseUrl,
      logo: `${baseUrl}/logo.png`,
      sameAs: [
        'https://facebook.com/cardealer',
        'https://instagram.com/cardealer',
        'https://twitter.com/cardealer',
      ],
      contactPoint: {
        '@type': 'ContactPoint',
        telephone: '+1-809-555-0123',
        contactType: 'customer service',
        areaServed: 'DO',
        availableLanguage: ['Spanish', 'English'],
      },
      address: {
        '@type': 'PostalAddress',
        addressCountry: 'DO',
        addressRegion: 'Santo Domingo',
      },
    };

    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.setAttribute('data-seo', 'organization');
    script.textContent = JSON.stringify(schema);
    document.head.appendChild(script);

    return () => {
      const existingScript = document.querySelector('script[type="application/ld+json"][data-seo="organization"]');
      existingScript?.remove();
    };
  }, []);

  return null;
};

// ============================================
// Search Action Schema (for sitelinks search box)
// ============================================

export const SearchActionSchema: React.FC = () => {
  useEffect(() => {
    const baseUrl = typeof window !== 'undefined' ? window.location.origin : '';
    
    const schema = {
      '@context': 'https://schema.org',
      '@type': 'WebSite',
      name: 'CarDealer Marketplace',
      url: baseUrl,
      potentialAction: {
        '@type': 'SearchAction',
        target: {
          '@type': 'EntryPoint',
          urlTemplate: `${baseUrl}/search?q={search_term_string}`,
        },
        'query-input': 'required name=search_term_string',
      },
    };

    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.setAttribute('data-seo', 'search');
    script.textContent = JSON.stringify(schema);
    document.head.appendChild(script);

    return () => {
      const existingScript = document.querySelector('script[type="application/ld+json"][data-seo="search"]');
      existingScript?.remove();
    };
  }, []);

  return null;
};

export default SEO;
