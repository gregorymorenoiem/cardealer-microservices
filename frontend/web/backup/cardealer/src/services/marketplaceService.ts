/**
 * Marketplace Service - Mock Data Implementation
 * Provides unified access to all marketplace verticals
 */

import type {
  Listing,
  VehicleListing,
  PropertyListing,
  MarketplaceSearchParams,
  VehicleSearchParams,
  PropertySearchParams,
  MarketplaceSearchResponse,
  VerticalCategory,
} from '@/types/marketplace';

import { mockVehicles } from '@/data/mockVehicles';
import { mockProperties } from '@/data/mockProperties';

// Convert mockVehicles to VehicleListing format
const convertVehicleToListing = (v: (typeof mockVehicles)[0]): VehicleListing => ({
  id: v.id,
  vertical: 'vehicles',
  title: `${v.year} ${v.make} ${v.model}`,
  description: v.description,
  price: v.price,
  currency: 'USD',
  status: 'active',
  isFeatured: v.isFeatured || false,
  images: v.images.map((url, i) => ({
    id: `img-${v.id}-${i}`,
    url,
    thumbnailUrl: url,
    sortOrder: i,
    isPrimary: i === 0,
  })),
  primaryImageUrl: v.images[0],
  location: {
    address: '',
    city: v.location.split(', ')[0],
    state: v.location.split(', ')[1] || '',
    zipCode: '',
    country: 'USA',
  },
  seller: {
    id: `seller-${v.id}`,
    name: v.seller.name,
    phone: v.seller.phone,
    rating: v.seller.rating,
    isVerified: true,
    isDealership: v.seller.type === 'Dealer',
  },
  viewCount: Math.floor(Math.random() * 1000) + 100,
  favoriteCount: Math.floor(Math.random() * 100) + 10,
  inquiryCount: Math.floor(Math.random() * 30) + 5,
  createdAt: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000).toISOString(),
  updatedAt: new Date().toISOString(),
  // Vehicle specific
  vehicleType: 'car',
  make: v.make,
  model: v.model,
  year: v.year,
  mileage: v.mileage,
  mileageUnit: 'miles',
  transmission: v.transmission.toLowerCase() as VehicleListing['transmission'],
  fuelType: v.fuelType.toLowerCase().replace(' ', '-') as VehicleListing['fuelType'],
  bodyType: v.bodyType.toLowerCase() as VehicleListing['bodyType'],
  drivetrain: v.drivetrain.toLowerCase() as VehicleListing['drivetrain'],
  engine: v.engine,
  horsepower: v.horsepower,
  mpg: v.mpg,
  exteriorColor: v.color,
  interiorColor: v.interiorColor,
  vin: v.vin,
  condition: v.condition.toLowerCase().replace(' ', '-') as VehicleListing['condition'],
  features: v.features,
});

// Convert all mock vehicles
const vehicleListings: VehicleListing[] = mockVehicles.map(convertVehicleToListing);

// All listings combined
const allListings: Listing[] = [...vehicleListings, ...mockProperties];

/**
 * Marketplace Service
 */
