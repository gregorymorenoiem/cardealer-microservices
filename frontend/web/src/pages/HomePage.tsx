/**
 * HomePage - Main marketplace landing page
 * Multi-vertical marketplace with clean, scalable design
 * Sprint 5: Integrated Featured Listings with HeroCarousel
 * Sprint 5.2: Removed SearchSection, moved to Navbar for space optimization
 */

import React, { useRef, useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { FiArrowRight, FiSearch, FiShield, FiMessageCircle, FiZap, FiChevronLeft, FiChevronRight, FiStar, FiMapPin, FiWifi, FiWifiOff } from 'react-icons/fi';
import { FaCar, FaHome, FaKey, FaBed } from 'react-icons/fa';
import HeroCarousel from '@/components/organisms/HeroCarousel';
import FeaturedListingGrid from '@/components/molecules/FeaturedListingGrid';
// COMMENTED: Mock data import - using real API data only
// import { mockVehicles, type Vehicle } from '@/data/mockVehicles';
import { type Vehicle } from '@/data/mockVehicles'; // Only import type, not mock data
import { mixFeaturedAndOrganic } from '@/utils/rankingAlgorithm';
import { generateListingUrl } from '@/utils/seoSlug';
import { getVehicleSaleImageUrl, getVehicleRentImageUrl, getPropertySaleImageUrl, getLodgingImageUrl } from '@/utils/s3ImageUrl';
import { useVehiclesSaleList, type VehicleListing } from '@/hooks/useVehiclesSale';

/* ============================================================
 * MOCK DATA COMENTADO - Solo usar datos reales de la API
 * TODO: Eliminar completamente cuando todas las secciones usen API
 * ============================================================ */

/* MOCK DATA COMENTADO - sedanesListings
const sedanesListings = [
  {
    id: 'sed1',
    title: 'Toyota Camry XSE 2024',
    price: 35000,
    image: getVehicleSaleImageUrl('photo-1621007947382-bb3c3994e3fb'),
    category: 'Sedanes',
    location: 'Miami, FL',
    rating: 4.8,
    reviews: 124,
  },
  {
    id: 'sed2',
    title: 'Honda Accord Sport 2024',
    price: 33500,
    image: getVehicleSaleImageUrl('photo-1619682817481-e994891cd1f5'),
    category: 'Sedanes',
    location: 'Los Angeles, CA',
    rating: 4.7,
    reviews: 98,
  },
  {
    id: 'sed3',
    title: 'BMW Serie 3 330i 2024',
    price: 48000,
    image: getVehicleSaleImageUrl('photo-1555215695-3004980ad54e'),
    category: 'Sedanes',
    location: 'New York, NY',
    rating: 4.9,
    reviews: 156,
  },
  {
    id: 'sed4',
    title: 'Mercedes-Benz Clase C 300 2024',
    price: 52000,
    image: getVehicleSaleImageUrl('photo-1618843479313-40f8afb4b4d8'),
    category: 'Sedanes',
    location: 'Dallas, TX',
    rating: 4.8,
    reviews: 87,
  },
  {
    id: 'sed5',
    title: 'Audi A4 Premium Plus 2024',
    price: 46500,
    image: getVehicleSaleImageUrl('photo-1606664515524-ed2f786a0bd6'),
    category: 'Sedanes',
    location: 'Chicago, IL',
    rating: 4.7,
    reviews: 72,
  },
  {
    id: 'sed6',
    title: 'Lexus ES 350 F Sport 2024',
    price: 49000,
    image: getVehicleSaleImageUrl('photo-1549317661-bd32c8ce0db2'),
    category: 'Sedanes',
    location: 'Houston, TX',
    rating: 4.9,
    reviews: 63,
  },
  {
    id: 'sed7',
    title: 'Mazda 6 Signature 2024',
    price: 38000,
    image: getVehicleSaleImageUrl('photo-1580273916550-e323be2ae537'),
    category: 'Sedanes',
    location: 'Phoenix, AZ',
    rating: 4.6,
    reviews: 45,
  },
  {
    id: 'sed8',
    title: 'Hyundai Sonata N Line 2024',
    price: 36500,
    image: getVehicleSaleImageUrl('photo-1605559424843-9e4c228bf1c2'),
    category: 'Sedanes',
    location: 'Atlanta, GA',
    rating: 4.5,
    reviews: 38,
  },
  {
    id: 'sed9',
    title: 'Kia K5 GT-Line 2024',
    price: 34000,
    image: getVehicleSaleImageUrl('photo-1603584173870-7f23fdae1b7a'),
    category: 'Sedanes',
    location: 'San Diego, CA',
    rating: 4.6,
    reviews: 52,
  },
  {
    id: 'sed10',
    title: 'Nissan Altima SR 2024',
    price: 32000,
    image: getVehicleSaleImageUrl('photo-1580274455191-1c62238fa333'),
    category: 'Sedanes',
    location: 'Denver, CO',
    rating: 4.4,
    reviews: 41,
  },
];
END sedanesListings */

/* MOCK DATA COMENTADO - suvsListings
const suvsListings = [
  {
    id: 'suv1',
    title: 'Toyota RAV4 Adventure 2024',
    price: 42000,
    image: getVehicleSaleImageUrl('photo-1519641471654-76ce0107ad1b'),
    category: 'SUVs',
    location: 'Seattle, WA',
    rating: 4.9,
    reviews: 187,
  },
  {
    id: 'suv2',
    title: 'Honda CR-V Touring 2024',
    price: 40500,
    image: getVehicleSaleImageUrl('photo-1568844293986-8c8f5e9f1a7a'),
    category: 'SUVs',
    location: 'Portland, OR',
    rating: 4.8,
    reviews: 143,
  },
  {
    id: 'suv3',
    title: 'BMW X5 xDrive40i 2024',
    price: 72000,
    image: getVehicleSaleImageUrl('photo-1606016159991-dfe4f2746ad5'),
    category: 'SUVs',
    location: 'Miami, FL',
    rating: 4.9,
    reviews: 98,
  },
  {
    id: 'suv4',
    title: 'Mercedes-Benz GLE 450 2024',
    price: 78000,
    image: getVehicleSaleImageUrl('photo-1563720223185-11003d516935'),
    category: 'SUVs',
    location: 'Los Angeles, CA',
    rating: 4.8,
    reviews: 76,
  },
  {
    id: 'suv5',
    title: 'Audi Q7 Premium Plus 2024',
    price: 68000,
    image: getVehicleSaleImageUrl('photo-1619767886558-efdc259cde1a'),
    category: 'SUVs',
    location: 'San Francisco, CA',
    rating: 4.7,
    reviews: 62,
  },
  {
    id: 'suv6',
    title: 'Jeep Grand Cherokee L 2024',
    price: 55000,
    image: getVehicleSaleImageUrl('photo-1533473359331-0135ef1b58bf'),
    category: 'SUVs',
    location: 'Austin, TX',
    rating: 4.6,
    reviews: 89,
  },
  {
    id: 'suv7',
    title: 'Ford Explorer ST 2024',
    price: 58000,
    image: getVehicleSaleImageUrl('photo-1560958089-b8a1929cea89'),
    category: 'SUVs',
    location: 'Nashville, TN',
    rating: 4.7,
    reviews: 67,
  },
  {
    id: 'suv8',
    title: 'Chevrolet Tahoe Premier 2024',
    price: 72000,
    image: getVehicleSaleImageUrl('photo-1551830820-330a71b99659'),
    category: 'SUVs',
    location: 'Detroit, MI',
    rating: 4.5,
    reviews: 54,
  },
  {
    id: 'suv9',
    title: 'Range Rover Sport HSE 2024',
    price: 95000,
    image: getVehicleSaleImageUrl('photo-1606016159991-dfe4f2746ad5'),
    category: 'SUVs',
    location: 'Beverly Hills, CA',
    rating: 4.9,
    reviews: 43,
  },
  {
    id: 'suv10',
    title: 'Porsche Cayenne S 2024',
    price: 98000,
    image: getVehicleSaleImageUrl('photo-1619767886558-efdc259cde1a'),
    category: 'SUVs',
    location: 'Scottsdale, AZ',
    rating: 4.9,
    reviews: 38,
  },
];
END suvsListings */

/* MOCK DATA COMENTADO - trucksListings
const trucksListings = [
  {
    id: 'truck1',
    title: 'Ford F-150 Platinum 2024',
    price: 68000,
    image: getVehicleSaleImageUrl('photo-1590362891991-f776e747a588'),
    category: 'Camionetas',
    location: 'Dallas, TX',
    rating: 4.9,
    reviews: 234,
  },
  {
    id: 'truck2',
    title: 'Chevrolet Silverado High Country 2024',
    price: 65000,
    image: getVehicleSaleImageUrl('photo-1583267746897-2cf1c11a8c77'),
    category: 'Camionetas',
    location: 'Houston, TX',
    rating: 4.8,
    reviews: 189,
  },
  {
    id: 'truck3',
    title: 'RAM 1500 Limited 2024',
    price: 72000,
    image: getVehicleSaleImageUrl('photo-1590362891991-f776e747a588'),
    category: 'Camionetas',
    location: 'Phoenix, AZ',
    rating: 4.9,
    reviews: 156,
  },
  {
    id: 'truck4',
    title: 'Toyota Tundra TRD Pro 2024',
    price: 62000,
    image: getVehicleSaleImageUrl('photo-1559416523-140ddc3d238c'),
    category: 'Camionetas',
    location: 'Denver, CO',
    rating: 4.8,
    reviews: 123,
  },
  {
    id: 'truck5',
    title: 'GMC Sierra Denali Ultimate 2024',
    price: 78000,
    image: getVehicleSaleImageUrl('photo-1544636331-e26879cd4d9b'),
    category: 'Camionetas',
    location: 'Salt Lake City, UT',
    rating: 4.7,
    reviews: 87,
  },
  {
    id: 'truck6',
    title: 'Ford Raptor R 2024',
    price: 115000,
    image: getVehicleSaleImageUrl('photo-1590362891991-f776e747a588'),
    category: 'Camionetas',
    location: 'Las Vegas, NV',
    rating: 4.9,
    reviews: 98,
  },
  {
    id: 'truck7',
    title: 'Nissan Titan PRO-4X 2024',
    price: 55000,
    image: getVehicleSaleImageUrl('photo-1583121274602-3e2820c69888'),
    category: 'Camionetas',
    location: 'San Antonio, TX',
    rating: 4.5,
    reviews: 56,
  },
  {
    id: 'truck8',
    title: 'Rivian R1T Adventure 2024',
    price: 85000,
    image: getVehicleSaleImageUrl('photo-1544636331-e26879cd4d9b'),
    category: 'Camionetas',
    location: 'San Jose, CA',
    rating: 4.8,
    reviews: 67,
  },
  {
    id: 'truck9',
    title: 'Tesla Cybertruck 2024',
    price: 98000,
    image: getVehicleSaleImageUrl('photo-1617788138017-80ad40651399'),
    category: 'Camionetas',
    location: 'Austin, TX',
    rating: 4.7,
    reviews: 145,
  },
  {
    id: 'truck10',
    title: 'Ford F-250 Tremor 2024',
    price: 75000,
    image: getVehicleSaleImageUrl('photo-1583267746897-2cf1c11a8c77'),
    category: 'Camionetas',
    location: 'Oklahoma City, OK',
    rating: 4.6,
    reviews: 43,
  },
];
END trucksListings */

/* MOCK DATA COMENTADO - deportivosListings
const deportivosListings = [
  {
    id: 'dep1',
    title: 'Porsche 911 Carrera S 2024',
    price: 145000,
    image: getVehicleSaleImageUrl('photo-1503376780353-7e6692767b70'),
    category: 'Deportivos',
    location: 'Miami, FL',
    rating: 5.0,
    reviews: 89,
  },
  {
    id: 'dep2',
    title: 'BMW M4 Competition 2024',
    price: 85000,
    image: getVehicleSaleImageUrl('photo-1617814076367-b759c7d7e738'),
    category: 'Deportivos',
    location: 'Los Angeles, CA',
    rating: 4.9,
    reviews: 76,
  },
  {
    id: 'dep3',
    title: 'Mercedes-AMG GT 63 S 2024',
    price: 178000,
    image: getVehicleSaleImageUrl('photo-1618843479313-40f8afb4b4d8'),
    category: 'Deportivos',
    location: 'New York, NY',
    rating: 4.9,
    reviews: 54,
  },
  {
    id: 'dep4',
    title: 'Audi RS7 Sportback 2024',
    price: 128000,
    image: getVehicleSaleImageUrl('photo-1606664515524-ed2f786a0bd6'),
    category: 'Deportivos',
    location: 'Chicago, IL',
    rating: 4.8,
    reviews: 62,
  },
  {
    id: 'dep5',
    title: 'Chevrolet Corvette Z06 2024',
    price: 115000,
    image: getVehicleSaleImageUrl('photo-1552519507-da3b142c6e3d'),
    category: 'Deportivos',
    location: 'Detroit, MI',
    rating: 4.9,
    reviews: 98,
  },
  {
    id: 'dep6',
    title: 'Ford Mustang Dark Horse 2024',
    price: 65000,
    image: getVehicleSaleImageUrl('photo-1584345604476-8ec5f82bd3f2'),
    category: 'Deportivos',
    location: 'Dallas, TX',
    rating: 4.7,
    reviews: 87,
  },
  {
    id: 'dep7',
    title: 'Nissan GT-R Nismo 2024',
    price: 225000,
    image: getVehicleSaleImageUrl('photo-1580273916550-e323be2ae537'),
    category: 'Deportivos',
    location: 'Las Vegas, NV',
    rating: 4.9,
    reviews: 45,
  },
  {
    id: 'dep8',
    title: 'Lamborghini Huracán EVO 2024',
    price: 285000,
    image: getVehicleSaleImageUrl('photo-1544636331-e26879cd4d9b'),
    category: 'Deportivos',
    location: 'Beverly Hills, CA',
    rating: 5.0,
    reviews: 34,
  },
  {
    id: 'dep9',
    title: 'Ferrari Roma 2024',
    price: 275000,
    image: getVehicleSaleImageUrl('photo-1583121274602-3e2820c69888'),
    category: 'Deportivos',
    location: 'Scottsdale, AZ',
    rating: 5.0,
    reviews: 28,
  },
  {
    id: 'dep10',
    title: 'McLaren 720S Spider 2024',
    price: 315000,
    image: getVehicleSaleImageUrl('photo-1621135802920-133df287f89c'),
    category: 'Deportivos',
    location: 'Palm Beach, FL',
    rating: 5.0,
    reviews: 21,
  },
];
END deportivosListings */

/* MOCK DATA COMENTADO - vehiculosListings
const vehiculosListings = [
  {
    id: 'v1',
    title: 'Mercedes-Benz Clase C AMG 2024',
    price: 75000,
    image: getVehicleSaleImageUrl('photo-1618843479313-40f8afb4b4d8'),
    category: 'Vehículos',
    location: 'Miami, FL',
    rating: 4.9,
    reviews: 47,
  },
  {
    id: 'v2',
    title: 'BMW Serie 7 Executive Package',
    price: 95000,
    image: getVehicleSaleImageUrl('photo-1555215695-3004980ad54e'),
    category: 'Vehículos',
    location: 'Los Angeles, CA',
    rating: 4.8,
    reviews: 62,
  },
  {
    id: 'v3',
    title: 'Porsche 911 Carrera S',
    price: 135000,
    image: getVehicleSaleImageUrl('photo-1503376780353-7e6692767b70'),
    category: 'Vehículos',
    location: 'New York, NY',
    rating: 4.9,
    reviews: 89,
  },
  {
    id: 'v4',
    title: 'Audi RS7 Sportback 2024',
    price: 128000,
    image: getVehicleSaleImageUrl('photo-1606664515524-ed2f786a0bd6'),
    category: 'Vehículos',
    location: 'Dallas, TX',
    rating: 4.7,
    reviews: 31,
  },
  {
    id: 'v5',
    title: 'Tesla Model S Plaid',
    price: 108000,
    image: getVehicleSaleImageUrl('photo-1617788138017-80ad40651399'),
    category: 'Vehículos',
    location: 'San Francisco, CA',
    rating: 4.8,
    reviews: 56,
  },
  {
    id: 'v6',
    title: 'Range Rover Sport HSE',
    price: 89000,
    image: getVehicleSaleImageUrl('photo-1606016159991-dfe4f2746ad5'),
    category: 'Vehículos',
    location: 'Phoenix, AZ',
    rating: 4.6,
    reviews: 28,
  },
];
END vehiculosListings */

/* MOCK DATA COMENTADO - rentaVehiculosListings
const rentaVehiculosListings = [
  {
    id: 'rv1',
    title: 'BMW X5 - Renta por Día',
    price: 150,
    priceLabel: '/día',
    image: getVehicleRentImageUrl('photo-1549317661-bd32c8ce0db2'),
    category: 'Renta de Vehículos',
    location: 'Miami, FL',
    rating: 4.9,
    reviews: 124,
  },
  {
    id: 'rv2',
    title: 'Mercedes GLE Coupe - Renta Semanal',
    price: 850,
    priceLabel: '/semana',
    image: getVehicleRentImageUrl('photo-1563720223185-11003d516935'),
    category: 'Renta de Vehículos',
    location: 'Los Angeles, CA',
    rating: 4.8,
    reviews: 89,
  },
  {
    id: 'rv3',
    title: 'Porsche Cayenne - Renta Premium',
    price: 200,
    priceLabel: '/día',
    image: getVehicleRentImageUrl('photo-1619767886558-efdc259cde1a'),
    category: 'Renta de Vehículos',
    location: 'Las Vegas, NV',
    rating: 5.0,
    reviews: 67,
  },
  {
    id: 'rv4',
    title: 'Cadillac Escalade - Eventos',
    price: 280,
    priceLabel: '/día',
    image: getVehicleRentImageUrl('photo-1533473359331-0135ef1b58bf'),
    category: 'Renta de Vehículos',
    location: 'Atlanta, GA',
    rating: 4.7,
    reviews: 45,
  },
  {
    id: 'rv5',
    title: 'Tesla Model X - Renta Ecológica',
    price: 175,
    priceLabel: '/día',
    image: getVehicleRentImageUrl('photo-1560958089-b8a1929cea89'),
    category: 'Renta de Vehículos',
    location: 'San Diego, CA',
    rating: 4.9,
    reviews: 98,
  },
  {
    id: 'rv6',
    title: 'Range Rover Velar - Lujo',
    price: 190,
    priceLabel: '/día',
    image: getVehicleRentImageUrl('photo-1551830820-330a71b99659'),
    category: 'Renta de Vehículos',
    location: 'Orlando, FL',
    rating: 4.6,
    reviews: 52,
  },
];
END rentaVehiculosListings */

/* MOCK DATA COMENTADO - propiedadesListings
const propiedadesListings = [
  {
    id: 'p1',
    title: 'Penthouse de Lujo con Vista al Mar',
    price: 1250000,
    image: getPropertySaleImageUrl('photo-1600607687939-ce8a6c25118c'),
    category: 'Propiedades',
    location: 'Cancún, MX',
    rating: 5.0,
    reviews: 23,
  },
  {
    id: 'p2',
    title: 'Villa Contemporánea con Piscina',
    price: 875000,
    image: getPropertySaleImageUrl('photo-1600596542815-ffad4c1539a9'),
    category: 'Propiedades',
    location: 'Houston, TX',
    rating: 4.7,
    reviews: 35,
  },
  {
    id: 'p3',
    title: 'Apartamento Moderno Centro',
    price: 450000,
    image: getPropertySaleImageUrl('photo-1502672260266-1c1ef2d93688'),
    category: 'Propiedades',
    location: 'Chicago, IL',
    rating: 4.6,
    reviews: 41,
  },
  {
    id: 'p4',
    title: 'Casa Colonial con Jardín Amplio',
    price: 680000,
    image: getPropertySaleImageUrl('photo-1600585154340-be6161a56a0c'),
    category: 'Propiedades',
    location: 'San Antonio, TX',
    rating: 4.8,
    reviews: 29,
  },
  {
    id: 'p5',
    title: 'Loft Industrial Renovado',
    price: 395000,
    image: getPropertySaleImageUrl('photo-1600607688969-a5bfcd646154'),
    category: 'Propiedades',
    location: 'Brooklyn, NY',
    rating: 4.5,
    reviews: 18,
  },
  {
    id: 'p6',
    title: 'Mansión con Vista Panorámica',
    price: 2150000,
    image: getPropertySaleImageUrl('photo-1613490493576-7fde63acd811'),
    category: 'Propiedades',
    location: 'Beverly Hills, CA',
    rating: 4.9,
    reviews: 12,
  },
];
END propiedadesListings */

/* MOCK DATA COMENTADO - hospedajeListings
const hospedajeListings = [
  {
    id: 'h1',
    title: 'Suite Premium Frente al Mar',
    price: 250,
    priceLabel: '/noche',
    image: getLodgingImageUrl('photo-1582719478250-c89cae4dc85b'),
    category: 'Hospedaje',
    location: 'Playa del Carmen, MX',
    rating: 4.9,
    reviews: 156,
  },
  {
    id: 'h2',
    title: 'Apartamento Ejecutivo Centro',
    price: 120,
    priceLabel: '/noche',
    image: getLodgingImageUrl('photo-1560448204-e02f11c3d0e2'),
    category: 'Hospedaje',
    location: 'Miami Beach, FL',
    rating: 4.7,
    reviews: 89,
  },
  {
    id: 'h3',
    title: 'Villa Privada con Alberca',
    price: 380,
    priceLabel: '/noche',
    image: getLodgingImageUrl('photo-1602002418082-a4443e081dd1'),
    category: 'Hospedaje',
    location: 'Tulum, MX',
    rating: 5.0,
    reviews: 67,
  },
  {
    id: 'h4',
    title: 'Cabaña Rústica en la Montaña',
    price: 95,
    priceLabel: '/noche',
    image: getLodgingImageUrl('photo-1587061949409-02df41d5e562'),
    category: 'Hospedaje',
    location: 'Aspen, CO',
    rating: 4.8,
    reviews: 134,
  },
  {
    id: 'h5',
    title: 'Penthouse con Terraza Privada',
    price: 320,
    priceLabel: '/noche',
    image: getLodgingImageUrl('photo-1578683010236-d716f9a3f461'),
    category: 'Hospedaje',
    location: 'New York, NY',
    rating: 4.6,
    reviews: 78,
  },
  {
    id: 'h6',
    title: 'Casa de Playa con Jacuzzi',
    price: 275,
    priceLabel: '/noche',
    image: getLodgingImageUrl('photo-1499793983690-e29da59ef1c2'),
    category: 'Hospedaje',
    location: 'Malibu, CA',
    rating: 4.9,
    reviews: 92,
  },
];
END hospedajeListings */

/* ============================================================
 * FIN DE MOCK DATA COMENTADO
 * ============================================================ */

// Vertical categories configuration
const verticals = [
  {
    id: 'vehicles',
    name: 'Vehículos',
    description: 'Compra y vende vehículos nuevos y usados',
    icon: FaCar,
    href: '/vehicles',
    gradient: 'from-blue-500 to-blue-600',
    bgLight: 'bg-blue-50',
    textColor: 'text-blue-600',
    image: getVehicleSaleImageUrl('photo-1492144534655-ae79c964c9d7'),
  },
  {
    id: 'vehicle-rental',
    name: 'Renta de Vehículos',
    description: 'Alquila el vehículo perfecto',
    icon: FaKey,
    href: '/vehicle-rental',
    gradient: 'from-amber-500 to-amber-600',
    bgLight: 'bg-amber-50',
    textColor: 'text-amber-600',
    image: getVehicleRentImageUrl('photo-1449965408869-eaa3f722e40d'),
  },
  {
    id: 'properties',
    name: 'Propiedades',
    description: 'Encuentra tu próximo hogar',
    icon: FaHome,
    href: '/properties',
    gradient: 'from-emerald-500 to-emerald-600',
    bgLight: 'bg-emerald-50',
    textColor: 'text-emerald-600',
    image: getPropertySaleImageUrl('photo-1600585154340-be6161a56a0c'),
  },
  {
    id: 'lodging',
    name: 'Hospedaje',
    description: 'Alojamientos para cada viaje',
    icon: FaBed,
    href: '/lodging',
    gradient: 'from-purple-500 to-purple-600',
    bgLight: 'bg-purple-50',
    textColor: 'text-purple-600',
    image: getLodgingImageUrl('photo-1566073771259-6a8506099945'),
  },
];

const features = [
  {
    icon: FiSearch,
    title: 'Encuentra lo que Buscas',
    description: 'Filtros avanzados y búsqueda inteligente para encontrar el producto perfecto.',
  },
  {
    icon: FiZap,
    title: 'Vende Más Rápido',
    description: 'Publica en minutos y conecta con compradores interesados en lo que ofreces.',
  },
  {
    icon: FiShield,
    title: 'Compra con Confianza',
    description: 'Todas las transacciones son seguras y protegidas.',
  },
  {
    icon: FiMessageCircle,
    title: 'Contacto Directo',
    description: 'Habla directamente con vendedores o compradores sin intermediarios.',
  },
];

// Format price helper
const formatPrice = (price: number, currency = 'USD') => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(price);
};

