/**
 * SEO Head Component
 * 
 * Dynamic meta tags for SEO optimization using React Helmet Async.
 * Provides structured data, Open Graph, and Twitter Card support.
 */

import { Helmet } from 'react-helmet-async';

export interface SEOHeadProps {
  /** Page title - will be appended with site name */
  title?: string;
  /** Meta description for search results */
  description?: string;
  /** Canonical URL for the page */
  canonicalUrl?: string;
  /** Open Graph image URL */
  image?: string;
  /** Type of content (website, article, product) */
  type?: 'website' | 'article' | 'product';
  /** Author name for articles */
  author?: string;
  /** Published date for articles */
  publishedDate?: string;
  /** Modified date for articles */
  modifiedDate?: string;
  /** Keywords for the page */
  keywords?: string[];
  /** Disable indexing for this page */
  noIndex?: boolean;
  /** Disable following links for this page */
  noFollow?: boolean;
  /** Product price (for product pages) */
  price?: number;
  /** Currency code (for product pages) */
  currency?: string;
  /** Product availability */
  availability?: 'in_stock' | 'out_of_stock' | 'preorder';
  /** Structured data (JSON-LD) */
  structuredData?: object;
}

const SITE_NAME = 'CarDealer';
const DEFAULT_DESCRIPTION = 'Find your perfect vehicle at CarDealer - Dominican Republic\'s premier marketplace for new and used cars, trucks, and SUVs.';
const DEFAULT_IMAGE = '/images/og-default.jpg';
const BASE_URL = import.meta.env.VITE_BASE_URL || 'https://cardealer.do';

export function SEOHead({
  title,
  description = DEFAULT_DESCRIPTION,
  canonicalUrl,
  image = DEFAULT_IMAGE,
  type = 'website',
  author,
  publishedDate,
  modifiedDate,
  keywords = [],
  noIndex = false,
  noFollow = false,
  price,
  currency = 'USD',
  availability,
  structuredData,
}: SEOHeadProps) {
  const fullTitle = title ? `${title} | ${SITE_NAME}` : SITE_NAME;
  const fullUrl = canonicalUrl ? `${BASE_URL}${canonicalUrl}` : BASE_URL;
  const fullImageUrl = image.startsWith('http') ? image : `${BASE_URL}${image}`;
  
  // Build robots meta
  const robotsContent = [
    noIndex ? 'noindex' : 'index',
    noFollow ? 'nofollow' : 'follow',
  ].join(', ');
  
  // Build structured data
  const baseStructuredData = {
    '@context': 'https://schema.org',
    '@type': type === 'product' ? 'Product' : 'WebPage',
    name: title || SITE_NAME,
    description,
    url: fullUrl,
    image: fullImageUrl,
    ...(structuredData || {}),
  };
  
  // Product structured data
  const productData = type === 'product' && price ? {
    ...baseStructuredData,
    '@type': 'Product',
    offers: {
      '@type': 'Offer',
      price,
      priceCurrency: currency,
      availability: availability ? `https://schema.org/${availability === 'in_stock' ? 'InStock' : availability === 'out_of_stock' ? 'OutOfStock' : 'PreOrder'}` : undefined,
    },
  } : baseStructuredData;

  return (
    <Helmet>
      {/* Basic Meta Tags */}
      <title>{fullTitle}</title>
      <meta name="description" content={description} />
      <meta name="robots" content={robotsContent} />
      {keywords.length > 0 && <meta name="keywords" content={keywords.join(', ')} />}
      {author && <meta name="author" content={author} />}
      
      {/* Canonical URL */}
      <link rel="canonical" href={fullUrl} />
      
      {/* Open Graph */}
      <meta property="og:site_name" content={SITE_NAME} />
      <meta property="og:title" content={fullTitle} />
      <meta property="og:description" content={description} />
      <meta property="og:type" content={type} />
      <meta property="og:url" content={fullUrl} />
      <meta property="og:image" content={fullImageUrl} />
      <meta property="og:locale" content="es_DO" />
      
      {/* Twitter Card */}
      <meta name="twitter:card" content="summary_large_image" />
      <meta name="twitter:title" content={fullTitle} />
      <meta name="twitter:description" content={description} />
      <meta name="twitter:image" content={fullImageUrl} />
      
      {/* Article specific */}
      {type === 'article' && publishedDate && (
        <meta property="article:published_time" content={publishedDate} />
      )}
      {type === 'article' && modifiedDate && (
        <meta property="article:modified_time" content={modifiedDate} />
      )}
      {type === 'article' && author && (
        <meta property="article:author" content={author} />
      )}
      
      {/* Product specific */}
      {type === 'product' && price && (
        <>
          <meta property="product:price:amount" content={price.toString()} />
          <meta property="product:price:currency" content={currency} />
        </>
      )}
      
      {/* Structured Data */}
      <script type="application/ld+json">
        {JSON.stringify(type === 'product' ? productData : baseStructuredData)}
      </script>
    </Helmet>
  );
}

export default SEOHead;
