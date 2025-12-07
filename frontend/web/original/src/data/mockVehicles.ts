import type { ListingTier, FeaturedBadge, FeaturedPage, DealerTier } from '../types/listing';

export interface Vehicle {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  location: string;
  images: string[];
  isFeatured?: boolean;
  isNew?: boolean;
  transmission: 'Automatic' | 'Manual' | 'CVT';
  fuelType: 'Gasoline' | 'Diesel' | 'Electric' | 'Hybrid' | 'Plug-in Hybrid';
  bodyType: 'Sedan' | 'SUV' | 'Truck' | 'Coupe' | 'Hatchback' | 'Van' | 'Convertible' | 'Wagon';
  drivetrain: 'FWD' | 'RWD' | 'AWD' | '4WD';
  engine: string;
  horsepower: number;
  mpg: {
    city: number;
    highway: number;
  };
  color: string;
  interiorColor: string;
  vin: string;
  condition: 'New' | 'Used' | 'Certified Pre-Owned';
  features: string[];
  description: string;
  seller: {
    name: string;
    type: 'Private' | 'Dealer';
    rating: number;
    phone: string;
  };
  
  // Featured Listing fields (Sprint 1)
  tier?: ListingTier;
  featuredUntil?: Date;
  featuredPosition?: number;
  featuredPages?: FeaturedPage[];
  featuredBadge?: FeaturedBadge;
  qualityScore?: number;
  engagementScore?: number;
  dealerId?: string;
  dealerTier?: DealerTier;
  dealerVerified?: boolean;
}

