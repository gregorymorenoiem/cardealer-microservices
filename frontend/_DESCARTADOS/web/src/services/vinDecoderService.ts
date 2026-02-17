/**
 * VIN Decoder Service
 *
 * Utiliza la API gratuita NHTSA vPIC para decodificar VINs
 * https://vpic.nhtsa.dot.gov/api/
 *
 * Esta API es GRATUITA y no requiere API key.
 * Proporciona: Marca, Modelo, Año, Motor, Transmisión, Tipo carrocería, etc.
 */

import axios from 'axios';

const NHTSA_API_URL = 'https://vpic.nhtsa.dot.gov/api/vehicles';

export interface VinDecodeResult {
  success: boolean;
  vin: string;

  // Datos básicos del vehículo
  make?: string;
  model?: string;
  year?: number;
  trim?: string;

  // Especificaciones
  bodyClass?: string; // Tipo de carrocería (Sedan, SUV, etc.)
  driveType?: string; // FWD, RWD, AWD, 4WD
  engineCylinders?: string; // Número de cilindros
  engineSize?: string; // Tamaño del motor (ej: "2.5L")
  engineModel?: string; // Modelo del motor
  fuelType?: string; // Gasoline, Diesel, Electric, etc.
  transmission?: string; // Automatic, Manual, CVT
  horsepower?: string; // Potencia
  doors?: number; // Número de puertas
  seats?: number; // Número de asientos

  // Precio sugerido (MSRP)
  basePrice?: number;

  // Features de seguridad detectados (para auto-select en FeaturesStep)
  safetyFeatures?: string[];

  // Información adicional
  manufacturer?: string; // Fabricante
  plantCountry?: string; // País de fabricación
  plantCity?: string; // Ciudad de fabricación
  vehicleType?: string; // PASSENGER CAR, TRUCK, etc.

  // Errores
  errorCode?: string;
  errorMessage?: string;
  suggestedVIN?: string; // VIN corregido si hay error menor
}

// DecodeVinValues devuelve un objeto con propiedades directas
interface NHTSADecodedVehicle {
  Make: string;
  Model: string;
  ModelYear: string;
  Trim: string;
  Trim2: string;
  BodyClass: string;
  DriveType: string;
  EngineNumberofCylinders: string;
  DisplacementL: string;
  EngineModel: string;
  FuelTypePrimary: string;
  TransmissionStyle: string;
  EngineBrakeHpFrom: string;
  EngineHP: string;
  ManufacturerName: string;
  PlantCountry: string;
  PlantCity: string;
  VehicleType: string;
  Doors: string;
  Seats: string;
  BasePrice: string;
  // Safety features
  ABS: string;
  RearVisibilitySystem: string;
  BlindSpotMon: string;
  LaneDepartureWarning: string;
  LaneKeepSystem: string;
  ForwardCollisionWarning: string;
  CIB: string;
  PedestrianAutomaticEmergencyBraking: string;
  AdaptiveCruiseControl: string;
  ParkAssist: string;
  ErrorCode: string;
  ErrorText: string;
  AdditionalErrorText: string;
  SuggestedVIN: string;
  [key: string]: string; // Para otras propiedades
}

interface NHTSAResponse {
  Count: number;
  Message: string;
  SearchCriteria: string;
  Results: NHTSADecodedVehicle[];
}

/**
 * Decodifica un VIN usando la API gratuita de NHTSA
 *
 * @param vin - El VIN de 17 caracteres
 * @returns Datos decodificados del vehículo
 */
