import { redirect } from 'next/navigation';

/**
 * Dealer Profile Redirect Page
 *
 * Legacy route redirect: /dealer/perfil → /cuenta/perfil
 * The canonical dealer profile is now part of the role-aware profile page
 * that handles personal info + dealer business card for dealers.
 *
 * Keeping this for backward compatibility with bookmarks and email links.
 */
export default function DealerProfilePage() {
  redirect('/cuenta/perfil');
}
import { Skeleton } from '@/components/ui/skeleton';
import {
  Building,
  MapPin,
  Phone,
  Mail,
  Clock,
  Camera,
  Save,
  Eye,
  Star,
  Shield,
  Facebook,
  Instagram,
  AlertCircle,
  Loader2,
} from 'lucide-react';
import { toast } from 'sonner';
import { useCurrentDealer, useUpdateDealer, useDealerLocations } from '@/hooks/use-dealers';
import { sanitizeText, sanitizePhone, sanitizeUrl } from '@/lib/security/sanitize';

// =============================================================================
// SKELETON COMPONENTS
// =============================================================================

function ProfileSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <Skeleton className="mb-2 h-8 w-48" />
          <Skeleton className="h-5 w-64" />
        </div>
        <div className="flex gap-3">
          <Skeleton className="h-10 w-36" />
          <Skeleton className="h-10 w-36" />
        </div>
      </div>
      <Card>
        <CardContent className="p-4">
          <div className="flex items-center gap-3">
            <Skeleton className="h-6 w-6" />
            <Skeleton className="h-6 w-48" />
          </div>
        </CardContent>
      </Card>
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Card>
            <CardHeader>
              <Skeleton className="h-6 w-48" />
              <Skeleton className="h-4 w-32" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-48 w-full rounded-lg" />
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <Skeleton className="h-6 w-40" />
            </CardHeader>
            <CardContent className="space-y-4">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-24 w-full" />
              <div className="grid gap-4 md:grid-cols-2">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </div>
            </CardContent>
          </Card>
        </div>
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <Skeleton className="h-6 w-40" />
            </CardHeader>
            <CardContent className="space-y-4">
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

// =============================================================================
// MAIN PAGE COMPONENT
// =============================================================================