export const mockVehicles: Vehicle[] = [
  {
    id: '1',
    make: 'Tesla',
    model: 'Model 3',
    year: 2023,
    price: 42990,
    mileage: 5200,
    location: 'Los Angeles, CA',
    images: [
      'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1561580125-028ee3bd62eb?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1536700503339-1e4b06520771?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1617531653332-bd46c24f2068?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
    isNew: true,
    transmission: 'Automatic',
    fuelType: 'Electric',
    bodyType: 'Sedan',
    drivetrain: 'AWD',
    engine: 'Dual Motor Electric',
    horsepower: 480,
    mpg: { city: 132, highway: 126 },
    color: 'Pearl White',
    interiorColor: 'Black',
    vin: '5YJ3E1EB4KF123456',
    condition: 'New',
    features: [
      'Autopilot',
      'Premium Audio',
      'Glass Roof',
      'Heated Seats',
      'Navigation System',
      'Bluetooth',
      'Backup Camera',
      'Keyless Entry',
    ],
    description: 'Brand new Tesla Model 3 with full self-driving capability. This electric sedan combines performance with efficiency, featuring a minimalist interior and cutting-edge technology.',
    seller: {
      name: 'Tesla Los Angeles',
      type: 'Dealer',
      rating: 4.8,
      phone: '+1 (555) 123-4567',
    },
  },
  {
    id: '2',
    make: 'BMW',
    model: '3 Series',
    year: 2022,
    price: 38500,
    mileage: 12000,
    location: 'Miami, FL',
    images: [
      'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'RWD',
    engine: '2.0L Turbo I4',
    horsepower: 255,
    mpg: { city: 26, highway: 36 },
    color: 'Alpine White',
    interiorColor: 'Black',
    vin: 'WBA8B9C59HK987654',
    condition: 'Used',
    features: [
      'Sunroof',
      'Leather Seats',
      'Navigation',
      'Parking Sensors',
      'LED Headlights',
      'Adaptive Cruise Control',
      'Lane Departure Warning',
    ],
    description: 'Excellent condition BMW 3 Series with low mileage. Well-maintained with full service history. Perfect blend of luxury and performance.',
    seller: {
      name: 'Miami Luxury Motors',
      type: 'Dealer',
      rating: 4.6,
      phone: '+1 (555) 234-5678',
    },
  },
  {
    id: '3',
    make: 'Toyota',
    model: 'Camry',
    year: 2021,
    price: 24900,
    mileage: 28000,
    location: 'Houston, TX',
    images: [
      'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&h=600&fit=crop',
    ],
    transmission: 'Automatic',
    fuelType: 'Hybrid',
    bodyType: 'Sedan',
    drivetrain: 'FWD',
    engine: '2.5L Hybrid I4',
    horsepower: 208,
    mpg: { city: 51, highway: 53 },
    color: 'Celestial Silver',
    interiorColor: 'Ash',
    vin: '4T1G11AK5MU123456',
    condition: 'Used',
    features: [
      'Hybrid Technology',
      'Backup Camera',
      'Bluetooth',
      'Adaptive Cruise Control',
      'Lane Keep Assist',
      'Apple CarPlay',
      'Android Auto',
    ],
    description: 'Reliable and fuel-efficient Toyota Camry Hybrid. Perfect for daily commuting with excellent gas mileage and proven reliability.',
    seller: {
      name: 'John Smith',
      type: 'Private',
      rating: 4.9,
      phone: '+1 (555) 345-6789',
    },
  },
  {
    id: '4',
    make: 'Ford',
    model: 'Mustang',
    year: 2023,
    price: 35990,
    mileage: 8500,
    location: 'Dallas, TX',
    images: [
      'https://images.unsplash.com/photo-1547744152-14d985cb937f?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1494905998402-395d579af36f?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1612825173281-9a193378527e?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
    transmission: 'Manual',
    fuelType: 'Gasoline',
    bodyType: 'Coupe',
    drivetrain: 'RWD',
    engine: '5.0L V8',
    horsepower: 450,
    mpg: { city: 15, highway: 24 },
    color: 'Race Red',
    interiorColor: 'Black',
    vin: '1FA6P8CF3L5123456',
    condition: 'Used',
    features: [
      'Performance Package',
      'Premium Audio',
      'Sport Seats',
      'Track Apps',
      'Launch Control',
      'Brembo Brakes',
    ],
    description: 'Powerful Ford Mustang GT with the legendary 5.0L V8 engine. Perfect for enthusiasts who love the thrill of driving.',
    seller: {
      name: 'Dallas Performance Cars',
      type: 'Dealer',
      rating: 4.7,
      phone: '+1 (555) 456-7890',
    },
  },
  {
    id: '5',
    make: 'Honda',
    model: 'Accord',
    year: 2022,
    price: 27500,
    mileage: 15000,
    location: 'Chicago, IL',
    images: [
      'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800&h=600&fit=crop',
    ],
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'FWD',
    engine: '1.5L Turbo I4',
    horsepower: 192,
    mpg: { city: 30, highway: 38 },
    color: 'Platinum White',
    interiorColor: 'Gray',
    vin: '1HGCV1F39NA123456',
    condition: 'Certified Pre-Owned',
    features: [
      'Honda Sensing Suite',
      'Sunroof',
      'Heated Seats',
      'Apple CarPlay',
      'Android Auto',
      'Wireless Charging',
    ],
    description: 'Honda Certified Pre-Owned Accord with comprehensive warranty. Spacious, comfortable, and loaded with safety features.',
    seller: {
      name: 'Chicago Honda',
      type: 'Dealer',
      rating: 4.5,
      phone: '+1 (555) 567-8901',
    },
  },
  {
    id: '6',
    make: 'Audi',
    model: 'A4',
    year: 2023,
    price: 41200,
    mileage: 6800,
    location: 'New York, NY',
    images: [
      'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'AWD',
    engine: '2.0L Turbo I4',
    horsepower: 261,
    mpg: { city: 24, highway: 33 },
    color: 'Mythos Black',
    interiorColor: 'Black',
    vin: 'WAUFFAFL5PN123456',
    condition: 'Used',
    features: [
      'Quattro AWD',
      'Virtual Cockpit',
      'Bang & Olufsen Audio',
      'Panoramic Sunroof',
      'Heated/Ventilated Seats',
      'Matrix LED Headlights',
    ],
    description: 'Luxurious Audi A4 with Quattro all-wheel drive. Premium materials, advanced technology, and exceptional handling.',
    seller: {
      name: 'Manhattan Audi',
      type: 'Dealer',
      rating: 4.8,
      phone: '+1 (555) 678-9012',
    },
  },
  {
    id: '7',
    make: 'Mercedes-Benz',
    model: 'C-Class',
    year: 2022,
    price: 43900,
    mileage: 11000,
    location: 'Seattle, WA',
    images: [
      'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'RWD',
    engine: '2.0L Turbo I4',
    horsepower: 255,
    mpg: { city: 23, highway: 33 },
    color: 'Selenite Grey',
    interiorColor: 'Black',
    vin: 'W1KWJ8DB8NA123456',
    condition: 'Certified Pre-Owned',
    features: [
      'MBUX Infotainment',
      'Burmester Audio',
      'AMG Line Package',
      'Panoramic Roof',
      '64-Color Ambient Lighting',
      'Driver Assistance Package',
    ],
    description: 'Mercedes-Benz Certified C-Class with AMG styling. Luxury, technology, and performance in a refined package.',
    seller: {
      name: 'Seattle Mercedes-Benz',
      type: 'Dealer',
      rating: 4.9,
      phone: '+1 (555) 789-0123',
    },
  },
  {
    id: '8',
    make: 'Chevrolet',
    model: 'Silverado 1500',
    year: 2021,
    price: 39500,
    mileage: 22000,
    location: 'Denver, CO',
    images: [
      'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800&h=600&fit=crop',
    ],
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Truck',
    drivetrain: '4WD',
    engine: '5.3L V8',
    horsepower: 355,
    mpg: { city: 16, highway: 20 },
    color: 'Summit White',
    interiorColor: 'Jet Black',
    vin: '1GCUYGEL5MZ123456',
    condition: 'Used',
    features: [
      'Z71 Off-Road Package',
      'Tow Package',
      'Bed Liner',
      'Tonneau Cover',
      'Rear Camera',
      'Teen Driver',
    ],
    description: 'Capable Chevrolet Silverado 1500 with 4WD and towing package. Perfect for work or adventure in the Rockies.',
    seller: {
      name: 'Denver Chevrolet',
      type: 'Dealer',
      rating: 4.4,
      phone: '+1 (555) 890-1234',
    },
  },
  {
    id: '9',
    make: 'Mazda',
    model: 'CX-5',
    year: 2023,
    price: 32900,
    mileage: 4500,
    location: 'Portland, OR',
    images: [
      'https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800&h=600&fit=crop',
    ],
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'SUV',
    drivetrain: 'AWD',
    engine: '2.5L Turbo I4',
    horsepower: 256,
    mpg: { city: 22, highway: 27 },
    color: 'Soul Red Crystal',
    interiorColor: 'Black',
    vin: 'JM3KFBDM3P0123456',
    condition: 'Used',
    features: [
      'Signature Package',
      'Bose Audio',
      'Head-Up Display',
      'Power Liftgate',
      'Heated Steering Wheel',
      'i-ACTIVSENSE Safety',
    ],
    description: 'Nearly new Mazda CX-5 with premium features. Sporty handling meets practical SUV utility with upscale materials.',
    seller: {
      name: 'Portland Mazda',
      type: 'Dealer',
      rating: 4.7,
      phone: '+1 (555) 901-2345',
    },
  },
  {
    id: '10',
    make: 'Volkswagen',
    model: 'Jetta',
    year: 2022,
    price: 22900,
    mileage: 18000,
    location: 'Austin, TX',
    images: [
      'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&h=600&fit=crop',
    ],
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'FWD',
    engine: '1.4L Turbo I4',
    horsepower: 147,
    mpg: { city: 30, highway: 41 },
    color: 'Pure White',
    interiorColor: 'Titan Black',
    vin: '3VWC57BU8NM123456',
    condition: 'Used',
    features: [
      'Digital Cockpit',
      'App-Connect',
      'Blind Spot Monitor',
      'Rear Cross Traffic Alert',
      'Forward Collision Warning',
    ],
    description: 'Fuel-efficient VW Jetta with modern technology. Great for city driving with surprising interior space.',
    seller: {
      name: 'Sarah Johnson',
      type: 'Private',
      rating: 4.8,
      phone: '+1 (555) 012-3456',
    },
  },
];

// Helper function to filter vehicles
export const filterVehicles = (
  vehicles: Vehicle[],
  filters: {
    make?: string;
    model?: string;
    minYear?: number;
    maxYear?: number;
    minPrice?: number;
    maxPrice?: number;
    minMileage?: number;
    maxMileage?: number;
    transmission?: string;
    fuelType?: string;
    bodyType?: string;
    condition?: string;
    features?: string[];
    minHorsepower?: number;
    drivetrain?: string;
  }
) => {
  return vehicles.filter((vehicle) => {
    if (filters.make && vehicle.make !== filters.make) return false;
    if (filters.model && !vehicle.model.toLowerCase().includes(filters.model.toLowerCase())) return false;
    if (filters.minYear && vehicle.year < filters.minYear) return false;
    if (filters.maxYear && vehicle.year > filters.maxYear) return false;
    if (filters.minPrice && vehicle.price < filters.minPrice) return false;
    if (filters.maxPrice && vehicle.price > filters.maxPrice) return false;
    if (filters.minMileage && vehicle.mileage < filters.minMileage) return false;
    if (filters.maxMileage && vehicle.mileage > filters.maxMileage) return false;
    if (filters.transmission && vehicle.transmission !== filters.transmission) return false;
    if (filters.fuelType && vehicle.fuelType !== filters.fuelType) return false;
    if (filters.bodyType && vehicle.bodyType !== filters.bodyType) return false;
    if (filters.condition && vehicle.condition !== filters.condition) return false;
    if (filters.minHorsepower && vehicle.horsepower < filters.minHorsepower) return false;
    if (filters.drivetrain && vehicle.drivetrain !== filters.drivetrain) return false;
    if (filters.features && filters.features.length > 0) {
      const hasAllFeatures = filters.features.every(feature => 
        vehicle.features.some(vf => vf.toLowerCase().includes(feature.toLowerCase()))
      );
      if (!hasAllFeatures) return false;
    }
    return true;
  });
};

// Helper function to sort vehicles
export const sortVehicles = (
  vehicles: Vehicle[],
  sortBy: 'price-asc' | 'price-desc' | 'year-desc' | 'year-asc' | 'mileage-asc' | 'mileage-desc' | 'horsepower-desc'
) => {
  const sorted = [...vehicles];
  switch (sortBy) {
    case 'price-asc':
      return sorted.sort((a, b) => a.price - b.price);
    case 'price-desc':
      return sorted.sort((a, b) => b.price - a.price);
    case 'year-desc':
      return sorted.sort((a, b) => b.year - a.year);
    case 'year-asc':
      return sorted.sort((a, b) => a.year - b.year);
    case 'mileage-asc':
      return sorted.sort((a, b) => a.mileage - b.mileage);
    case 'mileage-desc':
      return sorted.sort((a, b) => b.mileage - a.mileage);
    case 'horsepower-desc':
      return sorted.sort((a, b) => b.horsepower - a.horsepower);
    default:
      return sorted;
  }
};
