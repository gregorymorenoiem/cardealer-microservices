// ============================================================================
// ENUMS - Tipos de cuenta y planes (usando const para compatibilidad)
// ============================================================================

/**
 * Tipos de cuenta en la plataforma
 */
export const AccountType = {
  GUEST: 'guest',
  INDIVIDUAL: 'individual',
  DEALER: 'dealer',
  DEALER_EMPLOYEE: 'dealer_employee',
  ADMIN: 'admin',
  PLATFORM_EMPLOYEE: 'platform_employee'
} as const;

export type AccountType = typeof AccountType[keyof typeof AccountType];

/**
 * Roles específicos para empleados de dealers
 */
export const DealerRole = {
  OWNER: 'owner',
  MANAGER: 'manager',
  SALES_MANAGER: 'sales_manager',
  INVENTORY_MANAGER: 'inventory_manager',
  SALESPERSON: 'salesperson',
  VIEWER: 'viewer'
} as const;

export type DealerRole = typeof DealerRole[keyof typeof DealerRole];

/**
 * Roles específicos para empleados de plataforma
 */
export const PlatformRole = {
  SUPER_ADMIN: 'super_admin',
  ADMIN: 'admin',
  MODERATOR: 'moderator',
  SUPPORT: 'support',
  ANALYST: 'analyst'
} as const;

export type PlatformRole = typeof PlatformRole[keyof typeof PlatformRole];

/**
 * Planes de suscripción para dealers
 */
export const DealerPlan = {
  FREE: 'free',
  BASIC: 'basic',
  PRO: 'pro',
  ENTERPRISE: 'enterprise'
} as const;

export type DealerPlan = typeof DealerPlan[keyof typeof DealerPlan];

/**
 * Estados de verificación
 */
export const VerificationStatus = {
  PENDING: 'pending',
  VERIFIED: 'verified',
  REJECTED: 'rejected'
} as const;

export type VerificationStatus = typeof VerificationStatus[keyof typeof VerificationStatus];

/**
 * Permisos granulares para empleados de dealers
 */
export const DealerPermission = {
  // Inventory
  VIEW_INVENTORY: 'dealer:inventory:view',
  CREATE_LISTING: 'dealer:inventory:create',
  EDIT_LISTING: 'dealer:inventory:edit',
  DELETE_LISTING: 'dealer:inventory:delete',
  BULK_UPLOAD: 'dealer:inventory:bulk',
  
  // Leads
  VIEW_ALL_LEADS: 'dealer:leads:view:all',
  VIEW_ASSIGNED_LEADS: 'dealer:leads:view:assigned',
  ASSIGN_LEADS: 'dealer:leads:assign',
  EDIT_LEADS: 'dealer:leads:edit',
  
  // Team Management
  INVITE_EMPLOYEES: 'dealer:team:invite',
  MANAGE_EMPLOYEES: 'dealer:team:manage',
  ASSIGN_ROLES: 'dealer:team:assign_roles',
  
  // Analytics
  VIEW_ANALYTICS: 'dealer:analytics:view',
  EXPORT_REPORTS: 'dealer:analytics:export',
  
  // Billing
  VIEW_BILLING: 'dealer:billing:view',
  MANAGE_SUBSCRIPTION: 'dealer:billing:manage',
  
  // Settings
  EDIT_DEALER_PROFILE: 'dealer:settings:edit',
  MANAGE_BRANDING: 'dealer:settings:branding',
} as const;

export type DealerPermission = typeof DealerPermission[keyof typeof DealerPermission];

/**
 * Permisos para empleados de plataforma
 */
export const PlatformPermission = {
  // User Management
  VIEW_USERS: 'platform:users:view',
  EDIT_USERS: 'platform:users:edit',
  DELETE_USERS: 'platform:users:delete',
  SUSPEND_USERS: 'platform:users:suspend',
  
  // Dealer Management
  VIEW_DEALERS: 'platform:dealers:view',
  APPROVE_DEALERS: 'platform:dealers:approve',
  SUSPEND_DEALERS: 'platform:dealers:suspend',
  
  // Content Moderation
  MODERATE_LISTINGS: 'platform:content:moderate',
  DELETE_LISTINGS: 'platform:content:delete',
  
  // System Config
  MANAGE_SETTINGS: 'platform:settings:manage',
  VIEW_ANALYTICS: 'platform:analytics:view',
  
  // Support
  ACCESS_SUPPORT_TICKETS: 'platform:support:access',
  IMPERSONATE_USER: 'platform:support:impersonate',
} as const;

export type PlatformPermission = typeof PlatformPermission[keyof typeof PlatformPermission];

// ============================================================================
// INTERFACES - Suscripciones y límites
// ============================================================================

/**
 * Features disponibles por plan (para feature flags)
 */
