/**
 * Mis Campañas — Campaign management for sellers
 */

'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  useCampaignsByOwner,
  usePauseCampaign,
  useResumeCampaign,
  useCancelCampaign,
} from '@/hooks/use-advertising';
import useAuthStore from '@/hooks/use-auth';
import type { CampaignStatus, AdCampaignSummary } from '@/types/advertising';

const statusLabels: Record<
  CampaignStatus,
  { label: string; variant: 'default' | 'secondary' | 'danger' | 'outline' }
> = {
  PendingPayment: { label: 'Pago Pendiente', variant: 'outline' },
  Active: { label: 'Activa', variant: 'default' },
  Paused: { label: 'Pausada', variant: 'secondary' },
  Cancelled: { label: 'Cancelada', variant: 'danger' },
  Completed: { label: 'Completada', variant: 'secondary' },
  Expired: { label: 'Expirada', variant: 'outline' },
};

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(amount);
}

function CampaignCard({ campaign }: { campaign: AdCampaignSummary }) {
  const pauseMutation = usePauseCampaign();
  const resumeMutation = useResumeCampaign();
  const cancelMutation = useCancelCampaign();

  const statusInfo = statusLabels[campaign.status];
  const budgetPercent =
    campaign.totalBudget > 0
      ? Math.round((campaign.remainingBudget / campaign.totalBudget) * 100)
      : 0;

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">
            {campaign.placementType === 'FeaturedSpot' ? '⭐' : '💎'}{' '}
            {campaign.placementType === 'FeaturedSpot' ? 'Destacado' : 'Premium'}
          </CardTitle>
          <Badge variant={statusInfo.variant}>{statusInfo.label}</Badge>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-muted-foreground">Vistas</p>
            <p className="font-semibold">{campaign.totalViews.toLocaleString()}</p>
          </div>
          <div>
            <p className="text-muted-foreground">Clics</p>
            <p className="font-semibold">{campaign.totalClicks.toLocaleString()}</p>
          </div>
          <div>
            <p className="text-muted-foreground">CTR</p>
            <p className="font-semibold">{campaign.ctr.toFixed(2)}%</p>
          </div>
          <div>
            <p className="text-muted-foreground">Presupuesto</p>
            <p className="font-semibold">{budgetPercent}% restante</p>
          </div>
        </div>

        {/* Budget bar */}
        <div className="bg-muted mt-4 h-2 overflow-hidden rounded-full">
          <div
            className="bg-primary h-full rounded-full transition-all"
            style={{ width: `${budgetPercent}%` }}
          />
        </div>
        <p className="text-muted-foreground mt-1 text-xs">
          {formatCurrency(campaign.remainingBudget)} de {formatCurrency(campaign.totalBudget)}
        </p>

        {/* Actions */}
        <div className="mt-4 flex gap-2">
          {campaign.status === 'Active' && (
            <Button
              size="sm"
              variant="outline"
              onClick={() => pauseMutation.mutate(campaign.id)}
              disabled={pauseMutation.isPending}
            >
              Pausar
            </Button>
          )}
          {campaign.status === 'Paused' && (
            <Button
              size="sm"
              variant="outline"
              onClick={() => resumeMutation.mutate(campaign.id)}
              disabled={resumeMutation.isPending}
            >
              Reanudar
            </Button>
          )}
          {(campaign.status === 'Active' || campaign.status === 'Paused') && (
            <Button
              size="sm"
              variant="destructive"
              onClick={() => cancelMutation.mutate(campaign.id)}
              disabled={cancelMutation.isPending}
            >
              Cancelar
            </Button>
          )}
          <Link href={`/impulsar/mis-campanas/${campaign.id}`}>
            <Button size="sm" variant="ghost">
              Ver Detalles →
            </Button>
          </Link>
        </div>
      </CardContent>
    </Card>
  );
}

export default function MisCampanasPage() {
  const { user } = useAuthStore();
  const [page] = useState(1);

  const { data, isLoading, error } = useCampaignsByOwner(
    user?.id ?? '',
    user?.accountType === 'dealer' ? 'Dealer' : 'Individual',
    undefined,
    page,
    20
  );

  if (!user) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 text-center">
        <h1 className="mb-4 text-2xl font-bold">Inicia sesión</h1>
        <p className="text-muted-foreground mb-6">
          Necesitas iniciar sesión para ver tus campañas.
        </p>
        <Link href="/login">
          <Button>Iniciar Sesión</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Mis Campañas</h1>
          <p className="text-muted-foreground">Gestiona tus campañas publicitarias.</p>
        </div>
        <Link href="/impulsar">
          <Button>+ Nueva Campaña</Button>
        </Link>
      </div>

      {isLoading && (
        <div className="grid gap-4">
          {[1, 2, 3].map(i => (
            <Card key={i} className="animate-pulse">
              <CardContent className="h-40" />
            </Card>
          ))}
        </div>
      )}

      {error && (
        <Card className="border-destructive">
          <CardContent className="text-destructive py-8 text-center">
            Error al cargar campañas. Intenta de nuevo.
          </CardContent>
        </Card>
      )}

      {data && data.items.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground mb-4">No tienes campañas activas.</p>
            <Link href="/impulsar">
              <Button>Impulsar un Vehículo</Button>
            </Link>
          </CardContent>
        </Card>
      )}

      {data && data.items.length > 0 && (
        <div className="grid gap-4">
          {data.items.map(campaign => (
            <CampaignCard key={campaign.id} campaign={campaign} />
          ))}
        </div>
      )}
    </div>
  );
}
