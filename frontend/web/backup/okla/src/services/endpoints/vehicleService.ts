import api from '../api';
import type { 
  Vehicle, 
  VehicleSearchParams, 
  CreateVehicleRequest,
  PaginatedResponse,
  ApiResponse 
} from '../../types';

export const vehicleService = {
  async getVehicles(params?: VehicleSearchParams): Promise<ApiResponse<PaginatedResponse<Vehicle>>> {
    const response = await api.get('/api/vehicles', { params });
    return response.data;
  },

  async searchVehicles(params: VehicleSearchParams): Promise<ApiResponse<PaginatedResponse<Vehicle>>> {
    const response = await api.get('/api/vehicles/search', { params });
    return response.data;
  },

  async getVehicleById(id: string): Promise<ApiResponse<Vehicle>> {
    const response = await api.get(`/api/vehicles/${id}`);
    return response.data;
  },

  async createVehicle(data: CreateVehicleRequest): Promise<ApiResponse<{ id: string }>> {
    const response = await api.post('/api/vehicles', data);
    return response.data;
  },

  async updateVehicle(id: string, data: Partial<CreateVehicleRequest>): Promise<ApiResponse<void>> {
    const response = await api.put(`/api/vehicles/${id}`, data);
    return response.data;
  },

  async deleteVehicle(id: string): Promise<ApiResponse<void>> {
    const response = await api.delete(`/api/vehicles/${id}`);
    return response.data;
  },

  async toggleFavorite(id: string): Promise<ApiResponse<{ isFavorited: boolean }>> {
    const response = await api.post(`/api/vehicles/${id}/favorite`);
    return response.data;
  },

  async getUserFavorites(userId: string): Promise<ApiResponse<PaginatedResponse<Vehicle>>> {
    const response = await api.get(`/api/vehicles/user/${userId}/favorites`);
    return response.data;
  },

  async getSimilarVehicles(id: string, limit = 6): Promise<ApiResponse<Vehicle[]>> {
    const response = await api.get(`/api/vehicles/${id}/similar`, { params: { limit } });
    return response.data;
  },

  async getBrands(): Promise<ApiResponse<Array<{ name: string; count: number }>>> {
    const response = await api.get('/api/vehicles/filters/brands');
    return response.data;
  },

  async getModels(brand: string): Promise<ApiResponse<Array<{ name: string; count: number }>>> {
    const response = await api.get('/api/vehicles/filters/models', { params: { brand } });
    return response.data;
  },
};
