/**
 * usePermissions Hook
 * 
 * Centraliza la lógica de permisos y acceso a features
 * basado en el tipo de cuenta y plan del usuario
 */

import { useMemo } from 'react';
import { useAuthStore } from '@/store/authStore';
import { useDealerFeatures } from './useDealerFeatures';
import { 
  AccountType, 
  DealerPlan,
  DealerPermission,
  PlatformPermission,
  DEALER_PLAN_LIMITS 
} from '@/shared/types';
import type { User, DealerPlanFeatures } from '@/shared/types';

// ============================================================================
// PORTAL ACCESS DEFINITIONS
// ============================================================================

export interface PortalAccess {
  marketplace: boolean;
  dealerPanel: boolean;
  adminPanel: boolean;
  billing: boolean;
  crm: boolean;
  reports: boolean;
  marketing: boolean;
  finance: boolean;
  appointments: boolean;
  integrations: boolean;
  invoicing: boolean;
}

// ============================================================================
// SERVICE ACCESS BY PLAN
// ============================================================================

export interface ServiceAccess {
  appointmentService: boolean;
  billingService: boolean;
  crmService: boolean;
  financeService: boolean;
  integrationService: boolean;
  invoicingService: boolean;
  marketingService: boolean;
  reportsService: boolean;
}

const getServiceAccessByPlan = (plan: DealerPlan): ServiceAccess => {
  const features = DEALER_PLAN_LIMITS[plan];
  
  return {
    appointmentService: true, // Todos los planes
    billingService: true, // Todos los planes
    crmService: features.leadManagement,
    financeService: plan !== DealerPlan.FREE,
    integrationService: features.apiAccess,
    invoicingService: plan !== DealerPlan.FREE,
    marketingService: features.emailAutomation,
    reportsService: features.analyticsAccess,
  };
};

// ============================================================================
// PORTAL ACCESS BY ACCOUNT TYPE
// ============================================================================

const getPortalAccessByAccountType = (
  accountType: AccountType | null, 
  plan?: DealerPlan
): PortalAccess => {
  const baseAccess: PortalAccess = {
    marketplace: true,
    dealerPanel: false,
    adminPanel: false,
    billing: false,
    crm: false,
    reports: false,
    marketing: false,
    finance: false,
    appointments: false,
    integrations: false,
    invoicing: false,
  };
  
  if (!accountType) return baseAccess;
  
  switch (accountType) {
    case AccountType.GUEST:
    case AccountType.INDIVIDUAL:
      return baseAccess;
      
    case AccountType.DEALER:
    case AccountType.DEALER_EMPLOYEE: {
      const services = plan ? getServiceAccessByPlan(plan) : {
        appointmentService: true,
        billingService: true,
        crmService: false,
        financeService: false,
        integrationService: false,
        invoicingService: false,
        marketingService: false,
        reportsService: false,
      };
      
      return {
        marketplace: true,
        dealerPanel: true,
        adminPanel: false,
        billing: services.billingService,
        crm: services.crmService,
        reports: services.reportsService,
        marketing: services.marketingService,
        finance: services.financeService,
        appointments: services.appointmentService,
        integrations: services.integrationService,
        invoicing: services.invoicingService,
      };
    }
      
    case AccountType.ADMIN:
    case AccountType.PLATFORM_EMPLOYEE:
      return {
        marketplace: true,
        dealerPanel: false,
        adminPanel: true,
        billing: true,
        crm: true,
        reports: true,
        marketing: true,
        finance: true,
        appointments: true,
        integrations: true,
        invoicing: true,
      };
      
    default:
      return baseAccess;
  }
};

// ============================================================================
// HOOK
// ============================================================================

export interface UsePermissionsReturn {
  // Estado de autenticación
  isAuthenticated: boolean;
  user: User | null;
  accountType: AccountType | null;
  
  // Plan del dealer (si aplica)
  dealerPlan: DealerPlan | null;
  planFeatures: DealerPlanFeatures | null;
  
  // Acceso a portales
  portalAccess: PortalAccess;
  
  // Acceso a servicios
  serviceAccess: ServiceAccess | null;
  
  // Helpers de tipo de cuenta
  isGuest: boolean;
  isIndividual: boolean;
  isDealer: boolean;
  isDealerEmployee: boolean;
  isAdmin: boolean;
  isPlatformEmployee: boolean;
  
  // Helpers de dealer
  isDealerOwner: boolean;
  hasActiveSubscription: boolean;
  
  // Funciones de verificación
  canAccessPortal: (portal: keyof PortalAccess) => boolean;
  canAccessService: (service: keyof ServiceAccess) => boolean;
  canAccessFeature: (feature: keyof DealerPlanFeatures) => boolean;
  hasDealerPermission: (permission: DealerPermission) => boolean;
  hasPlatformPermission: (permission: PlatformPermission) => boolean;
  
  // Usage y límites (para dealers)
  usage: {
    listings: number;
    featuredListings: number;
  };
  limits: {
    maxListings: number;
    maxFeaturedListings: number;
  };
  hasReachedLimit: (type: 'listings' | 'featured') => boolean;
  getUsagePercentage: (type: 'listings' | 'featured') => number;
  
