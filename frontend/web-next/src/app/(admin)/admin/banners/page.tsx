/**
 * Admin Banners Management Page
 *
 * Manage promotional banners displayed on the platform
 */

'use client';

import * as React from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Switch } from '@/components/ui/switch';
import {
  Image as ImageIcon,
  Plus,
  AlertCircle,
  Edit,
  Trash2,
  Eye,
  ExternalLink,
  Monitor,
  Smartphone,
} from 'lucide-react';
import { toast } from 'sonner';

interface Banner {
  id: string;
  title: string;
  imageUrl: string;
  mobileImageUrl?: string;
  linkUrl?: string;
  position: 'hero' | 'sidebar' | 'inline' | 'popup';
  isActive: boolean;
  startDate?: string;
  endDate?: string;
  impressions: number;
  clicks: number;
  order: number;
}

const positionLabels: Record<string, string> = {
  hero: 'Hero Principal',
  sidebar: 'Barra Lateral',
  inline: 'En Línea',
  popup: 'Popup',
};

export default function BannersPage() {
  const [banners, setBanners] = React.useState<Banner[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    async function fetchBanners() {
      try {
        setLoading(true);
        // TODO: Replace with real API call
        // const data = await adminService.getBanners();
        setBanners([]);
      } catch (err) {
        console.error('Error fetching banners:', err);
        setError('No se pudieron cargar los banners.');
      } finally {
        setLoading(false);
      }
    }
    fetchBanners();
  }, []);

  const toggleBanner = async (id: string, isActive: boolean) => {
    try {
      // TODO: Replace with real API call
      setBanners(prev => prev.map(b => (b.id === id ? { ...b, isActive } : b)));
      toast.success(isActive ? 'Banner activado' : 'Banner desactivado');
    } catch {
      toast.error('Error al actualizar el banner');
    }
  };

  const deleteBanner = async (id: string) => {
    try {
      // TODO: Replace with real API call
      setBanners(prev => prev.filter(b => b.id !== id));
      toast.success('Banner eliminado');
    } catch {
      toast.error('Error al eliminar el banner');
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold">Banners</h1>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Card key={i}>
              <CardContent className="p-6">
                <Skeleton className="h-48 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-[300px] items-center justify-center">
        <div className="text-center">
          <AlertCircle className="mx-auto h-12 w-12 text-red-500" />
          <p className="text-muted-foreground mt-4">{error}</p>
        </div>
      </div>
    );
  }

  const activeCount = banners.filter(b => b.isActive).length;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-foreground text-2xl font-bold">Banners</h1>
          <p className="text-muted-foreground">
            Gestiona los banners promocionales de la plataforma ({activeCount} activos)
          </p>
        </div>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Nuevo Banner
        </Button>
      </div>

      {banners.length === 0 ? (
        <div className="flex min-h-[300px] items-center justify-center">
          <div className="text-center">
            <ImageIcon className="text-muted-foreground mx-auto h-12 w-12" />
            <p className="text-muted-foreground mt-4">No hay banners configurados</p>
            <Button className="mt-4" variant="outline">
              <Plus className="mr-2 h-4 w-4" />
              Crear primer banner
            </Button>
          </div>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {banners.map(banner => (
            <Card key={banner.id} className="overflow-hidden transition-shadow hover:shadow-md">
              {/* Banner Preview */}
              <div className="bg-muted relative aspect-video">
                {banner.imageUrl ? (
                  /* eslint-disable-next-line @next/next/no-img-element */
                  <img
                    src={banner.imageUrl}
                    alt={banner.title}
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <div className="flex h-full items-center justify-center">
                    <ImageIcon className="text-muted-foreground h-12 w-12" />
                  </div>
                )}
                <div className="absolute top-2 right-2 flex gap-1">
                  <Badge variant={banner.isActive ? 'default' : 'secondary'}>
                    {banner.isActive ? 'Activo' : 'Inactivo'}
                  </Badge>
                </div>
                <div className="absolute bottom-2 left-2">
                  <Badge variant="outline" className="bg-background/80 backdrop-blur-sm">
                    {positionLabels[banner.position]}
                  </Badge>
                </div>
              </div>

              <CardContent className="space-y-3 p-4">
                <div className="flex items-center justify-between">
                  <h3 className="truncate font-semibold">{banner.title}</h3>
                  <Switch
                    checked={banner.isActive}
                    onCheckedChange={checked => toggleBanner(banner.id, checked)}
                  />
                </div>

                {/* Stats */}
                <div className="text-muted-foreground flex gap-4 text-xs">
                  <span className="flex items-center gap-1">
                    <Eye className="h-3 w-3" />
                    {banner.impressions.toLocaleString()}
                  </span>
                  <span className="flex items-center gap-1">
                    <ExternalLink className="h-3 w-3" />
                    {banner.clicks} clicks
                  </span>
                  {banner.impressions > 0 && (
                    <span>CTR: {((banner.clicks / banner.impressions) * 100).toFixed(1)}%</span>
                  )}
                </div>

                {/* Devices */}
                <div className="text-muted-foreground flex items-center gap-2 text-xs">
                  <Monitor className="h-3 w-3" />
                  <span>Desktop</span>
                  {banner.mobileImageUrl && (
                    <>
                      <Smartphone className="ml-2 h-3 w-3" />
                      <span>Mobile</span>
                    </>
                  )}
                </div>

                {/* Actions */}
                <div className="flex gap-2 border-t pt-2">
                  <Button variant="outline" size="sm" className="flex-1">
                    <Edit className="mr-1 h-3 w-3" />
                    Editar
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    className="text-red-500 hover:text-red-600"
                    onClick={() => deleteBanner(banner.id)}
                  >
                    <Trash2 className="h-3 w-3" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
