/**
 * Auth Guard Component
 *
 * Reusable client component for protecting routes.
 * Provides defense-in-depth alongside middleware protection.
 *
 * Features:
 * - Redirects unauthenticated users to login with callbackUrl
 * - Optional role-based access control (RBAC)
 * - Optional accountType validation
 * - Customizable loading and access denied states
 *
 * SECURITY NOTE:
 * This is a client-side guard (defense-in-depth). The middleware.ts
 * handles server-side route protection. Both layers work together
 * to prevent unauthorized access.
 *
 * Usage:
 *   <AuthGuard>              - Requires authentication only
 *   <AuthGuard roles={['admin']}>  - Requires admin role
 *   <AuthGuard accountTypes={['dealer', 'admin']}>  - Requires dealer or admin account
 */

'use client';

import * as React from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import { Loader2, ShieldAlert, LogIn, Home, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/use-auth';

// =============================================================================
// TYPES
// =============================================================================

interface AuthGuardProps {
  children: React.ReactNode;
  /** Required roles (from JWT). User must have at least one. */
  roles?: string[];
  /** Required account types (from user profile). User must match one. */
  accountTypes?: string[];
  /** Custom redirect path when not authenticated. Default: /login */
  loginPath?: string;
  /** Custom forbidden path. If not set, shows inline access denied. */
  forbiddenRedirect?: string;
  /** Custom loading component */
  loadingComponent?: React.ReactNode;
  /** Custom access denied component */
  accessDeniedComponent?: React.ReactNode;
}

// =============================================================================
// LOADING STATE (Default)
// =============================================================================

function DefaultLoading() {
  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <div className="text-center">
        <Loader2 className="mx-auto h-10 w-10 animate-spin text-primary" />
        <p className="text-muted-foreground mt-4 text-sm">Verificando acceso...</p>
      </div>
    </div>
  );
}

// =============================================================================
// ACCESS DENIED STATE (Default)
// =============================================================================

function DefaultAccessDenied() {
  const router = useRouter();

  return (
    <div className="flex min-h-[60vh] items-center justify-center p-4">
      <div className="mx-4 max-w-md text-center">
        <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-red-100">
          <ShieldAlert className="h-10 w-10 text-red-500" />
        </div>
        <h1 className="text-foreground mb-2 text-2xl font-bold">Acceso Denegado</h1>
        <p className="text-muted-foreground mb-6">
          No tienes los permisos necesarios para acceder a esta sección. Si crees que esto es un
          error, contacta a soporte.
        </p>
        <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
          <Button variant="outline" onClick={() => router.back()}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Volver
          </Button>
          <Button className="bg-primary hover:bg-primary/90" asChild>
            <Link href="/">
              <Home className="mr-2 h-4 w-4" />
              Ir al Inicio
            </Link>
          </Button>
        </div>
        <div className="border-border mt-6 border-t pt-4">
          <Link
            href="/login"
            className="inline-flex items-center gap-1 text-sm text-primary hover:underline"
          >
            <LogIn className="h-3 w-3" />
            Iniciar sesión con otra cuenta
          </Link>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// AUTH GUARD COMPONENT
// =============================================================================

export function AuthGuard({
  children,
  roles,
  accountTypes,
  loginPath = '/login',
  forbiddenRedirect,
  loadingComponent,
  accessDeniedComponent,
}: AuthGuardProps) {
  const { user, isLoading, isAuthenticated } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const [authChecked, setAuthChecked] = React.useState(false);

  React.useEffect(() => {
    if (isLoading) return;

    // Not authenticated → redirect to login with callback
    if (!isAuthenticated || !user) {
      const callbackUrl = encodeURIComponent(pathname);
      router.push(`${loginPath}?callbackUrl=${callbackUrl}`);
      return;
    }

    setAuthChecked(true);
  }, [isLoading, isAuthenticated, user, router, pathname, loginPath]);

  // Show loading while checking auth
  if (isLoading || !authChecked) {
    return <>{loadingComponent || <DefaultLoading />}</>;
  }

  // Not authenticated (should not reach here but safety net)
  if (!isAuthenticated || !user) {
    return <>{loadingComponent || <DefaultLoading />}</>;
  }

  // Check role-based access (roles from JWT/auth context)
  if (roles && roles.length > 0) {
    const userRole = (user as unknown as Record<string, unknown>).role as
      | string
      | string[]
      | undefined;
    const userRoles: string[] = userRole
      ? Array.isArray(userRole)
        ? userRole.map(r => r.toLowerCase())
        : [userRole.toLowerCase()]
      : [];

    // Also check accountType as fallback for role mapping
    const accountType = user.accountType?.toLowerCase();
    if (accountType && !userRoles.includes(accountType)) {
      userRoles.push(accountType);
    }

    // Map 'buyer' or 'seller' accountType to 'user' role
    if ((accountType === 'buyer' || accountType === 'seller') && !userRoles.includes('user')) {
      userRoles.push('user');
    }

    // Map 'dealer_employee' to 'dealer' role (employees share dealer access)
    if (accountType === 'dealer_employee' && !userRoles.includes('dealer')) {
      userRoles.push('dealer');
    }

    // Map 'platform_employee' to 'admin' role (platform staff share admin access)
    if (accountType === 'platform_employee' && !userRoles.includes('admin')) {
      userRoles.push('admin');
    }

    const hasRole = roles.some(r => userRoles.includes(r.toLowerCase()));
    if (!hasRole) {
      if (forbiddenRedirect) {
        router.push(forbiddenRedirect);
        return <>{loadingComponent || <DefaultLoading />}</>;
      }
      return <>{accessDeniedComponent || <DefaultAccessDenied />}</>;
    }
  }

  // Check account type access
  if (accountTypes && accountTypes.length > 0) {
    const userAccountType = user.accountType?.toLowerCase();
    const hasAccountType =
      userAccountType && accountTypes.some(t => t.toLowerCase() === userAccountType);
    if (!hasAccountType) {
      if (forbiddenRedirect) {
        router.push(forbiddenRedirect);
        return <>{loadingComponent || <DefaultLoading />}</>;
      }
      return <>{accessDeniedComponent || <DefaultAccessDenied />}</>;
    }
  }

  // All checks passed
  return <>{children}</>;
}

export default AuthGuard;
