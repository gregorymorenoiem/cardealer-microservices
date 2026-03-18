/**
 * usePlansConfig — Platform Plan Catalog Hook
 *
 * Fetches the full subscription plan catalog from /api/plans.
 * Plans are priced from the admin-configurable ConfigurationService
 * (via AdminService /api/public/pricing → Gateway → /api/pricing BFF).
 *
 * Usage:
 *   const { sellerPlans, dealerPlans, isLoading } = usePlansConfig();
 *   const { allPlans } = usePlansConfig({ audience: 'seller' });
 */

'use client';

import { useState, useEffect } from 'react';
import type { PublicPlan, PlansCatalog } from '@/app/api/plans/route';

export type { PublicPlan, PlansCatalog };

// =============================================================================
// MODULE-LEVEL CACHE (shared across all hook instances)
// =============================================================================

let _cachedCatalog: PlansCatalog | null = null;
let _cacheTimestamp = 0;
const CACHE_TTL_MS = 60_000;
let _pendingFetch: Promise<PlansCatalog> | null = null;

// =============================================================================
// FETCH FUNCTION
// =============================================================================

async function fetchPlansCatalog(): Promise<PlansCatalog> {
  const now = Date.now();
  if (_cachedCatalog && now - _cacheTimestamp < CACHE_TTL_MS) {
    return _cachedCatalog;
  }
  if (_pendingFetch) return _pendingFetch;

  _pendingFetch = (async () => {
    try {
      const response = await fetch('/api/plans', {
        signal: AbortSignal.timeout(5000),
      });
      if (!response.ok) throw new Error(`Plans API: ${response.status}`);
      const data: PlansCatalog = await response.json();
      _cachedCatalog = data;
      _cacheTimestamp = Date.now();
      return data;
    } catch (err) {
      console.warn('[usePlansConfig] Failed to fetch plans:', err);
      return _cachedCatalog ?? { dealer: [], seller: [], updatedAt: new Date().toISOString() };
    } finally {
      _pendingFetch = null;
    }
  })();

  return _pendingFetch;
}

// =============================================================================
// HOOK
// =============================================================================

interface UsePlansConfigOptions {
  audience?: 'dealer' | 'seller' | 'all';
}

interface UsePlansConfigResult {
  dealerPlans: PublicPlan[];
  sellerPlans: PublicPlan[];
  allPlans: PublicPlan[];
  isLoading: boolean;
  error: string | null;
  /** Force refresh from server */
  refresh: () => void;
}

export function usePlansConfig(options: UsePlansConfigOptions = {}): UsePlansConfigResult {
  const [catalog, setCatalog] = useState<PlansCatalog | null>(_cachedCatalog);
  const [isLoading, setIsLoading] = useState(!_cachedCatalog);
  const [error, setError] = useState<string | null>(null);

  const load = () => {
    setIsLoading(true);
    setError(null);
    _cacheTimestamp = 0; // invalidate to force refetch
    fetchPlansCatalog()
      .then(data => {
        setCatalog(data);
        setIsLoading(false);
      })
      .catch(() => {
        setError('Error al cargar planes');
        setIsLoading(false);
      });
  };

  useEffect(() => {
    let mounted = true;
    fetchPlansCatalog()
      .then(data => {
        if (mounted) {
          setCatalog(data);
          setIsLoading(false);
        }
      })
      .catch(() => {
        if (mounted) {
          setError('Error al cargar planes');
          setIsLoading(false);
        }
      });
    return () => {
      mounted = false;
    };
  }, []);

  const dealerPlans = catalog?.dealer ?? [];
  const sellerPlans = catalog?.seller ?? [];
  const { audience = 'all' } = options;
  const allPlans =
    audience === 'dealer' ? dealerPlans : audience === 'seller' ? sellerPlans : [...dealerPlans, ...sellerPlans];

  return {
    dealerPlans,
    sellerPlans,
    allPlans,
    isLoading,
    error,
    refresh: load,
  };
}
