/**
 * Messages Redirect
 *
 * Redirects from old account sidebar route to new full-screen messaging layout
 * Old: /cuenta/mensajes → New: /mensajes
 */

'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2 } from 'lucide-react';

export default function MessagesRedirect() {
  const router = useRouter();

  useEffect(() => {
    // Redirect to new full-screen messaging layout
    router.replace('/mensajes');
  }, [router]);

  return (
    <div className="flex min-h-[400px] items-center justify-center">
      <div className="text-center">
        <Loader2 className="mx-auto h-8 w-8 animate-spin text-blue-600" />
        <p className="mt-4 text-muted-foreground">Redirigiendo a mensajes...</p>
      </div>
    </div>
  );
}
