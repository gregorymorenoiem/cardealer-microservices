/**
 * Valida un email
 * @param email - Email a validar
 * @returns true si es válido
 */
export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Valida una contraseña
 * @param password - Contraseña a validar
 * @param minLength - Longitud mínima (default: 8)
 * @returns Objeto con resultado y mensaje de error
 */
export function isValidPassword(
  password: string,
  minLength: number = 8
): { isValid: boolean; error?: string } {
  if (password.length < minLength) {
    return {
      isValid: false,
      error: `Password must be at least ${minLength} characters`,
    };
  }

  if (!/[a-z]/.test(password)) {
    return {
      isValid: false,
      error: 'Password must contain at least one lowercase letter',
    };
  }

  if (!/[A-Z]/.test(password)) {
    return {
      isValid: false,
      error: 'Password must contain at least one uppercase letter',
    };
  }

  if (!/\d/.test(password)) {
    return {
      isValid: false,
      error: 'Password must contain at least one number',
    };
  }

  return { isValid: true };
}

/**
 * Valida un número de teléfono US
 * @param phone - Número de teléfono
 * @returns true si es válido
 */
export function isValidPhoneNumber(phone: string): boolean {
  const cleaned = phone.replace(/\D/g, '');
  return cleaned.length === 10 || (cleaned.length === 11 && cleaned.startsWith('1'));
}

/**
 * Valida un ZIP code US
 * @param zipCode - ZIP code a validar
 * @returns true si es válido
 */
export function isValidZipCode(zipCode: string): boolean {
  const zipRegex = /^\d{5}(-\d{4})?$/;
  return zipRegex.test(zipCode);
}

/**
 * Valida una URL
 * @param url - URL a validar
 * @returns true si es válida
 */
export function isValidUrl(url: string): boolean {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
}

/**
 * Valida que un string no esté vacío o solo contenga espacios
 * @param value - String a validar
 * @returns true si no está vacío
 */
export function isNotEmpty(value: string): boolean {
  return value.trim().length > 0;
}

/**
 * Valida un rango numérico
 * @param value - Número a validar
 * @param min - Valor mínimo (inclusive)
 * @param max - Valor máximo (inclusive)
 * @returns true si está en el rango
 */
export function isInRange(value: number, min: number, max: number): boolean {
  return value >= min && value <= max;
}

/**
 * Valida que un año sea válido para un vehículo
 * @param year - Año a validar
 * @returns true si es válido
 */
export function isValidVehicleYear(year: number): boolean {
  const currentYear = new Date().getFullYear();
  return year >= 1900 && year <= currentYear + 1;
}

/**
 * Valida un VIN (Vehicle Identification Number)
 * @param vin - VIN a validar
 * @returns true si es válido
 */
export function isValidVIN(vin: string): boolean {
  // VIN debe tener 17 caracteres alfanuméricos
  const vinRegex = /^[A-HJ-NPR-Z0-9]{17}$/i;
  return vinRegex.test(vin);
}
