/**
 * Lazy Loading Components - Code splitting by vertical
 * Only load code when needed to reduce initial bundle size
 *
 * Features:
 * - Route-based code splitting
 * - Preloading on hover/focus
 * - Error boundaries for failed loads
 * - Loading fallbacks
 */

import React, { Suspense, lazy, useCallback, useState, useEffect } from 'react';
import type { ComponentType } from 'react';
import { CardGridSkeleton, DetailPageSkeleton, HeroSkeleton } from '../performance/SkeletonLoader';

// ============================================
// Error Boundary for Lazy Components
// ============================================

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

interface ErrorBoundaryProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
  onError?: (error: Error) => void;
}

class LazyErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error) {
    this.props.onError?.(error);
  }

  render() {
    if (this.state.hasError) {
      return (
        this.props.fallback || (
          <div className="flex flex-col items-center justify-center min-h-[400px] text-center p-8">
            <div className="w-16 h-16 mb-4 text-red-500">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1.5}
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              Error al cargar el contenido
            </h3>
            <p className="text-gray-500 mb-4">
              Hubo un problema cargando esta sección. Por favor, recarga la página.
            </p>
            <button
              onClick={() => window.location.reload()}
              className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
            >
              Recargar página
            </button>
          </div>
        )
      );
    }

    return this.props.children;
  }
}

// ============================================
// Loading Fallbacks
// ============================================

export const PageLoadingFallback: React.FC<{ type?: 'browse' | 'detail' | 'hero' | 'list' }> = ({
  type = 'browse',
}) => {
  switch (type) {
    case 'detail':
      return <DetailPageSkeleton />;
    case 'hero':
      return <HeroSkeleton />;
    case 'list':
      return (
        <div className="max-w-7xl mx-auto px-4 py-8">
          <CardGridSkeleton count={6} />
        </div>
      );
    default:
      return (
        <div className="max-w-7xl mx-auto px-4 py-8">
          <HeroSkeleton />
          <div className="mt-8">
            <CardGridSkeleton count={6} />
          </div>
        </div>
      );
  }
};

// ============================================
// Lazy Component Factory with Preload Support
// ============================================

interface LazyComponentOptions {
  fallback?: React.ReactNode;
  errorFallback?: React.ReactNode;
  minDelay?: number; // Minimum loading time to avoid flash
}

 
export function createLazyComponent<T extends ComponentType<any>>(
  importFn: () => Promise<{ default: T }>,
  options: LazyComponentOptions = {}
): {
  Component: React.FC<React.ComponentProps<T>>;
  preload: () => Promise<{ default: T }>;
} {
  const { fallback, errorFallback, minDelay = 0 } = options;

  // Create lazy component
  const LazyComponent = lazy(() => {
    const start = Date.now();
    return importFn().then((module) => {
      const elapsed = Date.now() - start;
      if (elapsed < minDelay) {
        return new Promise<{ default: T }>((resolve) => {
          setTimeout(() => resolve(module), minDelay - elapsed);
        });
      }
      return module;
    });
  });

  // Preload function
  const preload = importFn;

  // Wrapped component with Suspense and Error Boundary
   
  const Component: React.FC<React.ComponentProps<T>> = (props: any) => (
    <LazyErrorBoundary fallback={errorFallback}>
      <Suspense fallback={fallback || <PageLoadingFallback />}>
        <LazyComponent {...props} />
      </Suspense>
    </LazyErrorBoundary>
  );

  return { Component, preload };
}

// ============================================
// Preloadable Link Component
// ============================================

interface PreloadLinkProps extends React.AnchorHTMLAttributes<HTMLAnchorElement> {
  preload: () => Promise<unknown>;
  to: string;
  children: React.ReactNode;
}

export const PreloadLink: React.FC<PreloadLinkProps> = ({ preload, to, children, ...props }) => {
  const [isPreloaded, setIsPreloaded] = useState(false);

  const handlePreload = useCallback(() => {
    if (!isPreloaded) {
      preload().then(() => setIsPreloaded(true));
    }
  }, [isPreloaded, preload]);

  return (
    <a href={to} onMouseEnter={handlePreload} onFocus={handlePreload} {...props}>
      {children}
    </a>
  );
};

// ============================================
// Vertical-Specific Lazy Components
// ============================================

// Admin pages (heavy, should be lazy)
export const LazyAdminDashboard = createLazyComponent(
  () => import('@/pages/admin/AdminDashboardPage'),
  { fallback: <PageLoadingFallback type="list" /> }
);

export const LazyAdminCategories = createLazyComponent(
  () => import('@/pages/admin/CategoriesManagementPage'),
  { fallback: <PageLoadingFallback type="list" /> }
);

// Billing pages
export const LazyBillingDashboard = createLazyComponent(
  () => import('@/pages/billing/BillingDashboardPage'),
  { fallback: <PageLoadingFallback type="list" /> }
);

// ============================================
// Route Preloader Hook
// ============================================

export const useRoutePreloader = () => {
  const preloadAdmin = useCallback(() => {
    LazyAdminDashboard.preload();
    LazyAdminCategories.preload();
  }, []);

  const preloadBilling = useCallback(() => {
    LazyBillingDashboard.preload();
  }, []);

  return {
    preloadAdmin,
    preloadBilling,
  };
};

// ============================================
// Prefetch on Idle
// ============================================

export const usePrefetchOnIdle = (preloadFn: () => Promise<unknown>) => {
  useEffect(() => {
    let cancelled = false;

    if ('requestIdleCallback' in window) {
      const id = window.requestIdleCallback(
        () => {
          if (!cancelled) {
            preloadFn();
          }
        },
        { timeout: 5000 }
      );

      return () => {
        cancelled = true;
        window.cancelIdleCallback(id);
      };
    } else {
      const timeout = setTimeout(() => {
        if (!cancelled) {
          preloadFn();
        }
      }, 2000);

      return () => {
        cancelled = true;
        clearTimeout(timeout);
      };
    }
  }, [preloadFn]);
};

export default {
  createLazyComponent,
  PreloadLink,
  PageLoadingFallback,
  LazyAdminDashboard,
  LazyAdminCategories,
  LazyBillingDashboard,
  useRoutePreloader,
  usePrefetchOnIdle,
};