  // Upgrade
  needsUpgrade: (feature: keyof DealerPlanFeatures) => boolean;
  getNextPlan: () => DealerPlan | null;
}

export function usePermissions(): UsePermissionsReturn {
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated());
  
  // Dealer features (si aplica)
  const subscription = user?.subscription;
  const dealerFeatures = useDealerFeatures(subscription);
  
  const accountType = user?.accountType || null;
  const dealerPlan = subscription?.plan || null;
  
  // Calcular accesos
  const portalAccess = useMemo(
    () => getPortalAccessByAccountType(accountType, dealerPlan || undefined),
    [accountType, dealerPlan]
  );
  
  const serviceAccess = useMemo(
    () => dealerPlan ? getServiceAccessByPlan(dealerPlan) : null,
    [dealerPlan]
  );
  
  const planFeatures = useMemo(
    () => dealerPlan ? DEALER_PLAN_LIMITS[dealerPlan] : null,
    [dealerPlan]
  );
  
  // Usage actual
  const usage = useMemo(() => ({
    listings: subscription?.currentUsage?.listings || subscription?.usage?.currentListings || 0,
    featuredListings: subscription?.currentUsage?.featuredListings || subscription?.usage?.featuredUsed || 0,
  }), [subscription]);
  
  // Límites
  const limits = useMemo(() => ({
    maxListings: planFeatures?.maxListings || 0,
    maxFeaturedListings: planFeatures?.maxFeaturedListings || 0,
  }), [planFeatures]);
  
  // Helpers de tipo de cuenta
  const isGuest = !accountType || accountType === AccountType.GUEST;
  const isIndividual = accountType === AccountType.INDIVIDUAL;
  const isDealer = accountType === AccountType.DEALER;
  const isDealerEmployee = accountType === AccountType.DEALER_EMPLOYEE;
  const isAdmin = accountType === AccountType.ADMIN;
  const isPlatformEmployee = accountType === AccountType.PLATFORM_EMPLOYEE;
  
  // Helpers de dealer
  const isDealerOwner = isDealer && user?.dealerRole === 'owner';
  const hasActiveSubscription = subscription?.status === 'active';
  
  // Funciones de verificación
  const canAccessPortal = (portal: keyof PortalAccess): boolean => {
    return portalAccess[portal];
  };
  
  const canAccessService = (service: keyof ServiceAccess): boolean => {
    return serviceAccess?.[service] || false;
  };
  
  const canAccessFeature = (feature: keyof DealerPlanFeatures): boolean => {
    return dealerFeatures.canAccess(feature);
  };
  
  const hasDealerPermission = (permission: DealerPermission): boolean => {
    if (isDealerOwner) return true;
    return user?.dealerPermissions?.includes(permission) || false;
  };
  
  const hasPlatformPermission = (permission: PlatformPermission): boolean => {
    if (user?.platformRole === 'super_admin') return true;
    return user?.platformPermissions?.includes(permission) || false;
  };
  
  // Usage helpers
  const hasReachedLimit = (type: 'listings' | 'featured'): boolean => {
    if (type === 'listings') {
      return limits.maxListings !== Infinity && usage.listings >= limits.maxListings;
    }
    return limits.maxFeaturedListings !== Infinity && usage.featuredListings >= limits.maxFeaturedListings;
  };
  
  const getUsagePercentage = (type: 'listings' | 'featured'): number => {
    if (type === 'listings') {
      if (limits.maxListings === Infinity) return 0;
      return Math.min(100, (usage.listings / limits.maxListings) * 100);
    }
    if (limits.maxFeaturedListings === Infinity || limits.maxFeaturedListings === 0) return 0;
    return Math.min(100, (usage.featuredListings / limits.maxFeaturedListings) * 100);
  };
  
  // Upgrade helpers
  const needsUpgrade = (feature: keyof DealerPlanFeatures): boolean => {
    return dealerFeatures.needsUpgrade(feature);
  };
  
  const getNextPlan = (): DealerPlan | null => {
    return dealerFeatures.getNextPlan();
  };
  
  return {
    // Estado
    isAuthenticated,
    user,
    accountType,
    
    // Plan
    dealerPlan,
    planFeatures,
    
    // Accesos
    portalAccess,
    serviceAccess,
    
    // Helpers de tipo
    isGuest,
    isIndividual,
    isDealer,
    isDealerEmployee,
    isAdmin,
    isPlatformEmployee,
    
    // Helpers de dealer
    isDealerOwner,
    hasActiveSubscription,
    
    // Funciones
    canAccessPortal,
    canAccessService,
    canAccessFeature,
    hasDealerPermission,
    hasPlatformPermission,
    
    // Usage
    usage,
    limits,
    hasReachedLimit,
    getUsagePercentage,
    
    // Upgrade
    needsUpgrade,
    getNextPlan,
  };
}

export default usePermissions;
