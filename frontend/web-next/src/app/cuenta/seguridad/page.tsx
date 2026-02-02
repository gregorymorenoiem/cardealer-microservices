/**
 * Security Settings Page
 *
 * Manage password, 2FA, and active sessions - API Integrated
 */

'use client';

import * as React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Shield,
  Key,
  Smartphone,
  Laptop,
  Tablet,
  MapPin,
  Clock,
  LogOut,
  AlertTriangle,
  Check,
  Loader2,
  Eye,
  EyeOff,
  Copy,
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Skeleton } from '@/components/ui/skeleton';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { authService, type Session, type TwoFactorSetupResponse } from '@/services/auth';
import { cn } from '@/lib/utils';
import { toast } from 'sonner';
import { useRouter } from 'next/navigation';

// ============================================================
// SESSION ITEM COMPONENT
// ============================================================

function SessionItem({
  session,
  onRevoke,
  isRevoking,
}: {
  session: Session;
  onRevoke: () => void;
  isRevoking: boolean;
}) {
  const formatTime = (date: string) => {
    const d = new Date(date);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Activo ahora';
    if (diffMins < 60) return `Hace ${diffMins} minutos`;
    if (diffHours < 24) return `Hace ${diffHours} horas`;
    return `Hace ${diffDays} días`;
  };

  const DeviceIcon = {
    desktop: Laptop,
    mobile: Smartphone,
    tablet: Tablet,
    unknown: Laptop,
  }[session.deviceType];

  return (
    <div className="flex items-start gap-4 rounded-lg border border-gray-200 p-4">
      <div
        className={cn(
          'flex h-10 w-10 items-center justify-center rounded-lg',
          session.isCurrent ? 'bg-primary/10 text-primary' : 'bg-gray-100 text-gray-600'
        )}
      >
        <DeviceIcon className="h-5 w-5" />
      </div>

      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <h4 className="font-medium text-gray-900">{session.deviceName}</h4>
          {session.isCurrent && (
            <span className="bg-primary/10 text-primary rounded-full px-2 py-0.5 text-xs font-medium">
              Este dispositivo
            </span>
          )}
        </div>
        <p className="mt-0.5 text-sm text-gray-600">
          {session.browser} en {session.os}
        </p>
        <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-gray-500">
          {session.location && (
            <div className="flex items-center gap-1">
              <MapPin className="h-3 w-3" />
              {session.location}
            </div>
          )}
          <div className="flex items-center gap-1">
            <Clock className="h-3 w-3" />
            {formatTime(session.lastActiveAt)}
          </div>
        </div>
      </div>

      {!session.isCurrent && (
        <Button
          variant="ghost"
          size="sm"
          onClick={onRevoke}
          disabled={isRevoking}
          className="text-red-600 hover:bg-red-50 hover:text-red-700"
        >
          {isRevoking ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <LogOut className="h-4 w-4" />
          )}
        </Button>
      )}
    </div>
  );
}

// ============================================================
// 2FA SETUP DIALOG
// ============================================================