export async function decodeVIN(vin: string): Promise<VinDecodeResult> {
  // Validar formato básico del VIN
  const cleanVin = vin.trim().toUpperCase();

  if (cleanVin.length !== 17) {
    return {
      success: false,
      vin: cleanVin,
      errorCode: 'INVALID_LENGTH',
      errorMessage: `El VIN debe tener 17 caracteres. Ingresaste ${cleanVin.length}.`,
    };
  }

  // Caracteres no permitidos en VIN: I, O, Q
  if (/[IOQ]/.test(cleanVin)) {
    return {
      success: false,
      vin: cleanVin,
      errorCode: 'INVALID_CHARACTERS',
      errorMessage: 'El VIN contiene caracteres inválidos (I, O, Q no están permitidos).',
    };
  }

  try {
    const response = await axios.get<NHTSAResponse>(
      `${NHTSA_API_URL}/DecodeVinValues/${cleanVin}?format=json`,
      { timeout: 10000 }
    );

    const results = response.data.Results;
    if (!results || results.length === 0) {
      return {
        success: false,
        vin: cleanVin,
        errorCode: 'NO_RESULTS',
        errorMessage: 'No se encontraron datos para este VIN.',
      };
    }

    // DecodeVinValues devuelve un array con un solo objeto con todas las propiedades
    const data = results[0];

    // Error codes: 0 = No error, 1-4 = warnings (still usable), 5+ = fatal
    const errorCode = data.ErrorCode;
    if (errorCode && parseInt(errorCode) >= 5) {
      return {
        success: false,
        vin: cleanVin,
        errorCode: errorCode,
        errorMessage: data.ErrorText || 'Error al decodificar el VIN.',
        suggestedVIN: data.SuggestedVIN || undefined,
      };
    }

    // Extraer datos relevantes
    const result: VinDecodeResult = {
      success: true,
      vin: cleanVin,

      // Datos básicos - capitalizar Make correctamente
      make: capitalizeWords(data.Make),
      model: data.Model || undefined,
      year: data.ModelYear ? parseInt(data.ModelYear) : undefined,
      trim: data.Trim || data.Trim2 || undefined,

      // Especificaciones
      bodyClass: mapBodyClass(data.BodyClass),
      driveType: mapDriveType(data.DriveType),
      engineCylinders: data.EngineNumberofCylinders || undefined,
      engineSize: formatEngineSize(data.DisplacementL),
      engineModel: data.EngineModel || undefined,
      fuelType: mapFuelType(data.FuelTypePrimary),
      transmission: mapTransmission(data.TransmissionStyle),
      horsepower: data.EngineHP || data.EngineBrakeHpFrom || undefined,
      doors: data.Doors ? parseInt(data.Doors) : undefined,
      seats: data.Seats ? parseInt(data.Seats) : undefined,

      // Precio base (MSRP)
      basePrice: data.BasePrice ? parseInt(data.BasePrice) : undefined,

      // Features de seguridad
      safetyFeatures: extractSafetyFeatures(data),

      // Info adicional
      manufacturer: data.ManufacturerName || undefined,
      plantCountry: data.PlantCountry || undefined,
      plantCity: data.PlantCity || undefined,
      vehicleType: data.VehicleType || undefined,
    };

    // Verificar que al menos tenemos marca y modelo
    if (!result.make || !result.model) {
      return {
        ...result,
        success: false,
        errorCode: 'INCOMPLETE_DATA',
        errorMessage: 'No se pudo obtener la información completa del vehículo.',
      };
    }

    return result;
  } catch (error) {
    console.error('Error decoding VIN:', error);

    if (axios.isAxiosError(error)) {
      if (error.code === 'ECONNABORTED') {
        return {
          success: false,
          vin: cleanVin,
          errorCode: 'TIMEOUT',
          errorMessage: 'La consulta tardó demasiado. Intenta de nuevo.',
        };
      }
      if (!error.response) {
        return {
          success: false,
          vin: cleanVin,
          errorCode: 'NETWORK_ERROR',
          errorMessage: 'Error de conexión. Verifica tu internet.',
        };
      }
    }

    return {
      success: false,
      vin: cleanVin,
      errorCode: 'UNKNOWN',
      errorMessage: 'Error inesperado al decodificar el VIN.',
    };
  }
}

/**
 * Valida el formato de un VIN sin hacer llamada a la API
 */
