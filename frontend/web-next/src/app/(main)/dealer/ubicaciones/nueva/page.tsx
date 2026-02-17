/**
 * Dealer New Location Page
 *
 * Add a new branch/location to the dealer account.
 * Connected to real API via useAddDealerLocation hook.
 */

'use client';

import { useState, useCallback } from 'react';
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
import {
  ArrowLeft,
  MapPin,
  Clock,
  Phone,
  Mail,
  Building2,
  Loader2,
  CheckCircle2,
} from 'lucide-react';
import Link from 'next/link';
import { useCurrentDealer } from '@/hooks/use-dealers';
import { useAddDealerLocation } from '@/hooks/use-dealers';
import { sanitizeText, sanitizeEmail, sanitizePhone } from '@/lib/security/sanitize';
import type { DealerLocationDto, BusinessHours, DayHours } from '@/services/dealers';

const provinces = [
  'Distrito Nacional',
  'Santo Domingo',
  'Santiago',
  'La Vega',
  'Puerto Plata',
  'San Cristóbal',
  'La Romana',
  'San Pedro de Macorís',
  'Duarte',
  'Espaillat',
  'Azua',
  'Barahona',
  'Monte Plata',
  'Peravia',
  'Valverde',
  'María Trinidad Sánchez',
  'Samaná',
  'Sánchez Ramírez',
  'Monseñor Nouel',
  'La Altagracia',
  'El Seibo',
  'Hato Mayor',
];

const locationTypes: { value: DealerLocationDto['locationType']; label: string }[] = [
  { value: 'showroom', label: 'Showroom / Sala de Ventas' },
  { value: 'serviceCenter', label: 'Centro de Servicio' },
  { value: 'warehouse', label: 'Almacén' },
  { value: 'headquarters', label: 'Sede Principal / Oficina' },
  { value: 'branch', label: 'Sucursal' },
];

type DayKey = keyof BusinessHours;

const days: { key: DayKey; label: string }[] = [
  { key: 'monday', label: 'Lunes' },
  { key: 'tuesday', label: 'Martes' },
  { key: 'wednesday', label: 'Miércoles' },
  { key: 'thursday', label: 'Jueves' },
  { key: 'friday', label: 'Viernes' },
  { key: 'saturday', label: 'Sábado' },
  { key: 'sunday', label: 'Domingo' },
];

interface HoursState {
  enabled: boolean;
  open: string;
  close: string;
}

const defaultHours: Record<DayKey, HoursState> = {
  monday: { enabled: true, open: '08:00', close: '18:00' },
  tuesday: { enabled: true, open: '08:00', close: '18:00' },
  wednesday: { enabled: true, open: '08:00', close: '18:00' },
  thursday: { enabled: true, open: '08:00', close: '18:00' },
  friday: { enabled: true, open: '08:00', close: '18:00' },
  saturday: { enabled: true, open: '08:00', close: '13:00' },
  sunday: { enabled: false, open: '08:00', close: '13:00' },
};

