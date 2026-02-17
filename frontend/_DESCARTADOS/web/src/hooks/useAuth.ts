import { useState, useCallback } from 'react';
import { useAuthStore } from '@/store/authStore';
import { authService } from '@/services/authService';

/**
 * Custom hook para acceder al estado de autenticación
 * Proporciona métodos async con loading y error states
 */
export function useAuth() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const user = useAuthStore((state) => state.user);
  const accessToken = useAuthStore((state) => state.accessToken);
  const isAuthenticatedFromStore = useAuthStore((state) => state.isAuthenticated);
  const storeLogin = useAuthStore((state) => state.login);
  const storeLogout = useAuthStore((state) => state.logout);
  const updateUser = useAuthStore((state) => state.updateUser);

  const isAuthenticated = isAuthenticatedFromStore();

  /**
   * Login con email y password
   */
  const login = useCallback(
    async (email: string, password: string, rememberMe = false) => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await authService.login({ email, password, rememberMe });
        storeLogin(response);
        return response;
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Login failed';
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [storeLogin]
  );

  /**
   * Registro de nuevo usuario
   */
  const register = useCallback(
    async (data: {
      email: string;
      password: string;
      fullName: string;
      userName: string;
      accountType: 'individual' | 'dealer';
    }) => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await authService.register(data);
        storeLogin(response);
        return response;
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Registration failed';
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [storeLogin]
  );

  /**
   * Cerrar sesión
   */
  const logout = useCallback(async () => {
    setIsLoading(true);
    try {
      await authService.logout();
    } catch (err) {
      console.error('Logout error:', err);
    } finally {
      storeLogout();
      setIsLoading(false);
    }
  }, [storeLogout]);

  /**
   * Solicitar reset de contraseña
   */
  const forgotPassword = useCallback(async (email: string) => {
    setIsLoading(true);
    setError(null);
    try {
      await authService.forgotPassword(email);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to send reset email';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  /**
   * Limpiar errores
   */
  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    // State
    user,
    isAuthenticated,
    isLoading,
    error,

    // Actions
    login,
    register,
    logout,
    forgotPassword,
    updateUser,
    clearError,
  };
}