export function validateVINFormat(vin: string): { valid: boolean; message?: string } {
  const cleanVin = vin.trim().toUpperCase();

  if (cleanVin.length === 0) {
    return { valid: false, message: 'Ingresa el VIN del vehículo' };
  }

  if (cleanVin.length !== 17) {
    return { valid: false, message: `El VIN debe tener 17 caracteres (tienes ${cleanVin.length})` };
  }

  if (/[IOQ]/.test(cleanVin)) {
    return { valid: false, message: 'El VIN no puede contener las letras I, O, Q' };
  }

  if (!/^[A-HJ-NPR-Z0-9]{17}$/.test(cleanVin)) {
    return {
      valid: false,
      message: 'El VIN solo puede contener letras (excepto I, O, Q) y números',
    };
  }

  return { valid: true };
}

// ============================================================
// MAPEO DE VALORES NHTSA A NUESTROS VALORES
// ============================================================

/**
 * Capitaliza cada palabra (TOYOTA -> Toyota, MERCEDES-BENZ -> Mercedes-Benz)
 */
function capitalizeWords(value: string | null): string | undefined {
  if (!value) return undefined;
  return value
    .toLowerCase()
    .split(/[\s-]+/)
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(value.includes('-') ? '-' : ' ');
}

function mapBodyClass(value: string | null): string | undefined {
  if (!value) return undefined;

  const mapping: Record<string, string> = {
    'Sedan/Saloon': 'Sedan',
    Sedan: 'Sedan',
    'Hatchback/Liftback/Notchback': 'Hatchback',
    Hatchback: 'Hatchback',
    Coupe: 'Coupe',
    'Convertible/Cabriolet': 'Convertible',
    Convertible: 'Convertible',
    'Sport Utility Vehicle (SUV)/Multi-Purpose Vehicle (MPV)': 'SUV',
    'Sport Utility Vehicle (SUV)': 'SUV',
    SUV: 'SUV',
    'Crossover Utility Vehicle (CUV)': 'Crossover',
    Pickup: 'Truck',
    Truck: 'Truck',
    Van: 'Van',
    Minivan: 'Minivan',
    'Cargo Van': 'Van',
    Wagon: 'Wagon',
    'Station Wagon': 'Wagon',
  };

  // Buscar coincidencia parcial
  for (const [key, mapped] of Object.entries(mapping)) {
    if (value.toLowerCase().includes(key.toLowerCase())) {
      return mapped;
    }
  }

  return value;
}

function mapDriveType(value: string | null): string | undefined {
  if (!value) return undefined;

  const v = value.toUpperCase();

  if (v.includes('4WD') || v.includes('4X4') || v.includes('FOUR WHEEL')) return '4WD';
  if (v.includes('AWD') || v.includes('ALL WHEEL') || v.includes('ALL-WHEEL')) return 'AWD';
  if (v.includes('RWD') || v.includes('REAR WHEEL') || v.includes('REAR-WHEEL')) return 'RWD';
  if (v.includes('FWD') || v.includes('FRONT WHEEL') || v.includes('FRONT-WHEEL')) return 'FWD';

  return value;
}

function mapFuelType(value: string | null): string | undefined {
  if (!value) return undefined;

  const v = value.toLowerCase();

  if (v.includes('gasoline') || v.includes('gas')) return 'Gasoline';
  if (v.includes('diesel')) return 'Diesel';
  if (v.includes('electric') && v.includes('plug')) return 'Plug-in Hybrid';
  if (v.includes('electric')) return 'Electric';
  if (v.includes('hybrid')) return 'Hybrid';
  if (v.includes('flex') || v.includes('e85')) return 'Flex Fuel';

  return value;
}

function mapTransmission(value: string | null): string | undefined {
  if (!value) return undefined;

  const v = value.toLowerCase();

  if (v.includes('automatic')) return 'Automatic';
  if (v.includes('manual')) return 'Manual';
  if (v.includes('cvt') || v.includes('continuously variable')) return 'CVT';
  if (v.includes('dual') || v.includes('dct') || v.includes('dsg')) return 'Dual-Clutch';
  if (v.includes('automated')) return 'Semi-Automatic';

  return value;
}

