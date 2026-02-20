/**
 * Seller Wizard - Step 2: Seller Profile Setup
 *
 * Creates the seller profile via UserService.
 * If dealer, RNC and business name are required.
 *
 * Features:
 * - Display name (public-facing)
 * - Business name (for dealers)
 * - RNC validation (9 or 11 digits, Dominican Republic)
 * - Description / bio
 * - Specialties multi-select
 * - Location
 */

'use client';

import * as React from 'react';
import { Loader2, AlertCircle, Store, MapPin, Tag } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import {
  SELLER_SPECIALTIES,
  RD_PROVINCES,
  type SellerProfileFormData,
} from '@/lib/validations/seller-onboarding';

interface ProfileStepProps {
  data: SellerProfileFormData;
  onChange: (data: Partial<SellerProfileFormData>) => void;
  onSubmit: () => void;
  onBack: () => void;
  isDealer: boolean;
  isLoading: boolean;
  error: string | null;
  userName?: string;
}

export function ProfileStep({
  data,
  onChange,
  onSubmit,
  onBack,
  isDealer,
  isLoading,
  error,
  userName,
}: ProfileStepProps) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit();
  };

  const toggleSpecialty = (specialty: string) => {
    const current = data.specialties ?? [];
    const updated = current.includes(specialty)
      ? current.filter(s => s !== specialty)
      : [...current, specialty];
    onChange({ specialties: updated });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Header */}
      <div className="space-y-1">
        <h3 className="text-lg font-semibold">
          {isDealer ? 'Perfil de tu dealer' : 'Tu perfil de vendedor'}
        </h3>
        <p className="text-muted-foreground text-sm">
          Esta información será visible para los compradores.
          {userName && (
            <>
              {' '}
              ¡Bienvenido, <span className="font-medium text-[#00A870]">{userName}</span>!
            </>
          )}
        </p>
      </div>

      {/* Display Name */}
      <div className="space-y-2">
        <Label htmlFor="displayName">
          <Store className="mr-1 inline h-4 w-4" />
          Nombre público *
        </Label>
        <Input
          id="displayName"
          value={data.displayName}
          onChange={e => onChange({ displayName: e.target.value })}
          placeholder={isDealer ? 'Ej: Auto Dominicana Premium' : 'Ej: Juan Pérez Autos'}
          required
          maxLength={100}
        />
        <p className="text-muted-foreground text-xs">
          Así te verán los compradores en tus publicaciones
        </p>
      </div>

      {/* Business Name (dealer only) */}
      {isDealer && (
        <div className="space-y-2">
          <Label htmlFor="businessName">Razón Social *</Label>
          <Input
            id="businessName"
            value={data.businessName ?? ''}
            onChange={e => onChange({ businessName: e.target.value })}
            placeholder="Nombre legal de la empresa"
            required
            maxLength={150}
          />
        </div>
      )}

      {/* RNC */}
      <div className="space-y-2">
        <Label htmlFor="rnc">
          RNC {isDealer ? '*' : <span className="text-muted-foreground">(opcional)</span>}
        </Label>
        <Input
          id="rnc"
          value={data.rnc ?? ''}
          onChange={e => onChange({ rnc: e.target.value.replace(/\D/g, '').slice(0, 11) })}
          placeholder="123456789"
          maxLength={11}
          required={isDealer}
        />
        <p className="text-muted-foreground text-xs">
          Registro Nacional del Contribuyente (9 o 11 dígitos)
        </p>
      </div>

      {/* Phone */}
      <div className="space-y-2">
        <Label htmlFor="profilePhone">
          Teléfono de contacto <span className="text-muted-foreground">(opcional)</span>
        </Label>
        <div className="flex gap-2">
          <div className="bg-muted flex items-center rounded-md border px-3 text-sm">+1</div>
          <Input
            id="profilePhone"
            type="tel"
            value={data.phone ?? ''}
            onChange={e => onChange({ phone: e.target.value.replace(/\D/g, '').slice(0, 10) })}
            placeholder="8091234567"
            maxLength={10}
          />
        </div>
      </div>

      {/* Description */}
      <div className="space-y-2">
        <Label htmlFor="description">
          Descripción <span className="text-muted-foreground">(opcional)</span>
        </Label>
        <Textarea
          id="description"
          value={data.description ?? ''}
          onChange={e => onChange({ description: e.target.value })}
          placeholder={
            isDealer
              ? 'Cuéntale a los compradores sobre tu negocio, años de experiencia, servicios...'
              : 'Cuéntale a los compradores sobre ti...'
          }
          rows={3}
          maxLength={500}
        />
        <p className="text-muted-foreground text-right text-xs">
          {(data.description ?? '').length}/500
        </p>
      </div>

      {/* Location */}
      <div className="space-y-2">
        <Label htmlFor="location">
          <MapPin className="mr-1 inline h-4 w-4" />
          Ubicación <span className="text-muted-foreground">(opcional)</span>
        </Label>
        <select
          id="location"
          value={data.location ?? ''}
          onChange={e => onChange({ location: e.target.value })}
          className="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus:ring-2 focus:ring-offset-2 focus:outline-none"
        >
          <option value="">Seleccionar provincia</option>
          {RD_PROVINCES.map(province => (
            <option key={province} value={province}>
              {province}
            </option>
          ))}
        </select>
      </div>

      {/* Specialties */}
      <div className="space-y-3">
        <Label>
          <Tag className="mr-1 inline h-4 w-4" />
          Especialidades <span className="text-muted-foreground">(opcional)</span>
        </Label>
        <div className="flex flex-wrap gap-2">
          {SELLER_SPECIALTIES.map(specialty => {
            const isSelected = (data.specialties ?? []).includes(specialty);
            return (
              <Badge
                key={specialty}
                variant={isSelected ? 'default' : 'outline'}
                className={cn(
                  'cursor-pointer transition-colors',
                  isSelected
                    ? 'bg-[#00A870] hover:bg-[#009663]'
                    : 'hover:border-[#00A870] hover:text-[#00A870]'
                )}
                onClick={() => toggleSpecialty(specialty)}
              >
                {specialty}
              </Badge>
            );
          })}
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-red-600" />
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      {/* Actions */}
      <div className="flex gap-3">
        <Button type="button" variant="outline" onClick={onBack} disabled={isLoading}>
          Atrás
        </Button>
        <Button
          type="submit"
          className="flex-1 bg-[#00A870] hover:bg-[#009663]"
          disabled={isLoading || !data.displayName || (isDealer && !data.rnc)}
        >
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Guardando perfil...
            </>
          ) : (
            'Guardar perfil y continuar'
          )}
        </Button>
      </div>
    </form>
  );
}
