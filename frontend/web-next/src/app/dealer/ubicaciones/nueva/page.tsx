/**
 * Dealer New Location Page
 *
 * Add a new branch/location to the dealer account
 */

'use client';

import { useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ArrowLeft, MapPin, Clock, Phone, Mail, Building2 } from 'lucide-react';
import Link from 'next/link';

const provinces = [
  'Distrito Nacional',
  'Santo Domingo',
  'Santiago',
  'La Vega',
  'Puerto Plata',
  'San Cristóbal',
  'La Romana',
  'San Pedro de Macorís',
];

const days = [
  { id: 'mon', label: 'Lunes' },
  { id: 'tue', label: 'Martes' },
  { id: 'wed', label: 'Miércoles' },
  { id: 'thu', label: 'Jueves' },
  { id: 'fri', label: 'Viernes' },
  { id: 'sat', label: 'Sábado' },
  { id: 'sun', label: 'Domingo' },
];

export default function NewLocationPage() {
  const router = useRouter();

  return (
    <div className="max-w-3xl space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/dealer/ubicaciones">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-5 w-5" />
          </Button>
        </Link>
        <div>
          <h1 className="text-2xl font-bold">Nueva Ubicación</h1>
          <p className="text-gray-600">Agrega una nueva sucursal o punto de venta</p>
        </div>
      </div>

      {/* Form */}
      <div className="space-y-6">
        {/* Basic Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Información Básica
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label>Nombre de la Sucursal *</Label>
              <Input className="mt-2" placeholder="Ej: Sucursal Santiago" />
            </div>
            <div>
              <Label>Tipo de Ubicación *</Label>
              <Select>
                <SelectTrigger className="mt-2">
                  <SelectValue placeholder="Seleccionar tipo" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="showroom">Showroom / Sala de Ventas</SelectItem>
                  <SelectItem value="service">Centro de Servicio</SelectItem>
                  <SelectItem value="warehouse">Almacén</SelectItem>
                  <SelectItem value="office">Oficina Administrativa</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center justify-between rounded-lg bg-gray-50 p-4">
              <div>
                <p className="font-medium">Establecer como ubicación principal</p>
                <p className="text-sm text-gray-500">Esta será la ubicación por defecto</p>
              </div>
              <Switch />
            </div>
          </CardContent>
        </Card>

        {/* Address */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MapPin className="h-5 w-5" />
              Dirección
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label>Dirección *</Label>
              <Textarea className="mt-2" placeholder="Calle, número, sector" rows={2} />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Provincia *</Label>
                <Select>
                  <SelectTrigger className="mt-2">
                    <SelectValue placeholder="Seleccionar provincia" />
                  </SelectTrigger>
                  <SelectContent>
                    {provinces.map(prov => (
                      <SelectItem key={prov} value={prov}>
                        {prov}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Label>Ciudad *</Label>
                <Input className="mt-2" placeholder="Ciudad" />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Código Postal</Label>
                <Input className="mt-2" placeholder="00000" />
              </div>
              <div>
                <Label>Referencia</Label>
                <Input className="mt-2" placeholder="Cerca de..." />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Contact */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Phone className="h-5 w-5" />
              Contacto
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Teléfono Principal *</Label>
                <div className="relative mt-2">
                  <Phone className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
                  <Input className="pl-10" placeholder="809-000-0000" />
                </div>
              </div>
              <div>
                <Label>Teléfono Secundario</Label>
                <div className="relative mt-2">
                  <Phone className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
                  <Input className="pl-10" placeholder="809-000-0000" />
                </div>
              </div>
            </div>
            <div>
              <Label>Email de Contacto</Label>
              <div className="relative mt-2">
                <Mail className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <Input className="pl-10" type="email" placeholder="sucursal@dealer.com" />
              </div>
            </div>
            <div>
              <Label>WhatsApp</Label>
              <Input className="mt-2" placeholder="809-000-0000" />
            </div>
          </CardContent>
        </Card>

        {/* Hours */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5" />
              Horario de Atención
            </CardTitle>
            <CardDescription>Define los horarios de operación para esta ubicación</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {days.map(day => (
                <div key={day.id} className="flex items-center gap-4 rounded-lg bg-gray-50 p-3">
                  <div className="w-24">
                    <span className="font-medium">{day.label}</span>
                  </div>
                  <Switch />
                  <div className="flex flex-1 items-center gap-2">
                    <Input type="time" className="w-32" defaultValue="08:00" />
                    <span className="text-gray-400">a</span>
                    <Input type="time" className="w-32" defaultValue="18:00" />
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Actions */}
        <div className="flex justify-end gap-4">
          <Button variant="outline" onClick={() => router.back()}>
            Cancelar
          </Button>
          <Button className="bg-emerald-600 hover:bg-emerald-700">Guardar Ubicación</Button>
        </div>
      </div>
    </div>
  );
}
