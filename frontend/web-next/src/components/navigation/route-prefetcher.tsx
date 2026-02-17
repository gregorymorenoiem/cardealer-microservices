'use client';

/**
 * Route Prefetcher Component
 *
 * Prefetches critical routes on page load for faster navigation.
 * Uses Next.js Link component's prefetch capability.
 *
 * Strategy:
 * 1. Prefetch immediate next likely routes on mount
 * 2. Prefetch on viewport intersection (lazy)
 * 3. Prefetch on hover/focus for interactive elements
 */

import * as React from 'react';
import Link from 'next/link';
import { useEffect, useState } from 'react';
import { usePathname } from 'next/navigation';

// Critical routes that should always be prefetched
const CRITICAL_ROUTES = ['/', '/vehiculos', '/publicar'] as const;

// Routes to prefetch based on current page
const CONTEXTUAL_ROUTES: Record<string, string[]> = {
  '/': ['/vehiculos', '/publicar', '/dealers', '/search'],
  '/vehiculos': ['/', '/publicar', '/search'],
  '/publicar': ['/vehiculos', '/mis-vehiculos'],
  '/dealers': ['/vehiculos', '/search'],
  '/search': ['/vehiculos', '/publicar'],
  '/mis-vehiculos': ['/publicar', '/dashboard'],
  '/dashboard': ['/mis-vehiculos', '/settings', '/billing'],
};

// User authenticated routes
const AUTH_ROUTES = [
  '/dashboard',
  '/mis-vehiculos',
  '/favoritos',
  '/mensajes',
  '/settings',
] as const;

interface RoutePrefetcherProps {
  /** Whether user is authenticated */
  isAuthenticated?: boolean;
  /** Additional routes to prefetch */
  additionalRoutes?: string[];
  /** Whether to prefetch auth routes */
  prefetchAuthRoutes?: boolean;
}

/**
 * Invisible component that prefetches routes
 */
export function RoutePrefetcher({
  isAuthenticated = false,
  additionalRoutes = [],
  prefetchAuthRoutes = true,
}: RoutePrefetcherProps) {
  const pathname = usePathname();
  const [routesToPrefetch, setRoutesToPrefetch] = useState<string[]>([]);

  useEffect(() => {
    const routes = new Set<string>();

    // Add critical routes
    CRITICAL_ROUTES.forEach(route => routes.add(route));

    // Add contextual routes based on current page
    const contextualRoutes = CONTEXTUAL_ROUTES[pathname] || [];
    contextualRoutes.forEach(route => routes.add(route));

    // Add auth routes if authenticated
    if (isAuthenticated && prefetchAuthRoutes) {
      AUTH_ROUTES.forEach(route => routes.add(route));
    }

    // Add additional custom routes
    additionalRoutes.forEach(route => routes.add(route));

    // Remove current page from prefetch list
    routes.delete(pathname);

    setRoutesToPrefetch(Array.from(routes));
  }, [pathname, isAuthenticated, prefetchAuthRoutes, additionalRoutes]);

  // Render invisible links that trigger prefetch
  return (
    <div style={{ display: 'none' }} aria-hidden="true">
      {routesToPrefetch.map(route => (
        <Link key={route} href={route} prefetch={true} tabIndex={-1}>
          {route}
        </Link>
      ))}
    </div>
  );
}

/**
 * Enhanced Link component with hover prefetch
 * Prefetches on hover after a small delay to avoid unnecessary fetches
 */
interface PrefetchLinkProps extends React.ComponentProps<typeof Link> {
  /** Delay before prefetch on hover (ms) */
  hoverDelay?: number;
}

export function PrefetchLink({ children, hoverDelay = 100, ...props }: PrefetchLinkProps) {
  const [shouldPrefetch, setShouldPrefetch] = useState(false);

  const handleMouseEnter = () => {
    const timeout = setTimeout(() => {
      setShouldPrefetch(true);
    }, hoverDelay);

    return () => clearTimeout(timeout);
  };

  return (
    <Link
      {...props}
      prefetch={shouldPrefetch}
      onMouseEnter={handleMouseEnter}
      onFocus={() => setShouldPrefetch(true)}
    >
      {children}
    </Link>
  );
}

/**
 * Hook to manually trigger prefetch for specific routes
 */
export function usePrefetch() {
  const cleanupRefs = React.useRef<Set<ReturnType<typeof setTimeout>>>(new Set());

  // Cleanup all pending timeouts on unmount
  React.useEffect(() => {
    const refs = cleanupRefs.current;
    return () => {
      refs.forEach(id => clearTimeout(id));
      refs.clear();
    };
  }, []);

  const prefetchRoute = React.useCallback((href: string) => {
    const link = document.createElement('link');
    link.rel = 'prefetch';
    link.href = href;
    link.as = 'document';
    document.head.appendChild(link);

    const timeoutId = setTimeout(() => {
      if (link.parentNode) {
        document.head.removeChild(link);
      }
      cleanupRefs.current.delete(timeoutId);
    }, 100);
    cleanupRefs.current.add(timeoutId);
  }, []);

  const prefetchRoutes = React.useCallback(
    (routes: string[]) => {
      routes.forEach(route => prefetchRoute(route));
    },
    [prefetchRoute]
  );

  return { prefetchRoute, prefetchRoutes };
}

/**
 * Component to prefetch routes when visible in viewport
 */
interface ViewportPrefetchProps {
  /** Routes to prefetch when component enters viewport */
  routes: string[];
  /** Root margin for intersection observer */
  rootMargin?: string;
}

export function ViewportPrefetch({ routes, rootMargin = '200px' }: ViewportPrefetchProps) {
  const [shouldPrefetch, setShouldPrefetch] = useState(false);
  const { prefetchRoutes } = usePrefetch();

  useEffect(() => {
    if (shouldPrefetch) {
      prefetchRoutes(routes);
    }
  }, [shouldPrefetch, routes, prefetchRoutes]);

  useEffect(() => {
    const observer = new IntersectionObserver(
      entries => {
        if (entries[0].isIntersecting) {
          setShouldPrefetch(true);
          observer.disconnect();
        }
      },
      { rootMargin }
    );

    // Create a sentinel element
    const sentinel = document.createElement('div');
    sentinel.style.height = '1px';
    document.body.appendChild(sentinel);
    observer.observe(sentinel);

    return () => {
      observer.disconnect();
      sentinel.remove();
    };
  }, [rootMargin]);

  if (!shouldPrefetch) return null;

  return (
    <div style={{ display: 'none' }} aria-hidden="true">
      {routes.map(route => (
        <Link key={route} href={route} prefetch={true} tabIndex={-1}>
          {route}
        </Link>
      ))}
    </div>
  );
}

export default RoutePrefetcher;
