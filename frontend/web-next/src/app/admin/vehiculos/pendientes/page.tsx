/**
 * Admin Pending Vehicles Page
 *
 * Review and approve pending vehicle listings
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  ArrowLeft,
  Search,
  Filter,
  CheckCircle,
  XCircle,
  Clock,
  Car,
  User,
  Calendar,
  Eye,
  AlertTriangle,
  Image as ImageIcon,
  MoreVertical,
} from 'lucide-react';
import Link from 'next/link';

const pendingVehicles = [
  {
    id: '1',
    title: 'Toyota Camry 2023',
    price: 1450000,
    seller: 'Juan Martínez',
    sellerType: 'individual',
    submittedAt: '2024-02-15T10:30:00Z',
    images: 8,
    description: 'Vehículo en excelentes condiciones...',
    status: 'pending',
    priority: 'normal',
    flags: [],
  },
  {
    id: '2',
    title: 'Honda Accord 2022',
    price: 1350000,
    seller: 'Auto Premium RD',
    sellerType: 'dealer',
    submittedAt: '2024-02-15T09:15:00Z',
    images: 12,
    description: 'Único dueño, mantenimiento al día...',
    status: 'pending',
    priority: 'high',
    flags: ['precio_sospechoso'],
  },
  {
    id: '3',
    title: 'Nissan Altima 2021',
    price: 850000,
    seller: 'María García',
    sellerType: 'individual',
    submittedAt: '2024-02-14T16:45:00Z',
    images: 5,
    description: 'Buen estado general...',
    status: 'pending',
    priority: 'normal',
    flags: ['pocas_fotos'],
  },
  {
    id: '4',
    title: 'BMW X5 2023',
    price: 4500000,
    seller: 'Pedro Sánchez',
    sellerType: 'individual',
    submittedAt: '2024-02-14T14:20:00Z',
    images: 10,
    description: 'Importado directamente...',
    status: 'pending',
    priority: 'high',
    flags: ['verificar_documentos'],
  },
];

const stats = {
  pending: 45,
  approvedToday: 23,
  rejectedToday: 5,
  avgReviewTime: '2.5h',
};

export default function PendingVehiclesPage() {
  const [selectedVehicle, setSelectedVehicle] = useState<(typeof pendingVehicles)[0] | null>(null);
  const [reviewDialogOpen, setReviewDialogOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState('');

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      day: 'numeric',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const openReview = (vehicle: (typeof pendingVehicles)[0]) => {
    setSelectedVehicle(vehicle);
    setReviewDialogOpen(true);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/admin/vehiculos">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Vehículos Pendientes</h1>
            <p className="text-gray-400">Revisión y aprobación de publicaciones</p>
          </div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-yellow-900 p-2">
                <Clock className="h-5 w-5 text-yellow-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Pendientes</p>
                <p className="text-2xl font-bold text-white">{stats.pending}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-emerald-900 p-2">
                <CheckCircle className="h-5 w-5 text-emerald-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Aprobados Hoy</p>
                <p className="text-2xl font-bold text-white">{stats.approvedToday}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-red-900 p-2">
                <XCircle className="h-5 w-5 text-red-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Rechazados Hoy</p>
                <p className="text-2xl font-bold text-white">{stats.rejectedToday}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-900 p-2">
                <Clock className="h-5 w-5 text-blue-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Tiempo Promedio</p>
                <p className="text-2xl font-bold text-white">{stats.avgReviewTime}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="border-slate-700 bg-slate-800">
        <CardContent className="py-4">
          <div className="flex flex-col gap-4 sm:flex-row">
            <div className="relative flex-1">
              <Search className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
              <Input
                placeholder="Buscar por título, vendedor..."
                className="border-slate-600 bg-slate-700 pl-10"
              />
            </div>
            <Select defaultValue="all">
              <SelectTrigger className="w-40 border-slate-600 bg-slate-700">
                <SelectValue placeholder="Tipo vendedor" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="individual">Individual</SelectItem>
                <SelectItem value="dealer">Dealer</SelectItem>
              </SelectContent>
            </Select>
            <Select defaultValue="all">
              <SelectTrigger className="w-40 border-slate-600 bg-slate-700">
                <SelectValue placeholder="Prioridad" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas</SelectItem>
                <SelectItem value="high">Alta</SelectItem>
                <SelectItem value="normal">Normal</SelectItem>
              </SelectContent>
            </Select>
            <Button variant="outline">
              <Filter className="mr-2 h-4 w-4" />
              Más Filtros
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Pending List */}
      <Card className="border-slate-700 bg-slate-800">
        <CardHeader>
          <CardTitle className="text-white">Cola de Revisión</CardTitle>
          <CardDescription>{pendingVehicles.length} vehículos esperando revisión</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {pendingVehicles.map(vehicle => (
              <div
                key={vehicle.id}
                className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4"
              >
                <div className="flex items-center gap-4">
                  <div className="rounded-lg bg-slate-600 p-3">
                    <Car className="h-6 w-6 text-gray-300" />
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <p className="font-medium text-white">{vehicle.title}</p>
                      {vehicle.priority === 'high' && (
                        <Badge className="bg-red-600">Prioridad Alta</Badge>
                      )}
                    </div>
                    <p className="font-bold text-emerald-400">
                      RD$ {vehicle.price.toLocaleString()}
                    </p>
                    <div className="mt-1 flex items-center gap-4 text-sm text-gray-400">
                      <span className="flex items-center gap-1">
                        <User className="h-3 w-3" />
                        {vehicle.seller}
                      </span>
                      <Badge variant="outline" className="text-xs">
                        {vehicle.sellerType === 'dealer' ? 'Dealer' : 'Individual'}
                      </Badge>
                      <span className="flex items-center gap-1">
                        <ImageIcon className="h-3 w-3" />
                        {vehicle.images} fotos
                      </span>
                      <span className="flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        {formatDate(vehicle.submittedAt)}
                      </span>
                    </div>
                    {vehicle.flags.length > 0 && (
                      <div className="mt-2 flex gap-2">
                        {vehicle.flags.map(flag => (
                          <Badge key={flag} className="bg-yellow-600 text-xs">
                            <AlertTriangle className="mr-1 h-3 w-3" />
                            {flag.replace('_', ' ')}
                          </Badge>
                        ))}
                      </div>
                    )}
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Button variant="outline" size="sm" onClick={() => openReview(vehicle)}>
                    <Eye className="mr-1 h-4 w-4" />
                    Revisar
                  </Button>
                  <Button size="sm" className="bg-emerald-600 hover:bg-emerald-700">
                    <CheckCircle className="mr-1 h-4 w-4" />
                    Aprobar
                  </Button>
                  <Button variant="outline" size="sm" className="border-red-600 text-red-400">
                    <XCircle className="h-4 w-4" />
                  </Button>
                  <Button variant="outline" size="icon" className="h-8 w-8">
                    <MoreVertical className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Review Dialog */}
      <Dialog open={reviewDialogOpen} onOpenChange={setReviewDialogOpen}>
        <DialogContent className="max-w-2xl border-slate-700 bg-slate-800">
          <DialogHeader>
            <DialogTitle className="text-white">Revisar Publicación</DialogTitle>
          </DialogHeader>

          {selectedVehicle && (
            <div className="space-y-4">
              <div className="rounded-lg bg-slate-700/50 p-4">
                <h3 className="text-lg font-bold text-white">{selectedVehicle.title}</h3>
                <p className="text-xl font-bold text-emerald-400">
                  RD$ {selectedVehicle.price.toLocaleString()}
                </p>
              </div>

              <div className="grid grid-cols-2 gap-4 text-sm">
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="text-gray-400">Vendedor</p>
                  <p className="font-medium text-white">{selectedVehicle.seller}</p>
                </div>
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="text-gray-400">Tipo</p>
                  <p className="font-medium text-white">
                    {selectedVehicle.sellerType === 'dealer' ? 'Dealer' : 'Individual'}
                  </p>
                </div>
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="text-gray-400">Fotos</p>
                  <p className="font-medium text-white">{selectedVehicle.images}</p>
                </div>
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="text-gray-400">Enviado</p>
                  <p className="font-medium text-white">
                    {formatDate(selectedVehicle.submittedAt)}
                  </p>
                </div>
              </div>

              <div className="rounded-lg bg-slate-700/50 p-3">
                <p className="mb-2 text-gray-400">Descripción</p>
                <p className="text-sm text-white">{selectedVehicle.description}</p>
              </div>

              {selectedVehicle.flags.length > 0 && (
                <div className="rounded-lg border border-yellow-600/30 bg-yellow-900/20 p-3">
                  <p className="mb-2 font-medium text-yellow-400">Alertas Detectadas</p>
                  <div className="flex gap-2">
                    {selectedVehicle.flags.map(flag => (
                      <Badge key={flag} className="bg-yellow-600">
                        {flag.replace('_', ' ')}
                      </Badge>
                    ))}
                  </div>
                </div>
              )}

              <div>
                <p className="mb-2 text-gray-400">Razón de rechazo (opcional)</p>
                <Textarea
                  placeholder="Ingresa una razón si vas a rechazar..."
                  className="border-slate-600 bg-slate-700"
                  value={rejectReason}
                  onChange={e => setRejectReason(e.target.value)}
                />
              </div>
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setReviewDialogOpen(false)}>
              Cancelar
            </Button>
            <Button variant="outline" className="border-red-600 text-red-400">
              <XCircle className="mr-2 h-4 w-4" />
              Rechazar
            </Button>
            <Button className="bg-emerald-600 hover:bg-emerald-700">
              <CheckCircle className="mr-2 h-4 w-4" />
              Aprobar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
