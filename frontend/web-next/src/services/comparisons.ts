/**
 * Comparison Service - API client for vehicle comparisons
 * Connects via API Gateway to ComparisonService
 */

import { apiClient } from '@/lib/api-client';
import type { Vehicle, VehicleCardData } from '@/types';

// ============================================================
// API TYPES
// ============================================================

export interface VehicleComparison {
  id: string;
  userId?: string;
  sessionId?: string;
  name?: string;
  vehicles: ComparisonVehicle[];
  shareToken?: string;
  isPublic: boolean;
  createdAt: string;
  updatedAt: string;
  expiresAt?: string;
}

export interface ComparisonVehicle {
  vehicleId: string;
  vehicle: VehicleCardData;
  order: number;
  addedAt: string;
}

export interface ComparisonSpec {
  category: string;
  specs: SpecComparison[];
}

export interface SpecComparison {
  label: string;
  key: string;
  values: (string | number | boolean | null)[];
  type: 'text' | 'number' | 'boolean' | 'currency' | 'rating';
  highlight?: 'min' | 'max' | 'none';
  unit?: string;
}

export interface CreateComparisonRequest {
  vehicleIds: string[];
  name?: string;
  isPublic?: boolean;
}

export interface UpdateComparisonRequest {
  name?: string;
  isPublic?: boolean;
}

export interface ComparisonShareResult {
  shareUrl: string;
  shareToken: string;
  expiresAt: string;
}

// ============================================================
// LOCAL STORAGE KEY
// ============================================================

const COMPARISON_STORAGE_KEY = 'okla_comparison_vehicles';
const MAX_COMPARISON_VEHICLES = 3;

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get current user's comparisons
 */
export async function getComparisons(): Promise<VehicleComparison[]> {
  const response = await apiClient.get<VehicleComparison[]>('/api/vehiclecomparisons');
  return response.data;
}

/**
 * Get comparison by ID
 */
export async function getComparisonById(id: string): Promise<VehicleComparison> {
  const response = await apiClient.get<VehicleComparison>(`/api/vehiclecomparisons/${id}`);
  return response.data;
}

/**
 * Get comparison by share token (public access)
 */
export async function getComparisonByShareToken(token: string): Promise<VehicleComparison> {
  const response = await apiClient.get<VehicleComparison>(
    `/api/vehiclecomparisons/shared/${token}`
  );
  return response.data;
}

/**
 * Create a new comparison
 */
export async function createComparison(data: CreateComparisonRequest): Promise<VehicleComparison> {
  const response = await apiClient.post<VehicleComparison>('/api/vehiclecomparisons', data);
  return response.data;
}

/**
 * Update comparison
 */
export async function updateComparison(
  id: string,
  data: UpdateComparisonRequest
): Promise<VehicleComparison> {
  const response = await apiClient.put<VehicleComparison>(`/api/vehiclecomparisons/${id}`, data);
  return response.data;
}

/**
 * Delete comparison
 */
export async function deleteComparison(id: string): Promise<void> {
  await apiClient.delete(`/api/vehiclecomparisons/${id}`);
}

/**
 * Add vehicle to comparison
 */
export async function addVehicleToComparison(
  comparisonId: string,
  vehicleId: string
): Promise<VehicleComparison> {
  const response = await apiClient.post<VehicleComparison>(
    `/api/vehiclecomparisons/${comparisonId}/vehicles`,
    { vehicleId }
  );
  return response.data;
}

/**
 * Remove vehicle from comparison
 */
export async function removeVehicleFromComparison(
  comparisonId: string,
  vehicleId: string
): Promise<VehicleComparison> {
  const response = await apiClient.delete<VehicleComparison>(
    `/api/vehiclecomparisons/${comparisonId}/vehicles/${vehicleId}`
  );
  return response.data;
}

/**
 * Generate share link for comparison
 */
export async function shareComparison(comparisonId: string): Promise<ComparisonShareResult> {
  const response = await apiClient.post<ComparisonShareResult>(
    `/api/vehiclecomparisons/${comparisonId}/share`
  );
  return response.data;
}

/**
 * Get comparison specs (detailed comparison data)
 */
