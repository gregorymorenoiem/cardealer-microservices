import { redirect } from 'next/navigation';

/**
 * Dealer Import Redirect Page
 *
 * Legacy route redirect: /dealer/importar → /dealer/inventario/importar
 * The canonical import page with full API integration is at the inventario route.
 *
 * Keeping this for backward compatibility with bookmarks and email links.
 */
export default function DealerImportPage() {
  redirect('/dealer/inventario/importar');
}
