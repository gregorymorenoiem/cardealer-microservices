/**
 * Formatea un precio en dólares americanos
 * @param price - Precio a formatear
 * @returns String formateado (e.g., "$25,999")
 */
export function formatPrice(price: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(price);
}

/**
 * Formatea una fecha en formato legible
 * @param date - Fecha a formatear (Date, string, o número)
 * @param format - Formato de salida ('short' | 'medium' | 'long')
 * @returns String formateado (e.g., "Dec 4, 2025")
 */
export function formatDate(
  date: Date | string | number,
  format: 'short' | 'medium' | 'long' = 'medium'
): string {
  const dateObj = typeof date === 'string' || typeof date === 'number' 
    ? new Date(date) 
    : date;

  const optionsMap: Record<'short' | 'medium' | 'long', Intl.DateTimeFormatOptions> = {
    short: { month: 'numeric', day: 'numeric', year: '2-digit' },
    medium: { month: 'short', day: 'numeric', year: 'numeric' },
    long: { month: 'long', day: 'numeric', year: 'numeric' },
  };

  return new Intl.DateTimeFormat('en-US', optionsMap[format]).format(dateObj);
}

/**
 * Formatea un número de millas con separadores
 * @param mileage - Millas a formatear
 * @returns String formateado (e.g., "45,230 mi")
 */
export function formatMileage(mileage: number): string {
  return `${new Intl.NumberFormat('en-US').format(mileage)} mi`;
}

/**
 * Formatea un número de teléfono al formato US
 * @param phone - Número de teléfono (puede incluir caracteres especiales)
 * @returns String formateado (e.g., "(555) 123-4567")
 */
export function formatPhoneNumber(phone: string): string {
  const cleaned = phone.replace(/\D/g, '');
  
  if (cleaned.length === 10) {
    return `(${cleaned.slice(0, 3)}) ${cleaned.slice(3, 6)}-${cleaned.slice(6)}`;
  }
  
  if (cleaned.length === 11 && cleaned.startsWith('1')) {
    return `+1 (${cleaned.slice(1, 4)}) ${cleaned.slice(4, 7)}-${cleaned.slice(7)}`;
  }
  
  return phone;
}

/**
 * Trunca un texto a un número máximo de caracteres
 * @param text - Texto a truncar
 * @param maxLength - Longitud máxima
 * @param suffix - Sufijo a agregar (default: "...")
 * @returns String truncado
 */
export function truncateText(text: string, maxLength: number, suffix: string = '...'): string {
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength - suffix.length) + suffix;
}

/**
 * Convierte un string a formato Title Case
 * @param str - String a convertir
 * @returns String en Title Case
 */
export function toTitleCase(str: string): string {
  return str
    .toLowerCase()
    .split(' ')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

/**
 * Formatea un número como porcentaje
 * @param value - Valor a formatear (0-1 o 0-100)
 * @param decimals - Número de decimales (default: 0)
 * @returns String formateado (e.g., "75%")
 */
export function formatPercentage(value: number, decimals: number = 0): string {
  const percentage = value > 1 ? value : value * 100;
  return `${percentage.toFixed(decimals)}%`;
}
