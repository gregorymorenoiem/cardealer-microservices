import { redirect } from 'next/navigation';

/**
 * Admin Advertising Redirect Page
 *
 * Legacy route redirect: /admin/advertising → /admin/promociones
 * The canonical admin advertising/promotions page is at the promociones route.
 *
 * Keeping this for backward compatibility with bookmarks and email links.
 */
export default function AdminAdvertisingPage() {
  redirect('/admin/promociones');
}
