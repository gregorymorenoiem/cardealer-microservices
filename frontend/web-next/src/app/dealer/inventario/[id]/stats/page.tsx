/**
 * Dealer Vehicle Stats Page
 *
 * Analytics for a specific vehicle in dealer inventory
 */

'use client';

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  ArrowLeft,
  Eye,
  Phone,
  MessageSquare,
  Heart,
  TrendingUp,
  TrendingDown,
  Calendar,
  Clock,
  Smartphone,
  Monitor,
  Tablet,
  MapPin,
  Share2,
  Zap,
} from 'lucide-react';
import Link from 'next/link';

// Mock stats data
const vehicleStats = {
  id: 'INV-001',
  title: 'Toyota Camry 2023',
  price: 2150000,
  status: 'active',
  createdAt: '2025-01-15',
  daysActive: 16,
  overview: {
    views: 1245,
    viewsChange: 12.5,
    calls: 48,
    callsChange: 8.3,
    messages: 32,
    messagesChange: -5.2,
    saves: 89,
    savesChange: 15.7,
  },
  dailyViews: [
    { day: 'Lun', views: 145 },
    { day: 'Mar', views: 189 },
    { day: 'Mie', views: 167 },
    { day: 'Jue', views: 234 },
    { day: 'Vie', views: 278 },
    { day: 'Sab', views: 312 },
    { day: 'Dom', views: 198 },
  ],
  devices: {
    mobile: 68,
    desktop: 28,
    tablet: 4,
  },
  topCities: [
    { city: 'Santo Domingo', percentage: 45 },
    { city: 'Santiago', percentage: 22 },
    { city: 'Punta Cana', percentage: 12 },
    { city: 'San Pedro', percentage: 8 },
    { city: 'Otros', percentage: 13 },
  ],
  hourlyDistribution: [
    { hour: '6am', value: 5 },
    { hour: '9am', value: 12 },
    { hour: '12pm', value: 18 },
    { hour: '3pm', value: 15 },
    { hour: '6pm', value: 25 },
    { hour: '9pm', value: 20 },
    { hour: '12am', value: 5 },
  ],
  recentLeads: [
    { id: 1, name: 'Juan Pérez', type: 'message', time: 'Hace 2h' },
    { id: 2, name: 'María García', type: 'call', time: 'Hace 5h' },
    { id: 3, name: 'Carlos López', type: 'save', time: 'Hace 1d' },
    { id: 4, name: 'Ana Martínez', type: 'message', time: 'Hace 1d' },
  ],
};

const maxViews = Math.max(...vehicleStats.dailyViews.map(d => d.views));

