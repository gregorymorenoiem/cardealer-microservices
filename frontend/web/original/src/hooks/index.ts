export { useAuth } from './useAuth';
export { useDebounce } from './useDebounce';
export { useLocalStorage } from './useLocalStorage';
export { usePermissions } from './usePermissions';
export { useDealerFeatures } from './useDealerFeatures';

// Performance hooks for low-bandwidth optimization
export {
  useNetworkStatus,
  useLazyLoad,
  usePrefetch,
  useImagePreloader,
  useReducedMotion,
  useIdleCallback,
  useVirtualList,
  useDebouncedValue,
  useThrottledCallback,
  useProgressiveImage,
} from './usePerformance';
