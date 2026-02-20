import { redirect } from 'next/navigation';

/**
 * Legacy route redirect — /dashboard → /cuenta/perfil
 * The user dashboard sidebar is preserved in layout.tsx; the mock
 * stats page is removed.  /cuenta/* is the canonical account area.
 */
export default function DashboardRedirect() {
  redirect('/cuenta/perfil');
}