export default function DealerVehicleStatsPage() {
  return (
    <div className="min-h-screen bg-slate-900 p-6">
      {/* Header */}
      <div className="mb-6 flex flex-col justify-between gap-4 md:flex-row md:items-center">
        <div className="flex items-center gap-4">
          <Link href={`/dealer/inventario/${vehicleStats.id}`}>
            <Button
              variant="ghost"
              size="icon"
              className="text-slate-400 hover:bg-slate-800 hover:text-white"
            >
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold text-white">Estadísticas del Vehículo</h1>
            <p className="text-slate-400">{vehicleStats.title}</p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <Select defaultValue="7d">
            <SelectTrigger className="w-[150px] border-slate-700 bg-slate-800 text-white">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="7d">Últimos 7 días</SelectItem>
              <SelectItem value="30d">Últimos 30 días</SelectItem>
              <SelectItem value="90d">Últimos 90 días</SelectItem>
              <SelectItem value="all">Todo el tiempo</SelectItem>
            </SelectContent>
          </Select>
          <Link href={`/dealer/inventario/${vehicleStats.id}/boost`}>
            <Button className="bg-yellow-600 hover:bg-yellow-700">
              <Zap className="mr-2 h-4 w-4" />
              Promocionar
            </Button>
          </Link>
        </div>
      </div>

      {/* Overview Stats */}
      <div className="mb-6 grid grid-cols-2 gap-4 md:grid-cols-4">
        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <Eye className="h-5 w-5 text-blue-400" />
              <Badge
                className={vehicleStats.overview.viewsChange >= 0 ? 'bg-emerald-600' : 'bg-red-600'}
              >
                {vehicleStats.overview.viewsChange >= 0 ? '+' : ''}
                {vehicleStats.overview.viewsChange}%
              </Badge>
            </div>
            <p className="text-3xl font-bold text-white">
              {vehicleStats.overview.views.toLocaleString()}
            </p>
            <p className="text-sm text-slate-400">Vistas totales</p>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <Phone className="h-5 w-5 text-emerald-400" />
              <Badge
                className={vehicleStats.overview.callsChange >= 0 ? 'bg-emerald-600' : 'bg-red-600'}
              >
                {vehicleStats.overview.callsChange >= 0 ? '+' : ''}
                {vehicleStats.overview.callsChange}%
              </Badge>
            </div>
            <p className="text-3xl font-bold text-white">{vehicleStats.overview.calls}</p>
            <p className="text-sm text-slate-400">Llamadas</p>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <MessageSquare className="h-5 w-5 text-purple-400" />
              <Badge
                className={
                  vehicleStats.overview.messagesChange >= 0 ? 'bg-emerald-600' : 'bg-red-600'
                }
              >
                {vehicleStats.overview.messagesChange >= 0 ? '+' : ''}
                {vehicleStats.overview.messagesChange}%
              </Badge>
            </div>
            <p className="text-3xl font-bold text-white">{vehicleStats.overview.messages}</p>
            <p className="text-sm text-slate-400">Mensajes</p>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <Heart className="h-5 w-5 text-red-400" />
              <Badge
                className={vehicleStats.overview.savesChange >= 0 ? 'bg-emerald-600' : 'bg-red-600'}
              >
                {vehicleStats.overview.savesChange >= 0 ? '+' : ''}
                {vehicleStats.overview.savesChange}%
              </Badge>
            </div>
            <p className="text-3xl font-bold text-white">{vehicleStats.overview.saves}</p>
            <p className="text-sm text-slate-400">Guardados</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Charts Column */}
        <div className="space-y-6 lg:col-span-2">
          {/* Daily Views Chart */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Vistas por Día</CardTitle>
              <CardDescription className="text-slate-400">Últimos 7 días</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex h-48 items-end gap-2">
                {vehicleStats.dailyViews.map((day, i) => (
                  <div key={i} className="flex flex-1 flex-col items-center">
                    <div
                      className="w-full rounded-t bg-emerald-600"
                      style={{ height: `${(day.views / maxViews) * 150}px` }}
                    />
                    <span className="mt-2 text-xs text-slate-400">{day.day}</span>
                    <span className="text-xs text-white">{day.views}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Hourly Distribution */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-white">
                <Clock className="h-5 w-5" />
                Distribución por Hora
              </CardTitle>
              <CardDescription className="text-slate-400">
                Cuándo tus visitantes ven esta publicación
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex h-32 items-end gap-3">
                {vehicleStats.hourlyDistribution.map((h, i) => (
                  <div key={i} className="flex flex-1 flex-col items-center">
                    <div
                      className="w-full rounded-t bg-blue-500"
                      style={{ height: `${h.value * 4}px` }}
                    />
                    <span className="mt-2 text-xs text-slate-400">{h.hour}</span>
                  </div>
                ))}
              </div>
              <p className="mt-4 text-sm text-slate-400">
                💡 Mayor actividad entre 6pm y 9pm. Considera responder mensajes en ese horario.
              </p>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Device Breakdown */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Dispositivos</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Smartphone className="h-5 w-5 text-slate-400" />
                  <span className="text-slate-300">Móvil</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="h-2 w-24 overflow-hidden rounded-full bg-slate-700">
                    <div
                      className="h-full bg-emerald-500"
                      style={{ width: `${vehicleStats.devices.mobile}%` }}
                    />
                  </div>
                  <span className="font-medium text-white">{vehicleStats.devices.mobile}%</span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Monitor className="h-5 w-5 text-slate-400" />
                  <span className="text-slate-300">Escritorio</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="h-2 w-24 overflow-hidden rounded-full bg-slate-700">
                    <div
                      className="h-full bg-blue-500"
                      style={{ width: `${vehicleStats.devices.desktop}%` }}
                    />
                  </div>
                  <span className="font-medium text-white">{vehicleStats.devices.desktop}%</span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Tablet className="h-5 w-5 text-slate-400" />
                  <span className="text-slate-300">Tablet</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="h-2 w-24 overflow-hidden rounded-full bg-slate-700">
                    <div
                      className="h-full bg-purple-500"
                      style={{ width: `${vehicleStats.devices.tablet}%` }}
                    />
                  </div>
                  <span className="font-medium text-white">{vehicleStats.devices.tablet}%</span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Top Cities */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-white">
                <MapPin className="h-5 w-5" />
                Ciudades Principales
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {vehicleStats.topCities.map((city, i) => (
                <div key={i} className="flex items-center justify-between">
                  <span className="text-slate-300">{city.city}</span>
                  <div className="flex items-center gap-2">
                    <div className="h-2 w-20 overflow-hidden rounded-full bg-slate-700">
                      <div
                        className="h-full bg-emerald-500"
                        style={{ width: `${city.percentage}%` }}
                      />
                    </div>
                    <span className="text-sm font-medium text-white">{city.percentage}%</span>
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>

          {/* Recent Leads */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Leads Recientes</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {vehicleStats.recentLeads.map(lead => (
                <div
                  key={lead.id}
                  className="flex items-center justify-between rounded-lg bg-slate-900 p-3"
                >
                  <div className="flex items-center gap-3">
                    {lead.type === 'message' && (
                      <MessageSquare className="h-4 w-4 text-purple-400" />
                    )}
                    {lead.type === 'call' && <Phone className="h-4 w-4 text-emerald-400" />}
                    {lead.type === 'save' && <Heart className="h-4 w-4 text-red-400" />}
                    <div>
                      <p className="text-sm text-white">{lead.name}</p>
                      <p className="text-xs text-slate-400">{lead.time}</p>
                    </div>
                  </div>
                  <Button variant="ghost" size="sm" className="text-slate-400 hover:text-white">
                    Ver
                  </Button>
                </div>
              ))}
              <Link href="/dealer/leads">
                <Button
                  variant="outline"
                  className="w-full border-slate-700 text-slate-300 hover:bg-slate-700"
                >
                  Ver todos los leads
                </Button>
              </Link>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Performance Summary */}
      <Card className="mt-6 border-slate-700 bg-slate-800">
        <CardContent className="p-6">
          <div className="flex items-start gap-4">
            <div className="rounded-lg bg-emerald-600/20 p-3">
              <TrendingUp className="h-6 w-6 text-emerald-400" />
            </div>
            <div>
              <h3 className="mb-2 text-lg font-semibold text-white">Rendimiento General: Bueno</h3>
              <p className="text-slate-400">
                Esta publicación está funcionando mejor que el 65% de vehículos similares. Para
                mejorar aún más, considera promocionar la publicación o agregar más fotos.
              </p>
              <div className="mt-4 flex gap-3">
                <Link href={`/dealer/inventario/${vehicleStats.id}/boost`}>
                  <Button size="sm" className="bg-yellow-600 hover:bg-yellow-700">
                    <Zap className="mr-2 h-4 w-4" />
                    Promocionar
                  </Button>
                </Link>
                <Link href={`/dealer/inventario/${vehicleStats.id}`}>
                  <Button
                    size="sm"
                    variant="outline"
                    className="border-slate-700 text-slate-300 hover:bg-slate-700"
                  >
                    Editar publicación
                  </Button>
                </Link>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
