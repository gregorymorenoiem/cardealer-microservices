// ==============================================================================
// Asset Loader - Carga de imágenes desde AWS S3
// Generado automáticamente por: scripts/migrate-assets-to-s3.sh
// ==============================================================================

import assetsMap from '../config/s3-assets-map.json';

/**
 * Obtiene la URL de un asset desde S3
 * @param path - Ruta relativa del asset (ej: 'images/vehicles/car1.jpg')
 * @returns URL completa del asset en S3
 */
export const getAssetUrl = (path: string): string => {
  // Normalizar path (remover / inicial)
  const normalizedPath = path.startsWith('/') ? path.slice(1) : path;
  
  // Buscar en el mapeo
  const url = (assetsMap.assets as Record<string, string>)[normalizedPath];
  
  if (url) {
    return url;
  }
  
  // Fallback: construir URL dinámicamente
  const baseUrl = assetsMap.cdnUrl || assetsMap.baseUrl;
  return `${baseUrl}/${normalizedPath}`;
};

/**
 * Obtiene múltiples URLs de assets
 * @param paths - Array de rutas relativas
 * @returns Array de URLs completas
 */
export const getAssetUrls = (paths: string[]): string[] => {
  return paths.map(path => getAssetUrl(path));
};

/**
 * Precarga una imagen
 * @param url - URL de la imagen
 * @returns Promise que se resuelve cuando la imagen carga
 */
export const preloadImage = (url: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve();
    img.onerror = reject;
    img.src = url;
  });
};

/**
 * Precarga múltiples imágenes
 * @param urls - Array de URLs
 * @returns Promise que se resuelve cuando todas cargan
 */
export const preloadImages = async (urls: string[]): Promise<void> => {
  await Promise.all(urls.map(url => preloadImage(url)));
};

export default {
  getAssetUrl,
  getAssetUrls,
  preloadImage,
  preloadImages
};
