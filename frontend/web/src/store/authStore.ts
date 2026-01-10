import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type {
  User,
  LoginResponse,
  AccountType,
  DealerPermission,
  PlatformPermission,
} from '@/shared/types';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;

  // Actions
  login: (response: LoginResponse) => void;
  logout: () => void;
  updateUser: (user: User) => void;
  updateTokens: (accessToken: string, refreshToken: string) => void;
  setAccessToken: (token: string) => void;

  // Computed getters - Account Type
  isAuthenticated: () => boolean;
  accountType: () => AccountType | null;
  isGuest: () => boolean;
  isIndividual: () => boolean;
  isDealer: () => boolean;
  isDealerEmployee: () => boolean;
  isAdmin: () => boolean;
  isPlatformEmployee: () => boolean;

  // Computed getters - Dealer specific
  isDealerOwner: () => boolean;
  canManageDealerTeam: () => boolean;
  canAccessDealerPanel: () => boolean;
  hasActiveSubscription: () => boolean;

  // Computed getters - Platform specific
  isSuperAdmin: () => boolean;
  canManagePlatformUsers: () => boolean;
  canAccessAdminPanel: () => boolean;

  // Permission checkers
  hasDealerPermission: (permission: DealerPermission) => boolean;
  hasPlatformPermission: (permission: PlatformPermission) => boolean;
  hasAnyPermission: (permission: string) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,

      login: (response: LoginResponse) => {
        set({
          user: response.user,
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
        });

        // Save user email and accessToken to localStorage for compatibility with other services
        if (response.user?.email) {
          localStorage.setItem('userEmail', response.user.email);
        }

        if (response.accessToken) {
          localStorage.setItem('accessToken', response.accessToken);
        }
      },

      logout: () => {
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
        });

        // Clear localStorage data
        localStorage.removeItem('userEmail');
        localStorage.removeItem('accessToken');
      },

      updateUser: (user: User) => {
        set({ user });
      },

      updateTokens: (accessToken: string, refreshToken: string) => {
        set({ accessToken, refreshToken });
      },

      setAccessToken: (token: string) => {
        set({ accessToken: token });
      },

      // Computed getters - Account Type
      isAuthenticated: () => {
        const state = get();
        return !!state.user && !!state.accessToken;
      },

      accountType: () => {
        return get().user?.accountType || null;
      },

      isGuest: () => {
        const type = get().accountType();
        return type === 'guest' || type === null;
      },

      isIndividual: () => {
        return get().accountType() === 'individual';
      },

      isDealer: () => {
        return get().accountType() === 'dealer';
      },

      isDealerEmployee: () => {
        return get().accountType() === 'dealer_employee';
      },

      isAdmin: () => {
        return get().accountType() === 'admin';
      },

      isPlatformEmployee: () => {
        return get().accountType() === 'platform_employee';
      },

      // Computed getters - Dealer specific
      isDealerOwner: () => {
        const user = get().user;
        return user?.accountType === 'dealer' && user?.dealerRole === 'owner';
      },

      canManageDealerTeam: () => {
        const user = get().user;
        return (
          user?.dealerPermissions?.includes('dealer:team:manage' as DealerPermission) ||
          get().isDealerOwner()
        );
      },

      canAccessDealerPanel: () => {
        const state = get();
        return (state.isDealer() || state.isDealerEmployee()) && state.hasActiveSubscription();
      },

      hasActiveSubscription: () => {
        const user = get().user;
        if (!user?.subscription) return false;
        return user.subscription.status === 'active';
      },

      // Computed getters - Platform specific
      isSuperAdmin: () => {
        const user = get().user;
        return user?.accountType === 'admin' && user?.platformRole === 'super_admin';
      },

      canManagePlatformUsers: () => {
        const user = get().user;
        return (
          user?.platformPermissions?.includes('platform:users:edit' as PlatformPermission) ||
          get().isSuperAdmin()
        );
      },

      canAccessAdminPanel: () => {
        const state = get();
        return state.isAdmin() || state.isPlatformEmployee();
      },

      // Permission checkers
      hasDealerPermission: (permission: DealerPermission) => {
        const user = get().user;
        return user?.dealerPermissions?.includes(permission) || get().isDealerOwner();
      },

      hasPlatformPermission: (permission: PlatformPermission) => {
        const user = get().user;
        return user?.platformPermissions?.includes(permission) || get().isSuperAdmin();
      },

      hasAnyPermission: (permission: string) => {
        const user = get().user;
        return (
          user?.dealerPermissions?.includes(permission as DealerPermission) ||
          user?.platformPermissions?.includes(permission as PlatformPermission) ||
          false
        );
      },
    }),
    {
      name: 'auth-storage',
      storage: createJSONStorage(() => localStorage),
    }
  )
);

// Selectores exportados para uso optimizado
export const selectUser = (state: AuthState) => state.user;
export const selectAccessToken = (state: AuthState) => state.accessToken;
export const selectIsAuthenticated = (state: AuthState) => state.isAuthenticated();
export const selectIsDealer = (state: AuthState) => state.isDealer();
export const selectIsDealerOwner = (state: AuthState) => state.isDealerOwner();
export const selectIsDealerEmployee = (state: AuthState) => state.isDealerEmployee();
export const selectIsAdmin = (state: AuthState) => state.isAdmin();
export const selectIsSuperAdmin = (state: AuthState) => state.isSuperAdmin();
export const selectIsPlatformEmployee = (state: AuthState) => state.isPlatformEmployee();
export const selectCanAccessDealerPanel = (state: AuthState) => state.canAccessDealerPanel();
export const selectCanAccessAdminPanel = (state: AuthState) => state.canAccessAdminPanel();
export const selectCanManageDealerTeam = (state: AuthState) => state.canManageDealerTeam();
export const selectCanManagePlatformUsers = (state: AuthState) => state.canManagePlatformUsers();
