/**
 * Mock Users Data
 * Usuarios de prueba con diferentes tipos de cuenta y planes
 */

import type { User, LoginResponse, DealerSubscription } from '@/shared/types';
import { 
  AccountType, 
  DealerPlan, 
  DealerRole, 
  PlatformRole,
  DealerPermission,
  PlatformPermission,
  DEALER_PLAN_LIMITS
} from '@/shared/types';

// ============================================================================
// DEALER SUBSCRIPTIONS
// ============================================================================

const createSubscription = (
  plan: DealerPlan,
  currentListings: number,
  featuredUsed: number = 0,
  status: DealerSubscription['status'] = 'active'
): DealerSubscription => ({
  id: `sub-${plan}-${Date.now()}`,
  plan,
  status,
  startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
  endDate: new Date(Date.now() + 335 * 24 * 60 * 60 * 1000).toISOString(),
  currentUsage: {
    listings: currentListings,
    featuredListings: featuredUsed,
  },
  features: DEALER_PLAN_LIMITS[plan],
  autoRenew: true,
});

// ============================================================================
// MOCK USERS DATABASE
// ============================================================================

export interface MockUserRecord {
  user: User;
  password: string;
}

export const mockUsersDatabase: Record<string, MockUserRecord> = {
  // ===== FREE PLAN DEALERS =====
  'dealer.free@cardealer.com': {
    password: 'password123',
    user: {
      id: 'dealer-free-001',
      email: 'dealer.free@cardealer.com',
      firstName: 'Carlos',
      lastName: 'Mendez',
      name: 'Carlos Mendez',
      phone: '+52 55 1234 5678',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=carlos',
      accountType: AccountType.DEALER,
      dealerRole: DealerRole.OWNER,
      dealerId: 'dealer-free-001',
      subscription: createSubscription(DealerPlan.FREE, 1, 0),
      dealerPermissions: Object.values(DealerPermission),
      isEmailVerified: true,
      createdAt: '2024-01-15T10:00:00Z',
    },
  },
  
  'dealer.freenearlimit@cardealer.com': {
    password: 'password123',
    user: {
      id: 'dealer-free-002',
      email: 'dealer.freenearlimit@cardealer.com',
      firstName: 'Maria',
      lastName: 'Garcia',
      name: 'Maria Garcia',
      phone: '+52 55 2345 6789',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=maria',
      accountType: AccountType.DEALER,
      dealerRole: DealerRole.OWNER,
      dealerId: 'dealer-free-002',
      subscription: createSubscription(DealerPlan.FREE, 3, 0), // At limit (max 3)
      dealerPermissions: Object.values(DealerPermission),
      isEmailVerified: true,
      createdAt: '2024-02-20T10:00:00Z',
    },
  },

  // ===== BASIC PLAN DEALERS =====
  'dealer.basic@cardealer.com': {
    password: 'password123',
    user: {
      id: 'dealer-basic-001',
      email: 'dealer.basic@cardealer.com',
      firstName: 'Roberto',
      lastName: 'Silva',
      name: 'Roberto Silva',
      phone: '+52 55 3456 7890',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=roberto',
      accountType: AccountType.DEALER,
      dealerRole: DealerRole.OWNER,
      dealerId: 'dealer-basic-001',
      subscription: createSubscription(DealerPlan.BASIC, 25, 2),
      dealerPermissions: Object.values(DealerPermission),
      isEmailVerified: true,
      createdAt: '2024-01-10T10:00:00Z',
    },
  },
  
  'dealer.basicnearlimit@cardealer.com': {
    password: 'password123',
    user: {
      id: 'dealer-basic-002',
      email: 'dealer.basicnearlimit@cardealer.com',
      firstName: 'Ana',
      lastName: 'Lopez',
      name: 'Ana Lopez',
      phone: '+52 55 4567 8901',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=ana',
      accountType: AccountType.DEALER,
      dealerRole: DealerRole.OWNER,
      dealerId: 'dealer-basic-002',
      subscription: createSubscription(DealerPlan.BASIC, 48, 3), // Near limit (max 50)
      dealerPermissions: Object.values(DealerPermission),
      isEmailVerified: true,
      createdAt: '2024-03-05T10:00:00Z',
    },
  },

  // ===== PRO PLAN DEALERS =====
  'dealer.pro@cardealer.com': {
    password: 'password123',
    user: {
      id: 'dealer-pro-001',
      email: 'dealer.pro@cardealer.com',
      firstName: 'Fernando',
      lastName: 'Rodriguez',
      name: 'Fernando Rodriguez',
      phone: '+52 55 5678 9012',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=fernando',
      accountType: AccountType.DEALER,
      dealerRole: DealerRole.OWNER,
      dealerId: 'dealer-pro-001',
      subscription: createSubscription(DealerPlan.PRO, 85, 5),
      dealerPermissions: Object.values(DealerPermission),
      isEmailVerified: true,
      createdAt: '2023-11-20T10:00:00Z',
    },
  },

  // ===== ENTERPRISE PLAN DEALERS =====
  'dealer.enterprise@cardealer.com': {
    password: 'password123',
    user: {
      id: 'dealer-enterprise-001',
      email: 'dealer.enterprise@cardealer.com',
      firstName: 'Alejandra',
      lastName: 'Martinez',
      name: 'Alejandra Martinez',
      phone: '+52 55 6789 0123',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=alejandra',
      accountType: AccountType.DEALER,
      dealerRole: DealerRole.OWNER,
      dealerId: 'dealer-enterprise-001',
      subscription: createSubscription(DealerPlan.ENTERPRISE, 450, 25),
      dealerPermissions: Object.values(DealerPermission),
      isEmailVerified: true,
      createdAt: '2023-06-15T10:00:00Z',
    },
  },

  // ===== INDIVIDUAL USER =====
  'individual@cardealer.com': {
    password: 'password123',
    user: {
      id: 'individual-001',
      email: 'individual@cardealer.com',
      firstName: 'Pedro',
      lastName: 'Sanchez',
      name: 'Pedro Sanchez',
      phone: '+52 55 7890 1234',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=pedro',
      accountType: AccountType.INDIVIDUAL,
      isEmailVerified: true,
      createdAt: '2024-04-10T10:00:00Z',
    },
  },

  // ===== ADMIN USER =====
  'admin@cardealer.com': {
    password: 'admin123',
    user: {
      id: 'admin-001',
      email: 'admin@cardealer.com',
      firstName: 'Admin',
      lastName: 'System',
      name: 'Admin System',
      phone: '+52 55 8901 2345',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=admin',
      accountType: AccountType.ADMIN,
      platformRole: PlatformRole.SUPER_ADMIN,
      platformPermissions: Object.values(PlatformPermission),
      isEmailVerified: true,
      createdAt: '2023-01-01T10:00:00Z',
    },
  },
};

