/**
 * Admin Dealer Detail Page
 *
 * View and manage individual dealer details
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Progress } from '@/components/ui/progress';
import {
  ArrowLeft,
  Building2,
  Mail,
  Phone,
  Calendar,
  MapPin,
  Shield,
  Car,
  Users,
  DollarSign,
  Star,
  Ban,
  CheckCircle,
  AlertTriangle,
  MoreVertical,
  FileText,
  Eye,
  TrendingUp,
  Clock,
} from 'lucide-react';
import Link from 'next/link';

const dealerData = {
  id: 'dlr-456',
  name: 'Auto Premium RD',
  legalName: 'Auto Premium RD SRL',
  rnc: '130-12345-1',
  email: 'contacto@autopremiumrd.com',
  phone: '809-555-1234',
  logo: null,
  status: 'active',
  verified: true,
  plan: 'Pro',
  createdAt: '2023-03-10',
  location: 'Av. Winston Churchill #123, Santo Domingo',
  owner: 'Carlos Pérez',
  stats: {
    activeVehicles: 45,
    totalViews: 12500,
    totalLeads: 189,
    conversionRate: 8.5,
    rating: 4.8,
    reviews: 56,
    employees: 8,
    locations: 2,
  },
  billing: {
    plan: 'Pro',
    monthlyAmount: 12900,
    nextBillingDate: '2024-03-01',
    status: 'active',
  },
};

const vehicles = [
  { id: '1', title: 'Toyota RAV4 2023', price: 1850000, views: 342, leads: 18, status: 'active' },
  { id: '2', title: 'Honda CR-V 2022', price: 1650000, views: 289, leads: 15, status: 'active' },
  {
    id: '3',
    title: 'Hyundai Tucson 2023',
    price: 1450000,
    views: 267,
    leads: 12,
    status: 'active',
  },
  { id: '4', title: 'Mazda CX-5 2022', price: 1350000, views: 234, leads: 11, status: 'active' },
];

const documents = [
  { id: '1', name: 'RNC', status: 'verified', uploadedAt: '2023-03-10' },
  { id: '2', name: 'Registro Mercantil', status: 'verified', uploadedAt: '2023-03-10' },
  { id: '3', name: 'Licencia Comercial', status: 'verified', uploadedAt: '2023-03-10' },
  { id: '4', name: 'Cédula Representante', status: 'verified', uploadedAt: '2023-03-10' },
];

export default function DealerDetailPage() {
  const [activeTab, setActiveTab] = useState('overview');

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/admin/dealers">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Detalle de Dealer</h1>
            <p className="text-gray-400">ID: {dealerData.id}</p>
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

      {/* Dealer Card */}
      <Card className="border-slate-700 bg-slate-800">
        <CardContent className="py-6">
          <div className="flex items-start gap-6">
            <Avatar className="h-20 w-20">
              <AvatarImage src={dealerData.logo || undefined} />
              <AvatarFallback className="bg-emerald-600 text-2xl">
                {dealerData.name
                  .split(' ')
                  .map(n => n[0])
                  .join('')}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <div className="mb-2 flex items-center gap-3">
                <h2 className="text-xl font-bold text-white">{dealerData.name}</h2>
                {dealerData.verified && (
                  <Badge className="bg-emerald-600">
                    <CheckCircle className="mr-1 h-3 w-3" />
                    Verificado
                  </Badge>
                )}
                <Badge className="bg-purple-600">{dealerData.plan}</Badge>
                <Badge
                  className={
                    dealerData.status === 'active'
                      ? 'bg-emerald-100 text-emerald-700'
                      : 'bg-red-100 text-red-700'
                  }
                >
                  {dealerData.status === 'active' ? 'Activo' : 'Suspendido'}
                </Badge>
              </div>
              <p className="mb-2 text-sm text-gray-400">
                {dealerData.legalName} • RNC: {dealerData.rnc}
              </p>
              <div className="grid grid-cols-2 gap-4 text-sm md:grid-cols-4">
                <div className="flex items-center gap-2 text-gray-400">
                  <Mail className="h-4 w-4" />
                  <span>{dealerData.email}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Phone className="h-4 w-4" />
                  <span>{dealerData.phone}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <MapPin className="h-4 w-4" />
                  <span>2 ubicaciones</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Calendar className="h-4 w-4" />
                  <span>Desde {new Date(dealerData.createdAt).toLocaleDateString('es-DO')}</span>
                </div>
              </div>
            </div>
            <div className="text-right">
              <div className="mb-1 flex items-center gap-1 text-yellow-400">
                <Star className="h-5 w-5 fill-current" />
                <span className="text-xl font-bold">{dealerData.stats.rating}</span>
              </div>
              <p className="text-sm text-gray-400">{dealerData.stats.reviews} reviews</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-emerald-900 p-2">
                <Car className="h-5 w-5 text-emerald-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Vehículos Activos</p>
                <p className="text-2xl font-bold text-white">{dealerData.stats.activeVehicles}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-900 p-2">
                <Eye className="h-5 w-5 text-blue-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Vistas Totales</p>
                <p className="text-2xl font-bold text-white">
                  {dealerData.stats.totalViews.toLocaleString()}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-900 p-2">
                <Users className="h-5 w-5 text-purple-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Leads Totales</p>
                <p className="text-2xl font-bold text-white">{dealerData.stats.totalLeads}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-orange-900 p-2">
                <TrendingUp className="h-5 w-5 text-orange-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Conversión</p>
                <p className="text-2xl font-bold text-white">{dealerData.stats.conversionRate}%</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="border-slate-700 bg-slate-800">
          <TabsTrigger value="overview">Resumen</TabsTrigger>
          <TabsTrigger value="vehicles">Vehículos</TabsTrigger>
          <TabsTrigger value="documents">Documentos</TabsTrigger>
          <TabsTrigger value="billing">Facturación</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6">
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <Card className="border-slate-700 bg-slate-800">
              <CardHeader>
                <CardTitle className="text-white">Información del Negocio</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Nombre Comercial</span>
                  <span className="text-white">{dealerData.name}</span>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Razón Social</span>
                  <span className="text-white">{dealerData.legalName}</span>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">RNC</span>
                  <span className="text-white">{dealerData.rnc}</span>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Representante</span>
                  <span className="text-white">{dealerData.owner}</span>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Empleados</span>
                  <span className="text-white">{dealerData.stats.employees}</span>
                </div>
                <div className="flex justify-between py-2">
                  <span className="text-gray-400">Ubicaciones</span>
                  <span className="text-white">{dealerData.stats.locations}</span>
                </div>
              </CardContent>
            </Card>

            <Card className="border-slate-700 bg-slate-800">
              <CardHeader>
                <CardTitle className="text-white">Suscripción</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="rounded-lg bg-slate-700/50 p-4">
                  <div className="mb-2 flex items-center justify-between">
                    <span className="font-medium text-white">Plan {dealerData.billing.plan}</span>
                    <Badge className="bg-emerald-600">Activo</Badge>
                  </div>
                  <p className="text-2xl font-bold text-emerald-400">
                    RD$ {dealerData.billing.monthlyAmount.toLocaleString()}/mes
                  </p>
                </div>
                <div className="flex justify-between border-b border-slate-700 py-2">
                  <span className="text-gray-400">Próximo cobro</span>
                  <span className="text-white">
                    {new Date(dealerData.billing.nextBillingDate).toLocaleDateString('es-DO')}
                  </span>
                </div>
                <div className="flex justify-between py-2">
                  <span className="text-gray-400">Método de pago</span>
                  <span className="text-white">Visa •••• 4242</span>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="vehicles" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Vehículos Publicados</CardTitle>
              <CardDescription>{vehicles.length} vehículos activos</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {vehicles.map(vehicle => (
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
                        <p className="text-sm text-emerald-400">
                          RD$ {vehicle.price.toLocaleString()}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-6 text-sm">
                      <div className="text-center">
                        <p className="font-medium text-white">{vehicle.views}</p>
                        <p className="text-gray-400">vistas</p>
                      </div>
                      <div className="text-center">
                        <p className="font-medium text-white">{vehicle.leads}</p>
                        <p className="text-gray-400">leads</p>
                      </div>
                      <Badge className="bg-emerald-600">Activo</Badge>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="documents" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Documentos de Verificación</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {documents.map(doc => (
                  <div
                    key={doc.id}
                    className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4"
                  >
                    <div className="flex items-center gap-4">
                      <div className="rounded-lg bg-slate-600 p-3">
                        <FileText className="h-5 w-5 text-gray-300" />
                      </div>
                      <div>
                        <p className="font-medium text-white">{doc.name}</p>
                        <p className="text-sm text-gray-400">
                          Subido: {new Date(doc.uploadedAt).toLocaleDateString('es-DO')}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge className="bg-emerald-600">
                        <CheckCircle className="mr-1 h-3 w-3" />
                        Verificado
                      </Badge>
                      <Button variant="outline" size="sm">
                        <Eye className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="billing" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Historial de Facturación</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {[1, 2, 3].map(i => (
                  <div
                    key={i}
                    className="flex items-center justify-between rounded-lg bg-slate-700/50 p-4"
                  >
                    <div className="flex items-center gap-4">
                      <div className="rounded-lg bg-slate-600 p-3">
                        <DollarSign className="h-5 w-5 text-gray-300" />
                      </div>
                      <div>
                        <p className="font-medium text-white">Plan Pro - Febrero 2024</p>
                        <p className="text-sm text-gray-400">INV-2024-00{i}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <p className="font-bold text-white">RD$ 12,900</p>
                      <Badge className="bg-emerald-600">Pagada</Badge>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
