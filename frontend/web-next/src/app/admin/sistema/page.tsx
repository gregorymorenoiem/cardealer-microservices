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
} from 'lucide-react';

const services = [
  { name: 'API Gateway', status: 'healthy', latency: '45ms', uptime: '99.99%' },
  { name: 'Auth Service', status: 'healthy', latency: '32ms', uptime: '99.98%' },
  { name: 'Vehicle Service', status: 'healthy', latency: '78ms', uptime: '99.95%' },
  { name: 'User Service', status: 'healthy', latency: '41ms', uptime: '99.97%' },
  { name: 'Media Service', status: 'warning', latency: '156ms', uptime: '99.85%' },
  { name: 'Notification Service', status: 'healthy', latency: '28ms', uptime: '99.99%' },
  { name: 'Billing Service', status: 'healthy', latency: '52ms', uptime: '99.99%' },
  { name: 'Search Service', status: 'healthy', latency: '89ms', uptime: '99.92%' },
];

const databases = [
  { name: 'PostgreSQL Primary', status: 'healthy', connections: '45/100', size: '12.4 GB' },
  { name: 'PostgreSQL Replica', status: 'healthy', connections: '23/100', size: '12.4 GB' },
  { name: 'Redis Cache', status: 'healthy', memory: '2.1 GB', hitRate: '98.5%' },
  { name: 'RabbitMQ', status: 'healthy', queues: '12', messages: '1.2K' },
];

const infrastructure = [
  { name: 'Kubernetes Cluster', status: 'healthy', nodes: '3/3', pods: '24/30' },
  { name: 'Load Balancer', status: 'healthy', requests: '1.2K/min', bandwidth: '45 Mbps' },
  { name: 'CDN (DigitalOcean)', status: 'healthy', cacheHit: '94%', bandwidth: '120 Mbps' },
  { name: 'S3 Storage', status: 'healthy', objects: '45K', size: '89 GB' },
];

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'healthy':
      return <CheckCircle className="h-5 w-5 text-emerald-500" />;
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
      return <Badge className="bg-emerald-100 text-emerald-700">Saludable</Badge>;
    case 'warning':
      return <Badge className="bg-amber-100 text-amber-700">Advertencia</Badge>;
    case 'error':
      return <Badge className="bg-red-100 text-red-700">Error</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
};

export default function AdminSystemPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Estado del Sistema</h1>
          <p className="text-gray-400">Monitoreo de servicios e infraestructura</p>
        </div>
        <Button variant="outline">
          <RefreshCw className="mr-2 h-4 w-4" />
          Actualizar
        </Button>
      </div>

      {/* Overall Status */}
      <Card className="border-emerald-200 bg-emerald-50">
        <CardContent className="p-6">
          <div className="flex items-center gap-4">
            <div className="rounded-full bg-emerald-100 p-3">
              <CheckCircle className="h-8 w-8 text-emerald-600" />
            </div>
            <div>
              <h2 className="text-xl font-bold text-emerald-900">Todos los Sistemas Operativos</h2>
              <p className="text-emerald-700">
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
                <p className="text-2xl font-bold">24%</p>
                <p className="text-sm text-gray-500">CPU Usage</p>
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
                <p className="text-2xl font-bold">4.2 GB</p>
                <p className="text-sm text-gray-500">Memory</p>
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
                <p className="text-2xl font-bold">101 GB</p>
                <p className="text-sm text-gray-500">Storage</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-emerald-100 p-2">
                <Wifi className="h-5 w-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">165 Mbps</p>
                <p className="text-sm text-gray-500">Bandwidth</p>
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
                className="flex items-center justify-between rounded-lg bg-gray-50 p-4"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(service.status)}
                  <div>
                    <p className="font-medium">{service.name}</p>
                    <p className="text-xs text-gray-500">
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
                className="flex items-center justify-between rounded-lg bg-gray-50 p-4"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(db.status)}
                  <div>
                    <p className="font-medium">{db.name}</p>
                    <p className="text-xs text-gray-500">
                      {db.connections && `Conexiones: ${db.connections}`}
                      {db.memory && `Memoria: ${db.memory}`}
                      {db.queues && `Colas: ${db.queues} • Msgs: ${db.messages}`}
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
                className="flex items-center justify-between rounded-lg bg-gray-50 p-4"
              >
                <div className="flex items-center gap-3">
                  {getStatusIcon(infra.status)}
                  <div>
                    <p className="font-medium">{infra.name}</p>
                    <p className="text-xs text-gray-500">
                      {infra.nodes && `Nodos: ${infra.nodes} • Pods: ${infra.pods}`}
                      {infra.requests && `Requests: ${infra.requests}`}
                      {infra.cacheHit && `Cache Hit: ${infra.cacheHit}`}
                      {infra.objects && `Objetos: ${infra.objects} • ${infra.size}`}
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
          <div className="py-8 text-center text-gray-400">
            <CheckCircle className="mx-auto mb-4 h-12 w-12 opacity-50" />
            <p>No hay incidentes en las últimas 24 horas</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
