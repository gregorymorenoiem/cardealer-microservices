/**
 * Admin User Detail Page
 *
 * View and manage individual user details
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  ArrowLeft,
  User,
  Mail,
  Phone,
  Calendar,
  MapPin,
  Shield,
  Car,
  Heart,
  MessageSquare,
  Ban,
  CheckCircle,
  AlertTriangle,
  MoreVertical,
  Clock,
  Eye,
} from 'lucide-react';
import Link from 'next/link';

const userData = {
  id: 'usr-123',
  name: 'Juan Carlos Martínez',
  email: 'juan.martinez@email.com',
  phone: '809-555-0123',
  avatar: null,
  role: 'buyer',
  status: 'active',
  verified: true,
  createdAt: '2023-06-15',
  lastLogin: '2024-02-15T10:30:00Z',
  location: 'Santo Domingo, DN',
  stats: {
    vehiclesViewed: 156,
    favorites: 12,
    messages: 8,
    reports: 0,
  },
};

const activityLog = [
  { id: '1', action: 'Inició sesión', date: '2024-02-15T10:30:00Z', ip: '192.168.1.1' },
  {
    id: '2',
    action: 'Agregó vehículo a favoritos',
    date: '2024-02-14T15:20:00Z',
    target: 'Toyota RAV4 2023',
  },
  {
    id: '3',
    action: 'Envió mensaje a dealer',
    date: '2024-02-14T14:45:00Z',
    target: 'Auto Premium RD',
  },
  { id: '4', action: 'Vio vehículo', date: '2024-02-14T14:30:00Z', target: 'Honda CR-V 2022' },
  { id: '5', action: 'Actualizó perfil', date: '2024-02-13T09:15:00Z' },
];

const favoriteVehicles = [
  { id: '1', title: 'Toyota RAV4 2023', price: 1850000, addedAt: '2024-02-14' },
  { id: '2', title: 'Honda CR-V 2022', price: 1650000, addedAt: '2024-02-12' },
  { id: '3', title: 'Mazda CX-5 2022', price: 1450000, addedAt: '2024-02-10' },
];

export default function UserDetailPage() {
  const [activeTab, setActiveTab] = useState('overview');

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/admin/usuarios">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Detalle de Usuario</h1>
            <p className="text-gray-400">ID: {userData.id}</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="border-yellow-600 text-yellow-600">
            <AlertTriangle className="mr-2 h-4 w-4" />
            Advertir
          </Button>
          <Button variant="outline" className="border-red-600 text-red-600">
            <Ban className="mr-2 h-4 w-4" />
            Suspender
          </Button>
          <Button variant="outline" size="icon">
            <MoreVertical className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* User Card */}
      <Card className="border-slate-700 bg-slate-800">
        <CardContent className="py-6">
          <div className="flex items-start gap-6">
            <Avatar className="h-20 w-20">
              <AvatarImage src={userData.avatar || undefined} />
              <AvatarFallback className="bg-emerald-600 text-2xl">
                {userData.name
                  .split(' ')
                  .map(n => n[0])
                  .join('')}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <div className="mb-2 flex items-center gap-3">
                <h2 className="text-xl font-bold text-white">{userData.name}</h2>
                {userData.verified && (
                  <Badge className="bg-emerald-600">
                    <CheckCircle className="mr-1 h-3 w-3" />
                    Verificado
                  </Badge>
                )}
                <Badge
                  className={
                    userData.status === 'active'
                      ? 'bg-emerald-100 text-emerald-700'
                      : 'bg-red-100 text-red-700'
                  }
                >
                  {userData.status === 'active' ? 'Activo' : 'Suspendido'}
                </Badge>
              </div>
              <div className="grid grid-cols-2 gap-4 text-sm md:grid-cols-4">
                <div className="flex items-center gap-2 text-gray-400">
                  <Mail className="h-4 w-4" />
                  <span>{userData.email}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Phone className="h-4 w-4" />
                  <span>{userData.phone}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <MapPin className="h-4 w-4" />
                  <span>{userData.location}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Calendar className="h-4 w-4" />
                  <span>Desde {new Date(userData.createdAt).toLocaleDateString('es-DO')}</span>
                </div>
              </div>
            </div>
            <div className="text-right">
              <p className="text-xs text-gray-500">Último acceso</p>
              <p className="text-sm text-gray-300">{formatDate(userData.lastLogin)}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-900 p-2">
                <Eye className="h-5 w-5 text-blue-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Vehículos Vistos</p>
                <p className="text-2xl font-bold text-white">{userData.stats.vehiclesViewed}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-pink-900 p-2">
                <Heart className="h-5 w-5 text-pink-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Favoritos</p>
                <p className="text-2xl font-bold text-white">{userData.stats.favorites}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-900 p-2">
                <MessageSquare className="h-5 w-5 text-purple-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Mensajes</p>
                <p className="text-2xl font-bold text-white">{userData.stats.messages}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-red-900 p-2">
                <AlertTriangle className="h-5 w-5 text-red-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Reportes</p>
                <p className="text-2xl font-bold text-white">{userData.stats.reports}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="border-slate-700 bg-slate-800">
          <TabsTrigger value="overview">Resumen</TabsTrigger>
          <TabsTrigger value="activity">Actividad</TabsTrigger>
          <TabsTrigger value="favorites">Favoritos</TabsTrigger>
          <TabsTrigger value="security">Seguridad</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6">
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <Card className="border-slate-700 bg-slate-800">
              <CardHeader>
                <CardTitle className="text-white">Información Personal</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Nombre completo</span>
                  <span className="text-white">{userData.name}</span>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Email</span>
                  <span className="text-white">{userData.email}</span>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Teléfono</span>
                  <span className="text-white">{userData.phone}</span>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Ubicación</span>
                  <span className="text-white">{userData.location}</span>
                </div>
                <div className="flex justify-between py-2">
                  <span className="text-gray-400">Rol</span>
                  <Badge className="bg-blue-600">Comprador</Badge>
                </div>
              </CardContent>
            </Card>

            <Card className="border-slate-700 bg-slate-800">
              <CardHeader>
                <CardTitle className="text-white">Actividad Reciente</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {activityLog.slice(0, 4).map(activity => (
                    <div key={activity.id} className="flex items-start gap-3">
                      <div className="rounded-full bg-slate-700 p-1.5">
                        <Clock className="h-3 w-3 text-gray-400" />
                      </div>
                      <div className="flex-1">
                        <p className="text-sm text-white">{activity.action}</p>
                        {activity.target && (
                          <p className="text-xs text-gray-400">{activity.target}</p>
                        )}
                        <p className="text-xs text-gray-500">{formatDate(activity.date)}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="activity" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Historial de Actividad</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {activityLog.map(activity => (
                  <div
                    key={activity.id}
                    className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4"
                  >
                    <div className="flex items-center gap-4">
                      <Clock className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-white">{activity.action}</p>
                        {activity.target && (
                          <p className="text-sm text-gray-400">{activity.target}</p>
                        )}
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="text-sm text-gray-300">{formatDate(activity.date)}</p>
                      {activity.ip && <p className="text-xs text-gray-500">IP: {activity.ip}</p>}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="favorites" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Vehículos Favoritos</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {favoriteVehicles.map(vehicle => (
                  <div
                    key={vehicle.id}
                    className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4"
                  >
                    <div className="flex items-center gap-4">
                      <div className="rounded-lg bg-slate-600 p-3">
                        <Car className="h-5 w-5 text-gray-300" />
                      </div>
                      <div>
                        <p className="font-medium text-white">{vehicle.title}</p>
                        <p className="text-sm text-gray-400">
                          Agregado: {new Date(vehicle.addedAt).toLocaleDateString('es-DO')}
                        </p>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="font-bold text-emerald-400">
                        RD$ {vehicle.price.toLocaleString()}
                      </p>
                      <Link href={`/vehiculos/${vehicle.id}`}>
                        <Button variant="link" size="sm" className="text-gray-400">
                          Ver vehículo
                        </Button>
                      </Link>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="security" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Seguridad de la Cuenta</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4">
                <div className="flex items-center gap-3">
                  <Shield className="h-5 w-5 text-emerald-400" />
                  <div>
                    <p className="text-white">Email Verificado</p>
                    <p className="text-sm text-gray-400">{userData.email}</p>
                  </div>
                </div>
                <Badge className="bg-emerald-600">Verificado</Badge>
              </div>
              <div className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4">
                <div className="flex items-center gap-3">
                  <Phone className="h-5 w-5 text-emerald-400" />
                  <div>
                    <p className="text-white">Teléfono Verificado</p>
                    <p className="text-sm text-gray-400">{userData.phone}</p>
                  </div>
                </div>
                <Badge className="bg-emerald-600">Verificado</Badge>
              </div>
              <div className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4">
                <div className="flex items-center gap-3">
                  <Shield className="h-5 w-5 text-yellow-400" />
                  <div>
                    <p className="text-white">Autenticación 2FA</p>
                    <p className="text-sm text-gray-400">No configurada</p>
                  </div>
                </div>
                <Badge className="bg-yellow-600">Pendiente</Badge>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
