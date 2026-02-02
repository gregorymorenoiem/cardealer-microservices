/**
 * Settings Page
 *
 * General app settings and preferences
 */

'use client';

import * as React from 'react';
import {
  Settings,
  Bell,
  Globe,
  Moon,
  Sun,
  Monitor,
  Mail,
  MessageCircle,
  DollarSign,
  Eye,
  Loader2,
  Check,
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { cn } from '@/lib/utils';

interface NotificationSettings {
  email: {
    messages: boolean;
    inquiries: boolean;
    priceAlerts: boolean;
    newListings: boolean;
    marketing: boolean;
  };
  push: {
    messages: boolean;
    inquiries: boolean;
    priceAlerts: boolean;
  };
}

interface AppSettings {
  theme: 'light' | 'dark' | 'system';
  language: 'es' | 'en';
  currency: 'DOP' | 'USD';
}

type ThemeOption = 'light' | 'dark' | 'system';

function ThemeButton({
  theme,
  currentTheme,
  onClick,
  icon: Icon,
  label,
}: {
  theme: ThemeOption;
  currentTheme: ThemeOption;
  onClick: () => void;
  icon: React.ElementType;
  label: string;
}) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'flex flex-col items-center gap-2 rounded-lg border-2 p-4 transition-all',
        currentTheme === theme
          ? 'border-primary bg-primary/5'
          : 'border-gray-200 hover:border-gray-300'
      )}
    >
      <Icon className={cn('h-6 w-6', currentTheme === theme ? 'text-primary' : 'text-gray-600')} />
      <span
        className={cn(
          'text-sm font-medium',
          currentTheme === theme ? 'text-primary' : 'text-gray-700'
        )}
      >
        {label}
      </span>
    </button>
  );
}

function NotificationToggle({
  label,
  description,
  checked,
  onCheckedChange,
  icon: Icon,
}: {
  label: string;
  description: string;
  checked: boolean;
  onCheckedChange: (checked: boolean) => void;
  icon: React.ElementType;
}) {
  return (
    <div className="flex items-start gap-4 py-3">
      <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-100 text-gray-600">
        <Icon className="h-5 w-5" />
      </div>
      <div className="flex-1">
        <div className="flex items-center justify-between">
          <Label className="cursor-pointer font-medium">{label}</Label>
          <Switch checked={checked} onCheckedChange={onCheckedChange} />
        </div>
        <p className="mt-0.5 text-sm text-gray-600">{description}</p>
      </div>
    </div>
  );
}

