/**
 * Next.js Middleware
 *
 * Handles authentication and route protection
 * Based on the routing-map.md specification
 */

import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// =============================================================================
// ROUTE DEFINITIONS
// =============================================================================

// Public routes (no auth required)
const publicRoutes = [
  '/',
  '/vehiculos',
  '/vehiculos/[^/]+$', // /vehiculos/[slug]
  '/vehiculos/[^/]+/360$', // /vehiculos/[slug]/360
  '/buscar',
  '/dealers',
  '/dealers/[^/]+$', // /dealers/[slug]
  '/comparar',
  '/ayuda',
  '/ayuda/[^/]+$', // /ayuda/[category]
  '/ayuda/articulo/[^/]+$', // /ayuda/articulo/[slug]
  '/contacto',
  '/about',
  '/nosotros',
  '/terminos',
  '/privacidad',
];

// Guest-only routes (redirect if authenticated)
const guestOnlyRoutes = [
  '/login',
  '/registro',
  '/recuperar-contrasena',
  '/restablecer-contrasena',
  '/verificar-email',
  '/crear-contrasena',
];

// Role-protected routes (requires specific roles)
const roleProtectedRoutes: Record<string, string[]> = {
  '/dealer': ['dealer', 'admin'],
  '/admin': ['admin'],
  '/vender/publicar': ['seller', 'dealer', 'admin'],
};

// =============================================================================
// HELPERS
// =============================================================================

function isPublicRoute(pathname: string): boolean {
  return publicRoutes.some(route => {
    // Handle dynamic routes with regex
    if (route.includes('[^/]')) {
      const regex = new RegExp(`^${route.replace(/\[slug\]/g, '[^/]+')}$`);
      return regex.test(pathname);
    }
    return pathname === route || pathname.startsWith(`${route}/`);
  });
}

function isGuestOnlyRoute(pathname: string): boolean {
  return guestOnlyRoutes.some(route => pathname === route || pathname.startsWith(`${route}/`));
}

function getRequiredRoles(pathname: string): string[] | null {
  for (const [routePrefix, roles] of Object.entries(roleProtectedRoutes)) {
    if (pathname === routePrefix || pathname.startsWith(`${routePrefix}/`)) {
      return roles;
    }
  }
  return null;
}

// =============================================================================
// MIDDLEWARE
// =============================================================================

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Skip static files and API routes
  if (
    pathname.startsWith('/_next') ||
    pathname.startsWith('/api') ||
    pathname.includes('.') // static files
  ) {
    return NextResponse.next();
  }

  // Get auth token from cookie
  const token = request.cookies.get('auth-token')?.value;
  const isAuthenticated = !!token;

  // For now, we'll use a simple role extraction
  // In production, you'd verify the JWT and extract claims
  let userRole: string | null = null;
  if (token) {
    try {
      // Simple decode (not verification - that should be done server-side)
      const payload = JSON.parse(atob(token.split('.')[1]));
      userRole = payload.role || 'user';
    } catch {
      userRole = 'user';
    }
  }

  // Check public routes
  if (isPublicRoute(pathname)) {
    return NextResponse.next();
  }

  // Check guest-only routes
  if (isGuestOnlyRoute(pathname)) {
    if (isAuthenticated) {
      return NextResponse.redirect(new URL('/cuenta', request.url));
    }
    return NextResponse.next();
  }

  // All other routes require authentication
  if (!isAuthenticated) {
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('callbackUrl', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // Check role-protected routes
  const requiredRoles = getRequiredRoles(pathname);
  if (requiredRoles && userRole && !requiredRoles.includes(userRole)) {
    // User doesn't have required role
    return NextResponse.redirect(new URL('/403', request.url));
  }

  return NextResponse.next();
}

// =============================================================================
// CONFIG
// =============================================================================

export const config = {
  matcher: [
    /*
     * Match all request paths except:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public folder
     */
    '/((?!_next/static|_next/image|favicon.ico|images|fonts|.*\\..*).*)',
  ],
};
