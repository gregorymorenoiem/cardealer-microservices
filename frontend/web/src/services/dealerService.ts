/**
 * Dealer Service - Fetches dealers with their vehicle listings for map visualization
 * 
 * This service aggregates vehicles by dealer and provides location data
 * for displaying dealers on Google Maps
 */

import axios from 'axios';

// VehiclesSaleService API URL
const VEHICLES_SALE_API_URL = import.meta.env.VITE_VEHICLES_SALE_SERVICE_URL || 'http://localhost:15070/api';

// ============================================================
// INTERFACES
// ============================================================

export interface DealerListing {
  id: string;
  title: string;
  price: number;
  image: string;
  category: 'vehicle' | 'property';
  featured: boolean;
}

export interface DealerLocation {
  id: string;
  name: string;
  type: 'dealer' | 'individual';
  phone: string;
  rating: number;
  reviewCount: number;
  address: string;
  city: string;
  state: string;
  latitude: number;
  longitude: number;
  activeListings: number;
  featuredListings: DealerListing[];
  isVerified: boolean;
}

interface BackendVehicle {
  id: string;
  title: string;
  description: string;
  price: number;
  currency: string;
  status: number;
  vin: string;
  make: string;
  model: string;
  year: number;
  mileage: number;
  mileageUnit: number;
  vehicleType: number;
  bodyStyle: number;
  fuelType: number;
  transmission: number;
  driveType: number;
  engineSize: string | null;
  cylinders: number | null;
  horsepower: number | null;
  exteriorColor: string | null;
  interiorColor: string | null;
  condition: number;
  isCertified: boolean;
  hasCleanTitle: boolean;
  city: string | null;
  state: string | null;
  zipCode: string | null;
  country: string;
  latitude: number | null;
  longitude: number | null;
  sellerId: string;
  sellerName: string;
  sellerPhone: string | null;
  sellerEmail: string | null;
  dealerId: string;
  categoryId: string | null;
  isFeatured: boolean;
  featuredAt: string | null;
  viewCount: number;
  favoriteCount: number;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
  images: BackendVehicleImage[];
}

interface BackendVehicleImage {
  id: string;
  vehicleId: string;
  url: string;
  altText: string | null;
  isPrimary: boolean;
  sortOrder: number;
}

// Default coordinates for Dominican Republic (Santo Domingo)
const DEFAULT_LOCATION = {
  latitude: 18.4861,
  longitude: -69.9312
};

// ============================================================
// HELPER FUNCTIONS
// ============================================================

/**
 * Generate a random offset to spread dealers on the map when no coordinates
 */
const generateLocationOffset = (index: number): { lat: number; lng: number } => {
  // Create a grid-like distribution with some randomness
  const row = Math.floor(index / 5);
  const col = index % 5;
  const baseOffset = 0.015; // ~1.6km between points
  
  return {
    lat: (row - 2) * baseOffset + (Math.random() - 0.5) * 0.005,
    lng: (col - 2) * baseOffset + (Math.random() - 0.5) * 0.005
  };
};

/**
 * Get the primary image URL from vehicle images
 */
const getPrimaryImage = (images: BackendVehicleImage[] | undefined): string => {
  if (!images || images.length === 0) {
    return 'https://via.placeholder.com/400x300?text=No+Image';
  }
  const primary = images.find(img => img.isPrimary);
  return primary?.url || images[0]?.url || 'https://via.placeholder.com/400x300?text=No+Image';
};

/**
 * Transform backend vehicle to dealer listing format
 */
const transformVehicleToListing = (vehicle: BackendVehicle): DealerListing => {
  return {
    id: vehicle.id,
    title: `${vehicle.year} ${vehicle.make} ${vehicle.model}`,
    price: vehicle.price,
    image: getPrimaryImage(vehicle.images),
    category: 'vehicle',
    featured: vehicle.isFeatured
  };
};

// ============================================================
// MAIN SERVICE FUNCTIONS
// ============================================================

/**
 * Fetch all vehicles from backend and group them by dealer
 */
