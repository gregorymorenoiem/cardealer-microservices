import type { PropertyListing, PropertyAmenity } from '@/types/marketplace';

// Helper para generar amenidades
const createAmenities = (names: string[], category: PropertyAmenity['category']): PropertyAmenity[] =>
  names.map(name => ({ name, category }));

export const mockProperties: PropertyListing[] = [
  {
    id: 'prop-1',
    vertical: 'real-estate',
    title: 'Espectacular Casa en Polanco con Jardín Privado',
    description: 'Hermosa residencia de lujo en la exclusiva zona de Polanco. Cuenta con amplios espacios, acabados de primera calidad, jardín privado con alberca y seguridad 24 horas. Ideal para familias que buscan comodidad y elegancia.',
    price: 12500000,
    currency: 'MXN',
    status: 'active',
    isFeatured: true,
    propertyType: 'house',
    listingType: 'sale',
    pricePerSqMeter: 31250,
    isNegotiable: true,
    totalArea: 400,
    areaUnit: 'sqm',
    builtArea: 350,
    lotArea: 500,
    bedrooms: 4,
    bathrooms: 3,
    halfBathrooms: 1,
    parkingSpaces: 3,
    yearBuilt: 2019,
    hasGarden: true,
    hasPool: true,
    hasGym: false,
    hasElevator: false,
    hasSecurity: true,
    isFurnished: false,
    allowsPets: true,
    amenities: [
      ...createAmenities(['Vigilancia 24/7', 'Cámaras de seguridad', 'Alarma'], 'security'),
      ...createAmenities(['Alberca privada', 'Área de BBQ'], 'recreation'),
      ...createAmenities(['Aire acondicionado central', 'Calefacción'], 'comfort'),
      ...createAmenities(['Jardín amplio', 'Terraza techada'], 'outdoor'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
      { id: '2', url: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=400&h=300&fit=crop', sortOrder: 1, isPrimary: false },
      { id: '3', url: 'https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?w=400&h=300&fit=crop', sortOrder: 2, isPrimary: false },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&h=600&fit=crop',
    location: {
      address: 'Av. Presidente Masaryk 450',
      city: 'Ciudad de México',
      state: 'CDMX',
      zipCode: '11560',
      country: 'México',
      neighborhood: 'Polanco',
      latitude: 19.4320,
      longitude: -99.1937,
    },
    seller: {
      id: 'seller-1',
      name: 'Inmobiliaria Premium CDMX',
      phone: '+52 55 1234 5678',
      email: 'ventas@premiuminmobiliaria.mx',
      avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=IP',
      rating: 4.9,
      reviewCount: 127,
      memberSince: '2020-03-15',
      isVerified: true,
      isDealership: true,
    },
    viewCount: 1250,
    favoriteCount: 89,
    inquiryCount: 23,
    createdAt: '2025-11-01T10:00:00Z',
    updatedAt: '2025-12-05T14:30:00Z',
    publishedAt: '2025-11-01T12:00:00Z',
  },
  {
    id: 'prop-2',
    vertical: 'real-estate',
    title: 'Moderno Apartamento en Santa Fe con Vista Panorámica',
    description: 'Departamento de lujo en torre residencial premium de Santa Fe. Espectacular vista a la ciudad, amenidades de primer nivel y ubicación estratégica cerca de centros comerciales y corporativos.',
    price: 6800000,
    currency: 'MXN',
    status: 'active',
    isFeatured: true,
    propertyType: 'apartment',
    listingType: 'sale',
    pricePerSqMeter: 45333,
    isNegotiable: true,
    totalArea: 150,
    areaUnit: 'sqm',
    builtArea: 150,
    bedrooms: 3,
    bathrooms: 2,
    halfBathrooms: 1,
    parkingSpaces: 2,
    floor: 18,
    totalFloors: 25,
    yearBuilt: 2022,
    hasGarden: false,
    hasPool: true,
    hasGym: true,
    hasElevator: true,
    hasSecurity: true,
    isFurnished: true,
    allowsPets: false,
    amenities: [
      ...createAmenities(['Vigilancia 24/7', 'Control de acceso'], 'security'),
      ...createAmenities(['Gimnasio equipado', 'Alberca climatizada', 'Salón de eventos'], 'recreation'),
      ...createAmenities(['Elevadores de alta velocidad', 'A/C central'], 'comfort'),
      ...createAmenities(['Roof garden', 'Sky lounge'], 'outdoor'),
      ...createAmenities(['Internet fibra óptica incluido', 'Smart home'], 'technology'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
      { id: '2', url: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=400&h=300&fit=crop', sortOrder: 1, isPrimary: false },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&h=600&fit=crop',
    location: {
      address: 'Av. Vasco de Quiroga 3000',
      city: 'Ciudad de México',
      state: 'CDMX',
      zipCode: '05348',
      country: 'México',
      neighborhood: 'Santa Fe',
      latitude: 19.3590,
      longitude: -99.2616,
    },
    seller: {
      id: 'seller-2',
      name: 'Carlos Mendoza',
      phone: '+52 55 9876 5432',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=carlos',
      rating: 4.7,
      reviewCount: 45,
      memberSince: '2021-06-20',
      isVerified: true,
      isDealership: false,
    },
    viewCount: 890,
    favoriteCount: 67,
    inquiryCount: 18,
    createdAt: '2025-10-15T08:00:00Z',
    updatedAt: '2025-12-04T16:45:00Z',
    publishedAt: '2025-10-15T10:00:00Z',
  },
  {
    id: 'prop-3',
    vertical: 'real-estate',
    title: 'Acogedor Departamento en Renta - Roma Norte',
    description: 'Hermoso departamento en la colonia Roma Norte, ideal para profesionales. A pasos de restaurantes, cafés y el Parque México. Completamente amueblado con estilo moderno.',
    price: 28000,
    currency: 'MXN',
    status: 'active',
    isFeatured: false,
    propertyType: 'apartment',
    listingType: 'rent',
    maintenanceFee: 2500,
    isNegotiable: false,
    totalArea: 75,
    areaUnit: 'sqm',
    builtArea: 75,
    bedrooms: 2,
    bathrooms: 1,
    parkingSpaces: 1,
    floor: 3,
    totalFloors: 6,
    yearBuilt: 2015,
    hasGarden: false,
    hasPool: false,
    hasGym: false,
    hasElevator: true,
    hasSecurity: true,
    isFurnished: true,
    allowsPets: true,
    amenities: [
      ...createAmenities(['Portero', 'Intercomunicador'], 'security'),
      ...createAmenities(['Lavadora/Secadora', 'Cocina equipada'], 'comfort'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
      { id: '2', url: 'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=400&h=300&fit=crop', sortOrder: 1, isPrimary: false },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&h=600&fit=crop',
    location: {
      address: 'Calle Orizaba 150',
      city: 'Ciudad de México',
      state: 'CDMX',
      zipCode: '06700',
      country: 'México',
      neighborhood: 'Roma Norte',
      latitude: 19.4170,
      longitude: -99.1641,
    },
    seller: {
      id: 'seller-3',
      name: 'María García',
      phone: '+52 55 5555 1234',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=maria',
      rating: 4.8,
      reviewCount: 92,
      memberSince: '2019-02-10',
      isVerified: true,
      isDealership: false,
    },
    viewCount: 2340,
    favoriteCount: 156,
    inquiryCount: 45,
    createdAt: '2025-11-20T14:00:00Z',
    updatedAt: '2025-12-05T09:15:00Z',
    publishedAt: '2025-11-20T16:00:00Z',
  },
  {
    id: 'prop-4',
    vertical: 'real-estate',
    title: 'Casa Familiar en Coyoacán con Jardín Grande',
    description: 'Encantadora casa estilo colonial mexicano en el corazón de Coyoacán. Amplios espacios, jardín con árboles frutales y ambiente tranquilo. Perfecta para familias que aprecian la tradición y cultura.',
    price: 8900000,
    currency: 'MXN',
    status: 'active',
    isFeatured: true,
    propertyType: 'house',
    listingType: 'sale',
    pricePerSqMeter: 29666,
    isNegotiable: true,
    totalArea: 300,
    areaUnit: 'sqm',
    builtArea: 250,
    lotArea: 400,
    bedrooms: 4,
    bathrooms: 3,
    parkingSpaces: 2,
    yearBuilt: 1985,
    hasGarden: true,
    hasPool: false,
    hasGym: false,
    hasElevator: false,
    hasSecurity: false,
    isFurnished: false,
    allowsPets: true,
    amenities: [
      ...createAmenities(['Jardín amplio', 'Terraza', 'Patio interior'], 'outdoor'),
      ...createAmenities(['Chimenea', 'Bodega'], 'comfort'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
      { id: '2', url: 'https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?w=400&h=300&fit=crop', sortOrder: 1, isPrimary: false },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&h=600&fit=crop',
    location: {
      address: 'Calle Francisco Sosa 280',
      city: 'Ciudad de México',
      state: 'CDMX',
      zipCode: '04000',
      country: 'México',
      neighborhood: 'Coyoacán',
      latitude: 19.3500,
      longitude: -99.1620,
    },
    seller: {
      id: 'seller-4',
      name: 'Bienes Raíces Coyoacán',
      phone: '+52 55 4444 5555',
      email: 'info@brcoyoacan.mx',
      avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=BRC',
      rating: 4.6,
      reviewCount: 78,
      memberSince: '2018-08-01',
      isVerified: true,
      isDealership: true,
    },
    viewCount: 780,
    favoriteCount: 45,
    inquiryCount: 12,
    createdAt: '2025-10-28T11:00:00Z',
    updatedAt: '2025-12-03T10:30:00Z',
    publishedAt: '2025-10-28T14:00:00Z',
  },
  {
    id: 'prop-5',
    vertical: 'real-estate',
    title: 'Penthouse de Lujo en Bosques de las Lomas',
    description: 'Exclusivo penthouse con terraza privada y vista espectacular. Acabados de ultra lujo, domótica completa y amenidades de primer nivel. Una oportunidad única para los más exigentes.',
    price: 25000000,
    currency: 'MXN',
    status: 'active',
    isFeatured: true,
    propertyType: 'condo',
    listingType: 'sale',
    pricePerSqMeter: 62500,
    maintenanceFee: 15000,
    isNegotiable: true,
    totalArea: 400,
    areaUnit: 'sqm',
    builtArea: 400,
    bedrooms: 4,
    bathrooms: 4,
    halfBathrooms: 1,
    parkingSpaces: 4,
    floor: 20,
    totalFloors: 20,
    yearBuilt: 2023,
    hasGarden: false,
    hasPool: true,
    hasGym: true,
    hasElevator: true,
    hasSecurity: true,
    isFurnished: false,
    allowsPets: true,
    amenities: [
      ...createAmenities(['Vigilancia 24/7', 'Control biométrico', 'CCTV'], 'security'),
      ...createAmenities(['Gimnasio', 'Spa', 'Alberca infinity'], 'recreation'),
      ...createAmenities(['Domótica completa', 'A/C por zonas', 'Elevador privado'], 'comfort'),
      ...createAmenities(['Terraza panorámica', 'Jacuzzi exterior'], 'outdoor'),
      ...createAmenities(['Fibra óptica', 'Sistema de audio integrado'], 'technology'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1600607687644-c7171b42498f?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1600607687644-c7171b42498f?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
      { id: '2', url: 'https://images.unsplash.com/photo-1600566752355-35792bedcfea?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1600566752355-35792bedcfea?w=400&h=300&fit=crop', sortOrder: 1, isPrimary: false },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1600607687644-c7171b42498f?w=800&h=600&fit=crop',
    location: {
      address: 'Paseo de los Laureles 200',
      city: 'Ciudad de México',
      state: 'CDMX',
      zipCode: '05120',
      country: 'México',
      neighborhood: 'Bosques de las Lomas',
      latitude: 19.3950,
      longitude: -99.2580,
    },
    seller: {
      id: 'seller-5',
      name: 'Luxury Homes México',
      phone: '+52 55 8888 9999',
      email: 'elite@luxuryhomesmx.com',
      avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=LHM',
      rating: 5.0,
      reviewCount: 34,
      memberSince: '2022-01-10',
      isVerified: true,
      isDealership: true,
    },
    viewCount: 560,
    favoriteCount: 89,
    inquiryCount: 8,
    createdAt: '2025-11-10T09:00:00Z',
    updatedAt: '2025-12-05T11:00:00Z',
    publishedAt: '2025-11-10T12:00:00Z',
  },
  {
    id: 'prop-6',
    vertical: 'real-estate',
    title: 'Local Comercial en Zona Centro',
    description: 'Excelente local comercial en pleno centro histórico. Ideal para restaurante, boutique o negocio con alto tráfico peatonal. Cuenta con licencia comercial vigente.',
    price: 4500000,
    currency: 'MXN',
    status: 'active',
    isFeatured: false,
    propertyType: 'commercial',
    listingType: 'sale',
    pricePerSqMeter: 30000,
    isNegotiable: true,
    totalArea: 150,
    areaUnit: 'sqm',
    builtArea: 150,
    bedrooms: 0,
    bathrooms: 2,
    parkingSpaces: 0,
    yearBuilt: 1950,
    hasGarden: false,
    hasPool: false,
    hasGym: false,
    hasElevator: false,
    hasSecurity: true,
    isFurnished: false,
    allowsPets: false,
    amenities: [
      ...createAmenities(['Cortina metálica', 'Alarma'], 'security'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800&h=600&fit=crop',
    location: {
      address: 'Calle Madero 25',
      city: 'Ciudad de México',
      state: 'CDMX',
      zipCode: '06000',
      country: 'México',
      neighborhood: 'Centro Histórico',
      latitude: 19.4330,
      longitude: -99.1390,
    },
    seller: {
      id: 'seller-6',
      name: 'Comercial Inmobiliaria Centro',
      phone: '+52 55 2222 3333',
      email: 'ventas@cicmx.com',
      avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=CIC',
      rating: 4.4,
      reviewCount: 56,
      memberSince: '2017-05-20',
      isVerified: true,
      isDealership: true,
    },
    viewCount: 340,
    favoriteCount: 23,
    inquiryCount: 9,
    createdAt: '2025-11-05T15:00:00Z',
    updatedAt: '2025-12-02T08:45:00Z',
    publishedAt: '2025-11-05T17:00:00Z',
  },
  {
    id: 'prop-7',
    vertical: 'real-estate',
    title: 'Terreno en Zona de Crecimiento - Valle de Bravo',
    description: 'Hermoso terreno con vista al lago en Valle de Bravo. Zona de alta plusvalía, ideal para construcción de casa de descanso o inversión. Servicios disponibles.',
    price: 3200000,
    currency: 'MXN',
    status: 'active',
    isFeatured: false,
    propertyType: 'land',
    listingType: 'sale',
    pricePerSqMeter: 3200,
    isNegotiable: true,
    totalArea: 1000,
    areaUnit: 'sqm',
    lotArea: 1000,
    bedrooms: 0,
    bathrooms: 0,
    parkingSpaces: 0,
    hasGarden: false,
    hasPool: false,
    hasGym: false,
    hasElevator: false,
    hasSecurity: true,
    isFurnished: false,
    allowsPets: true,
    amenities: [
      ...createAmenities(['Caseta de vigilancia en fraccionamiento'], 'security'),
      ...createAmenities(['Agua potable', 'Electricidad', 'Drenaje'], 'services'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=800&h=600&fit=crop',
    location: {
      address: 'Fraccionamiento Avándaro Lote 45',
      city: 'Valle de Bravo',
      state: 'Estado de México',
      zipCode: '51200',
      country: 'México',
      neighborhood: 'Avándaro',
      latitude: 19.1930,
      longitude: -100.1340,
    },
    seller: {
      id: 'seller-7',
      name: 'Terrenos Valle de Bravo',
      phone: '+52 726 123 4567',
      email: 'ventas@terrenosvdb.mx',
      avatar: 'https://api.dicebear.com/7.x/initials/svg?seed=TVB',
      rating: 4.5,
      reviewCount: 28,
      memberSince: '2020-09-15',
      isVerified: true,
      isDealership: true,
    },
    viewCount: 420,
    favoriteCount: 34,
    inquiryCount: 15,
    createdAt: '2025-10-20T10:00:00Z',
    updatedAt: '2025-12-01T14:20:00Z',
    publishedAt: '2025-10-20T12:00:00Z',
  },
  {
    id: 'prop-8',
    vertical: 'real-estate',
    title: 'Estudio en Renta - Condesa',
    description: 'Práctico estudio completamente amueblado en la mejor zona de la Condesa. Perfecto para profesionales solteros. Incluye servicios y está a pasos del Parque México.',
    price: 15000,
    currency: 'MXN',
    status: 'active',
    isFeatured: false,
    propertyType: 'apartment',
    listingType: 'rent',
    maintenanceFee: 1500,
    isNegotiable: false,
    totalArea: 40,
    areaUnit: 'sqm',
    builtArea: 40,
    bedrooms: 0,
    bathrooms: 1,
    parkingSpaces: 0,
    floor: 2,
    totalFloors: 4,
    yearBuilt: 2018,
    hasGarden: false,
    hasPool: false,
    hasGym: false,
    hasElevator: false,
    hasSecurity: true,
    isFurnished: true,
    allowsPets: false,
    amenities: [
      ...createAmenities(['Portero'], 'security'),
      ...createAmenities(['Internet incluido', 'TV por cable'], 'technology'),
      ...createAmenities(['Cocina equipada', 'Closet vestidor'], 'comfort'),
    ],
    images: [
      { id: '1', url: 'https://images.unsplash.com/photo-1554995207-c18c203602cb?w=800&h=600&fit=crop', thumbnailUrl: 'https://images.unsplash.com/photo-1554995207-c18c203602cb?w=400&h=300&fit=crop', sortOrder: 0, isPrimary: true },
    ],
    primaryImageUrl: 'https://images.unsplash.com/photo-1554995207-c18c203602cb?w=800&h=600&fit=crop',
    location: {
      address: 'Calle Atlixco 45',
      city: 'Ciudad de México',
      state: 'CDMX',
      zipCode: '06140',
      country: 'México',
      neighborhood: 'Condesa',
      latitude: 19.4110,
      longitude: -99.1730,
    },
    seller: {
      id: 'seller-8',
      name: 'Ana Rodríguez',
      phone: '+52 55 6666 7777',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=ana',
      rating: 4.9,
      reviewCount: 63,
      memberSince: '2021-03-01',
      isVerified: true,
      isDealership: false,
    },
    viewCount: 1890,
    favoriteCount: 124,
    inquiryCount: 38,
    createdAt: '2025-11-25T16:00:00Z',
    updatedAt: '2025-12-05T07:30:00Z',
    publishedAt: '2025-11-25T18:00:00Z',
  },
];

// Helper functions
export const getFeaturedProperties = (): PropertyListing[] => 
  mockProperties.filter(p => p.isFeatured);

export const getPropertiesForRent = (): PropertyListing[] => 
  mockProperties.filter(p => p.listingType === 'rent');

export const getPropertiesForSale = (): PropertyListing[] => 
  mockProperties.filter(p => p.listingType === 'sale');

export const getPropertyById = (id: string): PropertyListing | undefined => 
  mockProperties.find(p => p.id === id);

export const searchProperties = (params: {
  query?: string;
  propertyType?: string;
  listingType?: string;
  minPrice?: number;
  maxPrice?: number;
  minBedrooms?: number;
  city?: string;
}): PropertyListing[] => {
  let results = [...mockProperties];

  if (params.query) {
    const q = params.query.toLowerCase();
    results = results.filter(p => 
      p.title.toLowerCase().includes(q) ||
      p.description.toLowerCase().includes(q) ||
      p.location.city.toLowerCase().includes(q) ||
      p.location.neighborhood?.toLowerCase().includes(q)
    );
  }

  if (params.propertyType) {
    results = results.filter(p => p.propertyType === params.propertyType);
  }

  if (params.listingType) {
    results = results.filter(p => p.listingType === params.listingType);
  }

  if (params.minPrice) {
    results = results.filter(p => p.price >= params.minPrice!);
  }

  if (params.maxPrice) {
    results = results.filter(p => p.price <= params.maxPrice!);
  }

  if (params.minBedrooms) {
    results = results.filter(p => p.bedrooms >= params.minBedrooms!);
  }

  if (params.city) {
    results = results.filter(p => p.location.city.toLowerCase() === params.city!.toLowerCase());
  }

  return results;
};

// Filter properties with advanced filters
export const filterProperties = (filters: {
  propertyType?: string;
  listingType?: string;
  minPrice?: number;
  maxPrice?: number;
  minBedrooms?: number;
  maxBedrooms?: number;
  minBathrooms?: number;
  maxBathrooms?: number;
  minArea?: number;
  maxArea?: number;
  hasParking?: boolean;
  hasGarden?: boolean;
  hasPool?: boolean;
  hasSecurity?: boolean;
  isFurnished?: boolean;
  allowsPets?: boolean;
  city?: string;
  state?: string;
  query?: string;
}): PropertyListing[] => {
  let results = [...mockProperties];

  // Text search
  if (filters.query) {
    const q = filters.query.toLowerCase();
    results = results.filter(p => 
      p.title.toLowerCase().includes(q) ||
      p.description.toLowerCase().includes(q) ||
      p.location.city.toLowerCase().includes(q) ||
      p.location.neighborhood?.toLowerCase().includes(q)
    );
  }

  // Property type filter
  if (filters.propertyType && filters.propertyType !== 'all') {
    results = results.filter(p => p.propertyType === filters.propertyType);
  }

  // Listing type filter
  if (filters.listingType && filters.listingType !== 'all') {
    results = results.filter(p => p.listingType === filters.listingType);
  }

  // Price filters
  if (filters.minPrice) {
    results = results.filter(p => p.price >= filters.minPrice!);
  }
  if (filters.maxPrice) {
    results = results.filter(p => p.price <= filters.maxPrice!);
  }

  // Bedrooms filters
  if (filters.minBedrooms) {
    results = results.filter(p => p.bedrooms >= filters.minBedrooms!);
  }
  if (filters.maxBedrooms) {
    results = results.filter(p => p.bedrooms <= filters.maxBedrooms!);
  }

  // Bathrooms filters
  if (filters.minBathrooms) {
    results = results.filter(p => p.bathrooms >= filters.minBathrooms!);
  }
  if (filters.maxBathrooms) {
    results = results.filter(p => p.bathrooms <= filters.maxBathrooms!);
  }

  // Area filters
  if (filters.minArea) {
    results = results.filter(p => p.totalArea >= filters.minArea!);
  }
  if (filters.maxArea) {
    results = results.filter(p => p.totalArea <= filters.maxArea!);
  }

  // Boolean filters
  if (filters.hasParking) {
    results = results.filter(p => (p.parkingSpaces ?? 0) > 0);
  }
  if (filters.hasGarden) {
    results = results.filter(p => p.hasGarden === true);
  }
  if (filters.hasPool) {
    results = results.filter(p => p.hasPool === true);
  }
  if (filters.hasSecurity) {
    results = results.filter(p => p.hasSecurity === true);
  }
  if (filters.isFurnished) {
    results = results.filter(p => p.isFurnished === true);
  }
  if (filters.allowsPets) {
    results = results.filter(p => p.allowsPets === true);
  }

  // Location filters
  if (filters.city) {
    results = results.filter(p => p.location.city.toLowerCase() === filters.city!.toLowerCase());
  }
  if (filters.state) {
    results = results.filter(p => p.location.state.toLowerCase() === filters.state!.toLowerCase());
  }

  return results;
};

// Sort properties
export const sortProperties = (
  properties: PropertyListing[],
  sortBy: string
): PropertyListing[] => {
  const sorted = [...properties];

  switch (sortBy) {
    case 'price-asc':
      return sorted.sort((a, b) => a.price - b.price);
    case 'price-desc':
      return sorted.sort((a, b) => b.price - a.price);
    case 'newest':
      return sorted.sort((a, b) => 
        new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime()
      );
    case 'oldest':
      return sorted.sort((a, b) => 
        new Date(a.createdAt || 0).getTime() - new Date(b.createdAt || 0).getTime()
      );
    case 'area-asc':
      return sorted.sort((a, b) => a.totalArea - b.totalArea);
    case 'area-desc':
      return sorted.sort((a, b) => b.totalArea - a.totalArea);
    case 'bedrooms-asc':
      return sorted.sort((a, b) => a.bedrooms - b.bedrooms);
    case 'bedrooms-desc':
      return sorted.sort((a, b) => b.bedrooms - a.bedrooms);
    default:
      return sorted;
  }
};
