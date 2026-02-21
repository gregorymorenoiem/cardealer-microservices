/**
 * Seller Registration Wizard Page
 *
 * 3-step onboarding flow for new sellers:
 * 1. Create Account (skipped if already authenticated)
 * 2. Seller Profile Setup
 * 3. Publish First Vehicle
 *
 * Route: /vender/registro
 *
 * Features:
 * - Draft auto-save to localStorage
 * - Skip Step 1 if logged in
 * - Buyer → Seller conversion for existing accounts
 * - Inline validation with Zod schemas
 * - Mobile-first responsive design
 */

'use client';

import * as React from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { Loader2, AlertCircle, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';

import { useAuth } from '@/hooks/use-auth';
import { useConvertToSeller, useCreateSellerProfile } from '@/hooks/use-seller';
import { useCreateVehicle } from '@/hooks/use-vehicles';

import { StepIndicator } from '@/components/seller-wizard/step-indicator';
import { AccountStep } from '@/components/seller-wizard/account-step';
import { ProfileStep } from '@/components/seller-wizard/profile-step';
import { VehicleStep } from '@/components/seller-wizard/vehicle-step';
import { SuccessScreen } from '@/components/seller-wizard/success-screen';
import type { UploadedImage } from '@/components/seller-wizard/photo-uploader';

import type { CreateVehicleRequest, CreateVehicleImage } from '@/services/vehicles';
import type { RegisterRequest } from '@/services/auth';

// =============================================================================
// CONSTANTS
// =============================================================================

const DRAFT_STORAGE_KEY = 'okla-seller-wizard-draft';

const STEPS = [
  { id: 'account', label: 'Cuenta', description: 'Crea tu cuenta' },
  { id: 'profile', label: 'Perfil', description: 'Tu perfil de vendedor' },
  { id: 'vehicle', label: 'Vehículo', description: 'Publica tu vehículo' },
] as const;

// =============================================================================
// TYPES
// =============================================================================

interface AccountData {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
  confirmPassword: string;
  accountType: 'individual' | 'dealer';
  acceptTerms: boolean;
}

interface ProfileData {
  displayName: string;
  businessName: string;
  rnc: string;
  description: string;
  phone: string;
  location: string;
  specialties: string[];
}

interface VehicleData {
  make: string;
  model: string;
  year: number;
  trim: string;
  mileage: number;
  vin: string;
  transmission: string;
  fuelType: string;
  bodyType: string;
  exteriorColor: string;
  condition: 'new' | 'used' | 'certified';
  price: number;
  currency: 'DOP' | 'USD';
  description: string;
  features: string[];
  city: string;
  province: string;
  isNegotiable: boolean;
  sellerPhone: string;
  sellerEmail: string;
}

interface WizardDraft {
  step: number;
  account: Partial<AccountData>;
  profile: Partial<ProfileData>;
  vehicle: Partial<VehicleData>;
  savedAt: string;
}

interface PublishedVehicle {
  id: string;
  slug: string;
  title: string;
}

// =============================================================================
// INITIAL STATE
// =============================================================================

const initialAccount: AccountData = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  password: '',
  confirmPassword: '',
  accountType: 'individual',
  acceptTerms: false,
};

const initialProfile: ProfileData = {
  displayName: '',
  businessName: '',
  rnc: '',
  description: '',
  phone: '',
  location: '',
  specialties: [],
};

const initialVehicle: VehicleData = {
  make: '',
  model: '',
  year: 0,
  trim: '',
  mileage: 0,
  vin: '',
  transmission: '',
  fuelType: '',
  bodyType: '',
  exteriorColor: '',
  condition: 'used' as const,
  price: 0,
  currency: 'DOP',
  description: '',
  features: [],
  city: '',
  province: '',
  isNegotiable: false,
  sellerPhone: '',
  sellerEmail: '',
};

// =============================================================================
// DRAFT PERSISTENCE HELPERS
// =============================================================================

function loadDraft(): WizardDraft | null {
  if (typeof window === 'undefined') return null;
  try {
    const raw = localStorage.getItem(DRAFT_STORAGE_KEY);
    if (!raw) return null;
    const draft = JSON.parse(raw) as WizardDraft;
    // Expire drafts older than 7 days
    const savedAt = new Date(draft.savedAt);
    const now = new Date();
    const daysDiff = (now.getTime() - savedAt.getTime()) / (1000 * 60 * 60 * 24);
    if (daysDiff > 7) {
      localStorage.removeItem(DRAFT_STORAGE_KEY);
      return null;
    }
    return draft;
  } catch {
    localStorage.removeItem(DRAFT_STORAGE_KEY);
    return null;
  }
}

