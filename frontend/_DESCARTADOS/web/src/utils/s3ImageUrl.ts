/**
 * S3 Image URL Utilities
 * 
 * Construye URLs de imágenes almacenadas en AWS S3 a partir de photoIds.
 * 
 * Bucket: okla-images-2026
 * Region: us-east-2
 * 
 * Estructura S3:
 * - frontend/assets/vehicles/sale/{photoId}.jpg
 * - frontend/assets/vehicles/rent/{photoId}.jpg
 * - frontend/assets/properties/sale/{photoId}.jpg
 * - frontend/assets/lodging/{photoId}.jpg
 */

// Configuración S3
const S3_BUCKET = 'okla-images-2026';
const S3_REGION = 'us-east-2';
const S3_BASE_URL = `https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com`;

// Categorías soportadas
export type ImageCategory = 'vehicles' | 'properties' | 'lodging';
export type ImageType = 'sale' | 'rent' | '';

/**
 * Construye la URL completa de S3 para una imagen
 * 
 * @param photoId - El ID de la foto (ej: "photo-1618843479313-40f8afb4b4d8")
 * @param category - La categoría (vehicles, properties, lodging)
 * @param type - El tipo (sale, rent) - vacío para lodging
 * @returns URL completa de S3
 * 
 * @example
 * getS3ImageUrl('photo-1618843479313-40f8afb4b4d8', 'vehicles', 'sale')
 * // Returns: https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1618843479313-40f8afb4b4d8.jpg
 */
export function getS3ImageUrl(
  photoId: string,
  category: ImageCategory,
  type: ImageType = 'sale'
): string {
  // Si ya es una URL completa (fallback para datos antiguos), retornarla directamente
  if (photoId.startsWith('http://') || photoId.startsWith('https://')) {
    return photoId;
  }

  // Construir path según categoría
  const path = type ? `${category}/${type}` : category;
  
  return `${S3_BASE_URL}/frontend/assets/${path}/${photoId}.jpg`;
}

/**
 * Construye URL de S3 para vehículos en venta
 */
export function getVehicleSaleImageUrl(photoId: string): string {
  return getS3ImageUrl(photoId, 'vehicles', 'sale');
}

/**
 * Construye URL de S3 para vehículos en renta
 */
export function getVehicleRentImageUrl(photoId: string): string {
  return getS3ImageUrl(photoId, 'vehicles', 'rent');
}

/**
 * Construye URL de S3 para propiedades en venta
 */
export function getPropertySaleImageUrl(photoId: string): string {
  return getS3ImageUrl(photoId, 'properties', 'sale');
}

/**
 * Construye URL de S3 para hospedaje
 */
export function getLodgingImageUrl(photoId: string): string {
  return getS3ImageUrl(photoId, 'lodging', '');
}

/**
 * Construye URL de thumbnail de S3
 */
export function getS3ThumbnailUrl(
  photoId: string,
  category: ImageCategory,
  type: ImageType = 'sale'
): string {
  return getS3ImageUrl(photoId, category, type);
}

/**
 * Extrae el photoId de una URL de Unsplash
 * Útil para migración de datos existentes
 * 
 * @example
 * extractPhotoIdFromUnsplash('https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&h=600')
 * // Returns: 'photo-1618843479313-40f8afb4b4d8'
 */
export function extractPhotoIdFromUnsplash(url: string): string | null {
  const match = url.match(/photo-([a-zA-Z0-9-]+)/);
  return match ? `photo-${match[1]}` : null;
}

/**
 * Verifica si un string es un photoId válido
 */
export function isValidPhotoId(value: string): boolean {
  return /^photo-[a-zA-Z0-9-]+$/.test(value);
}

/**
 * Placeholder image para cuando no hay imagen disponible
 */
export function getPlaceholderImageUrl(category: ImageCategory = 'vehicles'): string {
  const placeholders: Record<ImageCategory, string> = {
    vehicles: 'https://via.placeholder.com/800x600/1e40af/ffffff?text=No+Image',
    properties: 'https://via.placeholder.com/800x600/047857/ffffff?text=No+Image',
    lodging: 'https://via.placeholder.com/800x600/7c3aed/ffffff?text=No+Image',
  };
  return placeholders[category];
}

// Export configuration for debugging
export const S3_CONFIG = {
  bucket: S3_BUCKET,
  region: S3_REGION,
  baseUrl: S3_BASE_URL,
} as const;
