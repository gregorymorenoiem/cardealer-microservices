import { redirect } from 'next/navigation';

/**
 * Dealer Payment History Redirect Page
 *
 * Legacy route redirect: /dealer/historial-pagos → /dealer/facturacion/historial
 * The canonical billing history page is at the facturacion route.
 *
 * Keeping this for backward compatibility with bookmarks and email links.
 */
export default function DealerHistorialPagosPage() {
  redirect('/dealer/facturacion/historial');
}
