/**
 * Mis Vehículos Page
 *
 * Individual seller's vehicle listings management
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Car,
  Plus,
  Search,
  Eye,
  Phone,
  MessageSquare,
  MoreVertical,
  Edit,
  Trash2,
  Pause,
  Play,
  BarChart3,
  Sparkles,
  Clock,
  CheckCircle,
  XCircle,
  AlertCircle,
} from 'lucide-react';
import Link from 'next/link';

const vehicles = [
  {
    id: 'v1',
    make: 'Toyota',
    model: 'Corolla',
    year: 2022,
    price: 1450000,
    status: 'active',
    views: 234,
    calls: 12,
    messages: 8,
    daysOnline: 15,
    isBoosted: true,
    createdAt: '2024-02-01',
  },
  {
    id: 'v2',
    make: 'Honda',
    model: 'Civic',
    year: 2021,
    price: 1350000,
    status: 'active',
    views: 156,
    calls: 6,
    messages: 4,
    daysOnline: 10,
    isBoosted: false,
    createdAt: '2024-02-06',
  },
  {
    id: 'v3',
    make: 'Hyundai',
    model: 'Tucson',
    year: 2020,
    price: 1200000,
    status: 'pending',
    views: 0,
    calls: 0,
    messages: 0,
    daysOnline: 0,
    isBoosted: false,
    createdAt: '2024-02-15',
  },
  {
    id: 'v4',
    make: 'Nissan',
    model: 'Sentra',
    year: 2019,
    price: 850000,
    status: 'paused',
    views: 89,
    calls: 3,
    messages: 2,
    daysOnline: 30,
    isBoosted: false,
    createdAt: '2024-01-15',
  },
  {
    id: 'v5',
    make: 'Mazda',
    model: 'CX-5',
    year: 2022,
    price: 1650000,
    status: 'sold',
    views: 456,
    calls: 25,
    messages: 18,
    daysOnline: 45,
    isBoosted: false,
    createdAt: '2023-12-01',
  },
];

const stats = {
  active: 2,
  pending: 1,
  paused: 1,
  sold: 1,
  totalViews: 935,
  totalCalls: 46,
  totalMessages: 32,
};

export default function MisVehiculosPage() {
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'active':
        return <Badge className="bg-emerald-500">Activo</Badge>;
      case 'pending':
        return <Badge className="bg-yellow-500">En Revisión</Badge>;
      case 'paused':
        return (
          <Badge variant="outline" className="text-gray-500">
            Pausado
          </Badge>
        );
      case 'sold':
        return <Badge className="bg-blue-500">Vendido</Badge>;
      case 'rejected':
        return <Badge className="bg-red-500">Rechazado</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  const filteredVehicles = vehicles.filter(v => {
    const matchesSearch = `${v.make} ${v.model} ${v.year}`
      .toLowerCase()
      .includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || v.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-6xl px-4 py-8">
        {/* Header */}
        <div className="mb-8 flex flex-col justify-between gap-4 md:flex-row md:items-center">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Mis Vehículos</h1>
            <p className="text-gray-600">Gestiona tus publicaciones</p>
          </div>
          <Link href="/publicar">
            <Button className="bg-emerald-600 hover:bg-emerald-700">
              <Plus className="mr-2 h-4 w-4" />
              Publicar Vehículo
            </Button>
          </Link>
        </div>

        {/* Stats */}
        <div className="mb-8 grid grid-cols-2 gap-4 md:grid-cols-4">
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-lg bg-emerald-100 p-2">
                  <CheckCircle className="h-5 w-5 text-emerald-600" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.active}</p>
                  <p className="text-sm text-gray-500">Activos</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-lg bg-blue-100 p-2">
                  <Eye className="h-5 w-5 text-blue-600" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.totalViews.toLocaleString()}</p>
                  <p className="text-sm text-gray-500">Vistas Total</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-lg bg-amber-100 p-2">
                  <Phone className="h-5 w-5 text-amber-600" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.totalCalls}</p>
                  <p className="text-sm text-gray-500">Llamadas</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-lg bg-purple-100 p-2">
                  <MessageSquare className="h-5 w-5 text-purple-600" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.totalMessages}</p>
                  <p className="text-sm text-gray-500">Mensajes</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Filters */}
        <Card className="mb-6">
          <CardContent className="p-4">
            <div className="flex flex-col gap-4 md:flex-row">
              <div className="relative flex-1">
                <Search className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <Input
                  placeholder="Buscar por marca, modelo o año..."
                  className="pl-10"
                  value={searchTerm}
                  onChange={e => setSearchTerm(e.target.value)}
                />
              </div>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-full md:w-48">
                  <SelectValue placeholder="Estado" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los estados</SelectItem>
                  <SelectItem value="active">Activos</SelectItem>
                  <SelectItem value="pending">En Revisión</SelectItem>
                  <SelectItem value="paused">Pausados</SelectItem>
                  <SelectItem value="sold">Vendidos</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardContent>
        </Card>

        {/* Vehicles List */}
        <div className="space-y-4">
          {filteredVehicles.map(vehicle => (
            <Card key={vehicle.id} className="transition-shadow hover:shadow-md">
              <CardContent className="p-0">
                <div className="flex flex-col md:flex-row">
                  {/* Image */}
                  <div className="relative h-40 flex-shrink-0 bg-gray-200 md:h-auto md:w-48">
                    <div className="absolute inset-0 flex items-center justify-center text-gray-400">
                      <Car className="h-12 w-12" />
                    </div>
                    {vehicle.isBoosted && (
                      <Badge className="absolute top-2 left-2 bg-gradient-to-r from-amber-500 to-orange-500">
                        <Sparkles className="mr-1 h-3 w-3" />
                        Destacado
                      </Badge>
                    )}
                  </div>

                  {/* Content */}
                  <div className="flex-1 p-4">
                    <div className="flex flex-col justify-between gap-4 md:flex-row md:items-start">
                      <div>
                        <div className="mb-1 flex items-center gap-2">
                          <h3 className="text-lg font-semibold text-gray-900">
                            {vehicle.make} {vehicle.model} {vehicle.year}
                          </h3>
                          {getStatusBadge(vehicle.status)}
                        </div>
                        <p className="text-xl font-bold text-emerald-600">
                          RD$ {vehicle.price.toLocaleString()}
                        </p>

                        {/* Stats */}
                        <div className="mt-3 flex items-center gap-4 text-sm text-gray-500">
                          <span className="flex items-center gap-1">
                            <Eye className="h-4 w-4" />
                            {vehicle.views} vistas
                          </span>
                          <span className="flex items-center gap-1">
                            <Phone className="h-4 w-4" />
                            {vehicle.calls} llamadas
                          </span>
                          <span className="flex items-center gap-1">
                            <MessageSquare className="h-4 w-4" />
                            {vehicle.messages} mensajes
                          </span>
                          <span className="flex items-center gap-1">
                            <Clock className="h-4 w-4" />
                            {vehicle.daysOnline} días
                          </span>
                        </div>
                      </div>

                      {/* Actions */}
                      <div className="flex items-center gap-2">
                        <Link href={`/mis-vehiculos/${vehicle.id}/stats`}>
                          <Button variant="outline" size="sm">
                            <BarChart3 className="mr-1 h-4 w-4" />
                            Stats
                          </Button>
                        </Link>
                        <Link href={`/mis-vehiculos/${vehicle.id}`}>
                          <Button variant="outline" size="sm">
                            <Edit className="mr-1 h-4 w-4" />
                            Editar
                          </Button>
                        </Link>
                        {vehicle.status === 'active' && (
                          <Link href={`/mis-vehiculos/${vehicle.id}/boost`}>
                            <Button
                              size="sm"
                              className="bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600"
                            >
                              <Sparkles className="mr-1 h-4 w-4" />
                              Boost
                            </Button>
                          </Link>
                        )}
                        <Button variant="ghost" size="icon">
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* Empty State */}
        {filteredVehicles.length === 0 && (
          <Card className="p-12 text-center">
            <Car className="mx-auto mb-4 h-12 w-12 text-gray-300" />
            <h3 className="mb-2 text-lg font-medium text-gray-900">
              No tienes vehículos publicados
            </h3>
            <p className="mb-6 text-gray-500">
              Publica tu primer vehículo y comienza a recibir ofertas
            </p>
            <Link href="/publicar">
              <Button className="bg-emerald-600 hover:bg-emerald-700">
                <Plus className="mr-2 h-4 w-4" />
                Publicar Vehículo
              </Button>
            </Link>
          </Card>
        )}
      </div>
    </div>
  );
}
