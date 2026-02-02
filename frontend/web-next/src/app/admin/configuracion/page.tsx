/**
 * Admin Configuration Page
 *
 * Platform-wide settings and configuration
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import {
  Settings,
  Globe,
  DollarSign,
  Mail,
  Bell,
  Shield,
  Image,
  Save,
  RefreshCw,
  AlertTriangle,
  Check,
} from 'lucide-react';

export default function AdminConfigurationPage() {
  const [saved, setSaved] = useState(false);

  const handleSave = () => {
    setSaved(true);
    setTimeout(() => setSaved(false), 3000);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Configuración</h1>
          <p className="text-gray-600">Configuración general de la plataforma</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline">
            <RefreshCw className="mr-2 h-4 w-4" />
            Restablecer
          </Button>
          <Button className="bg-emerald-600 hover:bg-emerald-700" onClick={handleSave}>
            {saved ? (
              <>
                <Check className="mr-2 h-4 w-4" />
                Guardado
              </>
            ) : (
              <>
                <Save className="mr-2 h-4 w-4" />
                Guardar Cambios
              </>
            )}
          </Button>
        </div>
      </div>

      {/* General Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Globe className="h-5 w-5" />
            General
          </CardTitle>
          <CardDescription>Configuración básica del sitio</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium">Nombre del Sitio</label>
              <Input defaultValue="OKLA" className="mt-1" />
            </div>
            <div>
              <label className="text-sm font-medium">URL del Sitio</label>
              <Input defaultValue="https://okla.com.do" className="mt-1" />
            </div>
          </div>
          <div>
            <label className="text-sm font-medium">Descripción del Sitio</label>
            <Textarea
              defaultValue="El marketplace #1 de vehículos en República Dominicana"
              className="mt-1"
            />
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium">Email de Contacto</label>
              <Input defaultValue="contacto@okla.com.do" className="mt-1" />
            </div>
            <div>
              <label className="text-sm font-medium">Teléfono de Soporte</label>
              <Input defaultValue="809-555-OKLA" className="mt-1" />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Pricing Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <DollarSign className="h-5 w-5" />
            Precios y Comisiones
          </CardTitle>
          <CardDescription>Configuración de precios de la plataforma</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <label className="text-sm font-medium">Publicación Básica</label>
              <div className="relative mt-1">
                <span className="absolute top-1/2 left-3 -translate-y-1/2 text-gray-500">RD$</span>
                <Input defaultValue="0" className="pl-12" />
              </div>
              <p className="mt-1 text-xs text-gray-500">Gratis para usuarios</p>
            </div>
            <div>
              <label className="text-sm font-medium">Publicación Destacada</label>
              <div className="relative mt-1">
                <span className="absolute top-1/2 left-3 -translate-y-1/2 text-gray-500">RD$</span>
                <Input defaultValue="1499" className="pl-12" />
              </div>
              <p className="mt-1 text-xs text-gray-500">Por 7 días</p>
            </div>
            <div>
              <label className="text-sm font-medium">Publicación Premium</label>
              <div className="relative mt-1">
                <span className="absolute top-1/2 left-3 -translate-y-1/2 text-gray-500">RD$</span>
                <Input defaultValue="2999" className="pl-12" />
              </div>
              <p className="mt-1 text-xs text-gray-500">Por 30 días</p>
            </div>
          </div>

          <div className="mt-4 border-t pt-4">
            <h4 className="mb-3 font-medium">Planes de Dealers</h4>
            <div className="grid gap-4 md:grid-cols-3">
              <div>
                <label className="text-sm font-medium">Plan Starter</label>
                <div className="relative mt-1">
                  <span className="absolute top-1/2 left-3 -translate-y-1/2 text-gray-500">
                    RD$
                  </span>
                  <Input defaultValue="4999" className="pl-12" />
                </div>
                <p className="mt-1 text-xs text-gray-500">Mensual - 20 vehículos</p>
              </div>
              <div>
                <label className="text-sm font-medium">Plan Pro</label>
                <div className="relative mt-1">
                  <span className="absolute top-1/2 left-3 -translate-y-1/2 text-gray-500">
                    RD$
                  </span>
                  <Input defaultValue="24999" className="pl-12" />
                </div>
                <p className="mt-1 text-xs text-gray-500">Mensual - 50 vehículos</p>
              </div>
              <div>
                <label className="text-sm font-medium">Plan Enterprise</label>
                <div className="relative mt-1">
                  <span className="absolute top-1/2 left-3 -translate-y-1/2 text-gray-500">
                    RD$
                  </span>
                  <Input defaultValue="49999" className="pl-12" />
                </div>
                <p className="mt-1 text-xs text-gray-500">Mensual - Ilimitado</p>
              </div>
            </div>
          </div>

          <div className="mt-4 border-t pt-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <label className="text-sm font-medium">ITBIS (%)</label>
                <Input defaultValue="18" className="mt-1" type="number" />
              </div>
              <div>
                <label className="text-sm font-medium">Moneda</label>
                <select className="mt-1 w-full rounded-lg border px-3 py-2">
                  <option value="DOP">RD$ (Peso Dominicano)</option>
                  <option value="USD">$ (Dólar US)</option>
                </select>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Email Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Mail className="h-5 w-5" />
            Configuración de Email
          </CardTitle>
          <CardDescription>Servidor SMTP y plantillas</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium">Servidor SMTP</label>
              <Input defaultValue="smtp.sendgrid.net" className="mt-1" />
            </div>
            <div>
              <label className="text-sm font-medium">Puerto</label>
              <Input defaultValue="587" className="mt-1" type="number" />
            </div>
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium">Usuario SMTP</label>
              <Input defaultValue="apikey" className="mt-1" />
            </div>
            <div>
              <label className="text-sm font-medium">Contraseña SMTP</label>
              <Input defaultValue="••••••••••••••••" className="mt-1" type="password" />
            </div>
          </div>
          <div>
            <label className="text-sm font-medium">Email del Remitente</label>
            <Input defaultValue="no-reply@okla.com.do" className="mt-1" />
          </div>
        </CardContent>
      </Card>

      {/* Notification Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Bell className="h-5 w-5" />
            Notificaciones
          </CardTitle>
          <CardDescription>Configuración de alertas del sistema</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-3">
            {[
              { label: 'Nuevo usuario registrado', enabled: true },
              { label: 'Nueva publicación pendiente', enabled: true },
              { label: 'Nuevo dealer registrado', enabled: true },
              { label: 'Reporte de usuario', enabled: true },
              { label: 'Pago fallido', enabled: true },
              { label: 'Resumen diario', enabled: false },
            ].map(item => (
              <label
                key={item.label}
                className="flex cursor-pointer items-center justify-between rounded-lg p-3 hover:bg-gray-50"
              >
                <span className="font-medium">{item.label}</span>
                <input type="checkbox" defaultChecked={item.enabled} className="rounded" />
              </label>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Security Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            Seguridad
          </CardTitle>
          <CardDescription>Configuración de seguridad y autenticación</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium">Intentos de login máximos</label>
              <Input defaultValue="5" className="mt-1" type="number" />
            </div>
            <div>
              <label className="text-sm font-medium">Tiempo de bloqueo (minutos)</label>
              <Input defaultValue="15" className="mt-1" type="number" />
            </div>
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium">Expiración de sesión (horas)</label>
              <Input defaultValue="24" className="mt-1" type="number" />
            </div>
            <div>
              <label className="text-sm font-medium">Largo mínimo de contraseña</label>
              <Input defaultValue="8" className="mt-1" type="number" />
            </div>
          </div>
          <div className="space-y-3 border-t pt-4">
            <label className="flex cursor-pointer items-center justify-between rounded-lg p-3 hover:bg-gray-50">
              <span className="font-medium">Requerir verificación de email</span>
              <input type="checkbox" defaultChecked className="rounded" />
            </label>
            <label className="flex cursor-pointer items-center justify-between rounded-lg p-3 hover:bg-gray-50">
              <span className="font-medium">Permitir 2FA</span>
              <input type="checkbox" defaultChecked className="rounded" />
            </label>
            <label className="flex cursor-pointer items-center justify-between rounded-lg p-3 hover:bg-gray-50">
              <span className="font-medium">Forzar HTTPS</span>
              <input type="checkbox" defaultChecked className="rounded" />
            </label>
          </div>
        </CardContent>
      </Card>

      {/* Maintenance Mode */}
      <Card className="border-amber-200">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-amber-700">
            <AlertTriangle className="h-5 w-5" />
            Modo Mantenimiento
          </CardTitle>
          <CardDescription>
            Activar el modo mantenimiento deshabilitará el acceso público
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between rounded-lg bg-amber-50 p-4">
            <div>
              <p className="font-medium">Estado del Modo Mantenimiento</p>
              <p className="text-sm text-gray-600">El sitio está actualmente en línea</p>
            </div>
            <Button variant="outline" className="border-amber-500 text-amber-700 hover:bg-amber-50">
              Activar Mantenimiento
            </Button>
          </div>
          <div>
            <label className="text-sm font-medium">Mensaje de Mantenimiento</label>
            <Textarea
              defaultValue="Estamos realizando mejoras en el sitio. Volveremos pronto."
              className="mt-1"
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
