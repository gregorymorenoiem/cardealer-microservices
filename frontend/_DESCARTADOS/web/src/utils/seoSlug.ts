/**
 * SEO Slug Utilities
 * Generates SEO-friendly URLs for vehicles and listings
 */

/**
 * Converts a string to a URL-friendly slug
 * @param text - The text to convert
 * @returns URL-friendly slug
 */
export function slugify(text: string): string {
  return text
    .toString()
    .toLowerCase()
    .trim()
    .replace(/\s+/g, '-')        // Replace spaces with -
    .replace(/&/g, '-and-')      // Replace & with 'and'
    .replace(/[^\w\-]+/g, '')    // Remove all non-word chars
    .replace(/\-\-+/g, '-')      // Replace multiple - with single -
    .replace(/^-+/, '')          // Trim - from start of text
    .replace(/-+$/, '');         // Trim - from end of text
}

/**
 * Generates a SEO-friendly URL for a vehicle
 * Format: /vehicles/{year}-{make}-{model}-{id}
 * Example: /vehicles/2024-toyota-camry-se-abc123
 * 
 * @param vehicle - Vehicle object with year, make, model, and id
 * @returns SEO-friendly URL path
 */
export function generateVehicleUrl(vehicle: {
  id: string;
  year: number;
  make: string;
  model: string;
  trim?: string;
}): string {
  const parts = [
    vehicle.year.toString(),
    vehicle.make,
    vehicle.model,
    vehicle.trim
  ].filter(Boolean);
  
  const slug = slugify(parts.join(' '));
  return `/vehicles/${slug}-${vehicle.id}`;
}

/**
 * Generates a SEO-friendly URL from a title string
 * Format: /vehicles/{title-slug}-{id}
 * Example: /vehicles/2024-tesla-model-3-long-range-abc123
 * 
 * @param id - The unique identifier
 * @param title - The listing title
 * @param basePath - The base path (default: /vehicles)
 * @returns SEO-friendly URL path
 */
export function generateListingUrl(id: string, title: string, basePath: string = '/vehicles'): string {
  const slug = slugify(title);
  return `${basePath}/${slug}-${id}`;
}

/**
 * Generates a SEO-friendly URL for a property
 * Format: /properties/{type}-{bedrooms}bd-{location}-{id}
 * 
 * @param property - Property object
 * @returns SEO-friendly URL path
 */
export function generatePropertyUrl(property: {
  id: string;
  type: string;
  bedrooms?: number;
  location: string;
}): string {
  const parts = [
    property.type,
    property.bedrooms ? `${property.bedrooms}bd` : null,
    property.location.split(',')[0] // City only
  ].filter(Boolean);
  
  const slug = slugify(parts.join(' '));
  return `/properties/${slug}-${property.id}`;
}

/**
 * Extracts the ID from a SEO-friendly URL
 * Works with format: /vehicles/{slug}-{id} or /properties/{slug}-{id}
 * 
 * @param url - The SEO-friendly URL
 * @returns The extracted ID or null if not found
 */
export function extractIdFromSlug(url: string): string | null {
  // Get the last segment after the final slash
  const segments = url.split('/');
  const lastSegment = segments[segments.length - 1];
  
  // Extract ID from the end (after last hyphen)
  const parts = lastSegment.split('-');
  return parts.length > 0 ? parts[parts.length - 1] : null;
}

/**
 * Validates if a URL matches the expected SEO-friendly format
 * 
 * @param url - The URL to validate
 * @param type - The type of listing ('vehicle' | 'property')
 * @returns Boolean indicating if the URL is valid
 */
export function isValidSeoUrl(url: string, type: 'vehicle' | 'property'): boolean {
  const prefix = type === 'vehicle' ? '/vehicles/' : '/properties/';
  return url.startsWith(prefix) && url.split('-').length >= 2;
}
