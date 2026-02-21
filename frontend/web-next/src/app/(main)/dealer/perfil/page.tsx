import { redirect } from 'next/navigation';

/**
 * Dealer Profile Redirect Page
 *
 * Legacy route redirect: /dealer/perfil → /cuenta/perfil
 * The canonical unified profile page that handles all user types (buyer, seller, dealer) is at /cuenta/perfil.
 *
 * Keeping this for backward compatibility with bookmarks and email links.
 */
export default function DealerPerfilPage() {
  redirect('/cuenta/perfil');
}
