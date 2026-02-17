/**
 * Admin System Status Page
 *
 * Monitor system health and services
 */

'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Server,
  Database,
  Cloud,
  Activity,
  CheckCircle,
  AlertTriangle,
  XCircle,
  RefreshCw,
  HardDrive,
  Cpu,
  MemoryStick,
  Wifi,
  Loader2,
} from 'lucide-react';
import {
  useServicesHealth,
  useDatabasesHealth,
  useInfrastructureHealth,
  useSystemMetrics,
} from '@/hooks/use-admin-extended';

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'healthy':
      return <CheckCircle className="h-5 w-5 text-primary" />;
    case 'warning':
      return <AlertTriangle className="h-5 w-5 text-amber-500" />;
    case 'error':
      return <XCircle className="h-5 w-5 text-red-500" />;
    default:
      return null;
  }
};

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'healthy':
      return <Badge className="bg-primary/10 text-primary">Saludable</Badge>;
    case 'warning':
      return <Badge className="bg-amber-100 text-amber-700">Advertencia</Badge>;
    case 'error':
      return <Badge className="bg-red-100 text-red-700">Error</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
};

export default function AdminSystemPage() {
  const {
    data: services = [],
    isLoading: loadingServices,
    refetch: refetchServices,
  } = useServicesHealth();
  const { data: databases = [], isLoading: loadingDbs, refetch: refetchDbs } = useDatabasesHealth();
  const {
    data: infrastructure = [],
    isLoading: loadingInfra,
    refetch: refetchInfra,
  } = useInfrastructureHealth();
  const { data: metrics, isLoading: loadingMetrics, refetch: refetchMetrics } = useSystemMetrics();

  const isRefreshing = loadingServices || loadingDbs || loadingInfra || loadingMetrics;

  const handleRefresh = () => {
    refetchServices();
    refetchDbs();
    refetchInfra();
    refetchMetrics();
  };

  const allHealthy = services.length > 0 && services.every(s => s.status === 'healthy');

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Estado del Sistema</h1>
          <p className="text-muted-foreground">Monitoreo de servicios e infraestructura</p>
        </div>
        <Button variant="outline" onClick={handleRefresh} disabled={isRefreshing}>
          <RefreshCw className={`mr-2 h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
          Actualizar
        </Button>
      </div>

      {/* Overall Status */}
      <Card
        className={allHealthy ? 'border-primary bg-primary/10' : 'border-amber-200 bg-amber-50'}
      >
        <CardContent className="p-6">
          <div className="flex items-center gap-4">
            <div className={`rounded-full p-3 ${allHealthy ? 'bg-primary/10' : 'bg-amber-100'}`}>
              {loadingServices ? (
                <Loader2 className="text-muted-foreground h-8 w-8 animate-spin" />
              ) : allHealthy ? (
                <CheckCircle className="h-8 w-8 text-primary" />
              ) : (
                <AlertTriangle className="h-8 w-8 text-amber-600" />
              )}
            </div>
            <div>
              <h2
                className={`text-xl font-bold ${allHealthy ? 'text-primary' : 'text-amber-900'}`}
              >
                {loadingServices
                  ? 'Verificando...'
                  : allHealthy
                    ? 'Todos los Sistemas Operativos'
                    : 'Algunos servicios con advertencias'}
              </h2>
              <p className={allHealthy ? 'text-primary' : 'text-amber-700'}>
                Última verificación: {new Date().toLocaleTimeString('es-DO')}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Quick Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Cpu className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{metrics?.cpu ?? '—'}</p>
                <p className="text-muted-foreground text-sm">CPU Usage</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-100 p-2">
                <MemoryStick className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{metrics?.memory ?? '—'}</p>
                <p className="text-muted-foreground text-sm">Memory</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <HardDrive className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{metrics?.storage ?? '—'}</p>
                <p className="text-muted-foreground text-sm">Storage</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-primary/10 p-2">
                <Wifi className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{metrics?.bandwidth ?? '—'}</p>
                <p className="text-muted-foreground text-sm">Bandwidth</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Services */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Server className="h-5 w-5" />
            Microservicios
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {services.map(service => (
              <div
                key={service.name}
                className="bg-muted/50 flex items-center justify-between rounded-lg p-4"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(service.status)}
                  <div>
                    <p className="font-medium">{service.name}</p>
                    <p className="text-muted-foreground text-xs">
                      Latencia: {service.latency} • Uptime: {service.uptime}
                    </p>
                  </div>
                </div>
                {getStatusBadge(service.status)}
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Databases */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Database className="h-5 w-5" />
            Bases de Datos
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {databases.map(db => (
              <div
                key={db.name}
                className="bg-muted/50 flex items-center justify-between rounded-lg p-4"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(db.status)}
                  <div>
                    <p className="font-medium">{db.name}</p>
                    <p className="text-muted-foreground text-xs">
                      {db.connections && `Conexiones: ${db.connections}`}
                      {db.responseTime && ` • Respuesta: ${db.responseTime}`}
                    </p>
                  </div>
                </div>
                {getStatusBadge(db.status)}
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Infrastructure */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Cloud className="h-5 w-5" />
            Infraestructura
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {infrastructure.map(infra => (
              <div
                key={infra.name}
                className="bg-muted/50 flex items-center justify-between rounded-lg p-4"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(infra.status)}
                  <div>
                    <p className="font-medium">{infra.name}</p>
                    <p className="text-muted-foreground text-xs">
                      {infra.description} • Región: {infra.region}
                    </p>
                  </div>
                </div>
                {getStatusBadge(infra.status)}
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Recent Incidents */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Activity className="h-5 w-5" />
            Incidentes Recientes
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-muted-foreground py-8 text-center">
            <CheckCircle className="mx-auto mb-4 h-12 w-12 opacity-50" />
            <p>No hay incidentes en las últimas 24 horas</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
