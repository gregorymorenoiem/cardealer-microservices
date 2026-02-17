/**
 * S3 Image URL Helper
 * 
 * Construye URLs de imágenes desde photoId almacenado en base de datos.
 * Estructura S3: okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/{category}/{type}/{photoId}.jpg
 * 
 * @example
 * // En la DB se guarda: "photo-1618843479313-40f8afb4b4d8"
 * // URL generada: https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1618843479313-40f8afb4b4d8.jpg
 */

// ========== CONFIGURACIÓN ==========
const S3_BUCKET = 'okla-images-2026';
const S3_REGION = 'us-east-2';
export const S3_BASE_URL = `https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com/frontend/assets`;

// ========== TIPOS ==========
export type ImageCategory = 'vehicles' | 'properties' | 'lodging';
export type ImageType = 'sale' | 'rent';

export interface ImageOptions {
  /** Ancho de la imagen (solo para thumbnails locales, S3 sirve imagen completa) */
  width?: number;
  /** Alto de la imagen */
  height?: number;
  /** Usar thumbnail si está disponible */
  thumbnail?: boolean;
}

// ========== FUNCIONES PRINCIPALES ==========

/**
 * Construye la URL completa de S3 desde un photoId
 * 
 * @param photoId - ID de la foto (ej: "photo-1618843479313-40f8afb4b4d8")
 * @param category - Categoría: 'vehicles', 'properties', 'lodging'
 * @param type - Tipo: 'sale' o 'rent' (opcional para lodging)
 * @returns URL completa de S3
 * 
 * @example
 * getImageUrl('photo-1618843479313-40f8afb4b4d8', 'vehicles', 'sale')
 * // => https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1618843479313-40f8afb4b4d8.jpg
 */
export function getImageUrl(
  photoId: string,
  category: ImageCategory,
  type?: ImageType
): string {
  if (!photoId) {
    return getPlaceholderUrl(category);
  }

  // Normalizar photoId (quitar extensión si la tiene)
  const cleanPhotoId = photoId.replace(/\.(jpg|jpeg|png|webp)$/i, '');

  // Construir path
  if (type) {
    return `${S3_BASE_URL}/${category}/${type}/${cleanPhotoId}.jpg`;
  }
  return `${S3_BASE_URL}/${category}/${cleanPhotoId}.jpg`;
}

/**
 * Construye URL para vehículos en venta
 */
export function getVehicleSaleImageUrl(photoId: string): string {
  return getImageUrl(photoId, 'vehicles', 'sale');
}

/**
 * Construye URL para vehículos en renta
 */
export function getVehicleRentImageUrl(photoId: string): string {
  return getImageUrl(photoId, 'vehicles', 'rent');
}

/**
 * Construye URL para propiedades en venta
 */
export function getPropertySaleImageUrl(photoId: string): string {
  return getImageUrl(photoId, 'properties', 'sale');
}

/**
 * Construye URL para propiedades en renta
 */
export function getPropertyRentImageUrl(photoId: string): string {
  return getImageUrl(photoId, 'properties', 'rent');
}

/**
 * Construye URL para hospedaje
 */
export function getLodgingImageUrl(photoId: string): string {
  return getImageUrl(photoId, 'lodging');
}

// ========== UTILIDADES ==========

/**
 * Retorna URL de placeholder cuando no hay imagen
 */
export function getPlaceholderUrl(category: ImageCategory): string {
  const placeholders: Record<ImageCategory, string> = {
    vehicles: `${S3_BASE_URL}/vehicles/placeholder.jpg`,
    properties: `${S3_BASE_URL}/properties/placeholder.jpg`,
    lodging: `${S3_BASE_URL}/lodging/placeholder.jpg`,
  };
  return placeholders[category];
}

/**
 * Extrae el photoId de una URL completa de Unsplash o S3
 * 
 * @example
 * extractPhotoId('https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800')
 * // => 'photo-1618843479313-40f8afb4b4d8'
 */
export function extractPhotoId(url: string): string {
  if (!url) return '';
  
  // Formato Unsplash: https://images.unsplash.com/photo-XXXXX?params
  const unsplashMatch = url.match(/photo-([a-zA-Z0-9-]+)/);
  if (unsplashMatch) {
    return `photo-${unsplashMatch[1]}`;
  }

  // Formato S3: .../photo-XXXXX.jpg
  const s3Match = url.match(/(photo-[a-zA-Z0-9-]+)\.jpg/);
  if (s3Match) {
    return s3Match[1];
  }

  return '';
}

/**
 * Verifica si una URL es de S3 (nuestro bucket)
 */
export function isS3Url(url: string): boolean {
  return url.includes(S3_BUCKET);
}

/**
 * Verifica si una URL es de Unsplash
 */
export function isUnsplashUrl(url: string): boolean {
  return url.includes('unsplash.com');
}

/**
 * Convierte una URL de Unsplash a URL de S3
 * Útil durante la migración
 * 
 * @param unsplashUrl - URL original de Unsplash
 * @param category - Categoría destino
 * @param type - Tipo (sale/rent)
 * @returns URL de S3
 */
export function convertUnsplashToS3(
  unsplashUrl: string,
  category: ImageCategory,
  type?: ImageType
): string {
  const photoId = extractPhotoId(unsplashUrl);
  if (!photoId) {
    console.warn('Could not extract photoId from URL:', unsplashUrl);
    return unsplashUrl; // Retornar original si no se puede convertir
  }
  return getImageUrl(photoId, category, type);
}

// ========== MAPEO DE CATEGORÍAS ==========

/**
 * Mapea nombre de categoría en español a ImageCategory
 */
export function categoryFromSpanish(spanishCategory: string): ImageCategory {
  const mapping: Record<string, ImageCategory> = {
    'Vehículos': 'vehicles',
    'Renta de Vehículos': 'vehicles',
    'Propiedades': 'properties',
    'Hospedaje': 'lodging',
  };
  return mapping[spanishCategory] || 'vehicles';
}

/**
 * Mapea nombre de categoría en español a ImageType
 */
export function typeFromSpanish(spanishCategory: string): ImageType | undefined {
  const mapping: Record<string, ImageType | undefined> = {
    'Vehículos': 'sale',
    'Renta de Vehículos': 'rent',
    'Propiedades': 'sale',
    'Hospedaje': undefined,
  };
  return mapping[spanishCategory];
}

/**
 * Helper completo que toma una URL de cualquier origen y la convierte a S3
 */
export function normalizeImageUrl(
  url: string,
  category: ImageCategory,
  type?: ImageType
): string {
  // Si ya es S3, retornar como está
  if (isS3Url(url)) {
    return url;
  }

  // Si es Unsplash, convertir a S3
  if (isUnsplashUrl(url)) {
    return convertUnsplashToS3(url, category, type);
  }

  // Si es un photoId directo (sin http)
  if (url.startsWith('photo-')) {
    return getImageUrl(url, category, type);
  }

  // URL desconocida, retornar como está
  return url;
}
