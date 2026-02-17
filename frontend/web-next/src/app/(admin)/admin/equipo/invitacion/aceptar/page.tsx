/**
 * Accept Staff Invitation Page
 *
 * Public page where invited staff members can accept their invitation
 * and create their account. No authentication required.
 */

'use client';

import { useState } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useSiteConfig } from '@/providers/site-config-provider';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import {
  Loader2,
  CheckCircle2,
  XCircle,
  Shield,
  Building2,
  Briefcase,
  Mail,
  User,
  Lock,
  Phone,
  AlertTriangle,
  PartyPopper,
  Eye,
  EyeOff,
} from 'lucide-react';
import { toast } from 'sonner';
import { useValidateInvitation, useAcceptInvitation } from '@/hooks/use-staff';

const ROLE_LABELS: Record<string, { label: string; color: string }> = {
  SuperAdmin: { label: 'Super Admin', color: 'bg-red-100 text-red-700 border-red-200' },
  Admin: { label: 'Administrador', color: 'bg-purple-100 text-purple-700 border-purple-200' },
  Moderator: { label: 'Moderador', color: 'bg-blue-100 text-blue-700 border-blue-200' },
  Support: { label: 'Soporte', color: 'bg-green-100 text-green-700 border-green-200' },
  Analyst: { label: 'Analista', color: 'bg-amber-100 text-amber-700 border-amber-200' },
  Compliance: { label: 'Compliance', color: 'bg-teal-100 text-teal-700 border-teal-200' },
};

