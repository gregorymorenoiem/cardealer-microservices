import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import type { AccountType } from '@/shared/types';

interface PrivateRouteProps {
  children: ReactNode;
  requiredAccountTypes?: AccountType[];
  requireEmailVerification?: boolean;
}

/**
 * PrivateRoute - Protege rutas que requieren autenticación
 * 
 * @param children - Componente a renderizar si el usuario está autenticado
 * @param requiredAccountTypes - Tipos de cuenta permitidos (opcional)
 * @param requireEmailVerification - Si requiere email verificado (opcional)
 * 
 * @example
 * ```tsx
 * <Route path="/dealer" element={
 *   <PrivateRoute requiredAccountTypes={['dealer', 'dealer_employee']}>
 *     <DealerDashboard />
 *   </PrivateRoute>
 * } />
 * ```
 */
export function PrivateRoute({
  children,
  requiredAccountTypes,
  requireEmailVerification = false,
}: PrivateRouteProps) {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  // User not authenticated - redirect to login
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Check if user has required account type
  if (requiredAccountTypes && user) {
    const hasRequiredType = requiredAccountTypes.includes(user.accountType);
    
    if (!hasRequiredType) {
      // User doesn't have permission - redirect to their default dashboard
      return <Navigate to="/unauthorized" replace />;
    }
  }

  // Check if email verification is required
  if (requireEmailVerification && user && !user.emailVerified) {
    return <Navigate to="/verify-email" state={{ from: location }} replace />;
  }

  // User is authenticated and authorized - render children
  return <>{children}</>;
}

// Specific route guards for common use cases
export function DealerRoute({ children }: { children: ReactNode }) {
  return (
    <PrivateRoute requiredAccountTypes={['dealer', 'dealer_employee']}>
      {children}
    </PrivateRoute>
  );
}

export function AdminRoute({ children }: { children: ReactNode }) {
  return (
    <PrivateRoute requiredAccountTypes={['admin', 'platform_employee']}>
      {children}
    </PrivateRoute>
  );
}

export function IndividualRoute({ children }: { children: ReactNode }) {
  return (
    <PrivateRoute requiredAccountTypes={['individual']}>
      {children}
    </PrivateRoute>
  );
}
