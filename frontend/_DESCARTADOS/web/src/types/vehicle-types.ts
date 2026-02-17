// Auto-generated vehicle types from NHTSA catalog
// Generated: 2026-01-02T22:14:03.218566

export interface VehicleMake {
  id: number;
  name: string;
  slug: string;
  logoUrl?: string;
  country?: string;
}

export interface VehicleModel {
  id: number;
  makeId: number;
  name: string;
  slug: string;
  bodyType?: string;
}

export interface VehicleYear {
  id: number;
  modelId: number;
  year: number;
}

export interface VehicleTrim {
  id: number;
  yearId: number;
  name: string;
  engine?: string;
  transmission?: string;
  drivetrain?: string;
  fuelType?: string;
  mpgCity?: number;
  mpgHighway?: number;
  horsepower?: number;
  torque?: number;
  msrp?: number;
}

// Available makes in the catalog
export const VEHICLE_MAKES = [
  "Acura",
  "Alfa Romeo",
  "Audi",
  "BMW",
  "Bentley",
  "Buick",
  "Cadillac",
  "Chevrolet",
  "Chrysler",
  "Dodge",
  "Ferrari",
  "Fiat",
  "Ford",
  "GMC",
  "Genesis",
  "Honda",
  "Hyundai",
  "Infiniti",
  "Jaguar",
  "Jeep",
  "Kia",
  "Lamborghini",
  "Land Rover",
  "Lexus",
  "Lincoln",
  "Maserati",
  "Mazda",
  "Mercedes-Benz",
  "Mini",
  "Mitsubishi",
  "Nissan",
  "Porsche",
  "RAM",
  "Rolls-Royce",
  "Subaru",
  "Tesla",
  "Toyota",
  "Volkswagen",
  "Volvo",
] as const;

export type VehicleMakeName = typeof VEHICLE_MAKES[number];
