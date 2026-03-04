/**
 * Admin — Secciones del Homepage
 *
 * Manage CMS homepage sections: SUVs, Sedanes, Camionetas, Deportivos, Híbridos, Eléctricos, etc.
 * Uses the VehiclesSaleService /api/homepagesections CRUD endpoints.
 */

'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  LayoutGrid,
  Eye,
  EyeOff,
  Car,
  ChevronUp,
  ChevronDown,
  ExternalLink,
  RefreshCw,
  AlertCircle,
  Loader2,
} from 'lucide-react';
import { apiClient } from '@/lib/api-client';
import { toast } from 'sonner';
import type { HomepageSectionDto } from '@/services/homepage-sections';

// ─────────────────────────────────────────────────────────────────
// API Helpers
// ─────────────────────────────────────────────────────────────────

async function fetchSections(): Promise<HomepageSectionDto[]> {
  const res = await apiClient.get<HomepageSectionDto[]>('/api/homepagesections');
  const data = res.data as HomepageSectionDto[] | { data: HomepageSectionDto[] };
  return Array.isArray(data) ? data : ((data as { data: HomepageSectionDto[] }).data ?? []);
}

async function updateSection(slug: string, payload: Partial<HomepageSectionDto>): Promise<void> {
  await apiClient.put(`/api/homepagesections/${slug}`, payload);
}

// ─────────────────────────────────────────────────────────────────
// Section Card
// ─────────────────────────────────────────────────────────────────

const SLUG_LABELS: Record<string, string> = {
  carousel: '🎠 Carrusel Hero',
  destacados: '⭐ Destacados',
  suvs: '🚙 SUVs',
  sedanes: '🚗 Sedanes',
  camionetas: '🛻 Camionetas',
  deportivos: '🏎️ Deportivos',
  hibridos: '🔋 Híbridos',
  electricos: '⚡ Eléctricos',
  lujo: '💎 Lujo',
};

interface SectionRowProps {
  section: HomepageSectionDto;
  onToggleActive: (slug: string, current: boolean) => void;
  onMoveUp: (slug: string) => void;
  onMoveDown: (slug: string) => void;
  isFirst: boolean;
  isLast: boolean;
  isSaving: boolean;
}