// Category to color mapping
const getCategoryColor = (category: string): string => {
  switch (category) {
    case 'Vehículos':
      return 'blue';
    case 'Sedanes':
      return 'blue';
    case 'SUVs':
      return 'indigo';
    case 'Camionetas':
      return 'slate';
    case 'Deportivos':
      return 'red';
    case 'Renta de Vehículos':
      return 'amber';
    case 'Propiedades':
      return 'emerald';
    case 'Hospedaje':
      return 'purple';
    default:
      return 'blue';
  }
};

// Get Tailwind classes for category colors (to ensure they're included in build)
const getCategoryClasses = (category: string) => {
  const color = getCategoryColor(category);
  const colorClasses: Record<string, { badge: string; price: string }> = {
    blue: { badge: 'bg-blue-500', price: 'text-blue-600' },
    indigo: { badge: 'bg-indigo-500', price: 'text-indigo-600' },
    slate: { badge: 'bg-slate-600', price: 'text-slate-700' },
    red: { badge: 'bg-red-500', price: 'text-red-600' },
    amber: { badge: 'bg-amber-500', price: 'text-amber-600' },
    emerald: { badge: 'bg-emerald-500', price: 'text-emerald-600' },
    purple: { badge: 'bg-purple-500', price: 'text-purple-600' },
  };
  return colorClasses[color] || colorClasses.blue;
};

