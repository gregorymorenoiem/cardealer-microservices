/**
 * SuccessScreen — Post-publication congratulations
 *
 * Shown after the seller completes all 3 wizard steps:
 * 1. Account registered
 * 2. Seller profile created
 * 3. First vehicle published
 *
 * Includes link to the new listing, dashboard, and next steps.
 */

'use client';

import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import {
  CheckCircle,
  ExternalLink,
  LayoutDashboard,
  PlusCircle,
  Share2,
  Copy,
  PartyPopper,
} from 'lucide-react';
import { toast } from 'sonner';

// =============================================================================
// TYPES
// =============================================================================

interface SuccessScreenProps {
  vehicleId: string;
  vehicleSlug: string;
  vehicleTitle: string;
  sellerDisplayName: string;
}

// =============================================================================
// COMPONENT
// =============================================================================

export function SuccessScreen({
  vehicleId,
  vehicleSlug,
  vehicleTitle,
  sellerDisplayName,
}: SuccessScreenProps) {
  const vehicleUrl = `/vehiculos/${vehicleSlug}`;
  const fullUrl = typeof window !== 'undefined' ? `${window.location.origin}${vehicleUrl}` : '';

  const handleCopyLink = async () => {
    try {
      await navigator.clipboard.writeText(fullUrl);
      toast.success('¡Enlace copiado!');
    } catch {
      toast.error('No se pudo copiar el enlace');
    }
  };

  const handleShare = async () => {
    if (typeof navigator.share === 'function') {
      try {
        await navigator.share({
          title: vehicleTitle,
          text: `Mira este vehículo en OKLA: ${vehicleTitle}`,
          url: fullUrl,
        });
      } catch {
        // User cancelled or share failed — fallback to copy
        handleCopyLink();
      }
    } else {
      handleCopyLink();
    }
  };

  return (
    <div className="mx-auto max-w-2xl px-4 py-12 text-center">
      {/* Confetti Icon */}
      <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-[#00A870]/10">
        <PartyPopper className="h-10 w-10 text-[#00A870]" />
      </div>

      {/* Title */}
      <h1 className="mb-2 text-3xl font-bold text-gray-900">¡Felicidades, {sellerDisplayName}!</h1>
      <p className="mb-8 text-lg text-gray-600">
        Tu vehículo ha sido publicado exitosamente. Los compradores ya pueden verlo.
      </p>

      {/* Published Vehicle Card */}
      <Card className="mb-8 border-[#00A870]/20 bg-[#00A870]/5">
        <CardContent className="p-6">
          <div className="flex items-center justify-center gap-2 text-[#00A870]">
            <CheckCircle className="h-5 w-5" />
            <span className="font-semibold">Publicación activa</span>
          </div>
          <h2 className="mt-2 text-xl font-bold text-gray-900">{vehicleTitle}</h2>
          <p className="mt-1 text-sm text-gray-500">ID: {vehicleId}</p>

          {/* Action Buttons */}
          <div className="mt-4 flex flex-col items-center justify-center gap-3 sm:flex-row">
            <Button asChild variant="outline" size="sm">
              <Link href={vehicleUrl}>
                <ExternalLink className="mr-2 h-4 w-4" />
                Ver publicación
              </Link>
            </Button>
            <Button variant="outline" size="sm" onClick={handleShare}>
              <Share2 className="mr-2 h-4 w-4" />
              Compartir
            </Button>
            <Button variant="outline" size="sm" onClick={handleCopyLink}>
              <Copy className="mr-2 h-4 w-4" />
              Copiar enlace
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Next Steps */}
      <div className="mb-8 rounded-lg border border-gray-200 bg-white p-6 text-left">
        <h3 className="mb-4 text-lg font-semibold text-gray-900">¿Qué sigue?</h3>
        <ul className="space-y-3">
          <li className="flex items-start gap-3">
            <div className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-blue-100 text-xs font-bold text-blue-600">
              1
            </div>
            <div>
              <p className="font-medium text-gray-900">Verifica tu correo electrónico</p>
              <p className="text-sm text-gray-500">
                Revisa tu bandeja de entrada y confirma tu cuenta para mantener tu publicación
                activa.
              </p>
            </div>
          </li>
          <li className="flex items-start gap-3">
            <div className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-blue-100 text-xs font-bold text-blue-600">
              2
            </div>
            <div>
              <p className="font-medium text-gray-900">Verifica tu identidad</p>
              <p className="text-sm text-gray-500">
                Completa la verificación KYC para que los compradores confíen más en tu anuncio.
              </p>
            </div>
          </li>
          <li className="flex items-start gap-3">
            <div className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-blue-100 text-xs font-bold text-blue-600">
              3
            </div>
            <div>
              <p className="font-medium text-gray-900">Responde mensajes rápidamente</p>
              <p className="text-sm text-gray-500">
                Los vendedores que responden en menos de 1 hora venden hasta 3x más rápido.
              </p>
            </div>
          </li>
        </ul>
      </div>

      {/* Navigation Buttons */}
      <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
        <Button asChild size="lg" className="bg-[#00A870] hover:bg-[#009663]">
          <Link href="/vender/dashboard">
            <LayoutDashboard className="mr-2 h-5 w-5" />
            Ir a mi panel
          </Link>
        </Button>
        <Button asChild variant="outline" size="lg">
          <Link href="/publicar">
            <PlusCircle className="mr-2 h-5 w-5" />
            Publicar otro vehículo
          </Link>
        </Button>
      </div>
    </div>
  );
}