export const getDealersWithVehicles = async (): Promise<DealerLocation[]> => {
  try {
    console.log('🗺️ Fetching dealers with vehicles from:', VEHICLES_SALE_API_URL);
    
    // Fetch all active vehicles
    const response = await axios.get<{ vehicles: BackendVehicle[] }>(`${VEHICLES_SALE_API_URL}/vehicles`, {
      params: {
        page: 0,
        pageSize: 500, // Get all vehicles
        sortBy: 'CreatedAt',
        sortDescending: true
      }
    });

    const vehicles = response.data?.vehicles || response.data || [];
    console.log(`✅ Fetched ${Array.isArray(vehicles) ? vehicles.length : 0} vehicles`);

    if (!Array.isArray(vehicles) || vehicles.length === 0) {
      console.log('⚠️ No vehicles found, returning empty array');
      return [];
    }

    // Group vehicles by dealer
    const dealerMap = new Map<string, { dealer: Partial<DealerLocation>; vehicles: BackendVehicle[] }>();

    vehicles.forEach((vehicle: BackendVehicle) => {
      const dealerId = vehicle.dealerId || vehicle.sellerId;
      
      if (!dealerMap.has(dealerId)) {
        dealerMap.set(dealerId, {
          dealer: {
            id: dealerId,
            name: vehicle.sellerName || 'Dealer',
            phone: vehicle.sellerPhone || '',
            city: vehicle.city || 'Santo Domingo',
            state: vehicle.state || 'Distrito Nacional',
            latitude: vehicle.latitude ?? undefined,
            longitude: vehicle.longitude ?? undefined
          },
          vehicles: []
        });
      }
      
      dealerMap.get(dealerId)!.vehicles.push(vehicle);
    });

    // Transform to DealerLocation array
    const dealers: DealerLocation[] = [];
    let index = 0;

    dealerMap.forEach((data, dealerId) => {
      // Calculate location - use vehicle coordinates or generate offset
      let latitude = data.dealer.latitude || DEFAULT_LOCATION.latitude;
      let longitude = data.dealer.longitude || DEFAULT_LOCATION.longitude;
      
      // If no specific coordinates, add offset to spread dealers
      if (!data.dealer.latitude || !data.dealer.longitude) {
        const offset = generateLocationOffset(index);
        latitude = DEFAULT_LOCATION.latitude + offset.lat;
        longitude = DEFAULT_LOCATION.longitude + offset.lng;
      }

      // Get featured listings (up to 6 for display)
      const featuredListings = data.vehicles
        .slice(0, 6)
        .map(transformVehicleToListing);

      // Create dealer location object
      const dealer: DealerLocation = {
        id: dealerId,
        name: data.dealer.name || `Dealer ${index + 1}`,
        type: 'dealer',
        phone: data.dealer.phone || '+1 809 555 0000',
        rating: parseFloat((4.0 + (index % 10) / 10).toFixed(1)), // Rating with 1 decimal: 4.0, 4.1, ..., 4.9
        reviewCount: Math.floor(Math.random() * 100) + 10, // Placeholder reviews
        address: `${data.dealer.city}, ${data.dealer.state}`,
        city: data.dealer.city || 'Santo Domingo',
        state: data.dealer.state || 'DN',
        latitude,
        longitude,
        activeListings: data.vehicles.length,
        featuredListings,
        isVerified: data.vehicles.length > 5 // Verified if has 5+ listings
      };

      dealers.push(dealer);
      index++;
    });

    console.log(`✅ Processed ${dealers.length} dealers with vehicles`);
    return dealers;
  } catch (error) {
    console.error('❌ Error fetching dealers with vehicles:', error);
    
    // Return empty array on error - let UI handle empty state
    return [];
  }
};

/**
 * Get a single dealer with all their vehicles
 */
export const getDealerById = async (dealerId: string): Promise<DealerLocation | null> => {
  try {
    const response = await axios.get<BackendVehicle[]>(
      `${VEHICLES_SALE_API_URL}/vehicles/dealer/${dealerId}`
    );

    const vehicles = response.data || [];
    
    if (vehicles.length === 0) {
      return null;
    }

    const firstVehicle = vehicles[0];
    const featuredListings = vehicles.slice(0, 6).map(transformVehicleToListing);

    return {
      id: dealerId,
      name: firstVehicle.sellerName || 'Dealer',
      type: 'dealer',
      phone: firstVehicle.sellerPhone || '',
      rating: 4.5,
      reviewCount: 50,
      address: `${firstVehicle.city || 'Santo Domingo'}, ${firstVehicle.state || 'DN'}`,
      city: firstVehicle.city || 'Santo Domingo',
      state: firstVehicle.state || 'DN',
      latitude: firstVehicle.latitude || DEFAULT_LOCATION.latitude,
      longitude: firstVehicle.longitude || DEFAULT_LOCATION.longitude,
      activeListings: vehicles.length,
      featuredListings,
      isVerified: vehicles.length > 5
    };
  } catch (error) {
    console.error('❌ Error fetching dealer by ID:', error);
    return null;
  }
};

// ============================================================
// SERVICE OBJECT
// ============================================================

const dealerService = {
  getDealersWithVehicles,
  getDealerById
};

export default dealerService;