// Featured Section Component
interface FeaturedSectionProps {
  title: string;
  subtitle: string;
  listings: Array<{
    id: string;
    title: string;
    price: number;
    priceLabel?: string;
    image: string;
    category: string;
    location: string;
    rating: number;
    reviews: number;
  }>;
  viewAllHref: string;
  accentColor: string;
}

// Helper para obtener las clases de color de los botones de navegación
const getAccentClasses = (color: string) => {
  const colorMap: Record<string, { border: string; text: string; hoverBg: string; link: string; linkHover: string }> = {
    blue: {
      border: 'border-blue-500',
      text: 'text-blue-600',
      hoverBg: 'hover:bg-blue-500',
      link: 'text-blue-600',
      linkHover: 'hover:text-blue-700',
    },
    amber: {
      border: 'border-amber-500',
      text: 'text-amber-600',
      hoverBg: 'hover:bg-amber-500',
      link: 'text-amber-600',
      linkHover: 'hover:text-amber-700',
    },
    emerald: {
      border: 'border-emerald-500',
      text: 'text-emerald-600',
      hoverBg: 'hover:bg-emerald-500',
      link: 'text-emerald-600',
      linkHover: 'hover:text-emerald-700',
    },
    purple: {
      border: 'border-purple-500',
      text: 'text-purple-600',
      hoverBg: 'hover:bg-purple-500',
      link: 'text-purple-600',
      linkHover: 'hover:text-purple-700',
    },
  };
  return colorMap[color] || colorMap.blue;
};

