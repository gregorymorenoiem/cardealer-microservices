import { useAuthStore } from '@/store/authStore';

/**
 * Custom hook para acceder al estado de autenticación
 * Simplifica el acceso a authStore en los componentes
 */
export function useAuth() {
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const isLoading = useAuthStore((state) => state.isLoading);
  const login = useAuthStore((state) => state.login);
  const logout = useAuthStore((state) => state.logout);
  const updateUser = useAuthStore((state) => state.updateUser);

  return {
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
    updateUser,
  };
}
