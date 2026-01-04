/**
 * useVehiclesSale - React hooks for vehicles from VehiclesSaleService
 * 
 * This hook connects to VehiclesSaleService microservice (port 15070)
 * using real API data with images stored as photoIds transformed to S3 URLs.
 */

import { useState, useEffect, useCallback } from 'react';
import {
  getVehicles,
  getVehicleById,
  getFeaturedVehicles,
  getLatestVehicles,
} from '@/services/vehiclesSaleService';
import type {
  VehicleListing,
  VehiclesResponse,
  VehicleFilters,
} from '@/services/vehiclesSaleService';

// ============================================================
// HOOK: useVehiclesSaleList (paginated list with filters)
// ============================================================

interface UseVehiclesSaleListResult {
  vehicles: VehicleListing[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  setPage: (page: number) => void;
  setFilters: (filters: VehicleFilters) => void;
}

export const useVehiclesSaleList = (
  initialPage: number = 1,
  initialPageSize: number = 12,
  initialFilters?: VehicleFilters
): UseVehiclesSaleListResult => {
  const [vehicles, setVehicles] = useState<VehicleListing[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(initialPage);
  const [pageSize] = useState(initialPageSize);
  const [totalPages, setTotalPages] = useState(0);
  const [filters, setFilters] = useState<VehicleFilters | undefined>(initialFilters);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchVehicles = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getVehicles(page, pageSize, filters);
      setVehicles(response.vehicles);
      setTotal(response.total);
      setTotalPages(response.totalPages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch vehicles');
      setVehicles([]);
    } finally {
      setIsLoading(false);
    }
  }, [page, pageSize, filters]);

  useEffect(() => {
    fetchVehicles();
  }, [fetchVehicles]);

  return {
    vehicles,
    total,
    page,
    pageSize,
    totalPages,
    isLoading,
    error,
    refetch: fetchVehicles,
    setPage,
    setFilters,
  };
};

// ============================================================
// HOOK: useVehicleSaleDetail (single vehicle by ID)
// ============================================================

interface UseVehicleSaleDetailResult {
  vehicle: VehicleListing | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export const useVehicleSaleDetail = (id: string): UseVehicleSaleDetailResult => {
  const [vehicle, setVehicle] = useState<VehicleListing | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchVehicle = useCallback(async () => {
    if (!id) return;
    setIsLoading(true);
    setError(null);
    try {
      const result = await getVehicleById(id);
      setVehicle(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch vehicle');
      setVehicle(null);
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchVehicle();
  }, [fetchVehicle]);

  return {
    vehicle,
    isLoading,
    error,
    refetch: fetchVehicle,
  };
};

// ============================================================
// HOOK: useFeaturedVehiclesSale
// ============================================================

interface UseFeaturedVehiclesSaleResult {
  vehicles: VehicleListing[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export const useFeaturedVehiclesSale = (limit: number = 6): UseFeaturedVehiclesSaleResult => {
  const [vehicles, setVehicles] = useState<VehicleListing[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchVehicles = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await getFeaturedVehicles(limit);
      setVehicles(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch featured vehicles');
      setVehicles([]);
    } finally {
      setIsLoading(false);
    }
  }, [limit]);

  useEffect(() => {
    fetchVehicles();
  }, [fetchVehicles]);

  return {
    vehicles,
    isLoading,
    error,
    refetch: fetchVehicles,
  };
};

// ============================================================
// HOOK: useLatestVehiclesSale
// ============================================================

export const useLatestVehiclesSale = (limit: number = 6): UseFeaturedVehiclesSaleResult => {
  const [vehicles, setVehicles] = useState<VehicleListing[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchVehicles = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await getLatestVehicles(limit);
      setVehicles(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch latest vehicles');
      setVehicles([]);
    } finally {
      setIsLoading(false);
    }
  }, [limit]);

  useEffect(() => {
    fetchVehicles();
  }, [fetchVehicles]);

  return {
    vehicles,
    isLoading,
    error,
    refetch: fetchVehicles,
  };
};

// ============================================================
// HOOK: useHomePageVehiclesSale (for VehiclesOnlyHomePage sections)
// ============================================================

interface HomePageSection {
  vehicles: VehicleListing[];
  isLoading: boolean;
  error: string | null;
}

interface UseHomePageVehiclesSaleResult {
  sedans: HomePageSection;
  suvs: HomePageSection;
  trucks: HomePageSection;
  luxury: HomePageSection;
  electric: HomePageSection;
  allVehicles: VehicleListing[];
  isLoading: boolean;
}

export const useHomePageVehiclesSale = (): UseHomePageVehiclesSaleResult => {
  const [sedans, setSedans] = useState<HomePageSection>({ vehicles: [], isLoading: true, error: null });
  const [suvs, setSuvs] = useState<HomePageSection>({ vehicles: [], isLoading: true, error: null });
  const [trucks, setTrucks] = useState<HomePageSection>({ vehicles: [], isLoading: true, error: null });
  const [luxury, setLuxury] = useState<HomePageSection>({ vehicles: [], isLoading: true, error: null });
  const [electric, setElectric] = useState<HomePageSection>({ vehicles: [], isLoading: true, error: null });
  const [allVehicles, setAllVehicles] = useState<VehicleListing[]>([]);

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const response = await getVehicles(1, 50);
        const vehicles = response.vehicles;
        setAllVehicles(vehicles);

        // Filter by body style
        const sedanVehicles = vehicles.filter(v => 
          v.bodyStyle === 'Sedan' || v.bodyStyle === 'Coupe'
        ).slice(0, 6);
        
        const suvVehicles = vehicles.filter(v => 
          v.bodyStyle === 'SUV' || v.bodyStyle === 'Crossover' || v.bodyStyle === 'Wagon'
        ).slice(0, 6);
        
        const truckVehicles = vehicles.filter(v => 
          v.bodyStyle === 'Truck'
        ).slice(0, 6);
        
        // Luxury = high price vehicles (>= $70,000)
        const luxuryVehicles = vehicles
          .filter(v => v.price >= 70000)
          .slice(0, 6);
        
        // Electric = fuel type
        const electricVehicles = vehicles
          .filter(v => v.fuelType === 'Electric' || v.fuelType === 'Hybrid')
          .slice(0, 6);

        setSedans({ vehicles: sedanVehicles, isLoading: false, error: null });
        setSuvs({ vehicles: suvVehicles, isLoading: false, error: null });
        setTrucks({ vehicles: truckVehicles, isLoading: false, error: null });
        setLuxury({ vehicles: luxuryVehicles, isLoading: false, error: null });
        setElectric({ vehicles: electricVehicles, isLoading: false, error: null });
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to fetch vehicles';
        setSedans({ vehicles: [], isLoading: false, error: errorMessage });
        setSuvs({ vehicles: [], isLoading: false, error: errorMessage });
        setTrucks({ vehicles: [], isLoading: false, error: errorMessage });
        setLuxury({ vehicles: [], isLoading: false, error: errorMessage });
        setElectric({ vehicles: [], isLoading: false, error: errorMessage });
      }
    };

    fetchAll();
  }, []);

  const isLoading = sedans.isLoading || suvs.isLoading || trucks.isLoading || luxury.isLoading || electric.isLoading;

  return {
    sedans,
    suvs,
    trucks,
    luxury,
    electric,
    allVehicles,
    isLoading,
  };
};

// Re-export types
export type { VehicleListing, VehiclesResponse, VehicleFilters } from '@/services/vehiclesSaleService';
