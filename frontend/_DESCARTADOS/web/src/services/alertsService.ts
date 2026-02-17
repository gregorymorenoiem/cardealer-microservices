import api from './api';

// ============ PRICE ALERTS ============

export interface PriceAlert {
  id: string;
  vehicleId: string;
  targetPrice: number;
  condition: 'LessThanOrEqual' | 'GreaterThanOrEqual';
  isActive: boolean;
  isTriggered: boolean;
  triggeredAt?: string;
  createdAt: string;
  updatedAt: string;
  // Extended vehicle info (loaded separately)
  vehicle?: {
    id: string;
    title: string;
    price: number;
    imageUrl?: string;
  };
}

export interface CreatePriceAlertRequest {
  vehicleId: string;
  targetPrice: number;
  condition: number; // 0 = LessThanOrEqual, 1 = GreaterThanOrEqual
}

export interface UpdateTargetPriceRequest {
  targetPrice: number;
}

// Get all price alerts for current user
export const getPriceAlerts = async (): Promise<PriceAlert[]> => {
  try {
    const response = await api.get('/api/pricealerts');
    return response.data;
  } catch (error) {
    console.error('Error fetching price alerts:', error);
    throw new Error('Failed to fetch price alerts');
  }
};

// Get price alert by ID
export const getPriceAlertById = async (id: string): Promise<PriceAlert> => {
  try {
    const response = await api.get(`/api/pricealerts/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching price alert:', error);
    throw new Error('Failed to fetch price alert');
  }
};

// Create new price alert
export const createPriceAlert = async (data: CreatePriceAlertRequest): Promise<PriceAlert> => {
  try {
    const response = await api.post('/api/pricealerts', data);
    return response.data;
  } catch (error) {
    console.error('Error creating price alert:', error);
    throw new Error('Failed to create price alert');
  }
};

// Update target price
export const updatePriceAlertTarget = async (
  id: string,
  targetPrice: number
): Promise<PriceAlert> => {
  try {
    const response = await api.put(`/api/pricealerts/${id}/target-price`, { targetPrice });
    return response.data;
  } catch (error) {
    console.error('Error updating price alert:', error);
    throw new Error('Failed to update price alert');
  }
};

// Activate price alert
export const activatePriceAlert = async (id: string): Promise<PriceAlert> => {
  try {
    const response = await api.post(`/api/pricealerts/${id}/activate`);
    return response.data;
  } catch (error) {
    console.error('Error activating price alert:', error);
    throw new Error('Failed to activate price alert');
  }
};

// Deactivate price alert
export const deactivatePriceAlert = async (id: string): Promise<PriceAlert> => {
  try {
    const response = await api.post(`/api/pricealerts/${id}/deactivate`);
    return response.data;
  } catch (error) {
    console.error('Error deactivating price alert:', error);
    throw new Error('Failed to deactivate price alert');
  }
};

// Delete price alert
export const deletePriceAlert = async (id: string): Promise<void> => {
  try {
    await api.delete(`/api/pricealerts/${id}`);
  } catch (error) {
    console.error('Error deleting price alert:', error);
    throw new Error('Failed to delete price alert');
  }
};

// ============ SAVED SEARCHES ============

export interface SavedSearchCriteria {
  make?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
  bodyType?: string;
  fuelType?: string;
  transmission?: string;
  location?: string;
}

export interface SavedSearch {
  id: string;
  name: string;
  searchCriteria: string; // JSON string
  sendEmailNotifications: boolean;
  frequency: 'Instant' | 'Daily' | 'Weekly';
  lastNotificationSent?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  // Parsed criteria for display
  parsedCriteria?: SavedSearchCriteria;
}

export interface CreateSavedSearchRequest {
  name: string;
  searchCriteria: string; // JSON string of filters
  sendEmailNotifications: boolean;
  frequency: number; // 0 = Instant, 1 = Daily, 2 = Weekly
}

// Get all saved searches for current user
export const getSavedSearches = async (): Promise<SavedSearch[]> => {
  try {
    const response = await api.get('/api/savedsearches');
    // Parse criteria for each search
    return response.data.map((search: SavedSearch) => ({
      ...search,
      parsedCriteria: parseSearchCriteria(search.searchCriteria),
    }));
  } catch (error) {
    console.error('Error fetching saved searches:', error);
    throw new Error('Failed to fetch saved searches');
  }
};

// Get saved search by ID
export const getSavedSearchById = async (id: string): Promise<SavedSearch> => {
  try {
    const response = await api.get(`/api/savedsearches/${id}`);
    return {
      ...response.data,
      parsedCriteria: parseSearchCriteria(response.data.searchCriteria),
    };
  } catch (error) {
    console.error('Error fetching saved search:', error);
    throw new Error('Failed to fetch saved search');
  }
};

// Create new saved search
export const createSavedSearch = async (data: CreateSavedSearchRequest): Promise<SavedSearch> => {
  try {
    const response = await api.post('/api/savedsearches', data);
    return response.data;
  } catch (error) {
    console.error('Error creating saved search:', error);
    throw new Error('Failed to create saved search');
  }
};

// Update saved search name
export const updateSavedSearchName = async (id: string, name: string): Promise<SavedSearch> => {
  try {
    const response = await api.put(`/api/savedsearches/${id}/name`, { name });
    return response.data;
  } catch (error) {
    console.error('Error updating saved search name:', error);
    throw new Error('Failed to update saved search name');
  }
};

// Update saved search criteria
export const updateSavedSearchCriteria = async (
  id: string,
  searchCriteria: string
): Promise<SavedSearch> => {
  try {
    const response = await api.put(`/api/savedsearches/${id}/criteria`, { searchCriteria });
    return response.data;
  } catch (error) {
    console.error('Error updating saved search criteria:', error);
    throw new Error('Failed to update saved search criteria');
  }
};

// Update notification settings
export const updateSavedSearchNotifications = async (
  id: string,
  sendEmailNotifications: boolean,
  frequency: number
): Promise<SavedSearch> => {
  try {
    const response = await api.put(`/api/savedsearches/${id}/notifications`, {
      sendEmailNotifications,
      frequency,
    });
    return response.data;
  } catch (error) {
    console.error('Error updating notifications:', error);
    throw new Error('Failed to update notifications');
  }
};

// Activate saved search
export const activateSavedSearch = async (id: string): Promise<SavedSearch> => {
  try {
    const response = await api.post(`/api/savedsearches/${id}/activate`);
    return response.data;
  } catch (error) {
    console.error('Error activating saved search:', error);
    throw new Error('Failed to activate saved search');
  }
};

// Deactivate saved search
export const deactivateSavedSearch = async (id: string): Promise<SavedSearch> => {
  try {
    const response = await api.post(`/api/savedsearches/${id}/deactivate`);
    return response.data;
  } catch (error) {
    console.error('Error deactivating saved search:', error);
    throw new Error('Failed to deactivate saved search');
  }
};

// Delete saved search
export const deleteSavedSearch = async (id: string): Promise<void> => {
  try {
    await api.delete(`/api/savedsearches/${id}`);
  } catch (error) {
    console.error('Error deleting saved search:', error);
    throw new Error('Failed to delete saved search');
  }
};

// ============ HELPERS ============

export const parseSearchCriteria = (criteriaJson: string): SavedSearchCriteria => {
  try {
    return JSON.parse(criteriaJson);
  } catch {
    return {};
  }
};

export const formatCriteriaForDisplay = (criteria: SavedSearchCriteria): string => {
  const parts: string[] = [];

  if (criteria.make) parts.push(criteria.make);
  if (criteria.model) parts.push(criteria.model);
  if (criteria.bodyType) parts.push(criteria.bodyType);

  if (criteria.minYear && criteria.maxYear) {
    parts.push(`${criteria.minYear}-${criteria.maxYear}`);
  } else if (criteria.minYear) {
    parts.push(`Desde ${criteria.minYear}`);
  } else if (criteria.maxYear) {
    parts.push(`Hasta ${criteria.maxYear}`);
  }

  if (criteria.minPrice && criteria.maxPrice) {
    parts.push(`RD$${criteria.minPrice.toLocaleString()}-RD$${criteria.maxPrice.toLocaleString()}`);
  } else if (criteria.maxPrice) {
    parts.push(`Hasta RD$${criteria.maxPrice.toLocaleString()}`);
  } else if (criteria.minPrice) {
    parts.push(`Desde RD$${criteria.minPrice.toLocaleString()}`);
  }

  if (criteria.fuelType) parts.push(criteria.fuelType);
  if (criteria.transmission) parts.push(criteria.transmission);

  return parts.length > 0 ? parts.join(' • ') : 'Sin filtros específicos';
};

export const getFrequencyLabel = (frequency: string): string => {
  switch (frequency) {
    case 'Instant':
      return 'Inmediato';
    case 'Daily':
      return 'Diario';
    case 'Weekly':
      return 'Semanal';
    default:
      return frequency;
  }
};

export const getConditionLabel = (condition: string): string => {
  switch (condition) {
    case 'LessThanOrEqual':
      return 'Baja a';
    case 'GreaterThanOrEqual':
      return 'Sube a';
    default:
      return condition;
  }
};
