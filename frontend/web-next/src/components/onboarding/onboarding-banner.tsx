/**
 * OnboardingBanner — Welcome banner for first-time dealers
 *
 * Shows when URL has ?onboarding=true (post-registration redirect).
 * Displays a 3-step visual: ✅ Cuenta creada → 📸 Publica tu primer vehículo → 🎉 ¡Listo!
 * Encourages the dealer to complete their first listing in <5 minutes.
 */

'use client';

import { useSearchParams } from 'next/navigation';
import { useState } from 'react';
import { CheckCircle, Camera, PartyPopper, X, Sparkles, Clock } from 'lucide-react';

export function OnboardingBanner() {
  const searchParams = useSearchParams();
  const isOnboarding = searchParams.get('onboarding') === 'true';
  const [dismissed, setDismissed] = useState(false);

  if (!isOnboarding || dismissed) return null;

  return (
    <div className="border-primary/20 bg-primary/5 relative mb-6 overflow-hidden rounded-xl border p-6">
      {/* Dismiss button */}
      <button
        onClick={() => setDismissed(true)}
        className="text-muted-foreground hover:text-foreground absolute top-3 right-3"
        aria-label="Cerrar"
      >
        <X className="h-4 w-4" />
      </button>

      {/* Content */}
      <div className="flex items-start gap-4">
        <div className="bg-primary/10 flex h-12 w-12 shrink-0 items-center justify-center rounded-full">
          <Sparkles className="text-primary h-6 w-6" />
        </div>
        <div className="flex-1">
          <h3 className="text-lg font-bold">¡Bienvenido a OKLA! 🎉</h3>
          <p className="text-muted-foreground mb-4 text-sm">
            Tu cuenta de dealer fue creada exitosamente. Ahora publica tu primer vehículo — toma
            solo unos minutos.
          </p>

          {/* 3-step progress */}
          <div className="flex items-center gap-3">
            {/* Step 1 — Done */}
            <div className="flex items-center gap-2 rounded-lg bg-green-50 px-3 py-2">
              <CheckCircle className="h-5 w-5 text-green-600" />
              <span className="text-sm font-medium text-green-700">Cuenta creada</span>
            </div>

            <div className="text-muted-foreground">→</div>

            {/* Step 2 — Current */}
            <div className="border-primary bg-primary/10 flex items-center gap-2 rounded-lg border px-3 py-2">
              <Camera className="text-primary h-5 w-5" />
              <span className="text-primary text-sm font-medium">Publica tu primer vehículo</span>
            </div>

            <div className="text-muted-foreground">→</div>

            {/* Step 3 — Pending */}
            <div className="bg-muted flex items-center gap-2 rounded-lg px-3 py-2 opacity-50">
              <PartyPopper className="text-muted-foreground h-5 w-5" />
              <span className="text-muted-foreground text-sm font-medium">¡Listo para vender!</span>
            </div>
          </div>

          <div className="text-muted-foreground mt-3 flex items-center gap-1 text-xs">
            <Clock className="h-3 w-3" />
            Tiempo estimado: ~5 minutos
          </div>
        </div>
      </div>
    </div>
  );
}
