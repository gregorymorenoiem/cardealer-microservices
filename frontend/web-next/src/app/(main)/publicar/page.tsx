/**
 * Publicar Vehículo Page
 *
 * Smart Vehicle Publishing Wizard with VIN decode support
 * Upgraded to SmartPublishWizard - February 2026
 */

'use client';

import { SmartPublishWizard } from '@/components/vehicles/smart-publish';

export default function PublicarPage() {
  return (
    <div className="min-h-screen bg-muted/50">
      <div className="mx-auto max-w-4xl px-4 py-8">
        {/* Header */}
        <div className="mb-8 text-center">
          <h1 className="mb-2 text-3xl font-bold text-foreground">
            Publicar Vehículo
          </h1>
          <p className="text-muted-foreground">
            Publica tu vehículo de forma rápida con detección automática por VIN
          </p>
        </div>

        {/* Smart Publish Wizard */}
        <SmartPublishWizard mode="individual" />
      </div>
    </div>
  );
}