export default function AcceptInvitationPage() {
  const searchParams = useSearchParams();
  const token = searchParams.get('token') || '';
  const config = useSiteConfig();

  const {
    data: invitation,
    isLoading: isValidating,
    error: validationError,
  } = useValidateInvitation(token);
  const acceptMutation = useAcceptInvitation();

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    password: '',
    confirmPassword: '',
    phoneNumber: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [accepted, setAccepted] = useState(false);

  // Password validation
  const passwordChecks = {
    minLength: formData.password.length >= 8,
    uppercase: /[A-Z]/.test(formData.password),
    lowercase: /[a-z]/.test(formData.password),
    number: /[0-9]/.test(formData.password),
    special: /[^a-zA-Z0-9]/.test(formData.password),
  };
  const isPasswordValid = Object.values(passwordChecks).every(Boolean);
  const passwordsMatch =
    formData.password === formData.confirmPassword && formData.confirmPassword.length > 0;

  const isFormValid =
    formData.firstName.trim().length >= 2 &&
    formData.lastName.trim().length >= 2 &&
    isPasswordValid &&
    passwordsMatch;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isFormValid || !token) return;

    try {
      await acceptMutation.mutateAsync({
        token,
        password: formData.password,
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        phoneNumber: formData.phoneNumber.trim() || undefined,
      });

      setAccepted(true);
      toast.success('¡Cuenta creada exitosamente!');
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err?.response?.data?.message || 'Error al aceptar la invitación');
    }
  };

  // No token provided
  if (!token) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-50 to-blue-50 p-4">
        <Card className="w-full max-w-md text-center">
          <CardContent className="pt-8 pb-8">
            <XCircle className="mx-auto mb-4 h-16 w-16 text-red-400" />
            <h2 className="mb-2 text-xl font-semibold text-slate-800">Enlace inválido</h2>
            <p className="mb-6 text-slate-500">
              Este enlace de invitación no es válido. Por favor, verifica el enlace que recibiste
              por email.
            </p>
            <Link href="/login">
              <Button variant="outline">Ir al inicio de sesión</Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Loading validation
  if (isValidating) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-50 to-blue-50 p-4">
        <Card className="w-full max-w-md text-center">
          <CardContent className="pt-8 pb-8">
            <Loader2 className="mx-auto mb-4 h-12 w-12 animate-spin text-blue-500" />
            <h2 className="text-lg font-medium text-slate-700">Verificando invitación...</h2>
            <p className="mt-2 text-sm text-slate-500">Por favor espera un momento</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Invalid/expired invitation
  if (validationError || !invitation || !invitation.isValid) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-50 to-blue-50 p-4">
        <Card className="w-full max-w-md text-center">
          <CardContent className="pt-8 pb-8">
            <AlertTriangle className="mx-auto mb-4 h-16 w-16 text-amber-400" />
            <h2 className="mb-2 text-xl font-semibold text-slate-800">Invitación no válida</h2>
            <p className="mb-6 text-slate-500">
              Esta invitación ha expirado o ya fue utilizada. Contacta al administrador para recibir
              una nueva invitación.
            </p>
            <Link href="/login">
              <Button variant="outline">Ir al inicio de sesión</Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Success state
  if (accepted) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-50 to-green-50 p-4">
        <Card className="w-full max-w-md text-center">
          <CardContent className="pt-8 pb-8">
            <div className="relative mx-auto mb-4 h-20 w-20">
              <div className="absolute inset-0 animate-ping rounded-full bg-green-100 opacity-25" />
              <div className="relative flex h-20 w-20 items-center justify-center rounded-full bg-green-100">
                <PartyPopper className="h-10 w-10 text-green-600" />
              </div>
            </div>
            <h2 className="mb-2 text-2xl font-bold text-slate-800">¡Bienvenido al equipo!</h2>
            <p className="mb-6 text-slate-500">
              Tu cuenta ha sido creada exitosamente. Ya puedes iniciar sesión con tu email y
              contraseña.
            </p>
            <Link href="/login">
              <Button className="w-full bg-green-600 hover:bg-green-700">Iniciar Sesión</Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  const roleInfo = ROLE_LABELS[invitation.assignedRole || ''] || {
    label: invitation.assignedRole,
    color: 'bg-gray-100 text-gray-700',
  };

  // Invitation form
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-50 to-blue-50 p-4">
      <div className="w-full max-w-lg">
        {/* Header */}
        <div className="mb-6 text-center">
          <h1 className="mb-1 text-3xl font-bold text-blue-600">{config.siteName}</h1>
          <p className="text-sm text-slate-500">Plataforma de Vehículos</p>
        </div>

        <Card className="border-0 shadow-lg">
          <CardHeader className="pb-4 text-center">
            <CardTitle className="text-xl">¡Has sido invitado!</CardTitle>
            <CardDescription>Completa tu perfil para unirte al equipo</CardDescription>
          </CardHeader>
          <CardContent>
            {/* Invitation info */}
            <div className="mb-6 space-y-2 rounded-lg bg-blue-50 p-4">
              <div className="flex items-center gap-2 text-sm">
                <Mail className="h-4 w-4 text-blue-500" />
                <span className="text-slate-600">Email:</span>
                <span className="font-medium text-slate-800">{invitation.email}</span>
              </div>
              <div className="flex items-center gap-2 text-sm">
                <Shield className="h-4 w-4 text-blue-500" />
                <span className="text-slate-600">Rol asignado:</span>
                <Badge variant="outline" className={roleInfo.color}>
                  {roleInfo.label}
                </Badge>
              </div>
              {invitation.departmentName && (
                <div className="flex items-center gap-2 text-sm">
                  <Building2 className="h-4 w-4 text-blue-500" />
                  <span className="text-slate-600">Departamento:</span>
                  <span className="font-medium text-slate-800">{invitation.departmentName}</span>
                </div>
              )}
              {invitation.positionTitle && (
                <div className="flex items-center gap-2 text-sm">
                  <Briefcase className="h-4 w-4 text-blue-500" />
                  <span className="text-slate-600">Posición:</span>
                  <span className="font-medium text-slate-800">{invitation.positionTitle}</span>
                </div>
              )}
              {invitation.message && (
                <div className="mt-3 border-t border-blue-100 pt-3">
                  <p className="text-sm text-blue-700 italic">&ldquo;{invitation.message}&rdquo;</p>
                </div>
              )}
            </div>

            {/* Registration form */}
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1.5">
                  <Label htmlFor="firstName">Nombre *</Label>
                  <div className="relative">
                    <User className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
                    <Input
                      id="firstName"
                      placeholder="Juan"
                      value={formData.firstName}
                      onChange={e => setFormData(prev => ({ ...prev, firstName: e.target.value }))}
                      className="pl-9"
                      required
                    />
                  </div>
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="lastName">Apellido *</Label>
                  <Input
                    id="lastName"
                    placeholder="Pérez"
                    value={formData.lastName}
                    onChange={e => setFormData(prev => ({ ...prev, lastName: e.target.value }))}
                    required
                  />
                </div>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="phone">Teléfono (opcional)</Label>
                <div className="relative">
                  <Phone className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  <Input
                    id="phone"
                    type="tel"
                    placeholder="809-555-1234"
                    value={formData.phoneNumber}
                    onChange={e => setFormData(prev => ({ ...prev, phoneNumber: e.target.value }))}
                    className="pl-9"
                  />
                </div>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="password">Contraseña *</Label>
                <div className="relative">
                  <Lock className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  <Input
                    id="password"
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Mínimo 8 caracteres"
                    value={formData.password}
                    onChange={e => setFormData(prev => ({ ...prev, password: e.target.value }))}
                    className="pr-10 pl-9"
                    required
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute top-1/2 right-3 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                  >
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
                {formData.password && (
                  <div className="mt-2 grid grid-cols-2 gap-1">
                    {[
                      { check: passwordChecks.minLength, label: '8+ caracteres' },
                      { check: passwordChecks.uppercase, label: 'Mayúscula' },
                      { check: passwordChecks.lowercase, label: 'Minúscula' },
                      { check: passwordChecks.number, label: 'Número' },
                      { check: passwordChecks.special, label: 'Carácter especial' },
                    ].map(({ check, label }) => (
                      <div key={label} className="flex items-center gap-1.5">
                        {check ? (
                          <CheckCircle2 className="h-3.5 w-3.5 text-green-500" />
                        ) : (
                          <XCircle className="h-3.5 w-3.5 text-slate-300" />
                        )}
                        <span className={`text-xs ${check ? 'text-green-600' : 'text-slate-400'}`}>
                          {label}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="confirmPassword">Confirmar Contraseña *</Label>
                <div className="relative">
                  <Lock className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  <Input
                    id="confirmPassword"
                    type={showConfirmPassword ? 'text' : 'password'}
                    placeholder="Repite tu contraseña"
                    value={formData.confirmPassword}
                    onChange={e =>
                      setFormData(prev => ({ ...prev, confirmPassword: e.target.value }))
                    }
                    className="pr-10 pl-9"
                    required
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute top-1/2 right-3 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                  >
                    {showConfirmPassword ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </button>
                </div>
                {formData.confirmPassword && !passwordsMatch && (
                  <p className="mt-1 text-xs text-red-500">Las contraseñas no coinciden</p>
                )}
                {passwordsMatch && (
                  <p className="mt-1 flex items-center gap-1 text-xs text-green-500">
                    <CheckCircle2 className="h-3.5 w-3.5" /> Contraseñas coinciden
                  </p>
                )}
              </div>

              <Button
                type="submit"
                className="mt-2 w-full bg-blue-600 hover:bg-blue-700"
                disabled={!isFormValid || acceptMutation.isPending}
              >
                {acceptMutation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Creando cuenta...
                  </>
                ) : (
                  <>
                    <CheckCircle2 className="mr-2 h-4 w-4" />
                    Aceptar Invitación y Crear Cuenta
                  </>
                )}
              </Button>
            </form>

            <p className="mt-4 text-center text-xs text-slate-400">
              La invitación expira el{' '}
              {invitation.expiresAt
                ? new Date(invitation.expiresAt).toLocaleDateString('es-DO', {
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric',
                  })
                : 'N/A'}
            </p>
          </CardContent>
        </Card>

        <p className="mt-4 text-center text-xs text-slate-400">
          © {new Date().getFullYear()} {config.siteName} — {config.siteUrl.replace('https://', '')}
        </p>
      </div>
    </div>
  );
}
