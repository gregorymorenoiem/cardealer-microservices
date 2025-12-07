import { 
  DEALER_PLAN_LIMITS, 
  DealerPlan, 
  DealerRole, 
  PlatformRole,
  AccountType 
} from '@/shared/types';
import type { User } from '@/shared/types';

/**
 * Mock users para testing de planes
 */
export const MOCK_USERS: Record<string, User> = {
  // Usuario con plan FREE
  dealerFree: {
    id: 'user-free-001',
    email: 'dealer.free@cardealer.com',
    name: 'John Doe (FREE)',
    phone: '+1234567890',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=free',
    accountType: AccountType.DEALER,
    
    subscription: {
      plan: DealerPlan.FREE,
      status: 'active',
      startDate: '2025-01-01T00:00:00Z',
      endDate: undefined, // FREE es permanente
      features: DEALER_PLAN_LIMITS[DealerPlan.FREE],
      currentUsage: {
        listings: 2,               // 2 de 3 usados
        featuredListings: 0,       // 0 de 0 (no tiene)
      },
      usage: {
        currentListings: 2,
        listingsThisMonth: 2,
        featuredUsed: 0,
      },
    },
    
    dealerId: 'dealer-free-001',
    dealerRole: DealerRole.OWNER,
    dealerPermissions: [],
    
    roles: ['dealer'],
    emailVerified: true,
    phoneVerified: true,
    twoFactorEnabled: false,
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },

  // Usuario con plan BASIC
  dealerBasic: {
    id: 'user-basic-001',
    email: 'dealer.basic@cardealer.com',
    name: 'Jane Smith (BASIC)',
    phone: '+1234567891',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=basic',
    accountType: 'dealer' as AccountType,
    
    subscription: {
      plan: 'basic' as DealerPlan,
      status: 'active',
      startDate: '2025-01-01T00:00:00Z',
      endDate: '2026-01-01T00:00:00Z',
      features: DEALER_PLAN_LIMITS['basic' as DealerPlan],
      usage: {
        currentListings: 35,       // 35 de 50 usados
        listingsThisMonth: 12,
        featuredUsed: 1,           // 1 de 2 usados
      },
    },
    
    dealerId: 'dealer-basic-001',
    dealerRole: DealerRole.OWNER,
    dealerPermissions: [],
    
    roles: ['dealer'],
    emailVerified: true,
    phoneVerified: true,
    twoFactorEnabled: false,
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },

  // Usuario con plan PRO
  dealerPro: {
    id: 'user-pro-001',
    email: 'dealer.pro@cardealer.com',
    name: 'Mike Johnson (PRO)',
    phone: '+1234567892',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=pro',
    accountType: AccountType.DEALER,
    
    subscription: {
      plan: DealerPlan.PRO,
      status: 'active',
      startDate: '2025-01-01T00:00:00Z',
      endDate: '2026-01-01T00:00:00Z',
      features: DEALER_PLAN_LIMITS[DealerPlan.PRO],
      usage: {
        currentListings: 120,      // 120 de 200 usados
        listingsThisMonth: 45,
        featuredUsed: 7,           // 7 de 10 usados
      },
    },
    
    dealerId: 'dealer-pro-001',
    dealerRole: DealerRole.OWNER,
    dealerPermissions: [],
    
    roles: ['dealer'],
    emailVerified: true,
    phoneVerified: true,
    twoFactorEnabled: true,
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },

  // Usuario con plan ENTERPRISE
  dealerEnterprise: {
    id: 'user-enterprise-001',
    email: 'dealer.enterprise@cardealer.com',
    name: 'Sarah Williams (ENTERPRISE)',
    phone: '+1234567893',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=enterprise',
    accountType: AccountType.DEALER,
    
    subscription: {
      plan: DealerPlan.ENTERPRISE,
      status: 'active',
      startDate: '2025-01-01T00:00:00Z',
      endDate: '2026-01-01T00:00:00Z',
      features: DEALER_PLAN_LIMITS[DealerPlan.ENTERPRISE],
      usage: {
        currentListings: 550,      // 550 (ilimitado)
        listingsThisMonth: 180,
        featuredUsed: 30,          // 30 de 50 usados
      },
    },
    
    dealerId: 'dealer-enterprise-001',
    dealerRole: DealerRole.OWNER,
    dealerPermissions: [],
    
    roles: ['dealer'],
    emailVerified: true,
    phoneVerified: true,
    twoFactorEnabled: true,
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },

  // Usuario FREE cerca del límite (para testing de warnings)
  dealerFreeNearLimit: {
    id: 'user-free-002',
    email: 'dealer.freenearlimit@cardealer.com',
    name: 'Tom Brown (FREE - Near Limit)',
    phone: '+1234567894',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=nearlimit',
    accountType: AccountType.DEALER,
    
    subscription: {
      plan: DealerPlan.FREE,
      status: 'active',
      startDate: '2025-01-01T00:00:00Z',
      endDate: undefined,
      features: DEALER_PLAN_LIMITS[DealerPlan.FREE],
      usage: {
        currentListings: 3,        // 3 de 3 (límite alcanzado!)
        listingsThisMonth: 3,
        featuredUsed: 0,
      },
    },
    
    dealerId: 'dealer-free-002',
    dealerRole: DealerRole.OWNER,
    dealerPermissions: [],
    
    roles: ['dealer'],
    emailVerified: true,
    phoneVerified: false,
    twoFactorEnabled: false,
    createdAt: '2025-11-01T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },

  // Usuario BASIC cerca del límite
  dealerBasicNearLimit: {
    id: 'user-basic-002',
    email: 'dealer.basicnearlimit@cardealer.com',
    name: 'Lisa Davis (BASIC - Near Limit)',
    phone: '+1234567895',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=basiclimit',
    accountType: AccountType.DEALER,
    
    subscription: {
      plan: DealerPlan.BASIC,
      status: 'active',
      startDate: '2025-01-01T00:00:00Z',
      endDate: '2026-01-01T00:00:00Z',
      features: DEALER_PLAN_LIMITS[DealerPlan.BASIC],
      usage: {
        currentListings: 48,       // 48 de 50 (96% usado)
        listingsThisMonth: 15,
        featuredUsed: 2,           // 2 de 2 (límite alcanzado!)
      },
    },
    
    dealerId: 'dealer-basic-002',
    dealerRole: DealerRole.OWNER,
    dealerPermissions: [],
    
    roles: ['dealer'],
    emailVerified: true,
    phoneVerified: true,
    twoFactorEnabled: false,
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },

  // Usuario Individual (no dealer)
  individual: {
    id: 'user-individual-001',
    email: 'individual@cardealer.com',
    name: 'Robert Wilson (Individual)',
    phone: '+1234567896',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=individual',
    accountType: AccountType.INDIVIDUAL,
    
    // Sin subscription (no es dealer)
    subscription: undefined,
    
    roles: ['user'],
    emailVerified: true,
    phoneVerified: false,
    twoFactorEnabled: false,
    createdAt: '2025-11-15T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },

  // Admin de plataforma
  admin: {
    id: 'user-admin-001',
    email: 'admin@cardealer.com',
    name: 'Admin User',
    phone: '+1234567897',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=admin',
    accountType: AccountType.ADMIN,
    
    platformRole: PlatformRole.SUPER_ADMIN,
    platformPermissions: [],
    
    roles: ['admin', 'super_admin'],
    emailVerified: true,
    phoneVerified: true,
    twoFactorEnabled: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2025-12-05T00:00:00Z',
  },
};

/**
 * Helper para obtener un usuario mock por email
 */
export const getMockUserByEmail = (email: string): User | undefined => {
  return Object.values(MOCK_USERS).find(user => user.email === email);
};

/**
 * Helper para obtener todos los dealers mock
 */
export const getMockDealers = (): User[] => {
  return Object.values(MOCK_USERS).filter(user => user.accountType === 'dealer');
};

/**
 * Lista de emails disponibles para el login (para UI)
 */
export const AVAILABLE_MOCK_USERS = [
  { email: 'dealer.free@cardealer.com', plan: 'FREE', usage: '2/3 listings' },
  { email: 'dealer.basic@cardealer.com', plan: 'BASIC', usage: '35/50 listings' },
  { email: 'dealer.pro@cardealer.com', plan: 'PRO', usage: '120/200 listings' },
  { email: 'dealer.enterprise@cardealer.com', plan: 'ENTERPRISE', usage: '550 listings' },
  { email: 'dealer.freenearlimit@cardealer.com', plan: 'FREE', usage: '3/3 listings (LIMIT!)' },
  { email: 'dealer.basicnearlimit@cardealer.com', plan: 'BASIC', usage: '48/50 listings' },
  { email: 'individual@cardealer.com', plan: 'N/A', usage: 'Individual user' },
  { email: 'admin@cardealer.com', plan: 'N/A', usage: 'Platform admin' },
];
