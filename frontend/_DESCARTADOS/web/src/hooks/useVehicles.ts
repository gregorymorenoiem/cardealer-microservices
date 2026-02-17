/**
 * useVehicles - React Query hooks for vehicle data from ProductService backend
 * 
 * Este hook conecta con el backend real (ProductService) para CRUD de vehículos.
 * A diferencia de useMarketplace que usa mock data, este hook llama a la API real.
 */

import { useQuery, useMutation, useQueryClient, useInfiniteQuery } from '@tanstack/react-query';
import {
  getAllVehicles,
  getVehicleById,
  createVehicle,
  updateVehicle,
  deleteVehicle,
  getFeaturedVehicles,
  getCategories,
  type Vehicle,
  type VehicleFilters,
  type Category,
} from '@/services/vehicleService';

// Type aliases para mejor legibilidad
export type CreateVehicleRequest = Partial<Vehicle>;
export type UpdateVehicleRequest = Partial<Vehicle>;
export type VehicleCategory = Category;

// Query keys para cache management
export const vehicleKeys = {
  all: ['vehicles'] as const,
  lists: () => [...vehicleKeys.all, 'list'] as const,
  list: (filters?: VehicleFilters, page?: number, pageSize?: number) => 
    [...vehicleKeys.lists(), { filters, page, pageSize }] as const,
  details: () => [...vehicleKeys.all, 'detail'] as const,
  detail: (id: string) => [...vehicleKeys.details(), id] as const,
  featured: (limit?: number) => [...vehicleKeys.all, 'featured', limit] as const,
  categories: () => [...vehicleKeys.all, 'categories'] as const,
  byCategory: (categoryId: string) => [...vehicleKeys.all, 'byCategory', categoryId] as const,
};

/**
 * Hook para obtener lista de vehículos con filtros y paginación
 */
export function useVehiclesList(
  filters?: VehicleFilters,
  page = 1,
  pageSize = 12
) {
  return useQuery({
    queryKey: vehicleKeys.list(filters, page, pageSize),
    queryFn: () => getAllVehicles(filters, page, pageSize),
    staleTime: 30 * 1000, // 30 seconds
    gcTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Hook para infinite scroll de vehículos
 */
export function useInfiniteVehicles(filters?: VehicleFilters, pageSize = 12) {
  return useInfiniteQuery({
    queryKey: [...vehicleKeys.list(filters), 'infinite'],
    queryFn: ({ pageParam = 1 }) => getAllVehicles(filters, pageParam, pageSize),
    getNextPageParam: (lastPage) => {
      if (lastPage.page < lastPage.totalPages) {
        return lastPage.page + 1;
      }
      return undefined;
    },
    initialPageParam: 1,
  });
}

/**
 * Hook para obtener un vehículo por ID
 */
export function useVehicle(id: string) {
  return useQuery({
    queryKey: vehicleKeys.detail(id),
    queryFn: () => getVehicleById(id),
    enabled: !!id && id !== 'undefined',
    staleTime: 60 * 1000, // 1 minute
  });
}

/**
 * Hook para obtener vehículos destacados
 */
export function useFeaturedVehicles(limit = 6) {
  return useQuery({
    queryKey: vehicleKeys.featured(limit),
    queryFn: () => getFeaturedVehicles(limit),
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

/**
 * Hook para obtener categorías de vehículos
 */
export function useVehicleCategories() {
  return useQuery({
    queryKey: vehicleKeys.categories(),
    queryFn: () => getCategories(),
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

/**
 * Hook para obtener vehículos por categoría
 * Usa filtros de getAllVehicles con categoryId
 */
export function useVehiclesByCategory(categoryId: string) {
  return useQuery({
    queryKey: vehicleKeys.byCategory(categoryId),
    queryFn: () => getAllVehicles({ categoryId }),
    enabled: !!categoryId,
  });
}

/**
 * Hook para crear un nuevo vehículo
 */
export function useCreateVehicle() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateVehicleRequest) => createVehicle(data),
    onSuccess: () => {
      // Invalidate all vehicle lists
      queryClient.invalidateQueries({ queryKey: vehicleKeys.lists() });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.featured() });
    },
  });
}

/**
 * Hook para actualizar un vehículo
 */
export function useUpdateVehicle() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateVehicleRequest }) => 
      updateVehicle(id, data),
    onSuccess: (updatedVehicle) => {
      // Update cache for this specific vehicle
      queryClient.setQueryData(vehicleKeys.detail(updatedVehicle.id), updatedVehicle);
      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: vehicleKeys.lists() });
    },
  });
}

/**
 * Hook para eliminar un vehículo
 */
export function useDeleteVehicle() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteVehicle(id),
    onSuccess: (_, deletedId) => {
      // Remove from cache
      queryClient.removeQueries({ queryKey: vehicleKeys.detail(deletedId) });
      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: vehicleKeys.lists() });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.featured() });
    },
  });
}

// Re-export types for convenience
export type { Vehicle, VehicleFilters };