// ============================================================================
// AUTH FUNCTIONS (MOCK)
// ============================================================================

/**
 * Simula el login con las credenciales mock
 */
export const mockLogin = async (
  email: string, 
  password: string
): Promise<LoginResponse | null> => {
  // Simular delay de red
  await new Promise(resolve => setTimeout(resolve, 500));
  
  const record = mockUsersDatabase[email.toLowerCase()];
  
  if (!record) {
    return null;
  }
  
  if (record.password !== password) {
    return null;
  }
  
  // Generar tokens mock
  const accessToken = `mock-access-token-${record.user.id}-${Date.now()}`;
  const refreshToken = `mock-refresh-token-${record.user.id}-${Date.now()}`;
  
  return {
    user: record.user,
    accessToken,
    refreshToken,
  };
};

/**
 * Obtiene un usuario por email (para testing)
 */
export const getMockUserByEmail = (email: string): User | null => {
  const record = mockUsersDatabase[email.toLowerCase()];
  return record?.user || null;
};

/**
 * Lista todos los usuarios demo disponibles
 */
export const getDemoCredentials = () => [
  {
    category: 'FREE',
    description: 'Free Plan',
    users: [
      { email: 'dealer.free@cardealer.com', password: 'password123', note: '' },
      { email: 'dealer.freenearlimit@cardealer.com', password: 'password123', note: '(at limit)' },
    ],
  },
  {
    category: 'BASIC',
    description: 'Basic Plan',
    users: [
      { email: 'dealer.basic@cardealer.com', password: 'password123', note: '' },
      { email: 'dealer.basicnearlimit@cardealer.com', password: 'password123', note: '(near limit)' },
    ],
  },
  {
    category: 'PRO',
    description: 'Pro Plan',
    users: [
      { email: 'dealer.pro@cardealer.com', password: 'password123', note: '' },
    ],
  },
  {
    category: 'ENTERPRISE',
    description: 'Enterprise Plan',
    users: [
      { email: 'dealer.enterprise@cardealer.com', password: 'password123', note: '' },
    ],
  },
  {
    category: 'OTHER',
    description: 'Other Users',
    users: [
      { email: 'individual@cardealer.com', password: 'password123', note: '(Individual)' },
      { email: 'admin@cardealer.com', password: 'admin123', note: '(Admin)' },
    ],
  },
];

// ============================================================================
// PORTAL ACCESS BY ACCOUNT TYPE
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
}

/**
 * Determina el acceso a portales según el tipo de cuenta
 */
export const getPortalAccessByAccountType = (accountType: AccountType): PortalAccess => {
  const baseAccess: PortalAccess = {
    marketplace: true, // Todos tienen acceso al marketplace
    dealerPanel: false,
    adminPanel: false,
    billing: false,
    crm: false,
    reports: false,
    marketing: false,
    finance: false,
    appointments: false,
    integrations: false,
  };
  
  switch (accountType) {
    case AccountType.GUEST:
      return { ...baseAccess };
      
    case AccountType.INDIVIDUAL:
      return { ...baseAccess };
      
    case AccountType.DEALER:
    case AccountType.DEALER_EMPLOYEE:
      return {
        ...baseAccess,
        dealerPanel: true,
        billing: true,
        crm: true,
        reports: true,
        marketing: true,
        finance: true,
        appointments: true,
        integrations: true,
      };
      
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
      };
      
    default:
      return baseAccess;
  }
};

/**
 * Determina features adicionales según el plan del dealer
 */
export const getFeaturesByPlan = (plan: DealerPlan) => {
  const features = DEALER_PLAN_LIMITS[plan];
  
  return {
    // Servicios backend disponibles según plan
    services: {
      appointmentService: true, // Todos los planes
      billingService: true, // Todos los planes
      crmService: features.leadManagement,
      financeService: plan !== DealerPlan.FREE,
      integrationService: features.apiAccess,
      invoicingService: plan !== DealerPlan.FREE,
      marketingService: features.emailAutomation,
      reportsService: features.analyticsAccess,
    },
    
    // Features de UI
    ui: {
      advancedFilters: features.analyticsAccess,
      bulkActions: features.bulkUpload,
      exportData: features.advancedReporting,
      customBranding: features.customBranding,
      priorityBadge: features.prioritySupport,
    },
  };
};