const FeaturedSection: React.FC<FeaturedSectionProps> = ({ title, subtitle, listings, viewAllHref, accentColor }) => {
  const scrollRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(true);
  const colorClasses = getAccentClasses(accentColor);

  const checkScroll = () => {
    if (scrollRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollRef.current;
      setCanScrollLeft(scrollLeft > 0);
      setCanScrollRight(scrollLeft < scrollWidth - clientWidth - 10);
    }
  };

  const scroll = (direction: 'left' | 'right') => {
    if (scrollRef.current) {
      scrollRef.current.scrollBy({
        left: direction === 'left' ? -350 : 350,
        behavior: 'smooth',
      });
      setTimeout(checkScroll, 300);
    }
  };

  return (
    <section className="py-6 bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="flex items-end justify-between mb-4">
          <div>
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-1">{title}</h2>
            <p className="text-gray-600">{subtitle}</p>
          </div>
          <div className="flex items-center gap-3">
            {/* Navigation arrows */}
            <button
              onClick={() => scroll('left')}
              disabled={!canScrollLeft}
              className={`p-2 rounded-full border-2 transition-all ${
                canScrollLeft
                  ? `${colorClasses.border} ${colorClasses.text} ${colorClasses.hoverBg} hover:text-white`
                  : 'border-gray-200 text-gray-300 cursor-not-allowed'
              }`}
            >
              <FiChevronLeft className="w-5 h-5" />
            </button>
            <button
              onClick={() => scroll('right')}
              disabled={!canScrollRight}
              className={`p-2 rounded-full border-2 transition-all ${
                canScrollRight
                  ? `${colorClasses.border} ${colorClasses.text} ${colorClasses.hoverBg} hover:text-white`
                  : 'border-gray-200 text-gray-300 cursor-not-allowed'
              }`}
            >
              <FiChevronRight className="w-5 h-5" />
            </button>
            <Link
              to={viewAllHref}
              className={`hidden sm:flex items-center gap-1 ${colorClasses.link} ${colorClasses.linkHover} font-medium`}
            >
              Ver todo
              <FiArrowRight className="w-4 h-4" />
            </Link>
          </div>
        </div>

        {/* Scrollable cards */}
        <div
          ref={scrollRef}
          onScroll={checkScroll}
          className="flex gap-4 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4"
          style={{ scrollSnapType: 'x mandatory' }}
        >
          {listings.map((listing) => {
            const categoryClasses = getCategoryClasses(listing.category);
            // Generate SEO-friendly URL based on category
            const listingUrl = listing.category === 'Vehículos' || listing.category === 'Renta'
              ? generateListingUrl(listing.id, listing.title, '/vehicles')
              : generateListingUrl(listing.id, listing.title, '/properties');
            
            return (
              <Link
                key={listing.id}
                to={listingUrl}
                className="flex-shrink-0 w-72 bg-white rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 overflow-hidden group"
                style={{ scrollSnapAlign: 'start' }}
              >
                {/* Image */}
                <div className="relative h-48 overflow-hidden">
                  <img
                    src={listing.image}
                    alt={listing.title}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <div className="absolute top-3 left-3">
                    <span className={`px-3 py-1 ${categoryClasses.badge} text-white text-xs font-medium rounded-full`}>
                      {listing.category}
                    </span>
                  </div>
                </div>

                {/* Content */}
                <div className="p-4">
                  <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2 group-hover:text-blue-600 transition-colors">
                    {listing.title}
                  </h3>

                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-3">
                    <FiMapPin className="w-4 h-4" />
                    {listing.location}
                  </div>

                  <div className="flex items-center justify-between">
                    <p className={`text-xl font-bold ${categoryClasses.price}`}>
                      {formatPrice(listing.price)}
                      {listing.priceLabel && <span className="text-sm font-normal text-gray-500">{listing.priceLabel}</span>}
                    </p>
                    <div className="flex items-center gap-1 text-sm">
                      <FiStar className="w-4 h-4 text-amber-400 fill-current" />
                      <span className="font-medium">{listing.rating}</span>
                      <span className="text-gray-400">({listing.reviews})</span>
                    </div>
                  </div>
                </div>
              </Link>
            );
          })}
        </div>

        {/* Mobile view all link */}
        <div className="sm:hidden text-center mt-4">
          <Link
            to={viewAllHref}
            className={`inline-flex items-center gap-1 ${colorClasses.link} font-medium`}
          >
            Ver todo
            <FiArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </div>
    </section>
  );
};

