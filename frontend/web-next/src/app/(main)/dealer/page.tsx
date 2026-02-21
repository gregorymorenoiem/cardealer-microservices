'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

/**
 * /dealer root → redirects to /cuenta
 * El dashboard de dealer ahora vive en /cuenta (role-aware).
 * Las sub-páginas del portal (/dealer/inventario, /dealer/leads, etc.) siguen intactas.
 */
export default function DealerRootPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace('/cuenta');
  }, [router]);

  return null;
}
