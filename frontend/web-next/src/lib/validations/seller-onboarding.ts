/**
 * Seller Onboarding Validation Schemas
 *
 * Zod schemas for the 3-step seller registration wizard:
 * Step 1: Account registration
 * Step 2: Seller profile
 * Step 3: First vehicle publication
 */

import { z } from 'zod';

// =============================================================================
// STEP 1: Account Registration Schema
// =============================================================================

export const accountSchema = z
  .object({
    firstName: z
      .string()
      .min(2, 'El nombre debe tener al menos 2 caracteres')
      .max(50, 'El nombre no puede tener más de 50 caracteres')
      .regex(/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s'-]+$/, 'El nombre contiene caracteres inválidos'),
    lastName: z
      .string()
      .min(2, 'El apellido debe tener al menos 2 caracteres')
      .max(50, 'El apellido no puede tener más de 50 caracteres')
      .regex(/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s'-]+$/, 'El apellido contiene caracteres inválidos'),
    email: z.string().email('Ingresa un email válido').max(254, 'Email muy largo'),
    phone: z
      .string()
      .regex(/^\d{10}$/, 'Ingresa un número de 10 dígitos (ej: 8091234567)')
      .optional()
      .or(z.literal('')),
    password: z
      .string()
      .min(8, 'Mínimo 8 caracteres')
      .max(128, 'Máximo 128 caracteres')
      .regex(/[A-Z]/, 'Debe contener al menos una mayúscula')
      .regex(/[a-z]/, 'Debe contener al menos una minúscula')
      .regex(/\d/, 'Debe contener al menos un número')
      .regex(/[^a-zA-Z0-9]/, 'Debe contener al menos un carácter especial'),
    confirmPassword: z.string(),
    accountType: z.enum(['individual', 'dealer'], {
      message: 'Selecciona el tipo de cuenta',
    }),
    acceptTerms: z.boolean().refine(v => v === true, {
      message: 'Debes aceptar los términos y condiciones',
    }),
  })
  .refine(data => data.password === data.confirmPassword, {
    message: 'Las contraseñas no coinciden',
    path: ['confirmPassword'],
  });

export type AccountFormData = z.infer<typeof accountSchema>;

// =============================================================================
// STEP 2: Seller Profile Schema
// =============================================================================

/** RNC dominicano: 9 dígitos (personas físicas) u 11 dígitos (personas jurídicas) */
const rncRegex = /^(\d{9}|\d{11})$/;

export const sellerProfileSchema = z.object({
  displayName: z
    .string()
    .min(3, 'El nombre público debe tener al menos 3 caracteres')
    .max(100, 'El nombre público no puede exceder 100 caracteres'),
  businessName: z
    .string()
    .min(3, 'El nombre del negocio debe tener al menos 3 caracteres')
    .max(150, 'El nombre del negocio no puede exceder 150 caracteres')
    .optional()
    .or(z.literal('')),
  rnc: z.string().regex(rncRegex, 'El RNC debe tener 9 o 11 dígitos').optional().or(z.literal('')),
  description: z
    .string()
    .max(500, 'La descripción no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  phone: z
    .string()
    .regex(/^\d{10}$/, 'Ingresa un número de 10 dígitos')
    .optional()
    .or(z.literal('')),
  location: z
    .string()
    .max(200, 'La ubicación no puede exceder 200 caracteres')
    .optional()
    .or(z.literal('')),
  specialties: z.array(z.string()).optional().default([]),
});

export type SellerProfileFormData = z.infer<typeof sellerProfileSchema>;

/**
 * When accountType === 'dealer', RNC is required.
 * Apply this refinement in the component based on context.
 */
export const sellerProfileDealerSchema = sellerProfileSchema.extend({
  rnc: z.string().regex(rncRegex, 'El RNC es obligatorio para dealers (9 o 11 dígitos)'),
  businessName: z.string().min(3, 'El nombre del negocio es obligatorio para dealers'),
});

export type SellerProfileDealerFormData = z.infer<typeof sellerProfileDealerSchema>;

// =============================================================================
// STEP 3: Vehicle Publication Schema
// =============================================================================

const currentYear = new Date().getFullYear();

export const vehicleSchema = z.object({
  make: z.string().min(1, 'Selecciona una marca'),
  model: z.string().min(1, 'Selecciona un modelo'),
  year: z
    .number({ message: 'Selecciona el año' })
    .int()
    .min(1900, 'Año mínimo: 1900')
    .max(currentYear + 2, `Año máximo: ${currentYear + 2}`),
  trim: z.string().max(50, 'Máximo 50 caracteres').optional().or(z.literal('')),
  mileage: z
    .number({ message: 'Ingresa el kilometraje' })
    .int()
    .min(0, 'El kilometraje no puede ser negativo')
    .max(2_000_000, 'Kilometraje máximo: 2,000,000 km'),
  vin: z
    .string()
    .length(17, 'El VIN debe tener exactamente 17 caracteres')
    .regex(/^[A-HJ-NPR-Z0-9]{17}$/, 'VIN inválido (no usar I, O, Q)')
    .optional()
    .or(z.literal('')),
  transmission: z.string().min(1, 'Selecciona la transmisión'),
  fuelType: z.string().min(1, 'Selecciona el tipo de combustible'),
  bodyType: z.string().min(1, 'Selecciona el tipo de carrocería'),
  exteriorColor: z.string().optional().or(z.literal('')),
  interiorColor: z.string().optional().or(z.literal('')),
  condition: z.enum(['new', 'used', 'certified'], {
    message: 'Selecciona la condición',
  }),
  price: z
    .number({ message: 'Ingresa el precio' })
    .min(1, 'El precio debe ser mayor a 0')
    .max(100_000_000, 'Precio máximo: 100,000,000'),
  currency: z.enum(['DOP', 'USD']).default('DOP'),
  description: z
    .string()
    .min(20, 'La descripción debe tener al menos 20 caracteres')
    .max(5000, 'La descripción no puede exceder 5000 caracteres'),
  features: z.array(z.string()).default([]),
  city: z.string().min(1, 'Selecciona la ciudad'),
  province: z.string().min(1, 'Selecciona la provincia'),
  isNegotiable: z.boolean().default(false),
  sellerPhone: z
    .string()
    .regex(/^\d{10}$/, 'Ingresa un número de 10 dígitos')
    .optional()
    .or(z.literal('')),
  sellerEmail: z.string().email('Ingresa un email válido').optional().or(z.literal('')),
});

export type VehicleFormData = z.infer<typeof vehicleSchema>;

// =============================================================================
// IMAGE VALIDATION
// =============================================================================

export const IMAGE_CONSTRAINTS = {
  minImages: 3,
  maxImages: 10,
  maxSizeMB: 5,
  maxSizeBytes: 5 * 1024 * 1024,
  allowedTypes: ['image/jpeg', 'image/png', 'image/webp'] as const,
  maxWidth: 1200,
} as const;

export function validateVehicleImage(file: File): { valid: boolean; error?: string } {
  if (
    !IMAGE_CONSTRAINTS.allowedTypes.includes(
      file.type as (typeof IMAGE_CONSTRAINTS.allowedTypes)[number]
    )
  ) {
    return { valid: false, error: 'Solo se permiten imágenes JPG, PNG o WebP' };
  }
  if (file.size > IMAGE_CONSTRAINTS.maxSizeBytes) {
    return { valid: false, error: `La imagen no puede exceder ${IMAGE_CONSTRAINTS.maxSizeMB}MB` };
  }
  return { valid: true };
}

// =============================================================================
// SPECIALTIES OPTIONS (for seller profile)
// =============================================================================

export const SELLER_SPECIALTIES = [
  'Vehículos nuevos',
  'Vehículos usados',
  'Yipetas / SUV',
  'Sedanes',
  'Pickups / Camionetas',
  'Vehículos de lujo',
  'Vehículos eléctricos',
  'Financiamiento',
  'Importación directa',
  'Vehículos comerciales',
] as const;

// =============================================================================
// PROVINCES (Dominican Republic)
// =============================================================================

export const RD_PROVINCES = [
  'Distrito Nacional',
  'Santo Domingo',
  'Santiago',
  'San Cristóbal',
  'La Vega',
  'Puerto Plata',
  'San Pedro de Macorís',
  'Duarte',
  'La Romana',
  'La Altagracia',
  'San Juan',
  'Azua',
  'Monseñor Nouel',
  'Peravia',
  'Espaillat',
  'Barahona',
  'Valverde',
  'Monte Plata',
  'María Trinidad Sánchez',
  'Sánchez Ramírez',
  'Samaná',
  'Hato Mayor',
  'Monte Cristi',
  'Hermanas Mirabal',
  'El Seibo',
  'Elías Piña',
  'Dajabón',
  'Santiago Rodríguez',
  'Baoruco',
  'Independencia',
  'Pedernales',
  'San José de Ocoa',
] as const;
