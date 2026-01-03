import { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { AccountType } from '@/shared/types';

interface ProtectedRouteProps {
  children: ReactNode;
  requireAdmin?: boolean;
  requireDealer?: boolean;
}

/**
 * ProtectedRoute - HOC para proteger rutas que requieren autenticación
 * Redirige a /login si el usuario no está autenticado
 * Redirige a / si la ruta requiere admin y el usuario no lo es
 * Redirige a / si la ruta requiere dealer y el usuario no lo es
 * Guarda la URL original para redirect post-login
 */
export default function ProtectedRoute({ 
  children, 
  requireAdmin = false,
  requireDealer = false 
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, user } = useAuth();
  const location = useLocation();

  // Show loading spinner while checking auth state
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    // Save the location they were trying to access
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Redirect to home if admin required but user is not admin
  if (requireAdmin) {
    const isAdmin = user?.role === 'admin' || 
                    user?.accountType === AccountType.ADMIN || 
                    user?.accountType === AccountType.PLATFORM_EMPLOYEE;
    if (!isAdmin) {
      return <Navigate to="/" replace />;
    }
  }

  // Redirect to home if dealer required but user is not dealer
  if (requireDealer) {
    const isDealer = user?.accountType === AccountType.DEALER || 
                     user?.accountType === AccountType.DEALER_EMPLOYEE;
    if (!isDealer) {
      return <Navigate to="/" replace />;
    }
  }

  return <>{children}</>;
}
