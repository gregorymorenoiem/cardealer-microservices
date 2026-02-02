/**
 * Dealer Sales Analytics Page
 *
 * Sales performance and revenue analytics
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
  DollarSign,
  TrendingUp,
  Car,
  Target,
  Calendar,
  Download,
  LineChart,
  Users,
  Clock,
} from 'lucide-react';
import Link from 'next/link';

const salesStats = {
  totalRevenue: 18500000,
  vehiclesSold: 12,
  avgSalePrice: 1541667,
  conversionRate: 8.5,
  avgDaysToSell: 24,
  monthlyTarget: 15,
  monthlyProgress: 80,
};

const recentSales = [
  {
    id: '1',
    vehicle: 'Toyota Corolla 2023',
    price: 1150000,
    date: '2024-02-14',
    daysToSell: 18,
    source: 'WhatsApp',
  },
  {
    id: '2',
    vehicle: 'Honda Accord 2022',
    price: 1450000,
    date: '2024-02-12',
    daysToSell: 22,
    source: 'Portal',
  },
  {
    id: '3',
    vehicle: 'Hyundai Elantra 2023',
    price: 1050000,
    date: '2024-02-10',
    daysToSell: 15,
    source: 'Referido',
  },
  {
    id: '4',
    vehicle: 'Kia Cerato 2022',
    price: 980000,
    date: '2024-02-08',
    daysToSell: 28,
    source: 'Portal',
  },
  {
    id: '5',
    vehicle: 'Nissan Altima 2021',
    price: 1250000,
    date: '2024-02-05',
    daysToSell: 35,
    source: 'WhatsApp',
  },
];

const topCategories = [
  { name: 'Sedanes', sales: 5, revenue: 5850000 },
  { name: 'SUVs', sales: 4, revenue: 7200000 },
  { name: 'Camionetas', sales: 2, revenue: 4500000 },
  { name: 'Hatchbacks', sales: 1, revenue: 950000 },
];

const leadSources = [
  { name: 'Portal Web', leads: 156, sales: 6, conversion: 3.8 },
  { name: 'WhatsApp', leads: 89, sales: 4, conversion: 4.5 },
  { name: 'Referidos', leads: 23, sales: 2, conversion: 8.7 },
];

export default function SalesAnalyticsPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/dealer/analytics">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Analytics de Ventas</h1>
            <p className="text-gray-600">Rendimiento de ventas y métricas clave</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Select defaultValue="month">
            <SelectTrigger className="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="week">Esta semana</SelectItem>
              <SelectItem value="month">Este mes</SelectItem>
              <SelectItem value="quarter">Este trimestre</SelectItem>
              <SelectItem value="year">Este año</SelectItem>
            </SelectContent>
          </Select>
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Exportar
          </Button>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-emerald-100 p-2">
                <DollarSign className="h-5 w-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Ingresos</p>
                <p className="text-2xl font-bold">
                  RD$ {(salesStats.totalRevenue / 1000000).toFixed(1)}M
                </p>
                <div className="flex items-center text-xs text-emerald-600">
                  <TrendingUp className="mr-1 h-3 w-3" />
                  +15% vs mes anterior
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Car className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Vendidos</p>
                <p className="text-2xl font-bold">{salesStats.vehiclesSold}</p>
                <p className="text-xs text-gray-400">Meta: {salesStats.monthlyTarget}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-100 p-2">
                <Target className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Conversión</p>
                <p className="text-2xl font-bold">{salesStats.conversionRate}%</p>
                <p className="text-xs text-gray-400">leads → ventas</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-orange-100 p-2">
                <Clock className="h-5 w-5 text-orange-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Tiempo Venta</p>
                <p className="text-2xl font-bold">{salesStats.avgDaysToSell}</p>
                <p className="text-xs text-gray-400">días promedio</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Monthly Target Progress */}
      <Card>
        <CardContent className="py-6">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h3 className="font-semibold">Meta Mensual</h3>
              <p className="text-sm text-gray-500">
                {salesStats.vehiclesSold} de {salesStats.monthlyTarget} vehículos
              </p>
            </div>
            <Badge
              className={
                salesStats.monthlyProgress >= 100
                  ? 'bg-emerald-100 text-emerald-700'
                  : 'bg-yellow-100 text-yellow-700'
              }
            >
              {salesStats.monthlyProgress}%
            </Badge>
          </div>
          <div className="h-4 w-full rounded-full bg-gray-200">
            <div
              className="h-4 rounded-full bg-emerald-500 transition-all"
              style={{ width: `${Math.min(salesStats.monthlyProgress, 100)}%` }}
            />
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Recent Sales */}
        <Card>
          <CardHeader>
            <CardTitle>Ventas Recientes</CardTitle>
            <CardDescription>Últimas transacciones completadas</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentSales.map(sale => (
                <div
                  key={sale.id}
                  className="flex items-center justify-between rounded-lg bg-gray-50 p-3"
                >
                  <div className="flex items-center gap-3">
                    <div className="rounded-lg bg-emerald-100 p-2">
                      <Car className="h-4 w-4 text-emerald-600" />
                    </div>
                    <div>
                      <p className="text-sm font-medium">{sale.vehicle}</p>
                      <p className="text-xs text-gray-500">
                        {new Date(sale.date).toLocaleDateString('es-DO')} • {sale.source}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="font-bold text-emerald-600">RD$ {sale.price.toLocaleString()}</p>
                    <p className="text-xs text-gray-400">{sale.daysToSell} días</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Lead Sources Performance */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              Rendimiento por Fuente
            </CardTitle>
            <CardDescription>Conversión de leads por canal</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {leadSources.map(source => (
                <div key={source.name} className="rounded-lg bg-gray-50 p-4">
                  <div className="mb-2 flex items-center justify-between">
                    <span className="font-medium">{source.name}</span>
                    <Badge variant="outline">{source.conversion}% conversión</Badge>
                  </div>
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-gray-500">Leads</p>
                      <p className="font-bold">{source.leads}</p>
                    </div>
                    <div>
                      <p className="text-gray-500">Ventas</p>
                      <p className="font-bold text-emerald-600">{source.sales}</p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Sales by Category */}
      <Card>
        <CardHeader>
          <CardTitle>Ventas por Categoría</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
            {topCategories.map(cat => (
              <div key={cat.name} className="rounded-lg bg-gray-50 p-4 text-center">
                <p className="text-sm text-gray-500">{cat.name}</p>
                <p className="mt-1 text-2xl font-bold">{cat.sales}</p>
                <p className="text-sm font-medium text-emerald-600">
                  RD$ {(cat.revenue / 1000000).toFixed(1)}M
                </p>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Revenue Chart Placeholder */}
      <Card>
        <CardHeader>
          <CardTitle>Tendencia de Ingresos</CardTitle>
          <CardDescription>Ingresos por semana en los últimos 3 meses</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex h-64 items-center justify-center rounded-lg bg-gray-50">
            <div className="text-center text-gray-400">
              <LineChart className="mx-auto mb-2 h-12 w-12" />
              <p>Gráfico de tendencias de ingresos</p>
              <p className="text-sm">Integrar Chart.js o Recharts</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
