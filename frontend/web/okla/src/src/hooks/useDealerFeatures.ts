import { 
  DealerPlan, 
  DEALER_PLAN_LIMITS,
} from '@/shared/types';
import type { DealerPlanFeatures, DealerSubscription } from '@/shared/types';

/**
 * Hook para verificar acceso a features según el plan del dealer
 * 
 * @example
 * const { canAccess, limits, usage, needsUpgrade } = useDealerFeatures();
 * 
 * if (!canAccess('analyticsAccess')) {
 *   return <UpgradePrompt feature="Analytics" />;
 * }
 */
export const useDealerFeatures = (subscription?: DealerSubscription) => {
  const currentPlan = subscription?.plan || DealerPlan.FREE;
  const limits = DEALER_PLAN_LIMITS[currentPlan];
  const currentUsage = subscription?.currentUsage || {
    listings: 0,
    featuredListings: 0,
  };
  
  // Mantener compatibilidad con estructura antigua
  const usage = {
    currentListings: currentUsage.listings,
    listingsThisMonth: currentUsage.listings,
    featuredUsed: currentUsage.featuredListings,
  };

  /**
   * Verifica si el dealer tiene acceso a una feature específica
   */
  const canAccess = (feature: keyof DealerPlanFeatures): boolean => {
    const value = limits[feature];
    return typeof value === 'boolean' ? value : (value as number) > 0;
  };

  /**
   * Verifica si el dealer ha alcanzado el límite de una feature
   */
  const hasReachedLimit = (feature: 'listings' | 'featured'): boolean => {
    if (feature === 'listings') {
      return usage.currentListings >= limits.maxListings;
    }
    if (feature === 'featured') {
      return usage.featuredUsed >= limits.maxFeaturedListings;
    }
    return false;
  };

  /**
   * Calcula el progreso de uso de una feature con límite
   */
  const getUsageProgress = (feature: 'listings' | 'featured'): number => {
    if (feature === 'listings') {
      return (usage.currentListings / limits.maxListings) * 100;
    }
    if (feature === 'featured') {
      return limits.maxFeaturedListings > 0 
        ? (usage.featuredUsed / limits.maxFeaturedListings) * 100 
        : 0;
    }
    return 0;
  };

  /**
   * Obtiene el siguiente plan recomendado
   */
  const getNextPlan = (): DealerPlan | null => {
    const planOrder = [DealerPlan.FREE, DealerPlan.BASIC, DealerPlan.PRO, DealerPlan.ENTERPRISE];
    const currentIndex = planOrder.indexOf(currentPlan);
    return currentIndex < planOrder.length - 1 ? planOrder[currentIndex + 1] : null;
  };

  /**
   * Determina si el dealer necesita upgrade para una feature
   */
  const needsUpgrade = (feature: keyof DealerPlanFeatures): boolean => {
    return !canAccess(feature) && getNextPlan() !== null;
  };

  return {
    currentPlan,
    limits,
    usage,
    canAccess,
    hasReachedLimit,
    getUsageProgress,
    getNextPlan,
    needsUpgrade,
  };
};

/**
 * Información de precios por plan (cuando estén disponibles)
 */
export const DEALER_PLAN_PRICING = {
  [DealerPlan.FREE]: {
    price: 0,
    currency: 'USD',
    interval: 'forever',
    available: true,
  },
  [DealerPlan.BASIC]: {
    price: 99,
    currency: 'USD',
    interval: 'month',
    available: false, // Cambiar a true cuando esté listo
  },
  [DealerPlan.PRO]: {
    price: 199,
    currency: 'USD',
    interval: 'month',
    available: false,
  },
  [DealerPlan.ENTERPRISE]: {
    price: 499,
    currency: 'USD',
    interval: 'month',
    available: false,
  },
};

/**
 * Helper para obtener nombres legibles de features
 */
export const FEATURE_NAMES: Record<keyof DealerPlanFeatures, string> = {
  maxListings: 'Vehicle Listings',
  maxImages: 'Images per Vehicle',
  maxFeaturedListings: 'Featured Listings',
  analyticsAccess: 'Analytics Dashboard',
  bulkUpload: 'Bulk Upload (CSV/Excel)',
  prioritySupport: 'Priority Support',
  customBranding: 'Custom Branding',
  apiAccess: 'API Access',
  leadManagement: 'Lead Management (CRM)',
  emailAutomation: 'Email Automation',
  marketPriceAnalysis: 'Market Price Analysis',
  competitorTracking: 'Competitor Tracking',
  advancedReporting: 'Advanced Reporting',
  whiteLabel: 'White Label',
  dedicatedAccountManager: 'Dedicated Account Manager',
};