/**
 * Transform VehicleListing from API to Vehicle format for HeroCarousel/FeaturedListingGrid
 */
const transformToVehicle = (listing: VehicleListing): Vehicle => ({
  id: listing.id,
  make: listing.make,
  model: listing.model,
  year: listing.year,
  price: listing.price,
  mileage: listing.mileage,
  location: listing.location,
  images: listing.images.length > 0 ? listing.images : ['/placeholder-car.jpg'],
  isFeatured: listing.isFeatured,
  isNew: listing.condition === 'New',
  transmission: (listing.transmission as Vehicle['transmission']) || 'Automatic',
  fuelType: (listing.fuelType as Vehicle['fuelType']) || 'Gasoline',
  bodyType: (listing.bodyStyle as Vehicle['bodyType']) || 'Sedan',
  drivetrain: 'FWD',
  engine: '2.0L',
  horsepower: 200,
  mpg: { city: 25, highway: 32 },
  color: listing.exteriorColor || 'Unknown',
  interiorColor: 'Black',
  vin: 'N/A',
  condition: (listing.condition as Vehicle['condition']) || 'Used',
  features: [],
  description: listing.description,
  seller: {
    name: listing.sellerName || 'Dealer',
    type: 'Dealer',
    rating: 4.8,
    phone: '',
  },
});

