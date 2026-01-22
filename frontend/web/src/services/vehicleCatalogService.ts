/**
 * VehicleCatalogService - API client for vehicle catalog (makes, models, trims)
 *
 * Consumido por el formulario SellYourCarPage para:
 * 1. Cargar lista de marcas
 * 2. Cargar modelos de una marca
 * 3. Cargar años disponibles de un modelo
 * 4. Cargar trims con especificaciones completas para auto-fill
 */

import api from './api';

// ============================================================
// API CONFIGURATION
// ============================================================

// Use api instance with refresh token interceptor
const catalogApi = api;

// ============================================================
// TYPES
// ============================================================

export interface VehicleMake {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  country?: string;
  isPopular: boolean;
}

export interface VehicleModel {
  id: string;
  makeId: string;
  makeName: string;
  name: string;
  slug: string;
  vehicleType: string;
  bodyStyle?: string;
  startYear?: number;
  endYear?: number;
  isPopular: boolean;
}

export interface VehicleTrim {
  id: string;
  modelId: string;
  makeName: string;
  modelName: string;
  name: string;
  slug: string;
  year: number;

  // Specs para auto-fill del formulario
  engineSize?: string;
  horsepower?: number;
  torque?: number;
  fuelType?: string;
  transmission?: string;
  driveType?: string;

  // Fuel economy
  mpgCity?: number;
  mpgHighway?: number;
  mpgCombined?: number;

  // Precio de referencia
  baseMSRP?: number;
}

