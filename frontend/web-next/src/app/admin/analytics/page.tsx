/**
 * Admin Analytics Page
 *
 * Platform analytics and insights
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import {
  BarChart3,
  TrendingUp,
  TrendingDown,
  Users,
  Car,
  Eye,
  DollarSign,
  Calendar,
  Download,
  ArrowUpRight,
  ArrowDownRight,
} from 'lucide-react';

const overviewStats = [
  {
    label: 'Visitas Totales',
    value: '2.4M',
    change: '+18.2%',
    trend: 'up',
    icon: Eye,
  },
  {
    label: 'Usuarios Activos',
    value: '45.8K',
    change: '+12.5%',
    trend: 'up',
    icon: Users,
  },
  {
    label: 'Vehículos Publicados',
    value: '12.4K',
    change: '+8.3%',
    trend: 'up',
    icon: Car,
  },
  {
    label: 'MRR',
    value: 'RD$ 2.4M',
    change: '+22.1%',
    trend: 'up',
    icon: DollarSign,
  },
];

const weeklyData = [
  { day: 'Lun', visits: 85000, signups: 450, listings: 125 },
  { day: 'Mar', visits: 92000, signups: 520, listings: 145 },
  { day: 'Mié', visits: 78000, signups: 380, listings: 98 },
  { day: 'Jue', visits: 88000, signups: 445, listings: 132 },
  { day: 'Vie', visits: 95000, signups: 580, listings: 165 },
  { day: 'Sáb', visits: 120000, signups: 620, listings: 198 },
  { day: 'Dom', visits: 105000, signups: 510, listings: 145 },
];

const topVehicles = [
  { make: 'Toyota', model: 'Corolla', searches: 15420, views: 45200, leads: 1234 },
  { make: 'Honda', model: 'CR-V', searches: 12890, views: 38900, leads: 987 },
  { make: 'Toyota', model: 'RAV4', searches: 11250, views: 35400, leads: 876 },
  { make: 'Hyundai', model: 'Tucson', searches: 9870, views: 28900, leads: 765 },
  { make: 'Kia', model: 'Sportage', searches: 8540, views: 25600, leads: 654 },
];

const trafficSources = [
  { source: 'Búsqueda Orgánica', percentage: 45, visits: '1.08M' },
  { source: 'Directo', percentage: 25, visits: '600K' },
  { source: 'Redes Sociales', percentage: 18, visits: '432K' },
  { source: 'Referidos', percentage: 8, visits: '192K' },
  { source: 'Email', percentage: 4, visits: '96K' },
];

const deviceBreakdown = [
  { device: 'Mobile', percentage: 68 },
  { device: 'Desktop', percentage: 28 },
  { device: 'Tablet', percentage: 4 },
];

export default function AdminAnalyticsPage() {
  const [dateRange, setDateRange] = useState('7d');

  const maxVisits = Math.max(...weeklyData.map(d => d.visits));

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
          <p className="text-gray-600">Métricas y análisis de la plataforma</p>
        </div>
        <div className="flex gap-3">
          <div className="flex rounded-lg border">
            {['7d', '30d', '90d', '1y'].map(range => (
              <button
                key={range}
                onClick={() => setDateRange(range)}
                className={`px-3 py-1.5 text-sm ${
                  dateRange === range
                    ? 'bg-slate-900 text-white'
                    : 'text-gray-600 hover:bg-gray-100'
                } ${range === '7d' ? 'rounded-l-lg' : ''} ${range === '1y' ? 'rounded-r-lg' : ''}`}
              >
                {range}
              </button>
            ))}
          </div>
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Exportar
          </Button>
        </div>
      </div>

      {/* Overview Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {overviewStats.map(stat => {
          const Icon = stat.icon;
          const TrendIcon = stat.trend === 'up' ? ArrowUpRight : ArrowDownRight;
          return (
            <Card key={stat.label}>
              <CardContent className="p-4">
                <div className="mb-3 flex items-center justify-between">
                  <div className="rounded-lg bg-slate-100 p-2">
                    <Icon className="h-5 w-5 text-slate-600" />
                  </div>
                  <div
                    className={`flex items-center text-sm ${
                      stat.trend === 'up' ? 'text-emerald-600' : 'text-red-600'
                    }`}
                  >
                    <TrendIcon className="h-4 w-4" />
                    {stat.change}
                  </div>
                </div>
                <p className="text-2xl font-bold">{stat.value}</p>
                <p className="text-xs text-gray-500">{stat.label}</p>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Weekly Chart */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <BarChart3 className="h-5 w-5" />
            Visitas Semanales
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-64">
            <div className="flex h-full items-end gap-2">
              {weeklyData.map(data => (
                <div key={data.day} className="flex flex-1 flex-col items-center gap-2">
                  <div
                    className="w-full rounded-t-lg bg-gradient-to-t from-emerald-600 to-emerald-400 transition-all hover:opacity-80"
                    style={{ height: `${(data.visits / maxVisits) * 100}%` }}
                  />
                  <span className="text-xs text-gray-500">{data.day}</span>
                </div>
              ))}
            </div>
          </div>
          <div className="mt-4 flex justify-center gap-8 text-sm">
            <div className="flex items-center gap-2">
              <div className="h-3 w-3 rounded-full bg-emerald-500" />
              <span className="text-gray-600">Visitas</span>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Top Vehicles Searched */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Vehículos Más Buscados</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {topVehicles.map((vehicle, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between rounded-lg p-3 hover:bg-gray-50"
                >
                  <div className="flex items-center gap-3">
                    <span className="flex h-6 w-6 items-center justify-center rounded-full bg-slate-900 text-xs text-white">
                      {index + 1}
                    </span>
                    <div>
                      <p className="font-medium">
                        {vehicle.make} {vehicle.model}
                      </p>
                      <p className="text-xs text-gray-500">
                        {vehicle.searches.toLocaleString()} búsquedas
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-medium">{vehicle.views.toLocaleString()} vistas</p>
                    <p className="text-xs text-emerald-600">
                      {vehicle.leads.toLocaleString()} leads
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Traffic Sources */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Fuentes de Tráfico</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {trafficSources.map(source => (
                <div key={source.source}>
                  <div className="mb-1 flex justify-between text-sm">
                    <span className="font-medium">{source.source}</span>
                    <span className="text-gray-500">{source.visits}</span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                    <div
                      className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-emerald-400"
                      style={{ width: `${source.percentage}%` }}
                    />
                  </div>
                  <p className="mt-1 text-xs text-gray-500">{source.percentage}%</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Device Breakdown & Conversions */}
      <div className="grid gap-6 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Dispositivos</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {deviceBreakdown.map(device => (
                <div key={device.device} className="flex items-center gap-4">
                  <div className="flex-1">
                    <div className="mb-1 flex justify-between text-sm">
                      <span>{device.device}</span>
                      <span className="font-medium">{device.percentage}%</span>
                    </div>
                    <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                      <div
                        className="h-full rounded-full bg-slate-900"
                        style={{ width: `${device.percentage}%` }}
                      />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Conversiones</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="rounded-lg bg-gray-50 p-4">
                <p className="text-2xl font-bold">3.2%</p>
                <p className="text-sm text-gray-500">Visita → Registro</p>
              </div>
              <div className="rounded-lg bg-gray-50 p-4">
                <p className="text-2xl font-bold">12.5%</p>
                <p className="text-sm text-gray-500">Registro → Publicación</p>
              </div>
              <div className="rounded-lg bg-gray-50 p-4">
                <p className="text-2xl font-bold">8.7%</p>
                <p className="text-sm text-gray-500">Vista → Lead</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Ingresos por Canal</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex items-center justify-between rounded-lg bg-purple-50 p-3">
                <span className="font-medium text-purple-700">Suscripciones Dealers</span>
                <span className="font-bold text-purple-700">RD$ 1.8M</span>
              </div>
              <div className="flex items-center justify-between rounded-lg bg-blue-50 p-3">
                <span className="font-medium text-blue-700">Publicaciones Premium</span>
                <span className="font-bold text-blue-700">RD$ 450K</span>
              </div>
              <div className="flex items-center justify-between rounded-lg bg-amber-50 p-3">
                <span className="font-medium text-amber-700">Destacados</span>
                <span className="font-bold text-amber-700">RD$ 150K</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