function TwoFactorSetupDialog({
  open,
  onOpenChange,
  setupData,
  onVerify,
  isVerifying,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  setupData: TwoFactorSetupResponse | null;
  onVerify: (code: string) => void;
  isVerifying: boolean;
}) {
  const [code, setCode] = React.useState('');
  const [copiedSecret, setCopiedSecret] = React.useState(false);

  const handleCopySecret = () => {
    if (setupData?.secret) {
      navigator.clipboard.writeText(setupData.secret);
      setCopiedSecret(true);
      setTimeout(() => setCopiedSecret(false), 2000);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (code.length === 6) {
      onVerify(code);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Configurar autenticación de dos factores</DialogTitle>
          <DialogDescription>
            Escanea el código QR con tu aplicación de autenticación (Google Authenticator, Authy,
            etc.)
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* QR Code */}
          {setupData?.qrCodeUrl && (
            <div className="flex justify-center">
              <div className="rounded-lg border bg-white p-4">
                <img src={setupData.qrCodeUrl} alt="QR Code para 2FA" className="h-48 w-48" />
              </div>
            </div>
          )}

          {/* Manual Entry */}
          {setupData?.secret && (
            <div className="space-y-2">
              <Label className="text-sm text-gray-600">
                ¿No puedes escanear? Ingresa este código manualmente:
              </Label>
              <div className="flex items-center gap-2">
                <code className="flex-1 rounded bg-gray-100 px-3 py-2 font-mono text-sm">
                  {setupData.secret}
                </code>
                <Button variant="outline" size="sm" onClick={handleCopySecret}>
                  {copiedSecret ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                </Button>
              </div>
            </div>
          )}

          {/* Verification Code */}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="verification-code">Código de verificación</Label>
              <Input
                id="verification-code"
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                maxLength={6}
                placeholder="000000"
                value={code}
                onChange={e => setCode(e.target.value.replace(/\D/g, ''))}
                className="text-center text-lg tracking-widest"
              />
              <p className="text-xs text-gray-500">
                Ingresa el código de 6 dígitos de tu aplicación
              </p>
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                Cancelar
              </Button>
              <Button type="submit" disabled={code.length !== 6 || isVerifying}>
                {isVerifying ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Verificando...
                  </>
                ) : (
                  'Verificar y activar'
                )}
              </Button>
            </DialogFooter>
          </form>

          {/* Backup Codes */}
          {setupData?.backupCodes && setupData.backupCodes.length > 0 && (
            <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-4">
              <p className="text-sm font-medium text-yellow-800">
                Guarda estos códigos de respaldo en un lugar seguro:
              </p>
              <div className="mt-2 grid grid-cols-2 gap-2">
                {setupData.backupCodes.map((backupCode, i) => (
                  <code key={i} className="rounded bg-white px-2 py-1 font-mono text-sm">
                    {backupCode}
                  </code>
                ))}
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

// ============================================================
// DISABLE 2FA DIALOG
// ============================================================

function Disable2FADialog({
  open,
  onOpenChange,
  onDisable,
  isDisabling,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onDisable: (code: string) => void;
  isDisabling: boolean;
}) {
  const [code, setCode] = React.useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (code.length === 6) {
      onDisable(code);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Desactivar autenticación de dos factores</DialogTitle>
          <DialogDescription>
            Ingresa el código de tu aplicación de autenticación para confirmar la desactivación
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="disable-code">Código de verificación</Label>
            <Input
              id="disable-code"
              type="text"
              inputMode="numeric"
              pattern="[0-9]*"
              maxLength={6}
              placeholder="000000"
              value={code}
              onChange={e => setCode(e.target.value.replace(/\D/g, ''))}
              className="text-center text-lg tracking-widest"
            />
          </div>

          <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
            <AlertTriangle className="mb-1 inline h-4 w-4" /> Tu cuenta será menos segura sin 2FA
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button type="submit" variant="destructive" disabled={code.length !== 6 || isDisabling}>
              {isDisabling ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Desactivando...
                </>
              ) : (
                'Desactivar 2FA'
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ============================================================
// DELETE ACCOUNT DIALOG
// ============================================================

function DeleteAccountDialog({
  open,
  onOpenChange,
  onDelete,
  isDeleting,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onDelete: (password: string) => void;
  isDeleting: boolean;
}) {
  const [password, setPassword] = React.useState('');
  const [confirmation, setConfirmation] = React.useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (confirmation === 'ELIMINAR' && password) {
      onDelete(password);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-red-600">Eliminar cuenta permanentemente</DialogTitle>
          <DialogDescription>
            Esta acción eliminará todos tus datos, vehículos publicados, mensajes y historial. Esta
            acción NO se puede deshacer.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="delete-password">Ingresa tu contraseña</Label>
            <Input
              id="delete-password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="delete-confirmation">
              Escribe <strong>ELIMINAR</strong> para confirmar
            </Label>
            <Input
              id="delete-confirmation"
              type="text"
              value={confirmation}
              onChange={e => setConfirmation(e.target.value.toUpperCase())}
              placeholder="ELIMINAR"
              required
            />
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button
              type="submit"
              variant="destructive"
              disabled={confirmation !== 'ELIMINAR' || !password || isDeleting}
            >
              {isDeleting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Eliminando...
                </>
              ) : (
                'Eliminar mi cuenta'
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ============================================================
// MAIN COMPONENT
// ============================================================

export default function SecurityPage() {
  const router = useRouter();
  const queryClient = useQueryClient();

  // Password change state
  const [isChangingPassword, setIsChangingPassword] = React.useState(false);
  const [passwordForm, setPasswordForm] = React.useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [showPasswords, setShowPasswords] = React.useState({
    current: false,
    new: false,
    confirm: false,
  });
  const [passwordError, setPasswordError] = React.useState<string | null>(null);

  // 2FA dialogs
  const [showSetup2FA, setShowSetup2FA] = React.useState(false);
  const [showDisable2FA, setShowDisable2FA] = React.useState(false);
  const [setup2FAData, setSetup2FAData] = React.useState<TwoFactorSetupResponse | null>(null);

  // Delete account dialog
  const [showDeleteAccount, setShowDeleteAccount] = React.useState(false);

  // Revoking session ID for loading state
  const [revokingSessionId, setRevokingSessionId] = React.useState<string | null>(null);

  // ============================================================
  // QUERIES
  // ============================================================

  const { data: sessions = [], isLoading: sessionsLoading } = useQuery({
    queryKey: ['sessions'],
    queryFn: authService.getSessions,
  });

  const { data: twoFactorStatus, isLoading: twoFactorLoading } = useQuery({
    queryKey: ['2fa-status'],
    queryFn: authService.get2FAStatus,
  });

  // ============================================================
  // MUTATIONS
  // ============================================================

  const changePasswordMutation = useMutation({
    mutationFn: ({
      currentPassword,
      newPassword,
    }: {
      currentPassword: string;
      newPassword: string;
    }) => authService.changePassword(currentPassword, newPassword),
    onSuccess: () => {
      toast.success('Contraseña actualizada correctamente');
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setIsChangingPassword(false);
    },
    onError: (error: Error) => {
      setPasswordError(error.message || 'Error al cambiar la contraseña');
    },
  });

  const revokeSessionMutation = useMutation({
    mutationFn: authService.revokeSession,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
      toast.success('Sesión cerrada correctamente');
      setRevokingSessionId(null);
    },
    onError: () => {
      toast.error('Error al cerrar la sesión');
      setRevokingSessionId(null);
    },
  });

  const revokeAllSessionsMutation = useMutation({
    mutationFn: authService.revokeAllSessions,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
      toast.success('Todas las otras sesiones han sido cerradas');
    },
    onError: () => {
      toast.error('Error al cerrar las sesiones');
    },
  });

  const setup2FAMutation = useMutation({
    mutationFn: authService.setup2FA,
    onSuccess: data => {
      setSetup2FAData(data);
      setShowSetup2FA(true);
    },
    onError: () => {
      toast.error('Error al configurar 2FA');
    },
  });

  const enable2FAMutation = useMutation({
    mutationFn: authService.enable2FA,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['2fa-status'] });
      setShowSetup2FA(false);
      setSetup2FAData(null);
      toast.success('Autenticación de dos factores activada');
    },
    onError: () => {
      toast.error('Código inválido. Inténtalo de nuevo.');
    },
  });

  const disable2FAMutation = useMutation({
    mutationFn: authService.disable2FA,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['2fa-status'] });
      setShowDisable2FA(false);
      toast.success('Autenticación de dos factores desactivada');
    },
    onError: () => {
      toast.error('Código inválido. Inténtalo de nuevo.');
    },
  });

  const deleteAccountMutation = useMutation({
    mutationFn: authService.deleteAccount,
    onSuccess: () => {
      toast.success('Tu cuenta ha sido eliminada');
      router.push('/');
    },
    onError: () => {
      toast.error('Error al eliminar la cuenta. Verifica tu contraseña.');
    },
  });

  // ============================================================
  // HANDLERS
  // ============================================================

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordError(null);

    if (passwordForm.newPassword.length < 8) {
      setPasswordError('La contraseña debe tener al menos 8 caracteres');
      return;
    }

    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setPasswordError('Las contraseñas no coinciden');
      return;
    }

    changePasswordMutation.mutate({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    });
  };

  const handleToggle2FA = () => {
    if (twoFactorStatus?.isEnabled) {
      setShowDisable2FA(true);
    } else {
      setup2FAMutation.mutate();
    }
  };

  const handleRevokeSession = (sessionId: string) => {
    setRevokingSessionId(sessionId);
    revokeSessionMutation.mutate(sessionId);
  };

  // Password strength
  const getPasswordStrength = (password: string) => {
    let strength = 0;
    if (password.length >= 8) strength++;
    if (password.length >= 12) strength++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;
    return strength;
  };

  const passwordStrength = getPasswordStrength(passwordForm.newPassword);
  const strengthLabels = ['Muy débil', 'Débil', 'Regular', 'Fuerte', 'Muy fuerte'];
  const strengthColors = [
    'bg-red-500',
    'bg-orange-500',
    'bg-yellow-500',
    'bg-green-500',
    'bg-green-600',
  ];

  const otherSessions = sessions.filter(s => !s.isCurrent);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Seguridad</h1>
        <p className="text-gray-600">Gestiona tu contraseña y la seguridad de tu cuenta</p>
      </div>

      {/* Password Section */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Key className="h-5 w-5" />
            Contraseña
          </CardTitle>
          <CardDescription>
            Cambia tu contraseña periódicamente para mantener tu cuenta segura
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isChangingPassword ? (
            <form onSubmit={handlePasswordSubmit} className="space-y-4">
              {passwordError && (
                <div className="flex items-center gap-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                  <AlertTriangle className="h-4 w-4 flex-shrink-0" />
                  {passwordError}
                </div>
              )}

              <div className="space-y-2">
                <Label htmlFor="currentPassword">Contraseña actual</Label>
                <div className="relative">
                  <Input
                    id="currentPassword"
                    type={showPasswords.current ? 'text' : 'password'}
                    value={passwordForm.currentPassword}
                    onChange={e =>
                      setPasswordForm({ ...passwordForm, currentPassword: e.target.value })
                    }
                    required
                  />
                  <button
                    type="button"
                    onClick={() =>
                      setShowPasswords({ ...showPasswords, current: !showPasswords.current })
                    }
                    className="absolute top-1/2 right-3 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    {showPasswords.current ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </button>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="newPassword">Nueva contraseña</Label>
                <div className="relative">
                  <Input
                    id="newPassword"
                    type={showPasswords.new ? 'text' : 'password'}
                    value={passwordForm.newPassword}
                    onChange={e =>
                      setPasswordForm({ ...passwordForm, newPassword: e.target.value })
                    }
                    required
                  />
                  <button
                    type="button"
                    onClick={() => setShowPasswords({ ...showPasswords, new: !showPasswords.new })}
                    className="absolute top-1/2 right-3 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    {showPasswords.new ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </button>
                </div>
                {passwordForm.newPassword && (
                  <div className="space-y-1">
                    <div className="flex gap-1">
                      {[...Array(5)].map((_, i) => (
                        <div
                          key={i}
                          className={cn(
                            'h-1 flex-1 rounded-full transition-colors',
                            i < passwordStrength
                              ? strengthColors[passwordStrength - 1]
                              : 'bg-gray-200'
                          )}
                        />
                      ))}
                    </div>
                    <p className="text-xs text-gray-600">
                      Fortaleza: {strengthLabels[passwordStrength - 1] || 'Muy débil'}
                    </p>
                  </div>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirmPassword">Confirmar nueva contraseña</Label>
                <div className="relative">
                  <Input
                    id="confirmPassword"
                    type={showPasswords.confirm ? 'text' : 'password'}
                    value={passwordForm.confirmPassword}
                    onChange={e =>
                      setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })
                    }
                    required
                  />
                  <button
                    type="button"
                    onClick={() =>
                      setShowPasswords({ ...showPasswords, confirm: !showPasswords.confirm })
                    }
                    className="absolute top-1/2 right-3 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    {showPasswords.confirm ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </button>
                </div>
              </div>

              <div className="flex gap-2">
                <Button type="submit" disabled={changePasswordMutation.isPending}>
                  {changePasswordMutation.isPending ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Guardando...
                    </>
                  ) : (
                    'Cambiar contraseña'
                  )}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    setIsChangingPassword(false);
                    setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
                    setPasswordError(null);
                  }}
                >
                  Cancelar
                </Button>
              </div>
            </form>
          ) : (
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">
                  Usa una contraseña fuerte que no uses en otros sitios
                </p>
              </div>
              <Button variant="outline" onClick={() => setIsChangingPassword(true)}>
                Cambiar contraseña
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* 2FA Section */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Smartphone className="h-5 w-5" />
            Autenticación de dos factores (2FA)
          </CardTitle>
          <CardDescription>Añade una capa extra de seguridad a tu cuenta</CardDescription>
        </CardHeader>
        <CardContent>
          {twoFactorLoading ? (
            <div className="flex items-center justify-between">
              <div className="space-y-2">
                <Skeleton className="h-5 w-24" />
                <Skeleton className="h-4 w-48" />
              </div>
              <Skeleton className="h-6 w-11 rounded-full" />
            </div>
          ) : (
            <div className="flex items-center justify-between">
              <div className="space-y-1">
                <p className="font-medium">
                  {twoFactorStatus?.isEnabled ? 'Habilitado' : 'Deshabilitado'}
                </p>
                <p className="text-sm text-gray-600">
                  {twoFactorStatus?.isEnabled
                    ? `Tu cuenta está protegida con 2FA. ${twoFactorStatus.backupCodesRemaining} códigos de respaldo restantes.`
                    : 'Recomendamos activar 2FA para mayor seguridad'}
                </p>
              </div>
              <Switch
                checked={twoFactorStatus?.isEnabled ?? false}
                onCheckedChange={handleToggle2FA}
                disabled={setup2FAMutation.isPending}
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Active Sessions */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <Shield className="h-5 w-5" />
                Sesiones activas
              </CardTitle>
              <CardDescription>Dispositivos donde has iniciado sesión</CardDescription>
            </div>
            {otherSessions.length > 0 && (
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-red-600 hover:bg-red-50 hover:text-red-700"
                  >
                    Cerrar otras sesiones
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>¿Cerrar todas las otras sesiones?</AlertDialogTitle>
                    <AlertDialogDescription>
                      Tendrás que volver a iniciar sesión en esos dispositivos.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancelar</AlertDialogCancel>
                    <AlertDialogAction
                      onClick={() => revokeAllSessionsMutation.mutate()}
                      className="bg-red-600 hover:bg-red-700"
                    >
                      {revokeAllSessionsMutation.isPending ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          Cerrando...
                        </>
                      ) : (
                        'Cerrar sesiones'
                      )}
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            )}
          </div>
        </CardHeader>
        <CardContent className="space-y-3">
          {sessionsLoading ? (
            <>
              <Skeleton className="h-24 w-full rounded-lg" />
              <Skeleton className="h-24 w-full rounded-lg" />
            </>
          ) : sessions.length === 0 ? (
            <p className="py-4 text-center text-sm text-gray-500">No hay sesiones activas</p>
          ) : (
            sessions.map(session => (
              <SessionItem
                key={session.id}
                session={session}
                onRevoke={() => handleRevokeSession(session.id)}
                isRevoking={revokingSessionId === session.id}
              />
            ))
          )}
        </CardContent>
      </Card>

      {/* Danger Zone */}
      <Card className="border-red-200">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-red-600">
            <AlertTriangle className="h-5 w-5" />
            Zona de peligro
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium text-gray-900">Eliminar cuenta</p>
              <p className="text-sm text-gray-600">
                Esta acción es permanente y no se puede deshacer
              </p>
            </div>
            <Button variant="destructive" size="sm" onClick={() => setShowDeleteAccount(true)}>
              Eliminar cuenta
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* 2FA Setup Dialog */}
      <TwoFactorSetupDialog
        open={showSetup2FA}
        onOpenChange={setShowSetup2FA}
        setupData={setup2FAData}
        onVerify={code => enable2FAMutation.mutate(code)}
        isVerifying={enable2FAMutation.isPending}
      />

      {/* Disable 2FA Dialog */}
      <Disable2FADialog
        open={showDisable2FA}
        onOpenChange={setShowDisable2FA}
        onDisable={code => disable2FAMutation.mutate(code)}
        isDisabling={disable2FAMutation.isPending}
      />

      {/* Delete Account Dialog */}
      <DeleteAccountDialog
        open={showDeleteAccount}
        onOpenChange={setShowDeleteAccount}
        onDelete={password => deleteAccountMutation.mutate(password)}
        isDeleting={deleteAccountMutation.isPending}
      />
    </div>
  );
}
