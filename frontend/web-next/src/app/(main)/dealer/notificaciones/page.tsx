'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

/**
 * /dealer/notificaciones → redirects to /cuenta/notificaciones
 * Notifications live under /cuenta/notificaciones (role-aware page).
 * The dealer layout header links here, so this redirect prevents 404s.
 */
export default function DealerNotificacionesPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace('/cuenta/notificaciones');
  }, [router]);

  return null;
}
