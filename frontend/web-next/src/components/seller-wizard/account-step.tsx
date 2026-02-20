/**
 * Seller Wizard - Step 1: Account Registration
 *
 * Creates a new user account via AuthService.
 * If user is already authenticated, this step is skipped.
 *
 * Features:
 * - Email + password registration
 * - Account type selection (individual / dealer)
 * - Phone number (RD format)
 * - Password strength indicator
 * - Input sanitization
 * - Social login (Google / Apple)
 */

'use client';

import * as React from 'react';
import { Eye, EyeOff, Loader2, Check, X, User, Building2, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Checkbox } from '@/components/ui/checkbox';
import { cn } from '@/lib/utils';
import { sanitizeEmail, sanitizePhone, sanitizeText } from '@/lib/security/sanitize';
import type { AccountFormData } from '@/lib/validations/seller-onboarding';

interface AccountStepProps {
  data: AccountFormData;
  onChange: (data: Partial<AccountFormData>) => void;
  onSubmit: () => void;
  isLoading: boolean;
  error: string | null;
}

export function AccountStep({ data, onChange, onSubmit, isLoading, error }: AccountStepProps) {
  const [showPassword, setShowPassword] = React.useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = React.useState(false);

  // Password requirements
  const passwordRequirements = [
    { label: 'Mínimo 8 caracteres', test: (p: string) => p.length >= 8 },
    { label: 'Al menos una mayúscula', test: (p: string) => /[A-Z]/.test(p) },
    { label: 'Al menos una minúscula', test: (p: string) => /[a-z]/.test(p) },
    { label: 'Al menos un número', test: (p: string) => /\d/.test(p) },
    { label: 'Al menos un carácter especial', test: (p: string) => /[^a-zA-Z0-9]/.test(p) },
  ];

  const passwordStrength = passwordRequirements.filter(req => req.test(data.password)).length;
  const isPasswordValid = passwordStrength === passwordRequirements.length;
  const doPasswordsMatch =
    data.password === data.confirmPassword && data.confirmPassword.length > 0;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Sanitize inputs before submitting
    const sanitizedFirstName = sanitizeText(data.firstName.trim(), { maxLength: 50 });
    const sanitizedLastName = sanitizeText(data.lastName.trim(), { maxLength: 50 });
    const sanitizedEmail = sanitizeEmail(data.email);
    const sanitizedPhone = data.phone ? sanitizePhone(data.phone) : '';

    onChange({
      firstName: sanitizedFirstName,
      lastName: sanitizedLastName,
      email: sanitizedEmail,
      phone: sanitizedPhone,
    });

    onSubmit();
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Account Type Selection */}
      <div className="space-y-3">
        <Label className="text-base font-semibold">¿Cómo quieres vender?</Label>
        <RadioGroup
          value={data.accountType}
          onValueChange={v => onChange({ accountType: v as 'individual' | 'dealer' })}
          className="grid grid-cols-1 gap-3 sm:grid-cols-2"
        >
          <Label
            htmlFor="individual"
            className={cn(
              'flex cursor-pointer items-start gap-3 rounded-lg border-2 p-4 transition-all',
              data.accountType === 'individual'
                ? 'border-[#00A870] bg-[#00A870]/5'
                : 'border-border hover:border-[#00A870]/40'
            )}
          >
            <RadioGroupItem value="individual" id="individual" className="mt-0.5" />
            <div className="space-y-1">
              <div className="flex items-center gap-2">
                <User className="h-4 w-4 text-[#00A870]" />
                <span className="font-medium">Vendedor individual</span>
              </div>
              <p className="text-muted-foreground text-xs">
                Vende tu vehículo personal. Paga solo RD$29 por publicación.
              </p>
            </div>
          </Label>
          <Label
            htmlFor="dealer"
            className={cn(
              'flex cursor-pointer items-start gap-3 rounded-lg border-2 p-4 transition-all',
              data.accountType === 'dealer'
                ? 'border-[#00A870] bg-[#00A870]/5'
                : 'border-border hover:border-[#00A870]/40'
            )}
          >
            <RadioGroupItem value="dealer" id="dealer" className="mt-0.5" />
            <div className="space-y-1">
              <div className="flex items-center gap-2">
                <Building2 className="h-4 w-4 text-[#00A870]" />
                <span className="font-medium">Dealer / Empresa</span>
              </div>
              <p className="text-muted-foreground text-xs">
                Inventario completo. Planes desde RD$49/mes.
              </p>
            </div>
          </Label>
        </RadioGroup>
      </div>

      <Separator />

      {/* Name Fields */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="firstName">Nombre *</Label>
          <Input
            id="firstName"
            value={data.firstName}
            onChange={e => onChange({ firstName: e.target.value })}
            placeholder="Tu nombre"
            required
            autoComplete="given-name"
            maxLength={50}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="lastName">Apellido *</Label>
          <Input
            id="lastName"
            value={data.lastName}
            onChange={e => onChange({ lastName: e.target.value })}
            placeholder="Tu apellido"
            required
            autoComplete="family-name"
            maxLength={50}
          />
        </div>
      </div>

      {/* Email */}
      <div className="space-y-2">
        <Label htmlFor="email">Email *</Label>
        <Input
          id="email"
          type="email"
          value={data.email}
          onChange={e => onChange({ email: e.target.value })}
          placeholder="tu@email.com"
          required
          autoComplete="email"
          maxLength={254}
        />
      </div>

      {/* Phone */}
      <div className="space-y-2">
        <Label htmlFor="phone">
          Teléfono <span className="text-muted-foreground">(opcional)</span>
        </Label>
        <div className="flex gap-2">
          <div className="bg-muted flex items-center rounded-md border px-3 text-sm">+1</div>
          <Input
            id="phone"
            type="tel"
            value={data.phone}
            onChange={e => onChange({ phone: e.target.value.replace(/\D/g, '').slice(0, 10) })}
            placeholder="8091234567"
            maxLength={10}
            autoComplete="tel-national"
          />
        </div>
        <p className="text-muted-foreground text-xs">Formato: 10 dígitos sin guiones</p>
      </div>

      {/* Password */}
      <div className="space-y-2">
        <Label htmlFor="password">Contraseña *</Label>
        <div className="relative">
          <Input
            id="password"
            type={showPassword ? 'text' : 'password'}
            value={data.password}
            onChange={e => onChange({ password: e.target.value })}
            placeholder="Crea una contraseña segura"
            required
            autoComplete="new-password"
            maxLength={128}
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="text-muted-foreground hover:text-foreground absolute top-1/2 right-3 -translate-y-1/2"
            tabIndex={-1}
          >
            {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          </button>
        </div>

        {/* Password strength */}
        {data.password.length > 0 && (
          <div className="space-y-2">
            <div className="flex gap-1">
              {[1, 2, 3, 4, 5].map(i => (
                <div
                  key={i}
                  className={cn(
                    'h-1 flex-1 rounded-full transition-colors',
                    i <= passwordStrength
                      ? passwordStrength <= 2
                        ? 'bg-red-500'
                        : passwordStrength <= 4
                          ? 'bg-yellow-500'
                          : 'bg-green-500'
                      : 'bg-muted'
                  )}
                />
              ))}
            </div>
            <ul className="space-y-1">
              {passwordRequirements.map(req => (
                <li
                  key={req.label}
                  className={cn(
                    'flex items-center gap-2 text-xs',
                    req.test(data.password) ? 'text-green-600' : 'text-muted-foreground'
                  )}
                >
                  {req.test(data.password) ? (
                    <Check className="h-3 w-3" />
                  ) : (
                    <X className="h-3 w-3" />
                  )}
                  {req.label}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {/* Confirm Password */}
      <div className="space-y-2">
        <Label htmlFor="confirmPassword">Confirmar contraseña *</Label>
        <div className="relative">
          <Input
            id="confirmPassword"
            type={showConfirmPassword ? 'text' : 'password'}
            value={data.confirmPassword}
            onChange={e => onChange({ confirmPassword: e.target.value })}
            placeholder="Repite tu contraseña"
            required
            autoComplete="new-password"
            maxLength={128}
          />
          <button
            type="button"
            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
            className="text-muted-foreground hover:text-foreground absolute top-1/2 right-3 -translate-y-1/2"
            tabIndex={-1}
          >
            {showConfirmPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          </button>
        </div>
        {data.confirmPassword.length > 0 && !doPasswordsMatch && (
          <p className="text-xs text-red-500">Las contraseñas no coinciden</p>
        )}
      </div>

      {/* Terms */}
      <div className="flex items-start gap-3">
        <Checkbox
          id="acceptTerms"
          checked={!!data.acceptTerms}
          onCheckedChange={checked => onChange({ acceptTerms: checked === true })}
        />
        <Label
          htmlFor="acceptTerms"
          className="text-muted-foreground cursor-pointer text-sm leading-relaxed"
        >
          Acepto los{' '}
          <a href="/terminos" target="_blank" className="text-[#00A870] underline">
            Términos y Condiciones
          </a>{' '}
          y la{' '}
          <a href="/privacidad" target="_blank" className="text-[#00A870] underline">
            Política de Privacidad
          </a>
        </Label>
      </div>

      {/* Error */}
      {error && (
        <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-red-600" />
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      {/* Submit */}
      <Button
        type="submit"
        size="lg"
        className="w-full bg-[#00A870] hover:bg-[#009663]"
        disabled={isLoading || !isPasswordValid || !doPasswordsMatch || !data.acceptTerms}
      >
        {isLoading ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Creando cuenta...
          </>
        ) : (
          'Crear cuenta y continuar'
        )}
      </Button>

      {/* Social login separator */}
      <div className="relative">
        <div className="absolute inset-0 flex items-center">
          <Separator className="w-full" />
        </div>
        <div className="relative flex justify-center text-xs uppercase">
          <span className="bg-background text-muted-foreground px-2">o continúa con</span>
        </div>
      </div>

      {/* Social buttons */}
      <div className="grid grid-cols-2 gap-3">
        <Button variant="outline" type="button" disabled={isLoading} className="gap-2">
          <svg className="h-5 w-5" viewBox="0 0 24 24">
            <path
              fill="currentColor"
              d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
            />
            <path
              fill="currentColor"
              d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
            />
            <path
              fill="currentColor"
              d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
            />
            <path
              fill="currentColor"
              d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
            />
          </svg>
          Google
        </Button>
        <Button variant="outline" type="button" disabled={isLoading} className="gap-2">
          <svg className="h-5 w-5" viewBox="0 0 24 24" fill="currentColor">
            <path d="M17.05 20.28c-.98.95-2.05.88-3.08.41-1.09-.47-2.09-.48-3.24 0-1.44.62-2.2.44-3.06-.41C2.79 15.25 3.51 7.59 9.05 7.31c1.35.07 2.29.74 3.08.8 1.18-.24 2.31-.93 3.57-.84 1.51.12 2.65.72 3.4 1.8-3.12 1.87-2.38 5.98.48 7.13-.57 1.5-1.31 2.99-2.54 4.09l.01-.01zM12.03 7.25c-.15-2.23 1.66-4.07 3.74-4.25.29 2.58-2.34 4.5-3.74 4.25z" />
          </svg>
          Apple
        </Button>
      </div>
    </form>
  );
}