export interface DealerPlanFeatures {
  // Límites numéricos
  maxListings: number;              // Máximo de publicaciones activas
  maxImages: number;                // Máximo de imágenes por vehículo
  maxFeaturedListings: number;      // Máximo de destacados simultáneos
  
  // Features booleanas
  analyticsAccess: boolean;         // Acceso a analytics dashboard
  bulkUpload: boolean;              // Carga masiva CSV/Excel
  prioritySupport: boolean;         // Soporte prioritario
  customBranding: boolean;          // Personalización de marca
  apiAccess: boolean;               // Acceso a API
  leadManagement: boolean;          // CRM para leads
  emailAutomation: boolean;         // Email sequences
  marketPriceAnalysis: boolean;     // Análisis de precios de mercado
  competitorTracking: boolean;      // Seguimiento de competidores
  advancedReporting: boolean;       // Reportes avanzados
  whiteLabel: boolean;              // White-label completo
  dedicatedAccountManager: boolean; // Account manager dedicado
}

/**
 * Configuración de límites por plan
 */
export const DEALER_PLAN_LIMITS: Record<DealerPlan, DealerPlanFeatures> = {
  [DealerPlan.FREE]: {
    maxListings: 3,
    maxImages: 5,
    maxFeaturedListings: 0,
    analyticsAccess: false,
    bulkUpload: false,
    prioritySupport: false,
    customBranding: false,
    apiAccess: false,
    leadManagement: false,
    emailAutomation: false,
    marketPriceAnalysis: false,
    competitorTracking: false,
    advancedReporting: false,
    whiteLabel: false,
    dedicatedAccountManager: false,
  },
  [DealerPlan.BASIC]: {
    maxListings: 50,
    maxImages: 10,
    maxFeaturedListings: 3,
    analyticsAccess: true,
    bulkUpload: true,
    prioritySupport: false,
    customBranding: false,
    apiAccess: false,
    leadManagement: true,
    emailAutomation: false,
    marketPriceAnalysis: false,
    competitorTracking: false,
    advancedReporting: false,
    whiteLabel: false,
    dedicatedAccountManager: false,
  },
  [DealerPlan.PRO]: {
    maxListings: 200,
    maxImages: 20,
    maxFeaturedListings: 10,
    analyticsAccess: true,
    bulkUpload: true,
    prioritySupport: true,
    customBranding: true,
    apiAccess: false,
    leadManagement: true,
    emailAutomation: true,
    marketPriceAnalysis: true,
    competitorTracking: true,
    advancedReporting: true,
    whiteLabel: false,
    dedicatedAccountManager: false,
  },
  [DealerPlan.ENTERPRISE]: {
    maxListings: Infinity,
    maxImages: 30,
    maxFeaturedListings: Infinity,
    analyticsAccess: true,
    bulkUpload: true,
    prioritySupport: true,
    customBranding: true,
    apiAccess: true,
    leadManagement: true,
    emailAutomation: true,
    marketPriceAnalysis: true,
    competitorTracking: true,
    advancedReporting: true,
    whiteLabel: true,
    dedicatedAccountManager: true,
  },
};

/**
 * Información de suscripción del dealer
 */
export interface DealerSubscription {
  id?: string;
  dealerId?: string;
  plan: DealerPlan;
  status: 'active' | 'past_due' | 'canceled' | 'trialing';
  startDate: string;
  endDate?: string;
  canceledAt?: string;
  trialEndsAt?: string;
  
  // Uso actual
  currentUsage?: {
    listings: number;
    featuredListings: number;
  };
  
  // Compatibilidad con estructura antigua
  usage?: {
    currentListings: number;
    listingsThisMonth: number;
    featuredUsed: number;
  };
  
  // Configuración (opcional para mock)
  features?: DealerPlanFeatures;
  autoRenew?: boolean;
  
  // Metadata
  createdAt?: string;
  updatedAt?: string;
}

/**
 * User interface extendida
 */
export interface User {
  id: string;
  email: string;
  name?: string;                    // Para compatibilidad con mocks
  firstName?: string;
  lastName?: string;
  phone?: string;
  avatar?: string;
  
  // Account type y roles
  accountType: AccountType;
  dealerRole?: DealerRole;      // Si es dealer/dealer_employee
  platformRole?: PlatformRole;  // Si es admin/platform_employee
  
  // Permisos (opcional)
  dealerPermissions?: DealerPermission[];
  platformPermissions?: PlatformPermission[];
  
  // Información de dealer (si aplica)
  dealerId?: string;
  
  // Suscripción (si es dealer)
  subscription?: DealerSubscription;
  
  // Roles legacy (opcional)
  roles?: string[];
  
  // Metadata
  isEmailVerified?: boolean;
  emailVerified?: boolean;      // Compatibilidad
  phoneVerified?: boolean;
  twoFactorEnabled?: boolean;
  createdAt?: string;
  updatedAt?: string;
  lastLoginAt?: string;
}

/**
 * Login Response
 */
export interface LoginResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
}
