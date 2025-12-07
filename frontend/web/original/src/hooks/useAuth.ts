import { useAuthStore } from '@/store/authStore';

/**
 * Custom hook para acceder al estado de autenticación
 * Simplifica el acceso a authStore en los componentes
 */
export function useAuth() {
  const user = useAuthStore((state) => state.user);
  const accessToken = useAuthStore((state) => state.accessToken);
  const login = useAuthStore((state) => state.login);
  const logout = useAuthStore((state) => state.logout);
  const updateUser = useAuthStore((state) => state.updateUser);

  // isAuthenticated es una función en el store, la evaluamos aquí
  const isAuthenticated = !!user && !!accessToken;

  return {
    user,
    isAuthenticated,
    isLoading: false, // No hay loading state en el store actual
    login,
    logout,
    updateUser,
  };
}