export default function DealerProfilePage() {
  const { data: dealer, isLoading: isDealerLoading } = useCurrentDealer();
  const { data: locations } = useDealerLocations(dealer?.id);
  const updateMutation = useUpdateDealer();
  const logoInputRef = useRef<HTMLInputElement>(null);
  const bannerInputRef = useRef<HTMLInputElement>(null);

  // Form state
  const [formData, setFormData] = useState({
    businessName: '',
    legalName: '',
    description: '',
    establishedDate: '',
    rnc: '',
    phone: '',
    mobilePhone: '',
    email: '',
    facebookUrl: '',
    instagramUrl: '',
    whatsAppNumber: '',
  });

  const [hasChanges, setHasChanges] = useState(false);

  // Sync form with dealer data
  useEffect(() => {
    if (dealer) {
      setFormData({
        businessName: dealer.businessName || '',
        legalName: dealer.legalName || '',
        description: dealer.description || '',
        establishedDate: dealer.establishedDate ? dealer.establishedDate.split('T')[0] : '',
        rnc: dealer.rnc || '',
        phone: dealer.phone || '',
        mobilePhone: dealer.mobilePhone || '',
        email: dealer.email || '',
        facebookUrl: dealer.facebookUrl || '',
        instagramUrl: dealer.instagramUrl || '',
        whatsAppNumber: dealer.whatsAppNumber || '',
      });
    }
  }, [dealer]);

  const handleInputChange = (field: keyof typeof formData, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setHasChanges(true);
  };

  const handleSave = async () => {
    if (!dealer) return;

    try {
      // Sanitize all user inputs before sending to API
      await updateMutation.mutateAsync({
        id: dealer.id,
        data: {
          businessName: sanitizeText(formData.businessName, { maxLength: 200 }),
          legalName: formData.legalName
            ? sanitizeText(formData.legalName, { maxLength: 200 })
            : undefined,
          description: formData.description
            ? sanitizeText(formData.description, { maxLength: 2000 })
            : undefined,
          phone: formData.phone ? sanitizePhone(formData.phone) : undefined,
          mobilePhone: formData.mobilePhone ? sanitizePhone(formData.mobilePhone) : undefined,
          facebookUrl: formData.facebookUrl ? sanitizeUrl(formData.facebookUrl) : undefined,
          instagramUrl: formData.instagramUrl ? sanitizeUrl(formData.instagramUrl) : undefined,
          whatsAppNumber: formData.whatsAppNumber
            ? sanitizePhone(formData.whatsAppNumber)
            : undefined,
        },
      });
      toast.success('Perfil actualizado correctamente');
      setHasChanges(false);
    } catch {
      toast.error('Error al actualizar el perfil');
    }
  };

  // Get primary location
  const primaryLocation = locations?.find(l => l.isPrimary) || locations?.[0];

  // Get dealer initials for avatar
  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map(word => word[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  };

  // Get year from date
  const getYear = (dateStr?: string) => {
    if (!dateStr) return '';
    return new Date(dateStr).getFullYear().toString();
  };

  if (isDealerLoading) {
    return <ProfileSkeleton />;
  }

  if (!dealer) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <Card className="w-full max-w-md p-6 text-center">
          <CardContent>
            <AlertCircle className="mx-auto mb-4 h-12 w-12 text-amber-500" />
            <h2 className="mb-2 text-xl font-semibold">No se encontró el dealer</h2>
            <p className="text-muted-foreground">Por favor, inicia sesión como dealer.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const isVerified = dealer.verificationStatus === 'verified';

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-foreground text-2xl font-bold">Perfil del Dealer</h1>
          <p className="text-muted-foreground">Gestiona tu información pública</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline" asChild>
            <a href={`/dealers/${dealer.id}`} target="_blank">
              <Eye className="mr-2 h-4 w-4" />
              Ver Perfil Público
            </a>
          </Button>
          <Button
            className="bg-primary hover:bg-primary/90"
            onClick={handleSave}
            disabled={!hasChanges || updateMutation.isPending}
          >
            {updateMutation.isPending ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Save className="mr-2 h-4 w-4" />
            )}
            Guardar Cambios
          </Button>
        </div>
      </div>

      {/* Profile Status */}
      <Card
        className={isVerified ? 'border-primary bg-primary/10' : 'border-amber-200 bg-amber-50'}
      >
        <CardContent className="p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Shield className={`h-6 w-6 ${isVerified ? 'text-primary' : 'text-amber-600'}`} />
              <div>
                <p className={`font-medium ${isVerified ? 'text-primary' : 'text-amber-800'}`}>
                  {isVerified ? 'Dealer Verificado' : 'Verificación Pendiente'}
                </p>
                <p className={`text-sm ${isVerified ? 'text-primary' : 'text-amber-600'}`}>
                  {isVerified
                    ? 'Tu perfil está completo y verificado'
                    : 'Completa la verificación para activar tu perfil'}
                </p>
              </div>
            </div>
            {dealer.rating && (
              <div className="flex items-center gap-2">
                <Star className="h-5 w-5 fill-current text-amber-400" />
                <span className="font-bold">{dealer.rating.toFixed(1)}</span>
                {dealer.reviewCount && (
                  <span className="text-muted-foreground text-sm">
                    ({dealer.reviewCount} reseñas)
                  </span>
                )}
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Main Info */}
        <div className="space-y-6 lg:col-span-2">
          {/* Logo & Cover */}
          <Card>
            <CardHeader>
              <CardTitle>Imágenes del Dealer</CardTitle>
              <CardDescription>Logo y foto de portada</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Cover Image */}
              <div>
                <label className="text-sm font-medium">Foto de Portada</label>
                <div className="group bg-muted relative mt-2 h-48 overflow-hidden rounded-lg">
                  {dealer.bannerUrl ? (
                    <Image src={dealer.bannerUrl} alt="Cover" fill className="object-cover" />
                  ) : (
                    <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-gray-200 to-gray-300">
                      <Building className="text-muted-foreground h-16 w-16" />
                    </div>
                  )}
                  <div className="absolute inset-0 flex items-center justify-center bg-black/40 opacity-0 transition-opacity group-hover:opacity-100">
                    <Button variant="secondary" onClick={() => bannerInputRef.current?.click()}>
                      <Camera className="mr-2 h-4 w-4" />
                      Cambiar Portada
                    </Button>
                  </div>
                </div>
                <input
                  ref={bannerInputRef}
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={() => toast.info('Funcionalidad de subir banner próximamente')}
                />
              </div>

              {/* Logo */}
              <div>
                <label className="text-sm font-medium">Logo</label>
                <div className="mt-2 flex items-center gap-4">
                  <div className="group bg-muted relative h-24 w-24 overflow-hidden rounded-lg">
                    {dealer.logoUrl ? (
                      <Image src={dealer.logoUrl} alt="Logo" fill className="object-cover" />
                    ) : (
                      <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-primary/60 to-primary text-3xl font-bold text-white">
                        {getInitials(dealer.businessName)}
                      </div>
                    )}
                    <div
                      className="absolute inset-0 flex cursor-pointer items-center justify-center bg-black/40 opacity-0 transition-opacity group-hover:opacity-100"
                      onClick={() => logoInputRef.current?.click()}
                    >
                      <Camera className="h-6 w-6 text-white" />
                    </div>
                  </div>
                  <div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => logoInputRef.current?.click()}
                    >
                      Subir Logo
                    </Button>
                    <p className="text-muted-foreground mt-1 text-xs">PNG o JPG, máx 2MB</p>
                  </div>
                  <input
                    ref={logoInputRef}
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={() => toast.info('Funcionalidad de subir logo próximamente')}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Basic Info */}
          <Card>
            <CardHeader>
              <CardTitle>Información Básica</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <label className="text-sm font-medium">Nombre del Dealer</label>
                <Input
                  value={formData.businessName}
                  onChange={e => handleInputChange('businessName', e.target.value)}
                  className="mt-1"
                />
              </div>
              <div>
                <label className="text-sm font-medium">Descripción</label>
                <Textarea
                  value={formData.description}
                  onChange={e => handleInputChange('description', e.target.value)}
                  className="mt-1"
                  rows={4}
                  placeholder="Describe tu negocio..."
                />
              </div>
              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <label className="text-sm font-medium">Año de Fundación</label>
                  <Input
                    value={getYear(formData.establishedDate)}
                    className="mt-1"
                    disabled
                    placeholder="2008"
                  />
                </div>
                <div>
                  <label className="text-sm font-medium">RNC</label>
                  <Input
                    value={formData.rnc}
                    className="mt-1"
                    disabled
                    placeholder="123-456789-0"
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Contact Info */}
          <Card>
            <CardHeader>
              <CardTitle>Información de Contacto</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <label className="flex items-center gap-2 text-sm font-medium">
                    <Phone className="h-4 w-4" />
                    Teléfono Principal
                  </label>
                  <Input
                    value={formData.phone}
                    onChange={e => handleInputChange('phone', e.target.value)}
                    className="mt-1"
                    placeholder="809-555-0201"
                  />
                </div>
                <div>
                  <label className="flex items-center gap-2 text-sm font-medium">
                    <Phone className="h-4 w-4" />
                    WhatsApp
                  </label>
                  <Input
                    value={formData.mobilePhone}
                    onChange={e => handleInputChange('mobilePhone', e.target.value)}
                    className="mt-1"
                    placeholder="809-555-0202"
                  />
                </div>
              </div>
              <div>
                <label className="flex items-center gap-2 text-sm font-medium">
                  <Mail className="h-4 w-4" />
                  Email
                </label>
                <Input
                  value={formData.email}
                  className="mt-1"
                  disabled
                  placeholder="info@dealer.com"
                />
              </div>
              <div>
                <label className="flex items-center gap-2 text-sm font-medium">
                  <Phone className="h-4 w-4" />
                  WhatsApp
                </label>
                <Input
                  value={formData.whatsAppNumber}
                  onChange={e => handleInputChange('whatsAppNumber', e.target.value)}
                  className="mt-1"
                  placeholder="809-555-0202"
                />
              </div>
              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <label className="flex items-center gap-2 text-sm font-medium">
                    <Facebook className="h-4 w-4" />
                    Facebook
                  </label>
                  <Input
                    value={formData.facebookUrl}
                    onChange={e => handleInputChange('facebookUrl', e.target.value)}
                    className="mt-1"
                    placeholder="@tu_dealer"
                  />
                </div>
                <div>
                  <label className="flex items-center gap-2 text-sm font-medium">
                    <Instagram className="h-4 w-4" />
                    Instagram
                  </label>
                  <Input
                    value={formData.instagramUrl}
                    onChange={e => handleInputChange('instagramUrl', e.target.value)}
                    className="mt-1"
                    placeholder="@tu_dealer"
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Location */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                Ubicación Principal
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <label className="text-sm font-medium">Dirección</label>
                <Input
                  value={primaryLocation?.address || dealer.address || ''}
                  className="mt-1"
                  disabled
                />
              </div>
              <div>
                <label className="text-sm font-medium">Ciudad</label>
                <Input
                  value={primaryLocation?.city || dealer.city || ''}
                  className="mt-1"
                  disabled
                />
              </div>
              <div>
                <label className="text-sm font-medium">Provincia</label>
                <Input
                  value={primaryLocation?.province || dealer.province || ''}
                  className="mt-1"
                  disabled
                />
              </div>
              <Button variant="outline" className="w-full" asChild>
                <a href="/dealer/ubicaciones">Gestionar Ubicaciones</a>
              </Button>
            </CardContent>
          </Card>

          {/* Business Hours */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Horario de Atención
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {primaryLocation?.businessHours ? (
                <>
                  {primaryLocation.businessHours.monday && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Lunes - Viernes</span>
                      <span className="font-medium">
                        {primaryLocation.businessHours.monday.open} -{' '}
                        {primaryLocation.businessHours.monday.close}
                      </span>
                    </div>
                  )}
                  {primaryLocation.businessHours.saturday && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Sábado</span>
                      <span className="font-medium">
                        {primaryLocation.businessHours.saturday.isClosed
                          ? 'Cerrado'
                          : `${primaryLocation.businessHours.saturday.open} - ${primaryLocation.businessHours.saturday.close}`}
                      </span>
                    </div>
                  )}
                  {primaryLocation.businessHours.sunday && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Domingo</span>
                      <span className="font-medium">
                        {primaryLocation.businessHours.sunday.isClosed
                          ? 'Cerrado'
                          : `${primaryLocation.businessHours.sunday.open} - ${primaryLocation.businessHours.sunday.close}`}
                      </span>
                    </div>
                  )}
                </>
              ) : (
                <p className="text-muted-foreground text-sm">Sin horarios configurados</p>
              )}
              <Button variant="link" className="h-auto p-0 text-primary" asChild>
                <a href="/dealer/ubicaciones">Editar horarios</a>
              </Button>
            </CardContent>
          </Card>

          {/* Plan Info */}
          <Card>
            <CardHeader>
              <CardTitle>Plan Actual</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="mb-3 flex items-center justify-between">
                <Badge
                  className={
                    dealer.plan === 'enterprise'
                      ? 'bg-purple-100 text-purple-700'
                      : dealer.plan === 'pro'
                        ? 'bg-primary/10 text-primary'
                        : 'bg-muted text-foreground'
                  }
                >
                  {dealer.plan.toUpperCase()}
                </Badge>
                <span className="text-muted-foreground text-sm">
                  {dealer.currentActiveListings} /{' '}
                  {dealer.maxActiveListings === -1 ? '∞' : dealer.maxActiveListings} vehículos
                </span>
              </div>
              <div className="mb-3">
                <div className="bg-muted h-2 w-full rounded-full">
                  <div
                    className="h-2 rounded-full bg-primary/100"
                    style={{
                      width:
                        dealer.maxActiveListings === -1
                          ? '10%'
                          : `${Math.min(100, (dealer.currentActiveListings / dealer.maxActiveListings) * 100)}%`,
                    }}
                  />
                </div>
              </div>
              <Button variant="outline" className="w-full" asChild>
                <a href="/dealer/suscripcion">Gestionar Suscripción</a>
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