export const marketplaceService = {
  /**
   * Get all listings with optional filtering
   */
  async getListings(params: MarketplaceSearchParams = {}): Promise<MarketplaceSearchResponse<Listing>> {
    await simulateDelay();
    
    let results = [...allListings];

    // Filter by vertical
    if (params.vertical) {
      results = results.filter(l => l.vertical === params.vertical);
    }

    // Filter by query
    if (params.query) {
      const q = params.query.toLowerCase();
      results = results.filter(l => 
        l.title.toLowerCase().includes(q) ||
        l.description.toLowerCase().includes(q)
      );
    }

    // Filter by price
    if (params.minPrice) {
      results = results.filter(l => l.price >= params.minPrice!);
    }
    if (params.maxPrice) {
      results = results.filter(l => l.price <= params.maxPrice!);
    }

    // Filter by location
    if (params.city) {
      results = results.filter(l => l.location.city.toLowerCase().includes(params.city!.toLowerCase()));
    }

    // Filter by featured
    if (params.isFeatured !== undefined) {
      results = results.filter(l => l.isFeatured === params.isFeatured);
    }

    // Sort
    if (params.sortBy) {
      results.sort((a, b) => {
        const order = params.sortOrder === 'desc' ? -1 : 1;
        switch (params.sortBy) {
          case 'price':
            return (a.price - b.price) * order;
          case 'date':
            return (new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()) * order;
          default:
            return 0;
        }
      });
    } else {
      // Default: featured first, then by date
      results.sort((a, b) => {
        if (a.isFeatured && !b.isFeatured) return -1;
        if (!a.isFeatured && b.isFeatured) return 1;
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      });
    }

    // Pagination
    const page = params.page || 1;
    const pageSize = params.pageSize || 20;
    const totalCount = results.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const start = (page - 1) * pageSize;
    const paginatedResults = results.slice(start, start + pageSize);

    return {
      listings: paginatedResults,
      totalCount,
      page,
      pageSize,
      totalPages,
    };
  },

  /**
   * Get vehicles with specific filters
   */
  async getVehicles(params: VehicleSearchParams): Promise<MarketplaceSearchResponse<VehicleListing>> {
    await simulateDelay();
    
    let results = [...vehicleListings];

    // Apply base filters
    if (params.query) {
      const q = params.query.toLowerCase();
      results = results.filter(v => 
        v.title.toLowerCase().includes(q) ||
        v.make.toLowerCase().includes(q) ||
        v.model.toLowerCase().includes(q)
      );
    }

    if (params.make) results = results.filter(v => v.make.toLowerCase() === params.make!.toLowerCase());
    if (params.model) results = results.filter(v => v.model.toLowerCase() === params.model!.toLowerCase());
    if (params.minYear) results = results.filter(v => v.year >= params.minYear!);
    if (params.maxYear) results = results.filter(v => v.year <= params.maxYear!);
    if (params.minPrice) results = results.filter(v => v.price >= params.minPrice!);
    if (params.maxPrice) results = results.filter(v => v.price <= params.maxPrice!);
    if (params.minMileage) results = results.filter(v => v.mileage >= params.minMileage!);
    if (params.maxMileage) results = results.filter(v => v.mileage <= params.maxMileage!);
    if (params.transmission) results = results.filter(v => v.transmission === params.transmission);
    if (params.fuelType) results = results.filter(v => v.fuelType === params.fuelType);
    if (params.bodyType) results = results.filter(v => v.bodyType === params.bodyType);
    if (params.condition) results = results.filter(v => v.condition === params.condition);
    if (params.isFeatured !== undefined) results = results.filter(v => v.isFeatured === params.isFeatured);

    // Pagination
    const page = params.page || 1;
    const pageSize = params.pageSize || 20;
    const totalCount = results.length;
    const start = (page - 1) * pageSize;

    return {
      listings: results.slice(start, start + pageSize),
      totalCount,
      page,
      pageSize,
      totalPages: Math.ceil(totalCount / pageSize),
    };
  },

  /**
   * Get properties with specific filters
   */
  async getProperties(params: PropertySearchParams): Promise<MarketplaceSearchResponse<PropertyListing>> {
    await simulateDelay();
    
    let results = [...mockProperties];

    // Apply filters
    if (params.query) {
      const q = params.query.toLowerCase();
      results = results.filter(p => 
        p.title.toLowerCase().includes(q) ||
        p.description.toLowerCase().includes(q) ||
        p.location.city.toLowerCase().includes(q) ||
        p.location.neighborhood?.toLowerCase().includes(q)
      );
    }

    if (params.propertyType) results = results.filter(p => p.propertyType === params.propertyType);
    if (params.listingType) results = results.filter(p => p.listingType === params.listingType);
    if (params.minPrice) results = results.filter(p => p.price >= params.minPrice!);
    if (params.maxPrice) results = results.filter(p => p.price <= params.maxPrice!);
    if (params.minArea) results = results.filter(p => p.totalArea >= params.minArea!);
    if (params.maxArea) results = results.filter(p => p.totalArea <= params.maxArea!);
    if (params.minBedrooms) results = results.filter(p => p.bedrooms >= params.minBedrooms!);
    if (params.maxBedrooms) results = results.filter(p => p.bedrooms <= params.maxBedrooms!);
    if (params.minBathrooms) results = results.filter(p => p.bathrooms >= params.minBathrooms!);
    if (params.maxBathrooms) results = results.filter(p => p.bathrooms <= params.maxBathrooms!);
    if (params.hasPool !== undefined) results = results.filter(p => p.hasPool === params.hasPool);
    if (params.hasGarden !== undefined) results = results.filter(p => p.hasGarden === params.hasGarden);
    if (params.hasGym !== undefined) results = results.filter(p => p.hasGym === params.hasGym);
    if (params.hasSecurity !== undefined) results = results.filter(p => p.hasSecurity === params.hasSecurity);
    if (params.isFurnished !== undefined) results = results.filter(p => p.isFurnished === params.isFurnished);
    if (params.allowsPets !== undefined) results = results.filter(p => p.allowsPets === params.allowsPets);
    if (params.isFeatured !== undefined) results = results.filter(p => p.isFeatured === params.isFeatured);
    if (params.city) results = results.filter(p => p.location.city.toLowerCase().includes(params.city!.toLowerCase()));

    // Pagination
    const page = params.page || 1;
    const pageSize = params.pageSize || 20;
    const totalCount = results.length;
    const start = (page - 1) * pageSize;

    return {
      listings: results.slice(start, start + pageSize),
      totalCount,
      page,
      pageSize,
      totalPages: Math.ceil(totalCount / pageSize),
    };
  },

  /**
   * Get a single listing by ID
   */
  async getListingById(id: string): Promise<Listing | null> {
    await simulateDelay();
    return allListings.find(l => l.id === id) || null;
  },

  /**
   * Get featured listings
   */
  async getFeaturedListings(limit = 8): Promise<Listing[]> {
    await simulateDelay();
    return allListings.filter(l => l.isFeatured).slice(0, limit);
  },

  /**
   * Get featured vehicles
   */
  async getFeaturedVehicles(limit = 8): Promise<VehicleListing[]> {
    await simulateDelay();
    return vehicleListings.filter(v => v.isFeatured).slice(0, limit);
  },

  /**
   * Get featured properties
   */
  async getFeaturedProperties(limit = 8): Promise<PropertyListing[]> {
    await simulateDelay();
    return mockProperties.filter(p => p.isFeatured).slice(0, limit);
  },

  /**
   * Get marketplace categories/verticals
   */
  async getCategories(): Promise<VerticalCategory[]> {
    await simulateDelay();
    
    return [
      {
        id: 'vehicles',
        name: 'Vehículos',
        description: 'Autos, motos, camionetas y más',
        icon: '🚗',
        gradient: 'from-blue-500 to-cyan-500',
        stats: {
          listingCount: vehicleListings.length,
          label: 'vehículos disponibles',
        },
        subcategories: [
          { id: 'sedan', name: 'Sedán', count: vehicleListings.filter(v => v.bodyType === 'sedan').length },
          { id: 'suv', name: 'SUV', count: vehicleListings.filter(v => v.bodyType === 'suv').length },
          { id: 'truck', name: 'Pickup', count: vehicleListings.filter(v => v.bodyType === 'truck').length },
          { id: 'electric', name: 'Eléctricos', count: vehicleListings.filter(v => v.fuelType === 'electric').length },
        ],
      },
      {
        id: 'real-estate',
        name: 'Bienes Raíces',
        description: 'Casas, departamentos, terrenos y más',
        icon: '🏠',
        gradient: 'from-emerald-500 to-teal-500',
        stats: {
          listingCount: mockProperties.length,
          label: 'propiedades disponibles',
        },
        subcategories: [
          { id: 'house', name: 'Casas', count: mockProperties.filter(p => p.propertyType === 'house').length },
          { id: 'apartment', name: 'Departamentos', count: mockProperties.filter(p => p.propertyType === 'apartment').length },
          { id: 'commercial', name: 'Comercial', count: mockProperties.filter(p => p.propertyType === 'commercial').length },
          { id: 'land', name: 'Terrenos', count: mockProperties.filter(p => p.propertyType === 'land').length },
        ],
      },
    ];
  },

  /**
   * Get popular makes/brands for vehicles
   */
  async getPopularMakes(): Promise<{ make: string; count: number }[]> {
    await simulateDelay();
    
    const makeCounts = vehicleListings.reduce((acc, v) => {
      acc[v.make] = (acc[v.make] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    return Object.entries(makeCounts)
      .map(([make, count]) => ({ make, count }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 10);
  },

  /**
   * Get popular cities for properties
   */
  async getPopularCities(): Promise<{ city: string; count: number }[]> {
    await simulateDelay();
    
    const cityCounts = mockProperties.reduce((acc, p) => {
      acc[p.location.city] = (acc[p.location.city] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    return Object.entries(cityCounts)
      .map(([city, count]) => ({ city, count }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 10);
  },

  /**
   * Get similar listings
   */
  async getSimilarListings(listing: Listing, limit = 4): Promise<Listing[]> {
    await simulateDelay();
    
    const sameListing = allListings.filter(l => l.id !== listing.id && l.vertical === listing.vertical);
    
    // Simple similarity: same vertical, similar price range
    const priceRange = listing.price * 0.3;
    const similar = sameListing.filter(l => 
      Math.abs(l.price - listing.price) <= priceRange
    );

    return similar.slice(0, limit);
  },
};

// Helper: Simulate network delay
function simulateDelay(ms = 200): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

export default marketplaceService;
