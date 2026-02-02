/**
 * Dealer New Employee Page
 *
 * Add a new staff member to the dealer account
 */

'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ArrowLeft, User, Mail, Phone, Shield, Building2 } from 'lucide-react';
import Link from 'next/link';

const permissions = [
  { id: 'inventory_view', label: 'Ver inventario', description: 'Puede ver todos los vehículos' },
  {
    id: 'inventory_edit',
    label: 'Editar inventario',
    description: 'Puede agregar y editar vehículos',
  },
  { id: 'leads_view', label: 'Ver leads', description: 'Puede ver consultas de clientes' },
  { id: 'leads_respond', label: 'Responder leads', description: 'Puede responder a clientes' },
  { id: 'analytics_view', label: 'Ver analytics', description: 'Acceso a reportes y métricas' },
  { id: 'billing_view', label: 'Ver facturación', description: 'Puede ver facturas y pagos' },
  {
    id: 'employees_manage',
    label: 'Gestionar empleados',
    description: 'Puede agregar/editar empleados',
  },
  {
    id: 'settings_edit',
    label: 'Editar configuración',
    description: 'Puede cambiar configuraciones',
  },
];

const locations = [
  { id: '1', name: 'Sucursal Principal - Santo Domingo' },
  { id: '2', name: 'Sucursal Santiago' },
];

export default function NewEmployeePage() {
  const router = useRouter();
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([
    'inventory_view',
    'leads_view',
    'leads_respond',
  ]);

  const togglePermission = (id: string) => {
    setSelectedPermissions(prev =>
      prev.includes(id) ? prev.filter(p => p !== id) : [...prev, id]
    );
  };

  const selectRolePreset = (role: string) => {
    switch (role) {
      case 'admin':
        setSelectedPermissions(permissions.map(p => p.id));
        break;
      case 'sales':
        setSelectedPermissions(['inventory_view', 'leads_view', 'leads_respond']);
        break;
      case 'support':
        setSelectedPermissions(['leads_view', 'leads_respond']);
        break;
      default:
        setSelectedPermissions([]);
    }
  };

  return (
    <div className="max-w-3xl space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/dealer/empleados">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-5 w-5" />
          </Button>
        </Link>
        <div>
          <h1 className="text-2xl font-bold">Agregar Empleado</h1>
          <p className="text-gray-600">Invita a un nuevo miembro del equipo</p>
        </div>
      </div>

      {/* Form */}
      <div className="space-y-6">
        {/* Basic Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              Información Personal
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Nombre *</Label>
                <Input className="mt-2" placeholder="Nombre" />
              </div>
              <div>
                <Label>Apellido *</Label>
                <Input className="mt-2" placeholder="Apellido" />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Email *</Label>
                <div className="relative mt-2">
                  <Mail className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
                  <Input className="pl-10" type="email" placeholder="email@ejemplo.com" />
                </div>
              </div>
              <div>
                <Label>Teléfono</Label>
                <div className="relative mt-2">
                  <Phone className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
                  <Input className="pl-10" placeholder="809-000-0000" />
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Role & Location */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Rol y Ubicación
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Rol *</Label>
                <Select onValueChange={selectRolePreset}>
                  <SelectTrigger className="mt-2">
                    <SelectValue placeholder="Seleccionar rol" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="admin">Administrador</SelectItem>
                    <SelectItem value="sales">Ventas</SelectItem>
                    <SelectItem value="support">Soporte</SelectItem>
                    <SelectItem value="custom">Personalizado</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Label>Ubicación *</Label>
                <Select>
                  <SelectTrigger className="mt-2">
                    <SelectValue placeholder="Seleccionar ubicación" />
                  </SelectTrigger>
                  <SelectContent>
                    {locations.map(loc => (
                      <SelectItem key={loc.id} value={loc.id}>
                        {loc.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div>
              <Label>Cargo/Título</Label>
              <Input className="mt-2" placeholder="Ej: Ejecutivo de Ventas" />
            </div>
          </CardContent>
        </Card>

        {/* Permissions */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Permisos
            </CardTitle>
            <CardDescription>Selecciona los permisos que tendrá este empleado</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {permissions.map(permission => (
                <div
                  key={permission.id}
                  className="flex items-start space-x-3 rounded-lg bg-gray-50 p-3"
                >
                  <Checkbox
                    id={permission.id}
                    checked={selectedPermissions.includes(permission.id)}
                    onCheckedChange={() => togglePermission(permission.id)}
                  />
                  <div className="flex-1">
                    <label htmlFor={permission.id} className="cursor-pointer font-medium">
                      {permission.label}
                    </label>
                    <p className="text-sm text-gray-500">{permission.description}</p>
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
          <Button className="bg-emerald-600 hover:bg-emerald-700">
            <Mail className="mr-2 h-4 w-4" />
            Enviar Invitación
          </Button>
        </div>
      </div>
    </div>
  );
}
