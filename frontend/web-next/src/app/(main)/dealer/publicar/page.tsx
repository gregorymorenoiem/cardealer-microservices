/**
 * Dealer Vehicle Publishing Page
 *
 * Smart Vehicle Publishing Wizard with VIN decode, CSV import, and batch tools
 * Upgraded to SmartPublishWizard - February 2026
 */

'use client';

import { SmartPublishWizard } from '@/components/vehicles/smart-publish';
import { useCurrentDealer } from '@/hooks/use-dealers';
import { Loader2 } from 'lucide-react';

export default function DealerPublicarPage() {
  const { data: dealer, isLoading } = useCurrentDealer();

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-muted/50">
        <div className="flex flex-col items-center gap-3">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">Cargando perfil de dealer...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-muted/50">
      <div className="mx-auto max-w-4xl px-4 py-8">
        {/* Header */}
        <div className="mb-8 text-center">
          <h1 className="mb-2 text-3xl font-bold text-foreground">
            Publicar Vehículo
          </h1>
          <p className="text-muted-foreground">
            {dealer?.businessName
              ? `Publicando como ${dealer.businessName}`
              : 'Publica vehículos con VIN, escaneo o importación CSV'}
          </p>
        </div>

        {/* Smart Publish Wizard - Dealer Mode */}
        <SmartPublishWizard
          mode="dealer"
          dealerId={dealer?.id}
        />
      </div>
    </div>
  );
}