function saveDraft(step: number, account: AccountData, profile: ProfileData, vehicle: VehicleData) {
  if (typeof window === 'undefined') return;
  try {
    // Never save passwords
    const safeAccount = { ...account, password: '', confirmPassword: '' };
    const draft: WizardDraft = {
      step,
      account: safeAccount,
      profile,
      vehicle,
      savedAt: new Date().toISOString(),
    };
    localStorage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(draft));
  } catch {
    // Ignore storage errors (quota exceeded, private browsing, etc.)
  }
}

function clearDraft() {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(DRAFT_STORAGE_KEY);
}

// =============================================================================
// PAGE COMPONENT
// =============================================================================

export default function SellerRegistrationPage() {
  const router = useRouter();
  const { user, isAuthenticated, isLoading: authLoading, login, register } = useAuth();

  // ── Mutations ──
  const convertToSeller = useConvertToSeller();
  const createSellerProfile = useCreateSellerProfile();
  const createVehicle = useCreateVehicle();

  // ── Wizard State ──
  const [currentStep, setCurrentStep] = React.useState(0);
  const [accountData, setAccountData] = React.useState<AccountData>(initialAccount);
  const [profileData, setProfileData] = React.useState<ProfileData>(initialProfile);
  const [vehicleData, setVehicleData] = React.useState<VehicleData>(initialVehicle);
  const [uploadedImages, setUploadedImages] = React.useState<UploadedImage[]>([]);
  const [globalError, setGlobalError] = React.useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const [publishedVehicle, setPublishedVehicle] = React.useState<PublishedVehicle | null>(null);
  const [, setSellerProfileId] = React.useState<string | null>(null);
  const [draftLoaded, setDraftLoaded] = React.useState(false);

  // ── Determine effective step: skip account step if already logged in ──
  const isLoggedIn = isAuthenticated && !!user;
  const effectiveStartStep = isLoggedIn ? 1 : 0;

  // ── Load draft on mount ──
  React.useEffect(() => {
    const draft = loadDraft();
    if (draft) {
      if (draft.account) setAccountData(prev => ({ ...prev, ...draft.account }));
      if (draft.profile) setProfileData(prev => ({ ...prev, ...draft.profile }));
      if (draft.vehicle) setVehicleData(prev => ({ ...prev, ...draft.vehicle }));

      // If user is logged in, ensure we start at at least step 1
      const startStep = isLoggedIn ? Math.max(draft.step, 1) : draft.step;
      setCurrentStep(startStep);

      toast.info('Borrador recuperado', {
        description: 'Continuamos donde lo dejaste.',
        action: {
          label: 'Empezar de nuevo',
          onClick: () => {
            clearDraft();
            setCurrentStep(effectiveStartStep);
            setAccountData(initialAccount);
            setProfileData(initialProfile);
            setVehicleData(initialVehicle);
          },
        },
      });
    } else if (isLoggedIn) {
      setCurrentStep(1);
    }
    setDraftLoaded(true);
  }, [isLoggedIn]); // eslint-disable-line react-hooks/exhaustive-deps

  // ── Auto-save draft on data changes ──
  React.useEffect(() => {
    if (!draftLoaded) return;
    saveDraft(currentStep, accountData, profileData, vehicleData);
  }, [currentStep, accountData, profileData, vehicleData, draftLoaded]);

  // ── Pre-fill profile if user data available ──
  React.useEffect(() => {
    if (isLoggedIn && user && !profileData.displayName) {
      setProfileData(prev => ({
        ...prev,
        displayName: prev.displayName || `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim(),
        phone: prev.phone || user.phone || '',
      }));
      setVehicleData(prev => ({
        ...prev,
        sellerEmail: prev.sellerEmail || user.email || '',
        sellerPhone: prev.sellerPhone || user.phone || '',
      }));
    }
  }, [isLoggedIn, user]); // eslint-disable-line react-hooks/exhaustive-deps

  // ─────────────────────────────────────────────────────────────────────────────
  // STEP HANDLERS
  // ─────────────────────────────────────────────────────────────────────────────

  /**
   * Step 1: Register the user account
   */
  const handleAccountSubmit = async () => {
    setGlobalError(null);
    setIsSubmitting(true);
    try {
      const registerData: RegisterRequest = {
        firstName: accountData.firstName,
        lastName: accountData.lastName,
        email: accountData.email,
        phone: accountData.phone || undefined,
        password: accountData.password,
        acceptTerms: true,
      };
      await register(registerData);

      // After registration, automatically log in the user with the credentials they just provided
      // This allows them to continue to the seller profile step without verification
      try {
        // Use the login function from useAuth hook
        await login({
          email: accountData.email,
          password: accountData.password,
        });

        toast.success('¡Cuenta creada y sesión iniciada!', {
          description: 'Continuaremos con tu perfil de vendedor.',
        });
        // Advance to Step 2 — user is now authenticated
        setCurrentStep(1);
      } catch (loginErr) {
        // If auto-login fails, just notify user to verify email and log in manually
        console.warn('Auto-login failed after registration', loginErr);
        toast.success('¡Cuenta creada!', {
          description: 'Inicia sesión para continuar con tu perfil de vendedor.',
        });
        // Redirect to login page with redirect parameter
        router.push(
          `/login?email=${encodeURIComponent(accountData.email)}&redirect=${encodeURIComponent('/vender/registro')}`
        );
      }
    } catch (err) {
      const error = err as { message?: string };
      setGlobalError(error.message || 'Error al crear la cuenta. Intenta de nuevo.');
    } finally {
      setIsSubmitting(false);
    }
  };

  /**
   * Step 2: Create / convert seller profile
   */
  const handleProfileSubmit = async () => {
    setGlobalError(null);
    setIsSubmitting(true);
    try {
      if (isLoggedIn && user) {
        // Existing user → convert to seller
        try {
          const result = await convertToSeller.mutateAsync({
            data: {
              businessName: profileData.businessName || profileData.displayName,
              description: profileData.description || undefined,
              phone: profileData.phone || undefined,
              location: profileData.location || undefined,
              specialties: profileData.specialties.length > 0 ? profileData.specialties : undefined,
              acceptTerms: true,
            },
            idempotencyKey: `convert-seller-${user.id}-${Date.now()}`,
          });
          setSellerProfileId(result.sellerProfileId);
        } catch (err: unknown) {
          // Enhanced error handling for conversion failures
          const error = err as any;

          // Check for 401 Unauthorized (auth token issue)
          if (error?.response?.status === 401 || error?.status === 401) {
            console.error('🔐 Auth token issue - status 401. User:', user.id);
            setGlobalError(
              'Tu sesión ha expirado. Por favor, vuelve a iniciar sesión y intenta de nuevo.'
            );
            // Optionally trigger logout
            return;
          }

          // Check for 404 (endpoint not found)
          if (error?.response?.status === 404 || error?.status === 404) {
            console.error('🚫 Endpoint not found - status 404. URL:', error?.config?.url);
            setGlobalError(
              'El servicio de conversión no está disponible temporalmente. Por favor, intenta en unos momentos.'
            );
            return;
          }

          // Generic error message
          const message =
            error?.message || error?.response?.data?.detail || 'Error al convertirse a vendedor.';
          console.error('❌ Profile submit error:', { error, userID: user.id });
          setGlobalError(message);
        }
      } else {
        // New user just registered → create seller profile
        const profile = await createSellerProfile.mutateAsync({
          userId: user?.id || '', // Will be set after registration
          businessName: profileData.businessName || profileData.displayName,
          displayName: profileData.displayName,
          description: profileData.description || undefined,
          phone: profileData.phone || undefined,
          location: profileData.location || undefined,
          specialties: profileData.specialties.length > 0 ? profileData.specialties : undefined,
        });
        setSellerProfileId(profile.id);
      }

      // Only advance if no error occurred
      if (!globalError) {
        toast.success('¡Perfil de vendedor creado!');
        setCurrentStep(2);
      }
    } catch (err) {
      const error = err as { message?: string; status?: number };
      console.error('❌ handleProfileSubmit caught error:', error);
      setGlobalError(error.message || 'Error al crear el perfil de vendedor.');
    } finally {
      setIsSubmitting(false);
    }
  };

  /**
   * Step 3: Publish the first vehicle
   */
  const handleVehicleSubmit = async () => {
    setGlobalError(null);
    setIsSubmitting(true);
    try {
      // Build vehicle images from uploaded files
      const vehicleImages: CreateVehicleImage[] = uploadedImages
        .filter(img => img.status === 'done' && img.url)
        .map((img, index) => ({
          url: img.url!,
          order: index,
          isPrimary: img.isPrimary,
          alt: `${vehicleData.make} ${vehicleData.model} ${vehicleData.year} - Foto ${index + 1}`,
        }));

      if (vehicleImages.length < 3) {
        setGlobalError('Necesitas al menos 3 fotos para publicar tu vehículo.');
        setIsSubmitting(false);
        return;
      }

      const request: CreateVehicleRequest = {
        make: vehicleData.make,
        model: vehicleData.model,
        year: vehicleData.year,
        trim: vehicleData.trim || undefined,
        mileage: vehicleData.mileage || 0,
        vin: vehicleData.vin || undefined,
        transmission: vehicleData.transmission,
        fuelType: vehicleData.fuelType,
        bodyType: vehicleData.bodyType,
        exteriorColor: vehicleData.exteriorColor || undefined,
        condition: vehicleData.condition,
        price: vehicleData.price,
        currency: vehicleData.currency,
        description: vehicleData.description || undefined,
        features: vehicleData.features,
        images: vehicleImages,
        city: vehicleData.city,
        province: vehicleData.province,
        sellerPhone: vehicleData.sellerPhone || undefined,
        sellerEmail: vehicleData.sellerEmail || undefined,
        isNegotiable: vehicleData.isNegotiable,
      };

      const result = await createVehicle.mutateAsync(request);

      setPublishedVehicle({
        id: result.id,
        slug: result.slug,
        title: `${vehicleData.make} ${vehicleData.model} ${vehicleData.year}`,
      });

      // Clear draft on success
      clearDraft();

      toast.success('¡Vehículo publicado exitosamente!');
    } catch (err) {
      const error = err as { message?: string };
      setGlobalError(error.message || 'Error al publicar el vehículo. Intenta de nuevo.');
    } finally {
      setIsSubmitting(false);
    }
  };

  // ─────────────────────────────────────────────────────────────────────────────
  // NAVIGATION
  // ─────────────────────────────────────────────────────────────────────────────

  const handleBack = () => {
    setGlobalError(null);
    if (currentStep > effectiveStartStep) {
      setCurrentStep(currentStep - 1);
    }
  };

  // ─────────────────────────────────────────────────────────────────────────────
  // RENDER
  // ─────────────────────────────────────────────────────────────────────────────

  // Loading state while auth initializes
  if (authLoading || !draftLoaded) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <div className="text-center">
          <Loader2 className="mx-auto h-8 w-8 animate-spin text-[#00A870]" />
          <p className="mt-4 text-gray-500">Cargando...</p>
        </div>
      </div>
    );
  }

  // Show success screen after vehicle is published
  if (publishedVehicle) {
    return (
      <SuccessScreen
        vehicleId={publishedVehicle.id}
        vehicleSlug={publishedVehicle.slug}
        vehicleTitle={publishedVehicle.title}
        sellerDisplayName={
          profileData.displayName ||
          `${accountData.firstName} ${accountData.lastName}`.trim() ||
          user?.firstName ||
          'Vendedor'
        }
      />
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="border-b bg-white">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => router.push('/vender')}
              className="text-gray-500 hover:text-gray-700"
            >
              <ArrowLeft className="mr-1 h-4 w-4" />
              Volver
            </Button>
            <div className="h-6 w-px bg-gray-200" />
            <h1 className="text-lg font-semibold text-gray-900">
              {isLoggedIn ? 'Comienza a vender' : 'Crea tu cuenta de vendedor'}
            </h1>
          </div>
        </div>
      </div>

      {/* Step Indicator */}
      <div className="bg-white pb-2">
        <div className="container mx-auto px-4">
          <StepIndicator
            steps={STEPS.map(s => ({ label: s.label, description: s.description }))}
            currentStep={currentStep}
          />
        </div>
      </div>

      {/* Global Error */}
      {globalError && (
        <div className="container mx-auto px-4 pt-4">
          <Alert variant="destructive" className="mx-auto max-w-3xl">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{globalError}</AlertDescription>
          </Alert>
        </div>
      )}

      {/* Step Content */}
      <div className="container mx-auto px-4 py-6">
        <div className="mx-auto max-w-3xl">
          {/* Step 1: Account Registration */}
          {currentStep === 0 && !isLoggedIn && (
            <AccountStep
              data={accountData}
              onChange={partial => setAccountData(prev => ({ ...prev, ...partial }))}
              onSubmit={handleAccountSubmit}
              isLoading={isSubmitting}
              error={globalError}
            />
          )}

          {/* Step 2: Seller Profile */}
          {currentStep === 1 && (
            <ProfileStep
              data={profileData}
              onChange={partial => setProfileData(prev => ({ ...prev, ...partial }))}
              isDealer={accountData.accountType === 'dealer'}
              onSubmit={handleProfileSubmit}
              onBack={!isLoggedIn ? handleBack : () => {}}
              isLoading={isSubmitting}
              error={globalError}
            />
          )}

          {/* Step 3: Vehicle Publication */}
          {currentStep === 2 && (
            <VehicleStep
              data={vehicleData}
              onChange={partial => setVehicleData(prev => ({ ...prev, ...partial }))}
              images={uploadedImages}
              onSubmit={handleVehicleSubmit}
              onBack={handleBack}
              onImagesChange={setUploadedImages}
              isLoading={isSubmitting}
              error={globalError}
            />
          )}
        </div>
      </div>
    </div>
  );
}