function formatEngineSize(liters: string | null): string | undefined {
  if (!liters) return undefined;

  const num = parseFloat(liters);
  if (isNaN(num)) return liters;

  return `${num.toFixed(1)}L`;
}

/**
 * Extrae features de seguridad de los datos NHTSA
 * Mapea a los nombres usados en FeaturesStep.tsx
 */
function extractSafetyFeatures(data: Record<string, string>): string[] {
  const features: string[] = [];

  // Mapeo de campos NHTSA a nombres de features en el formulario
  const featureMap: Record<string, string> = {
    RearVisibilitySystem: 'Backup Camera',
    BlindSpotMon: 'Blind Spot Monitor',
    LaneDepartureWarning: 'Lane Departure Warning',
    LaneKeepSystem: 'Lane Keep Assist',
    ForwardCollisionWarning: 'Automatic Emergency Braking',
    CIB: 'Automatic Emergency Braking',
    PedestrianAutomaticEmergencyBraking: 'Automatic Emergency Braking',
    AdaptiveCruiseControl: 'Adaptive Cruise Control',
    ParkAssist: 'Front/Rear Parking Sensors',
  };

  for (const [nhtsaField, featureName] of Object.entries(featureMap)) {
    const value = data[nhtsaField];
    if (value && value.toLowerCase() === 'standard' && !features.includes(featureName)) {
      features.push(featureName);
    }
  }

  return features;
}

// ============================================================
// UTILIDADES ADICIONALES
// ============================================================

/**
 * Extrae el año del VIN (posición 10)
 * Útil para validación rápida sin API
 */
export function getYearFromVIN(vin: string): number | null {
  if (vin.length < 10) return null;

  const yearChar = vin.charAt(9).toUpperCase();
  const currentYear = new Date().getFullYear();

  // Mapa de caracteres a años
  // Nota: Los caracteres se repiten cada 30 años
  const yearMap: Record<string, number[]> = {
    A: [2010, 1980],
    B: [2011, 1981],
    C: [2012, 1982],
    D: [2013, 1983],
    E: [2014, 1984],
    F: [2015, 1985],
    G: [2016, 1986],
    H: [2017, 1987],
    J: [2018, 1988],
    K: [2019, 1989],
    L: [2020, 1990],
    M: [2021, 1991],
    N: [2022, 1992],
    P: [2023, 1993],
    R: [2024, 1994],
    S: [2025, 1995],
    T: [2026, 1996],
    V: [2027, 1997],
    W: [2028, 1998],
    X: [2029, 1999],
    Y: [2030, 2000],
    '1': [2001, 2031],
    '2': [2002, 2032],
    '3': [2003, 2033],
    '4': [2004, 2034],
    '5': [2005, 2035],
    '6': [2006, 2036],
    '7': [2007, 2037],
    '8': [2008, 2038],
    '9': [2009, 2039],
  };

  const years = yearMap[yearChar];
  if (!years) return null;

  // Devolver el año más cercano al actual
  return years.find((y) => y <= currentYear + 1) || years[0];
}

/**
 * Información sobre la API y límites
 */
export const VIN_DECODER_INFO = {
  provider: 'NHTSA vPIC API',
  website: 'https://vpic.nhtsa.dot.gov/',
  cost: 'FREE',
  rateLimit: 'No documented limit, but be reasonable',
  dataProvided: [
    'Make (Marca)',
    'Model (Modelo)',
    'Year (Año)',
    'Trim (Versión)',
    'Body Class (Tipo de carrocería)',
    'Drive Type (Tracción)',
    'Engine (Motor)',
    'Fuel Type (Combustible)',
    'Transmission (Transmisión)',
    'Manufacturer (Fabricante)',
    'Plant Country (País de fabricación)',
  ],
  notProvided: [
    'Historial de accidentes',
    'Historial de propietarios',
    'Historial de mantenimiento',
    'Reporte de título',
    'Kilometraje/Millaje',
    'Color',
  ],
};
