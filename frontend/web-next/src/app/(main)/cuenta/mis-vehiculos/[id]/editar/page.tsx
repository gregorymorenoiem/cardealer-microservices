'use client';

import { useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';

/**
 * /cuenta/mis-vehiculos/[id]/editar → redirects to /vender/editar/[id]
 * Vehicle editing lives at /vender/editar/[id].
 * This redirect prevents 404s from links in smart-publish-wizard and cuenta/page.tsx.
 */
export default function MisVehiculosEditarPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();

  useEffect(() => {
    if (params?.id) {
      router.replace(`/vender/editar/${params.id}`);
    }
  }, [router, params?.id]);

  return null;
}
