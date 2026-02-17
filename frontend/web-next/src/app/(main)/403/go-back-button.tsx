/**
 * Go Back Button - Tiny client island for the 403 page
 *
 * Extracted to keep the 403 page as a server component for
 * better performance (no JS bundle needed for static error pages).
 */

'use client';

import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { ArrowLeft } from 'lucide-react';

export function GoBackButton() {
  const router = useRouter();

  return (
    <Button variant="outline" onClick={() => router.back()}>
      <ArrowLeft className="mr-2 h-4 w-4" />
      Volver
    </Button>
  );
}
