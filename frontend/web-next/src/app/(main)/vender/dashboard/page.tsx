'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

/**
 * /vender/dashboard → redirects to /cuenta
 * El dashboard del vendedor ahora vive en /cuenta (role-aware).
 * Las sub-páginas (/vender/publicar, /vender/leads, etc.) siguen intactas.
 */
export default function VenderDashboardPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace('/cuenta');
  }, [router]);

  return null;
}