export default function SettingsPage() {
  const [isSaving, setIsSaving] = React.useState(false);
  const [saveSuccess, setSaveSuccess] = React.useState(false);

  const [appSettings, setAppSettings] = React.useState<AppSettings>({
    theme: 'system',
    language: 'es',
    currency: 'DOP',
  });

  const [notifications, setNotifications] = React.useState<NotificationSettings>({
    email: {
      messages: true,
      inquiries: true,
      priceAlerts: true,
      newListings: false,
      marketing: false,
    },
    push: {
      messages: true,
      inquiries: true,
      priceAlerts: false,
    },
  });

  const handleSave = async () => {
    setIsSaving(true);
    try {
      // TODO: Save settings to API
      await new Promise(resolve => setTimeout(resolve, 1000));
      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (error) {
      console.error('Error saving settings:', error);
    } finally {
      setIsSaving(false);
    }
  };

  const updateEmailNotification = (key: keyof NotificationSettings['email'], value: boolean) => {
    setNotifications(prev => ({
      ...prev,
      email: { ...prev.email, [key]: value },
    }));
  };

  const updatePushNotification = (key: keyof NotificationSettings['push'], value: boolean) => {
    setNotifications(prev => ({
      ...prev,
      push: { ...prev.push, [key]: value },
    }));
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Configuración</h1>
          <p className="text-gray-600">Personaliza tu experiencia en OKLA</p>
        </div>
        <Button onClick={handleSave} disabled={isSaving}>
          {isSaving ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Guardando...
            </>
          ) : saveSuccess ? (
            <>
              <Check className="mr-2 h-4 w-4" />
              Guardado
            </>
          ) : (
            'Guardar cambios'
          )}
        </Button>
      </div>

      {/* Appearance */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Apariencia
          </CardTitle>
          <CardDescription>Personaliza cómo se ve la aplicación</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Theme */}
          <div>
            <Label className="mb-3 block text-sm font-medium">Tema</Label>
            <div className="grid grid-cols-3 gap-3">
              <ThemeButton
                theme="light"
                currentTheme={appSettings.theme}
                onClick={() => setAppSettings({ ...appSettings, theme: 'light' })}
                icon={Sun}
                label="Claro"
              />
              <ThemeButton
                theme="dark"
                currentTheme={appSettings.theme}
                onClick={() => setAppSettings({ ...appSettings, theme: 'dark' })}
                icon={Moon}
                label="Oscuro"
              />
              <ThemeButton
                theme="system"
                currentTheme={appSettings.theme}
                onClick={() => setAppSettings({ ...appSettings, theme: 'system' })}
                icon={Monitor}
                label="Sistema"
              />
            </div>
          </div>

          <Separator />

          {/* Language */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-100 text-gray-600">
                <Globe className="h-5 w-5" />
              </div>
              <div>
                <Label className="font-medium">Idioma</Label>
                <p className="text-sm text-gray-600">Selecciona tu idioma preferido</p>
              </div>
            </div>
            <select
              value={appSettings.language}
              onChange={e =>
                setAppSettings({ ...appSettings, language: e.target.value as 'es' | 'en' })
              }
              className="focus:ring-primary/20 focus:border-primary rounded-lg border border-gray-300 px-3 py-2 focus:ring-2 focus:outline-none"
            >
              <option value="es">Español</option>
              <option value="en">English</option>
            </select>
          </div>

          <Separator />

          {/* Currency */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-100 text-gray-600">
                <DollarSign className="h-5 w-5" />
              </div>
              <div>
                <Label className="font-medium">Moneda</Label>
                <p className="text-sm text-gray-600">Moneda para mostrar precios</p>
              </div>
            </div>
            <select
              value={appSettings.currency}
              onChange={e =>
                setAppSettings({ ...appSettings, currency: e.target.value as 'DOP' | 'USD' })
              }
              className="focus:ring-primary/20 focus:border-primary rounded-lg border border-gray-300 px-3 py-2 focus:ring-2 focus:outline-none"
            >
              <option value="DOP">RD$ (Pesos Dominicanos)</option>
              <option value="USD">US$ (Dólares)</option>
            </select>
          </div>
        </CardContent>
      </Card>

      {/* Email Notifications */}
      <Card id="notificaciones">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Mail className="h-5 w-5" />
            Notificaciones por Email
          </CardTitle>
          <CardDescription>Elige qué emails quieres recibir</CardDescription>
        </CardHeader>
        <CardContent className="divide-y divide-gray-100">
          <NotificationToggle
            label="Mensajes"
            description="Recibe un email cuando alguien te envíe un mensaje"
            checked={notifications.email.messages}
            onCheckedChange={v => updateEmailNotification('messages', v)}
            icon={MessageCircle}
          />
          <NotificationToggle
            label="Consultas sobre tus vehículos"
            description="Cuando alguien esté interesado en tus publicaciones"
            checked={notifications.email.inquiries}
            onCheckedChange={v => updateEmailNotification('inquiries', v)}
            icon={Eye}
          />
          <NotificationToggle
            label="Alertas de precio"
            description="Cuando un vehículo en favoritos baje de precio"
            checked={notifications.email.priceAlerts}
            onCheckedChange={v => updateEmailNotification('priceAlerts', v)}
            icon={DollarSign}
          />
          <NotificationToggle
            label="Nuevas publicaciones"
            description="Basado en tus búsquedas guardadas"
            checked={notifications.email.newListings}
            onCheckedChange={v => updateEmailNotification('newListings', v)}
            icon={Bell}
          />
          <NotificationToggle
            label="Ofertas y promociones"
            description="Información sobre ofertas exclusivas de OKLA"
            checked={notifications.email.marketing}
            onCheckedChange={v => updateEmailNotification('marketing', v)}
            icon={Mail}
          />
        </CardContent>
      </Card>

      {/* Push Notifications */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Bell className="h-5 w-5" />
            Notificaciones Push
          </CardTitle>
          <CardDescription>Notificaciones en tiempo real en tu dispositivo</CardDescription>
        </CardHeader>
        <CardContent className="divide-y divide-gray-100">
          <NotificationToggle
            label="Mensajes"
            description="Notificación inmediata de nuevos mensajes"
            checked={notifications.push.messages}
            onCheckedChange={v => updatePushNotification('messages', v)}
            icon={MessageCircle}
          />
          <NotificationToggle
            label="Consultas sobre tus vehículos"
            description="Cuando alguien esté interesado en tus publicaciones"
            checked={notifications.push.inquiries}
            onCheckedChange={v => updatePushNotification('inquiries', v)}
            icon={Eye}
          />
          <NotificationToggle
            label="Alertas de precio"
            description="Notificación cuando un favorito baje de precio"
            checked={notifications.push.priceAlerts}
            onCheckedChange={v => updatePushNotification('priceAlerts', v)}
            icon={DollarSign}
          />
        </CardContent>
      </Card>

      {/* Data & Privacy */}
      <Card>
        <CardHeader>
          <CardTitle>Datos y Privacidad</CardTitle>
          <CardDescription>Gestiona tus datos personales</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between py-2">
            <div>
              <p className="font-medium text-gray-900">Descargar mis datos</p>
              <p className="text-sm text-gray-600">Obtén una copia de toda tu información</p>
            </div>
            <Button variant="outline" size="sm">
              Solicitar
            </Button>
          </div>
          <Separator />
          <div className="flex items-center justify-between py-2">
            <div>
              <p className="font-medium text-gray-900">Política de privacidad</p>
              <p className="text-sm text-gray-600">Lee cómo protegemos tus datos</p>
            </div>
            <Button variant="link" size="sm" asChild>
              <a href="/privacidad" target="_blank">
                Ver
              </a>
            </Button>
          </div>
          <Separator />
          <div className="flex items-center justify-between py-2">
            <div>
              <p className="font-medium text-gray-900">Términos de servicio</p>
              <p className="text-sm text-gray-600">Condiciones de uso de OKLA</p>
            </div>
            <Button variant="link" size="sm" asChild>
              <a href="/terminos" target="_blank">
                Ver
              </a>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