export async function getComparisonSpecs(vehicleIds: string[]): Promise<ComparisonSpec[]> {
  const response = await apiClient.post<ComparisonSpec[]>('/api/vehiclecomparisons/compare', {
    vehicleIds,
  });
  return response.data;
}

// ============================================================
// LOCAL STORAGE FUNCTIONS (Guest users)
// ============================================================

/**
 * Get vehicles from local comparison (for guest users)
 */
export function getLocalComparisonVehicles(): string[] {
  if (typeof window === 'undefined') return [];
  try {
    const stored = localStorage.getItem(COMPARISON_STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

/**
 * Add vehicle to local comparison
 */
export function addToLocalComparison(vehicleId: string): string[] {
  const vehicles = getLocalComparisonVehicles();
  if (vehicles.includes(vehicleId)) return vehicles;
  if (vehicles.length >= MAX_COMPARISON_VEHICLES) {
    // Remove oldest and add new
    vehicles.shift();
  }
  vehicles.push(vehicleId);
  localStorage.setItem(COMPARISON_STORAGE_KEY, JSON.stringify(vehicles));
  return vehicles;
}

/**
 * Remove vehicle from local comparison
 */
export function removeFromLocalComparison(vehicleId: string): string[] {
  const vehicles = getLocalComparisonVehicles().filter(id => id !== vehicleId);
  localStorage.setItem(COMPARISON_STORAGE_KEY, JSON.stringify(vehicles));
  return vehicles;
}

/**
 * Clear local comparison
 */
export function clearLocalComparison(): void {
  localStorage.removeItem(COMPARISON_STORAGE_KEY);
}

/**
 * Check if vehicle is in local comparison
 */
export function isInLocalComparison(vehicleId: string): boolean {
  return getLocalComparisonVehicles().includes(vehicleId);
}

/**
 * Get local comparison count
 */
export function getLocalComparisonCount(): number {
  return getLocalComparisonVehicles().length;
}

// ============================================================
// COMPARISON SPEC HELPERS
// ============================================================

/**
 * Get spec categories for comparison
 */
export const COMPARISON_CATEGORIES = [
  {
    id: 'general',
    name: 'General',
    specs: ['make', 'model', 'year', 'trim', 'bodyType', 'condition'],
  },
  {
    id: 'price',
    name: 'Precio',
    specs: ['price', 'dealRating', 'marketPrice', 'pricePerMile'],
  },
  {
    id: 'performance',
    name: 'Rendimiento',
    specs: ['horsepower', 'torque', 'engineSize', 'acceleration0to60'],
  },
  {
    id: 'efficiency',
    name: 'Eficiencia',
    specs: ['fuelType', 'mpgCity', 'mpgHighway', 'mpgCombined', 'range'],
  },
  {
    id: 'specs',
    name: 'Especificaciones',
    specs: ['mileage', 'transmission', 'drivetrain', 'doors', 'seats'],
  },
  {
    id: 'features',
    name: 'Características',
    specs: ['sunroof', 'leatherSeats', 'navigation', 'backupCamera', 'bluetooth'],
  },
  {
    id: 'safety',
    name: 'Seguridad',
    specs: ['airbags', 'abs', 'stabilityControl', 'blindSpotMonitor', 'laneDeparture'],
  },
];

/**
 * Build comparison specs from vehicles
 */
export function buildComparisonSpecs(vehicles: Vehicle[]): ComparisonSpec[] {
  return COMPARISON_CATEGORIES.map(category => ({
    category: category.name,
    specs: category.specs.map(specKey => ({
      label: getSpecLabel(specKey),
      key: specKey,
      values: vehicles.map(v => getVehicleSpecValue(v, specKey)),
      type: getSpecType(specKey),
      highlight: getSpecHighlight(specKey),
      unit: getSpecUnit(specKey),
    })),
  }));
}

function getSpecLabel(key: string): string {
  const labels: Record<string, string> = {
    make: 'Marca',
    model: 'Modelo',
    year: 'Año',
    trim: 'Versión',
    bodyType: 'Tipo',
    condition: 'Condición',
    price: 'Precio',
    dealRating: 'Calificación',
    marketPrice: 'Precio Mercado',
    pricePerMile: 'Precio/Km',
    horsepower: 'Potencia',
    torque: 'Torque',
    engineSize: 'Motor',
    acceleration0to60: '0-100 km/h',
    fuelType: 'Combustible',
    mpgCity: 'Consumo Ciudad',
    mpgHighway: 'Consumo Carretera',
    mpgCombined: 'Consumo Combinado',
    range: 'Autonomía',
    mileage: 'Kilometraje',
    transmission: 'Transmisión',
    drivetrain: 'Tracción',
    doors: 'Puertas',
    seats: 'Asientos',
    sunroof: 'Techo Solar',
    leatherSeats: 'Asientos Cuero',
    navigation: 'Navegación',
    backupCamera: 'Cámara Reversa',
    bluetooth: 'Bluetooth',
    airbags: 'Airbags',
    abs: 'ABS',
    stabilityControl: 'Control Estabilidad',
    blindSpotMonitor: 'Monitor Punto Ciego',
    laneDeparture: 'Alerta Carril',
  };
  return labels[key] || key;
}

function getSpecType(key: string): SpecComparison['type'] {
  const types: Record<string, SpecComparison['type']> = {
    price: 'currency',
    marketPrice: 'currency',
    pricePerMile: 'currency',
    mileage: 'number',
    horsepower: 'number',
    torque: 'number',
    doors: 'number',
    seats: 'number',
    sunroof: 'boolean',
    leatherSeats: 'boolean',
    navigation: 'boolean',
    backupCamera: 'boolean',
    bluetooth: 'boolean',
    airbags: 'boolean',
    abs: 'boolean',
    stabilityControl: 'boolean',
    blindSpotMonitor: 'boolean',
    laneDeparture: 'boolean',
  };
  return types[key] || 'text';
}

function getSpecHighlight(key: string): SpecComparison['highlight'] {
  const highlights: Record<string, SpecComparison['highlight']> = {
    price: 'min',
    mileage: 'min',
    horsepower: 'max',
    torque: 'max',
    mpgCombined: 'max',
    range: 'max',
  };
  return highlights[key] || 'none';
}

function getSpecUnit(key: string): string | undefined {
  const units: Record<string, string> = {
    mileage: 'km',
    horsepower: 'hp',
    torque: 'lb-ft',
    mpgCity: 'km/l',
    mpgHighway: 'km/l',
    mpgCombined: 'km/l',
    range: 'km',
    acceleration0to60: 's',
  };
  return units[key];
}

function getVehicleSpecValue(vehicle: Vehicle, key: string): string | number | boolean | null {
  const mapping: Record<string, unknown> = {
    make: vehicle.make,
    model: vehicle.model,
    year: vehicle.year,
    trim: vehicle.trim,
    bodyType: vehicle.bodyType,
    condition: vehicle.condition,
    price: vehicle.price,
    dealRating: vehicle.dealRating,
    marketPrice: vehicle.marketPrice,
    mileage: vehicle.mileage,
    transmission: vehicle.transmission,
    fuelType: vehicle.fuelType,
    drivetrain: vehicle.drivetrain,
    doors: vehicle.doors,
    seats: vehicle.seats,
    horsepower: vehicle.horsepower,
    engineSize: vehicle.engineSize,
  };

  const value = mapping[key];
  if (value === undefined) return null;
  return value as string | number | boolean | null;
}

// ============================================================
// SERVICE EXPORT
// ============================================================

export const comparisonService = {
  // API functions
  getComparisons,
  getComparisonById,
  getComparisonByShareToken,
  createComparison,
  updateComparison,
  deleteComparison,
  addVehicleToComparison,
  removeVehicleFromComparison,
  shareComparison,
  getComparisonSpecs,
  // Local storage functions
  getLocalComparisonVehicles,
  addToLocalComparison,
  removeFromLocalComparison,
  clearLocalComparison,
  isInLocalComparison,
  getLocalComparisonCount,
  // Helpers
  buildComparisonSpecs,
  COMPARISON_CATEGORIES,
  MAX_COMPARISON_VEHICLES,
};

export default comparisonService;