export interface CatalogStats {
  totalMakes: number;
  totalModels: number;
  totalTrims: number;
  minYear: number;
  maxYear: number;
  lastUpdated: string;
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Obtiene todas las marcas activas
 */
export async function getAllMakes(): Promise<VehicleMake[]> {
  try {
    const response = await catalogApi.get<VehicleMake[]>('/api/catalog/makes');
    return response.data;
  } catch (error) {
    console.error('Error fetching makes:', error);
    return [];
  }
}

/**
 * Obtiene las marcas más populares
 */
export async function getPopularMakes(limit: number = 20): Promise<VehicleMake[]> {
  try {
    const response = await catalogApi.get<VehicleMake[]>(
      `/api/catalog/makes/popular?take=${limit}`
    );
    return response.data;
  } catch (error) {
    console.error('Error fetching popular makes:', error);
    return [];
  }
}

/**
 * Busca marcas por nombre (para autocomplete)
 */
export async function searchMakes(query: string): Promise<VehicleMake[]> {
  if (!query || query.length < 2) return [];

  try {
    const response = await catalogApi.get<VehicleMake[]>(
      `/api/catalog/makes/search?q=${encodeURIComponent(query)}`
    );
    return response.data;
  } catch (error) {
    console.error('Error searching makes:', error);
    return [];
  }
}

/**
 * Obtiene todos los modelos de una marca por slug
 */
export async function getModelsByMake(makeSlug: string): Promise<VehicleModel[]> {
  try {
    const response = await catalogApi.get<VehicleModel[]>(
      `/api/catalog/makes/${encodeURIComponent(makeSlug)}/models`
    );
    return response.data;
  } catch (error) {
    console.error(`Error fetching models for ${makeSlug}:`, error);
    return [];
  }
}

/**
 * Busca modelos dentro de una marca (para autocomplete)
 */
export async function searchModels(makeId: string, query: string): Promise<VehicleModel[]> {
  if (!query || query.length < 2) return [];

  try {
    const response = await catalogApi.get<VehicleModel[]>(
      `/api/catalog/makes/${makeId}/models/search?q=${encodeURIComponent(query)}`
    );
    return response.data;
  } catch (error) {
    console.error('Error searching models:', error);
    return [];
  }
}

/**
 * Obtiene los años disponibles para un modelo
 */
export async function getAvailableYears(modelId: string): Promise<number[]> {
  try {
    const response = await catalogApi.get<number[]>(`/api/catalog/models/${modelId}/years`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching years for model ${modelId}:`, error);
    // Fallback: generar años 2016-2026
    const currentYear = new Date().getFullYear();
    return Array.from({ length: 11 }, (_, i) => currentYear - 10 + i + 1);
  }
}

/**
 * Obtiene todos los trims con especificaciones para un modelo y año
 * Este es el endpoint principal para auto-llenar el formulario
 */
export async function getTrimsByModelAndYear(
  modelId: string,
  year: number
): Promise<VehicleTrim[]> {
  try {
    const response = await catalogApi.get<VehicleTrim[]>(
      `/api/catalog/models/${modelId}/years/${year}/trims`
    );
    return response.data;
  } catch (error) {
    console.error(`Error fetching trims for model ${modelId} year ${year}:`, error);
    return [];
  }
}

/**
 * Obtiene las especificaciones completas de un trim específico
 */
export async function getTrimById(trimId: string): Promise<VehicleTrim | null> {
  try {
    const response = await catalogApi.get<VehicleTrim>(`/api/catalog/trims/${trimId}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching trim ${trimId}:`, error);
    return null;
  }
}

/**
 * Obtiene estadísticas del catálogo
 */
export async function getCatalogStats(): Promise<CatalogStats | null> {
  try {
    const response = await catalogApi.get<CatalogStats>('/api/catalog/stats');
    return response.data;
  } catch (error) {
    console.error('Error fetching catalog stats:', error);
    return null;
  }
}

// ============================================================
// HELPER FUNCTIONS
// ============================================================

/**
 * Convierte un string a slug
 */
export function createSlug(text: string): string {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
}

/**
 * Mapea el tipo de combustible del backend al formato frontend
 */
export function mapFuelTypeToDisplay(fuelType: string): string {
  const mapping: Record<string, string> = {
    Gasoline: 'Gasoline',
    Diesel: 'Diesel',
    Electric: 'Electric',
    Hybrid: 'Hybrid',
    PlugInHybrid: 'Plug-in Hybrid',
    Hydrogen: 'Hydrogen',
    FlexFuel: 'Flex Fuel',
    NaturalGas: 'Natural Gas',
  };
  return mapping[fuelType] || fuelType;
}

/**
 * Mapea la transmisión del backend al formato frontend
 */
export function mapTransmissionToDisplay(transmission: string): string {
  const mapping: Record<string, string> = {
    Automatic: 'Automatic',
    Manual: 'Manual',
    CVT: 'CVT',
    Automated: 'Semi-Automatic',
    DualClutch: 'Dual-Clutch',
  };
  return mapping[transmission] || transmission;
}

/**
 * Mapea el tipo de tracción del backend al formato frontend
 */
export function mapDriveTypeToDisplay(driveType: string): string {
  const mapping: Record<string, string> = {
    FWD: 'FWD',
    RWD: 'RWD',
    AWD: 'AWD',
    FourWD: '4WD',
  };
  return mapping[driveType] || driveType;
}

/**
 * Formatea el precio como moneda
 */
export function formatPrice(price: number, currency: string = 'USD'): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(price);
}

/**
 * Formatea MPG
 */
export function formatMPG(city?: number, highway?: number): string {
  if (city && highway) {
    return `${city}/${highway} MPG`;
  }
  return 'N/A';
}

// ============================================================
// REACT QUERY HOOKS (para uso con TanStack Query)
// ============================================================

// Estos hooks se pueden usar si el proyecto tiene TanStack Query configurado
// import { useQuery } from '@tanstack/react-query';
//
// export function useMakes() {
//   return useQuery({
//     queryKey: ['vehicle-makes'],
//     queryFn: getAllMakes,
//     staleTime: 1000 * 60 * 60, // 1 hora (el catálogo cambia poco)
//   });
// }
//
// export function useModelsByMake(makeSlug: string) {
//   return useQuery({
//     queryKey: ['vehicle-models', makeSlug],
//     queryFn: () => getModelsByMake(makeSlug),
//     enabled: !!makeSlug,
//     staleTime: 1000 * 60 * 60,
//   });
// }
//
// export function useTrims(modelId: string, year: number) {
//   return useQuery({
//     queryKey: ['vehicle-trims', modelId, year],
//     queryFn: () => getTrimsByModelAndYear(modelId, year),
//     enabled: !!modelId && !!year,
//     staleTime: 1000 * 60 * 60,
//   });
// }

export default {
  getAllMakes,
  getPopularMakes,
  searchMakes,
  getModelsByMake,
  searchModels,
  getAvailableYears,
  getTrimsByModelAndYear,
  getTrimById,
  getCatalogStats,
};