function SectionRow({
  section,
  onToggleActive,
  onMoveUp,
  onMoveDown,
  isFirst,
  isLast,
  isSaving,
}: SectionRowProps) {
  const label = SLUG_LABELS[section.slug] ?? section.name;

  return (
    <div className="border-border bg-card hover:bg-muted/40 flex items-center gap-3 rounded-lg border p-4 transition-colors">
      {/* Order controls */}
      <div className="flex flex-col gap-0.5">
        <Button
          variant="ghost"
          size="icon"
          className="h-6 w-6"
          onClick={() => onMoveUp(section.slug)}
          disabled={isFirst || isSaving}
          aria-label="Subir sección"
        >
          <ChevronUp className="h-3.5 w-3.5" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="h-6 w-6"
          onClick={() => onMoveDown(section.slug)}
          disabled={isLast || isSaving}
          aria-label="Bajar sección"
        >
          <ChevronDown className="h-3.5 w-3.5" />
        </Button>
      </div>

      {/* Order number */}
      <span className="text-muted-foreground w-7 text-center font-mono text-sm">
        #{section.displayOrder}
      </span>

      {/* Section info */}
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold">{label}</p>
        <p className="text-muted-foreground truncate text-xs">
          slug: <span className="font-mono">{section.slug}</span>
          {section.layoutType && (
            <span className="text-muted-foreground/60 ml-2">· {section.layoutType}</span>
          )}
        </p>
      </div>

      {/* Vehicle count */}
      <div className="text-muted-foreground flex items-center gap-1.5">
        <Car className="h-4 w-4" />
        <span className="text-sm font-medium">{section.vehicles?.length ?? 0}</span>
      </div>

      {/* Status badge */}
      {section.isActive ? (
        <Badge className="bg-primary/10 text-primary border-primary/20 shrink-0">Activa</Badge>
      ) : (
        <Badge variant="outline" className="text-muted-foreground shrink-0">
          Inactiva
        </Badge>
      )}

      {/* Actions */}
      <div className="flex shrink-0 items-center gap-1.5">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => onToggleActive(section.slug, section.isActive)}
          disabled={isSaving}
          title={section.isActive ? 'Desactivar sección' : 'Activar sección'}
        >
          {section.isActive ? (
            <EyeOff className="mr-1.5 h-4 w-4" />
          ) : (
            <Eye className="mr-1.5 h-4 w-4" />
          )}
          {section.isActive ? 'Ocultar' : 'Mostrar'}
        </Button>
        <Button variant="ghost" size="icon" asChild title="Ver en homepage">
          <a href="/" target="_blank" rel="noopener noreferrer">
            <ExternalLink className="h-4 w-4" />
          </a>
        </Button>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────
// Page
// ─────────────────────────────────────────────────────────────────

export default function AdminSeccionesPage() {
  const queryClient = useQueryClient();
  const [savingSlug, setSavingSlug] = useState<string | null>(null);

  const {
    data: sections = [],
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ['admin-homepage-sections'],
    queryFn: fetchSections,
    staleTime: 60_000,
  });

  // Sort sections by displayOrder for display
  const sorted = [...sections].sort((a, b) => a.displayOrder - b.displayOrder);

  const mutation = useMutation({
    mutationFn: ({ slug, payload }: { slug: string; payload: Partial<HomepageSectionDto> }) =>
      updateSection(slug, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-homepage-sections'] });
      setSavingSlug(null);
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : 'Error al guardar';
      toast.error(msg);
      setSavingSlug(null);
    },
  });

  const handleToggleActive = (slug: string, currentActive: boolean) => {
    const section = sections.find(s => s.slug === slug);
    if (!section) return;
    setSavingSlug(slug);
    toast.promise(
      updateSection(slug, { ...section, isActive: !currentActive }).then(() =>
        queryClient.invalidateQueries({ queryKey: ['admin-homepage-sections'] })
      ),
      {
        loading: currentActive ? 'Ocultando sección...' : 'Activando sección...',
        success: currentActive ? 'Sección ocultada' : 'Sección activada',
        error: 'Error al actualizar sección',
      }
    );
    setSavingSlug(null);
  };

  const handleMove = (slug: string, direction: 'up' | 'down') => {
    const idx = sorted.findIndex(s => s.slug === slug);
    const swapIdx = direction === 'up' ? idx - 1 : idx + 1;
    if (swapIdx < 0 || swapIdx >= sorted.length) return;

    const a = sorted[idx];
    const b = sorted[swapIdx];
    setSavingSlug(slug);

    // Swap display orders
    Promise.all([
      updateSection(a.slug, { ...a, displayOrder: b.displayOrder }),
      updateSection(b.slug, { ...b, displayOrder: a.displayOrder }),
    ])
      .then(() => {
        queryClient.invalidateQueries({ queryKey: ['admin-homepage-sections'] });
        toast.success('Orden actualizado');
      })
      .catch(() => toast.error('Error al reordenar'))
      .finally(() => setSavingSlug(null));
  };

  if (isLoading) {
    return (
      <div className="flex min-h-[300px] items-center justify-center">
        <Loader2 className="text-primary h-8 w-8 animate-spin" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex min-h-[300px] flex-col items-center justify-center gap-3 text-center">
        <AlertCircle className="h-10 w-10 text-red-500" />
        <p className="text-muted-foreground text-sm">No se pudieron cargar las secciones.</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>
          <RefreshCw className="mr-2 h-4 w-4" /> Reintentar
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold">
            <LayoutGrid className="text-primary h-6 w-6" />
            Secciones del Homepage
          </h1>
          <p className="text-muted-foreground mt-1 text-sm">
            Configura qué secciones aparecen en el homepage y en qué orden. Usa las flechas para
            reordenar. Los cambios se aplican en tiempo real.
          </p>
        </div>
        <Button variant="outline" size="sm" onClick={() => refetch()}>
          <RefreshCw className="mr-2 h-4 w-4" />
          Actualizar
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        {[
          {
            label: 'Total',
            value: sorted.length,
            color: 'text-foreground',
          },
          {
            label: 'Activas',
            value: sorted.filter(s => s.isActive).length,
            color: 'text-primary',
          },
          {
            label: 'Inactivas',
            value: sorted.filter(s => !s.isActive).length,
            color: 'text-muted-foreground',
          },
          {
            label: 'Total vehículos',
            value: sorted.reduce((acc, s) => acc + (s.vehicles?.length ?? 0), 0),
            color: 'text-foreground',
          },
        ].map(stat => (
          <Card key={stat.label} className="p-4">
            <p className="text-muted-foreground text-xs">{stat.label}</p>
            <p className={`mt-1 text-2xl font-bold ${stat.color}`}>{stat.value}</p>
          </Card>
        ))}
      </div>

      {/* Section list */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Secciones configuradas</CardTitle>
          <CardDescription>
            El homepage muestra las secciones activas en el orden indicado. Las secciones de tipo
            vehículo (SUV, Sedán, etc.) aparecen al final de la página, después de las secciones con
            fotos grandes.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-2">
          {sorted.length === 0 ? (
            <p className="text-muted-foreground py-8 text-center text-sm">
              No hay secciones configuradas aún.
            </p>
          ) : (
            sorted.map((section, idx) => (
              <SectionRow
                key={section.slug}
                section={section}
                onToggleActive={handleToggleActive}
                onMoveUp={s => handleMove(s, 'up')}
                onMoveDown={s => handleMove(s, 'down')}
                isFirst={idx === 0}
                isLast={idx === sorted.length - 1}
                isSaving={savingSlug === section.slug || mutation.isPending}
              />
            ))
          )}
        </CardContent>
      </Card>

      {/* Info on adding vehicles */}
      <Card className="border-primary/20 bg-primary/5">
        <CardContent className="pt-6">
          <div className="flex gap-3">
            <Car className="text-primary mt-0.5 h-5 w-5 shrink-0" />
            <div>
              <p className="text-sm font-semibold">¿Cómo añadir vehículos a una sección?</p>
              <p className="text-muted-foreground mt-1 text-sm">
                Para agregar vehículos a las secciones SUVs, Sedanes, Híbridos, etc., ve a{' '}
                <strong>Vehículos → Editar vehículo → Secciones</strong> y asígnalo a la sección
                correspondiente. También puedes usar la API:{' '}
                <code className="bg-muted rounded px-1 py-0.5 font-mono text-xs">
                  POST /api/homepagesections/&#123;slug&#125;/vehicles
                </code>
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
