/**
 * Dealer Settings Page
 *
 * Configure dealer account settings
 */

'use client';

import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Settings,
  Bell,
  Shield,
  Users,
  Mail,
  MessageSquare,
  Smartphone,
  Globe,
  Lock,
  Key,
  AlertTriangle,
  Check,
  Loader2,
} from 'lucide-react';
import { useCurrentDealer } from '@/hooks/use-dealers';
import { useDealerEmployees } from '@/hooks/use-dealer-employees';
import {
  useDealerSettings,
  useUpdateNotificationSettings,
  useUpdateSecuritySettings,
} from '@/hooks/use-dealer-settings';
import {
  defaultNotificationSettings,
  defaultSecuritySettings,
  sessionTimeoutOptions,
  type NotificationSettings,
} from '@/services/dealer-settings';
import { toast } from 'sonner';

// ============================================================================
// Loading Skeleton
// ============================================================================

function SettingsSkeleton() {
  return (
    <div className="space-y-6">
      <div>
        <Skeleton className="mb-2 h-8 w-40" />
        <Skeleton className="h-4 w-60" />
      </div>
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Card>
            <CardHeader>
              <Skeleton className="h-6 w-32" />
            </CardHeader>
            <CardContent className="space-y-6">
              {[1, 2, 3, 4].map(i => (
                <div key={i} className="flex items-center justify-between">
                  <div>
                    <Skeleton className="mb-1 h-4 w-32" />
                    <Skeleton className="h-3 w-48" />
                  </div>
                  <Skeleton className="h-6 w-10" />
                </div>
              ))}
            </CardContent>
          </Card>
        </div>
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <Skeleton className="h-6 w-28" />
            </CardHeader>
            <CardContent className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

export default function DealerSettingsPage() {
  // Get current dealer
  const { data: dealer, isLoading: dealerLoading } = useCurrentDealer();
  const dealerId = dealer?.id || '';

  // Get settings
  const { data: settings, isLoading: settingsLoading } = useDealerSettings(dealerId);

  // Get employees for team section
  const { data: employees } = useDealerEmployees(dealerId);

  // Mutations
  const updateNotifications = useUpdateNotificationSettings(dealerId);
  const updateSecurity = useUpdateSecuritySettings(dealerId);

  // Local state for form
  const [notifications, setNotifications] = useState<NotificationSettings>(
    defaultNotificationSettings
  );
  const [sessionTimeout, setSessionTimeout] = useState(30);
  const [hasChanges, setHasChanges] = useState(false);

  // Sync settings when loaded
  useEffect(() => {
    if (settings) {
      setNotifications(settings.notifications);
      setSessionTimeout(settings.security.sessionTimeoutMinutes);
    }
  }, [settings]);

  // Track changes
  useEffect(() => {
    if (settings) {
      const notifChanged = JSON.stringify(notifications) !== JSON.stringify(settings.notifications);
      const securityChanged = sessionTimeout !== settings.security.sessionTimeoutMinutes;
      setHasChanges(notifChanged || securityChanged);
    }
  }, [notifications, sessionTimeout, settings]);

  const isLoading = dealerLoading || settingsLoading;

  // Handle save
  const handleSave = async () => {
    try {
      await Promise.all([
        updateNotifications.mutateAsync(notifications),
        updateSecurity.mutateAsync({ sessionTimeoutMinutes: sessionTimeout }),
      ]);
      toast.success('Configuración guardada');
      setHasChanges(false);
    } catch {
      toast.error('Error al guardar la configuración');
    }
  };

  if (isLoading) {
    return <SettingsSkeleton />;
  }

  const isSaving = updateNotifications.isPending || updateSecurity.isPending;
  const twoFactorEnabled = settings?.security.twoFactorEnabled || false;
  const activeEmployees = employees?.filter(e => e.status === 'Active') || [];
  const maxEmployees = Math.floor((dealer?.maxActiveListings || 15) / 3);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Configuración</h1>
          <p className="text-gray-600">Personaliza tu experiencia en OKLA</p>
        </div>
        {hasChanges && (
          <Button
            onClick={handleSave}
            disabled={isSaving}
            className="bg-emerald-600 hover:bg-emerald-700"
          >
            {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Guardar Cambios
          </Button>
        )}
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Main Settings */}
        <div className="space-y-6 lg:col-span-2">
          {/* Notifications */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Bell className="h-5 w-5" />
                Notificaciones
              </CardTitle>
              <CardDescription>Configura cómo quieres recibir alertas</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Email */}
              <div>
                <h4 className="mb-4 flex items-center gap-2 font-medium">
                  <Mail className="h-4 w-4" />
                  Email
                </h4>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Nuevos leads</p>
                      <p className="text-sm text-gray-500">
                        Recibe un email cuando alguien contacte
                      </p>
                    </div>
                    <Switch
                      checked={notifications.emailNewLead}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, emailNewLead: checked }))
                      }
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Mensajes</p>
                      <p className="text-sm text-gray-500">Notificación de nuevos mensajes</p>
                    </div>
                    <Switch
                      checked={notifications.emailMessages}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, emailMessages: checked }))
                      }
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Citas programadas</p>
                      <p className="text-sm text-gray-500">Recordatorios de test drives</p>
                    </div>
                    <Switch
                      checked={notifications.emailAppointments}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, emailAppointments: checked }))
                      }
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Reporte semanal</p>
                      <p className="text-sm text-gray-500">Resumen de actividad cada lunes</p>
                    </div>
                    <Switch
                      checked={notifications.emailWeeklyReport}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, emailWeeklyReport: checked }))
                      }
                    />
                  </div>
                </div>
              </div>

              <hr />

              {/* SMS */}
              <div>
                <h4 className="mb-4 flex items-center gap-2 font-medium">
                  <Smartphone className="h-4 w-4" />
                  SMS
                </h4>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Nuevos leads urgentes</p>
                      <p className="text-sm text-gray-500">Solo leads de alta prioridad</p>
                    </div>
                    <Switch
                      checked={notifications.smsNewLead}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, smsNewLead: checked }))
                      }
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Recordatorio de citas</p>
                      <p className="text-sm text-gray-500">1 hora antes de cada cita</p>
                    </div>
                    <Switch
                      checked={notifications.smsAppointments}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, smsAppointments: checked }))
                      }
                    />
                  </div>
                </div>
              </div>

              <hr />

              {/* Push */}
              <div>
                <h4 className="mb-4 flex items-center gap-2 font-medium">
                  <MessageSquare className="h-4 w-4" />
                  Notificaciones Push
                </h4>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Mensajes</p>
                      <p className="text-sm text-gray-500">Notificación instantánea</p>
                    </div>
                    <Switch
                      checked={notifications.pushMessages}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, pushMessages: checked }))
                      }
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Nuevos leads</p>
                      <p className="text-sm text-gray-500">Alerta inmediata de contactos</p>
                    </div>
                    <Switch
                      checked={notifications.pushLeads}
                      onCheckedChange={checked =>
                        setNotifications(prev => ({ ...prev, pushLeads: checked }))
                      }
                    />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Security */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Shield className="h-5 w-5" />
                Seguridad
              </CardTitle>
              <CardDescription>Protege tu cuenta</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium">Autenticación de dos factores</p>
                  <p className="text-sm text-gray-500">Añade una capa extra de seguridad</p>
                </div>
                <div className="flex items-center gap-3">
                  {twoFactorEnabled ? (
                    <Badge className="bg-emerald-100 text-emerald-700">
                      <Check className="mr-1 h-3 w-3" />
                      Activo
                    </Badge>
                  ) : (
                    <Badge variant="outline">Desactivado</Badge>
                  )}
                  <Button variant="outline" size="sm">
                    {twoFactorEnabled ? 'Configurar' : 'Activar'}
                  </Button>
                </div>
              </div>

              <div>
                <Label>Tiempo de sesión inactiva</Label>
                <select
                  className="mt-2 w-full rounded-md border p-2"
                  value={sessionTimeout}
                  onChange={e => setSessionTimeout(Number(e.target.value))}
                >
                  {sessionTimeoutOptions.map(opt => (
                    <option key={opt.value} value={opt.value}>
                      {opt.label}
                    </option>
                  ))}
                </select>
                <p className="mt-1 text-xs text-gray-500">
                  Cierra sesión automáticamente después de inactividad
                </p>
              </div>

              <div>
                <Button variant="outline" className="w-full">
                  <Key className="mr-2 h-4 w-4" />
                  Cambiar Contraseña
                </Button>
              </div>

              <div className="rounded-lg border border-amber-200 bg-amber-50 p-4">
                <div className="flex items-start gap-3">
                  <AlertTriangle className="mt-0.5 h-5 w-5 flex-shrink-0 text-amber-600" />
                  <div>
                    <p className="font-medium text-amber-800">Sesiones activas</p>
                    <p className="text-sm text-amber-600">
                      Tienes 3 sesiones activas en diferentes dispositivos
                    </p>
                    <Button variant="link" className="h-auto p-0 text-amber-700">
                      Ver y cerrar sesiones
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Team */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="h-5 w-5" />
                Equipo
              </CardTitle>
              <CardDescription>Gestiona accesos de empleados</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {activeEmployees.slice(0, 3).map(employee => (
                  <div
                    key={employee.id}
                    className="flex items-center justify-between rounded-lg border p-3"
                  >
                    <div className="flex items-center gap-3">
                      {employee.avatarUrl ? (
                        <img
                          src={employee.avatarUrl}
                          alt={employee.name}
                          className="h-10 w-10 rounded-full object-cover"
                        />
                      ) : (
                        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-emerald-100 font-medium text-emerald-700">
                          {employee.name
                            .split(' ')
                            .map(n => n[0])
                            .join('')
                            .slice(0, 2)}
                        </div>
                      )}
                      <div>
                        <p className="font-medium">{employee.name}</p>
                        <p className="text-sm text-gray-500">{employee.email}</p>
                      </div>
                    </div>
                    <Badge variant={employee.role === 'Owner' ? 'default' : 'outline'}>
                      {employee.role}
                    </Badge>
                  </div>
                ))}
                {activeEmployees.length === 0 && (
                  <p className="py-4 text-center text-sm text-gray-500">
                    No hay empleados registrados
                  </p>
                )}
              </div>
              <Button variant="outline" className="mt-4 w-full">
                <Users className="mr-2 h-4 w-4" />
                Invitar Empleado
              </Button>
              <p className="mt-2 text-center text-xs text-gray-500">
                Plan {dealer?.plan || 'Pro'}: {activeEmployees.length} de {maxEmployees} empleados
                utilizados
              </p>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Quick Links */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Acceso Rápido</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <Button variant="ghost" className="w-full justify-start">
                <Globe className="mr-2 h-4 w-4" />
                Ver Perfil Público
              </Button>
              <Button variant="ghost" className="w-full justify-start">
                <Settings className="mr-2 h-4 w-4" />
                Editar Perfil
              </Button>
              <Button variant="ghost" className="w-full justify-start">
                <Lock className="mr-2 h-4 w-4" />
                Privacidad
              </Button>
            </CardContent>
          </Card>

          {/* Account Status */}
          {dealer && dealer.verificationStatus === 'verified' && (
            <Card className="border-emerald-200 bg-emerald-50">
              <CardContent className="p-4">
                <div className="mb-3 flex items-center gap-3">
                  <div className="rounded-full bg-emerald-100 p-2">
                    <Check className="h-5 w-5 text-emerald-600" />
                  </div>
                  <div>
                    <p className="font-medium text-emerald-800">Cuenta Verificada</p>
                    <p className="text-sm text-emerald-600">Dealer oficial</p>
                  </div>
                </div>
                <div className="space-y-1 text-sm text-emerald-700">
                  <p>✓ Documentos verificados</p>
                  <p>✓ Identidad confirmada</p>
                  {dealer.rnc && <p>✓ RNC validado</p>}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Danger Zone */}
          <Card className="border-red-200">
            <CardHeader>
              <CardTitle className="text-lg text-red-600">Zona de Peligro</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <Button
                variant="outline"
                className="w-full border-red-200 text-red-600 hover:bg-red-50"
              >
                Pausar Cuenta
              </Button>
              <Button
                variant="outline"
                className="w-full border-red-200 text-red-600 hover:bg-red-50"
              >
                Eliminar Cuenta
              </Button>
              <p className="text-xs text-gray-500">
                Eliminar tu cuenta es permanente y no se puede deshacer.
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