const HomePage: React.FC = () => {
  // Fetch real vehicles from VehiclesSaleService API
  const { vehicles: apiVehicles, isLoading, error } = useVehiclesSaleList(1, 20);
  
  // Debug: Log API response (remove in production)
  console.log('[HomePage] API State:', { 
    isLoading, 
    error, 
    apiVehiclesCount: apiVehicles.length,
    apiVehicles: apiVehicles.slice(0, 2)
  });
  
  // Check if we're using API data or fallback to mock
  // ONLY fallback to mock if there's an explicit error, NOT when loading or empty
  const isUsingMockData = !!error && !isLoading;
  
  // Transform API vehicles to Vehicle format
  const transformedVehicles = useMemo(() => {
    // If loading, return empty array to show skeleton
    if (isLoading) {
      return [];
    }
    // If we have API vehicles, use them
    if (apiVehicles.length > 0) {
      return apiVehicles.map(transformToVehicle);
    }
    // If there's an error, fallback to mock data
    if (error) {
      console.warn('[HomePage] API error, falling back to mock data:', error);
      return mockVehicles;
    }
    // No vehicles and no error = empty database
    return [];
  }, [apiVehicles, isLoading, error]);

  // Get featured vehicles for hero carousel (top 5 by ranking)
  const heroVehicles = useMemo(() => {
    return mixFeaturedAndOrganic(transformedVehicles, 'home').slice(0, 5);
  }, [transformedVehicles]);

  // Get featured vehicles for homepage grid (exclude hero vehicles)
  const gridVehicles = useMemo(() => {
    const heroIds = new Set(heroVehicles.map(v => v.id));
    return transformedVehicles.filter(v => !heroIds.has(v.id));
  }, [heroVehicles, transformedVehicles]);

  return (
    <MainLayout>
      {/* Hero Carousel - Sprint 5.2: 100% Visible, No Search Overlay */}
      {isLoading ? (
        <div className="h-[70vh] bg-gradient-to-r from-gray-800 to-gray-900 flex items-center justify-center">
          <div className="text-center">
            <div className="animate-spin text-4xl mb-4">⏳</div>
            <p className="text-white text-xl">Cargando vehículos destacados...</p>
          </div>
        </div>
      ) : heroVehicles.length > 0 ? (
        <HeroCarousel 
          vehicles={heroVehicles} 
          autoPlayInterval={5000}
          showScrollHint={false}
        />
      ) : (
        <div className="h-[70vh] bg-gradient-to-r from-gray-800 to-gray-900 flex items-center justify-center">
          <div className="text-center">
            <p className="text-white text-xl">No hay vehículos destacados disponibles</p>
            {error && <p className="text-red-400 text-sm mt-2">Error: {error}</p>}
          </div>
        </div>
      )}

      {/* Featured Listings Grid - Sprint 5.2: Immediately After Hero */}
      <section className="py-6 bg-gradient-to-b from-white to-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-6">
            <div className="flex items-center justify-center gap-2 mb-2">
              <h2 className="text-3xl md:text-4xl font-bold text-gray-900">
                Vehículos Destacados
              </h2>
              {/* Data source indicator */}
              {isLoading ? (
                <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-700">
                  <span className="animate-spin">⏳</span>
                  Cargando...
                </span>
              ) : (
                <span 
                  className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${
                    isUsingMockData 
                      ? 'bg-amber-100 text-amber-700' 
                      : 'bg-green-100 text-green-700'
                  }`}
                  title={isUsingMockData ? 'Usando datos demo (error en API)' : 'Datos en vivo de la base de datos'}
                >
                  {isUsingMockData ? <FiWifiOff size={12} /> : <FiWifi size={12} />}
                  {isUsingMockData ? 'Demo' : 'Live'}
                </span>
              )}
            </div>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              {isLoading 
                ? 'Conectando con la base de datos...' 
                : transformedVehicles.length > 0
                  ? `Explora nuestra selección de ${transformedVehicles.length} vehículos verificados`
                  : 'No hay vehículos disponibles en este momento'}
            </p>
          </div>
          
          {isLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {[1, 2, 3, 4, 5, 6].map((i) => (
                <div key={i} className="bg-gray-200 animate-pulse rounded-lg h-72" />
              ))}
            </div>
          ) : (
            <FeaturedListingGrid vehicles={gridVehicles} />
          )}
        </div>
      </section>

      {/* MOCK DATA COMENTADO - FeaturedSection components que usan mocks
      
      <FeaturedSection
        title="Destacados de la Semana"
        subtitle="Selección exclusiva de nuestros mejores anuncios"
        listings={[
          ...vehiculosListings.slice(0, 2),
          ...propiedadesListings.slice(0, 2),
          ...rentaVehiculosListings.slice(0, 1),
          ...hospedajeListings.slice(0, 1),
        ]}
        viewAllHref="/browse"
        accentColor="blue"
      />

      <FeaturedSection
        title="Sedanes"
        subtitle="Elegancia y confort para tu día a día"
        listings={sedanesListings}
        viewAllHref="/vehicles?category=sedanes"
        accentColor="blue"
      />

      <FeaturedSection
        title="SUVs"
        subtitle="Espacio, potencia y versatilidad"
        listings={suvsListings}
        viewAllHref="/vehicles?category=suvs"
        accentColor="blue"
      />

      <FeaturedSection
        title="Camionetas"
        subtitle="Potencia y capacidad para cualquier trabajo"
        listings={trucksListings}
        viewAllHref="/vehicles?category=trucks"
        accentColor="blue"
      />

      <FeaturedSection
        title="Deportivos"
        subtitle="Velocidad y adrenalina en cada curva"
        listings={deportivosListings}
        viewAllHref="/vehicles?category=deportivos"
        accentColor="blue"
      />

      <FeaturedSection
        title="Renta de Vehículos"
        subtitle="Alquila el vehículo perfecto para cualquier ocasión"
        listings={rentaVehiculosListings}
        viewAllHref="/vehicle-rental"
        accentColor="amber"
      />

      <FeaturedSection
        title="Propiedades Destacadas"
        subtitle="Encuentra tu próximo hogar o inversión"
        listings={propiedadesListings}
        viewAllHref="/properties"
        accentColor="emerald"
      />

      <FeaturedSection
        title="Hospedaje Destacado"
        subtitle="Apartamentos, hoteles y alojamientos"
        listings={hospedajeListings}
        viewAllHref="/lodging"
        accentColor="purple"
      />
      
      END FeaturedSection components con mocks */}

      {/* Categories Section */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Explora por Categoría
            </h2>
            <p className="text-gray-600">
              Encuentra exactamente lo que buscas
            </p>
          </div>

          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
            {verticals.map((vertical) => (
              <Link
                key={vertical.id}
                to={vertical.href}
                className="group relative bg-white rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 overflow-hidden"
              >
                {/* Image */}
                <div className="aspect-square relative overflow-hidden">
                  <img
                    src={vertical.image}
                    alt={vertical.name}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-gray-900/80 via-gray-900/20 to-transparent" />
                  
                  {/* Content overlay */}
                  <div className="absolute bottom-0 left-0 right-0 p-4">
                    <div className={`w-12 h-12 ${vertical.bgLight} rounded-xl flex items-center justify-center mb-3`}>
                      <vertical.icon className={`w-6 h-6 ${vertical.textColor}`} />
                    </div>
                    <h3 className="text-lg font-bold text-white mb-1">
                      {vertical.name}
                    </h3>
                    <p className="text-gray-300 text-sm">{vertical.description}</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Todo lo que Necesitas
            </h2>
            <p className="text-gray-600">
              Compra y vende de manera fácil, rápida y segura
            </p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="text-center p-4 bg-gray-50 rounded-2xl"
              >
                <div className="w-14 h-14 bg-blue-100 rounded-xl flex items-center justify-center mx-auto mb-3">
                  <feature.icon className="w-6 h-6 text-blue-600" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-1">
                  {feature.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {feature.description}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* How it Works */}
      <section className="py-6 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-4">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 mb-2">
              Cómo Funciona
            </h2>
            <p className="text-gray-600">
              Tres simples pasos para encontrar lo que buscas
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 max-w-4xl mx-auto">
            {[
              { step: '1', title: 'Explora', desc: 'Navega por miles de anuncios en las categorías que te interesan.' },
              { step: '2', title: 'Conecta', desc: 'Contacta directamente con vendedores para resolver tus dudas.' },
              { step: '3', title: 'Disfruta', desc: 'Cierra el trato y disfruta de tu nueva adquisición.' },
            ].map((item, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="relative text-center"
              >
                {index < 2 && (
                  <div className="hidden md:block absolute top-10 left-[60%] w-[80%] h-0.5 bg-gray-300" />
                )}
                
                <div className="relative z-10 w-20 h-20 bg-white rounded-2xl shadow-md flex items-center justify-center mx-auto mb-3">
                  <span className="text-2xl font-bold text-blue-600">{item.step}</span>
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-1">
                  {item.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {item.desc}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-6 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-gradient-to-br from-blue-600 to-emerald-600 rounded-3xl p-6 lg:p-8 text-center text-white">
            <h2 className="text-2xl lg:text-3xl font-bold mb-3">
              ¿Listo para empezar?
            </h2>
            <p className="text-blue-100 mb-6 max-w-xl mx-auto">
              Publica tu anuncio hoy y conecta con miles de compradores interesados
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/sell-your-car"
                className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-white text-blue-600 hover:bg-gray-100 font-medium rounded-xl transition-colors"
              >
                <FaCar className="w-5 h-5" />
                Publicar Vehículo
              </Link>
              <Link
                to="/properties/new"
                className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-white/20 hover:bg-white/30 text-white font-medium rounded-xl transition-colors border border-white/30"
              >
                <FaHome className="w-5 h-5" />
                Publicar Propiedad
              </Link>
            </div>
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default HomePage;
