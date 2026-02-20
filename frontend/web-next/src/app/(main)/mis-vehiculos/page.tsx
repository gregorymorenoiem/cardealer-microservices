import { redirect } from 'next/navigation';

/**
 * Legacy route redirect — /mis-vehiculos → /cuenta/mis-vehiculos
 * The canonical vehicle management page is under /cuenta/.
 */
export default function MisVehiculosRedirect() {
  redirect('/cuenta/mis-vehiculos');
}
