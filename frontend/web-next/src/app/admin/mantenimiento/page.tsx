/**
 * Admin Maintenance Mode Page
 *
 * Enable/disable maintenance mode for the platform
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { AlertTriangle, Calendar, Clock, Settings, Users, Bell, CheckCircle } from 'lucide-react';

export default function AdminMaintenancePage() {
  const [maintenanceEnabled, setMaintenanceEnabled] = useState(false);
  const [scheduledMaintenance, setScheduledMaintenance] = useState(false);
  const [notifyUsers, setNotifyUsers] = useState(true);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Modo Mantenimiento</h1>
          <p className="text-gray-400">Gestiona el modo de mantenimiento de la plataforma</p>
        </div>
      </div>

      {/* Current Status */}
      <Card
        className={
          maintenanceEnabled ? 'border-amber-300 bg-amber-50' : 'border-emerald-200 bg-emerald-50'
        }
      >
        <CardContent className="p-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div
                className={`rounded-full p-3 ${maintenanceEnabled ? 'bg-amber-100' : 'bg-emerald-100'}`}
              >
                {maintenanceEnabled ? (
                  <AlertTriangle className="h-8 w-8 text-amber-600" />
                ) : (
                  <CheckCircle className="h-8 w-8 text-emerald-600" />
                )}
              </div>
              <div>
                <h2
                  className={`text-xl font-bold ${maintenanceEnabled ? 'text-amber-900' : 'text-emerald-900'}`}
                >
                  {maintenanceEnabled ? 'Modo Mantenimiento Activo' : 'Plataforma Operativa'}
                </h2>
                <p className={maintenanceEnabled ? 'text-amber-700' : 'text-emerald-700'}>
                  {maintenanceEnabled
                    ? 'Los usuarios ven la página de mantenimiento'
                    : 'Todo funciona con normalidad'}
                </p>
              </div>
            </div>
            <Switch checked={maintenanceEnabled} onCheckedChange={setMaintenanceEnabled} />
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Immediate Maintenance */}
        <Card>
          <CardHeader>
            <CardTitle>Mantenimiento Inmediato</CardTitle>
            <CardDescription>Activa el modo mantenimiento ahora mismo</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label>Mensaje para usuarios</Label>
              <Textarea
                placeholder="Estamos realizando mejoras en la plataforma. Volvemos pronto..."
                className="mt-2"
              />
            </div>
            <div>
              <Label>Duración estimada (minutos)</Label>
              <Input type="number" placeholder="30" className="mt-2" />
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Switch checked={notifyUsers} onCheckedChange={setNotifyUsers} />
                <Label>Notificar a usuarios activos</Label>
              </div>
            </div>
            <Button
              className={maintenanceEnabled ? '' : 'bg-amber-600 hover:bg-amber-700'}
              disabled={maintenanceEnabled}
            >
              <AlertTriangle className="mr-2 h-4 w-4" />
              {maintenanceEnabled ? 'Ya está activo' : 'Activar Mantenimiento'}
            </Button>
          </CardContent>
        </Card>

        {/* Scheduled Maintenance */}
        <Card>
          <CardHeader>
            <CardTitle>Programar Mantenimiento</CardTitle>
            <CardDescription>Programa una ventana de mantenimiento futura</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Fecha de inicio</Label>
                <Input type="date" className="mt-2" />
              </div>
              <div>
                <Label>Hora de inicio</Label>
                <Input type="time" className="mt-2" />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Fecha de fin</Label>
                <Input type="date" className="mt-2" />
              </div>
              <div>
                <Label>Hora de fin</Label>
                <Input type="time" className="mt-2" />
              </div>
            </div>
            <div>
              <Label>Descripción del mantenimiento</Label>
              <Textarea
                placeholder="Actualización de base de datos y mejoras de rendimiento..."
                className="mt-2"
              />
            </div>
            <Button variant="outline">
              <Calendar className="mr-2 h-4 w-4" />
              Programar
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Scheduled List */}
      <Card>
        <CardHeader>
          <CardTitle>Mantenimientos Programados</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="py-8 text-center text-gray-400">
            <Calendar className="mx-auto mb-4 h-12 w-12 opacity-50" />
            <p>No hay mantenimientos programados</p>
          </div>
        </CardContent>
      </Card>

      {/* History */}
      <Card>
        <CardHeader>
          <CardTitle>Historial de Mantenimientos</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="flex items-center justify-between rounded-lg bg-gray-50 p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-lg bg-emerald-100 p-2">
                  <CheckCircle className="h-5 w-5 text-emerald-600" />
                </div>
                <div>
                  <p className="font-medium">Actualización de seguridad</p>
                  <p className="text-sm text-gray-500">15 Feb 2024, 02:00 - 02:45</p>
                </div>
              </div>
              <Badge className="bg-emerald-100 text-emerald-700">Completado</Badge>
            </div>
            <div className="flex items-center justify-between rounded-lg bg-gray-50 p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-lg bg-emerald-100 p-2">
                  <CheckCircle className="h-5 w-5 text-emerald-600" />
                </div>
                <div>
                  <p className="font-medium">Migración de base de datos</p>
                  <p className="text-sm text-gray-500">10 Feb 2024, 03:00 - 05:30</p>
                </div>
              </div>
              <Badge className="bg-emerald-100 text-emerald-700">Completado</Badge>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Configuración
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Permitir acceso a admins durante mantenimiento</p>
              <p className="text-sm text-gray-500">
                Los administradores pueden acceder normalmente
              </p>
            </div>
            <Switch defaultChecked />
          </div>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Enviar notificación previa</p>
              <p className="text-sm text-gray-500">Notificar a usuarios 30 min antes</p>
            </div>
            <Switch defaultChecked />
          </div>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Banner de advertencia</p>
              <p className="text-sm text-gray-500">
                Mostrar banner antes del mantenimiento programado
              </p>
            </div>
            <Switch defaultChecked />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
