/**
 * Profile Edit Page
 *
 * Allows users to edit their profile information
 */

'use client';

import * as React from 'react';
import { useRouter } from 'next/navigation';
import { User, Mail, Phone, MapPin, Camera, Loader2, Check, X, Trash2 } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { userService, type UpdateProfileRequest } from '@/services/users';
import { useAuth } from '@/hooks/use-auth';
import { sanitizeText, sanitizePhone } from '@/lib/security/sanitize';

export default function ProfilePage() {
  const router = useRouter();
  const { user: authUser } = useAuth(); // Get user from JWT/AuthService

  const [isLoading, setIsLoading] = React.useState(true);
  const [isSaving, setIsSaving] = React.useState(false);
  const [isUploadingAvatar, setIsUploadingAvatar] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [success, setSuccess] = React.useState(false);
  const [userServiceAvailable, setUserServiceAvailable] = React.useState(true); // Track if UserService has user profile

  const [profile, setProfile] = React.useState<{
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    bio: string;
    city: string;
    province: string;
    avatarUrl: string | null;
    isEmailVerified: boolean;
    isPhoneVerified: boolean;
  }>({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    bio: '',
    city: '',
    province: '',
    avatarUrl: null,
    isEmailVerified: false,
    isPhoneVerified: false,
  });

  const fileInputRef = React.useRef<HTMLInputElement>(null);

  // Load profile on mount
  React.useEffect(() => {
    async function loadProfile() {
      try {
        // Try to load from UserService first
        const data = await userService.getCurrentProfile();
        setProfile({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          phone: data.phone || '',
          bio: data.bio || '',
          city: data.city || '',
          province: data.province || '',
          avatarUrl: data.avatarUrl || null,
          isEmailVerified: data.isEmailVerified,
          isPhoneVerified: data.isPhoneVerified,
        });
        setUserServiceAvailable(true);
      } catch (err) {
        // Fallback to auth user data (from JWT) if UserService profile doesn't exist (404)
        // This happens when user registers via OAuth but UserService doesn't have their profile yet
        if (authUser) {
          setProfile({
            firstName: authUser.firstName || '',
            lastName: authUser.lastName || '',
            email: authUser.email || '',
            phone: '',
            bio: '',
            city: '',
            province: '',
            avatarUrl: authUser.avatarUrl || null,
            isEmailVerified: authUser.isEmailVerified || false,
            isPhoneVerified: false,
          });
          setUserServiceAvailable(false);
          // Note: Profile editing won't work until UserService syncs this user
          console.info('Using auth user data as fallback - profile editing may be limited');
        } else {
          setError('Error al cargar el perfil');
        }
      } finally {
        setIsLoading(false);
      }
    }
    loadProfile();
  }, [authUser]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);

    // If UserService wasn't available at load time, try once more before blocking the save
    if (!userServiceAvailable) {
      try {
        await userService.getCurrentProfile();
        setUserServiceAvailable(true);
        // Fall through and save normally
      } catch {
        setError('Tu perfil aún no está disponible para editar. Intenta recargar la página en unos momentos.');
        return;
      }
    }

    setIsSaving(true);

    try {
      const updateData: UpdateProfileRequest = {
        firstName: sanitizeText(profile.firstName.trim(), { maxLength: 50 }),
        lastName: sanitizeText(profile.lastName.trim(), { maxLength: 50 }),
        phone: profile.phone ? sanitizePhone(profile.phone) : undefined,
        bio: profile.bio ? sanitizeText(profile.bio, { maxLength: 500 }) : undefined,
        city: profile.city ? sanitizeText(profile.city.trim(), { maxLength: 100 }) : undefined,
        province: profile.province ? sanitizeText(profile.province.trim(), { maxLength: 100 }) : undefined,
      };

      await userService.updateProfile(updateData);
      setSuccess(true);
      router.refresh();

      // Hide success message after 3 seconds
      setTimeout(() => setSuccess(false), 3000);
    } catch (err) {
      const error = err as { message?: string };
      setError(error.message || 'Error al guardar el perfil');
    } finally {
      setIsSaving(false);
    }
  };

  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file
    if (!file.type.startsWith('image/')) {
      setError('Solo se permiten imágenes');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      setError('La imagen no puede superar 5MB');
      return;
    }

    setIsUploadingAvatar(true);
    setError(null);

    try {
      const { avatarUrl } = await userService.uploadAvatar(file);
      setProfile(prev => ({ ...prev, avatarUrl }));
      router.refresh();
    } catch (err) {
      setError('Error al subir la imagen');
    } finally {
      setIsUploadingAvatar(false);
    }
  };

  const handleDeleteAvatar = async () => {
    if (!confirm('¿Estás seguro de eliminar tu foto de perfil?')) return;

    setIsUploadingAvatar(true);
    try {
      await userService.deleteAvatar();
      setProfile(prev => ({ ...prev, avatarUrl: null }));
      router.refresh();
    } catch (err) {
      setError('Error al eliminar la imagen');
    } finally {
      setIsUploadingAvatar(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="text-primary h-8 w-8 animate-spin" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-foreground">Mi Perfil</h1>
        <p className="text-muted-foreground">Actualiza tu información personal</p>
      </div>

      {/* Sync pending info banner */}
      {!userServiceAvailable && !error && (
        <div className="flex items-center gap-2 rounded-lg border border-yellow-200 bg-yellow-50 p-4 text-yellow-800">
          <Loader2 className="h-5 w-5 animate-spin" />
          Tu perfil está siendo sincronizado. Los datos de arriba provienen de tu cuenta de acceso. En breve podrás editarlos.
        </div>
      )}

      {/* Success/Error Messages */}
      {success && (
        <div className="flex items-center gap-2 rounded-lg border border-green-200 bg-green-50 p-4 text-green-700">
          <Check className="h-5 w-5" />
          Perfil actualizado correctamente
        </div>
      )}

      {error && (
        <div className="flex items-center gap-2 rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">
          <X className="h-5 w-5" />
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Avatar Section */}
        <Card>
          <CardHeader>
            <CardTitle>Foto de Perfil</CardTitle>
            <CardDescription>Esta foto se mostrará en tu perfil público</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-6">
              <div className="relative">
                <div className="flex h-24 w-24 items-center justify-center overflow-hidden rounded-full bg-muted">
                  {isUploadingAvatar ? (
                    <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                  ) : profile.avatarUrl ? (
                    <img
                      src={profile.avatarUrl}
                      alt="Avatar"
                      className="h-full w-full object-cover"
                    />
                  ) : (
                    <User className="h-12 w-12 text-muted-foreground" />
                  )}
                </div>
                <button
                  type="button"
                  onClick={handleAvatarClick}
                  disabled={isUploadingAvatar}
                  className="bg-primary hover:bg-primary/90 absolute right-0 bottom-0 flex h-8 w-8 items-center justify-center rounded-full text-white transition-colors"
                >
                  <Camera className="h-4 w-4" />
                </button>
              </div>
              <div className="space-y-2">
                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleAvatarClick}
                    disabled={isUploadingAvatar}
                  >
                    Cambiar foto
                  </Button>
                  {profile.avatarUrl && (
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      onClick={handleDeleteAvatar}
                      disabled={isUploadingAvatar}
                      className="text-red-600 hover:bg-red-50 hover:text-red-700"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  )}
                </div>
                <p className="text-sm text-muted-foreground">JPG, PNG. Máximo 5MB.</p>
              </div>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                onChange={handleAvatarChange}
                className="hidden"
              />
            </div>
          </CardContent>
        </Card>

        {/* Personal Information */}
        <Card>
          <CardHeader>
            <CardTitle>Información Personal</CardTitle>
            <CardDescription>Actualiza tus datos básicos</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="firstName">Nombre</Label>
                <Input
                  id="firstName"
                  value={profile.firstName}
                  onChange={e => setProfile({ ...profile, firstName: e.target.value })}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lastName">Apellido</Label>
                <Input
                  id="lastName"
                  value={profile.lastName}
                  onChange={e => setProfile({ ...profile, lastName: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <div className="relative">
                <Mail className="absolute top-1/2 left-3 h-5 w-5 -translate-y-1/2 text-muted-foreground" />
                <Input id="email" type="email" value={profile.email} disabled className="pl-10" />
                {profile.isEmailVerified && (
                  <span className="absolute top-1/2 right-3 flex -translate-y-1/2 items-center gap-1 text-sm text-green-600">
                    <Check className="h-4 w-4" />
                    Verificado
                  </span>
                )}
              </div>
              <p className="text-sm text-muted-foreground">El email no puede ser cambiado</p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="phone">Teléfono</Label>
              <div className="relative">
                <Phone className="absolute top-1/2 left-3 h-5 w-5 -translate-y-1/2 text-muted-foreground" />
                <Input
                  id="phone"
                  type="tel"
                  placeholder="809-555-0123"
                  value={profile.phone}
                  onChange={e => setProfile({ ...profile, phone: e.target.value })}
                  className="pl-10"
                />
                {profile.phone && profile.isPhoneVerified && (
                  <span className="absolute top-1/2 right-3 flex -translate-y-1/2 items-center gap-1 text-sm text-green-600">
                    <Check className="h-4 w-4" />
                    Verificado
                  </span>
                )}
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="bio">Biografía</Label>
              <textarea
                id="bio"
                placeholder="Cuéntanos sobre ti..."
                value={profile.bio}
                onChange={e => setProfile({ ...profile, bio: e.target.value })}
                rows={3}
                className="focus:ring-primary/20 focus:border-primary w-full resize-none rounded-lg border border-border px-3 py-2 focus:ring-2 focus:outline-none"
              />
              <p className="text-sm text-muted-foreground">{profile.bio.length}/500 caracteres</p>
            </div>
          </CardContent>
        </Card>

        {/* Location */}
        <Card>
          <CardHeader>
            <CardTitle>Ubicación</CardTitle>
            <CardDescription>Tu ubicación aparecerá en tus publicaciones</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="city">Ciudad</Label>
                <div className="relative">
                  <MapPin className="absolute top-1/2 left-3 h-5 w-5 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="city"
                    placeholder="Santo Domingo"
                    value={profile.city}
                    onChange={e => setProfile({ ...profile, city: e.target.value })}
                    className="pl-10"
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="province">Provincia</Label>
                <Input
                  id="province"
                  placeholder="Distrito Nacional"
                  value={profile.province}
                  onChange={e => setProfile({ ...profile, province: e.target.value })}
                />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Submit */}
        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" onClick={() => router.back()}>
            Cancelar
          </Button>
          <Button type="submit" disabled={isSaving}>
            {isSaving ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Guardando...
              </>
            ) : (
              'Guardar cambios'
            )}
          </Button>
        </div>
      </form>
    </div>
  );
}