export default function NewLocationPage() {
  const router = useRouter();
  const { data: dealer } = useCurrentDealer();
  const dealerId = dealer?.id;

  const addLocation = useAddDealerLocation(dealerId || '');

  // Form state
  const [name, setName] = useState('');
  const [locationType, setLocationType] = useState<DealerLocationDto['locationType'] | ''>('');
  const [isPrimary, setIsPrimary] = useState(false);
  const [address, setAddress] = useState('');
  const [province, setProvince] = useState('');
  const [city, setCity] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [hours, setHours] = useState<Record<DayKey, HoursState>>(defaultHours);

  const updateHours = useCallback(
    (day: DayKey, field: keyof HoursState, value: string | boolean) => {
      setHours(prev => ({
        ...prev,
        [day]: { ...prev[day], [field]: value },
      }));
    },
    []
  );

  const buildBusinessHours = useCallback((): BusinessHours => {
    const bh: BusinessHours = {};
    for (const day of days) {
      const h = hours[day.key];
      const dayHours: DayHours = h.enabled
        ? { open: h.open, close: h.close, isClosed: false }
        : { open: '', close: '', isClosed: true };
      bh[day.key] = dayHours;
    }
    return bh;
  }, [hours]);

  const isFormValid = name.trim() && locationType && address.trim() && province && city.trim();

  const handleSubmit = useCallback(() => {
    if (!dealerId || !isFormValid || !locationType) return;

    const payload: Omit<DealerLocationDto, 'id' | 'dealerId'> = {
      name: sanitizeText(name.trim(), { maxLength: 100 }),
      locationType: locationType,
      isPrimary,
      address: sanitizeText(address.trim(), { maxLength: 300 }),
      province,
      city: sanitizeText(city.trim(), { maxLength: 100 }),
      phone: phone ? sanitizePhone(phone) : undefined,
      email: email ? sanitizeEmail(email) : undefined,
      businessHours: buildBusinessHours(),
    };

    addLocation.mutate(payload, {
      onSuccess: () => {
        router.push('/dealer/ubicaciones');
      },
    });
  }, [
    dealerId,
    isFormValid,
    locationType,
    name,
    isPrimary,
    address,
    province,
    city,
    phone,
    email,
    buildBusinessHours,
    addLocation,
    router,
  ]);

  if (!dealerId) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="text-muted-foreground h-8 w-8 animate-spin" />
      </div>
    );
  }

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
          <p className="text-muted-foreground">Agrega una nueva sucursal o punto de venta</p>
        </div>
      </div>

      {/* Success message */}
      {addLocation.isSuccess && (
        <div className="flex items-center gap-2 rounded-lg border border-primary bg-primary/10 p-4 text-primary">
          <CheckCircle2 className="h-5 w-5" />
          <span>Ubicación creada exitosamente. Redirigiendo...</span>
        </div>
      )}

      {/* Error message */}
      {addLocation.isError && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">
          Error al crear la ubicación: {(addLocation.error as Error)?.message || 'Intenta de nuevo'}
        </div>
      )}

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
              <Input
                className="mt-2"
                placeholder="Ej: Sucursal Santiago"
                value={name}
                onChange={e => setName(e.target.value)}
                maxLength={100}
              />
            </div>
            <div>
              <Label>Tipo de Ubicación *</Label>
              <Select
                value={locationType}
                onValueChange={v => setLocationType(v as DealerLocationDto['locationType'])}
              >
                <SelectTrigger className="mt-2">
                  <SelectValue placeholder="Seleccionar tipo" />
                </SelectTrigger>
                <SelectContent>
                  {locationTypes.map(lt => (
                    <SelectItem key={lt.value} value={lt.value}>
                      {lt.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="bg-muted/50 flex items-center justify-between rounded-lg p-4">
              <div>
                <p className="font-medium">Establecer como ubicación principal</p>
                <p className="text-muted-foreground text-sm">Esta será la ubicación por defecto</p>
              </div>
              <Switch checked={isPrimary} onCheckedChange={setIsPrimary} />
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
              <Textarea
                className="mt-2"
                placeholder="Calle, número, sector"
                rows={2}
                value={address}
                onChange={e => setAddress(e.target.value)}
                maxLength={300}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Provincia *</Label>
                <Select value={province} onValueChange={setProvince}>
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
                <Input
                  className="mt-2"
                  placeholder="Ciudad"
                  value={city}
                  onChange={e => setCity(e.target.value)}
                  maxLength={100}
                />
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
                <Label>Teléfono</Label>
                <div className="relative mt-2">
                  <Phone className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
                  <Input
                    className="pl-10"
                    placeholder="809-000-0000"
                    value={phone}
                    onChange={e => setPhone(e.target.value)}
                  />
                </div>
              </div>
              <div>
                <Label>Email de Contacto</Label>
                <div className="relative mt-2">
                  <Mail className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
                  <Input
                    className="pl-10"
                    type="email"
                    placeholder="sucursal@dealer.com"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                  />
                </div>
              </div>
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
              {days.map(day => {
                const h = hours[day.key];
                return (
                  <div key={day.key} className="bg-muted/50 flex items-center gap-4 rounded-lg p-3">
                    <div className="w-24">
                      <span className="font-medium">{day.label}</span>
                    </div>
                    <Switch
                      checked={h.enabled}
                      onCheckedChange={v => updateHours(day.key, 'enabled', v)}
                    />
                    {h.enabled ? (
                      <div className="flex flex-1 items-center gap-2">
                        <Input
                          type="time"
                          className="w-32"
                          value={h.open}
                          onChange={e => updateHours(day.key, 'open', e.target.value)}
                        />
                        <span className="text-muted-foreground">a</span>
                        <Input
                          type="time"
                          className="w-32"
                          value={h.close}
                          onChange={e => updateHours(day.key, 'close', e.target.value)}
                        />
                      </div>
                    ) : (
                      <span className="text-muted-foreground text-sm">Cerrado</span>
                    )}
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>

        {/* Actions */}
        <div className="flex justify-end gap-4">
          <Button variant="outline" onClick={() => router.back()} disabled={addLocation.isPending}>
            Cancelar
          </Button>
          <Button
            className="bg-primary hover:bg-primary/90"
            onClick={handleSubmit}
            disabled={!isFormValid || addLocation.isPending}
          >
            {addLocation.isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Guardando...
              </>
            ) : (
              'Guardar Ubicación'
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
