/**
 * User Dashboard Page
 *
 * Main dashboard for authenticated users showing overview and quick actions
 */

import { Metadata } from 'next';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Car,
  Heart,
  Bell,
  MessageSquare,
  Search,
  Eye,
  TrendingUp,
  Clock,
  ChevronRight,
  Plus,
  Bookmark,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Mi Dashboard | OKLA',
  description: 'Tu centro de control en OKLA',
};

// Mock data
const stats = [
  { label: 'Favoritos', value: 12, icon: Heart, color: 'text-red-500', href: '/cuenta/favoritos' },
  {
    label: 'Alertas Activas',
    value: 5,
    icon: Bell,
    color: 'text-amber-500',
    href: '/cuenta/alertas',
  },
  {
    label: 'Mensajes',
    value: 3,
    icon: MessageSquare,
    color: 'text-blue-500',
    href: '/cuenta/mensajes',
  },
  {
    label: 'Búsquedas Guardadas',
    value: 4,
    icon: Bookmark,
    color: 'text-purple-500',
    href: '/cuenta/busquedas-guardadas',
  },
];

const recentActivity = [
  {
    type: 'view',
    title: 'Viste Toyota Camry 2023',
    time: 'Hace 2 horas',
    icon: Eye,
  },
  {
    type: 'favorite',
    title: 'Agregaste Honda CR-V 2024 a favoritos',
    time: 'Hace 5 horas',
    icon: Heart,
  },
  {
    type: 'alert',
    title: 'Baja de precio en BMW X5 2022',
    time: 'Ayer',
    icon: TrendingUp,
  },
  {
    type: 'message',
    title: 'Nuevo mensaje de AutoMax RD',
    time: 'Hace 2 días',
    icon: MessageSquare,
  },
];

const recommendedVehicles = [
  {
    id: '1',
    title: 'Toyota Camry SE 2023',
    price: 1450000,
    image: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=300',
    location: 'Santo Domingo',
  },
  {
    id: '2',
    title: 'Honda Accord Sport 2024',
    price: 1680000,
    image: 'https://images.unsplash.com/photo-1606611013016-969c19ba27bb?w=300',
    location: 'Santiago',
  },
  {
    id: '3',
    title: 'Mazda CX-5 2023',
    price: 1520000,
    image: 'https://images.unsplash.com/photo-1568844293986-8c8f3d5b7b1c?w=300',
    location: 'Punta Cana',
  },
];

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      {/* Welcome Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">¡Hola, Usuario! 👋</h1>
          <p className="text-gray-600">Bienvenido a tu dashboard de OKLA</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline" asChild>
            <Link href="/buscar">
              <Search className="mr-2 h-4 w-4" />
              Buscar Vehículos
            </Link>
          </Button>
          <Button className="bg-emerald-600 hover:bg-emerald-700" asChild>
            <Link href="/vender/publicar">
              <Plus className="mr-2 h-4 w-4" />
              Publicar
            </Link>
          </Button>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {stats.map(stat => {
          const Icon = stat.icon;
          return (
            <Link key={stat.label} href={stat.href}>
              <Card className="cursor-pointer transition-shadow hover:shadow-md">
                <CardContent className="p-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-500">{stat.label}</p>
                      <p className="text-2xl font-bold">{stat.value}</p>
                    </div>
                    <div className={`rounded-full bg-gray-100 p-3 ${stat.color}`}>
                      <Icon className="h-5 w-5" />
                    </div>
                  </div>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Recent Activity */}
        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg">Actividad Reciente</CardTitle>
            <Button variant="ghost" size="sm" asChild>
              <Link href="/cuenta/historial">
                Ver todo <ChevronRight className="ml-1 h-4 w-4" />
              </Link>
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentActivity.map((activity, index) => {
                const Icon = activity.icon;
                return (
                  <div
                    key={index}
                    className="flex items-center gap-4 rounded-lg p-3 hover:bg-gray-50"
                  >
                    <div className="rounded-full bg-gray-100 p-2">
                      <Icon className="h-4 w-4 text-gray-600" />
                    </div>
                    <div className="flex-1">
                      <p className="text-sm font-medium">{activity.title}</p>
                      <p className="flex items-center gap-1 text-xs text-gray-500">
                        <Clock className="h-3 w-3" />
                        {activity.time}
                      </p>
                    </div>
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Acciones Rápidas</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link href="/buscar">
                <Search className="mr-2 h-4 w-4" />
                Buscar Vehículos
              </Link>
            </Button>
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link href="/cuenta/favoritos">
                <Heart className="mr-2 h-4 w-4" />
                Ver Favoritos
              </Link>
            </Button>
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link href="/cuenta/alertas">
                <Bell className="mr-2 h-4 w-4" />
                Crear Alerta
              </Link>
            </Button>
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link href="/comparar">
                <Car className="mr-2 h-4 w-4" />
                Comparar Vehículos
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Recommended Vehicles */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-lg">Recomendados para Ti</CardTitle>
          <Button variant="ghost" size="sm" asChild>
            <Link href="/buscar">
              Ver más <ChevronRight className="ml-1 h-4 w-4" />
            </Link>
          </Button>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {recommendedVehicles.map(vehicle => (
              <Link key={vehicle.id} href={`/vehiculos/${vehicle.id}`} className="group block">
                <div className="overflow-hidden rounded-lg border transition-shadow hover:shadow-md">
                  <div className="relative aspect-[16/10] overflow-hidden">
                    <img
                      src={vehicle.image}
                      alt={vehicle.title}
                      className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                    />
                    <Badge className="absolute top-2 right-2 bg-emerald-600">Nuevo</Badge>
                  </div>
                  <div className="p-3">
                    <h3 className="text-sm font-semibold transition-colors group-hover:text-emerald-600">
                      {vehicle.title}
                    </h3>
                    <p className="text-lg font-bold text-emerald-600">
                      RD$ {vehicle.price.toLocaleString()}
                    </p>
                    <p className="text-xs text-gray-500">{vehicle.location}</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
