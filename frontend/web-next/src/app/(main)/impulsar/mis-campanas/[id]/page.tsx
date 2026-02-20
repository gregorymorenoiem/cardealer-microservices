'use client';

import { use } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useCampaign, useCampaignReport } from '@/hooks/use-advertising';

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(amount);
}

export default function CampaignDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: campaign, isLoading: campaignLoading } = useCampaign(id);
  const { data: report, isLoading: reportLoading } = useCampaignReport(id);

  if (campaignLoading) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <div className="animate-pulse space-y-4">
          <div className="bg-muted h-8 w-64 rounded" />
          <div className="bg-muted h-48 rounded" />
        </div>
      </div>
    );
  }

  if (!campaign) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 text-center">
        <h1 className="mb-4 text-2xl font-bold">Campaña no encontrada</h1>
        <Link href="/impulsar/mis-campanas">
          <Button variant="outline">← Volver</Button>
        </Link>
      </div>
    );
  }

  const budgetPercent =
    campaign.totalBudget > 0
      ? Math.round((campaign.remainingBudget / campaign.totalBudget) * 100)
      : 0;

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <div className="mb-8 flex items-center gap-4">
        <Link href="/impulsar/mis-campanas">
          <Button variant="ghost" size="sm">
            ← Volver
          </Button>
        </Link>
        <h1 className="text-2xl font-bold">Detalle de Campaña</h1>
        <Badge variant={campaign.status === 'Active' ? 'default' : 'secondary'}>
          {campaign.status}
        </Badge>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {/* Stats */}
        <Card>
          <CardHeader>
            <CardTitle>Resumen</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-muted-foreground text-sm">Tipo</p>
              <p className="font-semibold">
                {campaign.placementType === 'FeaturedSpot' ? '⭐ Destacado' : '💎 Premium'}
              </p>
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Modelo de Precio</p>
              <p className="font-semibold">{campaign.pricingModel}</p>
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Precio/Unidad</p>
              <p className="font-semibold">{formatCurrency(campaign.pricePerUnit)}</p>
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Quality Score</p>
              <p className="font-semibold">{campaign.qualityScore.toFixed(1)}</p>
            </div>
          </CardContent>
        </Card>

        {/* Budget */}
        <Card>
          <CardHeader>
            <CardTitle>Presupuesto</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-primary text-3xl font-bold">
              {formatCurrency(campaign.remainingBudget)}
            </div>
            <p className="text-muted-foreground text-sm">
              de {formatCurrency(campaign.totalBudget)} ({budgetPercent}% restante)
            </p>
            <div className="bg-muted mt-4 h-3 overflow-hidden rounded-full">
              <div
                className="bg-primary h-full rounded-full transition-all"
                style={{ width: `${budgetPercent}%` }}
              />
            </div>
          </CardContent>
        </Card>

        {/* Performance */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>Rendimiento</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-3 gap-6">
            <div className="text-center">
              <p className="text-3xl font-bold">{campaign.totalViews.toLocaleString()}</p>
              <p className="text-muted-foreground text-sm">Vistas</p>
            </div>
            <div className="text-center">
              <p className="text-3xl font-bold">{campaign.totalClicks.toLocaleString()}</p>
              <p className="text-muted-foreground text-sm">Clics</p>
            </div>
            <div className="text-center">
              <p className="text-3xl font-bold">{campaign.ctr.toFixed(2)}%</p>
              <p className="text-muted-foreground text-sm">CTR</p>
            </div>
          </CardContent>
        </Card>

        {/* Daily report */}
        {report && !reportLoading && report.dailyData.length > 0 && (
          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle>Datos Diarios (últimos 30 días)</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="py-2 text-left">Fecha</th>
                      <th className="py-2 text-right">Vistas</th>
                      <th className="py-2 text-right">Clics</th>
                      <th className="py-2 text-right">Gastado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {report.dailyData.slice(0, 14).map(day => (
                      <tr key={day.date} className="border-muted border-b">
                        <td className="py-2">{new Date(day.date).toLocaleDateString('es-DO')}</td>
                        <td className="text-right">{day.views}</td>
                        <td className="text-right">{day.clicks}</td>
                        <td className="text-right">{formatCurrency(day.spent)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      <div className="text-muted-foreground mt-4 text-sm">
        <p>Fecha de inicio: {new Date(campaign.startDate).toLocaleDateString('es-DO')}</p>
        <p>Fecha de fin: {new Date(campaign.endDate).toLocaleDateString('es-DO')}</p>
      </div>
    </div>
  );
}
