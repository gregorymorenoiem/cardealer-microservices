import axios, { AxiosInstance } from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// Interfaces matching backend DTOs
export interface InventoryItemDto {
  id: string;
  dealerId: string;
  vehicleId: string;
  status: InventoryStatus;
  visibility: InventoryVisibility;
  internalNotes?: string;
  location?: string;
  stockNumber: number;
  vin?: string;
  costPrice?: number;
  listPrice: number;
  targetPrice?: number;
  minAcceptablePrice?: number;
  isNegotiable: boolean;
  acquiredDate?: string;
  acquisitionSource?: string;
  acquisitionDetails?: string;
  viewCount: number;
  inquiryCount: number;
  testDriveCount: number;
  offerCount: number;
  highestOffer?: number;
  lastViewedAt?: string;
  lastInquiryAt?: string;
  isFeatured: boolean;
  featuredUntil?: string;
  priority: number;
  tags?: string[];
  createdAt: string;
  updatedAt?: string;
  publishedAt?: string;
  soldAt?: string;
  soldPrice?: number;
  soldTo?: string;
  daysOnMarket: number;
  isHot: boolean;
  isOverdue: boolean;
}

export interface CreateInventoryItemRequest {
  dealerId: string;
  vehicleId: string;
  internalNotes?: string;
  location?: string;
  stockNumber?: number;
  vin?: string;
  costPrice?: number;
  listPrice: number;
  targetPrice?: number;
  minAcceptablePrice?: number;
  isNegotiable?: boolean;
  acquiredDate?: string;
  acquisitionSource?: string;
  acquisitionDetails?: string;
}

export interface UpdateInventoryItemRequest {
  internalNotes?: string;
  location?: string;
  listPrice?: number;
  targetPrice?: number;
  minAcceptablePrice?: number;
  isNegotiable?: boolean;
  isFeatured?: boolean;
  priority?: number;
}

export interface BulkUpdateStatusRequest {
  itemIds: string[];
  status: InventoryStatus;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface InventoryStatsDto {
  totalItems: number;
  activeItems: number;
  pausedItems: number;
  soldItems: number;
  overdueItems: number;
  hotItems: number;
  totalInventoryValue: number;
  avgPrice: number;
  projectedProfit: number;
}

export enum InventoryStatus {
  Active = 'Active',
  Paused = 'Paused',
  Sold = 'Sold'
}

export enum InventoryVisibility {
  Public = 'Public',
  Private = 'Private'
}

export interface InventoryFilters {
  dealerId: string;
  page?: number;
  pageSize?: number;
  status?: InventoryStatus;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

class InventoryManagementService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: `${API_URL}/api/inventory`,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Interceptor para agregar JWT token
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }

  /**
   * Get paginated inventory items for a dealer
   */
  async getInventoryItems(filters: InventoryFilters): Promise<PagedResultDto<InventoryItemDto>> {
    const params = new URLSearchParams();
    params.append('dealerId', filters.dealerId);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.status) params.append('status', filters.status);
    if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.sortDescending !== undefined) params.append('sortDescending', filters.sortDescending.toString());

    const response = await this.api.get<PagedResultDto<InventoryItemDto>>(`?${params.toString()}`);
    return response.data;
  }

  /**
   * Get inventory statistics for a dealer
   */
  async getInventoryStats(dealerId: string): Promise<InventoryStatsDto> {
    const response = await this.api.get<InventoryStatsDto>(`/stats?dealerId=${dealerId}`);
    return response.data;
  }

  /**
   * Get a single inventory item by ID
   */
  async getInventoryItemById(id: string): Promise<InventoryItemDto> {
    const response = await this.api.get<InventoryItemDto>(`/${id}`);
    return response.data;
  }

  /**
   * Create a new inventory item
   */
  async createInventoryItem(request: CreateInventoryItemRequest): Promise<InventoryItemDto> {
    const response = await this.api.post<InventoryItemDto>('', request);
    return response.data;
  }

  /**
   * Update an existing inventory item
   */
  async updateInventoryItem(id: string, request: UpdateInventoryItemRequest): Promise<InventoryItemDto> {
    const response = await this.api.put<InventoryItemDto>(`/${id}`, request);
    return response.data;
  }

  /**
   * Bulk update status for multiple items
   */
  async bulkUpdateStatus(request: BulkUpdateStatusRequest): Promise<void> {
    await this.api.post('/bulk/status', request);
  }

  /**
   * Delete an inventory item
   */
  async deleteInventoryItem(id: string): Promise<void> {
    await this.api.delete(`/${id}`);
  }

  /**
   * Get featured items for a dealer
   */
  async getFeaturedItems(dealerId: string): Promise<InventoryItemDto[]> {
    const response = await this.api.get<InventoryItemDto[]>(`/featured?dealerId=${dealerId}`);
    return response.data;
  }

  /**
   * Get hot items (high activity) for a dealer
   */
  async getHotItems(dealerId: string): Promise<InventoryItemDto[]> {
    const response = await this.api.get<InventoryItemDto[]>(`/hot?dealerId=${dealerId}`);
    return response.data;
  }

  /**
   * Get overdue items (90+ days) for a dealer
   */
  async getOverdueItems(dealerId: string): Promise<InventoryItemDto[]> {
    const response = await this.api.get<InventoryItemDto[]>(`/overdue?dealerId=${dealerId}`);
    return response.data;
  }

  /**
   * Format currency for Dominican Republic (DOP)
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  /**
   * Get status badge color
   */
  getStatusColor(status: InventoryStatus): string {
    switch (status) {
      case InventoryStatus.Active:
        return 'bg-green-100 text-green-800';
      case InventoryStatus.Paused:
        return 'bg-yellow-100 text-yellow-800';
      case InventoryStatus.Sold:
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  /**
   * Get status label
   */
  getStatusLabel(status: InventoryStatus): string {
    switch (status) {
      case InventoryStatus.Active:
        return 'Activo';
      case InventoryStatus.Paused:
        return 'Pausado';
      case InventoryStatus.Sold:
        return 'Vendido';
      default:
        return status;
    }
  }
}

export const inventoryManagementService = new InventoryManagementService();
export default inventoryManagementService;
